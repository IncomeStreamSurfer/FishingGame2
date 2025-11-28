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
        // KEY REQUIREMENT: Level 320 should be at 50,000,000 XP (halfway)
        // This means XP scales exponentially, heavily back-loaded
        xpTable = new long[MAX_LEVEL + 2]; // +2 for safety
        xpTable[1] = 0; // Level 1 starts at 0 XP

        // Two-phase exponential curve to ensure level 320 is at 50M XP
        // Phase 1: Levels 1-320 = 0 to 50M XP
        // Phase 2: Levels 320-399 = 50M to 100M XP

        const int MIDPOINT_LEVEL = 320;
        const long MIDPOINT_XP = 50000000; // 50M XP at level 320

        // Phase 1: Levels 2-320 (0 to 50M XP)
        // Use exponential curve that reaches 50M at level 320
        double phase1Base = 50.0;
        double phase1Growth = 1.025; // Higher growth for steeper curve

        // Calculate scale factor for phase 1
        double phase1Raw = 0;
        for (int i = 2; i <= MIDPOINT_LEVEL; i++)
        {
            phase1Raw += phase1Base * Math.Pow(phase1Growth, i - 2);
        }
        double phase1Scale = MIDPOINT_XP / phase1Raw;

        // Build phase 1 table
        long cumulativeXP = 0;
        for (int level = 2; level <= MIDPOINT_LEVEL; level++)
        {
            double xpForThisLevel = phase1Base * Math.Pow(phase1Growth, level - 2) * phase1Scale;
            cumulativeXP += (long)Math.Ceiling(xpForThisLevel);
            xpTable[level] = cumulativeXP;
        }

        // Ensure level 320 is exactly 50M
        xpTable[MIDPOINT_LEVEL] = MIDPOINT_XP;

        // Phase 2: Levels 321-399 (50M to 100M XP)
        // Much steeper curve - need to gain 50M XP in only 79 levels
        double phase2Base = 100000.0; // Start high since we're in endgame
        double phase2Growth = 1.045; // Very steep growth

        // Calculate scale factor for phase 2
        double phase2Raw = 0;
        for (int i = MIDPOINT_LEVEL + 1; i <= MAX_LEVEL; i++)
        {
            phase2Raw += phase2Base * Math.Pow(phase2Growth, i - MIDPOINT_LEVEL - 1);
        }
        double phase2Scale = (MAX_XP - MIDPOINT_XP) / phase2Raw;

        // Build phase 2 table
        cumulativeXP = MIDPOINT_XP;
        for (int level = MIDPOINT_LEVEL + 1; level <= MAX_LEVEL; level++)
        {
            double xpForThisLevel = phase2Base * Math.Pow(phase2Growth, level - MIDPOINT_LEVEL - 1) * phase2Scale;
            cumulativeXP += (long)Math.Ceiling(xpForThisLevel);
            xpTable[level] = cumulativeXP;
        }

        // Ensure max level is exactly MAX_XP
        xpTable[MAX_LEVEL] = MAX_XP;

        // Debug: Log milestone levels
        Debug.Log($"XP Table - Lvl 2: {xpTable[2]:N0}, Lvl 50: {xpTable[50]:N0}, Lvl 100: {xpTable[100]:N0}, Lvl 320: {xpTable[320]:N0}, Lvl 399: {xpTable[399]:N0}");
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

    // Reset progress on death (keeps cosmetics)
    public void ResetProgress()
    {
        Debug.Log($"Resetting XP progress - lost level {currentLevel} and {currentXP:N0} XP");
        currentXP = 0;
        currentLevel = 1;
        bonusLevels = 0;
    }
}
