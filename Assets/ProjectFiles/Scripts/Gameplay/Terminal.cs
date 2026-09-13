using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Terminal : MonoBehaviour
{
    [Header("Panels")]
    public GameObject terminalPanel;
    public TMP_InputField inputField;

    [Header("Display Areas")]
    [SerializeField] private TMP_Text asciiArtText;
    [SerializeField] private TMP_Text statusPanelText;

    [Header("Components")]
    public Voice voice;
    [SerializeField] private SpeechDetector speechDetector;
    [SerializeField] private TerminalOutput output;
    [SerializeField] private TerminalAudio audioController;
    [SerializeField] private TerminalConfig config;

    private bool isOpen = false;
    private bool suppressTypingSound = false;

    private readonly TerminalHistory history = new TerminalHistory();
    private TerminalCommandRegistry commandRegistry;
    private TerminalContext context;

    public TerminalCommandRegistry CommandRegistry => commandRegistry;

    void Awake()
    {
        commandRegistry = new TerminalCommandRegistry();
        commandRegistry.Register(new SpeakCommand());
        commandRegistry.Register(new HelpCommand());
        commandRegistry.Register(new StatusCommand());
        commandRegistry.Register(new PingCommand());
        commandRegistry.Register(new OverrideCommand());
        commandRegistry.Register(new ClearCommand());
        commandRegistry.Register(new HistoryCommand());
        commandRegistry.Register(new HackCommand());
        commandRegistry.Register(new CodeCommand());

        context = new TerminalContext
        {
            Terminal = this,
            Voice = voice,
            SpeechDetector = speechDetector,
            Output = output,
            Audio = audioController,
            History = history,
        };

        if (inputField != null)
            inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        //Toggle terminal with Tab (only if in Default state)
        if (keyboard.tabKey.wasPressedThisFrame)
        {
            if (!isOpen && PlayerStatesManager.Instance.IsInState(PlayerStates.Default))
            {
                OpenTerminal();
            }
            else if (isOpen)
            {
                CloseTerminal();
            }
        }

        if (!isOpen)
            return;

        //Execute command with Enter
        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            string command = inputField.text;
            if (!string.IsNullOrEmpty(command))
            {
                history.Add(command);
                ExecuteCommand(command);
            }
            SetInputTextSilently("");
        }
        //Also allow Escape to close terminal
        else if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CloseTerminal();
        }
        else if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            string previous = history.NavigatePrevious();
            if (previous != null)
                SetInputTextSilently(previous);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            string next = history.NavigateNext();
            if (next != null)
                SetInputTextSilently(next);
        }
    }

    void OpenTerminal()
    {
        isOpen = true;
        terminalPanel.SetActive(true);
        SetInputTextSilently("");
        inputField.ActivateInputField(); //Focus input

        //Change player state to Terminal
        PlayerStatesManager.Instance.SetState(PlayerStates.Terminal);

        audioController?.PlayOpen();
        RenderStatusPanel();
        PlayBootSequence();
    }

    void CloseTerminal()
    {
        isOpen = false;
        terminalPanel.SetActive(false);

        //Return to Default state
        if (PlayerStatesManager.Instance.IsInState(PlayerStates.Terminal))
        {
            PlayerStatesManager.Instance.SetState(PlayerStates.Default);
        }

        audioController?.PlayClose();
    }

    private void PlayBootSequence()
    {
        if (config == null || config.bootMessages == null)
            return;

        foreach (string line in config.bootMessages)
            output.PrintSystem(line);
    }

    private void RenderStatusPanel()
    {
        if (config == null)
            return;

        if (asciiArtText != null)
            output.TypeIntoText(asciiArtText, config.asciiArt);

        if (statusPanelText != null)
        {
            string randomMessage = config.randomStatusMessages != null && config.randomStatusMessages.Length > 0
                ? config.randomStatusMessages[Random.Range(0, config.randomStatusMessages.Length)]
                : "";

            string statusText =
                $"{config.terminalName} {config.terminalVersion}\n" +
                $"USER: {config.currentUser}\n" +
                $"CONNECTION: {config.connectionStatus}\n" +
                $"STATUS: {config.systemStatus}\n" +
                randomMessage;

            output.TypeIntoText(statusPanelText, statusText);
        }
    }

    private void ExecuteCommand(string rawCommand)
    {
        output.Print("> " + rawCommand);

        TerminalParser.ParsedCommand parsed = TerminalParser.Parse(rawCommand);
        if (string.IsNullOrEmpty(parsed.CommandName))
            return;

        if (commandRegistry.TryGet(parsed.CommandName, out ITerminalCommand command))
        {
            if (!command.IsUnlocked(context))
            {
                output.PrintError("COMMAND LOCKED: " + parsed.CommandName);
                return;
            }

            command.Execute(parsed.Args, context);

            if (command.ClosesTerminal)
                CloseTerminal();

            return;
        }

        int maxDistance = config != null ? config.suggestionMaxDistance : 2;
        string suggestion = SpellCorrection.FindClosestCommand(parsed.CommandName, commandRegistry.AllTokens, maxDistance);

        output.PrintError("UNKNOWN COMMAND: " + parsed.CommandName);
        if (suggestion != null)
            output.PrintError("DID YOU MEAN: " + suggestion + "?");
    }

    private void SetInputTextSilently(string text)
    {
        suppressTypingSound = true;
        inputField.text = text;
        inputField.caretPosition = text.Length;
        suppressTypingSound = false;
    }

    private void OnInputValueChanged(string _)
    {
        if (suppressTypingSound)
            return;

        audioController?.PlayTyping();
    }
}
