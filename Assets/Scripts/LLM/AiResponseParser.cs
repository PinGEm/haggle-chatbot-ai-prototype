using UnityEngine;

namespace LLM_Handler
{
    public class AiResponseParser
    {
        public string convo_message;
        public string convo_memory_fact;
        public string actual_intent;
        
        public int player_offer;
        public string offer_confidence;

        public void ParseResponse(string input)
        {
            Debug.Log(input);

            AIResponseData responseData = JsonUtility.FromJson<AIResponseData>(input);

            Debug.Log("ai_message: " + responseData.ai_message);
            Debug.Log("emotion: " + responseData.emotion);
            Debug.Log("reasoning: " + responseData.reasoning);
            Debug.Log("memory fact: " + responseData.memory_fact);

            player_offer = responseData.extracted_offer;
            offer_confidence = responseData.offer_confidence;

            convo_message = responseData.ai_message;
            convo_memory_fact = responseData.memory_fact;
        }
    }

    public class AIResponseData
    {
        public string reasoning;
        public int extracted_offer;
        public string offer_confidence;
        public string ai_message;
        public string emotion;
        public string memory_fact;
    }
}