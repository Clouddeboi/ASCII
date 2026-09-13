//A single entry in the player's inventory: an item definition and how many are held.
public class InventoryStack
{
    public ItemDefinition Definition { get; }
    public int Count { get; set; }

    public InventoryStack(ItemDefinition definition, int count)
    {
        Definition = definition;
        Count = count;
    }
}
