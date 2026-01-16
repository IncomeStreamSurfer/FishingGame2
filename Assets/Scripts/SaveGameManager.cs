using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// SaveGameManager - Handles saving and loading game state with screenshots
/// Saves player data (gold, fish inventory, XP/level, cosmetics) to JSON
/// Captures and stores screenshots as thumbnails for save slots
/// </summary>
public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance { get; private set; }

    // Save file constants
    private const int MAX_SAVE_SLOTS = 3;
    private const string SAVE_FILE_PREFIX = "savegame_";
    private const string SAVE_FILE_EXTENSION = ".json";
    private const string SCREENSHOT_PREFIX = "screenshot_";
    private const string SCREENSHOT_EXTENSION = ".png";

    // Cached screenshot for pending save
    private Texture2D pendingScreenshot;
    private int pendingSlot = -1;

    // Loaded save thumbnails
    private Dictionary<int, Texture2D> thumbnailCache = new Dictionary<int, Texture2D>();

    // Event for save completion
    public event Action<int, bool> OnSaveComplete; // slot, success
    public event Action<int, bool> OnLoadComplete; // slot, success

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
        }
    }

    void Start()
    {
        // Pre-load thumbnails for all slots
        LoadAllThumbnails();
    }

    /// <summary>
    /// Initiates a save game with screenshot capture
    /// Call this when player clicks SAVE GAME button
    /// </summary>
    public void InitiateSave(int slot)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid save slot: {slot}");
            return;
        }

        pendingSlot = slot;
        StartCoroutine(CaptureScreenshotAndSave(slot));
    }

    /// <summary>
    /// Coroutine to capture screenshot at end of frame and then save
    /// </summary>
    private IEnumerator CaptureScreenshotAndSave(int slot)
    {
        // Wait for end of frame to capture the current game view
        yield return new WaitForEndOfFrame();

        // Capture the screen
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // Create a thumbnail (smaller version for the save slot display)
        int thumbWidth = 192;
        int thumbHeight = 108;
        Texture2D thumbnail = ScaleTexture(screenshot, thumbWidth, thumbHeight);

        // Destroy the full-size screenshot
        Destroy(screenshot);

        // Save the thumbnail to disk
        SaveScreenshot(slot, thumbnail);

        // Cache the thumbnail for display
        if (thumbnailCache.ContainsKey(slot))
        {
            Destroy(thumbnailCache[slot]);
        }
        thumbnailCache[slot] = thumbnail;

        // Now save the game data
        bool success = SaveGameData(slot);

        // Notify listeners
        OnSaveComplete?.Invoke(slot, success);

        if (success)
        {
            Debug.Log($"Game saved successfully to slot {slot}");
        }
        else
        {
            Debug.LogError($"Failed to save game to slot {slot}");
        }

        pendingSlot = -1;
    }

    /// <summary>
    /// Scale a texture to a new size (for thumbnail creation)
    /// </summary>
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float u = (float)x / targetWidth;
                float v = (float)y / targetHeight;
                Color pixel = source.GetPixelBilinear(u, v);
                result.SetPixel(x, y, pixel);
            }
        }

        result.Apply();
        return result;
    }

    /// <summary>
    /// Save the screenshot thumbnail to disk
    /// </summary>
    private void SaveScreenshot(int slot, Texture2D thumbnail)
    {
        string path = GetScreenshotPath(slot);
        byte[] pngData = thumbnail.EncodeToPNG();

        try
        {
            File.WriteAllBytes(path, pngData);
            Debug.Log($"Screenshot saved to: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save screenshot: {e.Message}");
        }
    }

    /// <summary>
    /// Save all game data to a JSON file
    /// </summary>
    private bool SaveGameData(int slot)
    {
        try
        {
            SaveData data = new SaveData();

            // Save name and timestamp
            data.saveName = $"Save {slot + 1}";
            data.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.playTime = GetPlayTimeString();

            // Save gold and fish
            if (GameManager.Instance != null)
            {
                data.gold = GameManager.Instance.coins;
                data.totalFishCaught = GameManager.Instance.totalFishCaught;

                // Convert fish inventory to serializable format
                data.fishInventory = new List<FishInventoryEntry>();
                foreach (var kvp in GameManager.Instance.fishInventory)
                {
                    data.fishInventory.Add(new FishInventoryEntry { fishId = kvp.Key, count = kvp.Value });
                }
            }

            // Save XP and level
            if (LevelingSystem.Instance != null)
            {
                data.xp = LevelingSystem.Instance.GetCurrentXP();
                data.level = LevelingSystem.Instance.GetLevel();
            }

            // Save player position
            if (GameCache.IsPlayerValid())
            {
                Vector3 pos = GameCache.Player.position;
                data.posX = pos.x;
                data.posY = pos.y;
                data.posZ = pos.z;
            }

            // Save health
            if (PlayerHealth.Instance != null)
            {
                data.health = PlayerHealth.Instance.GetCurrentHealth();
                data.maxHealth = PlayerHealth.Instance.GetMaxHealth();
            }

            // Save cosmetics/equipment from CharacterPanel
            if (CharacterPanel.Instance != null)
            {
                data.equipment = new List<string>();
                // We'll save what's equipped - CharacterPanel has equippedItems array
                // For now, store the equipment slots
            }

            // Save accessories
            if (AccessorySystem.Instance != null)
            {
                data.ownedAccessories = new List<string>();
                data.equippedAccessories = new Dictionary<string, string>();

                foreach (var acc in AccessorySystem.Instance.GetOwnedAccessories())
                {
                    data.ownedAccessories.Add(acc.name);
                }

                foreach (var kvp in AccessorySystem.Instance.GetEquippedAccessories())
                {
                    data.equippedAccessories[kvp.Key] = kvp.Value.name;
                }
            }

            // Serialize to JSON
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(slot);
            File.WriteAllText(path, json);

            Debug.Log($"Save data written to: {path}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game data: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load a saved game from a slot
    /// </summary>
    public bool LoadGame(int slot)
    {
        if (!HasSaveData(slot))
        {
            Debug.LogWarning($"No save data in slot {slot}");
            OnLoadComplete?.Invoke(slot, false);
            return false;
        }

        try
        {
            string path = GetSavePath(slot);
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Load gold and fish
            if (GameManager.Instance != null)
            {
                GameManager.Instance.coins = data.gold;
                GameManager.Instance.totalFishCaught = data.totalFishCaught;

                // Restore fish inventory
                GameManager.Instance.fishInventory.Clear();
                if (data.fishInventory != null)
                {
                    foreach (var entry in data.fishInventory)
                    {
                        GameManager.Instance.fishInventory[entry.fishId] = entry.count;
                    }
                }
            }

            // Load player position
            if (GameCache.IsPlayerValid())
            {
                GameCache.Player.position = new Vector3(data.posX, data.posY, data.posZ);
            }

            // Load XP and Level
            if (LevelingSystem.Instance != null)
            {
                LevelingSystem.Instance.SetXPAndLevel(data.xp, data.level);
            }

            // Recalculate max health after loading level (health bonus depends on level)
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.RecalculateMaxHealth();
                PlayerHealth.Instance.SetHealth(data.health);
            }

            Debug.Log($"Loaded game from slot {slot}: Level {data.level}, {data.gold} Gold, {data.xp} XP");

            OnLoadComplete?.Invoke(slot, true);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            OnLoadComplete?.Invoke(slot, false);
            return false;
        }
    }

    /// <summary>
    /// Check if a save slot has data
    /// </summary>
    public bool HasSaveData(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    /// <summary>
    /// Get save data info for display (without loading the full save)
    /// </summary>
    public SaveData GetSaveInfo(int slot)
    {
        if (!HasSaveData(slot))
        {
            return null;
        }

        try
        {
            string path = GetSavePath(slot);
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read save info: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get the thumbnail for a save slot
    /// </summary>
    public Texture2D GetThumbnail(int slot)
    {
        if (thumbnailCache.ContainsKey(slot))
        {
            return thumbnailCache[slot];
        }
        return null;
    }

    /// <summary>
    /// Delete a save slot
    /// </summary>
    public void DeleteSave(int slot)
    {
        string savePath = GetSavePath(slot);
        string screenshotPath = GetScreenshotPath(slot);

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }

        if (File.Exists(screenshotPath))
        {
            File.Delete(screenshotPath);
        }

        if (thumbnailCache.ContainsKey(slot))
        {
            Destroy(thumbnailCache[slot]);
            thumbnailCache.Remove(slot);
        }

        Debug.Log($"Deleted save slot {slot}");
    }

    /// <summary>
    /// Load all thumbnails from disk
    /// </summary>
    private void LoadAllThumbnails()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            LoadThumbnail(i);
        }
    }

    /// <summary>
    /// Load a single thumbnail from disk
    /// </summary>
    private void LoadThumbnail(int slot)
    {
        string path = GetScreenshotPath(slot);

        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            byte[] pngData = File.ReadAllBytes(path);
            Texture2D thumbnail = new Texture2D(192, 108, TextureFormat.RGB24, false);
            thumbnail.LoadImage(pngData);

            if (thumbnailCache.ContainsKey(slot))
            {
                Destroy(thumbnailCache[slot]);
            }
            thumbnailCache[slot] = thumbnail;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load thumbnail for slot {slot}: {e.Message}");
        }
    }

    /// <summary>
    /// Get the file path for a save slot
    /// </summary>
    private string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"{SAVE_FILE_PREFIX}{slot}{SAVE_FILE_EXTENSION}");
    }

    /// <summary>
    /// Get the screenshot path for a save slot
    /// </summary>
    private string GetScreenshotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"{SCREENSHOT_PREFIX}{slot}{SCREENSHOT_EXTENSION}");
    }

    /// <summary>
    /// Get formatted play time string
    /// </summary>
    private string GetPlayTimeString()
    {
        // For now, return time since game started this session
        // A full implementation would track total play time across sessions
        float playTime = Time.timeSinceLevelLoad;
        int hours = (int)(playTime / 3600);
        int minutes = (int)((playTime % 3600) / 60);
        int seconds = (int)(playTime % 60);

        if (hours > 0)
        {
            return $"{hours}:{minutes:D2}:{seconds:D2}";
        }
        return $"{minutes}:{seconds:D2}";
    }

    void OnDestroy()
    {
        // Clean up cached textures
        foreach (var tex in thumbnailCache.Values)
        {
            if (tex != null)
            {
                Destroy(tex);
            }
        }
        thumbnailCache.Clear();
    }
}

/// <summary>
/// Serializable save data structure
/// </summary>
[System.Serializable]
public class SaveData
{
    // Meta info
    public string saveName;
    public string saveTimestamp;
    public string playTime;

    // Player stats
    public int gold;
    public long xp;
    public int level;
    public int totalFishCaught;
    public float health;
    public float maxHealth;

    // Position
    public float posX;
    public float posY;
    public float posZ;

    // Inventory
    public List<FishInventoryEntry> fishInventory;

    // Equipment/Cosmetics
    public List<string> equipment;
    public List<string> ownedAccessories;
    public Dictionary<string, string> equippedAccessories;
}

/// <summary>
/// Fish inventory entry for serialization
/// </summary>
[System.Serializable]
public class FishInventoryEntry
{
    public string fishId;
    public int count;
}
