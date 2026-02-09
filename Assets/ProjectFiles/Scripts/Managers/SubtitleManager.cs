using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject subtitlePanel;
    public TMP_Text subtitleText;

    [Header("Settings")]
    public float displayDuration = 3f; //Default duration if no audio
    public float fadeInDuration = 0.2f;
    public float fadeOutDuration = 0.3f;
    public bool autoCalculateDuration = true; //Calculate based on text length

    private CanvasGroup canvasGroup;
    private Coroutine currentSubtitleCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //Get or add CanvasGroup for fading
        canvasGroup = subtitlePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = subtitlePanel.AddComponent<CanvasGroup>();
        }

        //Start hidden
        HideImmediate();
    }

    public void ShowSubtitle(string text, float? customDuration = null)
    {
        //Stop any existing subtitle
        if (currentSubtitleCoroutine != null)
        {
            StopCoroutine(currentSubtitleCoroutine);
        }

        //Calculate duration
        float duration = customDuration ?? CalculateDuration(text);

        //Start new subtitle
        currentSubtitleCoroutine = StartCoroutine(DisplaySubtitleRoutine(text, duration));
    }

    public void ShowSubtitleWithAudio(string text, AudioClip clip)
    {
        //Use audio clip length as duration
        float duration = clip != null ? clip.length : CalculateDuration(text);
        ShowSubtitle(text, duration);
    }

    private float CalculateDuration(string text)
    {
        if (!autoCalculateDuration)
            return displayDuration;

        //Estimate reading time: average 200 words per minute
        //Plus some base time for short messages
        float wordsPerSecond = 200f / 60f;
        int wordCount = text.Split(' ').Length;
        float estimatedTime = Mathf.Max(2f, wordCount / wordsPerSecond + 0.5f);

        return estimatedTime;
    }

    private IEnumerator DisplaySubtitleRoutine(string text, float duration)
    {
        //Set text
        subtitleText.text = text;
        subtitlePanel.SetActive(true);

        //Fade in
        yield return FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration);

        //Wait for duration
        yield return new WaitForSeconds(duration);

        //Fade out
        yield return FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration);

        //Hide
        subtitlePanel.SetActive(false);
        currentSubtitleCoroutine = null;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        group.alpha = endAlpha;
    }

    public void HideSubtitle()
    {
        if (currentSubtitleCoroutine != null)
        {
            StopCoroutine(currentSubtitleCoroutine);
            currentSubtitleCoroutine = null;
        }

        StartCoroutine(HideSubtitleRoutine());
    }

    private IEnumerator HideSubtitleRoutine()
    {
        yield return FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, fadeOutDuration);
        subtitlePanel.SetActive(false);
    }

    private void HideImmediate()
    {
        subtitlePanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }
}