using System;
using UnityEngine;

public class PlayerStatesManager : MonoBehaviour
{
    [SerializeField] private PlayerStates currentState = PlayerStates.Default;
    private PlayerStates previousState;

    //Events for state changes
    public event Action<PlayerStates, PlayerStates> OnStateChanged;
    public event Action<PlayerStates> OnStateEnter;
    public event Action<PlayerStates> OnStateExit;

    // Singleton pattern (optional)
    public static PlayerStatesManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        //Initialize the starting state
        EnterState(currentState);
    }

    public void SetState(PlayerStates newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        
        // Exit current state
        ExitState(currentState);
        
        // Update state
        currentState = newState;
        
        // Enter new state
        EnterState(currentState);
        
        // Trigger state changed event
        OnStateChanged?.Invoke(previousState, currentState);
    }

    private void EnterState(PlayerStates state)
    {
        Debug.Log($"Entering state: {state}");
        
        switch (state)
        {
            case PlayerStates.Default:
                EnterDefaultState();
                break;
            case PlayerStates.Menu:
                EnterMenuState();
                break;
            case PlayerStates.Terminal:
                EnterTerminalState();
                break;
            case PlayerStates.NPC:
                EnterNPCState();
                break;
            case PlayerStates.Cutscene:
                EnterCutsceneState();
                break;
        }
        
        OnStateEnter?.Invoke(state);
    }

    private void ExitState(PlayerStates state)
    {
        Debug.Log($"Exiting state: {state}");
        
        switch (state)
        {
            case PlayerStates.Default:
                ExitDefaultState();
                break;
            case PlayerStates.Menu:
                ExitMenuState();
                break;
            case PlayerStates.Terminal:
                ExitTerminalState();
                break;
            case PlayerStates.NPC:
                ExitNPCState();
                break;
            case PlayerStates.Cutscene:
                ExitCutsceneState();
                break;
        }
        
        OnStateExit?.Invoke(state);
    }

    // State-specific enter methods
    private void EnterDefaultState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void EnterMenuState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f; // Pause game
    }

    private void EnterTerminalState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EnterNPCState()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EnterCutsceneState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // State-specific exit methods
    private void ExitDefaultState()
    {
        // Cleanup default state
    }

    private void ExitMenuState()
    {
        Time.timeScale = 1f; //Resume game
    }

    private void ExitTerminalState()
    {
        // Cleanup terminal
    }

    private void ExitNPCState()
    {
        // Close dialogue UI
    }

    private void ExitCutsceneState()
    {
        // Cleanup cutscene
    }

    // Utility methods
    public PlayerStates GetCurrentState() => currentState;
    public PlayerStates GetPreviousState() => previousState;
    public bool IsInState(PlayerStates state) => currentState == state;
    
    public bool CanPlayerMove()
    {
        return currentState == PlayerStates.Default;
    }

    public bool CanPlayerInteract()
    {
        return currentState == PlayerStates.Default;
    }
}