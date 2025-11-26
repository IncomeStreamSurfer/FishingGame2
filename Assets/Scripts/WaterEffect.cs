using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterEffect : MonoBehaviour
{
    public static WaterEffect Instance { get; private set; }

    [Header("Water Color - Crystal Clear Tropical")]
    public Color shallowWaterColor = new Color(0.3f, 0.75f, 0.85f, 0.45f); // Light turquoise, very clear
    public Color deepWaterColor = new Color(0.1f, 0.25f, 0.5f, 0.7f); // Darker blue for deep areas

    [Header("Wave Settings")]
    public float waveSpeed = 1.2f;
    public float waveHeight = 0.08f;

    [Header("Deep Water Danger")]
    public float safeDistanceFromIsland = 15f; // Player can safely be this far from any island
    public float warningDistance = 20f; // Start warning player
    public float drowningDistance = 25f; // Player starts drowning

    private Vector3 startPos;
    private Material waterMat;
    private Transform playerTransform;
    private float lastRippleTime;
    private List<GameObject> activeRipples = new List<GameObject>();

    // Island positions for distance checking
    private List<Vector3> islandPositions = new List<Vector3>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        startPos = transform.position;
        SetupWaterMaterial();
        playerTransform = GameObject.Find("Player")?.transform;

        // Find all islands and store their positions
        Invoke("FindIslands", 0.5f);
    }

    void FindIslands()
    {
        islandPositions.Clear();

        // Main island
        GameObject mainIsland = GameObject.Find("MainIsland");
        if (mainIsland != null)
            islandPositions.Add(mainIsland.transform.position);

        // Clothing shop island
        GameObject shopIsland = GameObject.Find("ClothingShopIsland");
        if (shopIsland != null)
            islandPositions.Add(shopIsland.transform.position);

        // Find all scattered islands
        for (int i = 0; i < 10; i++)
        {
            GameObject island = GameObject.Find("Island_" + i);
            if (island != null)
                islandPositions.Add(island.transform.position);
        }

        Debug.Log($"WaterEffect found {islandPositions.Count} islands for distance checking");
    }

    void SetupWaterMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        waterMat = new Material(Shader.Find("Standard"));

        // Setup transparency for crystal clear water
        waterMat.SetFloat("_Mode", 3);
        waterMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        waterMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        waterMat.SetInt("_ZWrite", 0);
        waterMat.DisableKeyword("_ALPHATEST_ON");
        waterMat.EnableKeyword("_ALPHABLEND_ON");
        waterMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        waterMat.renderQueue = 3000;

        // Crystal clear tropical water
        waterMat.color = shallowWaterColor;
        waterMat.SetFloat("_Metallic", 0.05f);
        waterMat.SetFloat("_Glossiness", 0.9f); // Smooth reflective surface

        rend.material = waterMat;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Smooth flowing wave motion
        float time = Time.time;

        // Multiple overlapping waves for natural flow
        float wave1 = Mathf.Sin(time * waveSpeed) * waveHeight;
        float wave2 = Mathf.Sin(time * waveSpeed * 0.7f + 2f) * waveHeight * 0.6f;
        float wave3 = Mathf.Sin(time * waveSpeed * 1.4f + 4f) * waveHeight * 0.3f;

        float combinedWave = wave1 + wave2 + wave3;
        transform.position = new Vector3(startPos.x, startPos.y + combinedWave, startPos.z);

        // Very subtle tilt for wave rolling effect
        float tiltX = Mathf.Sin(time * waveSpeed * 0.3f) * 0.15f;
        float tiltZ = Mathf.Cos(time * waveSpeed * 0.25f) * 0.1f;
        transform.rotation = Quaternion.Euler(tiltX, 0, tiltZ);

        // Check player water depth and create ripples
        if (playerTransform != null)
        {
            CheckPlayerInWater();
            CheckDeepWaterDanger();
        }

        // Clean up destroyed ripples
        activeRipples.RemoveAll(r => r == null);
    }

    float GetDistanceToNearestIsland(Vector3 position)
    {
        float minDist = float.MaxValue;
        foreach (Vector3 islandPos in islandPositions)
        {
            float dist = Vector3.Distance(new Vector3(position.x, 0, position.z),
                                         new Vector3(islandPos.x, 0, islandPos.z));
            if (dist < minDist)
                minDist = dist;
        }
        return minDist;
    }

    void CheckDeepWaterDanger()
    {
        if (playerTransform == null || islandPositions.Count == 0) return;

        // Only check if player is in water (below island level)
        if (playerTransform.position.y > 1.0f) return;

        float distToIsland = GetDistanceToNearestIsland(playerTransform.position);

        // Update water color based on depth (distance from islands)
        float depthFactor = Mathf.Clamp01((distToIsland - safeDistanceFromIsland) / (drowningDistance - safeDistanceFromIsland));
        waterMat.color = Color.Lerp(shallowWaterColor, deepWaterColor, depthFactor * 0.5f);

        // Warning when getting far
        if (distToIsland > warningDistance && distToIsland < drowningDistance)
        {
            // Show warning (pulse effect on water)
            float pulse = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
            Color warningColor = Color.Lerp(shallowWaterColor, new Color(0.2f, 0.3f, 0.6f, 0.6f), pulse * 0.3f);
            waterMat.color = warningColor;
        }

        // Drowning when too far
        if (distToIsland > drowningDistance)
        {
            // Trigger drowning in PlayerController
            PlayerController player = playerTransform.GetComponent<PlayerController>();
            if (player != null && !player.IsDead())
            {
                // Force the player to drown
                Debug.Log("TOO FAR FROM SHORE! The water is too deep!");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("THE WATER IS TOO DEEP!", Color.red);
                }

                // Trigger death by moving player below water death height
                playerTransform.position = new Vector3(playerTransform.position.x, -0.5f, playerTransform.position.z);
            }
        }
    }

    void CheckPlayerInWater()
    {
        if (playerTransform == null) return;

        // Check if player is at water level and moving
        float waterLevel = transform.position.y;
        if (playerTransform.position.y < waterLevel + 0.5f && playerTransform.position.y > waterLevel - 1f)
        {
            // Player is in/near water
            float moveSpeed = 0f;
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                moveSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
            }

            // Create ripples if moving and enough time has passed
            if (moveSpeed > 0.5f && Time.time - lastRippleTime > 0.15f)
            {
                CreateFootRipple(playerTransform.position);
                lastRippleTime = Time.time;
            }
        }
    }

    void CreateFootRipple(Vector3 playerPos)
    {
        Vector3 ripplePos = new Vector3(playerPos.x, transform.position.y + 0.05f, playerPos.z);
        StartCoroutine(FootRippleEffect(ripplePos));
    }

    IEnumerator FootRippleEffect(Vector3 pos)
    {
        // Create multiple expanding rings for realistic ripple
        for (int ring = 0; ring < 2; ring++)
        {
            GameObject ripple = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ripple.name = "FootRipple";
            ripple.transform.position = pos;
            ripple.transform.localScale = new Vector3(0.2f, 0.01f, 0.2f);
            Destroy(ripple.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3010;
            mat.color = new Color(1f, 1f, 1f, 0.35f);
            ripple.GetComponent<Renderer>().material = mat;

            activeRipples.Add(ripple);
            StartCoroutine(AnimateRipple(ripple, mat, ring * 0.1f));

            yield return new WaitForSeconds(0.08f);
        }
    }

    IEnumerator AnimateRipple(GameObject ripple, Material mat, float delay)
    {
        yield return new WaitForSeconds(delay);

        float t = 0;
        float duration = 0.8f;

        while (t < duration && ripple != null)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            float scale = 0.2f + progress * 1.5f;
            ripple.transform.localScale = new Vector3(scale, 0.01f, scale);

            float alpha = Mathf.Max(0, 0.35f * (1 - progress));
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

            yield return null;
        }

        if (ripple != null)
        {
            Destroy(ripple);
        }
    }

    // Public method to create splash at position (for fishing)
    public void CreateSplash(Vector3 worldPos)
    {
        StartCoroutine(SplashEffect(worldPos));
    }

    IEnumerator SplashEffect(Vector3 pos)
    {
        // Create multiple expanding rings for splash
        for (int i = 0; i < 3; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "SplashRing";
            ring.transform.position = new Vector3(pos.x, transform.position.y + 0.05f, pos.z);
            ring.transform.localScale = new Vector3(0.3f + i * 0.2f, 0.02f, 0.3f + i * 0.2f);
            Destroy(ring.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3010;
            mat.color = new Color(1f, 1f, 1f, 0.6f);
            ring.GetComponent<Renderer>().material = mat;

            StartCoroutine(AnimateSplashRing(ring, mat));
            yield return new WaitForSeconds(0.05f);
        }

        // Water droplets going up
        for (int i = 0; i < 5; i++)
        {
            GameObject droplet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droplet.name = "WaterDroplet";
            droplet.transform.position = pos;
            droplet.transform.localScale = Vector3.one * 0.08f;
            Destroy(droplet.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.7f, 0.9f, 1f, 0.8f);
            mat.SetFloat("_Glossiness", 0.9f);
            droplet.GetComponent<Renderer>().material = mat;

            StartCoroutine(AnimateDroplet(droplet, mat));
        }
    }

    IEnumerator AnimateSplashRing(GameObject ring, Material mat)
    {
        float t = 0;
        Vector3 startScale = ring.transform.localScale;

        while (t < 1.2f)
        {
            t += Time.deltaTime;
            float scale = startScale.x + t * 2.5f;
            ring.transform.localScale = new Vector3(scale, 0.02f, scale);

            float alpha = Mathf.Max(0, 0.6f - t * 0.5f);
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

            yield return null;
        }

        Destroy(ring);
    }

    IEnumerator AnimateDroplet(GameObject droplet, Material mat)
    {
        Vector3 velocity = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(2f, 4f),
            Random.Range(-1f, 1f)
        );

        float t = 0;
        while (t < 1f && droplet != null)
        {
            t += Time.deltaTime;
            velocity.y -= 9.8f * Time.deltaTime; // Gravity
            droplet.transform.position += velocity * Time.deltaTime;

            // Fade out
            if (t > 0.5f)
            {
                Color c = mat.color;
                c.a = Mathf.Max(0, 0.8f - (t - 0.5f) * 1.6f);
                mat.color = c;
            }

            yield return null;
        }

        if (droplet != null)
        {
            Destroy(droplet);
        }
    }
}
