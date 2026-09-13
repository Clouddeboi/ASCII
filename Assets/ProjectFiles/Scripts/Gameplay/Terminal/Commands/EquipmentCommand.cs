using System.Linq;
using System.Text;

//Shows and uses inventory items: "/equipment view" lists items, "/equipment use <item>" uses one.
public class EquipmentCommand : ITerminalCommand
{
    public string Name => "/equipment";
    public string[] Aliases => new string[0];
    public string Description => "View or use inventory items.";
    public string Usage => "/equipment view | /equipment use <item>";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        if (args.Length == 0)
        {
            context.Output.PrintError("USAGE: " + Usage);
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "view":
                ExecuteView(context);
                break;
            case "use":
                ExecuteUse(args, context);
                break;
            default:
                context.Output.PrintError("USAGE: " + Usage);
                break;
        }
    }

    private void ExecuteView(TerminalContext context)
    {
        if (Inventory.AllStacks.Count == 0)
        {
            context.Output.PrintResponse("INVENTORY EMPTY.");
            return;
        }

        StringBuilder sb = new StringBuilder("INVENTORY\n---------");
        foreach (InventoryStack stack in Inventory.AllStacks.OrderBy(s => s.Definition.DisplayName))
            sb.Append($"\n{stack.Definition.DisplayName} x{stack.Count}");

        context.Output.PrintResponse(sb.ToString());
        context.Audio.PlayResponse();
    }

    private void ExecuteUse(string[] args, TerminalContext context)
    {
        if (args.Length < 2)
        {
            context.Output.PrintError("USAGE: /equipment use <item>");
            return;
        }

        string itemQuery = string.Join(" ", args, 1, args.Length - 1);
        bool success = Inventory.TryUseItem(itemQuery, out string message);

        if (success)
        {
            context.Output.PrintResponse(message);
            context.Audio.PlayResponse();
        }
        else
        {
            context.Output.PrintError(message);
        }
    }
}
