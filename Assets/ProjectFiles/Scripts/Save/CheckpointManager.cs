using UnityEngine;

//Scene-local singleton (lazy, like InteractableRegistry): tracks the player's last checkpoint and performs
//a soft respawn on death, teleport + full heal only, world/inventory state is left untouched.
public class CheckpointManager : MonoBehaviour
{
    private static CheckpointManager instance;
    private static bool shuttingDown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        shuttingDown = false;
    }

    public static CheckpointManager Instance
    {
        get
        {
            if (shuttingDown)
                return null;

            if (instance == null)
            {
                instance = FindAnyObjectByType<CheckpointManager>();
                if (instance == null)
                {
                    var go = new GameObject("CheckpointManager");
                    instance = go.AddComponent<CheckpointManager>();
                }
            }
            return instance;
        }
    }

    public string CheckpointId { get; private set; }
    public Vector3 CheckpointPosition { get; private set; }
    public Quaternion CheckpointRotation { get; private set; }
    public bool HasCheckpoint { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        //Deferred to Start so PlayerHealth.Instance (set in its own Awake) is guaranteed to exist by now.
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnDeath += Respawn;
    }

    private void OnDestroy()
    {
        shuttingDown = true;
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnDeath -= Respawn;
    }

    public void SetCheckpoint(string checkpointId, Vector3 position, Quaternion rotation)
    {
        CheckpointId = checkpointId;
        CheckpointPosition = position;
        CheckpointRotation = rotation;
        HasCheckpoint = true;
    }

    private void Respawn()
    {
        if (!HasCheckpoint || PlayerHealth.Instance == null)
            return;

        //Teleport the shared root, not PlayerHealth's own transform - see SaveManager.RestorePlayerTransformAndHealth.
        PlayerTeleportUtility.Teleport(PlayerHealth.Instance.transform.root, CheckpointPosition, CheckpointRotation);
        PlayerHealth.Instance.SetHealthDirect(PlayerHealth.Instance.MaxHealth, PlayerHealth.Instance.MaxHealth);
    }
}
