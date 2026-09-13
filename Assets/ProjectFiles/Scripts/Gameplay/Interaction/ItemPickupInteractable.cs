using UnityEngine;

//A world item that can be picked up with E, adding it to the player's Inventory and removing itself from the world.
public class ItemPickupInteractable : InteractableBase
{
    [Header("Item")]
    [SerializeField] private ItemDefinition item;
    [SerializeField] private int amount = 1;

    public override string InteractVerb => "Pick up";

    protected override bool PerformAction(string[] args)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemPickup] No ItemDefinition assigned.", this);
            return false;
        }

        Inventory.AddItem(item, amount);
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }
}
