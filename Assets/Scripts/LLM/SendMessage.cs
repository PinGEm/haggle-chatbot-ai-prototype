using UnityEngine;
using TMPro;
using LLMAgent = LLMUnity.LLMAgent;

namespace LLM_Handler
{
    public class SendMessage : MonoBehaviour
    {
        [SerializeField] private LLMAgent _llmAgent;
        [SerializeField] private TMP_InputField _messageField;
        [SerializeField] private TMP_Text _aiMessage;

        public PersonalityScriptableObject _aiPersona;

        private AiResponseParser _aiParser;

        private void Awake()
        {
            _aiPersona.InitializePrompt();

            _aiParser = new AiResponseParser();
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
                    Item: {"Sword"}
                    Base price: {200}
                    Minimum price: {250}
                    Current asking price: {320}
                    Number of offers so far: {1}
                    Player behavior: {"Neutral"}

                    === ADDITIONAL MEMORY FACTS ===
                    None

                    Please remember to follow the personality, speech rules, game rules and ONLY respond in the given JSON format");
            _aiMessage.text = _aiParser.ParseResponse(reply);
            Debug.Log(reply);
        }
    }
}
