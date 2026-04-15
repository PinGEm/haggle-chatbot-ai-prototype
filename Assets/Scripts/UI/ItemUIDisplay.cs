using UnityEngine;
using UnityEngine.UI; // Needed for RawImage
using TMPro; // Needed for TextMeshProUGUI

public class ItemUIDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _detailsText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private RawImage _itemRawImage;


    public void UpdateItemDisplay()
    {
        // 1. Make sure the GameManager and ItemManager exist
        if (GameManager.Instance == null || GameManager.Instance.ItemManager == null) return;

        // 2. Grab the item that was already selected
        ItemScriptableObject currentItem = GameManager.Instance.ItemManager.SelectedItem;
        if (currentItem == null) return;

        // 3. Paste the data directly into your 4 UI slots exactly as they are
        if (_nameText != null) _nameText.text = currentItem.ItemName;
        if (_priceText != null) _priceText.text = "$" + GameManager.Instance.AiManager.StartingAIAskingPrice.ToString();
        if (_descriptionText != null) _descriptionText.text = currentItem.ItemDescription;
        if (_itemRawImage != null) _itemRawImage.texture = currentItem.ItemImage;

        // For the string[] array, we just print each detail on a new line
        if (_detailsText != null)
        {
            if (currentItem.ItemDetails != null && currentItem.ItemDetails.Length > 0)
            {
                _detailsText.text = string.Join("\n- ", currentItem.ItemDetails);
            }
            else
            {
                _detailsText.text = ""; // Clear it if the array is empty
            }
        }
    }
}