using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int coins = 0;
    public int totalFishCaught = 0;
    public Dictionary<string, int> fishInventory = new Dictionary<string, int>();

    // Gold multiplier for Ice Queen's Ring
    private float goldMultiplier = 1.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        // Ensure required managers exist
        EnsurePerformanceManager();
        EnsurePollBooth();
        EnsureCookingFire();
    }

    void EnsurePerformanceManager()
    {
        if (DynamicPerformanceManager.Instance == null)
        {
            GameObject perfManager = new GameObject("DynamicPerformanceManager");
            perfManager.AddComponent<DynamicPerformanceManager>();
            Debug.Log("[GameManager] Created DynamicPerformanceManager");
        }
    }

    void EnsurePollBooth()
    {
        if (PollBooth.Instance == null)
        {
            GameObject pollBooth = new GameObject("PollBooth");
            pollBooth.transform.position = new Vector3(12f, 1.3f, -8f); // Near docks, to the right
            pollBooth.AddComponent<PollBooth>();
            Debug.Log("[GameManager] Created PollBooth at docks");
        }
    }

    void EnsureCookingFire()
    {
        if (CookingFire.Instance == null)
        {
            GameObject cookingFire = new GameObject("CookingFire");
            cookingFire.transform.position = new Vector3(-5f, 1.3f, 5f); // Near the island center
            cookingFire.AddComponent<CookingFire>();
            Debug.Log("[GameManager] Created Cooking Fire with Chef Gusteau");
        }
    }

    public void AddCoins(int amount)
    {
        int finalAmount = Mathf.RoundToInt(amount * goldMultiplier);
        coins += finalAmount;
        if (goldMultiplier > 1f)
            Debug.Log($"Total coins: {coins} (+{finalAmount} with {goldMultiplier}x multiplier!)");
        else
            Debug.Log("Total coins: " + coins);
    }

    public void SetGoldMultiplier(float multiplier)
    {
        goldMultiplier = multiplier;
        Debug.Log($"Gold multiplier set to {multiplier}x!");
    }

    public float GetGoldMultiplier()
    {
        return goldMultiplier;
    }

    public void AddFish(FishData fish)
    {
        if (!fishInventory.ContainsKey(fish.id))
            fishInventory[fish.id] = 0;
        fishInventory[fish.id]++;
        totalFishCaught++;
    }

    public int GetCoins()
    {
        return coins;
    }

    public int GetTotalFishCaught()
    {
        return totalFishCaught;
    }

    public void ResetOnDeath()
    {
        // Player loses fish when they die, but KEEPS gold
        Debug.Log($"DEATH PENALTY: Lost {totalFishCaught} fish. Gold preserved: {coins}");
        // Gold is NOT reset - player keeps their gold
        totalFishCaught = 0;
        fishInventory.Clear();
    }

    // Reset fish stats only - keeps gold and cosmetics
    public void ResetFishStats()
    {
        Debug.Log($"Resetting fish stats - lost {totalFishCaught} fish. Gold preserved: {coins}");
        totalFishCaught = 0;
        fishInventory.Clear();
    }
}
