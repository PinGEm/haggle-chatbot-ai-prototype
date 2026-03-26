using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(LayoutElement))]
public class MaxWidthClamp : MonoBehaviour
{
    [SerializeField] private float maxWidth = 720f;
    private TextMeshProUGUI textMesh;
    private LayoutElement layoutElement;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        layoutElement = GetComponent<LayoutElement>();

        ClampWidth();
    }

    void LateUpdate()
    {
        ClampWidth();
    }

    void ClampWidth()
    {
        if (textMesh == null || layoutElement == null) return;


        if (textMesh.preferredWidth > maxWidth)
        {
            if (layoutElement.preferredWidth != maxWidth) layoutElement.preferredWidth = maxWidth;
        }
        else
        {
            if (layoutElement.preferredWidth != -1) layoutElement.preferredWidth = -1;
        }
    }
}