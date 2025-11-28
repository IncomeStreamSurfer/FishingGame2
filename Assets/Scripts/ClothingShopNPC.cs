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
    private string[] slotNames = { "Head", "Top", "Legs", "Accessory" };

    // Player's current equipment - starts naked!
    private Dictionary<string, string> playerEquipment = new Dictionary<string, string>()
    {
        { "Head", "None" },
        { "Top", "None" },
        { "Legs", "None" },
        { "Accessory", "None" }
    };

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;

    // Audio for voice
    private AudioSource voiceSource;

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
        SetupAudio();
        initialized = true;
    }

    void SetupAudio()
    {
        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.spatialBlend = 1f;  // 3D sound
        voiceSource.minDistance = 2f;
        voiceSource.maxDistance = 15f;
        voiceSource.volume = 0.5f;
        voiceSource.playOnAwake = false;
    }

    void PlayHelloSound()
    {
        StartCoroutine(GenerateHelloSound());
    }

    System.Collections.IEnumerator GenerateHelloSound()
    {
        // "Ohh helloooooo" - elderly woman voice
        int sampleRate = 44100;
        float duration = 1.2f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip helloClip = AudioClip.Create("HelloSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            float sample = 0f;

            // "Ohh" (0 - 0.3)
            if (progress < 0.25f)
            {
                float ohProgress = progress / 0.25f;
                // Rising "oh" sound
                float freq = Mathf.Lerp(280f, 350f, ohProgress);  // Higher pitch for elderly woman
                sample = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.3f;
                sample += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.15f;  // Harmonics
                sample += Mathf.Sin(2 * Mathf.PI * freq * 3f * t) * 0.08f;
                sample *= Mathf.Sin(ohProgress * Mathf.PI);  // Envelope
            }
            // "hell" (0.25 - 0.45)
            else if (progress < 0.4f)
            {
                float hellProgress = (progress - 0.25f) / 0.15f;
                float freq = Mathf.Lerp(380f, 420f, hellProgress);
                sample = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.35f;
                sample += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.15f;
                // Add some breathiness
                sample += (Random.value * 2f - 1f) * 0.05f * (1f - hellProgress);
                sample *= Mathf.Lerp(0.8f, 1f, Mathf.Sin(hellProgress * Mathf.PI));
            }
            // "oooooo" (0.4 - 1.0) - long drawn out
            else
            {
                float oooProgress = (progress - 0.4f) / 0.6f;
                // Wavering pitch like an old lady
                float waver = Mathf.Sin(t * 8f) * 15f + Mathf.Sin(t * 12f) * 8f;
                float freq = 350f + waver;

                sample = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.3f;
                sample += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.12f;
                sample += Mathf.Sin(2 * Mathf.PI * freq * 3f * t) * 0.06f;

                // Add slight tremolo (elderly voice characteristic)
                float tremolo = 1f + Mathf.Sin(t * 6f) * 0.1f;
                sample *= tremolo;

                // Fade out
                sample *= 1f - (oooProgress * 0.7f);
            }

            // Overall warmth (low pass simulation)
            samples[i] = sample * 0.7f;
        }

        helloClip.SetData(samples, 0);
        voiceSource.clip = helloClip;
        voiceSource.pitch = Random.Range(1.05f, 1.15f);  // Slightly higher for elderly woman
        voiceSource.Play();

        yield return null;
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
        // === HEAD ITEMS ===
        clothingItems.Add(new ClothingItem("Straw Hat", "Head", 100, "A breezy hat for sunny days", new Color(0.9f, 0.8f, 0.5f)));
        clothingItems.Add(new ClothingItem("Baseball Cap", "Head", 100, "Classic red baseball cap", new Color(0.85f, 0.15f, 0.1f)));
        clothingItems.Add(new ClothingItem("Fancy Top Hat", "Head", 250, "Dapper gentleman style", new Color(0.1f, 0.1f, 0.1f)));

        // === TOP ITEMS (shirts, t-shirts) ===
        clothingItems.Add(new ClothingItem("Coconut Bra", "Top", 10, "Tropical island fashion", new Color(0.55f, 0.35f, 0.2f)));
        clothingItems.Add(new ClothingItem("Red T-Shirt", "Top", 50, "Simple red t-shirt", new Color(0.85f, 0.15f, 0.1f)));
        clothingItems.Add(new ClothingItem("Blue Shirt", "Top", 50, "Casual blue shirt", new Color(0.15f, 0.35f, 0.65f)));
        clothingItems.Add(new ClothingItem("Lumberjack Shirt", "Top", 500, "Red and black checkered", new Color(0.8f, 0.1f, 0.1f)));

        // === LEGS ITEMS (pants, trousers) ===
        clothingItems.Add(new ClothingItem("Red Pants", "Legs", 40, "Bold red trousers", new Color(0.8f, 0.15f, 0.1f)));
        clothingItems.Add(new ClothingItem("Green Pants", "Legs", 40, "Forest green trousers", new Color(0.2f, 0.5f, 0.2f)));
        clothingItems.Add(new ClothingItem("Black Pants", "Legs", 40, "Classic black trousers", new Color(0.12f, 0.12f, 0.12f)));
        clothingItems.Add(new ClothingItem("Blue Jeans", "Legs", 150, "Classic denim jeans", new Color(0.2f, 0.35f, 0.6f)));

        // === ACCESSORIES ===
        clothingItems.Add(new ClothingItem("Pimp Cane", "Accessory", 1000, "Walk with style!", new Color(0.85f, 0.7f, 0.2f)));
        clothingItems.Add(new ClothingItem("Shoulder Parrot", "Accessory", 200000, "A loyal feathered friend!", new Color(0.2f, 0.8f, 0.3f)));

        // === CONSUMABLES ===
        clothingItems.Add(new ClothingItem("Lunch Box", "Consumable", 50, "Store fish! Use for 10 mins max HP!", new Color(0.7f, 0.4f, 0.2f)));

        // === SPECIAL: OUTFIT SETS ===
        // Fancy Tuxedo fills BOTH Top and Legs slots
        clothingItems.Add(new ClothingItem("Fancy Tuxedo", "Top", 2500, "Elegant black suit + white shirt", new Color(0.1f, 0.1f, 0.1f), true));
        clothingItems.Add(new ClothingItem("Fancy Tuxedo", "Legs", 0, "Part of tuxedo set", new Color(0.1f, 0.1f, 0.1f), true, true));
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
        // Play the old lady's greeting
        PlayHelloSound();
        // Enable fish selling
        if (FishInventoryPanel.Instance != null)
        {
            FishInventoryPanel.Instance.EnableSellMode("Granny");
        }
    }

    void CloseShop()
    {
        shopOpen = false;
        // Disable fish selling
        if (FishInventoryPanel.Instance != null)
        {
            FishInventoryPanel.Instance.DisableSellMode();
        }
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

    // Scroll position for clothing items
    private float clothingScrollPosition = 0f;

    void DrawShopUI()
    {
        // Darken background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("slotBg"));

        float panelWidth = 520;
        float panelHeight = 380;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Panel border and background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Red X close button
        GUIStyle xButtonStyle = new GUIStyle();
        xButtonStyle.fontSize = 16;
        xButtonStyle.fontStyle = FontStyle.Bold;
        xButtonStyle.alignment = TextAnchor.MiddleCenter;
        xButtonStyle.normal.textColor = Color.white;
        GUI.DrawTexture(new Rect(panelX + panelWidth - 32, panelY + 8, 24, 24), GetOrCreateColorTexture(new Color(0.8f, 0.2f, 0.2f)));
        if (GUI.Button(new Rect(panelX + panelWidth - 32, panelY + 8, 24, 24), "X", xButtonStyle))
        {
            CloseShop();
        }

        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 18;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.9f, 0.7f);
        GUI.Label(new Rect(panelX, panelY + 8, panelWidth, 24), "GRANNY'S BOUTIQUE", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle(GUI.skin.label);
        subStyle.fontSize = 9;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.7f, 0.6f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + 30, panelWidth, 14), "\"Finest threads this side of the sea!\"", subStyle);

        // Close button
        if (DrawCloseButton(new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22)))
        {
            CloseShop();
        }

        // Divider
        GUI.DrawTexture(new Rect(panelX + 15, panelY + 48, panelWidth - 30, 2), GetTexture("divider"));

        // Left side - Equipment slots
        float slotsX = panelX + 15;
        float slotsY = panelY + 58;
        float slotWidth = 140;
        float slotHeight = 48;

        GUIStyle slotHeader = new GUIStyle(GUI.skin.label);
        slotHeader.fontSize = 11;
        slotHeader.fontStyle = FontStyle.Bold;
        slotHeader.normal.textColor = new Color(0.9f, 0.8f, 0.6f);
        GUI.Label(new Rect(slotsX, slotsY, slotWidth, 16), "YOUR OUTFIT", slotHeader);
        slotsY += 20;

        for (int i = 0; i < slotNames.Length; i++)
        {
            bool isSelected = selectedSlot == i;
            Rect slotRect = new Rect(slotsX, slotsY + i * (slotHeight + 3), slotWidth, slotHeight);

            // Slot background
            GUI.DrawTexture(slotRect, isSelected ? GetTexture("slotSelected") : GetTexture("slotBg"));

            // Slot name
            GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize = 9;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.normal.textColor = new Color(0.7f, 0.6f, 0.5f);
            GUI.Label(new Rect(slotRect.x + 6, slotRect.y + 3, 80, 14), slotNames[i], nameStyle);

            // Equipped item
            string equipped = playerEquipment[slotNames[i]];
            GUIStyle itemNameStyle = new GUIStyle(GUI.skin.label);
            itemNameStyle.fontSize = 10;
            itemNameStyle.normal.textColor = equipped == "None" ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.9f, 0.85f, 0.7f);
            GUI.Label(new Rect(slotRect.x + 6, slotRect.y + 18, slotWidth - 30, 16), equipped, itemNameStyle);

            // Color swatch for equipped item
            if (equipped != "None")
            {
                ClothingItem equippedItem = clothingItems.Find(x => x.name == equipped);
                if (equippedItem != null)
                {
                    Texture2D swatchTex = GetOrCreateColorTexture(equippedItem.previewColor);
                    GUI.DrawTexture(new Rect(slotRect.x + slotWidth - 24, slotRect.y + 14, 18, 18), swatchTex);
                }
            }

            // Click to select
            if (GUI.Button(slotRect, "", GUIStyle.none))
            {
                selectedSlot = i;
            }
        }

        // Right side - Items for sale in selected slot
        float itemsX = panelX + 165;
        float itemsY = panelY + 58;
        float itemsWidth = panelWidth - 180;

        GUIStyle itemsHeader = new GUIStyle(GUI.skin.label);
        itemsHeader.fontSize = 11;
        itemsHeader.fontStyle = FontStyle.Bold;
        itemsHeader.normal.textColor = new Color(0.9f, 0.8f, 0.6f);
        GUI.Label(new Rect(itemsX, itemsY, itemsWidth, 16), slotNames[selectedSlot] + " - AVAILABLE", itemsHeader);

        // Player's gold
        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUIStyle goldStyle = new GUIStyle(GUI.skin.label);
        goldStyle.fontSize = 10;
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.alignment = TextAnchor.MiddleRight;
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(itemsX, itemsY, itemsWidth - 8, 16), "Gold: " + playerGold, goldStyle);

        itemsY += 20;

        // Filter items by selected slot (exclude hidden items like tuxedo pants)
        List<ClothingItem> slotItems = clothingItems.FindAll(x => x.slot == slotNames[selectedSlot] && !x.isHiddenInShop);

        // Scrollable area
        float itemHeight = 42;
        float visibleHeight = panelHeight - 100;
        float totalHeight = slotItems.Count * (itemHeight + 3);
        float maxScroll = Mathf.Max(0, totalHeight - visibleHeight);

        // Handle mouse wheel scrolling
        Rect scrollArea = new Rect(itemsX, itemsY, itemsWidth, visibleHeight);
        if (scrollArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                clothingScrollPosition += Event.current.delta.y * 20f;
                clothingScrollPosition = Mathf.Clamp(clothingScrollPosition, 0, maxScroll);
                Event.current.Use();
            }
        }

        // Begin clip area
        GUI.BeginGroup(scrollArea);

        float itemY = -clothingScrollPosition;
        for (int i = 0; i < slotItems.Count; i++)
        {
            if (itemY + itemHeight < 0 || itemY > visibleHeight)
            {
                itemY += itemHeight + 3;
                continue;
            }

            ClothingItem item = slotItems[i];
            Rect itemRect = new Rect(0, itemY, itemsWidth - 8, itemHeight);

            bool hover = new Rect(itemsX, itemsY + itemY, itemsWidth - 8, itemHeight).Contains(Event.current.mousePosition);
            bool isEquipped = playerEquipment[item.slot] == item.name;
            bool canAfford = playerGold >= item.price;

            // Item background
            GUI.DrawTexture(itemRect, hover ? GetTexture("itemHover") : GetTexture("itemBg"));

            // Equipped indicator
            if (isEquipped)
            {
                GUI.DrawTexture(new Rect(itemRect.x, itemRect.y, 3, itemRect.height), GetTexture("equipped"));
            }

            // Color swatch
            Texture2D itemSwatch = GetOrCreateColorTexture(item.previewColor);
            GUI.DrawTexture(new Rect(itemRect.x + 6, itemRect.y + 6, 28, 28), itemSwatch);

            // Item name
            GUIStyle itemTitleStyle = new GUIStyle(GUI.skin.label);
            itemTitleStyle.fontSize = 10;
            itemTitleStyle.fontStyle = FontStyle.Bold;
            itemTitleStyle.normal.textColor = isEquipped ? new Color(0.5f, 0.8f, 0.5f) : Color.white;
            string displayName = item.name + (isEquipped ? " [E]" : "");
            GUI.Label(new Rect(itemRect.x + 40, itemRect.y + 3, 150, 14), displayName, itemTitleStyle);

            // Description
            GUIStyle descStyle = new GUIStyle(GUI.skin.label);
            descStyle.fontSize = 8;
            descStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(itemRect.x + 40, itemRect.y + 17, 140, 14), item.description, descStyle);

            // Price
            GUIStyle priceStyle = new GUIStyle(GUI.skin.label);
            priceStyle.fontSize = 9;
            priceStyle.fontStyle = FontStyle.Bold;
            priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.2f) : new Color(0.8f, 0.3f, 0.3f);
            GUI.Label(new Rect(itemRect.x + 40, itemRect.y + 28, 60, 12), item.price + "g", priceStyle);

            // Buy/Equip button
            if (!isEquipped)
            {
                Rect btnRect = new Rect(itemRect.x + itemRect.width - 48, itemRect.y + 10, 42, 22);
                bool owned = IsItemOwned(item.name);

                if (owned)
                {
                    GUI.DrawTexture(btnRect, GetTexture("buttonEquip"));
                    GUIStyle btnStyle = new GUIStyle(GUI.skin.label);
                    btnStyle.fontSize = 9;
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
                    btnStyle.fontSize = 9;
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

            itemY += itemHeight + 3;
        }

        GUI.EndGroup();

        // Draw scroll indicator if needed
        if (maxScroll > 0)
        {
            float scrollBarHeight = visibleHeight * (visibleHeight / totalHeight);
            float scrollBarY = (clothingScrollPosition / maxScroll) * (visibleHeight - scrollBarHeight);
            GUI.DrawTexture(new Rect(itemsX + itemsWidth - 5, itemsY + scrollBarY, 3, scrollBarHeight),
                GetOrCreateColorTexture(new Color(0.5f, 0.4f, 0.3f, 0.7f)));
        }

        // Hint at bottom
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 9;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 20, panelWidth, 18), "Press ESC to close | Scroll to see more", hintStyle);
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
    // Player starts naked - no owned items!
    private List<string> ownedItems = new List<string>();

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

                // Special handling for consumables (Lunch Box)
                if (item.slot == "Consumable")
                {
                    if (item.name == "Lunch Box")
                    {
                        if (FoodInventory.Instance != null)
                        {
                            FoodInventory.Instance.AddLunchBox();
                            if (UIManager.Instance != null)
                            {
                                UIManager.Instance.ShowLootNotification("Lunch Box purchased! Press L to open.", new Color(0.7f, 0.4f, 0.2f));
                            }
                        }
                    }
                    Debug.Log($"Bought consumable: {item.name}");
                    return; // Don't add to wardrobe
                }

                ownedItems.Add(item.name);

                // Add to wardrobe system
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.AddToWardrobe(item.name, item.slot, item.previewColor);

                    // If it's a set item, add both pieces to wardrobe
                    if (item.isSetItem)
                    {
                        List<ClothingItem> setPieces = clothingItems.FindAll(x => x.name == item.name);
                        foreach (ClothingItem piece in setPieces)
                        {
                            if (piece != item) // Don't add the same piece twice
                            {
                                UIManager.Instance.AddToWardrobe(piece.name, piece.slot, piece.previewColor);
                            }
                        }
                    }
                }

                // If this is a set item (like tuxedo), equip all pieces
                if (item.isSetItem)
                {
                    EquipSetItem(item.name);
                }
                else
                {
                    EquipItem(item);
                }
                Debug.Log($"Bought {item.name} for {item.price}g");
            }
        }
    }

    void EquipSetItem(string itemName)
    {
        // Find all items with this name (multiple slots) and equip them all
        List<ClothingItem> setPieces = clothingItems.FindAll(x => x.name == itemName);
        foreach (ClothingItem piece in setPieces)
        {
            playerEquipment[piece.slot] = piece.name;

            // Update character panel
            if (CharacterPanel.Instance != null)
            {
                int slotIndex = System.Array.IndexOf(slotNames, piece.slot);
                if (slotIndex >= 0)
                {
                    CharacterPanel.Instance.SetEquipment(slotIndex, piece.name);
                }
            }

            // Update player visuals
            if (PlayerClothingVisuals.Instance != null)
            {
                PlayerClothingVisuals.Instance.EquipClothing(piece.slot, piece.name, piece.previewColor);
            }
        }
        Debug.Log($"Equipped full set: {itemName}");
    }

    void EquipItem(ClothingItem item)
    {
        // If equipping a set item, equip all pieces
        if (item.isSetItem)
        {
            EquipSetItem(item.name);
            return;
        }

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

        // Update player visual model with clothing
        if (PlayerClothingVisuals.Instance != null)
        {
            PlayerClothingVisuals.Instance.EquipClothing(item.slot, item.name, item.previewColor);
        }

        // Special handling for Shoulder Parrot
        if (item.name == "Shoulder Parrot")
        {
            if (ShoulderParrot.Instance != null)
            {
                ShoulderParrot.Instance.EquipParrot();
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

    public bool IsShopOpen() => shopOpen;
}

[System.Serializable]
public class ClothingItem
{
    public string name;
    public string slot;
    public int price;
    public string description;
    public Color previewColor;
    public bool isSetItem;      // Part of a multi-slot outfit
    public bool isHiddenInShop; // Hidden companion piece (like tuxedo pants)

    public ClothingItem(string name, string slot, int price, string description, Color color, bool isSet = false, bool hidden = false)
    {
        this.name = name;
        this.slot = slot;
        this.price = price;
        this.description = description;
        this.previewColor = color;
        this.isSetItem = isSet;
        this.isHiddenInShop = hidden;
    }
}
