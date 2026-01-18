using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Weather System - Occasional rain effects with lightning storms
/// Scene is mainly sunny with occasional rain showers
/// Lightning can strike during storms - deadly to weakened players!
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    // Weather state
    private bool isRaining = false;
    private float rainIntensity = 0f;
    private float targetRainIntensity = 0f;

    // Timing
    private float weatherTimer = 0f;
    private float nextWeatherChange = 60f; // Start with sun for 60 seconds
    private float minSunDuration = 90f;    // Minimum sunny period
    private float maxSunDuration = 180f;   // Maximum sunny period
    private float minRainDuration = 20f;   // Minimum rain period
    private float maxRainDuration = 45f;   // Maximum rain period

    // Rain particles
    private List<RainDrop> rainDrops = new List<RainDrop>();
    private int maxRainDrops = 200;

    // Lighting
    private Light sunLight;
    private float baseSunIntensity = 1.3f;
    private float rainSunIntensity = 0.6f;
    private Color baseSkyColor = new Color(0.5f, 0.7f, 1f);
    private Color rainSkyColor = new Color(0.4f, 0.45f, 0.5f);

    // Lightning system
    private float lightningTimer = 0f;
    private float nextLightningStrike = 10f;
    private float minLightningInterval = 8f;   // Minimum time between strikes
    private float maxLightningInterval = 25f;  // Maximum time between strikes
    private float lightningFlashDuration = 0f;
    private float lightningFlashIntensity = 0f;
    private Vector3 lastLightningPosition = Vector3.zero;
    private bool lightningActive = false;
    private float lightningDamageRadius = 5f;  // How close player must be to get struck
    private float lightningKillThreshold = 0.05f; // 5% health = instant death from lightning

    // Audio
    private AudioSource rainAudio;
    private float rainVolume = 0.15f;

    // Cached texture for rain UI
    private Texture2D rainTex;

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

        // Setup rain audio
        SetupRainAudio();

        // Create rain texture
        rainTex = new Texture2D(2, 2);
        rainTex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        rainTex.Apply();

        // Start with sunny weather, first rain after 60-120 seconds
        nextWeatherChange = Random.Range(60f, 120f);
    }

    void SetupRainAudio()
    {
        rainAudio = gameObject.AddComponent<AudioSource>();
        rainAudio.loop = true;
        rainAudio.spatialBlend = 0f;
        rainAudio.volume = 0f;
        rainAudio.playOnAwake = false;

        // Create rain sound (white noise with filtering)
        int sampleRate = 44100;
        float duration = 2f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip rainClip = AudioClip.Create("RainSound", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        // Generate filtered noise for rain sound
        float lastSample = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float noise = Random.Range(-1f, 1f);
            // Low-pass filter for softer rain sound
            lastSample = lastSample * 0.7f + noise * 0.3f;

            // Add some variation
            float t = (float)i / sampleRate;
            float variation = Mathf.Sin(t * 0.5f) * 0.3f + 0.7f;

            samples[i] = lastSample * 0.15f * variation;
        }

        rainClip.SetData(samples, 0);
        rainAudio.clip = rainClip;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Update weather timer
        weatherTimer += Time.deltaTime;

        if (weatherTimer >= nextWeatherChange)
        {
            weatherTimer = 0f;
            ToggleWeather();
        }

        // Smoothly transition rain intensity
        rainIntensity = Mathf.MoveTowards(rainIntensity, targetRainIntensity, Time.deltaTime * 0.5f);

        // Update lighting based on rain
        UpdateLighting();

        // Update rain particles
        if (rainIntensity > 0.01f)
        {
            UpdateRainParticles();
        }

        // Update lightning during storms
        if (isRaining && rainIntensity > 0.5f)
        {
            UpdateLightning();
        }

        // Update lightning flash effect
        if (lightningFlashDuration > 0f)
        {
            lightningFlashDuration -= Time.deltaTime;
            lightningFlashIntensity = Mathf.Lerp(0f, 2f, lightningFlashDuration / 0.15f);
        }
        else
        {
            lightningFlashIntensity = 0f;
            lightningActive = false;
        }

        // Update rain audio
        if (rainAudio != null)
        {
            rainAudio.volume = rainIntensity * rainVolume;
            if (rainIntensity > 0.01f && !rainAudio.isPlaying)
            {
                rainAudio.Play();
            }
            else if (rainIntensity <= 0.01f && rainAudio.isPlaying)
            {
                rainAudio.Stop();
            }
        }
    }

    void ToggleWeather()
    {
        if (isRaining)
        {
            // Stop raining, go sunny
            isRaining = false;
            targetRainIntensity = 0f;
            nextWeatherChange = Random.Range(minSunDuration, maxSunDuration);
            Debug.Log($"Weather: Sunny for {nextWeatherChange:F0} seconds");
        }
        else
        {
            // Start raining
            isRaining = true;
            targetRainIntensity = Random.Range(0.5f, 1f); // Variable rain intensity
            nextWeatherChange = Random.Range(minRainDuration, maxRainDuration);
            Debug.Log($"Weather: Raining (intensity {targetRainIntensity:F1}) for {nextWeatherChange:F0} seconds");
        }
    }

    void UpdateLighting()
    {
        if (sunLight != null)
        {
            // Dim sun during rain
            float targetIntensity = Mathf.Lerp(baseSunIntensity, rainSunIntensity, rainIntensity);
            sunLight.intensity = targetIntensity;

            // Slightly cooler light color during rain
            Color sunnyColor = new Color(1f, 0.95f, 0.85f);
            Color rainyColor = new Color(0.8f, 0.82f, 0.85f);
            sunLight.color = Color.Lerp(sunnyColor, rainyColor, rainIntensity);
        }

        // Update ambient lighting
        Color ambientColor = Color.Lerp(baseSkyColor, rainSkyColor, rainIntensity);
        RenderSettings.ambientLight = ambientColor;
    }

    void UpdateRainParticles()
    {
        // Get camera/player position for rain spawning
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;

        // Spawn new rain drops
        int targetDrops = (int)(maxRainDrops * rainIntensity);
        while (rainDrops.Count < targetDrops)
        {
            RainDrop drop = new RainDrop();
            drop.position = new Vector3(
                camPos.x + Random.Range(-25f, 25f),
                camPos.y + Random.Range(10f, 20f),
                camPos.z + Random.Range(-25f, 25f)
            );
            drop.velocity = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-15f, -20f),
                Random.Range(-1f, 1f)
            );
            drop.length = Random.Range(0.3f, 0.6f);
            drop.alpha = Random.Range(0.3f, 0.6f);
            rainDrops.Add(drop);
        }

        // Update existing drops
        for (int i = rainDrops.Count - 1; i >= 0; i--)
        {
            RainDrop drop = rainDrops[i];
            drop.position += drop.velocity * Time.deltaTime;

            // Remove drops that are too low or too far from camera
            if (drop.position.y < -2f ||
                Mathf.Abs(drop.position.x - camPos.x) > 30f ||
                Mathf.Abs(drop.position.z - camPos.z) > 30f)
            {
                rainDrops.RemoveAt(i);
            }
        }
    }

    void UpdateLightning()
    {
        lightningTimer += Time.deltaTime;

        if (lightningTimer >= nextLightningStrike)
        {
            lightningTimer = 0f;
            nextLightningStrike = Random.Range(minLightningInterval, maxLightningInterval);

            // More frequent lightning during intense storms
            if (rainIntensity > 0.8f)
            {
                nextLightningStrike *= 0.6f;
            }

            TriggerLightningStrike();
        }
    }

    void TriggerLightningStrike()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Determine strike location - random position around camera/player
        Vector3 camPos = cam.transform.position;
        lastLightningPosition = new Vector3(
            camPos.x + Random.Range(-30f, 30f),
            20f, // Strike from sky
            camPos.z + Random.Range(-30f, 30f)
        );

        // Trigger visual flash
        lightningFlashDuration = 0.15f;
        lightningFlashIntensity = 2f;
        lightningActive = true;

        // Play thunder sound (delayed based on distance)
        PlayThunderSound();

        // Check if player should be affected
        CheckLightningDamage();

        Debug.Log($"Lightning strike at {lastLightningPosition}!");
    }

    void PlayThunderSound()
    {
        // Create temporary audio source for thunder
        GameObject thunderObj = new GameObject("Thunder");
        AudioSource thunderAudio = thunderObj.AddComponent<AudioSource>();
        thunderAudio.spatialBlend = 0f;
        thunderAudio.volume = 0.4f * rainIntensity;

        // Generate procedural thunder sound
        int sampleRate = 44100;
        float duration = 1.5f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip thunderClip = AudioClip.Create("ThunderSound", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        // Thunder sound: low rumble with initial crack
        float lastSample = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float noise = Random.Range(-1f, 1f);

            // Heavy low-pass filter for deep rumble
            lastSample = lastSample * 0.95f + noise * 0.05f;

            // Initial crack (first 0.1 seconds)
            float crack = t < 0.1f ? Mathf.Sin(t * 100f) * (1f - t * 10f) * 0.5f : 0f;

            // Envelope - loud start, gradual fade
            float envelope = Mathf.Pow(1f - t, 2f);

            // Add some rumble variation
            float rumble = Mathf.Sin(t * 15f + Mathf.Sin(t * 7f) * 3f) * 0.3f;

            samples[i] = (lastSample + crack + rumble * envelope) * envelope * 0.4f;
        }

        thunderClip.SetData(samples, 0);
        thunderAudio.clip = thunderClip;
        thunderAudio.Play();

        // Destroy after playing
        Destroy(thunderObj, duration + 0.5f);
    }

    void CheckLightningDamage()
    {
        // Check if player is close to lightning strike
        if (!GameCache.IsPlayerValid()) return;

        Vector3 playerPos = GameCache.Player.position;
        float distanceToStrike = Vector3.Distance(
            new Vector3(playerPos.x, 0, playerPos.z),
            new Vector3(lastLightningPosition.x, 0, lastLightningPosition.z)
        );

        // If player is within damage radius
        if (distanceToStrike <= lightningDamageRadius)
        {
            if (PlayerHealth.Instance != null)
            {
                float healthPercent = PlayerHealth.Instance.GetHealthPercent();

                // If player is under 5% health, lightning kills them instantly
                if (healthPercent < lightningKillThreshold)
                {
                    // Set achievement flag for "Storm's Victim" death
                    PlayerPrefs.SetInt("Death_LightningStrike", PlayerPrefs.GetInt("Death_LightningStrike", 0) + 1);
                    PlayerPrefs.Save();

                    // Deal lethal damage with custom death message
                    PlayerHealth.Instance.TakeDamage(
                        PlayerHealth.Instance.GetCurrentHealth() + 10f,
                        "The storm claimed another soul... You were struck by lightning while weakened."
                    );

                    Debug.Log("Player killed by lightning strike while under 5% health!");
                }
                else
                {
                    // Player was struck but survived - deal some damage
                    float damage = Random.Range(5f, 15f);
                    PlayerHealth.Instance.TakeDamage(damage, "");

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowLootNotification("Lightning struck nearby!", new Color(1f, 1f, 0.3f));
                    }

                    Debug.Log($"Player struck by lightning for {damage} damage!");
                }
            }
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Draw lightning flash effect even when not raining (flash persists briefly)
        if (lightningFlashIntensity > 0f)
        {
            DrawLightningFlash();
        }

        if (rainIntensity <= 0.01f) return;
        if (rainTex == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Draw rain drops as screen-space lines
        GUI.color = new Color(0.7f, 0.75f, 0.85f, rainIntensity * 0.4f);

        foreach (RainDrop drop in rainDrops)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(drop.position);

            // Skip if behind camera
            if (screenPos.z < 0) continue;

            // Convert to GUI coordinates (flip Y)
            float x = screenPos.x;
            float y = Screen.height - screenPos.y;

            // Draw rain streak
            float streakLength = drop.length * 20f * (1f / Mathf.Max(0.1f, screenPos.z * 0.1f));

            GUI.color = new Color(0.8f, 0.85f, 0.95f, drop.alpha * rainIntensity);

            // Simple vertical line for rain
            GUI.DrawTexture(new Rect(x, y, 1, streakLength), rainTex);
        }

        GUI.color = Color.white;

        // Optional: slight screen overlay for atmosphere
        if (rainIntensity > 0.3f)
        {
            GUI.color = new Color(0.5f, 0.55f, 0.6f, rainIntensity * 0.1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), rainTex);
            GUI.color = Color.white;
        }
    }

    void DrawLightningFlash()
    {
        // Bright white flash across the screen
        float alpha = lightningFlashIntensity * 0.5f;
        GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), rainTex != null ? rainTex : Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Draw lightning bolt visual (simple branching effect)
        if (lightningActive && rainTex != null)
        {
            DrawLightningBolt();
        }
    }

    void DrawLightningBolt()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Convert strike position to screen coordinates
        Vector3 screenPos = cam.WorldToScreenPoint(lastLightningPosition);
        if (screenPos.z < 0) return;

        // Draw from top of screen to strike position
        float startX = screenPos.x + Random.Range(-20f, 20f);
        float startY = 0; // Top of screen
        float endX = screenPos.x;
        float endY = Screen.height - screenPos.y;

        // Draw main bolt
        GUI.color = new Color(0.9f, 0.95f, 1f, lightningFlashIntensity);

        int segments = 8;
        float lastX = startX;
        float lastY = startY;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float x = Mathf.Lerp(startX, endX, t) + Random.Range(-15f, 15f);
            float y = Mathf.Lerp(startY, endY, t);

            // Draw segment as rectangle
            float segWidth = 3f * (1f - t * 0.5f);
            float dx = x - lastX;
            float dy = y - lastY;
            float length = Mathf.Sqrt(dx * dx + dy * dy);

            // Simple rectangle approximation for lightning
            GUI.DrawTexture(new Rect(Mathf.Min(lastX, x), Mathf.Min(lastY, y), Mathf.Abs(dx) + segWidth, Mathf.Abs(dy) + 2), rainTex);

            // Branch occasionally
            if (Random.value < 0.3f && i > 2)
            {
                float branchX = x + Random.Range(-40f, 40f);
                float branchY = y + Random.Range(20f, 60f);
                GUI.DrawTexture(new Rect(Mathf.Min(x, branchX), y, Mathf.Abs(branchX - x) + 2, Mathf.Abs(branchY - y) + 2), rainTex);
            }

            lastX = x;
            lastY = y;
        }

        GUI.color = Color.white;
    }

    void OnDestroy()
    {
        if (rainTex != null)
        {
            Destroy(rainTex);
        }
    }

    // Public methods for other systems
    public bool IsRaining() => isRaining;
    public float GetRainIntensity() => rainIntensity;
    public bool IsLightningActive() => lightningActive;
    public bool IsStorming() => isRaining && rainIntensity > 0.5f;
}

public class RainDrop
{
    public Vector3 position;
    public Vector3 velocity;
    public float length;
    public float alpha;
}
