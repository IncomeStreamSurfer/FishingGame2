using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Developer Console for testing and debugging game mechanics
/// Opens with ~ or F1 or F12 key
/// Provides commands for triggering storms, changing time, healing, etc.
/// </summary>
public class ConsoleCommands : MonoBehaviour
{
    public static ConsoleCommands Instance { get; private set; }

    [Header("Console Settings")]
    [Tooltip("Show console on startup for easy testing")]
    public bool startVisible = false;

    private bool isVisible = false;
    private string inputText = "";
    private List<string> commandHistory = new List<string>();
    private List<string> outputLog = new List<string>();
    private int historyIndex = -1;
    private Vector2 scrollPosition = Vector2.zero;
    private const int maxLogLines = 20;

    // GUI positioning
    private Rect consoleRect;
    private Rect inputRect;
    private Rect outputRect;

    // Textures
    private Texture2D consoleBg;
    private Texture2D inputBg;
    private Texture2D outputBg;
    private Texture2D borderTex;  // Cached border texture
    private bool initialized = false;

    // Available commands
    private Dictionary<string, System.Action<string[]>> commands;

    void Awake()
    {
        // Disable in release mode
        if (GameConfig.RELEASE_MODE)
        {
            Destroy(gameObject);
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (GameConfig.RELEASE_MODE) return;

        InitializeCommands();
        CreateTextures();
        CalculateRects();
        initialized = true;
        isVisible = startVisible;

        // Welcome message
        if (startVisible)
        {
            LogOutput("Developer Console Ready. Type 'help' for commands.");
        }
    }

    void InitializeCommands()
    {
        commands = new Dictionary<string, System.Action<string[]>>()
        {
            { "help", CmdHelp },
            { "clear", CmdClear },
            { "storm", CmdStorm },
            { "lightning", CmdStorm },
            { "time", CmdTime },
            { "day", CmdDay },
            { "night", CmdNight },
            { "sunrise", CmdSunrise },
            { "sunset", CmdSunset },
            { "noon", CmdNoon },
            { "midnight", CmdMidnight },
            { "heal", CmdHeal },
            { "coins", CmdCoins },
            { "gold", CmdCoins },
            { "kill", CmdKill },
            { "tp", CmdTeleport },
            { "teleport", CmdTeleport },
            { "spawn", CmdSpawn },
            { "god", CmdGodMode },
            { "speed", CmdSpeed },
            { "endstorm", CmdEndStorm }
        };
    }

    void CreateTextures()
    {
        // Console background - dark with transparency
        consoleBg = new Texture2D(1, 1);
        consoleBg.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.85f));
        consoleBg.Apply();

        // Input background - slightly lighter
        inputBg = new Texture2D(1, 1);
        inputBg.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.95f));
        inputBg.Apply();

        // Output background - very dark
        outputBg = new Texture2D(1, 1);
        outputBg.SetPixel(0, 0, new Color(0.05f, 0.05f, 0.05f, 0.9f));
        outputBg.Apply();

        // Border texture (white, tinted with GUI.color)
        borderTex = new Texture2D(1, 1);
        borderTex.SetPixel(0, 0, Color.white);
        borderTex.Apply();
    }

    void CalculateRects()
    {
        // Reduced width to 60% to make room for quick action buttons on the right
        float consoleWidth = Screen.width * 0.6f;
        float consoleHeight = 400;
        float consoleX = (Screen.width - consoleWidth - 150) / 2; // Offset left to center with buttons
        float consoleY = 50;

        consoleRect = new Rect(consoleX, consoleY, consoleWidth, consoleHeight);
        inputRect = new Rect(consoleX + 10, consoleY + consoleHeight - 40, consoleWidth - 20, 30);
        outputRect = new Rect(consoleX + 10, consoleY + 30, consoleWidth - 20, consoleHeight - 80);
    }

    void Update()
    {
        if (!initialized) return;

        // Toggle console with ~ or F1 or F12
        if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.F12))
        {
            ToggleConsole();
        }

        // Close with Escape
        if (isVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            isVisible = false;
        }

        // Submit command with Enter
        if (isVisible && Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrWhiteSpace(inputText))
        {
            ExecuteCommand(inputText);
            inputText = "";
            historyIndex = -1;
        }

        // Command history navigation
        if (isVisible)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateHistory(1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateHistory(-1);
            }
        }
    }

    void ToggleConsole()
    {
        isVisible = !isVisible;
        if (isVisible && outputLog.Count == 0)
        {
            LogOutput("Developer Console - Type 'help' for commands");
        }
    }

    void NavigateHistory(int direction)
    {
        if (commandHistory.Count == 0) return;

        historyIndex += direction;
        historyIndex = Mathf.Clamp(historyIndex, -1, commandHistory.Count - 1);

        if (historyIndex >= 0)
        {
            inputText = commandHistory[commandHistory.Count - 1 - historyIndex];
        }
        else
        {
            inputText = "";
        }
    }

    void ExecuteCommand(string commandLine)
    {
        commandLine = commandLine.Trim();
        LogOutput("> " + commandLine);

        // Add to history
        if (!string.IsNullOrEmpty(commandLine))
        {
            commandHistory.Add(commandLine);
            if (commandHistory.Count > 50) // Keep last 50 commands
            {
                commandHistory.RemoveAt(0);
            }
        }

        // Parse command
        string[] parts = commandLine.Split(' ');
        string cmd = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        // Execute command
        if (commands.ContainsKey(cmd))
        {
            try
            {
                commands[cmd](args);
            }
            catch (System.Exception e)
            {
                LogOutput($"ERROR: {e.Message}");
            }
        }
        else
        {
            LogOutput($"Unknown command: {cmd}. Type 'help' for available commands.");
        }
    }

    void LogOutput(string message)
    {
        outputLog.Add(message);
        if (outputLog.Count > maxLogLines)
        {
            outputLog.RemoveAt(0);
        }
        // Auto-scroll to bottom
        scrollPosition = new Vector2(0, float.MaxValue);
    }

    // ============================================
    // COMMAND IMPLEMENTATIONS
    // ============================================

    void CmdHelp(string[] args)
    {
        LogOutput("=== DEVELOPER CONSOLE COMMANDS ===");
        LogOutput("Weather & Environment:");
        LogOutput("  storm/lightning - Trigger thunderstorm");
        LogOutput("  endstorm - End current storm");
        LogOutput("  time [hour] - Set time of day (0-24, e.g., 'time 14')");
        LogOutput("  day/noon - Set to noon (12:00)");
        LogOutput("  night/midnight - Set to midnight (0:00)");
        LogOutput("  sunrise - Set to sunrise (6:00)");
        LogOutput("  sunset - Set to sunset (18:00)");
        LogOutput("");
        LogOutput("Player:");
        LogOutput("  heal [amount] - Restore health (default: full heal)");
        LogOutput("  kill - Kill player instantly");
        LogOutput("  coins/gold [amount] - Give coins (e.g., 'coins 1000')");
        LogOutput("  tp/teleport [x] [y] [z] - Teleport to position");
        LogOutput("  spawn - Teleport to spawn point");
        LogOutput("  god - Toggle god mode (no damage)");
        LogOutput("  speed [multiplier] - Set movement speed");
        LogOutput("");
        LogOutput("Utility:");
        LogOutput("  clear - Clear console output");
        LogOutput("  help - Show this help message");
    }

    void CmdClear(string[] args)
    {
        outputLog.Clear();
        LogOutput("Console cleared.");
    }

    void CmdStorm(string[] args)
    {
        if (ThunderstormSystem.Instance != null)
        {
            // Access the private StartStorm method via reflection
            var method = typeof(ThunderstormSystem).GetMethod("StartStorm",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                method.Invoke(ThunderstormSystem.Instance, null);
                LogOutput("Thunderstorm triggered!");
            }
            else
            {
                LogOutput("ERROR: Could not trigger storm (reflection failed)");
            }
        }
        else
        {
            LogOutput("ERROR: ThunderstormSystem not found in scene");
        }
    }

    void CmdEndStorm(string[] args)
    {
        if (ThunderstormSystem.Instance != null)
        {
            // Access the private EndStorm method via reflection
            var method = typeof(ThunderstormSystem).GetMethod("EndStorm",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                method.Invoke(ThunderstormSystem.Instance, null);
                LogOutput("Storm ended.");
            }
            else
            {
                LogOutput("ERROR: Could not end storm (reflection failed)");
            }
        }
        else
        {
            LogOutput("ERROR: ThunderstormSystem not found in scene");
        }
    }

    void CmdTime(string[] args)
    {
        if (args.Length == 0)
        {
            if (DayNightCycle.Instance != null)
            {
                float hour = DayNightCycle.Instance.GetCurrentHour();
                LogOutput($"Current time: {hour:F1} hours ({FormatTime(hour)})");
            }
            else
            {
                LogOutput("ERROR: DayNightCycle not found");
            }
            return;
        }

        if (float.TryParse(args[0], out float targetHour))
        {
            if (targetHour < 0 || targetHour >= 24)
            {
                LogOutput("ERROR: Hour must be between 0 and 24");
                return;
            }

            if (DayNightCycle.Instance != null)
            {
                DayNightCycle.Instance.SetTimeOfDay(targetHour);
                LogOutput($"Time set to {targetHour:F1} hours ({FormatTime(targetHour)})");
            }
            else
            {
                LogOutput("ERROR: DayNightCycle not found in scene");
            }
        }
        else
        {
            LogOutput("ERROR: Invalid hour value. Use 'time [0-24]'");
        }
    }

    void CmdDay(string[] args)
    {
        CmdTime(new string[] { "12" });
    }

    void CmdNight(string[] args)
    {
        CmdTime(new string[] { "0" });
    }

    void CmdSunrise(string[] args)
    {
        CmdTime(new string[] { "6" });
    }

    void CmdSunset(string[] args)
    {
        CmdTime(new string[] { "18" });
    }

    void CmdNoon(string[] args)
    {
        CmdTime(new string[] { "12" });
    }

    void CmdMidnight(string[] args)
    {
        CmdTime(new string[] { "0" });
    }

    void CmdHeal(string[] args)
    {
        if (PlayerHealth.Instance == null)
        {
            LogOutput("ERROR: PlayerHealth not found");
            return;
        }

        if (args.Length == 0)
        {
            // Full heal
            PlayerHealth.Instance.HealToFull();
            LogOutput("Player fully healed!");
        }
        else if (float.TryParse(args[0], out float amount))
        {
            PlayerHealth.Instance.Heal(amount);
            LogOutput($"Healed {amount} HP");
        }
        else
        {
            LogOutput("ERROR: Invalid amount. Use 'heal' or 'heal [amount]'");
        }
    }

    void CmdCoins(string[] args)
    {
        if (GameManager.Instance == null)
        {
            LogOutput("ERROR: GameManager not found");
            return;
        }

        if (args.Length == 0)
        {
            int current = GameManager.Instance.GetCoins();
            LogOutput($"Current coins: {current}");
            return;
        }

        if (int.TryParse(args[0], out int amount))
        {
            GameManager.Instance.AddCoins(amount);
            LogOutput($"Added {amount} coins. Total: {GameManager.Instance.GetCoins()}");
        }
        else
        {
            LogOutput("ERROR: Invalid amount. Use 'coins [amount]'");
        }
    }

    void CmdKill(string[] args)
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(999f, "killed by console command");
            LogOutput("Player killed.");
        }
        else
        {
            LogOutput("ERROR: PlayerHealth not found");
        }
    }

    void CmdTeleport(string[] args)
    {
        if (!GameCache.IsPlayerValid())
        {
            LogOutput("ERROR: Player not found");
            return;
        }

        if (args.Length < 3)
        {
            Vector3 pos = GameCache.Player.position;
            LogOutput($"Current position: {pos.x:F1}, {pos.y:F1}, {pos.z:F1}");
            LogOutput("Usage: tp [x] [y] [z]");
            return;
        }

        if (float.TryParse(args[0], out float x) &&
            float.TryParse(args[1], out float y) &&
            float.TryParse(args[2], out float z))
        {
            Vector3 newPos = new Vector3(x, y, z);
            GameCache.Player.position = newPos;
            LogOutput($"Teleported to {x:F1}, {y:F1}, {z:F1}");
        }
        else
        {
            LogOutput("ERROR: Invalid coordinates. Use: tp [x] [y] [z]");
        }
    }

    void CmdSpawn(string[] args)
    {
        if (GameCache.IsPlayerValid())
        {
            GameCache.Player.position = new Vector3(0, 2f, -5f);
            LogOutput("Teleported to spawn point (0, 2, -5)");
        }
        else
        {
            LogOutput("ERROR: Player not found");
        }
    }

    void CmdGodMode(string[] args)
    {
        // Note: This would require adding a god mode flag to PlayerHealth
        // For now, just heal to full and notify
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.HealToFull();
            LogOutput("God mode not implemented yet. Player healed to full.");
            LogOutput("(Would need to add god mode flag to PlayerHealth.cs)");
        }
        else
        {
            LogOutput("ERROR: PlayerHealth not found");
        }
    }

    void CmdSpeed(string[] args)
    {
        LogOutput("Speed command not implemented yet.");
        LogOutput("(Would need to modify player movement script)");
    }

    // Helper method to format time nicely
    string FormatTime(float hour)
    {
        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60);
        string ampm = h >= 12 ? "PM" : "AM";
        int displayHour = h % 12;
        if (displayHour == 0) displayHour = 12;
        return $"{displayHour}:{m:D2} {ampm}";
    }

    // ============================================
    // GUI RENDERING
    // ============================================

    void OnGUI()
    {
        if (!initialized || !isVisible) return;

        // Recalculate rects if screen size changed
        if (Mathf.Abs(Screen.width - consoleRect.x * 2) > 10)
        {
            CalculateRects();
        }

        // Console background
        GUI.DrawTexture(consoleRect, consoleBg);

        // Border
        DrawBorder(consoleRect, new Color(0.3f, 0.8f, 0.3f), 2);

        // Title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);

        Rect titleRect = new Rect(consoleRect.x, consoleRect.y + 5, consoleRect.width, 20);
        GUI.Label(titleRect, "DEVELOPER CONSOLE", titleStyle);

        // Output log area
        GUI.DrawTexture(outputRect, outputBg);
        DrawOutputLog();

        // Input area
        GUI.DrawTexture(inputRect, inputBg);
        DrawInputField();

        // Quick action buttons panel
        DrawQuickButtons();

        // Help text
        GUIStyle helpStyle = new GUIStyle(GUI.skin.label);
        helpStyle.fontSize = 10;
        helpStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        Rect helpRect = new Rect(consoleRect.x + 10, consoleRect.y + consoleRect.height - 15, consoleRect.width - 20, 12);
        GUI.Label(helpRect, "~ or F1 or F12 to close | Enter to execute | Up/Down for history | Type 'help' for commands", helpStyle);
    }

    void DrawQuickButtons()
    {
        // Button panel on right side of console
        float buttonPanelX = consoleRect.x + consoleRect.width + 10;
        float buttonPanelY = consoleRect.y;
        float buttonWidth = 120;
        float buttonHeight = 30;
        float buttonSpacing = 5;

        // Panel background
        float panelWidth = buttonWidth + 20;
        float panelHeight = 200;
        GUI.DrawTexture(new Rect(buttonPanelX, buttonPanelY, panelWidth, panelHeight), consoleBg);
        DrawBorder(new Rect(buttonPanelX, buttonPanelY, panelWidth, panelHeight), new Color(0.3f, 0.8f, 0.3f), 2);

        // Panel title
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 12;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.3f, 1f, 0.3f);
        GUI.Label(new Rect(buttonPanelX, buttonPanelY + 5, panelWidth, 20), "QUICK ACTIONS", titleStyle);

        // Button style
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 11;
        buttonStyle.fontStyle = FontStyle.Bold;

        float currentY = buttonPanelY + 30;

        // Storm button (yellow/orange for weather)
        GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
        if (GUI.Button(new Rect(buttonPanelX + 10, currentY, buttonWidth, buttonHeight), "STORM", buttonStyle))
        {
            CmdStorm(new string[0]);
        }
        currentY += buttonHeight + buttonSpacing;

        // End Storm button
        GUI.backgroundColor = new Color(0.5f, 0.7f, 1f);
        if (GUI.Button(new Rect(buttonPanelX + 10, currentY, buttonWidth, buttonHeight), "END STORM", buttonStyle))
        {
            CmdEndStorm(new string[0]);
        }
        currentY += buttonHeight + buttonSpacing;

        // Heal button (green)
        GUI.backgroundColor = new Color(0.3f, 1f, 0.3f);
        if (GUI.Button(new Rect(buttonPanelX + 10, currentY, buttonWidth, buttonHeight), "HEAL", buttonStyle))
        {
            CmdHeal(new string[0]);
        }
        currentY += buttonHeight + buttonSpacing;

        // Give coins button (gold)
        GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
        if (GUI.Button(new Rect(buttonPanelX + 10, currentY, buttonWidth, buttonHeight), "+1000 COINS", buttonStyle))
        {
            CmdCoins(new string[] { "1000" });
        }
        currentY += buttonHeight + buttonSpacing;

        // Spawn (teleport home) button
        GUI.backgroundColor = new Color(0.7f, 0.5f, 1f);
        if (GUI.Button(new Rect(buttonPanelX + 10, currentY, buttonWidth, buttonHeight), "SPAWN", buttonStyle))
        {
            CmdSpawn(new string[0]);
        }

        // Reset background color
        GUI.backgroundColor = Color.white;
    }

    void DrawBorder(Rect rect, Color color, int thickness)
    {
        // Use cached texture with color tinting (no allocation!)
        GUI.color = color;
        // Top
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), borderTex);
        // Bottom
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), borderTex);
        // Left
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), borderTex);
        // Right
        GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), borderTex);
        GUI.color = Color.white;
    }

    void DrawOutputLog()
    {
        GUIStyle logStyle = new GUIStyle(GUI.skin.label);
        logStyle.fontSize = 12;
        logStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
        logStyle.wordWrap = true;
        logStyle.padding = new RectOffset(5, 5, 5, 5);

        // Calculate total height needed
        float totalHeight = 0;
        foreach (string line in outputLog)
        {
            totalHeight += logStyle.CalcHeight(new GUIContent(line), outputRect.width - 10) + 2;
        }

        // Scrollable area
        Rect viewRect = new Rect(0, 0, outputRect.width - 20, Mathf.Max(totalHeight, outputRect.height));
        scrollPosition = GUI.BeginScrollView(outputRect, scrollPosition, viewRect);

        float yOffset = 0;
        foreach (string line in outputLog)
        {
            // Color output differently based on content
            if (line.StartsWith(">"))
            {
                logStyle.normal.textColor = new Color(0.5f, 1f, 0.5f); // User input - bright green
            }
            else if (line.StartsWith("ERROR:"))
            {
                logStyle.normal.textColor = new Color(1f, 0.3f, 0.3f); // Errors - red
            }
            else if (line.StartsWith("==="))
            {
                logStyle.normal.textColor = new Color(1f, 1f, 0.5f); // Headers - yellow
            }
            else
            {
                logStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f); // Normal output - gray
            }

            float lineHeight = logStyle.CalcHeight(new GUIContent(line), viewRect.width - 10);
            GUI.Label(new Rect(5, yOffset, viewRect.width - 10, lineHeight), line, logStyle);
            yOffset += lineHeight + 2;
        }

        GUI.EndScrollView();
    }

    void DrawInputField()
    {
        GUIStyle inputStyle = new GUIStyle(GUI.skin.textField);
        inputStyle.fontSize = 14;
        inputStyle.normal.textColor = Color.white;
        inputStyle.focused.textColor = Color.white;
        inputStyle.padding = new RectOffset(5, 5, 8, 8);

        // Prompt
        GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
        promptStyle.fontSize = 14;
        promptStyle.normal.textColor = new Color(0.5f, 1f, 0.5f);
        promptStyle.alignment = TextAnchor.MiddleLeft;

        Rect promptRect = new Rect(inputRect.x + 5, inputRect.y, 20, inputRect.height);
        GUI.Label(promptRect, ">", promptStyle);

        // Input field
        GUI.SetNextControlName("ConsoleInput");
        Rect textFieldRect = new Rect(inputRect.x + 25, inputRect.y + 3, inputRect.width - 30, inputRect.height - 6);
        inputText = GUI.TextField(textFieldRect, inputText, inputStyle);

        // Auto-focus input field when console is visible
        if (Event.current.type == EventType.Layout)
        {
            GUI.FocusControl("ConsoleInput");
        }
    }

    void OnDestroy()
    {
        if (consoleBg != null) Destroy(consoleBg);
        if (inputBg != null) Destroy(inputBg);
        if (outputBg != null) Destroy(outputBg);
        if (borderTex != null) Destroy(borderTex);
    }
}
