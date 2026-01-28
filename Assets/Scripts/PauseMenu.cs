using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// In-game pause menu - ESC to open
/// Resume, Controls, Quit options
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }
    public static bool IsPaused { get; private set; } = false;

    private enum PauseState { Main, Controls }
    private PauseState currentState = PauseState.Main;

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;

    // Animation
    private float fadeAlpha = 0f;
    private float targetAlpha = 0f;

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
        Invoke("Initialize", 0.3f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        initialized = true;
    }

    void CreateCachedTextures()
    {
        CacheTexture("overlay", new Color(0f, 0f, 0f, 0.75f));
        CacheTexture("panelBg", new Color(0.1f, 0.1f, 0.12f, 0.95f));
        CacheTexture("panelBorder", new Color(1f, 0.85f, 0.4f, 1f));
        CacheTexture("buttonNormal", new Color(0.2f, 0.2f, 0.22f, 0.95f));
        CacheTexture("buttonHover", new Color(0.3f, 0.3f, 0.32f, 1f));
        CacheTexture("buttonPressed", new Color(0.15f, 0.15f, 0.17f, 1f));
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

    void Update()
    {
        if (!MainMenu.GameStarted) return;
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                if (currentState != PauseState.Main)
                {
                    currentState = PauseState.Main;
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }

        fadeAlpha = Mathf.MoveTowards(fadeAlpha, targetAlpha, Time.unscaledDeltaTime * 5f);
    }

    void PauseGame()
    {
        IsPaused = true;
        targetAlpha = 1f;
        Time.timeScale = 0f;
        currentState = PauseState.Main;
    }

    void ResumeGame()
    {
        IsPaused = false;
        targetAlpha = 0f;
        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        if (!initialized || fadeAlpha < 0.01f) return;

        GUI.color = new Color(1, 1, 1, fadeAlpha);

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("overlay"));

        float panelWidth = currentState == PauseState.Controls ? 450 : 300;
        float panelHeight = currentState == PauseState.Main ? 250 : 520;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 18;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = new Color(1f, 0.85f, 0.4f, fadeAlpha);
        GUI.Label(new Rect(panelX, panelY + 14, panelWidth, 30), "PAUSED", headerStyle);

        switch (currentState)
        {
            case PauseState.Main:
                DrawMainPauseMenu(panelX, panelY, panelWidth);
                break;
            case PauseState.Controls:
                DrawControlsMenu(panelX, panelY, panelWidth);
                break;
        }

        GUI.color = Color.white;
    }

    void DrawMainPauseMenu(float panelX, float panelY, float panelWidth)
    {
        float buttonWidth = 200;
        float buttonHeight = 45;
        float buttonSpacing = 12;
        float startY = panelY + 60;
        float centerX = panelX + (panelWidth - buttonWidth) / 2;

        if (DrawMenuButton(new Rect(centerX, startY, buttonWidth, buttonHeight), "RESUME"))
        {
            ResumeGame();
        }

        if (DrawMenuButton(new Rect(centerX, startY + (buttonHeight + buttonSpacing), buttonWidth, buttonHeight), "CONTROLS"))
        {
            currentState = PauseState.Controls;
        }

        if (DrawMenuButton(new Rect(centerX, startY + 2 * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight), "QUIT TO MENU"))
        {
            QuitToMenu();
        }
    }

    void DrawControlsMenu(float panelX, float panelY, float panelWidth)
    {
        GUIStyle subHeader = new GUIStyle(GUI.skin.label);
        subHeader.fontSize = 18;
        subHeader.fontStyle = FontStyle.Bold;
        subHeader.alignment = TextAnchor.MiddleCenter;
        subHeader.normal.textColor = new Color(1f, 0.85f, 0.4f, fadeAlpha);
        GUI.Label(new Rect(panelX, panelY + 50, panelWidth, 25), "CONTROLS", subHeader);

        GUIStyle keyStyle = new GUIStyle(GUI.skin.label);
        keyStyle.fontSize = 14;
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.alignment = TextAnchor.MiddleRight;
        keyStyle.normal.textColor = new Color(0.5f, 0.8f, 1f, fadeAlpha);

        GUIStyle actionStyle = new GUIStyle(GUI.skin.label);
        actionStyle.fontSize = 14;
        actionStyle.alignment = TextAnchor.MiddleLeft;
        actionStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, fadeAlpha);

        GUIStyle categoryStyle = new GUIStyle(GUI.skin.label);
        categoryStyle.fontSize = 12;
        categoryStyle.fontStyle = FontStyle.Bold;
        categoryStyle.alignment = TextAnchor.MiddleLeft;
        categoryStyle.normal.textColor = new Color(0.7f, 0.7f, 0.5f, fadeAlpha);

        float startY = panelY + 85;
        float keyX = panelX + 20;
        float actionX = panelX + 130;
        float lineHeight = 22f;
        float y = startY;

        GUI.Label(new Rect(keyX, y, 200, lineHeight), "-- MOVEMENT --", categoryStyle);
        y += lineHeight;
        DrawControlLine(keyX, actionX, ref y, lineHeight, "W A S D", "Move", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "SPACE", "Jump", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "SHIFT", "Sprint", keyStyle, actionStyle);

        y += 8;
        GUI.Label(new Rect(keyX, y, 200, lineHeight), "-- COMBAT --", categoryStyle);
        y += lineHeight;
        DrawControlLine(keyX, actionX, ref y, lineHeight, "LEFT CLICK", "Attack / Cast Rod", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "Q", "Quick Swap Weapon", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "1-5", "Select Weapon Slot", keyStyle, actionStyle);

        y += 8;
        GUI.Label(new Rect(keyX, y, 200, lineHeight), "-- INTERACTION --", categoryStyle);
        y += lineHeight;
        DrawControlLine(keyX, actionX, ref y, lineHeight, "E", "Interact / Talk", keyStyle, actionStyle);

        y += 8;
        GUI.Label(new Rect(keyX, y, 200, lineHeight), "-- UI PANELS --", categoryStyle);
        y += lineHeight;
        DrawControlLine(keyX, actionX, ref y, lineHeight, "TAB / C", "Character Panel", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "I", "Inventory", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "F", "Fish Inventory", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "J", "Fish Diary", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "ESC", "Pause Menu", keyStyle, actionStyle);

        float buttonY = panelY + 470;
        if (DrawMenuButton(new Rect(panelX + (panelWidth - 140) / 2, buttonY, 140, 40), "BACK"))
        {
            currentState = PauseState.Main;
        }
    }

    void DrawControlLine(float keyX, float actionX, ref float y, float lineHeight, string key, string action, GUIStyle keyStyle, GUIStyle actionStyle)
    {
        GUI.Label(new Rect(keyX, y, 100, lineHeight), key, keyStyle);
        GUI.Label(new Rect(actionX, y, 250, lineHeight), action, actionStyle);
        y += lineHeight;
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
        btnStyle.fontSize = 18;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = new Color(hover ? 1f : 0.85f, hover ? 1f : 0.9f, 1f, fadeAlpha);

        GUI.Label(rect, text, btnStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    void QuitToMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        // Save game before quitting to menu
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
        }

        // Full restart - reload the scene like pressing stop/play in editor
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;

        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}
