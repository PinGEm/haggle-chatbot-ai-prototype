using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Manager Prefabs")]
    [SerializeField] private GameObject _itemManagerPrefab;
    [SerializeField] private GameObject _aiManagerPrefab;
    public static GameManager Instance { get; private set; }


    // Getters
    public ItemManager ItemManager { get; private set; }
    public AIPersonaManager AiManager { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        ItemManager = SpawnManagerIfMissing<ItemManager>(_itemManagerPrefab);
        AiManager = SpawnManagerIfMissing<AIPersonaManager>(_aiManagerPrefab);


        //aiManager.SetStartingPrice();
    }

    void Start()
    {
        InitializeGame();
    }

    IEnumerator InitializeDelayed(ItemUIDisplay ui)
    {
        Debug.Log($"Item: {GameManager.Instance.ItemManager.SelectedItem}");
        yield return new WaitUntil(() => !string.IsNullOrWhiteSpace(GameManager.Instance.ItemManager.SelectedItem.ItemName));
        Debug.Log("Updating Item Display");
        ui.UpdateItemDisplay();
    }
    void InitializeGame()
    {
        Debug.Log("Initializing Game");
        AiManager.SetStartingPrice();

        var ui = FindAnyObjectByType<ItemUIDisplay>();
        if (ui != null)
        {
            Debug.Log($"Item UI Display Found: {ui.gameObject.name}");
            StartCoroutine(InitializeDelayed(ui));
            
        }
    }

    private T SpawnManagerIfMissing<T>(GameObject managerPrefab) where T : MonoBehaviour
    {
        T manager = FindAnyObjectByType<T>();

        if (manager == null)
        {
            manager = Instantiate(managerPrefab).GetComponent<T>();
        }

        return manager;
    }
}
