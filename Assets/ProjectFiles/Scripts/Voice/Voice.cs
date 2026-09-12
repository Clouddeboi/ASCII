using UnityEngine;
using SamSharp;

public class Voice : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource source;

    [Header("Preset")]
    [SerializeField] private VoicePreset preset;

    private int? overridePitch;
    private int? overrideSpeed;
    private int? overrideThroat;
    private int? overrideMouth;

    private AudioDistortionFilter distortionFilter;
    private AudioReverbFilter reverbFilter;
    private AudioLowPassFilter lowPassFilter;
    private AudioHighPassFilter highPassFilter;
    private AudioEchoFilter echoFilter;
    private AudioChorusFilter chorusFilter;

    private void Awake()
    {
        CacheFilters();
    }

    private void CacheFilters()
    {
        if (source == null) return;

        distortionFilter = source.GetComponent<AudioDistortionFilter>();
        if (distortionFilter == null) distortionFilter = source.gameObject.AddComponent<AudioDistortionFilter>();

        reverbFilter = source.GetComponent<AudioReverbFilter>();
        if (reverbFilter == null) reverbFilter = source.gameObject.AddComponent<AudioReverbFilter>();

        lowPassFilter = source.GetComponent<AudioLowPassFilter>();
        if (lowPassFilter == null) lowPassFilter = source.gameObject.AddComponent<AudioLowPassFilter>();

        highPassFilter = source.GetComponent<AudioHighPassFilter>();
        if (highPassFilter == null) highPassFilter = source.gameObject.AddComponent<AudioHighPassFilter>();

        echoFilter = source.GetComponent<AudioEchoFilter>();
        if (echoFilter == null) echoFilter = source.gameObject.AddComponent<AudioEchoFilter>();

        chorusFilter = source.GetComponent<AudioChorusFilter>();
        if (chorusFilter == null) chorusFilter = source.gameObject.AddComponent<AudioChorusFilter>();

        //All filters start disabled; ApplyAudioEffects enables only what the active preset needs.
        distortionFilter.enabled = false;
        reverbFilter.enabled = false;
        lowPassFilter.enabled = false;
        highPassFilter.enabled = false;
        echoFilter.enabled = false;
        chorusFilter.enabled = false;
    }

    public void SetPreset(VoicePreset newPreset)
    {
        preset = newPreset;
        ClearOverrides();
    }

    public void ClearOverrides()
    {
        overridePitch = null;
        overrideSpeed = null;
        overrideThroat = null;
        overrideMouth = null;
    }

    public void OverridePitch(int value) => overridePitch = Mathf.Clamp(value, 0, 255);
    public void OverrideSpeed(int value) => overrideSpeed = Mathf.Clamp(value, 0, 255);
    public void OverrideThroat(int value) => overrideThroat = Mathf.Clamp(value, 0, 255);
    public void OverrideMouth(int value) => overrideMouth = Mathf.Clamp(value, 0, 255);

    public bool Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (preset == null || source == null)
        {
            Debug.LogWarning("Voice.Speak requires both a VoicePreset and an AudioSource to be assigned.", this);
            return false;
        }

        try
        {
            string sanitizedText = SanitizeText(text);
            if (string.IsNullOrEmpty(sanitizedText))
                return false;

            AudioClip clip = GenerateClip(sanitizedText);

            ApplyPlaybackSettings();
            ApplyAudioEffects();

            source.PlayOneShot(clip);

            //Original unsanitized text is kept for subtitles.
            if (SubtitleManager.Instance != null)
                SubtitleManager.Instance.ShowSubtitleWithAudio(text, clip);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"SAM TTS failed for text: '{text}'. Error: {e.Message}", this);
            if (SubtitleManager.Instance != null)
                SubtitleManager.Instance.ShowSubtitle("[ERROR] Speech gargled with oil, speech cannot be detected.", 3f);
            return false;
        }
    }

    private AudioClip GenerateClip(string sanitizedText)
    {
        var options = new Options(
            pitch: (byte)(overridePitch ?? preset.samPitch),
            mouth: (byte)(overrideMouth ?? preset.samMouth),
            throat: (byte)(overrideThroat ?? preset.samThroat),
            speed: (byte)(overrideSpeed ?? preset.samSpeed),
            singMode: preset.singMode);

        var sam = new Sam(options);
        byte[] bytes = sam.Speak(sanitizedText);
        float[] samples = ConvertToFloats(bytes);

        AudioClip clip = AudioClip.Create("SAM", samples.Length, 1, 22050, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void ApplyPlaybackSettings()
    {
        source.pitch = preset.audioPitch;
        source.volume = preset.volume;
    }

    private void ApplyAudioEffects()
    {
        var fx = preset.effects;

        distortionFilter.enabled = fx.enableDistortion;
        if (fx.enableDistortion)
            distortionFilter.distortionLevel = fx.distortionLevel;

        reverbFilter.enabled = fx.enableReverb;
        if (fx.enableReverb)
        {
            reverbFilter.reverbPreset = fx.reverbPreset;
            reverbFilter.dryLevel = Mathf.Lerp(0, -10000, 1f - fx.reverbLevel);
            reverbFilter.room = Mathf.Lerp(-10000, 0, fx.reverbLevel);
        }

        lowPassFilter.enabled = fx.enableLowPass;
        if (fx.enableLowPass)
            lowPassFilter.cutoffFrequency = fx.lowPassCutoff;

        highPassFilter.enabled = fx.enableHighPass;
        if (fx.enableHighPass)
            highPassFilter.cutoffFrequency = fx.highPassCutoff;

        echoFilter.enabled = fx.enableEcho;
        if (fx.enableEcho)
        {
            echoFilter.delay = fx.echoDelay;
            echoFilter.decayRatio = fx.echoDecay;
        }

        chorusFilter.enabled = fx.enableChorus;
        if (fx.enableChorus)
            chorusFilter.depth = fx.chorusDepth;
    }

    private string SanitizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = text.Replace("'", "");
        text = text.Replace("`", "");

        string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,!?-";
        var result = new System.Text.StringBuilder(text.Length);

        foreach (char c in text)
            result.Append(allowed.IndexOf(c) >= 0 ? c : ' ');

        string collapsed = result.ToString();
        while (collapsed.Contains("  "))
            collapsed = collapsed.Replace("  ", " ");

        return collapsed.Trim();
    }

    private float[] ConvertToFloats(byte[] data)
    {
        float[] f = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
            f[i] = (data[i] - 128) / 128f;

        return f;
    }
}