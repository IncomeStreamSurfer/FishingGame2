using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Weather System - Occasional rain effects
/// Scene is mainly sunny with occasional rain showers
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

    // Audio
    private AudioSource rainAudio;
    private float rainVolume = 0.3f;

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

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
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
}

public class RainDrop
{
    public Vector3 position;
    public Vector3 velocity;
    public float length;
    public float alpha;
}
