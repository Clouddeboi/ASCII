using System;
using System.Collections.Generic;

[Serializable]
public class WorldSaveData
{
    public List<string> completedEvents = new List<string>();
    public List<string> pickedUpItems = new List<string>();
    public List<SaveableStateEntry> saveableStates = new List<SaveableStateEntry>();
}
