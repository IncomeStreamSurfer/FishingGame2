using UnityEngine;

/// <summary>
/// Nighttime Cold Mechanic for Tropical Island
/// - When night falls (6 PM to 6 AM), player gets cold
/// - If not wearing clothing, loses 5 HP every 10 seconds
/// - Shows "Too cold!" warning with pulsing effect
/// - Elevates heart rate monitor BPM when cold
/// - Only active in Tropical Island realm
/// </summary>
public class ColdMechanic : MonoBehaviour
{
    public static ColdMechanic Instance { get; private set; }

    // Cold damage settings
    private bool isCold = false;
    private float coldDamageTimer = 0f;
    private float coldDamageInterval = 10f; // 5 HP every 10 seconds
    private float coldDamageAmount = 5f;

    // Warning UI
    private bool showColdWarning = false;
    private float warningPulse = 0f;

    // BPM boost when cold
    private int coldBPMBoost = 25; // Add 25 BPM when cold

    // Cached textures
    private Texture2D warningBgTexture;
    private bool initialized = false;

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
        CreateCachedTextures();
        initialized = true;
    }

    void CreateCachedTextures()
    {
        // Create warning background texture
        warningBgTexture = new Texture2D(2, 2);
        Color[] pixels = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            pixels[i] = Color.white;
        }
        warningBgTexture.SetPixels(pixels);
        warningBgTexture.Apply();
    }

    void Update()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        // Only active in Tropical Island realm
        if (GameCache.GetCurrentRealm() != RealmType.TropicalIsland)
        {
            isCold = false;
            showColdWarning = false;
            return;
        }

        // Check if it's nighttime
        bool isNighttime = IsNighttime();

        // Check if player is wearing clothing
        bool isWearingClothes = IsWearingClothes();

        // Player is cold if it's night AND not wearing clothes
        bool shouldBeCold = isNighttime && !isWearingClothes;

        // Update cold state
        if (shouldBeCold && !isCold)
        {
            // Just became cold
            isCold = true;
            coldDamageTimer = 0f;
            Debug.Log("Player is getting cold! Put on some clothes!");
        }
        else if (!shouldBeCold && isCold)
        {
            // No longer cold
            isCold = false;
            showColdWarning = false;
            Debug.Log("Player warmed up!");
        }

        // Apply cold damage
        if (isCold)
        {
            showColdWarning = true;
            warningPulse += Time.deltaTime * 4f;

            coldDamageTimer += Time.deltaTime;
            if (coldDamageTimer >= coldDamageInterval)
            {
                coldDamageTimer = 0f;
                ApplyColdDamage();
            }
        }
    }

    void ApplyColdDamage()
    {
        if (PlayerHealth.Instance != null)
        {
            // Apply cold damage with custom death message
            PlayerHealth.Instance.TakeDamage(coldDamageAmount, "You froze to death in the cold night...");

            // Show notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"-{coldDamageAmount} HP (Cold)", new Color(0.5f, 0.7f, 1f));
            }

            Debug.Log($"Cold damage: {coldDamageAmount} HP");
        }
    }

    bool IsNighttime()
    {
        // Night is from 6 PM (18:00) to 6 AM (6:00)
        if (DayNightCycle.Instance != null)
        {
            return DayNightCycle.Instance.IsNight();
        }

        // Fallback - assume it's not night if DayNightCycle doesn't exist
        return false;
    }

    bool IsWearingClothes()
    {
        // Check if player is wearing any clothing
        if (PlayerClothingVisuals.Instance != null)
        {
            string headItem = PlayerClothingVisuals.Instance.GetCurrentHeadItem();
            string topItem = PlayerClothingVisuals.Instance.GetCurrentTopItem();
            string legsItem = PlayerClothingVisuals.Instance.GetCurrentLegsItem();
            string accessoryItem = PlayerClothingVisuals.Instance.GetCurrentAccessory();

            // Player is considered "wearing clothes" if they have any top or legs item
            // (excluding just underpants)
            bool hasTop = !string.IsNullOrEmpty(topItem) && topItem != "None";
            bool hasLegs = !string.IsNullOrEmpty(legsItem) && legsItem != "None" && legsItem != "Underpants";

            // Must have BOTH top and legs to stay warm
            return hasTop && hasLegs;
        }

        // Fallback - assume naked if we can't check
        return false;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !initialized) return;
        if (!showColdWarning) return;

        // Only show in Tropical Island
        if (GameCache.GetCurrentRealm() != RealmType.TropicalIsland) return;

        DrawColdWarning();
    }

    void DrawColdWarning()
    {
        // Pulsing cold warning (blue/cyan color scheme)
        float pulse = 0.7f + Mathf.Sin(warningPulse) * 0.3f;

        float boxWidth = 350;
        float boxHeight = 60;
        float boxX = (Screen.width - boxWidth) / 2;
        float boxY = Screen.height * 0.35f;

        // Cold blue background with pulse
        Color bgColor = new Color(0.2f, 0.5f, 0.9f, pulse * 0.9f);

        // Background with pulse
        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, boxHeight), warningBgTexture);
        GUI.color = Color.white;

        // Warning icon (snowflake-like)
        GUIStyle iconStyle = new GUIStyle();
        iconStyle.fontSize = 32;
        iconStyle.fontStyle = FontStyle.Bold;
        iconStyle.alignment = TextAnchor.MiddleCenter;
        iconStyle.normal.textColor = new Color(0.8f, 0.9f, 1f, pulse);
        GUI.Label(new Rect(boxX + 10, boxY, 40, boxHeight), "*", iconStyle);

        // Warning text
        GUIStyle warnStyle = new GUIStyle();
        warnStyle.fontSize = 16;
        warnStyle.fontStyle = FontStyle.Bold;
        warnStyle.alignment = TextAnchor.MiddleCenter;
        warnStyle.normal.textColor = Color.white;
        warnStyle.wordWrap = true;

        string warningText = "Too cold! Wear clothes or wait for dawn!";

        GUI.Label(new Rect(boxX + 50, boxY, boxWidth - 60, boxHeight), warningText, warnStyle);
    }

    // Public methods for other systems to query cold state
    public bool IsCold() => isCold;

    public int GetColdBPMBoost()
    {
        return isCold ? coldBPMBoost : 0;
    }

    void OnDestroy()
    {
        if (warningBgTexture != null)
        {
            Destroy(warningBgTexture);
        }
    }
}
