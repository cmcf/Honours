using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public Image fadeImage;  
    public float fadeDuration = 1.5f; 

    private void Start()
    {
        // Make sure the fade image starts completely transparent
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(Fade(0f, 1f, onComplete));
    }

    // Call this function to fade in
    public void FadeIn(System.Action onComplete = null)
    {
        StartCoroutine(Fade(1f, 0f, onComplete));
    }

    // Coroutine to handle the fade effect
    private IEnumerator Fade(float startAlpha, float endAlpha, System.Action onComplete)
    {
        float elapsedTime = 0f;
        fadeImage.color = new Color(0, 0, 0, startAlpha);

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalize the fade
        fadeImage.color = new Color(0, 0, 0, endAlpha);
        onComplete?.Invoke();
    }
}
