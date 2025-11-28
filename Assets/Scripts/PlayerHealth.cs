using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Player Health System
/// - Starts at 100 HP
/// - Loses 1 HP every 10 minutes
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
    private float healthDecayInterval = 600f; // 10 minutes = 600 seconds

    // Death state
    private bool isDead = false;
    private float deathTimer = 0f;
    private float respawnDelay = 3f;

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

        // Health decay - 1 HP every 10 minutes
        healthDecayTimer += Time.deltaTime;
        if (healthDecayTimer >= healthDecayInterval)
        {
            healthDecayTimer = 0f;
            TakeDamage(1f);
            Debug.Log("Health decayed by 1 HP due to hunger");
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
    }

    void DrawHealthUI()
    {
        float panelX = Screen.width - 180;
        float panelY = 10;
        float panelWidth = 170;

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

        // Death message
        GUIStyle deathStyle = new GUIStyle();
        deathStyle.fontSize = 48;
        deathStyle.fontStyle = FontStyle.Bold;
        deathStyle.alignment = TextAnchor.MiddleCenter;
        deathStyle.normal.textColor = Color.white;

        float remainingTime = respawnDelay - deathTimer;
        GUI.Label(new Rect(0, Screen.height / 2 - 60, Screen.width, 60), "YOU DIED", deathStyle);

        deathStyle.fontSize = 24;
        deathStyle.normal.textColor = new Color(1f, 0.8f, 0.8f);
        GUI.Label(new Rect(0, Screen.height / 2, Screen.width, 40), $"Respawning in {remainingTime:F1}s...", deathStyle);

        deathStyle.fontSize = 16;
        deathStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        GUI.Label(new Rect(0, Screen.height / 2 + 50, Screen.width, 30), "Gold and cosmetics will be preserved", deathStyle);
    }

    // Public getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public bool IsDead() => isDead;

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}
