public class ClearCommand : ITerminalCommand
{
    public string Name => "/clear";
    public string[] Aliases => new string[0];
    public string Description => "Clear terminal history.";
    public string Usage => "/clear";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        context.Output.Clear();
    }
}
