using System;

//Root object serialized to disk for a single save slot.
[Serializable]
public class SaveGameData
{
    public SaveMetadata metadata = new SaveMetadata();
    public PlayerSaveData player = new PlayerSaveData();
    public WorldSaveData world = new WorldSaveData();
}
