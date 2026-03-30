using UnityEngine;
using UnityEngine.UI; // Needed for RawImage
using TMPro; // Needed for TextMeshProUGUI

public class ItemUIDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private RawImage itemRawImage;


    public void UpdateItemDisplay()
    {
        // 1. Make sure the GameManager and ItemManager exist
        if (GameManager.Instance == null || GameManager.Instance.itemManager == null) return;

        // 2. Grab the item that was already selected
        ItemScriptableObject currentItem = GameManager.Instance.itemManager.SelectedItem;
        if (currentItem == null) return;

        // 3. Paste the data directly into your 4 UI slots exactly as they are
        if (nameText != null) nameText.text = currentItem.ItemName;
        if (priceText != null) priceText.text = "$" + GameManager.Instance.aiManager.StartingAIAskingPrice.ToString();
        if (descriptionText != null) descriptionText.text = currentItem.ItemDescription;
        if (itemRawImage != null) itemRawImage.texture = currentItem.ItemImage;

        // For the string[] array, we just print each detail on a new line
        if (detailsText != null)
        {
            if (currentItem.ItemDetails != null && currentItem.ItemDetails.Length > 0)
            {
                detailsText.text = string.Join("\n- ", currentItem.ItemDetails);
            }
            else
            {
                detailsText.text = ""; // Clear it if the array is empty
            }
        }
    }
}