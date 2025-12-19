using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Main Menu - Landing page for the game
/// Shows Start New Game, Load Game, Saved Games, Settings
/// </summary>
public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }
    public static bool GameStarted { get; set; } = false;

    private enum MenuState { Main, Settings, SavedGames, LoadGame }
    private MenuState currentState = MenuState.Main;

    // Settings
    private float musicVolume = 0.7f;
    private float sfxVolume = 1.0f;
    private bool fullscreen = true;
    private int qualityLevel = 2;
    private string[] qualityNames = { "Low", "Medium", "High", "Ultra" };

    // Saved games list
    private List<SavedGameInfo> savedGames = new List<SavedGameInfo>();

    // Animation
    private float titleBob = 0f;
    private float menuAlpha = 0f;
    private float fadeInTime = 0f;

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;
    private int guiFrameSkip = 0;

    // Water animation for background
    private float waterTime = 0f;

    // Title screen music - REMOVED (was causing audio issues)

    // Title screen effects
    private float lightningTimer = 0f;
    private float lightningFlash = 0f;
    private float skullPulse = 0f;
    private float[] fishPositions = new float[5];
    private float bloodDrip = 0f;

    // Cached GUIStyles for performance
    private static GUIStyle cachedVersionStyle;
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedSubStyle;
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
        GameStarted = false;
        LoadSettings();
        RefreshSavedGames();

        // Disable all game systems until game starts
        DisableGameSystems();

        // Title music removed - was causing audio issues

        // Delay texture creation
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
        CacheTexture("titleGlow", new Color(0.8f, 0.2f, 0.1f, 0.4f));
        CacheTexture("waterDark", new Color(0.02f, 0.05f, 0.12f, 1f));
        CacheTexture("waterLight", new Color(0.05f, 0.1f, 0.2f, 1f));
        CacheTexture("waterBlood", new Color(0.15f, 0.02f, 0.02f, 0.5f));
        CacheTexture("sliderBg", new Color(0.1f, 0.05f, 0.05f, 1f));
        CacheTexture("sliderFill", new Color(0.7f, 0.2f, 0.2f, 1f));
        CacheTexture("saveSlotBg", new Color(0.1f, 0.08f, 0.08f, 0.95f));
        CacheTexture("white", Color.white);
        CacheTexture("red", new Color(0.8f, 0.1f, 0.1f, 1f));
        CacheTexture("darkRed", new Color(0.4f, 0.05f, 0.05f, 1f));
        CacheTexture("lightning", new Color(1f, 1f, 1f, 0.9f));
        CacheTexture("skull", new Color(0.9f, 0.85f, 0.75f, 1f));

        // Initialize fish positions for swimming animation
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
        // Disable player controls and other systems
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

    void Update()
    {
        if (GameStarted) return;

        titleBob += Time.deltaTime;
        waterTime += Time.deltaTime;
        skullPulse += Time.deltaTime;
        bloodDrip += Time.deltaTime * 0.5f;

        // Lightning effect - random flashes
        lightningTimer -= Time.deltaTime;
        if (lightningTimer <= 0f)
        {
            lightningTimer = Random.Range(3f, 8f);
            lightningFlash = 1f;
        }
        lightningFlash = Mathf.Max(0f, lightningFlash - Time.deltaTime * 4f);

        // Animate fish swimming across screen
        for (int i = 0; i < fishPositions.Length; i++)
        {
            fishPositions[i] -= Time.deltaTime * (30f + i * 15f);
            if (fishPositions[i] < -100f)
            {
                fishPositions[i] = Screen.width + Random.Range(50f, 200f);
            }
        }

        // Fade in
        fadeInTime += Time.deltaTime;
        menuAlpha = Mathf.Clamp01(fadeInTime / 1.5f);

        // ESC to go back
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
        // Calculate 16:9 safe area within current screen
        float targetAspect = 16f / 9f;
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > targetAspect)
        {
            // Screen is wider than 16:9 - add horizontal margins
            float safeWidth = Screen.height * targetAspect;
            safeMarginX = (Screen.width - safeWidth) / 2f;
            safeMarginY = 0;
            safeArea = new Rect(safeMarginX, 0, safeWidth, Screen.height);
        }
        else
        {
            // Screen is taller than 16:9 - add vertical margins
            float safeHeight = Screen.width / targetAspect;
            safeMarginX = 0;
            safeMarginY = (Screen.height - safeHeight) / 2f;
            safeArea = new Rect(0, safeMarginY, Screen.width, safeHeight);
        }
    }

    void OnGUI()
    {
        // Performance: Skip frames when not actively needed (menu is less critical)
        if (!GameStarted)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 2 != 0) return; // Skip every other frame for smoother menu
        }

        if (GameStarted || !initialized) return;

        // Calculate safe area for 16:9
        CalculateSafeArea();

        // Initialize styles lazily (must be done in OnGUI context) - BEFORE any drawing
        if (!stylesInitialized)
        {
            cachedVersionStyle = new GUIStyle(GUI.skin.label);
            cachedVersionStyle.fontSize = 12;
            cachedVersionStyle.alignment = TextAnchor.LowerRight;

            cachedTitleStyle = new GUIStyle(GUI.skin.label);
            cachedTitleStyle.fontSize = 72;
            cachedTitleStyle.fontStyle = FontStyle.Bold;
            cachedTitleStyle.alignment = TextAnchor.MiddleCenter;

            cachedSubStyle = new GUIStyle(GUI.skin.label);
            cachedSubStyle.fontSize = 22;
            cachedSubStyle.fontStyle = FontStyle.Italic;
            cachedSubStyle.alignment = TextAnchor.MiddleCenter;

            stylesInitialized = true;
        }

        // Draw animated water background
        DrawWaterBackground();

        // Dark overlay
        GUI.color = new Color(1, 1, 1, menuAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("overlay"));

        // Title with glow effect
        DrawTitle();

        // Draw current menu state
        switch (currentState)
        {
            case MenuState.Main:
                DrawMainMenu();
                break;
            case MenuState.Settings:
                DrawSettingsMenu();
                break;
            case MenuState.SavedGames:
                DrawSavedGamesMenu();
                break;
            case MenuState.LoadGame:
                DrawLoadGameMenu();
                break;
        }

        // Version and credits - update color dynamically, within safe area
        cachedVersionStyle.normal.textColor = new Color(0.4f, 0.35f, 0.35f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + safeArea.width - 210, safeArea.y + safeArea.height - 30, 200, 25), "BETA v0.1", cachedVersionStyle);

        GUI.color = Color.white;
    }

    void DrawWaterBackground()
    {
        // Dark stormy ocean background
        GUI.color = new Color(0.02f, 0.04f, 0.08f, 1f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("white"));

        // Animated wave pattern - more dramatic
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

        // Swimming fish silhouettes in the background
        GUI.color = new Color(0.03f, 0.06f, 0.1f, 0.6f);
        for (int i = 0; i < fishPositions.Length; i++)
        {
            float y = Screen.height * 0.5f + Mathf.Sin(waterTime + i * 2f) * 100f + i * 60f;
            float size = 40f + i * 10f;
            // Simple fish shape (elongated oval)
            GUI.DrawTexture(new Rect(fishPositions[i], y, size * 2f, size), GetTexture("white"));
        }

        // Blood drips from top
        GUI.color = new Color(0.5f, 0.05f, 0.05f, 0.3f);
        for (int i = 0; i < 8; i++)
        {
            float x = Screen.width * (i + 0.5f) / 8f + Mathf.Sin(i * 1.5f) * 30f;
            float dripY = (bloodDrip * 200f + i * 50f) % (Screen.height + 100f) - 50f;
            float dripHeight = 80f + Mathf.Sin(i * 2f) * 40f;
            GUI.DrawTexture(new Rect(x, dripY, 4f, dripHeight), GetTexture("red"));
        }

        // Lightning flash overlay
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

        // Use safe area for positioning - moved title lower for better visibility
        float centerX = safeArea.x + safeArea.width / 2;
        float titleY = safeArea.y + safeArea.height * 0.12f; // Start 12% down the safe area

        // Large ominous glow behind title
        GUI.color = new Color(0.8f, 0.1f, 0.05f, 0.25f * menuAlpha);
        GUI.DrawTexture(new Rect(centerX - 350, titleY + 30 + bobOffset, 700, 180), GetTexture("white"));

        // Secondary glow
        GUI.color = new Color(1f, 0.3f, 0.1f, 0.15f * menuAlpha);
        GUI.DrawTexture(new Rect(centerX - 300, titleY + 50 + bobOffset, 600, 140), GetTexture("white"));

        // Draw crossed fishing rods behind title (X shape)
        GUI.color = new Color(0.3f, 0.25f, 0.2f, 0.8f * menuAlpha);
        DrawRotatedRect(new Rect(centerX - 180, titleY + 40 + bobOffset, 360, 8), 15f);
        DrawRotatedRect(new Rect(centerX - 180, titleY + 40 + bobOffset, 360, 8), -15f);

        // "FISH" text
        GUIStyle fishStyle = new GUIStyle();
        fishStyle.fontSize = 90;
        fishStyle.fontStyle = FontStyle.Bold;
        fishStyle.alignment = TextAnchor.MiddleCenter;

        // Black outline shadow for better visibility
        GUI.color = new Color(0f, 0f, 0f, 0.9f * menuAlpha);
        fishStyle.normal.textColor = new Color(0f, 0f, 0f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX + 3, titleY + 43 + bobOffset, safeArea.width, 100), "FISH", fishStyle);
        GUI.Label(new Rect(safeArea.x + shakeX - 3, titleY + 43 + bobOffset, safeArea.width, 100), "FISH", fishStyle);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 46 + bobOffset, safeArea.width, 100), "FISH", fishStyle);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 37 + bobOffset, safeArea.width, 100), "FISH", fishStyle);

        // Main "FISH" text - bright blood red for better visibility
        GUI.color = new Color(1, 1, 1, menuAlpha);
        fishStyle.normal.textColor = new Color(1.0f, 0.2f, 0.15f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 40 + bobOffset, safeArea.width, 100), "FISH", fishStyle);

        // "OR" text - smaller, bright white for visibility
        GUIStyle orStyle = new GUIStyle();
        orStyle.fontSize = 36;
        orStyle.fontStyle = FontStyle.BoldAndItalic;
        orStyle.alignment = TextAnchor.MiddleCenter;

        // Black outline for OR
        orStyle.normal.textColor = new Color(0f, 0f, 0f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX + 2, titleY + 117 + bobOffset, safeArea.width, 50), "OR", orStyle);
        GUI.Label(new Rect(safeArea.x + shakeX - 2, titleY + 117 + bobOffset, safeArea.width, 50), "OR", orStyle);

        orStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 115 + bobOffset, safeArea.width, 50), "OR", orStyle);

        // "DIE" text - even more dramatic
        GUIStyle dieStyle = new GUIStyle();
        dieStyle.fontSize = 100;
        dieStyle.fontStyle = FontStyle.Bold;
        dieStyle.alignment = TextAnchor.MiddleCenter;

        // Heavy black outline for DIE
        GUI.color = new Color(0, 0, 0, 0.9f * menuAlpha);
        dieStyle.normal.textColor = new Color(0, 0, 0, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX + 4, titleY + 139 + bobOffset, safeArea.width, 110), "DIE", dieStyle);
        GUI.Label(new Rect(safeArea.x + shakeX - 4, titleY + 139 + bobOffset, safeArea.width, 110), "DIE", dieStyle);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 143 + bobOffset, safeArea.width, 110), "DIE", dieStyle);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 131 + bobOffset, safeArea.width, 110), "DIE", dieStyle);

        // Main "DIE" text - brighter blood red, pulsing
        float diePulse = 0.85f + Mathf.Sin(skullPulse * 3f) * 0.15f;
        GUI.color = new Color(1, 1, 1, menuAlpha);
        dieStyle.normal.textColor = new Color(0.9f * diePulse, 0.1f, 0.1f, menuAlpha);
        GUI.Label(new Rect(safeArea.x + shakeX, titleY + 135 + bobOffset, safeArea.width, 110), "DIE", dieStyle);

        // Draw skull and crossbones between the words (simple version)
        DrawSkull(centerX - 15, titleY + 125 + bobOffset, 30f * pulseScale, menuAlpha);

        // Tagline - brighter for visibility
        GUIStyle tagStyle = new GUIStyle();
        tagStyle.fontSize = 18;
        tagStyle.fontStyle = FontStyle.Italic;
        tagStyle.alignment = TextAnchor.MiddleCenter;
        tagStyle.normal.textColor = new Color(0.8f, 0.85f, 0.9f, menuAlpha);
        GUI.Label(new Rect(safeArea.x, titleY + 235 + bobOffset, safeArea.width, 30), "\"In these waters, only the hungry survive.\"", tagStyle);

        // Beta version badge - brighter
        GUIStyle betaStyle = new GUIStyle();
        betaStyle.fontSize = 14;
        betaStyle.fontStyle = FontStyle.Bold;
        betaStyle.alignment = TextAnchor.MiddleCenter;
        betaStyle.normal.textColor = new Color(1f, 0.9f, 0.3f, menuAlpha);
        GUI.Label(new Rect(safeArea.x, titleY + 265 + bobOffset, safeArea.width, 25), "[ BETA - Tropical Island ]", betaStyle);
    }

    void DrawRotatedRect(Rect rect, float angle)
    {
        // Simple rotation approximation using multiple offset rects
        Matrix4x4 matrixBackup = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2));
        GUI.DrawTexture(rect, GetTexture("white"));
        GUI.matrix = matrixBackup;
    }

    void DrawSkull(float x, float y, float size, float alpha)
    {
        // Simple skull shape using primitives
        GUI.color = new Color(0.9f, 0.85f, 0.75f, alpha * 0.9f);

        // Skull head (circle approximation with oval)
        GUI.DrawTexture(new Rect(x - size * 0.4f, y - size * 0.5f, size * 0.8f, size * 0.7f), GetTexture("white"));

        // Eye sockets (dark)
        GUI.color = new Color(0.1f, 0.05f, 0.05f, alpha);
        GUI.DrawTexture(new Rect(x - size * 0.25f, y - size * 0.2f, size * 0.18f, size * 0.2f), GetTexture("white"));
        GUI.DrawTexture(new Rect(x + size * 0.07f, y - size * 0.2f, size * 0.18f, size * 0.2f), GetTexture("white"));

        // Nose hole
        GUI.DrawTexture(new Rect(x - size * 0.06f, y + size * 0.05f, size * 0.12f, size * 0.12f), GetTexture("white"));

        // Jaw/teeth area
        GUI.color = new Color(0.85f, 0.8f, 0.7f, alpha * 0.9f);
        GUI.DrawTexture(new Rect(x - size * 0.3f, y + size * 0.15f, size * 0.6f, size * 0.2f), GetTexture("white"));

        // Teeth lines
        GUI.color = new Color(0.1f, 0.05f, 0.05f, alpha * 0.7f);
        for (int i = 0; i < 4; i++)
        {
            GUI.DrawTexture(new Rect(x - size * 0.2f + i * size * 0.12f, y + size * 0.18f, 2, size * 0.12f), GetTexture("white"));
        }

        GUI.color = Color.white;
    }

    void DrawMainMenu()
    {
        float buttonWidth = 180;
        float buttonHeight = 36;
        float buttonSpacing = 12;
        float columnGap = 60; // Gap between left and right columns

        // Position buttons below the BETA text (which ends around 12% + 290 pixels)
        float startY = safeArea.y + safeArea.height * 0.12f + 310f; // Below BETA badge
        float centerX = safeArea.x + safeArea.width / 2;

        // Left column X position (buttons on the left)
        float leftX = centerX - buttonWidth - columnGap / 2;
        // Right column X position (buttons on the right)
        float rightX = centerX + columnGap / 2;

        // Left column: START NEW GAME, LOAD GAME
        if (DrawMenuButton(new Rect(leftX, startY, buttonWidth, buttonHeight), "START NEW GAME"))
        {
            StartNewGame();
        }
        if (DrawMenuButton(new Rect(leftX, startY + buttonHeight + buttonSpacing, buttonWidth, buttonHeight), "LOAD GAME"))
        {
            currentState = MenuState.LoadGame;
        }

        // Right column: SAVED GAMES, SETTINGS (symmetrical)
        if (DrawMenuButton(new Rect(rightX, startY, buttonWidth, buttonHeight), "SAVED GAMES"))
        {
            currentState = MenuState.SavedGames;
        }
        if (DrawMenuButton(new Rect(rightX, startY + buttonHeight + buttonSpacing, buttonWidth, buttonHeight), "SETTINGS"))
        {
            currentState = MenuState.Settings;
        }

        // Bottom center: QUIT
        float quitY = startY + (buttonHeight + buttonSpacing) * 2 + 20; // Extra spacing before quit
        if (DrawMenuButton(new Rect(centerX - buttonWidth / 2, quitY, buttonWidth, buttonHeight), "QUIT"))
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

        // Panel background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Header
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 28;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        GUI.Label(new Rect(panelX, panelY + 15, panelWidth, 40), "SETTINGS", headerStyle);

        // Close button
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

        // Music Volume
        GUI.Label(new Rect(panelX + 20, contentY, labelWidth, 25), "Music Volume", labelStyle);
        musicVolume = DrawSlider(new Rect(panelX + labelWidth + 30, contentY, sliderWidth, 20), musicVolume);
        contentY += 50;

        // SFX Volume
        GUI.Label(new Rect(panelX + 20, contentY, labelWidth, 25), "SFX Volume", labelStyle);
        sfxVolume = DrawSlider(new Rect(panelX + labelWidth + 30, contentY, sliderWidth, 20), sfxVolume);
        contentY += 50;

        // Quality
        GUI.Label(new Rect(panelX + 20, contentY, labelWidth, 25), "Quality", labelStyle);
        if (GUI.Button(new Rect(panelX + labelWidth + 30, contentY, 100, 28), "< " + qualityNames[qualityLevel] + " >"))
        {
            qualityLevel = (qualityLevel + 1) % qualityNames.Length;
            QualitySettings.SetQualityLevel(qualityLevel);
        }
        contentY += 50;

        // Fullscreen
        GUI.Label(new Rect(panelX + 20, contentY, labelWidth, 25), "Fullscreen", labelStyle);
        if (GUI.Button(new Rect(panelX + labelWidth + 30, contentY, 100, 28), fullscreen ? "ON" : "OFF"))
        {
            fullscreen = !fullscreen;
            Screen.fullScreen = fullscreen;
        }
        contentY += 70;

        // Save and Back buttons
        if (DrawMenuButton(new Rect(panelX + panelWidth / 2 - 100, contentY, 200, 45), "SAVE SETTINGS"))
        {
            SaveSettings();
            currentState = MenuState.Main;
        }
    }

    void DrawSavedGamesMenu()
    {
        float panelWidth = 550;
        float panelHeight = 450;
        float panelX = safeArea.x + (safeArea.width - panelWidth) / 2;
        float panelY = safeArea.y + (safeArea.height - panelHeight) / 2;

        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 28;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        GUI.Label(new Rect(panelX, panelY + 15, panelWidth, 40), "SAVED GAMES", headerStyle);

        if (DrawCloseButton(new Rect(panelX + panelWidth - 40, panelY + 10, 30, 30)))
        {
            currentState = MenuState.Main;
        }

        float contentY = panelY + 70;

        if (savedGames.Count == 0)
        {
            GUIStyle noSaveStyle = new GUIStyle(GUI.skin.label);
            noSaveStyle.fontSize = 18;
            noSaveStyle.alignment = TextAnchor.MiddleCenter;
            noSaveStyle.normal.textColor = new Color(0.6f, 0.6f, 0.7f);
            GUI.Label(new Rect(panelX, contentY + 100, panelWidth, 30), "No saved games found", noSaveStyle);
        }
        else
        {
            for (int i = 0; i < Mathf.Min(savedGames.Count, 5); i++)
            {
                DrawSaveSlot(new Rect(panelX + 20, contentY + i * 70, panelWidth - 40, 60), savedGames[i], false);
            }
        }
    }

    void DrawLoadGameMenu()
    {
        float panelWidth = 550;
        float panelHeight = 450;
        float panelX = safeArea.x + (safeArea.width - panelWidth) / 2;
        float panelY = safeArea.y + (safeArea.height - panelHeight) / 2;

        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 28;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        GUI.Label(new Rect(panelX, panelY + 15, panelWidth, 40), "LOAD GAME", headerStyle);

        if (DrawCloseButton(new Rect(panelX + panelWidth - 40, panelY + 10, 30, 30)))
        {
            currentState = MenuState.Main;
        }

        float contentY = panelY + 70;

        if (savedGames.Count == 0)
        {
            GUIStyle noSaveStyle = new GUIStyle(GUI.skin.label);
            noSaveStyle.fontSize = 18;
            noSaveStyle.alignment = TextAnchor.MiddleCenter;
            noSaveStyle.normal.textColor = new Color(0.6f, 0.6f, 0.7f);
            GUI.Label(new Rect(panelX, contentY + 100, panelWidth, 30), "No saved games to load", noSaveStyle);
        }
        else
        {
            for (int i = 0; i < Mathf.Min(savedGames.Count, 5); i++)
            {
                DrawSaveSlot(new Rect(panelX + 20, contentY + i * 70, panelWidth - 40, 60), savedGames[i], true);
            }
        }
    }

    void DrawSaveSlot(Rect rect, SavedGameInfo save, bool canLoad)
    {
        GUI.DrawTexture(rect, GetTexture("saveSlotBg"));

        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontSize = 16;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = Color.white;

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = 12;
        infoStyle.normal.textColor = new Color(0.7f, 0.7f, 0.8f);

        GUI.Label(new Rect(rect.x + 15, rect.y + 8, 300, 22), save.name, nameStyle);
        GUI.Label(new Rect(rect.x + 15, rect.y + 32, 300, 18), $"Level {save.level} | {save.gold} Gold | {save.playTime}", infoStyle);

        if (canLoad)
        {
            if (GUI.Button(new Rect(rect.x + rect.width - 80, rect.y + 15, 65, 30), "LOAD"))
            {
                LoadGame(save);
            }
        }
    }

    bool DrawMenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        bool pressed = hover && Input.GetMouseButton(0);

        Texture2D tex = pressed ? GetTexture("buttonPressed") :
                        hover ? GetTexture("buttonHover") : GetTexture("buttonNormal");

        // Button background
        GUI.DrawTexture(rect, tex);

        // Border on hover
        if (hover)
        {
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), GetTexture("panelBorder"));
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2), GetTexture("panelBorder"));
        }

        // Text
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

        // Handle click
        if (rect.Contains(Event.current.mousePosition) && Input.GetMouseButton(0))
        {
            value = (Event.current.mousePosition.x - rect.x) / rect.width;
            value = Mathf.Clamp01(value);
        }

        // Percentage label
        GUIStyle pctStyle = new GUIStyle(GUI.skin.label);
        pctStyle.fontSize = 12;
        pctStyle.alignment = TextAnchor.MiddleRight;
        pctStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(rect.x + rect.width + 10, rect.y, 40, rect.height), Mathf.RoundToInt(value * 100) + "%", pctStyle);

        return value;
    }

    void StartNewGame()
    {
        GameStarted = true;
        EnableGameSystems();

        // Reset game state for new game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = 0;
            GameManager.Instance.totalFishCaught = 0;
            GameManager.Instance.fishInventory.Clear();
        }

        Debug.Log("Starting new game!");
    }

    void LoadGame(SavedGameInfo save)
    {
        GameStarted = true;
        EnableGameSystems();

        // Load saved data
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = save.gold;
            GameManager.Instance.totalFishCaught = save.fishCaught;
        }

        // TODO: Load XP, level, equipment, etc.

        Debug.Log($"Loaded game: {save.name}");
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

    void RefreshSavedGames()
    {
        savedGames.Clear();

        // Check for save files
        string savePath = Application.persistentDataPath;
        if (Directory.Exists(savePath))
        {
            string[] files = Directory.GetFiles(savePath, "*.sav");
            foreach (string file in files)
            {
                // For now, create placeholder saves
                savedGames.Add(new SavedGameInfo
                {
                    name = Path.GetFileNameWithoutExtension(file),
                    level = 1,
                    gold = 0,
                    fishCaught = 0,
                    playTime = "0:00"
                });
            }
        }

        // Add a demo save for testing
        if (savedGames.Count == 0)
        {
            savedGames.Add(new SavedGameInfo
            {
                name = "Demo Save",
                level = 15,
                gold = 2500,
                fishCaught = 47,
                playTime = "2:34:12"
            });
        }
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

[System.Serializable]
public class SavedGameInfo
{
    public string name;
    public int level;
    public int gold;
    public int fishCaught;
    public string playTime;
}
