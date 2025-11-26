using UnityEngine;
using System.Collections.Generic;

public class CharacterPanel : MonoBehaviour
{
    public static CharacterPanel Instance { get; private set; }

    private bool isOpen = false;

    // Character info
    private string characterName = "The Fisherman";
    private int characterAge = 42;

    // Simple heartbeat
    private float heartbeatTime = 0f;
    private int bpm = 72;

    // Equipment slots
    private string[] equipmentSlots = { "Head", "Body", "Legs", "Feet", "Rod", "Bait", "Accessory" };
    private string[] equippedItems = { "Fishing Hat", "Blue Shirt", "Brown Pants", "Leather Boots", "Basic Rod", "Worm", "None" };

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
        CacheTexture("panelBg", new Color(0.12f, 0.12f, 0.15f, 0.95f));
        CacheTexture("panelBorder", new Color(0.4f, 0.35f, 0.2f, 1f));
        CacheTexture("slotBg", new Color(0.2f, 0.2f, 0.25f, 0.9f));
        CacheTexture("divider", new Color(0.4f, 0.35f, 0.2f, 0.8f));
        CacheTexture("modelBg", new Color(0.08f, 0.08f, 0.1f, 1f));
        CacheTexture("heartIcon", new Color(0.9f, 0.2f, 0.2f, 1f));
        CacheTexture("goldIcon", new Color(1f, 0.85f, 0.2f, 1f));
        CacheTexture("xpBarBg", new Color(0.15f, 0.15f, 0.2f, 1f));
        CacheTexture("xpBarFill", new Color(0.3f, 0.7f, 0.3f, 1f));
        CacheTexture("skin", new Color(0.85f, 0.7f, 0.55f, 1f));
        CacheTexture("hat", new Color(0.7f, 0.6f, 0.4f, 1f));
        CacheTexture("shirt", new Color(0.15f, 0.30f, 0.55f, 1f));
        CacheTexture("pants", new Color(0.22f, 0.18f, 0.12f, 1f));
        CacheTexture("boots", new Color(0.15f, 0.12f, 0.08f, 1f));
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
        if (textureCache.TryGetValue(name, out Texture2D tex))
        {
            return tex;
        }
        return Texture2D.whiteTexture;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;
        }
        heartbeatTime += Time.deltaTime;
    }

    void OnGUI()
    {
        if (!isOpen || !initialized || !MainMenu.GameStarted) return;

        float panelWidth = 400f;
        float panelHeight = 480f;
        float panelX = (Screen.width - panelWidth) / 2f;
        float panelY = (Screen.height - panelHeight) / 2f;

        // Panel background
        GUI.DrawTexture(new Rect(panelX - 4, panelY - 4, panelWidth + 8, panelHeight + 8), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);
        GUI.Label(new Rect(panelX, panelY + 10, panelWidth, 30), "CHARACTER", titleStyle);

        GUI.DrawTexture(new Rect(panelX + 20, panelY + 45, panelWidth - 40, 2), GetTexture("divider"));

        // Character model area
        float modelX = panelX + 20;
        float modelY = panelY + 55;
        GUI.DrawTexture(new Rect(modelX, modelY, 140, 200), GetTexture("modelBg"));
        DrawSimpleCharacter(modelX, modelY, 140, 200);

        // Name
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontSize = 14;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(modelX, modelY + 205, 140, 20), characterName, nameStyle);

        // Stats on right side
        float statsX = panelX + 175;
        float statsY = modelY;

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 13;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.8f, 0.75f, 0.5f);

        GUIStyle statStyle = new GUIStyle(GUI.skin.label);
        statStyle.fontSize = 12;
        statStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(statsX, statsY, 200, 18), "STATS", headerStyle);
        statsY += 22;

        int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;
        GUI.Label(new Rect(statsX, statsY, 200, 18), "Level: " + level, statStyle);
        statsY += 20;

        GUI.Label(new Rect(statsX, statsY, 200, 18), "Age: " + characterAge, statStyle);
        statsY += 20;

        // Simple heartbeat display
        float beatCycle = heartbeatTime * (bpm / 60f);
        bool isPeak = (beatCycle % 1f) < 0.15f;
        float heartSize = isPeak ? 16f : 12f;

        GUI.DrawTexture(new Rect(statsX, statsY + 2, heartSize, heartSize), GetTexture("heartIcon"));
        GUI.Label(new Rect(statsX + 22, statsY, 100, 18), bpm + " BPM", statStyle);
        statsY += 25;

        // Gold
        int gold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUI.DrawTexture(new Rect(statsX, statsY + 1, 14, 14), GetTexture("goldIcon"));
        GUIStyle goldStyle = new GUIStyle(statStyle);
        goldStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);
        GUI.Label(new Rect(statsX + 20, statsY, 100, 18), gold.ToString("N0"), goldStyle);
        statsY += 22;

        // XP
        if (LevelingSystem.Instance != null)
        {
            float progress = LevelingSystem.Instance.GetProgressToNextLevel();
            GUI.Label(new Rect(statsX, statsY, 200, 18), "XP Progress:", statStyle);
            statsY += 18;
            GUI.DrawTexture(new Rect(statsX, statsY, 180, 10), GetTexture("xpBarBg"));
            GUI.DrawTexture(new Rect(statsX + 1, statsY + 1, 178 * progress, 8), GetTexture("xpBarFill"));
            statsY += 18;
        }

        // Fish caught
        int fishCaught = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;
        GUI.Label(new Rect(statsX, statsY, 200, 18), "Fish: " + fishCaught, statStyle);
        statsY += 30;

        // Equipment
        GUI.Label(new Rect(statsX, statsY, 200, 18), "EQUIPMENT", headerStyle);
        statsY += 22;

        GUIStyle slotStyle = new GUIStyle(GUI.skin.label);
        slotStyle.fontSize = 10;
        slotStyle.normal.textColor = new Color(0.6f, 0.6f, 0.65f);

        GUIStyle itemStyle = new GUIStyle(GUI.skin.label);
        itemStyle.fontSize = 11;
        itemStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);

        for (int i = 0; i < equipmentSlots.Length && i < 7; i++)
        {
            GUI.DrawTexture(new Rect(statsX, statsY, 200, 22), GetTexture("slotBg"));
            GUI.Label(new Rect(statsX + 5, statsY + 2, 50, 18), equipmentSlots[i] + ":", slotStyle);

            string item = equippedItems[i];
            itemStyle.normal.textColor = item == "None" ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.5f, 0.8f, 1f);
            GUI.Label(new Rect(statsX + 60, statsY + 2, 140, 18), item, itemStyle);
            statsY += 24;
        }

        // Close hint
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 11;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 25, panelWidth, 20), "Press TAB to close", hintStyle);
    }

    void DrawSimpleCharacter(float x, float y, float width, float height)
    {
        float cx = x + width / 2;
        float sy = y + 20;

        // Head
        GUI.DrawTexture(new Rect(cx - 15, sy, 30, 35), GetTexture("skin"));
        // Hat
        GUI.DrawTexture(new Rect(cx - 20, sy - 8, 40, 12), GetTexture("hat"));
        // Body
        GUI.DrawTexture(new Rect(cx - 18, sy + 40, 36, 50), GetTexture("shirt"));
        // Pants
        GUI.DrawTexture(new Rect(cx - 16, sy + 90, 32, 45), GetTexture("pants"));
        // Boots
        GUI.DrawTexture(new Rect(cx - 16, sy + 135, 14, 12), GetTexture("boots"));
        GUI.DrawTexture(new Rect(cx + 2, sy + 135, 14, 12), GetTexture("boots"));
    }

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void SetEquipment(int slot, string itemName)
    {
        if (slot >= 0 && slot < equippedItems.Length)
        {
            equippedItems[slot] = itemName;
        }
    }
}
