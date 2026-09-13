using UnityEngine;

//Drives the player hand's Animator and SFX based on interaction type/outcome.
public class PlayerHandAnimator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator handAnimator;
    [SerializeField] private AudioSource handAudioSource;

    [Header("SFX")]
    [SerializeField] private AudioClip failClip;
    [SerializeField] private AudioClip lockedClip;

    //Animator trigger names per interaction type, matched by index against InteractionType.
    [SerializeField]
    private string[] triggersByType =
    {
        "Interact_Generic",
        "Interact_Door",
        "Interact_Box",
        "Interact_Button",
        "Interact_Lever",
        "Interact_Container"
    };

    public void PlayInteract(InteractionType type)
    {
        if (handAnimator == null) return;

        int index = (int)type;
        string trigger = index >= 0 && index < triggersByType.Length ? triggersByType[index] : triggersByType[0];
        handAnimator.SetTrigger(trigger);
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
