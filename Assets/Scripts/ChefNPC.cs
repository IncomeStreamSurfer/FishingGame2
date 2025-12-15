using UnityEngine;
using System.Collections.Generic;

public class ChefNPC : MonoBehaviour
{
    public static ChefNPC Instance { get; private set; }

    // Quest state
    private bool questStarted = false;
    private bool playerNearby = false;
    private float interactionRange = 4f;

    // Current active quest
    private string currentQuestFishId = null;
    private string currentQuestFishName = null;

    // UI state
    private bool showingDialogue = false;
    private int dialogueState = 0; // 0 = initial, 1 = quest selection, 2 = active quest, 3 = quest complete
    private float dialogueTimer = 0f;

    // Visuals
    private GameObject chefBody;
    private GameObject chefHead;
    private GameObject chefHat;
    private GameObject apron;
    private GameObject cookingFire;
    private GameObject pot;
    private GameObject steam;
    private ParticleSystem steamParticles;

    // Audio
    private AudioSource bubbleAudioSource;

    // Cached references
    private Transform playerTransform;
    private int guiFrameSkip = 0;

    // Cached textures
    private Texture2D dialogueBgTex;
    private Texture2D buttonTex;
    private Texture2D buttonHoverTex;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateChefVisuals();
        CreateCookingStation();
        SetupAudio();
        CreateCachedTextures();
        LoadQuestState();
    }

    void CreateCachedTextures()
    {
        dialogueBgTex = new Texture2D(1, 1);
        dialogueBgTex.SetPixel(0, 0, new Color(0.1f, 0.08f, 0.06f, 0.95f));
        dialogueBgTex.Apply();

        buttonTex = new Texture2D(1, 1);
        buttonTex.SetPixel(0, 0, new Color(0.3f, 0.25f, 0.2f, 1f));
        buttonTex.Apply();

        buttonHoverTex = new Texture2D(1, 1);
        buttonHoverTex.SetPixel(0, 0, new Color(0.5f, 0.4f, 0.3f, 1f));
        buttonHoverTex.Apply();
    }

    void CreateChefVisuals()
    {
        // Chef body (white apron over dark clothes)
        chefBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        chefBody.name = "ChefBody";
        chefBody.transform.SetParent(transform);
        chefBody.transform.localPosition = new Vector3(0, 1f, 0);
        chefBody.transform.localScale = new Vector3(0.8f, 1f, 0.5f);
        Object.Destroy(chefBody.GetComponent<Collider>());

        Material bodyMat = new Material(Shader.Find("Standard"));
        bodyMat.color = new Color(0.2f, 0.2f, 0.25f); // Dark clothes
        chefBody.GetComponent<Renderer>().material = bodyMat;

        // White apron (front of body)
        apron = GameObject.CreatePrimitive(PrimitiveType.Cube);
        apron.name = "Apron";
        apron.transform.SetParent(transform);
        apron.transform.localPosition = new Vector3(0, 0.9f, 0.2f);
        apron.transform.localScale = new Vector3(0.7f, 1.1f, 0.1f);
        Object.Destroy(apron.GetComponent<Collider>());

        Material apronMat = new Material(Shader.Find("Standard"));
        apronMat.color = new Color(0.95f, 0.95f, 0.9f); // White
        apron.GetComponent<Renderer>().material = apronMat;

        // Chef head
        chefHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        chefHead.name = "ChefHead";
        chefHead.transform.SetParent(transform);
        chefHead.transform.localPosition = new Vector3(0, 2.1f, 0);
        chefHead.transform.localScale = new Vector3(0.5f, 0.55f, 0.5f);
        Object.Destroy(chefHead.GetComponent<Collider>());

        Material headMat = new Material(Shader.Find("Standard"));
        headMat.color = new Color(0.9f, 0.75f, 0.6f); // Skin tone
        chefHead.GetComponent<Renderer>().material = headMat;

        // White chef hat (toque)
        chefHat = new GameObject("ChefHat");
        chefHat.transform.SetParent(transform);
        chefHat.transform.localPosition = new Vector3(0, 2.5f, 0);

        // Hat base (cylinder)
        GameObject hatBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hatBase.name = "HatBase";
        hatBase.transform.SetParent(chefHat.transform);
        hatBase.transform.localPosition = Vector3.zero;
        hatBase.transform.localScale = new Vector3(0.45f, 0.08f, 0.45f);
        Object.Destroy(hatBase.GetComponent<Collider>());

        Material hatMat = new Material(Shader.Find("Standard"));
        hatMat.color = new Color(0.98f, 0.98f, 0.95f); // Bright white
        hatBase.GetComponent<Renderer>().material = hatMat;

        // Hat poof (tall part)
        GameObject hatTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hatTop.name = "HatTop";
        hatTop.transform.SetParent(chefHat.transform);
        hatTop.transform.localPosition = new Vector3(0, 0.25f, 0);
        hatTop.transform.localScale = new Vector3(0.4f, 0.25f, 0.4f);
        Object.Destroy(hatTop.GetComponent<Collider>());
        hatTop.GetComponent<Renderer>().material = hatMat;

        // Hat crown (sphere on top for poofy look)
        GameObject hatCrown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hatCrown.name = "HatCrown";
        hatCrown.transform.SetParent(chefHat.transform);
        hatCrown.transform.localPosition = new Vector3(0, 0.45f, 0);
        hatCrown.transform.localScale = new Vector3(0.42f, 0.2f, 0.42f);
        Object.Destroy(hatCrown.GetComponent<Collider>());
        hatCrown.GetComponent<Renderer>().material = hatMat;

        // Eyes
        CreateEye(-0.1f);
        CreateEye(0.1f);

        // Mustache
        GameObject mustache = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mustache.name = "Mustache";
        mustache.transform.SetParent(chefHead.transform);
        mustache.transform.localPosition = new Vector3(0, -0.15f, 0.45f);
        mustache.transform.localScale = new Vector3(0.5f, 0.08f, 0.1f);
        Object.Destroy(mustache.GetComponent<Collider>());

        Material mustacheMat = new Material(Shader.Find("Standard"));
        mustacheMat.color = new Color(0.3f, 0.25f, 0.2f); // Brown
        mustache.GetComponent<Renderer>().material = mustacheMat;
    }

    void CreateEye(float xOffset)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Eye";
        eye.transform.SetParent(chefHead.transform);
        eye.transform.localPosition = new Vector3(xOffset, 0.1f, 0.4f);
        eye.transform.localScale = new Vector3(0.15f, 0.15f, 0.1f);
        Object.Destroy(eye.GetComponent<Collider>());

        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = Color.white;
        eye.GetComponent<Renderer>().material = eyeMat;

        // Pupil
        GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pupil.name = "Pupil";
        pupil.transform.SetParent(eye.transform);
        pupil.transform.localPosition = new Vector3(0, 0, 0.3f);
        pupil.transform.localScale = new Vector3(0.5f, 0.5f, 0.3f);
        Object.Destroy(pupil.GetComponent<Collider>());

        Material pupilMat = new Material(Shader.Find("Standard"));
        pupilMat.color = new Color(0.2f, 0.15f, 0.1f);
        pupil.GetComponent<Renderer>().material = pupilMat;
    }

    void CreateCookingStation()
    {
        // Cooking fire base (stones)
        GameObject fireBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fireBase.name = "FireBase";
        fireBase.transform.SetParent(transform);
        fireBase.transform.localPosition = new Vector3(1.5f, 0.2f, 0);
        fireBase.transform.localScale = new Vector3(1.2f, 0.3f, 1.2f);
        Object.Destroy(fireBase.GetComponent<Collider>());

        Material stoneMat = new Material(Shader.Find("Standard"));
        stoneMat.color = new Color(0.4f, 0.35f, 0.3f);
        fireBase.GetComponent<Renderer>().material = stoneMat;

        // Fire (glowing embers)
        cookingFire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cookingFire.name = "CookingFire";
        cookingFire.transform.SetParent(transform);
        cookingFire.transform.localPosition = new Vector3(1.5f, 0.4f, 0);
        cookingFire.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
        Object.Destroy(cookingFire.GetComponent<Collider>());

        Material fireMat = new Material(Shader.Find("Standard"));
        fireMat.color = new Color(1f, 0.4f, 0.1f);
        fireMat.EnableKeyword("_EMISSION");
        fireMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 2f);
        cookingFire.GetComponent<Renderer>().material = fireMat;

        // Add fire light
        GameObject fireLight = new GameObject("FireLight");
        fireLight.transform.SetParent(cookingFire.transform);
        fireLight.transform.localPosition = new Vector3(0, 0.5f, 0);
        Light light = fireLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.6f, 0.2f);
        light.intensity = 1.5f;
        light.range = 5f;

        // Pot support (tripod)
        for (int i = 0; i < 3; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"TripodLeg{i}";
            leg.transform.SetParent(transform);
            float angle = i * 120f * Mathf.Deg2Rad;
            leg.transform.localPosition = new Vector3(1.5f + Mathf.Sin(angle) * 0.4f, 0.7f, Mathf.Cos(angle) * 0.4f);
            leg.transform.localRotation = Quaternion.Euler(Mathf.Cos(angle) * 15f, 0, Mathf.Sin(angle) * 15f);
            leg.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
            Object.Destroy(leg.GetComponent<Collider>());

            Material legMat = new Material(Shader.Find("Standard"));
            legMat.color = new Color(0.15f, 0.12f, 0.1f);
            leg.GetComponent<Renderer>().material = legMat;
        }

        // Cooking pot
        pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pot.name = "CookingPot";
        pot.transform.SetParent(transform);
        pot.transform.localPosition = new Vector3(1.5f, 1f, 0);
        pot.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);
        Object.Destroy(pot.GetComponent<Collider>());

        Material potMat = new Material(Shader.Find("Standard"));
        potMat.color = new Color(0.2f, 0.2f, 0.22f);
        potMat.SetFloat("_Metallic", 0.7f);
        potMat.SetFloat("_Glossiness", 0.5f);
        pot.GetComponent<Renderer>().material = potMat;

        // Pot contents (soup/stew)
        GameObject potContents = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        potContents.name = "PotContents";
        potContents.transform.SetParent(pot.transform);
        potContents.transform.localPosition = new Vector3(0, 0.35f, 0);
        potContents.transform.localScale = new Vector3(0.85f, 0.1f, 0.85f);
        Object.Destroy(potContents.GetComponent<Collider>());

        Material soupMat = new Material(Shader.Find("Standard"));
        soupMat.color = new Color(0.6f, 0.4f, 0.2f);
        potContents.GetComponent<Renderer>().material = soupMat;

        // Steam particles
        CreateSteamEffect();
    }

    void CreateSteamEffect()
    {
        steam = new GameObject("Steam");
        steam.transform.SetParent(pot.transform);
        steam.transform.localPosition = new Vector3(0, 0.5f, 0);

        steamParticles = steam.AddComponent<ParticleSystem>();
        var main = steamParticles.main;
        main.startLifetime = 2f;
        main.startSpeed = 0.5f;
        main.startSize = 0.15f;
        main.startColor = new Color(0.9f, 0.9f, 0.9f, 0.3f);
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = steamParticles.emission;
        emission.rateOverTime = 10f;

        var shape = steamParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        var colorOverLifetime = steamParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.3f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = steamParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 2f);

        var velocityOverLifetime = steamParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.y = 0.8f;

        // Use default particle material
        var renderer = steam.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(1f, 1f, 1f, 0.3f);
    }

    void SetupAudio()
    {
        bubbleAudioSource = gameObject.AddComponent<AudioSource>();
        bubbleAudioSource.spatialBlend = 1f; // 3D sound
        bubbleAudioSource.minDistance = 2f;
        bubbleAudioSource.maxDistance = 10f;
        bubbleAudioSource.volume = 0.3f;
        bubbleAudioSource.loop = true;
        bubbleAudioSource.playOnAwake = true;

        // Generate bubbling sound
        int sampleRate = 44100;
        float duration = 2f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Random bubbles
            float bubble = 0f;
            for (int b = 0; b < 5; b++)
            {
                float bubbleFreq = 80f + Mathf.Sin(t * 3f + b) * 40f;
                float bubbleAmp = Mathf.Max(0, Mathf.Sin(t * (2f + b * 0.5f)) * 0.3f);
                bubble += Mathf.Sin(2f * Mathf.PI * bubbleFreq * t) * bubbleAmp;
            }

            // Low rumble
            float rumble = Mathf.Sin(2f * Mathf.PI * 50f * t) * 0.1f;

            samples[i] = (bubble + rumble) * 0.2f;
        }

        AudioClip bubbleClip = AudioClip.Create("Bubbling", sampleCount, 1, sampleRate, false);
        bubbleClip.SetData(samples, 0);
        bubbleAudioSource.clip = bubbleClip;
        bubbleAudioSource.Play();
    }

    void Update()
    {
        // Get player reference
        if (playerTransform == null && GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        if (playerTransform == null) return;

        // Check distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        playerNearby = distance <= interactionRange;

        // F key to interact
        if (playerNearby && Input.GetKeyDown(KeyCode.F) && MainMenu.GameStarted)
        {
            if (!showingDialogue)
            {
                OpenDialogue();
            }
        }

        // ESC to close dialogue
        if (showingDialogue && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogue();
        }

        // Animate fire flickering
        if (cookingFire != null)
        {
            float flicker = 1f + Mathf.Sin(Time.time * 10f) * 0.1f + Mathf.Sin(Time.time * 15f) * 0.05f;
            cookingFire.transform.localScale = new Vector3(0.8f * flicker, 0.4f * flicker, 0.8f * flicker);
        }

        // Look at player when nearby
        if (playerNearby && chefBody != null)
        {
            Vector3 lookDir = playerTransform.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 3f);
            }
        }
    }

    void OpenDialogue()
    {
        showingDialogue = true;

        // Determine dialogue state
        if (!questStarted)
        {
            dialogueState = 0; // Initial greeting
        }
        else if (currentQuestFishId != null)
        {
            // Check if player has the fish
            if (FishBuffSystem.Instance != null && FishBuffSystem.Instance.HasRequiredFish(currentQuestFishId))
            {
                dialogueState = 3; // Quest complete
            }
            else
            {
                dialogueState = 2; // Active quest
            }
        }
        else
        {
            dialogueState = 1; // Quest selection
        }
    }

    void CloseDialogue()
    {
        showingDialogue = false;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Show interaction prompt
        if (playerNearby && !showingDialogue)
        {
            DrawInteractionPrompt();
        }

        // Show dialogue
        if (showingDialogue)
        {
            DrawDialogue();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle promptStyle = new GUIStyle();
        promptStyle.fontSize = 16;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.alignment = TextAnchor.MiddleCenter;
        promptStyle.normal.textColor = new Color(1f, 0.9f, 0.7f);

        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 120, 200, 30), "Chef Gusteau", promptStyle);

        promptStyle.fontSize = 14;
        promptStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 95, 200, 25), "[F] Talk", promptStyle);
    }

    void DrawDialogue()
    {
        // Dark background overlay
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float panelWidth = 500;
        float panelHeight = 350;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Panel background
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), dialogueBgTex);

        // Border
        GUI.color = new Color(0.6f, 0.5f, 0.3f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX, panelY + panelHeight - 3, panelWidth, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX, panelY, 3, panelHeight), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX + panelWidth - 3, panelY, 3, panelHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.9f, 0.7f);
        GUI.Label(new Rect(panelX, panelY + 15, panelWidth, 30), "Chef Gusteau", titleStyle);

        // Close button
        if (GUI.Button(new Rect(panelX + panelWidth - 35, panelY + 10, 25, 25), "X"))
        {
            CloseDialogue();
        }

        // Draw based on dialogue state
        switch (dialogueState)
        {
            case 0:
                DrawInitialDialogue(panelX, panelY, panelWidth, panelHeight);
                break;
            case 1:
                DrawQuestSelection(panelX, panelY, panelWidth, panelHeight);
                break;
            case 2:
                DrawActiveQuest(panelX, panelY, panelWidth, panelHeight);
                break;
            case 3:
                DrawQuestComplete(panelX, panelY, panelWidth, panelHeight);
                break;
        }
    }

    void DrawInitialDialogue(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 16;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        dialogueStyle.wordWrap = true;

        GUI.Label(new Rect(panelX + 30, panelY + 60, panelWidth - 60, 100),
            "\"I've been waiting for you, quick caster! I've seen you slingin' that rod of yours...\n\nDo you want a job?\"",
            dialogueStyle);

        // Accept button
        if (DrawButton(new Rect(panelX + panelWidth / 2 - 80, panelY + panelHeight - 80, 160, 40), "Accept Job"))
        {
            questStarted = true;
            dialogueState = 1;
            SaveQuestState();
        }

        // Decline button
        if (DrawButton(new Rect(panelX + panelWidth / 2 - 60, panelY + panelHeight - 35, 120, 30), "Maybe Later"))
        {
            CloseDialogue();
        }
    }

    void DrawQuestSelection(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 14;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        dialogueStyle.wordWrap = true;

        GUI.Label(new Rect(panelX + 20, panelY + 55, panelWidth - 40, 40),
            "\"Bring me a special fish and I'll cook you something magical!\"",
            dialogueStyle);

        // List available quests
        float questY = panelY + 100;
        float questHeight = 38;

        GUIStyle questStyle = new GUIStyle();
        questStyle.fontSize = 13;
        questStyle.alignment = TextAnchor.MiddleLeft;

        if (FishBuffSystem.Instance != null)
        {
            foreach (var buff in FishBuffSystem.Instance.allBuffs)
            {
                bool completed = FishBuffSystem.Instance.IsQuestCompleted(buff.requiredFishId);

                // Quest button/label
                Rect questRect = new Rect(panelX + 25, questY, panelWidth - 50, questHeight);

                if (completed)
                {
                    // Completed quest - greyed out
                    questStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                    GUI.Label(questRect, $"[DONE] Catch {buff.requiredFishName} -> {buff.buffName}", questStyle);
                }
                else
                {
                    // Available quest
                    GUI.color = new Color(0.3f, 0.25f, 0.2f);
                    GUI.DrawTexture(questRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;

                    questStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);
                    GUI.Label(new Rect(questRect.x + 10, questRect.y, questRect.width - 20, questRect.height),
                        $"Catch {buff.requiredFishName} -> {buff.buffName}", questStyle);

                    if (GUI.Button(questRect, "", GUIStyle.none))
                    {
                        currentQuestFishId = buff.requiredFishId;
                        currentQuestFishName = buff.requiredFishName;
                        dialogueState = 2;
                        SaveQuestState();
                    }
                }

                questY += questHeight + 5;
            }
        }
    }

    void DrawActiveQuest(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 16;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        dialogueStyle.wordWrap = true;

        FishBuff currentBuff = null;
        if (FishBuffSystem.Instance != null)
        {
            currentBuff = FishBuffSystem.Instance.GetBuffByFishId(currentQuestFishId);
        }

        GUI.Label(new Rect(panelX + 30, panelY + 60, panelWidth - 60, 60),
            $"\"Bring me a {currentQuestFishName}!\"",
            dialogueStyle);

        dialogueStyle.fontSize = 14;
        dialogueStyle.normal.textColor = new Color(0.7f, 0.7f, 0.6f);
        GUI.Label(new Rect(panelX + 30, panelY + 130, panelWidth - 60, 60),
            $"Reward: {currentBuff?.buffName ?? "Special Buff"}\n{currentBuff?.description ?? ""}\n+2000 XP",
            dialogueStyle);

        // Check if player has the fish
        bool hasFish = FishBuffSystem.Instance != null && FishBuffSystem.Instance.HasRequiredFish(currentQuestFishId);

        if (hasFish)
        {
            GUIStyle readyStyle = new GUIStyle();
            readyStyle.fontSize = 18;
            readyStyle.fontStyle = FontStyle.Bold;
            readyStyle.alignment = TextAnchor.MiddleCenter;
            readyStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);
            GUI.Label(new Rect(panelX, panelY + 200, panelWidth, 30), "You have the fish!", readyStyle);

            if (DrawButton(new Rect(panelX + panelWidth / 2 - 80, panelY + panelHeight - 80, 160, 40), "Turn In"))
            {
                dialogueState = 3;
            }
        }
        else
        {
            dialogueStyle.normal.textColor = new Color(0.8f, 0.6f, 0.4f);
            GUI.Label(new Rect(panelX + 30, panelY + 200, panelWidth - 60, 30),
                "Go fishing to catch this special fish!", dialogueStyle);
        }

        // Cancel quest button
        if (DrawButton(new Rect(panelX + panelWidth / 2 - 60, panelY + panelHeight - 35, 120, 30), "Cancel Quest"))
        {
            currentQuestFishId = null;
            currentQuestFishName = null;
            dialogueState = 1;
            SaveQuestState();
        }
    }

    void DrawQuestComplete(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 16;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        dialogueStyle.wordWrap = true;

        FishBuff currentBuff = null;
        if (FishBuffSystem.Instance != null)
        {
            currentBuff = FishBuffSystem.Instance.GetBuffByFishId(currentQuestFishId);
        }

        GUI.Label(new Rect(panelX + 30, panelY + 60, panelWidth - 60, 80),
            $"\"Magnifique! This {currentQuestFishName} will make the perfect dish!\n\nHere, take this special recipe I've prepared...\"",
            dialogueStyle);

        // Show reward
        GUIStyle rewardStyle = new GUIStyle();
        rewardStyle.fontSize = 18;
        rewardStyle.fontStyle = FontStyle.Bold;
        rewardStyle.alignment = TextAnchor.MiddleCenter;
        rewardStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        GUI.Label(new Rect(panelX, panelY + 160, panelWidth, 30),
            $"Reward: {currentBuff?.buffName ?? "Special Buff"}", rewardStyle);

        rewardStyle.fontSize = 14;
        rewardStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + 190, panelWidth, 25), "+2000 XP", rewardStyle);

        // Claim button
        if (DrawButton(new Rect(panelX + panelWidth / 2 - 80, panelY + panelHeight - 80, 160, 40), "Claim Reward"))
        {
            // Complete the quest
            if (FishBuffSystem.Instance != null)
            {
                FishBuffSystem.Instance.ConsumeFish(currentQuestFishId);
                FishBuffSystem.Instance.CompleteQuest(currentQuestFishId);
            }

            // Show notification
            if (UIManager.Instance != null && currentBuff != null)
            {
                UIManager.Instance.ShowLootNotification($"Earned: {currentBuff.buffName}!", currentBuff.bowlColor);
            }

            // Reset quest state
            currentQuestFishId = null;
            currentQuestFishName = null;
            dialogueState = 1;
            SaveQuestState();
        }
    }

    bool DrawButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        GUI.DrawTexture(rect, hover ? buttonHoverTex : buttonTex);

        GUIStyle btnStyle = new GUIStyle();
        btnStyle.fontSize = 14;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = Color.white;
        GUI.Label(rect, text, btnStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    void SaveQuestState()
    {
        PlayerPrefs.SetInt("ChefQuestStarted", questStarted ? 1 : 0);
        PlayerPrefs.SetString("ChefCurrentQuestFish", currentQuestFishId ?? "");
        PlayerPrefs.SetString("ChefCurrentQuestName", currentQuestFishName ?? "");
        PlayerPrefs.Save();
    }

    void LoadQuestState()
    {
        questStarted = PlayerPrefs.GetInt("ChefQuestStarted", 0) == 1;
        currentQuestFishId = PlayerPrefs.GetString("ChefCurrentQuestFish", "");
        currentQuestFishName = PlayerPrefs.GetString("ChefCurrentQuestName", "");

        if (string.IsNullOrEmpty(currentQuestFishId))
        {
            currentQuestFishId = null;
            currentQuestFishName = null;
        }
    }

    void OnDestroy()
    {
        if (dialogueBgTex != null) Destroy(dialogueBgTex);
        if (buttonTex != null) Destroy(buttonTex);
        if (buttonHoverTex != null) Destroy(buttonHoverTex);
    }
}
