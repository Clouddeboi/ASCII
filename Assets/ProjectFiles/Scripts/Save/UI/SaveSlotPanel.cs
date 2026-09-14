using System;
using UnityEngine;

//Save-file selection screen. Lives as one of MainMenuManager's panels (opened via OpenPanel), so it reuses
//the existing panel-stack navigation instead of introducing a separate menu system.
public class SaveSlotPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private ConfirmationDialog confirmationDialog;
    [SerializeField] private string gameSceneName = "Game";

    [Header("Slot List")]
    [SerializeField] private Transform slotListContainer;
    [SerializeField] private SaveSlotEntryUI slotEntryPrefab;
    [SerializeField] private EmptySaveSlotUI emptySlotPrefab;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (slotListContainer == null || slotEntryPrefab == null || emptySlotPrefab == null)
        {
            Debug.LogError("[SaveSlotPanel] Missing Inspector reference (Slot List Container / Slot Entry Prefab / Empty Slot Prefab).", this);
            return;
        }

        for (int i = slotListContainer.childCount - 1; i >= 0; i--)
            Destroy(slotListContainer.GetChild(i).gameObject);

        var slots = SaveManager.Instance.ListSaveSlots();

        foreach (SaveMetadata metadata in slots)
        {
            SaveSlotEntryUI entry = Instantiate(slotEntryPrefab, slotListContainer);
            entry.Init(metadata, OnLoadSlotClicked, OnDeleteSlotClicked);
        }

        //Always offer one empty slot to start a new save - this is also what's shown when the list is empty.
        EmptySaveSlotUI emptySlot = Instantiate(emptySlotPrefab, slotListContainer);
        emptySlot.Init(() => OnCreateNewSaveClicked(slots.Count));
    }

    //Auto-names the save "Save (x)" - the player is never asked to type a name.
    private void OnCreateNewSaveClicked(int existingSlotCount)
    {
        string saveName = $"Save ({existingSlotCount + 1})";
        string slotId = $"save_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}";

        if (!SaveManager.Instance.StartNewGame(slotId, saveName, gameSceneName))
            Debug.LogError("[SaveSlotPanel] Failed to create new save.");
    }

    private void OnLoadSlotClicked(string slotId)
    {
        SaveManager.Instance.LoadGame(slotId);
    }

    private void OnDeleteSlotClicked(string slotId)
    {
        confirmationDialog.Show(
            "Delete this save file? This cannot be undone.",
            onConfirmed: () =>
            {
                SaveManager.Instance.DeleteSave(slotId);
                Refresh();
            });
    }

    //Wired to this panel's Back button.
    public void OnBackClicked()
    {
        mainMenuManager.GoBack();
    }
}
