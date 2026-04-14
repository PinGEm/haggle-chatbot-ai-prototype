using UnityEngine;

namespace LLM_Handler
{
    public class AiResponseParser
    {
        public string convo_memory_fact;
        public string actual_intent;
        
        public int player_offer;

        public string negotiate_message;
        public string counter_message;
        public string accept_message;
        public string reject_message;

        public string emotion;

        public void ParseResponse(string input)
        {
            Debug.Log(input);

            AIResponseData responseData = JsonUtility.FromJson<AIResponseData>(input);

            Debug.Log("emotion: " + responseData.emotion);
            Debug.Log("memory fact: " + responseData.memory_fact);
            Debug.Log("extracted offer: " + responseData.parsed_offer);

            emotion = responseData.emotion;

            negotiate_message = responseData.draft_negotiate;
            counter_message = responseData.draft_counter;
            accept_message = responseData.draft_accept;
            reject_message = responseData.draft_reject;

            player_offer = responseData.parsed_offer;

            convo_memory_fact = responseData.memory_fact;
        }
    }

    public class AIResponseData
    {
        public int parsed_offer;
        public string draft_negotiate;
        public string draft_counter;
        public string draft_accept;
        public string draft_reject;
        public string emotion;
        public string memory_fact;
    }
}