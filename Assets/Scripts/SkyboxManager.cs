using UnityEngine;

/// <summary>
/// Skybox Manager for Unity's Built-in Render Pipeline
/// Supports 6-sided cubemap skyboxes with day/night cycle blending
///
/// SETUP INSTRUCTIONS:
/// 1. Import 6 skybox textures (front, back, left, right, up, down)
/// 2. Set each texture's Wrap Mode to "Clamp" in the Inspector
/// 3. Create a skybox material: Right-click in Project > Create > Material
/// 4. Set the material's shader to "Skybox/6 Sided"
/// 5. Assign your 6 textures to the material slots
/// 6. Drag the material to the appropriate slot on this component
///
/// For day/night blending, create separate skybox materials for each time of day
/// </summary>
public class SkyboxManager : MonoBehaviour
{
    public static SkyboxManager Instance { get; private set; }

    [Header("Skybox Materials")]
    [Tooltip("Skybox for daytime (bright blue sky with clouds)")]
    public Material daySkybox;

    [Tooltip("Skybox for sunset/sunrise (orange/pink hues)")]
    public Material sunsetSkybox;

    [Tooltip("Skybox for nighttime (dark with stars)")]
    public Material nightSkybox;

    [Header("Procedural Skybox Settings (if not using textures)")]
    [Tooltip("Use Unity's procedural skybox shader instead of 6-sided")]
    public bool useProceduralSkybox = true;

    [Header("Procedural Sky Colors - Vibrant & Reduced Exposure")]
    public Color dayTopColor = new Color(0.25f, 0.55f, 0.90f);      // Rich blue sky top
    public Color dayHorizonColor = new Color(0.55f, 0.72f, 0.88f);  // Softer horizon (less white)
    public Color sunsetTopColor = new Color(0.25f, 0.22f, 0.50f);   // Purple-blue sunset top
    public Color sunsetHorizonColor = new Color(0.95f, 0.45f, 0.18f); // Vibrant orange horizon
    public Color nightTopColor = new Color(0.015f, 0.015f, 0.06f);  // Deep dark blue
    public Color nightHorizonColor = new Color(0.04f, 0.04f, 0.12f);

    [Header("Sun Settings (for procedural skybox)")]
    [Range(0f, 1f)]
    public float sunSize = 0.03f;      // Smaller sun
    [Range(0f, 1f)]
    public float sunConvergence = 5f;
    public float atmosphereThickness = 0.6f;  // Lower atmosphere
    public float exposure = 0.5f;             // DRASTICALLY REDUCED - fix overexposure

    [Header("Integration")]
    [Tooltip("Link to DayNightCycle for automatic time-based skybox changes")]
    public DayNightCycle dayNightCycle;

    [Tooltip("Disable the procedural sky dome in DayNightCycle when using skybox")]
    public bool disableProceduralSkyDome = true;

    // Private
    private Material proceduralSkyboxMaterial;
    private Material blendedSkyboxMaterial;
    private bool isInitialized = false;

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
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized) return;

        // Find DayNightCycle if not assigned
        if (dayNightCycle == null)
        {
            dayNightCycle = FindObjectOfType<DayNightCycle>();
        }

        if (useProceduralSkybox)
        {
            SetupProceduralSkybox();
        }
        else if (daySkybox != null)
        {
            // Use the provided 6-sided skybox
            RenderSettings.skybox = daySkybox;
        }

        // Disable the old sky dome if requested
        if (disableProceduralSkyDome && dayNightCycle != null)
        {
            DisableOldSkyDome();
        }

        isInitialized = true;
        Debug.Log("SkyboxManager initialized - Skybox system active");
    }

    void SetupProceduralSkybox()
    {
        // Try to find the procedural skybox shader
        Shader proceduralShader = Shader.Find("Skybox/Procedural");

        if (proceduralShader != null)
        {
            proceduralSkyboxMaterial = new Material(proceduralShader);

            // Configure initial settings
            proceduralSkyboxMaterial.SetFloat("_SunSize", sunSize);
            proceduralSkyboxMaterial.SetFloat("_SunSizeConvergence", sunConvergence);
            proceduralSkyboxMaterial.SetFloat("_AtmosphereThickness", atmosphereThickness);
            proceduralSkyboxMaterial.SetFloat("_Exposure", exposure);
            proceduralSkyboxMaterial.SetColor("_SkyTint", dayTopColor);
            proceduralSkyboxMaterial.SetColor("_GroundColor", new Color(0.4f, 0.3f, 0.2f));

            RenderSettings.skybox = proceduralSkyboxMaterial;
            Debug.Log("Procedural skybox shader applied");
        }
        else
        {
            // Fallback: Create a gradient skybox using a custom approach
            Debug.LogWarning("Procedural skybox shader not found, using gradient fallback");
            SetupGradientSkybox();
        }
    }

    void SetupGradientSkybox()
    {
        // Create a simple gradient skybox material
        Shader cubemapShader = Shader.Find("Skybox/Cubemap");
        if (cubemapShader != null)
        {
            blendedSkyboxMaterial = new Material(cubemapShader);
            // You would need to generate or assign a cubemap here
        }
    }

    void DisableOldSkyDome()
    {
        // Use the DayNightCycle's method to disable the sky dome
        if (dayNightCycle != null)
        {
            dayNightCycle.DisableSkyDome();
            Debug.Log("Disabled procedural SkyDome - using Unity Skybox instead");
        }
    }

    void Update()
    {
        if (!isInitialized) return;
        if (dayNightCycle == null) return;

        UpdateSkyboxForTimeOfDay();
    }

    void UpdateSkyboxForTimeOfDay()
    {
        float hour = dayNightCycle.GetCurrentHour();
        float daylight = dayNightCycle.GetDaylightIntensity();

        if (useProceduralSkybox && proceduralSkyboxMaterial != null)
        {
            UpdateProceduralSkybox(hour, daylight);
        }
        else if (daySkybox != null && nightSkybox != null)
        {
            UpdateBlendedSkybox(hour);
        }

        // Update ambient lighting to match skybox
        UpdateAmbientLighting(hour, daylight);
    }

    void UpdateProceduralSkybox(float hour, float daylight)
    {
        Color skyTint;
        Color groundColor;
        float currentExposure;
        float thickness;

        // Determine sky colors based on time
        if (hour >= 6f && hour < 8f)
        {
            // Sunrise
            float t = (hour - 6f) / 2f;
            skyTint = Color.Lerp(nightTopColor, sunsetTopColor, t);
            groundColor = Color.Lerp(nightHorizonColor, sunsetHorizonColor, t);
            currentExposure = Mathf.Lerp(0.3f, exposure, t);
            thickness = Mathf.Lerp(0.5f, atmosphereThickness * 1.5f, t);
        }
        else if (hour >= 8f && hour < 10f)
        {
            // Morning transition to day
            float t = (hour - 8f) / 2f;
            skyTint = Color.Lerp(sunsetTopColor, dayTopColor, t);
            groundColor = Color.Lerp(sunsetHorizonColor, dayHorizonColor, t);
            currentExposure = exposure;
            thickness = Mathf.Lerp(atmosphereThickness * 1.5f, atmosphereThickness, t);
        }
        else if (hour >= 10f && hour < 17f)
        {
            // Daytime
            skyTint = dayTopColor;
            groundColor = dayHorizonColor;
            currentExposure = exposure;
            thickness = atmosphereThickness;
        }
        else if (hour >= 17f && hour < 19f)
        {
            // Sunset
            float t = (hour - 17f) / 2f;
            skyTint = Color.Lerp(dayTopColor, sunsetTopColor, t);
            groundColor = Color.Lerp(dayHorizonColor, sunsetHorizonColor, t);
            currentExposure = Mathf.Lerp(exposure, exposure * 0.8f, t);
            thickness = Mathf.Lerp(atmosphereThickness, atmosphereThickness * 1.5f, t);
        }
        else if (hour >= 19f && hour < 21f)
        {
            // Dusk to night
            float t = (hour - 19f) / 2f;
            skyTint = Color.Lerp(sunsetTopColor, nightTopColor, t);
            groundColor = Color.Lerp(sunsetHorizonColor, nightHorizonColor, t);
            currentExposure = Mathf.Lerp(exposure * 0.8f, 0.3f, t);
            thickness = Mathf.Lerp(atmosphereThickness * 1.5f, 0.5f, t);
        }
        else
        {
            // Night
            skyTint = nightTopColor;
            groundColor = nightHorizonColor;
            currentExposure = 0.3f;
            thickness = 0.5f;
        }

        // Apply to material
        proceduralSkyboxMaterial.SetColor("_SkyTint", skyTint);
        proceduralSkyboxMaterial.SetColor("_GroundColor", groundColor);
        proceduralSkyboxMaterial.SetFloat("_Exposure", currentExposure);
        proceduralSkyboxMaterial.SetFloat("_AtmosphereThickness", thickness);
    }

    void UpdateBlendedSkybox(float hour)
    {
        // Simple skybox switching with blending support
        // Unity doesn't natively support skybox blending, so we swap materials

        Material targetSkybox;

        if (hour >= 6f && hour < 8f || hour >= 17f && hour < 19f)
        {
            // Sunrise/Sunset
            targetSkybox = sunsetSkybox != null ? sunsetSkybox : daySkybox;
        }
        else if (hour >= 8f && hour < 17f)
        {
            // Day
            targetSkybox = daySkybox;
        }
        else
        {
            // Night
            targetSkybox = nightSkybox != null ? nightSkybox : daySkybox;
        }

        if (targetSkybox != null && RenderSettings.skybox != targetSkybox)
        {
            RenderSettings.skybox = targetSkybox;
        }
    }

    void UpdateAmbientLighting(float hour, float daylight)
    {
        // Set ambient mode to skybox for best integration
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

        // Adjust ambient intensity based on time of day
        RenderSettings.ambientIntensity = Mathf.Lerp(0.3f, 1f, daylight);

        // Reflection intensity
        RenderSettings.reflectionIntensity = Mathf.Lerp(0.2f, 1f, daylight);
    }

    // Public methods for runtime skybox changes

    /// <summary>
    /// Set a custom skybox material at runtime
    /// </summary>
    public void SetSkybox(Material skyboxMaterial)
    {
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            useProceduralSkybox = false;
        }
    }

    /// <summary>
    /// Enable/disable the procedural skybox
    /// </summary>
    public void SetProceduralSkybox(bool enabled)
    {
        useProceduralSkybox = enabled;
        if (enabled)
        {
            SetupProceduralSkybox();
        }
    }

    /// <summary>
    /// Update procedural skybox colors
    /// </summary>
    public void SetSkyColors(Color topColor, Color horizonColor)
    {
        if (proceduralSkyboxMaterial != null)
        {
            proceduralSkyboxMaterial.SetColor("_SkyTint", topColor);
            proceduralSkyboxMaterial.SetColor("_GroundColor", horizonColor);
        }
    }

    /// <summary>
    /// Rotate the skybox (useful for adding variety or matching sun position)
    /// </summary>
    public void RotateSkybox(float degrees)
    {
        RenderSettings.skybox.SetFloat("_Rotation", degrees);
    }

    // Re-enable old sky dome if needed
    public void EnableProceduralSkyDome()
    {
        if (dayNightCycle != null)
        {
            dayNightCycle.EnableSkyDome();
        }

        // Clear the skybox
        RenderSettings.skybox = null;
    }

    void OnDestroy()
    {
        // Clean up created materials
        if (proceduralSkyboxMaterial != null)
        {
            Destroy(proceduralSkyboxMaterial);
        }
        if (blendedSkyboxMaterial != null)
        {
            Destroy(blendedSkyboxMaterial);
        }
    }
}
