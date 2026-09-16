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
}
