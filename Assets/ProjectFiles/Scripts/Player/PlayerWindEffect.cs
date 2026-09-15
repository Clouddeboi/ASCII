using UnityEngine;

//Drives a wind UI overlay + intensifying loop SFX based on the player's current speed (falling fast, sliding down steep slopes, sprinting, etc).
public class PlayerWindEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("UI Overlay")]
    [SerializeField] private CanvasGroup windOverlay;
    [SerializeField] private float maxOverlayAlpha = 0.85f;

    [Header("Audio")]
    [SerializeField] private AudioSource windAudioSource;
    [SerializeField] private AudioClip windLoopClip;
    [SerializeField] private float maxWindVolume = 1f;
    [SerializeField] private Vector2 windPitchRange = new Vector2(0.9f, 1.3f);

    [Header("Speed Thresholds")]
    [SerializeField] private float windStartSpeed = 12f;
    [SerializeField] private float windMaxSpeed = 30f;
    [SerializeField] private float intensityChangeSpeed = 4f;

    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    private float currentIntensity;

    private void Reset()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private void Update()
    {
        float speed = GetCurrentSpeed();
        float targetIntensity = Mathf.InverseLerp(windStartSpeed, windMaxSpeed, speed);
        currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, intensityChangeSpeed * Time.deltaTime);

        ApplyOverlay(currentIntensity);
        ApplyAudio(currentIntensity);

        if (debugLogging)
            Debug.Log($"[Wind] speed={speed:F2} (start={windStartSpeed}, max={windMaxSpeed}) onSteepSlope={playerMovement?.IsOnSteepSlope} intensity={currentIntensity:F2}");
    }

    private float GetCurrentSpeed()
    {
        if (playerMovement == null) return 0f;

        float flatSpeed = playerMovement.FlatSpeed;
        float fallSpeed = Mathf.Max(0f, -playerMovement.VerticalVelocity);
        return Mathf.Max(flatSpeed, fallSpeed);
    }

    private void ApplyOverlay(float intensity)
    {
        if (windOverlay == null) return;
        windOverlay.alpha = intensity * maxOverlayAlpha;
    }

    private void ApplyAudio(float intensity)
    {
        if (windAudioSource == null || windLoopClip == null) return;

        if (intensity <= 0.001f)
        {
            if (windAudioSource.isPlaying)
                windAudioSource.Stop();
            return;
        }

        if (!windAudioSource.isPlaying)
        {
            windAudioSource.clip = windLoopClip;
            windAudioSource.loop = true;
            windAudioSource.Play();
        }

        windAudioSource.volume = intensity * maxWindVolume;
        windAudioSource.pitch = Mathf.Lerp(windPitchRange.x, windPitchRange.y, intensity);
    }
}
