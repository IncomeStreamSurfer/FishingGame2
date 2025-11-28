using UnityEngine;
using System.Collections;

/// <summary>
/// Wetsuit Pete's Quest System with speech bubble dialog
/// Two quests: Tackle Box retrieval and Salmon collection
/// </summary>
public class WetsuitPeteQuests : MonoBehaviour
{
    public static WetsuitPeteQuests Instance { get; private set; }

    // Quest state
    public enum QuestState { None, TackleBoxAvailable, TackleBoxActive, SalmonAvailable, SalmonActive, AllComplete }
    public QuestState currentState = QuestState.TackleBoxAvailable;

    // Quest tracking
    private int salmonCaught = 0;
    private const int salmonRequired = 5;
    public bool tackleBoxFound = false;

    // UI State
    private bool playerNearby = false;
    private bool dialogOpen = false;
    private float interactionDistance = 5f;
    private float dialogTimer = 0f;
    private float dialogDuration = 10f;

    // Textures
    private Texture2D bubbleTexture;
    private Texture2D borderTexture;
    private bool stylesInitialized = false;

    // Audio
    private AudioSource audioSource;
    private float hummTimer = 0f;
    private float nextHummTime = 3f;
    private bool isHumming = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetupAudio();
        Invoke("InitializeStyles", 0.5f);
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 12f;
        audioSource.volume = 0.4f;
        audioSource.playOnAwake = false;
    }

    void InitializeStyles()
    {
        bubbleTexture = new Texture2D(1, 1);
        bubbleTexture.SetPixel(0, 0, new Color(0.95f, 0.98f, 1f, 0.98f)); // Slight blue tint
        bubbleTexture.Apply();

        borderTexture = new Texture2D(1, 1);
        borderTexture.SetPixel(0, 0, new Color(0.1f, 0.2f, 0.3f, 1f)); // Dark blue border
        borderTexture.Apply();

        stylesInitialized = true;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            bool wasNearby = playerNearby;
            playerNearby = distance < interactionDistance;

            // Auto-close dialog when player wanders too far
            if (dialogOpen && distance > interactionDistance * 1.5f)
            {
                dialogOpen = false;
                Debug.Log("Dialog closed - player walked away");
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
                PlayHumm();
            }
        }

        // Handle dialog timer and close inputs
        if (dialogOpen)
        {
            dialogTimer -= Time.deltaTime;
            if (dialogTimer <= 0 || Input.GetKeyDown(KeyCode.Escape))
            {
                dialogOpen = false;
            }
        }

        // Idle humming
        if (playerNearby && !dialogOpen)
        {
            hummTimer += Time.deltaTime;
            if (hummTimer >= nextHummTime)
            {
                PlayHumm();
                hummTimer = 0f;
                nextHummTime = Random.Range(4f, 8f);
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
        StartCoroutine(GenerateHummSound());
    }

    IEnumerator GenerateHummSound()
    {
        isHumming = true;

        int sampleRate = 44100;
        float duration = Random.Range(0.5f, 0.9f);
        int sampleCount = (int)(sampleRate * duration);
        AudioClip hummClip = AudioClip.Create("PeteHumm", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        float baseFreq = Random.Range(140f, 180f); // Lower humm for Pete

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;
            float envelope = Mathf.Sin(progress * Mathf.PI);

            float wave = 0f;
            wave += Mathf.Sin(2 * Mathf.PI * baseFreq * t) * 0.5f;
            wave += Mathf.Sin(2 * Mathf.PI * baseFreq * 2f * t) * 0.2f;
            wave += Mathf.Sin(2 * Mathf.PI * baseFreq * 3f * t) * 0.1f;

            float vibrato = Mathf.Sin(2 * Mathf.PI * 4f * t) * 0.03f;
            wave *= (1f + vibrato);

            samples[i] = wave * envelope * 0.25f;
        }

        hummClip.SetData(samples, 0);
        audioSource.clip = hummClip;
        audioSource.pitch = Random.Range(0.85f, 1.05f);
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
        float bubbleWidth = 480;
        float bubbleHeight = 140;
        float bubbleX = (Screen.width - bubbleWidth) / 2;
        float bubbleY = Screen.height * 0.15f;
        float borderWidth = 4;
        float pointerSize = 20;

        // Border
        GUI.DrawTexture(new Rect(bubbleX - borderWidth, bubbleY - borderWidth,
            bubbleWidth + borderWidth * 2, bubbleHeight + borderWidth * 2), borderTexture);

        // Bubble
        GUI.DrawTexture(new Rect(bubbleX, bubbleY, bubbleWidth, bubbleHeight), bubbleTexture);

        // Pointer
        GUI.DrawTexture(new Rect(bubbleX + bubbleWidth / 2 - borderWidth, bubbleY + bubbleHeight,
            borderWidth * 2, pointerSize), borderTexture);
        GUI.DrawTexture(new Rect(bubbleX + bubbleWidth / 2 - borderWidth + 2, bubbleY + bubbleHeight,
            borderWidth * 2 - 4, pointerSize - 2), bubbleTexture);

        // Name style
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 14;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = new Color(0.1f, 0.3f, 0.5f);
        nameStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(bubbleX, bubbleY + 8, bubbleWidth, 20), "~ Wetsuit Pete ~", nameStyle);

        // Dialog text style
        GUIStyle textStyle = new GUIStyle();
        textStyle.fontSize = 16;
        textStyle.fontStyle = FontStyle.Italic;
        textStyle.normal.textColor = new Color(0.1f, 0.1f, 0.15f);
        textStyle.alignment = TextAnchor.UpperCenter;
        textStyle.wordWrap = true;
        textStyle.padding = new RectOffset(20, 20, 0, 0);

        // Button style
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;

        string dialogText = "";
        bool showAcceptButton = false;
        bool showCompleteButton = false;

        switch (currentState)
        {
            case QuestState.TackleBoxAvailable:
                dialogText = "\"Ahoy mate! Fancy a dip? I need someone to retrieve my lost tackle box. Maybe you could try fishing with that weird stick you're holding... why are you holding it like that?\"";
                showAcceptButton = true;
                break;

            case QuestState.TackleBoxActive:
                if (tackleBoxFound)
                {
                    dialogText = "\"Blimey! You found it! My precious tackle box! Here, take this reward, you've earned it mate!\"";
                    showCompleteButton = true;
                }
                else
                {
                    dialogText = "\"Still looking for that tackle box eh? Keep fishing around here, it's gotta be in the water somewhere. You'll know it when you see it - golden glow and all!\"";
                }
                break;

            case QuestState.SalmonAvailable:
                dialogText = "\"Bonjour! I'm after 5 salmon, can you do me a solid? They should be easier to catch while you're helping me out!\"";
                showAcceptButton = true;
                break;

            case QuestState.SalmonActive:
                if (salmonCaught >= salmonRequired)
                {
                    dialogText = $"\"Magnifique! You got all {salmonRequired} salmon! You're a proper fishing legend, mate!\"";
                    showCompleteButton = true;
                }
                else
                {
                    dialogText = $"\"Keep at it! You've caught {salmonCaught} out of {salmonRequired} salmon so far. The waters are teeming with them right now!\"";
                }
                break;

            case QuestState.AllComplete:
                dialogText = "\"You've helped me out so much, mate! Cheers! Now go catch some big ones for yourself!\"";
                break;
        }

        GUI.Label(new Rect(bubbleX, bubbleY + 30, bubbleWidth, 70), dialogText, textStyle);

        // Buttons
        float buttonY = bubbleY + bubbleHeight - 35;

        if (showAcceptButton)
        {
            if (GUI.Button(new Rect(bubbleX + bubbleWidth / 2 - 60, buttonY, 120, 28), "ACCEPT QUEST", buttonStyle))
            {
                AcceptCurrentQuest();
            }
        }
        else if (showCompleteButton)
        {
            if (GUI.Button(new Rect(bubbleX + bubbleWidth / 2 - 70, buttonY, 140, 28), "CLAIM REWARD", buttonStyle))
            {
                CompleteCurrentQuest();
            }
        }

        // Close hint
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 11;
        hintStyle.normal.textColor = new Color(0.4f, 0.4f, 0.5f);
        hintStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(bubbleX, bubbleY + bubbleHeight + pointerSize + 5, bubbleWidth, 18),
            "Press ESC to close", hintStyle);
    }

    void DrawInteractionPrompt()
    {
        GUIStyle promptStyle = new GUIStyle();
        promptStyle.fontSize = 18;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.normal.textColor = new Color(0.7f, 0.9f, 1f);
        promptStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle shadowStyle = new GUIStyle(promptStyle);
        shadowStyle.normal.textColor = new Color(0, 0, 0, 0.7f);

        float promptY = Screen.height * 0.6f;
        string text = "[E] Talk to Pete";

        bool hasQuest = currentState == QuestState.TackleBoxAvailable || currentState == QuestState.SalmonAvailable;
        bool questReady = (currentState == QuestState.TackleBoxActive && tackleBoxFound) ||
                         (currentState == QuestState.SalmonActive && salmonCaught >= salmonRequired);

        if (hasQuest)
        {
            promptStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
            text = "[E] Talk to Pete (!)";
        }
        else if (questReady)
        {
            promptStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
            text = "[E] Talk to Pete (Quest Complete!)";
        }

        GUI.Label(new Rect(2, promptY + 2, Screen.width, 30), text, shadowStyle);
        GUI.Label(new Rect(0, promptY, Screen.width, 30), text, promptStyle);
    }

    void AcceptCurrentQuest()
    {
        if (currentState == QuestState.TackleBoxAvailable)
        {
            currentState = QuestState.TackleBoxActive;
            tackleBoxFound = false;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Quest Started: Find the Tackle Box!", new Color(0.3f, 0.7f, 1f));
            }
            Debug.Log("Tackle Box quest accepted - 10% chance to find it while fishing");
        }
        else if (currentState == QuestState.SalmonAvailable)
        {
            currentState = QuestState.SalmonActive;
            salmonCaught = 0;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Quest Started: Catch 5 Salmon!", new Color(0.3f, 0.7f, 1f));
            }
            Debug.Log("Salmon quest accepted - 40% salmon chance active");
        }

        dialogOpen = false;
    }

    void CompleteCurrentQuest()
    {
        if (currentState == QuestState.TackleBoxActive && tackleBoxFound)
        {
            // Reward: 500 coins, 200 XP
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(500);
            }
            if (LevelingSystem.Instance != null)
            {
                LevelingSystem.Instance.AddXP(200);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Quest Complete! +500 Gold, +200 XP", new Color(1f, 0.85f, 0.2f));
            }

            currentState = QuestState.SalmonAvailable;
            Debug.Log("Tackle Box quest complete! Salmon quest now available.");
        }
        else if (currentState == QuestState.SalmonActive && salmonCaught >= salmonRequired)
        {
            // Reward: 300 coins, 300 XP
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(300);
            }
            if (LevelingSystem.Instance != null)
            {
                LevelingSystem.Instance.AddXP(300);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Quest Complete! +300 Gold, +300 XP", new Color(1f, 0.85f, 0.2f));
            }

            currentState = QuestState.AllComplete;
            Debug.Log("Salmon quest complete! All Pete quests done.");
        }

        dialogOpen = false;
    }

    // Called by FishingSystem when a catch is made
    public void OnFishCaught(string fishId)
    {
        if (currentState == QuestState.SalmonActive && fishId == "salmon")
        {
            salmonCaught++;
            Debug.Log($"Salmon caught for Pete's quest: {salmonCaught}/{salmonRequired}");

            if (salmonCaught >= salmonRequired)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("All salmon caught! Return to Pete!", new Color(0.3f, 1f, 0.5f));
                }
            }
        }
    }

    // Called when tackle box is found
    public void OnTackleBoxFound()
    {
        if (currentState == QuestState.TackleBoxActive)
        {
            tackleBoxFound = true;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Tackle Box Found! Return to Pete!", new Color(1f, 0.9f, 0.3f));
            }
            Debug.Log("Tackle box found! Return to Pete.");
        }
    }

    // Check if tackle box quest is active (for FishingSystem to use)
    public bool IsTackleBoxQuestActive()
    {
        return currentState == QuestState.TackleBoxActive && !tackleBoxFound;
    }

    // Check if salmon quest is active (for FishingSystem to use)
    public bool IsSalmonQuestActive()
    {
        return currentState == QuestState.SalmonActive && salmonCaught < salmonRequired;
    }

    void OnDestroy()
    {
        if (bubbleTexture != null) Destroy(bubbleTexture);
        if (borderTexture != null) Destroy(borderTexture);
    }
}
