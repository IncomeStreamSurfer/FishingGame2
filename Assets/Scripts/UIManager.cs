using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // UI State
    private bool inventoryOpen = false;
    private bool shopOpen = false;
    private int selectedRodIndex = 0;
    private int currentTab = 0; // 0=Equipment, 1=Quests, 2=Shop

    private List<HighscoreEntry> highscores = new List<HighscoreEntry>();

    // Rod data
    private string[] rodNames = { "Basic Rod", "Bronze Rod", "Silver Rod", "Golden Rod", "Legendary Rod" };
    private Color[] rodColors = {
        new Color(0.6f, 0.4f, 0.2f),
        new Color(0.8f, 0.5f, 0.2f),
        new Color(0.75f, 0.75f, 0.8f),
        new Color(1f, 0.85f, 0.3f),
        new Color(0.8f, 0.4f, 1f)
    };
    private bool[] rodsUnlocked = { true, false, false, false, false };

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
    }

    void InitializeShopItems()
    {
        shopItems = new ShopItem[]
        {
            new ShopItem("Bait Pack (10)", "Basic bait for fishing", 50, ShopItemType.Consumable),
            new ShopItem("Premium Bait (10)", "Attracts rare fish", 200, ShopItemType.Consumable),
            new ShopItem("Lucky Charm", "+5% rare fish chance for 10 min", 500, ShopItemType.Consumable),
            new ShopItem("Bronze Rod", "Slightly better rod", 100, ShopItemType.Rod),
            new ShopItem("Silver Rod", "Good quality rod", 500, ShopItemType.Rod),
            new ShopItem("Golden Rod", "Excellent rod", 2000, ShopItemType.Rod),
            new ShopItem("Legendary Rod", "The best rod money can buy", 10000, ShopItemType.Rod),
            new ShopItem("Fisherman's Hat", "A stylish hat", 300, ShopItemType.Cosmetic),
            new ShopItem("Sailor's Coat", "Protection from the elements", 750, ShopItemType.Cosmetic),
            new ShopItem("XP Boost (1hr)", "Double XP for 1 hour", 1000, ShopItemType.Consumable),
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

        stylesInitialized = true;
    }

    Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
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
        }

        // Check for epic rod from bottle
        if (BottleEventSystem.Instance != null && BottleEventSystem.Instance.hasEpicFishingRod)
        {
            rodsUnlocked[4] = true;
        }

        // Update notification timers
        if (levelUpNotificationTime > 0) levelUpNotificationTime -= Time.deltaTime;
        if (lootNotificationTime > 0) lootNotificationTime -= Time.deltaTime;
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
        InitStyles();
        DrawHUD();
        DrawNotifications();

        if (npcDialogOpen)
        {
            DrawNPCDialog();
        }
        else if (inventoryOpen)
        {
            DrawInventoryPanel();
        }
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

        // Close X button in corner
        if (GUI.Button(new Rect(dialogX + dialogWidth - 35, dialogY + 5, 30, 25), "X", buttonStyle))
        {
            CloseNPCDialog();
        }
    }

    void DrawHUD()
    {
        // Bottom center action bar
        float barWidth = 380;
        float barHeight = 70;
        float barX = (Screen.width - barWidth) / 2;
        float barY = Screen.height - barHeight - 10;

        GUI.Box(new Rect(barX - 10, barY - 5, barWidth + 20, barHeight + 10), "", frameStyle);

        // Rod slot
        DrawEquipmentSlot(new Rect(barX, barY, 55, 55), "ROD", rodColors[selectedRodIndex]);

        // Wallet
        DrawWalletSlot(new Rect(barX + 60, barY, 55, 55));

        // Fish count
        DrawFishCountSlot(new Rect(barX + 120, barY, 55, 55));

        // Level display
        DrawLevelSlot(new Rect(barX + 180, barY, 70, 55));

        // Buttons
        if (GUI.Button(new Rect(barX + 260, barY + 5, 50, 22), "BAG", buttonStyle))
        {
            inventoryOpen = !inventoryOpen;
            currentTab = 0;
        }
        if (GUI.Button(new Rect(barX + 315, barY + 5, 55, 22), "SHOP", buttonStyle))
        {
            inventoryOpen = true;
            currentTab = 2;
        }
        if (GUI.Button(new Rect(barX + 260, barY + 32, 110, 22), "QUESTS", buttonStyle))
        {
            inventoryOpen = true;
            currentTab = 1;
        }

        // Controls (top left)
        GUI.Label(new Rect(10, 10, 200, 20), "WASD - Move | SPACE - Jump", labelStyle);
        GUI.Label(new Rect(10, 28, 200, 20), "LEFT CLICK - Fish | I - Menu", labelStyle);

        // XP Bar (top center)
        DrawXPBar();

        // Quest tracker (top right)
        DrawQuestTracker();
    }

    void DrawEquipmentSlot(Rect rect, string label, Color itemColor)
    {
        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        Rect iconRect = new Rect(rect.x + 6, rect.y + 6, rect.width - 12, rect.height - 18);
        GUI.DrawTexture(iconRect, MakeTexture(2, 2, itemColor));

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

        float barWidth = 300;
        float barHeight = 18;
        float barX = (Screen.width - barWidth) / 2;
        float barY = 8;

        // Background
        GUI.DrawTexture(new Rect(barX, barY, barWidth, barHeight), MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f)));

        // Progress
        float progress = LevelingSystem.Instance.GetProgressToNextLevel();
        GUI.DrawTexture(new Rect(barX + 2, barY + 2, (barWidth - 4) * progress, barHeight - 4),
            MakeTexture(2, 2, new Color(0.2f, 0.7f, 0.3f, 0.9f)));

        // Text
        GUIStyle xpStyle = new GUIStyle();
        xpStyle.normal.textColor = Color.white;
        xpStyle.fontSize = 11;
        xpStyle.alignment = TextAnchor.MiddleCenter;

        long currentXP = LevelingSystem.Instance.GetCurrentXP();
        long toNext = LevelingSystem.Instance.GetXPToNextLevel();
        int level = LevelingSystem.Instance.GetLevel();

        string xpText = $"Level {level} | {FormatNumber(currentXP)} XP | {FormatNumber(toNext)} to next";
        GUI.Label(new Rect(barX, barY, barWidth, barHeight), xpText, xpStyle);
    }

    void DrawQuestTracker()
    {
        if (QuestSystem.Instance == null) return;

        float x = Screen.width - 220;
        float y = 50;

        bool hasActiveQuest = QuestSystem.Instance.HasActiveQuest();
        bool hasPendingQuest = QuestSystem.Instance.HasPendingQuest();

        if (hasActiveQuest)
        {
            Quest quest = QuestSystem.Instance.GetActiveQuest();

            GUI.Box(new Rect(x - 5, y - 5, 215, 60), "", frameStyle);

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            titleStyle.fontSize = 12;
            titleStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(x, y, 200, 18), "Active Quest:", titleStyle);

            GUIStyle questStyle = new GUIStyle();
            questStyle.normal.textColor = new Color(0.9f, 0.9f, 0.8f);
            questStyle.fontSize = 11;

            GUI.Label(new Rect(x, y + 18, 200, 16), quest.questName, questStyle);

            GUIStyle progressStyle = new GUIStyle();
            progressStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            progressStyle.fontSize = 11;

            GUI.Label(new Rect(x, y + 34, 200, 16), $"Progress: {quest.currentAmount}/{quest.requiredAmount}", progressStyle);
        }
        else if (hasPendingQuest)
        {
            // Show hint to talk to NPC
            GUI.Box(new Rect(x - 5, y - 5, 215, 40), "", frameStyle);

            GUIStyle hintStyle = new GUIStyle();
            hintStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);
            hintStyle.fontSize = 11;
            hintStyle.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(x, y, 200, 30), "Quest available!\nTalk to the Old Captain", hintStyle);
        }
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

        float panelWidth = 650;
        float panelHeight = 480;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Panel background
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight),
            MakeTexture(2, 2, new Color(0.08f, 0.06f, 0.04f, 0.98f)));

        // Tabs
        string[] tabs = { "Equipment", "Quests", "Shop" };
        for (int i = 0; i < tabs.Length; i++)
        {
            GUIStyle style = (i == currentTab) ? tabActiveStyle : tabStyle;
            if (GUI.Button(new Rect(panelX + 10 + i * 105, panelY + 10, 100, 28), tabs[i], style))
            {
                currentTab = i;
            }
        }

        // Close button
        if (GUI.Button(new Rect(panelX + panelWidth - 40, panelY + 10, 30, 28), "X", buttonStyle))
        {
            inventoryOpen = false;
        }

        // Content area
        Rect contentRect = new Rect(panelX + 10, panelY + 50, panelWidth - 20, panelHeight - 60);

        switch (currentTab)
        {
            case 0: DrawEquipmentTab(contentRect); break;
            case 1: DrawQuestsTab(contentRect); break;
            case 2: DrawShopTab(contentRect); break;
        }
    }

    void DrawEquipmentTab(Rect rect)
    {
        // Left side - Rods
        GUI.Label(new Rect(rect.x, rect.y, 150, 22), "FISHING RODS", headerStyle);

        for (int i = 0; i < rodNames.Length; i++)
        {
            DrawRodSlot(new Rect(rect.x, rect.y + 30 + i * 50, 280, 45), i);
        }

        // Right side - Stats & Special Items
        float rightX = rect.x + 300;

        GUI.Label(new Rect(rightX, rect.y, 150, 22), "PLAYER STATS", headerStyle);

        GUIStyle statStyle = new GUIStyle(labelStyle);
        statStyle.fontSize = 12;

        int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetEffectiveLevel() : 1;
        long xp = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetCurrentXP() : 0;
        int questsDone = QuestSystem.Instance != null ? QuestSystem.Instance.GetCompletedQuestCount() : 0;

        GUI.Label(new Rect(rightX, rect.y + 30, 300, 18), $"Level: {level}", statStyle);
        GUI.Label(new Rect(rightX, rect.y + 48, 300, 18), $"Total XP: {FormatNumber(xp)}", statStyle);
        GUI.Label(new Rect(rightX, rect.y + 66, 300, 18), $"Quests Completed: {questsDone}", statStyle);

        // Special Items
        GUI.Label(new Rect(rightX, rect.y + 100, 150, 22), "SPECIAL ITEMS", headerStyle);

        float itemY = rect.y + 130;
        if (BottleEventSystem.Instance != null)
        {
            if (BottleEventSystem.Instance.hasGoldenFishingHat)
            {
                GUI.Label(new Rect(rightX, itemY, 300, 18), "★ Golden Fishing Hat", statStyle);
                itemY += 20;
            }
            if (BottleEventSystem.Instance.hasGroovyMarlinRing)
            {
                statStyle.normal.textColor = new Color(0.3f, 1f, 0.8f);
                GUI.Label(new Rect(rightX, itemY, 300, 18), "★ Groovy Marlin Ring (+10 Levels)", statStyle);
                itemY += 20;
            }
            if (BottleEventSystem.Instance.hasEpicFishingRod)
            {
                statStyle.normal.textColor = new Color(0.8f, 0.4f, 1f);
                GUI.Label(new Rect(rightX, itemY, 300, 18), "★ Epic Fishing Rod", statStyle);
            }
        }

        // Highscores
        GUI.Label(new Rect(rightX, rect.y + 250, 150, 22), "HIGHSCORES", headerStyle);
        DrawHighscores(new Rect(rightX, rect.y + 280, 300, 130));
    }

    void DrawRodSlot(Rect rect, int rodIndex)
    {
        bool isSelected = selectedRodIndex == rodIndex;
        bool isUnlocked = rodsUnlocked[rodIndex];

        Color bgColor = isSelected ? new Color(0.25f, 0.2f, 0.1f, 0.95f) : new Color(0.12f, 0.1f, 0.08f, 0.9f);
        if (!isUnlocked) bgColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);

        GUI.DrawTexture(rect, MakeTexture(2, 2, bgColor));

        Color iconColor = isUnlocked ? rodColors[rodIndex] : new Color(0.3f, 0.3f, 0.3f);
        GUI.DrawTexture(new Rect(rect.x + 5, rect.y + 5, 35, 35), MakeTexture(2, 2, iconColor));

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.normal.textColor = isUnlocked ? rodColors[rodIndex] : new Color(0.4f, 0.4f, 0.4f);
        nameStyle.fontSize = 13;
        nameStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(rect.x + 48, rect.y + 5, 180, 18), rodNames[rodIndex], nameStyle);

        GUIStyle statStyle = new GUIStyle();
        statStyle.fontSize = 10;

        if (isUnlocked)
        {
            statStyle.normal.textColor = new Color(0.4f, 0.8f, 0.4f);
            string bonus = $"Luck: +{rodIndex * 5}%";
            if (rodIndex > 0) bonus += $" | Speed: +{rodIndex * 10}%";
            GUI.Label(new Rect(rect.x + 48, rect.y + 24, 220, 16), bonus, statStyle);
        }
        else
        {
            statStyle.normal.textColor = new Color(0.8f, 0.3f, 0.3f);
            int required = rodIndex == 1 ? 100 : rodIndex == 2 ? 500 : rodIndex == 3 ? 2000 : 10000;
            GUI.Label(new Rect(rect.x + 48, rect.y + 24, 220, 16), $"Requires: {required} coins", statStyle);
        }

        if (isUnlocked && GUI.Button(rect, "", GUIStyle.none))
        {
            selectedRodIndex = rodIndex;
        }
    }

    void DrawQuestsTab(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, 200, 22), "ACTIVE QUEST", headerStyle);

        if (QuestSystem.Instance != null && QuestSystem.Instance.HasActiveQuest())
        {
            Quest quest = QuestSystem.Instance.GetActiveQuest();

            GUI.DrawTexture(new Rect(rect.x, rect.y + 30, 400, 80), MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.normal.textColor = new Color(1f, 0.9f, 0.5f);
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(rect.x + 10, rect.y + 35, 380, 20), quest.questName, titleStyle);

            GUIStyle descStyle = new GUIStyle(labelStyle);
            descStyle.fontSize = 11;
            descStyle.wordWrap = true;

            GUI.Label(new Rect(rect.x + 10, rect.y + 55, 380, 20), quest.description, descStyle);

            GUIStyle progressStyle = new GUIStyle();
            progressStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
            progressStyle.fontSize = 12;

            GUI.Label(new Rect(rect.x + 10, rect.y + 80, 200, 20),
                $"Progress: {quest.currentAmount}/{quest.requiredAmount}", progressStyle);

            GUIStyle rewardStyle = new GUIStyle();
            rewardStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
            rewardStyle.fontSize = 11;

            GUI.Label(new Rect(rect.x + 220, rect.y + 80, 180, 20),
                $"Reward: {quest.xpReward} XP, {quest.coinReward} coins", rewardStyle);
        }
        else
        {
            GUI.Label(new Rect(rect.x + 10, rect.y + 40, 300, 20), "No active quest. Talk to the NPC!", labelStyle);
        }

        // Completed quests
        GUI.Label(new Rect(rect.x, rect.y + 130, 200, 22), "COMPLETED QUESTS", headerStyle);

        int completed = QuestSystem.Instance != null ? QuestSystem.Instance.GetCompletedQuestCount() : 0;
        GUI.Label(new Rect(rect.x + 10, rect.y + 160, 300, 20), $"Total Completed: {completed}", labelStyle);

        // Quest NPC hint
        GUI.Label(new Rect(rect.x, rect.y + 220, 400, 60),
            "Visit the Quest NPC near the dock to receive new quests!\nComplete quests to earn XP and coins.", labelStyle);
    }

    void DrawShopTab(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, 200, 22), "FISHING SHOP", headerStyle);

        int coins = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;

        GUIStyle coinStyle = new GUIStyle();
        coinStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        coinStyle.fontSize = 14;
        coinStyle.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(rect.x + rect.width - 150, rect.y, 150, 22), $"Your Gold: {FormatNumber(coins)}", coinStyle);

        float itemY = rect.y + 35;
        foreach (var item in shopItems)
        {
            DrawShopItem(new Rect(rect.x, itemY, rect.width - 20, 38), item, coins);
            itemY += 42;
        }
    }

    void DrawShopItem(Rect rect, ShopItem item, int playerCoins)
    {
        bool canAfford = playerCoins >= item.price;

        GUI.DrawTexture(rect, MakeTexture(2, 2, new Color(0.12f, 0.1f, 0.08f, 0.9f)));

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.normal.textColor = canAfford ? new Color(0.9f, 0.9f, 0.8f) : new Color(0.5f, 0.5f, 0.5f);
        nameStyle.fontSize = 12;
        nameStyle.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(rect.x + 10, rect.y + 4, 200, 18), item.name, nameStyle);

        GUIStyle descStyle = new GUIStyle();
        descStyle.normal.textColor = new Color(0.6f, 0.6f, 0.5f);
        descStyle.fontSize = 10;

        GUI.Label(new Rect(rect.x + 10, rect.y + 20, 250, 16), item.description, descStyle);

        GUIStyle priceStyle = new GUIStyle();
        priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.2f) : new Color(0.8f, 0.3f, 0.3f);
        priceStyle.fontSize = 12;
        priceStyle.fontStyle = FontStyle.Bold;
        priceStyle.alignment = TextAnchor.MiddleRight;

        GUI.Label(new Rect(rect.x + rect.width - 150, rect.y + 4, 60, 30), $"{item.price}g", priceStyle);

        if (canAfford)
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 80, rect.y + 6, 70, 26), "BUY", buttonStyle))
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
