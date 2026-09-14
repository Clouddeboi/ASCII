using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//Persistent (DontDestroyOnLoad) full-screen black overlay used to mask scene loads during LoadGame/StartNewGame
//so the player never sees a hard cut or half-restored scene. Auto-builds its own overlay if none is placed
//in a scene, same convenience pattern as InteractableRegistry/SaveableRegistry's lazy auto-create.
public class ScreenFader : MonoBehaviour
{
    private static ScreenFader instance;
    private static bool shuttingDown;

    //See SaveManager.ResetStatics - same fix, required when domain reload is disabled on Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        shuttingDown = false;
    }

    public static ScreenFader Instance
    {
        get
        {
            if (shuttingDown)
                return null;

            if (instance == null)
            {
                instance = FindAnyObjectByType<ScreenFader>();
                if (instance == null)
                    instance = CreateDefault();
            }
            return instance;
        }
    }

    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine activeFade;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy() => shuttingDown = true;
    private void OnApplicationQuit() => shuttingDown = true;

    public void FadeOut(float duration, Action onComplete = null) => StartFade(1f, duration, onComplete);
    public void FadeIn(float duration, Action onComplete = null) => StartFade(0f, duration, onComplete);

    private void StartFade(float targetAlpha, float duration, Action onComplete)
    {
        if (activeFade != null)
            StopCoroutine(activeFade);
        activeFade = StartCoroutine(FadeRoutine(targetAlpha, duration, onComplete));
    }

    //Uses unscaled time so fades still play while Time.timeScale is 0 (pause menu).
    private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete)
    {
        canvasGroup.blocksRaycasts = true;
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, duration <= 0f ? 1f : elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0.99f;
        activeFade = null;
        onComplete?.Invoke();
    }

    private static ScreenFader CreateDefault()
    {
        var go = new GameObject("ScreenFader");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        var imageGO = new GameObject("Overlay");
        imageGO.transform.SetParent(go.transform, false);
        var image = imageGO.AddComponent<Image>();
        image.color = Color.black;
        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CanvasGroup group = go.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        ScreenFader fader = go.AddComponent<ScreenFader>();
        fader.canvasGroup = group;
        return fader;
    }
}
