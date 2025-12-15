using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates pixel art rice bowl sprites for fish buffs
/// Each bowl has a different color with a cooked fish on top
/// </summary>
public class FishBuffSprites : MonoBehaviour
{
    public static FishBuffSprites Instance { get; private set; }

    private Dictionary<FishBuffType, Texture2D> buffSprites = new Dictionary<FishBuffType, Texture2D>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GenerateAllSprites();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void GenerateAllSprites()
    {
        // Generate bowl sprites for each buff type
        buffSprites[FishBuffType.SnappersDelight] = GenerateRiceBowlSprite(
            new Color(0.9f, 0.3f, 0.3f),    // Red bowl
            new Color(0.95f, 0.4f, 0.35f),  // Cooked red fish
            new Color(0.8f, 0.25f, 0.25f)   // Bowl shadow
        );

        buffSprites[FishBuffType.MarlinsLuck] = GenerateRiceBowlSprite(
            new Color(0.3f, 0.5f, 0.9f),    // Blue bowl
            new Color(0.4f, 0.6f, 0.95f),   // Cooked blue fish
            new Color(0.25f, 0.4f, 0.75f)   // Bowl shadow
        );

        buffSprites[FishBuffType.TroutsFortune] = GenerateRiceBowlSprite(
            new Color(1f, 0.85f, 0.2f),     // Yellow/Gold bowl
            new Color(1f, 0.7f, 0.5f),      // Cooked rainbow fish (pinkish)
            new Color(0.85f, 0.7f, 0.15f)   // Bowl shadow
        );

        buffSprites[FishBuffType.SunshoreSurge] = GenerateRiceBowlSprite(
            new Color(1f, 0.6f, 0.2f),      // Orange bowl
            new Color(1f, 0.85f, 0.4f),     // Cooked golden fish
            new Color(0.85f, 0.5f, 0.15f)   // Bowl shadow
        );

        buffSprites[FishBuffType.SnubnoseSpeed] = GenerateRiceBowlSprite(
            new Color(0.7f, 0.75f, 0.8f),   // Silver/Grey bowl
            new Color(0.75f, 0.78f, 0.82f), // Cooked grey fish
            new Color(0.55f, 0.6f, 0.65f)   // Bowl shadow
        );

        buffSprites[FishBuffType.SeahorsesBounty] = GenerateRiceBowlSprite(
            new Color(0.3f, 0.8f, 0.4f),    // Green bowl
            new Color(1f, 0.7f, 0.4f),      // Cooked orange seahorse
            new Color(0.25f, 0.65f, 0.3f)   // Bowl shadow
        );
    }

    Texture2D GenerateRiceBowlSprite(Color bowlColor, Color fishColor, Color shadowColor)
    {
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        // Clear with transparent
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        // Rice color
        Color rice = new Color(0.98f, 0.95f, 0.9f);
        Color riceShade = new Color(0.9f, 0.85f, 0.8f);

        // Draw bowl (bottom half)
        // Bowl is like a rounded trapezoid shape

        // Bowl outer rim (top)
        for (int x = 6; x < 26; x++)
        {
            tex.SetPixel(x, 14, bowlColor);
            tex.SetPixel(x, 15, bowlColor);
        }

        // Bowl body (curved sides)
        for (int y = 4; y < 14; y++)
        {
            int indent = (14 - y) / 3;
            for (int x = 6 + indent; x < 26 - indent; x++)
            {
                // Left edge shadow
                if (x == 6 + indent || x == 7 + indent)
                {
                    tex.SetPixel(x, y, shadowColor);
                }
                // Right edge highlight
                else if (x == 25 - indent || x == 24 - indent)
                {
                    Color highlight = Color.Lerp(bowlColor, Color.white, 0.2f);
                    tex.SetPixel(x, y, highlight);
                }
                else
                {
                    tex.SetPixel(x, y, bowlColor);
                }
            }
        }

        // Bowl bottom (rounded)
        for (int x = 10; x < 22; x++)
        {
            tex.SetPixel(x, 3, shadowColor);
        }
        for (int x = 12; x < 20; x++)
        {
            tex.SetPixel(x, 2, shadowColor);
        }

        // Rice inside bowl (visible above bowl rim)
        for (int x = 8; x < 24; x++)
        {
            tex.SetPixel(x, 16, riceShade);
            tex.SetPixel(x, 17, rice);
        }
        for (int x = 10; x < 22; x++)
        {
            tex.SetPixel(x, 18, rice);
        }
        // Rice texture dots
        tex.SetPixel(11, 17, riceShade);
        tex.SetPixel(15, 17, riceShade);
        tex.SetPixel(19, 17, riceShade);
        tex.SetPixel(13, 18, riceShade);
        tex.SetPixel(17, 18, riceShade);

        // Draw cooked fish on top of rice
        Color fishShade = Color.Lerp(fishColor, Color.black, 0.2f);
        Color fishHighlight = Color.Lerp(fishColor, Color.white, 0.3f);

        // Fish body (elongated oval, slightly curved)
        // Main body
        for (int x = 9; x < 23; x++)
        {
            int yOffset = (x < 16) ? (x - 9) / 4 : (22 - x) / 4;
            tex.SetPixel(x, 20 + yOffset, fishColor);
            tex.SetPixel(x, 21 + yOffset, fishColor);
            if (x > 10 && x < 21)
            {
                tex.SetPixel(x, 22 + yOffset, fishColor);
            }
        }

        // Fish head (left side, rounded)
        tex.SetPixel(8, 20, fishColor);
        tex.SetPixel(8, 21, fishColor);
        tex.SetPixel(7, 21, fishShade);

        // Fish tail (right side)
        tex.SetPixel(23, 21, fishColor);
        tex.SetPixel(24, 20, fishShade);
        tex.SetPixel(24, 22, fishShade);
        tex.SetPixel(25, 19, fishShade);
        tex.SetPixel(25, 23, fishShade);

        // Fish eye
        tex.SetPixel(10, 22, Color.white);
        tex.SetPixel(10, 22, new Color(0.1f, 0.1f, 0.1f));

        // Fish highlight (top edge)
        tex.SetPixel(12, 23, fishHighlight);
        tex.SetPixel(14, 23, fishHighlight);
        tex.SetPixel(16, 23, fishHighlight);

        // Fish grill marks (cooked appearance)
        tex.SetPixel(13, 21, fishShade);
        tex.SetPixel(16, 21, fishShade);
        tex.SetPixel(19, 21, fishShade);

        // Steam wisps above fish
        Color steam = new Color(1f, 1f, 1f, 0.5f);
        tex.SetPixel(12, 26, steam);
        tex.SetPixel(14, 27, steam);
        tex.SetPixel(16, 26, steam);
        tex.SetPixel(18, 28, steam);
        tex.SetPixel(13, 28, steam);
        tex.SetPixel(17, 27, steam);

        // Bowl decorative pattern (simple dots)
        Color pattern = Color.Lerp(bowlColor, Color.white, 0.4f);
        tex.SetPixel(10, 10, pattern);
        tex.SetPixel(16, 8, pattern);
        tex.SetPixel(22, 10, pattern);
        tex.SetPixel(13, 6, pattern);
        tex.SetPixel(19, 6, pattern);

        tex.Apply();
        return tex;
    }

    public Texture2D GetBuffSprite(FishBuffType type)
    {
        if (buffSprites.TryGetValue(type, out Texture2D sprite))
        {
            return sprite;
        }
        return null;
    }

    // Generate a larger version for UI display
    public Texture2D GetBuffSpriteLarge(FishBuffType type, int scale = 2)
    {
        Texture2D original = GetBuffSprite(type);
        if (original == null) return null;

        int newSize = original.width * scale;
        Texture2D scaled = new Texture2D(newSize, newSize);
        scaled.filterMode = FilterMode.Point;

        for (int x = 0; x < newSize; x++)
        {
            for (int y = 0; y < newSize; y++)
            {
                Color pixel = original.GetPixel(x / scale, y / scale);
                scaled.SetPixel(x, y, pixel);
            }
        }

        scaled.Apply();
        return scaled;
    }

    void OnDestroy()
    {
        foreach (var sprite in buffSprites.Values)
        {
            if (sprite != null) Destroy(sprite);
        }
        buffSprites.Clear();
    }
}
