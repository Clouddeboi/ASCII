using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyInstance : MonoBehaviour, ISaveable
{
    [SerializeField] private EnemyData data;
    [Tooltip("Stable, scene-assigned identifier for the save system. Leave empty for enemies that don't need to persist.")]
    [SerializeField] private string saveId;

    public string RuntimeId { get; private set; }
    public bool IsShutDown { get; private set; }

    public EnemyData Data => data;
    public string DisplayName => data != null ? data.identity.displayName : name;
    public string SaveId => saveId;

    private void Awake()
    {
        if (data == null)
            Debug.LogError($"[EnemyInstance] '{name}' has no EnemyData assigned.", this);

        if (!string.IsNullOrEmpty(saveId) && SaveManager.Instance != null && SaveManager.Instance.IsEnemyDead(saveId))
        {
            Destroy(gameObject);
            return;
        }

        RuntimeId = Guid.NewGuid().ToString();
        EnemyRegistry.Instance?.Register(this);

        if (!string.IsNullOrEmpty(saveId))
            SaveableRegistry.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        EnemyRegistry.Instance?.Unregister(this);
        SaveableRegistry.Instance?.Unregister(this);
    }

    public string CaptureState()
    {
        EnemyHealth health = GetComponent<EnemyHealth>();
        var state = new EnemySaveState
        {
            position = transform.position,
            rotation = transform.rotation,
            currentHealth = health != null ? health.CurrentHealth : data.health.maxHealth,
            isShutDown = IsShutDown
        };
        return JsonUtility.ToJson(state);
    }

    public void RestoreState(string json)
    {
        EnemySaveState state = JsonUtility.FromJson<EnemySaveState>(json);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
            agent.Warp(state.position);
        else
            transform.position = state.position;
        transform.rotation = state.rotation;

        GetComponent<EnemyHealth>()?.SetHealthDirect(state.currentHealth);

        if (state.isShutDown)
        {
            IsShutDown = true;
            SetAiEnabled(false);
            GetComponent<EnemyAnimationController>()?.SetHacked(true);
        }
    }

    [Serializable]
    private struct EnemySaveState
    {
        public Vector3 position;
        public Quaternion rotation;
        public float currentHealth;
        public bool isShutDown;
    }

    //Attempts a hack, applies the configured success/failure response, and describes the outcome for terminal output.
    public bool TryHack(out string message)
    {
        EnemyTerminalData terminal = data.terminal;

        if (!terminal.hackable)
        {
            message = $"{DisplayName} IS NOT HACKABLE.";
            return false;
        }

        if (!string.IsNullOrEmpty(terminal.requiredItemId) && !Inventory.HasItem(terminal.requiredItemId))
        {
            message = $"{DisplayName}: MISSING REQUIRED HACKING TOOL.";
            return false;
        }

        bool success = UnityEngine.Random.value <= terminal.hackSuccessRate;

        if (success)
        {
            ApplyHackSuccess(terminal);
            message = DescribeSuccess(terminal.successResponse);
        }
        else
        {
            ApplyHackFailure(terminal);
            message = DescribeFailure(terminal.failureResponse);
        }

        return success;
    }

    private void ApplyHackSuccess(EnemyTerminalData terminal)
    {
        switch (terminal.successResponse)
        {
            case HackSuccessResponse.Kill:
                GetComponent<EnemyHealth>()?.Kill();
                break;
            case HackSuccessResponse.ShutDown:
                SetAiEnabled(false);
                IsShutDown = true;
                GetComponent<EnemyAnimationController>()?.SetHacked(true);
                break;
            case HackSuccessResponse.Stun:
            default:
                StartCoroutine(StunRoutine(terminal.stunDuration));
                break;
        }
    }

    private void ApplyHackFailure(EnemyTerminalData terminal)
    {
        switch (terminal.failureResponse)
        {
            case HackFailureResponse.RevealLocation:
                GetComponent<EnemyDetection>()?.ForceAlert();
                break;
            case HackFailureResponse.AlertNearbyEnemies:
                AlertNearbyEnemies();
                break;
            case HackFailureResponse.TriggerAlarm:
            default:
                Debug.Log($"[EnemyInstance] '{name}' triggered an alarm after a failed hack.", this);
                break;
        }
    }

    private void AlertNearbyEnemies()
    {
        if (EnemyRegistry.Instance == null) return;

        const float alertRadius = 15f;
        foreach (EnemyInstance other in EnemyRegistry.Instance.InRange(transform.position, alertRadius))
            other.GetComponent<EnemyDetection>()?.ForceAlert();
    }

    private void SetAiEnabled(bool isEnabled)
    {
        EnemyBrain brain = GetComponent<EnemyBrain>();
        if (brain != null) brain.enabled = isEnabled;

        EnemyCombat combat = GetComponent<EnemyCombat>();
        if (combat != null) combat.enabled = isEnabled;
    }

    private IEnumerator StunRoutine(float duration)
    {
        SetAiEnabled(false);
        GetComponent<EnemyAnimationController>()?.SetStunned(true);
        yield return new WaitForSeconds(Mathf.Max(duration, 0.1f));
        GetComponent<EnemyAnimationController>()?.SetStunned(false);
        SetAiEnabled(true);
    }

    private static string DescribeSuccess(HackSuccessResponse response)
    {
        switch (response)
        {
            case HackSuccessResponse.Kill: return "HACK SUCCESSFUL. TARGET TERMINATED.";
            case HackSuccessResponse.ShutDown: return "HACK SUCCESSFUL. SHUTDOWN COMPLETE.";
            case HackSuccessResponse.Stun: default: return "HACK SUCCESSFUL. TARGET STUNNED.";
        }
    }

    private static string DescribeFailure(HackFailureResponse response)
    {
        switch (response)
        {
            case HackFailureResponse.RevealLocation: return "HACK FAILED. YOUR LOCATION HAS BEEN REVEALED.";
            case HackFailureResponse.AlertNearbyEnemies: return "HACK FAILED. NEARBY ENEMIES ALERTED.";
            case HackFailureResponse.TriggerAlarm: default: return "HACK FAILED. ALARM TRIGGERED.";
        }
    }
}

