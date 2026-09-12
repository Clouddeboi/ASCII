//Shared references passed to every terminal command on execution.
public class TerminalContext
{
    public Terminal Terminal;
    public Voice Voice;
    public SpeechDetector SpeechDetector;
    public TerminalOutput Output;
    public TerminalAudio Audio;
    public TerminalHistory History;
}
