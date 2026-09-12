public class PingCommand : ITerminalCommand
{
    public string Name => "/ping";
    public string[] Aliases => new string[0];
    public string Description => "Display nearby entities.";
    public string Usage => "/ping";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        context.Output.PrintSystem("SCANNING AREA...");

        if (EntityManager.Instance == null)
        {
            context.Output.PrintError("ENTITY SCANNER UNAVAILABLE.");
            return;
        }

        int count = EntityManager.Instance.CountNearby(context.Terminal.transform.position);
        context.Output.PrintResponse($"ENTITIES DETECTED: {count}");
        context.Audio.PlayResponse();
    }
}
