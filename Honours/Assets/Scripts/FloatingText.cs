using UnityEngine;
using System.Collections;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeDuration = 0.5f;
    public TextMeshProUGUI textMesh;
    private Color startColour;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();  
        startColour = textMesh.color;  
    }

    public void SetText(string text, Color textColor)
    {
        textMesh.text = text;

        if (textMesh != null)
        {
            textMesh.color = textColor;  
        }

        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = startPos + Vector3.up * (moveSpeed * Time.deltaTime); // Move up
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1 - (elapsedTime / fadeDuration)); // Fade out
            yield return null;
        }

        Destroy(gameObject);  // Destroy the floating text after fade out
    }
}
