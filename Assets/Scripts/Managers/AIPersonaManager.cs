using System;
using TMPro;
using UnityEngine;

// Note: This class is teritiary and only serves
// to change AI personalities in the demo
// May be subject to deletion

public class AIPersonaManager : MonoBehaviour
{
    private PersonalityScriptableObject[] _aiPersonas;
    private PersonalityScriptableObject _selectedPersona;
    private int _startingAIAskingPrice;

    public int StartingAIAskingPrice { get { return _startingAIAskingPrice; } }
    public PersonalityScriptableObject SelectedPersona { get { return _selectedPersona; } }

    [SerializeField] private TextMeshProUGUI _nameLabel;
    bool _personaCheck = true;

    private ItemManager _itemManager;


    void Awake()
    {
        LoadAllPersona();
        SelectRandomPersona();

        _nameLabel.text = _selectedPersona.name;
        Debug.Log("Selected Personality This Run: " + _selectedPersona.ToString());

        string personalityTraits = "";
        string speechStyles = "";
        foreach(string trait in _selectedPersona.PersonalityTraits) personalityTraits += $"-{trait}\r\n";
        foreach (string style in _selectedPersona.SpeechStyles) speechStyles += $"-{style}\r\n";

        Debug.Log("Personality Description: " + _selectedPersona.PersonalityDescription + "\r\n" + 
            "Personality Traits: " + personalityTraits + "\r\n" + "Speech Styles: " + speechStyles);
    }

    void LoadAllPersona()
    {
        _aiPersonas = Resources.LoadAll<PersonalityScriptableObject>("AIPersonalities");

        if (_aiPersonas.Length == 0)
        {
            _personaCheck = false;
            Debug.LogWarning("No personalities found in Resources/AIPersonalities!");
        }
    }

    void SelectRandomPersona()
    {
        if (!_personaCheck) return;
        string personaList = "";

        foreach (var persona in _aiPersonas)
        {
            personaList += persona + ", ";
        }

        Debug.Log(personaList);

        int randIndex = UnityEngine.Random.Range(0, _aiPersonas.Length);
        _selectedPersona = _aiPersonas[randIndex];
    }

    public void SetStartingPrice()
    {
        _itemManager = GameManager.Instance.itemManager;

        _startingAIAskingPrice = Math.Max((int)(_itemManager.SelectedItem.ItemBaseAskPrice *
               (SelectedPersona.AskingPriceRate + (UnityEngine.Random.Range(-SelectedPersona.AskingRateVariation, SelectedPersona.AskingRateVariation)))), (int)(_itemManager.SelectedItem.ItemBaseAskPrice));
    } 
}
