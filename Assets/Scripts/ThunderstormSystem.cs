using UnityEngine;
using System.Collections;

/// <summary>
/// Thunderstorm System - Dangerous weather events
/// Occurs randomly once every 1-2 in-game days
/// Lightning can kill players on the dock
/// Players are safe on land
/// </summary>
public class ThunderstormSystem : MonoBehaviour
{
    public static ThunderstormSystem Instance { get; private set; }

    [Header("Storm Timing")]
    private float stormTimer = 0f;
    private float nextStormTime = 0f;
    private float minTimeBetweenStorms = 300f;  // 5 minutes (1 in-game day)
    private float maxTimeBetweenStorms = 600f;  // 10 minutes (2 in-game days)
    private float stormDuration = 0f;
    private float minStormDuration = 60f;       // 1 minute
    private float maxStormDuration = 120f;      // 2 minutes

    [Header("Storm State")]
    private bool isStormActive = false;
    private float stormIntensity = 0f;          // 0 to 1, fades in/out
    private float targetStormIntensity = 0f;
    private float stormElapsedTime = 0f;

    [Header("Lightning Strike")]
    private float lightningCheckTimer = 0f;
    private float lightningCheckInterval = 1f;  // Check every second
    private float lightningChancePerStorm = 0.25f; // 25% chance per storm // 1 in 100 chance
    private bool lightningWarningShown = false;
    private float warningTime = 2f;             // Show warning 2 seconds before strike
    private bool lightningStrikeQueued = false;
    private float lightningStrikeTimer = 0f;

    [Header("Visual Effects")]
    private float screenFlashAlpha = 0f;
    private float skyDarkenAmount = 0f;
    private Light sunLight;
    private float baseSunIntensity = 1.3f;
    private float stormSunIntensity = 0.3f;
    private Color baseSkyColor = new Color(0.5f, 0.7f, 1f);
    private Color stormSkyColor = new Color(0.2f, 0.2f, 0.25f);

    [Header("Audio")]
    private AudioSource thunderAudioSource;
    private AudioSource rainAudioSource;
    private AudioSource lightningCrackSource;

    [Header("Dock Detection")]
    // Main dock (Tropical): X=-12, Z=5 to Z=55
    private float mainDockXMin = -15f;
    private float mainDockXMax = -9f;
    private float mainDockZMin = 5f;
    private float mainDockZMax = 60f;
    // Ice Realm dock: X=500, Z=25 to Z=65
    private float iceDockXMin = 497f;
    private float iceDockXMax = 503f;
    private float iceDockZMin = 22f;
    private float iceDockZMax = 70f;
    // Jungle Realm dock: X=988, Z=5 to Z=55
    private float jungleDockXMin = 985f;
    private float jungleDockXMax = 991f;
    private float jungleDockZMin = 5f;
    private float jungleDockZMax = 60f;
    // Bridge to Goldie's island
    private float bridgeXMin = 22f;
    private float bridgeXMax = 28f;
    private float bridgeZMin = 25f;
    private float bridgeZMax = 80f;

    [Header("Cached Textures")]
    private Texture2D whiteTex;
    private Texture2D warningTex;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Find sun light
        GameObject sun = GameObject.Find("Sun");
        if (sun != null)
        {
            sunLight = sun.GetComponent<Light>();
            if (sunLight != null)
            {
                baseSunIntensity = sunLight.intensity;
            }
        }

        // Setup audio sources
        SetupAudio();

        // Create textures
        whiteTex = new Texture2D(2, 2);
        whiteTex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        whiteTex.Apply();

        warningTex = new Texture2D(2, 2);
        warningTex.SetPixels(new Color[] { Color.yellow, Color.yellow, Color.yellow, Color.yellow });
        warningTex.Apply();

        // Schedule first storm
        nextStormTime = Random.Range(minTimeBetweenStorms, maxTimeBetweenStorms);
        Debug.Log($"ThunderstormSystem: Next storm in {nextStormTime:F0} seconds");
    }

    void SetupAudio()
    {
        // Thunder rumble audio source
        thunderAudioSource = gameObject.AddComponent<AudioSource>();
        thunderAudioSource.loop = true;
        thunderAudioSource.spatialBlend = 0f;
        thunderAudioSource.volume = 0f;
        thunderAudioSource.playOnAwake = false;

        // Rain audio source
        rainAudioSource = gameObject.AddComponent<AudioSource>();
        rainAudioSource.loop = true;
        rainAudioSource.spatialBlend = 0f;
        rainAudioSource.volume = 0f;
        rainAudioSource.playOnAwake = false;

        // Lightning crack source (one-shot)
        lightningCrackSource = gameObject.AddComponent<AudioSource>();
        lightningCrackSource.spatialBlend = 0f;
        lightningCrackSource.playOnAwake = false;

        // Create procedural audio clips
        thunderAudioSource.clip = CreateThunderRumbleClip();
        rainAudioSource.clip = CreateHeavyRainClip();
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Update storm timer
        stormTimer += Time.deltaTime;

        if (!isStormActive)
        {
            // Check if time for next storm
            if (stormTimer >= nextStormTime)
            {
                StartStorm();
            }
        }
        else
        {
            // Update active storm
            UpdateStorm();
        }

        // Smoothly transition storm intensity
        stormIntensity = Mathf.MoveTowards(stormIntensity, targetStormIntensity, Time.deltaTime * 0.3f);

        // Update visual effects
        UpdateVisuals();

        // Update audio
        UpdateAudio();

        // Fade out screen flash
        if (screenFlashAlpha > 0f)
        {
            screenFlashAlpha -= Time.deltaTime * 3f;
            screenFlashAlpha = Mathf.Max(0f, screenFlashAlpha);
        }
    }

    void StartStorm()
    {
        isStormActive = true;
        targetStormIntensity = 1f;
        stormElapsedTime = 0f;
        stormDuration = Random.Range(minStormDuration, maxStormDuration);
        lightningWarningShown = false;
        lightningStrikeQueued = false;

        Debug.Log($"THUNDERSTORM STARTED! Duration: {stormDuration:F0} seconds");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Storm approaching...", new Color(0.6f, 0.6f, 0.7f));
        }

        // Start audio
        if (thunderAudioSource != null && !thunderAudioSource.isPlaying)
        {
            thunderAudioSource.Play();
        }
        if (rainAudioSource != null && !rainAudioSource.isPlaying)
        {
            rainAudioSource.Play();
        }

        // 25% chance to strike player anywhere during storm
        if (Random.value <= 0.25f)
        {
            lightningStrikeQueued = true;
            lightningStrikeTimer = Random.Range(stormDuration * 0.3f, stormDuration * 0.7f);
            Debug.Log("LIGHTNING STRIKE SCHEDULED!");
        }
    }

    void UpdateStorm()
    {
        stormElapsedTime += Time.deltaTime;

        if (stormElapsedTime >= stormDuration) { EndStorm(); return; }

        // Handle scheduled lightning strike (can happen anywhere)
        if (lightningStrikeQueued)
        {
            lightningStrikeTimer -= Time.deltaTime;

            // Show warning 2.5s before strike
            if (lightningStrikeTimer <= 2.5f && !lightningWarningShown)
            {
                lightningWarningShown = true;
                Debug.Log("Thunder rumbles nearby...");
            }

            if (lightningStrikeTimer <= 0f)
            {
                ExecuteLightningStrike();
                lightningStrikeQueued = false;
            }
        }

        // Random lightning flashes
        if (Random.value < 0.005f) { StartCoroutine(FlashLightningVisual()); }
    }

    void EndStorm()
    {
        isStormActive = false;
        targetStormIntensity = 0f;
        stormTimer = 0f;
        lightningStrikeQueued = false;
        lightningWarningShown = false;

        // Schedule next storm
        nextStormTime = Random.Range(minTimeBetweenStorms, maxTimeBetweenStorms);

        Debug.Log($"THUNDERSTORM ENDED! Next storm in {nextStormTime:F0} seconds");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Storm passing...", new Color(0.7f, 0.8f, 1f));
        }
    }

    void ExecuteLightningStrike()
    {
        Debug.Log("ZAP! Player struck by LIGHTNING!");

        // Play loud crack sound
        if (lightningCrackSource != null)
        {
            AudioClip crackClip = CreateLightningCrackClip();
            lightningCrackSource.PlayOneShot(crackClip, 1f);
        }

        // Full white screen flash
        screenFlashAlpha = 1f;

        // Kill player with custom death message
        if (PlayerHealth.Instance != null)
        {
            float currentHP = PlayerHealth.Instance.GetCurrentHealth(); float damage = currentHP * 0.5f; PlayerHealth.Instance.TakeDamage(damage); Debug.Log("Lightning dealt " + damage + " damage");
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("STRUCK BY LIGHTNING!", new Color(1f, 1f, 0.3f));
        }
    }

    IEnumerator FlashLightningVisual()
    {
        // Quick bright flash
        screenFlashAlpha = 0.3f;
        yield return new WaitForSeconds(0.05f);
        screenFlashAlpha = 0f;
        yield return new WaitForSeconds(0.05f);
        screenFlashAlpha = 0.2f;
        yield return new WaitForSeconds(0.05f);
        screenFlashAlpha = 0f;

        // Thunder sound after delay (sound travels slower than light)
        float delay = Random.Range(0.5f, 2f);
        yield return new WaitForSeconds(delay);

        // Play distant thunder crack
        if (lightningCrackSource != null)
        {
            AudioClip crackClip = CreateDistantThunderClip();
            lightningCrackSource.PlayOneShot(crackClip, 0.3f);
        }
    }

    void UpdateVisuals()
    {
        // Darken sky during storm
        skyDarkenAmount = stormIntensity;

        if (sunLight != null)
        {
            float targetIntensity = Mathf.Lerp(baseSunIntensity, stormSunIntensity, skyDarkenAmount);
            sunLight.intensity = targetIntensity;

            // Cool blue-gray light during storm
            Color sunnyColor = new Color(1f, 0.95f, 0.85f);
            Color stormyColor = new Color(0.6f, 0.65f, 0.7f);
            sunLight.color = Color.Lerp(sunnyColor, stormyColor, skyDarkenAmount);
        }

        // Update ambient lighting
        Color ambientColor = Color.Lerp(baseSkyColor, stormSkyColor, skyDarkenAmount);
        RenderSettings.ambientLight = ambientColor;
    }

    void UpdateAudio()
    {
        // Thunder rumble volume
        if (thunderAudioSource != null)
        {
            thunderAudioSource.volume = stormIntensity * 0.25f;
        }

        // Rain volume
        if (rainAudioSource != null)
        {
            rainAudioSource.volume = stormIntensity * 0.35f;
        }

        // Stop audio when storm ends
        if (stormIntensity <= 0.01f)
        {
            if (thunderAudioSource != null && thunderAudioSource.isPlaying)
            {
                thunderAudioSource.Stop();
            }
            if (rainAudioSource != null && rainAudioSource.isPlaying)
            {
                rainAudioSource.Stop();
            }
        }
    }

    bool IsPlayerOnDock()
    {
        if (!GameCache.IsPlayerValid()) return false;

        Vector3 pos = GameCache.Player.position;

        // Check main tropical dock
        bool onMainDock = pos.x > mainDockXMin && pos.x < mainDockXMax &&
                         pos.z > mainDockZMin && pos.z < mainDockZMax &&
                         pos.y > 2f && pos.y < 4f;

        // Check ice realm dock
        bool onIceDock = pos.x > iceDockXMin && pos.x < iceDockXMax &&
                        pos.z > iceDockZMin && pos.z < iceDockZMax &&
                        pos.y > 1.5f && pos.y < 4f;

        // Check jungle realm dock
        bool onJungleDock = pos.x > jungleDockXMin && pos.x < jungleDockXMax &&
                           pos.z > jungleDockZMin && pos.z < jungleDockZMax &&
                           pos.y > 2f && pos.y < 4f;

        // Check bridge (also dangerous - over water)
        bool onBridge = pos.x > bridgeXMin && pos.x < bridgeXMax &&
                       pos.z > bridgeZMin && pos.z < bridgeZMax &&
                       pos.y > 1.5f && pos.y < 3f;

        return onMainDock || onIceDock || onJungleDock || onBridge;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Screen flash (white for lightning)
        if (screenFlashAlpha > 0f)
        {
            GUI.color = new Color(1f, 1f, 1f, screenFlashAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTex);
            GUI.color = Color.white;
        }

        // Lightning warning message
        if (lightningStrikeQueued && !lightningWarningShown && lightningStrikeTimer > 0f)
        {
            lightningWarningShown = true;

            // Pulsing warning
            float pulse = 0.7f + Mathf.Sin(Time.time * 10f) * 0.3f;

            float boxWidth = 500;
            float boxHeight = 80;
            float boxX = (Screen.width - boxWidth) / 2;
            float boxY = Screen.height * 0.3f;

            // Red/yellow warning background
            GUI.color = new Color(1f, 0.3f, 0f, pulse * 0.95f);
            GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, boxHeight), whiteTex);
            GUI.color = Color.white;

            // Warning border
            GUI.color = new Color(1f, 1f, 0f, pulse);
            GUI.DrawTexture(new Rect(boxX - 3, boxY - 3, boxWidth + 6, 3), whiteTex);
            GUI.DrawTexture(new Rect(boxX - 3, boxY + boxHeight, boxWidth + 6, 3), whiteTex);
            GUI.DrawTexture(new Rect(boxX - 3, boxY, 3, boxHeight), whiteTex);
            GUI.DrawTexture(new Rect(boxX + boxWidth, boxY, 3, boxHeight), whiteTex);
            GUI.color = Color.white;

            // Warning icon
            GUIStyle iconStyle = new GUIStyle();
            iconStyle.fontSize = 36;
            iconStyle.fontStyle = FontStyle.Bold;
            iconStyle.alignment = TextAnchor.MiddleCenter;
            iconStyle.normal.textColor = new Color(1f, 1f, 0f, pulse);
            GUI.Label(new Rect(boxX, boxY + 5, 60, 60), "!", iconStyle);

            // Warning text
            GUIStyle warningStyle = new GUIStyle();
            warningStyle.fontSize = 22;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.alignment = TextAnchor.MiddleCenter;
            warningStyle.normal.textColor = Color.white;
            warningStyle.wordWrap = true;

            GUI.Label(new Rect(boxX + 60, boxY, boxWidth - 60, boxHeight),
                "Lightning strike approaching,\nget away from the water!!!", warningStyle);
        }

        // Storm indicator (top of screen)
        if (isStormActive && stormIntensity > 0.1f)
        {
            float indicatorWidth = 200;
            float indicatorX = (Screen.width - indicatorWidth) / 2;
            float indicatorY = 10;

            GUIStyle stormStyle = new GUIStyle();
            stormStyle.fontSize = 14;
            stormStyle.fontStyle = FontStyle.Bold;
            stormStyle.alignment = TextAnchor.MiddleCenter;
            stormStyle.normal.textColor = new Color(0.7f, 0.8f, 1f, stormIntensity);

            int remainingTime = Mathf.CeilToInt(stormDuration - stormElapsedTime);
            GUI.Label(new Rect(indicatorX, indicatorY, indicatorWidth, 20),
                $"THUNDERSTORM ({remainingTime}s)", stormStyle);
        }
    }

    // Procedural Audio Generation

    AudioClip CreateThunderRumbleClip()
    {
        int sampleRate = 44100;
        float duration = 4f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("ThunderRumble", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        // Generate deep rolling thunder
        float lastSample = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Multiple low frequencies for rumble
            float rumble = Mathf.Sin(t * 2 * Mathf.PI * 20f) * 0.3f;
            rumble += Mathf.Sin(t * 2 * Mathf.PI * 35f) * 0.25f;
            rumble += Mathf.Sin(t * 2 * Mathf.PI * 50f) * 0.2f;

            // Add filtered noise for texture
            float noise = Random.Range(-1f, 1f);
            lastSample = lastSample * 0.9f + noise * 0.1f;
            rumble += lastSample * 0.15f;

            // Rolling envelope
            float envelope = Mathf.Sin(progress * Mathf.PI * 2f) * 0.5f + 0.5f;

            samples[i] = rumble * envelope * 0.5f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateHeavyRainClip()
    {
        int sampleRate = 44100;
        float duration = 2f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("HeavyRain", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        // Generate heavy rain sound (white noise with specific filtering)
        float lastSample = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float noise = Random.Range(-1f, 1f);

            // Heavier low-pass filter for rain
            lastSample = lastSample * 0.75f + noise * 0.25f;

            // Add some variation
            float t = (float)i / sampleRate;
            float variation = Mathf.Sin(t * 0.3f) * 0.4f + 0.6f;

            samples[i] = lastSample * 0.2f * variation;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateLightningCrackClip()
    {
        int sampleRate = 44100;
        float duration = 1.2f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("LightningCrack", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Sharp crack at the start
            float crack = 0f;
            if (progress < 0.05f)
            {
                // Very sharp initial crack (white noise burst)
                crack = Random.Range(-1f, 1f) * (1f - progress / 0.05f);
            }
            else if (progress < 0.15f)
            {
                // Secondary crack
                float subProgress = (progress - 0.05f) / 0.1f;
                crack = Random.Range(-1f, 1f) * (1f - subProgress) * 0.5f;
            }

            // Rolling thunder after crack
            float rumble = 0f;
            if (progress > 0.1f)
            {
                float rumbleProgress = (progress - 0.1f) / 0.9f;
                rumble = Mathf.Sin(t * 2 * Mathf.PI * 30f) * (1f - rumbleProgress) * 0.4f;
                rumble += Mathf.Sin(t * 2 * Mathf.PI * 50f) * (1f - rumbleProgress) * 0.3f;
                rumble += Mathf.Sin(t * 2 * Mathf.PI * 80f) * (1f - rumbleProgress) * 0.2f;
            }

            samples[i] = Mathf.Clamp(crack + rumble, -0.95f, 0.95f);
        }

        // Smooth the sharp edges slightly
        for (int i = 1; i < sampleCount - 1; i++)
        {
            samples[i] = samples[i] * 0.8f + samples[i - 1] * 0.1f + samples[i + 1] * 0.1f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateDistantThunderClip()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("DistantThunder", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        // Softer, more filtered thunder for distant strikes
        float lastSample = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Low rumble
            float rumble = Mathf.Sin(t * 2 * Mathf.PI * 25f) * 0.3f;
            rumble += Mathf.Sin(t * 2 * Mathf.PI * 40f) * 0.2f;

            // Filtered noise
            float noise = Random.Range(-1f, 1f);
            lastSample = lastSample * 0.85f + noise * 0.15f;
            rumble += lastSample * 0.1f;

            // Envelope
            float envelope = Mathf.Sin(progress * Mathf.PI);

            samples[i] = rumble * envelope * 0.3f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    void OnDestroy()
    {
        if (whiteTex != null) Destroy(whiteTex);
        if (warningTex != null) Destroy(warningTex);
    }

    // Public getters
    public bool IsStormActive() => isStormActive;
    public float GetStormIntensity() => stormIntensity;
}

