using UnityEngine;

/// <summary>
/// Toxic Slime Puddle System for Void Realm
/// - Deals 15 damage every 5 seconds while player stands in puddle
/// - HAZMAT suit (ToxicImmunity) provides complete protection
/// - Visual green screen edge warning effect
/// - Procedural toxic bubbling sound effect
/// </summary>
public class ToxicSlime : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 15f;
    [SerializeField] private float damageInterval = 5f;

    [Header("Audio Settings")]
    [SerializeField] private bool enableSound = true;
    [SerializeField] private float soundVolume = 0.3f;

    // Player tracking
    private bool playerInPuddle = false;
    private float damageTimer = 0f;
    private GameObject player;

    // Visual warning
    private bool showWarning = false;
    private float warningPulse = 0f;

    // Audio
    private AudioSource audioSource;
    private float audioTimer = 0f;
    private float audioInterval = 1.5f; // Bubble sound every 1.5 seconds

    // Cached textures
    private Texture2D greenOverlayTexture;

    void Start()
    {
        // Setup audio source for procedural sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.7f; // Somewhat 3D
        audioSource.volume = soundVolume;
        audioSource.maxDistance = 20f;

        // Create green overlay texture
        CreateOverlayTexture();
    }

    void CreateOverlayTexture()
    {
        greenOverlayTexture = new Texture2D(2, 2);
        Color greenTint = new Color(0f, 0.8f, 0.2f, 0.3f);
        Color[] pixels = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            pixels[i] = greenTint;
        }
        greenOverlayTexture.SetPixels(pixels);
        greenOverlayTexture.Apply();
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        if (playerInPuddle)
        {
            // Check if player has HAZMAT suit (Toxic Immunity)
            bool hasProtection = AccessorySystem.Instance != null &&
                               AccessorySystem.Instance.HasEffect(AccessoryEffect.ToxicImmunity);

            if (!hasProtection)
            {
                // Increment damage timer
                damageTimer += Time.deltaTime;

                if (damageTimer >= damageInterval)
                {
                    // Deal damage
                    if (PlayerHealth.Instance != null)
                    {
                        PlayerHealth.Instance.TakeDamage(damageAmount);
                        Debug.Log($"Toxic slime dealt {damageAmount} damage!");
                    }
                    damageTimer = 0f;
                }

                // Update warning pulse
                warningPulse += Time.deltaTime * 3f;
            }
            else
            {
                // Protected by HAZMAT suit - no damage
                damageTimer = 0f;
            }

            // Play bubbling sound periodically
            if (enableSound)
            {
                audioTimer += Time.deltaTime;
                if (audioTimer >= audioInterval)
                {
                    PlayToxicBubbleSound();
                    audioTimer = 0f;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player" || other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerInPuddle = true;
            showWarning = true;
            damageTimer = 0f;
            audioTimer = 0f;

            // Check if player has protection
            bool hasProtection = AccessorySystem.Instance != null &&
                               AccessorySystem.Instance.HasEffect(AccessoryEffect.ToxicImmunity);

            if (hasProtection)
            {
                Debug.Log("Entered toxic slime puddle - HAZMAT suit protecting!");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("HAZMAT Suit Active", new Color(0.2f, 1f, 0.2f));
                }
            }
            else
            {
                Debug.Log("Entered toxic slime puddle - taking damage!");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("TOXIC SLIME!", new Color(0.2f, 1f, 0.2f));
                }
            }

            // Play initial bubble sound
            if (enableSound)
            {
                PlayToxicBubbleSound();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "Player" || other.CompareTag("Player"))
        {
            playerInPuddle = true;
            player = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player" || other.CompareTag("Player"))
        {
            playerInPuddle = false;
            showWarning = false;
            damageTimer = 0f;
            audioTimer = 0f;
            player = null;

            Debug.Log("Exited toxic slime puddle");
        }
    }

    void PlayToxicBubbleSound()
    {
        if (audioSource == null) return;

        // Generate procedural toxic bubbling sound
        int sampleRate = 22050;
        float duration = 0.3f;
        int sampleCount = (int)(sampleRate * duration);

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Combine multiple sine waves for a complex bubbling effect
            float baseFreq = Random.Range(80f, 150f); // Low bubbling frequency
            float harmonic1 = baseFreq * 2.5f;
            float harmonic2 = baseFreq * 4.2f;

            // Create bubble envelope (pop sound)
            float envelope = Mathf.Exp(-t * 8f);

            // Mix frequencies with random variation
            float noise = Random.Range(-0.1f, 0.1f);
            float sample = (
                Mathf.Sin(2 * Mathf.PI * baseFreq * t) * 0.5f +
                Mathf.Sin(2 * Mathf.PI * harmonic1 * t) * 0.3f +
                Mathf.Sin(2 * Mathf.PI * harmonic2 * t) * 0.2f +
                noise
            ) * envelope;

            samples[i] = sample * 0.5f; // Reduce volume
        }

        // Create and play audio clip
        AudioClip bubbleClip = AudioClip.Create("ToxicBubble", sampleCount, 1, sampleRate, false);
        bubbleClip.SetData(samples, 0);
        audioSource.clip = bubbleClip;
        audioSource.pitch = Random.Range(0.9f, 1.1f); // Slight pitch variation
        audioSource.Play();
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !showWarning || !playerInPuddle) return;

        // Check if player has protection
        bool hasProtection = AccessorySystem.Instance != null &&
                           AccessorySystem.Instance.HasEffect(AccessoryEffect.ToxicImmunity);

        if (hasProtection)
        {
            // Show protected indicator (lighter green)
            DrawProtectedOverlay();
        }
        else
        {
            // Show danger warning (stronger green with pulsing edges)
            DrawToxicWarning();
        }
    }

    void DrawToxicWarning()
    {
        // Pulsing green screen edge effect
        float pulse = 0.5f + Mathf.Sin(warningPulse) * 0.3f;

        // Draw green edges around screen
        int edgeThickness = 30;

        // Top edge
        GUI.color = new Color(0f, 1f, 0.2f, pulse * 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, edgeThickness), greenOverlayTexture);

        // Bottom edge
        GUI.DrawTexture(new Rect(0, Screen.height - edgeThickness, Screen.width, edgeThickness), greenOverlayTexture);

        // Left edge
        GUI.DrawTexture(new Rect(0, 0, edgeThickness, Screen.height), greenOverlayTexture);

        // Right edge
        GUI.DrawTexture(new Rect(Screen.width - edgeThickness, 0, edgeThickness, Screen.height), greenOverlayTexture);

        // Full screen subtle overlay
        GUI.color = new Color(0f, 0.8f, 0.2f, pulse * 0.15f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), greenOverlayTexture);

        GUI.color = Color.white;

        // Warning text
        GUIStyle warningStyle = new GUIStyle();
        warningStyle.fontSize = 18;
        warningStyle.fontStyle = FontStyle.Bold;
        warningStyle.alignment = TextAnchor.MiddleCenter;
        warningStyle.normal.textColor = new Color(0.2f, 1f, 0.3f, pulse);

        // Calculate time until next damage tick
        float timeUntilDamage = damageInterval - damageTimer;
        string warningText = $"TOXIC SLIME - {damageAmount} DMG in {Mathf.CeilToInt(timeUntilDamage)}s";

        float warningBoxWidth = 400;
        float warningBoxHeight = 40;
        float warningX = (Screen.width - warningBoxWidth) / 2;
        float warningY = Screen.height * 0.15f;

        // Background for warning text
        GUI.color = new Color(0.05f, 0.15f, 0.05f, pulse * 0.8f);
        GUI.DrawTexture(new Rect(warningX, warningY, warningBoxWidth, warningBoxHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(warningX, warningY, warningBoxWidth, warningBoxHeight), warningText, warningStyle);

        // Hint about HAZMAT suit
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 12;
        hintStyle.fontStyle = FontStyle.Italic;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.5f, 0.9f, 0.5f, pulse * 0.7f);

        GUI.Label(new Rect(warningX, warningY + warningBoxHeight + 5, warningBoxWidth, 20),
                  "Need HAZMAT Suit for protection!", hintStyle);
    }

    void DrawProtectedOverlay()
    {
        // Subtle green glow to indicate protection is active
        float pulse = 0.3f + Mathf.Sin(Time.time * 2f) * 0.1f;

        // Draw subtle green edges
        int edgeThickness = 15;

        GUI.color = new Color(0.2f, 1f, 0.3f, pulse * 0.3f);

        // Top edge
        GUI.DrawTexture(new Rect(0, 0, Screen.width, edgeThickness), greenOverlayTexture);

        // Bottom edge
        GUI.DrawTexture(new Rect(0, Screen.height - edgeThickness, Screen.width, edgeThickness), greenOverlayTexture);

        // Left edge
        GUI.DrawTexture(new Rect(0, 0, edgeThickness, Screen.height), greenOverlayTexture);

        // Right edge
        GUI.DrawTexture(new Rect(Screen.width - edgeThickness, 0, edgeThickness, Screen.height), greenOverlayTexture);

        GUI.color = Color.white;

        // Protected status text
        GUIStyle protectedStyle = new GUIStyle();
        protectedStyle.fontSize = 14;
        protectedStyle.fontStyle = FontStyle.Bold;
        protectedStyle.alignment = TextAnchor.MiddleCenter;
        protectedStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);

        float statusWidth = 300;
        float statusHeight = 30;
        float statusX = (Screen.width - statusWidth) / 2;
        float statusY = Screen.height * 0.15f;

        // Background
        GUI.color = new Color(0.05f, 0.2f, 0.05f, 0.7f);
        GUI.DrawTexture(new Rect(statusX, statusY, statusWidth, statusHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(statusX, statusY, statusWidth, statusHeight),
                  "HAZMAT SUIT - PROTECTED", protectedStyle);
    }

    void OnDestroy()
    {
        if (greenOverlayTexture != null)
        {
            Destroy(greenOverlayTexture);
        }
    }
}
