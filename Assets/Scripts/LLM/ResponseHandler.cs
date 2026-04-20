using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using LLMAgent = LLMUnity.LLMAgent;
using System;
using System.Collections;


namespace LLM_Handler
{
    public class ResponseHandler : MonoBehaviour
    {
        const int ACCEPTANCE_PATIENCE_METER = 4;
        const int REFUSAL_PATIENCE_METER = 5;

        enum NegotiationState
        {
            // Most probably gonna be moved somewhere else
            negotiation,
            accept,
            reject,
            counteroffer
        }

        private NegotiationState _negotiationState;

        [SerializeField] private LLMAgent _llmAgent;
        [SerializeField] private TMP_InputField _messageField;


        List<string> _memoryFacts = new List<string>();
        const int MAXIMUM_OFFERS = 7;

        #region Temporary Variables
        [SerializeField] private TMP_Text _aiIntent;
        [SerializeField] private ResultPage _resultScript;
        [SerializeField] private TMP_Text _debugText;

        // Temporary Variables
        int _offersMade = 0;
        float _currentAIAskingPrice;
        public float SetCurrentAIAskingPrice { set { _currentAIAskingPrice = value; } }
        float _patienceMeter = 0;
        #endregion


        #region Manager Classes
        public ChatManager chatManager;
        private ItemManager _itemManager;
        private AIPersonaManager _personaManager;
        #endregion

        private AiResponseParser _aiParser;
        private string _previousMessage;

        private void Start()
        {
            // Persona Manager
            _personaManager = GameManager.Instance.AiManager;

            _personaManager.SelectedPersona.InitializePrompt();

            _aiParser = new AiResponseParser();

            // Item Manager
            _itemManager = GameManager.Instance.ItemManager;
            StartCoroutine(InitializeDelayed());
        }

        IEnumerator InitializeDelayed()
        {
            yield return new WaitUntil(() => GameManager.Instance.AiManager.StartingAIAskingPrice != 0);
            _currentAIAskingPrice = GameManager.Instance.AiManager.StartingAIAskingPrice;
        }

        public void SendResponse(Button sendButton)
        {
            // Block player input while we get the AI's response
            sendButton.interactable = false;

            TryGetAIResponse(sendButton);
        }

        async void TryGetAIResponse(Button sendButton)
        {
            string fullPrompt = $"You are currently *SELLING* an item to the player. \r\n\r\n" + 
                GetPlayerInputPrompt() + GetCurrentItemState() + GetMemoryFacts() +
                $"IMPORTANT:\r\n1. Identify the Extracted_Offer first.\r\n2. Draft all responses according to your personality.\r\n3. Use the [V] placeholder in the counteroffer draft.";

            Debug.Log(fullPrompt);

            // Send full response to LLM
            string reply = await _llmAgent.Chat(fullPrompt);
            sendButton.interactable = true;
            
            // Parse Response to JSON
            _aiParser.ParseResponse(reply);
            try
            {
                JsonUtility.FromJsonOverwrite(reply, new object());
            }
            catch(System.Exception)
            {
                _resultScript.ShowResult("INVALID AI RESPONSE: (temporary to check)", Color.red);
            }

            // ----- DECISION MAKING SYSTEM ----- \\
            int parsedOffer = _aiParser.player_offer;
            parsedOffer = OfferParserCheck(_previousMessage);

            ApplyEmotionalEffect();
            _negotiationState = DetermineIntent(parsedOffer, _currentAIAskingPrice);
            float newAIPrice = DetermineNewAIPrice(_currentAIAskingPrice, parsedOffer);
            
            if (newAIPrice < parsedOffer)
            {
                newAIPrice = (float)parsedOffer;
                _negotiationState = NegotiationState.accept;
                _debugText.text = "AI PRICING CHANGE WIN:\r\n";
            }

            if (newAIPrice == 0) newAIPrice = _currentAIAskingPrice;

            _aiParser.actual_intent = _negotiationState.ToString();
            Debug.Log("System Determined Price: " + newAIPrice);

            // Choosing the right message
            string finalMessage = "";
            switch (_negotiationState)
            {
                case NegotiationState.negotiation: finalMessage = _aiParser.negotiate_message; break;
                case NegotiationState.accept: finalMessage = _aiParser.accept_message; break;
                case NegotiationState.reject: finalMessage = _aiParser.reject_message; break;
                case NegotiationState.counteroffer: finalMessage = _aiParser.counter_message; break;
            }

            finalMessage = finalMessage.Replace("[V]", newAIPrice.ToString());
            finalMessage = finalMessage.Replace("[v]", newAIPrice.ToString());

            // Validate AI Response
            bool valid = AIOutputValidator.ValidatePrice(finalMessage, _aiParser.actual_intent, (int)newAIPrice)
             && AIOutputValidator.ForbidMinimumPrice(finalMessage);

            if (!valid && _negotiationState != NegotiationState.accept)
            {
                Debug.Log("AI Response not Valid!");
                Debug.Log($"Original AI Response:\r\n {reply}");

                // Correct ai_message if numeric price is missing
                if (!AIOutputValidator.ValidatePrice(reply, _aiParser.actual_intent, (int)newAIPrice))
                {
                    finalMessage += $" I'm only going to sell this for {newAIPrice}.";
                }

                // Correct ai_message if minimum price is mentioned
                if (!AIOutputValidator.ForbidMinimumPrice(reply))
                {
                    finalMessage = reply.Replace("minimum price", "");
                }

                Debug.Log($"New Message: {finalMessage}");
            }

            if (chatManager != null) chatManager.ReceiveAIMessage(finalMessage);


            _currentAIAskingPrice = newAIPrice;
            _currentAIAskingPrice = Mathf.Max(_currentAIAskingPrice, _itemManager.SelectedItem.ItemBasePrice);
            _aiIntent.text = "AI Intent: " + _aiParser.actual_intent;

            Debug.Log(reply);

            Debug.Log("ACTUAL INTENT: " + _aiParser.actual_intent);

            if (_negotiationState == NegotiationState.accept)
            { 
                _resultScript.ShowResult($"AI Accepts Offer: {newAIPrice}", Color.green);
                _debugText.text += reply;
                if (_negotiationState == NegotiationState.accept)
                {
                    _resultScript.ShowResult($"AI Accepts Offer: {newAIPrice}", Color.green);
                    _debugText.text += reply;

                    // 1. Give the player the item!
                    _itemManager.AddItemToInventory(_itemManager.SelectedItem);

                    // 2. Tell the Inventory UI to refresh (if it's currently in the scene)
                    InventoryUI inventoryScript = UnityEngine.Object.FindAnyObjectByType<InventoryUI>();
                    if (inventoryScript != null)
                    {
                        inventoryScript.BuildInventory();
                    }
                }
            }

            if (_negotiationState == NegotiationState.reject) _resultScript.ShowResult("AI Declines Offer", Color.red);
        }

        string GetPlayerInputPrompt()
        {
            string player_input_prompt = "=== PLAYER INPUT===\r\n";

            player_input_prompt += $"Player Message: {_messageField.text}\r\n";

            // Apply punishments if player response is "bad"
            if (_previousMessage == _messageField.text) _patienceMeter -= 1;

            if (_messageField.text.Length < 5 || !System.Text.RegularExpressions.Regex.IsMatch(_messageField.text, @"[a-zA-Z0-9]"))
            {
                _patienceMeter -= 2; // Punish spam severely.
            }


            _previousMessage = _messageField.text;

            return player_input_prompt + "\r\n";
        }

        string GetCurrentItemState()
        {
            string item_state_prompt = "=== CURRENT ITEM STATE ===\r\n";

            item_state_prompt += $"Item: {_itemManager.SelectedItem.ItemName}\r\n";
            item_state_prompt += $"Current Asking Price: {_currentAIAskingPrice}\r\n";
            item_state_prompt += $"Item Description: {_itemManager.SelectedItem.ItemDescription}\r\n";

            item_state_prompt += "Item Details:\r\n";
            foreach (string item_detail in _itemManager.SelectedItem.ItemDetails)
            {
                item_state_prompt += "- " + item_detail + "\r\n";
            }

            item_state_prompt += "\r\n";

            return item_state_prompt;
        }

        string GetMemoryFacts()
        {
            string memory_facts_prompt = "=== ADDITIONAL MEMORY FACTS ===\r\n";

            if (!string.IsNullOrWhiteSpace(_aiParser.convo_memory_fact))
            {
                _memoryFacts.Add(_aiParser.convo_memory_fact + "\r\n");
            }

            if (_memoryFacts.Count > 0)
            {
                for (int i = 0; i < _memoryFacts.Count; i++)
                {
                    // Additionally: do a check if the current memory fact has been seen before (o(n) time complexity)
                    memory_facts_prompt += _memoryFacts[i];
                }

                memory_facts_prompt += "\r\n";
            }
            else
            {
                memory_facts_prompt += "None\r\n\r\n";
            }

            return memory_facts_prompt;
        }

        void ApplyEmotionalEffect()
        {
            // If more emotions are allowed inside the grammar, ensure to add effects here

            switch (_aiParser.emotion)
            {
                case "annoyed":
                    _patienceMeter -= 0.5f;
                    break;
                case "angry":
                    _patienceMeter -= 1;
                    break;
                case "pleased":
                    _patienceMeter += 0.5f;
                    break;
                case "happy":
                    _patienceMeter += 1;
                    break;
            }
        }

        NegotiationState DetermineIntent(float? playerOffer, float aiPrice)
        {
            bool isOffer = playerOffer.HasValue && playerOffer != 0;


            // Check the Player's Numerical Offer
            if (!isOffer) return NegotiationState.negotiation;

            if (aiPrice == playerOffer.Value) return NegotiationState.accept;

            if (playerOffer < _itemManager.SelectedItem.ItemBasePrice)
            {
                _patienceMeter -= 1;

                if (_offersMade < 3)
                {
                    return NegotiationState.counteroffer;
                }

                return NegotiationState.reject;
            }

            // Check for patience meter
            if (_patienceMeter >= ACCEPTANCE_PATIENCE_METER)
            {
                _debugText.text = "PATIENCE METER WIN\r\n";
                return NegotiationState.accept;
            }
            else if (_patienceMeter <= -REFUSAL_PATIENCE_METER)
            {
                return NegotiationState.reject;
            }

            if (_offersMade >= MAXIMUM_OFFERS) return NegotiationState.reject;

            // Assume counter offer
            return NegotiationState.counteroffer;
        }

        private float DetermineNewAIPrice(float currentAIPrice, float? currentPlayerPrice)
        {
            // Note: Implementation price reaction to things like
            // patience meter, and behavior reactions should be implemented when possible

            if (_negotiationState != NegotiationState.counteroffer || currentPlayerPrice <= 0) return currentAIPrice;

            float lowballLimit = 0.65f;

            float newPrice = currentAIPrice;
            bool isExtremeLowball = (currentPlayerPrice < _itemManager.SelectedItem.ItemBasePrice * lowballLimit);

            if (currentPlayerPrice.HasValue && !isExtremeLowball)
            {
                // If player price isn't considerably lower than the minimum price, do this calculation
                newPrice = (float)(currentAIPrice + currentPlayerPrice + ((currentAIPrice - currentPlayerPrice) * _personaManager.SelectedPersona.PricePrefs)) / 2;

                newPrice = GetRoundedPrice(newPrice);
            }
            else if(currentPlayerPrice.HasValue && isExtremeLowball)
            {
                // If player offer is considerably lower than minimum price, punish the player to prevent extreme lowball
                newPrice = currentAIPrice * 0.95f;

                if (newPrice < currentAIPrice - 100) newPrice = currentAIPrice - 100;

                newPrice = GetRoundedPrice(newPrice);
            }

            // Price Variation
            int variation = UnityEngine.Random.Range(1,7);
                    
            switch (variation)
            {
                case 1: newPrice += 5; break;
                case 2: newPrice += 10; break;
                case 3: newPrice -= 5; break;
                case 4: newPrice -= 10; break;
                case 5: newPrice += 0; break;
                case 6: newPrice -= 0; break;
            }

            newPrice = Mathf.Clamp(newPrice, _itemManager.SelectedItem.ItemBasePrice - 10, _currentAIAskingPrice + 10);
                    
            return newPrice;
        }


        // Helper Function

        float GetRoundedPrice(float value)
        {
            // Round the price to the nearest multiple of 5 or 10 (e.g: 212 -> 210)
            float nearestFifthMultiple = (float)Math.Round(value / 5) * 5;
            float nearestTenthMultiple = (float)Math.Round(value / 10) * 10;

            float dist_5 = Math.Abs(value - nearestFifthMultiple);
            float dist_10 = Math.Abs(value - nearestTenthMultiple);

            if (dist_5 < dist_10) return nearestFifthMultiple;
            else if (dist_5 > dist_10) return nearestTenthMultiple;
            else
            {
                if (_personaManager.SelectedPersona.PricePrefs >= 0.5) return Math.Max(nearestTenthMultiple, nearestFifthMultiple);
                else return Math.Min(nearestTenthMultiple,nearestFifthMultiple);
            }
        }

        int OfferParserCheck(string text)
        {
            string digits = System.Text.RegularExpressions.Regex.Match(text, @"\d+").Value;
            return string.IsNullOrEmpty(digits) ? 0 : int.Parse(digits);
        }
    }
}