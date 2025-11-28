using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates pixel art sprites for all fish types
/// Used in inventory display and catch popups
/// </summary>
public class FishSprites : MonoBehaviour
{
    public static FishSprites Instance { get; private set; }

    private Dictionary<string, Texture2D> fishTextures = new Dictionary<string, Texture2D>();
    private const int SIZE = 32;  // 32x32 pixel art

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GenerateAllFishSprites();
        }
        else Destroy(gameObject);
    }

    public Texture2D GetFishTexture(string fishId)
    {
        if (fishTextures.ContainsKey(fishId))
            return fishTextures[fishId];
        return fishTextures.ContainsKey("sardine") ? fishTextures["sardine"] : null;
    }

    void GenerateAllFishSprites()
    {
        // Common
        fishTextures["sardine"] = CreateSardine();
        fishTextures["anchovy"] = CreateAnchovy();
        fishTextures["minnow"] = CreateMinnow();
        fishTextures["cod"] = CreateCod();

        // Uncommon
        fishTextures["bass"] = CreateBass();
        fishTextures["salmon"] = CreateSalmon();
        fishTextures["baby_sea_turtle"] = CreateBabySeaTurtle();
        fishTextures["jellyfish"] = CreateJellyfish();

        // Rare
        fishTextures["tuna"] = CreateTuna();
        fishTextures["swordfish"] = CreateSwordfish();
        fishTextures["hammerhead"] = CreateHammerhead();
        fishTextures["ocean_sprinter_eel"] = CreateOceanSprinterEel();
        fishTextures["red_snapper"] = CreateRedSnapper();
        fishTextures["blue_marlin"] = CreateBlueMarlin();
        fishTextures["rainbow_trout"] = CreateRainbowTrout();
        fishTextures["sunshore_od"] = CreateSunshoreOd();
        fishTextures["icelandic_snubnose"] = CreateIcelandicSnubnose();

        // Epic
        fishTextures["shark"] = CreateShark();
        fishTextures["vampire_sealfish"] = CreateVampireSealfish();
        fishTextures["icelandic_sunscale"] = CreateIcelandicSunscale();
        fishTextures["sting_ray"] = CreateStingRay();
        fishTextures["rainbow_fish"] = CreateRainbowFish();
        fishTextures["hammerhead_shark"] = CreateHammerheadShark();
        fishTextures["whale_baby"] = CreateWhaleBaby();
        fishTextures["seahorse"] = CreateSeahorse();

        // Legendary
        fishTextures["whale"] = CreateWhale();
        fishTextures["dorgush_wrangler"] = CreateDorgushWrangler();
        fishTextures["danish_warblecock"] = CreateDanishWarblecock();
        fishTextures["golden_starfish"] = CreateGoldenStarfish();

        // Special
        fishTextures["petes_tackle_box"] = CreateTackleBox();
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

    // Helper to draw filled ellipse
    void DrawEllipse(Texture2D tex, int cx, int cy, int rx, int ry, Color col)
    {
        for (int x = cx - rx; x <= cx + rx; x++)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                float dx = (float)(x - cx) / rx;
                float dy = (float)(y - cy) / ry;
                if (dx * dx + dy * dy <= 1f && x >= 0 && x < SIZE && y >= 0 && y < SIZE)
                    tex.SetPixel(x, y, col);
            }
        }
    }

    // Helper to draw a line
    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            if (x0 >= 0 && x0 < SIZE && y0 >= 0 && y0 < SIZE)
                tex.SetPixel(x0, y0, col);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    // Helper to draw triangle (filled)
    void DrawTriangle(Texture2D tex, int x1, int y1, int x2, int y2, int x3, int y3, Color col)
    {
        int minX = Mathf.Min(x1, Mathf.Min(x2, x3));
        int maxX = Mathf.Max(x1, Mathf.Max(x2, x3));
        int minY = Mathf.Min(y1, Mathf.Min(y2, y3));
        int maxY = Mathf.Max(y1, Mathf.Max(y2, y3));

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (PointInTriangle(x, y, x1, y1, x2, y2, x3, y3) && x >= 0 && x < SIZE && y >= 0 && y < SIZE)
                    tex.SetPixel(x, y, col);
            }
        }
    }

    bool PointInTriangle(int px, int py, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        float d1 = Sign(px, py, x1, y1, x2, y2);
        float d2 = Sign(px, py, x2, y2, x3, y3);
        float d3 = Sign(px, py, x3, y3, x1, y1);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    float Sign(int px, int py, int x1, int y1, int x2, int y2)
    {
        return (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);
    }

    // ==================== COMMON FISH ====================

    Texture2D CreateSardine()
    {
        Texture2D tex = CreateTexture();
        Color silver = new Color(0.75f, 0.8f, 0.85f);
        Color darkBack = new Color(0.3f, 0.35f, 0.45f);
        Color belly = new Color(0.9f, 0.92f, 0.95f);
        Color eye = Color.black;

        // Body
        DrawEllipse(tex, 16, 16, 12, 5, silver);
        // Dark back
        DrawEllipse(tex, 16, 18, 10, 2, darkBack);
        // Light belly
        DrawEllipse(tex, 16, 14, 8, 2, belly);
        // Tail
        DrawTriangle(tex, 3, 16, 6, 20, 6, 12, silver);
        // Eye
        tex.SetPixel(25, 17, eye);
        tex.SetPixel(26, 17, eye);
        // Spots on back
        tex.SetPixel(12, 19, darkBack);
        tex.SetPixel(18, 19, darkBack);
        tex.SetPixel(22, 18, darkBack);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateAnchovy()
    {
        Texture2D tex = CreateTexture();
        Color silver = new Color(0.7f, 0.75f, 0.8f);
        Color pink = new Color(0.85f, 0.7f, 0.75f);
        Color dark = new Color(0.25f, 0.3f, 0.35f);

        // Slender body
        DrawEllipse(tex, 16, 16, 11, 3, silver);
        DrawEllipse(tex, 16, 17, 9, 2, pink);
        // Tail
        DrawTriangle(tex, 4, 16, 7, 19, 7, 13, silver);
        // Eye
        tex.SetPixel(25, 16, dark);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateMinnow()
    {
        Texture2D tex = CreateTexture();
        Color silver = new Color(0.8f, 0.82f, 0.85f);
        Color dark = new Color(0.4f, 0.42f, 0.45f);

        // Tiny body
        DrawEllipse(tex, 16, 16, 8, 3, silver);
        // Tail
        DrawTriangle(tex, 7, 16, 10, 18, 10, 14, silver);
        // Eye
        tex.SetPixel(22, 16, dark);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateCod()
    {
        Texture2D tex = CreateTexture();
        Color olive = new Color(0.55f, 0.5f, 0.35f);
        Color yellow = new Color(0.7f, 0.65f, 0.4f);
        Color dark = new Color(0.35f, 0.3f, 0.2f);
        Color white = new Color(0.9f, 0.88f, 0.8f);

        // Body
        DrawEllipse(tex, 16, 16, 11, 6, olive);
        DrawEllipse(tex, 16, 14, 9, 3, yellow);
        // White lateral line
        DrawLine(tex, 8, 16, 26, 16, white);
        // Tail
        DrawTriangle(tex, 3, 16, 7, 21, 7, 11, olive);
        // Dorsal fin
        DrawTriangle(tex, 12, 22, 20, 22, 16, 26, dark);
        // Barbel (chin whisker)
        DrawLine(tex, 26, 14, 28, 12, dark);
        // Eye
        tex.SetPixel(24, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    // ==================== UNCOMMON FISH ====================

    Texture2D CreateBass()
    {
        Texture2D tex = CreateTexture();
        Color green = new Color(0.35f, 0.5f, 0.3f);
        Color lightGreen = new Color(0.5f, 0.6f, 0.4f);
        Color stripe = new Color(0.25f, 0.35f, 0.2f);

        // Body
        DrawEllipse(tex, 16, 16, 10, 6, green);
        DrawEllipse(tex, 16, 14, 8, 3, lightGreen);
        // Stripes
        DrawLine(tex, 10, 18, 22, 18, stripe);
        DrawLine(tex, 11, 15, 21, 15, stripe);
        // Large mouth
        DrawLine(tex, 25, 15, 28, 17, Color.black);
        DrawLine(tex, 25, 17, 28, 17, Color.black);
        // Tail
        DrawTriangle(tex, 4, 16, 8, 21, 8, 11, green);
        // Eye
        tex.SetPixel(23, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateSalmon()
    {
        Texture2D tex = CreateTexture();
        Color silver = new Color(0.7f, 0.72f, 0.75f);
        Color pink = new Color(0.9f, 0.6f, 0.55f);
        Color dark = new Color(0.4f, 0.42f, 0.5f);

        // Body
        DrawEllipse(tex, 16, 16, 11, 5, silver);
        // Pink flesh showing
        DrawEllipse(tex, 16, 15, 7, 3, pink);
        // Tail
        DrawTriangle(tex, 3, 16, 7, 20, 7, 12, silver);
        // Dorsal fin
        DrawTriangle(tex, 14, 21, 18, 21, 16, 24, dark);
        // Eye
        tex.SetPixel(25, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateBabySeaTurtle()
    {
        Texture2D tex = CreateTexture();
        Color shell = new Color(0.25f, 0.4f, 0.3f);
        Color shellLight = new Color(0.35f, 0.5f, 0.4f);
        Color skin = new Color(0.3f, 0.45f, 0.35f);

        // Shell (oval)
        DrawEllipse(tex, 16, 16, 8, 6, shell);
        // Shell pattern
        DrawEllipse(tex, 16, 16, 5, 4, shellLight);
        // Head
        DrawEllipse(tex, 25, 16, 3, 3, skin);
        // Flippers
        DrawEllipse(tex, 12, 22, 4, 2, skin);
        DrawEllipse(tex, 20, 22, 4, 2, skin);
        DrawEllipse(tex, 12, 10, 4, 2, skin);
        DrawEllipse(tex, 20, 10, 4, 2, skin);
        // Eye
        tex.SetPixel(26, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateJellyfish()
    {
        Texture2D tex = CreateTexture();
        Color pink = new Color(0.9f, 0.6f, 0.7f);
        Color red = new Color(0.8f, 0.3f, 0.4f);
        Color white = new Color(0.95f, 0.9f, 0.95f);

        // Dome
        DrawEllipse(tex, 16, 20, 8, 6, pink);
        // Stripes on dome
        DrawLine(tex, 10, 20, 10, 24, red);
        DrawLine(tex, 13, 19, 13, 25, red);
        DrawLine(tex, 16, 19, 16, 25, red);
        DrawLine(tex, 19, 19, 19, 25, red);
        DrawLine(tex, 22, 20, 22, 24, red);
        // Tentacles
        for (int i = 0; i < 5; i++)
        {
            int x = 10 + i * 3;
            DrawLine(tex, x, 14, x, 6, white);
            DrawLine(tex, x, 6, x + 1, 4, white);
        }

        FinalizeTexture(tex);
        return tex;
    }

    // ==================== RARE FISH ====================

    Texture2D CreateTuna()
    {
        Texture2D tex = CreateTexture();
        Color blue = new Color(0.2f, 0.35f, 0.55f);
        Color silver = new Color(0.75f, 0.78f, 0.82f);
        Color yellow = new Color(0.85f, 0.75f, 0.3f);

        // Torpedo body
        DrawEllipse(tex, 16, 16, 12, 5, blue);
        DrawEllipse(tex, 16, 14, 10, 3, silver);
        // Tail (forked)
        DrawTriangle(tex, 2, 16, 5, 22, 6, 16, blue);
        DrawTriangle(tex, 2, 16, 5, 10, 6, 16, blue);
        // Yellow finlets
        for (int i = 0; i < 4; i++)
        {
            tex.SetPixel(6 + i * 2, 20, yellow);
            tex.SetPixel(6 + i * 2, 12, yellow);
        }
        // Dorsal fin
        DrawTriangle(tex, 18, 21, 22, 21, 20, 24, blue);
        // Eye
        tex.SetPixel(25, 16, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateSwordfish()
    {
        Texture2D tex = CreateTexture();
        Color gray = new Color(0.5f, 0.52f, 0.55f);
        Color light = new Color(0.7f, 0.72f, 0.75f);
        Color dark = new Color(0.3f, 0.32f, 0.35f);

        // Body
        DrawEllipse(tex, 14, 16, 8, 4, gray);
        DrawEllipse(tex, 14, 15, 6, 2, light);
        // Long bill/sword
        DrawLine(tex, 22, 16, 30, 16, dark);
        DrawLine(tex, 22, 17, 28, 17, dark);
        // Tall dorsal fin
        DrawTriangle(tex, 12, 20, 16, 20, 14, 28, dark);
        // Tail
        DrawTriangle(tex, 4, 16, 7, 20, 7, 12, gray);
        // Eye
        tex.SetPixel(20, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateHammerhead()
    {
        Texture2D tex = CreateTexture();
        Color tan = new Color(0.75f, 0.65f, 0.6f);
        Color pink = new Color(0.85f, 0.7f, 0.7f);

        // Body
        DrawEllipse(tex, 14, 16, 8, 4, tan);
        // Hammer head
        DrawEllipse(tex, 24, 16, 2, 6, tan);
        DrawEllipse(tex, 22, 16, 3, 3, tan);
        // Eyes on ends of hammer
        tex.SetPixel(24, 21, Color.black);
        tex.SetPixel(24, 11, Color.black);
        // Tail
        DrawTriangle(tex, 4, 16, 7, 20, 7, 12, tan);
        // Belly
        DrawEllipse(tex, 14, 14, 6, 2, pink);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateOceanSprinterEel()
    {
        Texture2D tex = CreateTexture();
        Color black = new Color(0.15f, 0.15f, 0.2f);
        Color white = new Color(0.9f, 0.92f, 0.95f);

        // Long snake body with stripes
        for (int i = 0; i < 24; i++)
        {
            int y = 16 + (int)(Mathf.Sin(i * 0.5f) * 3);
            Color col = (i / 3) % 2 == 0 ? black : white;
            DrawEllipse(tex, 4 + i, y, 2, 2, col);
        }
        // Eye
        tex.SetPixel(26, 16, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateRedSnapper()
    {
        Texture2D tex = CreateTexture();
        Color red = new Color(0.85f, 0.25f, 0.2f);
        Color pink = new Color(0.95f, 0.5f, 0.5f);
        Color darkRed = new Color(0.6f, 0.15f, 0.15f);

        // Round body
        DrawEllipse(tex, 16, 16, 9, 7, red);
        DrawEllipse(tex, 16, 14, 7, 4, pink);
        // Tail
        DrawTriangle(tex, 5, 16, 9, 21, 9, 11, red);
        // Dorsal fin
        DrawTriangle(tex, 12, 23, 20, 23, 16, 27, darkRed);
        // Scale pattern (dots)
        for (int x = 10; x < 22; x += 3)
            for (int y = 12; y < 20; y += 3)
                tex.SetPixel(x, y, darkRed);
        // Eye
        tex.SetPixel(22, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateBlueMarlin()
    {
        Texture2D tex = CreateTexture();
        Color blue = new Color(0.2f, 0.4f, 0.8f);
        Color silver = new Color(0.8f, 0.82f, 0.85f);
        Color darkBlue = new Color(0.1f, 0.2f, 0.5f);

        // Body
        DrawEllipse(tex, 14, 16, 9, 5, blue);
        DrawEllipse(tex, 14, 14, 7, 3, silver);
        // Long spear bill
        DrawLine(tex, 23, 16, 31, 16, darkBlue);
        DrawLine(tex, 23, 17, 29, 17, darkBlue);
        // Tall dorsal fin
        DrawTriangle(tex, 10, 21, 18, 21, 14, 29, blue);
        // Tail
        DrawTriangle(tex, 3, 16, 6, 21, 6, 11, blue);
        // Stripes
        DrawLine(tex, 8, 18, 20, 18, darkBlue);
        DrawLine(tex, 9, 15, 19, 15, darkBlue);
        // Eye
        tex.SetPixel(21, 16, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateRainbowTrout()
    {
        Texture2D tex = CreateTexture();
        Color olive = new Color(0.5f, 0.5f, 0.35f);
        Color pink = new Color(0.9f, 0.55f, 0.6f);
        Color cream = new Color(0.9f, 0.88f, 0.8f);
        Color spots = new Color(0.25f, 0.25f, 0.2f);

        // Body
        DrawEllipse(tex, 16, 16, 11, 5, olive);
        // Pink stripe
        DrawLine(tex, 6, 16, 26, 16, pink);
        DrawLine(tex, 7, 15, 25, 15, pink);
        // Light belly
        DrawEllipse(tex, 16, 13, 8, 2, cream);
        // Spots
        tex.SetPixel(10, 18, spots); tex.SetPixel(14, 19, spots);
        tex.SetPixel(18, 18, spots); tex.SetPixel(22, 17, spots);
        tex.SetPixel(12, 17, spots); tex.SetPixel(20, 19, spots);
        // Tail
        DrawTriangle(tex, 3, 16, 6, 20, 6, 12, olive);
        // Eye
        tex.SetPixel(25, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateSunshoreOd()
    {
        Texture2D tex = CreateTexture();
        Color blue = new Color(0.3f, 0.6f, 0.8f);
        Color green = new Color(0.4f, 0.75f, 0.5f);
        Color yellow = new Color(0.9f, 0.85f, 0.3f);

        // Body
        DrawEllipse(tex, 16, 15, 8, 6, blue);
        // Yellow stripe
        DrawEllipse(tex, 16, 16, 6, 3, yellow);
        // Green tint
        DrawEllipse(tex, 16, 18, 5, 2, green);
        // Tall dorsal fin
        DrawTriangle(tex, 12, 21, 20, 21, 16, 28, green);
        // Tail
        DrawTriangle(tex, 6, 15, 9, 19, 9, 11, blue);
        // Eye
        tex.SetPixel(22, 16, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateIcelandicSnubnose()
    {
        Texture2D tex = CreateTexture();
        Color brown = new Color(0.5f, 0.45f, 0.35f);
        Color olive = new Color(0.55f, 0.55f, 0.4f);
        Color white = new Color(0.9f, 0.88f, 0.85f);

        // Body
        DrawEllipse(tex, 16, 16, 10, 6, brown);
        DrawEllipse(tex, 16, 14, 8, 3, olive);
        // White lateral line
        DrawLine(tex, 8, 16, 24, 16, white);
        // Tail
        DrawTriangle(tex, 4, 16, 7, 20, 7, 12, brown);
        // Barbel
        DrawLine(tex, 25, 14, 27, 12, brown);
        // Fins
        DrawTriangle(tex, 14, 21, 18, 21, 16, 24, olive);
        // Eye
        tex.SetPixel(23, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    // ==================== EPIC FISH ====================

    Texture2D CreateShark()
    {
        Texture2D tex = CreateTexture();
        Color gray = new Color(0.45f, 0.48f, 0.52f);
        Color white = new Color(0.9f, 0.9f, 0.92f);
        Color dark = new Color(0.3f, 0.32f, 0.35f);

        // Sleek body
        DrawEllipse(tex, 16, 16, 11, 5, gray);
        // White underside
        DrawEllipse(tex, 16, 13, 9, 3, white);
        // Prominent dorsal fin
        DrawTriangle(tex, 14, 21, 18, 21, 16, 28, dark);
        // Tail
        DrawTriangle(tex, 3, 16, 6, 22, 7, 16, gray);
        DrawTriangle(tex, 3, 16, 6, 10, 7, 16, gray);
        // Pectoral fins
        DrawTriangle(tex, 12, 11, 18, 11, 15, 7, gray);
        // Gills
        DrawLine(tex, 20, 14, 20, 18, dark);
        DrawLine(tex, 22, 14, 22, 18, dark);
        // Eye
        tex.SetPixel(24, 17, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateVampireSealfish()
    {
        Texture2D tex = CreateTexture();
        Color gray = new Color(0.55f, 0.58f, 0.65f);
        Color blue = new Color(0.4f, 0.5f, 0.75f);
        Color silver = new Color(0.8f, 0.82f, 0.85f);

        // Body
        DrawEllipse(tex, 14, 14, 9, 4, gray);
        DrawEllipse(tex, 14, 13, 7, 2, silver);
        // HUGE sail dorsal fin with spots
        for (int x = 8; x < 22; x++)
        {
            int height = 28 - Mathf.Abs(x - 15);
            for (int y = 18; y < height; y++)
            {
                if (y < SIZE)
                    tex.SetPixel(x, y, blue);
            }
        }
        // Spots on sail
        tex.SetPixel(10, 22, Color.black); tex.SetPixel(14, 24, Color.black);
        tex.SetPixel(18, 23, Color.black); tex.SetPixel(12, 20, Color.black);
        // Long bill
        DrawLine(tex, 23, 14, 30, 14, gray);
        // Tail
        DrawTriangle(tex, 3, 14, 6, 18, 6, 10, gray);
        // Eye
        tex.SetPixel(21, 15, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateIcelandicSunscale()
    {
        Texture2D tex = CreateTexture();
        Color gold = new Color(0.9f, 0.75f, 0.25f);
        Color orange = new Color(0.95f, 0.6f, 0.2f);
        Color dark = new Color(0.6f, 0.4f, 0.1f);

        // Round body
        DrawEllipse(tex, 16, 16, 9, 8, gold);
        DrawEllipse(tex, 16, 16, 7, 6, orange);
        // Spiky fins all around
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2 / 12;
            int x1 = 16 + (int)(Mathf.Cos(angle) * 9);
            int y1 = 16 + (int)(Mathf.Sin(angle) * 8);
            int x2 = 16 + (int)(Mathf.Cos(angle) * 12);
            int y2 = 16 + (int)(Mathf.Sin(angle) * 11);
            DrawLine(tex, x1, y1, x2, y2, dark);
        }
        // Pointy snout with teeth
        DrawTriangle(tex, 25, 16, 30, 18, 30, 14, gold);
        tex.SetPixel(29, 16, Color.white);
        tex.SetPixel(28, 15, Color.white);
        // Eyes
        tex.SetPixel(22, 17, Color.black);
        tex.SetPixel(22, 18, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateStingRay()
    {
        Texture2D tex = CreateTexture();
        Color gray = new Color(0.55f, 0.55f, 0.58f);
        Color light = new Color(0.7f, 0.7f, 0.72f);

        // Diamond-shaped flat body
        DrawEllipse(tex, 16, 18, 10, 7, gray);
        DrawEllipse(tex, 16, 18, 7, 5, light);
        // Long tail
        DrawLine(tex, 16, 11, 16, 2, gray);
        DrawLine(tex, 15, 10, 15, 4, gray);
        // Eyes
        tex.SetPixel(13, 20, Color.black);
        tex.SetPixel(19, 20, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateRainbowFish()
    {
        Texture2D tex = CreateTexture();
        Color blue = new Color(0.5f, 0.7f, 0.9f);
        Color purple = new Color(0.6f, 0.5f, 0.8f);
        Color pink = new Color(0.9f, 0.5f, 0.6f);
        Color silver = new Color(0.85f, 0.85f, 0.9f);
        Color teal = new Color(0.4f, 0.8f, 0.7f);

        // Body
        DrawEllipse(tex, 16, 16, 9, 7, blue);
        // Shimmery rainbow scales
        tex.SetPixel(12, 18, silver); tex.SetPixel(14, 16, silver);
        tex.SetPixel(16, 18, silver); tex.SetPixel(18, 15, silver);
        tex.SetPixel(20, 17, silver); tex.SetPixel(13, 14, silver);
        tex.SetPixel(17, 13, silver); tex.SetPixel(19, 19, silver);
        // Color patches
        DrawEllipse(tex, 12, 17, 2, 2, purple);
        DrawEllipse(tex, 18, 14, 2, 2, pink);
        DrawEllipse(tex, 14, 14, 2, 2, teal);
        // Fins
        DrawTriangle(tex, 14, 23, 18, 23, 16, 27, purple);
        DrawTriangle(tex, 5, 16, 9, 20, 9, 12, pink);
        // Eye
        tex.SetPixel(22, 16, Color.blue);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateHammerheadShark()
    {
        Texture2D tex = CreateTexture();
        Color gray = new Color(0.5f, 0.52f, 0.55f);
        Color white = new Color(0.85f, 0.85f, 0.88f);

        // Body
        DrawEllipse(tex, 14, 16, 9, 5, gray);
        DrawEllipse(tex, 14, 14, 7, 3, white);
        // Wide T-shaped hammer head
        DrawEllipse(tex, 24, 16, 3, 8, gray);
        // Eyes at ends
        tex.SetPixel(25, 23, Color.black);
        tex.SetPixel(25, 9, Color.black);
        // Dorsal fin
        DrawTriangle(tex, 12, 21, 16, 21, 14, 26, gray);
        // Tail
        DrawTriangle(tex, 3, 16, 6, 21, 6, 11, gray);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateWhaleBaby()
    {
        Texture2D tex = CreateTexture();
        Color white = new Color(0.92f, 0.92f, 0.95f);
        Color cream = new Color(0.88f, 0.88f, 0.9f);
        Color pink = new Color(0.95f, 0.85f, 0.85f);

        // Round body (beluga)
        DrawEllipse(tex, 16, 16, 10, 8, white);
        // Rounded head bump
        DrawEllipse(tex, 22, 18, 5, 5, cream);
        // Pink tint
        DrawEllipse(tex, 20, 16, 3, 3, pink);
        // Flippers
        DrawEllipse(tex, 10, 12, 4, 2, white);
        // Tail flukes
        DrawTriangle(tex, 4, 16, 7, 20, 7, 16, white);
        DrawTriangle(tex, 4, 16, 7, 12, 7, 16, white);
        // Eye
        tex.SetPixel(24, 19, Color.black);
        // Smile
        DrawLine(tex, 26, 16, 28, 17, new Color(0.7f, 0.7f, 0.72f));

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateSeahorse()
    {
        Texture2D tex = CreateTexture();
        Color orange = new Color(0.9f, 0.65f, 0.25f);
        Color yellow = new Color(0.95f, 0.8f, 0.4f);
        Color dark = new Color(0.6f, 0.4f, 0.15f);

        // Head
        DrawEllipse(tex, 20, 24, 4, 3, orange);
        // Snout
        DrawLine(tex, 24, 24, 28, 24, orange);
        DrawLine(tex, 24, 25, 27, 25, orange);
        // Neck/body curve
        DrawEllipse(tex, 18, 20, 3, 3, orange);
        DrawEllipse(tex, 16, 16, 3, 3, orange);
        DrawEllipse(tex, 15, 12, 3, 3, orange);
        // Curled tail
        DrawEllipse(tex, 14, 8, 2, 2, orange);
        DrawEllipse(tex, 12, 6, 2, 2, orange);
        DrawEllipse(tex, 10, 6, 2, 2, orange);
        // Segments
        for (int y = 8; y < 22; y += 2)
            DrawLine(tex, 14, y, 18, y, dark);
        // Dorsal fin
        DrawTriangle(tex, 14, 14, 14, 18, 10, 16, yellow);
        // Eye
        tex.SetPixel(21, 25, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    // ==================== LEGENDARY FISH ====================

    Texture2D CreateWhale()
    {
        Texture2D tex = CreateTexture();
        Color blueGray = new Color(0.45f, 0.5f, 0.6f);
        Color light = new Color(0.65f, 0.7f, 0.75f);
        Color spots = new Color(0.55f, 0.6f, 0.68f);

        // Large body
        DrawEllipse(tex, 16, 16, 12, 7, blueGray);
        // Lighter underside
        DrawEllipse(tex, 16, 13, 10, 4, light);
        // White spots
        tex.SetPixel(10, 18, spots); tex.SetPixel(14, 19, spots);
        tex.SetPixel(18, 17, spots); tex.SetPixel(22, 18, spots);
        tex.SetPixel(12, 16, spots); tex.SetPixel(20, 15, spots);
        // Tail flukes
        DrawTriangle(tex, 2, 16, 5, 22, 6, 16, blueGray);
        DrawTriangle(tex, 2, 16, 5, 10, 6, 16, blueGray);
        // Flipper
        DrawEllipse(tex, 14, 10, 5, 2, blueGray);
        // Grooves on belly
        DrawLine(tex, 20, 12, 26, 12, light);
        DrawLine(tex, 21, 10, 25, 10, light);
        // Eye
        tex.SetPixel(25, 16, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateDorgushWrangler()
    {
        Texture2D tex = CreateTexture();
        Color orange = new Color(0.95f, 0.55f, 0.2f);
        Color gold = new Color(1f, 0.8f, 0.3f);

        // Round body
        DrawEllipse(tex, 16, 16, 8, 7, orange);
        // HUGE bubble eyes
        DrawEllipse(tex, 22, 20, 4, 4, gold);
        DrawEllipse(tex, 22, 12, 4, 4, gold);
        // Eye pupils
        tex.SetPixel(24, 20, Color.black);
        tex.SetPixel(24, 12, Color.black);
        // Flowing tail
        DrawTriangle(tex, 4, 16, 9, 22, 9, 10, orange);
        DrawEllipse(tex, 6, 16, 3, 5, orange);
        // Fins
        DrawTriangle(tex, 14, 23, 18, 23, 16, 26, orange);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateDanishWarblecock()
    {
        Texture2D tex = CreateTexture();
        Color brown = new Color(0.5f, 0.45f, 0.3f);
        Color olive = new Color(0.55f, 0.5f, 0.35f);
        Color finBlue = new Color(0.6f, 0.65f, 0.75f);
        Color spots = new Color(0.35f, 0.35f, 0.4f);

        // Body
        DrawEllipse(tex, 16, 14, 10, 5, brown);
        DrawEllipse(tex, 16, 13, 8, 3, olive);
        // Tall spotted dorsal fin
        for (int x = 10; x < 22; x++)
        {
            int height = 26 - Mathf.Abs(x - 16) / 2;
            for (int y = 19; y < height; y++)
                if (y < SIZE) tex.SetPixel(x, y, finBlue);
        }
        // Spots on fin
        tex.SetPixel(12, 22, spots); tex.SetPixel(16, 24, spots);
        tex.SetPixel(14, 21, spots); tex.SetPixel(18, 22, spots);
        tex.SetPixel(20, 21, spots);
        // Tail
        DrawTriangle(tex, 4, 14, 8, 18, 8, 10, brown);
        // Big eye
        tex.SetPixel(23, 15, Color.black);
        tex.SetPixel(24, 15, Color.black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateGoldenStarfish()
    {
        Texture2D tex = CreateTexture();
        Color gold = new Color(0.95f, 0.8f, 0.35f);
        Color darkGold = new Color(0.75f, 0.6f, 0.2f);
        Color shine = new Color(1f, 0.95f, 0.7f);

        // 5-pointed star
        int cx = 16, cy = 16;
        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2 / 5 - Mathf.PI / 2;
            int x1 = cx + (int)(Mathf.Cos(angle) * 10);
            int y1 = cy + (int)(Mathf.Sin(angle) * 10);

            float innerAngle = angle + Mathf.PI / 5;
            int x2 = cx + (int)(Mathf.Cos(innerAngle) * 4);
            int y2 = cy + (int)(Mathf.Sin(innerAngle) * 4);

            float nextAngle = (i + 1) * Mathf.PI * 2 / 5 - Mathf.PI / 2;
            int x3 = cx + (int)(Mathf.Cos(nextAngle) * 10);
            int y3 = cy + (int)(Mathf.Sin(nextAngle) * 10);

            DrawTriangle(tex, cx, cy, x1, y1, x2, y2, gold);
            DrawTriangle(tex, cx, cy, x2, y2, x3, y3, gold);
        }
        // Center
        DrawEllipse(tex, cx, cy, 4, 4, darkGold);
        // Shine
        tex.SetPixel(14, 18, shine);
        tex.SetPixel(15, 19, shine);
        // Texture dots
        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2 / 5 - Mathf.PI / 2;
            int x = cx + (int)(Mathf.Cos(angle) * 6);
            int y = cy + (int)(Mathf.Sin(angle) * 6);
            tex.SetPixel(x, y, darkGold);
        }

        FinalizeTexture(tex);
        return tex;
    }

    // ==================== SPECIAL ====================

    Texture2D CreateTackleBox()
    {
        Texture2D tex = CreateTexture();
        Color green = new Color(0.2f, 0.4f, 0.35f);
        Color cream = new Color(0.9f, 0.88f, 0.8f);
        Color gold = new Color(0.8f, 0.7f, 0.3f);
        Color handle = new Color(0.6f, 0.6f, 0.62f);

        // Main box
        for (int x = 6; x < 26; x++)
            for (int y = 8; y < 20; y++)
                tex.SetPixel(x, y, green);
        // Cream bottom tray
        for (int x = 7; x < 25; x++)
            for (int y = 8; y < 12; y++)
                tex.SetPixel(x, y, cream);
        // Handle
        for (int x = 12; x < 20; x++)
        {
            tex.SetPixel(x, 22, handle);
            tex.SetPixel(x, 23, handle);
        }
        tex.SetPixel(12, 20, handle); tex.SetPixel(12, 21, handle);
        tex.SetPixel(19, 20, handle); tex.SetPixel(19, 21, handle);
        // Latch
        tex.SetPixel(15, 14, gold);
        tex.SetPixel(16, 14, gold);
        tex.SetPixel(15, 15, gold);
        tex.SetPixel(16, 15, gold);

        FinalizeTexture(tex);
        return tex;
    }
}
