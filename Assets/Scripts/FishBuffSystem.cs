using UnityEngine;
using System.Collections.Generic;

public enum FishBuffType
{
    None,
    SnappersDelight,    // Red Snapper - No health loss for 5 min
    MarlinsLuck,        // Blue Marlin - Rare fish chance +50% for 5 min
    TroutsFortune,      // Rainbow Trout - +50% gold from fish for 5 min
    SunshoreSurge,      // Sunshore Cod - +50% XP for 5 min
    SnubnoseSpeed,      // Icelandic Snubnose - +25% movement speed for 5 min
    SeahorsesBounty,    // Seahorse - Double fish catches for 5 min
    Poisoned            // Debuff - 1 damage per second for 10 seconds
}

[System.Serializable]
public class FishBuff
{
    public FishBuffType type;
    public string buffName;
    public string description;
    public string requiredFishId;
    public string requiredFishName;
    public float duration;
    public Color bowlColor;
    public bool isUnlocked;
    public int quantity;

    public FishBuff(FishBuffType type, string buffName, string description, string fishId, string fishName, float duration, Color bowlColor)
    {
        this.type = type;
        this.buffName = buffName;
        this.description = description;
        this.requiredFishId = fishId;
        this.requiredFishName = fishName;
        this.duration = duration;
        this.bowlColor = bowlColor;
        this.isUnlocked = false;
        this.quantity = 0;
    }
}

[System.Serializable]
public class ActiveBuff
{
    public FishBuffType type;
    public float remainingTime;
    public string buffName;

    public ActiveBuff(FishBuffType type, float duration, string name)
    {
        this.type = type;
        this.remainingTime = duration;
        this.buffName = name;
    }
}

public class FishBuffSystem : MonoBehaviour
{
    public static FishBuffSystem Instance { get; private set; }

    // All available fish buffs
    public List<FishBuff> allBuffs = new List<FishBuff>();

    // Currently active buffs
    public List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    // Buff inventory (unlocked buffs with quantities)
    public Dictionary<FishBuffType, int> buffInventory = new Dictionary<FishBuffType, int>();

    // Quest completion tracking
    public Dictionary<string, bool> completedQuests = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeBuffs();
            LoadBuffData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeBuffs()
    {
        // Red Snapper - No health loss
        allBuffs.Add(new FishBuff(
            FishBuffType.SnappersDelight,
            "Snapper's Delight",
            "No health loss for 5 minutes!",
            "red_snapper",
            "Red Snapper",
            300f, // 5 minutes
            new Color(0.9f, 0.3f, 0.3f) // Red bowl
        ));

        // Blue Marlin - Rare fish chance
        allBuffs.Add(new FishBuff(
            FishBuffType.MarlinsLuck,
            "Marlin's Luck",
            "+50% rare fish chance for 5 minutes!",
            "blue_marlin",
            "Blue Marlin",
            300f,
            new Color(0.3f, 0.5f, 0.9f) // Blue bowl
        ));

        // Rainbow Trout - Gold bonus
        allBuffs.Add(new FishBuff(
            FishBuffType.TroutsFortune,
            "Trout's Fortune",
            "+50% gold from selling fish for 5 minutes!",
            "rainbow_trout",
            "Rainbow Trout",
            300f,
            new Color(1f, 0.85f, 0.2f) // Yellow/Gold bowl
        ));

        // Sunshore Cod - XP bonus
        allBuffs.Add(new FishBuff(
            FishBuffType.SunshoreSurge,
            "Sunshore Surge",
            "+50% XP from all sources for 5 minutes!",
            "sunshore_od",
            "Sunshore Cod",
            300f,
            new Color(1f, 0.6f, 0.2f) // Orange bowl
        ));

        // Icelandic Snubnose - Speed boost
        allBuffs.Add(new FishBuff(
            FishBuffType.SnubnoseSpeed,
            "Snubnose Speed",
            "+25% movement speed for 5 minutes!",
            "icelandic_snubnose",
            "Icelandic Grey Finned Snubnose",
            300f,
            new Color(0.7f, 0.75f, 0.8f) // Silver/Grey bowl
        ));

        // Seahorse - Double catches
        allBuffs.Add(new FishBuff(
            FishBuffType.SeahorsesBounty,
            "Seahorse's Bounty",
            "Double fish catches for 5 minutes!",
            "seahorse",
            "Seahorse",
            300f,
            new Color(0.3f, 0.8f, 0.4f) // Green bowl
        ));

        // Initialize inventory for each buff type
        foreach (var buff in allBuffs)
        {
            buffInventory[buff.type] = 0;
        }
    }

    // Cached textures and styles for OnGUI
    private Texture2D buffBgTex;
    private Texture2D buffBarTex;
    private GUIStyle buffLabelStyle;
    private bool guiInitialized = false;

    // Poison damage tracking
    private float poisonDamageTimer = 0f;

    void Update()
    {
        // Update active buff timers
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].remainingTime -= Time.deltaTime;

            // Apply poison damage (1 HP per second)
            if (activeBuffs[i].type == FishBuffType.Poisoned)
            {
                poisonDamageTimer += Time.deltaTime;
                if (poisonDamageTimer >= 1f)
                {
                    poisonDamageTimer = 0f;
                    if (PlayerHealth.Instance != null)
                    {
                        // Bypass health protection - poison cannot be prevented!
                        PlayerHealth.Instance.TakeDamage(1f, "", true);
                    }
                }
            }

            if (activeBuffs[i].remainingTime <= 0)
            {
                Debug.Log($"Buff expired: {activeBuffs[i].buffName}");
                if (UIManager.Instance != null)
                {
                    string message = activeBuffs[i].type == FishBuffType.Poisoned
                        ? "Poison has worn off!"
                        : $"{activeBuffs[i].buffName} has worn off!";
                    UIManager.Instance.ShowLootNotification(message, new Color(0.7f, 0.7f, 0.7f));
                }

                // Reset poison timer when poison expires
                if (activeBuffs[i].type == FishBuffType.Poisoned)
                {
                    poisonDamageTimer = 0f;
                }

                activeBuffs.RemoveAt(i);
            }
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (activeBuffs.Count == 0) return;

        // Initialize GUI resources once
        if (!guiInitialized)
        {
            buffBgTex = new Texture2D(1, 1);
            buffBgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.85f));
            buffBgTex.Apply();

            buffBarTex = new Texture2D(1, 1);
            buffBarTex.SetPixel(0, 0, new Color(0.3f, 0.8f, 0.4f, 0.9f));
            buffBarTex.Apply();

            buffLabelStyle = new GUIStyle(GUI.skin.label);
            guiInitialized = true;
        }

        // Draw active buffs on right side, below vital signs (HP bar + ECG is ~70px)
        float panelX = Screen.width - 180;
        float panelY = 75; // Below ECG monitor
        float buffHeight = 38;
        float buffWidth = 170;

        for (int i = 0; i < activeBuffs.Count; i++)
        {
            ActiveBuff buff = activeBuffs[i];

            float y = panelY + i * (buffHeight + 4);

            // Special handling for poison debuff
            if (buff.type == FishBuffType.Poisoned)
            {
                // Background
                GUI.DrawTexture(new Rect(panelX, y, buffWidth, buffHeight), buffBgTex);

                // Timer bar (green for poison)
                float pct = buff.remainingTime / 10f; // 10 second duration
                Color poisonColor = new Color(0.3f, 1f, 0.3f); // Bright green
                GUI.color = poisonColor;
                GUI.DrawTexture(new Rect(panelX + 2, y + buffHeight - 6, (buffWidth - 4) * pct, 4), buffBarTex);
                GUI.color = Color.white;

                // Buff name
                buffLabelStyle.fontSize = 11;
                buffLabelStyle.fontStyle = FontStyle.Bold;
                buffLabelStyle.normal.textColor = poisonColor;
                buffLabelStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(panelX + 6, y + 3, buffWidth - 12, 16), "POISONED", buffLabelStyle);

                // Time remaining
                int secs = Mathf.CeilToInt(buff.remainingTime);
                string timeStr = $"{secs}s";

                buffLabelStyle.fontSize = 10;
                buffLabelStyle.fontStyle = FontStyle.Normal;
                buffLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
                GUI.Label(new Rect(panelX + 6, y + 18, buffWidth - 12, 14), timeStr, buffLabelStyle);
            }
            else
            {
                // Normal buff display
                FishBuff data = GetBuffData(buff.type);
                if (data == null) continue;

                // Background
                GUI.DrawTexture(new Rect(panelX, y, buffWidth, buffHeight), buffBgTex);

                // Timer bar
                float pct = buff.remainingTime / data.duration;
                GUI.color = data.bowlColor;
                GUI.DrawTexture(new Rect(panelX + 2, y + buffHeight - 6, (buffWidth - 4) * pct, 4), buffBarTex);
                GUI.color = Color.white;

                // Buff name
                buffLabelStyle.fontSize = 11;
                buffLabelStyle.fontStyle = FontStyle.Bold;
                buffLabelStyle.normal.textColor = data.bowlColor;
                buffLabelStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(panelX + 6, y + 3, buffWidth - 12, 16), buff.buffName, buffLabelStyle);

                // Time remaining
                int mins = (int)(buff.remainingTime / 60);
                int secs = (int)(buff.remainingTime % 60);
                string timeStr = mins > 0 ? $"{mins}m {secs}s" : $"{secs}s";

                buffLabelStyle.fontSize = 10;
                buffLabelStyle.fontStyle = FontStyle.Normal;
                buffLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
                GUI.Label(new Rect(panelX + 6, y + 18, buffWidth - 12, 14), timeStr, buffLabelStyle);
            }
        }
    }

    void OnDestroy()
    {
        if (buffBgTex != null) Destroy(buffBgTex);
        if (buffBarTex != null) Destroy(buffBarTex);
        if (Instance == this) Instance = null;
    }

    // Check if a specific buff is active
    public bool IsBuffActive(FishBuffType type)
    {
        foreach (var buff in activeBuffs)
        {
            if (buff.type == type) return true;
        }
        return false;
    }

    // Get remaining time for a buff
    public float GetBuffRemainingTime(FishBuffType type)
    {
        foreach (var buff in activeBuffs)
        {
            if (buff.type == type) return buff.remainingTime;
        }
        return 0f;
    }

    // Activate a buff from inventory
    public bool ActivateBuff(FishBuffType type)
    {
        if (!buffInventory.ContainsKey(type) || buffInventory[type] <= 0)
        {
            Debug.Log($"No {type} buffs in inventory!");
            return false;
        }

        // Check if already active
        if (IsBuffActive(type))
        {
            Debug.Log($"{type} is already active!");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Buff already active!", new Color(1f, 0.6f, 0.3f));
            }
            return false;
        }

        // Find the buff data
        FishBuff buffData = GetBuffData(type);
        if (buffData == null) return false;

        // Consume one from inventory
        buffInventory[type]--;
        SaveBuffData();

        // Activate the buff
        activeBuffs.Add(new ActiveBuff(type, buffData.duration, buffData.buffName));

        Debug.Log($"Activated buff: {buffData.buffName} for {buffData.duration} seconds");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"{buffData.buffName} activated!", buffData.bowlColor);
        }

        return true;
    }

    // Add buff to inventory (quest reward)
    public void AddBuffToInventory(FishBuffType type, int count = 1)
    {
        if (!buffInventory.ContainsKey(type))
            buffInventory[type] = 0;

        buffInventory[type] += count;

        // Mark as unlocked
        FishBuff buff = GetBuffData(type);
        if (buff != null)
        {
            buff.isUnlocked = true;
        }

        SaveBuffData();
        Debug.Log($"Added {count}x {type} to buff inventory. Total: {buffInventory[type]}");
    }

    // Get buff count in inventory
    public int GetBuffCount(FishBuffType type)
    {
        return buffInventory.ContainsKey(type) ? buffInventory[type] : 0;
    }

    // Get buff data by type
    public FishBuff GetBuffData(FishBuffType type)
    {
        foreach (var buff in allBuffs)
        {
            if (buff.type == type) return buff;
        }
        return null;
    }

    // Get buff data by required fish ID
    public FishBuff GetBuffByFishId(string fishId)
    {
        foreach (var buff in allBuffs)
        {
            if (buff.requiredFishId == fishId) return buff;
        }
        return null;
    }

    // Check if player has the required fish in their special inventory
    public bool HasRequiredFish(string fishId)
    {
        if (FishingSystem.Instance == null) return false;

        foreach (var fish in FishingSystem.Instance.specialFishInventory)
        {
            if (fish.id == fishId) return true;
        }
        return false;
    }

    // Remove fish from special inventory (when completing quest)
    public bool ConsumeFish(string fishId)
    {
        if (FishingSystem.Instance == null) return false;

        var inventory = FishingSystem.Instance.specialFishInventory;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].id == fishId)
            {
                inventory.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    // Complete a quest - give buff reward
    public void CompleteQuest(string fishId)
    {
        FishBuff buff = GetBuffByFishId(fishId);
        if (buff == null) return;

        // Mark quest as completed
        completedQuests[fishId] = true;

        // Add buff to inventory
        AddBuffToInventory(buff.type, 1);

        // Award XP
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.AddXP(2000);
        }

        SaveBuffData();

        Debug.Log($"Quest completed! Earned {buff.buffName} buff and 2000 XP!");
    }

    // Check if quest is completed
    public bool IsQuestCompleted(string fishId)
    {
        return completedQuests.ContainsKey(fishId) && completedQuests[fishId];
    }

    // ========== BUFF EFFECT HELPERS ==========

    // Gold multiplier (for Trout's Fortune)
    public float GetGoldMultiplier()
    {
        return IsBuffActive(FishBuffType.TroutsFortune) ? 1.5f : 1f;
    }

    // XP multiplier (for Sunshore Surge)
    public float GetXPMultiplier()
    {
        return IsBuffActive(FishBuffType.SunshoreSurge) ? 1.5f : 1f;
    }

    // Speed multiplier (for Snubnose Speed)
    public float GetSpeedMultiplier()
    {
        return IsBuffActive(FishBuffType.SnubnoseSpeed) ? 1.25f : 1f;
    }

    // Rare fish chance bonus (for Marlin's Luck)
    public float GetRareFishBonus()
    {
        return IsBuffActive(FishBuffType.MarlinsLuck) ? 0.5f : 0f; // +50% additive
    }

    // Health protection (for Snapper's Delight)
    public bool HasHealthProtection()
    {
        return IsBuffActive(FishBuffType.SnappersDelight);
    }

    // Double catch (for Seahorse's Bounty)
    public bool HasDoubleCatch()
    {
        return IsBuffActive(FishBuffType.SeahorsesBounty);
    }

    // Apply poison debuff (5% chance when eating fish)
    public void ApplyPoison()
    {
        // Check if already poisoned
        if (IsBuffActive(FishBuffType.Poisoned))
        {
            Debug.Log("Already poisoned! Cannot stack poison.");
            return;
        }

        // Add poison debuff (10 seconds, 1 damage per second)
        activeBuffs.Add(new ActiveBuff(FishBuffType.Poisoned, 10f, "POISONED"));
        poisonDamageTimer = 0f;

        Debug.Log("Player has been POISONED! Will take 1 damage per second for 10 seconds.");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("You've been POISONED!", new Color(0.3f, 1f, 0.3f));
        }
    }

    // Clear all active buffs (called on player death)
    public void ClearAllActiveBuffs()
    {
        if (activeBuffs.Count > 0)
        {
            Debug.Log($"Clearing {activeBuffs.Count} active buffs due to death!");
            activeBuffs.Clear();
            poisonDamageTimer = 0f; // Reset poison timer

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("All buffs lost!", new Color(0.8f, 0.3f, 0.3f));
            }
        }
    }

    // ========== SAVE/LOAD ==========

    void SaveBuffData()
    {
        // Save buff inventory
        foreach (var kvp in buffInventory)
        {
            PlayerPrefs.SetInt($"BuffInv_{kvp.Key}", kvp.Value);
        }

        // Save completed quests
        foreach (var kvp in completedQuests)
        {
            PlayerPrefs.SetInt($"Quest_{kvp.Key}", kvp.Value ? 1 : 0);
        }

        // Save unlocked status
        foreach (var buff in allBuffs)
        {
            PlayerPrefs.SetInt($"BuffUnlocked_{buff.type}", buff.isUnlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    void LoadBuffData()
    {
        // Load buff inventory
        foreach (var buff in allBuffs)
        {
            buffInventory[buff.type] = PlayerPrefs.GetInt($"BuffInv_{buff.type}", 0);
            buff.isUnlocked = PlayerPrefs.GetInt($"BuffUnlocked_{buff.type}", 0) == 1;
        }

        // Load completed quests
        string[] questFishIds = { "red_snapper", "blue_marlin", "rainbow_trout", "sunshore_od", "icelandic_snubnose", "seahorse" };
        foreach (string fishId in questFishIds)
        {
            completedQuests[fishId] = PlayerPrefs.GetInt($"Quest_{fishId}", 0) == 1;
        }
    }
}
