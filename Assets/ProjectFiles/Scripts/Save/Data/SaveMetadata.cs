using System;

//Lightweight, fast-to-read info shown in the save slot selection UI without loading the full save.
[Serializable]
public class SaveMetadata
{
    public string slotId;
    public string saveName;
    public string lastSavedUtc;
    public float playtimeSeconds;
    public string sceneName;
}
