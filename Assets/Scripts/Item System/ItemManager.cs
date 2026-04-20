using UnityEngine;

public class ItemManager : MonoBehaviour
{

    private ItemScriptableObject[] _allItems;
    private ItemScriptableObject _selectedItem;

    public ItemScriptableObject SelectedItem {  get { return _selectedItem; } }

    bool isItemChecked = true;

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
            isItemChecked = false;
            Debug.LogWarning("No items found in Resources/Items!");
        }
    }

    void SelectRandomItem()
    {
        if (!isItemChecked) return;
        string itemList = "";

        foreach (var item in _allItems)
        {
            itemList += item + ", ";
        }

        Debug.Log(itemList);

        int randIndex = Random.Range(0,_allItems.Length);
        _selectedItem = _allItems[randIndex];
    }
}
