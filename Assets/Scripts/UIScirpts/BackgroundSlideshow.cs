using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundSlideshow : MonoBehaviour
{
    public Sprite[] slides; // Array of slides
    public float changeInterval = 5f; // Time in seconds for each slide
    public float fadeDuration = 1f; // Duration of the fade

    private Image backgroundImage;
    private int currentSlideIndex = 0;

    void Start()
    {
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
        {
            Debug.LogError("Error: No Image component found on the GameObject.");
            return;
        }

        if (slides.Length == 0)
        {
            Debug.LogError("Error: No slides assigned.");
            return;
        }

        // Start with the first slide
        backgroundImage.sprite = slides[currentSlideIndex];
        backgroundImage.color = new Color(0f, 0f, 0f, 1f); // Start with black

        // Start the slideshow
        StartCoroutine(SlideShow());
    }

    IEnumerator SlideShow()
    {
        while (true)
        {
            // Fade in the current slide
            yield return StartCoroutine(FadeImage(true));

            // Wait for the specified interval
            yield return new WaitForSeconds(changeInterval);

            // Fade to black
            yield return StartCoroutine(FadeImage(false));

            // Change slide
            currentSlideIndex = (currentSlideIndex + 1) % slides.Length;
            backgroundImage.sprite = slides[currentSlideIndex];
        }
    }

    IEnumerator FadeImage(bool fadeIn)
    {
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float currentTime = 0f;

        Color startColor = fadeIn ? Color.black : new Color(1f, 1f, 1f, 1f);
        Color endColor = fadeIn ? new Color(1f, 1f, 1f, 1f) : Color.black;

        while (currentTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, currentTime / fadeDuration);
            backgroundImage.color = Color.Lerp(startColor, endColor, currentTime / fadeDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }

        backgroundImage.color = endColor;
    }
}
