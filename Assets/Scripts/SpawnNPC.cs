using UnityEngine;

/// <summary>
/// NPC near player spawn who comments on the player being naked
/// Hums when idle, speaks when interacted with
/// </summary>
public class SpawnNPC : MonoBehaviour
{
    public static SpawnNPC Instance { get; private set; }

    private bool playerNearby = false;
    private bool dialogOpen = false;
    private float interactionDistance = 4f;
    private float dialogTimer = 0f;
    private float dialogDuration = 6f;

    // UI Textures
    private Texture2D bubbleTexture;
    private Texture2D borderTexture;
    private Texture2D pointerTexture;
    private bool stylesInitialized = false;

    // Audio
    private AudioSource audioSource;
    private float hummTimer = 0f;
    private float nextHummTime = 2f;
    private bool isHumming = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateNPCModel();
        SetupAudio();
        Invoke("InitializeStyles", 0.5f);
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 10f;
        audioSource.volume = 0.5f;
        audioSource.playOnAwake = false;
    }

    void InitializeStyles()
    {
        // Create speech bubble background (cream/white)
        bubbleTexture = new Texture2D(1, 1);
        bubbleTexture.SetPixel(0, 0, new Color(1f, 0.98f, 0.9f, 0.98f));
        bubbleTexture.Apply();

        // Create border texture (dark brown)
        borderTexture = new Texture2D(1, 1);
        borderTexture.SetPixel(0, 0, new Color(0.3f, 0.2f, 0.1f, 1f));
        borderTexture.Apply();

        // Create pointer texture
        pointerTexture = new Texture2D(1, 1);
        pointerTexture.SetPixel(0, 0, new Color(1f, 0.98f, 0.9f, 0.98f));
        pointerTexture.Apply();

        stylesInitialized = true;
    }

    void CreateNPCModel()
    {
        GameObject npcModel = new GameObject("NPCModel");
        npcModel.transform.SetParent(transform);
        npcModel.transform.localPosition = Vector3.zero;

        Material skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.85f, 0.65f, 0.5f);

        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.3f, 0.2f, 0.1f);

        Material shirtMat = new Material(Shader.Find("Standard"));
        shirtMat.color = new Color(0.2f, 0.5f, 0.3f);

        Material pantsMat = new Material(Shader.Find("Standard"));
        pantsMat.color = new Color(0.25f, 0.2f, 0.15f);

        // Body (torso)
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso";
        torso.transform.SetParent(npcModel.transform);
        torso.transform.localPosition = new Vector3(0, 1.1f, 0);
        torso.transform.localScale = new Vector3(0.4f, 0.5f, 0.25f);
        torso.GetComponent<Renderer>().material = shirtMat;
        Destroy(torso.GetComponent<Collider>());

        // Hips
        GameObject hips = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hips.name = "Hips";
        hips.transform.SetParent(npcModel.transform);
        hips.transform.localPosition = new Vector3(0, 0.75f, 0);
        hips.transform.localScale = new Vector3(0.35f, 0.2f, 0.22f);
        hips.GetComponent<Renderer>().material = pantsMat;
        Destroy(hips.GetComponent<Collider>());

        // Legs
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = "Leg";
            leg.transform.SetParent(npcModel.transform);
            leg.transform.localPosition = new Vector3(side * 0.1f, 0.4f, 0);
            leg.transform.localScale = new Vector3(0.12f, 0.3f, 0.12f);
            leg.GetComponent<Renderer>().material = pantsMat;
            Destroy(leg.GetComponent<Collider>());

            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foot.name = "Foot";
            foot.transform.SetParent(npcModel.transform);
            foot.transform.localPosition = new Vector3(side * 0.1f, 0.08f, 0.03f);
            foot.transform.localScale = new Vector3(0.1f, 0.06f, 0.15f);
            foot.GetComponent<Renderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.2f, 0.15f, 0.1f) };
            Destroy(foot.GetComponent<Collider>());
        }

        // Arms
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperArm.name = "UpperArm";
            upperArm.transform.SetParent(npcModel.transform);
            upperArm.transform.localPosition = new Vector3(side * 0.28f, 1.05f, 0);
            upperArm.transform.localRotation = Quaternion.Euler(0, 0, side * 15);
            upperArm.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);
            upperArm.GetComponent<Renderer>().material = shirtMat;
            Destroy(upperArm.GetComponent<Collider>());

            GameObject lowerArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lowerArm.name = "LowerArm";
            lowerArm.transform.SetParent(npcModel.transform);
            lowerArm.transform.localPosition = new Vector3(side * 0.32f, 0.8f, 0);
            lowerArm.transform.localScale = new Vector3(0.07f, 0.12f, 0.07f);
            lowerArm.GetComponent<Renderer>().material = skinMat;
            Destroy(lowerArm.GetComponent<Collider>());

            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hand.name = "Hand";
            hand.transform.SetParent(npcModel.transform);
            hand.transform.localPosition = new Vector3(side * 0.32f, 0.62f, 0);
            hand.transform.localScale = new Vector3(0.08f, 0.06f, 0.06f);
            hand.GetComponent<Renderer>().material = skinMat;
            Destroy(hand.GetComponent<Collider>());
        }

        // Neck
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.name = "Neck";
        neck.transform.SetParent(npcModel.transform);
        neck.transform.localPosition = new Vector3(0, 1.4f, 0);
        neck.transform.localScale = new Vector3(0.1f, 0.06f, 0.1f);
        neck.GetComponent<Renderer>().material = skinMat;
        Destroy(neck.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(npcModel.transform);
        head.transform.localPosition = new Vector3(0, 1.6f, 0);
        head.transform.localScale = new Vector3(0.24f, 0.28f, 0.24f);
        head.GetComponent<Renderer>().material = skinMat;
        Destroy(head.GetComponent<Collider>());

        // Hair
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.name = "Hair";
        hair.transform.SetParent(head.transform);
        hair.transform.localPosition = new Vector3(0, 0.35f, -0.1f);
        hair.transform.localScale = new Vector3(1.1f, 0.5f, 1.0f);
        hair.GetComponent<Renderer>().material = hairMat;
        Destroy(hair.GetComponent<Collider>());

        // Eyes
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eyeWhite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeWhite.name = "EyeWhite";
            eyeWhite.transform.SetParent(head.transform);
            eyeWhite.transform.localPosition = new Vector3(side * 0.25f, 0.1f, 0.4f);
            eyeWhite.transform.localScale = new Vector3(0.15f, 0.1f, 0.08f);
            eyeWhite.GetComponent<Renderer>().material = new Material(Shader.Find("Standard")) { color = Color.white };
            Destroy(eyeWhite.GetComponent<Collider>());

            GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.name = "Pupil";
            pupil.transform.SetParent(eyeWhite.transform);
            pupil.transform.localPosition = new Vector3(0, 0, 0.4f);
            pupil.transform.localScale = new Vector3(0.5f, 0.6f, 0.3f);
            pupil.GetComponent<Renderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.2f, 0.15f, 0.1f) };
            Destroy(pupil.GetComponent<Collider>());
        }

        // Nose
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "Nose";
        nose.transform.SetParent(head.transform);
        nose.transform.localPosition = new Vector3(0, -0.05f, 0.45f);
        nose.transform.localScale = new Vector3(0.12f, 0.15f, 0.15f);
        nose.GetComponent<Renderer>().material = skinMat;
        Destroy(nose.GetComponent<Collider>());

        // Mouth
        GameObject mouth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mouth.name = "Mouth";
        mouth.transform.SetParent(head.transform);
        mouth.transform.localPosition = new Vector3(0, -0.25f, 0.42f);
        mouth.transform.localScale = new Vector3(0.12f, 0.08f, 0.05f);
        mouth.GetComponent<Renderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.4f, 0.2f, 0.2f) };
        Destroy(mouth.GetComponent<Collider>());
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerNearby = distance < interactionDistance;

            // Auto-close dialog when player wanders too far
            if (dialogOpen && distance > interactionDistance * 1.5f)
            {
                dialogOpen = false;
                Debug.Log("SpawnNPC dialog closed - player walked away");
            }

            // Face the player when nearby
            if (playerNearby)
            {
                Vector3 dirToPlayer = player.transform.position - transform.position;
                dirToPlayer.y = 0;
                if (dirToPlayer.magnitude > 0.1f)
                {
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        Quaternion.LookRotation(dirToPlayer),
                        Time.deltaTime * 3f
                    );
                }
            }

            // Open dialog when E is pressed
            if (playerNearby && !dialogOpen && Input.GetKeyDown(KeyCode.E))
            {
                dialogOpen = true;
                dialogTimer = dialogDuration;
                PlayHumm(); // Play humm when starting to talk
            }
        }

        // Close dialog after timer or input
        if (dialogOpen)
        {
            dialogTimer -= Time.deltaTime;
            if (dialogTimer <= 0 || Input.GetKeyDown(KeyCode.Escape))
            {
                dialogOpen = false;
            }
        }

        // Idle humming when player is nearby but not in dialog
        if (playerNearby && !dialogOpen)
        {
            hummTimer += Time.deltaTime;
            if (hummTimer >= nextHummTime)
            {
                PlayHumm();
                hummTimer = 0f;
                nextHummTime = Random.Range(3f, 6f);
            }
        }
        else
        {
            hummTimer = 0f;
        }
    }

    void PlayHumm()
    {
        if (audioSource == null || isHumming) return;

        // Generate a procedural humm sound
        StartCoroutine(GenerateHummSound());
    }

    System.Collections.IEnumerator GenerateHummSound()
    {
        isHumming = true;

        // Create a short humm clip
        int sampleRate = 44100;
        float duration = Random.Range(0.4f, 0.8f);
        int sampleCount = (int)(sampleRate * duration);
        AudioClip hummClip = AudioClip.Create("Humm", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        float baseFreq = Random.Range(180f, 220f); // Low humm frequency

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Envelope: fade in and out
            float envelope = Mathf.Sin(progress * Mathf.PI);

            // Multiple harmonics for richer sound
            float wave = 0f;
            wave += Mathf.Sin(2 * Mathf.PI * baseFreq * t) * 0.5f;           // Fundamental
            wave += Mathf.Sin(2 * Mathf.PI * baseFreq * 2f * t) * 0.25f;     // 2nd harmonic
            wave += Mathf.Sin(2 * Mathf.PI * baseFreq * 3f * t) * 0.125f;    // 3rd harmonic

            // Add slight vibrato
            float vibrato = Mathf.Sin(2 * Mathf.PI * 5f * t) * 0.02f;
            wave *= (1f + vibrato);

            samples[i] = wave * envelope * 0.3f;
        }

        hummClip.SetData(samples, 0);

        audioSource.clip = hummClip;
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.Play();

        yield return new WaitForSeconds(duration);

        isHumming = false;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !stylesInitialized) return;

        if (dialogOpen)
        {
            DrawSpeechBubble();
        }
        else if (playerNearby)
        {
            DrawInteractionPrompt();
        }
    }

    void DrawSpeechBubble()
    {
        float bubbleWidth = 420;
        float bubbleHeight = 100;
        float bubbleX = (Screen.width - bubbleWidth) / 2;
        float bubbleY = Screen.height * 0.2f;
        float borderWidth = 4;
        float pointerSize = 20;

        // Draw border (outer rectangle)
        GUI.DrawTexture(new Rect(bubbleX - borderWidth, bubbleY - borderWidth,
            bubbleWidth + borderWidth * 2, bubbleHeight + borderWidth * 2), borderTexture);

        // Draw bubble background
        GUI.DrawTexture(new Rect(bubbleX, bubbleY, bubbleWidth, bubbleHeight), bubbleTexture);

        // Draw pointer triangle (pointing down toward NPC)
        // Simple approach: draw a small rectangle as pointer
        GUI.DrawTexture(new Rect(bubbleX + bubbleWidth / 2 - borderWidth, bubbleY + bubbleHeight,
            borderWidth * 2, pointerSize), borderTexture);
        GUI.DrawTexture(new Rect(bubbleX + bubbleWidth / 2 - borderWidth + 2, bubbleY + bubbleHeight,
            borderWidth * 2 - 4, pointerSize - 2), bubbleTexture);

        // NPC Name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 14;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = new Color(0.4f, 0.25f, 0.1f);
        nameStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(bubbleX, bubbleY + 8, bubbleWidth, 20), "~ Island Greeter ~", nameStyle);

        // Dialog text
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 18;
        textStyle.fontStyle = FontStyle.Italic;
        textStyle.normal.textColor = new Color(0.15f, 0.1f, 0.05f);
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.wordWrap = true;
        textStyle.padding = new RectOffset(20, 20, 0, 0);

        GUI.Label(new Rect(bubbleX, bubbleY + 30, bubbleWidth, 60),
            "\"Wow, you're naked! Go fishing for gold\nand buy yourself some garbs, sailor!\"", textStyle);

        // Close hint
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 12;
        hintStyle.normal.textColor = new Color(0.5f, 0.4f, 0.3f);
        hintStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(bubbleX, bubbleY + bubbleHeight + pointerSize + 5, bubbleWidth, 20),
            "Press ESC to close", hintStyle);
    }

    void DrawInteractionPrompt()
    {
        GUIStyle promptStyle = new GUIStyle();
        promptStyle.fontSize = 18;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.normal.textColor = new Color(1f, 0.95f, 0.7f);
        promptStyle.alignment = TextAnchor.MiddleCenter;

        // Add shadow for visibility
        GUIStyle shadowStyle = new GUIStyle(promptStyle);
        shadowStyle.normal.textColor = new Color(0, 0, 0, 0.7f);

        float promptY = Screen.height * 0.6f;
        string text = "[E] Talk";

        // Shadow
        GUI.Label(new Rect(2, promptY + 2, Screen.width, 30), text, shadowStyle);
        // Main text
        GUI.Label(new Rect(0, promptY, Screen.width, 30), text, promptStyle);
    }

    void OnDestroy()
    {
        if (bubbleTexture != null) Destroy(bubbleTexture);
        if (borderTexture != null) Destroy(borderTexture);
        if (pointerTexture != null) Destroy(pointerTexture);
    }
}
