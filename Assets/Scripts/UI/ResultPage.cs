using TMPro;
using UnityEngine;

public class ResultPage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultLabel;
    [SerializeField] private GameObject _resultUI;

    public void ShowResult(string result, Color color)
    {
        _resultUI.SetActive(true);
        _resultLabel.text = result;

        _resultLabel.color = color;
    }
}
