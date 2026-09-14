using System;

//Generic (saveId -> JSON blob) entry so WorldSaveData doesn't need to know each ISaveable's concrete state shape.
[Serializable]
public class SaveableStateEntry
{
    public string saveId;
    public string json;
}
