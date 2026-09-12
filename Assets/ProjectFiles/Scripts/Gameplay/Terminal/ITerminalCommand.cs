// Contract every terminal command must implement to be registered.
public interface ITerminalCommand
{
    string Name { get; }
    string[] Aliases { get; }
    string Description { get; }
    string Usage { get; }
    bool ClosesTerminal { get; }

    bool IsUnlocked(TerminalContext context);
    void Execute(string[] args, TerminalContext context);
}
