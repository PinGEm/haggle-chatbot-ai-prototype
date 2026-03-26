using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using LLMAgent = LLMUnity.LLMAgent;
using static UnityEngine.Audio.ProcessorInstance;
using System;

namespace LLM_Handler
{
    public class ResponseHandler : MonoBehaviour
    {
        [SerializeField] private LLMAgent _llmAgent;
        [SerializeField] private TMP_InputField _messageField;


        List<string> _memoryFacts = new List<string>();
        const int MAXIMUM_OFFERS = 7;

        #region Temporary Variables
        [SerializeField] private TMP_Text _aiIntent;

        // Temporary Scriptable Objects
        public ItemScriptableObject _item;
        public PersonalityScriptableObject _aiPersona;

        // Temporary Variables
        int _offersMade = 0;
        float _currentAIAskingPrice;
        float _lastDiscussedPrice;
        #endregion
        
        private AiResponseParser _aiParser;
        public ChatManager chatManager;

        private void Awake()
        {
            _aiPersona.InitializePrompt();

            _aiParser = new AiResponseParser();

            _currentAIAskingPrice = _item.ItemBasePrice * 1.5f;
        }

        public void SendResponse(Button sendButton)
        {
            // Block player input while we get the AI's response
            sendButton.interactable = false;

            TryGetAIResponse(sendButton);
            Debug.Log("Transaction Complete!");
        }

        async void TryGetAIResponse(Button sendButton)
        {
            // Parse Input for Player Offer
            var result = PlayerOfferParser.Parse(_messageField.text);
            float? offerValue_numerical = result.OfferValue;
            string offerValue = "";

            if (result.OfferValue.HasValue)
            {
                offerValue = result.OfferValue.Value.ToString();
                _lastDiscussedPrice = result.OfferValue.Value;
                _offersMade++;
            }
            else
            {
                offerValue = "none";
            }

            string confidenceLevel = result.Confidence.ToString().ToLower();

            if (_aiParser.asking_price == 0)
            {
                _aiParser.asking_price = (int)_item.ItemBasePrice;
            }

            _aiParser.actual_intent = DetermineIntent(offerValue_numerical, _aiParser.asking_price); // in the future, pass intent to influence the ai's reponse

            string fullPrompt = $"You are currently selling an item to the player. \r\n\r\n" + 
                GetPlayerInputPrompt(offerValue, confidenceLevel) + GetCurrentItemState() + GetMemoryFacts() +
                $"IMPORTANT:\r\n- You MUST follow the game's determined intent. The system has determined the intent to be: {_aiParser.actual_intent}.\r\n-Please remember to follow the personality, speech rules, game rules and ONLY respond in the given JSON format";

            Debug.Log(fullPrompt);

            // Send full response to LLM
            string reply = await _llmAgent.Chat(fullPrompt);
            
            sendButton.interactable = true;

            // Parse Response to JSON
            _aiParser.ParseResponse(reply);
            _currentAIAskingPrice = Mathf.Max(_aiParser.asking_price, _item.ItemBasePrice);
            _aiIntent.text = "AI Intent: " + _aiParser.actual_intent;

            Debug.Log(reply);

            Debug.Log("ACTUAL INTENT: " + _aiParser.actual_intent);

            if (chatManager != null)
            {
                chatManager.ReceiveAIMessage(reply);
            }
        }

        string GetPlayerInputPrompt(string offerValue, string confidenceLevel)
        {
            string player_input_prompt = "=== PLAYER INPUT===\r\n";

            player_input_prompt += $"Player Message: {_messageField.text}\r\n";
            player_input_prompt += $"Player Offer: {offerValue}\r\n"; 
            player_input_prompt += $"Offer Confidence: {confidenceLevel}\r\n"; 
            player_input_prompt += $"Last Discussed Price: {_lastDiscussedPrice}\r\n";
            //player_input_prompt += $"Player Tone: {"Neutral"}\r\n\r\n"; // Change to be the actual tone


            return player_input_prompt + "\r\n";
        }

        string GetCurrentItemState()
        {
            string item_state_prompt = "=== CURRENT ITEM STATE ===\r\n";

            item_state_prompt += $"Item: {_item.ItemName}\r\n";
            item_state_prompt += $"Item Description: {_item.ItemDescription}\r\n";

            item_state_prompt += "Item Details:\r\n";
            foreach (string item_detail in _item.ItemDetails)
            {
                item_state_prompt += "- " + item_detail + "\r\n";
            }

            item_state_prompt += $"Minimum Price: {_item.ItemBasePrice}\r\n";
            item_state_prompt += $"Current Asking Price: {_currentAIAskingPrice}\r\n";
            item_state_prompt += $"Number of offers made so far: {_offersMade}\r\n";
            //item_state_prompt += $"Player Behavior: {"Neutral"}\r\n\r\n"; // change to be aggressive (many low offers) | passive

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

        string DetermineIntent(float? playerOffer, float aiPrice)
        {
            if (!playerOffer.HasValue) return "negotiation";

            if (aiPrice == playerOffer.Value) return "accept";

            if (_offersMade >= MAXIMUM_OFFERS) return "reject";

            if (playerOffer < _item.ItemBasePrice)
            {
                if (_offersMade < 3)
                {
                    return "counteroffer";
                }

                return "reject";
            } 

            return "counteroffer";
        }
    }
}