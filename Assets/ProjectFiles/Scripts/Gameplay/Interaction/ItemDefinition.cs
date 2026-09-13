using UnityEngine;

//Data-only definition for a pickable inventory item. Doors/containers reference an item's Id as their requiredKeyId.
[CreateAssetMenu(fileName = "NewItem", menuName = "ASCII/Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string itemId = "item";
    [SerializeField] private string displayName = "Item";
    [SerializeField] private ItemCategory category = ItemCategory.QuestItem;
    [TextArea]
    [SerializeField] private string description;

    [Header("Stacking")]
    [SerializeField] private bool stackable = false;
    [SerializeField] private int maxStack = 1;

    [Header("Health (only used when Category is Health)")]
    [SerializeField] private float healAmount = 25f;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public ItemCategory Category => category;
    public string Description => description;
    public bool Stackable => stackable;
    public int MaxStack => maxStack;
    public float HealAmount => healAmount;
}
