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
    public static bool GameStarted { get; private set; } = false;

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

    // Water animation for background
    private float waterTime = 0f;

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
        CacheTexture("overlay", new Color(0f, 0f, 0f, 0.85f));
        CacheTexture("panelBg", new Color(0.08f, 0.12f, 0.18f, 0.95f));
        CacheTexture("panelBorder", new Color(0.2f, 0.4f, 0.6f, 1f));
        CacheTexture("buttonNormal", new Color(0.15f, 0.25f, 0.4f, 0.95f));
        CacheTexture("buttonHover", new Color(0.2f, 0.35f, 0.55f, 1f));
        CacheTexture("buttonPressed", new Color(0.1f, 0.2f, 0.3f, 1f));
        CacheTexture("titleGlow", new Color(0.3f, 0.6f, 0.9f, 0.3f));
        CacheTexture("waterDark", new Color(0.05f, 0.15f, 0.25f, 1f));
        CacheTexture("waterLight", new Color(0.1f, 0.25f, 0.4f, 1f));
        CacheTexture("sliderBg", new Color(0.1f, 0.1f, 0.15f, 1f));
        CacheTexture("sliderFill", new Color(0.3f, 0.5f, 0.8f, 1f));
        CacheTexture("saveSlotBg", new Color(0.1f, 0.15f, 0.22f, 0.95f));
        CacheTexture("white", Color.white);
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
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;
            var rodAnim = player.GetComponent<FishingRodAnimator>();
            if (rodAnim != null) rodAnim.enabled = false;
        }
    }

    void EnableGameSystems()
    {
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null) controller.enabled = true;
            var rodAnim = player.GetComponent<FishingRodAnimator>();
            if (rodAnim != null) rodAnim.enabled = true;
        }
    }

    void Update()
    {
        if (GameStarted) return;

        titleBob += Time.deltaTime;
        waterTime += Time.deltaTime;

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

    void OnGUI()
    {
        if (GameStarted || !initialized) return;

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

        // Version and credits
        GUIStyle versionStyle = new GUIStyle(GUI.skin.label);
        versionStyle.fontSize = 12;
        versionStyle.normal.textColor = new Color(0.5f, 0.6f, 0.7f, menuAlpha);
        versionStyle.alignment = TextAnchor.LowerRight;
        GUI.Label(new Rect(Screen.width - 210, Screen.height - 30, 200, 25), "v1.0 - Made with Claude", versionStyle);

        GUI.color = Color.white;
    }

    void DrawWaterBackground()
    {
        // Animated wave pattern
        int numWaves = 20;
        float waveHeight = Screen.height / (float)numWaves;

        for (int i = 0; i < numWaves; i++)
        {
            float waveOffset = Mathf.Sin(waterTime * 0.5f + i * 0.3f) * 20f;
            float alpha = 0.3f + Mathf.Sin(waterTime * 0.3f + i * 0.2f) * 0.1f;

            Texture2D tex = (i % 2 == 0) ? GetTexture("waterDark") : GetTexture("waterLight");
            GUI.color = new Color(1, 1, 1, alpha);
            GUI.DrawTexture(new Rect(waveOffset, i * waveHeight, Screen.width + 40, waveHeight + 2), tex);
        }
        GUI.color = Color.white;
    }

    void DrawTitle()
    {
        float bobOffset = Mathf.Sin(titleBob * 1.5f) * 8f;

        // Glow behind title
        GUI.color = new Color(1, 1, 1, 0.3f * menuAlpha);
        GUI.DrawTexture(new Rect(Screen.width / 2 - 250, 50 + bobOffset - 10, 500, 100), GetTexture("titleGlow"));

        // Title text
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 72;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.9f, 0.95f, 1f, menuAlpha);

        // Shadow
        GUI.color = new Color(0, 0, 0, 0.5f * menuAlpha);
        GUI.Label(new Rect(4, 64 + bobOffset, Screen.width, 80), "FISHING GAME", titleStyle);

        // Main title
        GUI.color = new Color(1, 1, 1, menuAlpha);
        GUI.Label(new Rect(0, 60 + bobOffset, Screen.width, 80), "FISHING GAME", titleStyle);

        // Subtitle - Quote from Wetsuit Pete
        GUIStyle subStyle = new GUIStyle(GUI.skin.label);
        subStyle.fontSize = 22;
        subStyle.fontStyle = FontStyle.Italic;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.6f, 0.8f, 1f, menuAlpha);
        GUI.Label(new Rect(0, 140 + bobOffset, Screen.width, 30), "\"eat fish, don't drown\" - wetsuit pete", subStyle);
    }

    void DrawMainMenu()
    {
        float buttonWidth = 280;
        float buttonHeight = 50;
        float buttonSpacing = 15;
        float startY = Screen.height / 2 - 50;
        float centerX = (Screen.width - buttonWidth) / 2;

        string[] buttons = { "START NEW GAME", "LOAD GAME", "SAVED GAMES", "SETTINGS", "QUIT" };

        for (int i = 0; i < buttons.Length; i++)
        {
            Rect btnRect = new Rect(centerX, startY + i * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);

            if (DrawMenuButton(btnRect, buttons[i]))
            {
                switch (i)
                {
                    case 0: StartNewGame(); break;
                    case 1: currentState = MenuState.LoadGame; break;
                    case 2: currentState = MenuState.SavedGames; break;
                    case 3: currentState = MenuState.Settings; break;
                    case 4: QuitGame(); break;
                }
            }
        }
    }

    void DrawSettingsMenu()
    {
        float panelWidth = 500;
        float panelHeight = 400;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

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
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

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
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

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
        btnStyle.fontSize = 20;
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
