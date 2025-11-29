using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates pixel art sprites for all fishing rod types
/// Used in inventory display and equipment panel
/// </summary>
public class RodSprites : MonoBehaviour
{
    public static RodSprites Instance { get; private set; }

    private Dictionary<int, Texture2D> rodTextures = new Dictionary<int, Texture2D>();
    private const int SIZE = 32;  // 32x32 pixel art

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GenerateAllRodSprites();
        }
        else Destroy(gameObject);
    }

    public Texture2D GetRodTexture(int rodIndex)
    {
        if (rodTextures.ContainsKey(rodIndex))
            return rodTextures[rodIndex];
        return rodTextures.ContainsKey(0) ? rodTextures[0] : null;
    }

    void GenerateAllRodSprites()
    {
        rodTextures[0] = CreateBasicRod();
        rodTextures[1] = CreateBronzeRod();
        rodTextures[2] = CreateSilverRod();
        rodTextures[3] = CreateGoldenRod();
        rodTextures[4] = CreateLegendaryRod();
        rodTextures[5] = CreateEpicRod();
    }

    Texture2D CreateTexture()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE);
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < SIZE; x++)
            for (int y = 0; y < SIZE; y++)
                tex.SetPixel(x, y, clear);
        return tex;
    }

    void FinalizeTexture(Texture2D tex)
    {
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
    }

    void DrawLine(Texture2D tex, int x1, int y1, int x2, int y2, Color col)
    {
        int dx = Mathf.Abs(x2 - x1);
        int dy = Mathf.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x1 >= 0 && x1 < SIZE && y1 >= 0 && y1 < SIZE)
                tex.SetPixel(x1, y1, col);

            if (x1 == x2 && y1 == y2) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x1 += sx; }
            if (e2 < dx) { err += dx; y1 += sy; }
        }
    }

    void DrawRect(Texture2D tex, int x, int y, int w, int h, Color col)
    {
        for (int px = x; px < x + w; px++)
            for (int py = y; py < y + h; py++)
                if (px >= 0 && px < SIZE && py >= 0 && py < SIZE)
                    tex.SetPixel(px, py, col);
    }

    // Basic Rod - Simple wooden rod
    Texture2D CreateBasicRod()
    {
        Texture2D tex = CreateTexture();

        Color wood = new Color(0.55f, 0.35f, 0.2f);
        Color woodDark = new Color(0.4f, 0.25f, 0.15f);
        Color line = new Color(0.7f, 0.7f, 0.7f);
        Color handle = new Color(0.3f, 0.2f, 0.1f);

        // Rod shaft (diagonal)
        DrawLine(tex, 4, 4, 26, 26, wood);
        DrawLine(tex, 5, 4, 27, 26, wood);
        DrawLine(tex, 4, 5, 26, 27, woodDark);

        // Handle grip
        DrawRect(tex, 2, 2, 5, 5, handle);

        // Tip
        tex.SetPixel(27, 27, line);
        tex.SetPixel(28, 28, line);

        // Simple fishing line
        DrawLine(tex, 28, 28, 30, 20, line);

        FinalizeTexture(tex);
        return tex;
    }

    // Bronze Rod - Slightly better with metal accents
    Texture2D CreateBronzeRod()
    {
        Texture2D tex = CreateTexture();

        Color bronze = new Color(0.8f, 0.5f, 0.2f);
        Color bronzeDark = new Color(0.6f, 0.35f, 0.15f);
        Color wood = new Color(0.5f, 0.3f, 0.18f);
        Color line = new Color(0.75f, 0.75f, 0.75f);

        // Rod shaft
        DrawLine(tex, 4, 4, 26, 26, wood);
        DrawLine(tex, 5, 4, 27, 26, wood);
        DrawLine(tex, 4, 5, 26, 27, wood);

        // Bronze rings/accents
        DrawRect(tex, 8, 8, 3, 3, bronze);
        DrawRect(tex, 14, 14, 3, 3, bronze);
        DrawRect(tex, 20, 20, 3, 3, bronze);

        // Handle with bronze end
        DrawRect(tex, 2, 2, 5, 5, bronzeDark);
        DrawRect(tex, 3, 3, 3, 3, bronze);

        // Tip
        tex.SetPixel(27, 27, bronze);
        tex.SetPixel(28, 28, bronze);

        // Fishing line
        DrawLine(tex, 28, 28, 30, 18, line);

        FinalizeTexture(tex);
        return tex;
    }

    // Silver Rod - Sleek metallic look
    Texture2D CreateSilverRod()
    {
        Texture2D tex = CreateTexture();

        Color silver = new Color(0.75f, 0.75f, 0.8f);
        Color silverBright = new Color(0.9f, 0.9f, 0.95f);
        Color silverDark = new Color(0.5f, 0.5f, 0.55f);
        Color line = new Color(0.85f, 0.85f, 0.9f);

        // Rod shaft - silver
        DrawLine(tex, 4, 4, 26, 26, silver);
        DrawLine(tex, 5, 4, 27, 26, silverBright);
        DrawLine(tex, 4, 5, 26, 27, silverDark);

        // Decorative bands
        DrawRect(tex, 6, 6, 3, 3, silverBright);
        DrawRect(tex, 12, 12, 3, 3, silverBright);
        DrawRect(tex, 18, 18, 3, 3, silverBright);
        DrawRect(tex, 24, 24, 2, 2, silverBright);

        // Handle
        DrawRect(tex, 2, 2, 5, 5, silverDark);
        tex.SetPixel(3, 3, silverBright);
        tex.SetPixel(4, 4, silverBright);

        // Tip with gem
        tex.SetPixel(27, 27, silverBright);
        tex.SetPixel(28, 28, new Color(0.6f, 0.8f, 1f)); // Blue gem

        // Fishing line
        DrawLine(tex, 28, 28, 30, 16, line);

        FinalizeTexture(tex);
        return tex;
    }

    // Golden Rod - Luxurious gold finish
    Texture2D CreateGoldenRod()
    {
        Texture2D tex = CreateTexture();

        Color gold = new Color(1f, 0.85f, 0.3f);
        Color goldBright = new Color(1f, 0.95f, 0.5f);
        Color goldDark = new Color(0.8f, 0.6f, 0.1f);
        Color line = new Color(1f, 0.9f, 0.6f);
        Color ruby = new Color(0.9f, 0.2f, 0.3f);

        // Rod shaft - gold
        DrawLine(tex, 4, 4, 26, 26, gold);
        DrawLine(tex, 5, 4, 27, 26, goldBright);
        DrawLine(tex, 4, 5, 26, 27, goldDark);
        DrawLine(tex, 5, 5, 27, 27, gold);

        // Ornate bands with gems
        DrawRect(tex, 6, 6, 3, 3, goldBright);
        tex.SetPixel(7, 7, ruby);
        DrawRect(tex, 12, 12, 3, 3, goldBright);
        tex.SetPixel(13, 13, ruby);
        DrawRect(tex, 18, 18, 3, 3, goldBright);
        tex.SetPixel(19, 19, ruby);

        // Ornate handle
        DrawRect(tex, 1, 1, 6, 6, goldDark);
        DrawRect(tex, 2, 2, 4, 4, gold);
        tex.SetPixel(3, 3, goldBright);
        tex.SetPixel(4, 4, ruby);

        // Tip with large gem
        tex.SetPixel(27, 27, goldBright);
        tex.SetPixel(28, 28, ruby);
        tex.SetPixel(29, 29, new Color(1f, 0.3f, 0.4f));

        // Golden fishing line
        DrawLine(tex, 29, 29, 31, 14, line);

        FinalizeTexture(tex);
        return tex;
    }

    // Legendary Rod - Magical purple glow
    Texture2D CreateLegendaryRod()
    {
        Texture2D tex = CreateTexture();

        Color purple = new Color(0.6f, 0.3f, 0.9f);
        Color purpleBright = new Color(0.8f, 0.5f, 1f);
        Color purpleDark = new Color(0.4f, 0.15f, 0.6f);
        Color magic = new Color(1f, 0.8f, 1f);
        Color gold = new Color(1f, 0.85f, 0.3f);

        // Magical glow aura
        for (int x = 3; x < 29; x++)
        {
            for (int y = 3; y < 29; y++)
            {
                float dist = Mathf.Abs(x - y);
                if (dist < 4)
                {
                    Color glow = new Color(0.5f, 0.2f, 0.7f, 0.15f);
                    tex.SetPixel(x, y, glow);
                }
            }
        }

        // Rod shaft - purple magical
        DrawLine(tex, 4, 4, 26, 26, purple);
        DrawLine(tex, 5, 4, 27, 26, purpleBright);
        DrawLine(tex, 4, 5, 26, 27, purpleDark);
        DrawLine(tex, 5, 5, 27, 27, purple);

        // Magical runes/sparkles
        tex.SetPixel(8, 8, magic);
        tex.SetPixel(12, 12, magic);
        tex.SetPixel(16, 16, magic);
        tex.SetPixel(20, 20, magic);
        tex.SetPixel(24, 24, magic);

        // Ornate gold bands
        DrawRect(tex, 6, 6, 2, 2, gold);
        DrawRect(tex, 14, 14, 2, 2, gold);
        DrawRect(tex, 22, 22, 2, 2, gold);

        // Legendary handle
        DrawRect(tex, 1, 1, 6, 6, purpleDark);
        DrawRect(tex, 2, 2, 4, 4, purple);
        tex.SetPixel(3, 3, magic);
        tex.SetPixel(4, 4, gold);

        // Magical tip
        tex.SetPixel(27, 27, purpleBright);
        tex.SetPixel(28, 28, magic);
        tex.SetPixel(29, 29, magic);

        // Magical fishing line
        DrawLine(tex, 29, 29, 31, 12, magic);

        FinalizeTexture(tex);
        return tex;
    }

    // Epic Rod - Fiery orange/red magical rod
    Texture2D CreateEpicRod()
    {
        Texture2D tex = CreateTexture();

        Color fire = new Color(1f, 0.5f, 0.1f);
        Color fireBright = new Color(1f, 0.8f, 0.3f);
        Color fireDark = new Color(0.8f, 0.2f, 0.05f);
        Color flame = new Color(1f, 1f, 0.5f);
        Color ember = new Color(1f, 0.3f, 0.1f);

        // Fire glow aura
        for (int x = 2; x < 30; x++)
        {
            for (int y = 2; y < 30; y++)
            {
                float dist = Mathf.Abs(x - y);
                if (dist < 5)
                {
                    Color glow = new Color(1f, 0.4f, 0.1f, 0.12f);
                    tex.SetPixel(x, y, glow);
                }
            }
        }

        // Rod shaft - fiery
        DrawLine(tex, 4, 4, 26, 26, fire);
        DrawLine(tex, 5, 4, 27, 26, fireBright);
        DrawLine(tex, 4, 5, 26, 27, fireDark);
        DrawLine(tex, 5, 5, 27, 27, fire);

        // Flame particles
        tex.SetPixel(7, 9, flame);
        tex.SetPixel(10, 11, ember);
        tex.SetPixel(13, 15, flame);
        tex.SetPixel(16, 17, ember);
        tex.SetPixel(19, 21, flame);
        tex.SetPixel(22, 23, ember);
        tex.SetPixel(25, 27, flame);

        // Fire bands
        DrawRect(tex, 6, 6, 3, 3, fireBright);
        DrawRect(tex, 12, 12, 3, 3, fireBright);
        DrawRect(tex, 18, 18, 3, 3, fireBright);
        DrawRect(tex, 24, 24, 2, 2, flame);

        // Epic handle with flames
        DrawRect(tex, 1, 1, 6, 6, fireDark);
        DrawRect(tex, 2, 2, 4, 4, fire);
        tex.SetPixel(3, 3, flame);
        tex.SetPixel(4, 4, flame);
        tex.SetPixel(2, 5, ember);
        tex.SetPixel(5, 2, ember);

        // Blazing tip
        tex.SetPixel(27, 27, fireBright);
        tex.SetPixel(28, 28, flame);
        tex.SetPixel(29, 29, flame);
        tex.SetPixel(28, 29, ember);
        tex.SetPixel(29, 28, ember);

        // Fire fishing line
        DrawLine(tex, 29, 29, 31, 10, flame);

        FinalizeTexture(tex);
        return tex;
    }
}
