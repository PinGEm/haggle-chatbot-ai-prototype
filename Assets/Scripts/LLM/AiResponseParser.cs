using UnityEngine;

namespace LLM_Handler
{
    public class AiResponseParser
    {
        public string convo_message;
        public string convo_memory_fact;
        public string actual_intent;

        public string ParseResponse(string input)
        {
            Debug.Log(input);

            AIResponseData responseData = JsonUtility.FromJson<AIResponseData>(input);

            Debug.Log("ai_message: " + responseData.ai_message);
            Debug.Log("emotion: " + responseData.emotion);
            Debug.Log("reasoning: " + responseData.reason);
            Debug.Log("memory fact: " + responseData.memory_fact);

            convo_message = responseData.ai_message;
            convo_memory_fact = responseData.memory_fact;
            return responseData.ai_message;
        }
    }

    public class AIResponseData
    {
        public string ai_message;
        public string emotion;
        public string reason;
        public string memory_fact;
    }
}