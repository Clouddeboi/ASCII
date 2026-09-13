using UnityEngine;

//Example interactable: toggles open/closed, driven by an Animator bool.
public class DoorInteractable : InteractableBase
{
    [Header("Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string openParam = "Open";

    private bool isOpen;

    protected override bool PerformAction(string[] args)
    {
        isOpen = !isOpen;
        if (doorAnimator != null)
            doorAnimator.SetBool(openParam, isOpen);

        return true;
    }
}
