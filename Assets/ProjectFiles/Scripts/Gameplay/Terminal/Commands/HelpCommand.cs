using System.Text;

public class HelpCommand : ITerminalCommand
{
    public string Name => "/help";
    public string[] Aliases => new string[0];
    public string Description => "Display available commands.";
    public string Usage => "/help";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("NEXUS TERMINAL COMMAND LIST");
        sb.AppendLine("---------------------------");

        foreach (ITerminalCommand command in context.Terminal.CommandRegistry.GetUnlocked(context))
        {
            sb.AppendLine();
            sb.AppendLine(command.Usage);
            sb.AppendLine(command.Description);
        }

        context.Output.PrintResponse(sb.ToString());
        context.Audio.PlayResponse();
    }
}
