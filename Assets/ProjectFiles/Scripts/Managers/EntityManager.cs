using System.Collections.Generic;
using UnityEngine;

//Tracks registered NPCs/enemies so the terminal can query nearby entity counts.
public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { get; private set; }

    [Header("Detection")]
    [SerializeField] private float defaultScanRadius = 15f;

    private readonly List<Transform> registeredEntities = new List<Transform>();

    public float DefaultScanRadius => defaultScanRadius;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(Transform entity)
    {
        if (entity == null) return;
        if (!registeredEntities.Contains(entity))
            registeredEntities.Add(entity);
    }

    public void Unregister(Transform entity)
    {
        if (entity == null) return;
        registeredEntities.Remove(entity);
    }

    public int CountNearby(Vector3 origin, float radius = -1f)
    {
        if (radius < 0f) radius = defaultScanRadius;
        float sqrRadius = radius * radius;
        int count = 0;

        for (int i = registeredEntities.Count - 1; i >= 0; i--)
        {
            Transform entity = registeredEntities[i];
            if (entity == null)
            {
                registeredEntities.RemoveAt(i);
                continue;
            }

            if ((entity.position - origin).sqrMagnitude <= sqrRadius)
                count++;
        }

        return count;
    }
}
