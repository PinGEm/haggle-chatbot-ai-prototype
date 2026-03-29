using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Manager Prefabs")]
    [SerializeField] private GameObject _itemManagerPrefab;
    [SerializeField] private GameObject _aiManagerPrefab;
    public static GameManager Instance { get; private set; }


    // Getters
    public ItemManager itemManager { get; private set; }
    public AIPersonaManager aiManager { get; private set; }

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

        itemManager = SpawnManagerIfMissing<ItemManager>(_itemManagerPrefab);
        aiManager = SpawnManagerIfMissing<AIPersonaManager>(_aiManagerPrefab);
    }

    private T SpawnManagerIfMissing<T>(GameObject manager_prefab) where T : MonoBehaviour
    {
        T manager = FindAnyObjectByType<T>();

        if (manager == null)
        {
            manager = Instantiate(manager_prefab).GetComponent<T>();
        }

        return manager;
    }
}
