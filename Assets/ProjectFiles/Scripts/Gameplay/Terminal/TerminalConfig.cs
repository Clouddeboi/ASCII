using UnityEngine;

//Editable terminal presentation data (text, colors, timing).
[CreateAssetMenu(fileName = "New Terminal Config", menuName = "Terminal/Terminal Config")]
public class TerminalConfig : ScriptableObject
{
    [Header("Identity")]
    public string terminalName = "NEXUS TERMINAL";
    public string terminalVersion = "v2.7.1";

    [Header("Boot Sequence")]
    [TextArea(3, 10)]
    public string[] bootMessages =
    {
        "INITIALIZING NEXUS TERMINAL...",
        "LOADING KERNEL MODULES...",
        "CONNECTION ESTABLISHED.",
    };

    [Header("Status Panel")]
    public string currentUser = "GUEST";
    public string connectionStatus = "LINKED";
    public string systemStatus = "NOMINAL";

    [TextArea(2, 6)]
    public string[] randomStatusMessages =
    {
        "NO ANOMALIES DETECTED.",
        "SIGNAL INTEGRITY: STABLE.",
        "AWAITING INPUT...",
    };

    [Header("ASCII Art")]
    [TextArea(5, 20)]
    public string asciiArt =
        "  ___   ___  ___ ___ \n" +
        " / _ \\ / __|/ __|_ _|\n" +
        "| (_) |\\__ \\ (__ | | \n" +
        " \\___/ |___/\\___|___|";

    [Header("Presentation")]
    public Color systemTextColor = Color.green;
    public Color errorTextColor = Color.red;
    public Color responseTextColor = Color.cyan;
    public float charactersPerSecond = 40f;

    [Header("Output Window")]
    [Tooltip("Oldest lines are removed once the output log exceeds this many lines.")]
    public int maxOutputLines = 200;

    [Header("Spelling Correction")]
    [Range(1, 4)]
    public int suggestionMaxDistance = 2;
}
