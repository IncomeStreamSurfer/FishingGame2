using UnityEngine;
using System.Collections;

/// <summary>
/// Day/Night cycle system with moving sun, changing sky colors, and dynamic lighting
/// Full day cycle in real-time (configurable speed)
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Header("Cycle Settings")]
    [Tooltip("How many real seconds for a full day cycle (24 in-game hours)")]
    public float dayLengthInSeconds = 600f; // 10 minutes = full day by default

    [Header("Sky Settings")]
    [Tooltip("Disable the procedural sky dome (use when SkyboxManager is active)")]
    public bool disableSkyDome = false;

    [Header("Sun Settings")]
    public float sunDistance = 100f;
    public float sunSize = 8f;

    [Header("Lighting Colors - Beautiful & Atmospheric")]
    public Color sunriseColor = new Color(0.95f, 0.60f, 0.40f);   // Warm orange-pink sunrise
    public Color noonColor = new Color(0.85f, 0.80f, 0.70f);      // Bright but not white noon
    public Color sunsetColor = new Color(0.98f, 0.50f, 0.30f);    // Dramatic orange-red sunset
    public Color nightColor = new Color(0.0f, 0.0f, 0.0f);        // PURE BLACK for dramatic moonlight and starlight

    [Header("Sky Colors - More Vibrant")]
    public Color daySkyColor = new Color(0.35f, 0.60f, 0.90f);    // Vibrant blue sky
    public Color sunsetSkyColor = new Color(0.95f, 0.50f, 0.40f); // Rich sunset
    public Color nightSkyColor = new Color(0.0f, 0.0f, 0.0f);     // PURE BLACK night sky for stars and moonlight

    // Time tracking (0-24 hours)
    private float currentTimeOfDay = 8f; // Start at 8 AM
    private float timeSpeed;

    // Day tracking
    private int currentDay = 1;
    private float previousTimeOfDay = 8f; // Used to detect midnight crossing
    private const string DAY_COUNTER_KEY = "CurrentDay";

    // Sun components
    private GameObject sunObject;
    private Light sunLight;
    private GameObject sunGlow;
    private Material sunMaterial;
    private Material glowMaterial;

    // Stars
    private GameObject starsContainer;
    private GameObject[] stars;

    // Ambient light reference
    private Light ambientLight;

    // Sky dome
    private GameObject skyDome;
    private Material skyMaterial;

    // Cached transform references (avoid transform.Find every frame)
    private Transform cachedSunSphere;
    private Transform cachedSunGlow;

    // Cached OnGUI style
    private GUIStyle cachedTimeStyle;
    private bool stylesInitialized = false;

    // Performance optimization - frame counter for star updates
    private int starUpdateFrameCounter = 0;

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
        timeSpeed = 24f / dayLengthInSeconds;

        // Load saved day counter
        LoadDayCounter();

        // Configure global shadow quality settings for smooth, soft shadows
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowProjection = ShadowProjection.StableFit;
        QualitySettings.shadowDistance = 150f;
        QualitySettings.shadowCascades = 4;
        QualitySettings.shadowCascade2Split = 0.33333f;
        QualitySettings.shadowCascade4Split = new Vector3(0.06666f, 0.2f, 0.46666f);

        CreateSun();
        CreateStars();
        CreateSkyDome();
        CreateAmbientLight();

        // Initial update
        UpdateCycle();
    }

    void CreateSun()
    {
        sunObject = new GameObject("Sun");
        sunObject.transform.SetParent(transform);

        // Sun sphere (bright glowing ball)
        GameObject sunSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sunSphere.name = "SunSphere";
        sunSphere.transform.SetParent(sunObject.transform);
        sunSphere.transform.localPosition = Vector3.zero;
        sunSphere.transform.localScale = Vector3.one * sunSize;
        Destroy(sunSphere.GetComponent<Collider>());

        sunMaterial = new Material(Shader.Find("Standard"));
        sunMaterial.color = new Color(1f, 0.95f, 0.8f);
        sunMaterial.EnableKeyword("_EMISSION");
        sunMaterial.SetColor("_EmissionColor", new Color(1f, 0.95f, 0.8f) * 3f);
        sunMaterial.renderQueue = 2250; // Render AFTER skybox and black dome overlay
        sunSphere.GetComponent<Renderer>().material = sunMaterial;

        // Sun glow (larger transparent sphere)
        sunGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sunGlow.name = "SunGlow";
        sunGlow.transform.SetParent(sunObject.transform);
        sunGlow.transform.localPosition = Vector3.zero;
        sunGlow.transform.localScale = Vector3.one * sunSize * 3f;
        Destroy(sunGlow.GetComponent<Collider>());

        glowMaterial = new Material(Shader.Find("Standard"));
        glowMaterial.SetFloat("_Mode", 3);
        glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glowMaterial.SetInt("_ZWrite", 0);
        glowMaterial.EnableKeyword("_ALPHABLEND_ON");
        glowMaterial.renderQueue = 2240; // Render AFTER skybox and black dome overlay, slightly before sun sphere
        glowMaterial.color = new Color(1f, 0.9f, 0.6f, 0.15f);
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.6f) * 0.5f);
        sunGlow.GetComponent<Renderer>().material = glowMaterial;

        // Cache the transforms for later use (avoids transform.Find every frame)
        cachedSunSphere = sunSphere.transform;
        cachedSunGlow = sunGlow.transform;

        // Sun directional light
        sunLight = sunObject.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.color = noonColor;
        sunLight.intensity = 1.2f;
        sunLight.shadows = LightShadows.Soft;

        // Configure high-quality soft shadows
        sunLight.shadowStrength = 1f;
        sunLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
        sunLight.shadowBias = 0.05f;
        sunLight.shadowNormalBias = 0.4f;
        sunLight.shadowNearPlane = 0.2f;
    }

    void CreateStars()
    {
        starsContainer = new GameObject("Stars");
        starsContainer.transform.SetParent(transform);

        int numStars = 20;  // Reduced from 200 to save ~40MB RAM
        stars = new GameObject[numStars];

        // Use ONE shared material for all stars to save memory
        Material starMat = new Material(Shader.Find("Standard"));
        starMat.color = Color.white;
        starMat.EnableKeyword("_EMISSION");
        starMat.SetColor("_EmissionColor", Color.white * 2.5f);
        starMat.renderQueue = 2200; // Render AFTER skybox and black dome overlay so stars appear on top

        for (int i = 0; i < numStars; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = "Star_" + i;
            star.transform.SetParent(starsContainer.transform);

            // Random position on sky sphere (above horizon)
            float theta = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float phi = Random.Range(10f, 85f) * Mathf.Deg2Rad; // Only upper hemisphere

            float skyRadius = sunDistance * 0.95f;
            star.transform.position = new Vector3(
                Mathf.Cos(theta) * Mathf.Sin(phi) * skyRadius,
                Mathf.Cos(phi) * skyRadius,
                Mathf.Sin(theta) * Mathf.Sin(phi) * skyRadius
            );

            float starSize = Random.Range(0.5f, 1.5f);  // Slightly bigger since fewer
            star.transform.localScale = Vector3.one * starSize;
            Destroy(star.GetComponent<Collider>());

            // Use shared material instead of creating one per star
            star.GetComponent<Renderer>().sharedMaterial = starMat;

            stars[i] = star;
        }
    }

    void CreateSkyDome()
    {
        // Skip sky dome creation if using SkyboxManager
        if (disableSkyDome)
        {
            Debug.Log("DayNightCycle: Sky dome disabled - using SkyboxManager instead");
            return;
        }

        // Create a large inverted sphere for the sky
        skyDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        skyDome.name = "SkyDome";
        skyDome.transform.SetParent(transform);
        skyDome.transform.position = Vector3.zero;
        skyDome.transform.localScale = Vector3.one * sunDistance * 2f;
        Destroy(skyDome.GetComponent<Collider>());

        // Flip normals by using negative scale
        skyDome.transform.localScale = new Vector3(-sunDistance * 2f, sunDistance * 2f, sunDistance * 2f);

        skyMaterial = new Material(Shader.Find("Standard"));
        skyMaterial.color = daySkyColor;
        skyMaterial.SetFloat("_Metallic", 0f);
        skyMaterial.SetFloat("_Glossiness", 0f);

        // Make it unlit looking
        skyMaterial.EnableKeyword("_EMISSION");
        skyMaterial.SetColor("_EmissionColor", daySkyColor * 0.5f);

        skyDome.GetComponent<Renderer>().material = skyMaterial;
    }

    /// <summary>
    /// Disable the sky dome at runtime (called by SkyboxManager)
    /// </summary>
    public void DisableSkyDome()
    {
        disableSkyDome = true;
        if (skyDome != null)
        {
            skyDome.SetActive(false);
        }
    }

    /// <summary>
    /// Enable the sky dome at runtime
    /// </summary>
    public void EnableSkyDome()
    {
        disableSkyDome = false;
        if (skyDome != null)
        {
            skyDome.SetActive(true);
        }
        else
        {
            CreateSkyDome();
        }
    }

    void CreateAmbientLight()
    {
        GameObject ambientObj = new GameObject("AmbientLight");
        ambientObj.transform.SetParent(transform);
        ambientObj.transform.position = new Vector3(0, 50, 0);

        ambientLight = ambientObj.AddComponent<Light>();
        ambientLight.type = LightType.Point;
        ambientLight.color = Color.white;
        ambientLight.intensity = 0.3f;
        ambientLight.range = 500f;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Store previous time before advancing
        previousTimeOfDay = currentTimeOfDay;

        // Advance time
        currentTimeOfDay += timeSpeed * Time.deltaTime;
        if (currentTimeOfDay >= 24f)
            currentTimeOfDay -= 24f;

        // Check for midnight crossing (clock went from 23:xx to 00:xx)
        CheckMidnightCrossing();

        UpdateCycle();
    }

    void CheckMidnightCrossing()
    {
        // Detect when time crosses midnight (0:00)
        // previousTimeOfDay was close to 24 and currentTimeOfDay wrapped to near 0
        if (previousTimeOfDay > 23f && currentTimeOfDay < 1f)
        {
            currentDay++;
            SaveDayCounter();
            Debug.Log($"Day {currentDay} has begun!");
        }
    }

    void SaveDayCounter()
    {
        PlayerPrefs.SetInt(DAY_COUNTER_KEY, currentDay);
        PlayerPrefs.Save();
    }

    void LoadDayCounter()
    {
        currentDay = PlayerPrefs.GetInt(DAY_COUNTER_KEY, 1);
        previousTimeOfDay = currentTimeOfDay;
    }

    void UpdateCycle()
    {
        UpdateSunPosition();
        UpdateLighting();
        UpdateSky();
        UpdateStars();
    }

    void UpdateSunPosition()
    {
        if (sunObject == null) return;

        // Sun rises at 4am, peaks at 1pm, sets at 10pm
        // Convert time to angle (0 at 4AM, 180 at 10PM)
        float sunAngle = (currentTimeOfDay - 4f) * 10f; // 10 degrees per hour (18 hour day)

        // Sun path: rises in front of dock (positive Z), arcs overhead, sets on horizon (negative Z)
        // This makes sunrise and sunset visible from the dock area
        float radAngle = sunAngle * Mathf.Deg2Rad;
        float height = Mathf.Sin(radAngle);
        float horizontal = Mathf.Cos(radAngle);

        // Changed: sun travels along Z-axis (forward/back) instead of X-axis (left/right)
        // Sunrise at positive Z (in front), sunset at negative Z (still visible on horizon)
        Vector3 sunPos = new Vector3(0, height * sunDistance, horizontal * sunDistance);
        sunObject.transform.position = sunPos;

        // Sun always looks at world center (for light direction)
        sunObject.transform.LookAt(Vector3.zero);

        // Visibility (below horizon = invisible)
        bool sunVisible = height > -0.1f;
        sunObject.SetActive(sunVisible);

        // Sun size variation (larger near horizon for atmospheric effect)
        if (sunVisible)
        {
            float horizonFactor = 1f + (1f - Mathf.Abs(height)) * 0.5f;
            // Use cached transforms instead of transform.Find every frame
            if (cachedSunSphere != null)
                cachedSunSphere.localScale = Vector3.one * sunSize * horizonFactor;
            if (cachedSunGlow != null)
                cachedSunGlow.localScale = Vector3.one * sunSize * 3f * horizonFactor;
        }
    }

    void UpdateLighting()
    {
        // Calculate sun height (0 = horizon, 1 = noon, negative = below horizon)
        // Light at 4am, dark at 10pm (18 hour day, 10 degrees per hour)
        float sunAngle = (currentTimeOfDay - 4f) * 10f;
        float sunHeight = Mathf.Sin(sunAngle * Mathf.Deg2Rad);

        Color currentSunColor;
        float intensity;

        // UPDATED: Light at 4am, dark at 10pm
        if (currentTimeOfDay >= 4f && currentTimeOfDay < 5.5f)
        {
            // Early sunrise (4am - 5:30am)
            float t = (currentTimeOfDay - 4f) / 1.5f;
            currentSunColor = Color.Lerp(nightColor, sunriseColor, t);
            intensity = Mathf.Lerp(0.05f, 0.4f, t);
        }
        else if (currentTimeOfDay >= 5.5f && currentTimeOfDay < 7f)
        {
            // Sunrise peak to day (5:30am - 7am)
            float t = (currentTimeOfDay - 5.5f) / 1.5f;
            currentSunColor = Color.Lerp(sunriseColor, noonColor, t);
            intensity = Mathf.Lerp(0.4f, 0.6f, t);
        }
        else if (currentTimeOfDay >= 7f && currentTimeOfDay < 19f)
        {
            // Full daytime - bright lighting (7am - 7pm)
            float t = Mathf.InverseLerp(7f, 13f, currentTimeOfDay);
            if (currentTimeOfDay > 13f)
            {
                t = Mathf.InverseLerp(19f, 13f, currentTimeOfDay);
            }
            currentSunColor = Color.Lerp(noonColor, new Color(1f, 0.98f, 0.92f), t); // Slightly brighter at noon
            intensity = 0.6f + t * 0.2f; // Max 0.8 at noon
        }
        else if (currentTimeOfDay >= 19f && currentTimeOfDay < 20.5f)
        {
            // Beginning of sunset (7pm - 8:30pm)
            float t = (currentTimeOfDay - 19f) / 1.5f;
            currentSunColor = Color.Lerp(noonColor, sunsetColor, t);
            intensity = Mathf.Lerp(0.6f, 0.5f, t);
        }
        else if (currentTimeOfDay >= 20.5f && currentTimeOfDay < 21.5f)
        {
            // Sunset peak - dramatic colors (8:30pm - 9:30pm)
            float t = (currentTimeOfDay - 20.5f) / 1f;
            Color enhancedSunset = new Color(1f, 0.45f, 0.25f); // More intense
            currentSunColor = Color.Lerp(sunsetColor, enhancedSunset, Mathf.Sin(t * Mathf.PI));
            intensity = Mathf.Lerp(0.5f, 0.3f, t);
        }
        else if (currentTimeOfDay >= 21.5f && currentTimeOfDay < 22f)
        {
            // Dusk to night (9:30pm - 10pm)
            float t = (currentTimeOfDay - 21.5f) / 0.5f;
            currentSunColor = Color.Lerp(new Color(1f, 0.45f, 0.25f), nightColor, t);
            intensity = Mathf.Lerp(0.3f, 0.05f, t);
        }
        else
        {
            // Night - very dark for dramatic starlight (10pm - 4am)
            currentSunColor = nightColor;
            intensity = 0.02f; // Much darker night
        }

        // Apply to sun light
        if (sunLight != null)
        {
            sunLight.color = currentSunColor;
            sunLight.intensity = intensity;
        }

        // Update sun material color - more vibrant during sunset/sunrise
        if (sunMaterial != null)
        {
            float emissionBoost = 3f;
            if ((currentTimeOfDay >= 5f && currentTimeOfDay < 8.5f) ||
                (currentTimeOfDay >= 16.5f && currentTimeOfDay < 20f))
            {
                emissionBoost = 5f; // Brighter sun during golden hour
            }
            sunMaterial.SetColor("_EmissionColor", currentSunColor * emissionBoost);
        }
        if (glowMaterial != null)
        {
            Color glowColor = new Color(currentSunColor.r, currentSunColor.g * 0.9f, currentSunColor.b * 0.6f, 0.15f);
            glowMaterial.color = glowColor;
            glowMaterial.SetColor("_EmissionColor", currentSunColor * 0.5f);
        }

        // Ambient light
        if (ambientLight != null)
        {
            ambientLight.intensity = 0.1f + Mathf.Max(0, sunHeight) * 0.4f;
            Color ambientColor = Color.Lerp(new Color(0.15f, 0.20f, 0.35f), new Color(0.95f, 0.92f, 0.88f), Mathf.Max(0, sunHeight));
            ambientLight.color = ambientColor;
        }
    }

    void UpdateSky()
    {
        if (skyMaterial == null) return;

        // 4am sunrise, 10pm sunset
        float sunAngle = (currentTimeOfDay - 4f) * 10f;
        float sunHeight = Mathf.Sin(sunAngle * Mathf.Deg2Rad);

        Color skyColor;

        if (sunHeight > 0.3f)
        {
            // Daytime - blue sky
            skyColor = daySkyColor;
        }
        else if (sunHeight > -0.1f)
        {
            // Sunrise/Sunset
            float t = (sunHeight + 0.1f) / 0.4f;
            skyColor = Color.Lerp(nightSkyColor, sunsetSkyColor, t);
            if (sunHeight > 0.1f)
            {
                skyColor = Color.Lerp(sunsetSkyColor, daySkyColor, (sunHeight - 0.1f) / 0.2f);
            }
        }
        else
        {
            // Night
            skyColor = nightSkyColor;
        }

        skyMaterial.color = skyColor;
        skyMaterial.SetColor("_EmissionColor", skyColor * 0.3f);
    }

    void UpdateStars()
    {
        if (starsContainer == null) return;

        // Stars visible at night (4am sunrise, 10pm sunset)
        float sunAngle = (currentTimeOfDay - 4f) * 10f;
        float sunHeight = Mathf.Sin(sunAngle * Mathf.Deg2Rad);

        float starVisibility = Mathf.Clamp01(-sunHeight * 2f + 0.2f);
        starsContainer.SetActive(starVisibility > 0.05f);

        // Twinkle effect - only update every 3rd frame for better performance
        if (starVisibility > 0.05f)
        {
            starUpdateFrameCounter++;
            if (starUpdateFrameCounter < 3)
            {
                return;
            }
            starUpdateFrameCounter = 0;

            foreach (var star in stars)
            {
                if (star == null) continue;
                Renderer r = star.GetComponent<Renderer>();
                if (r != null && r.material != null)
                {
                    float twinkle = 0.5f + Mathf.PerlinNoise(Time.time * 2f + star.GetHashCode() * 0.01f, 0) * 0.5f;
                    Color emission = r.material.GetColor("_EmissionColor");
                    // Normalize color manually (Color doesn't have .normalized)
                    float maxComponent = Mathf.Max(emission.r, Mathf.Max(emission.g, emission.b));
                    Color normalizedEmission = maxComponent > 0 ? emission / maxComponent : Color.white;
                    r.material.SetColor("_EmissionColor", normalizedEmission * twinkle * 2f * starVisibility);
                }
            }
        }
    }

    // Public getters for other systems
    public float GetDaylightIntensity()
    {
        // 4am sunrise, 10pm sunset
        float sunAngle = (currentTimeOfDay - 4f) * 10f;
        float sunHeight = Mathf.Sin(sunAngle * Mathf.Deg2Rad);
        return Mathf.Clamp01(sunHeight + 0.2f);
    }

    public Color GetSunColor()
    {
        if (sunLight != null)
            return sunLight.color;
        return Color.white;
    }

    public Vector3 GetSunDirection()
    {
        if (sunObject != null)
            return sunObject.transform.position.normalized;
        return Vector3.up;
    }

    public float GetCurrentHour()
    {
        return currentTimeOfDay;
    }

    public bool IsNight()
    {
        // Night is 10pm (22) to 4am
        return currentTimeOfDay < 4f || currentTimeOfDay >= 22f;
    }

    public void SetTimeOfDay(float hour)
    {
        currentTimeOfDay = Mathf.Repeat(hour, 24f);
        UpdateCycle();
    }

    public void SetDaySpeed(float secondsPerDay)
    {
        dayLengthInSeconds = secondsPerDay;
        timeSpeed = 24f / dayLengthInSeconds;
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }

    public float GetTimeOfDay()
    {
        return currentTimeOfDay / 24f; // Returns 0-1
    }

    public void SetCurrentDay(int day)
    {
        currentDay = Mathf.Max(1, day);
        SaveDayCounter();
    }

    public void ResetDayCounter()
    {
        currentDay = 1;
        SaveDayCounter();
        Debug.Log("Day counter reset to Day 1");
    }

    // GUI display for current time
    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Initialize style once
        if (!stylesInitialized)
        {
            cachedTimeStyle = new GUIStyle(GUI.skin.label);
            cachedTimeStyle.fontSize = 16;
            cachedTimeStyle.fontStyle = FontStyle.Bold;
            cachedTimeStyle.alignment = TextAnchor.MiddleRight;
            stylesInitialized = true;
        }

        // Time display in corner
        int hours = Mathf.FloorToInt(currentTimeOfDay);
        int minutes = Mathf.FloorToInt((currentTimeOfDay - hours) * 60);
        string ampm = hours >= 12 ? "PM" : "AM";
        int displayHour = hours % 12;
        if (displayHour == 0) displayHour = 12;

        string timeString = $"{displayHour}:{minutes:D2} {ampm}";

        // Color based on time of day (just update color, don't recreate style)
        if (IsNight())
            cachedTimeStyle.normal.textColor = new Color(0.7f, 0.75f, 0.9f);
        else if (currentTimeOfDay < 8f || currentTimeOfDay > 17f)
            cachedTimeStyle.normal.textColor = new Color(1f, 0.8f, 0.5f);
        else
            cachedTimeStyle.normal.textColor = new Color(1f, 0.95f, 0.8f);

        GUI.Label(new Rect(Screen.width - 110, 35, 100, 25), timeString, cachedTimeStyle);
    }
}
