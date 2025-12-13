using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// In-game pause menu - ESC to open
/// Save Game, Load Game, Resume, Quit options
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }
    public static bool IsPaused { get; private set; } = false;

    private enum PauseState { Main, SaveConfirm, LoadConfirm, Controls }
    private PauseState currentState = PauseState.Main;

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;
    private int guiFrameSkip = 0;

    // Animation
    private float fadeAlpha = 0f;
    private float targetAlpha = 0f;

    // Save slot selection
    private int selectedSlot = -1;
    private string[] saveSlotNames = { "Slot 1", "Slot 2", "Slot 3" };

    // Message display
    private string statusMessage = "";
    private float messageTimer = 0f;

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
        // Consistent UI style
        CacheTexture("overlay", new Color(0f, 0f, 0f, 0.75f));
        CacheTexture("panelBg", new Color(0.1f, 0.1f, 0.12f, 0.95f));
        CacheTexture("panelBorder", new Color(1f, 0.85f, 0.4f, 1f)); // Gold border
        CacheTexture("buttonNormal", new Color(0.2f, 0.2f, 0.22f, 0.95f));
        CacheTexture("buttonHover", new Color(0.3f, 0.3f, 0.32f, 1f));
        CacheTexture("buttonPressed", new Color(0.15f, 0.15f, 0.17f, 1f));
        CacheTexture("slotNormal", new Color(0.15f, 0.15f, 0.17f, 0.95f));
        CacheTexture("slotSelected", new Color(0.25f, 0.25f, 0.27f, 1f));
        CacheTexture("white", Color.white);
        CacheTexture("success", new Color(0.2f, 0.6f, 0.3f, 1f));
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
        // Only work when game has started
        if (!MainMenu.GameStarted) return;

        // Don't pause if player is dead
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead()) return;

        // ESC to toggle pause
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

        // Animate fade
        fadeAlpha = Mathf.MoveTowards(fadeAlpha, targetAlpha, Time.unscaledDeltaTime * 5f);

        // Message timer
        if (messageTimer > 0)
        {
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0)
            {
                statusMessage = "";
            }
        }
    }

    void PauseGame()
    {
        IsPaused = true;
        targetAlpha = 1f;
        Time.timeScale = 0f;
        currentState = PauseState.Main;
        selectedSlot = -1;
    }

    void ResumeGame()
    {
        IsPaused = false;
        targetAlpha = 0f;
        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        // Performance: Skip frames when not actively needed
        if (!IsPaused)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        if (!initialized || fadeAlpha < 0.01f) return;

        GUI.color = new Color(1, 1, 1, fadeAlpha);

        // Dark overlay
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), GetTexture("overlay"));

        // Panel
        float panelWidth = currentState == PauseState.Controls ? 450 : 350;
        float panelHeight = currentState == PauseState.Main ? 380 : (currentState == PauseState.Controls ? 520 : 380);
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Panel border and background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Header - smaller, gold color
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 18; // Smaller
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = new Color(1f, 0.85f, 0.4f, fadeAlpha); // Gold
        GUI.Label(new Rect(panelX, panelY + 14, panelWidth, 30), "PAUSED", headerStyle);

        // Draw current state
        switch (currentState)
        {
            case PauseState.Main:
                DrawMainPauseMenu(panelX, panelY, panelWidth);
                break;
            case PauseState.SaveConfirm:
                DrawSaveMenu(panelX, panelY, panelWidth);
                break;
            case PauseState.LoadConfirm:
                DrawLoadMenu(panelX, panelY, panelWidth);
                break;
            case PauseState.Controls:
                DrawControlsMenu(panelX, panelY, panelWidth);
                break;
        }

        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUIStyle msgStyle = new GUIStyle(GUI.skin.label);
            msgStyle.fontSize = 16;
            msgStyle.alignment = TextAnchor.MiddleCenter;
            msgStyle.normal.textColor = new Color(0.3f, 0.9f, 0.4f, fadeAlpha);
            GUI.Label(new Rect(panelX, panelY + panelHeight - 35, panelWidth, 25), statusMessage, msgStyle);
        }

        GUI.color = Color.white;
    }

    void DrawMainPauseMenu(float panelX, float panelY, float panelWidth)
    {
        float buttonWidth = 220;
        float buttonHeight = 45;
        float buttonSpacing = 12;
        float startY = panelY + 80;
        float centerX = panelX + (panelWidth - buttonWidth) / 2;

        // Resume button
        if (DrawMenuButton(new Rect(centerX, startY, buttonWidth, buttonHeight), "RESUME"))
        {
            ResumeGame();
        }

        // Controls button
        if (DrawMenuButton(new Rect(centerX, startY + (buttonHeight + buttonSpacing), buttonWidth, buttonHeight), "CONTROLS"))
        {
            currentState = PauseState.Controls;
        }

        // Save Game button
        if (DrawMenuButton(new Rect(centerX, startY + 2 * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight), "SAVE GAME"))
        {
            currentState = PauseState.SaveConfirm;
            selectedSlot = -1;
        }

        // Load Game button
        if (DrawMenuButton(new Rect(centerX, startY + 3 * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight), "LOAD GAME"))
        {
            currentState = PauseState.LoadConfirm;
            selectedSlot = -1;
        }

        // Quit button
        if (DrawMenuButton(new Rect(centerX, startY + 4 * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight), "QUIT TO MENU"))
        {
            QuitToMenu();
        }
    }

    void DrawSaveMenu(float panelX, float panelY, float panelWidth)
    {
        GUIStyle subHeader = new GUIStyle(GUI.skin.label);
        subHeader.fontSize = 18;
        subHeader.alignment = TextAnchor.MiddleCenter;
        subHeader.normal.textColor = new Color(0.7f, 0.8f, 0.9f, fadeAlpha);
        GUI.Label(new Rect(panelX, panelY + 70, panelWidth, 25), "Select a slot to save:", subHeader);

        float slotY = panelY + 105;
        float slotHeight = 55;
        float slotSpacing = 8;

        for (int i = 0; i < 3; i++)
        {
            DrawSaveSlot(new Rect(panelX + 25, slotY + i * (slotHeight + slotSpacing), panelWidth - 50, slotHeight), i);
        }

        float buttonY = slotY + 3 * (slotHeight + slotSpacing) + 10;

        // Save button (only if slot selected)
        if (selectedSlot >= 0)
        {
            if (DrawMenuButton(new Rect(panelX + 25, buttonY, 140, 40), "SAVE"))
            {
                SaveGame(selectedSlot);
                currentState = PauseState.Main;
            }
        }

        // Back button
        if (DrawMenuButton(new Rect(panelX + panelWidth - 165, buttonY, 140, 40), "BACK"))
        {
            currentState = PauseState.Main;
        }
    }

    void DrawLoadMenu(float panelX, float panelY, float panelWidth)
    {
        GUIStyle subHeader = new GUIStyle(GUI.skin.label);
        subHeader.fontSize = 18;
        subHeader.alignment = TextAnchor.MiddleCenter;
        subHeader.normal.textColor = new Color(0.7f, 0.8f, 0.9f, fadeAlpha);
        GUI.Label(new Rect(panelX, panelY + 70, panelWidth, 25), "Select a slot to load:", subHeader);

        float slotY = panelY + 105;
        float slotHeight = 55;
        float slotSpacing = 8;

        for (int i = 0; i < 3; i++)
        {
            DrawSaveSlot(new Rect(panelX + 25, slotY + i * (slotHeight + slotSpacing), panelWidth - 50, slotHeight), i);
        }

        float buttonY = slotY + 3 * (slotHeight + slotSpacing) + 10;

        // Load button (only if slot selected and has save)
        if (selectedSlot >= 0 && HasSaveData(selectedSlot))
        {
            if (DrawMenuButton(new Rect(panelX + 25, buttonY, 140, 40), "LOAD"))
            {
                LoadGame(selectedSlot);
                ResumeGame();
            }
        }

        // Back button
        if (DrawMenuButton(new Rect(panelX + panelWidth - 165, buttonY, 140, 40), "BACK"))
        {
            currentState = PauseState.Main;
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

        // Control list styles
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

        // Movement
        GUI.Label(new Rect(keyX, y, 200, lineHeight), "-- MOVEMENT --", categoryStyle);
        y += lineHeight;

        DrawControlLine(keyX, actionX, ref y, lineHeight, "W A S D", "Move", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "SPACE", "Jump", keyStyle, actionStyle);
        DrawControlLine(keyX, actionX, ref y, lineHeight, "SHIFT", "Sprint", keyStyle, actionStyle);

        y += 8; // Spacing
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

        // Back button at bottom
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

    void DrawSaveSlot(Rect rect, int slotIndex)
    {
        bool isSelected = selectedSlot == slotIndex;
        bool hasSave = HasSaveData(slotIndex);
        bool hover = rect.Contains(Event.current.mousePosition);

        // Background
        Texture2D bgTex = isSelected ? GetTexture("slotSelected") : GetTexture("slotNormal");
        GUI.DrawTexture(rect, bgTex);

        // Hover highlight
        if (hover && !isSelected)
        {
            GUI.color = new Color(1, 1, 1, 0.1f * fadeAlpha);
            GUI.DrawTexture(rect, GetTexture("white"));
            GUI.color = new Color(1, 1, 1, fadeAlpha);
        }

        // Slot name
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.fontSize = 16;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = new Color(0.9f, 0.95f, 1f, fadeAlpha);
        GUI.Label(new Rect(rect.x + 15, rect.y + 8, 200, 22), saveSlotNames[slotIndex], nameStyle);

        // Save info
        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.fontSize = 12;
        infoStyle.normal.textColor = new Color(0.6f, 0.7f, 0.8f, fadeAlpha);

        if (hasSave)
        {
            int gold = PlayerPrefs.GetInt($"Save{slotIndex}_Gold", 0);
            int level = PlayerPrefs.GetInt($"Save{slotIndex}_Level", 1);
            long xp = long.Parse(PlayerPrefs.GetString($"Save{slotIndex}_XP", "0"));
            GUI.Label(new Rect(rect.x + 15, rect.y + 30, 280, 18), $"Level {level} | {gold} Gold | {xp:N0} XP", infoStyle);
        }
        else
        {
            GUI.Label(new Rect(rect.x + 15, rect.y + 30, 200, 18), "Empty Slot", infoStyle);
        }

        // Click to select
        if (GUI.Button(rect, "", GUIStyle.none))
        {
            selectedSlot = slotIndex;
        }
    }

    bool DrawMenuButton(Rect rect, string text)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        bool pressed = hover && Input.GetMouseButton(0);

        Texture2D tex = pressed ? GetTexture("buttonPressed") :
                        hover ? GetTexture("buttonHover") : GetTexture("buttonNormal");

        GUI.DrawTexture(rect, tex);

        // Border on hover
        if (hover)
        {
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), GetTexture("panelBorder"));
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 2, rect.width, 2), GetTexture("panelBorder"));
        }

        // Text
        GUIStyle btnStyle = new GUIStyle(GUI.skin.label);
        btnStyle.fontSize = 18;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = new Color(hover ? 1f : 0.85f, hover ? 1f : 0.9f, 1f, fadeAlpha);

        GUI.Label(rect, text, btnStyle);

        return GUI.Button(rect, "", GUIStyle.none);
    }

    bool HasSaveData(int slot)
    {
        return PlayerPrefs.HasKey($"Save{slot}_Gold");
    }

    void SaveGame(int slot)
    {
        // Save gold
        if (GameManager.Instance != null)
        {
            PlayerPrefs.SetInt($"Save{slot}_Gold", GameManager.Instance.coins);
            PlayerPrefs.SetInt($"Save{slot}_FishCaught", GameManager.Instance.totalFishCaught);
        }

        // Save XP and level
        if (LevelingSystem.Instance != null)
        {
            PlayerPrefs.SetString($"Save{slot}_XP", LevelingSystem.Instance.GetCurrentXP().ToString());
            PlayerPrefs.SetInt($"Save{slot}_Level", LevelingSystem.Instance.GetLevel());
        }

        // Save player position
        if (GameCache.IsPlayerValid())
        {
            Vector3 pos = GameCache.Player.position;
            PlayerPrefs.SetFloat($"Save{slot}_PosX", pos.x);
            PlayerPrefs.SetFloat($"Save{slot}_PosY", pos.y);
            PlayerPrefs.SetFloat($"Save{slot}_PosZ", pos.z);
        }

        // Save health
        if (PlayerHealth.Instance != null)
        {
            PlayerPrefs.SetFloat($"Save{slot}_Health", PlayerHealth.Instance.GetCurrentHealth());
        }

        // Save timestamp
        PlayerPrefs.SetString($"Save{slot}_Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

        PlayerPrefs.Save();

        statusMessage = "Game Saved!";
        messageTimer = 2f;

        Debug.Log($"Game saved to slot {slot}");
    }

    void LoadGame(int slot)
    {
        if (!HasSaveData(slot)) return;

        // Load gold
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = PlayerPrefs.GetInt($"Save{slot}_Gold", 0);
            GameManager.Instance.totalFishCaught = PlayerPrefs.GetInt($"Save{slot}_FishCaught", 0);
        }

        // Load player position
        if (GameCache.IsPlayerValid() && PlayerPrefs.HasKey($"Save{slot}_PosX"))
        {
            float x = PlayerPrefs.GetFloat($"Save{slot}_PosX");
            float y = PlayerPrefs.GetFloat($"Save{slot}_PosY");
            float z = PlayerPrefs.GetFloat($"Save{slot}_PosZ");
            GameCache.Player.position = new Vector3(x, y, z);
        }

        // Load health
        if (PlayerHealth.Instance != null && PlayerPrefs.HasKey($"Save{slot}_Health"))
        {
            float health = PlayerPrefs.GetFloat($"Save{slot}_Health");
            PlayerHealth.Instance.SetHealth(health);
        }

        statusMessage = "Game Loaded!";
        messageTimer = 2f;

        Debug.Log($"Game loaded from slot {slot}");
    }

    void QuitToMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        // Reset MainMenu state to show menu again
        MainMenu.GameStarted = false;

        // Reset player to spawn
        if (GameCache.IsPlayerValid())
        {
            GameCache.Player.position = new Vector3(0, 2, 0);
        }

        // Reset health
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.SetHealth(100f);
        }
    }

    void OnDestroy()
    {
        // Ensure time scale is reset
        Time.timeScale = 1f;

        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}
