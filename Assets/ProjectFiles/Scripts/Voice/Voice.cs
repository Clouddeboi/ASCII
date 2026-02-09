using UnityEngine;
using SamSharp;

public class Voice : MonoBehaviour
{
    public AudioSource source;
    Sam sam = new Sam();
    
    public void Speak(string text)
    {
        byte[] bytes = sam.Speak(text);

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
    }

    float[] ConvertToFloats(byte[] data)
    {
        float[] f = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
            f[i] = (data[i] - 128) / 128f;

        return f;
    }
}
