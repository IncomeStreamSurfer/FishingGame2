using UnityEngine;
using System.Collections;

[System.Serializable]
public class BottleLoot
{
    public string itemName;
    public string description;
    public LootType lootType;
    public int value; // coins, xp, or item id
    public Color displayColor;
}

public enum LootType
{
    Coins,
    XP,
    GoldenFishingHat,
    EpicFishingRod,
    GroovyMarlinRing,
    JackpotCoins
}

public class BottleEventSystem : MonoBehaviour
{
    public static BottleEventSystem Instance { get; private set; }

    // 1/100 chance per cast
    public float bottleChance = 0.01f;

    private bool bottleActive = false;
    private GameObject activeBottle;

    // Inventory flags for special items
    public bool hasGoldenFishingHat = false;
    public bool hasEpicFishingRod = false;
    public bool hasGroovyMarlinRing = false;

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

    public void OnLineCast()
    {
        if (bottleActive) return;

        // 1/100 chance
        if (Random.value <= bottleChance)
        {
            StartCoroutine(SpawnBottleEvent());
        }
    }

    IEnumerator SpawnBottleEvent()
    {
        bottleActive = true;
        Debug.Log("A bottle is floating in from the horizon!");

        // Create bottle
        activeBottle = new GameObject("MessageBottle");

        // Bottle body
        GameObject bottleBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bottleBody.transform.SetParent(activeBottle.transform);
        bottleBody.transform.localPosition = Vector3.zero;
        bottleBody.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
        bottleBody.transform.localRotation = Quaternion.Euler(0, 0, 90);
        Object.Destroy(bottleBody.GetComponent<Collider>());

        Material glassMat = new Material(Shader.Find("Standard"));
        glassMat.SetFloat("_Mode", 3);
        glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glassMat.EnableKeyword("_ALPHABLEND_ON");
        glassMat.color = new Color(0.3f, 0.6f, 0.3f, 0.7f);
        glassMat.SetFloat("_Glossiness", 0.9f);
        bottleBody.GetComponent<Renderer>().material = glassMat;

        // Cork
        GameObject cork = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cork.transform.SetParent(activeBottle.transform);
        cork.transform.localPosition = new Vector3(0.25f, 0, 0);
        cork.transform.localScale = new Vector3(0.08f, 0.05f, 0.08f);
        cork.transform.localRotation = Quaternion.Euler(0, 0, 90);
        Object.Destroy(cork.GetComponent<Collider>());

        Material corkMat = new Material(Shader.Find("Standard"));
        corkMat.color = new Color(0.6f, 0.45f, 0.3f);
        cork.GetComponent<Renderer>().material = corkMat;

        // Paper inside (visible)
        GameObject paper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paper.transform.SetParent(activeBottle.transform);
        paper.transform.localPosition = new Vector3(-0.05f, 0, 0);
        paper.transform.localScale = new Vector3(0.15f, 0.06f, 0.02f);
        paper.transform.localRotation = Quaternion.Euler(0, 15, 90);
        Object.Destroy(paper.GetComponent<Collider>());

        Material paperMat = new Material(Shader.Find("Standard"));
        paperMat.color = new Color(0.95f, 0.9f, 0.8f);
        paper.GetComponent<Renderer>().material = paperMat;

        // Glow effect
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.transform.SetParent(activeBottle.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        Object.Destroy(glow.GetComponent<Collider>());

        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.SetFloat("_Mode", 3);
        glowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glowMat.EnableKeyword("_ALPHABLEND_ON");
        glowMat.color = new Color(1f, 0.9f, 0.3f, 0.3f);
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.2f) * 0.5f);
        glow.GetComponent<Renderer>().material = glowMat;

        // Start position (far in the water)
        GameObject player = GameObject.Find("Player");
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

        Vector3 startPos = playerPos + new Vector3(Random.Range(-20f, 20f), 0.4f, 50f);
        Vector3 endPos = playerPos + new Vector3(Random.Range(-3f, 3f), 0.4f, Random.Range(5f, 10f));

        activeBottle.transform.position = startPos;

        // Float towards player
        float travelTime = 5f;
        float t = 0;

        while (t < 1f && activeBottle != null)
        {
            t += Time.deltaTime / travelTime;

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            // Bobbing motion
            pos.y = 0.4f + Mathf.Sin(Time.time * 3f) * 0.1f;

            activeBottle.transform.position = pos;
            activeBottle.transform.Rotate(Vector3.up * 30 * Time.deltaTime);
            activeBottle.transform.Rotate(Vector3.forward * Mathf.Sin(Time.time * 2f) * 0.5f);

            // Pulse glow
            float pulse = (Mathf.Sin(Time.time * 4f) + 1f) / 2f;
            glowMat.color = new Color(1f, 0.9f, 0.3f, 0.2f + pulse * 0.2f);

            yield return null;
        }

        // Wait for player to click on it (or auto-collect after some time)
        float waitTime = 0;
        float maxWait = 10f;

        while (waitTime < maxWait && activeBottle != null)
        {
            waitTime += Time.deltaTime;

            // Check if player is close and clicks
            if (player != null && Input.GetMouseButtonDown(0))
            {
                float dist = Vector3.Distance(player.transform.position, activeBottle.transform.position);
                if (dist < 5f)
                {
                    OpenBottle();
                    yield break;
                }
            }

            // Keep bobbing
            Vector3 pos = activeBottle.transform.position;
            pos.y = 0.4f + Mathf.Sin(Time.time * 3f) * 0.1f;
            activeBottle.transform.position = pos;
            activeBottle.transform.Rotate(Vector3.up * 20 * Time.deltaTime);

            yield return null;
        }

        // Bottle floats away if not collected
        if (activeBottle != null)
        {
            Debug.Log("The bottle floated away...");
            Destroy(activeBottle);
            activeBottle = null;
            bottleActive = false;
        }
    }

    void OpenBottle()
    {
        if (activeBottle == null) return;

        BottleLoot loot = RollLoot();

        // Apply loot
        ApplyLoot(loot);

        // Show effect
        StartCoroutine(BottleOpenEffect(loot));
    }

    BottleLoot RollLoot()
    {
        float roll = Random.value * 100f; // 0-100

        BottleLoot loot = new BottleLoot();

        // 0.05% chance (0.0005) = 1,000,000 coins jackpot
        if (roll < 0.05f)
        {
            loot.itemName = "JACKPOT!";
            loot.description = "You found 1,000,000 coins!!!";
            loot.lootType = LootType.JackpotCoins;
            loot.value = 1000000;
            loot.displayColor = new Color(1f, 0.85f, 0f);
            return loot;
        }

        // 1% chance = Epic Fishing Rod
        if (roll < 1.05f)
        {
            loot.itemName = "Epic Fishing Rod";
            loot.description = "A legendary rod of immense power!";
            loot.lootType = LootType.EpicFishingRod;
            loot.value = 1;
            loot.displayColor = new Color(0.6f, 0.2f, 0.8f);
            return loot;
        }

        // 5% chance = Golden Fishing Hat
        if (roll < 6.05f)
        {
            loot.itemName = "Golden Fishing Hat";
            loot.description = "A shimmering golden hat!";
            loot.lootType = LootType.GoldenFishingHat;
            loot.value = 1;
            loot.displayColor = new Color(1f, 0.85f, 0.2f);
            return loot;
        }

        // 10% chance = Groovy Marlin Ring (+10 fishing levels)
        if (roll < 16.05f)
        {
            loot.itemName = "Groovy Marlin Ring";
            loot.description = "+10 Fishing Levels when worn!";
            loot.lootType = LootType.GroovyMarlinRing;
            loot.value = 10;
            loot.displayColor = new Color(0.3f, 0.8f, 1f);
            return loot;
        }

        // Rest is random coins or XP (roughly 50/50)
        if (Random.value < 0.5f)
        {
            // Random coins 10-10000
            int coins = Random.Range(10, 10001);
            loot.itemName = $"{coins} Coins";
            loot.description = "Some coins were inside!";
            loot.lootType = LootType.Coins;
            loot.value = coins;
            loot.displayColor = new Color(1f, 0.9f, 0.3f);
        }
        else
        {
            // Random XP 10-10000
            int xp = Random.Range(10, 10001);
            loot.itemName = $"{xp} XP";
            loot.description = "Ancient fishing knowledge!";
            loot.lootType = LootType.XP;
            loot.value = xp;
            loot.displayColor = new Color(0.3f, 1f, 0.5f);
        }

        return loot;
    }

    void ApplyLoot(BottleLoot loot)
    {
        switch (loot.lootType)
        {
            case LootType.Coins:
            case LootType.JackpotCoins:
                if (GameManager.Instance != null)
                    GameManager.Instance.AddCoins(loot.value);
                break;

            case LootType.XP:
                if (LevelingSystem.Instance != null)
                    LevelingSystem.Instance.AddXP(loot.value);
                break;

            case LootType.GoldenFishingHat:
                hasGoldenFishingHat = true;
                break;

            case LootType.EpicFishingRod:
                hasEpicFishingRod = true;
                // Unlock in UI
                break;

            case LootType.GroovyMarlinRing:
                hasGroovyMarlinRing = true;
                if (LevelingSystem.Instance != null)
                    LevelingSystem.Instance.SetBonusLevels(10);
                break;
        }

        Debug.Log($"Bottle Loot: {loot.itemName} - {loot.description}");

        // Show loot notification in UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"Bottle: {loot.itemName}", loot.displayColor);
        }
    }

    IEnumerator BottleOpenEffect(BottleLoot loot)
    {
        if (activeBottle == null) yield break;

        Vector3 bottlePos = activeBottle.transform.position;

        // Destroy bottle
        Destroy(activeBottle);
        activeBottle = null;

        // Create particle burst
        for (int i = 0; i < 20; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = bottlePos;
            particle.transform.localScale = Vector3.one * 0.1f;
            Object.Destroy(particle.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = loot.displayColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", loot.displayColor * 0.5f);
            particle.GetComponent<Renderer>().material = mat;

            StartCoroutine(ParticleBurst(particle, bottlePos));
        }

        // Show loot text (would be UI in full implementation)
        Debug.Log($"*** {loot.itemName} ***");

        yield return new WaitForSeconds(2f);

        bottleActive = false;
    }

    IEnumerator ParticleBurst(GameObject particle, Vector3 origin)
    {
        Vector3 direction = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 2f),
            Random.Range(-1f, 1f)
        ).normalized;

        float speed = Random.Range(3f, 6f);
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            particle.transform.position = origin + direction * speed * t - Vector3.up * t * t * 2f;
            particle.transform.localScale = Vector3.one * 0.1f * (1f - t);

            yield return null;
        }

        Destroy(particle);
    }

    public bool IsBottleActive()
    {
        return bottleActive;
    }
}
