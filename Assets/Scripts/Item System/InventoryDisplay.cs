using System.Collections.Generic;
using UnityEngine;

public class InventoryDisplay : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private Transform _contentContainer; // Drag the 'Content' object here
    [SerializeField] private GameObject _itemPrefab;       // Drag your Item Prefab here

    private void Start()
    {
        DisplayInventory();
    }

    public void DisplayInventory()
    {
        // 1. Get the items already loaded by your ItemManager
        // We use GameManager.Instance to reach the itemManager you already have
        if (GameManager.Instance == null || GameManager.Instance.ItemManager == null) return;

        ItemScriptableObject[] allItems = GameManager.Instance.ItemManager.GetAllItems();

        // 2. Clear existing UI items in the grid
        foreach (Transform child in _contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. Spawn a UI slot for every item found in Resources/Items
        foreach (ItemScriptableObject item in allItems)
        {
            GameObject itemSlot = Instantiate(_itemPrefab, _contentContainer);

            // 4. Update the prefab with the item's data
            // We'll update your ItemUIDisplay script next to handle this
            ItemUIDisplay uiLogic = itemSlot.GetComponent<ItemUIDisplay>();
            if (uiLogic != null)
            {
                uiLogic.InitializeSlot(item);
            }
        }
    }
}