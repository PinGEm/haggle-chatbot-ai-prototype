using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/ItemScriptableObject", order = 1)]
public class ItemScriptableObject : ScriptableObject
{
    [Header("Item Details")]
    
    [SerializeField] private Texture2D m_itemImage;
    [SerializeField] private string m_itemName;
    [SerializeField] private string m_itemDescription;
    [SerializeField] private string[] m_itemDetails;


    [Header("Price Details")]
    [SerializeField] private float m_itemBasePrice;
    [SerializeField] private float m_itemBaseAskPrice;


    public Texture2D ItemImage { get { return m_itemImage; } }
    public string ItemName { get { return m_itemName; } }
    public string ItemDescription { get { return m_itemDescription; } }
    public string[] ItemDetails { get { return m_itemDetails; } }
    public float ItemBasePrice { get { return m_itemBasePrice; } }
    public float ItemBaseAskPrice { get { return m_itemBaseAskPrice; } }


    private int _itemID;
}
