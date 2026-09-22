using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public InputActionAsset inputActions;

    public InputAction MoveAction { get; private set; }
    public InputAction LookAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction SprintAction { get; private set; }
    public InputAction CancelAction { get; private set; }
    public InputAction InteractAction { get; private set; }
    public InputAction AttackAction { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var playerMap = inputActions.FindActionMap("Player");
        var uiMap = inputActions.FindActionMap("UI");

        MoveAction = playerMap.FindAction("Move");
        LookAction = playerMap.FindAction("Look");
        JumpAction = playerMap.FindAction("Jump");
        SprintAction = playerMap.FindAction("Sprint");
        CancelAction = uiMap.FindAction("Cancel");
        InteractAction = playerMap.FindAction("Interact");
        AttackAction = playerMap.FindAction("Attack");
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
