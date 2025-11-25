using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int coins = 0;
    public int totalFishCaught = 0;
    public Dictionary<string, int> fishInventory = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log("Total coins: " + coins);
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
        // PERMANENT LOSS - player loses all gold and fish when they drown
        Debug.Log($"DEATH PENALTY: Lost {coins} gold and {totalFishCaught} fish forever!");
        coins = 0;
        totalFishCaught = 0;
        fishInventory.Clear();
    }
}
