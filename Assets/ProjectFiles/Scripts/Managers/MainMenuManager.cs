using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Root Panel")]
    [SerializeField] private GameObject mainPanel;

    [Header("Save Slot Panel")]
    [SerializeField] private GameObject saveSlotPanel;

    private readonly Stack<GameObject> panelStack = new Stack<GameObject>();

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject != mainPanel)
                child.gameObject.SetActive(false);
        }

        mainPanel.SetActive(true);
        panelStack.Push(mainPanel);
    }

    private void Update()
    {
        if (InputManager.Instance == null)
            return;

        if (InputManager.Instance.CancelAction.WasPressedThisFrame())
        {
            GoBack();
        }
    }

    public void OpenPanel(GameObject panel)
    {
        if (panel == null || panelStack.Peek() == panel)
            return;

        panelStack.Peek().SetActive(false);
        panel.SetActive(true);
        panelStack.Push(panel);
    }

    public void GoBack()
    {
        if (panelStack.Count <= 1)
            return;

        panelStack.Pop().SetActive(false);
        panelStack.Peek().SetActive(true);
    }

    public void OnPlayClicked()
    {
        OpenPanel(saveSlotPanel);
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
