using System;

//Item persisted by stable itemId (not an ItemDefinition reference), resolved via ItemDatabase on load.
[Serializable]
public class InventoryEntry
{
    public string itemId;
    public int count;
}
