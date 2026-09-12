using System.Text;

public class OverrideCommand : ITerminalCommand
{
    public string Name => "/override";
    public string[] Aliases => new string[0];
    public string Description => "Access entity pairing controls.";
    public string Usage => "/override [pairing index]";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        if (PairingManager.Instance == null)
        {
            context.Output.PrintError("OVERRIDE SYSTEM UNAVAILABLE.");
            return;
        }

        //"/override <n>" executes the selected unlocked pairing.
        if (args.Length > 0 && int.TryParse(args[0], out int selection))
        {
            string result = PairingManager.Instance.ExecutePairing(selection - 1);
            context.Output.PrintResponse(result);
            context.Audio.PlayResponse();
            return;
        }

        PairingData.PairingEntry[] pairings = PairingManager.Instance.GetAllPairings();
        if (pairings == null || pairings.Length == 0)
        {
            context.Output.PrintError("NO PAIRING DATA CONFIGURED.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("ENTITY OVERRIDE SYSTEM");
        sb.AppendLine("----------------------");
        sb.AppendLine();
        sb.AppendLine("AVAILABLE PAIRINGS:");
        sb.AppendLine();

        for (int i = 0; i < pairings.Length; i++)
        {
            if (PairingManager.Instance.IsUnlocked(i))
                sb.AppendLine($"{i + 1}. {pairings[i].displayName}");
        }

        sb.AppendLine();
        sb.AppendLine("LOCKED PAIRINGS:");
        sb.AppendLine();

        for (int i = 0; i < pairings.Length; i++)
        {
            if (!PairingManager.Instance.IsUnlocked(i))
                sb.AppendLine($"{i + 1}. {pairings[i].displayName}");
        }

        context.Output.PrintResponse(sb.ToString());
        context.Audio.PlayResponse();
    }
}
