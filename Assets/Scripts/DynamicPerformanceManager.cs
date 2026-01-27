using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Dynamic Performance Manager - Monitors FPS and automatically adjusts quality settings
/// to maintain smooth gameplay. Especially important for WebGL where hardware varies widely.
/// </summary>
public class DynamicPerformanceManager : MonoBehaviour
{
    public static DynamicPerformanceManager Instance { get; private set; }

    [Header("FPS Targets")]
    [Tooltip("Target FPS for good performance")]
    public int targetFPS = 30;

    [Tooltip("Minimum acceptable FPS before reducing quality")]
    public int minAcceptableFPS = 20;

    [Tooltip("FPS threshold for restoring quality")]
    public int restoreQualityFPS = 40;

    [Header("Monitoring")]
    [Tooltip("How often to check FPS (seconds)")]
    public float checkInterval = 2f;

    [Tooltip("Number of samples to average")]
    public int sampleCount = 30;

    [Header("Debug")]
    public bool showFPSCounter = false; // DISABLED - no longer needed
    public bool showDetailedStats = false; // Toggle with Shift+D for detailed view

    // FPS tracking
    private float[] fpsSamples;
    private int sampleIndex = 0;
    private float lastCheckTime = 0f;
    private float currentFPS = 60f;
    private float averageFPS = 60f;

    // Quality state
    private int currentQualityLevel = 2; // 0=Low, 1=Medium, 2=High
    private bool crittersDisbaled = false;
    private bool particlesReduced = false;
    private bool shadowsDisabled = false;

    // Performance mode flags
    private bool isWebGL = false;
    private bool isLowEndDevice = false;

    // Cached references
    private BeachCritters beachCrittersRef;
    private VolumetricLighting volumetricRef;
    private GoldParticleSystem goldParticlesRef;
    private WaterEffect waterEffectRef;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        #if UNITY_WEBGL
        isWebGL = true;
        #endif

        fpsSamples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            fpsSamples[i] = 60f;
        }
    }

    void Start()
    {
        // Apply initial WebGL optimizations
        if (isWebGL)
        {
            ApplyWebGLOptimizations();
        }

        // Cache references (will be null until objects exist)
        CacheReferences();
    }

    void CacheReferences()
    {
        beachCrittersRef = BeachCritters.Instance;
        volumetricRef = VolumetricLighting.Instance;
        goldParticlesRef = GoldParticleSystem.Instance;
        waterEffectRef = WaterEffect.Instance;
    }

    void Update()
    {
        // DISABLED - FPS overlays removed
        // Toggle FPS counter with Shift+F
        // if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.F))
        // {
        //     showFPSCounter = !showFPSCounter;
        //     Debug.Log($"FPS Counter: {(showFPSCounter ? "ON" : "OFF")}");
        // }

        // Toggle detailed stats with Shift+D
        // if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.D))
        // {
        //     showDetailedStats = !showDetailedStats;
        //     Debug.Log($"Detailed Stats: {(showDetailedStats ? "ON" : "OFF")}");
        // }

        // Sample FPS
        currentFPS = 1f / Time.unscaledDeltaTime;
        fpsSamples[sampleIndex] = currentFPS;
        sampleIndex = (sampleIndex + 1) % sampleCount;

        // Periodic check
        if (Time.unscaledTime - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.unscaledTime;
            EvaluatePerformance();
        }
    }

    void EvaluatePerformance()
    {
        // Calculate average FPS
        float sum = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            sum += fpsSamples[i];
        }
        averageFPS = sum / sampleCount;

        // Cache references if not yet cached
        if (beachCrittersRef == null) CacheReferences();

        // Check if we need to reduce quality
        if (averageFPS < minAcceptableFPS)
        {
            ReduceQuality();
        }
        // Check if we can restore quality
        else if (averageFPS > restoreQualityFPS && currentQualityLevel < 2)
        {
            RestoreQuality();
        }
    }

    void ReduceQuality()
    {
        if (currentQualityLevel <= 0)
        {
            // Already at minimum quality
            return;
        }

        currentQualityLevel--;
        Debug.Log($"[Performance] Reducing quality to level {currentQualityLevel} (FPS: {averageFPS:F1})");

        switch (currentQualityLevel)
        {
            case 1: // Medium - disable some effects
                DisableNonEssentialEffects();
                break;
            case 0: // Low - disable more effects
                ApplyLowQualitySettings();
                break;
        }
    }

    void RestoreQuality()
    {
        if (currentQualityLevel >= 2)
        {
            return;
        }

        currentQualityLevel++;
        Debug.Log($"[Performance] Restoring quality to level {currentQualityLevel} (FPS: {averageFPS:F1})");

        switch (currentQualityLevel)
        {
            case 1: // Restore to medium
                RestoreMediumQuality();
                break;
            case 2: // Restore to high
                RestoreHighQuality();
                break;
        }
    }

    void DisableNonEssentialEffects()
    {
        // Disable volumetric lighting
        if (volumetricRef != null && volumetricRef.enabled)
        {
            volumetricRef.enabled = false;
            Debug.Log("[Performance] Disabled volumetric lighting");
        }

        // Reduce water particles
        if (waterEffectRef != null)
        {
            particlesReduced = true;
        }
    }

    void ApplyLowQualitySettings()
    {
        // Disable shadows
        if (!shadowsDisabled)
        {
            QualitySettings.shadows = ShadowQuality.Disable;
            shadowsDisabled = true;
            Debug.Log("[Performance] Disabled shadows");
        }

        // Disable beach critters
        if (beachCrittersRef != null && beachCrittersRef.enabled)
        {
            beachCrittersRef.enabled = false;
            crittersDisbaled = true;
            Debug.Log("[Performance] Disabled beach critters");
        }

        // Disable gold particles
        if (goldParticlesRef != null && goldParticlesRef.enabled)
        {
            goldParticlesRef.enabled = false;
            Debug.Log("[Performance] Disabled gold particles");
        }

        // Lower texture quality
        QualitySettings.globalTextureMipmapLimit = 1;

        // Disable anti-aliasing
        QualitySettings.antiAliasing = 0;
    }

    void RestoreMediumQuality()
    {
        // Re-enable beach critters
        if (crittersDisbaled && beachCrittersRef != null)
        {
            beachCrittersRef.enabled = true;
            crittersDisbaled = false;
        }

        // Re-enable gold particles
        if (goldParticlesRef != null)
        {
            goldParticlesRef.enabled = true;
        }
    }

    void RestoreHighQuality()
    {
        // Re-enable volumetric lighting (non-WebGL only)
        if (!isWebGL && volumetricRef != null)
        {
            volumetricRef.enabled = true;
        }

        // Restore shadows (non-WebGL only)
        if (!isWebGL && shadowsDisabled)
        {
            QualitySettings.shadows = ShadowQuality.All;
            shadowsDisabled = false;
        }

        // Restore texture quality
        QualitySettings.globalTextureMipmapLimit = 0;

        particlesReduced = false;
    }

    void ApplyWebGLOptimizations()
    {
        Debug.Log("[Performance] Applying WebGL optimizations");

        // Disable shadows for WebGL
        QualitySettings.shadows = ShadowQuality.Disable;
        shadowsDisabled = true;

        // Disable VSync for WebGL (browser controls this)
        QualitySettings.vSyncCount = 0;

        // Set target frame rate
        Application.targetFrameRate = 30;

        // Reduce texture quality slightly
        QualitySettings.globalTextureMipmapLimit = 1;

        // Disable anti-aliasing
        QualitySettings.antiAliasing = 0;

        // Lower LOD bias
        QualitySettings.lodBias = 0.7f;

        // Reduce pixel light count
        QualitySettings.pixelLightCount = 1;

        // Disable soft particles
        QualitySettings.softParticles = false;

        // Set lower quality level
        currentQualityLevel = 1;
    }

    void OnGUI()
    {
        if (!showFPSCounter || !MainMenu.GameStarted) return;

        // Performance: Skip frames
        if (Time.frameCount % 10 != 0) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        // Color based on FPS
        if (averageFPS >= targetFPS)
            style.normal.textColor = Color.green;
        else if (averageFPS >= minAcceptableFPS)
            style.normal.textColor = Color.yellow;
        else
            style.normal.textColor = Color.red;

        string qualityName = currentQualityLevel switch
        {
            0 => "LOW",
            1 => "MED",
            _ => "HIGH"
        };

        // Centered at top middle of screen
        GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 25), $"FPS: {averageFPS:F0} | Quality: {qualityName}", style);

        // Toggle hint
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 10;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(Screen.width / 2 - 80, 28, 160, 15), "[Shift+F hide] [Shift+D details]", hintStyle);

        // Detailed stats panel
        if (showDetailedStats)
        {
            DrawDetailedStats();
        }
    }

    void DrawDetailedStats()
    {
        float panelX = 10;
        float panelY = 50;
        float panelWidth = 280;
        float panelHeight = 200;

        // Background
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(1f, 0.8f, 0.3f);
        GUI.Label(new Rect(panelX + 10, panelY + 5, panelWidth, 20), "PERFORMANCE DIAGNOSTICS", titleStyle);

        GUIStyle statStyle = new GUIStyle();
        statStyle.fontSize = 12;
        statStyle.normal.textColor = Color.white;

        float y = panelY + 30;
        float lineHeight = 18;

        // FPS Stats
        GUI.Label(new Rect(panelX + 10, y, panelWidth, lineHeight), $"Current FPS: {currentFPS:F1}", statStyle);
        y += lineHeight;
        GUI.Label(new Rect(panelX + 10, y, panelWidth, lineHeight), $"Average FPS: {averageFPS:F1}", statStyle);
        y += lineHeight;

        // Find min/max from samples
        float minFPS = float.MaxValue, maxFPS = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            if (fpsSamples[i] < minFPS) minFPS = fpsSamples[i];
            if (fpsSamples[i] > maxFPS) maxFPS = fpsSamples[i];
        }
        GUI.Label(new Rect(panelX + 10, y, panelWidth, lineHeight), $"Min/Max: {minFPS:F0} / {maxFPS:F0}", statStyle);
        y += lineHeight + 5;

        // Quality Settings
        statStyle.normal.textColor = new Color(0.7f, 0.9f, 1f);
        GUI.Label(new Rect(panelX + 10, y, panelWidth, lineHeight), $"Quality Level: {currentQualityLevel} ({(currentQualityLevel == 0 ? "Low" : currentQualityLevel == 1 ? "Med" : "High")})", statStyle);
        y += lineHeight;
        GUI.Label(new Rect(panelX + 10, y, panelWidth, lineHeight), $"Shadows: {(shadowsDisabled ? "OFF" : "ON")}", statStyle);
        y += lineHeight;
        GUI.Label(new Rect(panelX + 10, y, panelWidth, lineHeight), $"Platform: {(isWebGL ? "WebGL" : "Desktop")}", statStyle);
        y += lineHeight + 5;

        // Tell me what to report
        statStyle.fontSize = 10;
        statStyle.fontStyle = FontStyle.Italic;
        statStyle.normal.textColor = new Color(0.8f, 0.8f, 0.5f);
        GUI.Label(new Rect(panelX + 10, y, panelWidth - 20, 40), "Tell Claude: your FPS, where drops happen, and what you're doing when it lags!", statStyle);
    }

    // Public methods for external control
    public float GetAverageFPS() => averageFPS;
    public int GetQualityLevel() => currentQualityLevel;
    public bool IsWebGL() => isWebGL;

    public void ForceQualityLevel(int level)
    {
        level = Mathf.Clamp(level, 0, 2);
        if (level < currentQualityLevel)
        {
            while (currentQualityLevel > level) ReduceQuality();
        }
        else if (level > currentQualityLevel)
        {
            while (currentQualityLevel < level) RestoreQuality();
        }
    }
}
