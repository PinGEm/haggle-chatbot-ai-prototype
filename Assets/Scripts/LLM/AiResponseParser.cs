using UnityEngine;

namespace LLM_Handler
{
    public class AiResponseParser
    {
        public string ai_intent;
        public string convo_memory_fact;
        public int asking_price;

        public string ParseResponse(string input)
        {
            Debug.Log(input);

            AIResponseData responseData = JsonUtility.FromJson<AIResponseData>(input);

            Debug.Log("ai_message: " + responseData.ai_message);
            Debug.Log("ai_intent: " + responseData.intent);
            Debug.Log("price: " + responseData.price);
            Debug.Log("emotion: " + responseData.emotion);
            Debug.Log("reasoning: " + responseData.reason);
            Debug.Log("memory fact: " + responseData.memory_fact);

            ai_intent = responseData.intent;
            convo_memory_fact = responseData.memory_fact;
            asking_price = responseData.price;
            return responseData.ai_message;
        }
    }

    public class AIResponseData
    {
        public string ai_message;
        public string intent;
        public int price;
        public string emotion;
        public string reason;
        public string memory_fact;
    }
}