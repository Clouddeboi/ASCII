using System.Text;

public class HistoryCommand : ITerminalCommand
{
    public string Name => "/history";
    public string[] Aliases => new string[0];
    public string Description => "Display previous commands.";
    public string Usage => "/history";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        if (context.History.Entries.Count == 0)
        {
            context.Output.PrintResponse("NO COMMAND HISTORY.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("COMMAND HISTORY");
        sb.AppendLine("----------------");

        foreach (string entry in context.History.Entries)
            sb.AppendLine(entry);

        context.Output.PrintResponse(sb.ToString());
        context.Audio.PlayResponse();
    }
}
