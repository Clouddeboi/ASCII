using System.Collections.Generic;

//Tracks keys the player has collected, used by key-locked interactables.
public static class KeyInventory
{
    private static readonly HashSet<string> collectedKeys = new HashSet<string>();

    public static void AddKey(string keyId)
    {
        if (!string.IsNullOrEmpty(keyId))
            collectedKeys.Add(keyId);
    }

    public static bool HasKey(string keyId)
    {
        return !string.IsNullOrEmpty(keyId) && collectedKeys.Contains(keyId);
    }
}
