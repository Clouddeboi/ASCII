using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("AudioSources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private AudioSource AmbienceSource;

    // [Header("Music")]
    // [SerializeField] private AudioClip backgroundMusic;

    // [Header("SFX")]
    // [SerializeField] private AudioClip FootSteps;

    // [Header("Ambience")]
    // [SerializeField] private AudioClip WindAmbience;

    private void Start()
    {
        //Play background music
        // if (backgroundMusic != null)
        // {
        //     musicSource.clip = backgroundMusic;
        //     musicSource.loop = true;
        //     musicSource.Play();
        // }
    }

    //Play a specific music clip
    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    //Play a specific SFX clip
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            SFXSource.PlayOneShot(clip);
        }
    }

    //Play a specific Ambience clip
    public void PlayAmbience(AudioClip clip)
    {
        if (clip != null)
        {
            AmbienceSource.PlayOneShot(clip);
        }
    }
}