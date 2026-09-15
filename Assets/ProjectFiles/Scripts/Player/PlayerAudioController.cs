using UnityEngine;

//Per-tag footstep clip set (e.g. "Ground", "Wood", "Metal", "Water"), falls back to the default set if no tag matches.
[System.Serializable]
public class SurfaceFootstepSet
{
    public string surfaceTag = "Untagged";
    public AudioClip[] clips;
}

//Drives movement SFX (footsteps, jump, land) for the player. Called into by PlayerMovement, same pattern as PlayerHandAnimator.
public class PlayerAudioController : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float baseStepInterval = 0.5f;
    [SerializeField] private float minStepInterval = 0.25f;
    [SerializeField] private float stepIntervalSpeedReference = 6f;
    [SerializeField] private Vector2 footstepPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] [Range(0f, 1f)] private float footstepVolume = 0.7f;
    private float stepTimer;

    [Header("Footsteps - Surface Detection")]
    [SerializeField] private SurfaceFootstepSet[] surfaceFootsteps;
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private float groundCheckDistance = 1.2f;
    [SerializeField] private LayerMask groundCheckMask = ~0;

    [Header("Jump / Land")]
    [SerializeField] private AudioSource oneShotSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip hardLandClip;

    public void UpdateFootsteps(bool grounded, float flatSpeed, float moveSpeedReference)
    {
        if (!grounded || flatSpeed < 0.1f || footstepSource == null || footstepClips == null || footstepClips.Length == 0)
        {
            stepTimer = baseStepInterval;
            return;
        }

        float speedRatio = Mathf.Clamp01(flatSpeed / Mathf.Max(stepIntervalSpeedReference, 0.01f));
        float interval = Mathf.Lerp(baseStepInterval, minStepInterval, speedRatio);

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = interval;
        }
    }

    private void PlayFootstep()
    {
        AudioClip[] clips = GetClipsForCurrentSurface();
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        footstepSource.pitch = Random.Range(footstepPitchRange.x, footstepPitchRange.y);
        footstepSource.PlayOneShot(clip, footstepVolume);
    }

    //Raycasts down from under the player's feet to find the surface tag, and returns the matching clip set (or the default set).
    private AudioClip[] GetClipsForCurrentSurface()
    {
        if (surfaceFootsteps != null && surfaceFootsteps.Length > 0)
        {
            Vector3 origin = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundCheckMask))
            {
                foreach (SurfaceFootstepSet surface in surfaceFootsteps)
                {
                    if (surface.clips != null && surface.clips.Length > 0 && hit.collider.CompareTag(surface.surfaceTag))
                        return surface.clips;
                }
            }
        }

        return footstepClips;
    }

    public void PlayJump()
    {
        if (jumpClip != null)
            oneShotSource?.PlayOneShot(jumpClip);
    }

    public void PlayLand(bool hardLanding)
    {
        AudioClip clip = hardLanding && hardLandClip != null ? hardLandClip : landClip;
        if (clip != null)
            oneShotSource?.PlayOneShot(clip);
    }
}
