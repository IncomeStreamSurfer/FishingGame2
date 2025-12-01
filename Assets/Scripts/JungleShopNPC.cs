using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Jungle realm shopkeeper selling jungle-themed clothing and items
/// Located in the raised hut shop
/// </summary>
public class JungleShopNPC : MonoBehaviour
{
    private bool isNearPlayer = false;
    private bool shopOpen = false;
    private Vector2 scrollPosition;

    private List<ClothingItem> shopItems = new List<ClothingItem>();
    private static Dictionary<string, string> playerEquipment = new Dictionary<string, string>();

    // UI
    private GUIStyle headerStyle;
    private GUIStyle itemStyle;
    private GUIStyle buttonStyle;
    private GUIStyle priceStyle;
    private GUIStyle equippedStyle;
    private GUIStyle descStyle;

    private static HashSet<string> purchasedItems = new HashSet<string>();

    // Pixel art icon cache
    private Dictionary<string, Texture2D> itemIcons = new Dictionary<string, Texture2D>();

    void Start()
    {
        // Create pixel art icons first
        CreateItemIcons();

        // Initialize shop items - jungle themed clothing
        shopItems.Add(new ClothingItem
        {
            name = "Tribal Loincloth",
            slot = "Legs",
            price = 150,
            description = "Traditional jungle tribe attire",
            previewColor = new Color(0.6f, 0.45f, 0.25f)
        });

        shopItems.Add(new ClothingItem
        {
            name = "Vine Wrapped Top",
            slot = "Top",
            price = 200,
            description = "Woven from jungle vines",
            previewColor = new Color(0.3f, 0.5f, 0.2f)
        });

        shopItems.Add(new ClothingItem
        {
            name = "Explorer's Vest",
            slot = "Top",
            price = 350,
            description = "Lightweight vest for jungle expeditions",
            previewColor = new Color(0.55f, 0.5f, 0.35f)
        });

        shopItems.Add(new ClothingItem
        {
            name = "Feathered Headdress",
            slot = "Hat",
            price = 400,
            description = "Colorful parrot feather headdress",
            previewColor = new Color(0.9f, 0.3f, 0.3f)
        });

        shopItems.Add(new ClothingItem
        {
            name = "Safari Hat",
            slot = "Hat",
            price = 250,
            description = "Classic explorer's pith helmet",
            previewColor = new Color(0.85f, 0.8f, 0.65f)
        });

        shopItems.Add(new ClothingItem
        {
            name = "Jungle Camo Pants",
            slot = "Legs",
            price = 300,
            description = "Blend in with the foliage",
            previewColor = new Color(0.25f, 0.4f, 0.2f)
        });

        shopItems.Add(new ClothingItem
        {
            name = "Tribal Face Paint",
            slot = "Face",
            price = 100,
            description = "Traditional warrior markings",
            previewColor = new Color(0.8f, 0.2f, 0.2f)
        });

        shopItems.Add(new ClothingItem
        {
            name = "Snake Skin Boots",
            slot = "Legs",
            price = 500,
            description = "Made from jungle snake hide",
            previewColor = new Color(0.4f, 0.35f, 0.25f)
        });

        shopItems.Add(new ClothingItem
        {
            name = "Shaman's Robe",
            slot = "Top",
            price = 800,
            description = "Mystical jungle shaman garb",
            previewColor = new Color(0.4f, 0.2f, 0.5f),
            isFullBodyOutfit = true
        });
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check distance to player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            isNearPlayer = dist < 4f;
        }

        // Toggle shop with E
        if (isNearPlayer && Input.GetKeyDown(KeyCode.E))
        {
            shopOpen = !shopOpen;
        }

        // Close shop if player walks away
        if (!isNearPlayer && shopOpen)
        {
            shopOpen = false;
        }

        // Close with Escape
        if (shopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            shopOpen = false;
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Initialize styles - consistent with UI overhaul
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 14; // Smaller
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold

            itemStyle = new GUIStyle(GUI.skin.label);
            itemStyle.fontSize = 11; // Smaller
            itemStyle.fontStyle = FontStyle.Bold;
            itemStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 10; // Smaller
            buttonStyle.fontStyle = FontStyle.Bold;

            priceStyle = new GUIStyle(GUI.skin.label);
            priceStyle.fontSize = 10; // Smaller
            priceStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold
            priceStyle.alignment = TextAnchor.MiddleRight;

            equippedStyle = new GUIStyle(GUI.skin.label);
            equippedStyle.fontSize = 9; // Smaller
            equippedStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);
            equippedStyle.fontStyle = FontStyle.Italic;

            descStyle = new GUIStyle(GUI.skin.label);
            descStyle.fontSize = 9; // Smaller
            descStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f); // Light gray/cream
            descStyle.wordWrap = true;
        }

        // Show interact prompt - smaller
        if (isNearPlayer && !shopOpen)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 12; // Smaller
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);

            string prompt = "[E] Talk to Tribal Trader";
            Vector2 size = promptStyle.CalcSize(new GUIContent(prompt));
            GUI.Label(new Rect((Screen.width - size.x) / 2, Screen.height - 80, size.x, size.y), prompt, promptStyle);
        }

        if (!shopOpen) return;

        // Shop window - 35% smaller (500 -> 325, 550 -> 358)
        float windowWidth = 325;
        float windowHeight = 358;
        float x = (Screen.width - windowWidth) / 2;
        float y = (Screen.height - windowHeight) / 2;

        // Background - consistent style
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.12f, 0.95f));
        bgTex.Apply();
        GUI.DrawTexture(new Rect(x, y, windowWidth, windowHeight), bgTex);
        Object.Destroy(bgTex);

        // Header - smaller
        GUI.Label(new Rect(x, y + 6, windowWidth, 26), "Tribal Trader", headerStyle);

        // Player gold - smaller
        int gold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUI.Label(new Rect(x + windowWidth - 100, y + 10, 90, 20), "Gold: " + gold, priceStyle);

        // Close button - top-right, smaller
        GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
        closeStyle.fontSize = 10;
        closeStyle.fontStyle = FontStyle.Bold;
        if (GUI.Button(new Rect(x + windowWidth - 22, y + 4, 18, 18), "X", closeStyle))
        {
            shopOpen = false;
        }

        // Dialogue - smaller
        GUIStyle dialogueStyle = new GUIStyle(GUI.skin.label);
        dialogueStyle.fontSize = 9; // Smaller
        dialogueStyle.wordWrap = true;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f); // Light gray/cream
        dialogueStyle.fontStyle = FontStyle.Italic;

        GUI.Label(new Rect(x + 12, y + 32, windowWidth - 24, 26),
            "\"Welcome! Items to survive the jungle...\"", dialogueStyle);

        // Items scroll view - smaller
        float scrollY = y + 65;
        float scrollHeight = windowHeight - 78;
        float contentHeight = shopItems.Count * 55; // Smaller item height

        scrollPosition = GUI.BeginScrollView(
            new Rect(x + 6, scrollY, windowWidth - 12, scrollHeight),
            scrollPosition,
            new Rect(0, 0, windowWidth - 24, contentHeight)
        );

        float itemY = 0;
        foreach (var item in shopItems)
        {
            DrawShopItem(item, 0, itemY, windowWidth - 24);
            itemY += 55; // Smaller spacing
        }

        GUI.EndScrollView();
    }

    void DrawShopItem(ClothingItem item, float x, float y, float width)
    {
        // Item background - consistent style
        Texture2D itemBg = new Texture2D(1, 1);
        itemBg.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.17f, 0.9f));
        itemBg.Apply();
        GUI.DrawTexture(new Rect(x, y, width, 52), itemBg);
        Object.Destroy(itemBg);

        // Pixel art icon or color preview fallback - smaller
        if (itemIcons.ContainsKey(item.name))
        {
            GUI.DrawTexture(new Rect(x + 6, y + 6, 40, 40), itemIcons[item.name]);
        }
        else
        {
            // Fallback to color preview
            Color oldColor = GUI.color;
            GUI.color = item.previewColor;
            GUI.DrawTexture(new Rect(x + 6, y + 6, 40, 40), Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        // Item name - smaller
        GUI.Label(new Rect(x + 52, y + 3, width - 120, 16), item.name, itemStyle);

        // Slot - smaller
        GUIStyle slotStyle = new GUIStyle(GUI.skin.label);
        slotStyle.fontSize = 8; // Smaller
        slotStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(x + 52, y + 18, 80, 12), "[" + item.slot + "]", slotStyle);

        // Description - smaller
        GUI.Label(new Rect(x + 52, y + 30, width - 120, 20), item.description, descStyle);

        // Price - smaller
        GUI.Label(new Rect(x + width - 70, y + 3, 65, 16), item.price + "g", priceStyle);

        // Check if purchased/equipped
        bool isPurchased = purchasedItems.Contains(item.name);
        bool isEquipped = playerEquipment.ContainsKey(item.slot) && playerEquipment[item.slot] == item.name;

        if (isEquipped)
        {
            GUI.Label(new Rect(x + width - 70, y + 32, 65, 16), "EQUIPPED", equippedStyle);
        }
        else if (isPurchased)
        {
            if (GUI.Button(new Rect(x + width - 70, y + 29, 65, 20), "Equip", buttonStyle))
            {
                EquipItem(item);
            }
        }
        else
        {
            int gold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
            GUI.enabled = gold >= item.price;

            if (GUI.Button(new Rect(x + width - 70, y + 29, 65, 20), "Buy", buttonStyle))
            {
                BuyItem(item);
            }

            GUI.enabled = true;
        }
    }

    void BuyItem(ClothingItem item)
    {
        if (GameManager.Instance == null) return;

        int gold = GameManager.Instance.GetCoins();
        if (gold >= item.price)
        {
            GameManager.Instance.AddCoins(-item.price);
            purchasedItems.Add(item.name);
            EquipItem(item);
            Debug.Log("Purchased " + item.name + " for " + item.price + " gold!");
        }
    }

    void EquipItem(ClothingItem item)
    {
        if (item.isFullBodyOutfit)
        {
            // Full body outfit takes both Top and Legs slots
            PlayerClothingVisuals.Instance?.EquipClothing("Top", item.name, item.previewColor);
            PlayerClothingVisuals.Instance?.EquipClothing("Legs", item.name, item.previewColor);
            playerEquipment["Top"] = item.name;
            playerEquipment["Legs"] = item.name;
        }
        else
        {
            PlayerClothingVisuals.Instance?.EquipClothing(item.slot, item.name, item.previewColor);
            playerEquipment[item.slot] = item.name;
        }

        Debug.Log("Equipped " + item.name);
    }

    #region Pixel Art Icon Creation
    void CreateItemIcons()
    {
        // Create 24x24 pixel art icons for each jungle item
        itemIcons["Tribal Loincloth"] = CreateTribalLoinclothIcon();
        itemIcons["Vine Wrapped Top"] = CreateVineWrappedTopIcon();
        itemIcons["Explorer's Vest"] = CreateExplorersVestIcon();
        itemIcons["Feathered Headdress"] = CreateFeatheredHeaddressIcon();
        itemIcons["Safari Hat"] = CreateSafariHatIcon();
        itemIcons["Jungle Camo Pants"] = CreateJungleCamoPantsIcon();
        itemIcons["Tribal Face Paint"] = CreateTribalFacePaintIcon();
        itemIcons["Snake Skin Boots"] = CreateSnakeSkinBootsIcon();
        itemIcons["Shaman's Robe"] = CreateShamansRobeIcon();
    }

    Texture2D CreateTexture()
    {
        Texture2D tex = new Texture2D(24, 24);
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < 24; x++)
            for (int y = 0; y < 24; y++)
                tex.SetPixel(x, y, clear);
        return tex;
    }

    void FinalizeTexture(Texture2D tex)
    {
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
    }

    void FillRect(Texture2D tex, int x, int y, int w, int h, Color col)
    {
        for (int px = x; px < x + w && px < 24; px++)
            for (int py = y; py < y + h && py < 24; py++)
                if (px >= 0 && py >= 0)
                    tex.SetPixel(px, py, col);
    }

    Texture2D CreateTribalLoinclothIcon()
    {
        Texture2D tex = CreateTexture();
        Color tan = new Color(0.6f, 0.45f, 0.25f);
        Color tanDark = new Color(0.5f, 0.35f, 0.2f);
        Color brown = new Color(0.4f, 0.25f, 0.15f);

        // Waistband
        FillRect(tex, 4, 16, 16, 2, brown);

        // Loincloth flaps
        FillRect(tex, 6, 8, 5, 9, tan);
        FillRect(tex, 13, 8, 5, 9, tan);

        // Shading
        FillRect(tex, 6, 8, 1, 9, tanDark);
        FillRect(tex, 13, 8, 1, 9, tanDark);

        // Fringe at bottom
        for (int i = 6; i < 11; i++)
        {
            if (i % 2 == 0) tex.SetPixel(i, 7, tanDark);
        }
        for (int i = 13; i < 18; i++)
        {
            if (i % 2 == 0) tex.SetPixel(i, 7, tanDark);
        }

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateVineWrappedTopIcon()
    {
        Texture2D tex = CreateTexture();
        Color greenLight = new Color(0.4f, 0.6f, 0.3f);
        Color greenDark = new Color(0.25f, 0.45f, 0.2f);
        Color leaf = new Color(0.3f, 0.5f, 0.2f);

        // Vine straps wrapping around torso
        FillRect(tex, 5, 4, 14, 14, greenLight);

        // Vine pattern (diagonal wrapping)
        for (int i = 0; i < 3; i++)
        {
            int offset = i * 5;
            FillRect(tex, 5 + offset, 4, 2, 14, greenDark);
            FillRect(tex, 7 + offset, 4, 1, 14, leaf);
        }

        // Shoulder straps
        FillRect(tex, 7, 16, 3, 4, greenDark);
        FillRect(tex, 14, 16, 3, 4, greenDark);

        // Leaves accent
        tex.SetPixel(6, 14, leaf);
        tex.SetPixel(7, 15, leaf);
        tex.SetPixel(16, 12, leaf);
        tex.SetPixel(17, 13, leaf);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateExplorersVestIcon()
    {
        Texture2D tex = CreateTexture();
        Color khaki = new Color(0.55f, 0.5f, 0.35f);
        Color khakiLight = new Color(0.65f, 0.6f, 0.45f);
        Color khakiDark = new Color(0.45f, 0.4f, 0.25f);
        Color pocket = new Color(0.35f, 0.3f, 0.2f);

        // Vest body
        FillRect(tex, 5, 2, 14, 16, khaki);

        // Collar
        FillRect(tex, 8, 16, 8, 3, khakiLight);

        // Pockets
        FillRect(tex, 7, 6, 4, 4, pocket);
        FillRect(tex, 13, 6, 4, 4, pocket);

        // Pocket flaps
        FillRect(tex, 7, 9, 4, 1, khakiDark);
        FillRect(tex, 13, 9, 4, 1, khakiDark);

        // Buttons
        tex.SetPixel(12, 12, khakiDark);
        tex.SetPixel(12, 8, khakiDark);

        // Shading
        FillRect(tex, 5, 2, 1, 16, khakiDark);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateFeatheredHeaddressIcon()
    {
        Texture2D tex = CreateTexture();
        Color red = new Color(0.9f, 0.3f, 0.3f);
        Color yellow = new Color(0.95f, 0.85f, 0.3f);
        Color blue = new Color(0.3f, 0.5f, 0.9f);
        Color green = new Color(0.3f, 0.8f, 0.4f);
        Color band = new Color(0.5f, 0.35f, 0.2f);

        // Headband
        FillRect(tex, 4, 8, 16, 3, band);

        // Colorful feathers going upward
        Color[] featherColors = { red, yellow, blue, green, red, yellow };
        for (int i = 0; i < 6; i++)
        {
            int x = 6 + i * 2;
            // Feather shaft
            FillRect(tex, x, 11, 1, 8, featherColors[i]);
            // Feather tip
            tex.SetPixel(x - 1, 17, featherColors[i]);
            tex.SetPixel(x + 1, 17, featherColors[i]);
            tex.SetPixel(x, 19, featherColors[i]);
        }

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateSafariHatIcon()
    {
        Texture2D tex = CreateTexture();
        Color beige = new Color(0.85f, 0.8f, 0.65f);
        Color beigeLight = new Color(0.95f, 0.9f, 0.75f);
        Color beigeDark = new Color(0.7f, 0.65f, 0.5f);
        Color brown = new Color(0.4f, 0.3f, 0.2f);

        // Hat dome (pith helmet shape)
        FillRect(tex, 6, 12, 12, 8, beige);
        FillRect(tex, 8, 18, 8, 3, beige);

        // Top highlight
        FillRect(tex, 8, 18, 8, 2, beigeLight);

        // Brim
        FillRect(tex, 2, 10, 20, 3, beige);
        FillRect(tex, 2, 12, 20, 1, beigeDark);

        // Hat band
        FillRect(tex, 6, 12, 12, 2, brown);

        // Shading ridges (pith helmet detail)
        for (int i = 8; i < 16; i += 2)
        {
            tex.SetPixel(i, 16, beigeDark);
        }

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateJungleCamoPantsIcon()
    {
        Texture2D tex = CreateTexture();
        Color green1 = new Color(0.25f, 0.4f, 0.2f);
        Color green2 = new Color(0.3f, 0.5f, 0.25f);
        Color green3 = new Color(0.2f, 0.35f, 0.15f);
        Color darkGreen = new Color(0.15f, 0.25f, 0.1f);

        // Pants base
        FillRect(tex, 4, 2, 7, 16, green1);
        FillRect(tex, 13, 2, 7, 16, green1);
        FillRect(tex, 4, 14, 16, 4, green1);

        // Camo pattern (irregular spots)
        System.Random rng = new System.Random(42); // Fixed seed for consistent pattern
        for (int i = 0; i < 20; i++)
        {
            int x = 4 + rng.Next(16);
            int y = 2 + rng.Next(16);
            Color camoColor = rng.Next(3) == 0 ? green2 : (rng.Next(2) == 0 ? green3 : darkGreen);

            if (x >= 4 && x < 20 && y >= 2 && y < 18)
            {
                tex.SetPixel(x, y, camoColor);
                if (rng.Next(2) == 0 && x + 1 < 20) tex.SetPixel(x + 1, y, camoColor);
                if (rng.Next(2) == 0 && y + 1 < 18) tex.SetPixel(x, y + 1, camoColor);
            }
        }

        // Waistband
        FillRect(tex, 4, 16, 16, 2, darkGreen);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateTribalFacePaintIcon()
    {
        Texture2D tex = CreateTexture();
        Color skin = new Color(0.8f, 0.65f, 0.5f);
        Color red = new Color(0.8f, 0.2f, 0.2f);
        Color black = new Color(0.1f, 0.1f, 0.1f);

        // Face outline
        FillRect(tex, 7, 4, 10, 14, skin);
        FillRect(tex, 9, 2, 6, 2, skin);

        // War paint stripes (horizontal red)
        FillRect(tex, 7, 14, 10, 2, red);
        FillRect(tex, 7, 10, 10, 2, red);

        // Black stripes
        FillRect(tex, 7, 16, 10, 1, black);
        FillRect(tex, 7, 12, 10, 1, black);

        // Eyes
        tex.SetPixel(10, 8, black);
        tex.SetPixel(14, 8, black);

        // Tribal symbols (dots and lines)
        tex.SetPixel(8, 6, red);
        tex.SetPixel(16, 6, red);
        FillRect(tex, 11, 5, 2, 1, black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateSnakeSkinBootsIcon()
    {
        Texture2D tex = CreateTexture();
        Color scale1 = new Color(0.4f, 0.35f, 0.25f);
        Color scale2 = new Color(0.5f, 0.45f, 0.3f);
        Color scale3 = new Color(0.3f, 0.25f, 0.2f);
        Color black = new Color(0.1f, 0.1f, 0.1f);

        // Boot shape (tall boots)
        FillRect(tex, 4, 2, 6, 14, scale1);
        FillRect(tex, 14, 2, 6, 14, scale1);

        // Snake scale pattern
        for (int y = 3; y < 16; y += 2)
        {
            for (int x = 4; x < 10; x += 2)
            {
                Color scaleColor = (x + y) % 4 == 0 ? scale2 : scale3;
                tex.SetPixel(x, y, scaleColor);
                tex.SetPixel(x + 1, y, scaleColor);
            }
            for (int x = 14; x < 20; x += 2)
            {
                Color scaleColor = (x + y) % 4 == 0 ? scale2 : scale3;
                tex.SetPixel(x, y, scaleColor);
                tex.SetPixel(x + 1, y, scaleColor);
            }
        }

        // Boot tops
        FillRect(tex, 4, 14, 6, 2, black);
        FillRect(tex, 14, 14, 6, 2, black);

        // Soles
        FillRect(tex, 4, 2, 6, 1, black);
        FillRect(tex, 14, 2, 6, 1, black);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateShamansRobeIcon()
    {
        Texture2D tex = CreateTexture();
        Color purple = new Color(0.4f, 0.2f, 0.5f);
        Color purpleLight = new Color(0.5f, 0.3f, 0.6f);
        Color purpleDark = new Color(0.3f, 0.15f, 0.4f);
        Color mystical = new Color(0.8f, 0.6f, 1f);
        Color gold = new Color(0.9f, 0.75f, 0.3f);

        // Robe body (long flowing)
        FillRect(tex, 4, 0, 16, 20, purple);

        // Hood
        FillRect(tex, 6, 18, 12, 4, purpleDark);
        FillRect(tex, 8, 20, 8, 2, purpleDark);

        // Wide sleeves
        FillRect(tex, 0, 10, 5, 8, purple);
        FillRect(tex, 19, 10, 5, 8, purple);

        // Mystical symbols/runes
        tex.SetPixel(12, 8, mystical);
        tex.SetPixel(12, 9, mystical);
        tex.SetPixel(11, 9, mystical);
        tex.SetPixel(13, 9, mystical);

        tex.SetPixel(10, 4, gold);
        tex.SetPixel(14, 4, gold);
        FillRect(tex, 11, 6, 2, 1, gold);

        // Mystical energy dots
        tex.SetPixel(7, 12, mystical);
        tex.SetPixel(17, 14, mystical);
        tex.SetPixel(9, 6, mystical);

        // Hem decoration
        for (int x = 5; x < 19; x += 3)
        {
            tex.SetPixel(x, 1, gold);
        }

        // Shading
        FillRect(tex, 4, 0, 2, 20, purpleDark);

        FinalizeTexture(tex);
        return tex;
    }
    #endregion

    // ClothingItem class if not already defined elsewhere
    [System.Serializable]
    public class ClothingItem
    {
        public string name;
        public string slot;
        public int price;
        public string description;
        public Color previewColor;
        public bool isFullBodyOutfit = false;
    }
}
