using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // UI State
    private bool inventoryOpen = false;
    private bool shopOpen = false;
    private int selectedRodIndex = 0;
    private int currentTab = 0; // 0=Equipment, 1=Quests, 2=Buffs, 3=Wardrobe, 4=Melee, 5=Scores, 6=Achievements

    // Wardrobe data - tracks owned clothing items
    private List<WardrobeItem> ownedClothing = new List<WardrobeItem>();
    private float wardrobeScrollPos = 0f;
    private float scoresScrollPos = 0f;

    private List<HighscoreEntry> highscores = new List<HighscoreEntry>();

    // Rod data - Basic (brown), Bronze (bronze metallic), Silver (silver metallic), Golden (golden glow), Legendary (purple glow), Epic (yellow smoke/glow)
    private string[] rodNames = { "Basic Rod", "Bronze Rod", "Silver Rod", "Golden Rod", "Legendary Rod", "Epic Rod" };
    private Color[] rodColors = {
        new Color(0.45f, 0.30f, 0.15f),   // Basic - brown wood
        new Color(0.80f, 0.50f, 0.20f),   // Bronze - bronze metallic
        new Color(0.85f, 0.85f, 0.90f),   // Silver - silver metallic
        new Color(1f, 0.85f, 0.30f),      // Golden - golden
        new Color(0.70f, 0.30f, 1f),      // Legendary - purple glow
        new Color(1f, 0.95f, 0.20f)       // Epic - bright yellow
    };
    private float[] rodMetallic = { 0.1f, 0.7f, 0.85f, 0.9f, 0.6f, 0.95f };  // Metallic sheen per rod
    private float[] rodGlossiness = { 0.3f, 0.6f, 0.8f, 0.85f, 0.7f, 0.9f }; // Glossiness per rod
    private bool[] rodHasGlow = { false, false, false, true, true, true };   // Whether rod glows
    private bool[] rodHasSmoke = { false, false, false, false, false, true }; // Whether rod has smoke effect (Epic only)
    private bool[] rodsUnlocked = { true, false, false, false, false, false };

    // Shop items
    private ShopItem[] shopItems;

    // Styling
    private GUIStyle frameStyle;
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle tabStyle;
    private GUIStyle tabActiveStyle;
    private Texture2D frameTex;
    private Texture2D buttonTex;
    private Texture2D buttonHoverTex;
    private Texture2D tabTex;
    private Texture2D tabActiveTex;
    private bool stylesInitialized = false;

    // Cached textures to avoid creating new ones every frame
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;

    // Level up notification
    private float levelUpNotificationTime = 0f;
    private int levelUpFrom = 0;
    private int levelUpTo = 0;

    // Loot notification
    private float lootNotificationTime = 0f;
    private string lootNotificationText = "";
    private Color lootNotificationColor = Color.white;

    // Rod unlock notification
    private float rodUnlockNotificationTime = 0f;

    // NPC Dialog
    private bool npcDialogOpen = false;
    private string currentNPCName = "";

    // Quest tracker visibility
    private bool questTrackerHidden = false;

    // Draggable/Resizable Quest Tracker window
    private DraggableWindow questTrackerWindow;

    // Close button style
    private GUIStyle closeButtonStyle;

    // Rod/Weapon dropdown panel
    private bool rodDropdownOpen = false;
    private Rect rodSlotRect; // Cached rod slot position for click detection
    private float equipmentScrollPos = 0f;
    private int equipmentTab = 0; // 0 = RODS, 1 = WEAPONS

    // Cached styles for OnGUI (avoid creating new ones every frame)
    private GUIStyle cachedKeyStyle;
    private GUIStyle cachedPromptTextStyle;
    private GUIStyle cachedSlotLabelStyle;
    private GUIStyle cachedCoinStyle;
    private GUIStyle cachedFishStyle;
    private GUIStyle cachedLvlStyle;
    private GUIStyle cachedXpStyle;

    // NPC proximity for sell prompt
    private bool isNearNPC = false;
    private string nearbyNPCName = "";
    private float npcCheckTimer = 0f;
    private const float NPC_CHECK_INTERVAL = 0.2f;
    private const float NPC_SELL_RANGE = 4f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeShopItems();
        }
        else
        {
            Destroy(gameObject);
        }

        // Highscores now only show player's own stats - no example entries
    }

    void Start()
    {
        // Subscribe to level up events
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.OnLevelUp += OnLevelUp;
        }

        // Delay initialization to avoid texture creation issues
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        InitStyles();

        // Initialize draggable quest tracker window (top right corner)
        float questWidth = 220;
        float questHeight = 80;
        float questX = Screen.width - questWidth - 10;
        float questY = 160;
        questTrackerWindow = new DraggableWindow(
            new Rect(questX, questY, questWidth, questHeight),
            new Vector2(180, 60),   // Min size
            new Vector2(350, 150)   // Max size
        );

        initialized = true;
    }

    void InitializeShopItems()
    {
        // Note: Clothing items moved to Granny's Boutique on the island
        shopItems = new ShopItem[]
        {
            new ShopItem("Bait Pack (10)", "Basic bait for fishing", 50, ShopItemType.Consumable),
            new ShopItem("Premium Bait (10)", "Attracts rare fish", 200, ShopItemType.Consumable),
            new ShopItem("Lucky Charm", "+5% rare fish chance for 10 min", 500, ShopItemType.Consumable),
            new ShopItem("Bronze Rod", "Slightly better rod", 100, ShopItemType.Rod),
            new ShopItem("Silver Rod", "Good quality rod", 500, ShopItemType.Rod),
            new ShopItem("Golden Rod", "Excellent rod", 2000, ShopItemType.Rod),
            new ShopItem("Legendary Rod", "The best rod money can buy", 10000, ShopItemType.Rod),
            new ShopItem("Epic Rod", "Glowing yellow masterpiece!", 100000, ShopItemType.Rod),
            new ShopItem("XP Boost (1hr)", "Double XP for 1 hour", 1000, ShopItemType.Consumable),
            new ShopItem("Fish Finder", "Shows fish locations for 5 min", 750, ShopItemType.Consumable),
            new ShopItem("Tackle Box", "Store more bait types", 1500, ShopItemType.Consumable),
        };
    }

    void InitStyles()
    {
        if (stylesInitialized) return;

        // CONSISTENT STYLE: Dark semi-transparent background
        frameTex = MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.12f, 0.95f));
        buttonTex = MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.22f, 0.95f));
        buttonHoverTex = MakeTexture(2, 2, new Color(0.3f, 0.3f, 0.32f, 0.95f));
        tabTex = MakeTexture(2, 2, new Color(0.15f, 0.15f, 0.17f, 0.95f));
        tabActiveTex = MakeTexture(2, 2, new Color(0.25f, 0.25f, 0.27f, 0.95f));

        frameStyle = new GUIStyle();
        frameStyle.normal.background = frameTex;
        frameStyle.padding = new RectOffset(6, 6, 6, 6);

        headerStyle = new GUIStyle();
        headerStyle.normal.background = MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.98f));
        headerStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold/amber
        headerStyle.fontSize = 14; // Smaller header
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.padding = new RectOffset(4, 4, 4, 4);

        labelStyle = new GUIStyle();
        labelStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f); // Light gray/cream
        labelStyle.fontSize = 10; // Smaller body text
        labelStyle.alignment = TextAnchor.MiddleLeft;

        buttonStyle = new GUIStyle();
        buttonStyle.normal.background = buttonTex;
        buttonStyle.hover.background = buttonHoverTex;
        buttonStyle.active.background = buttonHoverTex;
        buttonStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold text
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.fontSize = 10; // Smaller button text
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        buttonStyle.padding = new RectOffset(4, 4, 3, 3);

        tabStyle = new GUIStyle(buttonStyle);
        tabStyle.normal.background = tabTex;
        tabStyle.fontSize = 9; // Smaller tabs

        tabActiveStyle = new GUIStyle(buttonStyle);
        tabActiveStyle.normal.background = tabActiveTex;
        tabActiveStyle.normal.textColor = new Color(1f, 0.95f, 0.7f);
        tabActiveStyle.fontSize = 9; // Smaller tabs

        // Close button style (X button)
        Texture2D closeTex = MakeTexture(2, 2, new Color(0.6f, 0.15f, 0.1f, 0.9f));
        Texture2D closeHoverTex = MakeTexture(2, 2, new Color(0.8f, 0.2f, 0.15f, 0.95f));
        closeButtonStyle = new GUIStyle();
        closeButtonStyle.normal.background = closeTex;
        closeButtonStyle.hover.background = closeHoverTex;
        closeButtonStyle.active.background = closeHoverTex;
        closeButtonStyle.normal.textColor = Color.white;
        closeButtonStyle.hover.textColor = Color.white;
        closeButtonStyle.fontSize = 10; // Smaller X
        closeButtonStyle.fontStyle = FontStyle.Bold;
        closeButtonStyle.alignment = TextAnchor.MiddleCenter;

        // Initialize cached styles for OnGUI (avoid creating new ones every frame)
        cachedKeyStyle = new GUIStyle();
        cachedKeyStyle.fontSize = 16;
        cachedKeyStyle.fontStyle = FontStyle.Bold;
        cachedKeyStyle.alignment = TextAnchor.MiddleCenter;
        cachedKeyStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

        cachedPromptTextStyle = new GUIStyle();
        cachedPromptTextStyle.fontSize = 13;
        cachedPromptTextStyle.fontStyle = FontStyle.Bold;
        cachedPromptTextStyle.alignment = TextAnchor.MiddleLeft;
        cachedPromptTextStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);

        cachedSlotLabelStyle = new GUIStyle();
        cachedSlotLabelStyle.normal.textColor = new Color(0.6f, 0.55f, 0.4f);
        cachedSlotLabelStyle.fontSize = 9;
        cachedSlotLabelStyle.alignment = TextAnchor.MiddleCenter;

        cachedCoinStyle = new GUIStyle();
        cachedCoinStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        cachedCoinStyle.fontSize = 14;
        cachedCoinStyle.fontStyle = FontStyle.Bold;
        cachedCoinStyle.alignment = TextAnchor.MiddleCenter;

        cachedFishStyle = new GUIStyle();
        cachedFishStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);
        cachedFishStyle.fontSize = 14;
        cachedFishStyle.fontStyle = FontStyle.Bold;
        cachedFishStyle.alignment = TextAnchor.MiddleCenter;

        cachedLvlStyle = new GUIStyle();
        cachedLvlStyle.fontSize = 16;
        cachedLvlStyle.fontStyle = FontStyle.Bold;
        cachedLvlStyle.alignment = TextAnchor.MiddleCenter;

        cachedXpStyle = new GUIStyle();
        cachedXpStyle.normal.textColor = Color.white;
        cachedXpStyle.fontSize = 9;
        cachedXpStyle.alignment = TextAnchor.MiddleCenter;

        stylesInitialized = true;
    }

    Texture2D MakeTexture(int width, int height, Color color)
    {
        // Create a cache key from the color
        string key = $"{color.r:F2}_{color.g:F2}_{color.b:F2}_{color.a:F2}";

        if (textureCache.TryGetValue(key, out Texture2D cached))
        {
            return cached;
        }

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pixels);
        tex.Apply();

        textureCache[key] = tex;
        return tex;
    }

    void OnDestroy()
    {
        // Clean up cached textures
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryOpen = !inventoryOpen;
            shopOpen = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inventoryOpen = false;
            shopOpen = false;
            rodDropdownOpen = false;
        }

        // Rod unlocks - Level-based system
        if (LevelingSystem.Instance != null)
        {
            int currentLevel = LevelingSystem.Instance.GetLevel();

            // Check each rod and trigger notification if newly unlocked
            // Bronze Rod: Level 25
            if (currentLevel >= 25 && !rodsUnlocked[1]) { rodsUnlocked[1] = true; ShowRodUnlockNotification(); }
            // Silver Rod: Level 55
            if (currentLevel >= 55 && !rodsUnlocked[2]) { rodsUnlocked[2] = true; ShowRodUnlockNotification(); }
            // Golden Rod: Level 100
            if (currentLevel >= 100 && !rodsUnlocked[3]) { rodsUnlocked[3] = true; ShowRodUnlockNotification(); }
            // Legendary Rod: Level 150
            if (currentLevel >= 150 && !rodsUnlocked[4]) { rodsUnlocked[4] = true; ShowRodUnlockNotification(); }
            // Epic Rod: Level 200
            if (currentLevel >= 200 && !rodsUnlocked[5]) { rodsUnlocked[5] = true; ShowRodUnlockNotification(); }
        }

        // Check for epic rod from bottle (unlocks Legendary when obtained this way)
        if (BottleEventSystem.Instance != null && BottleEventSystem.Instance.hasEpicFishingRod)
        {
            if (!rodsUnlocked[4])
            {
                rodsUnlocked[4] = true; // Legendary rod from bottle
                ShowRodUnlockNotification();
            }
        }

        // Update notification timers
        if (levelUpNotificationTime > 0) levelUpNotificationTime -= Time.deltaTime;
        if (lootNotificationTime > 0) lootNotificationTime -= Time.deltaTime;
        if (rodUnlockNotificationTime > 0) rodUnlockNotificationTime -= Time.deltaTime;
        if (specialFishDiscoveryTime > 0) specialFishDiscoveryTime -= Time.deltaTime;

        // Check for nearby NPCs periodically
        npcCheckTimer += Time.deltaTime;
        if (npcCheckTimer >= NPC_CHECK_INTERVAL)
        {
            npcCheckTimer = 0f;
            CheckNearbyNPCs();
        }

        // NOTE: F key is handled by FishInventoryPanel directly - do NOT duplicate handling here
        // Enable sell mode automatically when fish inventory is open near an NPC
        if (isNearNPC && FishInventoryPanel.Instance != null && FishInventoryPanel.Instance.IsOpen() && !FishInventoryPanel.Instance.sellModeEnabled)
        {
            FishInventoryPanel.Instance.EnableSellMode(nearbyNPCName);
        }

        // Disable sell mode when player moves away from NPCs
        if (!isNearNPC && FishInventoryPanel.Instance != null && FishInventoryPanel.Instance.sellModeEnabled)
        {
            FishInventoryPanel.Instance.DisableSellMode();
        }
    }

    void CheckNearbyNPCs()
    {
        isNearNPC = false;
        nearbyNPCName = "";

        // Use cached player reference instead of GameObject.Find
        if (!GameCache.IsPlayerValid()) return;

        Vector3 playerPos = GameCache.GetPlayerPosition();

        // Check cached NPC references from GameCache instead of Find() calls
        if (GameCache.GoldieBanks != null && GameCache.IsPlayerInRange(GameCache.GoldieBanks.transform.position, NPC_SELL_RANGE))
        {
            isNearNPC = true;
            nearbyNPCName = "Goldie Banks";
            return;
        }
        if (GameCache.ClothingShop != null && GameCache.IsPlayerInRange(GameCache.ClothingShop.transform.position, NPC_SELL_RANGE))
        {
            isNearNPC = true;
            nearbyNPCName = "Clothing Shop";
            return;
        }
        if (GameCache.TutCat != null && GameCache.IsPlayerInRange(GameCache.TutCat.transform.position, NPC_SELL_RANGE))
        {
            isNearNPC = true;
            nearbyNPCName = "Tutorial Cat";
            return;
        }
        // Wetsuit Pete - the main fish selling NPC
        if (GameCache.WetsuitPete != null && GameCache.IsPlayerInRange(GameCache.WetsuitPete.transform.position, NPC_SELL_RANGE))
        {
            isNearNPC = true;
            nearbyNPCName = "Wetsuit Pete";
            return;
        }
    }

    void OnLevelUp(int from, int to)
    {
        levelUpFrom = from;
        levelUpTo = to;
        levelUpNotificationTime = 4f;
    }

    public void ShowLootNotification(string text, Color color)
    {
        lootNotificationText = text;
        lootNotificationColor = color;
        lootNotificationTime = 3f;
    }

    // Special fish discovery notification (for cookable fish)
    private string specialFishDiscoveryText = "";
    private float specialFishDiscoveryTime = 0f;

    public void ShowSpecialFishDiscovery(string fishName)
    {
        specialFishDiscoveryText = $"You've found a special fish!\nCook the {fishName} at the fire\non the beach for a special buff!";
        specialFishDiscoveryTime = 5f; // Show for 5 seconds
    }

    public void ShowRodUnlockNotification()
    {
        rodUnlockNotificationTime = 4f;
    }

    public void OpenNPCDialog(string npcName)
    {
        npcDialogOpen = true;
        currentNPCName = npcName;
        inventoryOpen = false; // Close inventory if open
    }

    public void CloseNPCDialog()
    {
        npcDialogOpen = false;
        if (NPCInteraction.Instance != null)
        {
            NPCInteraction.Instance.CloseDialog();
        }
    }

    public bool IsInventoryOpen()
    {
        return inventoryOpen;
    }

    void OnGUI()
    {
        // CRITICAL HUD - NO FRAME SKIPPING to prevent flickering
        // The main HUD must update every frame for smooth display

        // Don't draw HUD if game hasn't started or not initialized
        if (!MainMenu.GameStarted || !initialized) return;

        DrawHUD();
        DrawNotifications();
        DrawNPCSellPrompt();

        if (npcDialogOpen)
        {
            DrawNPCDialog();
        }
        else if (inventoryOpen)
        {
            DrawInventoryPanel();
        }
    }

    void DrawNPCSellPrompt()
    {
        if (!isNearNPC) return;
        if (PauseMenu.IsPaused) return;

        // Draw "Press F to sell fish!" prompt on left side of screen
        float promptWidth = 180;
        float promptHeight = 38;
        float promptX = 15;
        float promptY = Screen.height / 2 - 20;

        // Pulsing effect
        float pulse = 0.85f + Mathf.Sin(Time.time * 3f) * 0.15f;

        // Background
        GUI.color = new Color(0.1f, 0.15f, 0.1f, 0.9f * pulse);
        GUI.DrawTexture(new Rect(promptX, promptY, promptWidth, promptHeight), Texture2D.whiteTexture);

        // Gold border
        GUI.color = new Color(1f, 0.85f, 0.3f, pulse);
        GUI.DrawTexture(new Rect(promptX, promptY, promptWidth, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(promptX, promptY + promptHeight - 2, promptWidth, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(promptX, promptY, 2, promptHeight), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(promptX + promptWidth - 2, promptY, 2, promptHeight), Texture2D.whiteTexture);

        GUI.color = Color.white;

        // Key indicator - use cached style
        GUI.color = new Color(0.2f, 0.25f, 0.2f, 1f);
        GUI.DrawTexture(new Rect(promptX + 10, promptY + 7, 26, 24), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(promptX + 10, promptY + 7, 26, 24), "F", cachedKeyStyle);

        // Text - use cached style
        GUI.Label(new Rect(promptX + 42, promptY, promptWidth - 50, promptHeight), "Sell fish!", cachedPromptTextStyle);
    }

    void DrawNPCDialog()
    {
        // Darken background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MakeTexture(2, 2, new Color(0, 0, 0, 0.7f)));

        // 30% smaller dialog (500 -> 350, 320 -> 224)
        float dialogWidth = 350;
        float dialogHeight = 224;
        float dialogX = (Screen.width - dialogWidth) / 2;
        float dialogY = (Screen.height - dialogHeight) / 2;

        // Dialog background - consistent style
        GUI.DrawTexture(new Rect(dialogX, dialogY, dialogWidth, dialogHeight),
            MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.12f, 0.95f)));

        // Header with NPC name
        GUI.Label(new Rect(dialogX, dialogY, dialogWidth, 24), currentNPCName, headerStyle);

        // X close button (top-right corner)
        if (GUI.Button(new Rect(dialogX + dialogWidth - 22, dialogY + 4, 18, 18), "X", closeButtonStyle))
        {
            CloseNPCDialog();
        }

        // NPC portrait area (left side) - smaller
        GUI.DrawTexture(new Rect(dialogX + 10, dialogY + 30, 56, 56),
            MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.9f)));

        // Draw a simple face icon - smaller
        GUI.DrawTexture(new Rect(dialogX + 20, dialogY + 36, 36, 44),
            MakeTexture(2, 2, new Color(0.85f, 0.70f, 0.55f))); // Face color

        // Dialog text area
        GUIStyle dialogTextStyle = new GUIStyle(labelStyle);
        dialogTextStyle.wordWrap = true;
        dialogTextStyle.fontSize = 10; // Smaller text

        float textX = dialogX + 72;
        float textWidth = dialogWidth - 82;

        // Check quest state
        bool hasPendingQuest = QuestSystem.Instance != null && QuestSystem.Instance.HasPendingQuest();
        bool hasActiveQuest = QuestSystem.Instance != null && QuestSystem.Instance.HasActiveQuest();

        if (hasPendingQuest)
        {
            Quest quest = QuestSystem.Instance.GetPendingQuest();

            // Greeting - smaller
            GUI.Label(new Rect(textX, dialogY + 30, textWidth, 18),
                "\"Ahoy there, young angler!\"", dialogTextStyle);

            // Quest offer
            GUIStyle questTitleStyle = new GUIStyle(dialogTextStyle);
            questTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold
            questTitleStyle.fontStyle = FontStyle.Bold;
            questTitleStyle.fontSize = 11; // Smaller

            GUI.Label(new Rect(textX, dialogY + 50, textWidth, 18), quest.questName, questTitleStyle);

            // Quest description - smaller
            GUI.Label(new Rect(textX, dialogY + 70, textWidth, 36), quest.description, dialogTextStyle);

            // Rewards
            GUIStyle rewardStyle = new GUIStyle(dialogTextStyle);
            rewardStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            rewardStyle.fontSize = 9; // Smaller

            GUI.Label(new Rect(textX, dialogY + 108, textWidth, 14),
                $"Rewards: {quest.xpReward} XP, {quest.coinReward} coins", rewardStyle);

            // Level indicator
            GUIStyle levelStyle = new GUIStyle(dialogTextStyle);
            levelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.9f);
            levelStyle.fontSize = 9; // Smaller

            GUI.Label(new Rect(textX, dialogY + 124, textWidth, 14),
                $"Quest Level: {quest.questLevel}", levelStyle);

            // Accept / Decline buttons - smaller
            if (GUI.Button(new Rect(dialogX + 70, dialogY + dialogHeight - 42, 90, 28), "ACCEPT", buttonStyle))
            {
                QuestSystem.Instance.AcceptQuest();
                ShowLootNotification("Quest Accepted!", new Color(0.3f, 1f, 0.5f));
                CloseNPCDialog();
            }

            if (GUI.Button(new Rect(dialogX + 175, dialogY + dialogHeight - 42, 90, 28), "DECLINE", buttonStyle))
            {
                QuestSystem.Instance.DeclineQuest();
                CloseNPCDialog();
            }
        }
        else if (hasActiveQuest)
        {
            Quest quest = QuestSystem.Instance.GetActiveQuest();

            // In progress message
            GUI.Label(new Rect(textX, dialogY + 30, textWidth, 18),
                "\"Still working on that task, eh?\"", dialogTextStyle);

            GUIStyle questTitleStyle = new GUIStyle(dialogTextStyle);
            questTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold
            questTitleStyle.fontStyle = FontStyle.Bold;
            questTitleStyle.fontSize = 11; // Smaller

            GUI.Label(new Rect(textX, dialogY + 56, textWidth, 18), quest.questName, questTitleStyle);

            // Progress
            GUIStyle progressStyle = new GUIStyle(dialogTextStyle);
            progressStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            progressStyle.fontSize = 10; // Smaller

            GUI.Label(new Rect(textX, dialogY + 78, textWidth, 18),
                $"Progress: {quest.currentAmount} / {quest.requiredAmount}", progressStyle);

            // Progress bar - smaller
            float barWidth = 140;
            float progress = (float)quest.currentAmount / quest.requiredAmount;
            GUI.DrawTexture(new Rect(textX, dialogY + 98, barWidth, 12),
                MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.2f)));
            GUI.DrawTexture(new Rect(textX + 2, dialogY + 100, (barWidth - 4) * progress, 8),
                MakeTexture(2, 2, new Color(0.3f, 0.8f, 0.4f)));

            // Encouragement
            GUI.Label(new Rect(textX, dialogY + 118, textWidth, 30),
                "\"Keep fishing! Bring me those fish!\"", dialogTextStyle);

            // Close button only - smaller
            if (GUI.Button(new Rect(dialogX + (dialogWidth - 90) / 2, dialogY + dialogHeight - 42, 90, 28), "CLOSE", buttonStyle))
            {
                CloseNPCDialog();
            }
        }
        else
        {
            // No quest available
            GUI.Label(new Rect(textX, dialogY + 30, textWidth, 42),
                "\"I don't have any work for you right now. Come back soon!\"", dialogTextStyle);

            if (GUI.Button(new Rect(dialogX + (dialogWidth - 90) / 2, dialogY + dialogHeight - 42, 90, 28), "CLOSE", buttonStyle))
            {
                CloseNPCDialog();
            }
        }

        // Right-click outside dialog to close
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            Rect dialogRect = new Rect(dialogX, dialogY, dialogWidth, dialogHeight);
            if (!dialogRect.Contains(mousePos))
            {
                CloseNPCDialog();
            }
        }
    }

    void DrawHUD()
    {
        // Bottom center action bar (scaled down ~85%)
        float barWidth = 320;
        float barHeight = 58;
        float barX = (Screen.width - barWidth) / 2;
        float barY = Screen.height - barHeight - 8;

        GUI.Box(new Rect(barX - 8, barY - 4, barWidth + 16, barHeight + 8), "", frameStyle);

        // Rod slot - cache rect for click detection
        rodSlotRect = new Rect(barX, barY, 46, 46);
        DrawEquipmentSlot(rodSlotRect, "ROD", rodColors[selectedRodIndex]);

        // Check for rod slot click
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Vector2 mousePos = Event.current.mousePosition;
            if (rodSlotRect.Contains(mousePos))
            {
                rodDropdownOpen = !rodDropdownOpen;
                Event.current.Use();
            }
            // Close dropdown if clicking outside
            else if (rodDropdownOpen)
            {
                Rect dropdownRect = GetRodDropdownRect();
                if (!dropdownRect.Contains(mousePos))
                {
                    rodDropdownOpen = false;
                    Event.current.Use();
                }
            }
        }

        // Wallet
        DrawWalletSlot(new Rect(barX + 50, barY, 46, 46));

        // Fish count (clickable to open fish inventory)
        Rect fishSlotRect = new Rect(barX + 100, barY, 46, 46);
        DrawFishCountSlot(fishSlotRect);

        // Check for fish slot click to open fish inventory
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (fishSlotRect.Contains(Event.current.mousePosition))
            {
                if (FishInventoryPanel.Instance != null)
                {
                    FishInventoryPanel.Instance.TogglePanel();
                }
                Event.current.Use();
            }
        }

        // Level display
        DrawLevelSlot(new Rect(barX + 150, barY, 58, 46));

        // Buttons
        if (GUI.Button(new Rect(barX + 216, barY + 4, 42, 18), "BAG", buttonStyle))
        {
            inventoryOpen = !inventoryOpen;
            currentTab = 0;
        }
        if (GUI.Button(new Rect(barX + 262, barY + 4, 48, 18), "BUFFS", buttonStyle))
        {
            inventoryOpen = true;
            currentTab = 2;
        }
        if (GUI.Button(new Rect(barX + 216, barY + 26, 46, 18), "QUEST", buttonStyle))
        {
            inventoryOpen = true;
            currentTab = 1;
        }
        if (GUI.Button(new Rect(barX + 266, barY + 26, 44, 18), "CHAR", buttonStyle))
        {
            if (CharacterPanel.Instance != null)
            {
                CharacterPanel.Instance.Toggle();
            }
        }

        // Controls hints removed - now in ESC > Controls menu

        // XP Bar (top center)
        DrawXPBar();

        // Quest tracker (top right)
        DrawQuestTracker();

        // Rod/Weapon dropdown (draw after HUD so it overlays)
        if (rodDropdownOpen)
        {
            DrawRodDropdownPanel();
        }
    }

    void DrawEquipmentSlot(Rect rect, string label, Color itemColor)
    {
        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        Rect iconRect = new Rect(rect.x + 6, rect.y + 4, rect.width - 12, rect.height - 16);

        // Draw rod icon if this is the ROD slot
        if (label == "ROD" && RodSprites.Instance != null)
        {
            Texture2D rodIcon = RodSprites.Instance.GetRodTexture(selectedRodIndex);
            if (rodIcon != null)
            {
                GUI.DrawTexture(iconRect, rodIcon);
            }
            else
            {
                GUI.DrawTexture(iconRect, MakeTexture(2, 2, itemColor));
            }
        }
        else
        {
            GUI.DrawTexture(iconRect, MakeTexture(2, 2, itemColor));
        }

        GUIStyle slotLabel = new GUIStyle();
        slotLabel.normal.textColor = new Color(0.6f, 0.55f, 0.4f);
        slotLabel.fontSize = 9;
        slotLabel.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 12, rect.width, 12), label, slotLabel);
    }

    void DrawWalletSlot(Rect rect)
    {
        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        int coins = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;

        // Use cached styles instead of creating new ones
        GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 25), FormatNumber(coins), cachedCoinStyle);
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 12, rect.width, 12), "GOLD", cachedSlotLabelStyle);
    }

    void DrawFishCountSlot(Rect rect)
    {
        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        int fish = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;

        // Use cached styles instead of creating new ones
        GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 25), fish.ToString(), cachedFishStyle);
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 12, rect.width, 12), "FISH", cachedSlotLabelStyle);
    }

    void DrawLevelSlot(Rect rect)
    {
        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetEffectiveLevel() : 1;
        int bonus = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetBonusLevels() : 0;

        // Update cached style color based on bonus (cheaper than creating new style)
        cachedLvlStyle.normal.textColor = bonus > 0 ? new Color(0.3f, 1f, 0.8f) : new Color(0.9f, 0.9f, 0.5f);

        string lvlText = bonus > 0 ? $"{level} (+{bonus})" : level.ToString();
        GUI.Label(new Rect(rect.x, rect.y + 5, rect.width, 25), lvlText, cachedLvlStyle);
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 12, rect.width, 12), "LEVEL", cachedSlotLabelStyle);
    }

    void DrawXPBar()
    {
        if (LevelingSystem.Instance == null) return;

        float barWidth = 250;
        float barHeight = 15;
        float barX = (Screen.width - barWidth) / 2;
        float barY = 6;

        // Background
        GUI.DrawTexture(new Rect(barX, barY, barWidth, barHeight), MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f)));

        // Progress
        float progress = LevelingSystem.Instance.GetProgressToNextLevel();
        GUI.DrawTexture(new Rect(barX + 2, barY + 2, (barWidth - 4) * progress, barHeight - 4),
            MakeTexture(2, 2, new Color(0.2f, 0.7f, 0.3f, 0.9f)));

        // Text - use cached style
        long currentXP = LevelingSystem.Instance.GetCurrentXP();
        long toNext = LevelingSystem.Instance.GetXPToNextLevel();
        int level = LevelingSystem.Instance.GetLevel();

        string xpText = $"Lv{level} | {FormatNumber(currentXP)} XP | {FormatNumber(toNext)} to next";
        GUI.Label(new Rect(barX, barY, barWidth, barHeight), xpText, cachedXpStyle);
    }

    void DrawQuestTracker()
    {
        if (QuestSystem.Instance == null) return;
        if (questTrackerHidden) return; // Allow hiding

        bool hasActiveQuest = QuestSystem.Instance.HasActiveQuest();
        bool hasPendingQuest = QuestSystem.Instance.HasPendingQuest();

        if (hasActiveQuest)
        {
            Quest quest = QuestSystem.Instance.GetActiveQuest();

            // Update window (handles dragging and resizing)
            questTrackerWindow.UpdateWindow();

            Rect rect = questTrackerWindow.WindowRect;
            float panelWidth = rect.width;
            float panelHeight = rect.height;
            float x = rect.x;
            float y = rect.y;

            // Background
            GUI.DrawTexture(new Rect(x, y, panelWidth, panelHeight), frameTex);

            // Title bar (draggable area)
            GUI.DrawTexture(new Rect(x, y, panelWidth, 20), MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.98f)));

            // X close button
            if (GUI.Button(new Rect(x + 2, y + 2, 16, 16), "X", closeButtonStyle))
            {
                questTrackerHidden = true;
            }

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            titleStyle.fontSize = Mathf.Max(9, (int)(panelWidth * 0.045f));
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(x, y + 2, panelWidth, 16), "Active Quest", titleStyle);

            // Content area
            float contentY = y + 24;

            GUIStyle questStyle = new GUIStyle();
            questStyle.normal.textColor = new Color(0.9f, 0.9f, 0.8f);
            questStyle.fontSize = Mathf.Max(8, (int)(panelWidth * 0.04f));
            questStyle.wordWrap = true;

            GUI.Label(new Rect(x + 8, contentY, panelWidth - 16, 24), quest.questName, questStyle);

            GUIStyle progressStyle = new GUIStyle();
            progressStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            progressStyle.fontSize = Mathf.Max(8, (int)(panelWidth * 0.04f));

            GUI.Label(new Rect(x + 8, contentY + 22, panelWidth - 16, 14), $"Progress: {quest.currentAmount}/{quest.requiredAmount}", progressStyle);

            // Draw resize handle
            questTrackerWindow.DrawResizeHandle();
        }
        // Don't show empty box for pending quests - removed the black empty box
    }

    void DrawNotifications()
    {
        // Level up notification
        if (levelUpNotificationTime > 0)
        {
            float alpha = Mathf.Min(1f, levelUpNotificationTime);
            GUIStyle lvlUpStyle = new GUIStyle();
            lvlUpStyle.normal.textColor = new Color(1f, 0.9f, 0.2f, alpha);
            lvlUpStyle.fontSize = 28;
            lvlUpStyle.fontStyle = FontStyle.Bold;
            lvlUpStyle.alignment = TextAnchor.MiddleCenter;

            float y = Screen.height / 3f - (4f - levelUpNotificationTime) * 20f;
            GUI.Label(new Rect(0, y, Screen.width, 40), $"LEVEL UP! {levelUpFrom} → {levelUpTo}", lvlUpStyle);
        }

        // Loot notification
        if (lootNotificationTime > 0)
        {
            float alpha = Mathf.Min(1f, lootNotificationTime);
            GUIStyle lootStyle = new GUIStyle();
            lootStyle.normal.textColor = new Color(lootNotificationColor.r, lootNotificationColor.g, lootNotificationColor.b, alpha);
            lootStyle.fontSize = 16; // Reduced from 22 to 16
            lootStyle.fontStyle = FontStyle.Bold;
            lootStyle.alignment = TextAnchor.MiddleCenter;

            float y = Screen.height / 2.5f;

            // Draw black outline
            GUIStyle outlineStyle = new GUIStyle(lootStyle);
            outlineStyle.normal.textColor = new Color(0, 0, 0, alpha);

            // Draw outline in 8 directions (up, down, left, right, and diagonals)
            int outlineOffset = 2;
            GUI.Label(new Rect(0, y - outlineOffset, Screen.width, 35), lootNotificationText, outlineStyle); // up
            GUI.Label(new Rect(0, y + outlineOffset, Screen.width, 35), lootNotificationText, outlineStyle); // down
            GUI.Label(new Rect(outlineOffset, y, Screen.width, 35), lootNotificationText, outlineStyle); // right
            GUI.Label(new Rect(-outlineOffset, y, Screen.width, 35), lootNotificationText, outlineStyle); // left
            GUI.Label(new Rect(outlineOffset, y - outlineOffset, Screen.width, 35), lootNotificationText, outlineStyle); // top-right
            GUI.Label(new Rect(-outlineOffset, y - outlineOffset, Screen.width, 35), lootNotificationText, outlineStyle); // top-left
            GUI.Label(new Rect(outlineOffset, y + outlineOffset, Screen.width, 35), lootNotificationText, outlineStyle); // bottom-right
            GUI.Label(new Rect(-outlineOffset, y + outlineOffset, Screen.width, 35), lootNotificationText, outlineStyle); // bottom-left

            // Draw main text on top
            GUI.Label(new Rect(0, y, Screen.width, 35), lootNotificationText, lootStyle);
        }

        // Rod unlock notification
        if (rodUnlockNotificationTime > 0)
        {
            float alpha = Mathf.Min(1f, rodUnlockNotificationTime);
            GUIStyle rodUnlockStyle = new GUIStyle();
            rodUnlockStyle.normal.textColor = new Color(0.3f, 1f, 0.5f, alpha);
            rodUnlockStyle.fontSize = 24;
            rodUnlockStyle.fontStyle = FontStyle.Bold;
            rodUnlockStyle.alignment = TextAnchor.MiddleCenter;

            float y = Screen.height / 2.5f;
            GUI.Label(new Rect(0, y, Screen.width, 35), "Well done matey! Try out this new rod, you've earned it!", rodUnlockStyle);
        }

        // Special fish discovery notification (cookable fish)
        if (specialFishDiscoveryTime > 0)
        {
            float alpha = Mathf.Min(1f, specialFishDiscoveryTime);

            // Draw a nice panel background
            float panelW = 380;
            float panelH = 90;
            float panelX = (Screen.width - panelW) / 2;
            float panelY = Screen.height / 3f;

            // Background
            GUI.color = new Color(0.1f, 0.08f, 0.05f, 0.9f * alpha);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Gold border
            GUI.color = new Color(1f, 0.7f, 0.3f, alpha);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panelX, panelY + panelH - 3, panelW, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panelX, panelY, 3, panelH), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panelX + panelW - 3, panelY, 3, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Text
            GUIStyle discoveryStyle = new GUIStyle();
            discoveryStyle.normal.textColor = new Color(1f, 0.85f, 0.4f, alpha);
            discoveryStyle.fontSize = 16;
            discoveryStyle.fontStyle = FontStyle.Bold;
            discoveryStyle.alignment = TextAnchor.MiddleCenter;
            discoveryStyle.wordWrap = true;

            GUI.Label(new Rect(panelX + 10, panelY + 10, panelW - 20, panelH - 20), specialFishDiscoveryText, discoveryStyle);
        }
    }

    void DrawInventoryPanel()
    {
        // Overlay
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MakeTexture(2, 2, new Color(0, 0, 0, 0.6f)));

        // 30% smaller (520 -> 364, 380 -> 266)
        float panelWidth = 364;
        float panelHeight = 266;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Panel background - consistent style
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight),
            MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.12f, 0.95f)));

        // Close button (top-right)
        if (GUI.Button(new Rect(panelX + panelWidth - 22, panelY + 4, 18, 18), "X", closeButtonStyle))
        {
            inventoryOpen = false;
        }

        // Right-click outside panel to close
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            Rect panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);
            if (!panelRect.Contains(mousePos))
            {
                inventoryOpen = false;
            }
        }

        // Tabs - smaller
        string[] tabs = { "Equipment", "Quests", "Buffs", "Wardrobe", "Melee", "Scores", "Achieve" };
        float tabWidth = 37f; // Smaller to fit 7 tabs
        for (int i = 0; i < tabs.Length; i++)
        {
            GUIStyle style = (i == currentTab) ? tabActiveStyle : tabStyle;
            if (GUI.Button(new Rect(panelX + 4 + i * (tabWidth + 1), panelY + 6, tabWidth, 18), tabs[i], style))
            {
                currentTab = i;
            }
        }

        // Content area - smaller
        Rect contentRect = new Rect(panelX + 6, panelY + 28, panelWidth - 12, panelHeight - 34);

        switch (currentTab)
        {
            case 0: DrawEquipmentTab(contentRect); break;
            case 1: DrawQuestsTab(contentRect); break;
            case 2: DrawFishBuffsTab(contentRect); break;
            case 3: DrawWardrobeTab(contentRect); break;
            case 4: DrawMeleeWeaponsTab(contentRect); break;
            case 5: DrawScoresTab(contentRect); break;
            case 6: DrawAchievementsTab(contentRect); break;
        }
    }

    void DrawEquipmentTab(Rect rect)
    {
        // Left side - Rods
        GUI.Label(new Rect(rect.x, rect.y, 120, 14), "FISHING RODS", headerStyle);

        for (int i = 0; i < rodNames.Length; i++)
        {
            DrawRodSlot(new Rect(rect.x, rect.y + 16 + i * 28, 154, 26), i);
        }

        // Right side - Stats & Special Items
        float rightX = rect.x + 164;

        GUI.Label(new Rect(rightX, rect.y, 120, 14), "PLAYER STATS", headerStyle);

        GUIStyle statStyle = new GUIStyle(labelStyle);
        statStyle.fontSize = 9; // Smaller

        int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetEffectiveLevel() : 1;
        long xp = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetCurrentXP() : 0;
        int questsDone = QuestSystem.Instance != null ? QuestSystem.Instance.GetCompletedQuestCount() : 0;

        GUI.Label(new Rect(rightX, rect.y + 22, 250, 14), $"Level: {level}", statStyle);
        GUI.Label(new Rect(rightX, rect.y + 36, 250, 14), $"Total XP: {FormatNumber(xp)}", statStyle);
        GUI.Label(new Rect(rightX, rect.y + 50, 250, 14), $"Quests Completed: {questsDone}", statStyle);

        // Special Items
        GUI.Label(new Rect(rightX, rect.y + 72, 120, 18), "SPECIAL ITEMS", headerStyle);

        float itemY = rect.y + 94;
        if (BottleEventSystem.Instance != null)
        {
            if (BottleEventSystem.Instance.hasGoldenFishingHat)
            {
                GUI.Label(new Rect(rightX, itemY, 250, 14), "★ Golden Fishing Hat", statStyle);
                itemY += 16;
            }
            if (BottleEventSystem.Instance.hasGroovyMarlinRing)
            {
                statStyle.normal.textColor = new Color(0.3f, 1f, 0.8f);
                GUI.Label(new Rect(rightX, itemY, 250, 14), "★ Groovy Marlin Ring (+10 Levels)", statStyle);
                itemY += 16;
            }
            if (BottleEventSystem.Instance.hasEpicFishingRod)
            {
                statStyle.normal.textColor = new Color(0.8f, 0.4f, 1f);
                GUI.Label(new Rect(rightX, itemY, 250, 14), "★ Epic Fishing Rod", statStyle);
            }
        }

    }

    void DrawRodSlot(Rect rect, int rodIndex)
    {
        bool isSelected = selectedRodIndex == rodIndex;
        bool isUnlocked = rodsUnlocked[rodIndex];

        Color bgColor = isSelected ? new Color(0.2f, 0.2f, 0.22f, 0.95f) : new Color(0.12f, 0.12f, 0.14f, 0.9f);
        if (!isUnlocked) bgColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        GUI.DrawTexture(rect, MakeTexture(2, 2, bgColor));

        // Draw rod icon from RodSprites - smaller
        Texture2D rodIcon = RodSprites.Instance != null ? RodSprites.Instance.GetRodTexture(rodIndex) : null;
        if (rodIcon != null && isUnlocked)
        {
            GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, 22, 22), rodIcon);
        }
        else
        {
            // Fallback to colored square if no icon or locked
            Color iconColor = isUnlocked ? rodColors[rodIndex] : new Color(0.3f, 0.3f, 0.3f);
            GUI.DrawTexture(new Rect(rect.x + 3, rect.y + 3, 20, 20), MakeTexture(2, 2, iconColor));
        }

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.normal.textColor = isUnlocked ? rodColors[rodIndex] : new Color(0.4f, 0.4f, 0.4f);
        nameStyle.fontSize = 9; // Smaller
        nameStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(rect.x + 26, rect.y + 2, 120, 12), rodNames[rodIndex], nameStyle);

        GUIStyle statStyle = new GUIStyle();
        statStyle.fontSize = 8; // Smaller

        if (isUnlocked)
        {
            statStyle.normal.textColor = new Color(0.4f, 0.8f, 0.4f);
            string bonus = $"Luck: +{rodIndex * 5}%";
            if (rodIndex > 0) bonus += $" | Spd: +{rodIndex * 10}%";
            GUI.Label(new Rect(rect.x + 26, rect.y + 14, 126, 10), bonus, statStyle);
        }
        else
        {
            statStyle.normal.textColor = new Color(0.8f, 0.3f, 0.3f);
            int required = rodIndex == 1 ? 100 : rodIndex == 2 ? 500 : rodIndex == 3 ? 2000 : rodIndex == 4 ? 10000 : 100000;
            GUI.Label(new Rect(rect.x + 26, rect.y + 14, 126, 10), $"Need: {FormatNumber(required)}g", statStyle);
        }

        if (isUnlocked && GUI.Button(rect, "", GUIStyle.none))
        {
            selectedRodIndex = rodIndex;
        }
    }

    void DrawQuestsTab(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, 160, 18), "ACTIVE QUEST", headerStyle);

        if (QuestSystem.Instance != null && QuestSystem.Instance.HasActiveQuest())
        {
            Quest quest = QuestSystem.Instance.GetActiveQuest();

            GUI.DrawTexture(new Rect(rect.x, rect.y + 22, 320, 65), MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            titleStyle.fontSize = 11;
            titleStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(rect.x + 8, rect.y + 26, 300, 16), quest.questName, titleStyle);

            GUIStyle descStyle = new GUIStyle(labelStyle);
            descStyle.fontSize = 9;
            descStyle.wordWrap = true;

            GUI.Label(new Rect(rect.x + 8, rect.y + 42, 300, 16), quest.description, descStyle);

            GUIStyle progressStyle = new GUIStyle();
            progressStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            progressStyle.fontSize = 10;

            GUI.Label(new Rect(rect.x + 8, rect.y + 62, 160, 16),
                $"Progress: {quest.currentAmount}/{quest.requiredAmount}", progressStyle);

            GUIStyle rewardStyle = new GUIStyle();
            rewardStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
            rewardStyle.fontSize = 9;

            GUI.Label(new Rect(rect.x + 170, rect.y + 62, 150, 16),
                $"Reward: {quest.xpReward} XP, {quest.coinReward}g", rewardStyle);
        }
        else
        {
            GUI.Label(new Rect(rect.x + 8, rect.y + 30, 250, 16), "No active quest. Talk to the NPC!", labelStyle);
        }

        // Completed quests
        GUI.Label(new Rect(rect.x, rect.y + 100, 160, 18), "COMPLETED QUESTS", headerStyle);

        int completed = QuestSystem.Instance != null ? QuestSystem.Instance.GetCompletedQuestCount() : 0;
        GUI.Label(new Rect(rect.x + 8, rect.y + 122, 250, 16), $"Total Completed: {completed}", labelStyle);

        // Quest NPC hint
        GUIStyle hintStyle = new GUIStyle(labelStyle);
        hintStyle.fontSize = 10;
        GUI.Label(new Rect(rect.x, rect.y + 160, 320, 50),
            "Visit the Quest NPC near the dock to receive new quests!\nComplete quests to earn XP and coins.", hintStyle);
    }

    // Fish Buffs scroll position
    private float buffScrollPosition = 0f;

    void DrawFishBuffsTab(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, 160, 18), "FISH BUFFS", headerStyle);

        if (FishBuffSystem.Instance == null)
        {
            GUIStyle errorStyle = new GUIStyle();
            errorStyle.normal.textColor = new Color(0.7f, 0.5f, 0.4f);
            errorStyle.fontSize = 11;
            errorStyle.alignment = TextAnchor.MiddleCenter;
            errorStyle.wordWrap = true;
            GUI.Label(new Rect(rect.x, rect.y + 60, rect.width, 60),
                "Fish Buff System not available.\nTalk to Chef Gusteau to unlock buffs!", errorStyle);
            return;
        }

        // Active buffs section
        GUIStyle activeStyle = new GUIStyle();
        activeStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
        activeStyle.fontSize = 10;
        activeStyle.fontStyle = FontStyle.Bold;

        float activeY = rect.y + 20;
        if (FishBuffSystem.Instance.activeBuffs.Count > 0)
        {
            GUI.Label(new Rect(rect.x, activeY, 100, 14), "ACTIVE:", activeStyle);
            activeY += 14;

            foreach (var active in FishBuffSystem.Instance.activeBuffs)
            {
                int mins = Mathf.FloorToInt(active.remainingTime / 60f);
                int secs = Mathf.FloorToInt(active.remainingTime % 60f);
                GUI.Label(new Rect(rect.x + 4, activeY, rect.width - 8, 12),
                    $"{active.buffName} - {mins}:{secs:D2}", activeStyle);
                activeY += 13;
            }
            activeY += 5;
        }
        else
        {
            GUIStyle noActiveStyle = new GUIStyle();
            noActiveStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            noActiveStyle.fontSize = 9;
            GUI.Label(new Rect(rect.x, activeY, rect.width, 14), "No active buffs", noActiveStyle);
            activeY += 18;
        }

        // Inventory section
        GUIStyle invHeader = new GUIStyle();
        invHeader.normal.textColor = new Color(1f, 0.9f, 0.6f);
        invHeader.fontSize = 10;
        invHeader.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(rect.x, activeY, 100, 14), "INVENTORY:", invHeader);
        activeY += 16;

        // Scrollable buff list
        float itemHeight = 38;
        float visibleHeight = rect.height - (activeY - rect.y) - 5;
        float totalHeight = FishBuffSystem.Instance.allBuffs.Count * (itemHeight + 3);
        float maxScroll = Mathf.Max(0, totalHeight - visibleHeight);

        Rect scrollArea = new Rect(rect.x, activeY, rect.width, visibleHeight);
        if (scrollArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                buffScrollPosition += Event.current.delta.y * 20f;
                buffScrollPosition = Mathf.Clamp(buffScrollPosition, 0, maxScroll);
                Event.current.Use();
            }
        }

        GUI.BeginGroup(scrollArea);

        float itemY = -buffScrollPosition;
        foreach (var buff in FishBuffSystem.Instance.allBuffs)
        {
            if (itemY + itemHeight > 0 && itemY < visibleHeight)
            {
                DrawBuffItem(new Rect(0, itemY, rect.width - 10, itemHeight), buff);
            }
            itemY += itemHeight + 3;
        }

        GUI.EndGroup();

        // Scroll indicator
        if (maxScroll > 0)
        {
            float scrollBarHeight = visibleHeight * (visibleHeight / totalHeight);
            float scrollBarY = (buffScrollPosition / maxScroll) * (visibleHeight - scrollBarHeight);
            GUI.DrawTexture(new Rect(rect.x + rect.width - 6, activeY + scrollBarY, 4, scrollBarHeight),
                MakeTexture(2, 2, new Color(0.5f, 0.4f, 0.3f, 0.7f)));
        }
    }

    void DrawBuffItem(Rect rect, FishBuff buff)
    {
        int count = FishBuffSystem.Instance.GetBuffCount(buff.type);
        bool hasAny = count > 0;
        bool isActive = FishBuffSystem.Instance.IsBuffActive(buff.type);

        // Background
        Color bgColor = isActive ? new Color(0.15f, 0.25f, 0.15f, 0.9f) :
                        hasAny ? new Color(0.12f, 0.1f, 0.08f, 0.9f) :
                        new Color(0.08f, 0.07f, 0.06f, 0.7f);
        GUI.DrawTexture(rect, MakeTexture(2, 2, bgColor));

        // Bowl sprite
        if (FishBuffSprites.Instance != null)
        {
            Texture2D bowlTex = FishBuffSprites.Instance.GetBuffSprite(buff.type);
            if (bowlTex != null)
            {
                GUI.DrawTexture(new Rect(rect.x + 4, rect.y + 3, 32, 32), bowlTex);
            }
        }
        else
        {
            // Fallback colored square
            GUI.DrawTexture(new Rect(rect.x + 4, rect.y + 3, 32, 32), MakeTexture(2, 2, buff.bowlColor));
        }

        // Buff name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.normal.textColor = hasAny ? new Color(1f, 0.9f, 0.6f) : new Color(0.5f, 0.5f, 0.5f);
        nameStyle.fontSize = 10;
        nameStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(rect.x + 40, rect.y + 2, 160, 14), buff.buffName, nameStyle);

        // Description
        GUIStyle descStyle = new GUIStyle();
        descStyle.normal.textColor = new Color(0.6f, 0.6f, 0.5f);
        descStyle.fontSize = 8;
        GUI.Label(new Rect(rect.x + 40, rect.y + 15, 180, 12), buff.description, descStyle);

        // Count
        GUIStyle countStyle = new GUIStyle();
        countStyle.normal.textColor = hasAny ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.4f, 0.4f, 0.4f);
        countStyle.fontSize = 10;
        countStyle.fontStyle = FontStyle.Bold;
        countStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(rect.x + rect.width - 70, rect.y + 2, 30, 14), $"x{count}", countStyle);

        // Use button
        if (hasAny && !isActive)
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 38, rect.y + 8, 34, 22), "USE", buttonStyle))
            {
                FishBuffSystem.Instance.ActivateBuff(buff.type);
            }
        }
        else if (isActive)
        {
            GUIStyle activeLabel = new GUIStyle();
            activeLabel.normal.textColor = new Color(0.3f, 1f, 0.5f);
            activeLabel.fontSize = 8;
            activeLabel.fontStyle = FontStyle.Bold;
            activeLabel.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x + rect.width - 45, rect.y + 10, 42, 18), "ACTIVE", activeLabel);
        }
        else
        {
            // Locked/empty indicator
            GUIStyle lockedStyle = new GUIStyle();
            lockedStyle.normal.textColor = new Color(0.4f, 0.35f, 0.3f);
            lockedStyle.fontSize = 7;
            lockedStyle.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(rect.x + 40, rect.y + 26, 180, 10), $"Catch {buff.requiredFishName}", lockedStyle);
        }
    }

    void BuyItem(ShopItem item)
    {
        if (GameManager.Instance == null) return;

        int coins = GameManager.Instance.GetCoins();
        if (coins < item.price) return;

        GameManager.Instance.AddCoins(-item.price);

        // Handle item effects
        switch (item.itemType)
        {
            case ShopItemType.Rod:
                if (item.name.Contains("Bronze") && !rodsUnlocked[1]) { rodsUnlocked[1] = true; ShowRodUnlockNotification(); }
                else if (item.name.Contains("Silver") && !rodsUnlocked[2]) { rodsUnlocked[2] = true; ShowRodUnlockNotification(); }
                else if (item.name.Contains("Golden") && !rodsUnlocked[3]) { rodsUnlocked[3] = true; ShowRodUnlockNotification(); }
                else if (item.name.Contains("Legendary") && !rodsUnlocked[4]) { rodsUnlocked[4] = true; ShowRodUnlockNotification(); }
                else if (item.name.Contains("Epic") && !rodsUnlocked[5]) { rodsUnlocked[5] = true; ShowRodUnlockNotification(); }
                break;
        }

        Debug.Log($"Purchased: {item.name}");
    }

    // =============== SCORES TAB ===============

    void DrawScoresTab(Rect rect)
    {
        // Title
        GUI.Label(new Rect(rect.x, rect.y, 200, 14), "YOUR SCORES", headerStyle);

        // Scrollable area
        Rect listRect = new Rect(rect.x, rect.y + 18, rect.width, rect.height - 22);
        float contentHeight = 320f; // Total height of all content

        // Handle scroll wheel
        if (listRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                scoresScrollPos += Event.current.delta.y * 15f;
                scoresScrollPos = Mathf.Clamp(scoresScrollPos, 0, Mathf.Max(0, contentHeight - listRect.height));
                Event.current.Use();
            }
        }

        // Background
        GUI.DrawTexture(listRect, MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.9f)));

        // Begin scroll view
        GUI.BeginGroup(listRect);

        GUIStyle statLabelStyle = new GUIStyle();
        statLabelStyle.fontSize = 10;
        statLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.75f);

        GUIStyle statValueStyle = new GUIStyle();
        statValueStyle.fontSize = 11;
        statValueStyle.fontStyle = FontStyle.Bold;

        GUIStyle sectionStyle = new GUIStyle();
        sectionStyle.fontSize = 9;
        sectionStyle.fontStyle = FontStyle.Bold;
        sectionStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);

        float y = 8 - scoresScrollPos;
        float labelX = 10;
        float valueX = 115;

        // FISHING STATS
        GUI.Label(new Rect(labelX, y, 100, 14), "FISHING STATS", sectionStyle);
        y += 16;

        int totalFish = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;
        statLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.85f);
        GUI.Label(new Rect(labelX, y, 100, 14), "Total Caught:", statLabelStyle);
        statValueStyle.normal.textColor = new Color(1f, 0.95f, 0.5f);
        GUI.Label(new Rect(valueX, y, 80, 14), totalFish.ToString("N0"), statValueStyle);
        y += 16;

        // Divider
        GUI.DrawTexture(new Rect(labelX, y, listRect.width - 20, 1), MakeTexture(1, 1, new Color(0.3f, 0.3f, 0.35f, 0.5f)));
        y += 6;

        // Get fish by rarity
        int commonFish = 0, uncommonFish = 0, rareFish = 0, epicFish = 0, legendaryFish = 0, mythicFish = 0;
        if (GameManager.Instance != null && FishingSystem.Instance != null)
        {
            foreach (var kvp in GameManager.Instance.fishInventory)
            {
                var fishData = FishingSystem.Instance.GetFishById(kvp.Key);
                if (fishData != null)
                {
                    switch (fishData.rarity)
                    {
                        case Rarity.Common: commonFish += kvp.Value; break;
                        case Rarity.Uncommon: uncommonFish += kvp.Value; break;
                        case Rarity.Rare: rareFish += kvp.Value; break;
                        case Rarity.Epic: epicFish += kvp.Value; break;
                        case Rarity.Legendary: legendaryFish += kvp.Value; break;
                        case Rarity.Mythic: mythicFish += kvp.Value; break;
                    }
                }
            }
        }

        // Fish by rarity - compact layout
        DrawScoreLine(ref y, labelX, valueX, "Common:", commonFish, new Color(0.6f, 0.6f, 0.6f), statLabelStyle, statValueStyle);
        DrawScoreLine(ref y, labelX, valueX, "Uncommon:", uncommonFish, new Color(0.3f, 0.85f, 0.3f), statLabelStyle, statValueStyle);
        DrawScoreLine(ref y, labelX, valueX, "Rare:", rareFish, new Color(0.4f, 0.6f, 1f), statLabelStyle, statValueStyle);
        DrawScoreLine(ref y, labelX, valueX, "Epic:", epicFish, new Color(0.8f, 0.4f, 1f), statLabelStyle, statValueStyle);
        DrawScoreLine(ref y, labelX, valueX, "Legendary:", legendaryFish, new Color(1f, 0.75f, 0.2f), statLabelStyle, statValueStyle);
        DrawScoreLine(ref y, labelX, valueX, "Mythic:", mythicFish, new Color(1f, 0.35f, 0.35f), statLabelStyle, statValueStyle);
        y += 8;

        // ECONOMY
        GUI.Label(new Rect(labelX, y, 100, 14), "ECONOMY", sectionStyle);
        y += 16;

        int totalGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        DrawScoreLine(ref y, labelX, valueX, "Current Gold:", totalGold, new Color(1f, 0.85f, 0.2f), statLabelStyle, statValueStyle);
        y += 8;

        // PROGRESSION
        GUI.Label(new Rect(labelX, y, 100, 14), "PROGRESSION", sectionStyle);
        y += 16;

        int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetEffectiveLevel() : 1;
        long xp = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetCurrentXP() : 0;
        int questsDone = QuestSystem.Instance != null ? QuestSystem.Instance.GetCompletedQuestCount() : 0;

        DrawScoreLine(ref y, labelX, valueX, "Level:", level, new Color(0.5f, 0.9f, 1f), statLabelStyle, statValueStyle);
        statLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.75f);
        GUI.Label(new Rect(labelX, y, 100, 14), "Total XP:", statLabelStyle);
        statValueStyle.normal.textColor = new Color(0.5f, 0.9f, 1f);
        GUI.Label(new Rect(valueX, y, 80, 14), FormatNumber(xp), statValueStyle);
        y += 14;
        DrawScoreLine(ref y, labelX, valueX, "Quests Done:", questsDone, new Color(0.5f, 0.9f, 1f), statLabelStyle, statValueStyle);
        y += 8;

        // SPECIAL ITEMS
        GUI.Label(new Rect(labelX, y, 100, 14), "SPECIAL ITEMS", sectionStyle);
        y += 16;

        if (BottleEventSystem.Instance != null)
        {
            if (BottleEventSystem.Instance.hasGoldenFishingHat)
            {
                statLabelStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
                GUI.Label(new Rect(labelX, y, 150, 14), "Golden Fishing Hat", statLabelStyle);
                y += 14;
            }
            if (BottleEventSystem.Instance.hasGroovyMarlinRing)
            {
                statLabelStyle.normal.textColor = new Color(0.3f, 1f, 0.8f);
                GUI.Label(new Rect(labelX, y, 150, 14), "Groovy Marlin Ring", statLabelStyle);
                y += 14;
            }
            if (BottleEventSystem.Instance.hasEpicFishingRod)
            {
                statLabelStyle.normal.textColor = new Color(0.8f, 0.4f, 1f);
                GUI.Label(new Rect(labelX, y, 150, 14), "Epic Fishing Rod", statLabelStyle);
                y += 14;
            }
        }

        if (ShoulderParrot.Instance != null && ShoulderParrot.Instance.HasParrotUnlocked())
        {
            statLabelStyle.normal.textColor = new Color(0.4f, 1f, 0.5f);
            GUI.Label(new Rect(labelX, y, 150, 14), "Shoulder Parrot", statLabelStyle);
            y += 14;
        }

        GUI.EndGroup();

        // Scrollbar
        if (contentHeight > listRect.height)
        {
            float scrollBarHeight = listRect.height * (listRect.height / contentHeight);
            float scrollBarY = (scoresScrollPos / (contentHeight - listRect.height)) * (listRect.height - scrollBarHeight);
            GUI.DrawTexture(new Rect(listRect.x + listRect.width - 6, listRect.y + scrollBarY, 4, scrollBarHeight),
                MakeTexture(1, 1, new Color(0.5f, 0.5f, 0.55f, 0.8f)));
        }
    }

    // =============== ACHIEVEMENTS TAB ===============

    private float achievementScrollPos = 0f;

    void DrawAchievementsTab(Rect rect)
    {
        // Title with counter
        int unlocked = AchievementSystem.Instance != null ? AchievementSystem.Instance.GetUnlockedCount() : 0;
        int total = AchievementSystem.Instance != null ? AchievementSystem.Instance.GetTotalCount() : 0;
        GUI.Label(new Rect(rect.x, rect.y, 200, 14), $"ACHIEVEMENTS ({unlocked}/{total})", headerStyle);

        // "Open Full Panel" button
        GUIStyle openBtnStyle = new GUIStyle(buttonStyle);
        openBtnStyle.fontSize = 9;
        if (GUI.Button(new Rect(rect.x + rect.width - 85, rect.y, 85, 16), "FULL VIEW", openBtnStyle))
        {
            if (AchievementSystem.Instance != null)
            {
                AchievementSystem.Instance.OpenPanel();
                inventoryOpen = false;
            }
        }

        // Scrollable list area
        Rect listRect = new Rect(rect.x, rect.y + 18, rect.width, rect.height - 22);
        GUI.DrawTexture(listRect, MakeTexture(2, 2, new Color(0.08f, 0.08f, 0.1f, 0.9f)));

        if (AchievementSystem.Instance == null)
        {
            GUIStyle errorStyle = new GUIStyle();
            errorStyle.fontSize = 11;
            errorStyle.alignment = TextAnchor.MiddleCenter;
            errorStyle.normal.textColor = new Color(0.8f, 0.5f, 0.5f);
            GUI.Label(listRect, "Achievement system not loaded", errorStyle);
            return;
        }

        var achievements = AchievementSystem.Instance.achievements;
        float itemHeight = 36f;
        float contentHeight = achievements.Count * itemHeight;

        // Handle scroll
        if (listRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                achievementScrollPos += Event.current.delta.y * 20f;
                achievementScrollPos = Mathf.Clamp(achievementScrollPos, 0, Mathf.Max(0, contentHeight - listRect.height));
                Event.current.Use();
            }
        }

        GUI.BeginGroup(listRect);

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 10;
        nameStyle.fontStyle = FontStyle.Bold;

        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 8;
        descStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

        GUIStyle lockedStyle = new GUIStyle();
        lockedStyle.fontSize = 10;
        lockedStyle.fontStyle = FontStyle.Bold;
        lockedStyle.normal.textColor = new Color(0.4f, 0.4f, 0.4f);

        GUIStyle chanceStyle = new GUIStyle();
        chanceStyle.fontSize = 8;
        chanceStyle.alignment = TextAnchor.MiddleRight;
        chanceStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

        float y = 4 - achievementScrollPos;

        for (int i = 0; i < achievements.Count; i++)
        {
            // Skip items outside visible area
            if (y + itemHeight < 0 || y > listRect.height)
            {
                y += itemHeight;
                continue;
            }

            Achievement achievement = achievements[i];
            Rect itemRect = new Rect(4, y, listRect.width - 12, itemHeight - 4);

            // Background
            Color bgColor = achievement.isUnlocked ?
                new Color(0.12f, 0.14f, 0.12f, 0.9f) :
                new Color(0.08f, 0.08f, 0.08f, 0.9f);
            GUI.DrawTexture(itemRect, MakeTexture(2, 2, bgColor));

            // Rarity color indicator (left edge)
            Color rarityColor = GetAchievementRarityColor(achievement.rarity);
            GUI.DrawTexture(new Rect(itemRect.x, itemRect.y, 3, itemRect.height),
                MakeTexture(1, 1, achievement.isUnlocked ? rarityColor : new Color(0.25f, 0.25f, 0.25f)));

            if (achievement.isUnlocked)
            {
                // Unlocked - show name and description
                nameStyle.normal.textColor = rarityColor;
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 4, 200, 14), achievement.name, nameStyle);
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 18, 200, 12), achievement.description, descStyle);

                // Chance display
                GUI.Label(new Rect(itemRect.x + itemRect.width - 60, itemRect.y + 4, 55, 12), achievement.chanceDisplay, chanceStyle);

                // Rarity label
                chanceStyle.normal.textColor = new Color(rarityColor.r * 0.7f, rarityColor.g * 0.7f, rarityColor.b * 0.7f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 60, itemRect.y + 18, 55, 12), achievement.rarity.ToString(), chanceStyle);
                chanceStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                // Locked - show "???"
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 4, 200, 14), "???", lockedStyle);
                descStyle.normal.textColor = new Color(0.4f, 0.4f, 0.4f);
                GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 18, 200, 12), "Achievement Locked", descStyle);
                descStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

                // Dimmed rarity hint
                chanceStyle.normal.textColor = new Color(0.3f, 0.3f, 0.3f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 60, itemRect.y + 10, 55, 12), achievement.rarity.ToString(), chanceStyle);
                chanceStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            }

            y += itemHeight;
        }

        GUI.EndGroup();

        // Scrollbar
        if (contentHeight > listRect.height)
        {
            float scrollBarHeight = listRect.height * (listRect.height / contentHeight);
            float scrollBarY = (achievementScrollPos / (contentHeight - listRect.height)) * (listRect.height - scrollBarHeight);
            GUI.DrawTexture(new Rect(listRect.x + listRect.width - 6, listRect.y + scrollBarY, 4, scrollBarHeight),
                MakeTexture(1, 1, new Color(0.5f, 0.5f, 0.55f, 0.8f)));
        }
    }

    Color GetAchievementRarityColor(AchievementRarity rarity)
    {
        switch (rarity)
        {
            case AchievementRarity.Common: return new Color(0.7f, 0.7f, 0.7f);
            case AchievementRarity.Rare: return new Color(0.4f, 0.6f, 1f);
            case AchievementRarity.Epic: return new Color(0.8f, 0.4f, 1f);
            case AchievementRarity.Legendary: return new Color(1f, 0.75f, 0.2f);
            case AchievementRarity.Mythic: return new Color(1f, 0.35f, 0.35f);
            default: return Color.white;
        }
    }

    void DrawScoreLine(ref float y, float labelX, float valueX, string label, int value, Color color, GUIStyle labelStyle, GUIStyle valueStyle)
    {
        labelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.75f);
        GUI.Label(new Rect(labelX, y, 100, 14), label, labelStyle);
        valueStyle.normal.textColor = color;
        GUI.Label(new Rect(valueX, y, 80, 14), value.ToString("N0"), valueStyle);
        y += 14;
    }

    string FormatNumber(long num)
    {
        if (num >= 1000000) return $"{num / 1000000f:F1}M";
        if (num >= 1000) return $"{num / 1000f:F1}K";
        return num.ToString();
    }

    public int GetSelectedRodIndex() { return selectedRodIndex; }
    public float GetLuckBonus() { return selectedRodIndex * 0.05f; }
    public float GetSpeedBonus() { return selectedRodIndex * 0.10f; }

    // Rod cosmetic data getters for FishingRodAnimator
    public Color GetRodColor(int index) { return index >= 0 && index < rodColors.Length ? rodColors[index] : rodColors[0]; }
    public float GetRodMetallic(int index) { return index >= 0 && index < rodMetallic.Length ? rodMetallic[index] : 0.1f; }
    public float GetRodGlossiness(int index) { return index >= 0 && index < rodGlossiness.Length ? rodGlossiness[index] : 0.3f; }
    public bool GetRodHasGlow(int index) { return index >= 0 && index < rodHasGlow.Length && rodHasGlow[index]; }
    public bool GetRodHasSmoke(int index) { return index >= 0 && index < rodHasSmoke.Length && rodHasSmoke[index]; }
    public string GetRodName(int index) { return index >= 0 && index < rodNames.Length ? rodNames[index] : "Unknown"; }

    // =============== WARDROBE TAB ===============

    void DrawWardrobeTab(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, 200, 20), "YOUR WARDROBE", headerStyle);

        GUIStyle statStyle = new GUIStyle(labelStyle);
        statStyle.fontSize = 11;

        if (ownedClothing.Count == 0)
        {
            statStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(rect.x, rect.y + 30, rect.width, 40),
                "You don't own any clothing yet!\nVisit the Old Lady's shop to buy some.", statStyle);
            return;
        }

        // Scrollable content area
        float itemHeight = 55f;
        float contentHeight = ownedClothing.Count * itemHeight;
        float visibleHeight = rect.height - 30;

        // Mouse wheel scrolling
        if (rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                wardrobeScrollPos += Event.current.delta.y * 20f;
                wardrobeScrollPos = Mathf.Clamp(wardrobeScrollPos, 0, Mathf.Max(0, contentHeight - visibleHeight));
                Event.current.Use();
            }
        }

        // Content area background
        GUI.DrawTexture(new Rect(rect.x, rect.y + 25, rect.width, visibleHeight),
            MakeTexture(2, 2, new Color(0.06f, 0.06f, 0.08f, 0.9f)));

        // Begin scroll area
        GUI.BeginGroup(new Rect(rect.x, rect.y + 25, rect.width, visibleHeight));

        for (int i = 0; i < ownedClothing.Count; i++)
        {
            float itemY = i * itemHeight - wardrobeScrollPos;

            // Skip items outside visible area
            if (itemY + itemHeight < 0 || itemY > visibleHeight) continue;

            WardrobeItem item = ownedClothing[i];
            DrawWardrobeItem(new Rect(5, itemY + 5, rect.width - 30, itemHeight - 10), item);
        }

        GUI.EndGroup();

        // Scrollbar
        if (contentHeight > visibleHeight)
        {
            float scrollBarHeight = visibleHeight * (visibleHeight / contentHeight);
            float scrollBarY = (wardrobeScrollPos / (contentHeight - visibleHeight)) * (visibleHeight - scrollBarHeight);

            GUI.DrawTexture(new Rect(rect.x + rect.width - 12, rect.y + 25, 10, visibleHeight),
                MakeTexture(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.8f)));
            GUI.DrawTexture(new Rect(rect.x + rect.width - 11, rect.y + 25 + scrollBarY, 8, scrollBarHeight),
                MakeTexture(2, 2, new Color(0.4f, 0.35f, 0.2f, 0.9f)));
        }
    }

    void DrawWardrobeItem(Rect rect, WardrobeItem item)
    {
        // Item background
        Color bgColor = item.isEquipped ? new Color(0.2f, 0.25f, 0.15f, 0.95f) : new Color(0.12f, 0.12f, 0.14f, 0.95f);
        GUI.DrawTexture(rect, MakeTexture(2, 2, bgColor));

        // Item icon/image
        Texture2D iconTex = GetClothingIcon(item.itemName, item.slot, item.color);
        GUI.DrawTexture(new Rect(rect.x + 5, rect.y + 5, 35, 35), iconTex);

        // Item name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 12;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = GetSlotColor(item.slot);
        GUI.Label(new Rect(rect.x + 48, rect.y + 5, 200, 18), item.itemName, nameStyle);

        // Slot type
        GUIStyle slotStyle = new GUIStyle();
        slotStyle.fontSize = 9;
        slotStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(rect.x + 48, rect.y + 22, 100, 14), item.slot, slotStyle);

        // Equipped indicator or Equip button
        if (item.isEquipped)
        {
            GUIStyle equippedStyle = new GUIStyle();
            equippedStyle.fontSize = 10;
            equippedStyle.fontStyle = FontStyle.Bold;
            equippedStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);
            equippedStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x + rect.width - 70, rect.y + 10, 60, 25), "EQUIPPED", equippedStyle);
        }
        else
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 70, rect.y + 8, 60, 28), "Equip", buttonStyle))
            {
                EquipWardrobeItem(item);
            }
        }
    }

    Texture2D GetClothingIcon(string itemName, string slot, Color itemColor)
    {
        string key = $"wardrobe_{itemName}";

        if (!textureCache.ContainsKey(key))
        {
            // Create a simple icon texture for the item
            Texture2D icon = new Texture2D(35, 35);
            Color[] pixels = new Color[35 * 35];

            // Background
            Color bg = new Color(0.15f, 0.15f, 0.18f);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;

            // Draw simple shape based on slot
            Color mainColor = itemColor;

            if (slot == "Head")
            {
                // Hat shape - dome on top
                for (int y = 0; y < 35; y++)
                {
                    for (int x = 0; x < 35; x++)
                    {
                        float dx = x - 17.5f;
                        float dy = y - 20f;
                        if (dy > 0 && dy < 12 && Mathf.Abs(dx) < 15) pixels[y * 35 + x] = mainColor; // Brim
                        if (dy >= 12 && dx * dx + (dy - 12) * (dy - 12) < 100) pixels[y * 35 + x] = mainColor; // Crown
                    }
                }
            }
            else if (slot == "Top")
            {
                // Shirt shape - T shape
                for (int y = 5; y < 30; y++)
                {
                    for (int x = 8; x < 27; x++)
                    {
                        if (y < 12 || (x > 12 && x < 23)) pixels[y * 35 + x] = mainColor;
                    }
                }
            }
            else if (slot == "Legs")
            {
                // Pants shape - two legs
                for (int y = 5; y < 30; y++)
                {
                    for (int x = 8; x < 27; x++)
                    {
                        if (y < 12) pixels[y * 35 + x] = mainColor;
                        else if (x < 16 || x > 19) pixels[y * 35 + x] = mainColor;
                    }
                }
            }
            else if (slot == "Accessory")
            {
                // Accessory - star/circle
                for (int y = 0; y < 35; y++)
                {
                    for (int x = 0; x < 35; x++)
                    {
                        float dx = x - 17.5f;
                        float dy = y - 17.5f;
                        if (dx * dx + dy * dy < 144) pixels[y * 35 + x] = mainColor;
                    }
                }
            }

            // Border
            for (int i = 0; i < 35; i++)
            {
                pixels[i] = Color.gray;
                pixels[34 * 35 + i] = Color.gray;
                pixels[i * 35] = Color.gray;
                pixels[i * 35 + 34] = Color.gray;
            }

            icon.SetPixels(pixels);
            icon.Apply();
            textureCache[key] = icon;
        }

        return textureCache[key];
    }

    Color GetSlotColor(string slot)
    {
        switch (slot)
        {
            case "Head": return new Color(0.9f, 0.7f, 0.3f);
            case "Top": return new Color(0.5f, 0.8f, 1f);
            case "Legs": return new Color(0.6f, 0.9f, 0.6f);
            case "Accessory": return new Color(1f, 0.6f, 0.9f);
            default: return Color.white;
        }
    }

    void EquipWardrobeItem(WardrobeItem itemToEquip)
    {
        // Unequip any item in the same slot
        foreach (WardrobeItem item in ownedClothing)
        {
            if (item.slot == itemToEquip.slot)
            {
                item.isEquipped = false;
            }
        }

        // Equip this item
        itemToEquip.isEquipped = true;

        // Update player visuals
        if (PlayerClothingVisuals.Instance != null)
        {
            PlayerClothingVisuals.Instance.EquipClothing(itemToEquip.slot, itemToEquip.itemName, itemToEquip.color);
        }

        // Update character panel
        if (CharacterPanel.Instance != null)
        {
            int slotIndex = GetSlotIndex(itemToEquip.slot);
            if (slotIndex >= 0)
            {
                CharacterPanel.Instance.SetEquipment(slotIndex, itemToEquip.itemName);
            }
        }

        Debug.Log($"Equipped {itemToEquip.itemName} from wardrobe");
    }

    int GetSlotIndex(string slot)
    {
        switch (slot)
        {
            case "Head": return 0;
            case "Top": return 1;
            case "Legs": return 2;
            case "Accessory": return 3;
            default: return -1;
        }
    }

    // Called by ClothingShopNPC when an item is purchased
    public void AddToWardrobe(string itemName, string slot, Color color)
    {
        // Check if already owned
        foreach (WardrobeItem item in ownedClothing)
        {
            if (item.itemName == itemName && item.slot == slot)
            {
                return; // Already owned
            }
        }

        WardrobeItem newItem = new WardrobeItem(itemName, slot, color);
        ownedClothing.Add(newItem);
        Debug.Log($"Added {itemName} to wardrobe");
    }

    public bool IsInWardrobe(string itemName)
    {
        foreach (WardrobeItem item in ownedClothing)
        {
            if (item.itemName == itemName) return true;
        }
        return false;
    }

    // =============== MELEE WEAPONS TAB ===============

    private float meleeScrollPos = 0f;

    void DrawMeleeWeaponsTab(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, 200, 20), "MELEE WEAPONS", headerStyle);

        if (WeaponSystem.Instance == null)
        {
            GUIStyle errorStyle = new GUIStyle(labelStyle);
            errorStyle.normal.textColor = new Color(0.8f, 0.3f, 0.3f);
            GUI.Label(new Rect(rect.x, rect.y + 30, rect.width, 40),
                "Weapon system not available.\nVisit a weapon shop to buy melee weapons!", errorStyle);
            return;
        }

        // Controls help
        GUIStyle helpStyle = new GUIStyle();
        helpStyle.normal.textColor = new Color(0.7f, 0.8f, 0.6f);
        helpStyle.fontSize = 9;
        GUI.Label(new Rect(rect.x, rect.y + 18, rect.width, 14), "Press Q to swap Rod/Weapon, 1-4 for weapon slots", helpStyle);

        // Get owned weapons
        List<WeaponData> ownedWeapons = WeaponSystem.Instance.GetOwnedWeapons();

        if (ownedWeapons == null || ownedWeapons.Count == 0)
        {
            GUIStyle emptyStyle = new GUIStyle(labelStyle);
            emptyStyle.alignment = TextAnchor.MiddleCenter;
            emptyStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(rect.x, rect.y + 60, rect.width, 40),
                "No weapons owned yet.\nVisit a weapon shop in Ice/Jungle realm!", emptyStyle);
            return;
        }

        // Scrollable area
        float itemHeight = 55f;
        float contentHeight = ownedWeapons.Count * itemHeight;
        float visibleHeight = rect.height - 40;

        // Mouse wheel scrolling
        Rect scrollArea = new Rect(rect.x, rect.y + 35, rect.width, visibleHeight);
        if (scrollArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                meleeScrollPos += Event.current.delta.y * 20f;
                meleeScrollPos = Mathf.Clamp(meleeScrollPos, 0, Mathf.Max(0, contentHeight - visibleHeight));
                Event.current.Use();
            }
        }

        // Content area background
        GUI.DrawTexture(new Rect(rect.x, rect.y + 35, rect.width, visibleHeight),
            MakeTexture(2, 2, new Color(0.06f, 0.06f, 0.08f, 0.9f)));

        // Begin scroll area
        GUI.BeginGroup(scrollArea);

        for (int i = 0; i < ownedWeapons.Count; i++)
        {
            float itemY = i * itemHeight - meleeScrollPos;

            // Skip items outside visible area
            if (itemY + itemHeight < 0 || itemY > visibleHeight) continue;

            DrawMeleeWeaponItem(new Rect(5, itemY + 5, rect.width - 30, itemHeight - 10), ownedWeapons[i], i);
        }

        GUI.EndGroup();

        // Scrollbar
        if (contentHeight > visibleHeight)
        {
            float scrollBarHeight = visibleHeight * (visibleHeight / contentHeight);
            float scrollBarY = (meleeScrollPos / (contentHeight - visibleHeight)) * (visibleHeight - scrollBarHeight);

            GUI.DrawTexture(new Rect(rect.x + rect.width - 12, rect.y + 35, 10, visibleHeight),
                MakeTexture(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.8f)));
            GUI.DrawTexture(new Rect(rect.x + rect.width - 11, rect.y + 35 + scrollBarY, 8, scrollBarHeight),
                MakeTexture(2, 2, new Color(0.4f, 0.35f, 0.2f, 0.9f)));
        }
    }

    void DrawMeleeWeaponItem(Rect rect, WeaponData weapon, int index)
    {
        bool isEquipped = WeaponSystem.Instance.IsWeaponEquipped(weapon);

        // Item background
        Color bgColor = isEquipped ? new Color(0.2f, 0.3f, 0.25f, 0.95f) :
                        new Color(0.12f, 0.12f, 0.14f, 0.95f);
        GUI.DrawTexture(rect, MakeTexture(2, 2, bgColor));

        // Slot number
        GUIStyle slotStyle = new GUIStyle();
        slotStyle.fontSize = 16;
        slotStyle.fontStyle = FontStyle.Bold;
        slotStyle.normal.textColor = new Color(0.5f, 0.6f, 0.7f);
        slotStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(rect.x + 5, rect.y + 5, 25, 35), (index + 1).ToString(), slotStyle);

        // Weapon name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 12;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = isEquipped ? new Color(0.5f, 1f, 0.6f) : Color.white;
        GUI.Label(new Rect(rect.x + 35, rect.y + 5, 150, 18), weapon.name, nameStyle);

        // Stats
        GUIStyle statsStyle = new GUIStyle();
        statsStyle.fontSize = 9;
        statsStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(rect.x + 35, rect.y + 22, 200, 14),
            $"DMG: {weapon.damage}  SPD: {weapon.attackSpeed:F1}s  RNG: {weapon.range:F1}", statsStyle);

        // Equip button
        if (isEquipped)
        {
            GUIStyle equippedStyle = new GUIStyle();
            equippedStyle.fontSize = 10;
            equippedStyle.fontStyle = FontStyle.Bold;
            equippedStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);
            equippedStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x + rect.width - 65, rect.y + 10, 60, 25), "ACTIVE", equippedStyle);
        }
        else
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 65, rect.y + 8, 60, 28), "EQUIP", buttonStyle))
            {
                WeaponSystem.Instance.EquipWeapon(weapon);
            }
        }
    }

    // =============== ROD/WEAPON DROPDOWN ===============

    Rect GetRodDropdownRect()
    {
        // Position dropdown above the rod slot
        float panelWidth = 280;
        float panelHeight = 320;
        float panelX = rodSlotRect.x;
        float panelY = rodSlotRect.y - panelHeight - 10;

        return new Rect(panelX, panelY, panelWidth, panelHeight);
    }

    void DrawRodDropdownPanel()
    {
        Rect panelRect = GetRodDropdownRect();

        // Panel background
        GUI.DrawTexture(panelRect, MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.12f, 0.95f)));

        // Border
        GUI.color = new Color(0.6f, 0.7f, 0.8f, 0.9f);
        GUI.DrawTexture(new Rect(panelRect.x, panelRect.y, panelRect.width, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x, panelRect.y + panelRect.height - 2, panelRect.width, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x + panelRect.width - 2, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Header
        GUI.Label(new Rect(panelRect.x, panelRect.y, panelRect.width, 24), "EQUIPMENT SELECTION", headerStyle);

        // Close button
        if (GUI.Button(new Rect(panelRect.x + panelRect.width - 22, panelRect.y + 4, 18, 18), "X", closeButtonStyle))
        {
            rodDropdownOpen = false;
        }

        // Tab buttons
        float tabY = panelRect.y + 28;
        float tabWidth = (panelRect.width - 16) / 2f;
        float tabHeight = 28;

        // RODS tab
        GUIStyle rodsTabStyle = new GUIStyle(buttonStyle);
        rodsTabStyle.fontSize = 10;
        rodsTabStyle.fontStyle = FontStyle.Bold;
        Color rodsTabColor = equipmentTab == 0 ? new Color(0.2f, 0.4f, 0.5f, 0.95f) : new Color(0.12f, 0.12f, 0.14f, 0.9f);
        GUI.DrawTexture(new Rect(panelRect.x + 6, tabY, tabWidth, tabHeight), MakeTexture(2, 2, rodsTabColor));
        if (equipmentTab == 0)
        {
            GUI.color = new Color(0.4f, 0.8f, 1f);
            GUI.DrawTexture(new Rect(panelRect.x + 6, tabY + tabHeight - 2, tabWidth, 2), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        if (GUI.Button(new Rect(panelRect.x + 6, tabY, tabWidth, tabHeight), "RODS", rodsTabStyle))
        {
            equipmentTab = 0;
            equipmentScrollPos = 0;
        }

        // WEAPONS tab
        GUIStyle weaponsTabStyle = new GUIStyle(buttonStyle);
        weaponsTabStyle.fontSize = 10;
        weaponsTabStyle.fontStyle = FontStyle.Bold;
        Color weaponsTabColor = equipmentTab == 1 ? new Color(0.2f, 0.4f, 0.5f, 0.95f) : new Color(0.12f, 0.12f, 0.14f, 0.9f);
        GUI.DrawTexture(new Rect(panelRect.x + 10 + tabWidth, tabY, tabWidth, tabHeight), MakeTexture(2, 2, weaponsTabColor));
        if (equipmentTab == 1)
        {
            GUI.color = new Color(0.4f, 0.8f, 1f);
            GUI.DrawTexture(new Rect(panelRect.x + 10 + tabWidth, tabY + tabHeight - 2, tabWidth, 2), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        if (GUI.Button(new Rect(panelRect.x + 10 + tabWidth, tabY, tabWidth, tabHeight), "WEAPONS", weaponsTabStyle))
        {
            equipmentTab = 1;
            equipmentScrollPos = 0;
        }

        // Scrollable content area (starts below tabs)
        Rect contentRect = new Rect(panelRect.x + 6, tabY + tabHeight + 4, panelRect.width - 12, panelRect.height - 66);

        // Calculate total content height based on selected tab
        int rodCount = rodNames.Length;
        int weaponCount = WeaponSystem.Instance != null ? WeaponSystem.Instance.GetOwnedWeapons().Count : 0;
        float itemHeight = 38;
        float totalHeight = equipmentTab == 0 ? rodCount * itemHeight : weaponCount * itemHeight;
        float visibleHeight = contentRect.height;

        // Mouse wheel scrolling
        if (contentRect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                equipmentScrollPos += Event.current.delta.y * 20f;
                equipmentScrollPos = Mathf.Clamp(equipmentScrollPos, 0, Mathf.Max(0, totalHeight - visibleHeight));
                Event.current.Use();
            }
        }

        // Begin scroll area
        GUI.BeginGroup(contentRect);

        float currentY = -equipmentScrollPos;

        // Draw content based on selected tab
        if (equipmentTab == 0)
        {
            // RODS TAB
            for (int i = 0; i < rodNames.Length; i++)
            {
                if (currentY + itemHeight > 0 && currentY < visibleHeight)
                {
                    DrawDropdownRodItem(new Rect(4, currentY, contentRect.width - 8, itemHeight - 4), i);
                }
                currentY += itemHeight;
            }
        }
        else
        {
            // WEAPONS TAB
            if (WeaponSystem.Instance != null)
            {
                List<WeaponData> ownedWeapons = WeaponSystem.Instance.GetOwnedWeapons();

                if (ownedWeapons != null && ownedWeapons.Count > 0)
                {
                    for (int i = 0; i < ownedWeapons.Count; i++)
                    {
                        if (currentY + itemHeight > 0 && currentY < visibleHeight)
                        {
                            DrawDropdownWeaponItem(new Rect(4, currentY, contentRect.width - 8, itemHeight - 4), ownedWeapons[i]);
                        }
                        currentY += itemHeight;
                    }
                }
                else
                {
                    // No weapons owned message
                    GUIStyle noWeaponsStyle = new GUIStyle();
                    noWeaponsStyle.fontSize = 10;
                    noWeaponsStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
                    noWeaponsStyle.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(4, 20, contentRect.width - 8, 40), "No weapons owned.\nVisit the Weapons Shop!", noWeaponsStyle);
                }
            }
        }

        GUI.EndGroup();

        // Scrollbar
        if (totalHeight > visibleHeight)
        {
            float scrollBarHeight = visibleHeight * (visibleHeight / totalHeight);
            float scrollBarY = (equipmentScrollPos / (totalHeight - visibleHeight)) * (visibleHeight - scrollBarHeight);

            GUI.DrawTexture(new Rect(contentRect.x + contentRect.width - 8, contentRect.y, 6, visibleHeight),
                MakeTexture(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.8f)));
            GUI.DrawTexture(new Rect(contentRect.x + contentRect.width - 7, contentRect.y + scrollBarY, 4, scrollBarHeight),
                MakeTexture(2, 2, new Color(0.5f, 0.6f, 0.7f, 0.9f)));
        }
    }

    void DrawDropdownRodItem(Rect rect, int rodIndex)
    {
        bool isSelected = selectedRodIndex == rodIndex && !IsInWeaponMode();
        bool isUnlocked = rodsUnlocked[rodIndex];

        Color bgColor = isSelected ? new Color(0.2f, 0.3f, 0.25f, 0.95f) :
                        new Color(0.12f, 0.12f, 0.14f, 0.9f);
        if (!isUnlocked) bgColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        GUI.DrawTexture(rect, MakeTexture(2, 2, bgColor));

        // Rod icon
        Texture2D rodIcon = RodSprites.Instance != null ? RodSprites.Instance.GetRodTexture(rodIndex) : null;
        if (rodIcon != null && isUnlocked)
        {
            GUI.DrawTexture(new Rect(rect.x + 4, rect.y + 4, 26, 26), rodIcon);
        }
        else
        {
            Color iconColor = isUnlocked ? rodColors[rodIndex] : new Color(0.3f, 0.3f, 0.3f);
            GUI.DrawTexture(new Rect(rect.x + 4, rect.y + 4, 26, 26), MakeTexture(2, 2, iconColor));
        }

        // Rod name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.normal.textColor = isUnlocked ? rodColors[rodIndex] : new Color(0.4f, 0.4f, 0.4f);
        nameStyle.fontSize = 10;
        nameStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(rect.x + 36, rect.y + 4, 140, 14), rodNames[rodIndex], nameStyle);

        // Stats or locked text
        GUIStyle statStyle = new GUIStyle();
        statStyle.fontSize = 8;
        if (isUnlocked)
        {
            statStyle.normal.textColor = new Color(0.5f, 0.8f, 0.5f);
            string bonus = $"Luck: +{rodIndex * 5}%";
            if (rodIndex > 0) bonus += $" | Spd: +{rodIndex * 10}%";
            GUI.Label(new Rect(rect.x + 36, rect.y + 18, 140, 12), bonus, statStyle);
        }
        else
        {
            statStyle.normal.textColor = new Color(0.8f, 0.3f, 0.3f);
            int required = rodIndex == 1 ? 100 : rodIndex == 2 ? 500 : rodIndex == 3 ? 2000 : rodIndex == 4 ? 10000 : 100000;
            GUI.Label(new Rect(rect.x + 36, rect.y + 18, 140, 12), $"Locked: {FormatNumber(required)}g", statStyle);
        }

        // Equipped indicator or button
        if (isSelected)
        {
            GUIStyle equippedStyle = new GUIStyle();
            equippedStyle.fontSize = 9;
            equippedStyle.fontStyle = FontStyle.Bold;
            equippedStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);
            equippedStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x + rect.width - 60, rect.y + 6, 56, 22), "✓ ACTIVE", equippedStyle);
        }
        else if (isUnlocked)
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 58, rect.y + 6, 54, 22), "EQUIP", buttonStyle))
            {
                selectedRodIndex = rodIndex;
                // Switch out of weapon mode if needed
                if (WeaponSystem.Instance != null && WeaponSystem.Instance.IsInWeaponMode())
                {
                    // Get player reference and re-enable fishing rod
                    if (GameCache.IsPlayerValid())
                    {
                        GameObject player = GameCache.Player.gameObject;
                        FishingRodAnimator rodAnim = player.GetComponent<FishingRodAnimator>();
                        if (rodAnim != null)
                        {
                            rodAnim.enabled = true;
                        }
                    }
                }
                rodDropdownOpen = false;
                ShowLootNotification($"Equipped: {rodNames[rodIndex]}", rodColors[rodIndex]);
            }
        }
    }

    void DrawDropdownWeaponItem(Rect rect, WeaponData weapon)
    {
        bool isEquipped = WeaponSystem.Instance != null && WeaponSystem.Instance.IsWeaponEquipped(weapon);

        Color bgColor = isEquipped ? new Color(0.2f, 0.3f, 0.25f, 0.95f) :
                        new Color(0.12f, 0.12f, 0.14f, 0.9f);

        GUI.DrawTexture(rect, MakeTexture(2, 2, bgColor));

        // Weapon icon (if available)
        Texture2D weaponIcon = WeaponShopNPC.Instance != null ?
            WeaponShopNPC.Instance.GetWeaponIcon(weapon.name) : null;

        if (weaponIcon != null)
        {
            GUI.DrawTexture(new Rect(rect.x + 4, rect.y + 4, 26, 26), weaponIcon);
        }
        else
        {
            // Fallback colored square
            GUI.DrawTexture(new Rect(rect.x + 4, rect.y + 4, 26, 26),
                MakeTexture(2, 2, new Color(0.6f, 0.4f, 0.3f)));
        }

        // Weapon name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 10;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = isEquipped ? new Color(0.5f, 1f, 0.6f) : Color.white;
        GUI.Label(new Rect(rect.x + 36, rect.y + 4, 140, 14), weapon.name, nameStyle);

        // Stats
        GUIStyle statStyle = new GUIStyle();
        statStyle.fontSize = 8;
        statStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(rect.x + 36, rect.y + 18, 140, 12),
            $"DMG: {weapon.damage} | SPD: {weapon.attackSpeed:F1}s | RNG: {weapon.range:F1}", statStyle);

        // Equipped indicator or button
        if (isEquipped)
        {
            GUIStyle equippedStyle = new GUIStyle();
            equippedStyle.fontSize = 9;
            equippedStyle.fontStyle = FontStyle.Bold;
            equippedStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);
            equippedStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x + rect.width - 60, rect.y + 6, 56, 22), "✓ ACTIVE", equippedStyle);
        }
        else
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 58, rect.y + 6, 54, 22), "EQUIP", buttonStyle))
            {
                if (WeaponSystem.Instance != null)
                {
                    WeaponSystem.Instance.EquipWeapon(weapon);
                }
                rodDropdownOpen = false;
                ShowLootNotification($"Equipped: {weapon.name}", new Color(0.8f, 0.6f, 0.5f));
            }
        }
    }

    bool IsInWeaponMode()
    {
        return WeaponSystem.Instance != null && WeaponSystem.Instance.IsInWeaponMode();
    }
}

[System.Serializable]
public class HighscoreEntry
{
    public string playerName;
    public int score;
    public HighscoreEntry(string name, int s) { playerName = name; score = s; }
}

[System.Serializable]
public class ShopItem
{
    public string name;
    public string description;
    public int price;
    public ShopItemType itemType;

    public ShopItem(string n, string d, int p, ShopItemType t)
    {
        name = n; description = d; price = p; itemType = t;
    }
}

public enum ShopItemType { Consumable, Rod, Cosmetic }

[System.Serializable]
public class WardrobeItem
{
    public string itemName;
    public string slot;       // Head, Top, Legs, Accessory
    public Color color;
    public bool isEquipped;

    public WardrobeItem(string name, string s, Color c)
    {
        itemName = name;
        slot = s;
        color = c;
        isEquipped = false;
    }
}
