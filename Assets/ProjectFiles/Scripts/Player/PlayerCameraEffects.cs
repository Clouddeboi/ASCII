using UnityEngine;

//Applies head bob and trauma based camera shake on top of MoveCamera/PlayerCam's position & rotation.
//Runs in LateUpdate so it always layers on top of that frame's base camera transform.
public class PlayerCameraEffects : MonoBehaviour
{
    public static PlayerCameraEffects Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Head Bob")]
    [SerializeField] private bool headBobEnabled = true;
    [SerializeField] private float bobFrequency = 1.8f;
    [SerializeField] private float bobVerticalAmplitude = 0.05f;
    [SerializeField] private float bobHorizontalAmplitude = 0.035f;
    [SerializeField] private float bobSmoothing = 8f;
    [SerializeField] private float minSpeedForBob = 0.5f;
    [SerializeField, Range(0f, 3f)] private float bobIntensity = 1f;

    [Header("Camera Shake")]
    [SerializeField] private bool cameraShakeEnabled = true;
    [SerializeField] private float traumaDecayPerSecond = 1.2f;
    [SerializeField] private float maxShakeAngle = 4f;
    [SerializeField] private float maxShakePositionOffset = 0.08f;
    [SerializeField] private float shakeNoiseFrequency = 20f;
    [SerializeField, Range(0f, 3f)] private float shakeIntensity = 1f;

    private float bobTimer;
    private Vector3 currentBobOffset;
    private float trauma;
    private float noiseSeedX, noiseSeedY, noiseSeedAngle;

    //Tracks last frame's applied world-space offset/roll so it can be undone before applying this frame's,
    //otherwise the offset/roll compound on top of themselves every frame instead of oscillating around base position.
    //This matters a lot while the game is paused (Time.timeScale = 0): LateUpdate still runs every real frame even
    //though Time.deltaTime/Time.time are frozen, so an uncorrected "+=" would spin/drift the camera indefinitely.
    private Vector3 lastAppliedOffset;
    private float lastAppliedRoll;

    public float CurrentTrauma => trauma;

    private void Awake()
    {
        Instance = this;
        noiseSeedX = Random.Range(0f, 100f);
        noiseSeedY = Random.Range(0f, 100f);
        noiseSeedAngle = Random.Range(0f, 100f);
    }

    private void OnDisable()
    {
        //Cleanly restore the base transform so disabling mid-effect (e.g. entering a paused/menu state) never leaves
        //the camera permanently offset/rotated.
        transform.position -= lastAppliedOffset;
        transform.rotation *= Quaternion.Euler(0f, 0f, -lastAppliedRoll);
        lastAppliedOffset = Vector3.zero;
        lastAppliedRoll = 0f;
        bobTimer = 0f;
        currentBobOffset = Vector3.zero;
        trauma = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void LateUpdate()
    {
        //Undo last frame's offset/roll before recomputing this frame's, so effects don't accumulate.
        transform.position -= lastAppliedOffset;
        transform.rotation *= Quaternion.Euler(0f, 0f, -lastAppliedRoll);

        Vector3 bobOffset = ApplyHeadBob();
        Vector3 shakeOffset = ApplyCameraShake(out float roll);

        Vector3 totalOffset = transform.right * (bobOffset.x + shakeOffset.x) + transform.up * (bobOffset.y + shakeOffset.y);
        transform.position += totalOffset;
        transform.rotation *= Quaternion.Euler(0f, 0f, roll);

        lastAppliedOffset = totalOffset;
        lastAppliedRoll = roll;
    }

    private Vector3 ApplyHeadBob()
    {
        Vector3 targetOffset = Vector3.zero;

        bool isWalking = headBobEnabled && playerMovement != null &&
                          playerMovement.IsGrounded && playerMovement.FlatSpeed > minSpeedForBob;

        if (isWalking)
        {
            bobTimer += Time.deltaTime * bobFrequency * (playerMovement.FlatSpeed / Mathf.Max(playerMovement.MoveSpeed, 0.01f));

            float vertical = Mathf.Sin(bobTimer * 2f) * bobVerticalAmplitude;
            float horizontal = Mathf.Cos(bobTimer) * bobHorizontalAmplitude;
            targetOffset = new Vector3(horizontal, vertical, 0f) * bobIntensity;
        }
        else
        {
            bobTimer = 0f;
        }

        currentBobOffset = Vector3.Lerp(currentBobOffset, targetOffset, Time.deltaTime * bobSmoothing);
        return currentBobOffset;
    }

    private Vector3 ApplyCameraShake(out float roll)
    {
        roll = 0f;

        if (cameraShakeEnabled)
            trauma = Mathf.Clamp01(trauma - traumaDecayPerSecond * Time.deltaTime);
        else
            trauma = 0f;

        if (trauma <= 0f)
            return Vector3.zero;

        float shake = trauma * trauma;
        float time = Time.time * shakeNoiseFrequency;

        roll = maxShakeAngle * shake * shakeIntensity * (Mathf.PerlinNoise(noiseSeedAngle, time) * 2f - 1f);
        float offsetX = maxShakePositionOffset * shake * shakeIntensity * (Mathf.PerlinNoise(noiseSeedX, time) * 2f - 1f);
        float offsetY = maxShakePositionOffset * shake * shakeIntensity * (Mathf.PerlinNoise(noiseSeedY, time) * 2f - 1f);

        return new Vector3(offsetX, offsetY, 0f);
    }

    //Call from gameplay events (hard landing, taking damage, explosions, etc). 0-1, additive and clamped.
    public void AddTrauma(float amount)
    {
        if (!cameraShakeEnabled) return;
        trauma = Mathf.Clamp01(trauma + amount);
    }

    //Hooks for a future player settings menu.
    public void SetHeadBobEnabled(bool value) => headBobEnabled = value;
    public void SetCameraShakeEnabled(bool value) => cameraShakeEnabled = value;
    public void SetHeadBobIntensity(float value) => bobIntensity = Mathf.Max(0f, value);
    public void SetShakeIntensity(float value) => shakeIntensity = Mathf.Max(0f, value);
}
