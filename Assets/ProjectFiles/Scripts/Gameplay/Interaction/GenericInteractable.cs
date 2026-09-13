using UnityEngine;

//Placeholder interactable for objects with no real behaviour yet; just confirms E interaction is wired up.
public class GenericInteractable : InteractableBase
{
    protected override bool PerformAction(string[] args)
    {
        Debug.Log($"[Interaction] {DisplayName} interacted with (no behaviour implemented yet).");
        return true;
    }
}
