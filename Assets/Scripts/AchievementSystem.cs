using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Achievement System - Collectible trading-card style achievements
/// Tracks player accomplishments and displays them in the UI
/// Uses PlayerPrefs for persistence
/// </summary>
public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }

    /// <summary>
    /// Auto-create the achievement system when the game starts if it doesn't exist
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateInstance()
    {
        if (Instance == null)
        {
            Debug.Log("[AchievementSystem] Auto-creating instance at runtime");
            GameObject go = new GameObject("AchievementSystem");
            go.AddComponent<AchievementSystem>();
        }
    }

    // Achievement definitions
    public List<Achievement> achievements = new List<Achievement>();

    // Tracking variables
    private int uniqueBuffsUsed = 0;
    private HashSet<FishBuffType> usedBuffTypes = new HashSet<FishBuffType>();
    private int totalFishCooked = 0;
    private int rastaQuestsCompleted = 0;

    // UI State
    private bool panelOpen = false;
    private float scrollPos = 0f;
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;

    // Achievement unlock notification
    private float unlockNotificationTime = 0f;
    private Achievement lastUnlockedAchievement = null;

    // Cached GUIStyles
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedCounterStyle;
    private static GUIStyle cachedCardTitleStyle;
    private static GUIStyle cachedCardDescStyle;
    private static GUIStyle cachedCardRarityStyle;
    private static GUIStyle cachedCardChanceStyle;
    private static GUIStyle cachedCardDateStyle;
    private static GUIStyle cachedLockedStyle;
    private static GUIStyle cachedCloseStyle;
    private static GUIStyle cachedNotificationTitleStyle;
    private static GUIStyle cachedNotificationDescStyle;
    private static bool stylesInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        LoadProgress();
        initialized = true;
    }

    void InitializeAchievements()
    {
        // Define all achievements with their properties
        // Format: id, name, description, chanceDisplay, rarityTier, checkCondition

        // Mythic tier (0.01% - 0.1%)
        achievements.Add(new Achievement(
            "golden_prize",
            "Golden Prize",
            "Catch a Golden Starfish",
            "0.01%",
            AchievementRarity.Mythic
        ));

        // Legendary tier (0.1% - 1%)
        achievements.Add(new Achievement(
            "legendary_angler",
            "Legendary Angler",
            "Catch a Legendary fish",
            "0.1%",
            AchievementRarity.Legendary
        ));

        // Epic tier (1% - 5%)
        achievements.Add(new Achievement(
            "epic_encounter",
            "Epic Encounter",
            "Catch an Epic fish",
            "1%",
            AchievementRarity.Epic
        ));

        // Rare tier (5% - 20%)
        achievements.Add(new Achievement(
            "rare_catch",
            "Rare Catch",
            "Catch a Rare fish",
            "5%",
            AchievementRarity.Rare
        ));

        // Death achievements
        achievements.Add(new Achievement(
            "storms_victim",
            "Storm's Victim",
            "Die by lightning strike",
            "Rare Event",
            AchievementRarity.Epic
        ));

        achievements.Add(new Achievement(
            "depths_below",
            "Depths Below",
            "Die by drowning",
            "Common Event",
            AchievementRarity.Rare
        ));

        achievements.Add(new Achievement(
            "first_blood",
            "First Blood",
            "Die for the first time",
            "100%",
            AchievementRarity.Common
        ));

        // Progression achievements
        achievements.Add(new Achievement(
            "buff_master",
            "Buff Master",
            "Use 10 different buffs",
            "Progression",
            AchievementRarity.Epic
        ));

        achievements.Add(new Achievement(
            "chefs_apprentice",
            "Chef's Apprentice",
            "Cook 10 fish at the fire",
            "Progression",
            AchievementRarity.Rare
        ));

        achievements.Add(new Achievement(
            "rastas_friend",
            "Rasta's Friend",
            "Complete 100 quests with Goldie Banks",
            "Dedication",
            AchievementRarity.Legendary
        ));

        achievements.Add(new Achievement(
            "century_survivor",
            "Century Survivor",
            "Survive for 100 days",
            "Dedication",
            AchievementRarity.Legendary
        ));

        achievements.Add(new Achievement(
            "wealthy_fisher",
            "Wealthy Fisher",
            "Earn 10,000 gold total",
            "Progression",
            AchievementRarity.Epic
        ));

        achievements.Add(new Achievement(
            "fish_hoarder",
            "Fish Hoarder",
            "Catch 500 fish total",
            "Dedication",
            AchievementRarity.Legendary
        ));
    }

    void CreateCachedTextures()
    {
        CacheTexture("panelBg", new Color(0.08f, 0.06f, 0.04f, 0.97f));
        CacheTexture("panelBorder", new Color(1f, 0.85f, 0.4f, 1f));
        CacheTexture("cardLocked", new Color(0.12f, 0.10f, 0.08f, 0.95f));
        CacheTexture("cardUnlocked", new Color(0.15f, 0.12f, 0.08f, 0.95f));
        CacheTexture("closeBtn", new Color(0.8f, 0.2f, 0.2f, 1f));
        CacheTexture("notificationBg", new Color(0.1f, 0.08f, 0.06f, 0.98f));

        // Rarity borders
        CacheTexture("borderCommon", new Color(0.6f, 0.6f, 0.6f, 1f));
        CacheTexture("borderRare", new Color(0.3f, 0.6f, 1f, 1f));
        CacheTexture("borderEpic", new Color(0.7f, 0.3f, 1f, 1f));
        CacheTexture("borderLegendary", new Color(1f, 0.7f, 0.2f, 1f));
        CacheTexture("borderMythic", new Color(1f, 0.3f, 0.3f, 1f));
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

    Texture2D GetOrCreateColorTexture(Color color)
    {
        string key = $"color_{color.r:F2}_{color.g:F2}_{color.b:F2}_{color.a:F2}";
        if (!textureCache.ContainsKey(key))
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            textureCache[key] = tex;
        }
        return textureCache[key];
    }

    void Update()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        // Check for achievement unlocks
        CheckAchievements();

        // Update notification timer
        if (unlockNotificationTime > 0)
        {
            unlockNotificationTime -= Time.deltaTime;
        }
    }

    void CheckAchievements()
    {
        // Check Golden Prize - Catch Golden Starfish
        if (FishingSystem.Instance != null)
        {
            var specialInv = FishingSystem.Instance.specialFishInventory;
            bool hasGoldenStarfish = specialInv.Exists(f => f.id == "golden_starfish");
            if (hasGoldenStarfish || PlayerPrefs.GetInt("Achievement_CaughtGoldenStarfish", 0) == 1)
            {
                UnlockAchievement("golden_prize");
            }
        }

        // Check Legendary/Epic/Rare Angler - based on fish diary or inventory
        if (FishingSystem.Instance != null)
        {
            foreach (var fish in FishingSystem.Instance.fishDatabase)
            {
                bool caught = PlayerPrefs.GetInt($"FishDiary_{fish.id}", 0) == 1;
                if (caught)
                {
                    if (fish.rarity == Rarity.Legendary || fish.rarity == Rarity.Mythic)
                    {
                        UnlockAchievement("legendary_angler");
                    }
                    if (fish.rarity == Rarity.Epic)
                    {
                        UnlockAchievement("epic_encounter");
                    }
                    if (fish.rarity == Rarity.Rare)
                    {
                        UnlockAchievement("rare_catch");
                    }
                }
            }
        }

        // Check death achievements - set by PlayerHealth
        if (PlayerPrefs.GetInt("Achievement_DiedByLightning", 0) == 1)
        {
            UnlockAchievement("storms_victim");
        }
        if (PlayerPrefs.GetInt("Achievement_DiedByDrowning", 0) == 1)
        {
            UnlockAchievement("depths_below");
        }
        if (PlayerPrefs.GetInt("Achievement_FirstDeath", 0) == 1)
        {
            UnlockAchievement("first_blood");
        }

        // Check Buff Master - 10 different buffs used
        if (uniqueBuffsUsed >= 10)
        {
            UnlockAchievement("buff_master");
        }

        // Check Chef's Apprentice - Cook 10 fish
        if (totalFishCooked >= 10)
        {
            UnlockAchievement("chefs_apprentice");
        }

        // Check Rasta's Friend - 100 quests with Fish Connoisseur
        if (rastaQuestsCompleted >= 100)
        {
            UnlockAchievement("rastas_friend");
        }

        // Check Century Survivor - 100 days
        if (DayNightCycle.Instance != null && DayNightCycle.Instance.GetCurrentDay() >= 100)
        {
            UnlockAchievement("century_survivor");
        }

        // Check Wealthy Fisher - 10,000 gold total earned
        int totalGold = PlayerPrefs.GetInt("TotalGoldEarned", 0);
        if (totalGold >= 10000)
        {
            UnlockAchievement("wealthy_fisher");
        }

        // Check Fish Hoarder - 500 fish caught total
        if (GameManager.Instance != null && GameManager.Instance.GetTotalFishCaught() >= 500)
        {
            UnlockAchievement("fish_hoarder");
        }
    }

    public void UnlockAchievement(string achievementId)
    {
        Achievement achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null && !achievement.isUnlocked)
        {
            achievement.isUnlocked = true;
            achievement.unlockDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Save to PlayerPrefs
            PlayerPrefs.SetInt($"Achievement_{achievementId}", 1);
            PlayerPrefs.SetString($"Achievement_{achievementId}_Date", achievement.unlockDate);
            PlayerPrefs.Save();

            // Show notification
            lastUnlockedAchievement = achievement;
            unlockNotificationTime = 5f;

            Debug.Log($"Achievement Unlocked: {achievement.name}");

            // Play unlock sound effect
            PlayUnlockSound();
        }
    }

    void PlayUnlockSound()
    {
        // Create a celebratory sound
        AudioSource audio = gameObject.GetComponent<AudioSource>();
        if (audio == null)
        {
            audio = gameObject.AddComponent<AudioSource>();
            audio.spatialBlend = 0f;
            audio.volume = 0.6f;
        }

        int sampleRate = 44100;
        float duration = 0.4f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("AchievementUnlock", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration);

            // Ascending notes for achievement sound
            float freq1 = 440f * Mathf.Pow(2f, t * 2f); // Rising pitch
            float freq2 = 660f * Mathf.Pow(2f, t * 1.5f);

            samples[i] = (Mathf.Sin(2f * Mathf.PI * freq1 * t) * 0.3f +
                          Mathf.Sin(2f * Mathf.PI * freq2 * t) * 0.2f) * envelope;
        }

        clip.SetData(samples, 0);
        audio.PlayOneShot(clip);
    }

    // Called by other systems to track progress
    public void OnBuffUsed(FishBuffType buffType)
    {
        if (!usedBuffTypes.Contains(buffType) && buffType != FishBuffType.None && buffType != FishBuffType.Poisoned)
        {
            usedBuffTypes.Add(buffType);
            uniqueBuffsUsed = usedBuffTypes.Count;
            SaveProgress();
        }
    }

    public void OnFishCooked()
    {
        totalFishCooked++;
        PlayerPrefs.SetInt("Achievement_TotalFishCooked", totalFishCooked);
        PlayerPrefs.Save();
    }

    public void OnRastaQuestCompleted()
    {
        rastaQuestsCompleted++;
        PlayerPrefs.SetInt("Achievement_RastaQuests", rastaQuestsCompleted);
        PlayerPrefs.Save();
    }

    public void OnPlayerDeath(string deathCause)
    {
        // Track first death
        if (PlayerPrefs.GetInt("Achievement_FirstDeath", 0) == 0)
        {
            PlayerPrefs.SetInt("Achievement_FirstDeath", 1);
            PlayerPrefs.Save();
        }

        // Track specific death causes
        if (deathCause.ToLower().Contains("lightning") || deathCause.ToLower().Contains("struck"))
        {
            PlayerPrefs.SetInt("Achievement_DiedByLightning", 1);
            PlayerPrefs.Save();
        }
        // Drowning check - look for ocean/current/drowning keywords
        if (deathCause.ToLower().Contains("drown") || deathCause.ToLower().Contains("ocean") || deathCause.ToLower().Contains("current"))
        {
            PlayerPrefs.SetInt("Achievement_DiedByDrowning", 1);
            PlayerPrefs.Save();
        }
    }

    public void OnFishCaught(FishData fish)
    {
        // Track Golden Starfish specifically
        if (fish.id == "golden_starfish")
        {
            PlayerPrefs.SetInt("Achievement_CaughtGoldenStarfish", 1);
            PlayerPrefs.Save();
        }

        // Track fish diary entry for rarity checks
        PlayerPrefs.SetInt($"FishDiary_{fish.id}", 1);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        // Load achievement unlock status
        foreach (var achievement in achievements)
        {
            int unlocked = PlayerPrefs.GetInt($"Achievement_{achievement.id}", 0);
            if (unlocked == 1)
            {
                achievement.isUnlocked = true;
                achievement.unlockDate = PlayerPrefs.GetString($"Achievement_{achievement.id}_Date", "Unknown");
            }
        }

        // Load tracking variables
        totalFishCooked = PlayerPrefs.GetInt("Achievement_TotalFishCooked", 0);
        rastaQuestsCompleted = PlayerPrefs.GetInt("Achievement_RastaQuests", 0);

        // Load used buff types
        string usedBuffsStr = PlayerPrefs.GetString("Achievement_UsedBuffs", "");
        if (!string.IsNullOrEmpty(usedBuffsStr))
        {
            string[] buffIds = usedBuffsStr.Split(',');
            foreach (string buffId in buffIds)
            {
                if (Enum.TryParse(buffId, out FishBuffType buffType))
                {
                    usedBuffTypes.Add(buffType);
                }
            }
            uniqueBuffsUsed = usedBuffTypes.Count;
        }
    }

    void SaveProgress()
    {
        // Save used buff types
        List<string> buffStrings = new List<string>();
        foreach (var buff in usedBuffTypes)
        {
            buffStrings.Add(buff.ToString());
        }
        PlayerPrefs.SetString("Achievement_UsedBuffs", string.Join(",", buffStrings));
        PlayerPrefs.Save();
    }

    // Reset all achievements for new game
    public void ResetAllAchievements()
    {
        // Reset all achievement unlock status
        foreach (var achievement in achievements)
        {
            achievement.isUnlocked = false;
            achievement.unlockDate = "";
            PlayerPrefs.DeleteKey($"Achievement_{achievement.id}");
            PlayerPrefs.DeleteKey($"Achievement_{achievement.id}_Date");
        }

        // Reset tracking variables
        totalFishCooked = 0;
        rastaQuestsCompleted = 0;
        uniqueBuffsUsed = 0;
        usedBuffTypes.Clear();

        // Reset PlayerPrefs tracking data
        PlayerPrefs.DeleteKey("Achievement_TotalFishCooked");
        PlayerPrefs.DeleteKey("Achievement_RastaQuests");
        PlayerPrefs.DeleteKey("Achievement_UsedBuffs");
        PlayerPrefs.DeleteKey("Achievement_CaughtGoldenStarfish");
        PlayerPrefs.DeleteKey("Achievement_DiedByLightning");
        PlayerPrefs.DeleteKey("Achievement_DiedByDrowning");

        PlayerPrefs.Save();
        Debug.Log("[AchievementSystem] All achievements reset for new game");
    }

    // Public methods for UI
    public void OpenPanel()
    {
        panelOpen = true;
        scrollPos = 0f;
    }

    public void ClosePanel()
    {
        panelOpen = false;
    }

    public void TogglePanel()
    {
        panelOpen = !panelOpen;
        if (panelOpen) scrollPos = 0f;
    }

    public bool IsPanelOpen()
    {
        return panelOpen;
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var a in achievements)
        {
            if (a.isUnlocked) count++;
        }
        return count;
    }

    public int GetTotalCount()
    {
        return achievements.Count;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        // Draw unlock notification
        if (unlockNotificationTime > 0 && lastUnlockedAchievement != null)
        {
            DrawUnlockNotification();
        }

        // Draw panel
        if (panelOpen)
        {
            DrawAchievementsPanel();
        }
    }

    void InitializeStyles()
    {
        if (stylesInitialized) return;

        cachedTitleStyle = new GUIStyle();
        cachedTitleStyle.fontSize = 18;
        cachedTitleStyle.fontStyle = FontStyle.Bold;
        cachedTitleStyle.alignment = TextAnchor.MiddleCenter;
        cachedTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

        cachedCounterStyle = new GUIStyle();
        cachedCounterStyle.fontSize = 12;
        cachedCounterStyle.alignment = TextAnchor.MiddleCenter;
        cachedCounterStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

        cachedCardTitleStyle = new GUIStyle();
        cachedCardTitleStyle.fontSize = 12;
        cachedCardTitleStyle.fontStyle = FontStyle.Bold;
        cachedCardTitleStyle.alignment = TextAnchor.MiddleCenter;
        cachedCardTitleStyle.normal.textColor = Color.white;

        cachedCardDescStyle = new GUIStyle();
        cachedCardDescStyle.fontSize = 9;
        cachedCardDescStyle.alignment = TextAnchor.MiddleCenter;
        cachedCardDescStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
        cachedCardDescStyle.wordWrap = true;

        cachedCardRarityStyle = new GUIStyle();
        cachedCardRarityStyle.fontSize = 10;
        cachedCardRarityStyle.fontStyle = FontStyle.Bold;
        cachedCardRarityStyle.alignment = TextAnchor.MiddleCenter;

        cachedCardChanceStyle = new GUIStyle();
        cachedCardChanceStyle.fontSize = 8;
        cachedCardChanceStyle.alignment = TextAnchor.MiddleCenter;
        cachedCardChanceStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);

        cachedCardDateStyle = new GUIStyle();
        cachedCardDateStyle.fontSize = 8;
        cachedCardDateStyle.alignment = TextAnchor.MiddleCenter;
        cachedCardDateStyle.normal.textColor = new Color(0.5f, 0.7f, 0.5f);

        cachedLockedStyle = new GUIStyle();
        cachedLockedStyle.fontSize = 24;
        cachedLockedStyle.fontStyle = FontStyle.Bold;
        cachedLockedStyle.alignment = TextAnchor.MiddleCenter;
        cachedLockedStyle.normal.textColor = new Color(0.4f, 0.4f, 0.4f);

        cachedCloseStyle = new GUIStyle();
        cachedCloseStyle.fontSize = 14;
        cachedCloseStyle.fontStyle = FontStyle.Bold;
        cachedCloseStyle.alignment = TextAnchor.MiddleCenter;
        cachedCloseStyle.normal.textColor = Color.white;

        cachedNotificationTitleStyle = new GUIStyle();
        cachedNotificationTitleStyle.fontSize = 16;
        cachedNotificationTitleStyle.fontStyle = FontStyle.Bold;
        cachedNotificationTitleStyle.alignment = TextAnchor.MiddleCenter;
        cachedNotificationTitleStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

        cachedNotificationDescStyle = new GUIStyle();
        cachedNotificationDescStyle.fontSize = 12;
        cachedNotificationDescStyle.alignment = TextAnchor.MiddleCenter;
        cachedNotificationDescStyle.normal.textColor = Color.white;

        stylesInitialized = true;
    }

    void DrawUnlockNotification()
    {
        InitializeStyles();

        float alpha = Mathf.Min(1f, unlockNotificationTime);
        float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.05f;

        float notifWidth = 320f * pulse;
        float notifHeight = 100f;
        float notifX = (Screen.width - notifWidth) / 2f;
        float notifY = 80f;

        GUI.color = new Color(1f, 1f, 1f, alpha);

        // Gold border based on rarity
        Color borderColor = GetRarityColor(lastUnlockedAchievement.rarity);
        GUI.DrawTexture(new Rect(notifX - 4, notifY - 4, notifWidth + 8, notifHeight + 8),
            GetOrCreateColorTexture(new Color(borderColor.r, borderColor.g, borderColor.b, alpha)));

        // Background
        GUI.DrawTexture(new Rect(notifX, notifY, notifWidth, notifHeight),
            GetOrCreateColorTexture(new Color(0.1f, 0.08f, 0.06f, 0.98f * alpha)));

        // "ACHIEVEMENT UNLOCKED" header
        cachedNotificationTitleStyle.normal.textColor = new Color(1f, 0.9f, 0.4f, alpha);
        GUI.Label(new Rect(notifX, notifY + 10, notifWidth, 24), "ACHIEVEMENT UNLOCKED!", cachedNotificationTitleStyle);

        // Achievement name
        cachedCardTitleStyle.normal.textColor = new Color(borderColor.r, borderColor.g, borderColor.b, alpha);
        GUI.Label(new Rect(notifX, notifY + 38, notifWidth, 20), lastUnlockedAchievement.name, cachedCardTitleStyle);

        // Achievement description
        cachedNotificationDescStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, alpha);
        GUI.Label(new Rect(notifX + 20, notifY + 60, notifWidth - 40, 30), lastUnlockedAchievement.description, cachedNotificationDescStyle);

        GUI.color = Color.white;
    }

    void DrawAchievementsPanel()
    {
        InitializeStyles();

        // Panel dimensions
        float panelWidth = 500f;
        float panelHeight = 450f;
        float panelX = (Screen.width - panelWidth) / 2f;
        float panelY = (Screen.height - panelHeight) / 2f;

        // Semi-transparent overlay
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height),
            GetOrCreateColorTexture(new Color(0f, 0f, 0f, 0.6f)));

        // Gold border
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));

        // Panel background
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Title
        GUI.Label(new Rect(panelX, panelY + 10, panelWidth, 30), "ACHIEVEMENTS", cachedTitleStyle);

        // Counter
        int unlocked = GetUnlockedCount();
        int total = GetTotalCount();
        float completionPercent = total > 0 ? (float)unlocked / total * 100f : 0f;
        GUI.Label(new Rect(panelX, panelY + 38, panelWidth, 20),
            $"{unlocked}/{total} Unlocked ({completionPercent:F0}%)", cachedCounterStyle);

        // Close button
        Rect closeRect = new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22);
        GUI.DrawTexture(closeRect, GetTexture("closeBtn"));
        GUI.Label(closeRect, "X", cachedCloseStyle);
        if (GUI.Button(closeRect, "", GUIStyle.none))
        {
            panelOpen = false;
        }

        // Divider
        GUI.DrawTexture(new Rect(panelX + 20, panelY + 62, panelWidth - 40, 2), GetTexture("panelBorder"));

        // Achievement cards area
        float cardAreaY = panelY + 70;
        float cardAreaHeight = panelHeight - 90;
        Rect cardArea = new Rect(panelX + 15, cardAreaY, panelWidth - 30, cardAreaHeight);

        // Card dimensions (2 columns)
        float cardWidth = 220f;
        float cardHeight = 100f;
        float cardSpacingX = 15f;
        float cardSpacingY = 10f;

        int columns = 2;
        int rows = Mathf.CeilToInt((float)achievements.Count / columns);
        float totalContentHeight = rows * (cardHeight + cardSpacingY);
        float maxScroll = Mathf.Max(0, totalContentHeight - cardAreaHeight);

        // Handle scrolling
        if (cardArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                scrollPos += Event.current.delta.y * 25f;
                scrollPos = Mathf.Clamp(scrollPos, 0, maxScroll);
                Event.current.Use();
            }
        }

        GUI.BeginGroup(cardArea);

        for (int i = 0; i < achievements.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;

            float cardX = col * (cardWidth + cardSpacingX);
            float cardY = row * (cardHeight + cardSpacingY) - scrollPos;

            // Skip cards outside visible area
            if (cardY + cardHeight < 0 || cardY > cardAreaHeight) continue;

            DrawAchievementCard(new Rect(cardX, cardY, cardWidth, cardHeight), achievements[i]);
        }

        GUI.EndGroup();

        // Scroll indicator
        if (maxScroll > 0)
        {
            float scrollBarHeight = cardAreaHeight * (cardAreaHeight / totalContentHeight);
            float scrollBarY = cardAreaY + (scrollPos / maxScroll) * (cardAreaHeight - scrollBarHeight);
            GUI.DrawTexture(new Rect(panelX + panelWidth - 10, scrollBarY, 4, scrollBarHeight),
                GetOrCreateColorTexture(new Color(0.6f, 0.5f, 0.3f)));
        }

        // ESC to close hint
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 9;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 20, panelWidth, 16), "Press ESC to close", hintStyle);

        // Handle ESC
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            panelOpen = false;
            Event.current.Use();
        }
    }

    void DrawAchievementCard(Rect rect, Achievement achievement)
    {
        Color rarityColor = GetRarityColor(achievement.rarity);
        string rarityBorderTex = GetRarityBorderTexture(achievement.rarity);

        if (achievement.isUnlocked)
        {
            // === UNLOCKED CARD ===

            // Gold/rarity border (3px)
            GUI.DrawTexture(new Rect(rect.x - 3, rect.y - 3, rect.width + 6, rect.height + 6),
                GetTexture(rarityBorderTex));

            // Card background
            GUI.DrawTexture(rect, GetTexture("cardUnlocked"));

            // Rarity label at top
            cachedCardRarityStyle.normal.textColor = rarityColor;
            GUI.Label(new Rect(rect.x, rect.y + 6, rect.width, 14), achievement.rarity.ToString().ToUpper(), cachedCardRarityStyle);

            // Achievement name
            cachedCardTitleStyle.normal.textColor = rarityColor;
            GUI.Label(new Rect(rect.x, rect.y + 22, rect.width, 18), achievement.name, cachedCardTitleStyle);

            // Description
            cachedCardDescStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
            GUI.Label(new Rect(rect.x + 10, rect.y + 42, rect.width - 20, 30), achievement.description, cachedCardDescStyle);

            // Chance/rarity display
            GUI.Label(new Rect(rect.x, rect.y + 72, rect.width, 12), $"Chance: {achievement.chanceDisplay}", cachedCardChanceStyle);

            // Unlock date
            GUI.Label(new Rect(rect.x, rect.y + rect.height - 16, rect.width, 12),
                $"Unlocked: {achievement.unlockDate}", cachedCardDateStyle);
        }
        else
        {
            // === LOCKED CARD ===

            // Dark border
            GUI.DrawTexture(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4),
                GetOrCreateColorTexture(new Color(0.2f, 0.2f, 0.2f)));

            // Dark card background
            GUI.DrawTexture(rect, GetTexture("cardLocked"));

            // Large "???" in center
            GUI.Label(new Rect(rect.x, rect.y + 25, rect.width, 40), "???", cachedLockedStyle);

            // "LOCKED" text
            GUIStyle lockedLabelStyle = new GUIStyle();
            lockedLabelStyle.fontSize = 10;
            lockedLabelStyle.fontStyle = FontStyle.Bold;
            lockedLabelStyle.alignment = TextAnchor.MiddleCenter;
            lockedLabelStyle.normal.textColor = new Color(0.35f, 0.35f, 0.35f);
            GUI.Label(new Rect(rect.x, rect.y + 65, rect.width, 14), "LOCKED", lockedLabelStyle);

            // Rarity hint (dimmed)
            cachedCardRarityStyle.normal.textColor = new Color(rarityColor.r * 0.4f, rarityColor.g * 0.4f, rarityColor.b * 0.4f);
            GUI.Label(new Rect(rect.x, rect.y + rect.height - 16, rect.width, 12),
                achievement.rarity.ToString(), cachedCardRarityStyle);
        }
    }

    Color GetRarityColor(AchievementRarity rarity)
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

    string GetRarityBorderTexture(AchievementRarity rarity)
    {
        switch (rarity)
        {
            case AchievementRarity.Common: return "borderCommon";
            case AchievementRarity.Rare: return "borderRare";
            case AchievementRarity.Epic: return "borderEpic";
            case AchievementRarity.Legendary: return "borderLegendary";
            case AchievementRarity.Mythic: return "borderMythic";
            default: return "borderCommon";
        }
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

// Achievement data class
[System.Serializable]
public class Achievement
{
    public string id;
    public string name;
    public string description;
    public string chanceDisplay;
    public AchievementRarity rarity;
    public bool isUnlocked;
    public string unlockDate;

    public Achievement(string id, string name, string description, string chanceDisplay, AchievementRarity rarity)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.chanceDisplay = chanceDisplay;
        this.rarity = rarity;
        this.isUnlocked = false;
        this.unlockDate = "";
    }
}

// Achievement rarity tiers
public enum AchievementRarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    Mythic
}
