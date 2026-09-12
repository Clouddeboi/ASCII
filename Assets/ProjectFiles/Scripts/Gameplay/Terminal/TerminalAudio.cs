using UnityEngine;

//Centralized terminal audio feedback using a single reusable AudioSource.
public class TerminalAudio : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;

    [Header("Typing")]
    [SerializeField] private AudioClip[] typingClips;
    [SerializeField] private Vector2 typingPitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] [Range(0f, 1f)] private float typingVolume = 0.5f;

    [Header("Send / Response / Loading")]
    [SerializeField] private AudioClip sendClip;
    [SerializeField] private AudioClip responseClip;
    [SerializeField] private AudioClip loadingClip;

    [Header("Open / Close")]
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    public void PlayTyping()
    {
        if (sfxSource == null || typingClips == null || typingClips.Length == 0) return;

        AudioClip clip = typingClips[Random.Range(0, typingClips.Length)];
        sfxSource.pitch = Random.Range(typingPitchRange.x, typingPitchRange.y);
        sfxSource.PlayOneShot(clip, typingVolume);
    }

    public void PlaySend()
    {
        PlayOneShotReset(sendClip);
    }

    public void PlayResponse()
    {
        PlayOneShotReset(responseClip);
    }

    public void PlayOpen()
    {
        PlayOneShotReset(openClip);
    }

    public void PlayClose()
    {
        StopLoading();
        PlayOneShotReset(closeClip);
    }

    public void StartLoading()
    {
        if (loopSource == null || loadingClip == null) return;
        loopSource.clip = loadingClip;
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StopLoading()
    {
        if (loopSource == null) return;
        loopSource.Stop();
        loopSource.clip = null;
    }

    private void PlayOneShotReset(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(clip);
    }

    private void OnDisable()
    {
        StopLoading();
    }
}
