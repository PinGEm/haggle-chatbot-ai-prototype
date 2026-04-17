using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Drag and Drop in Inspector")]
    [SerializeField] private Transform _contentContainer; // The Grid
    [SerializeField] private GameObject _itemSlotPrefab;  // Your Prefab

    private void Start()
    {
        BuildInventory();
    }

    public void BuildInventory()
    {
        // 1. Ask the GameManager for the ItemManager, and get the list of items
        var allItems = GameManager.Instance.ItemManager.GetOwnedItems();

        // 2. Clear out any old test UI slots so we don't have duplicates
        foreach (Transform child in _contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. For every single item in the list, spawn a prefab
        foreach (ItemScriptableObject item in allItems)
        {
            // Spawn the prefab inside the Grid
            GameObject newSlot = Instantiate(_itemSlotPrefab, _contentContainer);

            // Tell the prefab's UI script what item it's supposed to display
            ItemUIDisplay slotDisplay = newSlot.GetComponent<ItemUIDisplay>();
            if (slotDisplay != null)
            {
                slotDisplay.InitializeSlot(item);
            }
        }
    }

    // Put this at the bottom of InventoryUI.cs
    public void TestAddCurrentItem()
    {
        // 1. Find out what item the GameManager currently has selected
        ItemScriptableObject currentItem = GameManager.Instance.ItemManager.SelectedItem;

        if (currentItem != null)
        {
            // 2. Shove it in the bag
            GameManager.Instance.ItemManager.AddItemToInventory(currentItem);

            // 3. Redraw the inventory so the new item pops up on screen
            BuildInventory();
        }
    }
}