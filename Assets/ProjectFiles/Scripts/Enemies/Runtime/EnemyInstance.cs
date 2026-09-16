using System;
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
}
