using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FishData
{
    public string id;
    public string fishName;
    public Rarity rarity;
    public int coinValue;
    public float weight;
    public Color fishColor;

    // Special fish properties
    public bool isSpecialFish = false;      // Can't be BBQ'd, goes to special inventory
    public int sellToNPC = 0;               // Sell value to Wetsuit Pete
    public int xpValue = 0;                 // Custom XP override (0 = use default)
    public HealthBuffType healthBuff = HealthBuffType.None;
    public float healthBuffDuration = 0f;   // Duration in seconds (0 = instant)
    public float glowIntensity = 0f;        // 0 = no glow, higher = brighter
    public Color glowColor = Color.white;
}

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }

public enum HealthBuffType
{
    None,
    InstantFullHealth,      // Rare fish - instant full health
    MaxHealthTimed          // Epic/Legendary - max health for duration
}

public class FishingSystem : MonoBehaviour
{
    public static FishingSystem Instance { get; private set; }

    public List<FishData> fishDatabase = new List<FishData>();

    private bool isFishing = false;
    private bool canFish = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializeFish();
    }

    void InitializeFish()
    {
        // Common fish
        fishDatabase.Add(new FishData { id = "sardine", fishName = "Sardine", rarity = Rarity.Common, coinValue = 5, weight = 50f, fishColor = Color.gray });
        fishDatabase.Add(new FishData { id = "anchovy", fishName = "Anchovy", rarity = Rarity.Common, coinValue = 5, weight = 50f, fishColor = new Color(0.6f, 0.6f, 0.7f) });
        fishDatabase.Add(new FishData { id = "minnow", fishName = "Minnow", rarity = Rarity.Common, coinValue = 3, weight = 60f, fishColor = new Color(0.7f, 0.75f, 0.8f) });
        fishDatabase.Add(new FishData { id = "cod", fishName = "Cod", rarity = Rarity.Common, coinValue = 8, weight = 45f, fishColor = new Color(0.6f, 0.55f, 0.4f) });

        // Special quest item - Tackle Box (not in normal pool, handled separately)
        fishDatabase.Add(new FishData { id = "tackle_box", fishName = "Pete's Tackle Box", rarity = Rarity.Legendary, coinValue = 0, weight = 0f, fishColor = new Color(1f, 0.85f, 0.2f) });

        // Uncommon fish
        fishDatabase.Add(new FishData { id = "bass", fishName = "Bass", rarity = Rarity.Uncommon, coinValue = 20, weight = 20f, fishColor = new Color(0.3f, 0.5f, 0.3f) });
        fishDatabase.Add(new FishData { id = "salmon", fishName = "Salmon", rarity = Rarity.Uncommon, coinValue = 30, weight = 18f, fishColor = new Color(0.9f, 0.5f, 0.4f) });
        fishDatabase.Add(new FishData { id = "baby_turtle", fishName = "Baby Sea Turtle", rarity = Rarity.Uncommon, coinValue = 50, weight = 12f, fishColor = new Color(0.2f, 0.5f, 0.3f) });
        fishDatabase.Add(new FishData { id = "jellyfish", fishName = "Jellyfish", rarity = Rarity.Uncommon, coinValue = 35, weight = 15f, fishColor = new Color(0.8f, 0.6f, 0.9f) });

        // Rare fish
        fishDatabase.Add(new FishData { id = "tuna", fishName = "Tuna", rarity = Rarity.Rare, coinValue = 75, weight = 8f, fishColor = new Color(0.2f, 0.3f, 0.6f) });
        fishDatabase.Add(new FishData { id = "swordfish", fishName = "Swordfish", rarity = Rarity.Rare, coinValue = 100, weight = 6f, fishColor = new Color(0.4f, 0.4f, 0.8f) });
        fishDatabase.Add(new FishData { id = "hammerhead", fishName = "Hammerhead", rarity = Rarity.Rare, coinValue = 120, weight = 5f, fishColor = new Color(0.45f, 0.45f, 0.5f) });
        fishDatabase.Add(new FishData { id = "ocean_eel", fishName = "Ocean Sprinter Eel", rarity = Rarity.Rare, coinValue = 90, weight = 7f, fishColor = new Color(0.2f, 0.25f, 0.3f) });

        // Epic fish
        fishDatabase.Add(new FishData { id = "shark", fishName = "Shark", rarity = Rarity.Epic, coinValue = 250, weight = 3f, fishColor = new Color(0.5f, 0.5f, 0.6f) });
        fishDatabase.Add(new FishData { id = "vampire_sealfish", fishName = "Vampire Sealfish", rarity = Rarity.Epic, coinValue = 350, weight = 2.5f, fishColor = new Color(0.3f, 0.1f, 0.15f) });
        fishDatabase.Add(new FishData { id = "icelandic_sunscale", fishName = "Icelandic Sunscale", rarity = Rarity.Epic, coinValue = 400, weight = 2f, fishColor = new Color(0.9f, 0.8f, 0.3f) });

        // Legendary fish
        fishDatabase.Add(new FishData { id = "whale", fishName = "Whale", rarity = Rarity.Legendary, coinValue = 1000, weight = 0.7f, fishColor = new Color(0.3f, 0.4f, 0.7f) });
        fishDatabase.Add(new FishData { id = "dorgush_wrangler", fishName = "Dorgush Cross-Eyed Wrangler", rarity = Rarity.Legendary, coinValue = 1500, weight = 0.5f, fishColor = new Color(0.6f, 0.4f, 0.2f) });
        fishDatabase.Add(new FishData { id = "danish_warblecock", fishName = "Danish Warblecock", rarity = Rarity.Legendary, coinValue = 2000, weight = 0.4f, fishColor = new Color(0.8f, 0.2f, 0.4f) });

        // Mythic fish
        fishDatabase.Add(new FishData { id = "kraken", fishName = "Baby Kraken", rarity = Rarity.Mythic, coinValue = 5000, weight = 0.1f, fishColor = new Color(0.6f, 0.2f, 0.6f) });

        // Special - Humpback Whale (can break your rod!)
        fishDatabase.Add(new FishData { id = "humpback_whale", fishName = "Humpback Whale", rarity = Rarity.Mythic, coinValue = 10000, weight = 0.05f, fishColor = new Color(0.25f, 0.35f, 0.5f) });

        // ============ SPECIAL RARE FISH (5% chance, blue glow) ============
        // 100 XP, 100g sell to Pete, instant full health, can't be BBQ'd
        Color rareGlow = new Color(0.3f, 0.5f, 1f);
        fishDatabase.Add(new FishData {
            id = "red_snapper", fishName = "Red Snapper", rarity = Rarity.Rare,
            coinValue = 0, weight = 0, fishColor = new Color(0.9f, 0.3f, 0.3f),
            isSpecialFish = true, sellToNPC = 100, xpValue = 100,
            healthBuff = HealthBuffType.InstantFullHealth, glowIntensity = 0.5f, glowColor = rareGlow
        });
        fishDatabase.Add(new FishData {
            id = "blue_marlin", fishName = "Blue Marlin", rarity = Rarity.Rare,
            coinValue = 0, weight = 0, fishColor = new Color(0.2f, 0.4f, 0.9f),
            isSpecialFish = true, sellToNPC = 100, xpValue = 100,
            healthBuff = HealthBuffType.InstantFullHealth, glowIntensity = 0.5f, glowColor = rareGlow
        });
        fishDatabase.Add(new FishData {
            id = "rainbow_trout", fishName = "Rainbow Trout", rarity = Rarity.Rare,
            coinValue = 0, weight = 0, fishColor = new Color(0.9f, 0.5f, 0.7f),
            isSpecialFish = true, sellToNPC = 100, xpValue = 100,
            healthBuff = HealthBuffType.InstantFullHealth, glowIntensity = 0.5f, glowColor = rareGlow
        });
        fishDatabase.Add(new FishData {
            id = "sunshore_od", fishName = "Sunshore Od", rarity = Rarity.Rare,
            coinValue = 0, weight = 0, fishColor = new Color(1f, 0.8f, 0.3f),
            isSpecialFish = true, sellToNPC = 100, xpValue = 100,
            healthBuff = HealthBuffType.InstantFullHealth, glowIntensity = 0.5f, glowColor = rareGlow
        });
        fishDatabase.Add(new FishData {
            id = "icelandic_snubnose", fishName = "Icelandic Grey Finned Snubnose", rarity = Rarity.Rare,
            coinValue = 0, weight = 0, fishColor = new Color(0.6f, 0.65f, 0.7f),
            isSpecialFish = true, sellToNPC = 100, xpValue = 100,
            healthBuff = HealthBuffType.InstantFullHealth, glowIntensity = 0.5f, glowColor = rareGlow
        });

        // ============ SPECIAL EPIC FISH (0.5% chance, purple glow) ============
        // 5000 XP, 5000g sell to Pete, 10 min max health, can't be BBQ'd
        Color epicGlow = new Color(0.7f, 0.2f, 1f);
        fishDatabase.Add(new FishData {
            id = "sting_ray", fishName = "Sting Ray", rarity = Rarity.Epic,
            coinValue = 0, weight = 0, fishColor = new Color(0.5f, 0.4f, 0.6f),
            isSpecialFish = true, sellToNPC = 5000, xpValue = 5000,
            healthBuff = HealthBuffType.MaxHealthTimed, healthBuffDuration = 600f, // 10 mins
            glowIntensity = 0.8f, glowColor = epicGlow
        });
        fishDatabase.Add(new FishData {
            id = "rainbow_fish", fishName = "Rainbow Fish", rarity = Rarity.Epic,
            coinValue = 0, weight = 0, fishColor = new Color(1f, 0.5f, 0.8f),
            isSpecialFish = true, sellToNPC = 5000, xpValue = 5000,
            healthBuff = HealthBuffType.MaxHealthTimed, healthBuffDuration = 600f,
            glowIntensity = 0.8f, glowColor = epicGlow
        });
        fishDatabase.Add(new FishData {
            id = "hammerhead_special", fishName = "Hammerhead Shark", rarity = Rarity.Epic,
            coinValue = 0, weight = 0, fishColor = new Color(0.4f, 0.45f, 0.5f),
            isSpecialFish = true, sellToNPC = 5000, xpValue = 5000,
            healthBuff = HealthBuffType.MaxHealthTimed, healthBuffDuration = 600f,
            glowIntensity = 0.8f, glowColor = epicGlow
        });
        fishDatabase.Add(new FishData {
            id = "whale_baby", fishName = "Whale Baby", rarity = Rarity.Epic,
            coinValue = 0, weight = 0, fishColor = new Color(0.35f, 0.45f, 0.7f),
            isSpecialFish = true, sellToNPC = 5000, xpValue = 5000,
            healthBuff = HealthBuffType.MaxHealthTimed, healthBuffDuration = 600f,
            glowIntensity = 0.8f, glowColor = epicGlow
        });
        fishDatabase.Add(new FishData {
            id = "seahorse", fishName = "Seahorse", rarity = Rarity.Epic,
            coinValue = 0, weight = 0, fishColor = new Color(1f, 0.6f, 0.3f),
            isSpecialFish = true, sellToNPC = 5000, xpValue = 5000,
            healthBuff = HealthBuffType.MaxHealthTimed, healthBuffDuration = 600f,
            glowIntensity = 0.8f, glowColor = epicGlow
        });

        // ============ LEGENDARY FISH (0.02% chance, golden glow) ============
        // 100,000 XP, 100,000g sell to Pete, 1 hour max health, special effects
        Color legendaryGlow = new Color(1f, 0.9f, 0.3f);
        fishDatabase.Add(new FishData {
            id = "golden_starfish", fishName = "GOLDEN STARFISH", rarity = Rarity.Legendary,
            coinValue = 0, weight = 0, fishColor = new Color(1f, 0.85f, 0.2f),
            isSpecialFish = true, sellToNPC = 100000, xpValue = 100000,
            healthBuff = HealthBuffType.MaxHealthTimed, healthBuffDuration = 3600f, // 1 hour
            glowIntensity = 2f, glowColor = legendaryGlow
        });
    }

    // Special fish inventory (for rare/epic/legendary that can't be BBQ'd)
    public List<FishData> specialFishInventory = new List<FishData>();

    public void AddSpecialFish(FishData fish)
    {
        specialFishInventory.Add(fish);
        Debug.Log($"Added {fish.fishName} to special fish inventory!");
    }

    public bool SellSpecialFish(int index)
    {
        if (index < 0 || index >= specialFishInventory.Count) return false;

        FishData fish = specialFishInventory[index];
        if (fish.sellToNPC > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(fish.sellToNPC);
            specialFishInventory.RemoveAt(index);
            Debug.Log($"Sold {fish.fishName} for {fish.sellToNPC} gold!");
            return true;
        }
        return false;
    }

    public bool CanFish()
    {
        return canFish && !isFishing;
    }

    // Check if player is on a dock - FISHING ONLY ALLOWED ON DOCKS
    public bool IsOnDock()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return false;

        Vector3 pos = player.transform.position;

        // Main dock: centered at x=-12, from z=8 to z=58, at y=2.5 (surface)
        // Player can be slightly above the dock surface
        bool onMainDock = pos.x > -15f && pos.x < -9f && pos.z > 5f && pos.z < 60f && pos.y > 2f && pos.y < 4f;

        // Add any additional docks here in the future
        // bool onSecondDock = ...

        return onMainDock;
    }

    // Legacy method for compatibility - now just checks dock
    public bool IsNearWater()
    {
        return IsOnDock();
    }

    public void StartFishing()
    {
        if (!CanFish()) return;

        // Check if player is on a dock
        if (!IsOnDock())
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("You can only fish from docks!", new Color(0.9f, 0.6f, 0.2f));
            }
            Debug.Log("Cannot fish here - must be on a dock!");
            return;
        }

        // Find the player's rod animator and tell it to cast
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            FishingRodAnimator rodAnimator = player.GetComponent<FishingRodAnimator>();
            // Don't start if already charging, line is out, or rod is broken
            if (rodAnimator != null && !rodAnimator.IsLineOut() && !rodAnimator.IsCharging() && !rodAnimator.IsRodBroken())
            {
                isFishing = true;
                canFish = false;
                rodAnimator.CastLine();
                Debug.Log("Casting line...");
            }
        }
    }

    // Called by FishingRodAnimator when player successfully reels in during a bite
    public void CompleteCatch()
    {
        // Check if player should find tackle box (10% chance during quest)
        if (WetsuitPeteQuests.Instance != null && WetsuitPeteQuests.Instance.IsTackleBoxQuestActive())
        {
            if (Random.Range(0f, 100f) < 10f)
            {
                // Found the tackle box!
                FishData tackleBox = fishDatabase.Find(f => f.id == "tackle_box");
                if (tackleBox != null)
                {
                    WetsuitPeteQuests.Instance.OnTackleBoxFound();
                    SpawnTackleBoxEffect();
                    Debug.Log("TACKLE BOX FOUND! Return to Wetsuit Pete!");
                    isFishing = false;
                    StartCoroutine(ResetCooldown());
                    return;
                }
            }
        }

        FishData fish = GetRandomFish();

        // HUMPBACK WHALE SPECIAL: 1% chance to break the fishing rod!
        if (fish.id == "humpback_whale")
        {
            // Random chance to break rod (1% of the time when you catch a humpback)
            if (Random.Range(0f, 100f) < 1f)
            {
                // Rod breaks! No fish, need to get a new rod
                StartCoroutine(RodBreakSequence());
                isFishing = false;
                return;
            }
        }

        // ========== SPECIAL FISH HANDLING ==========
        if (fish.isSpecialFish)
        {
            // Add to special inventory (can't be BBQ'd)
            AddSpecialFish(fish);

            // Give gold on catch (bonus gold!)
            int catchGold = fish.sellToNPC / 10; // 10% of sell value as immediate bonus
            if (catchGold > 0)
            {
                GameManager.Instance.AddCoins(catchGold);
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification($"+{catchGold} gold!", new Color(1f, 0.85f, 0.2f));
                }
            }

            // Apply health buff
            if (PlayerHealth.Instance != null)
            {
                if (fish.healthBuff == HealthBuffType.InstantFullHealth)
                {
                    PlayerHealth.Instance.HealToFull();
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowLootNotification("FULL HEALTH RESTORED!", new Color(0.3f, 1f, 0.5f));
                    }
                }
                else if (fish.healthBuff == HealthBuffType.MaxHealthTimed)
                {
                    PlayerHealth.Instance.ApplyMaxHealthBuff(fish.healthBuffDuration);
                    string duration = fish.healthBuffDuration >= 3600 ? "1 HOUR" :
                                     fish.healthBuffDuration >= 60 ? $"{fish.healthBuffDuration / 60f:F0} MINS" :
                                     $"{fish.healthBuffDuration}s";
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowLootNotification($"MAX HEALTH FOR {duration}!", new Color(1f, 0.8f, 0.3f));
                    }
                }
            }

            // Give custom XP
            if (LevelingSystem.Instance != null && fish.xpValue > 0)
            {
                LevelingSystem.Instance.AddXP(fish.xpValue);
            }

            // Special effects for legendary fish
            if (fish.id == "golden_starfish")
            {
                SpawnGoldenStarfishEffect(fish);
            }
            else
            {
                SpawnSpecialFishEffects(fish);
            }

            Debug.Log($"SPECIAL CATCH: {fish.fishName} - {fish.xpValue} XP! Sell to Pete for {fish.sellToNPC}g!");
            isFishing = false;
            StartCoroutine(ResetCooldown());
            return;
        }

        // ========== NORMAL FISH HANDLING ==========
        GameManager.Instance.AddCoins(fish.coinValue);
        GameManager.Instance.AddFish(fish);

        // Show gold notification
        if (UIManager.Instance != null && fish.coinValue > 0)
        {
            UIManager.Instance.ShowLootNotification($"+{fish.coinValue} gold!", new Color(1f, 0.85f, 0.2f));
        }

        // Add fish to food inventory for cooking
        if (FoodInventory.Instance != null)
        {
            FoodInventory.Instance.AddRawFish(fish);
        }

        // Give XP based on fish rarity
        if (LevelingSystem.Instance != null)
        {
            int xp = LevelingSystem.GetFishXP(fish.rarity);
            LevelingSystem.Instance.AddXP(xp);
        }

        // Update quest progress - Old Captain's quests
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.OnFishCaught(fish.id);
        }

        // Update Wetsuit Pete's quest progress
        if (WetsuitPeteQuests.Instance != null)
        {
            WetsuitPeteQuests.Instance.OnFishCaught(fish.id);
        }

        // Spawn fish and coins from water
        SpawnCatchEffects(fish);

        Debug.Log("Caught: " + fish.fishName + " (" + fish.rarity + ") - +" + fish.coinValue + " coins!");

        isFishing = false;
        StartCoroutine(ResetCooldown());
    }

    void SpawnTackleBoxEffect()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        Vector3 spawnPos = player.transform.position + player.transform.forward * 3f;
        spawnPos.y = 1.5f;

        // Create tackle box with golden glow
        GameObject tackleBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tackleBox.name = "TackleBox";
        tackleBox.transform.position = spawnPos;
        tackleBox.transform.localScale = new Vector3(0.5f, 0.3f, 0.35f);
        Object.Destroy(tackleBox.GetComponent<Collider>());

        Material boxMat = new Material(Shader.Find("Standard"));
        boxMat.color = new Color(0.5f, 0.35f, 0.15f);
        boxMat.EnableKeyword("_EMISSION");
        boxMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 2f); // Golden glow
        tackleBox.GetComponent<Renderer>().material = boxMat;

        // Add handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(tackleBox.transform);
        handle.transform.localPosition = new Vector3(0, 0.25f, 0);
        handle.transform.localScale = new Vector3(0.4f, 0.15f, 0.15f);
        handle.transform.localRotation = Quaternion.Euler(0, 0, 90);
        Object.Destroy(handle.GetComponent<Collider>());
        Material handleMat = new Material(Shader.Find("Standard"));
        handleMat.color = new Color(0.3f, 0.3f, 0.35f);
        handle.GetComponent<Renderer>().material = handleMat;

        StartCoroutine(TackleBoxAnimation(tackleBox));
    }

    IEnumerator TackleBoxAnimation(GameObject box)
    {
        Vector3 startPos = box.transform.position;
        float t = 0;

        // Float up with golden sparkle
        while (t < 2f)
        {
            t += Time.deltaTime;
            box.transform.position = startPos + Vector3.up * (t * 0.5f);
            box.transform.Rotate(Vector3.up * 90 * Time.deltaTime);

            // Pulse the glow
            float pulse = 1.5f + Mathf.Sin(t * 8f) * 0.5f;
            box.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * pulse);

            yield return null;
        }

        // Fade out
        Material mat = box.GetComponent<Renderer>().material;
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;

        Color c = mat.color;
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            c.a = 1 - t;
            mat.color = c;
            yield return null;
        }

        Destroy(box);
    }

    IEnumerator RodBreakSequence()
    {
        Debug.Log("THE HUMPBACK WHALE WAS TOO POWERFUL! YOUR FISHING ROD SNAPPED!");

        // Show UI notification
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("YOUR ROD SNAPPED!", Color.red);
        }

        // Get player's rod animator and trigger break effect
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            FishingRodAnimator rodAnimator = player.GetComponent<FishingRodAnimator>();
            if (rodAnimator != null)
            {
                rodAnimator.BreakRod();
            }
        }

        yield return new WaitForSeconds(2f);

        // Show message about getting a new rod
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("The whale got away... Find a new rod!", new Color(1f, 0.8f, 0.3f));
        }

        StartCoroutine(ResetCooldown());
    }

    // Called when player reels in without a fish
    public void ResetFishing()
    {
        isFishing = false;
        StartCoroutine(ResetCooldown());
    }

    void SpawnCatchEffects(FishData fish)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        Vector3 spawnPos = player.transform.position + player.transform.forward * 3f;
        spawnPos.y = 1.5f;

        // Create detailed fish model
        GameObject fishObj = CreateDetailedFish(fish);
        fishObj.name = "CaughtFish";
        fishObj.transform.position = spawnPos;
        fishObj.transform.rotation = Quaternion.Euler(0, 90, 0);

        StartCoroutine(FishCaughtAnimation(fishObj, fish));

        // Spawn coins
        int numCoins = Mathf.Min(fish.coinValue / 5, 15);
        for (int i = 0; i < numCoins; i++)
        {
            StartCoroutine(SpawnCoin(spawnPos, i * 0.08f));
        }
    }

    void SpawnSpecialFishEffects(FishData fish)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        Vector3 spawnPos = player.transform.position + player.transform.forward * 3f;
        spawnPos.y = 1.5f;

        // Create glowing fish model
        GameObject fishObj = CreateDetailedFish(fish);
        fishObj.name = "SpecialFish";
        fishObj.transform.position = spawnPos;
        fishObj.transform.rotation = Quaternion.Euler(0, 90, 0);

        // Add glow to all parts
        foreach (Renderer rend in fishObj.GetComponentsInChildren<Renderer>())
        {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", fish.glowColor * fish.glowIntensity);
        }

        StartCoroutine(SpecialFishAnimation(fishObj, fish));
    }

    void SpawnGoldenStarfishEffect(FishData fish)
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        Vector3 spawnPos = player.transform.position + player.transform.forward * 4f;
        spawnPos.y = 1f; // In water

        // Create golden starfish
        GameObject starfish = new GameObject("GoldenStarfish");
        starfish.transform.position = spawnPos;

        // Create star shape with 5 arms
        Material starMat = new Material(Shader.Find("Standard"));
        starMat.color = fish.fishColor;
        starMat.EnableKeyword("_EMISSION");
        starMat.SetColor("_EmissionColor", fish.glowColor * fish.glowIntensity);

        // Center
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        center.name = "Center";
        center.transform.SetParent(starfish.transform);
        center.transform.localPosition = Vector3.zero;
        center.transform.localScale = new Vector3(0.25f, 0.08f, 0.25f);
        center.GetComponent<Renderer>().material = starMat;
        Object.Destroy(center.GetComponent<Collider>());

        // 5 arms
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f * Mathf.Deg2Rad;
            Vector3 armDir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm" + i;
            arm.transform.SetParent(starfish.transform);
            arm.transform.localPosition = armDir * 0.2f;
            arm.transform.localScale = new Vector3(0.08f, 0.05f, 0.25f);
            arm.transform.localRotation = Quaternion.LookRotation(armDir) * Quaternion.Euler(0, 90, 0);
            arm.GetComponent<Renderer>().material = starMat;
            Object.Destroy(arm.GetComponent<Collider>());

            // Arm tip
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "Tip" + i;
            tip.transform.SetParent(starfish.transform);
            tip.transform.localPosition = armDir * 0.35f;
            tip.transform.localScale = new Vector3(0.06f, 0.04f, 0.08f);
            tip.GetComponent<Renderer>().material = starMat;
            Object.Destroy(tip.GetComponent<Collider>());
        }

        StartCoroutine(GoldenStarfishAnimation(starfish, fish));
    }

    IEnumerator SpecialFishAnimation(GameObject fishObj, FishData fish)
    {
        // Show popup
        if (UIManager.Instance != null)
        {
            Color rarityColor = GetRarityColor(fish.rarity);
            UIManager.Instance.ShowLootNotification($"SPECIAL: {fish.fishName}!", rarityColor);
        }

        Vector3 startPos = fishObj.transform.position;
        float t = 0;

        // Glowing, floating animation
        while (t < 2.5f)
        {
            t += Time.deltaTime;
            float floatY = Mathf.Sin(t * 3f) * 0.3f;
            fishObj.transform.position = startPos + new Vector3(0, floatY + t * 0.3f, 0);
            fishObj.transform.Rotate(Vector3.up * 120 * Time.deltaTime);

            // Pulse glow
            float pulse = 1f + Mathf.Sin(t * 6f) * 0.3f;
            foreach (Renderer rend in fishObj.GetComponentsInChildren<Renderer>())
            {
                rend.material.SetColor("_EmissionColor", fish.glowColor * fish.glowIntensity * pulse);
            }

            yield return null;
        }

        // Fade out
        Renderer[] renderers = fishObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            Material mat = rend.material;
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
        }

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            foreach (Renderer rend in renderers)
            {
                Color c = rend.material.color;
                c.a = 1 - t;
                rend.material.color = c;
            }
            yield return null;
        }

        Destroy(fishObj);
    }

    IEnumerator GoldenStarfishAnimation(GameObject starfish, FishData fish)
    {
        // Epic announcement
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("!!! LEGENDARY: GOLDEN STARFISH !!!", new Color(1f, 0.85f, 0.2f));
        }

        Vector3 startPos = starfish.transform.position;
        float t = 0;

        // Float in water with sunshine beams
        List<GameObject> sunBeams = new List<GameObject>();

        // Create sun beams
        for (int i = 0; i < 8; i++)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "SunBeam" + i;
            beam.transform.SetParent(starfish.transform);
            float angle = i * 45f * Mathf.Deg2Rad;
            beam.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.1f, 0.05f, Mathf.Sin(angle) * 0.1f);
            beam.transform.localScale = new Vector3(0.02f, 0.8f, 0.02f);
            beam.transform.localRotation = Quaternion.Euler(Mathf.Cos(angle) * 30, 0, Mathf.Sin(angle) * 30);

            Material beamMat = new Material(Shader.Find("Standard"));
            beamMat.color = new Color(1f, 0.95f, 0.6f, 0.5f);
            beamMat.EnableKeyword("_EMISSION");
            beamMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.4f) * 3f);
            beamMat.SetFloat("_Mode", 3);
            beamMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            beamMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            beamMat.EnableKeyword("_ALPHABLEND_ON");
            beamMat.renderQueue = 3000;
            beam.GetComponent<Renderer>().material = beamMat;
            Object.Destroy(beam.GetComponent<Collider>());

            sunBeams.Add(beam);
        }

        // Float and glow in water for 3 seconds
        while (t < 3f)
        {
            t += Time.deltaTime;
            float floatY = Mathf.Sin(t * 2f) * 0.2f;
            starfish.transform.position = startPos + new Vector3(0, floatY + 0.5f, 0);
            starfish.transform.Rotate(Vector3.up * 60 * Time.deltaTime);

            // Animate sun beams
            for (int i = 0; i < sunBeams.Count; i++)
            {
                float beamPulse = 0.5f + Mathf.Sin(t * 4f + i) * 0.3f;
                sunBeams[i].transform.localScale = new Vector3(0.02f, 0.8f + beamPulse * 0.3f, 0.02f);
            }

            // Pulse main glow
            float pulse = 2f + Mathf.Sin(t * 8f) * 1f;
            foreach (Renderer rend in starfish.GetComponentsInChildren<Renderer>())
            {
                if (!rend.name.Contains("SunBeam"))
                {
                    rend.material.SetColor("_EmissionColor", fish.glowColor * pulse);
                }
            }

            yield return null;
        }

        // Rise up into player inventory
        Vector3 endPos = startPos + Vector3.up * 3f;
        t = 0;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            float progress = t / 1.5f;
            starfish.transform.position = Vector3.Lerp(startPos + Vector3.up * 0.5f, endPos, progress);
            starfish.transform.Rotate(Vector3.up * 180 * Time.deltaTime);

            // Scale down and brighten
            float scale = 1f - progress * 0.5f;
            starfish.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(starfish);
    }

    GameObject CreateDetailedFish(FishData fish)
    {
        // Size based on rarity
        float sizeMultiplier = 1f + (int)fish.rarity * 0.2f;

        // Root object
        GameObject fishRoot = new GameObject("DetailedFish");

        // Create materials
        Material bodyMat = new Material(Shader.Find("Standard"));
        bodyMat.color = fish.fishColor;
        bodyMat.SetFloat("_Metallic", 0.3f);
        bodyMat.SetFloat("_Glossiness", 0.7f); // Shiny wet fish

        Material bellyMat = new Material(Shader.Find("Standard"));
        bellyMat.color = Color.Lerp(fish.fishColor, Color.white, 0.7f); // Lighter belly
        bellyMat.SetFloat("_Glossiness", 0.8f);

        Material finMat = new Material(Shader.Find("Standard"));
        finMat.color = Color.Lerp(fish.fishColor, new Color(0.3f, 0.3f, 0.3f), 0.3f);
        finMat.SetFloat("_Glossiness", 0.5f);

        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = Color.white;
        eyeMat.SetFloat("_Glossiness", 0.95f);

        Material pupilMat = new Material(Shader.Find("Standard"));
        pupilMat.color = Color.black;

        // === MAIN BODY (elongated ellipsoid using scaled sphere) ===
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(fishRoot.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.5f, 0.25f, 0.2f) * sizeMultiplier;
        Object.Destroy(body.GetComponent<Collider>());
        body.GetComponent<Renderer>().material = bodyMat;

        // === BELLY (lighter underside) ===
        GameObject belly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        belly.name = "Belly";
        belly.transform.SetParent(fishRoot.transform);
        belly.transform.localPosition = new Vector3(0, -0.03f, 0) * sizeMultiplier;
        belly.transform.localScale = new Vector3(0.4f, 0.15f, 0.15f) * sizeMultiplier;
        Object.Destroy(belly.GetComponent<Collider>());
        belly.GetComponent<Renderer>().material = bellyMat;

        // === HEAD (slightly larger front) ===
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(fishRoot.transform);
        head.transform.localPosition = new Vector3(0.22f, 0.02f, 0) * sizeMultiplier;
        head.transform.localScale = new Vector3(0.18f, 0.18f, 0.16f) * sizeMultiplier;
        Object.Destroy(head.GetComponent<Collider>());
        head.GetComponent<Renderer>().material = bodyMat;

        // === SNOUT ===
        GameObject snout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        snout.name = "Snout";
        snout.transform.SetParent(fishRoot.transform);
        snout.transform.localPosition = new Vector3(0.32f, 0, 0) * sizeMultiplier;
        snout.transform.localScale = new Vector3(0.1f, 0.08f, 0.08f) * sizeMultiplier;
        Object.Destroy(snout.GetComponent<Collider>());
        snout.GetComponent<Renderer>().material = bodyMat;

        // === MOUTH (darker opening) ===
        GameObject mouth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mouth.name = "Mouth";
        mouth.transform.SetParent(fishRoot.transform);
        mouth.transform.localPosition = new Vector3(0.36f, -0.01f, 0) * sizeMultiplier;
        mouth.transform.localScale = new Vector3(0.03f, 0.04f, 0.05f) * sizeMultiplier;
        Object.Destroy(mouth.GetComponent<Collider>());
        Material mouthMat = new Material(Shader.Find("Standard"));
        mouthMat.color = new Color(0.2f, 0.1f, 0.1f);
        mouth.GetComponent<Renderer>().material = mouthMat;

        // === EYES ===
        // Right eye
        GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEye.name = "RightEye";
        rightEye.transform.SetParent(fishRoot.transform);
        rightEye.transform.localPosition = new Vector3(0.22f, 0.05f, 0.08f) * sizeMultiplier;
        rightEye.transform.localScale = new Vector3(0.05f, 0.05f, 0.03f) * sizeMultiplier;
        Object.Destroy(rightEye.GetComponent<Collider>());
        rightEye.GetComponent<Renderer>().material = eyeMat;

        // Right pupil
        GameObject rightPupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightPupil.name = "RightPupil";
        rightPupil.transform.SetParent(fishRoot.transform);
        rightPupil.transform.localPosition = new Vector3(0.23f, 0.05f, 0.095f) * sizeMultiplier;
        rightPupil.transform.localScale = new Vector3(0.025f, 0.025f, 0.015f) * sizeMultiplier;
        Object.Destroy(rightPupil.GetComponent<Collider>());
        rightPupil.GetComponent<Renderer>().material = pupilMat;

        // Left eye
        GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEye.name = "LeftEye";
        leftEye.transform.SetParent(fishRoot.transform);
        leftEye.transform.localPosition = new Vector3(0.22f, 0.05f, -0.08f) * sizeMultiplier;
        leftEye.transform.localScale = new Vector3(0.05f, 0.05f, 0.03f) * sizeMultiplier;
        Object.Destroy(leftEye.GetComponent<Collider>());
        leftEye.GetComponent<Renderer>().material = eyeMat;

        // Left pupil
        GameObject leftPupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftPupil.name = "LeftPupil";
        leftPupil.transform.SetParent(fishRoot.transform);
        leftPupil.transform.localPosition = new Vector3(0.23f, 0.05f, -0.095f) * sizeMultiplier;
        leftPupil.transform.localScale = new Vector3(0.025f, 0.025f, 0.015f) * sizeMultiplier;
        Object.Destroy(leftPupil.GetComponent<Collider>());
        leftPupil.GetComponent<Renderer>().material = pupilMat;

        // === TAIL FIN (forked tail) ===
        // Upper tail lobe
        GameObject tailUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tailUpper.name = "TailUpper";
        tailUpper.transform.SetParent(fishRoot.transform);
        tailUpper.transform.localPosition = new Vector3(-0.32f, 0.06f, 0) * sizeMultiplier;
        tailUpper.transform.localScale = new Vector3(0.15f, 0.08f, 0.02f) * sizeMultiplier;
        tailUpper.transform.localRotation = Quaternion.Euler(0, 0, 30);
        Object.Destroy(tailUpper.GetComponent<Collider>());
        tailUpper.GetComponent<Renderer>().material = finMat;

        // Lower tail lobe
        GameObject tailLower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tailLower.name = "TailLower";
        tailLower.transform.SetParent(fishRoot.transform);
        tailLower.transform.localPosition = new Vector3(-0.32f, -0.06f, 0) * sizeMultiplier;
        tailLower.transform.localScale = new Vector3(0.15f, 0.08f, 0.02f) * sizeMultiplier;
        tailLower.transform.localRotation = Quaternion.Euler(0, 0, -30);
        Object.Destroy(tailLower.GetComponent<Collider>());
        tailLower.GetComponent<Renderer>().material = finMat;

        // Tail base
        GameObject tailBase = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tailBase.name = "TailBase";
        tailBase.transform.SetParent(fishRoot.transform);
        tailBase.transform.localPosition = new Vector3(-0.22f, 0, 0) * sizeMultiplier;
        tailBase.transform.localScale = new Vector3(0.12f, 0.1f, 0.08f) * sizeMultiplier;
        Object.Destroy(tailBase.GetComponent<Collider>());
        tailBase.GetComponent<Renderer>().material = bodyMat;

        // === DORSAL FIN (top fin) ===
        GameObject dorsalFin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dorsalFin.name = "DorsalFin";
        dorsalFin.transform.SetParent(fishRoot.transform);
        dorsalFin.transform.localPosition = new Vector3(0, 0.15f, 0) * sizeMultiplier;
        dorsalFin.transform.localScale = new Vector3(0.2f, 0.1f, 0.015f) * sizeMultiplier;
        Object.Destroy(dorsalFin.GetComponent<Collider>());
        dorsalFin.GetComponent<Renderer>().material = finMat;

        // Dorsal fin spike
        GameObject dorsalSpike = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dorsalSpike.name = "DorsalSpike";
        dorsalSpike.transform.SetParent(fishRoot.transform);
        dorsalSpike.transform.localPosition = new Vector3(0.05f, 0.2f, 0) * sizeMultiplier;
        dorsalSpike.transform.localScale = new Vector3(0.08f, 0.06f, 0.01f) * sizeMultiplier;
        dorsalSpike.transform.localRotation = Quaternion.Euler(0, 0, 15);
        Object.Destroy(dorsalSpike.GetComponent<Collider>());
        dorsalSpike.GetComponent<Renderer>().material = finMat;

        // === ANAL FIN (bottom rear fin) ===
        GameObject analFin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        analFin.name = "AnalFin";
        analFin.transform.SetParent(fishRoot.transform);
        analFin.transform.localPosition = new Vector3(-0.1f, -0.12f, 0) * sizeMultiplier;
        analFin.transform.localScale = new Vector3(0.1f, 0.06f, 0.015f) * sizeMultiplier;
        Object.Destroy(analFin.GetComponent<Collider>());
        analFin.GetComponent<Renderer>().material = finMat;

        // === PECTORAL FINS (side fins) ===
        // Right pectoral
        GameObject pectoralRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pectoralRight.name = "PectoralRight";
        pectoralRight.transform.SetParent(fishRoot.transform);
        pectoralRight.transform.localPosition = new Vector3(0.12f, -0.02f, 0.1f) * sizeMultiplier;
        pectoralRight.transform.localScale = new Vector3(0.08f, 0.02f, 0.12f) * sizeMultiplier;
        pectoralRight.transform.localRotation = Quaternion.Euler(0, -20, 15);
        Object.Destroy(pectoralRight.GetComponent<Collider>());
        pectoralRight.GetComponent<Renderer>().material = finMat;

        // Left pectoral
        GameObject pectoralLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pectoralLeft.name = "PectoralLeft";
        pectoralLeft.transform.SetParent(fishRoot.transform);
        pectoralLeft.transform.localPosition = new Vector3(0.12f, -0.02f, -0.1f) * sizeMultiplier;
        pectoralLeft.transform.localScale = new Vector3(0.08f, 0.02f, 0.12f) * sizeMultiplier;
        pectoralLeft.transform.localRotation = Quaternion.Euler(0, 20, -15);
        Object.Destroy(pectoralLeft.GetComponent<Collider>());
        pectoralLeft.GetComponent<Renderer>().material = finMat;

        // === PELVIC FINS (bottom front fins) ===
        // Right pelvic
        GameObject pelvicRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pelvicRight.name = "PelvicRight";
        pelvicRight.transform.SetParent(fishRoot.transform);
        pelvicRight.transform.localPosition = new Vector3(0.05f, -0.1f, 0.05f) * sizeMultiplier;
        pelvicRight.transform.localScale = new Vector3(0.05f, 0.02f, 0.06f) * sizeMultiplier;
        pelvicRight.transform.localRotation = Quaternion.Euler(0, -10, 20);
        Object.Destroy(pelvicRight.GetComponent<Collider>());
        pelvicRight.GetComponent<Renderer>().material = finMat;

        // Left pelvic
        GameObject pelvicLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pelvicLeft.name = "PelvicLeft";
        pelvicLeft.transform.SetParent(fishRoot.transform);
        pelvicLeft.transform.localPosition = new Vector3(0.05f, -0.1f, -0.05f) * sizeMultiplier;
        pelvicLeft.transform.localScale = new Vector3(0.05f, 0.02f, 0.06f) * sizeMultiplier;
        pelvicLeft.transform.localRotation = Quaternion.Euler(0, 10, -20);
        Object.Destroy(pelvicLeft.GetComponent<Collider>());
        pelvicLeft.GetComponent<Renderer>().material = finMat;

        // === GILL LINES ===
        GameObject gillRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gillRight.name = "GillRight";
        gillRight.transform.SetParent(fishRoot.transform);
        gillRight.transform.localPosition = new Vector3(0.17f, 0, 0.085f) * sizeMultiplier;
        gillRight.transform.localScale = new Vector3(0.02f, 0.08f, 0.005f) * sizeMultiplier;
        gillRight.transform.localRotation = Quaternion.Euler(0, 20, 0);
        Object.Destroy(gillRight.GetComponent<Collider>());
        Material gillMat = new Material(Shader.Find("Standard"));
        gillMat.color = Color.Lerp(fish.fishColor, Color.red, 0.3f);
        gillRight.GetComponent<Renderer>().material = gillMat;

        GameObject gillLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gillLeft.name = "GillLeft";
        gillLeft.transform.SetParent(fishRoot.transform);
        gillLeft.transform.localPosition = new Vector3(0.17f, 0, -0.085f) * sizeMultiplier;
        gillLeft.transform.localScale = new Vector3(0.02f, 0.08f, 0.005f) * sizeMultiplier;
        gillLeft.transform.localRotation = Quaternion.Euler(0, -20, 0);
        Object.Destroy(gillLeft.GetComponent<Collider>());
        gillLeft.GetComponent<Renderer>().material = gillMat;

        // === SCALE PATTERN (decorative stripes for larger fish) ===
        if (fish.rarity >= Rarity.Uncommon)
        {
            Material stripeMat = new Material(Shader.Find("Standard"));
            stripeMat.color = Color.Lerp(fish.fishColor, Color.black, 0.3f);
            stripeMat.SetFloat("_Glossiness", 0.6f);

            for (int i = 0; i < 3; i++)
            {
                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "Stripe" + i;
                stripe.transform.SetParent(fishRoot.transform);
                float xPos = -0.05f + i * 0.08f;
                stripe.transform.localPosition = new Vector3(xPos, 0.05f, 0) * sizeMultiplier;
                stripe.transform.localScale = new Vector3(0.015f, 0.15f, 0.22f) * sizeMultiplier;
                stripe.transform.localRotation = Quaternion.Euler(0, 0, 5 - i * 5);
                Object.Destroy(stripe.GetComponent<Collider>());
                stripe.GetComponent<Renderer>().material = stripeMat;
            }
        }

        // === SPECIAL FEATURES FOR RARE FISH ===
        if (fish.rarity >= Rarity.Epic)
        {
            // Add glow effect for epic+ fish
            Material glowMat = new Material(Shader.Find("Standard"));
            glowMat.color = fish.fishColor;
            glowMat.EnableKeyword("_EMISSION");
            glowMat.SetColor("_EmissionColor", fish.fishColor * 0.5f);
            body.GetComponent<Renderer>().material = glowMat;
        }

        // Special features for specific fish types
        if (fish.id == "swordfish")
        {
            // Add sword/bill
            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sword.name = "Sword";
            sword.transform.SetParent(fishRoot.transform);
            sword.transform.localPosition = new Vector3(0.5f, 0, 0) * sizeMultiplier;
            sword.transform.localScale = new Vector3(0.02f, 0.2f, 0.02f) * sizeMultiplier;
            sword.transform.localRotation = Quaternion.Euler(0, 0, 90);
            Object.Destroy(sword.GetComponent<Collider>());
            Material swordMat = new Material(Shader.Find("Standard"));
            swordMat.color = new Color(0.4f, 0.35f, 0.3f);
            sword.GetComponent<Renderer>().material = swordMat;
        }
        else if (fish.id == "shark")
        {
            // More prominent dorsal fin for shark
            dorsalFin.transform.localScale = new Vector3(0.25f, 0.18f, 0.02f) * sizeMultiplier;
            dorsalFin.transform.localPosition = new Vector3(-0.02f, 0.18f, 0) * sizeMultiplier;
            dorsalSpike.transform.localScale = new Vector3(0.12f, 0.1f, 0.015f) * sizeMultiplier;
        }
        else if (fish.id == "kraken")
        {
            // Add tentacles for baby kraken
            Material tentacleMat = new Material(Shader.Find("Standard"));
            tentacleMat.color = fish.fishColor;
            tentacleMat.EnableKeyword("_EMISSION");
            tentacleMat.SetColor("_EmissionColor", fish.fishColor * 0.8f);

            for (int i = 0; i < 6; i++)
            {
                GameObject tentacle = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                tentacle.name = "Tentacle" + i;
                tentacle.transform.SetParent(fishRoot.transform);
                float angle = i * 60f * Mathf.Deg2Rad;
                float xOff = Mathf.Cos(angle) * 0.15f;
                float zOff = Mathf.Sin(angle) * 0.15f;
                tentacle.transform.localPosition = new Vector3(-0.15f + xOff * 0.3f, -0.08f, zOff) * sizeMultiplier;
                tentacle.transform.localScale = new Vector3(0.03f, 0.12f, 0.03f) * sizeMultiplier;
                tentacle.transform.localRotation = Quaternion.Euler(xOff * 100, 0, zOff * 100);
                Object.Destroy(tentacle.GetComponent<Collider>());
                tentacle.GetComponent<Renderer>().material = tentacleMat;
            }
        }

        return fishRoot;
    }

    IEnumerator FishCaughtAnimation(GameObject fish, FishData fishData)
    {
        // Show fish name popup
        StartCoroutine(ShowCatchPopup(fishData));

        Vector3 startPos = fish.transform.position;
        float t = 0;

        // Fish flops around
        while (t < 1.5f)
        {
            t += Time.deltaTime;

            // Flopping motion
            float flopHeight = Mathf.Sin(t * 15f) * 0.2f * (1.5f - t);
            float wobble = Mathf.Sin(t * 20f) * 15f;

            fish.transform.position = startPos + new Vector3(0, flopHeight + 0.5f, 0);
            fish.transform.rotation = Quaternion.Euler(wobble, t * 180f, wobble * 0.5f);

            yield return null;
        }

        // Get all renderers in the fish hierarchy
        Renderer[] renderers = fish.GetComponentsInChildren<Renderer>();
        List<Material> materials = new List<Material>();
        List<Color> originalColors = new List<Color>();

        // Setup transparency on all materials
        foreach (Renderer rend in renderers)
        {
            Material mat = rend.material;
            materials.Add(mat);
            originalColors.Add(mat.color);

            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
        }

        // Fade out all parts
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            for (int i = 0; i < materials.Count; i++)
            {
                Color c = originalColors[i];
                c.a = 1 - t;
                materials[i].color = c;
            }
            yield return null;
        }

        Destroy(fish);
    }

    IEnumerator ShowCatchPopup(FishData fish)
    {
        // This would show a UI popup - for now we'll just log it
        // A full implementation would create a world-space canvas or OnGUI element
        yield return null;
    }

    IEnumerator SpawnCoin(Vector3 basePos, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.name = "Coin";
        coin.transform.position = basePos + new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        coin.transform.localScale = new Vector3(0.2f, 0.03f, 0.2f);
        Object.Destroy(coin.GetComponent<Collider>());

        Material coinMat = new Material(Shader.Find("Standard"));
        coinMat.color = new Color(1f, 0.85f, 0f);
        coinMat.SetFloat("_Metallic", 0.9f);
        coinMat.SetFloat("_Glossiness", 0.8f);
        coin.GetComponent<Renderer>().material = coinMat;

        // Animate coin flying up and towards player
        Vector3 startPos = coin.transform.position;
        GameObject player = GameObject.Find("Player");
        Vector3 endPos = player != null ? player.transform.position + Vector3.up * 1.5f : startPos + Vector3.up * 2f;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;
            coin.transform.position = pos;

            coin.transform.Rotate(Vector3.up * 720 * Time.deltaTime);
            coin.transform.Rotate(Vector3.right * 360 * Time.deltaTime);

            yield return null;
        }

        Destroy(coin);
    }

    IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(0.5f);
        canFish = true;
    }

    // Get rod tier bonus for special fish chances
    float GetRodTierBonus()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            FishingRodAnimator rodAnimator = player.GetComponent<FishingRodAnimator>();
            if (rodAnimator != null)
            {
                // Higher tier rods increase special fish chances
                // Tier 1 = 0%, Tier 2 = 10%, Tier 3 = 25%, Tier 4 = 50%, Tier 5 = 100%
                int tier = rodAnimator.GetCurrentRodTier();
                return (tier - 1) * 0.25f; // 0, 0.25, 0.5, 0.75, 1.0
            }
        }
        return 0f;
    }

    FishData GetRandomFish()
    {
        float rodBonus = GetRodTierBonus();

        // ========== CHECK FOR SPECIAL FISH FIRST ==========

        // LEGENDARY: 0.02% base chance (+ rod bonus)
        float legendaryChance = 0.02f * (1f + rodBonus);
        if (Random.Range(0f, 100f) < legendaryChance)
        {
            FishData legendary = fishDatabase.Find(f => f.id == "golden_starfish");
            if (legendary != null)
            {
                Debug.Log("!!! LEGENDARY FISH - GOLDEN STARFISH !!!");
                return legendary;
            }
        }

        // EPIC: 0.5% base chance (+ rod bonus)
        float epicChance = 0.5f * (1f + rodBonus);
        if (Random.Range(0f, 100f) < epicChance)
        {
            string[] epicIds = { "sting_ray", "rainbow_fish", "hammerhead_special", "whale_baby", "seahorse" };
            string randomEpic = epicIds[Random.Range(0, epicIds.Length)];
            FishData epic = fishDatabase.Find(f => f.id == randomEpic);
            if (epic != null)
            {
                Debug.Log("!! EPIC FISH - " + epic.fishName + " !!");
                return epic;
            }
        }

        // RARE SPECIAL: 5% base chance (+ rod bonus)
        float rareSpecialChance = 5f * (1f + rodBonus * 0.5f);
        if (Random.Range(0f, 100f) < rareSpecialChance)
        {
            string[] rareIds = { "red_snapper", "blue_marlin", "rainbow_trout", "sunshore_od", "icelandic_snubnose" };
            string randomRare = rareIds[Random.Range(0, rareIds.Length)];
            FishData rare = fishDatabase.Find(f => f.id == randomRare);
            if (rare != null)
            {
                Debug.Log("! RARE SPECIAL FISH - " + rare.fishName + " !");
                return rare;
            }
        }

        // ========== NORMAL FISH SELECTION ==========

        // Check if salmon quest is active - 40% chance to get salmon
        if (WetsuitPeteQuests.Instance != null && WetsuitPeteQuests.Instance.IsSalmonQuestActive())
        {
            if (Random.Range(0f, 100f) < 40f)
            {
                FishData salmon = fishDatabase.Find(f => f.id == "salmon");
                if (salmon != null)
                {
                    Debug.Log("Salmon quest bonus triggered!");
                    return salmon;
                }
            }
        }

        // Apply luck bonus - increases rare fish chances
        float luckBonus = UIManager.Instance != null ? UIManager.Instance.GetLuckBonus() : 0f;

        // Get cast distance bonus - longer casts = better fish
        float distanceBonus = 0f;
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            FishingRodAnimator rodAnimator = player.GetComponent<FishingRodAnimator>();
            if (rodAnimator != null)
            {
                // Cast distance ranges from ~6 to ~25
                // Convert to bonus: 0 at 6m, 0.5 at 25m
                float castDist = rodAnimator.LastCastDistance;
                distanceBonus = Mathf.Clamp01((castDist - 6f) / 38f) * 0.5f;
            }
        }

        float totalBonus = luckBonus + distanceBonus;

        // Adjust weights based on luck and distance
        float totalWeight = 0f;
        List<float> adjustedWeights = new List<float>();

        foreach (var fish in fishDatabase)
        {
            // Skip special items (tackle box has 0 weight)
            if (fish.weight <= 0f)
            {
                adjustedWeights.Add(0f);
                continue;
            }

            float adjustedWeight = fish.weight;

            if (fish.rarity == Rarity.Common)
                adjustedWeight *= (1f - totalBonus * 0.8f); // Much less common fish with good casts
            else if (fish.rarity == Rarity.Uncommon)
                adjustedWeight *= (1f - totalBonus * 0.3f);
            else if (fish.rarity == Rarity.Rare || fish.rarity == Rarity.Epic)
                adjustedWeight *= (1f + totalBonus * 3f);
            else if (fish.rarity == Rarity.Legendary || fish.rarity == Rarity.Mythic)
                adjustedWeight *= (1f + totalBonus * 5f); // Big bonus for mythic on power casts

            adjustedWeights.Add(adjustedWeight);
            totalWeight += adjustedWeight;
        }

        float random = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < fishDatabase.Count; i++)
        {
            cumulative += adjustedWeights[i];
            if (random <= cumulative) return fishDatabase[i];
        }
        return fishDatabase[0];
    }

    public Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.gray;
            case Rarity.Uncommon: return Color.green;
            case Rarity.Rare: return Color.blue;
            case Rarity.Epic: return new Color(0.6f, 0.2f, 0.8f);
            case Rarity.Legendary: return new Color(1f, 0.8f, 0f);
            case Rarity.Mythic: return new Color(1f, 0.3f, 0.3f);
            default: return Color.white;
        }
    }
}
