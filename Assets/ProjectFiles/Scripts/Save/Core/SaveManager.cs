using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

//Central save/load orchestrator. Lazily created like InteractableRegistry (auto-spawns on first Instance
//access) and persists across scene loads. Owns disk I/O, slot management and gathering/applying player &
//world state - individual gameplay scripts never touch files directly.
public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;
    private static bool shuttingDown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        shuttingDown = false;
    }

    public static SaveManager Instance
    {
        get
        {
            if (shuttingDown)
                return null;

            if (instance == null)
            {
                instance = FindAnyObjectByType<SaveManager>();
                if (instance == null)
                {
                    var go = new GameObject("SaveManager");
                    instance = go.AddComponent<SaveManager>();
                }
            }
            return instance;
        }
    }

    [Header("Auto-Save")]
    [SerializeField] private float minAutoSaveIntervalSeconds = 30f;

    [Header("Scene Transition")]
    [SerializeField] private float fadeDuration = 0.5f;

    private const string SaveFolderName = "Saves";

    private float lastAutoSaveRealtime = -999f;
    private float sessionStartTime;
    private readonly HashSet<string> pickedUpItems = new HashSet<string>();

    public string CurrentSlotId { get; private set; }
    public bool HasActiveSave => !string.IsNullOrEmpty(CurrentSlotId);

    public event Action<string> OnSaveCompleted;
    public event Action<string> OnSaveFailed;
    public event Action<string> OnLoadCompleted;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        sessionStartTime = Time.realtimeSinceStartup;
    }

    private void OnDestroy() => shuttingDown = true;
    private void OnApplicationQuit() => shuttingDown = true;

    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, SaveFolderName);
    private string SlotPath(string slotId) => Path.Combine(SaveFolderPath, $"{slotId}.json");

    //Metadata only, for the save-slot selection UI. Corrupted files are skipped rather than thrown.
    public List<SaveMetadata> ListSaveSlots()
    {
        var result = new List<SaveMetadata>();
        if (!Directory.Exists(SaveFolderPath))
            return result;

        foreach (string file in Directory.GetFiles(SaveFolderPath, "*.json"))
        {
            if (TryReadSaveFile(Path.GetFileNameWithoutExtension(file), out SaveGameData data))
                result.Add(data.metadata);
        }

        return result.OrderByDescending(m => m.lastSavedUtc).ToList();
    }

    public bool SaveSlotExists(string slotId) => File.Exists(SlotPath(slotId));

    //Creates a fresh slot with default player/world data and writes it immediately.
    public bool NewGame(string slotId, string saveName, string startingSceneName)
    {
        if (string.IsNullOrEmpty(slotId))
        {
            Debug.LogError("[SaveManager] NewGame called with empty slotId.");
            return false;
        }

        var data = new SaveGameData
        {
            metadata = new SaveMetadata
            {
                slotId = slotId,
                saveName = string.IsNullOrEmpty(saveName) ? slotId : saveName,
                lastSavedUtc = DateTime.UtcNow.ToString("o"),
                playtimeSeconds = 0f,
                sceneName = startingSceneName
            },
            player = new PlayerSaveData
            {
                currentHealth = 100f,
                maxHealth = 100f,
                position = Vector3.zero,
                rotation = Quaternion.identity
            },
            world = new WorldSaveData()
        };

        CurrentSlotId = slotId;
        sessionStartTime = Time.realtimeSinceStartup;
        pickedUpItems.Clear();

        return WriteSaveFile(slotId, data);
    }

    //Captures current player/world state into CurrentSlotId. Requires a slot selected via NewGame/LoadGame first.
    public bool SaveGame()
    {
        if (!HasActiveSave)
        {
            Debug.LogWarning("[SaveManager] SaveGame called with no active slot.");
            return false;
        }

        if (!TryReadSaveFile(CurrentSlotId, out SaveGameData data))
            data = new SaveGameData { metadata = new SaveMetadata { slotId = CurrentSlotId } };

        data.metadata.lastSavedUtc = DateTime.UtcNow.ToString("o");
        data.metadata.sceneName = SceneManager.GetActiveScene().name;
        data.metadata.playtimeSeconds += Time.realtimeSinceStartup - sessionStartTime;
        sessionStartTime = Time.realtimeSinceStartup;

        data.player = GatherPlayerData(data.player);
        data.world = GatherWorldData(data.world);

        lastAutoSaveRealtime = Time.realtimeSinceStartup;
        return WriteSaveFile(CurrentSlotId, data);
    }

    //Loads slotId, fading to black first, switching scenes if required, then applying player/world state and
    //fading back in once the scene is ready. Used for manual loads and the pause menu's "Load" button.
    public void LoadGame(string slotId, Action onComplete = null)
    {
        if (!TryReadSaveFile(slotId, out SaveGameData data))
        {
            Debug.LogError($"[SaveManager] Failed to load save '{slotId}'.");
            onComplete?.Invoke();
            return;
        }

        ScreenFader.Instance?.FadeOut(fadeDuration, () => BeginLoad(slotId, data, onComplete));
    }

    private void BeginLoad(string slotId, SaveGameData data, Action onComplete)
    {
        CurrentSlotId = slotId;
        sessionStartTime = Time.realtimeSinceStartup;
        pickedUpItems.Clear();
        pickedUpItems.UnionWith(data.world.pickedUpItems);

        string targetScene = data.metadata.sceneName;
        if (string.IsNullOrEmpty(targetScene) || SceneManager.GetActiveScene().name == targetScene)
        {
            ApplySaveData(data);
            FinishLoad(slotId, onComplete);
            return;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ApplySaveData(data);
            FinishLoad(slotId, onComplete);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(targetScene);
    }

    private void FinishLoad(string slotId, Action onComplete)
    {
        ScreenFader.Instance?.FadeIn(fadeDuration, () =>
        {
            onComplete?.Invoke();
            OnLoadCompleted?.Invoke(slotId);
        });
    }

    //Creates a fresh slot and starts play, fading to black across the scene load like LoadGame does.
    public bool StartNewGame(string slotId, string saveName, string startingSceneName)
    {
        if (!NewGame(slotId, saveName, startingSceneName))
            return false;

        ScreenFader.Instance?.FadeOut(fadeDuration, () =>
        {
            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                ScreenFader.Instance?.FadeIn(fadeDuration);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(startingSceneName);
        });

        return true;
    }

    public bool DeleteSave(string slotId)
    {
        try
        {
            string path = SlotPath(slotId);
            if (File.Exists(path))
                File.Delete(path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to delete save '{slotId}': {e.Message}");
            return false;
        }
    }

    //Reusable entry point for any future system (quests, events, enemies) to request an auto-save without
    //depending on SaveManager internals. Debounced so rapid-fire requests don't thrash disk I/O.
    public void RequestAutoSave(string reason)
    {
        if (!HasActiveSave)
            return;

        if (Time.realtimeSinceStartup - lastAutoSaveRealtime < minAutoSaveIntervalSeconds)
            return;

        Debug.Log($"[SaveManager] Auto-saving ({reason}).");
        SaveGame();
    }

    //Called by ItemPickupInteractable when its item is collected, so a reload doesn't respawn it.
    //Only affects the in-memory session; persisted to disk on the next SaveGame().
    public void MarkItemPickedUp(string interactableId)
    {
        if (string.IsNullOrEmpty(interactableId))
            return;
        pickedUpItems.Add(interactableId);
    }

    public bool IsPickedUp(string interactableId)
    {
        return !string.IsNullOrEmpty(interactableId) && pickedUpItems.Contains(interactableId);
    }

    private PlayerSaveData GatherPlayerData(PlayerSaveData existing)
    {
        var data = existing ?? new PlayerSaveData();

        if (PlayerHealth.Instance != null)
        {
            data.currentHealth = PlayerHealth.Instance.CurrentHealth;
            data.maxHealth = PlayerHealth.Instance.MaxHealth;
            //Root, not PlayerHealth's own transform - the movement body, look orientation and camera anchor
            //are separate sibling children under one root, so only moving the shared root keeps them in sync.
            Transform playerRoot = PlayerHealth.Instance.transform.root;
            data.position = playerRoot.position;
            data.rotation = playerRoot.rotation;
            Debug.Log($"[SaveManager] Captured player position {data.position}, rotation {data.rotation.eulerAngles} (root '{playerRoot.name}').");
        }
        else
        {
            Debug.LogWarning("[SaveManager] PlayerHealth.Instance not found while saving - position/health not captured.");
        }

        data.inventory.Clear();
        foreach (InventoryStack stack in Inventory.AllStacks)
        {
            data.inventory.Add(new InventoryEntry { itemId = stack.Definition.ItemId, count = stack.Count });
        }

        if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
        {
            data.hasCheckpoint = true;
            data.checkpointId = CheckpointManager.Instance.CheckpointId;
            data.checkpointPosition = CheckpointManager.Instance.CheckpointPosition;
            data.checkpointRotation = CheckpointManager.Instance.CheckpointRotation;
        }

        return data;
    }

    private WorldSaveData GatherWorldData(WorldSaveData existing)
    {
        var data = existing ?? new WorldSaveData();

        data.pickedUpItems = pickedUpItems.ToList();

        //Merge rather than replace: only the currently loaded scene's ISaveables are registered right now,
        //so entries belonging to other scenes the player has visited must be preserved, not wiped out.
        Dictionary<string, SaveableStateEntry> statesById = data.saveableStates.ToDictionary(e => e.saveId);

        if (SaveableRegistry.Instance != null)
        {
            foreach (ISaveable saveable in SaveableRegistry.Instance.AllSaveables)
            {
                statesById[saveable.SaveId] = new SaveableStateEntry
                {
                    saveId = saveable.SaveId,
                    json = saveable.CaptureState()
                };
            }
        }

        data.saveableStates = statesById.Values.ToList();

        return data;
    }

    private void ApplySaveData(SaveGameData data)
    {
        ApplyPlayerTransformAndHealth(data.player);

        Inventory.Clear();
        if (ItemDatabase.Instance != null)
        {
            foreach (InventoryEntry entry in data.player.inventory)
            {
                ItemDefinition def = ItemDatabase.Instance.FindById(entry.itemId);
                if (def != null)
                    Inventory.AddItem(def, entry.count);
                else
                    Debug.LogWarning($"[SaveManager] Unknown itemId '{entry.itemId}' in save, skipping.");
            }
        }
        else
        {
            Debug.LogWarning("[SaveManager] No ItemDatabase found at Resources/ItemDatabase - inventory not restored.");
        }

        if (data.player.hasCheckpoint && CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetCheckpoint(data.player.checkpointId, data.player.checkpointPosition, data.player.checkpointRotation);
        }

        if (SaveableRegistry.Instance != null)
        {
            Dictionary<string, string> lookup = data.world.saveableStates.ToDictionary(e => e.saveId, e => e.json);
            foreach (ISaveable saveable in SaveableRegistry.Instance.AllSaveables)
            {
                if (lookup.TryGetValue(saveable.SaveId, out string json))
                    saveable.RestoreState(json);
            }
        }
    }

    //PlayerHealth.Instance should already be set by the time this runs (SceneManager.sceneLoaded fires after
    //Awake/OnEnable of the new scene's objects), but retries across a few frames as a safety net in case
    //some other script's Awake/Start ordering delays it.
    private void ApplyPlayerTransformAndHealth(PlayerSaveData playerData)
    {
        if (PlayerHealth.Instance != null)
        {
            RestorePlayerTransformAndHealth(playerData);
            return;
        }

        StartCoroutine(ApplyPlayerTransformAndHealthDeferred(playerData));
    }

    private IEnumerator ApplyPlayerTransformAndHealthDeferred(PlayerSaveData playerData)
    {
        int attempts = 0;
        while (PlayerHealth.Instance == null && attempts < 30)
        {
            attempts++;
            yield return null;
        }

        if (PlayerHealth.Instance == null)
        {
            Debug.LogError("[SaveManager] PlayerHealth.Instance never became available - could not restore player position/health.");
            yield break;
        }

        RestorePlayerTransformAndHealth(playerData);
    }

    private void RestorePlayerTransformAndHealth(PlayerSaveData playerData)
    {
        //PlayerMovement co-location check (health/movement should be on the same GameObject).
        Transform playerTransform = PlayerHealth.Instance.transform;
        if (playerTransform.GetComponent<PlayerMovement>() == null)
        {
            Debug.LogWarning($"[SaveManager] PlayerHealth is on '{playerTransform.name}' but PlayerMovement is not on that same GameObject.");
        }

        //Teleport the shared root, not PlayerHealth's own transform - the movement body, look orientation
        //and camera anchor are separate sibling children, so only the root move keeps them all in sync.
        Transform playerRoot = playerTransform.root;

        PlayerHealth.Instance.SetHealthDirect(playerData.currentHealth, playerData.maxHealth);
        PlayerTeleportUtility.Teleport(playerRoot, playerData.position, playerData.rotation);
        Debug.Log($"[SaveManager] Restored player to position {playerData.position}, rotation {playerData.rotation.eulerAngles} (root '{playerRoot.name}'). " +
            $"Transform after teleport: {playerRoot.position}.");
    }

    private bool TryReadSaveFile(string slotId, out SaveGameData data)
    {
        data = null;
        string path = SlotPath(slotId);

        if (!File.Exists(path))
            return false;

        try
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<SaveGameData>(json);
            return data != null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Corrupted save file '{slotId}': {e.Message}");
            return false;
        }
    }

    //Writes to a temp file first and swaps it in, so a crash/power-loss mid-write can't corrupt the real save.
    private bool WriteSaveFile(string slotId, SaveGameData data)
    {
        try
        {
            Directory.CreateDirectory(SaveFolderPath);
            string json = JsonUtility.ToJson(data, true);
            string finalPath = SlotPath(slotId);
            string tempPath = finalPath + ".tmp";

            File.WriteAllText(tempPath, json);
            if (File.Exists(finalPath))
                File.Delete(finalPath);
            File.Move(tempPath, finalPath);

            OnSaveCompleted?.Invoke(slotId);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to write save '{slotId}': {e.Message}");
            OnSaveFailed?.Invoke(slotId);
            return false;
        }
    }
}
