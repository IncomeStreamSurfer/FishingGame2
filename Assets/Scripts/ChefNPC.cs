using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Chef Gusteau NPC - Cooks fish into buff meals
/// Optimized for maximum performance
/// </summary>
public class ChefNPC : MonoBehaviour
{
    public static ChefNPC Instance { get; private set; }

    // State
    private bool questStarted = false;
    private bool playerNearby = false;
    private bool showingDialogue = false;
    private int dialogueState = 0;
    private string currentQuestFishId = null;
    private string currentQuestFishName = null;

    // References
    private Transform playerTransform;
    private GameObject cookingFire;
    private Light fireLight;

    // Performance: Pre-allocated
    private static readonly float interactionRange = 4f;
    private static readonly Vector3 fireScaleBase = new Vector3(0.8f, 0.4f, 0.8f);
    private Vector3 fireScaleTemp = new Vector3(0.8f, 0.4f, 0.8f);
    private float flickerTime = 0f;

    // Cached textures (created once)
    private Texture2D bgTex;
    private Texture2D btnTex;
    private Texture2D btnHoverTex;

    // Cached GUIStyle (single style, reused)
    private GUIStyle labelStyle;
    private bool stylesReady = false;

    // Static colors
    private static readonly Color titleColor = new Color(1f, 0.9f, 0.7f);
    private static readonly Color textColor = new Color(0.9f, 0.85f, 0.7f);
    private static readonly Color dimColor = new Color(0.7f, 0.7f, 0.6f);
    private static readonly Color greenColor = new Color(0.3f, 1f, 0.4f);
    private static readonly Color goldColor = new Color(1f, 0.85f, 0.3f);
    private static readonly Color grayColor = new Color(0.5f, 0.5f, 0.5f);

    void Awake()
    {
        // Strict singleton - destroy duplicates immediately
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("ChefNPC: Duplicate detected, destroying.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        CreateVisuals();
        CreateTextures();
        LoadQuestState();
    }

    void CreateTextures()
    {
        bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0.1f, 0.08f, 0.06f, 0.95f));
        bgTex.Apply();

        btnTex = new Texture2D(1, 1);
        btnTex.SetPixel(0, 0, new Color(0.3f, 0.25f, 0.2f, 1f));
        btnTex.Apply();

        btnHoverTex = new Texture2D(1, 1);
        btnHoverTex.SetPixel(0, 0, new Color(0.5f, 0.4f, 0.3f, 1f));
        btnHoverTex.Apply();
    }

    void CreateVisuals()
    {
        // Simple chef body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 1f, 0);
        body.transform.localScale = new Vector3(0.8f, 1f, 0.5f);
        Destroy(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.2f, 0.25f));

        // White apron
        GameObject apron = GameObject.CreatePrimitive(PrimitiveType.Cube);
        apron.name = "Apron";
        apron.transform.SetParent(transform);
        apron.transform.localPosition = new Vector3(0, 0.9f, 0.2f);
        apron.transform.localScale = new Vector3(0.7f, 1.1f, 0.1f);
        Destroy(apron.GetComponent<Collider>());
        apron.GetComponent<Renderer>().material = MakeMat(new Color(0.95f, 0.95f, 0.9f));

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 2.1f, 0);
        head.transform.localScale = new Vector3(0.5f, 0.55f, 0.5f);
        Destroy(head.GetComponent<Collider>());
        head.GetComponent<Renderer>().material = MakeMat(new Color(0.9f, 0.75f, 0.6f));

        // Chef hat
        GameObject hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hat.name = "Hat";
        hat.transform.SetParent(transform);
        hat.transform.localPosition = new Vector3(0, 2.5f, 0);
        hat.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
        Destroy(hat.GetComponent<Collider>());
        hat.GetComponent<Renderer>().material = MakeMat(new Color(0.98f, 0.98f, 0.95f));

        // Cooking fire (simple)
        cookingFire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cookingFire.name = "Fire";
        cookingFire.transform.SetParent(transform);
        cookingFire.transform.localPosition = new Vector3(1.5f, 0.4f, 0);
        cookingFire.transform.localScale = fireScaleBase;
        Destroy(cookingFire.GetComponent<Collider>());
        Material fireMat = MakeMat(new Color(1f, 0.4f, 0.1f));
        fireMat.EnableKeyword("_EMISSION");
        fireMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 2f);
        cookingFire.GetComponent<Renderer>().material = fireMat;

        // Fire light
        GameObject lightObj = new GameObject("FireLight");
        lightObj.transform.SetParent(cookingFire.transform);
        lightObj.transform.localPosition = Vector3.up * 0.5f;
        fireLight = lightObj.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.6f, 0.2f);
        fireLight.intensity = 1.5f;
        fireLight.range = 5f;

        // Cooking pot
        GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pot.name = "Pot";
        pot.transform.SetParent(transform);
        pot.transform.localPosition = new Vector3(1.5f, 0.8f, 0);
        pot.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
        Destroy(pot.GetComponent<Collider>());
        Material potMat = MakeMat(new Color(0.2f, 0.2f, 0.22f));
        potMat.SetFloat("_Metallic", 0.7f);
        pot.GetComponent<Renderer>().material = potMat;
    }

    Material MakeMat(Color c)
    {
        Material m = new Material(Shader.Find("Standard"));
        m.color = c;
        return m;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Get player once
        if (playerTransform == null && GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        if (playerTransform == null) return;

        // Distance check
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        playerNearby = dist <= interactionRange;

        // Input
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !showingDialogue)
            OpenDialogue();

        if (showingDialogue && Input.GetKeyDown(KeyCode.Escape))
            showingDialogue = false;

        // Fire flicker (no allocation - reuse vector)
        flickerTime += Time.deltaTime;
        float flicker = 1f + Mathf.Sin(flickerTime * 10f) * 0.1f;
        fireScaleTemp.x = fireScaleBase.x * flicker;
        fireScaleTemp.y = fireScaleBase.y * flicker;
        fireScaleTemp.z = fireScaleBase.z * flicker;
        cookingFire.transform.localScale = fireScaleTemp;

        // Look at player
        if (playerNearby)
        {
            Vector3 dir = playerTransform.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 3f);
        }
    }

    void OnGUI()
    {
        // Skip entirely if not needed
        if (!MainMenu.GameStarted) return;
        if (!playerNearby && !showingDialogue) return;

        // Init style once
        if (!stylesReady)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            stylesReady = true;
        }

        if (playerNearby && !showingDialogue)
            DrawPrompt();

        if (showingDialogue)
            DrawDialogue();
    }

    void DrawPrompt()
    {
        labelStyle.fontSize = 16;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = titleColor;
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 120, 200, 30), "Chef Gusteau", labelStyle);

        labelStyle.fontSize = 14;
        labelStyle.fontStyle = FontStyle.Normal;
        labelStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 95, 200, 25), "[E] Talk", labelStyle);
    }

    void DrawDialogue()
    {
        // Overlay
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float w = 480, h = 320;
        float x = (Screen.width - w) / 2;
        float y = (Screen.height - h) / 2;

        GUI.DrawTexture(new Rect(x, y, w, h), bgTex);

        // Title
        labelStyle.fontSize = 22;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = titleColor;
        GUI.Label(new Rect(x, y + 15, w, 30), "Chef Gusteau", labelStyle);

        // Close button
        if (GUI.Button(new Rect(x + w - 35, y + 10, 25, 25), "X"))
            showingDialogue = false;

        // Content based on state
        labelStyle.fontSize = 15;
        labelStyle.fontStyle = FontStyle.Normal;
        labelStyle.wordWrap = true;
        labelStyle.alignment = TextAnchor.UpperCenter;
        labelStyle.normal.textColor = textColor;

        switch (dialogueState)
        {
            case 0: DrawIntro(x, y, w, h); break;
            case 1: DrawQuestList(x, y, w, h); break;
            case 2: DrawActiveQuest(x, y, w, h); break;
            case 3: DrawComplete(x, y, w, h); break;
        }
    }

    void DrawIntro(float x, float y, float w, float h)
    {
        GUI.Label(new Rect(x + 30, y + 60, w - 60, 100),
            "\"Ah, a fisher! I am Chef Gusteau. Bring me special fish and I will cook you something... magical!\"", labelStyle);

        if (DrawBtn(new Rect(x + w / 2 - 70, y + h - 70, 140, 35), "Accept"))
        {
            questStarted = true;
            dialogueState = 1;
            SaveState();
        }
    }

    void DrawQuestList(float x, float y, float w, float h)
    {
        GUI.Label(new Rect(x + 20, y + 55, w - 40, 35),
            "\"Which fish shall I prepare for you?\"", labelStyle);

        float qy = y + 100;
        if (FishBuffSystem.Instance != null)
        {
            labelStyle.fontSize = 13;
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = new Color(0.4f, 0.7f, 1f); // Blue for all quests

            foreach (var buff in FishBuffSystem.Instance.allBuffs)
            {
                bool completedBefore = FishBuffSystem.Instance.IsQuestCompleted(buff.requiredFishId);
                Rect r = new Rect(x + 25, qy, w - 50, 32);

                GUI.DrawTexture(r, btnTex);

                // Show "Repeatable" only after completing quest once
                string questText = completedBefore
                    ? $"{buff.requiredFishName} -> {buff.buffName} (Repeatable)"
                    : $"{buff.requiredFishName} -> {buff.buffName}";

                GUI.Label(new Rect(r.x + 10, r.y, r.width - 20, r.height), questText, labelStyle);

                if (GUI.Button(r, "", GUIStyle.none))
                {
                    currentQuestFishId = buff.requiredFishId;
                    currentQuestFishName = buff.requiredFishName;
                    dialogueState = 2;
                    SaveState();
                }

                qy += 38;
            }
        }
    }

    void DrawActiveQuest(float x, float y, float w, float h)
    {
        labelStyle.alignment = TextAnchor.UpperCenter;
        labelStyle.normal.textColor = textColor;
        GUI.Label(new Rect(x + 30, y + 60, w - 60, 50),
            $"\"Bring me a {currentQuestFishName}!\"", labelStyle);

        FishBuff buff = FishBuffSystem.Instance?.GetBuffByFishId(currentQuestFishId);
        bool isFirstTime = FishBuffSystem.Instance != null && !FishBuffSystem.Instance.IsQuestCompleted(currentQuestFishId);

        labelStyle.normal.textColor = dimColor;
        labelStyle.fontSize = 13;

        // Show different rewards for first completion vs repeat
        string rewardText = isFirstTime
            ? $"Reward: {buff?.buffName ?? "Buff"}\n+2000 XP (First Time Bonus!)"
            : $"Reward: {buff?.buffName ?? "Buff"} (added to inventory)";
        GUI.Label(new Rect(x + 30, y + 120, w - 60, 50), rewardText, labelStyle);

        bool hasFish = FishBuffSystem.Instance != null && FishBuffSystem.Instance.HasRequiredFish(currentQuestFishId);

        if (hasFish)
        {
            labelStyle.normal.textColor = greenColor;
            labelStyle.fontSize = 16;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(x, y + 180, w, 25), "You have the fish!", labelStyle);

            if (DrawBtn(new Rect(x + w / 2 - 70, y + h - 70, 140, 35), "Turn In"))
                dialogueState = 3;
        }

        if (DrawBtn(new Rect(x + w / 2 - 50, y + h - 30, 100, 25), "Cancel"))
        {
            currentQuestFishId = null;
            currentQuestFishName = null;
            dialogueState = 1;
            SaveState();
        }
    }

    void DrawComplete(float x, float y, float w, float h)
    {
        labelStyle.alignment = TextAnchor.UpperCenter;
        labelStyle.normal.textColor = textColor;
        GUI.Label(new Rect(x + 30, y + 60, w - 60, 60),
            $"\"Magnifique! This {currentQuestFishName} is perfect!\"", labelStyle);

        FishBuff buff = FishBuffSystem.Instance?.GetBuffByFishId(currentQuestFishId);
        bool isFirstTime = FishBuffSystem.Instance != null && !FishBuffSystem.Instance.IsQuestCompleted(currentQuestFishId);

        labelStyle.normal.textColor = goldColor;
        labelStyle.fontSize = 18;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x, y + 140, w, 25), $"Earned: {buff?.buffName ?? "Buff"}!", labelStyle);

        // Show XP only for first time completion
        if (isFirstTime)
        {
            labelStyle.normal.textColor = greenColor;
            labelStyle.fontSize = 14;
            GUI.Label(new Rect(x, y + 165, w, 25), "+2000 XP", labelStyle);
        }
        else
        {
            labelStyle.normal.textColor = dimColor;
            labelStyle.fontSize = 12;
            GUI.Label(new Rect(x, y + 165, w, 25), "(Added to inventory)", labelStyle);
        }

        if (DrawBtn(new Rect(x + w / 2 - 70, y + h - 70, 140, 35), "Claim"))
        {
            FishBuffSystem.Instance?.ConsumeFish(currentQuestFishId);
            FishBuffSystem.Instance?.CompleteQuest(currentQuestFishId);

            // Show different notification based on first time or repeat
            if (UIManager.Instance != null && buff != null)
            {
                if (isFirstTime)
                    UIManager.Instance.ShowLootNotification($"Earned: {buff.buffName} + 2000 XP!", buff.bowlColor);
                else
                    UIManager.Instance.ShowLootNotification($"+1 {buff.buffName}!", buff.bowlColor);
            }

            currentQuestFishId = null;
            currentQuestFishName = null;
            dialogueState = 1;
            SaveState();
        }
    }

    bool DrawBtn(Rect r, string text)
    {
        bool hover = r.Contains(Event.current.mousePosition);
        GUI.DrawTexture(r, hover ? btnHoverTex : btnTex);

        labelStyle.fontSize = 14;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        GUI.Label(r, text, labelStyle);

        return GUI.Button(r, "", GUIStyle.none);
    }

    void OpenDialogue()
    {
        showingDialogue = true;

        // Play NPC voice greeting
        if (IslandSoundManager.Instance != null)
        {
            IslandSoundManager.Instance.PlayNPCVoice("ooh");
        }

        if (!questStarted)
            dialogueState = 0;
        else if (currentQuestFishId != null)
        {
            if (FishBuffSystem.Instance != null && FishBuffSystem.Instance.HasRequiredFish(currentQuestFishId))
                dialogueState = 3;
            else
                dialogueState = 2;
        }
        else
            dialogueState = 1;
    }

    void SaveState()
    {
        PlayerPrefs.SetInt("ChefQuestStarted", questStarted ? 1 : 0);
        PlayerPrefs.SetString("ChefQuestFish", currentQuestFishId ?? "");
        PlayerPrefs.SetString("ChefQuestName", currentQuestFishName ?? "");
        PlayerPrefs.Save();
    }

    void LoadQuestState()
    {
        questStarted = PlayerPrefs.GetInt("ChefQuestStarted", 0) == 1;
        currentQuestFishId = PlayerPrefs.GetString("ChefQuestFish", "");
        currentQuestFishName = PlayerPrefs.GetString("ChefQuestName", "");
        if (string.IsNullOrEmpty(currentQuestFishId))
        {
            currentQuestFishId = null;
            currentQuestFishName = null;
        }
    }

    public static bool IsPlayerNearChef()
    {
        if (Instance == null || !GameCache.IsPlayerValid()) return false;
        return Vector3.Distance(Instance.transform.position, GameCache.Player.position) <= 5f;
    }

    public static bool HasCompletedFirstQuest()
    {
        if (FishBuffSystem.Instance == null) return false;
        foreach (var kvp in FishBuffSystem.Instance.completedQuests)
            if (kvp.Value) return true;
        return false;
    }

    void OnDestroy()
    {
        if (bgTex != null) Destroy(bgTex);
        if (btnTex != null) Destroy(btnTex);
        if (btnHoverTex != null) Destroy(btnHoverTex);
        if (Instance == this) Instance = null;
    }
}
