using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;

    [Header("Save / Load")]
    [SerializeField] private ConfirmationDialog confirmationDialog;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isOpen = false;

    void Start()
    {
        menuPanel.SetActive(false);
        isOpen = false;
    }

    void Update()
    {
        if (InputManager.Instance == null)
            return;

        if (InputManager.Instance.CancelAction.WasPressedThisFrame())
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnResumeClicked()
    {
        CloseMenu();
    }

    public void OnSaveClicked()
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.HasActiveSave)
        {
            Debug.LogWarning("[MenuManager] No active save slot to save to (did you start via the save slot panel?).");
            return;
        }

        if (confirmationDialog == null)
        {
            Debug.LogError("[MenuManager] Confirmation Dialog not assigned in the Inspector.", this);
            return;
        }

        confirmationDialog.Show(
            "Overwrite your current save with this progress?",
            onConfirmed: () => SaveManager.Instance.SaveGame());
    }

    public void OnLoadClicked()
    {
        if (SaveManager.Instance == null || !SaveManager.Instance.HasActiveSave)
        {
            Debug.LogWarning("[MenuManager] No active save slot to load.");
            return;
        }

        if (confirmationDialog == null)
        {
            Debug.LogError("[MenuManager] Confirmation Dialog not assigned in the Inspector.", this);
            return;
        }

        string slotId = SaveManager.Instance.CurrentSlotId;
        confirmationDialog.Show(
            "Load your last save? Unsaved progress will be lost.",
            onConfirmed: () =>
            {
                CloseMenu();
                SaveManager.Instance.LoadGame(slotId);
            });
    }

    public void OnMainMenuClicked()
    {
        if (confirmationDialog == null)
        {
            Debug.LogError("[MenuManager] Confirmation Dialog not assigned in the Inspector.", this);
            return;
        }

        confirmationDialog.Show(
            "Return to the main menu? Unsaved progress will be lost.",
            onConfirmed: () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(mainMenuSceneName);
            });
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