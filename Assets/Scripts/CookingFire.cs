using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Beautiful flickering cooking fire with Chef Gusteau
/// Press F to cook special fish into powerful buffs
/// </summary>
public class CookingFire : MonoBehaviour
{
    public static CookingFire Instance { get; private set; }

    [Header("Interaction")]
    public float interactionRange = 4f;
    public KeyCode interactKey = KeyCode.F;

    [Header("Fire Settings")]
    public int flameCount = 12;
    public float fireIntensity = 1.2f;

    // Fire components
    private List<GameObject> flames = new List<GameObject>();
    private List<GameObject> embers = new List<GameObject>();
    private GameObject fireGlow;
    private GameObject smokeEmitter;
    private Light fireLight;

    // Chef Gusteau model
    private GameObject chefGusteau;

    // UI State
    private bool isOpen = false;
    private bool playerNearby = false;
    private Transform playerTransform;

    // Cached materials
    private Material flameMaterial;
    private Material emberMaterial;
    private Material glowMaterial;

    // Animation
    private float flickerTime = 0f;
    private float[] flamePhases;

    // Cached textures for UI
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    // Special fish that can be cooked
    private readonly string[] cookableFishIds = {
        "red_snapper",
        "blue_marlin",
        "rainbow_trout",
        "sunshore_od",
        "icelandic_snubnose",
        "seahorse"
    };

    private readonly Dictionary<string, string> fishBuffNames = new Dictionary<string, string>
    {
        { "red_snapper", "Snapper's Delight - No health loss for 5 min" },
        { "blue_marlin", "Marlin's Luck - +50% rare fish chance for 5 min" },
        { "rainbow_trout", "Trout's Fortune - +50% gold for 5 min" },
        { "sunshore_od", "Sunshore Surge - +50% XP for 5 min" },
        { "icelandic_snubnose", "Snubnose Speed - +25% movement speed for 5 min" },
        { "seahorse", "Seahorse's Bounty - Double fish catches for 5 min" }
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        // PERFORMANCE: Create simplified fire (just light + minimal visuals)
        CreateFireMaterials();
        CreateSimplifiedCampfire(); // Reduced from full campfire
        CreateCachedTextures();

        // Initialize flame phases for varied animation
        flamePhases = new float[flameCount];
        for (int i = 0; i < flameCount; i++)
        {
            flamePhases[i] = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void CreateSimplifiedCampfire()
    {
        // PERFORMANCE: Minimal fire - just light and a few visual elements
        // Instead of 52 primitives, we create ~5

        // Single fire pit marker
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "FirePit";
        marker.transform.SetParent(transform);
        marker.transform.localPosition = new Vector3(0, 0.05f, 0);
        marker.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
        Material pitMat = new Material(Shader.Find("Standard"));
        pitMat.color = new Color(0.2f, 0.15f, 0.1f);
        marker.GetComponent<Renderer>().sharedMaterial = pitMat;
        Destroy(marker.GetComponent<Collider>());

        // Single flame representation
        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flame.name = "Flame";
        flame.transform.SetParent(transform);
        flame.transform.localPosition = new Vector3(0, 0.4f, 0);
        flame.transform.localScale = new Vector3(0.5f, 0.8f, 1f);
        flame.GetComponent<Renderer>().material = flameMaterial;
        Destroy(flame.GetComponent<Collider>());
        flames.Add(flame);
        flameCount = 1; // Override to just 1 flame

        // Point light for fire glow
        CreateFireLight();

        // Simple chef indicator (just a sign post)
        GameObject chefPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chefPost.name = "ChefPost";
        chefPost.transform.SetParent(transform);
        chefPost.transform.localPosition = new Vector3(1.0f, 0.5f, 0);
        chefPost.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
        Material postMat = new Material(Shader.Find("Standard"));
        postMat.color = new Color(0.4f, 0.3f, 0.2f);
        chefPost.GetComponent<Renderer>().sharedMaterial = postMat;
        Destroy(chefPost.GetComponent<Collider>());

        // Chef sign
        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "ChefSign";
        sign.transform.SetParent(transform);
        sign.transform.localPosition = new Vector3(1.0f, 1.1f, 0);
        sign.transform.localScale = new Vector3(0.6f, 0.3f, 0.05f);
        Material signMat = new Material(Shader.Find("Standard"));
        signMat.color = Color.white;
        sign.GetComponent<Renderer>().sharedMaterial = signMat;
        Destroy(sign.GetComponent<Collider>());

        Debug.Log("[CookingFire] Using simplified visuals - 5 primitives instead of 52");
    }

    void CreateFireMaterials()
    {
        // Flame material - bright orange/yellow with emission
        flameMaterial = new Material(Shader.Find("Standard"));
        flameMaterial.SetFloat("_Mode", 3); // Transparent
        flameMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        flameMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
        flameMaterial.SetInt("_ZWrite", 0);
        flameMaterial.EnableKeyword("_ALPHABLEND_ON");
        flameMaterial.EnableKeyword("_EMISSION");
        flameMaterial.color = new Color(1f, 0.6f, 0.1f, 0.8f);
        flameMaterial.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.05f) * 2f);
        flameMaterial.renderQueue = 3100;

        // Ember material - red/orange glowing
        emberMaterial = new Material(Shader.Find("Standard"));
        emberMaterial.EnableKeyword("_EMISSION");
        emberMaterial.color = new Color(0.8f, 0.2f, 0.05f);
        emberMaterial.SetColor("_EmissionColor", new Color(1f, 0.3f, 0.05f) * 1.5f);

        // Glow material
        glowMaterial = new Material(Shader.Find("Standard"));
        glowMaterial.SetFloat("_Mode", 3);
        glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        glowMaterial.SetInt("_ZWrite", 0);
        glowMaterial.EnableKeyword("_ALPHABLEND_ON");
        glowMaterial.color = new Color(1f, 0.5f, 0.1f, 0.3f);
        glowMaterial.renderQueue = 3050;
    }

    void CreateCampfire()
    {
        // Fire pit base (stones)
        CreateFirePit();

        // Wood logs
        CreateLogs();

        // Flames
        CreateFlames();

        // Embers
        CreateEmbers();

        // Ambient glow
        CreateGlow();

        // Point light
        CreateFireLight();
    }

    void CreateFirePit()
    {
        Material stoneMat = new Material(Shader.Find("Standard"));
        stoneMat.color = new Color(0.3f, 0.28f, 0.25f);

        // Circle of stones
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            float radius = 0.6f;

            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = "FireStone";
            stone.transform.SetParent(transform);
            stone.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                0.1f,
                Mathf.Sin(angle) * radius
            );
            stone.transform.localScale = new Vector3(0.25f, 0.15f, 0.25f);
            stone.GetComponent<Renderer>().sharedMaterial = stoneMat;
            Destroy(stone.GetComponent<Collider>());
        }
    }

    void CreateLogs()
    {
        Material logMat = new Material(Shader.Find("Standard"));
        logMat.color = new Color(0.35f, 0.2f, 0.1f);

        Material charredMat = new Material(Shader.Find("Standard"));
        charredMat.color = new Color(0.1f, 0.08f, 0.05f);
        charredMat.EnableKeyword("_EMISSION");
        charredMat.SetColor("_EmissionColor", new Color(0.3f, 0.1f, 0.02f));

        // Crossed logs
        for (int i = 0; i < 3; i++)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = "FireLog";
            log.transform.SetParent(transform);

            float angle = i * 60f;
            log.transform.localPosition = new Vector3(0, 0.15f, 0);
            log.transform.localRotation = Quaternion.Euler(90, angle, 8);
            log.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
            log.GetComponent<Renderer>().sharedMaterial = i == 0 ? charredMat : logMat;
            Destroy(log.GetComponent<Collider>());
        }
    }

    void CreateFlames()
    {
        for (int i = 0; i < flameCount; i++)
        {
            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flame.name = "Flame";
            flame.transform.SetParent(transform);

            float angle = (i / (float)flameCount) * 360f;
            float radius = Random.Range(0.05f, 0.2f);
            float height = Random.Range(0.3f, 0.5f);

            flame.transform.localPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                height,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );

            float scale = Random.Range(0.2f, 0.4f);
            flame.transform.localScale = new Vector3(scale, scale * 1.8f, 1f);

            // Make flame material instance for individual animation
            Material mat = new Material(flameMaterial);
            flame.GetComponent<Renderer>().material = mat;
            Destroy(flame.GetComponent<Collider>());

            flames.Add(flame);
        }
    }

    void CreateEmbers()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject ember = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ember.name = "Ember";
            ember.transform.SetParent(transform);

            ember.transform.localPosition = new Vector3(
                Random.Range(-0.3f, 0.3f),
                0.05f + Random.Range(0f, 0.1f),
                Random.Range(-0.3f, 0.3f)
            );
            ember.transform.localScale = Vector3.one * Random.Range(0.02f, 0.05f);
            ember.GetComponent<Renderer>().sharedMaterial = emberMaterial;
            Destroy(ember.GetComponent<Collider>());

            embers.Add(ember);
        }
    }

    void CreateGlow()
    {
        fireGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fireGlow.name = "FireGlow";
        fireGlow.transform.SetParent(transform);
        fireGlow.transform.localPosition = new Vector3(0, 0.4f, 0);
        fireGlow.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);
        fireGlow.GetComponent<Renderer>().sharedMaterial = glowMaterial;
        Destroy(fireGlow.GetComponent<Collider>());
    }

    void CreateFireLight()
    {
        GameObject lightObj = new GameObject("FireLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = new Vector3(0, 0.8f, 0);

        fireLight = lightObj.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.6f, 0.2f);
        fireLight.intensity = 1.5f;
        fireLight.range = 8f;
        fireLight.shadows = LightShadows.Soft;
    }

    void CreateChefGusteau()
    {
        // Create Chef Gusteau next to the fire
        chefGusteau = new GameObject("ChefGusteau");
        chefGusteau.transform.SetParent(transform);
        chefGusteau.transform.localPosition = new Vector3(1.2f, 0, 0);
        chefGusteau.transform.localRotation = Quaternion.Euler(0, -90, 0);

        Material skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.95f, 0.8f, 0.7f);

        Material chefWhiteMat = new Material(Shader.Find("Standard"));
        chefWhiteMat.color = Color.white;

        Material pantsMat = new Material(Shader.Find("Standard"));
        pantsMat.color = new Color(0.2f, 0.2f, 0.2f);

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(chefGusteau.transform);
        body.transform.localPosition = new Vector3(0, 0.7f, 0);
        body.transform.localScale = new Vector3(0.4f, 0.5f, 0.3f);
        body.GetComponent<Renderer>().sharedMaterial = chefWhiteMat;
        Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(chefGusteau.transform);
        head.transform.localPosition = new Vector3(0, 1.3f, 0);
        head.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Destroy(head.GetComponent<Collider>());

        // Chef hat (toque)
        GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hat.name = "ChefHat";
        hat.transform.SetParent(chefGusteau.transform);
        hat.transform.localPosition = new Vector3(0, 1.6f, 0);
        hat.transform.localScale = new Vector3(0.25f, 0.2f, 0.25f);
        hat.GetComponent<Renderer>().sharedMaterial = chefWhiteMat;
        Destroy(hat.GetComponent<Collider>());

        // Hat puff
        GameObject hatPuff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hatPuff.name = "HatPuff";
        hatPuff.transform.SetParent(chefGusteau.transform);
        hatPuff.transform.localPosition = new Vector3(0, 1.85f, 0);
        hatPuff.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
        hatPuff.GetComponent<Renderer>().sharedMaterial = chefWhiteMat;
        Destroy(hatPuff.GetComponent<Collider>());

        // Legs
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = "Leg";
            leg.transform.SetParent(chefGusteau.transform);
            leg.transform.localPosition = new Vector3(i * 0.1f, 0.25f, 0);
            leg.transform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
            leg.GetComponent<Renderer>().sharedMaterial = pantsMat;
            Destroy(leg.GetComponent<Collider>());
        }

        // Mustache
        GameObject mustache = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mustache.name = "Mustache";
        mustache.transform.SetParent(chefGusteau.transform);
        mustache.transform.localPosition = new Vector3(0, 1.22f, 0.12f);
        mustache.transform.localScale = new Vector3(0.15f, 0.03f, 0.03f);
        Material mustacheMat = new Material(Shader.Find("Standard"));
        mustacheMat.color = new Color(0.2f, 0.15f, 0.1f);
        mustache.GetComponent<Renderer>().sharedMaterial = mustacheMat;
        Destroy(mustache.GetComponent<Collider>());
    }

    void CreateCachedTextures()
    {
        CacheTexture("panelBg", new Color(0.1f, 0.08f, 0.06f, 0.95f));
        CacheTexture("panelBorder", new Color(0.6f, 0.4f, 0.2f, 1f));
        CacheTexture("buttonNormal", new Color(0.4f, 0.25f, 0.1f, 1f));
        CacheTexture("buttonHover", new Color(0.5f, 0.35f, 0.15f, 1f));
        CacheTexture("fishSlot", new Color(0.2f, 0.15f, 0.1f, 1f));
        CacheTexture("fishSlotHover", new Color(0.3f, 0.2f, 0.1f, 1f));
        CacheTexture("white", Color.white);
    }

    void CacheTexture(string name, Color color)
    {
        if (!textureCache.ContainsKey(name))
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            textureCache[name] = tex;
        }
    }

    Texture2D GetTexture(string name)
    {
        return textureCache.TryGetValue(name, out Texture2D tex) ? tex : Texture2D.whiteTexture;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Update player reference
        if (playerTransform == null && GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            playerNearby = dist <= interactionRange;
        }

        // Toggle cooking UI
        if (playerNearby && Input.GetKeyDown(interactKey) && !isOpen)
        {
            OpenCookingUI();
        }
        else if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(interactKey)))
        {
            CloseCookingUI();
        }

        // Animate fire
        AnimateFire();
    }

    // Frame counter for performance - only animate every few frames
    private int animFrameCounter = 0;

    void AnimateFire()
    {
        // PERFORMANCE: Only animate every 3rd frame
        animFrameCounter++;
        if (animFrameCounter % 3 != 0) return;

        flickerTime += Time.deltaTime * 3f; // Compensate for skipped frames

        // Simple flame animation (only 1 flame now)
        foreach (var flame in flames)
        {
            if (flame == null) continue;

            // Billboard to camera
            if (Camera.main != null)
            {
                flame.transform.LookAt(Camera.main.transform);
                flame.transform.Rotate(0, 180, 0);
            }

            // Simple flicker
            float flicker = 0.7f + Mathf.Sin(flickerTime * 8f) * 0.3f;
            flame.transform.localScale = new Vector3(0.5f, 0.8f * flicker, 1f);
        }

        // Simple light flicker
        if (fireLight != null)
        {
            float lightFlicker = 1.2f + Mathf.Sin(flickerTime * 10f) * 0.3f;
            fireLight.intensity = lightFlicker * fireIntensity;
        }
    }

    void OpenCookingUI()
    {
        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cooking fire opened");
    }

    void CloseCookingUI()
    {
        isOpen = false;
        // Keep cursor visible - this is a point and click game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cooking fire closed");
    }

    public bool IsOpen() => isOpen;
    public bool IsPlayerNearby() => playerNearby;

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // PERFORMANCE: Only draw if there's something to show
        if (!playerNearby && !isOpen) return;

        // Show interaction prompt when nearby
        if (playerNearby && !isOpen)
        {
            DrawInteractionPrompt();
        }

        // Draw cooking UI
        if (isOpen)
        {
            DrawCookingUI();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = new Color(1f, 0.7f, 0.3f);

        GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 60, 300, 30), "[F] Chef Gusteau's Fire", style);

        style.fontSize = 14;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 35, 300, 25), "Cook special fish for powerful buffs!", style);
    }

    void DrawCookingUI()
    {
        float panelWidth = 500;
        float panelHeight = 450;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Panel border and background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.8f, 0.4f);
        GUI.Label(new Rect(panelX, panelY + 15, panelWidth, 35), "Chef Gusteau's Cooking Fire", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = 12;
        subStyle.fontStyle = FontStyle.Italic;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        GUI.Label(new Rect(panelX, panelY + 45, panelWidth, 20), "\"Anyone can cook... special fish!\"", subStyle);

        // Close button
        if (DrawButton(new Rect(panelX + panelWidth - 35, panelY + 10, 25, 25), "X"))
        {
            CloseCookingUI();
        }

        // Fish list
        float slotY = panelY + 80;
        float slotHeight = 55;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 11;
        labelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(panelX + 20, slotY - 18, 200, 20), "Special Fish Available:", labelStyle);

        int fishCooked = 0;

        foreach (string fishId in cookableFishIds)
        {
            // Check if player has this fish
            int count = GetSpecialFishCount(fishId);
            bool hasFish = count > 0;

            Rect slotRect = new Rect(panelX + 20, slotY, panelWidth - 40, slotHeight);
            bool hover = slotRect.Contains(Event.current.mousePosition);

            // Slot background
            GUI.DrawTexture(slotRect, hover && hasFish ? GetTexture("fishSlotHover") : GetTexture("fishSlot"));

            // Fish name
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = 16;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.normal.textColor = hasFish ? new Color(1f, 0.9f, 0.6f) : new Color(0.5f, 0.5f, 0.5f);

            string fishName = GetFishDisplayName(fishId);
            GUI.Label(new Rect(slotRect.x + 10, slotRect.y + 5, 200, 25), fishName, nameStyle);

            // Count
            GUIStyle countStyle = new GUIStyle();
            countStyle.fontSize = 14;
            countStyle.alignment = TextAnchor.MiddleRight;
            countStyle.normal.textColor = hasFish ? Color.green : Color.gray;
            GUI.Label(new Rect(slotRect.x + slotRect.width - 80, slotRect.y + 5, 70, 25), $"x{count}", countStyle);

            // Buff description
            if (fishBuffNames.TryGetValue(fishId, out string buffDesc))
            {
                GUIStyle descStyle = new GUIStyle();
                descStyle.fontSize = 11;
                descStyle.fontStyle = FontStyle.Italic;
                descStyle.normal.textColor = new Color(0.6f, 0.8f, 0.6f);
                descStyle.wordWrap = true;
                GUI.Label(new Rect(slotRect.x + 10, slotRect.y + 28, slotRect.width - 100, 25), buffDesc, descStyle);
            }

            // Cook button
            if (hasFish)
            {
                if (DrawButton(new Rect(slotRect.x + slotRect.width - 70, slotRect.y + 28, 60, 22), "Cook"))
                {
                    CookFish(fishId);
                    fishCooked++;
                }
            }

            slotY += slotHeight + 5;
        }

        // Instructions
        GUIStyle instrStyle = new GUIStyle();
        instrStyle.fontSize = 11;
        instrStyle.alignment = TextAnchor.MiddleCenter;
        instrStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 35, panelWidth, 20), "Catch special fish while fishing, then cook them here!", instrStyle);
        GUI.Label(new Rect(panelX, panelY + panelHeight - 20, panelWidth, 20), "Press [F] or [ESC] to close", instrStyle);
    }

    bool DrawButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        GUI.DrawTexture(rect, hover ? GetTexture("buttonHover") : GetTexture("buttonNormal"));

        GUIStyle btnStyle = new GUIStyle();
        btnStyle.fontSize = 12;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = Color.white;
        GUI.Label(rect, text, btnStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    int GetSpecialFishCount(string fishId)
    {
        if (FishingSystem.Instance == null) return 0;

        int count = 0;
        foreach (var fish in FishingSystem.Instance.GetSpecialFishInventory())
        {
            if (fish.id == fishId) count++;
        }
        return count;
    }

    string GetFishDisplayName(string fishId)
    {
        if (FishingSystem.Instance != null)
        {
            var fish = FishingSystem.Instance.GetFishById(fishId);
            if (fish != null) return fish.fishName;
        }

        // Fallback names
        return fishId switch
        {
            "red_snapper" => "Red Snapper",
            "blue_marlin" => "Blue Marlin",
            "rainbow_trout" => "Rainbow Trout",
            "sunshore_od" => "Sunshore Cod",
            "icelandic_snubnose" => "Icelandic Snubnose",
            "seahorse" => "Seahorse",
            _ => fishId
        };
    }

    void CookFish(string fishId)
    {
        if (FishingSystem.Instance == null || FishBuffSystem.Instance == null) return;

        // Remove the fish from inventory
        bool removed = FishingSystem.Instance.RemoveSpecialFish(fishId);
        if (!removed)
        {
            Debug.Log($"Failed to remove {fishId} from inventory");
            return;
        }

        // Add the corresponding buff to inventory
        FishBuffType buffType = fishId switch
        {
            "red_snapper" => FishBuffType.SnappersDelight,
            "blue_marlin" => FishBuffType.MarlinsLuck,
            "rainbow_trout" => FishBuffType.TroutsFortune,
            "sunshore_od" => FishBuffType.SunshoreSurge,
            "icelandic_snubnose" => FishBuffType.SnubnoseSpeed,
            "seahorse" => FishBuffType.SeahorsesBounty,
            _ => FishBuffType.None
        };

        if (buffType != FishBuffType.None)
        {
            FishBuffSystem.Instance.AddBuffToInventory(buffType);

            // Grant 1000 XP for cooking
            if (LevelingSystem.Instance != null)
            {
                LevelingSystem.Instance.AddXP(1000);
            }

            // Show notification
            if (UIManager.Instance != null)
            {
                string buffName = fishBuffNames.TryGetValue(fishId, out string desc) ? desc.Split('-')[0].Trim() : "Buff";
                UIManager.Instance.ShowLootNotification($"Cooked {GetFishDisplayName(fishId)}! +1000 XP! {buffName} added to hotbar.", new Color(1f, 0.8f, 0.3f));
            }

            Debug.Log($"Cooked {fishId} into {buffType} buff! +1000 XP");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}
