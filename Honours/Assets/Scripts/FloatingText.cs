using UnityEngine;
using System.Collections;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    public TextMeshProUGUI textMesh;
    private Color startColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        startColor = textMesh.color;
    }

    public void SetText(string text)
    {
        textMesh.text = text;
        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 1 - (elapsedTime / fadeDuration)); // Fade out
            yield return null;
        }

        Destroy(gameObject); // Destroy the text after fading out
    }
}
