using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Quest
{
    public string questId;
    public string questName;
    public string description;
    public string requiredFishId;
    public string requiredFishName;
    public int requiredAmount;
    public int currentAmount;
    public bool isCompleted;
    public bool isActive;      // True when player has accepted and is working on it
    public bool isPending;     // True when quest is available but not yet accepted
    public int xpReward;
    public int coinReward;
    public int questLevel;     // Level the quest was generated for

    public Quest(string fishId, string fishName, int amount, int level)
    {
        questId = Guid.NewGuid().ToString();
        requiredFishId = fishId;
        requiredFishName = fishName;
        requiredAmount = amount;
        currentAmount = 0;
        isCompleted = false;
        isActive = false;      // Not active until accepted
        isPending = true;      // Available to accept
        questLevel = level;

        // Scale rewards with level
        // Base: 100 XP + 25 XP per level, coins scale similarly
        xpReward = 100 + (level * 25) + (amount * 50);
        coinReward = 10 + (level * 5) + (amount * 15);

        questName = $"Catch {amount} {fishName}";
        description = $"Old Captain needs {amount} {fishName} for the guild. Reward: {xpReward} XP, {coinReward} coins.";
    }
}

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem Instance { get; private set; }

    public Quest activeQuest;
    public Quest pendingQuest;  // Quest waiting to be accepted
    public List<Quest> completedQuests = new List<Quest>();

    // Fish types that can be requested - more options unlock at higher levels
    private string[] questFishIds = { "sardine", "anchovy", "bass", "salmon", "tuna", "swordfish", "shark" };
    private string[] questFishNames = { "Sardine", "Anchovy", "Bass", "Salmon", "Tuna", "Swordfish", "Shark" };
    private int[] fishLevelRequirements = { 1, 1, 5, 10, 20, 35, 50 }; // Level required to get quests for this fish

    public event Action<Quest> OnQuestAccepted;
    public event Action<Quest> OnQuestProgress;
    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest> OnNewQuestAvailable;

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
        // Generate first quest (pending, not active)
        GenerateNewQuest();
    }

    public void GenerateNewQuest()
    {
        // Don't generate if we already have an active or pending quest
        if (activeQuest != null && activeQuest.isActive && !activeQuest.isCompleted)
            return;
        if (pendingQuest != null && pendingQuest.isPending)
            return;

        int playerLevel = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;

        // Build list of available fish based on player level
        List<int> availableFishIndices = new List<int>();
        for (int i = 0; i < questFishIds.Length; i++)
        {
            if (playerLevel >= fishLevelRequirements[i])
            {
                availableFishIndices.Add(i);
            }
        }

        // Pick a random available fish (weighted toward higher level fish)
        int fishIndex;
        if (availableFishIndices.Count > 1 && UnityEngine.Random.value > 0.4f)
        {
            // 60% chance to pick from upper half of available fish
            int upperStart = availableFishIndices.Count / 2;
            fishIndex = availableFishIndices[UnityEngine.Random.Range(upperStart, availableFishIndices.Count)];
        }
        else
        {
            fishIndex = availableFishIndices[UnityEngine.Random.Range(0, availableFishIndices.Count)];
        }

        string fishId = questFishIds[fishIndex];
        string fishName = questFishNames[fishIndex];

        // Amount scales with level: base 3-5 at level 1, up to 8-15 at higher levels
        int minAmount = 3 + (playerLevel / 50);
        int maxAmount = 6 + (playerLevel / 25);
        int amount = UnityEngine.Random.Range(minAmount, maxAmount + 1);

        pendingQuest = new Quest(fishId, fishName, amount, playerLevel);

        Debug.Log($"New Quest Available from NPC: {pendingQuest.questName}");
        OnNewQuestAvailable?.Invoke(pendingQuest);
    }

    public void AcceptQuest()
    {
        if (pendingQuest != null && pendingQuest.isPending)
        {
            activeQuest = pendingQuest;
            activeQuest.isPending = false;
            activeQuest.isActive = true;
            pendingQuest = null;

            OnQuestAccepted?.Invoke(activeQuest);
            Debug.Log($"Quest Accepted: {activeQuest.questName}");
        }
    }

    public void DeclineQuest()
    {
        if (pendingQuest != null && pendingQuest.isPending)
        {
            Debug.Log($"Quest Declined: {pendingQuest.questName}");
            pendingQuest = null;

            // Generate a new quest after declining
            Invoke(nameof(GenerateNewQuest), 1f);
        }
    }

    public void OnFishCaught(string fishId)
    {
        if (activeQuest == null || !activeQuest.isActive || activeQuest.isCompleted)
            return;

        if (fishId == activeQuest.requiredFishId)
        {
            activeQuest.currentAmount++;
            OnQuestProgress?.Invoke(activeQuest);

            Debug.Log($"Quest Progress: {activeQuest.currentAmount}/{activeQuest.requiredAmount} {activeQuest.requiredFishName}");

            if (activeQuest.currentAmount >= activeQuest.requiredAmount)
            {
                CompleteQuest();
            }
        }
    }

    void CompleteQuest()
    {
        if (activeQuest == null) return;

        activeQuest.isCompleted = true;
        activeQuest.isActive = false;

        // Give rewards
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(activeQuest.coinReward);
        }

        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.AddXP(activeQuest.xpReward);
        }

        Debug.Log($"Quest Complete! +{activeQuest.xpReward} XP, +{activeQuest.coinReward} coins");

        // Show notification
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"Quest Complete! +{activeQuest.xpReward} XP", new Color(0.3f, 1f, 0.5f));
        }

        OnQuestCompleted?.Invoke(activeQuest);

        // Store completed quest
        completedQuests.Add(activeQuest);

        // Clear active quest and generate new one
        activeQuest = null;
        Invoke(nameof(GenerateNewQuest), 2f);
    }

    public Quest GetActiveQuest()
    {
        return activeQuest;
    }

    public Quest GetPendingQuest()
    {
        return pendingQuest;
    }

    public bool HasActiveQuest()
    {
        return activeQuest != null && activeQuest.isActive && !activeQuest.isCompleted;
    }

    public bool HasPendingQuest()
    {
        return pendingQuest != null && pendingQuest.isPending;
    }

    public int GetCompletedQuestCount()
    {
        return completedQuests.Count;
    }
}
