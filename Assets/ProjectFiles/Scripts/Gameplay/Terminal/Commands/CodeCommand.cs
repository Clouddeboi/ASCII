//Submits a code for the interactable currently awaiting one from a prior /hack attempt.
public class CodeCommand : ITerminalCommand
{
    public string Name => "/code";
    public string[] Aliases => new string[0];
    public string Description => "Submit a code for a pending hack code challenge.";
    public string Usage => "/code <code>";
    public bool ClosesTerminal => false;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        if (!HackSessionState.HasPendingChallenge)
        {
            context.Output.PrintError("NO ACTIVE CODE CHALLENGE.");
            return;
        }

        if (args.Length == 0)
        {
            context.Output.PrintError("USAGE: " + Usage);
            return;
        }

        IInteractable target = HackSessionState.PendingTarget;
        bool accepted = target.SubmitHackCode(args[0]);

        if (accepted)
        {
            context.Output.PrintResponse("CODE ACCEPTED. ACCESS GRANTED: " + target.DisplayName);
            context.Audio.PlayResponse();
            HackSessionState.Clear();
        }
        else
        {
            context.Output.PrintError("INCORRECT CODE.");
        }
    }
}
