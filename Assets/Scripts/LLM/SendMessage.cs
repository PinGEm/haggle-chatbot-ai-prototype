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
        string reply = await _llmAgent.Chat(_messageField.text);
        _aiMessage.text = reply;
        Debug.Log(reply);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
