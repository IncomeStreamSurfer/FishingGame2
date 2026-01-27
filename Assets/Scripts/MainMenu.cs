using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Main Menu - Landing page for the game
/// Shows Start New Game, Settings, Quit
/// </summary>
public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }
    public static bool GameStarted { get; set; } = false;

    private enum MenuState { Main, Settings }
    private MenuState currentState = MenuState.Main;

    // Settings
    private float musicVolume = 0.7f;
    private float sfxVolume = 1.0f;
    private bool fullscreen = true;
    private int qualityLevel = 2;
    private string[] qualityNames = { "Low", "Medium", "High", "Ultra" };

    // Animation
    private float titleBob = 0f;
    private float menuAlpha = 0f;
    private float fadeInTime = 0f;

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;

    // Water animation for background
    private float waterTime = 0f;

    // Title screen effects
    private float lightningTimer = 0f;
    private float lightningFlash = 0f;
    private float skullPulse = 0f;
    private float[] fishPositions = new float[5];
    private float bloodDrip = 0f;

    // Cached GUIStyles for performance
    private static GUIStyle cachedVersionStyle;
    private static bool stylesInitialized = false;

    // 16:9 safe area calculations
    private Rect safeArea;
    private float safeMarginX;
    private float safeMarginY;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Check if we're returning from a scene reload for new game
        if (PlayerPrefs.GetInt("PendingNewGame", 0) == 1)
        {
            PlayerPrefs.DeleteKey("PendingNewGame");
            PlayerPrefs.Save();

            // Clear all runtime state for fresh start
            Invoke("ClearAllRuntimeState", 0.1f);

            GameStarted = true;
            EnableGameSystems();
            Debug.Log("New game started after scene reload - all entities cleared!");
            return;
        }

        GameStarted = false;
        LoadSettings();
        DisableGameSystems();
        Invoke("Initialize", 0.2f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        initialized = true;
    }

    void CreateCachedTextures()
    {
        CacheTexture("overlay", new Color(0f, 0f, 0f, 0.75f));
        CacheTexture("panelBg", new Color(0.05f, 0.08f, 0.12f, 0.95f));
        CacheTexture("panelBorder", new Color(0.6f, 0.2f, 0.2f, 1f));
        CacheTexture("buttonNormal", new Color(0.2f, 0.08f, 0.08f, 0.95f));
        CacheTexture("buttonHover", new Color(0.4f, 0.15f, 0.15f, 1f));
        CacheTexture("buttonPressed", new Color(0.15f, 0.05f, 0.05f, 1f));
        CacheTexture("waterDark", new Color(0.02f, 0.05f, 0.12f, 1f));
        CacheTexture("waterLight", new Color(0.05f, 0.1f, 0.2f, 1f));
        CacheTexture("waterBlood", new Color(0.15f, 0.02f, 0.02f, 0.5f));
        CacheTexture("sliderBg", new Color(0.1f, 0.05f, 0.05f, 1f));
        CacheTexture("sliderFill", new Color(0.7f, 0.2f, 0.2f, 1f));
        CacheTexture("white", Color.white);
        CacheTexture("red", new Color(0.8f, 0.1f, 0.1f, 1f));

        for (int i = 0; i < fishPositions.Length; i++)
        {
            fishPositions[i] = Random.Range(0f, Screen.width);
        }
    }

    void CacheTexture(string name, Color color)
    {
        if (!textureCache.ContainsKey(name))
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            textureCache[name] = tex;
        }
    }

    Texture2D GetTexture(string name)
    {
        return textureCache.TryGetValue(name, out Texture2D tex) ? tex : Texture2D.whiteTexture;
    }

    void DisableGameSystems()
    {
        if (GameCache.IsPlayerValid() && GameCache.PlayerObject != null)
        {
            var controller = GameCache.PlayerObject.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;
            var rodAnim = GameCache.PlayerObject.GetComponent<FishingRodAnimator>();
            if (rodAnim != null) rodAnim.enabled = false;
        }
    }

    void EnableGameSystems()
    {
        if (GameCache.IsPlayerValid() && GameCache.PlayerObject != null)
        {
            var controller = GameCache.PlayerObject.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = true;
            var rodAnim = GameCache.PlayerObject.GetComponent<FishingRodAnimator>();
            if (rodAnim != null) rodAnim.enabled = true;
        }
    }

    /// <summary>
    /// Called when returning to menu from gameplay (e.g., quit to menu)
    /// </summary>
    public void ReturnToMenu()
    {
        GameStarted = false;
        currentState = MenuState.Main;
        fadeInTime = 0f; // Reset fade so menu fades in fresh
        menuAlpha = 0f;
        DisableGameSystems();
        Debug.Log("Returned to main menu");
    }

    void ClearAllRuntimeState()
    {
        // Clear food inventory (hotbar and raw fish)
        if (FoodInventory.Instance != null)
        {
            FoodInventory.Instance.ClearInventory();
            FoodInventory.Instance.lunchBoxCount = 0;
            FoodInventory.Instance.lunchBoxFishCount = 0;
        }

        // Clear GameManager state (persists via DontDestroyOnLoad)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = 0;
            GameManager.Instance.totalFishCaught = 0;
            GameManager.Instance.fishInventory.Clear();
        }

        // Reset player health
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.ResetHealth();
        }

        // Reset leveling to level 1
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.ResetToLevel1();
        }

        // Clear active buffs
        if (FishBuffSystem.Instance != null)
        {
            FishBuffSystem.Instance.ClearAllActiveBuffs();
        }

        // Reset wet debuff
        if (WetDebuffSystem.Instance != null)
        {
            WetDebuffSystem.Instance.ClearWetDebuff();
        }

        // Reset player position
        if (GameCache.IsPlayerValid())
        {
            GameCache.Player.position = new Vector3(0, 2, 0);
        }

        Debug.Log("All runtime state cleared for new game!");
    }

    void Update()
    {
        if (GameStarted) return;

        titleBob += Time.deltaTime;
        waterTime += Time.deltaTime;
        skullPulse += Time.deltaTime;
        bloodDrip += Time.deltaTime * 0.5f;

        lightningTimer -= Time.deltaTime;
        if (lightningTimer <= 0f)
        {
            lightningTimer = Random.Range(3f, 8f);
            lightningFlash = 1f;
        }
        lightningFlash = Mathf.Max(0f, lightningFlash - Time.deltaTime * 4f);

        for (int i = 0; i < fishPositions.Length; i++)
        {
            fishPositions[i] -= Time.deltaTime * (30f + i * 15f);
            if (fishPositions[i] < -100f)
            {
                fishPositions[i] = Screen.width + Random.Range(50f, 200f);
            }
        }

        fadeInTime += Time.deltaTime;
        menuAlpha = Mathf.Clamp01(fadeInTime / 1.5f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState != MenuState.Main)
            {
                currentState = MenuState.Main;
            }
        }
    }

    void CalculateSafeArea()
    {
        float targetAspect = 16f / 9f;
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > targetAspect)
        {
            float safeWidth = Screen.height * targetAspect;
            safeMarginX = (Screen.width - safeWidth) / 2f;
            safeMarginY = 0;
            safeArea = new Rect(safeMarginX, 0, safeWidth, Screen.height);
        }
        else
        {
            float safeHeight = Screen.width / targetAspect;
            safeMarginX = 0;
            safeMarginY = (Screen.height - safeHeight) / 2f;
            safeArea = new Rect(0, safeMarginY, Screen.width, safeHeight);
        }
    }

    void OnGUI()
    {
        if (GameStarted || !initialized) return;

        CalculateSafeArea();

        if (!stylesInitialized)
        {
            cachedVersionStyle = new GUIStyle(GUI.skin.label);
            cachedVersionStyle.fontSize = 12;
            cachedVersionStyle.alignment = TextAnchor.LowerRight;
            stylesInitialized = true;
        }

        DrawWaterBackground();

        GUI.color = new Color(1, 1, 1, menuAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("overlay"));

        DrawTitle();

        switch (currentState)
        {
            case MenuState.Main:
                DrawMainMenu();
                break;
            case MenuState.Settings:
                DrawSettingsMenu();
                break;
        }

        cachedVersionStyle.normal.textColor = new Color(0.4f, 0.35f, 0.35f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + safeArea.width - 260, safeArea.y + safeArea.height - 30, 250, 25), "OPEN TESTING v0.2", cachedVersionStyle);

        GUI.color = Color.white;
    }

    void DrawWaterBackground()
    {
        GUI.color = new Color(0.02f, 0.04f, 0.08f, 1f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("white"));

        int numWaves = 25;
        float waveHeight = Screen.height / (float)numWaves;

        for (int i = 0; i < numWaves; i++)
        {
            float waveOffset = Mathf.Sin(waterTime * 0.8f + i * 0.4f) * 30f;
            float waveOffset2 = Mathf.Cos(waterTime * 0.6f + i * 0.25f) * 15f;
            float alpha = 0.4f + Mathf.Sin(waterTime * 0.4f + i * 0.15f) * 0.2f;

            Texture2D tex = (i % 3 == 0) ? GetTexture("waterBlood") :
                           (i % 2 == 0) ? GetTexture("waterDark") : GetTexture("waterLight");
            GUI.color = new Color(1, 1, 1, alpha);
            GUI.DrawTexture(new Rect(waveOffset + waveOffset2, i * waveHeight, Screen.width + 60, waveHeight + 3), tex);
        }

        GUI.color = new Color(0.03f, 0.06f, 0.1f, 0.6f);
        for (int i = 0; i < fishPositions.Length; i++)
        {
            float y = Screen.height * 0.5f + Mathf.Sin(waterTime + i * 2f) * 100f + i * 60f;
            float size = 40f + i * 10f;
            GUI.DrawTexture(new Rect(fishPositions[i], y, size * 2f, size), GetTexture("white"));
        }

        GUI.color = new Color(0.5f, 0.05f, 0.05f, 0.3f);
        for (int i = 0; i < 8; i++)
        {
            float x = Screen.width * (i + 0.5f) / 8f + Mathf.Sin(i * 1.5f) * 30f;
            float dripY = (bloodDrip * 200f + i * 50f) % (Screen.height + 100f) - 50f;
            float dripHeight = 80f + Mathf.Sin(i * 2f) * 40f;
            GUI.DrawTexture(new Rect(x, dripY, 4f, dripHeight), GetTexture("red"));
        }

        if (lightningFlash > 0f)
        {
            GUI.color = new Color(1f, 1f, 1f, lightningFlash * 0.3f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("white"));
        }

        GUI.color = Color.white;
    }

    void DrawTitle()
    {
        float bobOffset = Mathf.Sin(titleBob * 1.5f) * 8f;
        float shakeX = Mathf.Sin(titleBob * 12f) * lightningFlash * 5f;
        float pulseScale = 1f + Mathf.Sin(skullPulse * 2f) * 0.02f;

        float centerX = safeArea.x + safeArea.width / 2;
        float titleY = safeArea.y + safeArea.height * 0.12f;

        GUI.color = new Color(0.8f, 0.1f, 0.05f, 0.25f * menuAlpha);
        GUI.DrawTexture(new Rect(centerX - 350, titleY + 30 + bobOffset, 700, 180), GetTexture("white"));

        GUI.color = new Color(1f, 0.3f, 0.1f, 0.15f * menuAlpha);
        GUI.DrawTexture(new Rect(centerX - 300, titleY + 50 + bobOffset, 600, 140), GetTexture("white"));

        GUI.color = new Color(0.3f, 0.25f, 0.2f, 0.8f * menuAlpha);
        DrawRotatedRect(new Rect(centerX - 180, titleY + 40 + bobOffset, 360, 8), 15f);
        DrawRotatedRect(new Rect(centerX - 180, titleY + 40 + bobOffset, 360, 8), -15f);

        GUIStyle fishStyle = new GUIStyle();
        fishStyle.fontSize = 90;
        fishStyle.fontStyle = FontStyle.Bold;
        fishStyle.alignment = TextAnchor.MiddleCenter;

        GUI.color = new Color(0f, 0f, 0f, 0.9f * menuAlpha);
        fishStyle.normal.textColor = new Color(0f, 0f, 0f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX + 3, titleY + 43 + bobOffset, safeArea.width, 100), "FISH", fishStyle);
        GUI.Label(new Rect(safeArea.x + shakeX - 3, titleY + 43 + bobOffset, safeArea.width, 100), "FISH", fishStyle);

        GUI.color = new Color(1, 1, 1, menuAlpha);
        fishStyle.normal.textColor = new Color(1.0f, 0.2f, 0.15f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 40 + bobOffset, safeArea.width, 100), "FISH", fishStyle);

        GUIStyle orStyle = new GUIStyle();
        orStyle.fontSize = 36;
        orStyle.fontStyle = FontStyle.BoldAndItalic;
        orStyle.alignment = TextAnchor.MiddleCenter;

        orStyle.normal.textColor = new Color(0f, 0f, 0f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX + 2, titleY + 117 + bobOffset, safeArea.width, 50), "OR", orStyle);

        orStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 115 + bobOffset, safeArea.width, 50), "OR", orStyle);

        GUIStyle dieStyle = new GUIStyle();
        dieStyle.fontSize = 100;
        dieStyle.fontStyle = FontStyle.Bold;
        dieStyle.alignment = TextAnchor.MiddleCenter;

        GUI.color = new Color(0, 0, 0, 0.9f * menuAlpha);
        dieStyle.normal.textColor = new Color(0, 0, 0, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX + 4, titleY + 139 + bobOffset, safeArea.width, 110), "DIE", dieStyle);
        GUI.Label(new Rect(safeArea.x + shakeX - 4, titleY + 139 + bobOffset, safeArea.width, 110), "DIE", dieStyle);

        float diePulse = 0.85f + Mathf.Sin(skullPulse * 3f) * 0.15f;
        GUI.color = new Color(1, 1, 1, menuAlpha);
        dieStyle.normal.textColor = new Color(0.9f * diePulse, 0.1f, 0.1f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 135 + bobOffset, safeArea.width, 110), "DIE", dieStyle);

        DrawSkull(centerX - 15, titleY + 125 + bobOffset, 30f * pulseScale, menuAlpha);

        GUIStyle tagStyle = new GUIStyle();
        tagStyle.fontSize = 18;
        tagStyle.fontStyle = FontStyle.Italic;
        tagStyle.alignment = TextAnchor.MiddleCenter;
        tagStyle.normal.textColor = new Color(0.8f, 0.85f, 0.9f, menuAlpha);
        GUI.Label(new Rect(safeArea.x, titleY + 235 + bobOffset, safeArea.width, 30), "\"In these waters, only the hungry survive.\"", tagStyle);

        GUIStyle openTestingStyle = new GUIStyle();
        openTestingStyle.fontSize = 14;
        openTestingStyle.fontStyle = FontStyle.Bold;
        openTestingStyle.alignment = TextAnchor.MiddleCenter;
        openTestingStyle.normal.textColor = new Color(0.3f, 1f, 0.5f, menuAlpha);
        GUI.Label(new Rect(safeArea.x, titleY + 265 + bobOffset, safeArea.width, 25), "[ OPEN TESTING - Tropical Island ]", openTestingStyle);
    }

    void DrawRotatedRect(Rect rect, float angle)
    {
        Matrix4x4 matrixBackup = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2));
        GUI.DrawTexture(rect, GetTexture("white"));
        GUI.matrix = matrixBackup;
    }

    void DrawSkull(float x, float y, float size, float alpha)
    {
        GUI.color = new Color(0.9f, 0.85f, 0.75f, alpha * 0.9f);
        GUI.DrawTexture(new Rect(x - size * 0.4f, y - size * 0.5f, size * 0.8f, size * 0.7f), GetTexture("white"));

        GUI.color = new Color(0.1f, 0.05f, 0.05f, alpha);
        GUI.DrawTexture(new Rect(x - size * 0.25f, y - size * 0.2f, size * 0.18f, size * 0.2f), GetTexture("white"));
        GUI.DrawTexture(new Rect(x + size * 0.07f, y - size * 0.2f, size * 0.18f, size * 0.2f), GetTexture("white"));
        GUI.DrawTexture(new Rect(x - size * 0.06f, y + size * 0.05f, size * 0.12f, size * 0.12f), GetTexture("white"));

        GUI.color = new Color(0.85f, 0.8f, 0.7f, alpha * 0.9f);
        GUI.DrawTexture(new Rect(x - size * 0.3f, y + size * 0.15f, size * 0.6f, size * 0.2f), GetTexture("white"));

        GUI.color = new Color(0.1f, 0.05f, 0.05f, alpha * 0.7f);
        for (int i = 0; i < 4; i++)
        {
            GUI.DrawTexture(new Rect(x - size * 0.2f + i * size * 0.12f, y + size * 0.18f, 2, size * 0.12f), GetTexture("white"));
        }

        GUI.color = Color.white;
    }

    void DrawMainMenu()
    {
        float buttonWidth = 220;
        float buttonHeight = 42;
        float buttonSpacing = 16;

        float centerX = safeArea.x + safeArea.width / 2;
        float buttonX = centerX - buttonWidth / 2;
        float startY = safeArea.y + safeArea.height * 0.12f + 310f;

        if (DrawMenuButton(new Rect(buttonX, startY, buttonWidth, buttonHeight), "START GAME"))
        {
            StartNewGame();
        }

        if (DrawMenuButton(new Rect(buttonX, startY + (buttonHeight + buttonSpacing), buttonWidth, buttonHeight), "SETTINGS"))
        {
            currentState = MenuState.Settings;
        }

        if (DrawMenuButton(new Rect(buttonX, startY + (buttonHeight + buttonSpacing) * 2, buttonWidth, buttonHeight), "QUIT"))
        {
            QuitGame();
        }
    }

    void DrawSettingsMenu()
    {
        float panelWidth = 500;
        float panelHeight = 400;
        float panelX = safeArea.x + (safeArea.width - panelWidth) / 2;
        float panelY = safeArea.y + (safeArea.height - panelHeight) / 2;

        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 28;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        GUI.Label(new Rect(panelX, panelY + 15, panelWidth, 40), "SETTINGS", headerStyle);

        if (DrawCloseButton(new Rect(panelX + panelWidth - 40, panelY + 10, 30, 30)))
        {
            currentState = MenuState.Main;
        }

        float contentY = panelY + 70;
        float labelWidth = 150;
        float sliderWidth = 280;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 16;
        labelStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(panelX + 20, contentY, labelWidth, 25), "Music Volume", labelStyle);
        musicVolume = DrawSlider(new Rect(panelX + labelWidth + 30, contentY, sliderWidth, 20), musicVolume);
        contentY += 50;

        GUI.Label(new Rect(panelX + 20, contentY, labelWidth, 25), "SFX Volume", labelStyle);
        sfxVolume = DrawSlider(new Rect(panelX + labelWidth + 30, contentY, sliderWidth, 20), sfxVolume);
        contentY += 50;

        GUI.Label(new Rect(panelX + 20, contentY, labelWidth, 25), "Quality", labelStyle);
        if (GUI.Button(new Rect(panelX + labelWidth + 30, contentY, 100, 28), "< " + qualityNames[qualityLevel] + " >"))
        {
            qualityLevel = (qualityLevel + 1) % qualityNames.Length;
            QualitySettings.SetQualityLevel(qualityLevel);
        }
        contentY += 50;

        GUI.Label(new Rect(panelX + 20, contentY, labelWidth, 25), "Fullscreen", labelStyle);
        if (GUI.Button(new Rect(panelX + labelWidth + 30, contentY, 100, 28), fullscreen ? "ON" : "OFF"))
        {
            fullscreen = !fullscreen;
            Screen.fullScreen = fullscreen;
        }
        contentY += 70;

        if (DrawMenuButton(new Rect(panelX + panelWidth / 2 - 100, contentY, 200, 45), "SAVE SETTINGS"))
        {
            SaveSettings();
            currentState = MenuState.Main;
        }
    }

    bool DrawMenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        bool pressed = hover && Input.GetMouseButton(0);

        Texture2D tex = pressed ? GetTexture("buttonPressed") :
                        hover ? GetTexture("buttonHover") : GetTexture("buttonNormal");

        GUI.DrawTexture(rect, tex);

        if (hover)
        {
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), GetTexture("panelBorder"));
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2), GetTexture("panelBorder"));
        }

        GUIStyle btnStyle = new GUIStyle(GUI.skin.label);
        btnStyle.fontSize = 13;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = hover ? Color.white : new Color(0.8f, 0.9f, 1f);

        GUI.Label(rect, text, btnStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    bool DrawCloseButton(Rect rect)
    {
        bool hover = rect.Contains(Event.current.mousePosition);

        GUI.color = hover ? new Color(1f, 0.3f, 0.3f) : new Color(0.8f, 0.4f, 0.4f);
        GUI.DrawTexture(rect, GetTexture("white"));
        GUI.color = Color.white;

        GUIStyle xStyle = new GUIStyle(GUI.skin.label);
        xStyle.fontSize = 18;
        xStyle.fontStyle = FontStyle.Bold;
        xStyle.alignment = TextAnchor.MiddleCenter;
        xStyle.normal.textColor = Color.white;
        GUI.Label(rect, "X", xStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    float DrawSlider(Rect rect, float value)
    {
        GUI.DrawTexture(rect, GetTexture("sliderBg"));
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * value, rect.height), GetTexture("sliderFill"));

        if (rect.Contains(Event.current.mousePosition) && Input.GetMouseButton(0))
        {
            value = (Event.current.mousePosition.x - rect.x) / rect.width;
            value = Mathf.Clamp01(value);
        }

        GUIStyle pctStyle = new GUIStyle(GUI.skin.label);
        pctStyle.fontSize = 12;
        pctStyle.alignment = TextAnchor.MiddleRight;
        pctStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(rect.x + rect.width + 10, rect.y, 40, rect.height), Mathf.RoundToInt(value * 100) + "%", pctStyle);

        return value;
    }

    void StartNewGame()
    {
        // Reset PlayerPrefs for new game (except achievements)
        PlayerPrefs.SetInt("PlayerXP", 0);
        PlayerPrefs.SetInt("PlayerLevel", 1);
        PlayerPrefs.DeleteKey("BuffInv_SnappersDelight");
        PlayerPrefs.DeleteKey("BuffInv_MarlinsLuck");
        PlayerPrefs.DeleteKey("BuffInv_TroutsFortune");
        PlayerPrefs.DeleteKey("BuffInv_SunshoreSurge");
        PlayerPrefs.DeleteKey("BuffInv_SnubnoseSpeed");
        PlayerPrefs.DeleteKey("BuffInv_SeahorsesBounty");
        PlayerPrefs.DeleteKey("Quest_red_snapper");
        PlayerPrefs.DeleteKey("Quest_blue_marlin");
        PlayerPrefs.DeleteKey("Quest_rainbow_trout");
        PlayerPrefs.DeleteKey("Quest_sunshore_od");
        PlayerPrefs.DeleteKey("Quest_icelandic_snubnose");
        PlayerPrefs.DeleteKey("Quest_seahorse");
        PlayerPrefs.SetInt("ConnoisseurCurrentQuest", -1);
        for (int i = 0; i < 4; i++)
        {
            PlayerPrefs.SetInt($"ConnoisseurQuest_{i}", 0);
        }
        PlayerPrefs.SetInt("CurrentDay", 1);
        PlayerPrefs.DeleteKey("CookableFishDiscovered_red_snapper");
        PlayerPrefs.DeleteKey("CookableFishDiscovered_blue_marlin");
        PlayerPrefs.DeleteKey("CookableFishDiscovered_rainbow_trout");
        PlayerPrefs.DeleteKey("CookableFishDiscovered_sunshore_od");
        PlayerPrefs.DeleteKey("CookableFishDiscovered_icelandic_snubnose");
        PlayerPrefs.DeleteKey("CookableFishDiscovered_seahorse");
        PlayerPrefs.DeleteKey("TotalGoldEarned");
        PlayerPrefs.DeleteKey("FishDiary_golden_starfish");
        PlayerPrefs.DeleteKey("Death_Total");
        PlayerPrefs.DeleteKey("Death_Lightning");
        PlayerPrefs.DeleteKey("Death_Starvation");
        PlayerPrefs.DeleteKey("Death_Storm");

        PlayerPrefs.SetInt("PendingNewGame", 1);
        PlayerPrefs.Save();

        Debug.Log("Starting fresh new game - reloading scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("Quality", qualityLevel);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        qualityLevel = PlayerPrefs.GetInt("Quality", 2);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}
