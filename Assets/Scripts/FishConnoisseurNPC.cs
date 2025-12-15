using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Fish Connoisseur NPC - French-themed fish collector who pays big gold for legendary fish
/// Wears a beret and black/white striped shirt
/// Offers 10,000 gold per legendary fish quest
/// </summary>
public class FishConnoisseurNPC : MonoBehaviour
{
    public static FishConnoisseurNPC Instance { get; private set; }

    // Quest data
    [System.Serializable]
    public class LegendaryQuest
    {
        public string questName;      // French-style name
        public string fishId;         // ID in fish database
        public string fishName;       // Display name
        public string description;    // Quest description
        public int goldReward;
        public bool isCompleted;

        public LegendaryQuest(string name, string id, string fish, string desc, int gold)
        {
            questName = name;
            fishId = id;
            fishName = fish;
            description = desc;
            goldReward = gold;
            isCompleted = false;
        }
    }

    public List<LegendaryQuest> legendaryQuests = new List<LegendaryQuest>();
    private int currentQuestIndex = -1; // -1 = no active quest

    // NPC state
    private bool playerNearby = false;
    private float interactionRange = 4f;
    private bool showingDialogue = false;
    private int dialogueState = 0; // 0=greeting, 1=quest select, 2=active quest, 3=turn in

    // Visuals
    private GameObject body;
    private GameObject head;
    private GameObject beret;
    private Transform playerTransform;

    // Cached textures
    private Texture2D dialogueBgTex;
    private Texture2D buttonTex;
    private Texture2D buttonHoverTex;
    private Texture2D questBgTex;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeQuests();
        CreateVisuals();
        CreateCachedTextures();
        LoadQuestProgress();
    }

    void InitializeQuests()
    {
        // Legendary fish quests with French flair
        legendaryQuests.Add(new LegendaryQuest(
            "Le Warblecocque!",
            "danish_warblecock",
            "Danish Warblecock",
            "Ah, ze legendary Warblecock! Its pink flesh is magnifique for my signature dish!",
            10000
        ));

        legendaryQuests.Add(new LegendaryQuest(
            "La Baleine Royale",
            "whale",
            "Whale",
            "Ze mighty whale! Only ze greatest fishermen can catch such a beast!",
            10000
        ));

        legendaryQuests.Add(new LegendaryQuest(
            "Le Dorgush Mystérieux",
            "dorgush_wrangler",
            "Dorgush Cross-Eyed Wrangler",
            "Ze Cross-Eyed Wrangler! So rare, so peculiar, so... delicieux!",
            10000
        ));

        legendaryQuests.Add(new LegendaryQuest(
            "L'Étoile Dorée",
            "golden_starfish",
            "GOLDEN STARFISH",
            "Ze Golden Starfish! Ze rarest creature in all ze ocean! I must have it!",
            10000
        ));
    }

    void CreateCachedTextures()
    {
        dialogueBgTex = new Texture2D(1, 1);
        dialogueBgTex.SetPixel(0, 0, new Color(0.12f, 0.1f, 0.15f, 0.95f));
        dialogueBgTex.Apply();

        buttonTex = new Texture2D(1, 1);
        buttonTex.SetPixel(0, 0, new Color(0.25f, 0.2f, 0.35f, 1f));
        buttonTex.Apply();

        buttonHoverTex = new Texture2D(1, 1);
        buttonHoverTex.SetPixel(0, 0, new Color(0.4f, 0.3f, 0.5f, 1f));
        buttonHoverTex.Apply();

        questBgTex = new Texture2D(1, 1);
        questBgTex.SetPixel(0, 0, new Color(0.15f, 0.12f, 0.18f, 0.9f));
        questBgTex.Apply();
    }

    void CreateVisuals()
    {
        // Body with striped shirt (black and white horizontal stripes)
        body = new GameObject("Body");
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 1f, 0);

        // Main body capsule
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        torso.name = "Torso";
        torso.transform.SetParent(body.transform);
        torso.transform.localPosition = Vector3.zero;
        torso.transform.localScale = new Vector3(0.7f, 0.9f, 0.45f);
        Object.Destroy(torso.GetComponent<Collider>());

        // Create striped material (approximate with base color)
        Material shirtMat = new Material(Shader.Find("Standard"));
        shirtMat.color = new Color(0.9f, 0.9f, 0.9f); // White base
        torso.GetComponent<Renderer>().material = shirtMat;

        // Add black stripes as thin cubes
        for (int i = 0; i < 5; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = $"Stripe{i}";
            stripe.transform.SetParent(body.transform);
            stripe.transform.localPosition = new Vector3(0, -0.35f + i * 0.18f, 0.23f);
            stripe.transform.localScale = new Vector3(0.72f, 0.06f, 0.02f);
            Object.Destroy(stripe.GetComponent<Collider>());

            Material stripeMat = new Material(Shader.Find("Standard"));
            stripeMat.color = new Color(0.1f, 0.1f, 0.15f);
            stripe.GetComponent<Renderer>().material = stripeMat;
        }

        // Back stripes
        for (int i = 0; i < 5; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = $"BackStripe{i}";
            stripe.transform.SetParent(body.transform);
            stripe.transform.localPosition = new Vector3(0, -0.35f + i * 0.18f, -0.23f);
            stripe.transform.localScale = new Vector3(0.72f, 0.06f, 0.02f);
            Object.Destroy(stripe.GetComponent<Collider>());

            Material stripeMat = new Material(Shader.Find("Standard"));
            stripeMat.color = new Color(0.1f, 0.1f, 0.15f);
            stripe.GetComponent<Renderer>().material = stripeMat;
        }

        // Legs (dark pants)
        CreateLeg(-0.15f);
        CreateLeg(0.15f);

        // Head
        head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 2.1f, 0);
        head.transform.localScale = new Vector3(0.5f, 0.55f, 0.5f);
        Object.Destroy(head.GetComponent<Collider>());

        Material skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.9f, 0.78f, 0.65f);
        head.GetComponent<Renderer>().material = skinMat;

        // French beret
        CreateBeret();

        // Face details
        CreateFace();

        // Thin mustache (French style)
        CreateMustache();

        // Arms
        CreateArm(-0.4f);
        CreateArm(0.4f);
    }

    void CreateLeg(float xOffset)
    {
        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leg.name = "Leg";
        leg.transform.SetParent(transform);
        leg.transform.localPosition = new Vector3(xOffset, 0.4f, 0);
        leg.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
        Object.Destroy(leg.GetComponent<Collider>());

        Material pantsMat = new Material(Shader.Find("Standard"));
        pantsMat.color = new Color(0.15f, 0.15f, 0.2f); // Dark navy pants
        leg.GetComponent<Renderer>().material = pantsMat;
    }

    void CreateArm(float xOffset)
    {
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        arm.name = "Arm";
        arm.transform.SetParent(body.transform);
        arm.transform.localPosition = new Vector3(xOffset, -0.1f, 0);
        arm.transform.localScale = new Vector3(0.15f, 0.35f, 0.15f);
        arm.transform.localRotation = Quaternion.Euler(0, 0, xOffset > 0 ? -15f : 15f);
        Object.Destroy(arm.GetComponent<Collider>());

        // Striped sleeve
        Material armMat = new Material(Shader.Find("Standard"));
        armMat.color = new Color(0.85f, 0.85f, 0.85f);
        arm.GetComponent<Renderer>().material = armMat;
    }

    void CreateBeret()
    {
        beret = new GameObject("Beret");
        beret.transform.SetParent(head.transform);
        beret.transform.localPosition = new Vector3(0.05f, 0.35f, 0);

        // Beret base (flattened sphere, tilted)
        GameObject beretBase = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beretBase.name = "BeretBase";
        beretBase.transform.SetParent(beret.transform);
        beretBase.transform.localPosition = Vector3.zero;
        beretBase.transform.localScale = new Vector3(0.55f, 0.15f, 0.55f);
        beretBase.transform.localRotation = Quaternion.Euler(0, 0, 15f); // Tilted
        Object.Destroy(beretBase.GetComponent<Collider>());

        Material beretMat = new Material(Shader.Find("Standard"));
        beretMat.color = new Color(0.15f, 0.12f, 0.2f); // Dark purple/navy beret
        beretBase.GetComponent<Renderer>().material = beretMat;

        // Beret stem/nub on top
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stem.name = "BeretStem";
        stem.transform.SetParent(beret.transform);
        stem.transform.localPosition = new Vector3(0, 0.08f, 0);
        stem.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f);
        Object.Destroy(stem.GetComponent<Collider>());
        stem.GetComponent<Renderer>().material = beretMat;
    }

    void CreateFace()
    {
        Material skinMat = head.GetComponent<Renderer>().material;

        // Eyes
        CreateEye(-0.12f);
        CreateEye(0.12f);

        // Nose (prominent French nose)
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "Nose";
        nose.transform.SetParent(head.transform);
        nose.transform.localPosition = new Vector3(0, 0, 0.45f);
        nose.transform.localScale = new Vector3(0.15f, 0.12f, 0.2f);
        Object.Destroy(nose.GetComponent<Collider>());
        nose.GetComponent<Renderer>().material = skinMat;
    }

    void CreateEye(float xOffset)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Eye";
        eye.transform.SetParent(head.transform);
        eye.transform.localPosition = new Vector3(xOffset, 0.08f, 0.38f);
        eye.transform.localScale = new Vector3(0.12f, 0.12f, 0.08f);
        Object.Destroy(eye.GetComponent<Collider>());

        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = Color.white;
        eye.GetComponent<Renderer>().material = eyeMat;

        // Pupil
        GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pupil.name = "Pupil";
        pupil.transform.SetParent(eye.transform);
        pupil.transform.localPosition = new Vector3(0, 0, 0.35f);
        pupil.transform.localScale = new Vector3(0.45f, 0.45f, 0.3f);
        Object.Destroy(pupil.GetComponent<Collider>());

        Material pupilMat = new Material(Shader.Find("Standard"));
        pupilMat.color = new Color(0.25f, 0.2f, 0.15f); // Brown eyes
        pupil.GetComponent<Renderer>().material = pupilMat;
    }

    void CreateMustache()
    {
        // Thin curled French mustache
        GameObject mustacheL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mustacheL.name = "MustacheL";
        mustacheL.transform.SetParent(head.transform);
        mustacheL.transform.localPosition = new Vector3(-0.08f, -0.12f, 0.42f);
        mustacheL.transform.localScale = new Vector3(0.12f, 0.03f, 0.04f);
        mustacheL.transform.localRotation = Quaternion.Euler(0, 0, 15f);
        Object.Destroy(mustacheL.GetComponent<Collider>());

        Material mustacheMat = new Material(Shader.Find("Standard"));
        mustacheMat.color = new Color(0.2f, 0.15f, 0.1f);
        mustacheL.GetComponent<Renderer>().material = mustacheMat;

        GameObject mustacheR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mustacheR.name = "MustacheR";
        mustacheR.transform.SetParent(head.transform);
        mustacheR.transform.localPosition = new Vector3(0.08f, -0.12f, 0.42f);
        mustacheR.transform.localScale = new Vector3(0.12f, 0.03f, 0.04f);
        mustacheR.transform.localRotation = Quaternion.Euler(0, 0, -15f);
        Object.Destroy(mustacheR.GetComponent<Collider>());
        mustacheR.GetComponent<Renderer>().material = mustacheMat;

        // Curled tips
        GameObject tipL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tipL.name = "TipL";
        tipL.transform.SetParent(head.transform);
        tipL.transform.localPosition = new Vector3(-0.14f, -0.1f, 0.42f);
        tipL.transform.localScale = new Vector3(0.04f, 0.04f, 0.03f);
        Object.Destroy(tipL.GetComponent<Collider>());
        tipL.GetComponent<Renderer>().material = mustacheMat;

        GameObject tipR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tipR.name = "TipR";
        tipR.transform.SetParent(head.transform);
        tipR.transform.localPosition = new Vector3(0.14f, -0.1f, 0.42f);
        tipR.transform.localScale = new Vector3(0.04f, 0.04f, 0.03f);
        Object.Destroy(tipR.GetComponent<Collider>());
        tipR.GetComponent<Renderer>().material = mustacheMat;
    }

    void Update()
    {
        if (playerTransform == null && GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        playerNearby = distance <= interactionRange;

        // E key to interact
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && MainMenu.GameStarted)
        {
            if (!showingDialogue)
            {
                OpenDialogue();
            }
        }

        // ESC to close
        if (showingDialogue && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogue();
        }

        // Look at player
        if (playerNearby)
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

        if (currentQuestIndex >= 0)
        {
            // Check if player has the fish
            if (HasRequiredFish(legendaryQuests[currentQuestIndex].fishId))
            {
                dialogueState = 3; // Turn in
            }
            else
            {
                dialogueState = 2; // Active quest
            }
        }
        else
        {
            // Check if all quests done
            bool allDone = true;
            foreach (var q in legendaryQuests)
            {
                if (!q.isCompleted) { allDone = false; break; }
            }

            if (allDone)
            {
                dialogueState = 4; // All complete
            }
            else
            {
                dialogueState = 0; // Greeting/quest select
            }
        }
    }

    void CloseDialogue()
    {
        showingDialogue = false;
    }

    bool HasRequiredFish(string fishId)
    {
        if (GameManager.Instance == null) return false;

        // Check normal inventory
        if (GameManager.Instance.fishInventory.ContainsKey(fishId) &&
            GameManager.Instance.fishInventory[fishId] > 0)
        {
            return true;
        }

        // Check special fish inventory
        if (FishingSystem.Instance != null)
        {
            foreach (var fish in FishingSystem.Instance.specialFishInventory)
            {
                if (fish.id == fishId) return true;
            }
        }

        return false;
    }

    void RemoveFishFromInventory(string fishId)
    {
        // Try normal inventory first
        if (GameManager.Instance != null &&
            GameManager.Instance.fishInventory.ContainsKey(fishId) &&
            GameManager.Instance.fishInventory[fishId] > 0)
        {
            GameManager.Instance.fishInventory[fishId]--;
            if (GameManager.Instance.fishInventory[fishId] <= 0)
            {
                GameManager.Instance.fishInventory.Remove(fishId);
            }
            return;
        }

        // Try special fish inventory
        if (FishingSystem.Instance != null)
        {
            var inv = FishingSystem.Instance.specialFishInventory;
            for (int i = 0; i < inv.Count; i++)
            {
                if (inv[i].id == fishId)
                {
                    inv.RemoveAt(i);
                    return;
                }
            }
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        if (playerNearby && !showingDialogue)
        {
            DrawInteractionPrompt();
        }

        if (showingDialogue)
        {
            DrawDialogue();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 16;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.normal.textColor = new Color(0.9f, 0.85f, 1f);

        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 120, 200, 30), "Pierre le Connoisseur", nameStyle);

        nameStyle.fontSize = 14;
        nameStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 95, 200, 25), "[E] Talk", nameStyle);
    }

    void DrawDialogue()
    {
        // Dark overlay
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float panelWidth = 520;
        float panelHeight = 380;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), dialogueBgTex);

        // Gold border
        GUI.color = new Color(0.8f, 0.7f, 0.3f);
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
        titleStyle.normal.textColor = new Color(0.9f, 0.85f, 1f);
        GUI.Label(new Rect(panelX, panelY + 15, panelWidth, 30), "Pierre le Connoisseur", titleStyle);

        // Close button
        if (GUI.Button(new Rect(panelX + panelWidth - 35, panelY + 10, 25, 25), "X"))
        {
            CloseDialogue();
        }

        switch (dialogueState)
        {
            case 0: DrawQuestSelect(panelX, panelY, panelWidth, panelHeight); break;
            case 2: DrawActiveQuest(panelX, panelY, panelWidth, panelHeight); break;
            case 3: DrawTurnIn(panelX, panelY, panelWidth, panelHeight); break;
            case 4: DrawAllComplete(panelX, panelY, panelWidth, panelHeight); break;
        }
    }

    void DrawQuestSelect(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 14;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.88f, 0.95f);
        dialogueStyle.wordWrap = true;

        GUI.Label(new Rect(panelX + 20, panelY + 55, panelWidth - 40, 50),
            "\"Bonjour, mon ami! I am Pierre, collector of ze finest fish!\nBring me a legendary catch and I shall pay you handsomely!\"",
            dialogueStyle);

        // Quest list
        float questY = panelY + 115;
        float questHeight = 55;

        foreach (var quest in legendaryQuests)
        {
            DrawQuestOption(new Rect(panelX + 20, questY, panelWidth - 40, questHeight), quest);
            questY += questHeight + 5;
        }
    }

    void DrawQuestOption(Rect rect, LegendaryQuest quest)
    {
        bool isCompleted = quest.isCompleted;

        GUI.DrawTexture(rect, questBgTex);

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 14;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = isCompleted ? new Color(0.5f, 0.5f, 0.5f) : new Color(1f, 0.9f, 0.5f);

        GUI.Label(new Rect(rect.x + 10, rect.y + 5, rect.width - 100, 20), quest.questName, nameStyle);

        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 11;
        descStyle.normal.textColor = new Color(0.7f, 0.7f, 0.75f);
        descStyle.wordWrap = true;

        string status = isCompleted ? "[COMPLETED]" : $"Find: {quest.fishName}";
        GUI.Label(new Rect(rect.x + 10, rect.y + 25, rect.width - 120, 25), status, descStyle);

        // Reward
        GUIStyle rewardStyle = new GUIStyle();
        rewardStyle.fontSize = 12;
        rewardStyle.fontStyle = FontStyle.Bold;
        rewardStyle.alignment = TextAnchor.MiddleRight;
        rewardStyle.normal.textColor = isCompleted ? new Color(0.4f, 0.4f, 0.4f) : new Color(1f, 0.85f, 0.2f);

        GUI.Label(new Rect(rect.x + rect.width - 90, rect.y + 15, 80, 25),
            isCompleted ? "DONE" : $"{quest.goldReward}g", rewardStyle);

        // Accept button
        if (!isCompleted && currentQuestIndex < 0)
        {
            Rect btnRect = new Rect(rect.x + rect.width - 70, rect.y + 5, 60, 20);
            bool hover = btnRect.Contains(Event.current.mousePosition);
            GUI.DrawTexture(btnRect, hover ? buttonHoverTex : buttonTex);

            GUIStyle btnStyle = new GUIStyle();
            btnStyle.fontSize = 10;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.alignment = TextAnchor.MiddleCenter;
            btnStyle.normal.textColor = Color.white;
            GUI.Label(btnRect, "ACCEPT", btnStyle);

            if (GUI.Button(btnRect, "", GUIStyle.none))
            {
                currentQuestIndex = legendaryQuests.IndexOf(quest);
                dialogueState = 2;
                SaveQuestProgress();
            }
        }
    }

    void DrawActiveQuest(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        if (currentQuestIndex < 0 || currentQuestIndex >= legendaryQuests.Count)
        {
            dialogueState = 0;
            return;
        }

        var quest = legendaryQuests[currentQuestIndex];

        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 15;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.88f, 0.95f);
        dialogueStyle.wordWrap = true;

        GUI.Label(new Rect(panelX + 30, panelY + 60, panelWidth - 60, 80),
            $"\"{quest.description}\"", dialogueStyle);

        // Quest info
        GUIStyle questStyle = new GUIStyle();
        questStyle.fontSize = 18;
        questStyle.fontStyle = FontStyle.Bold;
        questStyle.alignment = TextAnchor.MiddleCenter;
        questStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

        GUI.Label(new Rect(panelX, panelY + 150, panelWidth, 30), quest.questName, questStyle);

        questStyle.fontSize = 14;
        questStyle.normal.textColor = new Color(0.8f, 0.8f, 0.85f);
        GUI.Label(new Rect(panelX, panelY + 180, panelWidth, 25), $"Catch: {quest.fishName}", questStyle);

        // Reward
        questStyle.fontSize = 16;
        questStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
        GUI.Label(new Rect(panelX, panelY + 210, panelWidth, 25), $"Reward: {quest.goldReward} Gold", questStyle);

        // Check if player has fish
        bool hasFish = HasRequiredFish(quest.fishId);
        if (hasFish)
        {
            GUIStyle readyStyle = new GUIStyle();
            readyStyle.fontSize = 16;
            readyStyle.fontStyle = FontStyle.Bold;
            readyStyle.alignment = TextAnchor.MiddleCenter;
            readyStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
            GUI.Label(new Rect(panelX, panelY + 250, panelWidth, 25), "You have the fish!", readyStyle);

            if (DrawButton(new Rect(panelX + panelWidth / 2 - 70, panelY + panelHeight - 70, 140, 35), "TURN IN"))
            {
                dialogueState = 3;
            }
        }
        else
        {
            dialogueStyle.normal.textColor = new Color(0.7f, 0.6f, 0.5f);
            GUI.Label(new Rect(panelX, panelY + 250, panelWidth, 25),
                "Go catch this legendary fish!", dialogueStyle);
        }

        // Cancel button
        if (DrawButton(new Rect(panelX + panelWidth / 2 - 50, panelY + panelHeight - 35, 100, 25), "Cancel"))
        {
            currentQuestIndex = -1;
            dialogueState = 0;
            SaveQuestProgress();
        }
    }

    void DrawTurnIn(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        if (currentQuestIndex < 0) return;

        var quest = legendaryQuests[currentQuestIndex];

        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 16;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.88f, 0.95f);
        dialogueStyle.wordWrap = true;

        GUI.Label(new Rect(panelX + 30, panelY + 60, panelWidth - 60, 80),
            $"\"Magnifique! Ze {quest.fishName}! It is even more beautiful than I imagined!\n\nHere is your payment, mon ami!\"",
            dialogueStyle);

        // Reward display
        GUIStyle rewardStyle = new GUIStyle();
        rewardStyle.fontSize = 28;
        rewardStyle.fontStyle = FontStyle.Bold;
        rewardStyle.alignment = TextAnchor.MiddleCenter;
        rewardStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

        GUI.Label(new Rect(panelX, panelY + 170, panelWidth, 40), $"+{quest.goldReward} GOLD!", rewardStyle);

        // Claim button
        if (DrawButton(new Rect(panelX + panelWidth / 2 - 80, panelY + panelHeight - 80, 160, 45), "CLAIM REWARD"))
        {
            // Remove fish
            RemoveFishFromInventory(quest.fishId);

            // Give gold
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(quest.goldReward);
            }

            // Mark complete
            quest.isCompleted = true;
            currentQuestIndex = -1;

            // Notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"+{quest.goldReward} Gold from Pierre!", new Color(1f, 0.85f, 0.3f));
            }

            SaveQuestProgress();
            dialogueState = 0;
        }
    }

    void DrawAllComplete(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        GUIStyle dialogueStyle = new GUIStyle();
        dialogueStyle.fontSize = 16;
        dialogueStyle.alignment = TextAnchor.UpperCenter;
        dialogueStyle.normal.textColor = new Color(0.9f, 0.88f, 0.95f);
        dialogueStyle.wordWrap = true;

        GUI.Label(new Rect(panelX + 30, panelY + 80, panelWidth - 60, 100),
            "\"Ah, mon ami! You have brought me every legendary fish I could dream of!\n\nYou are truly ze greatest fisherman in all ze land!\n\nMerci beaucoup!\"",
            dialogueStyle);

        GUIStyle completeStyle = new GUIStyle();
        completeStyle.fontSize = 20;
        completeStyle.fontStyle = FontStyle.Bold;
        completeStyle.alignment = TextAnchor.MiddleCenter;
        completeStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        GUI.Label(new Rect(panelX, panelY + 200, panelWidth, 30), "ALL QUESTS COMPLETE!", completeStyle);

        completeStyle.fontSize = 14;
        completeStyle.normal.textColor = new Color(0.7f, 0.7f, 0.75f);
        GUI.Label(new Rect(panelX, panelY + 230, panelWidth, 25), "Total earned: 40,000 Gold", completeStyle);
    }

    bool DrawButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        GUI.DrawTexture(rect, hover ? buttonHoverTex : buttonTex);

        GUIStyle btnStyle = new GUIStyle();
        btnStyle.fontSize = 13;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = Color.white;
        GUI.Label(rect, text, btnStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    void SaveQuestProgress()
    {
        PlayerPrefs.SetInt("ConnoisseurCurrentQuest", currentQuestIndex);
        for (int i = 0; i < legendaryQuests.Count; i++)
        {
            PlayerPrefs.SetInt($"ConnoisseurQuest_{i}", legendaryQuests[i].isCompleted ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    void LoadQuestProgress()
    {
        currentQuestIndex = PlayerPrefs.GetInt("ConnoisseurCurrentQuest", -1);
        for (int i = 0; i < legendaryQuests.Count; i++)
        {
            legendaryQuests[i].isCompleted = PlayerPrefs.GetInt($"ConnoisseurQuest_{i}", 0) == 1;
        }
    }

    void OnDestroy()
    {
        if (dialogueBgTex != null) Destroy(dialogueBgTex);
        if (buttonTex != null) Destroy(buttonTex);
        if (buttonHoverTex != null) Destroy(buttonHoverTex);
        if (questBgTex != null) Destroy(questBgTex);
    }
}
