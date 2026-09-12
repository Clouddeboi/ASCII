using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pairing Data", menuName = "Terminal/Pairing Data")]
public class PairingData : ScriptableObject
{
    [Serializable]
    public class PairingEntry
    {
        public string displayName = "PLAYER -> UNKNOWN";
        public bool unlockedByDefault = false;
    }

    public PairingEntry[] pairings;
}
