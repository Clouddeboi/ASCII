using System;
using UnityEngine;

[RequireComponent(typeof(EnemyInstance))]
public class EnemyDetection : MonoBehaviour
{
    [SerializeField] private float tickInterval = 0.15f;
    [SerializeField] private Vector3 eyeOffset = new Vector3(0f, 1.6f, 0f);

    private EnemyDetectionData data;
    private Transform player;
    private PlayerMovement playerMovement;
    private float tickTimer;
    private float sightTimer;
    private float memoryTimer;

    public bool CanSeePlayer { get; private set; }
    public bool CanHearPlayer { get; private set; }
    public bool IsAlerted { get; private set; }
    public Vector3? LastKnownPlayerPosition { get; private set; }
    public Transform PlayerTransform => player;

    public event Action OnPlayerDetected;
    public event Action OnPlayerLost;
    public event Action<Vector3> OnSoundHeard;

    private void Awake()
    {
        data = GetComponent<EnemyInstance>().Data.detection;
    }

    private void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;
        float dt = tickTimer;
        tickTimer = 0f;

        if (!ResolvePlayer()) return;

        Evaluate(dt);
    }

    private bool ResolvePlayer()
    {
        if (player != null) return true;
        if (PlayerHealth.Instance == null) return false;

        player = PlayerHealth.Instance.transform.root;
        playerMovement = player.GetComponentInChildren<PlayerMovement>();
        return true;
    }

    private void Evaluate(float dt)
    {
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead)
        {
            CanSeePlayer = false;
            CanHearPlayer = false;
            sightTimer = 0f;
            memoryTimer = 0f;

            if (IsAlerted)
            {
                IsAlerted = false;
                Debug.Log($"[EnemyDetection] '{name}' disengaged - player is dead.", this);
                OnPlayerLost?.Invoke();
            }

            return;
        }

        CanSeePlayer = data.useVision && EvaluateVision();
        CanHearPlayer = data.useHearing && EvaluateHearing();

        if (CanSeePlayer)
        {
            sightTimer += dt;
            LastKnownPlayerPosition = player.position;
            memoryTimer = data.searchMemoryDuration;

            if (!IsAlerted && sightTimer >= data.detectionDelay)
            {
                IsAlerted = true;
                Debug.Log($"[EnemyDetection] '{name}' spotted the player.", this);
                OnPlayerDetected?.Invoke();
            }

            return;
        }

        sightTimer = 0f;

        if (CanHearPlayer)
        {
            LastKnownPlayerPosition = player.position;
            memoryTimer = data.searchMemoryDuration;

            if (!IsAlerted)
            {
                IsAlerted = true;
                Debug.Log($"[EnemyDetection] '{name}' heard the player.", this);
                OnSoundHeard?.Invoke(player.position);
            }

            return;
        }

        if (memoryTimer > 0f)
        {
            memoryTimer -= dt;
            if (memoryTimer <= 0f && IsAlerted)
            {
                IsAlerted = false;
                Debug.Log($"[EnemyDetection] '{name}' lost track of the player.", this);
                OnPlayerLost?.Invoke();
            }
        }
    }

    private bool EvaluateVision()
    {
        Vector3 eyePosition = transform.position + eyeOffset;
        Vector3 toPlayer = player.position - eyePosition;

        if (toPlayer.sqrMagnitude > data.visionRange * data.visionRange)
            return false;

        // Flatten for the FOV check - otherwise standing close (steep vertical angle from the eye height) falsely fails.
        Vector3 flatToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (Vector3.Angle(transform.forward, flatToPlayer) > data.fieldOfViewAngle * 0.5f)
            return false;

        if (Physics.Linecast(eyePosition, player.position, out RaycastHit hit, data.lineOfSightObstruction))
        {
            // Ignore hits on the enemy's own collider(s) or the player's own collider - only a third-party obstruction blocks sight.
            if (hit.transform.root != player && hit.transform.root != transform.root)
                return false;
        }

        return true;
    }

    private bool EvaluateHearing()
    {
        if (playerMovement == null) return false;

        float sqrDistance = (player.position - transform.position).sqrMagnitude;
        if (sqrDistance > data.hearingRange * data.hearingRange)
            return false;

        return playerMovement.FlatSpeed >= data.hearingSensitivity;
    }
}
