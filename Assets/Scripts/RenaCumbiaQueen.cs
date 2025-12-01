using UnityEngine;

/// <summary>
/// Rena the Cumbia Queen - Jungle Realm quest giver NPC
/// A festive woman in cheetah print who gives quests related to jungle exploration and snake hunting
/// </summary>
public class RenaCumbiaQueen : MonoBehaviour
{
    private bool isNearPlayer = false;
    private bool dialogueOpen = false;
    private int dialogueStage = 0;

    private enum QuestState { NotStarted, InProgress, Completed, Rewarded }
    private QuestState currentQuestState = QuestState.NotStarted;

    private int snakesKilled = 0;
    private int snakesRequired = 3;

    private GUIStyle headerStyle;
    private GUIStyle dialogueStyle;
    private GUIStyle buttonStyle;
    private GUIStyle questStyle;

    private GameObject questMarker;

    void Start()
    {
        // Find quest marker
        questMarker = transform.Find("QuestMarker")?.gameObject;

        // Create visual model
        CreateVisualModel();
    }

    void CreateVisualModel()
    {
        // Materials
        Material skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.7f, 0.5f, 0.4f); // Skin tone

        Material cheetahMat = new Material(Shader.Find("Standard"));
        cheetahMat.color = new Color(0.95f, 0.75f, 0.35f); // Tan/yellow base

        Material spotMat = new Material(Shader.Find("Standard"));
        spotMat.color = new Color(0.15f, 0.1f, 0.05f); // Dark brown spots

        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.15f, 0.1f, 0.08f); // Dark brown hair

        Material earringMat = new Material(Shader.Find("Standard"));
        earringMat.color = new Color(1f, 0.85f, 0.2f); // Gold earrings
        earringMat.SetFloat("_Metallic", 0.8f);

        // Body (female proportions - slightly narrower torso)
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso";
        torso.transform.SetParent(transform);
        torso.transform.localPosition = new Vector3(0, 1.2f, 0);
        torso.transform.localScale = new Vector3(0.35f, 0.6f, 0.25f);
        torso.GetComponent<Renderer>().material = cheetahMat;
        Object.Destroy(torso.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 1.8f, 0);
        head.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
        head.GetComponent<Renderer>().material = skinMat;
        Object.Destroy(head.GetComponent<Collider>());

        // Hair (long flowing hair for the Cumbia Queen)
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.name = "Hair";
        hair.transform.SetParent(head.transform);
        hair.transform.localPosition = new Vector3(0, 0.3f, -0.2f);
        hair.transform.localScale = new Vector3(1.2f, 1.1f, 1.3f);
        hair.GetComponent<Renderer>().material = hairMat;
        Object.Destroy(hair.GetComponent<Collider>());

        // Headband
        GameObject headband = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headband.name = "Headband";
        headband.transform.SetParent(head.transform);
        headband.transform.localPosition = new Vector3(0, 0.4f, 0);
        headband.transform.localScale = new Vector3(1.1f, 0.15f, 1.1f);
        headband.GetComponent<Renderer>().material = cheetahMat;
        Object.Destroy(headband.GetComponent<Collider>());

        // Eyes
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(head.transform);
            eye.transform.localPosition = new Vector3(side * 0.2f, 0.1f, 0.35f);
            eye.transform.localScale = Vector3.one * 0.2f;
            Material eyeMat = new Material(Shader.Find("Standard"));
            eyeMat.color = new Color(0.4f, 0.25f, 0.15f); // Brown eyes
            eye.GetComponent<Renderer>().material = eyeMat;
            Object.Destroy(eye.GetComponent<Collider>());
        }

        // Earrings
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject earring = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earring.name = "Earring";
            earring.transform.SetParent(head.transform);
            earring.transform.localPosition = new Vector3(side * 0.4f, -0.1f, 0);
            earring.transform.localScale = Vector3.one * 0.15f;
            earring.GetComponent<Renderer>().material = earringMat;
            Object.Destroy(earring.GetComponent<Collider>());
        }

        // Arms
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm";
            arm.transform.SetParent(transform);
            arm.transform.localPosition = new Vector3(side * 0.3f, 1.2f, 0);
            arm.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            arm.GetComponent<Renderer>().material = skinMat;
            Object.Destroy(arm.GetComponent<Collider>());
        }

        // Cheetah print skirt/pants
        GameObject skirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skirt.name = "Skirt";
        skirt.transform.SetParent(transform);
        skirt.transform.localPosition = new Vector3(0, 0.7f, 0);
        skirt.transform.localScale = new Vector3(0.4f, 0.4f, 0.28f);
        skirt.GetComponent<Renderer>().material = cheetahMat;
        Object.Destroy(skirt.GetComponent<Collider>());

        // Legs
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "Leg";
            leg.transform.SetParent(transform);
            leg.transform.localPosition = new Vector3(side * 0.12f, 0.3f, 0);
            leg.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            leg.GetComponent<Renderer>().material = skinMat;
            Object.Destroy(leg.GetComponent<Collider>());
        }

        // Add cheetah spots to clothing
        AddCheetahSpots(torso, spotMat, 8);
        AddCheetahSpots(skirt, spotMat, 6);
        AddCheetahSpots(headband, spotMat, 3);
    }

    void AddCheetahSpots(GameObject parent, Material spotMat, int spotCount)
    {
        for (int i = 0; i < spotCount; i++)
        {
            GameObject spot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spot.name = "Spot" + i;
            spot.transform.SetParent(parent.transform);

            // Random position on the surface
            float x = Random.Range(-0.4f, 0.4f);
            float y = Random.Range(-0.4f, 0.4f);
            float z = 0.51f; // Just in front of the surface

            spot.transform.localPosition = new Vector3(x, y, z);
            spot.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
            spot.GetComponent<Renderer>().material = spotMat;
            Object.Destroy(spot.GetComponent<Collider>());
        }
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check distance to player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            isNearPlayer = dist < 3.5f;
        }

        // Toggle dialogue with E
        if (isNearPlayer && Input.GetKeyDown(KeyCode.E))
        {
            dialogueOpen = !dialogueOpen;
            if (dialogueOpen)
            {
                dialogueStage = 0;
            }
        }

        // Close if player walks away
        if (!isNearPlayer && dialogueOpen)
        {
            dialogueOpen = false;
        }

        // Close with Escape
        if (dialogueOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            dialogueOpen = false;
        }

        // Update quest marker visibility
        if (questMarker != null)
        {
            questMarker.SetActive(currentQuestState == QuestState.NotStarted ||
                                  currentQuestState == QuestState.Completed);
        }
    }

    public void OnSnakeKilled()
    {
        if (currentQuestState == QuestState.InProgress)
        {
            snakesKilled++;
            if (snakesKilled >= snakesRequired)
            {
                currentQuestState = QuestState.Completed;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Quest Complete! Return to Rena the Cumbia Queen.", new Color(0.3f, 1f, 0.5f));
                }
            }
            else
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification($"Snakes killed: {snakesKilled}/{snakesRequired}", new Color(1f, 0.9f, 0.5f));
                }
            }
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Initialize styles
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 26;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = new Color(0.4f, 0.8f, 0.3f);

            dialogueStyle = new GUIStyle(GUI.skin.label);
            dialogueStyle.fontSize = 16;
            dialogueStyle.wordWrap = true;
            dialogueStyle.normal.textColor = new Color(0.95f, 0.95f, 0.9f);

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
            buttonStyle.fontStyle = FontStyle.Bold;

            questStyle = new GUIStyle(GUI.skin.label);
            questStyle.fontSize = 14;
            questStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
            questStyle.fontStyle = FontStyle.Italic;
        }

        // Show interact prompt
        if (isNearPlayer && !dialogueOpen)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 20;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);

            string prompt = "[E] Talk to Rena";
            Vector2 size = promptStyle.CalcSize(new GUIContent(prompt));
            GUI.Label(new Rect((Screen.width - size.x) / 2, Screen.height - 120, size.x, size.y), prompt, promptStyle);
        }

        if (!dialogueOpen) return;

        // Dialogue window
        float windowWidth = 500;
        float windowHeight = 300;
        float x = (Screen.width - windowWidth) / 2;
        float y = (Screen.height - windowHeight) / 2;

        // Background
        GUI.Box(new Rect(x - 10, y - 10, windowWidth + 20, windowHeight + 20), "");
        GUI.Box(new Rect(x, y, windowWidth, windowHeight), "");

        // Header
        GUI.Label(new Rect(x, y + 10, windowWidth, 35), "Rena the Cumbia Queen", headerStyle);

        // Close button
        if (GUI.Button(new Rect(x + windowWidth - 35, y + 5, 30, 30), "X"))
        {
            dialogueOpen = false;
        }

        // Dialogue content based on quest state
        float contentY = y + 60;

        switch (currentQuestState)
        {
            case QuestState.NotStarted:
                DrawQuestStart(x, contentY, windowWidth);
                break;
            case QuestState.InProgress:
                DrawQuestInProgress(x, contentY, windowWidth);
                break;
            case QuestState.Completed:
                DrawQuestCompleted(x, contentY, windowWidth);
                break;
            case QuestState.Rewarded:
                DrawQuestRewarded(x, contentY, windowWidth);
                break;
        }
    }

    void DrawQuestStart(float x, float y, float width)
    {
        string[] dialogues = {
            "\"Ay, mi amor! Welcome to my jungle paradise!\n\nI'm Rena, the Cumbia Queen! I dance with the rhythm of the jungle, but these snakes... they're killing my vibe, chica!\"",
            "\"These serpientes are ruining my dance performances! The music can't flow when I'm dodging fangs, you know?\n\nKill 3 snakes for me and I'll reward you like royalty, mi rey!\"",
            "\"So what do you say, guapo? Will you help the Cumbia Queen reclaim her dance floor?\""
        };

        if (dialogueStage < dialogues.Length)
        {
            GUI.Label(new Rect(x + 20, y, width - 40, 120), dialogues[dialogueStage], dialogueStyle);

            if (dialogueStage < dialogues.Length - 1)
            {
                if (GUI.Button(new Rect(x + width / 2 - 60, y + 150, 120, 35), "Continue"))
                {
                    dialogueStage++;
                }
            }
            else
            {
                if (GUI.Button(new Rect(x + 80, y + 150, 140, 35), "Accept Quest"))
                {
                    currentQuestState = QuestState.InProgress;
                    snakesKilled = 0;

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowLootNotification("Quest Started: Kill 3 Snakes", new Color(0.5f, 0.8f, 1f));
                    }

                    dialogueOpen = false;
                }

                if (GUI.Button(new Rect(x + width - 220, y + 150, 140, 35), "Maybe Later"))
                {
                    dialogueOpen = false;
                }
            }
        }
    }

    void DrawQuestInProgress(float x, float y, float width)
    {
        GUI.Label(new Rect(x + 20, y, width - 40, 80),
            "\"Hola again, mi guerrero! How's the snake hunting going?\n\nI need you to clear 3 snakes so I can dance freely again! Dale, dale!\"",
            dialogueStyle);

        // Quest progress
        GUI.Label(new Rect(x + 20, y + 100, width - 40, 30),
            $"Progress: {snakesKilled}/{snakesRequired} snakes killed",
            questStyle);

        // Progress bar
        float barWidth = width - 80;
        float progress = (float)snakesKilled / snakesRequired;
        GUI.Box(new Rect(x + 40, y + 130, barWidth, 20), "");

        Color oldColor = GUI.color;
        GUI.color = new Color(0.3f, 0.8f, 0.3f);
        GUI.DrawTexture(new Rect(x + 42, y + 132, (barWidth - 4) * progress, 16), Texture2D.whiteTexture);
        GUI.color = oldColor;

        if (GUI.Button(new Rect(x + width / 2 - 50, y + 170, 100, 35), "OK"))
        {
            dialogueOpen = false;
        }
    }

    void DrawQuestCompleted(float x, float y, float width)
    {
        GUI.Label(new Rect(x + 20, y, width - 40, 100),
            "\"Ay, que maravilla! You did it, mi campeon!\n\nThe jungle is safe for dancing again! Here's your reward - a machete sharp enough to cut through anything! Time to celebrate with cumbia!\"",
            dialogueStyle);

        if (GUI.Button(new Rect(x + width / 2 - 70, y + 150, 140, 40), "Claim Reward"))
        {
            // Give reward
            int goldReward = 500;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(goldReward);
            }

            // Give machete weapon
            WeaponShopNPC weaponShop = FindObjectOfType<WeaponShopNPC>();
            if (weaponShop != null)
            {
                weaponShop.UnlockWeapon("Machete");
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"Received {goldReward} gold and Machete!", new Color(1f, 0.85f, 0.3f));
            }

            currentQuestState = QuestState.Rewarded;
        }
    }

    void DrawQuestRewarded(float x, float y, float width)
    {
        GUI.Label(new Rect(x + 20, y, width - 40, 100),
            "\"Gracias once more, mi corazon!\n\nNow I can dance to the rhythm of the jungle again! If you go deeper into the selva, watch out - there are ancient ruins with treasures... and danger! Cuidado!\"",
            dialogueStyle);

        if (GUI.Button(new Rect(x + width / 2 - 50, y + 150, 100, 35), "Goodbye"))
        {
            dialogueOpen = false;
        }
    }
}
