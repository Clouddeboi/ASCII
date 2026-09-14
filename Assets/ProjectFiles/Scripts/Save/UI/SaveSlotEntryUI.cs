using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//A single row in the save-slot list: shows metadata and forwards Load/Delete clicks back to SaveSlotPanel.
public class SaveSlotEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text saveNameText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;

    private string slotId;
    private Action<string> onLoad;
    private Action<string> onDelete;

    public void Init(SaveMetadata metadata, Action<string> onLoadClicked, Action<string> onDeleteClicked)
    {
        slotId = metadata.slotId;
        onLoad = onLoadClicked;
        onDelete = onDeleteClicked;

        if (saveNameText != null)
            saveNameText.text = metadata.saveName;

        if (detailsText != null)
        {
            TimeSpan playtime = TimeSpan.FromSeconds(metadata.playtimeSeconds);
            string lastSaved = DateTime.TryParse(metadata.lastSavedUtc, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime savedUtc)
                ? savedUtc.ToLocalTime().ToString("g")
                : "Unknown";

            detailsText.text = $"{metadata.sceneName}  |  {lastSaved}  |  {(int)playtime.TotalHours:00}:{playtime.Minutes:00}:{playtime.Seconds:00}";
        }

        loadButton?.onClick.AddListener(() => onLoad?.Invoke(slotId));
        deleteButton?.onClick.AddListener(() => onDelete?.Invoke(slotId));
    }
}
