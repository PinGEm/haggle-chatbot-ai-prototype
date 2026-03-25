using UnityEngine;

namespace LLM_Handler
{
    public class AiResponseParser
    {
        public string ai_intent;

        public string ParseResponse(string input)
        {
            AIResponseData responseData = JsonUtility.FromJson<AIResponseData>(input);

            Debug.Log("ai_message: " + responseData.ai_message);
            Debug.Log("ai_intent: " + responseData.intent);
            Debug.Log("price: " + responseData.price);
            Debug.Log("emotion: " + responseData.emotion);
            Debug.Log("reasoning: " + responseData.reasoning);
            Debug.Log("memory fact: " + responseData.convo_memory_fact);

            ai_intent = responseData.intent;
            return responseData.ai_message;
        }
    }

    public class AIResponseData
    {
        public string ai_message;
        public string intent;
        public int price;
        public string emotion;
        public string reasoning;
        public string convo_memory_fact;
    }
}