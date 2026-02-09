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
    private int maxHistory = 10;//Show last 10 commands

    void Update()
    {
        //Toggle terminal with Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;
            terminalPanel.SetActive(isOpen);

            if (isOpen)
            {
                inputField.text = "";
                inputField.ActivateInputField();//Focus input
            }
        }

        //Execute command with Enter
        if (isOpen && Input.GetKeyDown(KeyCode.Return))
        {
            string command = inputField.text;
            if (!string.IsNullOrEmpty(command))
            {
                AddToHistory(command);//Save to history
                HandleCommand(command);
            }

            //Close terminal
            terminalPanel.SetActive(false);
            isOpen = false;
        }
    }

    void HandleCommand(string command)
    {
        string[] parts = command.Split(' ', 2);
        string cmd = parts[0].ToLower();//Case-insensitive
        string args = parts.Length > 1 ? parts[1] : "";

        switch (cmd)
        {
            case "/speak":
                if (!string.IsNullOrEmpty(args))
                    voice.Speak(args);
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
