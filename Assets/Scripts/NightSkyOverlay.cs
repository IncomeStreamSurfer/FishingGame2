using UnityEngine;

/// <summary>
/// Night Sky Overlay System - Ensures sky is TRULY BLACK at night
///
/// This system creates a large black dome that fades in/out based on time of day:
/// - 6 AM - 8 AM: Black fades to transparent (sunrise)
/// - 8 AM - 6 PM: Fully transparent (blue sky visible)
/// - 6 PM - 8 PM: Transparent fades to black (sunset)
/// - 8 PM - 6 AM: Fully black (night)
///
/// The dome renders BEHIND stars/moon but IN FRONT of the blue procedural skybox,
/// ensuring stars and moon remain visible against a pure black background.
/// </summary>
public class NightSkyOverlay : MonoBehaviour
{
    public static NightSkyOverlay Instance { get; private set; }

    [Header("Integration")]
    [Tooltip("Reference to DayNightCycle for time-based updates")]
    public DayNightCycle dayNightCycle;

    [Header("Overlay Settings")]
    [Tooltip("Distance of the black dome from the center")]
    public float domeDistance = 150f;

    [Tooltip("Pure black color for night sky")]
    public Color nightBlackColor = new Color(0f, 0f, 0f, 1f);

    [Header("Transition Times (24-hour format)")]
    [Tooltip("Hour when sunrise starts (black begins to fade out)")]
    [Range(0f, 12f)]
    public float sunriseStartHour = 6f;

    [Tooltip("Hour when sunrise ends (fully transparent)")]
    [Range(0f, 12f)]
    public float sunriseEndHour = 8f;

    [Tooltip("Hour when sunset starts (begins to fade in)")]
    [Range(12f, 24f)]
    public float sunsetStartHour = 18f;

    [Tooltip("Hour when sunset ends (fully black)")]
    [Range(12f, 24f)]
    public float sunsetEndHour = 20f;

    // Private components
    private GameObject blackDome;
    private Material blackDomeMaterial;
    private Renderer domeRenderer;

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
        // Find DayNightCycle if not assigned
        if (dayNightCycle == null)
        {
            dayNightCycle = FindObjectOfType<DayNightCycle>();
        }

        CreateBlackDome();
        UpdateOverlay(); // Initial update
    }

    void CreateBlackDome()
    {
        // Create a large inverted sphere to act as night sky overlay
        blackDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        blackDome.name = "NightSkyOverlay_BlackDome";
        blackDome.transform.SetParent(transform);
        blackDome.transform.position = Vector3.zero;

        // Make it large enough to encompass the scene
        // Use negative X scale to flip normals inward
        blackDome.transform.localScale = new Vector3(-domeDistance * 2f, domeDistance * 2f, domeDistance * 2f);

        // Remove collider - we don't need physics
        Destroy(blackDome.GetComponent<Collider>());

        // Create transparent black material
        blackDomeMaterial = new Material(Shader.Find("Standard"));

        // Configure for transparency
        blackDomeMaterial.SetFloat("_Mode", 3); // Transparent mode
        blackDomeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        blackDomeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        blackDomeMaterial.SetInt("_ZWrite", 0);
        blackDomeMaterial.EnableKeyword("_ALPHABLEND_ON");

        // Render queue: AFTER skybox (2000) but BEFORE transparent objects (3000)
        // Must be AFTER stars (1700) and moon (1500) so they render on top of the black
        // Skybox = 2000, this dome = 2100, stars/moon appear in front because they're emissive
        blackDomeMaterial.renderQueue = 2100;

        // Start with pure black, alpha will be animated
        blackDomeMaterial.color = new Color(0f, 0f, 0f, 0f);

        // No metallic or glossiness - pure matte black
        blackDomeMaterial.SetFloat("_Metallic", 0f);
        blackDomeMaterial.SetFloat("_Glossiness", 0f);

        // Apply material
        domeRenderer = blackDome.GetComponent<Renderer>();
        domeRenderer.material = blackDomeMaterial;

        Debug.Log("NightSkyOverlay: Created black dome overlay for true black night sky");
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;
        if (dayNightCycle == null) return;

        UpdateOverlay();
    }

    // Log rendering status for debugging
    void OnWillRenderObject()
    {
        // This is called every time the object is rendered by any camera
        // If you see this log in Game view, the dome is rendering
        if (Application.isPlaying && blackDome != null && blackDome.activeSelf)
        {
            Debug.Log($"NightSkyOverlay rendering - Alpha: {blackDomeMaterial?.color.a:F2}");
        }
    }

    void UpdateOverlay()
    {
        if (blackDomeMaterial == null || dayNightCycle == null) return;

        float currentHour = dayNightCycle.GetCurrentHour();
        float blackAlpha = CalculateBlackAlpha(currentHour);

        // Update material alpha
        Color currentColor = blackDomeMaterial.color;
        currentColor.a = blackAlpha;
        blackDomeMaterial.color = currentColor;

        // Optionally disable the dome entirely when fully transparent for performance
        if (blackDome != null)
        {
            blackDome.SetActive(blackAlpha > 0.001f);
        }
    }

    /// <summary>
    /// Calculate how opaque the black overlay should be based on time of day
    /// Returns 0 (transparent) during day, 1 (opaque black) at night
    /// </summary>
    float CalculateBlackAlpha(float hour)
    {
        // Night time (fully black)
        if (hour < sunriseStartHour || hour >= sunsetEndHour)
        {
            return 1f;
        }

        // Sunrise transition (black fading to transparent)
        if (hour >= sunriseStartHour && hour < sunriseEndHour)
        {
            float t = (hour - sunriseStartHour) / (sunriseEndHour - sunriseStartHour);
            // Smooth fade using smoothstep for better visual quality
            t = Mathf.SmoothStep(0f, 1f, t);
            return 1f - t; // 1.0 -> 0.0
        }

        // Daytime (fully transparent - blue sky visible)
        if (hour >= sunriseEndHour && hour < sunsetStartHour)
        {
            return 0f;
        }

        // Sunset transition (transparent fading to black)
        if (hour >= sunsetStartHour && hour < sunsetEndHour)
        {
            float t = (hour - sunsetStartHour) / (sunsetEndHour - sunsetStartHour);
            // Smooth fade using smoothstep
            t = Mathf.SmoothStep(0f, 1f, t);
            return t; // 0.0 -> 1.0
        }

        // Default to transparent
        return 0f;
    }

    /// <summary>
    /// Get the current opacity of the night overlay (0-1)
    /// </summary>
    public float GetNightOverlayAlpha()
    {
        if (dayNightCycle == null) return 0f;
        return CalculateBlackAlpha(dayNightCycle.GetCurrentHour());
    }

    /// <summary>
    /// Check if night overlay is currently active (visible)
    /// </summary>
    public bool IsNightOverlayActive()
    {
        return GetNightOverlayAlpha() > 0.001f;
    }

    /// <summary>
    /// Manually set the night overlay alpha (useful for testing)
    /// </summary>
    public void SetOverlayAlpha(float alpha)
    {
        if (blackDomeMaterial != null)
        {
            alpha = Mathf.Clamp01(alpha);
            Color currentColor = blackDomeMaterial.color;
            currentColor.a = alpha;
            blackDomeMaterial.color = currentColor;
        }
    }

    /// <summary>
    /// Change the black color (advanced customization)
    /// </summary>
    public void SetBlackColor(Color newBlackColor)
    {
        nightBlackColor = newBlackColor;
        if (blackDomeMaterial != null)
        {
            float currentAlpha = blackDomeMaterial.color.a;
            blackDomeMaterial.color = new Color(
                nightBlackColor.r,
                nightBlackColor.g,
                nightBlackColor.b,
                currentAlpha
            );
        }
    }

    void OnDestroy()
    {
        // Clean up material
        if (blackDomeMaterial != null)
        {
            Destroy(blackDomeMaterial);
        }

        // Clean up dome object
        if (blackDome != null)
        {
            Destroy(blackDome);
        }
    }

    // Debug visualization in editor
    void OnDrawGizmosSelected()
    {
        // Draw a wireframe sphere showing the dome's position
        Gizmos.color = new Color(0f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, domeDistance);
    }
}
