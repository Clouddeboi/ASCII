using UnityEngine;

//Contract every interactable object (door, box, button, etc.) must implement.
public interface IInteractable
{
    string InteractableId { get; } //Matched against /hack <id>, e.g. "door"
    string DisplayName { get; }
    InteractionType Type { get; }
    InteractionMode Mode { get; }
    LockState CurrentLock { get; }
    Transform Transform { get; }

    //Normal "press E" interaction.
    InteractResult Interact();

    //Forced interaction via the "/hack <id> [args]" terminal command. Args allow future verbs (e.g. "turnon").
    HackResult Hack(string[] args);

    //Response to a follow-up "/code <code>" command while this interactable is awaiting one.
    bool SubmitHackCode(string code);
}
