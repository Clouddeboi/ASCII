public class StatusCommand : ITerminalCommand
{
    public string Name => "/status";
    public string[] Aliases => new string[0];
    public string Description => "Display player health.";
    public string Usage => "/status";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        if (PlayerHealth.Instance == null)
        {
            context.Output.PrintError("PLAYER STATUS UNAVAILABLE.");
            return;
        }

        context.Output.PrintResponse(
            "PLAYER STATUS\n-------------\n" +
            $"HEALTH: {PlayerHealth.Instance.CurrentHealth:0} / {PlayerHealth.Instance.MaxHealth:0}");
        context.Audio.PlayResponse();
    }
}
