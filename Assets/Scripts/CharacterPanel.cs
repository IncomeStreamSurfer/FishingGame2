using UnityEngine;
using System.Collections.Generic;

public class CharacterPanel : MonoBehaviour
{
    public static CharacterPanel Instance { get; private set; }

    private bool isOpen = false;

    // Character info
    private string characterName = "The Fisherman";
    private int characterAge = 42;

    // Health and heartbeat
    private float heartbeatTime = 0f;
    private int bpm = 72;
    private float currentHealth = 100f;
    private float maxHealth = 100f;

    // ECG line data
    private float[] ecgHistory = new float[60];
    private int ecgIndex = 0;
    private float ecgTimer = 0f;

    // Equipment slots - matches ClothingShopNPC slots
    private string[] equipmentSlots = { "Head", "Top", "Legs", "Accessory", "Rod", "Bait" };
    private string[] equippedItems = { "None", "None", "None", "None", "Basic Rod", "Worm" };

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
        // Initialize ECG history
        for (int i = 0; i < ecgHistory.Length; i++)
            ecgHistory[i] = 0f;
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
        CacheTexture("monitorBg", new Color(0.02f, 0.05f, 0.02f, 1f));
        CacheTexture("monitorBorder", new Color(0.2f, 0.25f, 0.2f, 1f));
        CacheTexture("healthBarBg", new Color(0.15f, 0.05f, 0.05f, 1f));
        CacheTexture("healthBarFill", new Color(0.8f, 0.2f, 0.2f, 1f));
        CacheTexture("healthBarGreen", new Color(0.2f, 0.8f, 0.3f, 1f));
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
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = false;
        }
        heartbeatTime += Time.deltaTime;

        // Update ECG
        UpdateECG();
    }

    void UpdateECG()
    {
        ecgTimer += Time.deltaTime;
        float beatInterval = 60f / bpm;
        float sampleRate = 0.03f; // Update every 30ms

        if (ecgTimer >= sampleRate)
        {
            ecgTimer = 0f;

            // Calculate ECG value based on heartbeat phase
            float phase = (heartbeatTime % beatInterval) / beatInterval;
            float ecgValue = CalculateECGValue(phase);

            // Store in history
            ecgHistory[ecgIndex] = ecgValue;
            ecgIndex = (ecgIndex + 1) % ecgHistory.Length;
        }
    }

    float CalculateECGValue(float phase)
    {
        // Simulate realistic ECG waveform (PQRST complex)
        if (phase < 0.1f)
        {
            // P wave (small bump)
            float t = phase / 0.1f;
            return Mathf.Sin(t * Mathf.PI) * 0.15f;
        }
        else if (phase < 0.15f)
        {
            // PR segment (flat)
            return 0f;
        }
        else if (phase < 0.18f)
        {
            // Q wave (small dip)
            float t = (phase - 0.15f) / 0.03f;
            return -Mathf.Sin(t * Mathf.PI) * 0.1f;
        }
        else if (phase < 0.25f)
        {
            // R wave (tall spike)
            float t = (phase - 0.18f) / 0.07f;
            return Mathf.Sin(t * Mathf.PI) * 1.0f;
        }
        else if (phase < 0.30f)
        {
            // S wave (small dip)
            float t = (phase - 0.25f) / 0.05f;
            return -Mathf.Sin(t * Mathf.PI) * 0.2f;
        }
        else if (phase < 0.45f)
        {
            // ST segment (flat)
            return 0f;
        }
        else if (phase < 0.60f)
        {
            // T wave (medium bump)
            float t = (phase - 0.45f) / 0.15f;
            return Mathf.Sin(t * Mathf.PI) * 0.25f;
        }
        else
        {
            // Baseline
            return 0f;
        }
    }

    void OnGUI()
    {
        if (!isOpen || !initialized || !MainMenu.GameStarted) return;

        float panelWidth = 450f;
        float panelHeight = 520f;
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

        // ============ HEARTBEAT MONITOR ============
        float monitorX = panelX + 175;
        float monitorY = modelY;
        DrawHeartbeatMonitor(monitorX, monitorY);

        // Stats below monitor
        float statsX = panelX + 175;
        float statsY = monitorY + 135;

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
        statsY += 18;

        GUI.Label(new Rect(statsX, statsY, 200, 18), "Age: " + characterAge, statStyle);
        statsY += 18;

        // Gold
        int gold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUI.DrawTexture(new Rect(statsX, statsY + 1, 14, 14), GetTexture("goldIcon"));
        GUIStyle goldStyle = new GUIStyle(statStyle);
        goldStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);
        GUI.Label(new Rect(statsX + 20, statsY, 100, 18), gold.ToString("N0"), goldStyle);
        statsY += 20;

        // Fish caught
        int fishCaught = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;
        GUI.Label(new Rect(statsX, statsY, 200, 18), "Fish Caught: " + fishCaught, statStyle);
        statsY += 25;

        // Equipment
        GUI.Label(new Rect(statsX, statsY, 200, 18), "EQUIPMENT", headerStyle);
        statsY += 22;

        GUIStyle slotStyle = new GUIStyle(GUI.skin.label);
        slotStyle.fontSize = 10;
        slotStyle.normal.textColor = new Color(0.6f, 0.6f, 0.65f);

        GUIStyle itemStyle = new GUIStyle(GUI.skin.label);
        itemStyle.fontSize = 11;
        itemStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);

        for (int i = 0; i < equipmentSlots.Length && i < 6; i++)
        {
            GUI.DrawTexture(new Rect(statsX, statsY, 240, 22), GetTexture("slotBg"));
            GUI.Label(new Rect(statsX + 5, statsY + 2, 60, 18), equipmentSlots[i] + ":", slotStyle);

            string item = equippedItems[i];
            itemStyle.normal.textColor = item == "None" ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.5f, 0.8f, 1f);
            GUI.Label(new Rect(statsX + 70, statsY + 2, 170, 18), item, itemStyle);
            statsY += 24;
        }

        // Close hint
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 11;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 25, panelWidth, 20), "Press TAB to close", hintStyle);
    }

    void DrawHeartbeatMonitor(float x, float y)
    {
        float monitorWidth = 250f;
        float monitorHeight = 130f;

        // Monitor frame/border
        GUI.DrawTexture(new Rect(x - 3, y - 3, monitorWidth + 6, monitorHeight + 6), GetTexture("monitorBorder"));

        // Monitor screen background (dark green like hospital monitor)
        GUI.DrawTexture(new Rect(x, y, monitorWidth, monitorHeight), GetTexture("monitorBg"));

        // Monitor title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 10;
        titleStyle.normal.textColor = new Color(0.3f, 0.6f, 0.3f);
        titleStyle.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(x + 5, y + 3, 100, 14), "VITAL SIGNS", titleStyle);

        // BPM display (top right, large)
        GUIStyle bpmStyle = new GUIStyle();
        bpmStyle.fontSize = 28;
        bpmStyle.fontStyle = FontStyle.Bold;
        bpmStyle.normal.textColor = new Color(0.2f, 1f, 0.3f);
        bpmStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(x + monitorWidth - 90, y + 2, 85, 35), bpm.ToString(), bpmStyle);

        GUIStyle bpmLabelStyle = new GUIStyle();
        bpmLabelStyle.fontSize = 10;
        bpmLabelStyle.normal.textColor = new Color(0.2f, 0.8f, 0.3f);
        bpmLabelStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(x + monitorWidth - 90, y + 32, 85, 14), "BPM", bpmLabelStyle);

        // Heart icon that pulses
        float beatCycle = heartbeatTime * (bpm / 60f);
        bool isPeak = (beatCycle % 1f) < 0.15f;
        float heartSize = isPeak ? 18f : 14f;
        Color heartColor = isPeak ? new Color(1f, 0.3f, 0.3f) : new Color(0.8f, 0.2f, 0.2f);

        GUI.color = heartColor;
        GUI.DrawTexture(new Rect(x + monitorWidth - 105, y + 8, heartSize, heartSize), GetTexture("heartIcon"));
        GUI.color = Color.white;

        // ECG waveform area
        float waveX = x + 5;
        float waveY = y + 50;
        float waveWidth = monitorWidth - 10;
        float waveHeight = 40f;

        // Draw ECG line
        DrawECGWaveform(waveX, waveY, waveWidth, waveHeight);

        // Health bar
        float healthBarY = y + monitorHeight - 25;
        float healthBarWidth = monitorWidth - 10;
        float healthBarHeight = 18f;

        // Health label
        GUIStyle healthLabelStyle = new GUIStyle();
        healthLabelStyle.fontSize = 9;
        healthLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(x + 5, healthBarY - 12, 50, 12), "HEALTH", healthLabelStyle);

        // Health bar background
        GUI.DrawTexture(new Rect(x + 5, healthBarY, healthBarWidth, healthBarHeight), GetTexture("healthBarBg"));

        // Health bar fill (color changes based on health)
        float healthPercent = currentHealth / maxHealth;
        Color healthColor;
        if (healthPercent > 0.6f)
            healthColor = new Color(0.2f, 0.85f, 0.3f); // Green
        else if (healthPercent > 0.3f)
            healthColor = new Color(0.9f, 0.8f, 0.2f); // Yellow
        else
            healthColor = new Color(0.9f, 0.2f, 0.2f); // Red

        Texture2D healthFillTex = new Texture2D(1, 1);
        healthFillTex.SetPixel(0, 0, healthColor);
        healthFillTex.Apply();

        GUI.DrawTexture(new Rect(x + 6, healthBarY + 1, (healthBarWidth - 2) * healthPercent, healthBarHeight - 2), healthFillTex);

        // Health text
        GUIStyle healthTextStyle = new GUIStyle();
        healthTextStyle.fontSize = 11;
        healthTextStyle.fontStyle = FontStyle.Bold;
        healthTextStyle.normal.textColor = Color.white;
        healthTextStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x + 5, healthBarY, healthBarWidth, healthBarHeight),
            Mathf.RoundToInt(currentHealth) + " / " + Mathf.RoundToInt(maxHealth), healthTextStyle);

        Object.Destroy(healthFillTex);
    }

    void DrawECGWaveform(float x, float y, float width, float height)
    {
        // Draw the ECG line
        float centerY = y + height / 2;
        float amplitude = height / 2 - 2;

        // Create a green color for the ECG line
        Color ecgColor = new Color(0.2f, 1f, 0.3f);

        // Draw each point in the ECG history
        float stepX = width / (ecgHistory.Length - 1);

        for (int i = 0; i < ecgHistory.Length - 1; i++)
        {
            // Get indices relative to current position (so newest is on right)
            int idx1 = (ecgIndex + i) % ecgHistory.Length;
            int idx2 = (ecgIndex + i + 1) % ecgHistory.Length;

            float x1 = x + i * stepX;
            float x2 = x + (i + 1) * stepX;
            float y1 = centerY - ecgHistory[idx1] * amplitude;
            float y2 = centerY - ecgHistory[idx2] * amplitude;

            // Draw line segment (using small rectangles)
            DrawLine(x1, y1, x2, y2, ecgColor, 2f);
        }

        // Draw scanning line effect (bright vertical line at current position)
        float scanX = x + ((ecgHistory.Length - 1) * stepX);
        GUI.color = new Color(0.5f, 1f, 0.5f, 0.5f);
        GUI.DrawTexture(new Rect(scanX - 1, y, 3, height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness)
    {
        // Calculate angle and length
        float dx = x2 - x1;
        float dy = y2 - y1;
        float length = Mathf.Sqrt(dx * dx + dy * dy);
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

        // Save and set GUI matrix for rotation
        Matrix4x4 matrixBackup = GUI.matrix;

        // Pivot point
        GUIUtility.RotateAroundPivot(angle, new Vector2(x1, y1));

        GUI.color = color;
        GUI.DrawTexture(new Rect(x1, y1 - thickness / 2, length, thickness), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Restore matrix
        GUI.matrix = matrixBackup;
    }

    void DrawSimpleCharacter(float x, float y, float width, float height)
    {
        float cx = x + width / 2;
        float sy = y + 20;

        // Head
        GUI.DrawTexture(new Rect(cx - 15, sy, 30, 35), GetTexture("skin"));

        // Hat (only if equipped)
        if (equippedItems[0] != "None")
        {
            GUI.DrawTexture(new Rect(cx - 20, sy - 8, 40, 12), GetTexture("hat"));
        }

        // Body - color based on equipped item
        string bodyItem = equippedItems[1]; // Top slot
        if (bodyItem == "None")
        {
            // Naked - show skin
            GUI.DrawTexture(new Rect(cx - 18, sy + 40, 36, 50), GetTexture("skin"));
        }
        else
        {
            Texture2D shirtTex = GetShirtTexture(bodyItem);
            GUI.DrawTexture(new Rect(cx - 18, sy + 40, 36, 50), shirtTex);
        }

        // Legs - color based on equipped item
        string legsItem = equippedItems[2]; // Legs slot
        if (legsItem == "None")
        {
            // Naked legs - show skin
            GUI.DrawTexture(new Rect(cx - 16, sy + 90, 32, 45), GetTexture("skin"));
        }
        else
        {
            Texture2D pantsTex = GetPantsTexture(legsItem);
            GUI.DrawTexture(new Rect(cx - 16, sy + 90, 32, 45), pantsTex);
        }

        // Feet
        GUI.DrawTexture(new Rect(cx - 16, sy + 135, 14, 12), GetTexture("skin"));
        GUI.DrawTexture(new Rect(cx + 2, sy + 135, 14, 12), GetTexture("skin"));

        // Draw parrot if equipped
        string accessory = equippedItems[3]; // Accessory slot
        if (accessory == "Shoulder Parrot")
        {
            GUI.color = new Color(0.2f, 0.75f, 0.25f);
            GUI.DrawTexture(new Rect(cx + 18, sy + 38, 12, 10), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.7f, 0.1f);
            GUI.DrawTexture(new Rect(cx + 30, sy + 40, 5, 3), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // Draw pimp cane if equipped
        if (accessory == "Pimp Cane")
        {
            // Cane shaft (black)
            GUI.color = new Color(0.1f, 0.1f, 0.1f);
            GUI.DrawTexture(new Rect(cx + 22, sy + 60, 4, 80), Texture2D.whiteTexture);
            // Gold handle
            GUI.color = new Color(0.95f, 0.8f, 0.2f);
            GUI.DrawTexture(new Rect(cx + 20, sy + 55, 8, 8), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    Texture2D GetShirtTexture(string itemName)
    {
        switch (itemName)
        {
            case "Coconut Bra":
                CacheTexture("coconut", new Color(0.55f, 0.35f, 0.2f));
                return GetTexture("coconut");
            case "Red T-Shirt":
                CacheTexture("redshirt", new Color(0.85f, 0.15f, 0.1f));
                return GetTexture("redshirt");
            case "Blue Shirt":
                return GetTexture("shirt");
            case "Lumberjack Shirt":
                CacheTexture("lumberjack", new Color(0.75f, 0.12f, 0.08f));
                return GetTexture("lumberjack");
            case "Fancy Tuxedo":
                CacheTexture("tuxedo", new Color(0.08f, 0.08f, 0.08f));
                return GetTexture("tuxedo");
            default:
                return GetTexture("shirt");
        }
    }

    Texture2D GetPantsTexture(string itemName)
    {
        switch (itemName)
        {
            case "Red Pants":
                CacheTexture("redpants", new Color(0.8f, 0.15f, 0.1f));
                return GetTexture("redpants");
            case "Green Pants":
                CacheTexture("greenpants", new Color(0.2f, 0.5f, 0.2f));
                return GetTexture("greenpants");
            case "Black Pants":
                CacheTexture("blackpants", new Color(0.12f, 0.12f, 0.12f));
                return GetTexture("blackpants");
            case "Blue Jeans":
                CacheTexture("bluejeans", new Color(0.2f, 0.35f, 0.6f));
                return GetTexture("bluejeans");
            case "Fancy Tuxedo":
                CacheTexture("tuxedopants", new Color(0.08f, 0.08f, 0.08f));
                return GetTexture("tuxedopants");
            default:
                return GetTexture("pants");
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

    public void SetHealth(float health, float max)
    {
        currentHealth = Mathf.Clamp(health, 0, max);
        maxHealth = max;
    }

    public void SetBPM(int newBpm)
    {
        bpm = Mathf.Clamp(newBpm, 40, 200);
    }
}
