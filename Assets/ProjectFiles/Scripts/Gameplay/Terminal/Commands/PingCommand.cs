using System.Collections.Generic;

public class PingCommand : ITerminalCommand
{
    private const float ScanRadius = 25f;

    public string Name => "/ping";
    public string[] Aliases => new string[0];
    public string Description => "Display nearby entities.";
    public string Usage => "/ping";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        context.Output.PrintSystem("SCANNING AREA...");

        if (EnemyRegistry.Instance == null)
        {
            context.Output.PrintError("ENTITY SCANNER UNAVAILABLE.");
            return;
        }

        Dictionary<string, int> counts = EnemyRegistry.Instance.GroupPingableByDisplayName(context.Terminal.transform.position, ScanRadius);

        if (counts.Count == 0)
        {
            context.Output.PrintResponse("PING COMPLETE\n\nNO ENTITIES DETECTED.");
            context.Audio.PlayResponse();
            return;
        }

        var lines = new List<string>();
        foreach (KeyValuePair<string, int> entry in counts)
            lines.Add($"{entry.Key} x{entry.Value}");

        context.Output.PrintResponse("PING COMPLETE\n\n" + string.Join("\n", lines));
        context.Audio.PlayResponse();
    }
}

