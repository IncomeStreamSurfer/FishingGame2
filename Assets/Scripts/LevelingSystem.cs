using UnityEngine;
using System;

public class LevelingSystem : MonoBehaviour
{
    public static LevelingSystem Instance { get; private set; }

    // Total XP cap is 100,000,000 at level 399
    public const int MAX_LEVEL = 399;
    public const long MAX_XP = 100000000;

    private long currentXP = 0;
    private int currentLevel = 1;

    // Bonus levels from equipment (e.g., Groovy Marlin Ring)
    private int bonusLevels = 0;

    // XP table - OSRS style exponential scaling
    private long[] xpTable;

    // Events
    public event Action<int, int> OnLevelUp; // oldLevel, newLevel
    public event Action<long, long> OnXPGain; // amount, totalXP

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            GenerateXPTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void GenerateXPTable()
    {
        // Proper leveling: Level 1 at 0 XP, Level 399 at 100,000,000 XP
        // Each level requires progressively more XP (exponential growth)
        xpTable = new long[MAX_LEVEL + 2]; // +2 for safety
        xpTable[1] = 0; // Level 1 starts at 0 XP

        // Calculate XP thresholds for each level using smooth exponential curve
        // Formula: XP for level L = base * (growth^(L-1) - 1) / (growth - 1)
        // We want level 2 to be achievable quickly, level 399 to require 100M total

        // Using a curve where early levels are fast, later levels are slow
        // Level 2: ~83 XP, Level 10: ~2,500 XP, Level 50: ~125,000 XP, Level 399: 100,000,000 XP

        double baseXP = 83.0; // XP needed for level 2
        double growthRate = 1.0175; // ~1.75% more XP per level

        // Calculate the scale factor to hit exactly 100M at level 399
        double rawTotal = 0;
        for (int i = 2; i <= MAX_LEVEL; i++)
        {
            rawTotal += baseXP * Math.Pow(growthRate, i - 2);
        }
        double scaleFactor = MAX_XP / rawTotal;

        // Now build the actual XP table
        long cumulativeXP = 0;
        for (int level = 2; level <= MAX_LEVEL; level++)
        {
            double xpForThisLevel = baseXP * Math.Pow(growthRate, level - 2) * scaleFactor;
            cumulativeXP += (long)Math.Ceiling(xpForThisLevel);
            xpTable[level] = cumulativeXP;
        }

        // Ensure max level is exactly MAX_XP
        xpTable[MAX_LEVEL] = MAX_XP;

        // Debug: Log some milestone levels
        Debug.Log($"XP Table Generated - Lvl 2: {xpTable[2]}, Lvl 10: {xpTable[10]}, Lvl 50: {xpTable[50]}, Lvl 100: {xpTable[100]}, Lvl 399: {xpTable[399]}");
    }

    public void AddXP(long amount)
    {
        if (amount <= 0) return;
        if (currentXP >= MAX_XP) return;

        long oldXP = currentXP;
        currentXP = Math.Min(currentXP + amount, MAX_XP);

        OnXPGain?.Invoke(amount, currentXP);

        // Check for level up
        int oldLevel = currentLevel;
        UpdateLevel();

        if (currentLevel > oldLevel)
        {
            OnLevelUp?.Invoke(oldLevel, currentLevel);
            Debug.Log($"LEVEL UP! {oldLevel} -> {currentLevel}");
        }

        Debug.Log($"+{amount} XP (Total: {currentXP:N0})");
    }

    void UpdateLevel()
    {
        // Find current level based on XP
        for (int level = MAX_LEVEL; level >= 1; level--)
        {
            if (currentXP >= xpTable[level])
            {
                currentLevel = level;
                return;
            }
        }
        currentLevel = 1;
    }

    public int GetLevel()
    {
        return currentLevel;
    }

    public int GetEffectiveLevel()
    {
        // Level including bonuses from equipment
        return Math.Min(currentLevel + bonusLevels, MAX_LEVEL);
    }

    public void SetBonusLevels(int bonus)
    {
        bonusLevels = bonus;
    }

    public int GetBonusLevels()
    {
        return bonusLevels;
    }

    public long GetCurrentXP()
    {
        return currentXP;
    }

    public long GetXPForLevel(int level)
    {
        if (level < 1) return 0;
        if (level > MAX_LEVEL) return MAX_XP;
        return xpTable[level];
    }

    public long GetXPToNextLevel()
    {
        if (currentLevel >= MAX_LEVEL) return 0;
        return xpTable[currentLevel + 1] - currentXP;
    }

    public float GetProgressToNextLevel()
    {
        if (currentLevel >= MAX_LEVEL) return 1f;

        long currentLevelXP = xpTable[currentLevel];
        long nextLevelXP = xpTable[currentLevel + 1];
        long xpIntoLevel = currentXP - currentLevelXP;
        long xpNeeded = nextLevelXP - currentLevelXP;

        return (float)xpIntoLevel / xpNeeded;
    }

    // XP rewards for various actions
    public static int GetFishXP(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 10;
            case Rarity.Uncommon: return 25;
            case Rarity.Rare: return 75;
            case Rarity.Epic: return 200;
            case Rarity.Legendary: return 500;
            case Rarity.Mythic: return 2000;
            default: return 10;
        }
    }

    public static int GetQuestXP()
    {
        return 1000;
    }
}
