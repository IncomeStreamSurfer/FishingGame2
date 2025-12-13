using UnityEngine;

/// <summary>
/// Performance configuration script - sets up optimal settings at game start
/// Attach this to a persistent game object or add to your main scene
/// This fixes VSync, frame rate, and quality issues
/// </summary>
public class PerformanceConfig : MonoBehaviour
{
    public static PerformanceConfig Instance { get; private set; }

    [Header("Performance Settings")]
    [Tooltip("Target frame rate (0 = unlimited)")]
    public int targetFrameRate = 60;

    [Tooltip("Enable VSync (0=off, 1=on, 2=every second frame)")]
    public int vSyncCount = 0;

    [Header("Quality Settings")]
    [Tooltip("Quality level index (0=lowest, higher=better)")]
    public int qualityLevel = 2;

    [Tooltip("Texture quality (0=full, 1=half, 2=quarter, 3=eighth)")]
    public int textureQuality = 0;

    [Tooltip("Anti-aliasing (0=off, 2, 4, or 8)")]
    public int antiAliasing = 2;

    [Header("Shadow Settings")]
    [Tooltip("Shadow distance (lower = better performance)")]
    public float shadowDistance = 50f;

    [Tooltip("Shadow resolution (0=low, 1=medium, 2=high, 3=very high)")]
    public int shadowResolution = 1;

    [Header("LOD Settings")]
    [Tooltip("LOD bias (higher = more detail at distance)")]
    public float lodBias = 1f;

    [Tooltip("Maximum LOD level (0 = highest detail)")]
    public int maximumLODLevel = 0;

    [Header("Physics Settings")]
    [Tooltip("Fixed timestep for physics (lower = more accurate but slower)")]
    public float fixedTimeStep = 0.02f;

    [Tooltip("Maximum allowed timestep")]
    public float maximumAllowedTimestep = 0.1f;

    private bool initialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyPerformanceSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Apply again in Start to ensure settings stick after scene load
        if (!initialized)
        {
            ApplyPerformanceSettings();
            initialized = true;
        }
    }

    public void ApplyPerformanceSettings()
    {
        // Frame Rate
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = vSyncCount;

        // Quality Level
        if (qualityLevel >= 0 && qualityLevel < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(qualityLevel, true);
        }

        // Texture Quality
        QualitySettings.globalTextureMipmapLimit = textureQuality;

        // Anti-Aliasing
        QualitySettings.antiAliasing = antiAliasing;

        // Shadows
        QualitySettings.shadowDistance = shadowDistance;
        QualitySettings.shadowResolution = (ShadowResolution)shadowResolution;

        // LOD
        QualitySettings.lodBias = lodBias;
        QualitySettings.maximumLODLevel = maximumLODLevel;

        // Physics
        Time.fixedDeltaTime = fixedTimeStep;
        Time.maximumDeltaTime = maximumAllowedTimestep;

        // Optimize rendering
        QualitySettings.softParticles = false;  // Disable soft particles for performance
        QualitySettings.realtimeReflectionProbes = false;  // Disable real-time reflections
        QualitySettings.billboardsFaceCameraPosition = false;  // Cheaper billboard rendering

        // Optimize skin weights for characters
        QualitySettings.skinWeights = SkinWeights.TwoBones;

        Debug.Log($"PerformanceConfig: Applied settings - {targetFrameRate}fps, VSync={vSyncCount}, Quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}");
    }

    /// <summary>
    /// Apply high performance preset (for weaker hardware)
    /// </summary>
    public void ApplyHighPerformancePreset()
    {
        targetFrameRate = 60;
        vSyncCount = 0;
        qualityLevel = 1;
        textureQuality = 1;
        antiAliasing = 0;
        shadowDistance = 30f;
        shadowResolution = 0;
        lodBias = 0.5f;
        maximumLODLevel = 1;
        fixedTimeStep = 0.025f;
        maximumAllowedTimestep = 0.15f;

        ApplyPerformanceSettings();
        Debug.Log("PerformanceConfig: High Performance preset applied");
    }

    /// <summary>
    /// Apply balanced preset (for most hardware)
    /// </summary>
    public void ApplyBalancedPreset()
    {
        targetFrameRate = 60;
        vSyncCount = 0;
        qualityLevel = 2;
        textureQuality = 0;
        antiAliasing = 2;
        shadowDistance = 50f;
        shadowResolution = 1;
        lodBias = 1f;
        maximumLODLevel = 0;
        fixedTimeStep = 0.02f;
        maximumAllowedTimestep = 0.1f;

        ApplyPerformanceSettings();
        Debug.Log("PerformanceConfig: Balanced preset applied");
    }

    /// <summary>
    /// Apply quality preset (for powerful hardware)
    /// </summary>
    public void ApplyQualityPreset()
    {
        targetFrameRate = 120;
        vSyncCount = 0;
        qualityLevel = 4;
        textureQuality = 0;
        antiAliasing = 4;
        shadowDistance = 100f;
        shadowResolution = 2;
        lodBias = 1.5f;
        maximumLODLevel = 0;
        fixedTimeStep = 0.0166f;
        maximumAllowedTimestep = 0.08f;

        ApplyPerformanceSettings();
        Debug.Log("PerformanceConfig: Quality preset applied");
    }

    /// <summary>
    /// Emergency performance mode - maximum FPS at cost of visuals
    /// </summary>
    public void ApplyEmergencyPerformanceMode()
    {
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
        QualitySettings.SetQualityLevel(0, true);
        QualitySettings.globalTextureMipmapLimit = 2;
        QualitySettings.antiAliasing = 0;
        QualitySettings.shadowDistance = 0;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.skinWeights = SkinWeights.OneBone;
        Time.fixedDeltaTime = 0.04f;

        Debug.Log("PerformanceConfig: EMERGENCY MODE - Shadows disabled, minimum quality");
    }

    // Expose settings to DevPanel
    public static void SetTargetFPS(int fps)
    {
        Application.targetFrameRate = fps;
        if (Instance != null)
            Instance.targetFrameRate = fps;
    }

    public static void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        if (Instance != null)
            Instance.vSyncCount = enabled ? 1 : 0;
    }

    public static void SetShadowsEnabled(bool enabled)
    {
        QualitySettings.shadows = enabled ? ShadowQuality.All : ShadowQuality.Disable;
    }
}
