using UnityEngine;
using SamSharp;

public class Voice : MonoBehaviour
{
    public AudioSource source;
    Sam sam = new Sam();
    
    public bool Speak(string text)
    {
        try
        {
            // Sanitize text for SAM compatibility
            string sanitizedText = SanitizeText(text);
            
            byte[] bytes = sam.Speak(sanitizedText);

            float[] samples = ConvertToFloats(bytes);

            AudioClip clip = AudioClip.Create(
                "SAM",
                samples.Length,
                1,
                22050,
                false
            );

            clip.SetData(samples, 0);
            source.PlayOneShot(clip);

            // Show original text in subtitles (not sanitized)
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitleWithAudio(text, clip);
            }
            
            return true; // Success
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"SAM TTS failed for text: '{text}'. Error: {e.Message}");
            SubtitleManager.Instance.ShowSubtitle("[ERROR] Speech gargled with oil, speech cannot be detected.", 3f);
            
            return false; // Failed
        }
    }

    private string SanitizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Replace apostrophes with nothing
        text = text.Replace("'", "");
        text = text.Replace("`", "");
        
        // Remove any other non-ASCII characters that SAM can't handle
        // Keep only letters, numbers, spaces, and basic punctuation
        string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,!?-";
        string result = "";
        
        foreach (char c in text)
        {
            if (allowed.Contains(c.ToString()))
            {
                result += c;
            }
            else
            {
                result += " "; // Replace unknown characters with space
            }
        }
        
        // Clean up multiple spaces
        while (result.Contains("  "))
        {
            result = result.Replace("  ", " ");
        }
        
        return result.Trim();
    }

    float[] ConvertToFloats(byte[] data)
    {
        float[] f = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
            f[i] = (data[i] - 128) / 128f;

        return f;
    }
}