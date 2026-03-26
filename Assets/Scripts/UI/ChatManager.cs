using LLM_Handler;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        // 1. Don't send blank messages
        if (string.IsNullOrWhiteSpace(chatInput.text)) return;

        // 2. Spawn the new message row inside the Content box
        GameObject newMessage = Instantiate(playerMessagePrefab, contentContainer);

        // 3. Find the Text component inside the new bubble and set its text
        TextMeshProUGUI messageText = newMessage.GetComponentInChildren<TextMeshProUGUI>();
        messageText.text = chatInput.text;

        // 4. Clear the input field for the next message
        chatInput.text = "";

        // 5. Keep the cursor in the input field so the player can keep typing
        chatInput.ActivateInputField();
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
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

}