using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Save System - Handles saving/loading game state for WebGL/itch.io
/// Uses PlayerPrefs which persists in browser IndexedDB
/// Supports backup codes for manual save/load
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    // Save data structure
    [Serializable]
    public class SaveData
    {
        // Player stats
        public int gold;
        public int xp;
        public int level;
        public float health;

        // Position
        public float posX;
        public float posY;
        public float posZ;

        // Time
        public float timeOfDay; // 0-1
        public int currentDay;
        public float timeAlive;

        // Inventory
        public List<string> fishInventory = new List<string>();
        public List<string> specialFishInventory = new List<string>();
        public int lunchBoxCount;
        public int lunchBoxFishCount;

        // Buffs inventory
        public int buffSnappersDelight;
        public int buffMarlinsLuck;
        public int buffTroutsFortune;
        public int buffSunshoreSurge;
        public int buffSnubnoseSpeed;
        public int buffSeahorsesBounty;

        // Clothing/Items purchased
        public List<string> purchasedClothing = new List<string>();
        public List<string> equippedClothing = new List<string>();

        // Achievements (IDs of unlocked achievements)
        public List<string> unlockedAchievements = new List<string>();

        // Fish Diary (discovered fish IDs)
        public List<string> discoveredFish = new List<string>();

        // Quests
        public int connoisseurCurrentQuest;
        public List<int> completedQuests = new List<int>();

        // Stats
        public int totalFishCaught;
        public int totalGoldEarned;
        public float bestTimeAlive;
        public int totalDeaths;

        // Metadata
        public string saveDate;
        public string version = "1.0";
    }

    // Current save data
    private SaveData currentSave;

    // Save exists flag
    public bool HasSaveData { get; private set; }

    // Screenshot texture (for continue screen)
    private Texture2D screenshotTexture;
    private bool hasScreenshot = false;

    // Auto-save timer
    private float autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 60f; // Save every 60 seconds

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CheckForExistingSave();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CheckForExistingSave()
    {
        HasSaveData = PlayerPrefs.HasKey("SaveData_Exists") && PlayerPrefs.GetInt("SaveData_Exists") == 1;

        if (HasSaveData)
        {
            LoadScreenshot();
        }
    }

    void Update()
    {
        // Auto-save while game is active
        if (MainMenu.GameStarted)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
            {
                autoSaveTimer = 0f;
                AutoSave();
            }
        }
    }

    /// <summary>
    /// Save the current game state
    /// </summary>
    public void SaveGame()
    {
        currentSave = new SaveData();

        // Player stats
        if (GameManager.Instance != null)
        {
            currentSave.gold = GameManager.Instance.coins;
            currentSave.totalFishCaught = GameManager.Instance.totalFishCaught;
        }

        if (LevelingSystem.Instance != null)
        {
            currentSave.xp = (int)LevelingSystem.Instance.GetCurrentXP();
            currentSave.level = LevelingSystem.Instance.GetCurrentLevel();
        }

        if (PlayerHealth.Instance != null)
        {
            currentSave.health = PlayerHealth.Instance.GetCurrentHealth();
        }

        // Position
        if (GameCache.IsPlayerValid())
        {
            currentSave.posX = GameCache.Player.position.x;
            currentSave.posY = GameCache.Player.position.y;
            currentSave.posZ = GameCache.Player.position.z;
        }

        // Time
        if (DayNightCycle.Instance != null)
        {
            currentSave.timeOfDay = DayNightCycle.Instance.GetTimeOfDay();
            currentSave.currentDay = DayNightCycle.Instance.GetCurrentDay();
        }

        if (TimeAliveTracker.Instance != null)
        {
            currentSave.timeAlive = TimeAliveTracker.Instance.GetCurrentTime();
            currentSave.bestTimeAlive = TimeAliveTracker.Instance.GetBestTime();
        }

        // Fish inventory
        if (FishingSystem.Instance != null)
        {
            foreach (var fish in FishingSystem.Instance.GetSpecialFishInventory())
            {
                currentSave.specialFishInventory.Add(fish.id);
            }
        }

        // Food inventory
        if (FoodInventory.Instance != null)
        {
            currentSave.lunchBoxCount = FoodInventory.Instance.lunchBoxCount;
            currentSave.lunchBoxFishCount = FoodInventory.Instance.lunchBoxFishCount;
        }

        // Buff inventory
        if (FishBuffSystem.Instance != null)
        {
            currentSave.buffSnappersDelight = FishBuffSystem.Instance.GetBuffCount(FishBuffType.SnappersDelight);
            currentSave.buffMarlinsLuck = FishBuffSystem.Instance.GetBuffCount(FishBuffType.MarlinsLuck);
            currentSave.buffTroutsFortune = FishBuffSystem.Instance.GetBuffCount(FishBuffType.TroutsFortune);
            currentSave.buffSunshoreSurge = FishBuffSystem.Instance.GetBuffCount(FishBuffType.SunshoreSurge);
            currentSave.buffSnubnoseSpeed = FishBuffSystem.Instance.GetBuffCount(FishBuffType.SnubnoseSpeed);
            currentSave.buffSeahorsesBounty = FishBuffSystem.Instance.GetBuffCount(FishBuffType.SeahorsesBounty);
        }

        // Achievements are saved separately in PlayerPrefs and persist across games

        // Stats from PlayerPrefs
        currentSave.totalGoldEarned = PlayerPrefs.GetInt("TotalGoldEarned", 0);
        currentSave.totalDeaths = PlayerPrefs.GetInt("Death_Total", 0);

        // Quests
        currentSave.connoisseurCurrentQuest = PlayerPrefs.GetInt("ConnoisseurCurrentQuest", -1);
        for (int i = 0; i < 10; i++)
        {
            if (PlayerPrefs.GetInt($"ConnoisseurQuest_{i}", 0) == 1)
            {
                currentSave.completedQuests.Add(i);
            }
        }

        // Clothing
        for (int i = 0; i < 20; i++)
        {
            string key = $"Clothing_Owned_{i}";
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                currentSave.purchasedClothing.Add(i.ToString());
            }
        }

        // Metadata
        currentSave.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        currentSave.version = "1.0";

        // Serialize and save
        string json = JsonUtility.ToJson(currentSave);
        PlayerPrefs.SetString("SaveData_JSON", json);
        PlayerPrefs.SetInt("SaveData_Exists", 1);
        PlayerPrefs.Save();

        // Take screenshot
        CaptureScreenshot();

        Debug.Log("Game saved successfully!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Game Saved!", new Color(0.3f, 1f, 0.5f));
        }
    }

    /// <summary>
    /// Load the saved game state
    /// </summary>
    public bool LoadGame()
    {
        if (!HasSaveData)
        {
            Debug.Log("No save data found!");
            return false;
        }

        string json = PlayerPrefs.GetString("SaveData_JSON", "");
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("Save data is empty!");
            return false;
        }

        try
        {
            currentSave = JsonUtility.FromJson<SaveData>(json);
            ApplySaveData();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load save: {e.Message}");
            return false;
        }
    }

    void ApplySaveData()
    {
        if (currentSave == null) return;

        // Player stats
        if (GameManager.Instance != null)
        {
            GameManager.Instance.coins = currentSave.gold;
            GameManager.Instance.totalFishCaught = currentSave.totalFishCaught;
        }

        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.SetXPAndLevel(currentSave.xp, currentSave.level);
        }

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.SetHealth(currentSave.health);
        }

        // Position
        if (GameCache.IsPlayerValid())
        {
            GameCache.Player.position = new Vector3(currentSave.posX, currentSave.posY, currentSave.posZ);
        }

        // Time (timeOfDay is stored as 0-1, SetTimeOfDay takes hours 0-24)
        if (DayNightCycle.Instance != null)
        {
            DayNightCycle.Instance.SetTimeOfDay(currentSave.timeOfDay * 24f);
            DayNightCycle.Instance.SetCurrentDay(currentSave.currentDay);
        }

        // Food inventory
        if (FoodInventory.Instance != null)
        {
            FoodInventory.Instance.lunchBoxCount = currentSave.lunchBoxCount;
            FoodInventory.Instance.lunchBoxFishCount = currentSave.lunchBoxFishCount;
        }

        // Buff inventory
        if (FishBuffSystem.Instance != null)
        {
            FishBuffSystem.Instance.SetBuffCount(FishBuffType.SnappersDelight, currentSave.buffSnappersDelight);
            FishBuffSystem.Instance.SetBuffCount(FishBuffType.MarlinsLuck, currentSave.buffMarlinsLuck);
            FishBuffSystem.Instance.SetBuffCount(FishBuffType.TroutsFortune, currentSave.buffTroutsFortune);
            FishBuffSystem.Instance.SetBuffCount(FishBuffType.SunshoreSurge, currentSave.buffSunshoreSurge);
            FishBuffSystem.Instance.SetBuffCount(FishBuffType.SnubnoseSpeed, currentSave.buffSnubnoseSpeed);
            FishBuffSystem.Instance.SetBuffCount(FishBuffType.SeahorsesBounty, currentSave.buffSeahorsesBounty);
        }

        // Restore PlayerPrefs stats
        PlayerPrefs.SetInt("TotalGoldEarned", currentSave.totalGoldEarned);
        PlayerPrefs.SetInt("Death_Total", currentSave.totalDeaths);
        PlayerPrefs.SetInt("ConnoisseurCurrentQuest", currentSave.connoisseurCurrentQuest);

        foreach (int quest in currentSave.completedQuests)
        {
            PlayerPrefs.SetInt($"ConnoisseurQuest_{quest}", 1);
        }

        // Clothing
        foreach (string clothingId in currentSave.purchasedClothing)
        {
            if (int.TryParse(clothingId, out int id))
            {
                PlayerPrefs.SetInt($"Clothing_Owned_{id}", 1);
            }
        }

        PlayerPrefs.Save();

        Debug.Log("Game loaded successfully!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Game Loaded!", new Color(0.3f, 0.8f, 1f));
        }
    }

    /// <summary>
    /// Delete all save data (for new game)
    /// </summary>
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey("SaveData_JSON");
        PlayerPrefs.DeleteKey("SaveData_Exists");
        PlayerPrefs.DeleteKey("SaveData_Screenshot");
        PlayerPrefs.Save();

        HasSaveData = false;
        currentSave = null;

        if (screenshotTexture != null)
        {
            Destroy(screenshotTexture);
            screenshotTexture = null;
        }
        hasScreenshot = false;

        Debug.Log("Save data deleted!");
    }

    /// <summary>
    /// Export save data as a backup code (base64)
    /// </summary>
    public string ExportSaveCode()
    {
        if (!HasSaveData)
        {
            // Save current state first
            SaveGame();
        }

        string json = PlayerPrefs.GetString("SaveData_JSON", "");
        if (string.IsNullOrEmpty(json)) return "";

        byte[] bytes = Encoding.UTF8.GetBytes(json);
        string base64 = Convert.ToBase64String(bytes);

        // Add a simple checksum
        int checksum = 0;
        foreach (char c in json)
        {
            checksum += (int)c;
        }
        checksum = checksum % 10000;

        return $"FISH-{base64}-{checksum:D4}";
    }

    /// <summary>
    /// Import save data from a backup code
    /// </summary>
    public bool ImportSaveCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return false;

        try
        {
            // Remove prefix and checksum
            if (!code.StartsWith("FISH-")) return false;

            string[] parts = code.Split('-');
            if (parts.Length != 3) return false;

            string base64 = parts[1];
            int providedChecksum = int.Parse(parts[2]);

            byte[] bytes = Convert.FromBase64String(base64);
            string json = Encoding.UTF8.GetString(bytes);

            // Verify checksum
            int checksum = 0;
            foreach (char c in json)
            {
                checksum += (int)c;
            }
            checksum = checksum % 10000;

            if (checksum != providedChecksum)
            {
                Debug.LogError("Save code checksum mismatch!");
                return false;
            }

            // Validate JSON by parsing
            SaveData testSave = JsonUtility.FromJson<SaveData>(json);
            if (testSave == null) return false;

            // Save to PlayerPrefs
            PlayerPrefs.SetString("SaveData_JSON", json);
            PlayerPrefs.SetInt("SaveData_Exists", 1);
            PlayerPrefs.Save();

            HasSaveData = true;
            currentSave = testSave;

            Debug.Log("Save code imported successfully!");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to import save code: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Capture a screenshot for the save file
    /// </summary>
    void CaptureScreenshot()
    {
        // Create a small screenshot (160x90 for 16:9)
        int width = 160;
        int height = 90;

        RenderTexture rt = new RenderTexture(width, height, 24);

        if (Camera.main != null)
        {
            Camera.main.targetTexture = rt;
            Camera.main.Render();

            RenderTexture.active = rt;

            if (screenshotTexture != null)
            {
                Destroy(screenshotTexture);
            }

            screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshotTexture.Apply();

            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            // Save screenshot as base64 PNG
            byte[] pngBytes = screenshotTexture.EncodeToPNG();
            string base64 = Convert.ToBase64String(pngBytes);
            PlayerPrefs.SetString("SaveData_Screenshot", base64);
            PlayerPrefs.Save();

            hasScreenshot = true;
        }
    }

    /// <summary>
    /// Load the saved screenshot
    /// </summary>
    void LoadScreenshot()
    {
        string base64 = PlayerPrefs.GetString("SaveData_Screenshot", "");
        if (string.IsNullOrEmpty(base64)) return;

        try
        {
            byte[] pngBytes = Convert.FromBase64String(base64);

            if (screenshotTexture != null)
            {
                Destroy(screenshotTexture);
            }

            screenshotTexture = new Texture2D(2, 2);
            screenshotTexture.LoadImage(pngBytes);
            hasScreenshot = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load screenshot: {e.Message}");
            hasScreenshot = false;
        }
    }

    /// <summary>
    /// Get the saved screenshot texture
    /// </summary>
    public Texture2D GetScreenshot()
    {
        return hasScreenshot ? screenshotTexture : null;
    }

    /// <summary>
    /// Get save info for display
    /// </summary>
    public (float timeAlive, int day, float timeOfDay, string saveDate) GetSaveInfo()
    {
        if (!HasSaveData) return (0, 1, 0.5f, "");

        string json = PlayerPrefs.GetString("SaveData_JSON", "");
        if (string.IsNullOrEmpty(json)) return (0, 1, 0.5f, "");

        try
        {
            SaveData save = JsonUtility.FromJson<SaveData>(json);
            return (save.timeAlive, save.currentDay, save.timeOfDay, save.saveDate);
        }
        catch
        {
            return (0, 1, 0.5f, "");
        }
    }

    /// <summary>
    /// Auto-save (call periodically or at key moments)
    /// </summary>
    public void AutoSave()
    {
        if (MainMenu.GameStarted)
        {
            SaveGame();
        }
    }

    void OnDestroy()
    {
        if (screenshotTexture != null)
        {
            Destroy(screenshotTexture);
        }
    }
}
