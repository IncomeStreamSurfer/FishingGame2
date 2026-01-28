using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Time Alive Tracker - Shows survival time in top left with skull icon
/// Records time to leaderboard when player dies
/// </summary>
public class TimeAliveTracker : MonoBehaviour
{
    public static TimeAliveTracker Instance { get; private set; }

    // Time tracking
    private float timeAlive = 0f;
    private bool isTracking = false;
    private bool gameStartedThisSession = false;

    // Leaderboard (top 10 times)
    private List<float> bestTimes = new List<float>();
    private const int MAX_LEADERBOARD_ENTRIES = 10;

    // GUI
    private Texture2D bgTex;
    private Texture2D skullTex;
    private GUIStyle timerStyle;
    private GUIStyle skullStyle;
    private GUIStyle dayStyle;
    private bool guiInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadLeaderboard();

        // Subscribe to game over event
        PlayerHealth.OnGameOver += OnPlayerDeath;

        // Subscribe to scene loaded to reset on game restart
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        PlayerHealth.OnGameOver -= OnPlayerDeath;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (bgTex != null) Destroy(bgTex);
        if (skullTex != null) Destroy(skullTex);
        if (Instance == this) Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset timer state when scene reloads (quit to menu)
        timeAlive = 0f;
        isTracking = false;
        gameStartedThisSession = false;
    }

    void InitializeGUI()
    {
        // Background texture
        bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.7f));
        bgTex.Apply();

        // Create skull texture (simple pixel art skull)
        skullTex = CreateSkullTexture();

        timerStyle = new GUIStyle();
        timerStyle.fontSize = 18;
        timerStyle.fontStyle = FontStyle.Bold;
        timerStyle.alignment = TextAnchor.MiddleLeft;
        timerStyle.normal.textColor = Color.white;

        skullStyle = new GUIStyle();
        skullStyle.fontSize = 20;
        skullStyle.alignment = TextAnchor.MiddleCenter;

        dayStyle = new GUIStyle();
        dayStyle.fontSize = 14;
        dayStyle.fontStyle = FontStyle.Bold;
        dayStyle.alignment = TextAnchor.MiddleCenter;
        dayStyle.normal.textColor = new Color(0.9f, 0.85f, 0.6f); // Golden/amber color

        guiInitialized = true;
    }

    Texture2D CreateSkullTexture()
    {
        // Create a simple 16x16 skull icon
        int size = 16;
        Texture2D tex = new Texture2D(size, size);
        Color clear = new Color(0, 0, 0, 0);
        Color white = Color.white;
        Color gray = new Color(0.8f, 0.8f, 0.8f);

        // Fill with transparent
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, clear);

        // Draw skull shape (simplified)
        // Top of skull (rows 12-15)
        for (int x = 4; x <= 11; x++) tex.SetPixel(x, 14, white);
        for (int x = 3; x <= 12; x++) tex.SetPixel(x, 13, white);
        for (int x = 2; x <= 13; x++) tex.SetPixel(x, 12, white);

        // Middle skull (rows 7-11)
        for (int y = 7; y <= 11; y++)
        {
            for (int x = 2; x <= 13; x++) tex.SetPixel(x, y, white);
        }

        // Eye sockets (dark)
        tex.SetPixel(4, 10, clear); tex.SetPixel(5, 10, clear);
        tex.SetPixel(4, 9, clear); tex.SetPixel(5, 9, clear);
        tex.SetPixel(10, 10, clear); tex.SetPixel(11, 10, clear);
        tex.SetPixel(10, 9, clear); tex.SetPixel(11, 9, clear);

        // Nose (dark triangle)
        tex.SetPixel(7, 7, clear); tex.SetPixel(8, 7, clear);
        tex.SetPixel(7, 6, clear); tex.SetPixel(8, 6, clear);

        // Jaw (rows 3-6)
        for (int x = 3; x <= 12; x++) tex.SetPixel(x, 5, white);
        for (int x = 4; x <= 11; x++) tex.SetPixel(x, 4, white);
        for (int x = 4; x <= 11; x++) tex.SetPixel(x, 3, gray);

        // Teeth gaps
        tex.SetPixel(5, 4, clear); tex.SetPixel(7, 4, clear);
        tex.SetPixel(9, 4, clear); tex.SetPixel(11, 4, clear);
        tex.SetPixel(5, 3, clear); tex.SetPixel(7, 3, clear);
        tex.SetPixel(9, 3, clear); tex.SetPixel(11, 3, clear);

        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    void Update()
    {
        // Start tracking when game starts
        if (MainMenu.GameStarted && !gameStartedThisSession)
        {
            gameStartedThisSession = true;
            isTracking = true;
            timeAlive = 0f;
        }

        // Track time while alive
        if (isTracking && MainMenu.GameStarted)
        {
            // Check if player is dead
            if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead())
            {
                // Stop tracking while dead (will restart on respawn handled by OnPlayerDeath)
            }
            else
            {
                timeAlive += Time.deltaTime;
            }
        }
    }

    void OnPlayerDeath()
    {
        if (!isTracking) return;

        // Record the time
        float finalTime = timeAlive;
        Debug.Log($"Player died! Time alive: {FormatTime(finalTime)}");

        // Add to leaderboard
        AddToLeaderboard(finalTime);

        // Show notification
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"Survived: {FormatTime(finalTime)}", new Color(0.8f, 0.2f, 0.2f));
        }

        // Reset timer for next life
        timeAlive = 0f;
    }

    void AddToLeaderboard(float time)
    {
        bestTimes.Add(time);
        bestTimes.Sort((a, b) => b.CompareTo(a)); // Sort descending (longest first)

        // Keep only top entries
        while (bestTimes.Count > MAX_LEADERBOARD_ENTRIES)
        {
            bestTimes.RemoveAt(bestTimes.Count - 1);
        }

        SaveLeaderboard();
    }

    void SaveLeaderboard()
    {
        for (int i = 0; i < MAX_LEADERBOARD_ENTRIES; i++)
        {
            if (i < bestTimes.Count)
                PlayerPrefs.SetFloat($"TimeAlive_{i}", bestTimes[i]);
            else
                PlayerPrefs.SetFloat($"TimeAlive_{i}", 0f);
        }
        PlayerPrefs.SetFloat("LastTimeAlive", bestTimes.Count > 0 ? bestTimes[bestTimes.Count - 1] : 0f);
        PlayerPrefs.SetFloat("BestTimeAlive", bestTimes.Count > 0 ? bestTimes[0] : 0f);
        PlayerPrefs.Save();
    }

    void LoadLeaderboard()
    {
        bestTimes.Clear();
        for (int i = 0; i < MAX_LEADERBOARD_ENTRIES; i++)
        {
            float time = PlayerPrefs.GetFloat($"TimeAlive_{i}", 0f);
            if (time > 0f)
                bestTimes.Add(time);
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        if (!guiInitialized)
            InitializeGUI();

        DrawTimeAlive();
    }

    void DrawTimeAlive()
    {
        // Position in top left
        float x = 10;
        float y = 10;
        float width = 140;
        float height = 32;
        float dayHeight = 22;

        // Background for time alive
        GUI.DrawTexture(new Rect(x, y, width, height), bgTex);

        // Skull icon
        GUI.DrawTexture(new Rect(x + 6, y + 8, 16, 16), skullTex);

        // Time text
        string timeText = FormatTime(timeAlive);

        // Pulse red when time is low (first 30 seconds)
        if (timeAlive < 30f)
        {
            float pulse = 0.7f + Mathf.Sin(Time.time * 3f) * 0.3f;
            timerStyle.normal.textColor = new Color(1f, pulse * 0.5f, pulse * 0.5f);
        }
        else
        {
            timerStyle.normal.textColor = Color.white;
        }

        GUI.Label(new Rect(x + 28, y + 6, width - 35, height - 10), timeText, timerStyle);

        // Day counter display below time alive
        float dayY = y + height + 2;

        // Background for day display
        GUI.DrawTexture(new Rect(x, dayY, width, dayHeight), bgTex);

        // Get current day from DayNightCycle
        int currentDay = 1;
        if (DayNightCycle.Instance != null)
        {
            currentDay = DayNightCycle.Instance.GetCurrentDay();
        }

        // Day text
        string dayText = $"Day {currentDay}";
        GUI.Label(new Rect(x, dayY + 2, width, dayHeight - 4), dayText, dayStyle);
    }

    string FormatTime(float seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);

        if (mins >= 60)
        {
            int hours = mins / 60;
            mins = mins % 60;
            return $"{hours}:{mins:D2}:{secs:D2}";
        }
        return $"{mins}:{secs:D2}";
    }

    // Public getters for stats board
    public float GetCurrentTime() => timeAlive;
    public float GetBestTime() => bestTimes.Count > 0 ? bestTimes[0] : 0f;
    public List<float> GetLeaderboard() => new List<float>(bestTimes);

    // Set timer (called when loading a save)
    public void SetCurrentTime(float time)
    {
        timeAlive = time;
        isTracking = true;
    }

    // Reset timer (called when starting new game)
    public void ResetTimer()
    {
        timeAlive = 0f;
        isTracking = true;
    }
}
