using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(LayoutElement))]
public class MaxWidthClamp : MonoBehaviour
{
    public float maxWidth = 720f;
    private TextMeshProUGUI textMesh;
    private LayoutElement layoutElement;

    void Update()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshProUGUI>();
        if (layoutElement == null) layoutElement = GetComponent<LayoutElement>();

        // If the text wants to be wider than 720, force it to wrap.
        // If it's short like "hi", let it stay small!
        if (textMesh.preferredWidth > maxWidth)
        {
            layoutElement.preferredWidth = maxWidth;
        }
        else
        {
            layoutElement.preferredWidth = -1; // -1 means "turn off preferred width"
        }
    }
}