using UnityEngine;
using System.Collections.Generic;

public class CharacterPanel : MonoBehaviour
{
    public static CharacterPanel Instance { get; private set; }

    private bool isOpen = false;
    private int guiFrameSkip = 0;

    // Draggable window support
    private DraggableWindow window;

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
        // Initialize draggable window (35% smaller: 293x338)
        float panelWidth = 293f;
        float panelHeight = 338f;
        Rect initialRect = new Rect(
            (Screen.width - panelWidth) / 2f,
            (Screen.height - panelHeight) / 2f,
            panelWidth,
            panelHeight
        );
        window = new DraggableWindow(initialRect, new Vector2(250, 280), new Vector2(500, 600));
        initialized = true;
    }

    void CreateCachedTextures()
    {
        // Consistent UI style
        CacheTexture("panelBg", new Color(0.1f, 0.1f, 0.12f, 0.95f));
        CacheTexture("panelBorder", new Color(1f, 0.85f, 0.4f, 1f)); // Gold border
        CacheTexture("closeBtn", new Color(0.8f, 0.2f, 0.2f, 1f));
        CacheTexture("slotBg", new Color(0.15f, 0.15f, 0.17f, 0.9f));
        CacheTexture("divider", new Color(1f, 0.85f, 0.4f, 0.8f)); // Gold divider
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
        // TAB or C key to toggle character panel
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.C))
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

    // Public method to open panel (for HUD button)
    public void Open()
    {
        isOpen = true;
    }

    public void Toggle()
    {
        isOpen = !isOpen;
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
        // Performance: Skip frames when not actively needed
        if (!isOpen)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        if (!isOpen || !initialized || !MainMenu.GameStarted || window == null) return;

        // Handle dragging and resizing
        window.UpdateWindow();

        // Get window rect
        Rect rect = window.WindowRect;
        float panelX = rect.x;
        float panelY = rect.y;
        float panelWidth = rect.width;
        float panelHeight = rect.height;

        // Consistent padding
        float padding = 10f;
        float contentWidth = panelWidth - (padding * 2);

        // Panel background with border
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // ============ TITLE BAR ============
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);
        GUI.Label(new Rect(panelX, panelY + 4, panelWidth, 20), "CHARACTER", titleStyle);

        // Close button (top-right, inside border)
        GUIStyle xButtonStyle = new GUIStyle();
        xButtonStyle.fontSize = 10;
        xButtonStyle.fontStyle = FontStyle.Bold;
        xButtonStyle.alignment = TextAnchor.MiddleCenter;
        xButtonStyle.normal.textColor = Color.white;
        float closeBtnSize = 18f;
        GUI.DrawTexture(new Rect(panelX + panelWidth - closeBtnSize - 4, panelY + 4, closeBtnSize, closeBtnSize), GetTexture("closeBtn"));
        if (GUI.Button(new Rect(panelX + panelWidth - closeBtnSize - 4, panelY + 4, closeBtnSize, closeBtnSize), "X", xButtonStyle))
        {
            isOpen = false;
        }

        // Divider below title
        float dividerY = panelY + 26;
        GUI.DrawTexture(new Rect(panelX + padding, dividerY, contentWidth, 1), GetTexture("divider"));

        // ============ TOP ROW: Model + Monitor (side by side, centered) ============
        float topRowY = dividerY + 6;
        float modelWidth = 80f;
        float modelHeight = 115f;
        float monitorWidth = 130f;
        float monitorHeight = 75f;
        float gapBetween = 8f;

        // Calculate positions to center the top row
        float topRowTotalWidth = modelWidth + gapBetween + monitorWidth;
        float topRowStartX = panelX + (panelWidth - topRowTotalWidth) / 2f;

        // Character model
        float modelX = topRowStartX;
        float modelY = topRowY;
        GUI.DrawTexture(new Rect(modelX, modelY, modelWidth, modelHeight), GetTexture("modelBg"));
        DrawSimpleCharacter(modelX, modelY, modelWidth, modelHeight);

        // Character name below model
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontSize = 9;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(modelX, modelY + modelHeight + 2, modelWidth, 12), characterName, nameStyle);

        // Heartbeat monitor (right of model)
        float monitorX = modelX + modelWidth + gapBetween;
        float monitorY = topRowY;
        DrawHeartbeatMonitor(monitorX, monitorY, monitorWidth, monitorHeight);

        // ============ STATS SECTION (full width, below top row) ============
        float statsY = topRowY + modelHeight + 18;
        float statsX = panelX + padding;
        float statsWidth = contentWidth;

        // Stats header
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 11;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.8f, 0.75f, 0.5f);

        GUIStyle statStyle = new GUIStyle(GUI.skin.label);
        statStyle.fontSize = 10;
        statStyle.normal.textColor = Color.white;

        // Stats in two columns
        GUI.Label(new Rect(statsX, statsY, statsWidth, 14), "STATS", headerStyle);
        statsY += 16;

        // Background for stats area
        GUI.DrawTexture(new Rect(statsX, statsY, statsWidth, 38), GetTexture("slotBg"));

        float col1X = statsX + 6;
        float col2X = statsX + statsWidth / 2;
        float rowH = 16f;

        int level = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;
        GUI.Label(new Rect(col1X, statsY + 4, 120, rowH), "Level: " + level, statStyle);
        GUI.Label(new Rect(col2X, statsY + 4, 120, rowH), "Age: " + characterAge, statStyle);

        // Gold row
        int gold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUI.DrawTexture(new Rect(col1X, statsY + rowH + 6, 12, 12), GetTexture("goldIcon"));
        GUIStyle goldStyle = new GUIStyle(statStyle);
        goldStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);
        GUI.Label(new Rect(col1X + 16, statsY + rowH + 4, 80, rowH), gold.ToString("N0"), goldStyle);

        int fishCaught = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;
        GUI.Label(new Rect(col2X, statsY + rowH + 4, 120, rowH), "Fish: " + fishCaught, statStyle);

        statsY += 42;

        // ============ EQUIPMENT SECTION ============
        GUI.Label(new Rect(statsX, statsY, statsWidth, 14), "EQUIPMENT", headerStyle);
        statsY += 16;

        GUIStyle slotStyle = new GUIStyle(GUI.skin.label);
        slotStyle.fontSize = 9;
        slotStyle.normal.textColor = new Color(0.6f, 0.6f, 0.65f);

        GUIStyle itemStyle = new GUIStyle(GUI.skin.label);
        itemStyle.fontSize = 9;
        itemStyle.normal.textColor = new Color(0.5f, 0.8f, 1f);

        // Equipment in two columns (3 rows x 2 columns)
        float slotHeight = 20f;
        float halfWidth = (statsWidth - 4) / 2f;

        for (int row = 0; row < 3; row++)
        {
            float rowY = statsY + row * (slotHeight + 2);

            // Left slot
            int leftIdx = row * 2;
            if (leftIdx < equipmentSlots.Length)
            {
                GUI.DrawTexture(new Rect(statsX, rowY, halfWidth, slotHeight), GetTexture("slotBg"));
                GUI.Label(new Rect(statsX + 4, rowY + 2, 45, 16), equipmentSlots[leftIdx] + ":", slotStyle);
                string leftItem = equippedItems[leftIdx];
                itemStyle.normal.textColor = leftItem == "None" ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.5f, 0.8f, 1f);
                GUI.Label(new Rect(statsX + 50, rowY + 2, halfWidth - 54, 16), leftItem, itemStyle);
            }

            // Right slot
            int rightIdx = row * 2 + 1;
            if (rightIdx < equipmentSlots.Length)
            {
                GUI.DrawTexture(new Rect(statsX + halfWidth + 4, rowY, halfWidth, slotHeight), GetTexture("slotBg"));
                GUI.Label(new Rect(statsX + halfWidth + 8, rowY + 2, 45, 16), equipmentSlots[rightIdx] + ":", slotStyle);
                string rightItem = equippedItems[rightIdx];
                itemStyle.normal.textColor = rightItem == "None" ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.5f, 0.8f, 1f);
                GUI.Label(new Rect(statsX + halfWidth + 54, rowY + 2, halfWidth - 54, 16), rightItem, itemStyle);
            }
        }

        // ============ FOOTER ============
        GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.fontSize = 9;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 18, panelWidth, 14), "TAB to close | Drag title to move", hintStyle);

        // Draw resize handle
        window.DrawResizeHandle();
    }

    void DrawHeartbeatMonitor(float x, float y, float monitorWidth = 150f, float monitorHeight = 80f)
    {

        // Monitor frame/border
        GUI.DrawTexture(new Rect(x - 3, y - 3, monitorWidth + 6, monitorHeight + 6), GetTexture("monitorBorder"));

        // Monitor screen background (dark green like hospital monitor)
        GUI.DrawTexture(new Rect(x, y, monitorWidth, monitorHeight), GetTexture("monitorBg"));

        // Monitor title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 7;
        titleStyle.normal.textColor = new Color(0.3f, 0.6f, 0.3f);
        titleStyle.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(x + 5, y + 2, 80, 10), "VITAL SIGNS", titleStyle);

        // BPM display (top right, smaller)
        GUIStyle bpmStyle = new GUIStyle();
        bpmStyle.fontSize = 16;
        bpmStyle.fontStyle = FontStyle.Bold;
        bpmStyle.normal.textColor = new Color(0.2f, 1f, 0.3f);
        bpmStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(x + monitorWidth - 55, y + 2, 50, 20), bpm.ToString(), bpmStyle);

        GUIStyle bpmLabelStyle = new GUIStyle();
        bpmLabelStyle.fontSize = 8;
        bpmLabelStyle.normal.textColor = new Color(0.2f, 0.8f, 0.3f);
        bpmLabelStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(x + monitorWidth - 55, y + 20, 50, 10), "BPM", bpmLabelStyle);

        // Heart icon that pulses
        float beatCycle = heartbeatTime * (bpm / 60f);
        bool isPeak = (beatCycle % 1f) < 0.15f;
        float heartSize = isPeak ? 11f : 9f;
        Color heartColor = isPeak ? new Color(1f, 0.3f, 0.3f) : new Color(0.8f, 0.2f, 0.2f);

        GUI.color = heartColor;
        GUI.DrawTexture(new Rect(x + monitorWidth - 65, y + 5, heartSize, heartSize), GetTexture("heartIcon"));
        GUI.color = Color.white;

        // ECG waveform area
        float waveX = x + 5;
        float waveY = y + 32;
        float waveWidth = monitorWidth - 10;
        float waveHeight = 25f;

        // Draw ECG line
        DrawECGWaveform(waveX, waveY, waveWidth, waveHeight);

        // Health bar
        float healthBarY = y + monitorHeight - 16;
        float healthBarWidth = monitorWidth - 10;
        float healthBarHeight = 12f;

        // Health label
        GUIStyle healthLabelStyle = new GUIStyle();
        healthLabelStyle.fontSize = 7;
        healthLabelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(x + 5, healthBarY - 9, 50, 9), "HEALTH", healthLabelStyle);

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
        healthTextStyle.fontSize = 9;
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
        // Scale factor based on default size (91x130)
        float scaleX = width / 91f;
        float scaleY = height / 130f;
        float scale = Mathf.Min(scaleX, scaleY);

        float cx = x + width / 2;
        float sy = y + (10 * scale);

        // Head
        float headW = 26 * scale;
        float headH = 30 * scale;
        GUI.DrawTexture(new Rect(cx - headW/2, sy, headW, headH), GetTexture("skin"));

        // Face details
        GUI.color = new Color(0.2f, 0.2f, 0.2f);
        float eyeSize = 3 * scale;
        float eyeY = sy + (8 * scale);
        GUI.DrawTexture(new Rect(cx - (6 * scale), eyeY, eyeSize, eyeSize), Texture2D.whiteTexture); // Left eye
        GUI.DrawTexture(new Rect(cx + (3 * scale), eyeY, eyeSize, eyeSize), Texture2D.whiteTexture); // Right eye
        GUI.DrawTexture(new Rect(cx - (3 * scale), sy + (18 * scale), 6 * scale, 2 * scale), Texture2D.whiteTexture); // Mouth
        GUI.color = Color.white;

        // Hat (based on equipped item)
        string hatItem = equippedItems[0];
        if (hatItem != "None")
        {
            DrawHatScaled(cx, sy, hatItem, scale);
        }

        // Body
        float bodyY = sy + (32 * scale);
        float bodyW = 30 * scale;
        float bodyH = 40 * scale;

        string bodyItem = equippedItems[1];
        if (bodyItem == "None")
        {
            GUI.DrawTexture(new Rect(cx - bodyW/2, bodyY, bodyW, bodyH), GetTexture("skin"));
        }
        else
        {
            DrawShirtScaled(cx, bodyY, bodyItem, scale);
        }

        // Arms (with hands)
        float armW = 6 * scale;
        float armH = 32 * scale;
        GUI.DrawTexture(new Rect(cx - bodyW/2 - armW, bodyY + (2 * scale), armW, armH), GetTexture("skin"));
        GUI.DrawTexture(new Rect(cx + bodyW/2, bodyY + (2 * scale), armW, armH), GetTexture("skin"));

        // Hands (small circles at end of arms)
        float handSize = 5 * scale;
        GUI.DrawTexture(new Rect(cx - bodyW/2 - armW, bodyY + armH + (2 * scale), handSize, handSize), GetTexture("skin"));
        GUI.DrawTexture(new Rect(cx + bodyW/2 + 1, bodyY + armH + (2 * scale), handSize, handSize), GetTexture("skin"));

        // Legs
        float legY = bodyY + bodyH;
        float legW = 10 * scale;
        float legH = 35 * scale;

        string legsItem = equippedItems[2];
        Texture2D legTex = legsItem == "None" ? GetTexture("skin") : GetPantsTexture(legsItem);
        GUI.DrawTexture(new Rect(cx - (11 * scale), legY, legW, legH), legTex);
        GUI.DrawTexture(new Rect(cx + (1 * scale), legY, legW, legH), legTex);

        // Feet
        float footY = legY + legH;
        float footW = 10 * scale;
        float footH = 6 * scale;
        GUI.DrawTexture(new Rect(cx - (11 * scale), footY, footW, footH), GetTexture("skin"));
        GUI.DrawTexture(new Rect(cx + (1 * scale), footY, footW, footH), GetTexture("skin"));

        // Accessories
        string accessory = equippedItems[3];
        if (accessory == "Shoulder Parrot")
        {
            float parrotScale = scale * 0.8f;
            GUI.color = new Color(0.2f, 0.75f, 0.25f);
            GUI.DrawTexture(new Rect(cx + (16 * scale), bodyY - (2 * scale), 10 * parrotScale, 8 * parrotScale), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.7f, 0.1f);
            GUI.DrawTexture(new Rect(cx + (25 * scale), bodyY, 4 * parrotScale, 3 * parrotScale), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        else if (accessory == "Pimp Cane")
        {
            float caneX = cx + (20 * scale);
            GUI.color = new Color(0.1f, 0.1f, 0.1f);
            GUI.DrawTexture(new Rect(caneX, bodyY + (15 * scale), 3 * scale, 55 * scale), Texture2D.whiteTexture);
            GUI.color = new Color(0.95f, 0.8f, 0.2f);
            GUI.DrawTexture(new Rect(caneX - (2 * scale), bodyY + (10 * scale), 7 * scale, 7 * scale), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    void DrawHatScaled(float cx, float sy, string hatName, float scale)
    {
        switch (hatName)
        {
            case "Straw Hat":
                GUI.color = new Color(0.9f, 0.8f, 0.5f);
                GUI.DrawTexture(new Rect(cx - (20 * scale), sy - (4 * scale), 40 * scale, 6 * scale), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - (10 * scale), sy - (14 * scale), 20 * scale, 12 * scale), Texture2D.whiteTexture);
                GUI.color = new Color(0.45f, 0.20f, 0.10f);
                GUI.DrawTexture(new Rect(cx - (11 * scale), sy - (5 * scale), 22 * scale, 3 * scale), Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;
            case "Baseball Cap":
                GUI.color = new Color(0.85f, 0.15f, 0.1f);
                GUI.DrawTexture(new Rect(cx - (13 * scale), sy - (8 * scale), 26 * scale, 11 * scale), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - (4 * scale), sy + (2 * scale), 16 * scale, 5 * scale), Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;
            case "Fancy Top Hat":
                GUI.color = new Color(0.1f, 0.1f, 0.1f);
                GUI.DrawTexture(new Rect(cx - (14 * scale), sy - (4 * scale), 28 * scale, 5 * scale), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - (10 * scale), sy - (24 * scale), 20 * scale, 22 * scale), Texture2D.whiteTexture);
                GUI.color = new Color(0.6f, 0.1f, 0.1f);
                GUI.DrawTexture(new Rect(cx - (10 * scale), sy - (6 * scale), 20 * scale, 3 * scale), Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;
            default:
                GUI.DrawTexture(new Rect(cx - (16 * scale), sy - (6 * scale), 32 * scale, 10 * scale), GetTexture("hat"));
                break;
        }
    }

    void DrawShirtScaled(float cx, float y, string shirtName, float scale)
    {
        float bodyW = 30 * scale;
        float bodyH = 40 * scale;
        Rect bodyRect = new Rect(cx - bodyW/2, y, bodyW, bodyH);

        switch (shirtName)
        {
            case "Coconut Bra":
                GUI.DrawTexture(bodyRect, GetTexture("skin"));
                GUI.color = new Color(0.55f, 0.35f, 0.2f);
                GUI.DrawTexture(new Rect(cx - (11 * scale), y + (6 * scale), 9 * scale, 7 * scale), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx + (2 * scale), y + (6 * scale), 9 * scale, 7 * scale), Texture2D.whiteTexture);
                GUI.color = new Color(0.6f, 0.5f, 0.35f);
                GUI.DrawTexture(new Rect(cx - bodyW/2, y + (5 * scale), bodyW, 2 * scale), Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;
            case "Lumberjack Shirt":
                int checks = 4;
                float checkSize = bodyW / checks;
                for (int row = 0; row < 6; row++)
                {
                    for (int col = 0; col < checks; col++)
                    {
                        bool isRed = (row + col) % 2 == 0;
                        GUI.color = isRed ? new Color(0.75f, 0.12f, 0.08f) : new Color(0.1f, 0.08f, 0.05f);
                        GUI.DrawTexture(new Rect(cx - bodyW/2 + col * checkSize, y + row * (bodyH/6), checkSize, bodyH/6), Texture2D.whiteTexture);
                    }
                }
                GUI.color = Color.white;
                break;
            case "Fancy Tuxedo":
                GUI.color = new Color(0.08f, 0.08f, 0.08f);
                GUI.DrawTexture(bodyRect, Texture2D.whiteTexture);
                GUI.color = new Color(0.95f, 0.95f, 0.95f);
                GUI.DrawTexture(new Rect(cx - (6 * scale), y, 12 * scale, bodyH), Texture2D.whiteTexture);
                GUI.color = new Color(0.05f, 0.05f, 0.05f);
                GUI.DrawTexture(new Rect(cx - (2 * scale), y + (6 * scale), 4 * scale, bodyH - (10 * scale)), Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;
            case "Red T-Shirt":
                GUI.color = new Color(0.85f, 0.15f, 0.1f);
                GUI.DrawTexture(bodyRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;
            case "Blue Shirt":
                GUI.color = new Color(0.15f, 0.35f, 0.65f);
                GUI.DrawTexture(bodyRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;
            default:
                Texture2D shirtTex = GetShirtTexture(shirtName);
                GUI.DrawTexture(bodyRect, shirtTex);
                break;
        }
    }

    void DrawHat(float cx, float sy, string hatName)
    {
        switch (hatName)
        {
            case "Straw Hat":
                // Straw colored hat with brim
                GUI.color = new Color(0.9f, 0.8f, 0.5f);
                GUI.DrawTexture(new Rect(cx - 25, sy - 5, 50, 8), Texture2D.whiteTexture); // Brim
                GUI.DrawTexture(new Rect(cx - 12, sy - 18, 24, 15), Texture2D.whiteTexture); // Crown
                GUI.color = new Color(0.45f, 0.20f, 0.10f);
                GUI.DrawTexture(new Rect(cx - 13, sy - 6, 26, 4), Texture2D.whiteTexture); // Band
                GUI.color = Color.white;
                break;
            case "Baseball Cap":
                // Red cap with visor
                GUI.color = new Color(0.85f, 0.15f, 0.1f);
                GUI.DrawTexture(new Rect(cx - 16, sy - 10, 32, 14), Texture2D.whiteTexture); // Dome
                GUI.DrawTexture(new Rect(cx - 5, sy + 2, 20, 6), Texture2D.whiteTexture); // Visor
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(cx, sy - 12, 4, 4), Texture2D.whiteTexture); // Button
                break;
            case "Fancy Top Hat":
                // Black top hat
                GUI.color = new Color(0.1f, 0.1f, 0.1f);
                GUI.DrawTexture(new Rect(cx - 18, sy - 5, 36, 6), Texture2D.whiteTexture); // Brim
                GUI.DrawTexture(new Rect(cx - 12, sy - 30, 24, 28), Texture2D.whiteTexture); // Tall crown
                GUI.color = new Color(0.6f, 0.1f, 0.1f);
                GUI.DrawTexture(new Rect(cx - 13, sy - 8, 26, 4), Texture2D.whiteTexture); // Red band
                GUI.color = Color.white;
                break;
            default:
                GUI.DrawTexture(new Rect(cx - 20, sy - 8, 40, 12), GetTexture("hat"));
                break;
        }
    }

    void DrawShirt(float cx, float sy, string shirtName)
    {
        Rect bodyRect = new Rect(cx - 18, sy, 36, 50);

        switch (shirtName)
        {
            case "Coconut Bra":
                // Skin with coconut bra
                GUI.DrawTexture(bodyRect, GetTexture("skin"));
                // Coconuts (brown circles)
                GUI.color = new Color(0.55f, 0.35f, 0.2f);
                GUI.DrawTexture(new Rect(cx - 14, sy + 8, 12, 10), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx + 2, sy + 8, 12, 10), Texture2D.whiteTexture);
                // Rope
                GUI.color = new Color(0.6f, 0.5f, 0.35f);
                GUI.DrawTexture(new Rect(cx - 18, sy + 6, 36, 3), Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;

            case "Lumberjack Shirt":
                // Red/black checkerboard pattern
                int checkSize = 6;
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 6; col++)
                    {
                        bool isRed = (row + col) % 2 == 0;
                        GUI.color = isRed ? new Color(0.75f, 0.12f, 0.08f) : new Color(0.1f, 0.08f, 0.05f);
                        GUI.DrawTexture(new Rect(cx - 18 + col * checkSize, sy + row * checkSize, checkSize, checkSize), Texture2D.whiteTexture);
                    }
                }
                GUI.color = Color.white;
                break;

            case "Fancy Tuxedo":
                // Black jacket
                GUI.color = new Color(0.08f, 0.08f, 0.08f);
                GUI.DrawTexture(bodyRect, Texture2D.whiteTexture);
                // White shirt front
                GUI.color = new Color(0.95f, 0.95f, 0.95f);
                GUI.DrawTexture(new Rect(cx - 8, sy, 16, 50), Texture2D.whiteTexture);
                // Black tie
                GUI.color = new Color(0.05f, 0.05f, 0.05f);
                GUI.DrawTexture(new Rect(cx - 3, sy + 8, 6, 35), Texture2D.whiteTexture);
                // Tie knot
                GUI.DrawTexture(new Rect(cx - 5, sy + 2, 10, 8), Texture2D.whiteTexture);
                // Lapels (darker gray)
                GUI.color = new Color(0.12f, 0.12f, 0.12f);
                GUI.DrawTexture(new Rect(cx - 16, sy + 5, 8, 25), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx + 8, sy + 5, 8, 25), Texture2D.whiteTexture);
                // Buttons
                GUI.color = new Color(0.8f, 0.8f, 0.8f);
                GUI.DrawTexture(new Rect(cx - 1, sy + 15, 3, 3), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - 1, sy + 25, 3, 3), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - 1, sy + 35, 3, 3), Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;

            case "Red T-Shirt":
                GUI.color = new Color(0.85f, 0.15f, 0.1f);
                GUI.DrawTexture(bodyRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;

            case "Blue Shirt":
                GUI.color = new Color(0.15f, 0.35f, 0.65f);
                GUI.DrawTexture(bodyRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                break;

            default:
                Texture2D shirtTex = GetShirtTexture(shirtName);
                GUI.DrawTexture(bodyRect, shirtTex);
                break;
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
