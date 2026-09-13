using UnityEngine;

//Common base for interactable objects (doors, boxes, buttons, etc.).
//Handles lock-state gating, hack-code challenges and success/fail/locked SFX.
//Subclasses only need to implement PerformAction() with the object-specific behaviour.
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    [SerializeField] private string interactableId = "generic";
    [SerializeField] private string displayName = "Object";
    [SerializeField] private InteractionType type = InteractionType.Generic;
    [SerializeField] private InteractionMode mode = InteractionMode.Normal;

    [Header("Lock")]
    [SerializeField] private LockState lockState = LockState.None;
    [SerializeField] private string requiredCode;
    [SerializeField] private string requiredKeyId;

    [Header("Audio")]
    [SerializeField] private AudioSource objectAudioSource;
    [SerializeField] private AudioClip successSfx;
    [SerializeField] private AudioClip failSfx;
    [SerializeField] private AudioClip lockedSfx;

    public string InteractableId => interactableId;
    public string DisplayName => displayName;
    public virtual string InteractVerb => "Interact";
    public InteractionType Type => type;
    public InteractionMode Mode => mode;
    public LockState CurrentLock => lockState;
    public Transform Transform => transform;

    protected virtual void Awake()
    {
        InteractableRegistry.Instance?.Register(this);
    }

    protected virtual void OnDestroy()
    {
        InteractableRegistry.Instance?.Unregister(this);
        if (HackSessionState.PendingTarget == (IInteractable)this)
            HackSessionState.Clear();
    }

    public InteractResult Interact()
    {
        if (mode == InteractionMode.HackOnly)
        {
            PlayFailSfx();
            return InteractResult.RequiresHack;
        }

        switch (lockState)
        {
            case LockState.Permanent:
                PlayLockedSfx();
                return InteractResult.LockedPermanent;
            case LockState.Code:
                PlayLockedSfx();
                return InteractResult.LockedCode;
            case LockState.Key:
                if (!Inventory.HasItem(requiredKeyId))
                {
                    PlayLockedSfx();
                    return InteractResult.LockedKey;
                }
                break;
        }

        bool handled = PerformAction(null);
        if (!handled)
        {
            PlayFailSfx();
            return InteractResult.Failed;
        }

        PlaySuccessSfx();
        return InteractResult.Success;
    }

    public HackResult Hack(string[] args)
    {
        switch (lockState)
        {
            case LockState.Permanent:
                PlayLockedSfx();
                return HackResult.LockedPermanent;
            case LockState.Code:
                //Allow the code to be supplied inline, e.g. "/hack door 1234", not just via a follow-up /code.
                if (args != null && args.Length > 0)
                {
                    if (string.Equals(args[0], requiredCode, System.StringComparison.Ordinal))
                    {
                        lockState = LockState.None;
                        break;
                    }

                    PlayFailSfx();
                    HackSessionState.BeginCodeChallenge(this);
                    return HackResult.RequiresCode;
                }

                HackSessionState.BeginCodeChallenge(this);
                return HackResult.RequiresCode;
            case LockState.Key:
                if (!Inventory.HasItem(requiredKeyId))
                {
                    PlayLockedSfx();
                    return HackResult.RequiresKey;
                }
                break;
        }

        bool handled = PerformAction(args);
        if (!handled)
        {
            PlayFailSfx();
            return HackResult.Failed;
        }

        PlaySuccessSfx();
        return HackResult.Success;
    }

    public bool SubmitHackCode(string code)
    {
        if (lockState != LockState.Code)
            return false;

        if (!string.Equals(code, requiredCode, System.StringComparison.Ordinal))
        {
            PlayFailSfx();
            return false;
        }

        lockState = LockState.None;
        PerformAction(null);
        PlaySuccessSfx();
        return true;
    }

    //Executed once all lock checks have passed. Return false if the object could not perform its action.
    //args is null for a plain E interaction / accepted code, and populated for /hack <id> <args...>.
    protected abstract bool PerformAction(string[] args);

    protected void PlaySuccessSfx() => PlayClip(successSfx);
    protected void PlayFailSfx() => PlayClip(failSfx);
    protected void PlayLockedSfx() => PlayClip(lockedSfx);

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;

        if (objectAudioSource != null)
            objectAudioSource.PlayOneShot(clip);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }
}
