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
        LoadInventory(); // <-- NEW: Load our saved items right away!
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

        SaveInventory(); // <-- NEW: Save the game whenever we accept a deal!
    }

    // --- SAVE / LOAD SYSTEM ---

    private void SaveInventory()
    {
        // 1. Create a blank string to hold our list
        System.Collections.Generic.List<string> itemNames = new System.Collections.Generic.List<string>();

        // 2. Add the unique asset name of every item we own to that list
        foreach (ItemScriptableObject item in _ownedItems)
        {
            itemNames.Add(item.name);
        }

        // 3. Mash them all together separated by a comma (e.g. "SwordItem,DiamondRingItem,BookItem")
        string saveString = string.Join(",", itemNames);

        // 4. Save it to the player's hard drive!
        PlayerPrefs.SetString("SavedBackpack", saveString);
        PlayerPrefs.Save();

        Debug.Log("Game Saved! Current Backpack: " + saveString);
    }

    private void LoadInventory()
    {
        _ownedItems.Clear(); // Empty the backpack just in case

        // Check if the player actually has a save file
        if (PlayerPrefs.HasKey("SavedBackpack"))
        {
            // 1. Grab the big string of names we saved
            string saveString = PlayerPrefs.GetString("SavedBackpack");

            // If it's completely empty, do nothing
            if (string.IsNullOrEmpty(saveString)) return;

            // 2. Chop the string back up into individual names
            string[] loadedNames = saveString.Split(',');

            // 3. For every name we loaded, find the matching item in Resources and give it to the player
            foreach (string loadedName in loadedNames)
            {
                foreach (ItemScriptableObject item in _allItems)
                {
                    // If the names match, add it to our actual inventory!
                    if (item.name == loadedName)
                    {
                        _ownedItems.Add(item);
                        break;
                    }
                }
            }
            Debug.Log("Save Loaded! Found " + _ownedItems.Count + " items.");
        }
    }
}
