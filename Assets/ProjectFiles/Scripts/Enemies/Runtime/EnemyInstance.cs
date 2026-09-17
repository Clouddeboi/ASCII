using System;
using System.Collections;
using UnityEngine;

public class EnemyInstance : MonoBehaviour
{
    [SerializeField] private EnemyData data;

    public string RuntimeId { get; private set; }

    public EnemyData Data => data;
    public string DisplayName => data != null ? data.identity.displayName : name;

    private void Awake()
    {
        if (data == null)
            Debug.LogError($"[EnemyInstance] '{name}' has no EnemyData assigned.", this);

        RuntimeId = Guid.NewGuid().ToString();
        EnemyRegistry.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        EnemyRegistry.Instance?.Unregister(this);
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
        yield return new WaitForSeconds(Mathf.Max(duration, 0.1f));
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

