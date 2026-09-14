using System.Collections;
using TMPro;
using UnityEngine;

//Brief "Game Saved" / "Save Failed" popup. Decoupled from whatever triggered the save - just subscribes to
//SaveManager's existing OnSaveCompleted/OnSaveFailed events, so it also covers auto-saves for free.
public class SaveToastUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine activeRoutine;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnSaveCompleted += HandleSaveCompleted;
            SaveManager.Instance.OnSaveFailed += HandleSaveFailed;
        }
    }

    private void OnDisable()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnSaveCompleted -= HandleSaveCompleted;
            SaveManager.Instance.OnSaveFailed -= HandleSaveFailed;
        }
    }

    private void HandleSaveCompleted(string slotId) => Display("Game Saved");
    private void HandleSaveFailed(string slotId) => Display("Save Failed");

    private void Display(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowThenHide());
    }

    private IEnumerator ShowThenHide()
    {
        yield return Fade(1f, fadeDuration);
        yield return new WaitForSecondsRealtime(displayDuration);
        yield return Fade(0f, fadeDuration);
        activeRoutine = null;
    }

    private IEnumerator Fade(float target, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float start = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, duration <= 0f ? 1f : elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = target;
    }
}
