using UnityEngine;
using System.Collections;

/// <summary>
/// Goldie Banks - Rastafarian NPC on the small island at the end of the bridge
/// Sits in a chair, smokes, sips a Banks beer, has dreadlocks
/// Quest: Find his hidden bag of weeds on the island (1000 XP, 500 gold)
/// </summary>
public class GoldieBanksNPC : MonoBehaviour
{
    public static GoldieBanksNPC Instance { get; private set; }

    // Quest state
    public bool questActive = false;
    public bool questComplete = false;
    public bool weedBagFound = false;

    // UI State
    private bool playerNearby = false;
    private bool dialogOpen = false;
    private float interactionDistance = 5f;
    private float dialogTimer = 0f;

    // Visual components
    private GameObject head;
    private GameObject body;
    private GameObject chair;
    private GameObject beerBottle;
    private GameObject smokeParticle;
    private Transform leftHand;
    private Transform rightHand;

    // Materials
    private Material skinMat;
    private Material hairMat;
    private Material clothesMat;
    private Material chairMat;

    // Animation
    private float sipTimer = 0f;
    private float smokeTimer = 0f;
    private bool isSipping = false;

    // Audio
    private AudioSource audioSource;

    // Textures for UI
    private Texture2D bubbleTexture;
    private Texture2D borderTexture;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateNPC();
        SetupAudio();
        CreateUITextures();
        StartCoroutine(SmokeEffect());
    }

    void CreateUITextures()
    {
        bubbleTexture = new Texture2D(1, 1);
        bubbleTexture.SetPixel(0, 0, new Color(0.1f, 0.08f, 0.06f, 0.95f));
        bubbleTexture.Apply();

        borderTexture = new Texture2D(1, 1);
        borderTexture.SetPixel(0, 0, new Color(0.4f, 0.6f, 0.3f, 1f)); // Rasta green border
        borderTexture.Apply();
    }

    void CreateNPC()
    {
        // Materials
        skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.35f, 0.22f, 0.12f); // Dark skin

        hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.08f, 0.06f, 0.04f); // Black dreadlocks

        clothesMat = new Material(Shader.Find("Standard"));
        clothesMat.color = new Color(0.2f, 0.5f, 0.15f); // Green/yellow/red rasta colors

        chairMat = new Material(Shader.Find("Standard"));
        chairMat.color = new Color(0.4f, 0.25f, 0.1f); // Wood

        // Create chair
        chair = new GameObject("Chair");
        chair.transform.SetParent(transform);
        chair.transform.localPosition = Vector3.zero;

        // Chair seat
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.transform.SetParent(chair.transform);
        seat.transform.localPosition = new Vector3(0, 0.3f, 0);
        seat.transform.localScale = new Vector3(0.6f, 0.1f, 0.5f);
        seat.GetComponent<Renderer>().sharedMaterial = chairMat;
        Object.Destroy(seat.GetComponent<Collider>());

        // Chair back
        GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.transform.SetParent(chair.transform);
        back.transform.localPosition = new Vector3(0, 0.65f, -0.2f);
        back.transform.localScale = new Vector3(0.6f, 0.6f, 0.08f);
        back.GetComponent<Renderer>().sharedMaterial = chairMat;
        Object.Destroy(back.GetComponent<Collider>());

        // Chair legs
        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.transform.SetParent(chair.transform);
            float x = (i % 2 == 0) ? -0.22f : 0.22f;
            float z = (i < 2) ? -0.18f : 0.18f;
            leg.transform.localPosition = new Vector3(x, 0.15f, z);
            leg.transform.localScale = new Vector3(0.06f, 0.3f, 0.06f);
            leg.GetComponent<Renderer>().sharedMaterial = chairMat;
            Object.Destroy(leg.GetComponent<Collider>());
        }

        // Body (sitting)
        body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 0.6f, 0);
        body.transform.localScale = new Vector3(0.4f, 0.35f, 0.3f);
        body.GetComponent<Renderer>().sharedMaterial = clothesMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Head
        head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 1.1f, 0);
        head.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.Destroy(head.GetComponent<Collider>());

        // Dreadlocks
        CreateDreadlocks();

        // Arms
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArm.transform.SetParent(transform);
        leftArm.transform.localPosition = new Vector3(-0.25f, 0.7f, 0.1f);
        leftArm.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
        leftArm.transform.localRotation = Quaternion.Euler(30, 0, 20);
        leftArm.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.Destroy(leftArm.GetComponent<Collider>());
        leftHand = leftArm.transform;

        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArm.transform.SetParent(transform);
        rightArm.transform.localPosition = new Vector3(0.25f, 0.7f, 0.1f);
        rightArm.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
        rightArm.transform.localRotation = Quaternion.Euler(30, 0, -20);
        rightArm.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.Destroy(rightArm.GetComponent<Collider>());
        rightHand = rightArm.transform;

        // Beer bottle in left hand
        CreateBeerBottle();

        // Legs (bent, sitting)
        GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLeg.transform.SetParent(transform);
        leftLeg.transform.localPosition = new Vector3(-0.12f, 0.35f, 0.15f);
        leftLeg.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
        leftLeg.transform.localRotation = Quaternion.Euler(70, 0, 0);
        leftLeg.GetComponent<Renderer>().sharedMaterial = clothesMat;
        Object.Destroy(leftLeg.GetComponent<Collider>());

        GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLeg.transform.SetParent(transform);
        rightLeg.transform.localPosition = new Vector3(0.12f, 0.35f, 0.15f);
        rightLeg.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
        rightLeg.transform.localRotation = Quaternion.Euler(70, 0, 0);
        rightLeg.GetComponent<Renderer>().sharedMaterial = clothesMat;
        Object.Destroy(rightLeg.GetComponent<Collider>());

        // Empty bottles around
        CreateEmptyBottles();
    }

    void CreateDreadlocks()
    {
        // Multiple dreadlocks hanging from head
        int dreadCount = 12;
        for (int i = 0; i < dreadCount; i++)
        {
            float angle = (360f / dreadCount) * i * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * 0.12f;
            float z = Mathf.Sin(angle) * 0.12f;

            GameObject dread = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dread.name = "Dreadlock";
            dread.transform.SetParent(head.transform);
            dread.transform.localPosition = new Vector3(x, -0.3f, z);
            dread.transform.localScale = new Vector3(0.08f, 0.25f, 0.08f);
            dread.transform.localRotation = Quaternion.Euler(Random.Range(10f, 30f), angle * Mathf.Rad2Deg, 0);
            dread.GetComponent<Renderer>().sharedMaterial = hairMat;
            Object.Destroy(dread.GetComponent<Collider>());
        }
    }

    void CreateBeerBottle()
    {
        beerBottle = new GameObject("BeerBottle");
        beerBottle.transform.SetParent(leftHand);
        beerBottle.transform.localPosition = new Vector3(0, -0.15f, 0.05f);

        Material bottleMat = new Material(Shader.Find("Standard"));
        bottleMat.color = new Color(0.3f, 0.2f, 0.1f); // Brown bottle

        // Bottle body
        GameObject bottleBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bottleBody.transform.SetParent(beerBottle.transform);
        bottleBody.transform.localPosition = Vector3.zero;
        bottleBody.transform.localScale = new Vector3(0.06f, 0.12f, 0.06f);
        bottleBody.GetComponent<Renderer>().sharedMaterial = bottleMat;
        Object.Destroy(bottleBody.GetComponent<Collider>());

        // Bottle neck
        GameObject bottleNeck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bottleNeck.transform.SetParent(beerBottle.transform);
        bottleNeck.transform.localPosition = new Vector3(0, 0.1f, 0);
        bottleNeck.transform.localScale = new Vector3(0.025f, 0.05f, 0.025f);
        bottleNeck.GetComponent<Renderer>().sharedMaterial = bottleMat;
        Object.Destroy(bottleNeck.GetComponent<Collider>());
    }

    void CreateEmptyBottles()
    {
        Material emptyBottleMat = new Material(Shader.Find("Standard"));
        emptyBottleMat.color = new Color(0.25f, 0.15f, 0.08f);

        // Scatter empty bottles around chair
        Vector3[] bottlePositions = new Vector3[]
        {
            new Vector3(-0.5f, 0.05f, 0.3f),
            new Vector3(0.6f, 0.05f, 0.1f),
            new Vector3(-0.3f, 0.05f, -0.4f),
            new Vector3(0.4f, 0.05f, 0.5f),
            new Vector3(-0.7f, 0.05f, -0.1f)
        };

        foreach (var pos in bottlePositions)
        {
            GameObject bottle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottle.name = "EmptyBottle";
            bottle.transform.SetParent(transform);
            bottle.transform.localPosition = pos;
            bottle.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
            bottle.transform.localRotation = Quaternion.Euler(Random.Range(60f, 90f), Random.Range(0f, 360f), 0);
            bottle.GetComponent<Renderer>().sharedMaterial = emptyBottleMat;
            Object.Destroy(bottle.GetComponent<Collider>());
        }
    }

    IEnumerator SmokeEffect()
    {
        Material smokeMat = new Material(Shader.Find("Standard"));
        smokeMat.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        smokeMat.SetFloat("_Mode", 3); // Transparent
        smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            if (!MainMenu.GameStarted) continue;

            // Create smoke puff
            GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.name = "SmokePuff";
            smoke.transform.position = head.transform.position + Vector3.up * 0.3f + Vector3.forward * 0.15f;
            smoke.transform.localScale = Vector3.one * 0.1f;
            smoke.GetComponent<Renderer>().sharedMaterial = smokeMat;
            Object.Destroy(smoke.GetComponent<Collider>());

            // Animate smoke rising
            StartCoroutine(AnimateSmoke(smoke));
        }
    }

    IEnumerator AnimateSmoke(GameObject smoke)
    {
        float duration = 3f;
        float elapsed = 0f;
        Vector3 startPos = smoke.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Rise and expand
            smoke.transform.position = startPos + Vector3.up * t * 2f + new Vector3(Mathf.Sin(t * 5f) * 0.1f, 0, 0);
            smoke.transform.localScale = Vector3.one * (0.1f + t * 0.3f);

            // Fade out
            Color c = smoke.GetComponent<Renderer>().material.color;
            c.a = 0.5f * (1f - t);
            smoke.GetComponent<Renderer>().material.color = c;

            yield return null;
        }

        Destroy(smoke);
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.8f;
        audioSource.volume = 0.4f;
        audioSource.playOnAwake = false;
    }

    void PlayGreeting()
    {
        // Simple "Yeah mon" sound
        int sampleRate = 44100;
        float duration = 0.8f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("YeahMon", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Sin(t / duration * Mathf.PI);
            float freq = 120f + Mathf.Sin(t * 3f) * 20f; // Deep voice with variation
            samples[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.3f;
        }

        clip.SetData(samples, 0);
        audioSource.clip = clip;
        audioSource.Play();
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

            // Open dialog when E is pressed
            if (playerNearby && !dialogOpen && Input.GetKeyDown(KeyCode.E))
            {
                dialogOpen = true;
                dialogTimer = 15f;
                PlayGreeting();
                // Enable fish selling
                if (FishInventoryPanel.Instance != null)
                {
                    FishInventoryPanel.Instance.EnableSellMode("Goldie Banks");
                }
            }
        }

        // Handle dialog
        if (dialogOpen)
        {
            dialogTimer -= Time.deltaTime;
            if (dialogTimer <= 0 || Input.GetKeyDown(KeyCode.Escape))
            {
                dialogOpen = false;
                if (FishInventoryPanel.Instance != null)
                {
                    FishInventoryPanel.Instance.DisableSellMode();
                }
            }
        }

        // Sipping animation
        sipTimer += Time.deltaTime;
        if (sipTimer > 8f && !isSipping)
        {
            StartCoroutine(SipBeer());
            sipTimer = 0f;
        }
    }

    IEnumerator SipBeer()
    {
        isSipping = true;
        float sipDuration = 1.5f;
        float elapsed = 0f;
        Vector3 startPos = leftHand.localPosition;
        Quaternion startRot = leftHand.localRotation;

        // Bring bottle to mouth
        while (elapsed < sipDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (sipDuration / 2);
            leftHand.localPosition = Vector3.Lerp(startPos, new Vector3(-0.1f, 0.95f, 0.15f), t);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(0.5f);

        // Return
        elapsed = 0f;
        while (elapsed < sipDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (sipDuration / 2);
            leftHand.localPosition = Vector3.Lerp(new Vector3(-0.1f, 0.95f, 0.15f), startPos, t);
            yield return null;
        }

        leftHand.localPosition = startPos;
        isSipping = false;
    }

    public void OnWeedBagFound()
    {
        if (questActive && !weedBagFound)
        {
            weedBagFound = true;
            questComplete = true;
            questActive = false;

            // Reward
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(500);
            }
            if (LevelingSystem.Instance != null)
            {
                LevelingSystem.Instance.AddXP(1000);
            }
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Quest Complete! +1000 XP, +500 Gold!", new Color(0.4f, 1f, 0.5f));
            }
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        if (playerNearby && !dialogOpen)
        {
            DrawInteractionPrompt();
        }

        if (dialogOpen)
        {
            DrawDialog();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle promptStyle = new GUIStyle();
        promptStyle.fontSize = 18;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.alignment = TextAnchor.MiddleCenter;
        promptStyle.normal.textColor = new Color(0.4f, 0.8f, 0.3f); // Rasta green

        float promptY = Screen.height * 0.7f;
        GUI.Label(new Rect(0, promptY, Screen.width, 30), "Press E to talk to Goldie Banks", promptStyle);
    }

    void DrawDialog()
    {
        float panelWidth = 400;
        float panelHeight = 200;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = Screen.height - panelHeight - 100;

        // Border
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), borderTexture);
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), bubbleTexture);

        // Name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 16;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = new Color(1f, 0.8f, 0.2f); // Gold
        GUI.Label(new Rect(panelX + 15, panelY + 10, 200, 25), "Goldie Banks", nameStyle);

        // Red X close button
        GUIStyle xStyle = new GUIStyle();
        xStyle.fontSize = 14;
        xStyle.fontStyle = FontStyle.Bold;
        xStyle.alignment = TextAnchor.MiddleCenter;
        xStyle.normal.textColor = Color.white;
        GUI.DrawTexture(new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22), borderTexture);
        if (GUI.Button(new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22), "X", xStyle))
        {
            dialogOpen = false;
        }

        // Dialog text
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 14;
        textStyle.wordWrap = true;
        textStyle.normal.textColor = Color.white;

        string dialogText = "";

        if (!questActive && !questComplete)
        {
            dialogText = "Yeah mon! Welcome to my little paradise, ya know?\n\n" +
                        "I seem to have lost my bag of... herbs... somewhere on this island.\n" +
                        "Find it for me and I'll make it worth your while, seen?";

            GUI.Label(new Rect(panelX + 15, panelY + 40, panelWidth - 30, 100), dialogText, textStyle);

            // Accept quest button
            GUIStyle btnStyle = new GUIStyle();
            btnStyle.fontSize = 14;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.alignment = TextAnchor.MiddleCenter;
            btnStyle.normal.textColor = Color.white;

            Texture2D btnTex = new Texture2D(1, 1);
            btnTex.SetPixel(0, 0, new Color(0.3f, 0.6f, 0.2f));
            btnTex.Apply();

            GUI.DrawTexture(new Rect(panelX + panelWidth / 2 - 60, panelY + panelHeight - 45, 120, 30), btnTex);
            if (GUI.Button(new Rect(panelX + panelWidth / 2 - 60, panelY + panelHeight - 45, 120, 30), "Accept Quest", btnStyle))
            {
                questActive = true;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Quest: Find Goldie's Bag", new Color(0.4f, 0.8f, 0.3f));
                }
            }
        }
        else if (questActive && !weedBagFound)
        {
            dialogText = "Still looking for my bag, mon?\n\n" +
                        "It's somewhere on this island... look around the bushes and shack.\n" +
                        "Bring it back to me, irie?";
            GUI.Label(new Rect(panelX + 15, panelY + 40, panelWidth - 30, 100), dialogText, textStyle);
        }
        else if (questComplete)
        {
            dialogText = "IRIE! You found it, big up yourself!\n\n" +
                        "Here's your reward, mon. Respect!\n" +
                        "Feel free to sell your fish to me anytime.";
            GUI.Label(new Rect(panelX + 15, panelY + 40, panelWidth - 30, 100), dialogText, textStyle);
        }
    }

    void OnDestroy()
    {
        if (bubbleTexture != null) Destroy(bubbleTexture);
        if (borderTexture != null) Destroy(borderTexture);
    }
}
