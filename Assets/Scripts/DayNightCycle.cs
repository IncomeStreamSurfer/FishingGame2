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

    [Header("Lighting Colors - DRASTICALLY REDUCED")]
    public Color dawnColor = new Color(0.70f, 0.40f, 0.25f);      // Darker warm dawn
    public Color noonColor = new Color(0.75f, 0.70f, 0.60f);      // Much darker noon - no more white!
    public Color duskColor = new Color(0.70f, 0.35f, 0.20f);      // Darker orange dusk
    public Color nightColor = new Color(0.05f, 0.08f, 0.18f);     // Very deep blue night

    [Header("Sky Colors - More Vibrant")]
    public Color daySkyColor = new Color(0.35f, 0.60f, 0.90f);    // Vibrant blue sky
    public Color sunsetSkyColor = new Color(0.95f, 0.50f, 0.40f); // Rich sunset
    public Color nightSkyColor = new Color(0.03f, 0.03f, 0.12f);  // Deep night

    // Time tracking (0-24 hours)
    private float currentTimeOfDay = 8f; // Start at 8 AM
    private float timeSpeed;

    // Sun components
    private GameObject sunObject;
    private Light sunLight;
    private GameObject sunGlow;
    private Material sunMaterial;
    private Material glowMaterial;

    // Moon components
    private GameObject moonObject;
    private Material moonMaterial;

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

        CreateSun();
        CreateMoon();
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
        glowMaterial.renderQueue = 3000;
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
    }

    void CreateMoon()
    {
        moonObject = new GameObject("Moon");
        moonObject.transform.SetParent(transform);

        GameObject moonSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moonSphere.name = "MoonSphere";
        moonSphere.transform.SetParent(moonObject.transform);
        moonSphere.transform.localPosition = Vector3.zero;
        moonSphere.transform.localScale = Vector3.one * sunSize * 0.6f;
        Destroy(moonSphere.GetComponent<Collider>());

        moonMaterial = new Material(Shader.Find("Standard"));
        moonMaterial.color = new Color(0.9f, 0.9f, 0.95f);
        moonMaterial.EnableKeyword("_EMISSION");
        moonMaterial.SetColor("_EmissionColor", new Color(0.7f, 0.75f, 0.9f) * 0.5f);
        moonMaterial.SetFloat("_Metallic", 0f);
        moonMaterial.SetFloat("_Glossiness", 0.2f);
        moonSphere.GetComponent<Renderer>().material = moonMaterial;

        // Moon glow
        GameObject moonGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moonGlow.name = "MoonGlow";
        moonGlow.transform.SetParent(moonObject.transform);
        moonGlow.transform.localPosition = Vector3.zero;
        moonGlow.transform.localScale = Vector3.one * sunSize * 1.5f;
        Destroy(moonGlow.GetComponent<Collider>());

        Material moonGlowMat = new Material(Shader.Find("Standard"));
        moonGlowMat.SetFloat("_Mode", 3);
        moonGlowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        moonGlowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        moonGlowMat.EnableKeyword("_ALPHABLEND_ON");
        moonGlowMat.renderQueue = 3000;
        moonGlowMat.color = new Color(0.7f, 0.75f, 0.9f, 0.1f);
        moonGlow.GetComponent<Renderer>().material = moonGlowMat;
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

        // Advance time
        currentTimeOfDay += timeSpeed * Time.deltaTime;
        if (currentTimeOfDay >= 24f)
            currentTimeOfDay -= 24f;

        UpdateCycle();
    }

    void UpdateCycle()
    {
        UpdateSunPosition();
        UpdateMoonPosition();
        UpdateLighting();
        UpdateSky();
        UpdateStars();
    }

    void UpdateSunPosition()
    {
        if (sunObject == null) return;

        // Sun rises at 6, peaks at 12, sets at 18
        // Convert time to angle (0 at 6AM, 180 at 6PM)
        float sunAngle = (currentTimeOfDay - 6f) * 15f; // 15 degrees per hour

        // Sun path is an arc from east to west
        float radAngle = sunAngle * Mathf.Deg2Rad;
        float height = Mathf.Sin(radAngle);
        float horizontal = Mathf.Cos(radAngle);

        Vector3 sunPos = new Vector3(horizontal * sunDistance, height * sunDistance, 0);
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

    void UpdateMoonPosition()
    {
        if (moonObject == null) return;

        // Moon is opposite to sun
        float moonAngle = (currentTimeOfDay - 6f) * 15f + 180f;
        float radAngle = moonAngle * Mathf.Deg2Rad;
        float height = Mathf.Sin(radAngle);
        float horizontal = Mathf.Cos(radAngle);

        Vector3 moonPos = new Vector3(horizontal * sunDistance * 0.9f, height * sunDistance * 0.9f, 0);
        moonObject.transform.position = moonPos;
        moonObject.transform.LookAt(Vector3.zero);

        bool moonVisible = height > -0.1f;
        moonObject.SetActive(moonVisible);
    }

    void UpdateLighting()
    {
        // Calculate sun height (0 = horizon, 1 = noon, negative = below horizon)
        float sunAngle = (currentTimeOfDay - 6f) * 15f;
        float sunHeight = Mathf.Sin(sunAngle * Mathf.Deg2Rad);

        Color currentSunColor;
        float intensity;

        if (sunHeight > 0.3f)
        {
            // Daytime - DRASTICALLY REDUCED brightness
            currentSunColor = Color.Lerp(dawnColor, noonColor, (sunHeight - 0.3f) / 0.7f);
            intensity = 0.3f + sunHeight * 0.2f;  // Much lower - max ~0.5
        }
        else if (sunHeight > 0f)
        {
            // Dawn/Dusk
            float t = sunHeight / 0.3f;
            bool isMorning = currentTimeOfDay < 12f;
            currentSunColor = isMorning ? Color.Lerp(nightColor, dawnColor, t) : Color.Lerp(nightColor, duskColor, t);
            intensity = 0.1f + t * 0.3f;  // Lower dawn/dusk
        }
        else
        {
            // Night
            currentSunColor = nightColor;
            intensity = 0.05f;  // Very dark night
        }

        // Apply to sun light
        if (sunLight != null)
        {
            sunLight.color = currentSunColor;
            sunLight.intensity = intensity;
        }

        // Update sun material color
        if (sunMaterial != null)
        {
            sunMaterial.SetColor("_EmissionColor", currentSunColor * 3f);
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
            ambientLight.intensity = 0.1f + Mathf.Max(0, sunHeight) * 0.3f;
            ambientLight.color = Color.Lerp(new Color(0.2f, 0.25f, 0.4f), Color.white, Mathf.Max(0, sunHeight));
        }
    }

    void UpdateSky()
    {
        if (skyMaterial == null) return;

        float sunAngle = (currentTimeOfDay - 6f) * 15f;
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

        // Stars visible at night
        float sunAngle = (currentTimeOfDay - 6f) * 15f;
        float sunHeight = Mathf.Sin(sunAngle * Mathf.Deg2Rad);

        float starVisibility = Mathf.Clamp01(-sunHeight * 2f + 0.2f);
        starsContainer.SetActive(starVisibility > 0.05f);

        // Twinkle effect
        if (starVisibility > 0.05f)
        {
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
        float sunAngle = (currentTimeOfDay - 6f) * 15f;
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
        return currentTimeOfDay < 6f || currentTimeOfDay > 18f;
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
