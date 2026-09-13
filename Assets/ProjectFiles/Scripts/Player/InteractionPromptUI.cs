using UnityEngine;
using TMPro;

//Shows the "[E] Interact: <Object>" prompt when looking at an interactable, and brief result feedback
//(locked/failed/etc.) after an interaction attempt. Subscribes to InteractionController's events.
public class InteractionPromptUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;

    [Header("Settings")]
    [SerializeField] private string interactKeyLabel = "E";
    [SerializeField] private float feedbackDuration = 1.5f;

    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        HidePrompt();
    }

    private void OnEnable()
    {
        if (interactionController == null)
            return;

        interactionController.OnTargetChanged += HandleTargetChanged;
        interactionController.OnInteractResult += HandleInteractResult;
    }

    private void OnDisable()
    {
        if (interactionController == null)
            return;

        interactionController.OnTargetChanged -= HandleTargetChanged;
        interactionController.OnInteractResult -= HandleInteractResult;
    }

    private void HandleTargetChanged(IInteractable target)
    {
        //Don't clobber a result message that's still being shown.
        if (feedbackCoroutine != null)
            return;

        if (target == null)
        {
            HidePrompt();
            return;
        }

        ShowPrompt($"[{interactKeyLabel}] {target.DisplayName}");
    }

    private void HandleInteractResult(IInteractable target, InteractResult result)
    {
        string message = result switch
        {
            InteractResult.Success => null, //No feedback needed, object reacted normally.
            InteractResult.RequiresHack => $"{target.DisplayName} NOT RESPONDING - TRY /hack {target.InteractableId}",
            InteractResult.LockedPermanent => $"{target.DisplayName} IS PERMANENTLY LOCKED",
            InteractResult.LockedCode => $"{target.DisplayName} REQUIRES A CODE",
            InteractResult.LockedKey => $"{target.DisplayName} REQUIRES A KEY",
            InteractResult.Failed => $"{target.DisplayName} INTERACTION FAILED",
            _ => null
        };

        Debug.Log($"[Interaction] {target.DisplayName} -> {result}");

        if (message == null)
            return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(ShowFeedbackThenRevert(message));
    }

    private System.Collections.IEnumerator ShowFeedbackThenRevert(string message)
    {
        ShowPrompt(message);
        yield return new WaitForSeconds(feedbackDuration);
        feedbackCoroutine = null;

        //Revert to the normal prompt if still looking at something, otherwise hide.
        HandleTargetChanged(GetCurrentTarget());
    }

    private IInteractable GetCurrentTarget()
    {
        return interactionController != null ? interactionController.CurrentTarget : null;
    }

    private void ShowPrompt(string text)
    {
        if (promptPanel != null)
            promptPanel.SetActive(true);
        if (promptText != null)
            promptText.text = text;
    }

    private void HidePrompt()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }
}
