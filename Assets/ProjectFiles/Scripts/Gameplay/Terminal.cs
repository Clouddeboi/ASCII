using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Terminal : MonoBehaviour
{
    public GameObject terminalPanel;
    public TMP_InputField inputField;
    public TMP_Text historyText;
    public Voice voice;

    private bool isOpen = false;
    private List<string> commandHistory = new List<string>();
    private int maxHistory = 10; //Show last 10 commands

    [SerializeField] private SpeechDetector speechDetector;

    void Update()
    {
        //Toggle terminal with Tab (only if in Default state)
        if (Input.GetKeyDown(KeyCode.Tab))
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

        //Execute command with Enter
        if (isOpen && Input.GetKeyDown(KeyCode.Return))
        {
            string command = inputField.text;
            if (!string.IsNullOrEmpty(command))
            {
                AddToHistory(command); //Save to history
                HandleCommand(command);
            }

            //Close terminal
            CloseTerminal();
        }

        //Also allow Escape to close terminal
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTerminal();
        }
    }

    void OpenTerminal()
    {
        isOpen = true;
        terminalPanel.SetActive(true);
        inputField.text = "";
        inputField.ActivateInputField(); //Focus input
        
        //Change player state to Terminal
        PlayerStatesManager.Instance.SetState(PlayerStates.Terminal);
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
    }

    void HandleCommand(string command)
    {
        string[] parts = command.Split(' ', 2);
        string cmd = parts[0].ToLower(); //Case-insensitive
        string args = parts.Length > 1 ? parts[1] : "";

        switch (cmd)
        {
        case "/speak":
            if (!string.IsNullOrEmpty(args))
            {
                bool success = voice.Speak(args);
                if (success)
                {
                    speechDetector.NotifySpeech(gameObject, args);
                }
                else
                {
                    Debug.Log("Speech failed - NPCs won't hear garbled text");
                }
            }
            break;

            default:
                Debug.Log("Unknown command: " + cmd);
                break;
        }
    }

    void AddToHistory(string command)
    {
        //Add new command to history
        commandHistory.Add(command);

        //Keep only last maxHistory commands
        if (commandHistory.Count > maxHistory)
            commandHistory.RemoveAt(0);

        //Update UI
        historyText.text = string.Join("\n", commandHistory);
    }
}