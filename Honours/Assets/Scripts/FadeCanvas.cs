using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeCanvas : MonoBehaviour
{
    public static FadeCanvas Instance;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public IEnumerator FadeOut()
    {
        yield return StartCoroutine(Fade(0f, 1f)); // Fade to black
    }

    public IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(1f, 0f)); // Fade from black
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
    }
}
