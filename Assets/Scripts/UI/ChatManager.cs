using UnityEngine;
using TMPro;

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField chatInput;
    public Transform contentContainer;

    [Header("Prefabs")]
    public GameObject playerMessagePrefab;

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
}