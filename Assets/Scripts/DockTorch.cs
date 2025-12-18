using UnityEngine;

/// <summary>
/// Victorian street lamp at the end of the dock that turns on at night.
/// Features gas-lamp style flickering and ornate design.
/// </summary>
public class DockTorch : MonoBehaviour
{
    [Header("Lamp Components (Auto-generated)")]
    private GameObject lampPost;
    private GameObject lampHousing;
    private GameObject flame;
    private Light torchLight;
    private GameObject[] glassPanels;

    [Header("Lighting Settings")]
    [Tooltip("Warm gas-lamp light color")]
    public Color lightColor = new Color(1.0f, 0.85f, 0.5f); // Warmer, more yellow gas-light

    [Tooltip("Light range in units")]
    public float lightRange = 30f;

    [Tooltip("Base light intensity")]
    public float baseIntensity = 5.0f;

    [Header("Flicker Settings")]
    [Tooltip("Intensity variation amount")]
    public float flickerAmount = 0.15f; // Subtler for gas lamp

    [Tooltip("Speed of flicker effect")]
    public float flickerSpeed = 3f; // Slower, steadier

    [Tooltip("Flame scale pulse amount")]
    public float flamePulseAmount = 0.08f; // Subtler

    [Header("Time Settings")]
    [Tooltip("Hour when lamp turns on")]
    public float nightStartHour = 18f;

    [Tooltip("Hour when lamp turns off")]
    public float nightEndHour = 6f;

    // Animation state
    private Vector3 baseFlameScale;
    private float flickerOffset;
    private bool isLit = false;
    private Material glassMat;

    void Start()
    {
        // Create the Victorian lamp visual
        CreateVictorianLamp();

        // Random flicker offset so multiple lamps don't flicker in sync
        flickerOffset = Random.Range(0f, 100f);

        // Store base flame scale for animation
        if (flame != null)
        {
            baseFlameScale = flame.transform.localScale;
        }

        // Initial state check
        UpdateTorchState();
    }

    void CreateVictorianLamp()
    {
        // Materials
        Material ironMat = new Material(Shader.Find("Standard"));
        ironMat.color = new Color(0.15f, 0.15f, 0.18f); // Dark wrought iron
        ironMat.SetFloat("_Metallic", 0.9f);
        ironMat.SetFloat("_Glossiness", 0.3f);

        Material brassMat = new Material(Shader.Find("Standard"));
        brassMat.color = new Color(0.7f, 0.55f, 0.2f); // Brass accents
        brassMat.SetFloat("_Metallic", 0.95f);
        brassMat.SetFloat("_Glossiness", 0.7f);

        glassMat = new Material(Shader.Find("Standard"));
        glassMat.color = new Color(0.9f, 0.95f, 1f, 0.3f); // Frosted glass
        glassMat.SetFloat("_Mode", 3); // Transparent
        glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glassMat.EnableKeyword("_ALPHABLEND_ON");
        glassMat.renderQueue = 3000;

        float postHeight = 3.5f;
        float lampY = postHeight + 0.3f;

        // === MAIN POST (Tapered iron column) ===
        lampPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lampPost.name = "LampPost";
        lampPost.transform.SetParent(transform);
        lampPost.transform.localPosition = new Vector3(0, postHeight / 2f, 0);
        lampPost.transform.localScale = new Vector3(0.15f, postHeight / 2f, 0.15f);
        lampPost.GetComponent<Renderer>().material = ironMat;
        Destroy(lampPost.GetComponent<Collider>());

        // === BASE (Ornate foundation) ===
        GameObject baseBottom = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseBottom.name = "LampBase";
        baseBottom.transform.SetParent(transform);
        baseBottom.transform.localPosition = new Vector3(0, 0.1f, 0);
        baseBottom.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        baseBottom.GetComponent<Renderer>().material = ironMat;
        Destroy(baseBottom.GetComponent<Collider>());

        GameObject baseMid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseMid.name = "LampBaseMid";
        baseMid.transform.SetParent(transform);
        baseMid.transform.localPosition = new Vector3(0, 0.25f, 0);
        baseMid.transform.localScale = new Vector3(0.35f, 0.08f, 0.35f);
        baseMid.GetComponent<Renderer>().material = ironMat;
        Destroy(baseMid.GetComponent<Collider>());

        // === DECORATIVE COLLAR (where post meets lamp) ===
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        collar.name = "Collar";
        collar.transform.SetParent(transform);
        collar.transform.localPosition = new Vector3(0, lampY - 0.15f, 0);
        collar.transform.localScale = new Vector3(0.25f, 0.08f, 0.25f);
        collar.GetComponent<Renderer>().material = brassMat;
        Destroy(collar.GetComponent<Collider>());

        // === LAMP HOUSING (Victorian lantern style) ===
        lampHousing = new GameObject("LampHousing");
        lampHousing.transform.SetParent(transform);
        lampHousing.transform.localPosition = new Vector3(0, lampY + 0.4f, 0);

        // Bottom plate of housing
        GameObject housingBottom = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        housingBottom.name = "HousingBottom";
        housingBottom.transform.SetParent(lampHousing.transform);
        housingBottom.transform.localPosition = new Vector3(0, -0.3f, 0);
        housingBottom.transform.localScale = new Vector3(0.45f, 0.03f, 0.45f);
        housingBottom.GetComponent<Renderer>().material = ironMat;
        Destroy(housingBottom.GetComponent<Collider>());

        // Top cap (dome/pyramid shape approximation)
        GameObject housingTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        housingTop.name = "HousingTop";
        housingTop.transform.SetParent(lampHousing.transform);
        housingTop.transform.localPosition = new Vector3(0, 0.35f, 0);
        housingTop.transform.localScale = new Vector3(0.5f, 0.08f, 0.5f);
        housingTop.GetComponent<Renderer>().material = ironMat;
        Destroy(housingTop.GetComponent<Collider>());

        // Finial on top
        GameObject finial = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        finial.name = "Finial";
        finial.transform.SetParent(lampHousing.transform);
        finial.transform.localPosition = new Vector3(0, 0.5f, 0);
        finial.transform.localScale = new Vector3(0.12f, 0.15f, 0.12f);
        finial.GetComponent<Renderer>().material = brassMat;
        Destroy(finial.GetComponent<Collider>());

        // === GLASS PANELS (4 sides) ===
        glassPanels = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "GlassPanel_" + i;
            panel.transform.SetParent(lampHousing.transform);

            float angle = i * 90f;
            float radius = 0.18f;
            float px = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            float pz = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;

            panel.transform.localPosition = new Vector3(px, 0, pz);
            panel.transform.localRotation = Quaternion.Euler(0, angle, 0);
            panel.transform.localScale = new Vector3(0.3f, 0.5f, 0.02f);
            panel.GetComponent<Renderer>().material = glassMat;
            Destroy(panel.GetComponent<Collider>());
            glassPanels[i] = panel;
        }

        // === CORNER POSTS (4 vertical iron bars) ===
        for (int i = 0; i < 4; i++)
        {
            GameObject cornerPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cornerPost.name = "CornerPost_" + i;
            cornerPost.transform.SetParent(lampHousing.transform);

            float angle = i * 90f + 45f;
            float radius = 0.22f;
            float px = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            float pz = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;

            cornerPost.transform.localPosition = new Vector3(px, 0, pz);
            cornerPost.transform.localScale = new Vector3(0.04f, 0.35f, 0.04f);
            cornerPost.GetComponent<Renderer>().material = ironMat;
            Destroy(cornerPost.GetComponent<Collider>());
        }

        // === FLAME (Gas mantle inside) ===
        flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "GasFlame";
        flame.transform.SetParent(lampHousing.transform);
        flame.transform.localPosition = Vector3.zero;
        flame.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);

        Material flameMat = new Material(Shader.Find("Standard"));
        flameMat.color = new Color(1f, 0.9f, 0.6f);
        flameMat.EnableKeyword("_EMISSION");
        flameMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.4f) * 4f);
        flame.GetComponent<Renderer>().material = flameMat;
        Destroy(flame.GetComponent<Collider>());

        // === POINT LIGHT ===
        GameObject lightObj = new GameObject("LampLight");
        lightObj.transform.SetParent(lampHousing.transform);
        lightObj.transform.localPosition = Vector3.zero;

        torchLight = lightObj.AddComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.color = lightColor;
        torchLight.range = lightRange;
        torchLight.intensity = baseIntensity;
        torchLight.shadows = LightShadows.Soft;
        torchLight.shadowStrength = 0.6f;

        // Start disabled (will be enabled based on time)
        flame.SetActive(false);
        torchLight.enabled = false;
        SetGlassGlow(false);
    }

    void SetGlassGlow(bool glowing)
    {
        if (glassMat == null) return;

        if (glowing)
        {
            glassMat.EnableKeyword("_EMISSION");
            glassMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 0.5f);
        }
        else
        {
            glassMat.DisableKeyword("_EMISSION");
            glassMat.SetColor("_EmissionColor", Color.black);
        }
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check if torch should be lit based on time
        UpdateTorchState();

        // Animate flame flicker when lit
        if (isLit)
        {
            AnimateFlicker();
        }
    }

    void UpdateTorchState()
    {
        if (DayNightCycle.Instance == null) return;

        float currentHour = DayNightCycle.Instance.GetCurrentHour();

        // Lamp is ON during night (after nightStartHour or before nightEndHour)
        bool shouldBeLit = currentHour >= nightStartHour || currentHour < nightEndHour;

        if (shouldBeLit != isLit)
        {
            isLit = shouldBeLit;

            // Enable/disable flame, light, and glass glow together
            if (flame != null)
                flame.SetActive(isLit);

            if (torchLight != null)
                torchLight.enabled = isLit;

            SetGlassGlow(isLit);
        }
    }

    void AnimateFlicker()
    {
        if (flame == null || torchLight == null) return;

        float time = Time.time * flickerSpeed + flickerOffset;

        // Perlin noise for smooth, natural flickering
        float flicker1 = Mathf.PerlinNoise(time, 0f);
        float flicker2 = Mathf.PerlinNoise(time * 1.3f, 10f);
        float flicker3 = Mathf.PerlinNoise(time * 0.7f, 20f);

        // Combine multiple noise frequencies for realistic flame movement
        float flickerValue = (flicker1 + flicker2 * 0.5f + flicker3 * 0.3f) / 1.8f;

        // Vary light intensity
        float intensityVariation = 1f + (flickerValue - 0.5f) * flickerAmount;
        torchLight.intensity = baseIntensity * intensityVariation;

        // Pulse flame scale
        float scaleVariation = 1f + (flickerValue - 0.5f) * flamePulseAmount;
        flame.transform.localScale = new Vector3(
            baseFlameScale.x * scaleVariation,
            baseFlameScale.y * scaleVariation,
            baseFlameScale.z * scaleVariation
        );

        // Slight color temperature variation (warmer/cooler)
        float colorVariation = 0.95f + flickerValue * 0.1f;
        torchLight.color = new Color(
            lightColor.r * colorVariation,
            lightColor.g * colorVariation,
            lightColor.b * Mathf.Min(colorVariation * 1.2f, 1f)
        );

        // Add very subtle rotation to flame for extra movement
        float rotationAngle = Mathf.Sin(time * 0.5f) * 5f;
        flame.transform.localRotation = Quaternion.Euler(0, rotationAngle, 0);
    }

    // Public method to manually control torch (if needed)
    public void SetLit(bool lit)
    {
        isLit = lit;
        if (flame != null)
            flame.SetActive(lit);
        if (torchLight != null)
            torchLight.enabled = lit;
    }

    // Debug visualization in editor
    void OnDrawGizmosSelected()
    {
        if (torchLight != null)
        {
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(torchLight.transform.position, lightRange);
        }
    }
}
