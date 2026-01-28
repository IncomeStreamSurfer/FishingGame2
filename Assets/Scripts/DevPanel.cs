using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Developer Control Panel - Press F12 to toggle
/// Allows testing by modifying gold, XP, and levels
/// </summary>
public class DevPanel : MonoBehaviour
{
    public static DevPanel Instance { get; private set; }

    private bool isOpen = false;
    private bool isDragging = false;
    private Vector2 dragOffset;
    private Rect windowRect = new Rect(20, 20, 320, 850); // Increased height for fish spawner

    // Fish spawner scroll position
    private Vector2 fishScrollPos = Vector2.zero;

    // Input fields
    private string goldInput = "1000";
    private string xpInput = "10000";
    private string levelInput = "10";

    // Time control
    private float timeSliderValue = 0.5f; // 0-1 representing time of day

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;
    private int guiFrameSkip = 0;

    // Cached GUIStyles (initialized once in OnGUI to avoid allocations)
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedCloseStyle;
    private static GUIStyle cachedStatStyle;
    private static GUIStyle cachedSectionTitle;
    private static GUIStyle cachedLabelStyle;
    private static GUIStyle cachedInputStyle;
    private static GUIStyle cachedBtnStyle;
    private static GUIStyle cachedTimeLabel;
    private static bool stylesInitialized = false;

    void Awake()
    {
        // Disable in release mode
        if (GameConfig.RELEASE_MODE)
        {
            Destroy(gameObject);
            return;
        }

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GameConfig.RELEASE_MODE) return;
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        initialized = true;
    }

    void CreateCachedTextures()
    {
        // Consistent UI style matching CharacterPanel
        CacheTexture("panelBg", new Color(0.1f, 0.1f, 0.12f, 0.95f));
        CacheTexture("headerBg", new Color(0.6f, 0.2f, 0.2f, 1f));
        CacheTexture("sectionBg", new Color(0.15f, 0.15f, 0.17f, 0.95f));
        CacheTexture("buttonNormal", new Color(0.2f, 0.4f, 0.6f, 1f));
        CacheTexture("buttonHover", new Color(0.3f, 0.5f, 0.7f, 1f));
        CacheTexture("buttonDanger", new Color(0.6f, 0.2f, 0.2f, 1f));
        CacheTexture("inputBg", new Color(0.05f, 0.05f, 0.08f, 1f));
        CacheTexture("divider", new Color(1f, 0.85f, 0.4f, 0.8f)); // Gold divider
        CacheTexture("sliderTrack", new Color(0.15f, 0.15f, 0.2f, 1f));
        CacheTexture("sliderFill", new Color(1f, 0.7f, 0.2f, 1f));
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

    void InitializeCachedStyles()
    {
        cachedTitleStyle = new GUIStyle(GUI.skin.label);
        cachedTitleStyle.fontSize = 14;
        cachedTitleStyle.fontStyle = FontStyle.Bold;
        cachedTitleStyle.alignment = TextAnchor.MiddleCenter;
        cachedTitleStyle.normal.textColor = Color.white;

        cachedCloseStyle = new GUIStyle(GUI.skin.button);
        cachedCloseStyle.normal.textColor = Color.white;
        cachedCloseStyle.fontSize = 12;

        cachedStatStyle = new GUIStyle(GUI.skin.label);
        cachedStatStyle.fontSize = 12;
        cachedStatStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);

        cachedSectionTitle = new GUIStyle(GUI.skin.label);
        cachedSectionTitle.fontSize = 11;
        cachedSectionTitle.fontStyle = FontStyle.Bold;
        cachedSectionTitle.normal.textColor = new Color(1f, 0.8f, 0.4f);

        cachedLabelStyle = new GUIStyle(GUI.skin.label);
        cachedLabelStyle.fontSize = 11;
        cachedLabelStyle.normal.textColor = Color.white;

        cachedInputStyle = new GUIStyle(GUI.skin.textField);
        cachedInputStyle.fontSize = 12;
        cachedInputStyle.normal.background = GetTexture("inputBg");
        cachedInputStyle.normal.textColor = Color.white;
        cachedInputStyle.alignment = TextAnchor.MiddleCenter;

        cachedBtnStyle = new GUIStyle(GUI.skin.label);
        cachedBtnStyle.fontSize = 11;
        cachedBtnStyle.fontStyle = FontStyle.Bold;
        cachedBtnStyle.alignment = TextAnchor.MiddleCenter;
        cachedBtnStyle.normal.textColor = Color.white;

        cachedTimeLabel = new GUIStyle(GUI.skin.label);
        cachedTimeLabel.fontSize = 11;
        cachedTimeLabel.normal.textColor = Color.white;

        stylesInitialized = true;
    }

    void Update()
    {
        // Toggle with F12
        if (Input.GetKeyDown(KeyCode.F12))
        {
            isOpen = !isOpen;
        }

        // Close with ESC
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = false;
        }

        // Handle dragging
        if (isDragging)
        {
            windowRect.x = Input.mousePosition.x - dragOffset.x;
            windowRect.y = Screen.height - Input.mousePosition.y - dragOffset.y;

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
    }

    void OnGUI()
    {
        // Performance: Skip frames when not actively needed
        if (!isOpen)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        if (!isOpen || !initialized || !MainMenu.GameStarted) return;

        // Initialize cached styles once (must be done in OnGUI context)
        if (!stylesInitialized)
        {
            InitializeCachedStyles();
        }

        // Main panel
        GUI.DrawTexture(windowRect, GetTexture("panelBg"));

        // Header bar (draggable)
        Rect headerRect = new Rect(windowRect.x, windowRect.y, windowRect.width, 30);
        GUI.DrawTexture(headerRect, GetTexture("headerBg"));

        // Header title - use cached style
        GUI.Label(headerRect, "DEV PANEL (F12)", cachedTitleStyle);

        // Handle dragging
        if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
        {
            isDragging = true;
            dragOffset = new Vector2(
                Input.mousePosition.x - windowRect.x,
                Screen.height - Input.mousePosition.y - windowRect.y
            );
            Event.current.Use();
        }

        // Close button - use cached style
        if (GUI.Button(new Rect(windowRect.x + windowRect.width - 25, windowRect.y + 5, 20, 20), "X", cachedCloseStyle))
        {
            isOpen = false;
        }

        // Content area
        float contentY = windowRect.y + 40;
        float padding = 10;
        float contentWidth = windowRect.width - padding * 2;

        // Current Stats Section
        DrawSection("CURRENT STATS", ref contentY, padding, contentWidth, () =>
        {
            int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;
            long xp = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetCurrentXP() : 0;
            int gold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
            int fish = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;

            GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"Level: {level} / {LevelingSystem.MAX_LEVEL}", cachedStatStyle);
            contentY += 20;
            GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"XP: {xp:N0} / {LevelingSystem.MAX_XP:N0}", cachedStatStyle);
            contentY += 20;
            GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"Gold: {gold:N0}", cachedStatStyle);
            contentY += 20;
            GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"Fish Caught: {fish}", cachedStatStyle);
            contentY += 25;
        });

        // Divider
        GUI.DrawTexture(new Rect(windowRect.x + padding, contentY, contentWidth, 1), GetTexture("divider"));
        contentY += 10;

        // Gold Section
        DrawSection("ADD GOLD", ref contentY, padding, contentWidth, () =>
        {
            DrawInputWithButtons(ref contentY, padding, contentWidth, ref goldInput, "Gold:",
                () => AddGold(int.Parse(goldInput)),
                new string[] { "100", "1K", "10K", "100K" },
                new int[] { 100, 1000, 10000, 100000 });
        });

        contentY += 5;

        // XP Section
        DrawSection("ADD XP", ref contentY, padding, contentWidth, () =>
        {
            DrawInputWithButtons(ref contentY, padding, contentWidth, ref xpInput, "XP:",
                () => AddXP(long.Parse(xpInput)),
                new string[] { "1K", "10K", "100K", "1M" },
                new int[] { 1000, 10000, 100000, 1000000 });
        });

        contentY += 5;

        // Level Section
        DrawSection("SET LEVEL", ref contentY, padding, contentWidth, () =>
        {
            DrawInputWithButtons(ref contentY, padding, contentWidth, ref levelInput, "Level:",
                () => SetLevel(int.Parse(levelInput)),
                new string[] { "10", "50", "100", "320" },
                new int[] { 10, 50, 100, 320 });
        });

        contentY += 10;

        // Quick Actions
        GUI.DrawTexture(new Rect(windowRect.x + padding, contentY, contentWidth, 1), GetTexture("divider"));
        contentY += 10;

        GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), "QUICK ACTIONS", cachedSectionTitle);
        contentY += 22;

        // Quick action buttons
        float btnWidth = (contentWidth - 10) / 2;

        if (DrawButton(new Rect(windowRect.x + padding, contentY, btnWidth, 25), "Max Level"))
        {
            SetLevel(LevelingSystem.MAX_LEVEL);
        }
        if (DrawButton(new Rect(windowRect.x + padding + btnWidth + 10, contentY, btnWidth, 25), "Max Gold"))
        {
            AddGold(999999);
        }
        contentY += 30;

        if (DrawButton(new Rect(windowRect.x + padding, contentY, btnWidth, 25), "+50 Levels"))
        {
            int currentLvl = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;
            SetLevel(Mathf.Min(currentLvl + 50, LevelingSystem.MAX_LEVEL));
        }
        if (DrawButton(new Rect(windowRect.x + padding + btnWidth + 10, contentY, btnWidth, 25), "+100 Fish"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.totalFishCaught += 100;
            }
        }
        contentY += 35;

        // Time Control Section
        GUI.DrawTexture(new Rect(windowRect.x + padding, contentY, contentWidth, 1), GetTexture("divider"));
        contentY += 10;

        // Use section title with different color for time
        Color prevColor = cachedSectionTitle.normal.textColor;
        cachedSectionTitle.normal.textColor = new Color(0.4f, 0.8f, 1f);
        GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), "TIME OF DAY", cachedSectionTitle);
        cachedSectionTitle.normal.textColor = prevColor;
        contentY += 22;

        // Get current time from DayNightCycle (0-24 hours)
        float currentHour = 12f;
        if (DayNightCycle.Instance != null)
        {
            currentHour = DayNightCycle.Instance.GetCurrentHour();
            timeSliderValue = currentHour / 24f; // Convert to 0-1 for slider
        }

        // Time label - use cached style
        string timeStr = GetTimeString(timeSliderValue);
        GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"Time: {timeStr}", cachedTimeLabel);
        contentY += 20;

        // Draw slider track
        Rect sliderTrackRect = new Rect(windowRect.x + padding, contentY, contentWidth, 16);
        GUI.DrawTexture(sliderTrackRect, GetTexture("sliderTrack"));

        // Draw slider fill
        Rect sliderFillRect = new Rect(windowRect.x + padding, contentY, contentWidth * timeSliderValue, 16);
        GUI.DrawTexture(sliderFillRect, GetTexture("sliderFill"));

        // Slider interaction
        if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
        {
            if (sliderTrackRect.Contains(Event.current.mousePosition))
            {
                float newValue = (Event.current.mousePosition.x - sliderTrackRect.x) / sliderTrackRect.width;
                timeSliderValue = Mathf.Clamp01(newValue);
                if (DayNightCycle.Instance != null)
                {
                    DayNightCycle.Instance.SetTimeOfDay(timeSliderValue * 24f); // Convert back to 0-24
                }
                Event.current.Use();
            }
        }
        contentY += 22;

        // Time preset buttons
        float timeBtnWidth = (contentWidth - 30) / 4;
        string[] timeLabels = { "Dawn", "Noon", "Dusk", "Night" };
        float[] timeHours = { 6f, 12f, 18f, 0f }; // In hours
        for (int i = 0; i < timeLabels.Length; i++)
        {
            float hour = timeHours[i];
            if (DrawButton(new Rect(windowRect.x + padding + i * (timeBtnWidth + 10), contentY, timeBtnWidth, 22), timeLabels[i]))
            {
                timeSliderValue = hour / 24f;
                if (DayNightCycle.Instance != null)
                {
                    DayNightCycle.Instance.SetTimeOfDay(hour);
                }
            }
        }
        contentY += 30;

        // Weather Control Section
        GUI.DrawTexture(new Rect(windowRect.x + padding, contentY, contentWidth, 1), GetTexture("divider"));
        contentY += 10;

        prevColor = cachedSectionTitle.normal.textColor;
        cachedSectionTitle.normal.textColor = new Color(0.8f, 0.6f, 1f); // Purple for weather
        GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), "WEATHER EVENTS", cachedSectionTitle);
        cachedSectionTitle.normal.textColor = prevColor;
        contentY += 22;

        if (DrawButton(new Rect(windowRect.x + padding, contentY, btnWidth, 25), "Lightning Strike"))
        {
            TriggerLightningStrike();
        }
        if (DrawButton(new Rect(windowRect.x + padding + btnWidth + 10, contentY, btnWidth, 25), "Find Parrot"))
        {
            TriggerParrotFind();
        }
        contentY += 35;

        // Fish Spawner Section
        GUI.DrawTexture(new Rect(windowRect.x + padding, contentY, contentWidth, 1), GetTexture("divider"));
        contentY += 10;

        prevColor = cachedSectionTitle.normal.textColor;
        cachedSectionTitle.normal.textColor = new Color(0.4f, 1f, 0.8f); // Cyan for fish
        GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), "FISH SPAWNER", cachedSectionTitle);
        cachedSectionTitle.normal.textColor = prevColor;
        contentY += 22;

        // Fish list in scrollable area
        DrawFishSpawner(ref contentY, padding, contentWidth);
        contentY += 10;

        // Danger zone - use section title with different color
        prevColor = cachedSectionTitle.normal.textColor;
        cachedSectionTitle.normal.textColor = new Color(1f, 0.4f, 0.4f);
        GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), "DANGER ZONE", cachedSectionTitle);
        cachedSectionTitle.normal.textColor = prevColor;
        contentY += 22;

        if (DrawButton(new Rect(windowRect.x + padding, contentY, contentWidth, 25), "Reset All Progress", true))
        {
            ResetProgress();
        }
    }

    void DrawFishSpawner(ref float contentY, float padding, float contentWidth)
    {
        // Fish categories with their IDs
        string[][] fishCategories = new string[][]
        {
            // Common
            new string[] { "sardine", "anchovy", "minnow", "cod" },
            // Uncommon
            new string[] { "bass", "salmon", "baby_turtle", "jellyfish" },
            // Rare
            new string[] { "tuna", "swordfish", "hammerhead", "ocean_eel" },
            // Special (Cookable)
            new string[] { "red_snapper", "blue_marlin", "rainbow_trout", "sunshore_od", "icelandic_snubnose", "seahorse" },
            // Epic
            new string[] { "shark", "sting_ray", "rainbow_fish", "whale_baby" },
            // Legendary
            new string[] { "whale", "dorgush_wrangler", "danish_warblecock" },
            // Mythic
            new string[] { "golden_starfish" }
        };

        string[] categoryNames = { "Common", "Uncommon", "Rare", "Special", "Epic", "Legendary", "Mythic" };
        Color[] categoryColors = {
            new Color(0.7f, 0.7f, 0.7f),  // Common - gray
            new Color(0.4f, 0.8f, 0.4f),  // Uncommon - green
            new Color(0.4f, 0.6f, 1f),    // Rare - blue
            new Color(1f, 0.8f, 0.3f),    // Special - gold
            new Color(0.8f, 0.4f, 1f),    // Epic - purple
            new Color(1f, 0.6f, 0.2f),    // Legendary - orange
            new Color(1f, 0.3f, 0.3f)     // Mythic - red
        };

        float scrollHeight = 150;
        Rect scrollViewRect = new Rect(windowRect.x + padding, contentY, contentWidth, scrollHeight);
        Rect scrollContentRect = new Rect(0, 0, contentWidth - 20, 400); // Approximate content height

        fishScrollPos = GUI.BeginScrollView(scrollViewRect, fishScrollPos, scrollContentRect);

        float y = 0;
        float smallBtnWidth = 65;
        float smallBtnHeight = 20;

        for (int cat = 0; cat < fishCategories.Length; cat++)
        {
            // Category label
            GUIStyle catStyle = new GUIStyle();
            catStyle.fontSize = 11;
            catStyle.fontStyle = FontStyle.Bold;
            catStyle.normal.textColor = categoryColors[cat];
            GUI.Label(new Rect(0, y, contentWidth, 16), categoryNames[cat] + ":", catStyle);
            y += 18;

            // Fish buttons in rows of 4
            for (int i = 0; i < fishCategories[cat].Length; i++)
            {
                int col = i % 4;
                if (i > 0 && col == 0) y += smallBtnHeight + 2;

                string fishId = fishCategories[cat][i];
                string displayName = GetShortFishName(fishId);

                Rect btnRect = new Rect(col * (smallBtnWidth + 5), y, smallBtnWidth, smallBtnHeight);
                bool hover = btnRect.Contains(Event.current.mousePosition);

                GUI.DrawTexture(btnRect, hover ? GetTexture("buttonHover") : GetTexture("buttonNormal"));

                GUIStyle btnStyle = new GUIStyle();
                btnStyle.fontSize = 9;
                btnStyle.alignment = TextAnchor.MiddleCenter;
                btnStyle.normal.textColor = Color.white;
                GUI.Label(btnRect, displayName, btnStyle);

                if (GUI.Button(btnRect, "", GUIStyle.none))
                {
                    SpawnFish(fishId);
                }
            }
            y += smallBtnHeight + 8;
        }

        GUI.EndScrollView();
        contentY += scrollHeight + 5;
    }

    string GetShortFishName(string fishId)
    {
        // Shorten long fish names for buttons
        return fishId switch
        {
            "sardine" => "Sardine",
            "anchovy" => "Anchovy",
            "minnow" => "Minnow",
            "cod" => "Cod",
            "bass" => "Bass",
            "salmon" => "Salmon",
            "baby_turtle" => "Turtle",
            "jellyfish" => "Jelly",
            "tuna" => "Tuna",
            "swordfish" => "Sword",
            "hammerhead" => "Hammer",
            "ocean_eel" => "Eel",
            "red_snapper" => "Snapper",
            "blue_marlin" => "Marlin",
            "rainbow_trout" => "Trout",
            "sunshore_od" => "Sunshore",
            "icelandic_snubnose" => "Snubnose",
            "seahorse" => "Seahorse",
            "shark" => "Shark",
            "sting_ray" => "Ray",
            "rainbow_fish" => "Rainbow",
            "whale_baby" => "BabyWhale",
            "whale" => "Whale",
            "dorgush_wrangler" => "Dorgush",
            "danish_warblecock" => "Warble",
            "golden_starfish" => "GOLDEN",
            _ => fishId.Substring(0, Mathf.Min(7, fishId.Length))
        };
    }

    void SpawnFish(string fishId)
    {
        if (FishingSystem.Instance == null)
        {
            Debug.LogWarning("FishingSystem not found!");
            return;
        }

        FishData fish = FishingSystem.Instance.GetFishById(fishId);
        if (fish == null)
        {
            Debug.LogWarning($"Fish not found: {fishId}");
            return;
        }

        // Special fish go to special inventory
        if (fish.isSpecialFish)
        {
            FishingSystem.Instance.AddSpecialFish(fish);
        }
        else
        {
            // Normal fish go to GameManager inventory
            GameManager.Instance.AddFish(fish);
        }

        Debug.Log($"[DevPanel] Spawned {fish.fishName} into inventory!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"DEV: Added {fish.fishName}", new Color(0.4f, 1f, 0.8f));
        }
    }

    void DrawSection(string title, ref float y, float padding, float width, System.Action content)
    {
        GUI.Label(new Rect(windowRect.x + padding, y, width, 18), title, cachedSectionTitle);
        y += 20;
        content();
    }

    void DrawInputWithButtons(ref float y, float padding, float width, ref string inputValue, string label,
        System.Action onApply, string[] presetLabels, int[] presetValues)
    {
        // Label and input - use cached styles
        GUI.Label(new Rect(windowRect.x + padding, y + 2, 45, 20), label, cachedLabelStyle);
        inputValue = GUI.TextField(new Rect(windowRect.x + padding + 50, y, 80, 22), inputValue, cachedInputStyle);

        // Apply button
        if (DrawButton(new Rect(windowRect.x + padding + 140, y, 60, 22), "Apply"))
        {
            onApply();
        }
        y += 28;

        // Preset buttons
        float btnWidth = (width - 30) / 4;
        for (int i = 0; i < presetLabels.Length; i++)
        {
            int val = presetValues[i];
            if (DrawButton(new Rect(windowRect.x + padding + i * (btnWidth + 10), y, btnWidth, 22), presetLabels[i]))
            {
                inputValue = val.ToString();
                onApply();
            }
        }
        y += 28;
    }

    bool DrawButton(Rect rect, string text, bool isDanger = false)
    {
        bool hover = rect.Contains(Event.current.mousePosition);

        Texture2D bgTex = isDanger ? GetTexture("buttonDanger") :
                          (hover ? GetTexture("buttonHover") : GetTexture("buttonNormal"));
        GUI.DrawTexture(rect, bgTex);

        // Use cached button style
        GUI.Label(rect, text, cachedBtnStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    void AddGold(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(amount);
            Debug.Log($"[DEV] Added {amount} gold");
        }
    }

    void AddXP(long amount)
    {
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.AddXP(amount);
            Debug.Log($"[DEV] Added {amount} XP");
        }
    }

    void SetLevel(int targetLevel)
    {
        if (LevelingSystem.Instance != null)
        {
            // Get XP required for target level and set it
            long xpNeeded = LevelingSystem.Instance.GetXPForLevel(targetLevel);
            long currentXP = LevelingSystem.Instance.GetCurrentXP();

            if (xpNeeded > currentXP)
            {
                LevelingSystem.Instance.AddXP(xpNeeded - currentXP + 1);
            }
            Debug.Log($"[DEV] Set level to {targetLevel}");
        }
    }

    void ResetProgress()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = 0;
            GameManager.Instance.totalFishCaught = 0;
            GameManager.Instance.fishInventory.Clear();
        }
        Debug.Log("[DEV] Reset all progress");
    }

    void TriggerLightningStrike()
    {
        if (ThunderstormSystem.Instance != null)
        {
            // Use reflection to call StartStorm since it's private
            var startStormMethod = typeof(ThunderstormSystem).GetMethod("StartStorm",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (startStormMethod != null)
            {
                startStormMethod.Invoke(ThunderstormSystem.Instance, null);
                Debug.Log("[DEV] Thunderstorm triggered!");

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("DEV: Thunderstorm triggered!", new Color(0.8f, 0.6f, 1f));
                }
            }
            else
            {
                Debug.LogError("[DEV] Could not find StartStorm method");
            }
        }
        else
        {
            Debug.LogError("[DEV] ThunderstormSystem not found in scene");
        }
    }

    void TriggerParrotFind()
    {
        if (ShoulderParrot.Instance != null)
        {
            // Check if already unlocked
            if (ShoulderParrot.Instance.HasParrotUnlocked())
            {
                Debug.Log("[DEV] Parrot already unlocked! Re-triggering animation anyway.");
            }

            ShoulderParrot.Instance.OnParrotFishedUp();
            Debug.Log("[DEV] Parrot find triggered!");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("DEV: Parrot find triggered!", new Color(0.4f, 1f, 0.5f));
            }
        }
        else
        {
            Debug.LogError("[DEV] ShoulderParrot not found in scene");
        }
    }

    string GetTimeString(float timeValue)
    {
        // Convert 0-1 to 24-hour time (0 = midnight, 0.5 = noon)
        float hours = timeValue * 24f;
        int hour = Mathf.FloorToInt(hours);
        int minutes = Mathf.FloorToInt((hours - hour) * 60f);
        string ampm = hour >= 12 ? "PM" : "AM";
        int displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12;
        return $"{displayHour}:{minutes:D2} {ampm}";
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
