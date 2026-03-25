using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LLM_Handler; // This lets us see your AIResponseData!

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField chatInput;
    public Transform contentContainer;
    public ScrollRect chatScrollRect;
    
    [Header("Prefabs")]
    public GameObject playerMessagePrefab;
    public GameObject aiMessagePrefab;

    public void SendChatMessage()
    {
        if (string.IsNullOrWhiteSpace(chatInput.text)) return;

        // Spawn Player Message
        GameObject newMessage = Instantiate(playerMessagePrefab, contentContainer);
        newMessage.GetComponentInChildren<TextMeshProUGUI>().text = chatInput.text;

        chatInput.text = "";
        chatInput.ActivateInputField();

        StartCoroutine(ScrollToBottom());
    }

    // This receives the RAW JSON from your ResponseHandler
    public void ReceiveAIMessage(string rawJson)
    {
        // 1. Read the JSON to grab ONLY the ai_message string
        AIResponseData responseData = JsonUtility.FromJson<AIResponseData>(rawJson);
        string cleanAiText = responseData.ai_message;

        // 2. Spawn the AI Message Row
        GameObject newAiMessage = Instantiate(aiMessagePrefab, contentContainer);

        // 3. Find the Text component and set it to the clean text we just extracted
        TextMeshProUGUI messageText = newAiMessage.GetComponentInChildren<TextMeshProUGUI>();
        messageText.text = cleanAiText;

        // 4. Scroll down to see the new message
        StartCoroutine(ScrollToBottom());
    }

    System.Collections.IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
}