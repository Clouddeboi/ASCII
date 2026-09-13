using System.Collections.Generic;
using UnityEngine;

//Tracks all registered interactables so the player and /hack can query them.
public class InteractableRegistry : MonoBehaviour
{
    private static InteractableRegistry instance;
    private static bool shuttingDown;

    //Lazily created so registration never silently fails due to Awake execution order against interactables.
    //Returns null once the app/scene is shutting down instead of spawning a new instance from OnDestroy.
    public static InteractableRegistry Instance
    {
        get
        {
            if (shuttingDown)
                return null;

            if (instance == null)
            {
                instance = FindAnyObjectByType<InteractableRegistry>();
                if (instance == null)
                {
                    var go = new GameObject("InteractableRegistry");
                    instance = go.AddComponent<InteractableRegistry>();
                }
            }
            return instance;
        }
    }

    private readonly List<IInteractable> registered = new List<IInteractable>();

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

    public void Register(IInteractable interactable)
    {
        if (interactable == null) return;
        if (!registered.Contains(interactable))
            registered.Add(interactable);

        Debug.Log($"[InteractableRegistry] Registered '{interactable.InteractableId}' ({interactable.DisplayName}). Total registered: {registered.Count}");
    }

    public void Unregister(IInteractable interactable)
    {
        if (interactable == null) return;
        registered.Remove(interactable);
    }

    //Finds the closest interactable matching interactableId (case-insensitive) within range of origin.
    public IInteractable FindNearest(string interactableId, Vector3 origin, float maxRange)
    {
        IInteractable closest = null;
        float closestSqrDist = maxRange * maxRange;

        for (int i = registered.Count - 1; i >= 0; i--)
        {
            IInteractable candidate = registered[i];
            if (candidate == null || candidate.Transform == null)
            {
                registered.RemoveAt(i);
                continue;
            }

            if (!string.Equals(candidate.InteractableId, interactableId, System.StringComparison.OrdinalIgnoreCase))
                continue;

            float sqrDist = (candidate.Transform.position - origin).sqrMagnitude;
            if (sqrDist <= closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = candidate;
            }
        }

        if (closest == null)
        {
            Debug.Log($"[InteractableRegistry] No match for '{interactableId}'. Registered ({registered.Count}): " +
                string.Join(", ", registered.ConvertAll(r => $"{r.InteractableId}@{Vector3.Distance(r.Transform.position, origin):F1}m")));
        }

        return closest;
    }
}
