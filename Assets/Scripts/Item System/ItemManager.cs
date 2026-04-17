using UnityEngine;

public class ItemManager : MonoBehaviour
{

    private ItemScriptableObject[] _allItems;
    private ItemScriptableObject _selectedItem;

    // This is the player's actual backpack
    private System.Collections.Generic.List<ItemScriptableObject> _ownedItems = new System.Collections.Generic.List<ItemScriptableObject>();
    public ItemScriptableObject SelectedItem {  get { return _selectedItem; } }

    bool _itemCheck = true;

    void Awake()
    {
        LoadAllItems();
        SelectRandomItem();

        Debug.Log("Selected Item This Run: " + SelectedItem.ToString());
    }

    void LoadAllItems()
    {
        _allItems = Resources.LoadAll<ItemScriptableObject>("Items");

        if (_allItems.Length == 0)
        {
            _itemCheck = false;
            Debug.LogWarning("No items found in Resources/Items!");
        }
    }

    public ItemScriptableObject[] GetAllItems()
    {
        return _allItems;
    }

    void SelectRandomItem()
    {
        if (!_itemCheck) return;
        string itemList = "";

        foreach (var item in _allItems)
        {
            itemList += item + ", ";
        }

        Debug.Log(itemList);

        int randIndex = Random.Range(0,_allItems.Length);
        _selectedItem = _allItems[randIndex];
    }
    public System.Collections.Generic.List<ItemScriptableObject> GetOwnedItems()
    {
        return _ownedItems;
    }

    public void AddItemToInventory(ItemScriptableObject itemToAdd)
    {
        _ownedItems.Add(itemToAdd);
        Debug.Log("Cheat Button: Added " + itemToAdd.ItemName + " to the backpack!");
    }
}
