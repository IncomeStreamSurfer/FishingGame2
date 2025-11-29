using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Pixel art sprites for all clothing items in the shop
/// 24x24 pixel sprites for each item
/// </summary>
public class ClothingSprites : MonoBehaviour
{
    public static ClothingSprites Instance { get; private set; }

    private Dictionary<string, Texture2D> clothingTextures = new Dictionary<string, Texture2D>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CreateAllSprites();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CreateAllSprites()
    {
        // Head items
        clothingTextures["Straw Hat"] = CreateStrawHat();
        clothingTextures["Baseball Cap"] = CreateBaseballCap();
        clothingTextures["Fancy Top Hat"] = CreateTopHat();

        // Top items
        clothingTextures["Coconut Bra"] = CreateCoconutBra();
        clothingTextures["Red T-Shirt"] = CreateTShirt(new Color(0.85f, 0.15f, 0.1f));
        clothingTextures["Blue Shirt"] = CreateTShirt(new Color(0.15f, 0.35f, 0.65f));
        clothingTextures["Lumberjack Shirt"] = CreateLumberjackShirt();
        clothingTextures["Fancy Tuxedo"] = CreateTuxedo();

        // Leg items
        clothingTextures["Red Pants"] = CreatePants(new Color(0.8f, 0.15f, 0.1f));
        clothingTextures["Green Pants"] = CreatePants(new Color(0.2f, 0.5f, 0.2f));
        clothingTextures["Black Pants"] = CreatePants(new Color(0.12f, 0.12f, 0.12f));
        clothingTextures["Blue Jeans"] = CreateJeans();

        // Accessories
        clothingTextures["Pimp Cane"] = CreatePimpCane();
        clothingTextures["Shoulder Parrot"] = CreateParrot();

        // Consumables
        clothingTextures["Lunch Box"] = CreateLunchBox();

        // === ICE REALM CLOTHING ===
        // Head items
        clothingTextures["Bear Skin Hat"] = CreateBearSkinHat();
        clothingTextures["Pirate Hat"] = CreatePirateHat();
        clothingTextures["Beret"] = CreateBeret();

        // Top items
        clothingTextures["Husband Beater"] = CreateHusbandBeater();
        clothingTextures["Pink Fur Coat"] = CreatePinkFurCoat();

        // Leg items
        clothingTextures["Black Leather Hotpants"] = CreateLeatherHotpants();
        clothingTextures["Whale Bladder Pants"] = CreateWhaleBladderPants();
        clothingTextures["Pink Leather Pants"] = CreatePinkLeatherPants();
    }

    public Texture2D GetClothingTexture(string name)
    {
        if (clothingTextures.TryGetValue(name, out Texture2D tex))
            return tex;
        return null;
    }

    Texture2D CreateTexture()
    {
        Texture2D tex = new Texture2D(24, 24);
        tex.filterMode = FilterMode.Point;
        Color[] clear = new Color[24 * 24];
        for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
        tex.SetPixels(clear);
        return tex;
    }

    void SetPixel(Texture2D tex, int x, int y, Color color)
    {
        if (x >= 0 && x < 24 && y >= 0 && y < 24)
            tex.SetPixel(x, y, color);
    }

    void FillRect(Texture2D tex, int x, int y, int w, int h, Color color)
    {
        for (int py = y; py < y + h; py++)
            for (int px = x; px < x + w; px++)
                SetPixel(tex, px, py, color);
    }

    // === HEAD ITEMS ===

    Texture2D CreateStrawHat()
    {
        Texture2D tex = CreateTexture();
        Color straw = new Color(0.9f, 0.8f, 0.5f);
        Color strawDark = new Color(0.75f, 0.65f, 0.4f);
        Color band = new Color(0.4f, 0.25f, 0.15f);

        // Brim
        FillRect(tex, 2, 6, 20, 3, straw);
        FillRect(tex, 4, 5, 16, 1, strawDark);

        // Crown
        FillRect(tex, 6, 9, 12, 8, straw);
        FillRect(tex, 7, 17, 10, 2, straw);

        // Hat band
        FillRect(tex, 6, 9, 12, 2, band);

        // Weave pattern
        for (int y = 10; y < 17; y += 2)
            for (int x = 7; x < 17; x += 2)
                SetPixel(tex, x, y, strawDark);

        tex.Apply();
        return tex;
    }

    Texture2D CreateBaseballCap()
    {
        Texture2D tex = CreateTexture();
        Color red = new Color(0.85f, 0.15f, 0.1f);
        Color redDark = new Color(0.65f, 0.1f, 0.08f);
        Color white = Color.white;

        // Brim
        FillRect(tex, 2, 6, 14, 3, redDark);
        FillRect(tex, 3, 7, 12, 2, redDark);

        // Crown
        FillRect(tex, 6, 9, 12, 10, red);
        FillRect(tex, 8, 19, 8, 2, red);

        // Button on top
        SetPixel(tex, 11, 20, redDark);
        SetPixel(tex, 12, 20, redDark);

        // White logo/detail
        FillRect(tex, 9, 12, 6, 4, white);

        tex.Apply();
        return tex;
    }

    Texture2D CreateTopHat()
    {
        Texture2D tex = CreateTexture();
        Color black = new Color(0.1f, 0.1f, 0.1f);
        Color darkGrey = new Color(0.2f, 0.2f, 0.2f);
        Color gold = new Color(0.85f, 0.7f, 0.2f);

        // Brim
        FillRect(tex, 2, 4, 20, 3, black);
        FillRect(tex, 3, 5, 18, 2, darkGrey);

        // Tall crown
        FillRect(tex, 6, 7, 12, 14, black);
        FillRect(tex, 7, 8, 10, 12, darkGrey);

        // Gold band
        FillRect(tex, 6, 8, 12, 2, gold);

        // Top
        FillRect(tex, 7, 20, 10, 2, black);

        tex.Apply();
        return tex;
    }

    // === TOP ITEMS ===

    Texture2D CreateCoconutBra()
    {
        Texture2D tex = CreateTexture();
        Color coconut = new Color(0.55f, 0.35f, 0.2f);
        Color coconutDark = new Color(0.4f, 0.25f, 0.15f);
        Color string_ = new Color(0.6f, 0.5f, 0.3f);

        // Left coconut
        FillRect(tex, 3, 8, 7, 6, coconut);
        FillRect(tex, 4, 9, 5, 4, coconutDark);
        SetPixel(tex, 5, 10, coconut);

        // Right coconut
        FillRect(tex, 14, 8, 7, 6, coconut);
        FillRect(tex, 15, 9, 5, 4, coconutDark);
        SetPixel(tex, 16, 10, coconut);

        // String connections
        FillRect(tex, 10, 10, 4, 1, string_);
        FillRect(tex, 6, 14, 3, 1, string_);
        FillRect(tex, 15, 14, 3, 1, string_);

        // Neck string
        SetPixel(tex, 8, 15, string_);
        SetPixel(tex, 9, 16, string_);
        SetPixel(tex, 10, 17, string_);
        SetPixel(tex, 11, 18, string_);
        SetPixel(tex, 12, 17, string_);
        SetPixel(tex, 13, 16, string_);
        SetPixel(tex, 14, 15, string_);

        tex.Apply();
        return tex;
    }

    Texture2D CreateTShirt(Color baseColor)
    {
        Texture2D tex = CreateTexture();
        Color dark = new Color(baseColor.r * 0.7f, baseColor.g * 0.7f, baseColor.b * 0.7f);

        // Body
        FillRect(tex, 6, 2, 12, 14, baseColor);

        // Sleeves
        FillRect(tex, 2, 10, 4, 6, baseColor);
        FillRect(tex, 18, 10, 4, 6, baseColor);

        // Collar
        FillRect(tex, 9, 16, 6, 2, dark);
        FillRect(tex, 10, 15, 4, 1, dark);

        // Shading
        FillRect(tex, 6, 2, 2, 12, dark);
        FillRect(tex, 2, 10, 1, 6, dark);

        tex.Apply();
        return tex;
    }

    Texture2D CreateLumberjackShirt()
    {
        Texture2D tex = CreateTexture();
        Color red = new Color(0.8f, 0.1f, 0.1f);
        Color black = new Color(0.15f, 0.15f, 0.15f);

        // Body base
        FillRect(tex, 6, 2, 12, 14, red);

        // Sleeves
        FillRect(tex, 2, 10, 4, 6, red);
        FillRect(tex, 18, 10, 4, 6, red);

        // Checkered pattern
        for (int y = 2; y < 16; y += 3)
        {
            for (int x = 6; x < 18; x += 3)
            {
                if ((x + y) % 6 < 3)
                    FillRect(tex, x, y, 2, 2, black);
            }
        }

        // Collar
        FillRect(tex, 9, 16, 6, 2, black);

        tex.Apply();
        return tex;
    }

    Texture2D CreateTuxedo()
    {
        Texture2D tex = CreateTexture();
        Color black = new Color(0.1f, 0.1f, 0.1f);
        Color white = Color.white;
        Color red = new Color(0.7f, 0.1f, 0.1f);

        // Body (jacket)
        FillRect(tex, 5, 2, 14, 14, black);

        // Sleeves
        FillRect(tex, 1, 10, 4, 6, black);
        FillRect(tex, 19, 10, 4, 6, black);

        // White shirt front
        FillRect(tex, 10, 2, 4, 12, white);

        // Lapels
        FillRect(tex, 7, 10, 3, 6, black);
        FillRect(tex, 14, 10, 3, 6, black);

        // Bow tie
        FillRect(tex, 10, 14, 4, 2, red);
        SetPixel(tex, 9, 14, red);
        SetPixel(tex, 14, 14, red);

        // Buttons
        SetPixel(tex, 11, 8, white);
        SetPixel(tex, 11, 5, white);

        tex.Apply();
        return tex;
    }

    // === LEG ITEMS ===

    Texture2D CreatePants(Color baseColor)
    {
        Texture2D tex = CreateTexture();
        Color dark = new Color(baseColor.r * 0.7f, baseColor.g * 0.7f, baseColor.b * 0.7f);

        // Waistband
        FillRect(tex, 5, 18, 14, 3, dark);

        // Left leg
        FillRect(tex, 5, 2, 6, 16, baseColor);
        FillRect(tex, 5, 2, 2, 14, dark);

        // Right leg
        FillRect(tex, 13, 2, 6, 16, baseColor);
        FillRect(tex, 13, 2, 2, 14, dark);

        // Crotch
        FillRect(tex, 11, 12, 2, 6, baseColor);

        tex.Apply();
        return tex;
    }

    Texture2D CreateJeans()
    {
        Texture2D tex = CreateTexture();
        Color blue = new Color(0.2f, 0.35f, 0.6f);
        Color blueDark = new Color(0.15f, 0.25f, 0.45f);
        Color gold = new Color(0.8f, 0.6f, 0.2f);

        // Waistband
        FillRect(tex, 5, 18, 14, 3, blueDark);

        // Left leg
        FillRect(tex, 5, 2, 6, 16, blue);
        FillRect(tex, 5, 2, 2, 14, blueDark);

        // Right leg
        FillRect(tex, 13, 2, 6, 16, blue);
        FillRect(tex, 13, 2, 2, 14, blueDark);

        // Crotch
        FillRect(tex, 11, 12, 2, 6, blue);

        // Gold stitching details
        for (int y = 4; y < 16; y += 3)
        {
            SetPixel(tex, 7, y, gold);
            SetPixel(tex, 16, y, gold);
        }

        // Button
        SetPixel(tex, 11, 19, gold);
        SetPixel(tex, 12, 19, gold);

        tex.Apply();
        return tex;
    }

    // === ACCESSORIES ===

    Texture2D CreatePimpCane()
    {
        Texture2D tex = CreateTexture();
        Color gold = new Color(0.85f, 0.7f, 0.2f);
        Color goldDark = new Color(0.65f, 0.5f, 0.15f);
        Color black = new Color(0.15f, 0.15f, 0.15f);

        // Handle (gold ball)
        FillRect(tex, 9, 18, 6, 4, gold);
        FillRect(tex, 10, 19, 4, 2, goldDark);

        // Shaft
        FillRect(tex, 11, 2, 2, 16, black);
        FillRect(tex, 11, 2, 1, 16, goldDark);

        // Gold tip
        FillRect(tex, 10, 1, 4, 2, gold);

        tex.Apply();
        return tex;
    }

    Texture2D CreateParrot()
    {
        Texture2D tex = CreateTexture();
        Color green = new Color(0.2f, 0.8f, 0.3f);
        Color greenDark = new Color(0.15f, 0.6f, 0.2f);
        Color red = new Color(0.9f, 0.2f, 0.1f);
        Color yellow = new Color(0.95f, 0.85f, 0.2f);
        Color orange = new Color(0.95f, 0.5f, 0.1f);
        Color black = new Color(0.1f, 0.1f, 0.1f);

        // Body
        FillRect(tex, 8, 8, 8, 10, green);
        FillRect(tex, 9, 9, 6, 8, greenDark);

        // Head
        FillRect(tex, 10, 18, 6, 4, green);
        FillRect(tex, 11, 19, 4, 2, red);

        // Beak
        FillRect(tex, 16, 18, 3, 3, orange);
        SetPixel(tex, 18, 19, yellow);

        // Eye
        SetPixel(tex, 14, 20, black);
        SetPixel(tex, 15, 20, Color.white);

        // Wings
        FillRect(tex, 5, 10, 3, 6, greenDark);
        FillRect(tex, 16, 10, 3, 6, greenDark);
        SetPixel(tex, 6, 12, red);
        SetPixel(tex, 17, 12, red);

        // Tail
        FillRect(tex, 10, 2, 4, 6, green);
        FillRect(tex, 9, 2, 2, 4, red);
        FillRect(tex, 13, 2, 2, 4, yellow);

        tex.Apply();
        return tex;
    }

    Texture2D CreateLunchBox()
    {
        Texture2D tex = CreateTexture();
        Color metal = new Color(0.7f, 0.4f, 0.2f);
        Color metalDark = new Color(0.5f, 0.3f, 0.15f);
        Color handle = new Color(0.3f, 0.3f, 0.3f);
        Color latch = new Color(0.8f, 0.8f, 0.8f);

        // Main box
        FillRect(tex, 3, 4, 18, 12, metal);
        FillRect(tex, 4, 5, 16, 10, metalDark);

        // Lid line
        FillRect(tex, 3, 14, 18, 1, metalDark);

        // Handle
        FillRect(tex, 9, 16, 6, 2, handle);
        FillRect(tex, 8, 18, 2, 3, handle);
        FillRect(tex, 14, 18, 2, 3, handle);
        FillRect(tex, 8, 20, 8, 1, handle);

        // Latch
        FillRect(tex, 10, 9, 4, 3, latch);
        SetPixel(tex, 11, 10, metalDark);

        // Highlight
        FillRect(tex, 5, 12, 14, 1, metal);

        tex.Apply();
        return tex;
    }

    // === ICE REALM CLOTHING SPRITES ===

    Texture2D CreateBearSkinHat()
    {
        // White fluffy polar bear hat with 2 small ears
        Texture2D tex = CreateTexture();
        Color white = new Color(0.95f, 0.95f, 0.98f);
        Color lightGray = new Color(0.85f, 0.87f, 0.9f);
        Color darkGray = new Color(0.7f, 0.72f, 0.75f);
        Color nose = new Color(0.15f, 0.15f, 0.15f);
        Color eye = Color.black;

        // Main fluffy body of hat
        FillRect(tex, 4, 3, 16, 12, white);
        FillRect(tex, 5, 2, 14, 2, white);
        FillRect(tex, 6, 1, 12, 1, lightGray);

        // Fluffy texture
        SetPixel(tex, 5, 10, lightGray);
        SetPixel(tex, 8, 8, lightGray);
        SetPixel(tex, 12, 11, lightGray);
        SetPixel(tex, 15, 7, lightGray);
        SetPixel(tex, 18, 9, lightGray);
        SetPixel(tex, 7, 5, lightGray);
        SetPixel(tex, 14, 4, lightGray);

        // Left ear
        FillRect(tex, 4, 15, 4, 4, white);
        FillRect(tex, 5, 16, 2, 2, lightGray);

        // Right ear
        FillRect(tex, 16, 15, 4, 4, white);
        FillRect(tex, 17, 16, 2, 2, lightGray);

        // Bear face details
        // Eyes
        SetPixel(tex, 8, 8, eye);
        SetPixel(tex, 9, 8, eye);
        SetPixel(tex, 14, 8, eye);
        SetPixel(tex, 15, 8, eye);

        // Nose
        FillRect(tex, 10, 5, 4, 2, nose);
        SetPixel(tex, 11, 4, nose);
        SetPixel(tex, 12, 4, nose);

        tex.Apply();
        return tex;
    }

    Texture2D CreatePirateHat()
    {
        // Black tricorn pirate hat with skull
        Texture2D tex = CreateTexture();
        Color black = new Color(0.08f, 0.08f, 0.1f);
        Color darkGray = new Color(0.15f, 0.15f, 0.18f);
        Color gold = new Color(0.85f, 0.7f, 0.3f);
        Color white = new Color(0.9f, 0.9f, 0.9f);
        Color bone = new Color(0.85f, 0.82f, 0.75f);

        // Main hat body (tricorn shape)
        FillRect(tex, 2, 5, 20, 8, black);
        FillRect(tex, 4, 4, 16, 2, black);
        FillRect(tex, 6, 3, 12, 1, black);

        // Brim folds
        FillRect(tex, 1, 5, 3, 6, darkGray);
        FillRect(tex, 20, 5, 3, 6, darkGray);
        FillRect(tex, 8, 13, 8, 3, black);

        // Gold trim
        FillRect(tex, 2, 12, 20, 1, gold);

        // Skull and crossbones
        FillRect(tex, 9, 7, 6, 4, bone);
        SetPixel(tex, 10, 9, black); // Left eye
        SetPixel(tex, 13, 9, black); // Right eye
        SetPixel(tex, 11, 7, black); // Nose
        SetPixel(tex, 12, 7, black);

        // Crossbones
        SetPixel(tex, 7, 6, bone);
        SetPixel(tex, 8, 7, bone);
        SetPixel(tex, 15, 7, bone);
        SetPixel(tex, 16, 6, bone);
        SetPixel(tex, 7, 10, bone);
        SetPixel(tex, 8, 9, bone);
        SetPixel(tex, 15, 9, bone);
        SetPixel(tex, 16, 10, bone);

        tex.Apply();
        return tex;
    }

    Texture2D CreateBeret()
    {
        // French beret - red/burgundy
        Texture2D tex = CreateTexture();
        Color red = new Color(0.6f, 0.1f, 0.15f);
        Color darkRed = new Color(0.45f, 0.08f, 0.1f);
        Color black = new Color(0.1f, 0.1f, 0.1f);

        // Main beret shape (flat and round)
        FillRect(tex, 4, 6, 16, 6, red);
        FillRect(tex, 6, 5, 12, 2, red);
        FillRect(tex, 8, 4, 8, 1, red);
        FillRect(tex, 3, 8, 18, 4, red);
        FillRect(tex, 2, 9, 20, 2, red);

        // Shading
        FillRect(tex, 4, 6, 8, 3, darkRed);
        SetPixel(tex, 5, 9, darkRed);
        SetPixel(tex, 6, 10, darkRed);

        // Band
        FillRect(tex, 3, 5, 18, 1, black);

        // Little stem on top
        SetPixel(tex, 12, 12, black);
        SetPixel(tex, 12, 13, black);

        tex.Apply();
        return tex;
    }

    Texture2D CreateHusbandBeater()
    {
        // White sleeveless vest/tank top
        Texture2D tex = CreateTexture();
        Color white = new Color(0.95f, 0.95f, 0.95f);
        Color offWhite = new Color(0.88f, 0.88f, 0.88f);
        Color shadow = new Color(0.8f, 0.8f, 0.8f);

        // Main body
        FillRect(tex, 7, 2, 10, 14, white);
        FillRect(tex, 6, 4, 12, 10, white);

        // Shoulder straps
        FillRect(tex, 7, 14, 3, 4, white);
        FillRect(tex, 14, 14, 3, 4, white);

        // Neckline
        FillRect(tex, 9, 15, 6, 3, Color.clear);
        SetPixel(tex, 10, 14, Color.clear);
        SetPixel(tex, 13, 14, Color.clear);

        // Ribbed texture
        for (int y = 3; y < 14; y += 2)
        {
            FillRect(tex, 7, y, 10, 1, offWhite);
        }

        // Shadows
        FillRect(tex, 6, 4, 1, 10, shadow);
        FillRect(tex, 17, 4, 1, 10, shadow);

        tex.Apply();
        return tex;
    }

    Texture2D CreatePinkFurCoat()
    {
        // Luxurious pink fur coat with fluffy texture
        Texture2D tex = CreateTexture();
        Color pink = new Color(1f, 0.6f, 0.7f);
        Color lightPink = new Color(1f, 0.75f, 0.82f);
        Color darkPink = new Color(0.85f, 0.45f, 0.55f);
        Color white = new Color(1f, 0.95f, 0.95f);

        // Main coat body
        FillRect(tex, 3, 1, 18, 16, pink);

        // Fluffy collar
        FillRect(tex, 5, 15, 14, 4, lightPink);
        FillRect(tex, 4, 16, 16, 3, lightPink);

        // Sleeves
        FillRect(tex, 1, 6, 4, 8, pink);
        FillRect(tex, 19, 6, 4, 8, pink);

        // Fur texture (random light spots for fluffiness)
        SetPixel(tex, 5, 12, lightPink);
        SetPixel(tex, 8, 8, lightPink);
        SetPixel(tex, 12, 14, lightPink);
        SetPixel(tex, 15, 10, lightPink);
        SetPixel(tex, 18, 6, lightPink);
        SetPixel(tex, 6, 4, lightPink);
        SetPixel(tex, 14, 3, lightPink);
        SetPixel(tex, 10, 11, lightPink);
        SetPixel(tex, 17, 13, lightPink);

        // Fluffy hairs sticking out (white tips)
        SetPixel(tex, 4, 19, white);
        SetPixel(tex, 7, 18, white);
        SetPixel(tex, 10, 19, white);
        SetPixel(tex, 14, 18, white);
        SetPixel(tex, 17, 19, white);
        SetPixel(tex, 2, 13, white);
        SetPixel(tex, 21, 12, white);
        SetPixel(tex, 1, 8, white);
        SetPixel(tex, 22, 9, white);

        // Center seam
        FillRect(tex, 11, 1, 2, 14, darkPink);

        // Buttons
        SetPixel(tex, 12, 4, white);
        SetPixel(tex, 12, 7, white);
        SetPixel(tex, 12, 10, white);

        tex.Apply();
        return tex;
    }

    Texture2D CreateLeatherHotpants()
    {
        // Shiny black leather hotpants
        Texture2D tex = CreateTexture();
        Color black = new Color(0.08f, 0.08f, 0.08f);
        Color darkGray = new Color(0.15f, 0.15f, 0.15f);
        Color shine = new Color(0.35f, 0.35f, 0.38f);
        Color silver = new Color(0.7f, 0.7f, 0.75f);

        // Main shorts body
        FillRect(tex, 4, 8, 16, 10, black);
        FillRect(tex, 6, 7, 12, 2, black);

        // Left leg
        FillRect(tex, 4, 2, 7, 7, black);

        // Right leg
        FillRect(tex, 13, 2, 7, 7, black);

        // Shiny highlights
        FillRect(tex, 6, 14, 3, 2, shine);
        FillRect(tex, 15, 13, 2, 3, shine);
        SetPixel(tex, 5, 5, shine);
        SetPixel(tex, 17, 4, shine);

        // Waistband
        FillRect(tex, 4, 16, 16, 2, darkGray);

        // Belt buckle
        FillRect(tex, 10, 16, 4, 2, silver);

        // Seams
        FillRect(tex, 11, 8, 2, 8, darkGray);

        tex.Apply();
        return tex;
    }

    Texture2D CreateWhaleBladderPants()
    {
        // Fleshy pink traditional pants
        Texture2D tex = CreateTexture();
        Color flesh = new Color(0.9f, 0.7f, 0.7f);
        Color darkFlesh = new Color(0.75f, 0.55f, 0.55f);
        Color vein = new Color(0.7f, 0.5f, 0.6f);

        // Main pants body
        FillRect(tex, 4, 8, 16, 10, flesh);
        FillRect(tex, 6, 7, 12, 2, flesh);

        // Left leg
        FillRect(tex, 4, 1, 7, 8, flesh);

        // Right leg
        FillRect(tex, 13, 1, 7, 8, flesh);

        // Organic texture (veins/wrinkles)
        SetPixel(tex, 6, 12, darkFlesh);
        SetPixel(tex, 7, 11, darkFlesh);
        SetPixel(tex, 8, 13, darkFlesh);
        SetPixel(tex, 15, 14, darkFlesh);
        SetPixel(tex, 16, 12, darkFlesh);
        SetPixel(tex, 5, 5, vein);
        SetPixel(tex, 6, 4, vein);
        SetPixel(tex, 17, 6, vein);
        SetPixel(tex, 18, 5, vein);

        // Waistband (leather tie)
        FillRect(tex, 4, 16, 16, 2, darkFlesh);
        SetPixel(tex, 11, 17, vein);
        SetPixel(tex, 12, 18, vein);
        SetPixel(tex, 13, 17, vein);

        tex.Apply();
        return tex;
    }

    Texture2D CreatePinkLeatherPants()
    {
        // Bright neon pink leather pants
        Texture2D tex = CreateTexture();
        Color pink = new Color(1f, 0.3f, 0.6f);
        Color lightPink = new Color(1f, 0.5f, 0.7f);
        Color darkPink = new Color(0.8f, 0.2f, 0.45f);
        Color shine = new Color(1f, 0.7f, 0.85f);
        Color silver = new Color(0.8f, 0.8f, 0.85f);

        // Main pants body
        FillRect(tex, 4, 8, 16, 10, pink);
        FillRect(tex, 6, 7, 12, 2, pink);

        // Left leg
        FillRect(tex, 4, 1, 7, 8, pink);

        // Right leg
        FillRect(tex, 13, 1, 7, 8, pink);

        // Shiny highlights
        FillRect(tex, 6, 12, 2, 4, shine);
        FillRect(tex, 16, 11, 2, 3, shine);
        SetPixel(tex, 5, 4, shine);
        SetPixel(tex, 18, 5, shine);

        // Seams
        FillRect(tex, 11, 7, 2, 10, darkPink);
        FillRect(tex, 7, 1, 1, 7, darkPink);
        FillRect(tex, 16, 1, 1, 7, darkPink);

        // Waistband
        FillRect(tex, 4, 16, 16, 2, darkPink);

        // Belt buckle
        FillRect(tex, 10, 16, 4, 2, silver);

        tex.Apply();
        return tex;
    }

    void OnDestroy()
    {
        foreach (var tex in clothingTextures.Values)
        {
            if (tex != null) Destroy(tex);
        }
        clothingTextures.Clear();
    }
}
