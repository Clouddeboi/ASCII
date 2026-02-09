using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    
    private bool isOpen = false;

    void Start()
    {
        // Make sure menu is closed at start
        menuPanel.SetActive(false);
        isOpen = false;
    }

    void Update()
    {
        // Toggle menu with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isOpen && PlayerStatesManager.Instance.IsInState(PlayerStates.Default))
            {
                OpenMenu();
            }
            else if (isOpen)
            {
                CloseMenu();
            }
        }
    }

    public void OpenMenu()
    {
        isOpen = true;
        menuPanel.SetActive(true);
        PlayerStatesManager.Instance.SetState(PlayerStates.Menu);
    }

    public void CloseMenu()
    {
        isOpen = false;
        menuPanel.SetActive(false);
        PlayerStatesManager.Instance.SetState(PlayerStates.Default);

        // Force mouse logic fallback
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Public methods for UI buttons
    public void OnResumeClicked()
    {
        CloseMenu();
    }

    public void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}