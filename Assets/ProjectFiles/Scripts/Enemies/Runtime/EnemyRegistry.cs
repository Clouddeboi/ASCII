using System.Collections.Generic;
using UnityEngine;

public class EnemyRegistry : MonoBehaviour
{
    private static EnemyRegistry instance;
    private static bool shuttingDown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        shuttingDown = false;
    }

    public static EnemyRegistry Instance
    {
        get
        {
            if (shuttingDown)
                return null;

            if (instance == null)
            {
                instance = FindAnyObjectByType<EnemyRegistry>();
                if (instance == null)
                {
                    var go = new GameObject("EnemyRegistry");
                    instance = go.AddComponent<EnemyRegistry>();
                }
            }
            return instance;
        }
    }

    private readonly List<EnemyInstance> registered = new List<EnemyInstance>();

    public IReadOnlyList<EnemyInstance> All => registered;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnDestroy()
    {
        shuttingDown = true;
    }

    private void OnApplicationQuit()
    {
        shuttingDown = true;
    }

    public void Register(EnemyInstance enemy)
    {
        if (enemy == null) return;
        if (!registered.Contains(enemy))
            registered.Add(enemy);
    }

    public void Unregister(EnemyInstance enemy)
    {
        if (enemy == null) return;
        registered.Remove(enemy);
    }

    public List<EnemyInstance> InRange(Vector3 origin, float range)
    {
        float sqrRange = range * range;
        var result = new List<EnemyInstance>();

        for (int i = registered.Count - 1; i >= 0; i--)
        {
            EnemyInstance enemy = registered[i];
            if (enemy == null)
            {
                registered.RemoveAt(i);
                continue;
            }

            if ((enemy.transform.position - origin).sqrMagnitude <= sqrRange)
                result.Add(enemy);
        }

        return result;
    }

    //Groups pingable enemies within range by display name, respecting each enemy's own terminalDetectionRange.
    public Dictionary<string, int> GroupPingableByDisplayName(Vector3 origin, float scanRange)
    {
        var counts = new Dictionary<string, int>();

        foreach (EnemyInstance enemy in InRange(origin, scanRange))
        {
            EnemyTerminalData terminalData = enemy.Data.terminal;
            if (!terminalData.pingable) continue;

            float effectiveRange = Mathf.Min(scanRange, terminalData.terminalDetectionRange);
            if ((enemy.transform.position - origin).sqrMagnitude > effectiveRange * effectiveRange)
                continue;

            counts.TryGetValue(enemy.DisplayName, out int existing);
            counts[enemy.DisplayName] = existing + 1;
        }

        return counts;
    }

    //Highest ThreatLevel among anomaly-detectable enemies in range, or null if none are present.
    public ThreatLevel? GetHighestAnomalyDanger(Vector3 origin, float scanRange)
    {
        ThreatLevel? highest = null;

        foreach (EnemyInstance enemy in InRange(origin, scanRange))
        {
            EnemyTerminalData terminalData = enemy.Data.terminal;
            if (!terminalData.anomalyDetectable) continue;

            float effectiveRange = Mathf.Min(scanRange, terminalData.terminalDetectionRange);
            if ((enemy.transform.position - origin).sqrMagnitude > effectiveRange * effectiveRange)
                continue;

            if (!highest.HasValue || terminalData.dangerClassification > highest.Value)
                highest = terminalData.dangerClassification;
        }

        return highest;
    }
}
