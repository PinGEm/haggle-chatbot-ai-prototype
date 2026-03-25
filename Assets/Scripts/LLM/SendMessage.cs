using UnityEngine;
using TMPro;
using LLMAgent = LLMUnity.LLMAgent;

namespace LLM_Handler
{
    public class SendMessage : MonoBehaviour
    {
        [SerializeField] private LLMAgent _llmAgent;
        [SerializeField] private TMP_InputField _messageField;


        #region Temporary Variables
        [SerializeField] private TMP_Text _aiMessage;
        [SerializeField] private TMP_Text _aiIntent;


        // Temporary Scriptable Objects
        public ItemScriptableObject _item;
        public PersonalityScriptableObject _aiPersona;

        // Temporary Variables
        int _offersMade = 0;
        float _currentAIAskingPrice;
        float _minimumItemPrice;
        #endregion


        private AiResponseParser _aiParser;

        private void Awake()
        {
            _aiPersona.InitializePrompt();

            _aiParser = new AiResponseParser();

            _currentAIAskingPrice = _item.ItemBasePrice * 1.5f;
            _minimumItemPrice = _item.ItemBasePrice + 50;
        }

        public void SendResponse()
        {
            GameAsync();
        }

        async void GameAsync()
        {
            string reply = await _llmAgent.Chat($@"                    
                    You are currently selling an item to the player.

                    === PLAYER INPUT ===
                    Player message: {_messageField.text}
                    Player offer: {300}
                    Last discussed price: {300}
                    Player tone: {"Neutral"}

                    === CURRENT ITEM STATE ===
                    Item: {_item.ItemName}
                    Item Description: {_item.ItemDescription}
                    Base price: {_item.ItemBasePrice}
                    Minimum price: {_minimumItemPrice}
                    Current asking price: {_currentAIAskingPrice}
                    Number of offers so far: {_offersMade}
                    Player behavior: {"Neutral"}

                    === ADDITIONAL MEMORY FACTS ===
                    None

                    Please remember to follow the personality, speech rules, game rules and ONLY respond in the given JSON format");


            _aiMessage.text = _aiParser.ParseResponse(reply);
            _aiIntent.text = "AI Intent: " + _aiParser.ai_intent;
            
            Debug.Log(reply);
        }
    }
}