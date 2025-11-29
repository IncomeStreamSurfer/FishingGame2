using UnityEngine;
using System.Collections;

/// <summary>
/// BBQ Station - Located at the end of the dock
/// - Press E to open/close
/// - Sizzling sound plays ONLY when open
/// - Player can cook fish from inventory when open
/// </summary>
public class BBQStation : MonoBehaviour
{
    public static BBQStation Instance { get; private set; }

    private bool isOpen = false;
    private bool playerNearby = false;
    private float interactionDistance = 4f;

    // Audio
    private AudioSource audioSource;
    private AudioSource sfxSource;  // For one-shot sounds
    private AudioClip sizzleClip;
    private bool sizzlePlaying = false;

    // Visual components
    private GameObject fireGlow;
    private GameObject smoke;
    private float fireIntensity = 0f;

    // UI
    private Texture2D promptBg;
    private bool initialized = false;

    void Awake()
    {
        // Allow multiple BBQ instances (one per realm)
        // Instance points to the nearest active BBQ for UI purposes
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        CreateBBQModel();
        SetupAudio();
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        promptBg = new Texture2D(1, 1);
        promptBg.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        promptBg.Apply();
        initialized = true;
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 15f;
        audioSource.volume = 0.6f;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Second audio source for one-shot sounds
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 1f;
        sfxSource.minDistance = 2f;
        sfxSource.maxDistance = 10f;
        sfxSource.volume = 0.5f;
        sfxSource.playOnAwake = false;

        // Generate procedural sizzle sound
        GenerateSizzleSound();
    }

    void PlayLidOpenSound()
    {
        StartCoroutine(GenerateLidSound(true));
    }

    void PlayLidCloseSound()
    {
        StartCoroutine(GenerateLidSound(false));
    }

    System.Collections.IEnumerator GenerateLidSound(bool opening)
    {
        int sampleRate = 44100;
        float duration = 0.4f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip lidClip = AudioClip.Create("LidSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Metal scrape/clang
            float freq = opening ? Mathf.Lerp(200f, 400f, progress) : Mathf.Lerp(400f, 200f, progress);
            float metal = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.3f;
            metal += Mathf.Sin(2 * Mathf.PI * freq * 2.5f * t) * 0.15f;
            metal += Mathf.Sin(2 * Mathf.PI * freq * 4f * t) * 0.1f;

            // Metallic noise
            float noise = (Random.value * 2f - 1f) * 0.15f * Mathf.Exp(-progress * 3f);

            // Clang at the end
            float clang = 0f;
            if (progress > 0.7f)
            {
                float clangProgress = (progress - 0.7f) / 0.3f;
                clang = Mathf.Sin(2 * Mathf.PI * 800f * t) * Mathf.Exp(-clangProgress * 10f) * 0.4f;
            }

            // Envelope
            float envelope = Mathf.Sin(progress * Mathf.PI);

            samples[i] = (metal + noise + clang) * envelope * 0.5f;
        }

        lidClip.SetData(samples, 0);
        sfxSource.clip = lidClip;
        sfxSource.pitch = opening ? 1.0f : 0.9f;
        sfxSource.Play();

        yield return null;
    }

    void GenerateSizzleSound()
    {
        int sampleRate = 44100;
        float duration = 2f; // Loop every 2 seconds
        int sampleCount = (int)(sampleRate * duration);

        sizzleClip = AudioClip.Create("Sizzle", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        System.Random rand = new System.Random(42);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // White noise base
            float noise = (float)(rand.NextDouble() * 2 - 1);

            // Filter to make it more "sizzly" - emphasize higher frequencies
            float sizzle = 0f;

            // Multiple frequency bands for realistic sizzle
            sizzle += Mathf.Sin(t * 2000f + noise * 50f) * 0.1f;
            sizzle += Mathf.Sin(t * 4000f + noise * 100f) * 0.15f;
            sizzle += Mathf.Sin(t * 6000f + noise * 150f) * 0.1f;

            // Random pops and crackles
            if (rand.NextDouble() < 0.002f)
            {
                sizzle += (float)(rand.NextDouble() * 0.5f - 0.25f);
            }

            // Add filtered noise
            sizzle += noise * 0.15f;

            // Envelope for variation
            float envelope = 0.8f + Mathf.Sin(t * 3f) * 0.2f;
            envelope *= 0.8f + Mathf.Sin(t * 7f) * 0.15f;

            samples[i] = sizzle * envelope * 0.4f;
        }

        sizzleClip.SetData(samples, 0);
        audioSource.clip = sizzleClip;
    }

    void CreateBBQModel()
    {
        // Main body - rectangular grill
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "BBQBody";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 0.5f, 0);
        body.transform.localScale = new Vector3(0.8f, 0.4f, 0.5f);
        Material bodyMat = new Material(Shader.Find("Standard"));
        bodyMat.color = new Color(0.15f, 0.15f, 0.15f); // Dark metal
        bodyMat.SetFloat("_Metallic", 0.8f);
        bodyMat.SetFloat("_Glossiness", 0.3f);
        body.GetComponent<Renderer>().material = bodyMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Legs (4 corners)
        Material legMat = new Material(Shader.Find("Standard"));
        legMat.color = new Color(0.1f, 0.1f, 0.1f);

        float legOffset = 0.3f;
        Vector3[] legPositions = {
            new Vector3(-legOffset, 0.15f, -0.18f),
            new Vector3(legOffset, 0.15f, -0.18f),
            new Vector3(-legOffset, 0.15f, 0.18f),
            new Vector3(legOffset, 0.15f, 0.18f)
        };

        foreach (Vector3 pos in legPositions)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "BBQLeg";
            leg.transform.SetParent(transform);
            leg.transform.localPosition = pos;
            leg.transform.localScale = new Vector3(0.06f, 0.15f, 0.06f);
            leg.GetComponent<Renderer>().material = legMat;
            Object.Destroy(leg.GetComponent<Collider>());
        }

        // Grill grate on top
        GameObject grate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grate.name = "BBQGrate";
        grate.transform.SetParent(transform);
        grate.transform.localPosition = new Vector3(0, 0.72f, 0);
        grate.transform.localScale = new Vector3(0.7f, 0.02f, 0.4f);
        Material grateMat = new Material(Shader.Find("Standard"));
        grateMat.color = new Color(0.2f, 0.2f, 0.2f);
        grateMat.SetFloat("_Metallic", 0.9f);
        grate.GetComponent<Renderer>().material = grateMat;
        Object.Destroy(grate.GetComponent<Collider>());

        // Grill lines
        for (int i = 0; i < 6; i++)
        {
            GameObject grillLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grillLine.name = "GrillLine";
            grillLine.transform.SetParent(transform);
            grillLine.transform.localPosition = new Vector3(-0.3f + i * 0.12f, 0.73f, 0);
            grillLine.transform.localScale = new Vector3(0.02f, 0.02f, 0.38f);
            grillLine.GetComponent<Renderer>().material = grateMat;
            Object.Destroy(grillLine.GetComponent<Collider>());
        }

        // Side shelf
        GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shelf.name = "BBQShelf";
        shelf.transform.SetParent(transform);
        shelf.transform.localPosition = new Vector3(0.55f, 0.45f, 0);
        shelf.transform.localScale = new Vector3(0.25f, 0.02f, 0.4f);
        shelf.GetComponent<Renderer>().material = bodyMat;
        Object.Destroy(shelf.GetComponent<Collider>());

        // Handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "BBQHandle";
        handle.transform.SetParent(transform);
        handle.transform.localPosition = new Vector3(-0.45f, 0.55f, 0);
        handle.transform.localScale = new Vector3(0.04f, 0.15f, 0.04f);
        handle.transform.localRotation = Quaternion.Euler(0, 0, 90);
        Material handleMat = new Material(Shader.Find("Standard"));
        handleMat.color = new Color(0.3f, 0.2f, 0.1f);
        handle.GetComponent<Renderer>().material = handleMat;
        Object.Destroy(handle.GetComponent<Collider>());

        // Fire glow (hidden initially)
        fireGlow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fireGlow.name = "FireGlow";
        fireGlow.transform.SetParent(transform);
        fireGlow.transform.localPosition = new Vector3(0, 0.65f, 0);
        fireGlow.transform.localScale = new Vector3(0.65f, 0.1f, 0.35f);
        Material fireMat = new Material(Shader.Find("Standard"));
        fireMat.color = new Color(1f, 0.4f, 0.1f);
        fireMat.EnableKeyword("_EMISSION");
        fireMat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f) * 2f);
        fireGlow.GetComponent<Renderer>().material = fireMat;
        Object.Destroy(fireGlow.GetComponent<Collider>());
        fireGlow.SetActive(false);

        // Smoke particles placeholder (just a moving sphere for now)
        smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        smoke.name = "Smoke";
        smoke.transform.SetParent(transform);
        smoke.transform.localPosition = new Vector3(0, 1f, 0);
        smoke.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
        Material smokeMat = new Material(Shader.Find("Standard"));
        smokeMat.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        smokeMat.SetFloat("_Mode", 3);
        smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        smokeMat.EnableKeyword("_ALPHABLEND_ON");
        smokeMat.renderQueue = 3000;
        smoke.GetComponent<Renderer>().material = smokeMat;
        Object.Destroy(smoke.GetComponent<Collider>());
        smoke.SetActive(false);

        // Add a collider to the main object for interaction
        BoxCollider col = gameObject.AddComponent<BoxCollider>();
        col.size = new Vector3(1f, 0.8f, 0.6f);
        col.center = new Vector3(0, 0.4f, 0);
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check player distance
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerNearby = distance < interactionDistance;

            // Toggle BBQ with E
            if (playerNearby && Input.GetKeyDown(KeyCode.E))
            {
                ToggleBBQ();
            }

            // Close with ESC
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseBBQ();
            }

            // Close if player walks away
            if (isOpen && distance > interactionDistance * 1.5f)
            {
                CloseBBQ();
            }
        }

        // Update fire animation when open
        if (isOpen)
        {
            UpdateFireAnimation();
        }
    }

    void ToggleBBQ()
    {
        if (isOpen)
            CloseBBQ();
        else
            OpenBBQ();
    }

    void OpenBBQ()
    {
        isOpen = true;
        fireGlow.SetActive(true);
        smoke.SetActive(true);

        // Play lid opening sound
        PlayLidOpenSound();

        // Start sizzle sound
        if (!sizzlePlaying)
        {
            audioSource.Play();
            sizzlePlaying = true;
        }

        Debug.Log("BBQ opened - click fish in inventory to cook!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("BBQ Open - Click fish to cook!", new Color(1f, 0.6f, 0.2f));
        }
    }

    void CloseBBQ()
    {
        // Play lid closing sound
        PlayLidCloseSound();

        isOpen = false;
        fireGlow.SetActive(false);
        smoke.SetActive(false);

        // Stop sizzle sound
        if (sizzlePlaying)
        {
            audioSource.Stop();
            sizzlePlaying = false;
        }

        Debug.Log("BBQ closed");
    }

    void UpdateFireAnimation()
    {
        // Animate fire glow
        fireIntensity = 1.5f + Mathf.Sin(Time.time * 8f) * 0.3f + Mathf.Sin(Time.time * 13f) * 0.2f;
        if (fireGlow != null)
        {
            Material mat = fireGlow.GetComponent<Renderer>().material;
            mat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f) * fireIntensity);
        }

        // Animate smoke rising
        if (smoke != null)
        {
            float smokeY = 1f + Mathf.Sin(Time.time * 2f) * 0.1f + (Time.time % 2f) * 0.2f;
            smoke.transform.localPosition = new Vector3(
                Mathf.Sin(Time.time * 1.5f) * 0.1f,
                smokeY,
                Mathf.Cos(Time.time * 1.3f) * 0.1f
            );

            float smokeScale = 0.3f + (Time.time % 2f) * 0.15f;
            smoke.transform.localScale = new Vector3(smokeScale, smokeScale * 0.5f, smokeScale);

            // Fade smoke
            Material smokeMat = smoke.GetComponent<Renderer>().material;
            float alpha = 0.4f - (Time.time % 2f) * 0.2f;
            Color c = smokeMat.color;
            c.a = Mathf.Max(0.1f, alpha);
            smokeMat.color = c;
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        if (playerNearby && !isOpen)
        {
            DrawInteractionPrompt();
        }
        else if (isOpen)
        {
            DrawBBQOpenUI();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle promptStyle = new GUIStyle();
        promptStyle.fontSize = 18;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.normal.textColor = new Color(1f, 0.8f, 0.4f);
        promptStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle shadowStyle = new GUIStyle(promptStyle);
        shadowStyle.normal.textColor = new Color(0, 0, 0, 0.7f);

        float promptY = Screen.height * 0.6f;
        string text = "[E] Use BBQ";

        GUI.Label(new Rect(2, promptY + 2, Screen.width, 30), text, shadowStyle);
        GUI.Label(new Rect(0, promptY, Screen.width, 30), text, promptStyle);
    }

    void DrawBBQOpenUI()
    {
        float bgWidth = 200;
        float bgHeight = 85;
        float bgX = (Screen.width - bgWidth) / 2;
        float bgY = 100;

        // Background
        GUI.DrawTexture(new Rect(bgX, bgY, bgWidth, bgHeight), promptBg);

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.6f, 0.2f);
        GUI.Label(new Rect(bgX, bgY + 8, bgWidth, 20), "BBQ IS OPEN", titleStyle);

        // Instruction
        GUIStyle infoStyle = new GUIStyle();
        infoStyle.fontSize = 11;
        infoStyle.alignment = TextAnchor.MiddleCenter;
        infoStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        GUI.Label(new Rect(bgX, bgY + 28, bgWidth, 16), "Click fish in inventory to cook", infoStyle);

        // E to Close hint
        GUIStyle closeHintStyle = new GUIStyle();
        closeHintStyle.fontSize = 12;
        closeHintStyle.fontStyle = FontStyle.Bold;
        closeHintStyle.alignment = TextAnchor.MiddleCenter;
        closeHintStyle.normal.textColor = new Color(0.9f, 0.9f, 0.5f);
        GUI.Label(new Rect(bgX, bgY + 48, bgWidth, 16), "[E] to Close", closeHintStyle);

        // Close button as backup
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 10;

        if (GUI.Button(new Rect(bgX + bgWidth / 2 - 40, bgY + 65, 80, 18), "CLOSE", buttonStyle))
        {
            CloseBBQ();
        }
    }

    public bool IsOpen() => isOpen;
    public bool IsPlayerNearby() => playerNearby;

    void OnDestroy()
    {
        if (promptBg != null) Destroy(promptBg);
        if (sizzleClip != null) Destroy(sizzleClip);
    }
}
