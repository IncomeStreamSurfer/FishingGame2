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
    private Rect windowRect = new Rect(20, 20, 320, 450);

    // Input fields
    private string goldInput = "1000";
    private string xpInput = "10000";
    private string levelInput = "10";

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
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        initialized = true;
    }

    void CreateCachedTextures()
    {
        CacheTexture("panelBg", new Color(0.08f, 0.08f, 0.12f, 0.98f));
        CacheTexture("headerBg", new Color(0.6f, 0.2f, 0.2f, 1f));
        CacheTexture("sectionBg", new Color(0.12f, 0.12f, 0.18f, 0.95f));
        CacheTexture("buttonNormal", new Color(0.2f, 0.4f, 0.6f, 1f));
        CacheTexture("buttonHover", new Color(0.3f, 0.5f, 0.7f, 1f));
        CacheTexture("buttonDanger", new Color(0.6f, 0.2f, 0.2f, 1f));
        CacheTexture("inputBg", new Color(0.05f, 0.05f, 0.08f, 1f));
        CacheTexture("divider", new Color(0.4f, 0.4f, 0.5f, 0.5f));
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

    void Update()
    {
        // Toggle with F12
        if (Input.GetKeyDown(KeyCode.F12))
        {
            isOpen = !isOpen;
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
        if (!isOpen || !initialized || !MainMenu.GameStarted) return;

        // Main panel
        GUI.DrawTexture(windowRect, GetTexture("panelBg"));

        // Header bar (draggable)
        Rect headerRect = new Rect(windowRect.x, windowRect.y, windowRect.width, 30);
        GUI.DrawTexture(headerRect, GetTexture("headerBg"));

        // Header title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;
        GUI.Label(headerRect, "DEV PANEL (F12)", titleStyle);

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

        // Close button
        GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
        closeStyle.normal.textColor = Color.white;
        closeStyle.fontSize = 12;
        if (GUI.Button(new Rect(windowRect.x + windowRect.width - 25, windowRect.y + 5, 20, 20), "X", closeStyle))
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
            GUIStyle statStyle = new GUIStyle(GUI.skin.label);
            statStyle.fontSize = 12;
            statStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);

            int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;
            long xp = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetCurrentXP() : 0;
            int gold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
            int fish = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;

            GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"Level: {level} / {LevelingSystem.MAX_LEVEL}", statStyle);
            contentY += 20;
            GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"XP: {xp:N0} / {LevelingSystem.MAX_XP:N0}", statStyle);
            contentY += 20;
            GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"Gold: {gold:N0}", statStyle);
            contentY += 20;
            GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), $"Fish Caught: {fish}", statStyle);
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

        GUIStyle sectionTitle = new GUIStyle(GUI.skin.label);
        sectionTitle.fontSize = 11;
        sectionTitle.fontStyle = FontStyle.Bold;
        sectionTitle.normal.textColor = new Color(1f, 0.8f, 0.4f);
        GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), "QUICK ACTIONS", sectionTitle);
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

        // Danger zone
        GUIStyle dangerTitle = new GUIStyle(sectionTitle);
        dangerTitle.normal.textColor = new Color(1f, 0.4f, 0.4f);
        GUI.Label(new Rect(windowRect.x + padding, contentY, contentWidth, 18), "DANGER ZONE", dangerTitle);
        contentY += 22;

        if (DrawButton(new Rect(windowRect.x + padding, contentY, contentWidth, 25), "Reset All Progress", true))
        {
            ResetProgress();
        }
    }

    void DrawSection(string title, ref float y, float padding, float width, System.Action content)
    {
        GUIStyle sectionTitle = new GUIStyle(GUI.skin.label);
        sectionTitle.fontSize = 11;
        sectionTitle.fontStyle = FontStyle.Bold;
        sectionTitle.normal.textColor = new Color(1f, 0.8f, 0.4f);

        GUI.Label(new Rect(windowRect.x + padding, y, width, 18), title, sectionTitle);
        y += 20;
        content();
    }

    void DrawInputWithButtons(ref float y, float padding, float width, ref string inputValue, string label,
        System.Action onApply, string[] presetLabels, int[] presetValues)
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 11;
        labelStyle.normal.textColor = Color.white;

        GUIStyle inputStyle = new GUIStyle(GUI.skin.textField);
        inputStyle.fontSize = 12;
        inputStyle.normal.background = GetTexture("inputBg");
        inputStyle.normal.textColor = Color.white;
        inputStyle.alignment = TextAnchor.MiddleCenter;

        // Label and input
        GUI.Label(new Rect(windowRect.x + padding, y + 2, 45, 20), label, labelStyle);
        inputValue = GUI.TextField(new Rect(windowRect.x + padding + 50, y, 80, 22), inputValue, inputStyle);

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

        GUIStyle btnStyle = new GUIStyle(GUI.skin.label);
        btnStyle.fontSize = 11;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = Color.white;

        GUI.Label(rect, text, btnStyle);

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

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}
