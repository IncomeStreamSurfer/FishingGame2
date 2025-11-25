using UnityEngine;
using System.Collections.Generic;

public class CharacterPanel : MonoBehaviour
{
    public static CharacterPanel Instance { get; private set; }

    private bool isOpen = false;

    // Character info
    private string characterName = "The Fisherman";
    private int characterAge = 42;
    private float heartbeatTimer = 0f;
    private bool heartbeatPulse = false;

    // Equipment slots
    private string[] equipmentSlots = { "Head", "Body", "Legs", "Feet", "Rod", "Bait", "Accessory" };
    private string[] equippedItems = { "Fishing Hat", "Blue Shirt", "Brown Pants", "Leather Boots", "Basic Rod", "Worm", "None" };

    // Cached textures - IMPORTANT: Create once, reuse always
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pre-create all textures we need (only once!)
        CreateCachedTextures();
    }

    void CreateCachedTextures()
    {
        // Panel backgrounds
        CacheTexture("panelBg", new Color(0.12f, 0.12f, 0.15f, 0.95f));
        CacheTexture("panelBorder", new Color(0.4f, 0.35f, 0.2f, 1f));
        CacheTexture("slotBg", new Color(0.2f, 0.2f, 0.25f, 0.9f));
        CacheTexture("divider", new Color(0.4f, 0.35f, 0.2f, 0.8f));
        CacheTexture("modelBg", new Color(0.08f, 0.08f, 0.1f, 1f));

        // Icons
        CacheTexture("heartIcon", new Color(0.9f, 0.2f, 0.2f, 1f));
        CacheTexture("goldIcon", new Color(1f, 0.85f, 0.2f, 1f));

        // XP bar
        CacheTexture("xpBarBg", new Color(0.15f, 0.15f, 0.2f, 1f));
        CacheTexture("xpBarFill", new Color(0.3f, 0.7f, 0.3f, 1f));

        // Character model colors
        CacheTexture("skin", new Color(0.85f, 0.7f, 0.55f, 1f));
        CacheTexture("hat", new Color(0.7f, 0.6f, 0.4f, 1f));
        CacheTexture("shirt", new Color(0.15f, 0.30f, 0.55f, 1f));
        CacheTexture("pants", new Color(0.22f, 0.18f, 0.12f, 1f));
        CacheTexture("boots", new Color(0.15f, 0.12f, 0.08f, 1f));
        CacheTexture("rod", new Color(0.35f, 0.22f, 0.12f, 1f));
        CacheTexture("eyeWhite", Color.white);
        CacheTexture("eyeIris", new Color(0.2f, 0.3f, 0.2f, 1f));
        CacheTexture("legSep", new Color(0.12f, 0.12f, 0.15f, 1f));
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
        // Toggle panel with Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isOpen = !isOpen;
        }

        // Heartbeat animation
        heartbeatTimer += Time.deltaTime;
        if (heartbeatTimer > 0.8f)
        {
            heartbeatPulse = !heartbeatPulse;
            heartbeatTimer = 0f;
        }
    }

    void OnGUI()
    {
        if (!isOpen) return;

        // Main panel dimensions
        float panelWidth = 450f;
        float panelHeight = 550f;
        float panelX = (Screen.width - panelWidth) / 2f;
        float panelY = (Screen.height - panelHeight) / 2f;

        // Draw main panel background
        GUI.DrawTexture(new Rect(panelX - 5, panelY - 5, panelWidth + 10, panelHeight + 10), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);
        GUI.Label(new Rect(panelX, panelY + 10, panelWidth, 35), "CHARACTER", titleStyle);

        // Divider line
        GUI.DrawTexture(new Rect(panelX + 20, panelY + 50, panelWidth - 40, 2), GetTexture("divider"));

        // === LEFT SIDE: Character Model Preview ===
        float modelX = panelX + 20;
        float modelY = panelY + 65;
        float modelWidth = 180;
        float modelHeight = 280;

        // Model background
        GUI.DrawTexture(new Rect(modelX, modelY, modelWidth, modelHeight), GetTexture("modelBg"));

        // Draw simple character representation
        DrawCharacterModel(modelX, modelY, modelWidth, modelHeight);

        // Character name below model
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontSize = 16;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(modelX, modelY + modelHeight + 5, modelWidth, 25), characterName, nameStyle);

        // === RIGHT SIDE: Stats & Equipment ===
        float statsX = panelX + 220;
        float statsY = panelY + 65;
        float statsWidth = 210;

        // Stats section
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.8f, 0.75f, 0.5f);

        GUIStyle statStyle = new GUIStyle(GUI.skin.label);
        statStyle.fontSize = 13;
        statStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(statsX, statsY, statsWidth, 20), "STATS", headerStyle);

        float statY = statsY + 25;

        // Level
        int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;
        GUI.Label(new Rect(statsX, statY, statsWidth, 20), "Level: " + level, statStyle);
        statY += 22;

        // Age
        GUI.Label(new Rect(statsX, statY, statsWidth, 20), "Age: " + characterAge + " years", statStyle);
        statY += 22;

        // Heartbeat with animated icon
        float heartSize = heartbeatPulse ? 18f : 14f;
        float heartOffset = heartbeatPulse ? 0f : 2f;
        GUI.DrawTexture(new Rect(statsX + heartOffset, statY + heartOffset, heartSize, heartSize), GetTexture("heartIcon"));
        int bpm = 65 + (heartbeatPulse ? 5 : 0);
        GUIStyle heartStyle = new GUIStyle(GUI.skin.label);
        heartStyle.fontSize = 13;
        heartStyle.normal.textColor = new Color(1f, 0.7f, 0.7f);
        GUI.Label(new Rect(statsX + 25, statY, statsWidth, 20), bpm + " BPM", heartStyle);
        statY += 25;

        // Gold with icon
        int gold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUI.DrawTexture(new Rect(statsX, statY, 16, 16), GetTexture("goldIcon"));
        GUIStyle goldStyle = new GUIStyle(GUI.skin.label);
        goldStyle.fontSize = 13;
        goldStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);
        GUI.Label(new Rect(statsX + 22, statY, statsWidth, 20), "x " + gold.ToString("N0"), goldStyle);
        statY += 25;

        // XP Progress
        if (LevelingSystem.Instance != null)
        {
            long currentXP = LevelingSystem.Instance.GetCurrentXP();
            float progress = LevelingSystem.Instance.GetProgressToNextLevel();

            GUI.Label(new Rect(statsX, statY, statsWidth, 20), "XP: " + currentXP.ToString("N0"), statStyle);
            statY += 20;

            // XP bar
            GUI.DrawTexture(new Rect(statsX, statY, statsWidth - 10, 12), GetTexture("xpBarBg"));
            GUI.DrawTexture(new Rect(statsX + 1, statY + 1, (statsWidth - 12) * progress, 10), GetTexture("xpBarFill"));
            statY += 20;
        }

        // Fish caught
        int fishCaught = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;
        GUI.Label(new Rect(statsX, statY, statsWidth, 20), "Fish Caught: " + fishCaught, statStyle);
        statY += 30;

        // === EQUIPMENT SECTION ===
        GUI.Label(new Rect(statsX, statY, statsWidth, 20), "EQUIPMENT", headerStyle);
        statY += 25;

        // Equipment slots
        GUIStyle slotLabelStyle = new GUIStyle(GUI.skin.label);
        slotLabelStyle.fontSize = 11;
        slotLabelStyle.normal.textColor = new Color(0.6f, 0.6f, 0.65f);

        GUIStyle itemStyle = new GUIStyle(GUI.skin.label);
        itemStyle.fontSize = 12;

        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            // Slot background
            GUI.DrawTexture(new Rect(statsX, statY, statsWidth - 10, 28), GetTexture("slotBg"));

            // Slot label
            GUI.Label(new Rect(statsX + 5, statY + 2, 60, 20), equipmentSlots[i] + ":", slotLabelStyle);

            // Equipped item
            string item = equippedItems[i];
            if (item == "None")
            {
                itemStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                itemStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);
            }
            GUI.Label(new Rect(statsX + 70, statY + 5, statsWidth - 80, 20), item, itemStyle);

            statY += 30;
        }

        // Close hint
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 12;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 30, panelWidth, 25), "Press TAB to close", hintStyle);
    }

    void DrawCharacterModel(float x, float y, float width, float height)
    {
        // Simple 2D representation of character using cached textures
        float centerX = x + width / 2;
        float startY = y + 30;

        // Head
        GUI.DrawTexture(new Rect(centerX - 20, startY, 40, 45), GetTexture("skin"));

        // Hat
        GUI.DrawTexture(new Rect(centerX - 28, startY - 5, 56, 15), GetTexture("hat"));
        GUI.DrawTexture(new Rect(centerX - 18, startY - 18, 36, 18), GetTexture("hat"));

        // Eyes
        GUI.DrawTexture(new Rect(centerX - 12, startY + 15, 8, 6), GetTexture("eyeWhite"));
        GUI.DrawTexture(new Rect(centerX + 4, startY + 15, 8, 6), GetTexture("eyeWhite"));
        GUI.DrawTexture(new Rect(centerX - 10, startY + 16, 4, 4), GetTexture("eyeIris"));
        GUI.DrawTexture(new Rect(centerX + 6, startY + 16, 4, 4), GetTexture("eyeIris"));

        // Body (blue shirt)
        GUI.DrawTexture(new Rect(centerX - 25, startY + 50, 50, 60), GetTexture("shirt"));

        // Arms
        GUI.DrawTexture(new Rect(centerX - 38, startY + 50, 15, 50), GetTexture("shirt"));
        GUI.DrawTexture(new Rect(centerX + 23, startY + 50, 15, 50), GetTexture("shirt"));

        // Hands
        GUI.DrawTexture(new Rect(centerX - 35, startY + 95, 12, 15), GetTexture("skin"));
        GUI.DrawTexture(new Rect(centerX + 23, startY + 95, 12, 15), GetTexture("skin"));

        // Pants (brown)
        GUI.DrawTexture(new Rect(centerX - 22, startY + 110, 44, 55), GetTexture("pants"));

        // Legs separation
        GUI.DrawTexture(new Rect(centerX - 2, startY + 120, 4, 45), GetTexture("legSep"));

        // Boots
        GUI.DrawTexture(new Rect(centerX - 24, startY + 165, 20, 15), GetTexture("boots"));
        GUI.DrawTexture(new Rect(centerX + 4, startY + 165, 20, 15), GetTexture("boots"));

        // Fishing rod (held)
        GUI.DrawTexture(new Rect(centerX + 30, startY + 20, 4, 90), GetTexture("rod"));
        GUI.DrawTexture(new Rect(centerX + 30, startY - 20, 3, 45), GetTexture("rod"));
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
