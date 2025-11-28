using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Goldie Banks - Rastafarian NPC who walks along the beaches
/// Smoking a cigarette with visible smoke
/// Repeatable quest: Find his lost bag of "good stuff"
/// </summary>
public class GoldieBanksNPC : MonoBehaviour
{
    public static GoldieBanksNPC Instance { get; private set; }

    // Quest state - REPEATABLE
    public bool questActive = false;
    public bool weedBagFound = false;
    private int questsCompleted = 0;

    // UI State
    private bool playerNearby = false;
    private bool dialogOpen = false;
    private float interactionDistance = 4f;

    // Walking behavior
    private Vector3[] beachWaypoints;
    private int currentWaypoint = 0;
    private float walkSpeed = 1.5f;
    private bool isWalking = true;
    private float idleTimer = 0f;
    private float idleDuration = 3f;

    // Visual components
    private GameObject head;
    private GameObject body;
    private GameObject leftLeg;
    private GameObject rightLeg;
    private GameObject cigarette;
    private List<GameObject> smokeParticles = new List<GameObject>();

    // Materials
    private Material skinMat;
    private Material hairMat;
    private Material clothesMat;

    // Animation
    private float walkCycle = 0f;
    private float smokeTimer = 0f;

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
        SetupBeachWaypoints();
        CreateNPC();
        CreateUITextures();
        StartCoroutine(SmokeEffect());
    }

    void SetupBeachWaypoints()
    {
        // Waypoints along the beach areas of the main island
        beachWaypoints = new Vector3[]
        {
            new Vector3(15f, 1.6f, 25f),
            new Vector3(20f, 1.6f, 15f),
            new Vector3(25f, 1.6f, 5f),
            new Vector3(20f, 1.6f, -5f),
            new Vector3(15f, 1.6f, -15f),
            new Vector3(10f, 1.6f, -20f),
            new Vector3(0f, 1.6f, -25f),
            new Vector3(-10f, 1.6f, -20f),
            new Vector3(-15f, 1.6f, -10f),
            new Vector3(-20f, 1.6f, 0f),
            new Vector3(-15f, 1.6f, 10f),
            new Vector3(-10f, 1.6f, 20f),
            new Vector3(0f, 1.6f, 25f),
            new Vector3(10f, 1.6f, 25f),
        };
    }

    void CreateUITextures()
    {
        bubbleTexture = new Texture2D(1, 1);
        bubbleTexture.SetPixel(0, 0, new Color(0.1f, 0.08f, 0.06f, 0.95f));
        bubbleTexture.Apply();

        borderTexture = new Texture2D(1, 1);
        borderTexture.SetPixel(0, 0, new Color(0.4f, 0.6f, 0.3f, 1f));
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
        clothesMat.color = new Color(0.2f, 0.5f, 0.15f); // Rasta green

        // Body
        body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.4f, 0.45f, 0.3f);
        body.GetComponent<Renderer>().sharedMaterial = clothesMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Head
        head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 1.55f, 0);
        head.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.Destroy(head.GetComponent<Collider>());

        // Dreadlocks
        CreateDreadlocks();

        // Arms
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArm.name = "LeftArm";
        leftArm.transform.SetParent(transform);
        leftArm.transform.localPosition = new Vector3(-0.28f, 1.0f, 0);
        leftArm.transform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
        leftArm.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.Destroy(leftArm.GetComponent<Collider>());

        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArm.name = "RightArm";
        rightArm.transform.SetParent(transform);
        rightArm.transform.localPosition = new Vector3(0.28f, 1.0f, 0);
        rightArm.transform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
        rightArm.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.Destroy(rightArm.GetComponent<Collider>());

        // Legs
        leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLeg.name = "LeftLeg";
        leftLeg.transform.SetParent(transform);
        leftLeg.transform.localPosition = new Vector3(-0.12f, 0.35f, 0);
        leftLeg.transform.localScale = new Vector3(0.14f, 0.35f, 0.14f);
        leftLeg.GetComponent<Renderer>().sharedMaterial = clothesMat;
        Object.Destroy(leftLeg.GetComponent<Collider>());

        rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLeg.name = "RightLeg";
        rightLeg.transform.SetParent(transform);
        rightLeg.transform.localPosition = new Vector3(0.12f, 0.35f, 0);
        rightLeg.transform.localScale = new Vector3(0.14f, 0.35f, 0.14f);
        rightLeg.GetComponent<Renderer>().sharedMaterial = clothesMat;
        Object.Destroy(rightLeg.GetComponent<Collider>());

        // Cigarette in mouth
        CreateCigarette();

        // Rasta tam (hat)
        CreateRastaTam();
    }

    void CreateDreadlocks()
    {
        // Multiple dreadlock strands
        for (int i = 0; i < 12; i++)
        {
            GameObject dread = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dread.name = "Dreadlock" + i;
            dread.transform.SetParent(head.transform);

            float angle = i * 30f * Mathf.Deg2Rad;
            float radius = 0.4f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            float length = Random.Range(0.6f, 1.0f);

            dread.transform.localPosition = new Vector3(x, -0.3f, z);
            dread.transform.localScale = new Vector3(0.15f, length, 0.15f);
            dread.transform.localRotation = Quaternion.Euler(Random.Range(10f, 30f), 0, Random.Range(-15f, 15f));
            dread.GetComponent<Renderer>().sharedMaterial = hairMat;
            Object.Destroy(dread.GetComponent<Collider>());
        }
    }

    void CreateCigarette()
    {
        // Cigarette
        cigarette = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cigarette.name = "Cigarette";
        cigarette.transform.SetParent(head.transform);
        cigarette.transform.localPosition = new Vector3(0.15f, -0.25f, 0.4f);
        cigarette.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
        cigarette.transform.localRotation = Quaternion.Euler(90, 0, 20);
        Object.Destroy(cigarette.GetComponent<Collider>());

        Material cigMat = new Material(Shader.Find("Standard"));
        cigMat.color = new Color(0.9f, 0.85f, 0.7f);
        cigarette.GetComponent<Renderer>().material = cigMat;

        // Glowing tip
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "CigTip";
        tip.transform.SetParent(cigarette.transform);
        tip.transform.localPosition = new Vector3(0, 1f, 0);
        tip.transform.localScale = new Vector3(1.2f, 0.3f, 1.2f);
        Object.Destroy(tip.GetComponent<Collider>());

        Material tipMat = new Material(Shader.Find("Standard"));
        tipMat.color = new Color(1f, 0.4f, 0.1f);
        tipMat.EnableKeyword("_EMISSION");
        tipMat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0f) * 2f);
        tip.GetComponent<Renderer>().material = tipMat;
    }

    void CreateRastaTam()
    {
        // Rasta tam/beanie
        GameObject tam = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tam.name = "RastaTam";
        tam.transform.SetParent(head.transform);
        tam.transform.localPosition = new Vector3(0, 0.35f, 0);
        tam.transform.localScale = new Vector3(1.3f, 0.6f, 1.3f);
        Object.Destroy(tam.GetComponent<Collider>());

        Material tamMat = new Material(Shader.Find("Standard"));
        tamMat.color = new Color(0.8f, 0.1f, 0.1f); // Red
        tam.GetComponent<Renderer>().material = tamMat;

        // Yellow stripe
        GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stripe.name = "TamStripe";
        stripe.transform.SetParent(tam.transform);
        stripe.transform.localPosition = new Vector3(0, -0.2f, 0);
        stripe.transform.localScale = new Vector3(1.05f, 0.15f, 1.05f);
        Object.Destroy(stripe.GetComponent<Collider>());

        Material stripeMat = new Material(Shader.Find("Standard"));
        stripeMat.color = new Color(1f, 0.85f, 0.1f); // Yellow
        stripe.GetComponent<Renderer>().material = stripeMat;

        // Green stripe
        GameObject stripe2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stripe2.name = "TamStripe2";
        stripe2.transform.SetParent(tam.transform);
        stripe2.transform.localPosition = new Vector3(0, -0.4f, 0);
        stripe2.transform.localScale = new Vector3(1.1f, 0.15f, 1.1f);
        Object.Destroy(stripe2.GetComponent<Collider>());

        Material stripe2Mat = new Material(Shader.Find("Standard"));
        stripe2Mat.color = new Color(0.1f, 0.6f, 0.1f); // Green
        stripe2.GetComponent<Renderer>().material = stripe2Mat;
    }

    IEnumerator SmokeEffect()
    {
        Material smokeMat = new Material(Shader.Find("Standard"));
        smokeMat.color = new Color(0.8f, 0.8f, 0.8f, 0.6f);
        smokeMat.SetFloat("_Mode", 3);
        smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        smokeMat.EnableKeyword("_ALPHABLEND_ON");
        smokeMat.renderQueue = 3000;

        while (true)
        {
            // Create smoke puff every 0.3 seconds
            yield return new WaitForSeconds(0.3f);

            if (cigarette == null) continue;

            GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.name = "Smoke";
            smoke.transform.position = cigarette.transform.position + cigarette.transform.up * 0.15f;
            smoke.transform.localScale = Vector3.one * 0.08f;
            smoke.GetComponent<Renderer>().material = smokeMat;
            Object.Destroy(smoke.GetComponent<Collider>());

            smokeParticles.Add(smoke);
            StartCoroutine(AnimateSmoke(smoke));
        }
    }

    IEnumerator AnimateSmoke(GameObject smoke)
    {
        float lifetime = 2f;
        float t = 0;
        Vector3 startPos = smoke.transform.position;
        Vector3 startScale = smoke.transform.localScale;

        while (t < lifetime && smoke != null)
        {
            t += Time.deltaTime;
            float progress = t / lifetime;

            // Rise and drift
            smoke.transform.position = startPos + new Vector3(
                Mathf.Sin(t * 2f) * 0.1f,
                t * 0.5f,
                Mathf.Cos(t * 2f) * 0.1f
            );

            // Expand and fade
            smoke.transform.localScale = startScale * (1f + progress * 3f);

            Material mat = smoke.GetComponent<Renderer>().material;
            Color c = mat.color;
            c.a = 0.6f * (1f - progress);
            mat.color = c;

            yield return null;
        }

        if (smoke != null)
        {
            smokeParticles.Remove(smoke);
            Destroy(smoke);
        }
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        CheckPlayerProximity();
        HandleInput();

        if (!dialogOpen)
        {
            HandleWalking();
            AnimateWalk();
        }
    }

    void HandleWalking()
    {
        if (beachWaypoints == null || beachWaypoints.Length == 0) return;

        if (isWalking)
        {
            Vector3 target = beachWaypoints[currentWaypoint];
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0;

            // Move towards waypoint
            transform.position += direction * walkSpeed * Time.deltaTime;

            // Face movement direction
            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }

            // Check if reached waypoint
            float dist = Vector3.Distance(transform.position, target);
            if (dist < 1f)
            {
                currentWaypoint = (currentWaypoint + 1) % beachWaypoints.Length;

                // Occasionally stop and idle
                if (Random.value < 0.3f)
                {
                    isWalking = false;
                    idleTimer = idleDuration;
                }
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                isWalking = true;
            }
        }
    }

    void AnimateWalk()
    {
        if (leftLeg == null || rightLeg == null) return;

        if (isWalking && !dialogOpen)
        {
            walkCycle += Time.deltaTime * 8f;
            float legSwing = Mathf.Sin(walkCycle) * 20f;

            leftLeg.transform.localRotation = Quaternion.Euler(legSwing, 0, 0);
            rightLeg.transform.localRotation = Quaternion.Euler(-legSwing, 0, 0);
        }
        else
        {
            leftLeg.transform.localRotation = Quaternion.identity;
            rightLeg.transform.localRotation = Quaternion.identity;
        }
    }

    void CheckPlayerProximity()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerNearby = distance < interactionDistance;

        if (!playerNearby && dialogOpen)
        {
            dialogOpen = false;
        }
    }

    void HandleInput()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogOpen)
            {
                dialogOpen = true;
                isWalking = false;

                // Face the player
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    Vector3 lookDir = (player.transform.position - transform.position).normalized;
                    lookDir.y = 0;
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
                // Quest is now accepted via button, not automatically
            }
            else
            {
                dialogOpen = false;
                isWalking = true;
            }
        }
    }

    public void AcceptQuest()
    {
        if (!questActive && !weedBagFound)
        {
            questActive = true;
            SpawnWeedBag();
            Debug.Log("Quest accepted! Find Goldie's bag!");
        }
    }

    void SpawnWeedBag()
    {
        // Remove old bag if exists
        GameObject oldBag = GameObject.Find("WeedBagCollectible");
        if (oldBag != null) Destroy(oldBag);

        // Spawn bag at random hidden location on the island
        Vector3[] hiddenSpots = new Vector3[]
        {
            new Vector3(-25f, 1.6f, 15f),
            new Vector3(20f, 1.6f, -18f),
            new Vector3(-18f, 1.6f, -22f),
            new Vector3(25f, 1.6f, 20f),
            new Vector3(-30f, 1.6f, -5f),
            new Vector3(15f, 1.6f, 30f),
            new Vector3(-20f, 1.6f, 25f),
            new Vector3(30f, 1.6f, -10f),
        };

        Vector3 spawnPos = hiddenSpots[Random.Range(0, hiddenSpots.Length)];

        GameObject weedBag = new GameObject("WeedBagCollectible");
        weedBag.transform.position = spawnPos;
        weedBag.AddComponent<WeedBagCollectible>();

        Debug.Log("Weed bag spawned at: " + spawnPos);
    }

    public void OnWeedBagFound()
    {
        weedBagFound = true;
        questActive = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Found Goldie's bag! Return to him!", new Color(0.4f, 0.8f, 0.3f));
        }
    }

    public void CompleteQuest()
    {
        questsCompleted++;
        weedBagFound = false;

        // Rewards - scale with completions
        int goldReward = 500 + (questsCompleted * 100);
        int xpReward = 1000 + (questsCompleted * 200);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(goldReward);
        }
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.AddXP(xpReward);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"+{goldReward}g, +{xpReward} XP!", new Color(1f, 0.85f, 0.2f));
        }

        Debug.Log($"Quest completed {questsCompleted} times! Rewards: {goldReward}g, {xpReward} XP");
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (!playerNearby) return;

        // "Press E" prompt
        if (!dialogOpen)
        {
            DrawInteractionPrompt();
        }
        else
        {
            DrawDialog();
        }
    }

    void DrawInteractionPrompt()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        float width = 180;
        float height = 35;
        float x = (Screen.width - width) / 2;
        float y = Screen.height * 0.65f;

        GUI.DrawTexture(new Rect(x, y, width, height), bubbleTexture);
        GUI.Label(new Rect(x, y, width, height), "Press E to talk", style);
    }

    void DrawDialog()
    {
        float boxWidth = 450;
        float boxHeight = 220; // Taller to fit button
        float boxX = (Screen.width - boxWidth) / 2;
        float boxY = Screen.height * 0.25f;

        // Background
        GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, boxHeight), bubbleTexture);

        // Border
        GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, 4), borderTexture);
        GUI.DrawTexture(new Rect(boxX, boxY + boxHeight - 4, boxWidth, 4), borderTexture);
        GUI.DrawTexture(new Rect(boxX, boxY, 4, boxHeight), borderTexture);
        GUI.DrawTexture(new Rect(boxX + boxWidth - 4, boxY, 4, boxHeight), borderTexture);

        // Name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 18;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = new Color(0.4f, 0.8f, 0.3f);
        GUI.Label(new Rect(boxX + 20, boxY + 15, boxWidth, 30), "Goldie Banks", nameStyle);

        // Dialog text
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 14;
        textStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        textStyle.wordWrap = true;

        string dialogText;
        if (weedBagFound)
        {
            dialogText = "YAAAA MON! You found it! Bless up, bless up! Here, take dis for ya troubles...";
            CompleteQuest();
        }
        else if (questActive)
        {
            dialogText = "AHHHHH MON... I lost ma bag of good stuff... Been searchin' ALL DAYYYY, can you help me?\n\n*The bag has a faint golden glow. Search the island!*";
        }
        else
        {
            dialogText = "AHHHHH MON... I lost ma bag of good stuff... Been searchin' ALL DAYYYY, can you help me?";
        }

        GUI.Label(new Rect(boxX + 20, boxY + 50, boxWidth - 40, 70), dialogText, textStyle);

        // Accept Quest button - only show if quest not active and bag not found
        if (!questActive && !weedBagFound)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = new Color(0.4f, 1f, 0.4f);

            // Green button background
            Texture2D btnTex = new Texture2D(1, 1);
            btnTex.SetPixel(0, 0, new Color(0.2f, 0.5f, 0.2f));
            btnTex.Apply();
            buttonStyle.normal.background = btnTex;

            Texture2D btnHoverTex = new Texture2D(1, 1);
            btnHoverTex.SetPixel(0, 0, new Color(0.3f, 0.7f, 0.3f));
            btnHoverTex.Apply();
            buttonStyle.hover.background = btnHoverTex;

            float btnWidth = 150;
            float btnHeight = 35;
            float btnX = boxX + (boxWidth - btnWidth) / 2;
            float btnY = boxY + 130;

            if (GUI.Button(new Rect(btnX, btnY, btnWidth, btnHeight), "Accept Quest", buttonStyle))
            {
                AcceptQuest();
            }

            // Clean up textures
            Destroy(btnTex);
            Destroy(btnHoverTex);
        }
        else if (questActive && !weedBagFound)
        {
            // Show quest status
            GUIStyle statusStyle = new GUIStyle();
            statusStyle.fontSize = 14;
            statusStyle.fontStyle = FontStyle.Bold;
            statusStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);
            statusStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(boxX, boxY + 135, boxWidth, 25), "Quest Active - Find the bag!", statusStyle);
        }

        // Press E to close hint
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 12;
        hintStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        hintStyle.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(boxX, boxY + boxHeight - 30, boxWidth - 15, 20), "[E] Close", hintStyle);
    }

    void OnDestroy()
    {
        if (bubbleTexture != null) Destroy(bubbleTexture);
        if (borderTexture != null) Destroy(borderTexture);

        foreach (var smoke in smokeParticles)
        {
            if (smoke != null) Destroy(smoke);
        }
    }

    public bool IsDialogueOpen()
    {
        return dialogOpen;
    }
}
