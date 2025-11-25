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
}

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }

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
        fishDatabase.Add(new FishData { id = "sardine", fishName = "Sardine", rarity = Rarity.Common, coinValue = 5, weight = 60f, fishColor = Color.gray });
        fishDatabase.Add(new FishData { id = "anchovy", fishName = "Anchovy", rarity = Rarity.Common, coinValue = 5, weight = 60f, fishColor = new Color(0.6f, 0.6f, 0.7f) });
        fishDatabase.Add(new FishData { id = "bass", fishName = "Bass", rarity = Rarity.Uncommon, coinValue = 20, weight = 25f, fishColor = new Color(0.3f, 0.5f, 0.3f) });
        fishDatabase.Add(new FishData { id = "salmon", fishName = "Salmon", rarity = Rarity.Uncommon, coinValue = 30, weight = 20f, fishColor = new Color(0.9f, 0.5f, 0.4f) });
        fishDatabase.Add(new FishData { id = "tuna", fishName = "Tuna", rarity = Rarity.Rare, coinValue = 75, weight = 8f, fishColor = new Color(0.2f, 0.3f, 0.6f) });
        fishDatabase.Add(new FishData { id = "swordfish", fishName = "Swordfish", rarity = Rarity.Rare, coinValue = 100, weight = 6f, fishColor = new Color(0.4f, 0.4f, 0.8f) });
        fishDatabase.Add(new FishData { id = "shark", fishName = "Shark", rarity = Rarity.Epic, coinValue = 250, weight = 3f, fishColor = new Color(0.5f, 0.5f, 0.6f) });
        fishDatabase.Add(new FishData { id = "whale", fishName = "Whale", rarity = Rarity.Legendary, coinValue = 1000, weight = 0.7f, fishColor = new Color(0.3f, 0.4f, 0.7f) });
        fishDatabase.Add(new FishData { id = "kraken", fishName = "Baby Kraken", rarity = Rarity.Mythic, coinValue = 5000, weight = 0.1f, fishColor = new Color(0.6f, 0.2f, 0.6f) });
    }

    public bool CanFish()
    {
        return canFish && !isFishing;
    }

    public void StartFishing()
    {
        if (!CanFish()) return;

        // Find the player's rod animator and tell it to cast
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            FishingRodAnimator rodAnimator = player.GetComponent<FishingRodAnimator>();
            if (rodAnimator != null && !rodAnimator.IsLineOut())
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
        FishData fish = GetRandomFish();
        GameManager.Instance.AddCoins(fish.coinValue);
        GameManager.Instance.AddFish(fish);

        // Give XP based on fish rarity
        if (LevelingSystem.Instance != null)
        {
            int xp = LevelingSystem.GetFishXP(fish.rarity);
            LevelingSystem.Instance.AddXP(xp);
        }

        // Update quest progress
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.OnFishCaught(fish.id);
        }

        // Spawn fish and coins from water
        SpawnCatchEffects(fish);

        Debug.Log("Caught: " + fish.fishName + " (" + fish.rarity + ") - +" + fish.coinValue + " coins!");

        isFishing = false;
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

        // Spawn the fish
        GameObject fishObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        fishObj.name = "CaughtFish";
        fishObj.transform.position = spawnPos;

        // Size based on rarity
        float sizeMultiplier = 1f + (int)fish.rarity * 0.15f;
        fishObj.transform.localScale = new Vector3(0.2f * sizeMultiplier, 0.4f * sizeMultiplier, 0.2f * sizeMultiplier);
        fishObj.transform.rotation = Quaternion.Euler(0, 0, 90);
        Object.Destroy(fishObj.GetComponent<Collider>());

        // Add a tail
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tail.transform.SetParent(fishObj.transform);
        tail.transform.localPosition = new Vector3(0, -0.7f, 0);
        tail.transform.localScale = new Vector3(0.5f, 0.4f, 0.1f);
        tail.transform.localRotation = Quaternion.Euler(0, 0, 45);
        Object.Destroy(tail.GetComponent<Collider>());

        Material fishMat = new Material(Shader.Find("Standard"));
        fishMat.color = fish.fishColor;
        fishObj.GetComponent<Renderer>().material = fishMat;
        tail.GetComponent<Renderer>().material = fishMat;

        StartCoroutine(FishCaughtAnimation(fishObj, fish));

        // Spawn coins
        int numCoins = Mathf.Min(fish.coinValue / 5, 15);
        for (int i = 0; i < numCoins; i++)
        {
            StartCoroutine(SpawnCoin(spawnPos, i * 0.08f));
        }
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
            fish.transform.rotation = Quaternion.Euler(wobble, t * 180f, 90 + wobble * 0.5f);

            yield return null;
        }

        // Fade out
        Material mat = fish.GetComponent<Renderer>().material;
        Color originalColor = mat.color;

        // Setup transparency
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            Color c = originalColor;
            c.a = 1 - t;
            mat.color = c;
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

    FishData GetRandomFish()
    {
        // Apply luck bonus - increases rare fish chances
        float luckBonus = UIManager.Instance != null ? UIManager.Instance.GetLuckBonus() : 0f;

        // Adjust weights based on luck
        float totalWeight = 0f;
        List<float> adjustedWeights = new List<float>();

        foreach (var fish in fishDatabase)
        {
            float adjustedWeight = fish.weight;

            if (fish.rarity == Rarity.Common)
                adjustedWeight *= (1f - luckBonus * 0.5f);
            else if (fish.rarity == Rarity.Rare || fish.rarity == Rarity.Epic)
                adjustedWeight *= (1f + luckBonus * 2f);
            else if (fish.rarity == Rarity.Legendary || fish.rarity == Rarity.Mythic)
                adjustedWeight *= (1f + luckBonus * 3f);

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
