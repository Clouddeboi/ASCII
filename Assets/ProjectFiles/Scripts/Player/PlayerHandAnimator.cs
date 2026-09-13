using UnityEngine;

//Drives the player hand's Animator and SFX based on interaction type/outcome and movement state.
public class PlayerHandAnimator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator handAnimator;
    [SerializeField] private AudioSource handAudioSource;

    [Header("SFX")]
    [SerializeField] private AudioClip failClip;
    [SerializeField] private AudioClip lockedClip;

    [Header("Movement Animator Params")]
    [SerializeField] private string runningParam = "IsRunning";
    [SerializeField] private string jumpTrigger = "Jump";
    [SerializeField] private string fallingParam = "IsFalling";
    [SerializeField] private string groundedParam = "IsGrounded";

    //Animator trigger names per interaction type, matched by index against InteractionType.
    [SerializeField]
    private string[] triggersByType =
    {
        "Interact_Generic",
        "Interact_Door",
        "Interact_Box",
        "Interact_Button",
        "Interact_Lever",
        "Interact_Container",
        "Interact_Item"
    };

    public void PlayInteract(InteractionType type)
    {
        if (handAnimator == null) return;

        int index = (int)type;
        string trigger = index >= 0 && index < triggersByType.Length ? triggersByType[index] : triggersByType[0];
        handAnimator.SetTrigger(trigger);
    }

    public void SetRunning(bool isRunning)
    {
        if (handAnimator == null) return;
        handAnimator.SetBool(runningParam, isRunning);
    }

    public void PlayJump()
    {
        if (handAnimator == null) return;
        handAnimator.SetTrigger(jumpTrigger);
    }

    public void SetFalling(bool isFalling)
    {
        if (handAnimator == null) return;
        handAnimator.SetBool(fallingParam, isFalling);
    }

    public void SetGrounded(bool isGrounded)
    {
        if (handAnimator == null) return;
        handAnimator.SetBool(groundedParam, isGrounded);
    }

    public void PlayResultFeedback(InteractResult result)
    {
        switch (result)
        {
            case InteractResult.Failed:
            case InteractResult.RequiresHack:
                PlayClip(failClip);
                break;
            case InteractResult.LockedPermanent:
            case InteractResult.LockedCode:
            case InteractResult.LockedKey:
                PlayClip(lockedClip);
                break;
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null)
            handAudioSource?.PlayOneShot(clip);
    }
}
