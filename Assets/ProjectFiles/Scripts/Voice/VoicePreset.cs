using UnityEngine;

[CreateAssetMenu(fileName = "New Voice Preset", menuName = "Audio/Voice Preset")]
public class VoicePreset : ScriptableObject
{
    [Header("Info")]
    public string presetName = "New Voice";
    [TextArea]
    public string description;

    [Header("SAM Parameters (0-255, see SamSharp README for reference values)")]
    [Range(0, 255)] public int samPitch = 64;
    [Range(0, 255)] public int samSpeed = 72;
    [Range(0, 255)] public int samThroat = 128;
    [Range(0, 255)] public int samMouth = 128;
    public bool singMode = false;

    [Header("Unity Playback (secondary, coarser knob - see docs)")]
    [Range(0.5f, 2f)] public float audioPitch = 1f;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Audio Effects")]
    public VoiceEffectsSettings effects = new VoiceEffectsSettings();
}

[System.Serializable]
public class VoiceEffectsSettings
{
    [Header("Distortion")]
    public bool enableDistortion = false;
    [Range(0f, 1f)] public float distortionLevel = 0f;

    [Header("Reverb")]
    public bool enableReverb = false;
    public AudioReverbPreset reverbPreset = AudioReverbPreset.Generic;
    [Range(0f, 1f)] public float reverbLevel = 0.5f;

    [Header("Low Pass (muffled/radio-behind-wall feel)")]
    public bool enableLowPass = false;
    [Range(10f, 22000f)] public float lowPassCutoff = 5000f;

    [Header("High Pass (thin/radio/robotic feel)")]
    public bool enableHighPass = false;
    [Range(10f, 22000f)] public float highPassCutoff = 1000f;

    [Header("Echo")]
    public bool enableEcho = false;
    [Range(10f, 5000f)] public float echoDelay = 250f;
    [Range(0f, 1f)] public float echoDecay = 0.3f;

    [Header("Chorus (robot/doubled-voice feel)")]
    public bool enableChorus = false;
    [Range(0f, 1f)] public float chorusDepth = 0.3f;
}
