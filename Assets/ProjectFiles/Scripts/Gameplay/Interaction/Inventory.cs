using System;
using System.Collections.Generic;
using System.Linq;

//Tracks all items the player has collected (keys, quest items, health items). Keys are looked up by
//Id from InteractableBase's requiredKeyId, so a key ItemDefinition's ItemId must match that string.
public static class Inventory
{
    private static readonly Dictionary<string, InventoryStack> stacks = new Dictionary<string, InventoryStack>(StringComparer.OrdinalIgnoreCase);

    public static event Action OnChanged;

    public static IReadOnlyCollection<InventoryStack> AllStacks => stacks.Values;

    public static void AddItem(ItemDefinition definition, int amount = 1)
    {
        if (definition == null || amount <= 0)
            return;

        if (stacks.TryGetValue(definition.ItemId, out InventoryStack existing))
        {
            int max = definition.Stackable ? definition.MaxStack : 1;
            existing.Count = max > 0 ? Mathf_Min(existing.Count + amount, max) : existing.Count + amount;
        }
        else
        {
            int max = definition.Stackable ? definition.MaxStack : 1;
            int count = max > 0 ? Mathf_Min(amount, max) : amount;
            stacks[definition.ItemId] = new InventoryStack(definition, count);
        }

        OnChanged?.Invoke();
    }

    public static bool HasItem(string itemId)
    {
        return !string.IsNullOrEmpty(itemId) && stacks.TryGetValue(itemId, out InventoryStack stack) && stack.Count > 0;
    }

    public static int GetCount(string itemId)
    {
        return !string.IsNullOrEmpty(itemId) && stacks.TryGetValue(itemId, out InventoryStack stack) ? stack.Count : 0;
    }

    public static bool RemoveItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return false;

        if (!stacks.TryGetValue(itemId, out InventoryStack stack) || stack.Count < amount)
            return false;

        stack.Count -= amount;
        if (stack.Count <= 0)
            stacks.Remove(itemId);

        OnChanged?.Invoke();
        return true;
    }

    //Finds a stack by exact Id or case-insensitive display name match, for use with "/equipment use <item>".
    public static InventoryStack FindByNameOrId(string query)
    {
        if (string.IsNullOrEmpty(query))
            return null;

        if (stacks.TryGetValue(query, out InventoryStack byId))
            return byId;

        return stacks.Values.FirstOrDefault(s => string.Equals(s.Definition.DisplayName, query, StringComparison.OrdinalIgnoreCase));
    }

    //Applies item-specific use behaviour (currently only Health items). Returns a user-facing message either way.
    public static bool TryUseItem(string query, out string message)
    {
        InventoryStack stack = FindByNameOrId(query);
        if (stack == null)
        {
            message = "ITEM NOT FOUND: " + query;
            return false;
        }

        switch (stack.Definition.Category)
        {
            case ItemCategory.Health:
                if (PlayerHealth.Instance == null)
                {
                    message = "PLAYER HEALTH UNAVAILABLE.";
                    return false;
                }

                PlayerHealth.Instance.Heal(stack.Definition.HealAmount);
                RemoveItem(stack.Definition.ItemId, 1);
                message = $"USED {stack.Definition.DisplayName}. HEALED {stack.Definition.HealAmount:0} HP.";
                return true;

            default:
                message = stack.Definition.DisplayName + " CANNOT BE USED DIRECTLY.";
                return false;
        }
    }

    private static int Mathf_Min(int a, int b) => a < b ? a : b;
}
