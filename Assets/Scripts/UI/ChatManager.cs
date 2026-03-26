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
    private MaxWidthClamp AiChatWidth;

    [Header("Prefabs")]
    public GameObject playerMessagePrefab;
    public GameObject aiMessagePrefab;

    [Header("Typing Indicator")]
    public GameObject typingIndicatorPrefab;
    private GameObject currentTypingIndicator; // Keeps track of the active dots

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

        float randomWaitTime = Random.Range(1.0f, 2.5f);

        StartCoroutine(DelayedTypingIndicator(randomWaitTime));
    }

    private System.Collections.IEnumerator DelayedTypingIndicator(float delayTime)
    {
        // Wait for the specified amount of seconds
        yield return new WaitForSeconds(delayTime);

        // Now show the dots!
        ShowTypingIndicator();
    }

    // This receives the RAW JSON from your ResponseHandler
    public void ReceiveAIMessage(string rawJson)
    {
        Debug.Log("1");
        StartCoroutine(ProcessAIMessageWithDelay(rawJson));
    }

    private System.Collections.IEnumerator ProcessAIMessageWithDelay(string rawJson)
    {
        // 1. Pick a random delay between 1.5 and 3 seconds
        float typingDelay = Random.Range(1.5f, 3.0f);

        // 2. Wait for that amount of time
        yield return new WaitForSeconds(typingDelay);

        // 3. Hide the dots
        HideTypingIndicator();

        // 4. Read the JSON 
        AIResponseData responseData = JsonUtility.FromJson<AIResponseData>(rawJson);
        string cleanAiText = responseData.ai_message;

        // 5. Spawn the AI Message Row (The Clone)
        GameObject newAiMessage = Instantiate(aiMessagePrefab, contentContainer);
        TextMeshProUGUI messageText = newAiMessage.GetComponentInChildren<TextMeshProUGUI>();

        // 6. Set the text
        messageText.text = cleanAiText;

        // --- THE NUCLEAR LAYOUT FIX ---

        // Force text to wake up
        messageText.ForceMeshUpdate();

        // Wait until the exact end of the frame so Unity fully registers the new text
        yield return new WaitForEndOfFrame();

        // Now clamp it
        AiChatWidth = newAiMessage.GetComponentInChildren<MaxWidthClamp>();
        if (AiChatWidth != null)
        {
            AiChatWidth.ClampWidth();
        }

        // Wait one more frame for the clamp math to finish
        yield return new WaitForEndOfFrame();

        // Now, forcefully rebuild the UI from the INSIDE OUT
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageText.GetComponent<RectTransform>()); // 1. Rebuild text box
        LayoutRebuilder.ForceRebuildLayoutImmediate(newAiMessage.GetComponent<RectTransform>()); // 2. Rebuild grey background
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer.GetComponent<RectTransform>()); // 3. Rebuild entire chat list

        // Force the main canvas to redraw right now
        Canvas.ForceUpdateCanvases();

        // Scroll down to see the new message
        StartCoroutine(ScrollToBottom());
    }

    public void ShowTypingIndicator()
    {
        // Only spawn it if one doesn't already exist
        if (currentTypingIndicator == null && typingIndicatorPrefab != null)
        {
            // Spawn the dots in the chat list
            currentTypingIndicator = Instantiate(typingIndicatorPrefab, contentContainer.transform);

            // Force the scrollbar to the bottom so the player sees the dots
            StartCoroutine(ScrollToBottom());
        }
    }

    public void HideTypingIndicator()
    {
        // Destroy the dots when the real message arrives
        if (currentTypingIndicator != null)
        {
            Destroy(currentTypingIndicator);
            currentTypingIndicator = null; // Reset the tracker
        }
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