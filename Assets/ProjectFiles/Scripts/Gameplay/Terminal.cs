using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Terminal : MonoBehaviour
{
    public GameObject terminalPanel;
    public TMP_InputField inputField;
    public Voice voice;

    private bool isOpen = false;

    void Update()
    {
        // Toggle terminal
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;
            terminalPanel.SetActive(isOpen);

            if (isOpen)
            {
                inputField.text = "";
                inputField.ActivateInputField();//Focus the input
            }
        }

        //Execute command when Enter is pressed
        if (isOpen && Input.GetKeyDown(KeyCode.Return))
        {
            string command = inputField.text;
            HandleCommand(command);

            //Close terminal after executing command
            terminalPanel.SetActive(false);
            isOpen = false;
        }
    }

    void HandleCommand(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;

        //Split command and args
        string[] parts = command.Split(' ', 2);
        string cmd = parts[0].ToLower();
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
}
