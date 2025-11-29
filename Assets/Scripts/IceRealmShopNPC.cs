using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ice Realm Igloo Shop - Native old woman selling winter clothing
/// Located in a large igloo with a roaring fire
/// </summary>
public class IceRealmShopNPC : MonoBehaviour
{
    public static IceRealmShopNPC Instance { get; private set; }

    // State
    private bool shopOpen = false;
    private bool playerNearby = false;
    private float interactionDistance = 5f;

    // Shop inventory
    private List<ClothingItem> clothingItems = new List<ClothingItem>();
    private int selectedSlot = 0;
    private int selectedItemIndex = 0;
    private string[] slotNames = { "Head", "Top", "Legs", "Accessory", "Special" };

    // Player equipment tracking
    private Dictionary<string, string> playerEquipment = new Dictionary<string, string>()
    {
        { "Head", "None" },
        { "Top", "None" },
        { "Legs", "None" },
        { "Accessory", "None" },
        { "Special", "None" }
    };
    private List<string> ownedItems = new List<string>();

    // Scroll position for items
    private Vector2 scrollPosition;

    // UI Textures
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Dictionary<string, Texture2D> itemIcons = new Dictionary<string, Texture2D>();

    // NPC dialogue
    private string[] greetings = {
        "Welcome to my igloo, traveler...",
        "The cold winds brought you here...",
        "My furs will keep you warm...",
        "Come, see my wares..."
    };
    private string currentGreeting;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeClothingItems();
        CreateUITextures();
        CreateItemIcons();
        currentGreeting = greetings[Random.Range(0, greetings.Length)];
    }

    void InitializeClothingItems()
    {
        // HEAD ITEMS
        clothingItems.Add(new ClothingItem("Bear Skin Hat", "Head", 5000,
            "A fearsome polar bear head worn as a hat. Strike fear into your enemies!",
            new Color(0.95f, 0.95f, 0.98f)));

        clothingItems.Add(new ClothingItem("Pirate Hat", "Head", 1000,
            "A black top hat with skull and crossbones. Yarr!",
            new Color(0.1f, 0.1f, 0.1f)));

        clothingItems.Add(new ClothingItem("Beret", "Head", 500,
            "A stylish French beret. Very artistic.",
            new Color(0.6f, 0.1f, 0.15f)));

        // TOP ITEMS
        clothingItems.Add(new ClothingItem("Husband Beater", "Top", 50,
            "A basic white sleeveless vest. Simple and practical.",
            new Color(0.95f, 0.95f, 0.95f)));

        clothingItems.Add(new ClothingItem("Pink Fur Coat", "Top", 100000,
            "A luxurious long pink fur coat. Soft and beautiful!",
            new Color(1f, 0.6f, 0.7f)));

        // LEGS ITEMS
        clothingItems.Add(new ClothingItem("Black Leather Hotpants", "Legs", 10000,
            "Short, shiny black leather hotpants. Bold choice!",
            new Color(0.08f, 0.08f, 0.08f)));

        clothingItems.Add(new ClothingItem("Whale Bladder Pants", "Legs", 1000,
            "Fleshy pink pants made from whale bladder. Traditional!",
            new Color(0.9f, 0.7f, 0.7f)));

        clothingItems.Add(new ClothingItem("Pink Leather Pants", "Legs", 10000,
            "Bright neon pink leather pants with a glossy shine!",
            new Color(1f, 0.2f, 0.6f)));

        // SPECIAL ITEMS (Accessory slot but with special effects)
        clothingItems.Add(new ClothingItem("Ice Queens Ring", "Special", 1000000,
            "A magnificent diamond ring! Doubles all gold earned while worn. Press E to toggle glow.",
            new Color(0.9f, 0.95f, 1f))
        {
            statBonus = "+100% Gold",
            luckBonus = 0f // We'll handle gold bonus specially
        });

        clothingItems.Add(new ClothingItem("Polar Bear Cub", "Special", 250000,
            "An adorable baby polar bear that follows you! Feed it fish with E.",
            new Color(0.98f, 0.98f, 1f)));
    }

    void CreateUITextures()
    {
        // Panel background - dark icy blue
        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0.15f, 0.2f, 0.3f, 0.95f));
        panelTexture.Apply();

        // Button textures
        buttonTexture = new Texture2D(1, 1);
        buttonTexture.SetPixel(0, 0, new Color(0.3f, 0.4f, 0.5f, 0.9f));
        buttonTexture.Apply();

        buttonHoverTexture = new Texture2D(1, 1);
        buttonHoverTexture.SetPixel(0, 0, new Color(0.4f, 0.55f, 0.7f, 0.95f));
        buttonHoverTexture.Apply();
    }

    void CreateItemIcons()
    {
        // Create 24x24 pixel art icons for each item
        itemIcons["Bear Skin Hat"] = CreateBearSkinHatIcon();
        itemIcons["Pirate Hat"] = CreatePirateHatIcon();
        itemIcons["Beret"] = CreateBeretIcon();
        itemIcons["Husband Beater"] = CreateHusbandBeaterIcon();
        itemIcons["Pink Fur Coat"] = CreatePinkFurCoatIcon();
        itemIcons["Black Leather Hotpants"] = CreateBlackHotpantsIcon();
        itemIcons["Whale Bladder Pants"] = CreateWhaleBladderPantsIcon();
        itemIcons["Pink Leather Pants"] = CreatePinkLeatherPantsIcon();
        itemIcons["Ice Queens Ring"] = CreateIceQueensRingIcon();
        itemIcons["Polar Bear Cub"] = CreatePolarBearCubIcon();
    }

    #region Pixel Art Icon Creation
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

    Texture2D CreateBearSkinHatIcon()
    {
        Texture2D tex = CreateTexture();
        Color fur = new Color(0.95f, 0.95f, 0.98f);
        Color furDark = new Color(0.8f, 0.8f, 0.85f);
        Color nose = new Color(0.15f, 0.15f, 0.15f);
        Color eye = Color.black;

        // Bear head shape
        FillRect(tex, 4, 6, 16, 14, fur);
        FillRect(tex, 6, 4, 12, 2, fur);
        FillRect(tex, 2, 10, 2, 8, fur); // Left ear
        FillRect(tex, 20, 10, 2, 8, fur); // Right ear

        // Snout
        FillRect(tex, 8, 2, 8, 6, furDark);
        FillRect(tex, 10, 2, 4, 2, nose); // Nose

        // Eyes
        tex.SetPixel(8, 12, eye);
        tex.SetPixel(9, 12, eye);
        tex.SetPixel(14, 12, eye);
        tex.SetPixel(15, 12, eye);

        // Fur texture
        for (int i = 0; i < 8; i++)
        {
            int rx = Random.Range(5, 19);
            int ry = Random.Range(7, 18);
            tex.SetPixel(rx, ry, furDark);
        }

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreatePirateHatIcon()
    {
        Texture2D tex = CreateTexture();
        Color black = new Color(0.1f, 0.1f, 0.1f);
        Color gold = new Color(0.9f, 0.75f, 0.3f);
        Color white = Color.white;

        // Hat body (rounded top)
        FillRect(tex, 2, 4, 20, 12, black);
        FillRect(tex, 4, 16, 16, 4, black);
        FillRect(tex, 6, 20, 12, 2, black);

        // Brim
        FillRect(tex, 0, 2, 24, 3, black);

        // Skull
        FillRect(tex, 9, 10, 6, 6, white);
        tex.SetPixel(10, 14, black); // Eye
        tex.SetPixel(13, 14, black); // Eye
        FillRect(tex, 10, 10, 4, 2, white); // Jaw

        // Crossbones
        tex.SetPixel(7, 8, white);
        tex.SetPixel(8, 9, white);
        tex.SetPixel(15, 9, white);
        tex.SetPixel(16, 8, white);
        tex.SetPixel(7, 12, white);
        tex.SetPixel(8, 11, white);
        tex.SetPixel(15, 11, white);
        tex.SetPixel(16, 12, white);

        // Gold trim
        FillRect(tex, 0, 4, 24, 1, gold);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateBeretIcon()
    {
        Texture2D tex = CreateTexture();
        Color red = new Color(0.6f, 0.1f, 0.15f);
        Color redDark = new Color(0.4f, 0.05f, 0.1f);
        Color stem = new Color(0.2f, 0.2f, 0.2f);

        // Beret shape (flat round)
        FillRect(tex, 4, 6, 16, 10, red);
        FillRect(tex, 6, 4, 12, 2, red);
        FillRect(tex, 8, 16, 8, 2, red);

        // Band
        FillRect(tex, 4, 6, 16, 2, redDark);

        // Stem on top
        FillRect(tex, 11, 18, 2, 3, stem);

        // Shading
        FillRect(tex, 4, 6, 3, 8, redDark);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateHusbandBeaterIcon()
    {
        Texture2D tex = CreateTexture();
        Color white = new Color(0.95f, 0.95f, 0.95f);
        Color gray = new Color(0.85f, 0.85f, 0.85f);

        // Vest body
        FillRect(tex, 6, 2, 12, 16, white);

        // Arm holes (cut out sides at top)
        FillRect(tex, 4, 12, 4, 6, new Color(0, 0, 0, 0));
        FillRect(tex, 16, 12, 4, 6, new Color(0, 0, 0, 0));

        // Neck opening
        FillRect(tex, 9, 16, 6, 3, new Color(0, 0, 0, 0));

        // Shading
        FillRect(tex, 6, 2, 2, 14, gray);
        FillRect(tex, 6, 2, 12, 2, gray);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreatePinkFurCoatIcon()
    {
        Texture2D tex = CreateTexture();
        Color pink = new Color(1f, 0.6f, 0.7f);
        Color pinkLight = new Color(1f, 0.75f, 0.82f);
        Color pinkDark = new Color(0.85f, 0.45f, 0.55f);

        // Coat body (long)
        FillRect(tex, 4, 0, 16, 20, pink);

        // Collar (fluffy)
        FillRect(tex, 6, 18, 12, 4, pinkLight);
        FillRect(tex, 4, 16, 4, 4, pinkLight);
        FillRect(tex, 16, 16, 4, 4, pinkLight);

        // Sleeves
        FillRect(tex, 0, 10, 5, 8, pink);
        FillRect(tex, 19, 10, 5, 8, pink);

        // Cuffs (fluffy)
        FillRect(tex, 0, 10, 5, 2, pinkLight);
        FillRect(tex, 19, 10, 5, 2, pinkLight);

        // Fur texture dots
        for (int i = 0; i < 15; i++)
        {
            int rx = Random.Range(5, 19);
            int ry = Random.Range(2, 18);
            tex.SetPixel(rx, ry, pinkLight);
        }

        // Shading
        FillRect(tex, 4, 0, 2, 18, pinkDark);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateBlackHotpantsIcon()
    {
        Texture2D tex = CreateTexture();
        Color black = new Color(0.08f, 0.08f, 0.08f);
        Color shine = new Color(0.3f, 0.3f, 0.35f);

        // Short pants
        FillRect(tex, 4, 8, 16, 10, black);

        // Waistband
        FillRect(tex, 4, 16, 16, 2, new Color(0.15f, 0.15f, 0.15f));

        // Leg openings
        FillRect(tex, 4, 8, 7, 2, black);
        FillRect(tex, 13, 8, 7, 2, black);

        // Shine effect (glossy)
        FillRect(tex, 8, 12, 3, 4, shine);
        FillRect(tex, 14, 14, 2, 2, shine);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateWhaleBladderPantsIcon()
    {
        Texture2D tex = CreateTexture();
        Color flesh = new Color(0.9f, 0.7f, 0.7f);
        Color fleshDark = new Color(0.75f, 0.55f, 0.55f);

        // Pants shape
        FillRect(tex, 4, 2, 7, 16, flesh);
        FillRect(tex, 13, 2, 7, 16, flesh);
        FillRect(tex, 4, 14, 16, 4, flesh);

        // Waistband
        FillRect(tex, 4, 16, 16, 2, fleshDark);

        // Organic texture
        for (int i = 0; i < 10; i++)
        {
            int rx = Random.Range(5, 19);
            int ry = Random.Range(4, 16);
            tex.SetPixel(rx, ry, fleshDark);
        }

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreatePinkLeatherPantsIcon()
    {
        Texture2D tex = CreateTexture();
        Color pink = new Color(1f, 0.2f, 0.6f);
        Color shine = new Color(1f, 0.6f, 0.8f);
        Color pinkDark = new Color(0.8f, 0.1f, 0.4f);

        // Pants shape
        FillRect(tex, 4, 2, 7, 16, pink);
        FillRect(tex, 13, 2, 7, 16, pink);
        FillRect(tex, 4, 14, 16, 4, pink);

        // Waistband
        FillRect(tex, 4, 16, 16, 2, pinkDark);

        // Glossy shine streaks
        FillRect(tex, 6, 6, 2, 8, shine);
        FillRect(tex, 15, 4, 2, 10, shine);
        tex.SetPixel(8, 10, shine);
        tex.SetPixel(17, 8, shine);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateIceQueensRingIcon()
    {
        Texture2D tex = CreateTexture();
        Color gold = new Color(1f, 0.85f, 0.3f);
        Color goldDark = new Color(0.8f, 0.65f, 0.2f);
        Color diamond = new Color(0.9f, 0.95f, 1f);
        Color diamondShine = Color.white;

        // Ring band
        FillRect(tex, 6, 6, 12, 4, gold);
        FillRect(tex, 4, 8, 2, 2, gold);
        FillRect(tex, 18, 8, 2, 2, gold);
        FillRect(tex, 6, 4, 12, 2, goldDark);

        // Diamond (large faceted)
        FillRect(tex, 8, 10, 8, 10, diamond);
        FillRect(tex, 10, 20, 4, 2, diamond);

        // Diamond facets/shine
        tex.SetPixel(10, 18, diamondShine);
        tex.SetPixel(11, 17, diamondShine);
        tex.SetPixel(13, 19, diamondShine);
        tex.SetPixel(14, 16, diamondShine);
        FillRect(tex, 11, 14, 2, 3, diamondShine);

        // Gold prongs
        tex.SetPixel(8, 10, gold);
        tex.SetPixel(15, 10, gold);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreatePolarBearCubIcon()
    {
        Texture2D tex = CreateTexture();
        Color fur = new Color(0.98f, 0.98f, 1f);
        Color furShade = new Color(0.85f, 0.87f, 0.9f);
        Color nose = new Color(0.15f, 0.15f, 0.15f);
        Color eye = Color.black;

        // Body (round cute)
        FillRect(tex, 6, 2, 12, 10, fur);

        // Head
        FillRect(tex, 7, 10, 10, 10, fur);

        // Ears
        FillRect(tex, 5, 18, 4, 4, fur);
        FillRect(tex, 15, 18, 4, 4, fur);
        FillRect(tex, 6, 19, 2, 2, furShade);
        FillRect(tex, 16, 19, 2, 2, furShade);

        // Face
        tex.SetPixel(9, 16, eye);
        tex.SetPixel(14, 16, eye);
        FillRect(tex, 10, 12, 4, 3, furShade); // Snout
        FillRect(tex, 11, 13, 2, 1, nose); // Nose

        // Paws
        FillRect(tex, 4, 2, 3, 3, fur);
        FillRect(tex, 17, 2, 3, 3, fur);

        // Shading
        FillRect(tex, 6, 2, 2, 8, furShade);

        FinalizeTexture(tex);
        return tex;
    }
    #endregion

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerNearby = distance < interactionDistance;

        if (playerNearby && !shopOpen && Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }

        if (shopOpen)
        {
            HandleShopInput();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }
    }

    void OpenShop()
    {
        shopOpen = true;
        currentGreeting = greetings[Random.Range(0, greetings.Length)];
        selectedSlot = 0;
        selectedItemIndex = 0;
    }

    void CloseShop()
    {
        shopOpen = false;
    }

    void HandleShopInput()
    {
        // Tab switching with Q/E or arrow keys
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedSlot = (selectedSlot + 1) % slotNames.Length;
            selectedItemIndex = 0;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedSlot = (selectedSlot - 1 + slotNames.Length) % slotNames.Length;
            selectedItemIndex = 0;
        }

        // Item selection with W/S or up/down
        List<ClothingItem> currentItems = GetItemsForSlot(slotNames[selectedSlot]);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (currentItems.Count > 0)
                selectedItemIndex = (selectedItemIndex - 1 + currentItems.Count) % currentItems.Count;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (currentItems.Count > 0)
                selectedItemIndex = (selectedItemIndex + 1) % currentItems.Count;
        }

        // Purchase with Enter or Space
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (currentItems.Count > 0 && selectedItemIndex < currentItems.Count)
            {
                TryPurchaseItem(currentItems[selectedItemIndex]);
            }
        }
    }

    List<ClothingItem> GetItemsForSlot(string slot)
    {
        List<ClothingItem> items = new List<ClothingItem>();
        foreach (var item in clothingItems)
        {
            if (item.slot == slot)
                items.Add(item);
        }
        return items;
    }

    void TryPurchaseItem(ClothingItem item)
    {
        if (ownedItems.Contains(item.name))
        {
            // Already owned - equip it
            EquipItem(item);
            return;
        }

        int playerCoins = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;

        if (playerCoins >= item.price)
        {
            // Purchase
            if (GameManager.Instance != null)
                GameManager.Instance.AddCoins(-item.price);

            ownedItems.Add(item.name);

            // Add to wardrobe
            if (UIManager.Instance != null)
                UIManager.Instance.AddToWardrobe(item.name, item.slot, item.previewColor);

            // Handle special items
            if (item.name == "Polar Bear Cub")
            {
                SpawnPolarBearCubPet();
            }
            else if (item.name == "Ice Queens Ring")
            {
                EquipIceQueensRing();
            }
            else
            {
                EquipItem(item);
            }

            Debug.Log($"Purchased {item.name} for {item.price}g!");
        }
        else
        {
            Debug.Log($"Not enough gold! Need {item.price}g, have {playerCoins}g");
        }
    }

    void EquipItem(ClothingItem item)
    {
        string actualSlot = item.slot == "Special" ? "Accessory" : item.slot;
        playerEquipment[item.slot] = item.name;

        if (PlayerClothingVisuals.Instance != null)
        {
            PlayerClothingVisuals.Instance.EquipClothing(actualSlot, item.name, item.previewColor);
        }
    }

    void SpawnPolarBearCubPet()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            // Check if pet already exists
            PolarBearCubPet existingPet = FindObjectOfType<PolarBearCubPet>();
            if (existingPet == null)
            {
                GameObject petObj = new GameObject("PolarBearCubPet");
                petObj.transform.position = player.transform.position + Vector3.right * 2f;
                petObj.AddComponent<PolarBearCubPet>();
            }
        }
    }

    void EquipIceQueensRing()
    {
        // Enable the gold boost effect
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGoldMultiplier(2.0f); // 100% bonus = 2x
        }

        // Visual ring on player
        if (PlayerClothingVisuals.Instance != null)
        {
            PlayerClothingVisuals.Instance.EquipClothing("Accessory", "Ice Queens Ring", new Color(0.9f, 0.95f, 1f));
        }

        Debug.Log("Ice Queen's Ring equipped! Gold earnings doubled!");
    }

    public bool IsShopOpen()
    {
        return shopOpen;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Interaction prompt
        if (playerNearby && !shopOpen)
        {
            DrawInteractionPrompt();
        }

        // Shop UI
        if (shopOpen)
        {
            DrawShopUI();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle promptStyle = new GUIStyle();
        promptStyle.fontSize = 18;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.alignment = TextAnchor.MiddleCenter;
        promptStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);

        float promptWidth = 300;
        float promptHeight = 60;
        Rect promptRect = new Rect(
            Screen.width / 2 - promptWidth / 2,
            Screen.height - 150,
            promptWidth,
            promptHeight
        );

        GUI.Label(promptRect, "[E] Talk to the Elder", promptStyle);
    }

    void DrawShopUI()
    {
        // Main panel
        float panelWidth = 700;
        float panelHeight = 500;
        Rect panelRect = new Rect(
            Screen.width / 2 - panelWidth / 2,
            Screen.height / 2 - panelHeight / 2,
            panelWidth,
            panelHeight
        );

        // Background
        GUI.DrawTexture(panelRect, panelTexture);

        // Border
        GUI.color = new Color(0.5f, 0.7f, 0.9f);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y - 2, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y + panelRect.height, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x + panelRect.width, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + 10, panelRect.width, 30), "FROST ELDER'S WARES", titleStyle);

        // Greeting
        GUIStyle greetStyle = new GUIStyle();
        greetStyle.fontSize = 14;
        greetStyle.fontStyle = FontStyle.Italic;
        greetStyle.alignment = TextAnchor.MiddleCenter;
        greetStyle.normal.textColor = new Color(0.6f, 0.75f, 0.85f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + 40, panelRect.width, 25), $"\"{currentGreeting}\"", greetStyle);

        // Player gold display
        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUIStyle goldStyle = new GUIStyle();
        goldStyle.fontSize = 16;
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.alignment = TextAnchor.MiddleRight;
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        GUI.Label(new Rect(panelRect.x + panelRect.width - 160, panelRect.y + 10, 150, 25), $"Gold: {playerGold:N0}", goldStyle);

        // Slot tabs
        float tabY = panelRect.y + 75;
        float tabWidth = panelWidth / slotNames.Length - 10;

        GUIStyle tabStyle = new GUIStyle();
        tabStyle.fontSize = 12;
        tabStyle.fontStyle = FontStyle.Bold;
        tabStyle.alignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < slotNames.Length; i++)
        {
            Rect tabRect = new Rect(panelRect.x + 10 + i * (tabWidth + 5), tabY, tabWidth, 28);

            if (i == selectedSlot)
            {
                GUI.DrawTexture(tabRect, buttonHoverTexture);
                tabStyle.normal.textColor = Color.white;
            }
            else
            {
                GUI.DrawTexture(tabRect, buttonTexture);
                tabStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            }

            GUI.Label(tabRect, slotNames[i], tabStyle);

            // Left mouse button click to select tab
            if (GUI.Button(tabRect, "", GUIStyle.none))
            {
                selectedSlot = i;
                selectedItemIndex = 0;
            }
        }

        // Items list
        float itemsY = tabY + 40;
        float itemsHeight = panelHeight - 150;
        Rect itemsRect = new Rect(panelRect.x + 15, itemsY, panelWidth - 30, itemsHeight);

        List<ClothingItem> currentItems = GetItemsForSlot(slotNames[selectedSlot]);

        GUIStyle itemStyle = new GUIStyle();
        itemStyle.fontSize = 14;
        itemStyle.alignment = TextAnchor.MiddleLeft;

        GUIStyle priceStyle = new GUIStyle();
        priceStyle.fontSize = 14;
        priceStyle.fontStyle = FontStyle.Bold;
        priceStyle.alignment = TextAnchor.MiddleRight;

        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 11;
        descStyle.wordWrap = true;
        descStyle.alignment = TextAnchor.UpperLeft;
        descStyle.normal.textColor = new Color(0.65f, 0.75f, 0.85f);

        for (int i = 0; i < currentItems.Count; i++)
        {
            ClothingItem item = currentItems[i];
            float itemY = itemsY + i * 70;
            Rect itemRect = new Rect(itemsRect.x, itemY, itemsRect.width, 65);

            // Selection highlight
            if (i == selectedItemIndex)
            {
                GUI.DrawTexture(itemRect, buttonHoverTexture);
                itemStyle.normal.textColor = Color.white;
            }
            else
            {
                GUI.DrawTexture(itemRect, buttonTexture);
                itemStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            }

            // Item icon
            if (itemIcons.ContainsKey(item.name))
            {
                GUI.DrawTexture(new Rect(itemRect.x + 5, itemRect.y + 5, 48, 48), itemIcons[item.name]);
            }

            // Item name
            bool owned = ownedItems.Contains(item.name);
            string nameText = owned ? $"{item.name} [OWNED]" : item.name;
            GUI.Label(new Rect(itemRect.x + 60, itemRect.y + 5, 300, 25), nameText, itemStyle);

            // Description
            GUI.Label(new Rect(itemRect.x + 60, itemRect.y + 28, itemRect.width - 180, 35), item.description, descStyle);

            // Price
            if (!owned)
            {
                bool canAfford = playerGold >= item.price;
                priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.3f) : new Color(0.8f, 0.3f, 0.3f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 120, itemRect.y + 20, 110, 25), $"{item.price:N0}g", priceStyle);
            }

            // Stat bonus
            if (!string.IsNullOrEmpty(item.statBonus))
            {
                GUIStyle bonusStyle = new GUIStyle();
                bonusStyle.fontSize = 10;
                bonusStyle.fontStyle = FontStyle.Bold;
                bonusStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
                bonusStyle.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(itemRect.x + itemRect.width - 120, itemRect.y + 45, 110, 18), item.statBonus, bonusStyle);
            }

            // Left mouse button click to select and buy/equip
            if (GUI.Button(itemRect, "", GUIStyle.none))
            {
                selectedItemIndex = i;
                TryPurchaseItem(item);
            }
        }

        // Instructions
        GUIStyle instructStyle = new GUIStyle();
        instructStyle.fontSize = 12;
        instructStyle.alignment = TextAnchor.MiddleCenter;
        instructStyle.normal.textColor = new Color(0.5f, 0.6f, 0.7f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + panelHeight - 30, panelRect.width, 25),
            "[Arrow Keys/WASD or Click] Navigate | [Enter/Space or Click] Buy | [ESC] Close", instructStyle);
    }
}
