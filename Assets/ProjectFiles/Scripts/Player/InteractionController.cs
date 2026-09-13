using System;
using UnityEngine;

//Detects the interactable the player is looking at and handles "press E" interaction.
public class InteractionController : MonoBehaviour
{
    public static InteractionController Instance { get; private set; }

    [Header("Detection")]
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    [Header("Components")]
    [SerializeField] private PlayerHandAnimator handAnimator;

    private IInteractable currentTarget;

    public IInteractable CurrentTarget => currentTarget;

    //World position to use for range checks (e.g. /hack) - same origin normal E interaction raycasts from.
    public Vector3 OriginPosition => interactionOrigin != null ? interactionOrigin.position : transform.position;

    //Raised whenever the looked-at interactable changes, for UI prompt hookup ("Press E to open Door").
    public event Action<IInteractable> OnTargetChanged;

    //Raised after an interaction attempt resolves, for UI feedback (locked/failed/success).
    public event Action<IInteractable, InteractResult> OnInteractResult;

    private void Awake()
    {
        Instance = this;

        if (interactionOrigin == null)
            Debug.LogWarning("[Interaction] interactionOrigin is not assigned - target raycast will never hit anything.", this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (PlayerStatesManager.Instance != null && !PlayerStatesManager.Instance.IsInState(PlayerStates.Default))
        {
            SetTarget(null);
            return;
        }

        UpdateTarget();

        //WasPressedThisFrame fires immediately on key-down; WasPerformedThisFrame would wait on the
        //Interact action's Hold interaction to finish, which reads like the key press is being ignored.
        if (InputManager.Instance != null && InputManager.Instance.InteractAction.WasPressedThisFrame())
        {
            if (currentTarget != null)
                TryInteract();
            else
                Debug.Log("[Interaction] E pressed but no interactable in range.");
        }
    }

    private void UpdateTarget()
    {
        IInteractable found = null;

        if (interactionOrigin != null &&
            Physics.Raycast(interactionOrigin.position, interactionOrigin.forward, out RaycastHit hit, interactionRange, interactableLayer))
        {
            found = hit.collider.GetComponentInParent<IInteractable>();
        }

        if (!ReferenceEquals(found, currentTarget))
            SetTarget(found);
    }

    private void SetTarget(IInteractable target)
    {
        currentTarget = target;
        OnTargetChanged?.Invoke(currentTarget);
    }

    private void TryInteract()
    {
        IInteractable target = currentTarget;
        InteractResult result = target.Interact();

        handAnimator?.PlayInteract(target.Type);
        handAnimator?.PlayResultFeedback(result);

        Debug.Log($"[Interaction] {target.DisplayName} ({target.InteractableId}) -> {result}");

        if (result == InteractResult.RequiresHack)
            Debug.Log($"{target.DisplayName} is not responding. Try /hack {target.InteractableId}.");

        OnInteractResult?.Invoke(target, result);
    }
}
