using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterEffect : MonoBehaviour
{
    public static WaterEffect Instance { get; private set; }

    [Header("Water Color Gradient - Multi-tone ocean")]
    // Multiple water colors for depth/distance gradient
    public Color shorelineColor = new Color(0.15f, 0.55f, 0.60f, 0.55f);     // Turquoise near shore
    public Color shallowWaterColor = new Color(0.08f, 0.40f, 0.58f, 0.70f);  // Medium blue-green
    public Color midWaterColor = new Color(0.05f, 0.30f, 0.55f, 0.78f);      // Blue transition
    public Color deepWaterColor = new Color(0.02f, 0.15f, 0.45f, 0.88f);     // Deep ocean blue
    public Color abyssColor = new Color(0.01f, 0.08f, 0.25f, 0.95f);         // Very deep/dark
    public Color shimmerColor = new Color(0.20f, 0.55f, 0.70f, 0.18f);       // Cyan shimmer

    [Header("Wave Settings")]
    public float waveSpeed = 1.2f;
    public float waveHeight = 0.08f;

    [Header("Deep Water Danger")]
    public float safeDistanceFromIsland = 15f; // Player can safely be this far from any island
    public float warningDistance = 20f; // Start warning player
    public float drowningDistance = 25f; // Player starts drowning

    [Header("Water Particles")]
    public int maxSparkles = 30;
    public int maxFoamParticles = 20;

    private Vector3 startPos;
    private Material waterMat;
    private Transform playerTransform;
    private float lastRippleTime;
    private List<GameObject> activeRipples = new List<GameObject>();

    // Island positions for distance checking
    private List<Vector3> islandPositions = new List<Vector3>();

    // Water particle effects
    private List<GameObject> sparkleParticles = new List<GameObject>();
    private List<GameObject> foamParticles = new List<GameObject>();
    private GameObject reflectionPlane;
    private float lastSparkleTime;
    private float lastFoamTime;

    // Caustic light projection
    private List<GameObject> causticLights = new List<GameObject>();

    // Sun reflection
    private GameObject sunReflection;

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

        // Initialize water particle effects
        Invoke("InitializeWaterEffects", 0.6f);
    }

    void InitializeWaterEffects()
    {
        // Create sun reflection on water
        CreateSunReflection();

        // Create caustic light effects
        CreateCausticLights();

        // Create shimmering ripple effect
        CreateShimmerRipples();

        // Create animated wave ridges
        CreateAnimatedWaves();

        // Start spawning sparkles and foam
        StartCoroutine(SpawnWaterSparkles());
        StartCoroutine(SpawnFoamParticles());
        StartCoroutine(AnimateShimmerRipples());
    }

    // Animated wave objects
    private List<GameObject> waveRidges = new List<GameObject>();

    void CreateAnimatedWaves()
    {
        // Create multiple wave ridges that travel across the water
        int waveCount = 12;

        Material waveMat = new Material(Shader.Find("Standard"));
        waveMat.SetFloat("_Mode", 3);
        waveMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        waveMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        waveMat.SetInt("_ZWrite", 0);
        waveMat.EnableKeyword("_ALPHABLEND_ON");
        waveMat.renderQueue = 3020;
        waveMat.color = new Color(0.7f, 0.9f, 0.95f, 0.3f);
        waveMat.SetFloat("_Glossiness", 0.95f);

        for (int i = 0; i < waveCount; i++)
        {
            GameObject wave = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wave.name = "WaveRidge_" + i;
            wave.transform.SetParent(transform);

            // Spread waves across the water
            float startZ = -60f + i * 10f;
            wave.transform.localPosition = new Vector3(0, 0.12f, startZ);
            wave.transform.localScale = new Vector3(100f, 0.08f, 1.5f);

            Destroy(wave.GetComponent<Collider>());
            wave.GetComponent<Renderer>().material = new Material(waveMat);

            waveRidges.Add(wave);
        }

        // Start wave animation
        StartCoroutine(AnimateWaveRidges());
    }

    IEnumerator AnimateWaveRidges()
    {
        float waveSpacing = 10f;
        float waveSpeedMultiplier = 3f;

        while (true)
        {
            yield return null;
            if (!MainMenu.GameStarted) continue;

            float time = Time.time;

            for (int i = 0; i < waveRidges.Count; i++)
            {
                if (waveRidges[i] == null) continue;

                // Move wave forward
                Vector3 pos = waveRidges[i].transform.localPosition;
                pos.z += waveSpeedMultiplier * Time.deltaTime;

                // Wrap around when wave goes too far
                if (pos.z > 70f)
                {
                    pos.z = -70f;
                }

                // Vertical wave motion
                float waveY = 0.1f + Mathf.Sin(time * 2f + i * 0.5f) * 0.04f;
                pos.y = waveY;

                waveRidges[i].transform.localPosition = pos;

                // Fade waves near edges
                float distFromCenter = Mathf.Abs(pos.z);
                float alpha = 0.3f * (1f - distFromCenter / 80f);

                Renderer r = waveRidges[i].GetComponent<Renderer>();
                if (r != null)
                {
                    Color c = r.material.color;
                    c.a = Mathf.Max(0.05f, alpha);
                    r.material.color = c;
                }

                // Scale variation for more natural look
                float scaleX = 100f + Mathf.Sin(time + i) * 10f;
                float scaleZ = 1.5f + Mathf.Sin(time * 1.5f + i * 0.3f) * 0.5f;
                waveRidges[i].transform.localScale = new Vector3(scaleX, 0.08f, scaleZ);
            }
        }
    }

    // Shimmer ripple objects
    private List<GameObject> shimmerRipples = new List<GameObject>();
    private List<Material> shimmerMaterials = new List<Material>();

    void CreateShimmerRipples()
    {
        // Create multiple shimmer/ripple circles across the water surface
        int rippleCount = 15;

        for (int i = 0; i < rippleCount; i++)
        {
            GameObject ripple = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ripple.name = "ShimmerRipple_" + i;
            ripple.transform.SetParent(transform);

            // Random position across water
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(10f, 80f);
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;

            ripple.transform.localPosition = new Vector3(x, 0.05f, z);
            ripple.transform.localScale = new Vector3(Random.Range(2f, 6f), 0.005f, Random.Range(2f, 6f));
            Destroy(ripple.GetComponent<Collider>());

            // Shimmer material - light turquoise with transparency
            Material shimmerMat = new Material(Shader.Find("Standard"));
            shimmerMat.SetFloat("_Mode", 3);
            shimmerMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            shimmerMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            shimmerMat.SetInt("_ZWrite", 0);
            shimmerMat.EnableKeyword("_ALPHABLEND_ON");
            shimmerMat.renderQueue = 3050;
            shimmerMat.color = new Color(shimmerColor.r, shimmerColor.g, shimmerColor.b, 0f);
            shimmerMat.SetFloat("_Glossiness", 0.98f);
            shimmerMat.SetFloat("_Metallic", 0.2f);
            shimmerMat.EnableKeyword("_EMISSION");
            shimmerMat.SetColor("_EmissionColor", shimmerColor * 0.3f);

            ripple.GetComponent<Renderer>().material = shimmerMat;

            shimmerRipples.Add(ripple);
            shimmerMaterials.Add(shimmerMat);
        }
    }

    IEnumerator AnimateShimmerRipples()
    {
        // Store random phase offsets for each ripple
        float[] phases = new float[shimmerRipples.Count];
        float[] speeds = new float[shimmerRipples.Count];
        Vector3[] baseScales = new Vector3[shimmerRipples.Count];

        for (int i = 0; i < shimmerRipples.Count; i++)
        {
            phases[i] = Random.Range(0f, Mathf.PI * 2f);
            speeds[i] = Random.Range(0.8f, 1.5f);
            if (shimmerRipples[i] != null)
                baseScales[i] = shimmerRipples[i].transform.localScale;
        }

        while (true)
        {
            yield return null;
            if (!MainMenu.GameStarted) continue;

            float daylight = DayNightCycle.Instance != null ? DayNightCycle.Instance.GetDaylightIntensity() : 1f;
            float time = Time.time;

            for (int i = 0; i < shimmerRipples.Count; i++)
            {
                if (shimmerRipples[i] == null) continue;

                // Pulsing shimmer effect
                float shimmerPhase = time * speeds[i] + phases[i];
                float shimmerIntensity = Mathf.Sin(shimmerPhase) * 0.5f + 0.5f;
                shimmerIntensity *= shimmerIntensity; // Sharper peaks

                // Only show shimmer during daytime
                float alpha = shimmerIntensity * 0.25f * daylight;

                // Update material
                if (shimmerMaterials[i] != null)
                {
                    Color c = shimmerMaterials[i].color;
                    c.a = alpha;
                    shimmerMaterials[i].color = c;
                    shimmerMaterials[i].SetColor("_EmissionColor", shimmerColor * shimmerIntensity * 0.4f * daylight);
                }

                // Gentle scale pulsing
                float scalePulse = 1f + Mathf.Sin(shimmerPhase * 0.5f) * 0.15f;
                shimmerRipples[i].transform.localScale = new Vector3(
                    baseScales[i].x * scalePulse,
                    baseScales[i].y,
                    baseScales[i].z * scalePulse
                );

                // Slow drift movement
                Vector3 pos = shimmerRipples[i].transform.localPosition;
                pos.x += Mathf.Sin(time * 0.1f + phases[i]) * 0.002f;
                pos.z += Mathf.Cos(time * 0.08f + phases[i]) * 0.002f;
                shimmerRipples[i].transform.localPosition = pos;
            }
        }
    }

    void CreateSunReflection()
    {
        sunReflection = new GameObject("SunReflection");
        sunReflection.transform.SetParent(transform);

        // Main sun reflection glow - subtle warm glow only, no bright core
        GameObject mainGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mainGlow.name = "SunGlow";
        mainGlow.transform.SetParent(sunReflection.transform);
        mainGlow.transform.localPosition = new Vector3(0, 0.1f, 0);
        mainGlow.transform.localScale = new Vector3(6f, 0.01f, 6f);
        Destroy(mainGlow.GetComponent<Collider>());

        // Softer golden glow - not white
        Material glowMat = CreateTransparentMaterial(new Color(1f, 0.85f, 0.5f, 0.15f));
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.4f) * 0.3f);
        mainGlow.GetComponent<Renderer>().material = glowMat;

        // SunCore REMOVED - was causing white lighting artifact
    }

    void CreateCausticLights()
    {
        // Create several point lights under water for caustic effect
        for (int i = 0; i < 5; i++)
        {
            GameObject lightObj = new GameObject("CausticLight_" + i);
            lightObj.transform.SetParent(transform);

            float angle = i * (360f / 5) * Mathf.Deg2Rad;
            float radius = Random.Range(5f, 15f);
            lightObj.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                -0.5f,
                Mathf.Sin(angle) * radius
            );

            Light causticLight = lightObj.AddComponent<Light>();
            causticLight.type = LightType.Point;
            causticLight.color = new Color(0.6f, 0.85f, 1f);
            causticLight.intensity = 0.5f;
            causticLight.range = 8f;

            causticLights.Add(lightObj);
        }
    }

    IEnumerator SpawnWaterSparkles()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

            if (!MainMenu.GameStarted) continue;

            // Only spawn sparkles during daytime
            float daylight = DayNightCycle.Instance != null ? DayNightCycle.Instance.GetDaylightIntensity() : 1f;
            if (daylight < 0.3f) continue;

            // Remove dead sparkles
            sparkleParticles.RemoveAll(s => s == null);

            if (sparkleParticles.Count < maxSparkles)
            {
                CreateSparkle();
            }
        }
    }

    void CreateSparkle()
    {
        // Random position on water surface
        float range = 40f;
        Vector3 pos = new Vector3(
            Random.Range(-range, range),
            transform.position.y + 0.15f,
            Random.Range(-range, range)
        );

        GameObject sparkle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sparkle.name = "WaterSparkle";
        sparkle.transform.position = pos;
        sparkle.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f);
        Destroy(sparkle.GetComponent<Collider>());

        Material sparkleMat = new Material(Shader.Find("Standard"));
        sparkleMat.color = new Color(0.7f, 0.9f, 1f, 0.9f); // Light cyan sparkle instead of white
        sparkleMat.EnableKeyword("_EMISSION");

        // Sparkle color based on time of day - tinted cyan
        Color sunColor = DayNightCycle.Instance != null ? DayNightCycle.Instance.GetSunColor() : new Color(0.8f, 0.95f, 1f);
        Color sparkleEmission = new Color(sunColor.r * 0.8f, sunColor.g * 0.95f, sunColor.b * 1f);
        sparkleMat.SetColor("_EmissionColor", sparkleEmission * 1.5f);
        sparkleMat.SetFloat("_Metallic", 0.9f);
        sparkleMat.SetFloat("_Glossiness", 1f);

        sparkle.GetComponent<Renderer>().material = sparkleMat;
        sparkleParticles.Add(sparkle);

        StartCoroutine(AnimateSparkle(sparkle, sparkleMat));
    }

    IEnumerator AnimateSparkle(GameObject sparkle, Material mat)
    {
        float lifetime = Random.Range(0.3f, 0.8f);
        float t = 0;
        Vector3 startScale = sparkle.transform.localScale;

        while (t < lifetime && sparkle != null)
        {
            t += Time.deltaTime;
            float progress = t / lifetime;

            // Pulse and fade
            float pulse = Mathf.Sin(progress * Mathf.PI);
            sparkle.transform.localScale = startScale * (0.5f + pulse * 0.5f);

            Color emission = mat.GetColor("_EmissionColor");
            emission.a = pulse;
            mat.SetColor("_EmissionColor", emission * pulse);

            yield return null;
        }

        if (sparkle != null)
        {
            sparkleParticles.Remove(sparkle);
            Destroy(sparkle);
        }
    }

    IEnumerator SpawnFoamParticles()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));

            if (!MainMenu.GameStarted) continue;

            foamParticles.RemoveAll(f => f == null);

            if (foamParticles.Count < maxFoamParticles)
            {
                CreateFoamParticle();
            }
        }
    }

    void CreateFoamParticle()
    {
        // Spawn foam near shore/dock areas
        float range = 35f;
        Vector3 pos = new Vector3(
            Random.Range(-range, range),
            transform.position.y + 0.08f,
            Random.Range(-range, range)
        );

        GameObject foam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        foam.name = "WaterFoam";
        foam.transform.position = pos;
        foam.transform.localScale = new Vector3(Random.Range(0.3f, 0.8f), 0.02f, Random.Range(0.3f, 0.8f));
        foam.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        Destroy(foam.GetComponent<Collider>());

        // Light cyan foam instead of white
        Material foamMat = CreateTransparentMaterial(new Color(0.75f, 0.9f, 0.95f, 0.35f));
        foam.GetComponent<Renderer>().material = foamMat;
        foamParticles.Add(foam);

        StartCoroutine(AnimateFoam(foam, foamMat));
    }

    IEnumerator AnimateFoam(GameObject foam, Material mat)
    {
        float lifetime = Random.Range(2f, 4f);
        float t = 0;
        Vector3 startPos = foam.transform.position;
        Vector3 drift = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

        while (t < lifetime && foam != null)
        {
            t += Time.deltaTime;
            float progress = t / lifetime;

            // Drift slowly
            foam.transform.position = startPos + drift * progress;
            foam.transform.position = new Vector3(
                foam.transform.position.x,
                transform.position.y + 0.08f + Mathf.Sin(Time.time * 2f) * 0.02f,
                foam.transform.position.z
            );

            // Fade out
            Color c = mat.color;
            c.a = 0.4f * (1f - progress * progress);
            mat.color = c;

            yield return null;
        }

        if (foam != null)
        {
            foamParticles.Remove(foam);
            Destroy(foam);
        }
    }

    Material CreateTransparentMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3100;
        mat.color = color;
        return mat;
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

        // Create secondary stylized wave layers
        CreateWaveLayers();
    }

    void CreateWaveLayers()
    {
        // Create depth gradient rings around the island
        CreateDepthGradientRings();

        // Create multiple thin wave layers for parallax effect - using ocean blue tints
        for (int i = 0; i < 3; i++)
        {
            GameObject waveLayer = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waveLayer.name = "WaveLayer_" + i;
            waveLayer.transform.SetParent(transform);
            waveLayer.transform.localPosition = new Vector3(0, 0.02f + i * 0.015f, 0);
            waveLayer.transform.localScale = new Vector3(4.8f - i * 0.1f, 1, 4.8f - i * 0.1f);
            Destroy(waveLayer.GetComponent<Collider>());

            Material waveMat = new Material(Shader.Find("Standard"));
            waveMat.SetFloat("_Mode", 3);
            waveMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            waveMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            waveMat.SetInt("_ZWrite", 0);
            waveMat.EnableKeyword("_ALPHABLEND_ON");
            waveMat.renderQueue = 3001 + i;

            // Each layer using turquoise-blue gradient colors
            float alpha = 0.18f - i * 0.05f;
            // Gradient from turquoise (inner) to deeper blue (outer)
            float blueShift = i * 0.1f;
            waveMat.color = new Color(0.12f - blueShift, 0.65f - blueShift * 0.5f, 0.72f - blueShift * 0.2f, alpha);
            waveMat.SetFloat("_Glossiness", 0.95f);
            waveMat.SetFloat("_Metallic", 0.1f);

            waveLayer.GetComponent<Renderer>().material = waveMat;
        }

        // Create wave crest highlights (subtle light blue foam)
        CreateWaveCrests();

        // Create underwater light beams
        CreateUnderwaterBeams();
    }

    void CreateDepthGradientRings()
    {
        // Create concentric rings showing water depth gradient
        Color[] depthColors = { shorelineColor, shallowWaterColor, midWaterColor, deepWaterColor };
        float[] ringRadii = { 55f, 70f, 90f, 120f };

        for (int i = 0; i < depthColors.Length; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "DepthRing_" + i;
            ring.transform.SetParent(transform);
            ring.transform.localPosition = new Vector3(0, -0.02f - i * 0.03f, 0);

            float radius = ringRadii[i] / 10f; // Scale for plane
            ring.transform.localScale = new Vector3(radius, 0.01f, radius);
            Destroy(ring.GetComponent<Collider>());

            Material ringMat = CreateTransparentMaterial(depthColors[i]);
            ringMat.renderQueue = 2995 + i;
            ring.GetComponent<Renderer>().material = ringMat;
        }
    }

    void CreateWaveCrests()
    {
        // Light blue foam-like crests that move across the water - NOT WHITE
        for (int i = 0; i < 15; i++)
        {
            GameObject crest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crest.name = "WaveCrest_" + i;
            crest.transform.SetParent(transform);

            float x = Random.Range(-40f, 40f);
            float z = Random.Range(-40f, 40f);
            crest.transform.localPosition = new Vector3(x, 0.08f, z);
            crest.transform.localScale = new Vector3(Random.Range(2f, 6f), 0.01f, Random.Range(0.3f, 0.8f));
            crest.transform.localRotation = Quaternion.Euler(0, Random.Range(-30f, 30f), 0);

            Destroy(crest.GetComponent<Collider>());

            Material crestMat = new Material(Shader.Find("Standard"));
            crestMat.SetFloat("_Mode", 3);
            crestMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            crestMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            crestMat.EnableKeyword("_ALPHABLEND_ON");
            crestMat.renderQueue = 3010;
            // Soft seafoam color for wave crests
            crestMat.color = new Color(0.85f, 0.95f, 0.95f, 0.25f);
            crestMat.EnableKeyword("_EMISSION");
            crestMat.SetColor("_EmissionColor", new Color(0.7f, 0.9f, 0.9f) * 0.2f);

            crest.GetComponent<Renderer>().material = crestMat;

            // Animate crest
            StartCoroutine(AnimateWaveCrest(crest, crestMat));
        }
    }

    IEnumerator AnimateWaveCrest(GameObject crest, Material mat)
    {
        Vector3 startPos = crest.transform.localPosition;
        float speed = Random.Range(0.5f, 1.5f);
        float direction = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 moveDir = new Vector3(Mathf.Cos(direction), 0, Mathf.Sin(direction));

        while (crest != null)
        {
            // Move crest
            crest.transform.localPosition += moveDir * speed * Time.deltaTime;

            // Wave undulation
            float wave = Mathf.Sin(Time.time * 2f + startPos.x * 0.1f) * 0.02f;
            Vector3 pos = crest.transform.localPosition;
            pos.y = 0.08f + wave;
            crest.transform.localPosition = pos;

            // Fade based on position
            float distFromCenter = new Vector2(pos.x, pos.z).magnitude;
            float fade = 1f - Mathf.Clamp01(distFromCenter / 45f);
            Color c = mat.color;
            c.a = 0.25f * fade;
            mat.color = c;

            // Wrap around
            if (pos.x > 45f) crest.transform.localPosition = new Vector3(-45f, pos.y, pos.z);
            if (pos.x < -45f) crest.transform.localPosition = new Vector3(45f, pos.y, pos.z);
            if (pos.z > 45f) crest.transform.localPosition = new Vector3(pos.x, pos.y, -45f);
            if (pos.z < -45f) crest.transform.localPosition = new Vector3(pos.x, pos.y, 45f);

            yield return null;
        }
    }

    void CreateUnderwaterBeams()
    {
        // Light beams visible under/through water
        for (int i = 0; i < 8; i++)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "UnderwaterBeam_" + i;
            beam.transform.SetParent(transform);

            float angle = i * 45f * Mathf.Deg2Rad;
            float dist = Random.Range(10f, 30f);
            beam.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * dist,
                -0.3f,
                Mathf.Sin(angle) * dist
            );

            beam.transform.localScale = new Vector3(Random.Range(0.5f, 1.5f), 2f, Random.Range(0.3f, 0.8f));
            beam.transform.localRotation = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(0f, 360f), Random.Range(-10f, 10f));

            Destroy(beam.GetComponent<Collider>());

            Material beamMat = new Material(Shader.Find("Standard"));
            beamMat.SetFloat("_Mode", 3);
            beamMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            beamMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
            beamMat.EnableKeyword("_ALPHABLEND_ON");
            beamMat.renderQueue = 2999;
            beamMat.color = new Color(0.4f, 0.7f, 0.9f, 0.08f);

            beam.GetComponent<Renderer>().material = beamMat;

            StartCoroutine(AnimateUnderwaterBeam(beam, beamMat));
        }
    }

    IEnumerator AnimateUnderwaterBeam(GameObject beam, Material mat)
    {
        Vector3 startPos = beam.transform.localPosition;
        float phaseOffset = Random.Range(0f, Mathf.PI * 2f);

        while (beam != null)
        {
            // Subtle swaying motion
            float sway = Mathf.Sin(Time.time * 0.5f + phaseOffset) * 0.5f;
            beam.transform.localPosition = startPos + new Vector3(sway, 0, sway * 0.5f);

            // Intensity variation based on day/night
            if (DayNightCycle.Instance != null)
            {
                float daylight = DayNightCycle.Instance.GetDaylightIntensity();
                float flicker = 0.05f + Mathf.Sin(Time.time * 3f + phaseOffset) * 0.03f;
                Color c = mat.color;
                c.a = flicker * daylight;
                mat.color = c;
            }

            yield return null;
        }
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

        // Update sun reflection position and intensity
        UpdateSunReflection();

        // Animate caustic lights
        UpdateCausticLights();

        // Update water color based on time of day
        UpdateWaterLighting();
    }

    void UpdateSunReflection()
    {
        if (sunReflection == null || DayNightCycle.Instance == null) return;

        // Position sun reflection based on sun position
        Vector3 sunDir = DayNightCycle.Instance.GetSunDirection();
        float reflectionX = sunDir.x * 30f;
        float reflectionZ = sunDir.z * 30f;

        sunReflection.transform.localPosition = new Vector3(reflectionX, 0.1f, reflectionZ);

        // Scale and intensity based on sun height (no reflection at night)
        float daylight = DayNightCycle.Instance.GetDaylightIntensity();
        float scale = 6f * daylight;

        Transform glow = sunReflection.transform.Find("SunGlow");

        if (glow != null)
        {
            glow.localScale = new Vector3(scale, 0.01f, scale);
            Renderer r = glow.GetComponent<Renderer>();
            if (r != null)
            {
                Color sunColor = DayNightCycle.Instance.GetSunColor();
                // Softer golden color, not white
                Color glowColor = new Color(sunColor.r * 0.9f, sunColor.g * 0.8f, sunColor.b * 0.5f, 0.15f * daylight);
                r.material.color = glowColor;
                r.material.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.4f) * 0.3f * daylight);
            }
        }

        // Shimmer effect
        float shimmer = 1f + Mathf.Sin(Time.time * 5f) * 0.1f;
        sunReflection.transform.localScale = Vector3.one * shimmer;
    }

    void UpdateCausticLights()
    {
        if (DayNightCycle.Instance == null) return;

        float daylight = DayNightCycle.Instance.GetDaylightIntensity();
        Color sunColor = DayNightCycle.Instance.GetSunColor();

        for (int i = 0; i < causticLights.Count; i++)
        {
            GameObject lightObj = causticLights[i];
            if (lightObj == null) continue;

            Light light = lightObj.GetComponent<Light>();
            if (light == null) continue;

            // Animate position for moving caustic pattern
            float angle = (i * (360f / causticLights.Count) + Time.time * 10f) * Mathf.Deg2Rad;
            float radius = 8f + Mathf.Sin(Time.time * 0.5f + i) * 3f;

            lightObj.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                -0.3f,
                Mathf.Sin(angle) * radius
            );

            // Intensity based on daylight
            light.intensity = 0.5f * daylight * (0.8f + Mathf.Sin(Time.time * 3f + i * 0.5f) * 0.2f);
            light.color = Color.Lerp(new Color(0.6f, 0.85f, 1f), sunColor, 0.3f);
        }
    }

    void UpdateWaterLighting()
    {
        if (waterMat == null) return;

        if (DayNightCycle.Instance != null)
        {
            float daylight = DayNightCycle.Instance.GetDaylightIntensity();
            Color sunColor = DayNightCycle.Instance.GetSunColor();

            // Darken water at night
            Color dayWater = shallowWaterColor;
            Color nightWater = new Color(
                shallowWaterColor.r * 0.2f,
                shallowWaterColor.g * 0.3f,
                shallowWaterColor.b * 0.4f,
                shallowWaterColor.a + 0.2f
            );

            Color currentWater = Color.Lerp(nightWater, dayWater, daylight);

            // Tint with sun color
            currentWater = Color.Lerp(currentWater, sunColor * 0.3f + currentWater * 0.7f, 0.3f);

            // Only update if not in danger zone
            if (playerTransform == null || playerTransform.position.y > 1.0f ||
                GetDistanceToNearestIsland(playerTransform.position) < warningDistance)
            {
                waterMat.color = currentWater;
            }

            // Adjust reflectivity based on light
            waterMat.SetFloat("_Glossiness", 0.7f + daylight * 0.25f);
        }
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
