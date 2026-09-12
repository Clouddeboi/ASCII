using UnityEngine;

public class SpeakCommand : ITerminalCommand
{
    public string Name => "/speak";
    public string[] Aliases => new string[0];
    public string Description => "Transmit a vocal message.";
    public string Usage => "/speak <message>";
    public bool ClosesTerminal => true;

    public bool IsUnlocked(TerminalContext context) => true;

    public void Execute(string[] args, TerminalContext context)
    {
        if (args.Length == 0)
        {
            context.Output.PrintError("USAGE: " + Usage);
            return;
        }

        string message = string.Join(" ", args);
        context.Output.PrintSystem("TRANSMITTING...");
        context.Audio.PlaySend();

        bool success = context.Voice != null && context.Voice.Speak(message);

        if (success)
        {
            context.SpeechDetector?.NotifySpeech(context.Terminal.gameObject, message);
        }
        else
        {
            context.Output.PrintError("TRANSMISSION FAILED: SPEECH SYNTHESIS ERROR.");
        }
    }
}
