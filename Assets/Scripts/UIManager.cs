using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // UI State
    private bool inventoryOpen = false;
    private bool shopOpen = false;
    private int selectedRodIndex = 0;
    private int currentTab = 0; // 0=Equipment, 1=Quests, 2=Shop, 3=Wardrobe

    // Wardrobe data - tracks owned clothing items
    private List<WardrobeItem> ownedClothing = new List<WardrobeItem>();
    private float wardrobeScrollPos = 0f;

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

    // NPC Dialog
    private bool npcDialogOpen = false;
    private string currentNPCName = "";

    // Quest tracker visibility
    private bool questTrackerHidden = false;

    // Close button style
    private GUIStyle closeButtonStyle;

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

        highscores.Add(new HighscoreEntry("FishMaster99", 15420));
        highscores.Add(new HighscoreEntry("ProAngler", 12350));
        highscores.Add(new HighscoreEntry("CatchKing", 9870));
        highscores.Add(new HighscoreEntry("ReelDeal", 7650));
        highscores.Add(new HighscoreEntry("HookLine", 5200));
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

        frameTex = MakeTexture(2, 2, new Color(0.1f, 0.08f, 0.06f, 0.95f));
        buttonTex = MakeTexture(2, 2, new Color(0.25f, 0.2f, 0.12f, 0.95f));
        buttonHoverTex = MakeTexture(2, 2, new Color(0.4f, 0.32f, 0.18f, 0.95f));
        tabTex = MakeTexture(2, 2, new Color(0.15f, 0.12f, 0.08f, 0.95f));
        tabActiveTex = MakeTexture(2, 2, new Color(0.3f, 0.25f, 0.15f, 0.95f));

        frameStyle = new GUIStyle();
        frameStyle.normal.background = frameTex;
        frameStyle.padding = new RectOffset(10, 10, 10, 10);

        headerStyle = new GUIStyle();
        headerStyle.normal.background = MakeTexture(2, 2, new Color(0.08f, 0.06f, 0.04f, 0.98f));
        headerStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.padding = new RectOffset(5, 5, 6, 6);

        labelStyle = new GUIStyle();
        labelStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        labelStyle.fontSize = 13;
        labelStyle.alignment = TextAnchor.MiddleLeft;

        buttonStyle = new GUIStyle();
        buttonStyle.normal.background = buttonTex;
        buttonStyle.hover.background = buttonHoverTex;
        buttonStyle.active.background = buttonHoverTex;
        buttonStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.fontSize = 13;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        buttonStyle.padding = new RectOffset(8, 8, 6, 6);

        tabStyle = new GUIStyle(buttonStyle);
        tabStyle.normal.background = tabTex;

        tabActiveStyle = new GUIStyle(buttonStyle);
        tabActiveStyle.normal.background = tabActiveTex;
        tabActiveStyle.normal.textColor = new Color(1f, 0.95f, 0.7f);

        // Close button style (X button)
        Texture2D closeTex = MakeTexture(2, 2, new Color(0.6f, 0.15f, 0.1f, 0.9f));
        Texture2D closeHoverTex = MakeTexture(2, 2, new Color(0.8f, 0.2f, 0.15f, 0.95f));
        closeButtonStyle = new GUIStyle();
        closeButtonStyle.normal.background = closeTex;
        closeButtonStyle.hover.background = closeHoverTex;
        closeButtonStyle.active.background = closeHoverTex;
        closeButtonStyle.normal.textColor = Color.white;
        closeButtonStyle.hover.textColor = Color.white;
        closeButtonStyle.fontSize = 11;
        closeButtonStyle.fontStyle = FontStyle.Bold;
        closeButtonStyle.alignment = TextAnchor.MiddleCenter;

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
        }

        // Rod unlocks
        if (GameManager.Instance != null)
        {
            int coins = GameManager.Instance.GetCoins();
            if (coins >= 100) rodsUnlocked[1] = true;
            if (coins >= 500) rodsUnlocked[2] = true;
            if (coins >= 2000) rodsUnlocked[3] = true;
            if (coins >= 10000) rodsUnlocked[4] = true;
            if (coins >= 100000) rodsUnlocked[5] = true;
        }

        // Check for epic rod from bottle (unlocks Legendary when obtained this way)
        if (BottleEventSystem.Instance != null && BottleEventSystem.Instance.hasEpicFishingRod)
        {
            rodsUnlocked[4] = true; // Legendary rod from bottle
        }

        // Update notification timers
        if (levelUpNotificationTime > 0) levelUpNotificationTime -= Time.deltaTime;
        if (lootNotificationTime > 0) lootNotificationTime -= Time.deltaTime;

        // Check for nearby NPCs periodically
        npcCheckTimer += Time.deltaTime;
        if (npcCheckTimer >= NPC_CHECK_INTERVAL)
        {
            npcCheckTimer = 0f;
            CheckNearbyNPCs();
        }

        // Handle F key press when near NPC to open fish inventory in sell mode
        if (isNearNPC && Input.GetKeyDown(KeyCode.F))
        {
            if (FishInventoryPanel.Instance != null)
            {
                FishInventoryPanel.Instance.EnableSellMode(nearbyNPCName);
            }
        }
    }

    void CheckNearbyNPCs()
    {
        isNearNPC = false;
        nearbyNPCName = "";

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        Vector3 playerPos = player.transform.position;

        // Check known NPC locations
        string[] npcNames = { "QuestNPC", "SpawnNPC", "ClothingShopNPC", "GoldieBanksNPC", "WetsuitPete" };

        foreach (string npcName in npcNames)
        {
            GameObject npc = GameObject.Find(npcName);
            if (npc != null)
            {
                float distance = Vector3.Distance(playerPos, npc.transform.position);
                if (distance <= NPC_SELL_RANGE)
                {
                    isNearNPC = true;
                    // Get a friendly name for the NPC
                    if (npcName == "QuestNPC") nearbyNPCName = "Wetsuit Pete";
                    else if (npcName == "SpawnNPC") nearbyNPCName = "Islander";
                    else if (npcName == "ClothingShopNPC") nearbyNPCName = "Clothing Shop";
                    else if (npcName == "GoldieBanksNPC") nearbyNPCName = "Goldie Banks";
                    else nearbyNPCName = npcName;
                    return;
                }
            }
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

    void OnGUI()
    {
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

        // Key indicator
        GUIStyle keyStyle = new GUIStyle();
        keyStyle.fontSize = 16;
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.alignment = TextAnchor.MiddleCenter;
        keyStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

        GUI.color = new Color(0.2f, 0.25f, 0.2f, 1f);
        GUI.DrawTexture(new Rect(promptX + 10, promptY + 7, 26, 24), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(promptX + 10, promptY + 7, 26, 24), "F", keyStyle);

        // Text
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 13;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.alignment = TextAnchor.MiddleLeft;
        textStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);

        GUI.Label(new Rect(promptX + 42, promptY, promptWidth - 50, promptHeight), "Sell fish!", textStyle);
    }

    void DrawNPCDialog()
    {
        // Darken background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MakeTexture(2, 2, new Color(0, 0, 0, 0.7f)));

        float dialogWidth = 500;
        float dialogHeight = 320;
        float dialogX = (Screen.width - dialogWidth) / 2;
        float dialogY = (Screen.height - dialogHeight) / 2;

        // Dialog background
        GUI.DrawTexture(new Rect(dialogX, dialogY, dialogWidth, dialogHeight),
            MakeTexture(2, 2, new Color(0.12f, 0.10f, 0.08f, 0.98f)));

        // Header with NPC name
        GUI.Label(new Rect(dialogX, dialogY, dialogWidth, 35), currentNPCName, headerStyle);

        // NPC portrait area (left side)
        GUI.DrawTexture(new Rect(dialogX + 15, dialogY + 45, 80, 80),
            MakeTexture(2, 2, new Color(0.08f, 0.06f, 0.04f, 0.9f)));

        // Draw a simple face icon
        GUI.DrawTexture(new Rect(dialogX + 30, dialogY + 55, 50, 60),
            MakeTexture(2, 2, new Color(0.85f, 0.70f, 0.55f))); // Face color

        // Dialog text area
        GUIStyle dialogTextStyle = new GUIStyle(labelStyle);
        dialogTextStyle.wordWrap = true;
        dialogTextStyle.fontSize = 13;

        float textX = dialogX + 110;
        float textWidth = dialogWidth - 130;

        // Check quest state
        bool hasPendingQuest = QuestSystem.Instance != null && QuestSystem.Instance.HasPendingQuest();
        bool hasActiveQuest = QuestSystem.Instance != null && QuestSystem.Instance.HasActiveQuest();

        if (hasPendingQuest)
        {
            Quest quest = QuestSystem.Instance.GetPendingQuest();

            // Greeting
            GUI.Label(new Rect(textX, dialogY + 45, textWidth, 25),
                "\"Ahoy there, young angler!\"", dialogTextStyle);

            // Quest offer
            GUIStyle questTitleStyle = new GUIStyle(dialogTextStyle);
            questTitleStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            questTitleStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(textX, dialogY + 75, textWidth, 25), quest.questName, questTitleStyle);

            // Quest description
            GUI.Label(new Rect(textX, dialogY + 100, textWidth, 50), quest.description, dialogTextStyle);

            // Rewards
            GUIStyle rewardStyle = new GUIStyle(dialogTextStyle);
            rewardStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);

            GUI.Label(new Rect(textX, dialogY + 155, textWidth, 20),
                $"Rewards: {quest.xpReward} XP, {quest.coinReward} coins", rewardStyle);

            // Level indicator
            GUIStyle levelStyle = new GUIStyle(dialogTextStyle);
            levelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.9f);
            levelStyle.fontSize = 11;

            GUI.Label(new Rect(textX, dialogY + 175, textWidth, 20),
                $"Quest Level: {quest.questLevel}", levelStyle);

            // Accept / Decline buttons
            if (GUI.Button(new Rect(dialogX + 100, dialogY + dialogHeight - 60, 130, 35), "ACCEPT", buttonStyle))
            {
                QuestSystem.Instance.AcceptQuest();
                ShowLootNotification("Quest Accepted!", new Color(0.3f, 1f, 0.5f));
                CloseNPCDialog();
            }

            if (GUI.Button(new Rect(dialogX + 250, dialogY + dialogHeight - 60, 130, 35), "DECLINE", buttonStyle))
            {
                QuestSystem.Instance.DeclineQuest();
                CloseNPCDialog();
            }
        }
        else if (hasActiveQuest)
        {
            Quest quest = QuestSystem.Instance.GetActiveQuest();

            // In progress message
            GUI.Label(new Rect(textX, dialogY + 45, textWidth, 25),
                "\"Still working on that task, eh?\"", dialogTextStyle);

            GUIStyle questTitleStyle = new GUIStyle(dialogTextStyle);
            questTitleStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            questTitleStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(textX, dialogY + 80, textWidth, 25), quest.questName, questTitleStyle);

            // Progress
            GUIStyle progressStyle = new GUIStyle(dialogTextStyle);
            progressStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            progressStyle.fontSize = 14;

            GUI.Label(new Rect(textX, dialogY + 110, textWidth, 25),
                $"Progress: {quest.currentAmount} / {quest.requiredAmount}", progressStyle);

            // Progress bar
            float barWidth = 200;
            float progress = (float)quest.currentAmount / quest.requiredAmount;
            GUI.DrawTexture(new Rect(textX, dialogY + 140, barWidth, 15),
                MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.2f)));
            GUI.DrawTexture(new Rect(textX + 2, dialogY + 142, (barWidth - 4) * progress, 11),
                MakeTexture(2, 2, new Color(0.3f, 0.8f, 0.4f)));

            // Encouragement
            GUI.Label(new Rect(textX, dialogY + 170, textWidth, 40),
                "\"Keep fishing! Bring me those fish and you'll be rewarded handsomely!\"", dialogTextStyle);

            // Close button only
            if (GUI.Button(new Rect(dialogX + (dialogWidth - 130) / 2, dialogY + dialogHeight - 60, 130, 35), "CLOSE", buttonStyle))
            {
                CloseNPCDialog();
            }
        }
        else
        {
            // No quest available
            GUI.Label(new Rect(textX, dialogY + 45, textWidth, 60),
                "\"I don't have any work for you right now. Come back soon, there's always fish to catch!\"", dialogTextStyle);

            if (GUI.Button(new Rect(dialogX + (dialogWidth - 130) / 2, dialogY + dialogHeight - 60, 130, 35), "CLOSE", buttonStyle))
            {
                CloseNPCDialog();
            }
        }

        // Close X button in corner (top-left as requested)
        if (GUI.Button(new Rect(dialogX + 5, dialogY + 5, 25, 22), "X", closeButtonStyle))
        {
            CloseNPCDialog();
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

        // Rod slot
        DrawEquipmentSlot(new Rect(barX, barY, 46, 46), "ROD", rodColors[selectedRodIndex]);

        // Wallet
        DrawWalletSlot(new Rect(barX + 50, barY, 46, 46));

        // Fish count
        DrawFishCountSlot(new Rect(barX + 100, barY, 46, 46));

        // Level display
        DrawLevelSlot(new Rect(barX + 150, barY, 58, 46));

        // Buttons
        if (GUI.Button(new Rect(barX + 216, barY + 4, 42, 18), "BAG", buttonStyle))
        {
            inventoryOpen = !inventoryOpen;
            currentTab = 0;
        }
        if (GUI.Button(new Rect(barX + 262, barY + 4, 48, 18), "SHOP", buttonStyle))
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

        // Controls (top left - smaller text)
        GUI.Label(new Rect(8, 8, 180, 16), "WASD - Move | SPACE - Jump", labelStyle);
        GUI.Label(new Rect(8, 22, 180, 16), "LEFT CLICK - Fish | I - Menu", labelStyle);

        // XP Bar (top center)
        DrawXPBar();

        // Quest tracker (top right)
        DrawQuestTracker();
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

        GUIStyle coinStyle = new GUIStyle();
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        coinStyle.fontSize = 14;
        coinStyle.fontStyle = FontStyle.Bold;
        coinStyle.alignment = TextAnchor.MiddleCenter;

        GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 25), FormatNumber(coins), coinStyle);

        GUIStyle labelS = new GUIStyle();
        labelS.normal.textColor = new Color(0.6f, 0.55f, 0.4f);
        labelS.fontSize = 9;
        labelS.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 12, rect.width, 12), "GOLD", labelS);
    }

    void DrawFishCountSlot(Rect rect)
    {
        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        int fish = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;

        GUIStyle fishStyle = new GUIStyle();
        fishStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);
        fishStyle.fontSize = 14;
        fishStyle.fontStyle = FontStyle.Bold;
        fishStyle.alignment = TextAnchor.MiddleCenter;

        GUI.Label(new Rect(rect.x, rect.y + 8, rect.width, 25), fish.ToString(), fishStyle);

        GUIStyle labelS = new GUIStyle();
        labelS.normal.textColor = new Color(0.6f, 0.55f, 0.4f);
        labelS.fontSize = 9;
        labelS.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 12, rect.width, 12), "FISH", labelS);
    }

    void DrawLevelSlot(Rect rect)
    {
        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetEffectiveLevel() : 1;
        int bonus = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetBonusLevels() : 0;

        GUIStyle lvlStyle = new GUIStyle();
        lvlStyle.normal.textColor = bonus > 0 ? new Color(0.3f, 1f, 0.8f) : new Color(0.9f, 0.9f, 0.5f);
        lvlStyle.fontSize = 16;
        lvlStyle.fontStyle = FontStyle.Bold;
        lvlStyle.alignment = TextAnchor.MiddleCenter;

        string lvlText = bonus > 0 ? $"{level} (+{bonus})" : level.ToString();
        GUI.Label(new Rect(rect.x, rect.y + 5, rect.width, 25), lvlText, lvlStyle);

        GUIStyle labelS = new GUIStyle();
        labelS.normal.textColor = new Color(0.6f, 0.55f, 0.4f);
        labelS.fontSize = 9;
        labelS.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 12, rect.width, 12), "LEVEL", labelS);
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

        // Text
        GUIStyle xpStyle = new GUIStyle();
        xpStyle.normal.textColor = Color.white;
        xpStyle.fontSize = 9;
        xpStyle.alignment = TextAnchor.MiddleCenter;

        long currentXP = LevelingSystem.Instance.GetCurrentXP();
        long toNext = LevelingSystem.Instance.GetXPToNextLevel();
        int level = LevelingSystem.Instance.GetLevel();

        string xpText = $"Lv{level} | {FormatNumber(currentXP)} XP | {FormatNumber(toNext)} to next";
        GUI.Label(new Rect(barX, barY, barWidth, barHeight), xpText, xpStyle);
    }

    void DrawQuestTracker()
    {
        if (QuestSystem.Instance == null) return;
        if (questTrackerHidden) return; // Allow hiding

        float x = Screen.width - 185;
        float y = 160; // Moved lower down

        bool hasActiveQuest = QuestSystem.Instance.HasActiveQuest();
        bool hasPendingQuest = QuestSystem.Instance.HasPendingQuest();

        if (hasActiveQuest)
        {
            Quest quest = QuestSystem.Instance.GetActiveQuest();

            GUI.Box(new Rect(x - 4, y - 4, 180, 50), "", frameStyle);

            // X close button
            if (GUI.Button(new Rect(x - 4, y - 4, 18, 16), "X", closeButtonStyle))
            {
                questTrackerHidden = true;
            }

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            titleStyle.fontSize = 10;
            titleStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(x + 16, y, 150, 14), "Active Quest:", titleStyle);

            GUIStyle questStyle = new GUIStyle();
            questStyle.normal.textColor = new Color(0.9f, 0.9f, 0.8f);
            questStyle.fontSize = 9;

            GUI.Label(new Rect(x, y + 14, 170, 14), quest.questName, questStyle);

            GUIStyle progressStyle = new GUIStyle();
            progressStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            progressStyle.fontSize = 9;

            GUI.Label(new Rect(x, y + 28, 170, 14), $"Progress: {quest.currentAmount}/{quest.requiredAmount}", progressStyle);
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
            lootStyle.fontSize = 22;
            lootStyle.fontStyle = FontStyle.Bold;
            lootStyle.alignment = TextAnchor.MiddleCenter;

            float y = Screen.height / 2.5f;
            GUI.Label(new Rect(0, y, Screen.width, 35), lootNotificationText, lootStyle);
        }
    }

    void DrawInventoryPanel()
    {
        // Overlay
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MakeTexture(2, 2, new Color(0, 0, 0, 0.6f)));

        float panelWidth = 520;
        float panelHeight = 380;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Panel background
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight),
            MakeTexture(2, 2, new Color(0.08f, 0.06f, 0.04f, 0.98f)));

        // Close button (top-left)
        if (GUI.Button(new Rect(panelX + 4, panelY + 6, 22, 18), "X", closeButtonStyle))
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

        // Tabs (shifted to accommodate close button)
        string[] tabs = { "Equipment", "Quests", "Shop", "Wardrobe" };
        for (int i = 0; i < tabs.Length; i++)
        {
            GUIStyle style = (i == currentTab) ? tabActiveStyle : tabStyle;
            if (GUI.Button(new Rect(panelX + 30 + i * 75, panelY + 8, 70, 22), tabs[i], style))
            {
                currentTab = i;
            }
        }

        // Also keep a close button on right for convenience
        if (GUI.Button(new Rect(panelX + panelWidth - 32, panelY + 8, 24, 22), "X", closeButtonStyle))
        {
            inventoryOpen = false;
        }

        // Content area
        Rect contentRect = new Rect(panelX + 8, panelY + 38, panelWidth - 16, panelHeight - 46);

        switch (currentTab)
        {
            case 0: DrawEquipmentTab(contentRect); break;
            case 1: DrawQuestsTab(contentRect); break;
            case 2: DrawShopTab(contentRect); break;
            case 3: DrawWardrobeTab(contentRect); break;
        }
    }

    void DrawEquipmentTab(Rect rect)
    {
        // Left side - Rods
        GUI.Label(new Rect(rect.x, rect.y, 120, 18), "FISHING RODS", headerStyle);

        for (int i = 0; i < rodNames.Length; i++)
        {
            DrawRodSlot(new Rect(rect.x, rect.y + 22 + i * 38, 220, 35), i);
        }

        // Right side - Stats & Special Items
        float rightX = rect.x + 235;

        GUI.Label(new Rect(rightX, rect.y, 120, 18), "PLAYER STATS", headerStyle);

        GUIStyle statStyle = new GUIStyle(labelStyle);
        statStyle.fontSize = 10;

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

        // Highscores
        GUI.Label(new Rect(rightX, rect.y + 180, 120, 18), "HIGHSCORES", headerStyle);
        DrawHighscores(new Rect(rightX, rect.y + 200, 250, 100));
    }

    void DrawRodSlot(Rect rect, int rodIndex)
    {
        bool isSelected = selectedRodIndex == rodIndex;
        bool isUnlocked = rodsUnlocked[rodIndex];

        Color bgColor = isSelected ? new Color(0.25f, 0.2f, 0.1f, 0.95f) : new Color(0.12f, 0.1f, 0.08f, 0.9f);
        if (!isUnlocked) bgColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        GUI.DrawTexture(rect, MakeTexture(2, 2, bgColor));

        // Draw rod icon from RodSprites
        Texture2D rodIcon = RodSprites.Instance != null ? RodSprites.Instance.GetRodTexture(rodIndex) : null;
        if (rodIcon != null && isUnlocked)
        {
            GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, 31, 31), rodIcon);
        }
        else
        {
            // Fallback to colored square if no icon or locked
            Color iconColor = isUnlocked ? rodColors[rodIndex] : new Color(0.3f, 0.3f, 0.3f);
            GUI.DrawTexture(new Rect(rect.x + 4, rect.y + 4, 27, 27), MakeTexture(2, 2, iconColor));
        }

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.normal.textColor = isUnlocked ? rodColors[rodIndex] : new Color(0.4f, 0.4f, 0.4f);
        nameStyle.fontSize = 11;
        nameStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(rect.x + 36, rect.y + 4, 150, 14), rodNames[rodIndex], nameStyle);

        GUIStyle statStyle = new GUIStyle();
        statStyle.fontSize = 9;

        if (isUnlocked)
        {
            statStyle.normal.textColor = new Color(0.4f, 0.8f, 0.4f);
            string bonus = $"Luck: +{rodIndex * 5}%";
            if (rodIndex > 0) bonus += $" | Speed: +{rodIndex * 10}%";
            GUI.Label(new Rect(rect.x + 36, rect.y + 19, 180, 14), bonus, statStyle);
        }
        else
        {
            statStyle.normal.textColor = new Color(0.8f, 0.3f, 0.3f);
            int required = rodIndex == 1 ? 100 : rodIndex == 2 ? 500 : rodIndex == 3 ? 2000 : rodIndex == 4 ? 10000 : 100000;
            GUI.Label(new Rect(rect.x + 36, rect.y + 19, 180, 14), $"Requires: {FormatNumber(required)} coins", statStyle);
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

    // Shop scroll position
    private float shopScrollPosition = 0f;

    void DrawShopTab(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, 160, 18), "FISHING SHOP", headerStyle);

        int coins = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;

        GUIStyle coinStyle = new GUIStyle();
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        coinStyle.fontSize = 11;
        coinStyle.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(rect.x + rect.width - 120, rect.y, 120, 18), $"Gold: {FormatNumber(coins)}", coinStyle);

        // Scrollable area for items
        float itemHeight = 32;
        float visibleHeight = rect.height - 30;
        float totalHeight = shopItems.Length * (itemHeight + 3);
        float maxScroll = Mathf.Max(0, totalHeight - visibleHeight);

        // Handle mouse wheel scrolling when mouse is over the shop area
        Rect scrollArea = new Rect(rect.x, rect.y + 22, rect.width, visibleHeight);
        if (scrollArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                shopScrollPosition += Event.current.delta.y * 20f;
                shopScrollPosition = Mathf.Clamp(shopScrollPosition, 0, maxScroll);
                Event.current.Use();
            }
        }

        // Begin clip area
        GUI.BeginGroup(scrollArea);

        float itemY = -shopScrollPosition;
        foreach (var item in shopItems)
        {
            // Only draw visible items
            if (itemY + itemHeight > 0 && itemY < visibleHeight)
            {
                DrawShopItem(new Rect(0, itemY, rect.width - 10, itemHeight), item, coins);
            }
            itemY += itemHeight + 3;
        }

        GUI.EndGroup();

        // Draw scroll indicator if needed
        if (maxScroll > 0)
        {
            float scrollBarHeight = visibleHeight * (visibleHeight / totalHeight);
            float scrollBarY = (shopScrollPosition / maxScroll) * (visibleHeight - scrollBarHeight);
            GUI.DrawTexture(new Rect(rect.x + rect.width - 6, rect.y + 22 + scrollBarY, 4, scrollBarHeight),
                MakeTexture(2, 2, new Color(0.5f, 0.4f, 0.3f, 0.7f)));
        }
    }

    void DrawShopItem(Rect rect, ShopItem item, int playerCoins)
    {
        bool canAfford = playerCoins >= item.price;

        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.normal.textColor = canAfford ? new Color(0.9f, 0.9f, 0.8f) : new Color(0.5f, 0.5f, 0.5f);
        nameStyle.fontSize = 10;
        nameStyle.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(rect.x + 6, rect.y + 2, 160, 14), item.name, nameStyle);

        GUIStyle descStyle = new GUIStyle();
        descStyle.normal.textColor = new Color(0.6f, 0.6f, 0.5f);
        descStyle.fontSize = 8;

        GUI.Label(new Rect(rect.x + 6, rect.y + 16, 200, 14), item.description, descStyle);

        GUIStyle priceStyle = new GUIStyle();
        priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.2f) : new Color(0.8f, 0.3f, 0.3f);
        priceStyle.fontSize = 10;
        priceStyle.fontStyle = FontStyle.Bold;
        priceStyle.alignment = TextAnchor.MiddleRight;

        GUI.Label(new Rect(rect.x + rect.width - 120, rect.y + 2, 50, 28), $"{item.price}g", priceStyle);

        if (canAfford)
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 60, rect.y + 4, 52, 22), "BUY", buttonStyle))
            {
                BuyItem(item);
            }
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
                if (item.name.Contains("Bronze")) rodsUnlocked[1] = true;
                else if (item.name.Contains("Silver")) rodsUnlocked[2] = true;
                else if (item.name.Contains("Golden")) rodsUnlocked[3] = true;
                else if (item.name.Contains("Legendary")) rodsUnlocked[4] = true;
                else if (item.name.Contains("Epic")) rodsUnlocked[5] = true;
                break;
        }

        Debug.Log($"Purchased: {item.name}");
    }

    void DrawHighscores(Rect rect)
    {
        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.1f, 0.08f, 0.06f, 0.9f)));

        GUIStyle scoreStyle = new GUIStyle();
        scoreStyle.fontSize = 11;

        for (int i = 0; i < highscores.Count && i < 5; i++)
        {
            Color rankColor = i == 0 ? new Color(1f, 0.85f, 0.2f) :
                              i == 1 ? new Color(0.8f, 0.8f, 0.85f) :
                              i == 2 ? new Color(0.8f, 0.5f, 0.2f) :
                              new Color(0.7f, 0.7f, 0.7f);

            scoreStyle.normal.textColor = rankColor;
            GUI.Label(new Rect(rect.x + 8, rect.y + 5 + i * 22, 20, 20), $"{i + 1}.", scoreStyle);

            scoreStyle.normal.textColor = new Color(0.85f, 0.85f, 0.8f);
            GUI.Label(new Rect(rect.x + 30, rect.y + 5 + i * 22, 120, 20), highscores[i].playerName, scoreStyle);

            scoreStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            GUI.Label(new Rect(rect.x + 160, rect.y + 5 + i * 22, 80, 20), highscores[i].score.ToString("N0"), scoreStyle);
        }
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
