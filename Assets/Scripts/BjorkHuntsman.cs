using UnityEngine;

/// <summary>
/// Bjork the Huntsman - Quest NPC in Ice Realm
/// Asks player to collect 3 polar bear skins
/// Hidden behind trees by a small fire
/// </summary>
public class BjorkHuntsman : MonoBehaviour
{
    public static BjorkHuntsman Instance { get; private set; }

    // Quest state
    private int bearSkinsCollected = 0;
    private const int REQUIRED_SKINS = 3;
    private bool questComplete = false;
    private bool questRewardGiven = false;

    // Interaction
    private bool playerNearby = false;
    private bool dialogueOpen = false;
    private float interactionDistance = 4f;

    // Audio
    private AudioSource audioSource;

    // Dialogue
    private string[] idleDialogue = {
        "Hmph. You look cold.",
        "The bears here... they're dangerous.",
        "I've been hunting these lands for years...",
        "Watch yourself out there."
    };

    private string[] questDialogue = {
        "Ah, a fellow hunter? I have a task for you.",
        "Bring me 3 Polar Bear Skins.",
        "The bears roam this frozen land. Be careful.",
        "Come back when you have the skins."
    };

    private string[] progressDialogue = {
        "You've got {0} out of {1} skins.",
        "Keep hunting. {2} more to go.",
        "The bears won't hunt themselves!"
    };

    private string[] completeDialogue = {
        "Excellent! You've done well, hunter.",
        "These skins will keep many warm.",
        "Take this reward. You've earned it.",
        "May the frost guide your path."
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 15f;

        CreateBjorkModel();
        CreateCampfire();
    }

    void CreateBjorkModel()
    {
        // Bjork is a burly native huntsman

        Material skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.75f, 0.6f, 0.5f);

        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = new Color(0.4f, 0.35f, 0.3f); // Dark brown fur

        Material beltMat = new Material(Shader.Find("Standard"));
        beltMat.color = new Color(0.25f, 0.15f, 0.1f);

        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.2f, 0.15f, 0.1f);

        // Body (fur coat)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 1f, 0);
        body.transform.localScale = new Vector3(0.6f, 0.7f, 0.4f);
        body.GetComponent<Renderer>().material = furMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 1.85f, 0);
        head.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
        head.GetComponent<Renderer>().material = skinMat;
        Object.Destroy(head.GetComponent<Collider>());

        // Beard
        GameObject beard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beard.name = "Beard";
        beard.transform.SetParent(transform);
        beard.transform.localPosition = new Vector3(0, 1.7f, 0.12f);
        beard.transform.localScale = new Vector3(0.25f, 0.25f, 0.15f);
        beard.GetComponent<Renderer>().material = hairMat;
        Object.Destroy(beard.GetComponent<Collider>());

        // Hair (messy)
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.name = "Hair";
        hair.transform.SetParent(transform);
        hair.transform.localPosition = new Vector3(0, 2f, -0.05f);
        hair.transform.localScale = new Vector3(0.38f, 0.2f, 0.35f);
        hair.GetComponent<Renderer>().material = hairMat;
        Object.Destroy(hair.GetComponent<Collider>());

        // Fur hood
        GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hood.name = "Hood";
        hood.transform.SetParent(transform);
        hood.transform.localPosition = new Vector3(0, 2.05f, -0.1f);
        hood.transform.localScale = new Vector3(0.45f, 0.25f, 0.4f);
        hood.GetComponent<Renderer>().material = furMat;
        Object.Destroy(hood.GetComponent<Collider>());

        // Eyes
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(transform);
            eye.transform.localPosition = new Vector3(side * 0.08f, 1.9f, 0.15f);
            eye.transform.localScale = Vector3.one * 0.05f;

            Material eyeMat = new Material(Shader.Find("Standard"));
            eyeMat.color = new Color(0.3f, 0.4f, 0.5f); // Blue-gray eyes
            eye.GetComponent<Renderer>().material = eyeMat;
            Object.Destroy(eye.GetComponent<Collider>());
        }

        // Belt
        GameObject belt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        belt.name = "Belt";
        belt.transform.SetParent(transform);
        belt.transform.localPosition = new Vector3(0, 0.5f, 0);
        belt.transform.localScale = new Vector3(0.5f, 0.05f, 0.35f);
        belt.GetComponent<Renderer>().material = beltMat;
        Object.Destroy(belt.GetComponent<Collider>());

        // Legs (fur pants)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = "Leg";
            leg.transform.SetParent(transform);
            leg.transform.localPosition = new Vector3(side * 0.15f, 0.25f, 0);
            leg.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
            leg.GetComponent<Renderer>().material = furMat;
            Object.Destroy(leg.GetComponent<Collider>());
        }

        // Spear (held weapon)
        CreateHuntingSpear();
    }

    void CreateHuntingSpear()
    {
        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.5f, 0.35f, 0.2f);

        Material tipMat = new Material(Shader.Find("Standard"));
        tipMat.color = new Color(0.5f, 0.55f, 0.6f);
        tipMat.SetFloat("_Metallic", 0.6f);

        GameObject spear = new GameObject("HuntingSpear");
        spear.transform.SetParent(transform);
        spear.transform.localPosition = new Vector3(0.4f, 0.8f, 0);
        spear.transform.localRotation = Quaternion.Euler(0, 0, -15);

        // Shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(spear.transform);
        shaft.transform.localPosition = Vector3.zero;
        shaft.transform.localScale = new Vector3(0.04f, 1f, 0.04f);
        shaft.GetComponent<Renderer>().material = woodMat;
        Object.Destroy(shaft.GetComponent<Collider>());

        // Tip
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.name = "Tip";
        tip.transform.SetParent(spear.transform);
        tip.transform.localPosition = new Vector3(0, 1.05f, 0);
        tip.transform.localScale = new Vector3(0.05f, 0.15f, 0.02f);
        tip.GetComponent<Renderer>().material = tipMat;
        Object.Destroy(tip.GetComponent<Collider>());
    }

    void CreateCampfire()
    {
        GameObject fire = new GameObject("Campfire");
        fire.transform.SetParent(transform);
        fire.transform.localPosition = new Vector3(0, 0, 1.5f);

        // Log circle
        Material logMat = new Material(Shader.Find("Standard"));
        logMat.color = new Color(0.3f, 0.2f, 0.1f);

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = "Log";
            log.transform.SetParent(fire.transform);
            log.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.3f, 0.08f, Mathf.Sin(angle) * 0.3f);
            log.transform.localRotation = Quaternion.Euler(90, i * 30, 0);
            log.transform.localScale = new Vector3(0.1f, 0.25f, 0.1f);
            log.GetComponent<Renderer>().material = logMat;
            Object.Destroy(log.GetComponent<Collider>());
        }

        // Fire (glowing center)
        Material fireMat = new Material(Shader.Find("Standard"));
        fireMat.color = new Color(1f, 0.5f, 0.1f);
        fireMat.EnableKeyword("_EMISSION");
        fireMat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * 3f);

        GameObject flames = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flames.name = "Flames";
        flames.transform.SetParent(fire.transform);
        flames.transform.localPosition = new Vector3(0, 0.25f, 0);
        flames.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        flames.GetComponent<Renderer>().material = fireMat;
        Object.Destroy(flames.GetComponent<Collider>());

        // Add light
        GameObject lightObj = new GameObject("FireLight");
        lightObj.transform.SetParent(fire.transform);
        lightObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        Light fireLight = lightObj.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.6f, 0.3f);
        fireLight.intensity = 2f;
        fireLight.range = 8f;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerNearby = distance < interactionDistance;

        // Face player when nearby
        if (playerNearby)
        {
            Vector3 lookDir = (player.transform.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(lookDir), Time.deltaTime * 3f);
            }
        }

        if (playerNearby && !dialogueOpen && Input.GetKeyDown(KeyCode.E))
        {
            OpenDialogue();
        }

        if (dialogueOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogue();
        }
    }

    void OpenDialogue()
    {
        dialogueOpen = true;
        PlayGruntSound();
    }

    void CloseDialogue()
    {
        dialogueOpen = false;
    }

    public void AddBearSkin()
    {
        if (questComplete) return;

        bearSkinsCollected++;
        Debug.Log($"Bear skin collected! {bearSkinsCollected}/{REQUIRED_SKINS}");

        if (bearSkinsCollected >= REQUIRED_SKINS)
        {
            questComplete = true;
            Debug.Log("Quest complete! Return to Bjork for reward!");
        }
    }

    public int GetBearSkins()
    {
        return bearSkinsCollected;
    }

    void PlayGruntSound()
    {
        if (audioSource != null)
        {
            AudioClip grunt = CreateGruntClip();
            audioSource.PlayOneShot(grunt, 0.6f);
        }
    }

    AudioClip CreateGruntClip()
    {
        int sampleRate = 44100;
        int samples = sampleRate / 4; // 0.25 second
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t * 4f);
            envelope = Mathf.Max(0, envelope);

            // Low grunt sound
            float freq = 100f + Random.Range(-15f, 15f);
            float wave = Mathf.Sin(2 * Mathf.PI * freq * t);
            wave += (Random.value - 0.5f) * 0.3f * envelope;

            data[i] = wave * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("Grunt", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    void GiveQuestReward()
    {
        if (questRewardGiven) return;
        questRewardGiven = true;

        // Give gold reward
        int reward = 5000;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(reward);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"+{reward} Gold (Quest Complete!)",
                new Color(1f, 0.85f, 0.3f));
        }

        Debug.Log($"Quest reward: {reward} gold!");
    }

    public bool IsDialogueOpen()
    {
        return dialogueOpen;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Interaction prompt
        if (playerNearby && !dialogueOpen)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 18;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.9f, 0.8f, 0.6f);

            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 150, 300, 30),
                "[E] Talk to Bjork", promptStyle);
        }

        // Dialogue window
        if (dialogueOpen)
        {
            DrawDialogueUI();
        }
    }

    void DrawDialogueUI()
    {
        float panelWidth = 500;
        float panelHeight = 300;
        Rect panelRect = new Rect(
            Screen.width / 2 - panelWidth / 2,
            Screen.height / 2 - panelHeight / 2,
            panelWidth,
            panelHeight
        );

        // Background
        GUI.color = new Color(0.15f, 0.12f, 0.1f, 0.95f);
        GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Border
        GUI.color = new Color(0.6f, 0.5f, 0.35f);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y - 2, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y + panelRect.height, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x + panelRect.width, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.9f, 0.8f, 0.6f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + 15, panelRect.width, 30), "BJORK THE HUNTSMAN", titleStyle);

        // Dialogue text
        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 16;
        dialogueStyle.wordWrap = true;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.85f, 0.8f, 0.7f);

        string dialogueText = "";

        if (!questComplete && bearSkinsCollected == 0)
        {
            dialogueText = string.Join("\n\n", questDialogue);
        }
        else if (!questComplete && bearSkinsCollected > 0)
        {
            dialogueText = $"You've got {bearSkinsCollected} out of {REQUIRED_SKINS} skins.\n\n" +
                           $"Keep hunting. {REQUIRED_SKINS - bearSkinsCollected} more to go.\n\n" +
                           "The bears won't hunt themselves!";
        }
        else if (questComplete && !questRewardGiven)
        {
            dialogueText = string.Join("\n\n", completeDialogue);

            // Give reward button
            GUIStyle btnStyle = new GUIStyle();
            btnStyle.fontSize = 14;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.alignment = TextAnchor.MiddleCenter;
            btnStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
            btnStyle.normal.background = Texture2D.whiteTexture;

            Rect btnRect = new Rect(panelRect.x + panelRect.width / 2 - 75, panelRect.y + panelHeight - 80, 150, 35);
            GUI.color = new Color(0.3f, 0.25f, 0.15f);
            GUI.DrawTexture(btnRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (GUI.Button(btnRect, "CLAIM REWARD", btnStyle))
            {
                GiveQuestReward();
            }
        }
        else
        {
            dialogueText = "You've proven yourself a true hunter.\n\n" +
                          "May the frost guide your path.\n\n" +
                          "Perhaps we'll hunt together someday...";
        }

        GUI.Label(new Rect(panelRect.x + 30, panelRect.y + 60, panelRect.width - 60, panelHeight - 120), dialogueText, dialogueStyle);

        // Quest tracker
        GUIStyle questStyle = new GUIStyle();
        questStyle.fontSize = 14;
        questStyle.fontStyle = FontStyle.Bold;
        questStyle.alignment = TextAnchor.MiddleCenter;
        questStyle.normal.textColor = questComplete ? new Color(0.3f, 1f, 0.5f) : new Color(0.9f, 0.7f, 0.4f);

        string questText = questComplete ?
            "QUEST COMPLETE!" :
            $"Bear Skins: {bearSkinsCollected}/{REQUIRED_SKINS}";

        GUI.Label(new Rect(panelRect.x, panelRect.y + panelHeight - 45, panelRect.width, 25), questText, questStyle);

        // Close instruction
        GUIStyle closeStyle = new GUIStyle();
        closeStyle.fontSize = 11;
        closeStyle.alignment = TextAnchor.MiddleCenter;
        closeStyle.normal.textColor = new Color(0.5f, 0.45f, 0.4f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + panelHeight - 25, panelRect.width, 20),
            "[ESC] Close", closeStyle);
    }
}
