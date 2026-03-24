using UnityEngine;
using TMPro;
using LLMAgent = LLMUnity.LLMAgent;

public class SendMessage : MonoBehaviour
{
    [SerializeField] private LLMAgent _llmAgent;
    [SerializeField] private TMP_InputField _messageField;
    [SerializeField] private TMP_Text _aiMessage;

    public void SendResponse()
    {
        GameAsync();
    }

    async void GameAsync()
    {

        string reply = await _llmAgent.Chat($@"
                === PLAYER INPUT ===
                    Player message: {_messageField.text}
                    Player offer: {300}
                    Last discussed price: {300}
                    Player tone: {"Neutral"}

                === CURRENT STATE ===
                    Item: {"Sword"}
                    Base price: {200}
                    Minimum price: {250}
                    Current asking price: {300}
                    Number of offers so far: {1}
                    Player behavior: {"Neutral"}


                Please remember to follow the game rules and ONLY respond in the given JSON format");
        _aiMessage.text = reply;
        Debug.Log(reply);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
