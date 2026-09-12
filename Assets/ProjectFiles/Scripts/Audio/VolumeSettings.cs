using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider MasterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider AmbienceSlider;
    [SerializeField] private Slider VoiceSlider;

    private void Start()
    {
        if(PlayerPrefs.HasKey("MusicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetAmbienceVolume();
            SetVoiceVolume();
            SetMasterVolume();
        }
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("Music", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    public void SetAmbienceVolume()
    {
        float volume = AmbienceSlider.value;
        myMixer.SetFloat("Ambience", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("AmbienceVolume", volume);
    }
    
    public void SetVoiceVolume()
    {
        float volume = VoiceSlider.value;
        myMixer.SetFloat("Voice", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("VoiceVolume", volume);
    }

    public void SetMasterVolume()
    {
        float volume = MasterSlider.value;
        myMixer.SetFloat("Master", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        AmbienceSlider.value = PlayerPrefs.GetFloat("AmbienceVolume");
        VoiceSlider.value = PlayerPrefs.GetFloat("VoiceVolume");
        MasterSlider.value = PlayerPrefs.GetFloat("MasterVolume");

        SetMusicVolume();
        SetSFXVolume();
        SetAmbienceVolume();
        SetVoiceVolume();
        SetMasterVolume();
    }
}