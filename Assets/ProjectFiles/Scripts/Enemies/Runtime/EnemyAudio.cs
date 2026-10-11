using UnityEngine;

//every call is a safe no-op if audioSource/clip is unassigned.
public class EnemyAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip alertClip;

    public void PlayAttack() => PlayClip(attackClip);
    public void PlayHit() => PlayClip(hitClip);
    public void PlayDeath() => PlayClip(deathClip);
    public void PlayAlert() => PlayClip(alertClip);

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
