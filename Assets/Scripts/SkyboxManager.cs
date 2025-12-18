using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    [Header("Procedural Sky Colors - Beautiful & Atmospheric")]
    // Day colors - vibrant blue sky
    public Color dayTopColor = new Color(0.20f, 0.50f, 0.95f);      // Bright azure sky
    public Color dayHorizonColor = new Color(0.60f, 0.75f, 0.92f);  // Soft blue horizon
    // Sunrise colors - pink and orange
    public Color sunriseTopColor = new Color(0.40f, 0.25f, 0.65f);  // Purple-pink sky
    public Color sunriseHorizonColor = new Color(0.98f, 0.55f, 0.35f); // Orange-pink glow
    // Sunset colors - dramatic orange and red
    public Color sunsetTopColor = new Color(0.30f, 0.20f, 0.55f);   // Deep purple sunset
    public Color sunsetHorizonColor = new Color(0.98f, 0.40f, 0.20f); // Intense orange-red
    // Night colors - COMPLETELY BLACK for dramatic starlight and moonlight
    public Color nightTopColor = new Color(0.0f, 0.0f, 0.0f);  // Pure black night sky
    public Color nightHorizonColor = new Color(0.0f, 0.0f, 0.0f); // Pure black horizon

    [Header("Sun Settings (for procedural skybox)")]
    [Range(0f, 1f)]
    public float sunSize = 0.03f;      // Smaller sun
    [Range(0f, 1f)]
    public float sunConvergence = 5f;
    public float atmosphereThickness = 0.6f;  // Lower atmosphere
    public float exposure = 0.5f;             // DRASTICALLY REDUCED - fix overexposure

    [Header("Cloud System")]
    [Tooltip("Enable procedural cloud system")]
    public bool enableClouds = true;
    [Range(0, 30)]
    public int cloudCount = 6;  // Reduced from 12 for better performance
    public float cloudSpeed = 0.5f;
    public float cloudHeight = 45f;

    [Header("Star System")]
    [Tooltip("Enable procedural star system at night")]
    public bool enableStars = true;
    [Range(50, 800)]
    public int starCount = 100;          // Reduced from 400 for better performance
    public float starDistance = 95f;
    [Range(0f, 1f)]
    public float starTwinkleSpeed = 0.6f;
    public float starFadeInHour = 18.5f; // Stars start appearing at 6:30 PM
    public float starFadeOutHour = 6.5f; // Stars fade out at 6:30 AM
    [Range(1f, 5f)]
    public float starBrightness = 3f;    // Star emission multiplier

    [Header("Moon System")]
    [Tooltip("Enable moon that appears at night")]
    public bool enableMoon = true;
    public float moonSize = 1.2f;  // MUCH smaller moon (was 3f)
    public float moonDistance = 80f;
    public Color moonColor = new Color(1f, 1f, 1f);  // Bright white for moonlight
    public float moonGlowIntensity = 4f;  // Increased for brighter moonlight

    [Header("Integration")]
    [Tooltip("Link to DayNightCycle for automatic time-based skybox changes")]
    public DayNightCycle dayNightCycle;

    [Tooltip("Disable the procedural sky dome in DayNightCycle when using skybox")]
    public bool disableProceduralSkyDome = true;

    // Private
    private Material proceduralSkyboxMaterial;
    private Material blendedSkyboxMaterial;
    private bool isInitialized = false;

    // Cloud system
    private List<GameObject> clouds = new List<GameObject>();
    private List<Material> cloudMaterials = new List<Material>();
    private List<Vector3> cloudVelocities = new List<Vector3>();

    // Star system
    private List<GameObject> stars = new List<GameObject>();
    private List<Material> starMaterials = new List<Material>();
    private List<float> starTwinkleOffsets = new List<float>();

    // Moon
    private GameObject moon;
    private Material moonMaterial;
    private Light moonLight;

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

        // Debug: Check camera settings
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"SkyboxManager: Camera clear flags = {mainCam.clearFlags}");
            Debug.Log($"SkyboxManager: Camera culling mask = {mainCam.cullingMask}");
            Debug.Log($"SkyboxManager: RenderSettings.skybox = {RenderSettings.skybox?.name ?? "null"}");
        }
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

        // Create cloud system
        if (enableClouds)
        {
            CreateCloudSystem();
        }

        // Create star system
        if (enableStars)
        {
            CreateStarSystem();
        }

        // Create moon
        if (enableMoon)
        {
            CreateMoon();
        }

        isInitialized = true;
        Debug.Log("SkyboxManager initialized - Skybox system active");
    }

    void CreateCloudSystem()
    {
        for (int i = 0; i < cloudCount; i++)
        {
            // Create cloud as a stretched sphere
            GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cloud.name = "Cloud_" + i;
            cloud.transform.SetParent(transform);

            // Random position in sky
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(60f, 90f);
            float height = cloudHeight + Random.Range(-10f, 10f);

            cloud.transform.position = new Vector3(
                Mathf.Cos(angle) * distance,
                height,
                Mathf.Sin(angle) * distance
            );

            // Stretched, fluffy cloud shape
            float sizeX = Random.Range(8f, 16f);
            float sizeY = Random.Range(3f, 6f);
            float sizeZ = Random.Range(6f, 12f);
            cloud.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
            cloud.transform.rotation = Quaternion.Euler(
                Random.Range(-10f, 10f),
                Random.Range(0f, 360f),
                Random.Range(-5f, 5f)
            );

            Destroy(cloud.GetComponent<Collider>());

            // Cloud material - white/light gray, transparent
            Material cloudMat = new Material(Shader.Find("Standard"));
            cloudMat.SetFloat("_Mode", 3);
            cloudMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            cloudMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            cloudMat.SetInt("_ZWrite", 0);
            cloudMat.EnableKeyword("_ALPHABLEND_ON");
            cloudMat.renderQueue = 2150; // Render AFTER black dome overlay but BEFORE stars/moon
            cloudMat.color = new Color(1f, 1f, 1f, 0.7f);
            cloudMat.SetFloat("_Metallic", 0f);
            cloudMat.SetFloat("_Glossiness", 0.1f);

            cloud.GetComponent<Renderer>().material = cloudMat;

            clouds.Add(cloud);
            cloudMaterials.Add(cloudMat);

            // Random cloud velocity
            Vector3 velocity = new Vector3(
                Random.Range(-cloudSpeed, cloudSpeed),
                0f,
                Random.Range(-cloudSpeed, cloudSpeed)
            );
            cloudVelocities.Add(velocity);
        }

        Debug.Log($"Created {cloudCount} procedural clouds");
    }

    void CreateStarSystem()
    {
        for (int i = 0; i < starCount; i++)
        {
            // Create star as a small sphere
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = "Star_" + i;
            star.transform.SetParent(transform);

            // Random position on a sphere around the sky
            Vector3 randomDir = Random.onUnitSphere;
            // Keep stars above the horizon
            if (randomDir.y < 0.05f) randomDir.y = 0.05f + Random.Range(0f, 0.1f);
            randomDir.Normalize();

            star.transform.position = randomDir * starDistance;

            // Varied star sizes - some big bright stars, many small ones
            float sizeRoll = Random.Range(0f, 1f);
            float size;
            if (sizeRoll < 0.05f)
            {
                // 5% are large bright stars
                size = Random.Range(0.35f, 0.5f);
            }
            else if (sizeRoll < 0.2f)
            {
                // 15% are medium stars
                size = Random.Range(0.2f, 0.35f);
            }
            else
            {
                // 80% are small stars
                size = Random.Range(0.1f, 0.2f);
            }
            star.transform.localScale = Vector3.one * size;

            // Remove collider
            Destroy(star.GetComponent<Collider>());

            // Star material - emissive with varied colors
            Material starMat = new Material(Shader.Find("Standard"));
            starMat.SetFloat("_Mode", 3);
            starMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            starMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            starMat.SetInt("_ZWrite", 0);
            starMat.EnableKeyword("_EMISSION");
            starMat.renderQueue = 2200; // Render AFTER skybox and black dome overlay so stars appear on top

            // Star color - varied whites, yellows, and occasional blue/red stars
            Color baseColor;
            float colorRoll = Random.Range(0f, 1f);
            if (colorRoll < 0.6f)
            {
                // White stars
                baseColor = new Color(1f, 1f, 1f);
            }
            else if (colorRoll < 0.8f)
            {
                // Warm yellow/orange stars
                baseColor = new Color(1f, 0.9f, 0.7f);
            }
            else if (colorRoll < 0.92f)
            {
                // Cool blue-white stars
                baseColor = new Color(0.85f, 0.9f, 1f);
            }
            else
            {
                // Rare red giants
                baseColor = new Color(1f, 0.7f, 0.6f);
            }

            starMat.color = baseColor;
            // Brighter emission based on size
            float emissionMultiplier = (size > 0.3f) ? starBrightness * 1.5f : starBrightness;
            starMat.SetColor("_EmissionColor", baseColor * emissionMultiplier);
            starMat.SetFloat("_Metallic", 0f);
            starMat.SetFloat("_Glossiness", 0f);

            star.GetComponent<Renderer>().material = starMat;

            stars.Add(star);
            starMaterials.Add(starMat);
            starTwinkleOffsets.Add(Random.Range(0f, 100f));
        }

        Debug.Log($"Created {starCount} procedural stars");
    }

    void CreateMoon()
    {
        // Create moon as a sphere
        moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moon.name = "Moon";
        moon.transform.SetParent(transform);

        moon.transform.localScale = Vector3.one * moonSize;

        // Remove collider
        Destroy(moon.GetComponent<Collider>());

        // Moon material - emissive with glow
        moonMaterial = new Material(Shader.Find("Standard"));
        moonMaterial.SetFloat("_Mode", 3);
        moonMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        moonMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        moonMaterial.SetInt("_ZWrite", 0);
        moonMaterial.EnableKeyword("_EMISSION");
        moonMaterial.renderQueue = 2250; // Render AFTER skybox and black dome overlay so moon appears on top

        moonMaterial.color = moonColor;
        moonMaterial.SetColor("_EmissionColor", moonColor * moonGlowIntensity);
        moonMaterial.SetFloat("_Metallic", 0f);
        moonMaterial.SetFloat("_Glossiness", 0.3f);

        moon.GetComponent<Renderer>().material = moonMaterial;

        // Add a DIRECTIONAL light to the moon for realistic moonlight that shines DOWN
        moonLight = moon.AddComponent<Light>();
        moonLight.type = LightType.Directional;  // Changed from Point to Directional
        moonLight.color = moonColor;
        moonLight.intensity = 0.3f;  // Directional lights need lower intensity
        moonLight.shadows = LightShadows.Soft;  // Soft shadows for moonlight

        // Configure high-quality soft shadows for moonlight
        moonLight.shadowStrength = 0.8f;
        moonLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
        moonLight.shadowBias = 0.05f;
        moonLight.shadowNormalBias = 0.4f;
        moonLight.shadowNearPlane = 0.2f;

        Debug.Log("Created moon with directional light");
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

    // Frame counter for periodic debug logging
    private int debugLogFrameCounter = 0;

    void Update()
    {
        if (!isInitialized) return;
        if (dayNightCycle == null) return;
        if (!MainMenu.GameStarted) return;

        UpdateSkyboxForTimeOfDay();
        UpdateClouds();
        UpdateStars();
        UpdateMoon();

        // Periodic debug logging (every 60 frames = ~1 second)
        // Debug logging disabled to prevent MissingReferenceException spam
        // debugLogFrameCounter++;
        // if (debugLogFrameCounter >= 60)
        // {
        //     debugLogFrameCounter = 0;
        //     float hour = dayNightCycle.GetCurrentHour();
        //     Debug.Log($"SkyboxManager Update: Hour={hour:F1}");
        // }
    }

    void UpdateClouds()
    {
        if (!enableClouds || clouds.Count == 0) return;

        float hour = dayNightCycle.GetCurrentHour();
        float daylight = dayNightCycle.GetDaylightIntensity();

        for (int i = 0; i < clouds.Count; i++)
        {
            if (clouds[i] == null) continue;

            // Move clouds
            clouds[i].transform.position += cloudVelocities[i] * Time.deltaTime;

            // Wrap clouds around (simple wrapping)
            Vector3 pos = clouds[i].transform.position;
            if (pos.x > 100f) pos.x = -100f;
            if (pos.x < -100f) pos.x = 100f;
            if (pos.z > 100f) pos.z = -100f;
            if (pos.z < -100f) pos.z = 100f;
            clouds[i].transform.position = pos;

            // Update cloud color based on time of day
            if (cloudMaterials[i] != null)
            {
                Color cloudColor = Color.white;

                // Sunrise/sunset tinting
                if (hour >= 5f && hour < 8.5f)
                {
                    // Sunrise - pink/orange clouds
                    float t = (hour - 5f) / 3.5f;
                    cloudColor = Color.Lerp(
                        new Color(0.3f, 0.3f, 0.4f), // Dark blue-gray
                        new Color(1f, 0.7f, 0.5f),   // Orange-pink
                        t
                    );
                }
                else if (hour >= 16.5f && hour < 20f)
                {
                    // Sunset - orange/red/purple clouds
                    float t = (hour - 16.5f) / 3.5f;
                    if (t < 0.5f)
                    {
                        cloudColor = Color.Lerp(
                            Color.white,
                            new Color(1f, 0.6f, 0.4f), // Orange
                            t * 2f
                        );
                    }
                    else
                    {
                        cloudColor = Color.Lerp(
                            new Color(1f, 0.6f, 0.4f),   // Orange
                            new Color(0.5f, 0.3f, 0.5f), // Purple-gray
                            (t - 0.5f) * 2f
                        );
                    }
                }
                else if (hour < 5f || hour >= 20f)
                {
                    // Night - dark clouds
                    cloudColor = new Color(0.15f, 0.15f, 0.2f);
                }

                // Apply color and fade based on daylight
                float alpha = Mathf.Lerp(0.3f, 0.7f, daylight);
                cloudMaterials[i].color = new Color(cloudColor.r, cloudColor.g, cloudColor.b, alpha);
            }
        }
    }

    // Frame skipping for star updates
    private int starUpdateFrameCounter = 0;

    void UpdateStars()
    {
        if (!enableStars || stars.Count == 0) return;

        // Only update stars every 3rd frame for better performance
        starUpdateFrameCounter++;
        if (starUpdateFrameCounter < 3)
        {
            return;
        }
        starUpdateFrameCounter = 0;

        float hour = dayNightCycle.GetCurrentHour();

        // Calculate star visibility based on time of day
        float starVisibility = 0f;

        if (hour >= starFadeInHour || hour < starFadeOutHour)
        {
            // Night time - stars visible
            if (hour >= starFadeInHour && hour < 24f)
            {
                // Evening - fade in over 1.5 hours
                float fadeInDuration = 1.5f;
                float timeSinceFadeIn = hour - starFadeInHour;
                starVisibility = Mathf.Clamp01(timeSinceFadeIn / fadeInDuration);
            }
            else if (hour >= 20f || hour < 5.5f)
            {
                // Deep night - fully visible
                starVisibility = 1f;
            }
            else if (hour >= 5.5f && hour < starFadeOutHour)
            {
                // Morning - fade out
                float fadeOutDuration = starFadeOutHour - 5.5f;
                float timeUntilFadeOut = starFadeOutHour - hour;
                starVisibility = Mathf.Clamp01(timeUntilFadeOut / fadeOutDuration);
            }
        }

        // Update each star
        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] == null || starMaterials[i] == null) continue;

            // Twinkle effect - vary brightness with more variation
            float twinkle = Mathf.PerlinNoise(
                Time.time * starTwinkleSpeed + starTwinkleOffsets[i],
                starTwinkleOffsets[i]
            );
            // Map twinkle from 0-1 to 0.5-1.0 for noticeable variation
            float twinkleBrightness = Mathf.Lerp(0.5f, 1.0f, twinkle);

            // Get original star color from its scale (larger stars are brighter)
            float starScale = stars[i].transform.localScale.x;
            float sizeBrightness = (starScale > 0.3f) ? 1.5f : (starScale > 0.2f ? 1.2f : 1f);

            // Apply visibility and twinkle
            float finalBrightness = starVisibility * twinkleBrightness * sizeBrightness * starBrightness;

            // Get the star's base color from material
            Color baseColor = starMaterials[i].color;

            // Set star material emission - brighter stars for more visible starlight
            starMaterials[i].SetColor("_EmissionColor", baseColor * finalBrightness);

            // Enable/disable star based on visibility
            stars[i].SetActive(starVisibility > 0.01f);
        }
    }

    void UpdateMoon()
    {
        if (!enableMoon || moon == null) return;

        float hour = dayNightCycle.GetCurrentHour();

        // Moon is opposite to sun (180 degree phase shift)
        // Moon movement is SLOW and tied to game time - moves gradually across night sky
        // Sun rises at 6AM, peaks at 12PM, sets at 6PM
        // Moon rises at 6PM, peaks at 12AM (midnight), sets at 6AM
        // Movement speed matches the day/night cycle speed (smooth and realistic)

        // Moon angle follows same math as sun but with 180 degree offset
        // When currentTimeOfDay = 18 (6PM), moonAngle should be 0 (horizon)
        // When currentTimeOfDay = 0 (midnight), moonAngle should be 90 (overhead)
        // When currentTimeOfDay = 6 (6AM), moonAngle should be 180 (horizon)
        float moonAngle = (hour - 6f) * 15f + 180f; // Add 180 degrees to be opposite of sun
        float radAngle = moonAngle * Mathf.Deg2Rad;

        // Calculate moon height (y position) and horizontal position (z position)
        float height = Mathf.Sin(radAngle);  // Height above/below horizon (-1 to 1)
        float horizontal = Mathf.Cos(radAngle);  // Horizontal position along Z-axis

        // Position moon along same arc path as sun but opposite
        // Moon travels along Z-axis (forward/back) just like the sun
        Vector3 moonPos = new Vector3(0, height * moonDistance, horizontal * moonDistance);
        moon.transform.position = moonPos;

        // Moon light should point toward world center (shine DOWN onto scene)
        moon.transform.LookAt(Vector3.zero);

        // CRITICAL: Moon only visible when ABOVE horizon (y > 0, which means height > 0)
        // This ensures moon rises FROM BEHIND horizon and sets BEHIND horizon
        bool moonVisible = height > 0f;

        // Calculate fade-in/fade-out near horizon for smooth transitions
        float moonVisibility = Mathf.Clamp01((height + 0.05f) / 0.15f);  // Fade from -0.05 to +0.10

        // Update moon brightness and glow based on visibility
        Color currentMoonColor = moonColor * moonVisibility;
        moonMaterial.SetColor("_EmissionColor", currentMoonColor * moonGlowIntensity);

        // Update moon light - directional light shines from moon's position DOWN onto scene
        if (moonLight != null)
        {
            moonLight.intensity = 0.3f * moonVisibility;  // Directional light intensity
        }

        // Enable/disable moon based on visibility
        moon.SetActive(moonVisible);
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

        // Determine sky colors based on time - ENHANCED with beautiful transitions
        if (hour >= 5f && hour < 6.5f)
        {
            // Early sunrise - dark to pink/orange
            float t = (hour - 5f) / 1.5f;
            skyTint = Color.Lerp(nightTopColor, sunriseTopColor, t);
            groundColor = Color.Lerp(nightHorizonColor, sunriseHorizonColor, t);
            currentExposure = Mathf.Lerp(0.25f, exposure * 0.9f, t);
            thickness = Mathf.Lerp(0.4f, atmosphereThickness * 2.0f, t); // Thick atmosphere for vibrant sunrise
        }
        else if (hour >= 6.5f && hour < 8.5f)
        {
            // Sunrise peak - beautiful orange/pink glow
            float t = (hour - 6.5f) / 2f;
            skyTint = Color.Lerp(sunriseTopColor, dayTopColor, t);
            groundColor = Color.Lerp(sunriseHorizonColor, dayHorizonColor, t);
            currentExposure = Mathf.Lerp(exposure * 0.9f, exposure, t);
            thickness = Mathf.Lerp(atmosphereThickness * 2.0f, atmosphereThickness, t);
        }
        else if (hour >= 8.5f && hour < 16.5f)
        {
            // Full daytime - clear blue sky
            skyTint = dayTopColor;
            groundColor = dayHorizonColor;
            currentExposure = exposure;
            thickness = atmosphereThickness;
        }
        else if (hour >= 16.5f && hour < 18f)
        {
            // Beginning of sunset - blue to orange
            float t = (hour - 16.5f) / 1.5f;
            skyTint = Color.Lerp(dayTopColor, sunsetTopColor, t);
            groundColor = Color.Lerp(dayHorizonColor, sunsetHorizonColor, t);
            currentExposure = Mathf.Lerp(exposure, exposure * 0.85f, t);
            thickness = Mathf.Lerp(atmosphereThickness, atmosphereThickness * 2.2f, t); // Thicker for dramatic sunset
        }
        else if (hour >= 18f && hour < 19.5f)
        {
            // Sunset peak - dramatic orange/red/purple
            float t = (hour - 18f) / 1.5f;
            // Enhanced sunset colors with extra vibrancy
            Color enhancedSunsetTop = Color.Lerp(sunsetTopColor, new Color(0.20f, 0.10f, 0.40f), t);
            Color enhancedSunsetHorizon = Color.Lerp(sunsetHorizonColor, new Color(0.90f, 0.30f, 0.15f), t);
            skyTint = enhancedSunsetTop;
            groundColor = enhancedSunsetHorizon;
            currentExposure = Mathf.Lerp(exposure * 0.85f, exposure * 0.6f, t);
            thickness = Mathf.Lerp(atmosphereThickness * 2.2f, atmosphereThickness * 1.8f, t);
        }
        else if (hour >= 19.5f && hour < 21f)
        {
            // Dusk to night - fade to darkness
            float t = (hour - 19.5f) / 1.5f;
            skyTint = Color.Lerp(new Color(0.20f, 0.10f, 0.40f), nightTopColor, t);
            groundColor = Color.Lerp(new Color(0.90f, 0.30f, 0.15f), nightHorizonColor, t);
            currentExposure = Mathf.Lerp(exposure * 0.6f, 0.25f, t);
            thickness = Mathf.Lerp(atmosphereThickness * 1.8f, 0.4f, t);
        }
        else
        {
            // Night - dark sky
            skyTint = nightTopColor;
            groundColor = nightHorizonColor;
            currentExposure = 0.25f;
            thickness = 0.4f;
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

        // Adjust ambient intensity based on time of day - darker at night
        float ambientIntensity;
        if (hour < 5f || hour >= 20f)
        {
            // Deep night - very dark ambient
            ambientIntensity = 0.15f;
        }
        else if (hour >= 5f && hour < 7f)
        {
            // Early morning - gradual increase
            float t = (hour - 5f) / 2f;
            ambientIntensity = Mathf.Lerp(0.15f, 0.8f, t);
        }
        else if (hour >= 18f && hour < 20f)
        {
            // Evening - gradual decrease
            float t = (hour - 18f) / 2f;
            ambientIntensity = Mathf.Lerp(0.9f, 0.15f, t);
        }
        else
        {
            // Daytime - normal ambient
            ambientIntensity = Mathf.Lerp(0.7f, 1f, daylight);
        }
        RenderSettings.ambientIntensity = ambientIntensity;

        // Reflection intensity - lower at night for darker feel
        RenderSettings.reflectionIntensity = Mathf.Lerp(0.1f, 1f, daylight);
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

        // Clean up clouds
        foreach (var cloudMat in cloudMaterials)
        {
            if (cloudMat != null) Destroy(cloudMat);
        }
        foreach (var cloud in clouds)
        {
            if (cloud != null) Destroy(cloud);
        }

        // Clean up stars
        foreach (var starMat in starMaterials)
        {
            if (starMat != null) Destroy(starMat);
        }
        foreach (var star in stars)
        {
            if (star != null) Destroy(star);
        }

        // Clean up moon
        if (moonMaterial != null) Destroy(moonMaterial);
        if (moon != null) Destroy(moon);
    }
}
