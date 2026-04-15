using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(LayoutElement))]
public class MaxWidthClamp : MonoBehaviour
{
    [SerializeField] private float _maxWidth = 720f;
    private TextMeshProUGUI _textMesh;
    private LayoutElement _layoutElement;

    void Start()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _layoutElement = GetComponent<LayoutElement>();

        ClampWidth();
    }

    void LateUpdate()
    {
        ClampWidth();
    }

    public void ClampWidth()
    {
        if (_textMesh == null || _layoutElement == null) return;


        if (_textMesh.preferredWidth > _maxWidth)
        {
            if (_layoutElement.preferredWidth != _maxWidth) _layoutElement.preferredWidth = _maxWidth;
        }
        else
        {
            if (_layoutElement.preferredWidth != -1) _layoutElement.preferredWidth = -1;
        }
    }
}