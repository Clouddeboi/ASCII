using System.Collections.Generic;
using UnityEngine;

//Tracks all ISaveable objects in the current scene so SaveManager can capture/restore them generically.
//Mirrors the InteractableRegistry lazy-singleton pattern already used in this project.
public class SaveableRegistry : MonoBehaviour
{
    private static SaveableRegistry instance;
    private static bool shuttingDown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        shuttingDown = false;
    }

    public static SaveableRegistry Instance
    {
        get
        {
            if (shuttingDown)
                return null;

            if (instance == null)
            {
                instance = FindAnyObjectByType<SaveableRegistry>();
                if (instance == null)
                {
                    var go = new GameObject("SaveableRegistry");
                    instance = go.AddComponent<SaveableRegistry>();
                }
            }
            return instance;
        }
    }

    private readonly List<ISaveable> registered = new List<ISaveable>();

    public IReadOnlyList<ISaveable> AllSaveables => registered;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnDestroy() => shuttingDown = true;
    private void OnApplicationQuit() => shuttingDown = true;

    public void Register(ISaveable saveable)
    {
        if (saveable == null) return;
        if (!registered.Contains(saveable))
            registered.Add(saveable);
    }

    public void Unregister(ISaveable saveable)
    {
        if (saveable == null) return;
        registered.Remove(saveable);
    }
}
