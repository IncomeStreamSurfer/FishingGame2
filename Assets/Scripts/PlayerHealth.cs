using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Player Health System
/// - Starts at 100 HP
/// - Loses 1 HP every 5 seconds (hunger)
/// - Displays HP bar and heartbeat sensor in top right
/// - Death resets stats/fish but keeps cosmetics and gold
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    // Health
    private float maxHealth = 100f;
    private float currentHealth = 100f;
    private float healthDecayTimer = 0f;
    private float healthDecayInterval = 20f; // 20 seconds

    // Death state
    private bool isDead = false;
    private float deathTimer = 0f;
    private float respawnDelay = 3f;

    // Low health warning
    private bool showLowHealthWarning = false;
    private float warningPulse = 0f;

    // Drowning system
    private bool isDrowning = false;
    private float drowningDamageTimer = 0f;
    private float drowningDamageInterval = 1f; // 1 HP per second while drowning
    private float waterLevel = 0.85f; // Y level where drowning starts (water surface is at 0.75)

    // Tutorial tip for new players
    private bool showTutorialTip = true;
    private float tutorialTimer = 0f;

    // Max health buff (from special fish)
    private bool hasMaxHealthBuff = false;
    private float maxHealthBuffTimeRemaining = 0f;

    // ECG/Heartbeat visualization
    private float[] ecgHistory = new float[100];
    private int ecgIndex = 0;
    private float ecgTimer = 0f;
    private float heartbeatPhase = 0f;
    private int currentBPM = 72;

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
        currentHealth = maxHealth;
        InitializeECG();
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        initialized = true;
    }

    void CreateCachedTextures()
    {
        CacheTexture("hpBarBg", new Color(0.1f, 0.1f, 0.1f, 0.9f));
        CacheTexture("hpBarFill", new Color(0.8f, 0.2f, 0.2f, 1f));
        CacheTexture("hpBarFillMid", new Color(0.9f, 0.7f, 0.2f, 1f));
        CacheTexture("hpBarFillHigh", new Color(0.2f, 0.8f, 0.3f, 1f));
        CacheTexture("ecgBg", new Color(0.02f, 0.08f, 0.02f, 0.95f));
        CacheTexture("ecgLine", new Color(0.2f, 1f, 0.3f, 1f));
        CacheTexture("ecgGrid", new Color(0.05f, 0.15f, 0.05f, 0.5f));
        CacheTexture("border", new Color(0.3f, 0.3f, 0.3f, 0.8f));
        CacheTexture("white", Color.white);
        CacheTexture("deathOverlay", new Color(0.5f, 0f, 0f, 0.7f));
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

    void InitializeECG()
    {
        for (int i = 0; i < ecgHistory.Length; i++)
        {
            ecgHistory[i] = 0f;
        }
    }

    void Update()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        if (isDead)
        {
            HandleDeath();
            return;
        }

        // Health decay - 1 HP every 20 seconds (hunger)
        healthDecayTimer += Time.deltaTime;
        if (healthDecayTimer >= healthDecayInterval)
        {
            healthDecayTimer = 0f;
            TakeDamage(1f);
        }

        // Check for drowning
        CheckDrowning();

        // Check for low health warning
        showLowHealthWarning = (currentHealth <= 5f && currentHealth > 0f) || isDrowning;
        if (showLowHealthWarning)
        {
            warningPulse += Time.deltaTime * 5f;
        }

        // Tutorial tip for level 1-2 players
        if (showTutorialTip)
        {
            tutorialTimer += Time.deltaTime;
            // Check if player is above level 2
            if (LevelingSystem.Instance != null && LevelingSystem.Instance.GetLevel() > 2)
            {
                showTutorialTip = false;
            }
            // Or if they close it by pressing any key after 5 seconds
            if (tutorialTimer > 5f && Input.anyKeyDown)
            {
                showTutorialTip = false;
            }
        }

        // Max health buff timer
        if (hasMaxHealthBuff)
        {
            maxHealthBuffTimeRemaining -= Time.deltaTime;
            // Keep health at max while buff is active
            currentHealth = maxHealth;
            if (maxHealthBuffTimeRemaining <= 0f)
            {
                hasMaxHealthBuff = false;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Max health buff expired!", new Color(0.8f, 0.6f, 0.3f));
                }
            }
        }

        // Update ECG
        UpdateECG();

        // Adjust BPM based on health
        if (currentHealth > 70f)
            currentBPM = 72;
        else if (currentHealth > 40f)
            currentBPM = 85;
        else if (currentHealth > 20f)
            currentBPM = 100;
        else
            currentBPM = 120; // Danger zone - heart racing
    }

    void UpdateECG()
    {
        ecgTimer += Time.deltaTime;
        float beatInterval = 60f / currentBPM;

        if (ecgTimer >= 0.02f) // Update at 50Hz
        {
            ecgTimer = 0f;
            heartbeatPhase += 0.02f / beatInterval;
            if (heartbeatPhase >= 1f) heartbeatPhase -= 1f;

            float ecgValue = CalculateECGValue(heartbeatPhase);
            ecgHistory[ecgIndex] = ecgValue;
            ecgIndex = (ecgIndex + 1) % ecgHistory.Length;
        }
    }

    float CalculateECGValue(float phase)
    {
        // PQRST complex simulation
        float value = 0f;

        // P wave (0.0 - 0.1)
        if (phase < 0.1f)
        {
            float t = phase / 0.1f;
            value = Mathf.Sin(t * Mathf.PI) * 0.15f;
        }
        // PR segment (0.1 - 0.15)
        else if (phase < 0.15f)
        {
            value = 0f;
        }
        // Q wave (0.15 - 0.18)
        else if (phase < 0.18f)
        {
            float t = (phase - 0.15f) / 0.03f;
            value = -0.1f * Mathf.Sin(t * Mathf.PI);
        }
        // R wave - tall spike (0.18 - 0.25)
        else if (phase < 0.25f)
        {
            float t = (phase - 0.18f) / 0.07f;
            value = Mathf.Sin(t * Mathf.PI) * 1f;
        }
        // S wave (0.25 - 0.30)
        else if (phase < 0.30f)
        {
            float t = (phase - 0.25f) / 0.05f;
            value = -0.2f * Mathf.Sin(t * Mathf.PI);
        }
        // ST segment (0.30 - 0.45)
        else if (phase < 0.45f)
        {
            value = 0f;
        }
        // T wave (0.45 - 0.65)
        else if (phase < 0.65f)
        {
            float t = (phase - 0.45f) / 0.2f;
            value = Mathf.Sin(t * Mathf.PI) * 0.25f;
        }
        // Rest (0.65 - 1.0)
        else
        {
            value = 0f;
        }

        return value;
    }

    void CheckDrowning()
    {
        if (isDead) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        float playerY = player.transform.position.y;

        // Check if player is below water level
        // Water (blue part) rapidly drains health - 5 HP per second!
        // Player MUST stand on docks to fish safely
        if (playerY < waterLevel)
        {
            isDrowning = true;
            // Rapid health loss - 5 HP per second
            TakeDamage(5f * Time.deltaTime);
        }
        else
        {
            isDrowning = false;
        }
    }

    public bool IsDrowning() => isDrowning;

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"+{amount} HP", new Color(0.3f, 1f, 0.4f));
        }
    }

    public void HealToFull()
    {
        if (isDead) return;
        currentHealth = maxHealth;
    }

    public void ApplyMaxHealthBuff(float duration)
    {
        hasMaxHealthBuff = true;
        maxHealthBuffTimeRemaining = duration;
        currentHealth = maxHealth;
        Debug.Log($"Max health buff applied for {duration} seconds!");
    }

    public bool HasMaxHealthBuff() => hasMaxHealthBuff;
    public float GetMaxHealthBuffTimeRemaining() => maxHealthBuffTimeRemaining;

    void Die()
    {
        isDead = true;
        deathTimer = 0f;
        Debug.Log("PLAYER DIED! Stats will be reset...");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("YOU DIED!", Color.red);
        }
    }

    void HandleDeath()
    {
        deathTimer += Time.deltaTime;

        if (deathTimer >= respawnDelay)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        // Reset stats but keep gold and cosmetics
        currentHealth = maxHealth;
        healthDecayTimer = 0f;
        isDead = false;

        // Reset fish count
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetFishStats();
        }

        // Reset XP/Level
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.ResetProgress();
        }

        // Reset quests
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.ResetQuests();
        }

        // Clear food inventory
        if (FoodInventory.Instance != null)
        {
            FoodInventory.Instance.ClearInventory();
        }

        // Move player back to spawn
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0, 2f, -5f);
        }

        Debug.Log("Player respawned! Gold and cosmetics preserved.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Respawned - Gold & Cosmetics Saved!", new Color(0.3f, 0.8f, 1f));
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        DrawHealthUI();

        if (isDead)
        {
            DrawDeathScreen();
        }

        // Low health warning
        if (showLowHealthWarning)
        {
            DrawLowHealthWarning();
        }

        // Tutorial tip for new players
        if (showTutorialTip && !isDead)
        {
            DrawTutorialTip();
        }
    }

    void DrawLowHealthWarning()
    {
        // Pulsing red warning
        float pulse = 0.7f + Mathf.Sin(warningPulse) * 0.3f;

        float boxWidth = 350;
        float boxHeight = 60;
        float boxX = (Screen.width - boxWidth) / 2;
        float boxY = Screen.height * 0.35f;

        // Different colors for drowning vs low health
        Color bgColor = isDrowning ? new Color(0.1f, 0.2f, 0.8f, pulse * 0.9f) : new Color(0.8f, 0.1f, 0.1f, pulse * 0.9f);

        // Background with pulse
        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, boxHeight), GetTexture("white"));
        GUI.color = Color.white;

        // Warning icon
        GUIStyle iconStyle = new GUIStyle();
        iconStyle.fontSize = 28;
        iconStyle.fontStyle = FontStyle.Bold;
        iconStyle.alignment = TextAnchor.MiddleCenter;
        iconStyle.normal.textColor = new Color(1f, 1f, 0.3f, pulse);
        GUI.Label(new Rect(boxX + 10, boxY, 40, boxHeight), isDrowning ? "~" : "!", iconStyle);

        // Warning text
        GUIStyle warnStyle = new GUIStyle();
        warnStyle.fontSize = 16;
        warnStyle.fontStyle = FontStyle.Bold;
        warnStyle.alignment = TextAnchor.MiddleCenter;
        warnStyle.normal.textColor = Color.white;
        warnStyle.wordWrap = true;

        string warningText = isDrowning
            ? "DROWNING! Get back to land!"
            : "LOW HEALTH! Eat some fish from the BBQ now!";

        GUI.Label(new Rect(boxX + 50, boxY, boxWidth - 60, boxHeight), warningText, warnStyle);
    }

    void DrawTutorialTip()
    {
        // Cute speech bubble
        float bubbleWidth = 320;
        float bubbleHeight = 100;
        float bubbleX = 20;
        float bubbleY = Screen.height - 180;

        // Bubble background (cream colored)
        GUI.color = new Color(1f, 0.98f, 0.9f, 0.95f);
        GUI.DrawTexture(new Rect(bubbleX, bubbleY, bubbleWidth, bubbleHeight), GetTexture("white"));

        // Border
        GUI.color = new Color(0.3f, 0.5f, 0.7f, 1f);
        GUI.DrawTexture(new Rect(bubbleX - 2, bubbleY - 2, bubbleWidth + 4, 2), GetTexture("white"));
        GUI.DrawTexture(new Rect(bubbleX - 2, bubbleY + bubbleHeight, bubbleWidth + 4, 2), GetTexture("white"));
        GUI.DrawTexture(new Rect(bubbleX - 2, bubbleY, 2, bubbleHeight), GetTexture("white"));
        GUI.DrawTexture(new Rect(bubbleX + bubbleWidth, bubbleY, 2, bubbleHeight), GetTexture("white"));
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 12;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.2f, 0.4f, 0.6f);
        GUI.Label(new Rect(bubbleX, bubbleY + 8, bubbleWidth, 16), "~ Wetsuit Pete says ~", titleStyle);

        // Tip text
        GUIStyle tipStyle = new GUIStyle();
        tipStyle.fontSize = 14;
        tipStyle.fontStyle = FontStyle.Italic;
        tipStyle.alignment = TextAnchor.MiddleCenter;
        tipStyle.normal.textColor = new Color(0.15f, 0.15f, 0.15f);
        tipStyle.wordWrap = true;
        tipStyle.padding = new RectOffset(15, 15, 0, 0);

        GUI.Label(new Rect(bubbleX, bubbleY + 28, bubbleWidth, 50),
            "\"You are losing health! You must cook the fishy on the barby to stay alive...\"", tipStyle);

        // Dismiss hint
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 10;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(bubbleX, bubbleY + bubbleHeight - 20, bubbleWidth, 16), "(press any key to dismiss)", hintStyle);
    }

    void DrawHealthUI()
    {
        float panelX = Screen.width - 180;
        float panelY = 10;
        float panelWidth = 170;

        // Show buff timer if active
        if (hasMaxHealthBuff)
        {
            DrawBuffTimer(panelX, panelY - 30, panelWidth);
        }

        // HP Bar section
        float hpBarHeight = 22;

        // Border
        GUI.DrawTexture(new Rect(panelX - 2, panelY - 2, panelWidth + 4, hpBarHeight + 4), GetTexture("border"));

        // Background
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, hpBarHeight), GetTexture("hpBarBg"));

        // Fill based on health percentage
        float healthPercent = currentHealth / maxHealth;
        Texture2D fillTex;
        if (healthPercent > 0.6f)
            fillTex = GetTexture("hpBarFillHigh");
        else if (healthPercent > 0.3f)
            fillTex = GetTexture("hpBarFillMid");
        else
            fillTex = GetTexture("hpBarFill");

        GUI.DrawTexture(new Rect(panelX + 2, panelY + 2, (panelWidth - 4) * healthPercent, hpBarHeight - 4), fillTex);

        // HP Text
        GUIStyle hpStyle = new GUIStyle();
        hpStyle.fontSize = 12;
        hpStyle.fontStyle = FontStyle.Bold;
        hpStyle.alignment = TextAnchor.MiddleCenter;
        hpStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(panelX, panelY, panelWidth, hpBarHeight), $"HP: {Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}", hpStyle);

        // ECG Monitor below HP bar
        float ecgY = panelY + hpBarHeight + 5;
        float ecgHeight = 50;

        DrawECGMonitor(new Rect(panelX, ecgY, panelWidth, ecgHeight));
    }

    void DrawECGMonitor(Rect rect)
    {
        // Border
        GUI.DrawTexture(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), GetTexture("border"));

        // Dark green background (hospital monitor style)
        GUI.DrawTexture(rect, GetTexture("ecgBg"));

        // Grid lines
        for (int i = 1; i < 5; i++)
        {
            float gridY = rect.y + (rect.height * i / 5f);
            GUI.DrawTexture(new Rect(rect.x, gridY, rect.width, 1), GetTexture("ecgGrid"));
        }
        for (int i = 1; i < 8; i++)
        {
            float gridX = rect.x + (rect.width * i / 8f);
            GUI.DrawTexture(new Rect(gridX, rect.y, 1, rect.height), GetTexture("ecgGrid"));
        }

        // Draw ECG waveform
        float centerY = rect.y + rect.height * 0.5f;
        float amplitude = rect.height * 0.4f;

        for (int i = 1; i < ecgHistory.Length; i++)
        {
            int prevIdx = (ecgIndex + i - 1) % ecgHistory.Length;
            int currIdx = (ecgIndex + i) % ecgHistory.Length;

            float x1 = rect.x + (float)(i - 1) / ecgHistory.Length * rect.width;
            float x2 = rect.x + (float)i / ecgHistory.Length * rect.width;
            float y1 = centerY - ecgHistory[prevIdx] * amplitude;
            float y2 = centerY - ecgHistory[currIdx] * amplitude;

            DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), GetTexture("ecgLine"), 2);
        }

        // BPM display
        GUIStyle bpmStyle = new GUIStyle();
        bpmStyle.fontSize = 10;
        bpmStyle.fontStyle = FontStyle.Bold;
        bpmStyle.normal.textColor = new Color(0.2f, 1f, 0.3f);

        GUI.Label(new Rect(rect.x + 5, rect.y + 2, 60, 14), $"{currentBPM} BPM", bpmStyle);

        // Heart icon (pulsing)
        float pulse = 0.8f + Mathf.Sin(Time.time * currentBPM / 60f * Mathf.PI * 2f) * 0.2f;
        bpmStyle.fontSize = (int)(12 * pulse);
        bpmStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
        GUI.Label(new Rect(rect.x + rect.width - 20, rect.y + 2, 20, 16), "<3", bpmStyle);
    }

    void DrawBuffTimer(float x, float y, float width)
    {
        // Golden background for buff timer
        GUI.color = new Color(1f, 0.85f, 0.3f, 0.9f);
        GUI.DrawTexture(new Rect(x, y, width, 22), GetTexture("white"));
        GUI.color = Color.white;

        // Format time remaining
        int mins = (int)(maxHealthBuffTimeRemaining / 60);
        int secs = (int)(maxHealthBuffTimeRemaining % 60);
        string timeStr = mins > 0 ? $"{mins}m {secs}s" : $"{secs}s";

        GUIStyle buffStyle = new GUIStyle();
        buffStyle.fontSize = 11;
        buffStyle.fontStyle = FontStyle.Bold;
        buffStyle.alignment = TextAnchor.MiddleCenter;
        buffStyle.normal.textColor = new Color(0.3f, 0.2f, 0f);

        GUI.Label(new Rect(x, y, width, 22), $"MAX HP: {timeStr}", buffStyle);
    }

    void DrawLine(Vector2 start, Vector2 end, Texture2D tex, float width)
    {
        Vector2 delta = end - start;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        float length = delta.magnitude;

        if (length < 0.1f) return;

        Matrix4x4 matrixBackup = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width / 2, length, width), tex);
        GUI.matrix = matrixBackup;
    }

    void DrawDeathScreen()
    {
        // Red overlay
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("deathOverlay"));

        // Simple death message
        GUIStyle deathStyle = new GUIStyle();
        deathStyle.fontSize = 56;
        deathStyle.fontStyle = FontStyle.Bold;
        deathStyle.alignment = TextAnchor.MiddleCenter;
        deathStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(0, Screen.height / 2 - 50, Screen.width, 70), "YOU DIED", deathStyle);

        // Countdown
        float remainingTime = respawnDelay - deathTimer;
        deathStyle.fontSize = 20;
        deathStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
        GUI.Label(new Rect(0, Screen.height / 2 + 30, Screen.width, 30), $"{remainingTime:F0}", deathStyle);
    }

    // Public getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public bool IsDead() => isDead;

    // Setter for save/load system
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        if (currentHealth <= 0 && !isDead)
        {
            Die();
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
