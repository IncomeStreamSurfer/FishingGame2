using UnityEngine;

/// <summary>
/// Wet Debuff System
/// - When player touches water (Y < 0.85), they get the WET debuff
/// - Displays a blue debuff bar saying "WET"
/// - While wet, player loses 1 extra HP per 5 seconds (on top of normal hunger)
/// - Debuff goes away when player gets out of water (Y >= 1.0)
/// </summary>
public class WetDebuffSystem : MonoBehaviour
{
    public static WetDebuffSystem Instance { get; private set; }

    // Wet state
    private bool isWet = false;
    private bool isDrying = false;  // True when out of water but still wet
    private float dryingTimer = 0f;
    private float dryingDuration = 15f;  // 15 seconds to dry off after leaving water
    private float wetDamageTimer = 0f;
    private float wetDamageInterval = 5f; // 1 HP every 5 seconds
    private float wetDamageAmount = 1f;

    // Water level detection
    private float waterLevel = 0.85f; // Below this = wet
    private float dryLevel = 1.0f; // Above this = dry

    // UI elements
    private Texture2D debuffBgTex;
    private Texture2D debuffBarTex;
    private GUIStyle debuffLabelStyle;
    private bool guiInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        InitializeGUI();
    }

    void InitializeGUI()
    {
        // Background texture - dark blue
        debuffBgTex = new Texture2D(1, 1);
        debuffBgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.2f, 0.85f));
        debuffBgTex.Apply();

        // Bar texture - blue
        debuffBarTex = new Texture2D(1, 1);
        debuffBarTex.SetPixel(0, 0, new Color(0.3f, 0.5f, 0.9f, 0.9f));
        debuffBarTex.Apply();

        // Label style will be initialized in OnGUI (can't access GUI.skin outside OnGUI)
        debuffLabelStyle = null;

        guiInitialized = true;
    }

    void Update()
    {
        if (!MainMenu.GameStarted || !guiInitialized) return;

        // Check if player is in water
        if (!GameCache.IsPlayerValid()) return;

        float playerY = GameCache.Player.position.y;

        // Player gets wet when below water level
        if (playerY < waterLevel)
        {
            if (!isWet)
            {
                // Just got wet
                isWet = true;
                isDrying = false;
                dryingTimer = 0f;
                wetDamageTimer = 0f;
                Debug.Log("Player got WET!");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("You are WET!", new Color(0.4f, 0.6f, 1f));
                }
            }
            else if (isDrying)
            {
                // Player went back in water while drying - reset drying
                isDrying = false;
                dryingTimer = 0f;
            }
        }
        // Player leaves water - start drying timer
        else if (playerY >= dryLevel)
        {
            if (isWet && !isDrying)
            {
                // Just left water - start drying
                isDrying = true;
                dryingTimer = 0f;
                Debug.Log("Player left water - drying for 15 seconds...");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Drying off... (15s)", new Color(0.5f, 0.7f, 0.9f));
                }
            }
        }

        // Handle drying timer
        if (isDrying)
        {
            dryingTimer += Time.deltaTime;
            if (dryingTimer >= dryingDuration)
            {
                // Fully dried off
                isWet = false;
                isDrying = false;
                dryingTimer = 0f;
                wetDamageTimer = 0f;
                Debug.Log("Player fully dried off!");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("You dried off!", new Color(0.7f, 0.8f, 0.9f));
                }
            }
        }

        // Apply wet damage (both while in water AND while drying)
        if (isWet)
        {
            wetDamageTimer += Time.deltaTime;
            if (wetDamageTimer >= wetDamageInterval)
            {
                wetDamageTimer = 0f;
                ApplyWetDamage();
            }
        }
    }

    void ApplyWetDamage()
    {
        if (PlayerHealth.Instance != null)
        {
            // Apply wet damage (on top of normal hunger damage)
            PlayerHealth.Instance.TakeDamage(wetDamageAmount);

            // Show notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"-{wetDamageAmount} HP (Wet)", new Color(0.5f, 0.7f, 1f));
            }

            Debug.Log($"Wet damage: {wetDamageAmount} HP");
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !guiInitialized) return;
        if (!isWet) return;

        // Initialize label style here (can only access GUI.skin inside OnGUI)
        if (debuffLabelStyle == null)
        {
            debuffLabelStyle = new GUIStyle(GUI.skin.label);
        }

        DrawWetDebuff();
    }

    void DrawWetDebuff()
    {
        // Draw WET debuff bar on right side, below buffs and HP/ECG
        // HP bar + ECG = ~70px, buffs take ~40px each
        // We'll position below all active buffs

        float panelX = Screen.width - 180;
        float panelY = 75; // Start below ECG

        // Offset by active buffs if FishBuffSystem exists
        if (FishBuffSystem.Instance != null)
        {
            int activeBuffCount = FishBuffSystem.Instance.activeBuffs.Count;
            panelY += activeBuffCount * (38 + 4); // Each buff is 38px high + 4px spacing
        }

        float debuffHeight = 38;
        float debuffWidth = 170;

        // Background
        GUI.DrawTexture(new Rect(panelX, panelY, debuffWidth, debuffHeight), debuffBgTex);

        // Timer bar - shows drying progress if drying, otherwise damage tick timer
        float pct;
        Color barColor;
        if (isDrying)
        {
            // Show drying countdown (bar depletes as you dry)
            pct = 1f - (dryingTimer / dryingDuration);
            barColor = new Color(0.5f, 0.7f, 0.9f); // Lighter blue when drying
        }
        else
        {
            // Show damage tick timer
            pct = 1f - (wetDamageTimer / wetDamageInterval);
            barColor = new Color(0.3f, 0.5f, 0.9f); // Blue color
        }
        GUI.color = barColor;
        GUI.DrawTexture(new Rect(panelX + 2, panelY + debuffHeight - 6, (debuffWidth - 4) * pct, 4), debuffBarTex);
        GUI.color = Color.white;

        // Debuff name - "WET" or "WET (Drying)"
        debuffLabelStyle.fontSize = 11;
        debuffLabelStyle.fontStyle = FontStyle.Bold;
        debuffLabelStyle.normal.textColor = new Color(0.4f, 0.6f, 1f); // Light blue
        debuffLabelStyle.alignment = TextAnchor.UpperLeft;

        string wetLabel = isDrying ? $"WET ({Mathf.CeilToInt(dryingDuration - dryingTimer)}s)" : "WET";
        GUI.Label(new Rect(panelX + 6, panelY + 3, debuffWidth - 12, 16), wetLabel, debuffLabelStyle);

        // Description
        debuffLabelStyle.fontSize = 9;
        debuffLabelStyle.fontStyle = FontStyle.Normal;
        debuffLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        string desc = isDrying ? "-1 HP every 5s (drying...)" : "-1 HP every 5s";
        GUI.Label(new Rect(panelX + 6, panelY + 18, debuffWidth - 12, 14), desc, debuffLabelStyle);
    }

    // Public methods
    public bool IsWet() => isWet;

    public void ClearWetDebuff()
    {
        isWet = false;
        isDrying = false;
        dryingTimer = 0f;
        wetDamageTimer = 0f;
        Debug.Log("Wet debuff cleared");
    }

    void OnDestroy()
    {
        if (debuffBgTex != null) Destroy(debuffBgTex);
        if (debuffBarTex != null) Destroy(debuffBarTex);
        if (Instance == this) Instance = null;
    }
}
