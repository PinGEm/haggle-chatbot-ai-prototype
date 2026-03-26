using System.Collections;
using UnityEngine;
using TMPro;

public class TypingAnimator : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI dotText;

    [Header("Settings")]
    public float delayBetweenDots = 0.3f; // How fast the dots appear

    private void OnEnable()
    {
        // OnEnable runs the moment this prefab is spawned into the chat!
        StartCoroutine(AnimateDots());
    }

    private IEnumerator AnimateDots()
    {
        // This 'while(true)' loop will run endlessly as long as the chat bubble exists
        while (true)
        {
            dotText.text = ".";
            yield return new WaitForSeconds(delayBetweenDots);

            dotText.text = ". .";
            yield return new WaitForSeconds(delayBetweenDots);

            dotText.text = ". . .";
            yield return new WaitForSeconds(delayBetweenDots);
        }
    }
}