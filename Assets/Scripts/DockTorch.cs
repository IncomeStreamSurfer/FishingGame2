using UnityEngine;

/// <summary>
/// Standing torch at the end of the dock that turns on at night.
/// Features flickering flame animation and dynamic lighting.
/// </summary>
public class DockTorch : MonoBehaviour
{
    [Header("Torch Components (Auto-generated)")]
    private GameObject torchPost;
    private GameObject metalBracket;
    private GameObject flame;
    private Light torchLight;

    [Header("Lighting Settings")]
    [Tooltip("Warm orange torch light color")]
    public Color lightColor = new Color(1.0f, 0.6f, 0.2f);

    [Tooltip("Light range in units")]
    public float lightRange = 12f;

    [Tooltip("Base light intensity")]
    public float baseIntensity = 1.5f;

    [Header("Flicker Settings")]
    [Tooltip("Intensity variation amount")]
    public float flickerAmount = 0.2f;

    [Tooltip("Speed of flicker effect")]
    public float flickerSpeed = 5f;

    [Tooltip("Flame scale pulse amount")]
    public float flamePulseAmount = 0.15f;

    [Header("Time Settings")]
    [Tooltip("Hour when torch turns on (6 PM)")]
    public float nightStartHour = 18f;

    [Tooltip("Hour when torch turns off (6 AM)")]
    public float nightEndHour = 6f;

    // Animation state
    private Vector3 baseFlameScale;
    private float flickerOffset;
    private bool isLit = false;

    void Start()
    {
        // Create the torch visual
        CreateTorchModel();

        // Random flicker offset so multiple torches don't flicker in sync
        flickerOffset = Random.Range(0f, 100f);

        // Store base flame scale for animation
        if (flame != null)
        {
            baseFlameScale = flame.transform.localScale;
        }

        // Initial state check
        UpdateTorchState();
    }

    void CreateTorchModel()
    {
        // === WOODEN POST ===
        torchPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        torchPost.name = "TorchPost";
        torchPost.transform.SetParent(transform);
        torchPost.transform.localPosition = Vector3.zero;
        torchPost.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f); // Tall post

        Material postMat = new Material(Shader.Find("Standard"));
        postMat.color = new Color(0.25f, 0.15f, 0.08f); // Dark brown wood
        torchPost.GetComponent<Renderer>().material = postMat;
        Destroy(torchPost.GetComponent<Collider>()); // Visual only

        // === METAL BRACKET ===
        metalBracket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        metalBracket.name = "MetalBracket";
        metalBracket.transform.SetParent(transform);
        metalBracket.transform.localPosition = new Vector3(0, 1.3f, 0);
        metalBracket.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);

        Material bracketMat = new Material(Shader.Find("Standard"));
        bracketMat.color = new Color(0.2f, 0.2f, 0.2f); // Dark metal
        bracketMat.SetFloat("_Metallic", 0.8f);
        bracketMat.SetFloat("_Glossiness", 0.6f);
        metalBracket.GetComponent<Renderer>().material = bracketMat;
        Destroy(metalBracket.GetComponent<Collider>()); // Visual only

        // === FLAME (Emissive Sphere) ===
        flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Flame";
        flame.transform.SetParent(transform);
        flame.transform.localPosition = new Vector3(0, 1.5f, 0);
        flame.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);

        Material flameMat = new Material(Shader.Find("Standard"));
        flameMat.color = new Color(1f, 0.6f, 0.2f); // Orange flame color
        flameMat.EnableKeyword("_EMISSION");
        flameMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 3f); // Bright emission
        flame.GetComponent<Renderer>().material = flameMat;
        Destroy(flame.GetComponent<Collider>()); // Visual only

        // === POINT LIGHT ===
        GameObject lightObj = new GameObject("TorchLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = new Vector3(0, 1.5f, 0);

        torchLight = lightObj.AddComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.color = lightColor;
        torchLight.range = lightRange;
        torchLight.intensity = baseIntensity;
        torchLight.shadows = LightShadows.Soft;
        torchLight.shadowStrength = 0.7f;

        // Start disabled (will be enabled based on time)
        flame.SetActive(false);
        torchLight.enabled = false;
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

        // Torch is ON during night (after 6 PM or before 6 AM)
        bool shouldBeLit = currentHour >= nightStartHour || currentHour < nightEndHour;

        if (shouldBeLit != isLit)
        {
            isLit = shouldBeLit;

            // Enable/disable flame and light together
            if (flame != null)
                flame.SetActive(isLit);

            if (torchLight != null)
                torchLight.enabled = isLit;
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
