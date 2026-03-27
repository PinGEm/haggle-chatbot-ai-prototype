using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using LLMAgent = LLMUnity.LLMAgent;
using static UnityEngine.Audio.ProcessorInstance;
using System;
using System.Net.Security;
using System.Text.RegularExpressions;

namespace LLM_Handler
{
    public class ResponseHandler : MonoBehaviour
    {
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

            _negotiationState = DetermineIntent(offerValue_numerical, _currentAIAskingPrice);
            _aiParser.actual_intent = _negotiationState.ToString();

            float newAIPrice = DetermineNewAIPrice(_currentAIAskingPrice, offerValue_numerical);
            Debug.Log("System Determined Price: " + newAIPrice);

            string fullPrompt = $"You are currently selling an item to the player. \r\n\r\n" + 
                GetPlayerInputPrompt(offerValue, confidenceLevel) + GetCurrentItemState() + GetMemoryFacts() +
                $"IMPORTANT:\r\n- You MUST follow the game's determined intent and price.\r\n-System Determined intent: {_aiParser.actual_intent}.\r\n-System Determined price: {newAIPrice}\r\n-Please remember to follow the behavior guidelines, personality, speech rules, offer interpretation rules, and game rules and ONLY respond in the given JSON format";

            Debug.Log(fullPrompt);

            // Send full response to LLM
            string reply = await _llmAgent.Chat(fullPrompt);
            
            sendButton.interactable = true;


            // Validate AI Response
            bool valid = AIOutputValidator.ValidatePrice(reply, _aiParser.actual_intent, (int)newAIPrice)
             && AIOutputValidator.ForbidMinimumPrice(reply);

            if (!valid)
            {
                Debug.Log("AI Response not Valid!");

                // Correct ai_message if numeric price is missing
                if (!AIOutputValidator.ValidatePrice(reply, _aiParser.actual_intent, (int)newAIPrice))
                {
                    // Insert the system-determined price
                    reply = $"My lowest is {newAIPrice}.";
                }

                // Correct ai_message if minimum price is mentioned
                if (!AIOutputValidator.ForbidMinimumPrice(reply))
                {
                    reply = reply.Replace("minimum price", "");
                }
            }


            // Parse Response to JSON
            _aiParser.ParseResponse(reply);
            _currentAIAskingPrice = newAIPrice;
            _currentAIAskingPrice = Mathf.Max(_currentAIAskingPrice, _item.ItemBasePrice);
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

        NegotiationState DetermineIntent(float? playerOffer, float aiPrice)
        {
            // Note:
            // There should probably be a way for the AI to ACCEPT the Player's offer.
            // This function should probably also handle that

            if (!playerOffer.HasValue) return NegotiationState.negotiation;

            if (aiPrice == playerOffer.Value) return NegotiationState.accept;

            if (_offersMade >= MAXIMUM_OFFERS) return NegotiationState.reject;

            if (playerOffer < _item.ItemBasePrice)
            {
                if (_offersMade < 3)
                {
                    return NegotiationState.counteroffer;
                }

                return NegotiationState.reject;
            } 

            // Assume counter offer
            return NegotiationState.counteroffer;
        }

        private float DetermineNewAIPrice(float currentAIPrice, float? currentPlayerPrice)
        {
            switch (_negotiationState)
            {
                case NegotiationState.counteroffer:
                    // Note: Implementation price reaction to things like
                    // patience meter, and behavior reactions should be implemented when possible

                    float newPrice = currentAIPrice;

                    if (currentPlayerPrice.HasValue)
                    {
                        newPrice = (float)(currentAIPrice + currentPlayerPrice + (  (currentAIPrice - currentPlayerPrice ) * _aiPersona.PricePrefs) ) / 2;

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

                    newPrice = Mathf.Clamp(newPrice, _item.ItemBasePrice + 10, _currentAIAskingPrice - 10);
                    
                    return newPrice;
                default:
                    // If the state is: Accept, Reject, Negotiation;
                    // Make the AI stand on business

                    return currentAIPrice;
            }
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
                if (_aiPersona.PricePrefs >= 0.5) return Math.Max(nearestTenthMultiple, nearestFifthMultiple);
                else return Math.Min(nearestTenthMultiple,nearestFifthMultiple);
            }
        }
    }
}