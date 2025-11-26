using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Clothing Shop NPC - A small woman who sits knitting in a rocking chair
/// Located on a small island with a palm tree
/// </summary>
public class ClothingShopNPC : MonoBehaviour
{
    public static ClothingShopNPC Instance { get; private set; }

    private bool shopOpen = false;
    private bool playerNearby = false;
    private float interactionDistance = 4f;

    // Knitting animation
    private float knittingTime = 0f;
    private Transform leftHand;
    private Transform rightHand;
    private Transform rockingChair;

    // Shop data
    private List<ClothingItem> clothingItems = new List<ClothingItem>();
    private int selectedSlot = 0;
    private string[] slotNames = { "Head", "Body", "Legs", "Feet", "Gloves", "Accessory" };

    // Player's current equipment
    private Dictionary<string, string> playerEquipment = new Dictionary<string, string>()
    {
        { "Head", "None" },
        { "Body", "Blue Shirt" },
        { "Legs", "Brown Pants" },
        { "Feet", "Leather Boots" },
        { "Gloves", "None" },
        { "Accessory", "None" }
    };

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeClothingItems();

        // Find animation components
        rockingChair = transform.Find("RockingChair");
        leftHand = transform.Find("NPCModel/LeftHand");
        rightHand = transform.Find("NPCModel/RightHand");

        // Delay texture creation
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        initialized = true;
    }

    void CreateCachedTextures()
    {
        CacheTexture("panelBg", new Color(0.15f, 0.1f, 0.08f, 0.98f));
        CacheTexture("panelBorder", new Color(0.6f, 0.4f, 0.2f, 1f));
        CacheTexture("slotBg", new Color(0.1f, 0.08f, 0.06f, 0.95f));
        CacheTexture("slotSelected", new Color(0.3f, 0.2f, 0.1f, 0.95f));
        CacheTexture("itemBg", new Color(0.12f, 0.1f, 0.08f, 0.9f));
        CacheTexture("itemHover", new Color(0.2f, 0.15f, 0.1f, 0.95f));
        CacheTexture("buttonBuy", new Color(0.2f, 0.5f, 0.3f, 1f));
        CacheTexture("buttonEquip", new Color(0.3f, 0.4f, 0.6f, 1f));
        CacheTexture("equipped", new Color(0.4f, 0.6f, 0.3f, 1f));
        CacheTexture("divider", new Color(0.5f, 0.4f, 0.3f, 0.6f));
        CacheTexture("white", Color.white);
    }

    void CacheTexture(string name, Color color)
    {
        if (!textureCache.ContainsKey(name))
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            textureCache[name] = tex;
        }
    }

    Texture2D GetTexture(string name)
    {
        return textureCache.TryGetValue(name, out Texture2D tex) ? tex : Texture2D.whiteTexture;
    }

    void InitializeClothingItems()
    {
        // Head items
        clothingItems.Add(new ClothingItem("Straw Hat", "Head", 150, "A breezy hat for sunny days", new Color(0.9f, 0.8f, 0.5f)));
        clothingItems.Add(new ClothingItem("Captain's Cap", "Head", 500, "Command respect on the seas", new Color(0.1f, 0.2f, 0.4f)));
        clothingItems.Add(new ClothingItem("Bandana", "Head", 100, "Pirate style", new Color(0.8f, 0.2f, 0.2f)));
        clothingItems.Add(new ClothingItem("Fishing Hat", "Head", 300, "Professional angler look", new Color(0.5f, 0.6f, 0.4f)));

        // Body items
        clothingItems.Add(new ClothingItem("Sailor's Coat", "Body", 750, "Stay warm at sea", new Color(0.2f, 0.3f, 0.5f)));
        clothingItems.Add(new ClothingItem("Hawaiian Shirt", "Body", 400, "Tropical vibes", new Color(1f, 0.5f, 0.3f)));
        clothingItems.Add(new ClothingItem("Fisherman's Vest", "Body", 550, "Practical and stylish", new Color(0.4f, 0.5f, 0.3f)));
        clothingItems.Add(new ClothingItem("Striped Sweater", "Body", 350, "Classic maritime look", new Color(0.9f, 0.9f, 0.95f)));

        // Legs items
        clothingItems.Add(new ClothingItem("Cargo Shorts", "Legs", 200, "Plenty of pockets", new Color(0.6f, 0.5f, 0.4f)));
        clothingItems.Add(new ClothingItem("Sailor Pants", "Legs", 350, "Bell-bottom style", new Color(0.2f, 0.25f, 0.4f)));
        clothingItems.Add(new ClothingItem("Waders", "Legs", 600, "Stay dry while fishing", new Color(0.3f, 0.4f, 0.3f)));

        // Feet items
        clothingItems.Add(new ClothingItem("Sandals", "Feet", 100, "Feel the breeze", new Color(0.6f, 0.4f, 0.2f)));
        clothingItems.Add(new ClothingItem("Rubber Boots", "Feet", 250, "Waterproof protection", new Color(0.8f, 0.7f, 0.2f)));
        clothingItems.Add(new ClothingItem("Deck Shoes", "Feet", 400, "Non-slip grip", new Color(0.4f, 0.3f, 0.2f)));

        // Gloves
        clothingItems.Add(new ClothingItem("Fishing Gloves", "Gloves", 200, "Protect your hands", new Color(0.3f, 0.3f, 0.35f)));
        clothingItems.Add(new ClothingItem("Fingerless Gloves", "Gloves", 150, "Grip without losing feel", new Color(0.2f, 0.2f, 0.25f)));

        // Accessories
        clothingItems.Add(new ClothingItem("Sunglasses", "Accessory", 300, "Look cool, see clearly", new Color(0.1f, 0.1f, 0.1f)));
        clothingItems.Add(new ClothingItem("Anchor Necklace", "Accessory", 450, "Lucky charm", new Color(0.7f, 0.7f, 0.8f)));
        clothingItems.Add(new ClothingItem("Compass Watch", "Accessory", 800, "Never get lost", new Color(0.8f, 0.6f, 0.3f)));
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check distance to player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerNearby = distance < interactionDistance;

            // Show interaction prompt
            if (playerNearby && !shopOpen && Input.GetKeyDown(KeyCode.E))
            {
                OpenShop();
            }
        }

        // Close shop with ESC
        if (shopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }

        // Animate knitting and rocking
        AnimateNPC();
    }

    void AnimateNPC()
    {
        knittingTime += Time.deltaTime;

        // Rock the chair
        if (rockingChair != null)
        {
            float rockAngle = Mathf.Sin(knittingTime * 1.5f) * 8f;
            rockingChair.localRotation = Quaternion.Euler(rockAngle, 0, 0);
        }

        // Knitting hands motion
        if (leftHand != null && rightHand != null)
        {
            float knit = Mathf.Sin(knittingTime * 4f) * 0.05f;
            leftHand.localPosition = new Vector3(-0.15f, 0.3f, 0.2f + knit);
            rightHand.localPosition = new Vector3(0.15f, 0.3f, 0.2f - knit);
        }
    }

    void OpenShop()
    {
        shopOpen = true;
        // Could pause game or disable player movement here
    }

    void CloseShop()
    {
        shopOpen = false;
    }

    void OnGUI()
    {
        if (!initialized || !MainMenu.GameStarted) return;

        // Show interaction prompt when nearby
        if (playerNearby && !shopOpen)
        {
            DrawInteractionPrompt();
        }

        // Draw shop UI
        if (shopOpen)
        {
            DrawShopUI();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
        promptStyle.fontSize = 18;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.alignment = TextAnchor.MiddleCenter;
        promptStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);

        float promptY = Screen.height * 0.7f;
        GUI.Label(new Rect(0, promptY, Screen.width, 30), "Press E to browse clothes", promptStyle);

        promptStyle.fontSize = 14;
        promptStyle.normal.textColor = new Color(0.8f, 0.7f, 0.5f);
        GUI.Label(new Rect(0, promptY + 25, Screen.width, 25), "~ Granny's Boutique ~", promptStyle);
    }

    void DrawShopUI()
    {
        // Darken background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("slotBg"));

        float panelWidth = 700;
        float panelHeight = 520;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Panel border and background
        GUI.DrawTexture(new Rect(panelX - 4, panelY - 4, panelWidth + 8, panelHeight + 8), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 26;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.9f, 0.7f);
        GUI.Label(new Rect(panelX, panelY + 12, panelWidth, 35), "GRANNY'S BOUTIQUE", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle(GUI.skin.label);
        subStyle.fontSize = 13;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.7f, 0.6f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + 42, panelWidth, 20), "\"Finest threads this side of the sea!\"", subStyle);

        // Close button
        if (DrawCloseButton(new Rect(panelX + panelWidth - 38, panelY + 10, 28, 28)))
        {
            CloseShop();
        }

        // Divider
        GUI.DrawTexture(new Rect(panelX + 20, panelY + 65, panelWidth - 40, 2), GetTexture("divider"));

        // Left side - Equipment slots
        float slotsX = panelX + 20;
        float slotsY = panelY + 80;
        float slotWidth = 180;
        float slotHeight = 65;

        GUIStyle slotHeader = new GUIStyle(GUI.skin.label);
        slotHeader.fontSize = 14;
        slotHeader.fontStyle = FontStyle.Bold;
        slotHeader.normal.textColor = new Color(0.9f, 0.8f, 0.6f);
        GUI.Label(new Rect(slotsX, slotsY, slotWidth, 22), "YOUR OUTFIT", slotHeader);
        slotsY += 28;

        for (int i = 0; i < slotNames.Length; i++)
        {
            bool isSelected = selectedSlot == i;
            Rect slotRect = new Rect(slotsX, slotsY + i * (slotHeight + 5), slotWidth, slotHeight);

            // Slot background
            GUI.DrawTexture(slotRect, isSelected ? GetTexture("slotSelected") : GetTexture("slotBg"));

            // Slot name
            GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize = 12;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.normal.textColor = new Color(0.7f, 0.6f, 0.5f);
            GUI.Label(new Rect(slotRect.x + 10, slotRect.y + 5, 100, 18), slotNames[i], nameStyle);

            // Equipped item
            string equipped = playerEquipment[slotNames[i]];
            GUIStyle itemNameStyle = new GUIStyle(GUI.skin.label);
            itemNameStyle.fontSize = 14;
            itemNameStyle.normal.textColor = equipped == "None" ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.9f, 0.85f, 0.7f);
            GUI.Label(new Rect(slotRect.x + 10, slotRect.y + 25, slotWidth - 20, 20), equipped, itemNameStyle);

            // Color swatch for equipped item
            if (equipped != "None")
            {
                ClothingItem equippedItem = clothingItems.Find(x => x.name == equipped);
                if (equippedItem != null)
                {
                    Texture2D swatchTex = GetOrCreateColorTexture(equippedItem.previewColor);
                    GUI.DrawTexture(new Rect(slotRect.x + slotWidth - 35, slotRect.y + 20, 25, 25), swatchTex);
                }
            }

            // Click to select
            if (GUI.Button(slotRect, "", GUIStyle.none))
            {
                selectedSlot = i;
            }
        }

        // Right side - Items for sale in selected slot
        float itemsX = panelX + 220;
        float itemsY = panelY + 80;
        float itemsWidth = panelWidth - 240;

        GUIStyle itemsHeader = new GUIStyle(GUI.skin.label);
        itemsHeader.fontSize = 14;
        itemsHeader.fontStyle = FontStyle.Bold;
        itemsHeader.normal.textColor = new Color(0.9f, 0.8f, 0.6f);
        GUI.Label(new Rect(itemsX, itemsY, itemsWidth, 22), slotNames[selectedSlot] + " - AVAILABLE", itemsHeader);

        // Player's gold
        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUIStyle goldStyle = new GUIStyle(GUI.skin.label);
        goldStyle.fontSize = 14;
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.alignment = TextAnchor.MiddleRight;
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(itemsX, itemsY, itemsWidth - 10, 22), "Gold: " + playerGold, goldStyle);

        itemsY += 30;

        // Filter items by selected slot
        List<ClothingItem> slotItems = clothingItems.FindAll(x => x.slot == slotNames[selectedSlot]);

        float itemHeight = 55;
        for (int i = 0; i < slotItems.Count; i++)
        {
            ClothingItem item = slotItems[i];
            Rect itemRect = new Rect(itemsX, itemsY + i * (itemHeight + 5), itemsWidth - 10, itemHeight);

            bool hover = itemRect.Contains(Event.current.mousePosition);
            bool isEquipped = playerEquipment[item.slot] == item.name;
            bool canAfford = playerGold >= item.price;

            // Item background
            GUI.DrawTexture(itemRect, hover ? GetTexture("itemHover") : GetTexture("itemBg"));

            // Equipped indicator
            if (isEquipped)
            {
                GUI.DrawTexture(new Rect(itemRect.x, itemRect.y, 4, itemRect.height), GetTexture("equipped"));
            }

            // Color swatch
            Texture2D itemSwatch = GetOrCreateColorTexture(item.previewColor);
            GUI.DrawTexture(new Rect(itemRect.x + 10, itemRect.y + 10, 35, 35), itemSwatch);

            // Item name
            GUIStyle itemTitleStyle = new GUIStyle(GUI.skin.label);
            itemTitleStyle.fontSize = 14;
            itemTitleStyle.fontStyle = FontStyle.Bold;
            itemTitleStyle.normal.textColor = isEquipped ? new Color(0.5f, 0.8f, 0.5f) : Color.white;
            GUI.Label(new Rect(itemRect.x + 55, itemRect.y + 5, 200, 20), item.name + (isEquipped ? " [EQUIPPED]" : ""), itemTitleStyle);

            // Description
            GUIStyle descStyle = new GUIStyle(GUI.skin.label);
            descStyle.fontSize = 11;
            descStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(itemRect.x + 55, itemRect.y + 25, 200, 18), item.description, descStyle);

            // Price
            GUIStyle priceStyle = new GUIStyle(GUI.skin.label);
            priceStyle.fontSize = 13;
            priceStyle.fontStyle = FontStyle.Bold;
            priceStyle.alignment = TextAnchor.MiddleRight;
            priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.2f) : new Color(0.8f, 0.3f, 0.3f);
            GUI.Label(new Rect(itemRect.x + 55, itemRect.y + 28, 180, 20), item.price + "g", priceStyle);

            // Buy/Equip button
            if (!isEquipped)
            {
                Rect btnRect = new Rect(itemRect.x + itemRect.width - 70, itemRect.y + 12, 60, 30);
                bool owned = IsItemOwned(item.name);

                if (owned)
                {
                    GUI.DrawTexture(btnRect, GetTexture("buttonEquip"));
                    GUIStyle btnStyle = new GUIStyle(GUI.skin.label);
                    btnStyle.fontSize = 11;
                    btnStyle.fontStyle = FontStyle.Bold;
                    btnStyle.alignment = TextAnchor.MiddleCenter;
                    btnStyle.normal.textColor = Color.white;
                    GUI.Label(btnRect, "EQUIP", btnStyle);

                    if (GUI.Button(btnRect, "", GUIStyle.none))
                    {
                        EquipItem(item);
                    }
                }
                else if (canAfford)
                {
                    GUI.DrawTexture(btnRect, GetTexture("buttonBuy"));
                    GUIStyle btnStyle = new GUIStyle(GUI.skin.label);
                    btnStyle.fontSize = 11;
                    btnStyle.fontStyle = FontStyle.Bold;
                    btnStyle.alignment = TextAnchor.MiddleCenter;
                    btnStyle.normal.textColor = Color.white;
                    GUI.Label(btnRect, "BUY", btnStyle);

                    if (GUI.Button(btnRect, "", GUIStyle.none))
                    {
                        BuyItem(item);
                    }
                }
            }
        }

        // Hint at bottom
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 12;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 30, panelWidth, 25), "Press ESC to close", hintStyle);
    }

    bool DrawCloseButton(Rect rect)
    {
        bool hover = rect.Contains(Event.current.mousePosition);

        GUI.color = hover ? new Color(0.9f, 0.3f, 0.2f) : new Color(0.7f, 0.4f, 0.3f);
        GUI.DrawTexture(rect, GetTexture("white"));
        GUI.color = Color.white;

        GUIStyle xStyle = new GUIStyle(GUI.skin.label);
        xStyle.fontSize = 16;
        xStyle.fontStyle = FontStyle.Bold;
        xStyle.alignment = TextAnchor.MiddleCenter;
        xStyle.normal.textColor = Color.white;
        GUI.Label(rect, "X", xStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    // Track owned items (simplified - in real game would persist)
    private List<string> ownedItems = new List<string>() { "Blue Shirt", "Brown Pants", "Leather Boots" };

    bool IsItemOwned(string itemName)
    {
        return ownedItems.Contains(itemName);
    }

    void BuyItem(ClothingItem item)
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.GetCoins() >= item.price)
            {
                GameManager.Instance.AddCoins(-item.price);
                ownedItems.Add(item.name);
                EquipItem(item);
                Debug.Log($"Bought {item.name} for {item.price}g");
            }
        }
    }

    void EquipItem(ClothingItem item)
    {
        playerEquipment[item.slot] = item.name;

        // Update character panel if it exists
        if (CharacterPanel.Instance != null)
        {
            int slotIndex = System.Array.IndexOf(slotNames, item.slot);
            if (slotIndex >= 0)
            {
                CharacterPanel.Instance.SetEquipment(slotIndex, item.name);
            }
        }

        Debug.Log($"Equipped {item.name}");
    }

    Texture2D GetOrCreateColorTexture(Color color)
    {
        string key = $"color_{color.r:F2}_{color.g:F2}_{color.b:F2}_{color.a:F2}";

        if (textureCache.TryGetValue(key, out Texture2D cached))
        {
            return cached;
        }

        Texture2D tex = new Texture2D(2, 2);
        Color[] pixels = new Color[4];
        for (int i = 0; i < 4; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        textureCache[key] = tex;
        return tex;
    }

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}

[System.Serializable]
public class ClothingItem
{
    public string name;
    public string slot;
    public int price;
    public string description;
    public Color previewColor;

    public ClothingItem(string name, string slot, int price, string description, Color color)
    {
        this.name = name;
        this.slot = slot;
        this.price = price;
        this.description = description;
        this.previewColor = color;
    }
}
