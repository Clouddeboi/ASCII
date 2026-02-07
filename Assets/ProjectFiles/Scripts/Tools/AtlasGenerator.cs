using UnityEngine;
using UnityEditor;
using System.IO;

public class ASCIIAtlasGenerator
{
    [MenuItem("Tools/Generate ASCII Atlas")]
    static void GenerateAtlas()
    {
        //Define exact order (by file names)
        string[] asciiOrder = new string[]
        {
            "space", "dot", "colon", "minus", "equal", "plus",
            "asterisk", "hash", "percent", "at"
        };

        //Load selected textures
        Texture2D[] selectedTextures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);

        Texture2D[] orderedGlyphs = new Texture2D[asciiOrder.Length];

        //Match textures by name
        for (int i = 0; i < asciiOrder.Length; i++)
        {
            Texture2D tex = null;
            foreach (Texture2D t in selectedTextures)
            {
                if (t.name.ToLower().Contains(asciiOrder[i]))
                {
                    tex = t;
                    break;
                }
            }

            if (tex == null)
            {
                Debug.LogError($"No texture found for '{asciiOrder[i]}'!");
            }

            orderedGlyphs[i] = tex;
        }

        //Assume all glyphs same size
        int glyphWidth = orderedGlyphs[0].width;
        int glyphHeight = orderedGlyphs[0].height;
        int padding = 2;

        int atlasWidth = orderedGlyphs.Length * (glyphWidth + padding);
        int atlasHeight = glyphHeight;

        Texture2D atlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);

        //Fill with transparent
        Color[] blank = new Color[atlasWidth * atlasHeight];
        for (int i = 0; i < blank.Length; i++) blank[i] = Color.clear;
        atlas.SetPixels(blank);

        //Copy glyphs sequentially
        for (int i = 0; i < orderedGlyphs.Length; i++)
        {
            Texture2D g = orderedGlyphs[i];

            if (!g.isReadable)
            {
                Debug.LogError($"Texture {g.name} is not readable! Enable Read/Write in import settings.");
                continue;
            }

            Color[] pixels = g.GetPixels();
            int xPos = i * (glyphWidth + padding);
            atlas.SetPixels(xPos, 0, glyphWidth, glyphHeight, pixels);
        }

        atlas.Apply();

        //Save as PNG (sprite settings get applied manually)
        string folder = "Assets/Atlas";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        byte[] png = atlas.EncodeToPNG();
        File.WriteAllBytes(folder + "/asciiAtlas.png", png);

        AssetDatabase.Refresh();
        Debug.Log("ASCII atlas generated in exact order!");
    }
}
