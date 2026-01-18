using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Chef Gusteau - Cooks special fish into buff meals
/// Simple static chef model (no fire effects for performance)
/// </summary>
public class ChefNPC : MonoBehaviour
{
    public static ChefNPC Instance { get; private set; }

    // State
    private bool playerNearby = false;
    private bool showingCookingMenu = false;

    // References
    private Transform playerTransform;

    // Performance: Pre-allocated
    private static readonly float interactionRange = 4f;

    // Cached textures (created once)
    private Texture2D bgTex;
    private Texture2D btnTex;
    private Texture2D btnHoverTex;
    private Texture2D headerTex;

    // Cached GUIStyle (single style, reused)
    private GUIStyle labelStyle;
    private bool stylesReady = false;

    // Static colors (cached to avoid GC allocations)
    private static readonly Color titleColor = new Color(1f, 0.9f, 0.7f);
    private static readonly Color dimColor = new Color(0.7f, 0.7f, 0.6f);
    private static readonly Color goldColor = new Color(1f, 0.85f, 0.3f);
    private static readonly Color grayColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color overlayColor = new Color(0, 0, 0, 0.6f);

    // Performance
    private int guiFrameSkip = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        CreateVisuals();
        CreateTextures();
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

        headerTex = new Texture2D(1, 1);
        headerTex.SetPixel(0, 0, new Color(0.15f, 0.12f, 0.08f, 1f));
        headerTex.Apply();
    }

    void CreateVisuals()
    {
        // Simple chef body - dark uniform
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

        // Simple cooking pot next to chef (no fire, no light)
        GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pot.name = "Pot";
        pot.transform.SetParent(transform);
        pot.transform.localPosition = new Vector3(1.5f, 0.4f, 0);
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
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !showingCookingMenu)
            OpenCookingMenu();

        if (showingCookingMenu && Input.GetKeyDown(KeyCode.Escape))
            showingCookingMenu = false;

        // Look at player when nearby (simple, no frame skip needed for rotation)
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
        if (!MainMenu.GameStarted) return;
        if (!playerNearby && !showingCookingMenu) return;

        // Frame skipping when just showing prompt
        if (!showingCookingMenu)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        // Init style once
        if (!stylesReady)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            stylesReady = true;
        }

        if (playerNearby && !showingCookingMenu)
            DrawPrompt();

        if (showingCookingMenu)
            DrawCookingMenu();
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
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 95, 200, 25), "[E] Cook", labelStyle);
    }

    void DrawCookingMenu()
    {
        // Overlay
        GUI.color = overlayColor;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float w = 420, h = 400;
        float x = (Screen.width - w) / 2;
        float y = (Screen.height - h) / 2;

        // Background
        GUI.DrawTexture(new Rect(x, y, w, h), bgTex);

        // Header
        GUI.DrawTexture(new Rect(x, y, w, 50), headerTex);

        // Title
        labelStyle.fontSize = 20;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = titleColor;
        GUI.Label(new Rect(x, y + 10, w, 30), "Chef Gusteau's Kitchen", labelStyle);

        // Close button
        if (GUI.Button(new Rect(x + w - 35, y + 10, 25, 25), "X"))
            showingCookingMenu = false;

        // Subtitle
        labelStyle.fontSize = 12;
        labelStyle.fontStyle = FontStyle.Italic;
        labelStyle.normal.textColor = dimColor;
        GUI.Label(new Rect(x, y + 55, w, 20), "\"Bring me special fish and I cook for you!\"", labelStyle);

        // Fish list area
        float listY = y + 85;

        if (FishBuffSystem.Instance != null)
        {
            // Check if player has any cookable fish
            bool hasAnyCookableFish = false;
            foreach (var buff in FishBuffSystem.Instance.allBuffs)
            {
                if (FishBuffSystem.Instance.HasRequiredFish(buff.requiredFishId))
                {
                    hasAnyCookableFish = true;
                    break;
                }
            }

            if (!hasAnyCookableFish)
            {
                labelStyle.fontSize = 14;
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.normal.textColor = grayColor;
                GUI.Label(new Rect(x, listY + 80, w, 60), "No special fish to cook.\n\nCatch special fish while fishing\nand bring them here!", labelStyle);
            }
            else
            {
                labelStyle.fontSize = 13;
                labelStyle.alignment = TextAnchor.MiddleLeft;

                float itemHeight = 70;
                float itemY = listY;

                foreach (var buff in FishBuffSystem.Instance.allBuffs)
                {
                    bool hasFish = FishBuffSystem.Instance.HasRequiredFish(buff.requiredFishId);
                    if (!hasFish) continue;

                    Rect itemRect = new Rect(x + 15, itemY, w - 30, itemHeight);
                    bool hover = itemRect.Contains(Event.current.mousePosition);
                    GUI.DrawTexture(itemRect, hover ? btnHoverTex : btnTex);

                    // Fish name
                    labelStyle.fontSize = 15;
                    labelStyle.fontStyle = FontStyle.Bold;
                    labelStyle.normal.textColor = buff.bowlColor;
                    GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 8, itemRect.width - 100, 22), buff.requiredFishName, labelStyle);

                    // Arrow and buff name
                    labelStyle.fontSize = 13;
                    labelStyle.fontStyle = FontStyle.Normal;
                    labelStyle.normal.textColor = goldColor;
                    GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 30, itemRect.width - 100, 18), $"→ {buff.buffName}", labelStyle);

                    // Buff description
                    labelStyle.fontSize = 11;
                    labelStyle.normal.textColor = dimColor;
                    GUI.Label(new Rect(itemRect.x + 10, itemRect.y + 48, itemRect.width - 100, 16), buff.description, labelStyle);

                    // Cook button
                    Rect cookBtn = new Rect(itemRect.x + itemRect.width - 75, itemRect.y + 20, 65, 30);
                    if (DrawBtn(cookBtn, "COOK"))
                    {
                        CookFish(buff);
                    }

                    itemY += itemHeight + 8;
                }
            }
        }

        // Footer hint
        labelStyle.fontSize = 11;
        labelStyle.fontStyle = FontStyle.Normal;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = grayColor;
        GUI.Label(new Rect(x, y + h - 35, w, 20), "Buffs are added to your inventory (TAB to use)", labelStyle);
    }

    void CookFish(FishBuff buff)
    {
        if (FishBuffSystem.Instance == null) return;

        bool isFirstTime = !FishBuffSystem.Instance.IsQuestCompleted(buff.requiredFishId);

        FishBuffSystem.Instance.ConsumeFish(buff.requiredFishId);
        FishBuffSystem.Instance.CompleteQuest(buff.requiredFishId);

        // Award 500 XP for repeat cooks (first time already gives 2000 XP via CompleteQuest)
        if (!isFirstTime && LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.AddXP(500);
        }

        if (IslandSoundManager.Instance != null)
        {
            IslandSoundManager.Instance.PlayChime();
        }

        if (UIManager.Instance != null)
        {
            if (isFirstTime)
                UIManager.Instance.ShowLootNotification($"Cooked: {buff.buffName} + 2000 XP!", buff.bowlColor);
            else
                UIManager.Instance.ShowLootNotification($"Cooked: {buff.buffName} + 500 XP!", buff.bowlColor);
        }
    }

    bool DrawBtn(Rect r, string text)
    {
        bool hover = r.Contains(Event.current.mousePosition);
        GUI.DrawTexture(r, hover ? btnHoverTex : btnTex);

        if (hover)
        {
            GUI.color = goldColor;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - 2, r.width, 2), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        labelStyle.fontSize = 12;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = hover ? goldColor : Color.white;
        GUI.Label(r, text, labelStyle);

        return GUI.Button(r, "", GUIStyle.none);
    }

    void OpenCookingMenu()
    {
        showingCookingMenu = true;

        if (IslandSoundManager.Instance != null)
        {
            IslandSoundManager.Instance.PlayNPCVoice("ooh");
        }
    }

    // Legacy compatibility methods
    public static bool IsPlayerNearChef()
    {
        return IsPlayerNearFire();
    }

    public static bool HasCompletedFirstQuest()
    {
        return true;
    }

    public static bool IsPlayerNearFire()
    {
        if (Instance == null || !GameCache.IsPlayerValid()) return false;
        return Vector3.Distance(Instance.transform.position, GameCache.Player.position) <= 5f;
    }

    void OnDestroy()
    {
        if (bgTex != null) Destroy(bgTex);
        if (btnTex != null) Destroy(btnTex);
        if (btnHoverTex != null) Destroy(btnHoverTex);
        if (headerTex != null) Destroy(headerTex);
        if (Instance == this) Instance = null;
    }
}
