using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cooking Fire - Cook special fish into buff meals
/// Standalone fire interaction (no NPC)
/// </summary>
public class ChefNPC : MonoBehaviour
{
    public static ChefNPC Instance { get; private set; }

    // State
    private bool playerNearby = false;
    private bool showingCookingMenu = false;

    // References
    private Transform playerTransform;
    private GameObject cookingFire;
    private GameObject cookingPot;
    private Light fireLight;

    // Performance: Pre-allocated
    private static readonly float interactionRange = 3.5f;
    private static readonly Vector3 fireScaleBase = new Vector3(0.9f, 0.5f, 0.9f);
    private Vector3 fireScaleTemp = new Vector3(0.9f, 0.5f, 0.9f);
    private float flickerTime = 0f;

    // Cached textures (created once)
    private Texture2D bgTex;
    private Texture2D btnTex;
    private Texture2D btnHoverTex;
    private Texture2D headerTex;

    // Cached GUIStyle (single style, reused)
    private GUIStyle labelStyle;
    private bool stylesReady = false;

    // Static colors (cached to avoid GC allocations)
    private static readonly Color titleColor = new Color(1f, 0.7f, 0.3f);
    private static readonly Color textColor = new Color(0.9f, 0.85f, 0.7f);
    private static readonly Color dimColor = new Color(0.7f, 0.7f, 0.6f);
    private static readonly Color greenColor = new Color(0.3f, 1f, 0.4f);
    private static readonly Color goldColor = new Color(1f, 0.85f, 0.3f);
    private static readonly Color grayColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color overlayColor = new Color(0, 0, 0, 0.6f);
    private static readonly Color buffBlueColor = new Color(0.5f, 0.8f, 1f);

    // Performance
    private int guiFrameSkip = 0;
    private int fireFrameSkip = 0;

    // Scroll position for cooking menu
    private Vector2 scrollPosition = Vector2.zero;

    void Awake()
    {
        // Strict singleton - destroy duplicates immediately
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("CookingFire: Duplicate detected, destroying.");
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
        bgTex.SetPixel(0, 0, new Color(0.08f, 0.06f, 0.04f, 0.95f));
        bgTex.Apply();

        btnTex = new Texture2D(1, 1);
        btnTex.SetPixel(0, 0, new Color(0.25f, 0.18f, 0.12f, 1f));
        btnTex.Apply();

        btnHoverTex = new Texture2D(1, 1);
        btnHoverTex.SetPixel(0, 0, new Color(0.4f, 0.28f, 0.18f, 1f));
        btnHoverTex.Apply();

        headerTex = new Texture2D(1, 1);
        headerTex.SetPixel(0, 0, new Color(0.15f, 0.1f, 0.06f, 1f));
        headerTex.Apply();
    }

    void CreateVisuals()
    {
        // Fire pit base (stones in a circle)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = $"Stone_{i}";
            stone.transform.SetParent(transform);
            stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.6f, 0.1f, Mathf.Sin(angle) * 0.6f);
            stone.transform.localScale = new Vector3(0.25f, 0.2f, 0.25f);
            Destroy(stone.GetComponent<Collider>());
            stone.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.28f, 0.25f));
        }

        // Logs under the fire
        for (int i = 0; i < 3; i++)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = $"Log_{i}";
            log.transform.SetParent(transform);
            log.transform.localPosition = new Vector3(0, 0.12f, 0);
            log.transform.localRotation = Quaternion.Euler(90f, i * 60f, 0);
            log.transform.localScale = new Vector3(0.12f, 0.4f, 0.12f);
            Destroy(log.GetComponent<Collider>());
            log.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.2f, 0.1f));
        }

        // Cooking fire (flames)
        cookingFire = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cookingFire.name = "Fire";
        cookingFire.transform.SetParent(transform);
        cookingFire.transform.localPosition = new Vector3(0, 0.4f, 0);
        cookingFire.transform.localScale = fireScaleBase;
        Destroy(cookingFire.GetComponent<Collider>());
        Material fireMat = MakeMat(new Color(1f, 0.5f, 0.1f));
        fireMat.EnableKeyword("_EMISSION");
        fireMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 2.5f);
        cookingFire.GetComponent<Renderer>().material = fireMat;

        // Inner flame (brighter)
        GameObject innerFlame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerFlame.name = "InnerFlame";
        innerFlame.transform.SetParent(cookingFire.transform);
        innerFlame.transform.localPosition = new Vector3(0, 0.1f, 0);
        innerFlame.transform.localScale = new Vector3(0.6f, 0.7f, 0.6f);
        Destroy(innerFlame.GetComponent<Collider>());
        Material innerMat = MakeMat(new Color(1f, 0.8f, 0.2f));
        innerMat.EnableKeyword("_EMISSION");
        innerMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.3f) * 3f);
        innerFlame.GetComponent<Renderer>().material = innerMat;

        // Fire light - optimized for performance
        GameObject lightObj = new GameObject("FireLight");
        lightObj.transform.SetParent(cookingFire.transform);
        lightObj.transform.localPosition = Vector3.up * 0.5f;
        fireLight = lightObj.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.6f, 0.2f);
        fireLight.intensity = 1.5f;
        fireLight.range = 5f;
        fireLight.renderMode = LightRenderMode.ForceVertex;
        fireLight.shadows = LightShadows.None;

        // Cooking pot on tripod
        // Tripod legs
        for (int i = 0; i < 3; i++)
        {
            float angle = i * Mathf.PI * 2f / 3f + Mathf.PI / 6f;
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"TripodLeg_{i}";
            leg.transform.SetParent(transform);
            leg.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.3f, 0.5f, Mathf.Sin(angle) * 0.3f);
            leg.transform.localRotation = Quaternion.Euler(15f * Mathf.Cos(angle), 0, 15f * Mathf.Sin(angle));
            leg.transform.localScale = new Vector3(0.04f, 0.5f, 0.04f);
            Destroy(leg.GetComponent<Collider>());
            leg.GetComponent<Renderer>().material = MakeMat(new Color(0.15f, 0.12f, 0.1f));
        }

        // Cooking pot
        cookingPot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cookingPot.name = "Pot";
        cookingPot.transform.SetParent(transform);
        cookingPot.transform.localPosition = new Vector3(0, 0.75f, 0);
        cookingPot.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
        Destroy(cookingPot.GetComponent<Collider>());
        Material potMat = MakeMat(new Color(0.15f, 0.15f, 0.18f));
        potMat.SetFloat("_Metallic", 0.8f);
        potMat.SetFloat("_Smoothness", 0.4f);
        cookingPot.GetComponent<Renderer>().material = potMat;

        // Pot rim
        GameObject potRim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        potRim.name = "PotRim";
        potRim.transform.SetParent(cookingPot.transform);
        potRim.transform.localPosition = new Vector3(0, 0.45f, 0);
        potRim.transform.localScale = new Vector3(1.1f, 0.1f, 1.1f);
        Destroy(potRim.GetComponent<Collider>());
        potRim.GetComponent<Renderer>().material = potMat;
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

        // Distance check - use squared distance for performance
        float distSq = (transform.position - playerTransform.position).sqrMagnitude;
        playerNearby = distSq <= (interactionRange * interactionRange);

        // Input
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !showingCookingMenu)
            OpenCookingMenu();

        if (showingCookingMenu && Input.GetKeyDown(KeyCode.Escape))
            showingCookingMenu = false;

        // Only run visual effects when player is within 30 units (900 sqr)
        if (distSq < 900f)
        {
            // Fire flicker - only update every 3rd frame for performance
            fireFrameSkip++;
            if (fireFrameSkip % 3 == 0)
            {
                flickerTime += Time.deltaTime * 3f;
                float flicker = 1f + Mathf.Sin(flickerTime * 10f) * 0.12f + Mathf.Sin(flickerTime * 15f) * 0.05f;
                fireScaleTemp.x = fireScaleBase.x * flicker;
                fireScaleTemp.y = fireScaleBase.y * (flicker + Mathf.Sin(flickerTime * 8f) * 0.1f);
                fireScaleTemp.z = fireScaleBase.z * flicker;
                cookingFire.transform.localScale = fireScaleTemp;

                // Flicker light intensity
                if (fireLight != null)
                {
                    fireLight.intensity = 1.3f + Mathf.Sin(flickerTime * 12f) * 0.3f;
                }
            }
        }
    }

    void OnGUI()
    {
        // Skip entirely if not needed
        if (!MainMenu.GameStarted) return;
        if (!playerNearby && !showingCookingMenu) return;

        // Frame skipping when just showing prompt (not menu)
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
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 120, 200, 30), "Cooking Fire", labelStyle);

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
        GUI.Label(new Rect(x, y + 10, w, 30), "Cooking Fire", labelStyle);

        // Close button
        if (GUI.Button(new Rect(x + w - 35, y + 10, 25, 25), "X"))
            showingCookingMenu = false;

        // Subtitle
        labelStyle.fontSize = 12;
        labelStyle.fontStyle = FontStyle.Italic;
        labelStyle.normal.textColor = dimColor;
        GUI.Label(new Rect(x, y + 55, w, 20), "Cook special fish into powerful buffs", labelStyle);

        // Fish list area
        float listY = y + 85;
        float listHeight = h - 130;

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
                // No fish to cook
                labelStyle.fontSize = 14;
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.normal.textColor = grayColor;
                GUI.Label(new Rect(x, listY + 80, w, 60), "No special fish to cook.\n\nCatch special fish while fishing\nand bring them here!", labelStyle);
            }
            else
            {
                // Draw cookable fish list
                labelStyle.fontSize = 13;
                labelStyle.alignment = TextAnchor.MiddleLeft;

                float itemHeight = 70;
                float itemY = listY;

                foreach (var buff in FishBuffSystem.Instance.allBuffs)
                {
                    bool hasFish = FishBuffSystem.Instance.HasRequiredFish(buff.requiredFishId);
                    if (!hasFish) continue;

                    // Item background
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

        // Consume the fish
        FishBuffSystem.Instance.ConsumeFish(buff.requiredFishId);

        // Complete the "quest" (gives buff + XP on first time)
        FishBuffSystem.Instance.CompleteQuest(buff.requiredFishId);

        // Play cooking sound
        if (IslandSoundManager.Instance != null)
        {
            IslandSoundManager.Instance.PlayChime();
        }

        // Show notification
        if (UIManager.Instance != null)
        {
            if (isFirstTime)
                UIManager.Instance.ShowLootNotification($"Cooked: {buff.buffName} + 2000 XP!", buff.bowlColor);
            else
                UIManager.Instance.ShowLootNotification($"Cooked: {buff.buffName}!", buff.bowlColor);
        }

        Debug.Log($"Cooked {buff.requiredFishName} into {buff.buffName}!");
    }

    bool DrawBtn(Rect r, string text)
    {
        bool hover = r.Contains(Event.current.mousePosition);
        GUI.DrawTexture(r, hover ? btnHoverTex : btnTex);

        // Border on hover
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
        scrollPosition = Vector2.zero;

        // Play sound when opening menu
        if (IslandSoundManager.Instance != null)
        {
            IslandSoundManager.Instance.PlayChime();
        }
    }

    // Legacy compatibility methods
    public static bool IsPlayerNearChef()
    {
        return IsPlayerNearFire();
    }

    public static bool HasCompletedFirstQuest()
    {
        // Cooking is always available now (no quest required)
        return true;
    }

    public static bool IsPlayerNearFire()
    {
        if (Instance == null || !GameCache.IsPlayerValid()) return false;
        return Vector3.Distance(Instance.transform.position, GameCache.Player.position) <= 4f;
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
