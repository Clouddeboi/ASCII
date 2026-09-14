using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Resolves saved itemId strings back to ItemDefinition assets on load (never serialize ItemDefinition refs directly)
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ASCII/Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();

    private static ItemDatabase instance;

    public static ItemDatabase Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<ItemDatabase>("ItemDatabase");
            return instance;
        }
    }

    public ItemDefinition FindById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        return items.FirstOrDefault(i => i != null && string.Equals(i.ItemId, itemId, System.StringComparison.OrdinalIgnoreCase));
    }
}
