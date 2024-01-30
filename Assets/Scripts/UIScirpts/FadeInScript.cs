using UnityEngine;
using System.Collections;

public class MenuFadeIn : MonoBehaviour
{
    public float fadeDuration = 1.0f; // Duration of the fade-in effect
    public float fadeDelay = 1.0f; // Duration of the fade-in effect
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("Canvas Group component not found!");
            return;
        }

        // Start with the menu fully transparent
        canvasGroup.alpha = 0.0f;

        // Start the fade-in effect after a delay
        StartCoroutine(FadeInAfterDelay(fadeDelay)); // 1 second delay
    }

    private IEnumerator FadeInAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Fade in
        float elapsedTime = 0.0f;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = elapsedTime / fadeDuration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1.0f; // Ensure it's fully visible
    }
}
