using UnityEngine;

//Forces interaction with the closest matching interactable in range, bypassing normal "press E" gating.
public class HackCommand : ITerminalCommand
{
    private const float HackRange = 6f;

    public string Name => "/hack";
    public string[] Aliases => new string[0];
    public string Description => "Forcibly interact with the nearest matching object.";
    public string Usage => "/hack <target> [args]";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        if (args.Length == 0)
        {
            context.Output.PrintError("USAGE: " + Usage);
            return;
        }

        string targetId = args[0];
        string[] extraArgs = args.Length > 1 ? args[1..] : new string[0];

        if (InteractableRegistry.Instance == null)
        {
            context.Output.PrintError("INTERACTION SYSTEM UNAVAILABLE.");
            return;
        }

        Vector3 origin = GetPlayerPosition(context);
        IInteractable target = InteractableRegistry.Instance.FindNearest(targetId, origin, HackRange);

        if (target == null)
        {
            Debug.Log($"[Hack] No '{targetId}' found within {HackRange}m of {origin}.");
            context.Output.PrintError("NO MATCHING TARGET IN RANGE: " + targetId);
            return;
        }

        Debug.Log($"[Hack] Targeting {target.DisplayName} at distance {Vector3.Distance(origin, target.Transform.position):F1}m.");

        HackResult result = target.Hack(extraArgs);

        switch (result)
        {
            case HackResult.Success:
                context.Output.PrintResponse("HACK SUCCESSFUL: " + target.DisplayName);
                context.Audio.PlayResponse();
                break;
            case HackResult.RequiresCode:
                context.Output.PrintSystem("CODE REQUIRED. ENTER /code <code>");
                break;
            case HackResult.RequiresKey:
                context.Output.PrintError("KEY REQUIRED: cannot hack " + target.DisplayName + " without the proper key.");
                break;
            case HackResult.LockedPermanent:
                context.Output.PrintError(target.DisplayName + " IS PERMANENTLY LOCKED.");
                break;
            case HackResult.Failed:
            default:
                context.Output.PrintError("HACK FAILED: " + target.DisplayName);
                break;
        }
    }

    //context.Terminal is the terminal UI object, not the player, so its position can't be used for range checks.
    private static Vector3 GetPlayerPosition(TerminalContext context)
    {
        if (InteractionController.Instance != null)
            return InteractionController.Instance.OriginPosition;

        if (Camera.main != null)
            return Camera.main.transform.position;

        return context.Terminal.transform.position;
    }
}
