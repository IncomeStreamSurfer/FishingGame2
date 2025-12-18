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
    public bool isRare; // For determining sound effects
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

    // Audio sources for procedural sounds
    private AudioSource audioSource;

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

        // Create audio source for sound effects
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = 0.7f;
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
        Debug.Log("A mysterious bottle appears before you!");

        // Play exciting spawn sound
        PlayBottleSpawnSound();

        // Create bottle
        activeBottle = new GameObject("MessageBottle");

        // Bottle body - NOW GOLDEN AND GLEAMING
        GameObject bottleBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bottleBody.transform.SetParent(activeBottle.transform);
        bottleBody.transform.localPosition = Vector3.zero;
        bottleBody.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
        bottleBody.transform.localRotation = Quaternion.Euler(0, 0, 90);
        Object.Destroy(bottleBody.GetComponent<Collider>());

        // Golden glass material with strong emission
        Material glassMat = new Material(Shader.Find("Standard"));
        glassMat.SetFloat("_Mode", 3);
        glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glassMat.EnableKeyword("_ALPHABLEND_ON");
        glassMat.color = new Color(1f, 0.85f, 0.2f, 0.9f); // Golden color
        glassMat.SetFloat("_Glossiness", 1.0f); // Maximum glossiness
        glassMat.SetFloat("_Metallic", 0.8f); // Metallic look
        glassMat.EnableKeyword("_EMISSION");
        glassMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 2.0f); // Strong golden emission
        bottleBody.GetComponent<Renderer>().material = glassMat;

        // Cork - also golden
        GameObject cork = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cork.transform.SetParent(activeBottle.transform);
        cork.transform.localPosition = new Vector3(0.25f, 0, 0);
        cork.transform.localScale = new Vector3(0.08f, 0.05f, 0.08f);
        cork.transform.localRotation = Quaternion.Euler(0, 0, 90);
        Object.Destroy(cork.GetComponent<Collider>());

        Material corkMat = new Material(Shader.Find("Standard"));
        corkMat.color = new Color(1f, 0.85f, 0.2f); // Golden cork
        corkMat.EnableKeyword("_EMISSION");
        corkMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 0.5f);
        cork.GetComponent<Renderer>().material = corkMat;

        // Paper inside (visible)
        GameObject paper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paper.transform.SetParent(activeBottle.transform);
        paper.transform.localPosition = new Vector3(-0.05f, 0, 0);
        paper.transform.localScale = new Vector3(0.15f, 0.06f, 0.02f);
        paper.transform.localRotation = Quaternion.Euler(0, 15, 90);
        Object.Destroy(paper.GetComponent<Collider>());

        Material paperMat = new Material(Shader.Find("Standard"));
        paperMat.color = new Color(1f, 0.95f, 0.7f); // Slightly golden paper
        paperMat.EnableKeyword("_EMISSION");
        paperMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.5f) * 0.3f);
        paper.GetComponent<Renderer>().material = paperMat;

        // Enhanced glow effect with stronger golden aura
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.transform.SetParent(activeBottle.transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f); // Slightly larger
        Object.Destroy(glow.GetComponent<Collider>());

        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.SetFloat("_Mode", 3);
        glowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glowMat.EnableKeyword("_ALPHABLEND_ON");
        glowMat.color = new Color(1f, 0.9f, 0.3f, 0.5f);
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 1.5f);
        glow.GetComponent<Renderer>().material = glowMat;

        // Add Point Light for golden rays
        GameObject lightObj = new GameObject("GoldenLight");
        lightObj.transform.SetParent(activeBottle.transform);
        lightObj.transform.localPosition = Vector3.zero;
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.85f, 0.2f);
        light.intensity = 3.0f;
        light.range = 10f;
        light.shadows = LightShadows.Soft;

        // Create golden light rays (multiple thin cylinders emanating outward)
        GameObject raysContainer = new GameObject("GoldenRays");
        raysContainer.transform.SetParent(activeBottle.transform);
        raysContainer.transform.localPosition = Vector3.zero;

        Material rayMat = new Material(Shader.Find("Standard"));
        rayMat.SetFloat("_Mode", 3);
        rayMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        rayMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive blending
        rayMat.EnableKeyword("_ALPHABLEND_ON");
        rayMat.color = new Color(1f, 0.85f, 0.2f, 0.3f);
        rayMat.EnableKeyword("_EMISSION");
        rayMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 2.0f);

        // Create 8 rays in a star pattern
        for (int i = 0; i < 8; i++)
        {
            GameObject ray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ray.transform.SetParent(raysContainer.transform);
            Object.Destroy(ray.GetComponent<Collider>());

            float angle = i * 45f; // 360/8 = 45 degrees apart
            ray.transform.localRotation = Quaternion.Euler(0, angle, 0);
            ray.transform.localPosition = Vector3.zero;
            ray.transform.localScale = new Vector3(0.05f, 0.05f, 3f); // Long thin ray

            ray.GetComponent<Renderer>().material = rayMat;
        }

        // Create GIANT GOLDEN QUESTION MARK floating above bottle
        GameObject questionMark = CreateQuestionMark();
        questionMark.transform.SetParent(activeBottle.transform);
        questionMark.transform.localPosition = new Vector3(0, 1.5f, 0); // Float 1.5 units above
        questionMark.transform.localScale = Vector3.one * 0.8f; // Big and visible

        // Spawn bottle DIRECTLY IN FRONT OF PLAYER
        Vector3 playerPos = GameCache.IsPlayerValid() ? GameCache.Player.position : Vector3.zero;
        Vector3 playerForward = GameCache.IsPlayerValid() ? GameCache.Player.forward : Vector3.forward;

        // Position bottle 4-6 units in front of player at water level
        Vector3 spawnPos = playerPos + playerForward * Random.Range(4f, 6f);
        spawnPos.y = 0.4f; // Water surface level

        activeBottle.transform.position = spawnPos;

        // Dramatic entrance: bottle rises from below with scale-in effect
        float entranceTime = 1.5f;
        float t = 0;
        Vector3 startY = spawnPos - Vector3.up * 2f; // Start below water

        while (t < 1f && activeBottle != null)
        {
            t += Time.deltaTime / entranceTime;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic

            Vector3 pos = Vector3.Lerp(startY, spawnPos, easeT);
            activeBottle.transform.position = pos;

            // Scale in effect
            float scale = Mathf.Lerp(0.1f, 1f, easeT);
            activeBottle.transform.localScale = Vector3.one * scale;

            // Rotate dramatically
            activeBottle.transform.Rotate(Vector3.up * 200 * Time.deltaTime);

            // Pulse light
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) / 2f;
            light.intensity = 3.0f + pulse * 2.0f;

            yield return null;
        }

        // Wait for player to click on it (or auto-collect after some time)
        float waitTime = 0;
        float maxWait = 15f; // Increased to give player more time to appreciate the spectacle

        while (waitTime < maxWait && activeBottle != null)
        {
            waitTime += Time.deltaTime;

            // Check if player is close and clicks
            if (GameCache.IsPlayerValid() && Input.GetMouseButtonDown(0))
            {
                float dist = Vector3.Distance(GameCache.Player.position, activeBottle.transform.position);
                if (dist < 5f)
                {
                    OpenBottle();
                    yield break;
                }
            }

            // Keep bobbing and rotating with enhanced effects
            Vector3 pos = activeBottle.transform.position;
            pos.y = spawnPos.y + Mathf.Sin(Time.time * 2f) * 0.15f; // Slower, more pronounced bobbing
            activeBottle.transform.position = pos;
            activeBottle.transform.Rotate(Vector3.up * 30 * Time.deltaTime);

            // Pulse light intensity
            float pulse = (Mathf.Sin(Time.time * 6f) + 1f) / 2f;
            light.intensity = 3.0f + pulse * 2.0f;

            // Rotate the rays for dynamic effect
            raysContainer.transform.Rotate(Vector3.up * 50 * Time.deltaTime);

            // Animate question mark - gentle float up and down
            Transform qmTransform = questionMark.transform;
            Vector3 qmPos = qmTransform.localPosition;
            qmPos.y = 1.5f + Mathf.Sin(Time.time * 3f) * 0.2f;
            qmTransform.localPosition = qmPos;

            // Slowly rotate question mark
            qmTransform.Rotate(Vector3.up * 40 * Time.deltaTime);

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

        // Play collection sound
        PlayCollectionSound(loot);

        // Apply loot
        ApplyLoot(loot);

        // Show effect
        StartCoroutine(BottleOpenEffect(loot));
    }

    BottleLoot RollLoot()
    {
        float roll = Random.value * 100f; // 0-100

        BottleLoot loot = new BottleLoot();

        // 0.1% chance = MEGA JACKPOT (5,000,000 coins!)
        if (roll < 0.1f)
        {
            loot.itemName = "MEGA JACKPOT!!!";
            loot.description = "HOLY FISH! 5,000,000 COINS!!!";
            loot.lootType = LootType.JackpotCoins;
            loot.value = 5000000;
            loot.displayColor = new Color(1f, 0.85f, 0f);
            loot.isRare = true;
            return loot;
        }

        // 0.4% chance = SUPER JACKPOT (1,000,000 coins)
        if (roll < 0.5f)
        {
            loot.itemName = "SUPER JACKPOT!!";
            loot.description = "INCREDIBLE! 1,000,000 coins!!!";
            loot.lootType = LootType.JackpotCoins;
            loot.value = 1000000;
            loot.displayColor = new Color(1f, 0.85f, 0f);
            loot.isRare = true;
            return loot;
        }

        // 1% chance = Epic Fishing Rod
        if (roll < 1.5f)
        {
            loot.itemName = "EPIC FISHING ROD!";
            loot.description = "A LEGENDARY rod of IMMENSE POWER!";
            loot.lootType = LootType.EpicFishingRod;
            loot.value = 1;
            loot.displayColor = new Color(0.6f, 0.2f, 0.8f);
            loot.isRare = true;
            return loot;
        }

        // 3% chance = Golden Fishing Hat
        if (roll < 4.5f)
        {
            loot.itemName = "GOLDEN FISHING HAT!";
            loot.description = "A SHIMMERING GOLDEN HAT! Pure style!";
            loot.lootType = LootType.GoldenFishingHat;
            loot.value = 1;
            loot.displayColor = new Color(1f, 0.85f, 0.2f);
            loot.isRare = true;
            return loot;
        }

        // 8% chance = Groovy Marlin Ring (+10 fishing levels)
        if (roll < 12.5f)
        {
            loot.itemName = "GROOVY MARLIN RING!";
            loot.description = "+10 FISHING LEVELS! Groovy!";
            loot.lootType = LootType.GroovyMarlinRing;
            loot.value = 10;
            loot.displayColor = new Color(0.3f, 0.8f, 1f);
            loot.isRare = true;
            return loot;
        }

        // Rest is random coins or XP with MUCH BETTER REWARDS!
        if (Random.value < 0.5f)
        {
            // Weighted coin distribution (higher chance for better rewards)
            float coinRoll = Random.value;
            int coins;
            string description;

            if (coinRoll < 0.02f) // 2% chance
            {
                // HUGE PAYOUT!
                coins = Random.Range(50000, 100001);
                loot.itemName = $"JACKPOT! {coins} Coins!";
                description = "MASSIVE PAYOUT! You're RICH!";
                loot.isRare = true;
            }
            else if (coinRoll < 0.1f) // 8% chance
            {
                // Big payout
                coins = Random.Range(20000, 50001);
                loot.itemName = $"BIG WIN! {coins} Coins!";
                description = "Wow! That's a lot of coins!";
                loot.isRare = false;
            }
            else if (coinRoll < 0.3f) // 20% chance
            {
                // Good payout
                coins = Random.Range(5000, 20001);
                loot.itemName = $"LUCKY! {coins} Coins!";
                description = "Nice find! Some sweet coins!";
                loot.isRare = false;
            }
            else // 70% chance
            {
                // Standard payout (still generous)
                coins = Random.Range(1000, 5001);
                loot.itemName = $"{coins} Coins";
                description = "Some coins were inside!";
                loot.isRare = false;
            }

            loot.lootType = LootType.Coins;
            loot.value = coins;
            loot.description = description;
            loot.displayColor = new Color(1f, 0.9f, 0.3f);
        }
        else
        {
            // Weighted XP distribution
            float xpRoll = Random.value;
            int xp;
            string description;

            if (xpRoll < 0.02f) // 2% chance
            {
                // MEGA XP!
                xp = Random.Range(50000, 100001);
                loot.itemName = $"WISDOM SURGE! {xp} XP!";
                description = "ANCIENT KNOWLEDGE FLOODS YOUR MIND!";
                loot.isRare = true;
            }
            else if (xpRoll < 0.1f) // 8% chance
            {
                // Big XP
                xp = Random.Range(20000, 50001);
                loot.itemName = $"ENLIGHTENMENT! {xp} XP!";
                description = "The secrets of the sea reveal themselves!";
                loot.isRare = false;
            }
            else if (xpRoll < 0.3f) // 20% chance
            {
                // Good XP
                xp = Random.Range(5000, 20001);
                loot.itemName = $"INSIGHT! {xp} XP!";
                description = "You learn ancient fishing techniques!";
                loot.isRare = false;
            }
            else // 70% chance
            {
                // Standard XP (still generous)
                xp = Random.Range(1000, 5001);
                loot.itemName = $"{xp} XP";
                description = "Ancient fishing knowledge!";
                loot.isRare = false;
            }

            loot.lootType = LootType.XP;
            loot.value = xp;
            loot.description = description;
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
                // Add to wardrobe so player can see and equip it
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.AddToWardrobe("Golden Fishing Hat", "Head", new Color(1f, 0.85f, 0.2f));
                }
                break;

            case LootType.EpicFishingRod:
                hasEpicFishingRod = true;
                // The rod is already unlocked via UIManager checking hasEpicFishingRod
                // But also notify the rod animator to upgrade
                if (FishingRodAnimator.Instance != null)
                {
                    FishingRodAnimator.Instance.SetRodTier(5); // Epic tier
                }
                break;

            case LootType.GroovyMarlinRing:
                hasGroovyMarlinRing = true;
                if (LevelingSystem.Instance != null)
                    LevelingSystem.Instance.SetBonusLevels(10);
                // Add ring to accessory inventory
                if (AccessorySystem.Instance != null)
                {
                    AccessoryItem ring = new AccessoryItem();
                    ring.name = "Groovy Marlin Ring";
                    ring.slot = "Ring";
                    ring.price = 0; // Free from bottle
                    ring.description = "+10 Fishing Levels when worn!";
                    ring.effect = AccessoryEffect.None; // Bonus levels handled by LevelingSystem
                    AccessorySystem.Instance.AddAccessory(ring);
                }
                break;
        }

        Debug.Log($"Bottle Loot: {loot.itemName} - {loot.description}");

        // Show loot notification in UI with exciting messages
        if (UIManager.Instance != null)
        {
            string notificationText = loot.isRare ?
                $"BOTTLE BONANZA! {loot.itemName}" :
                $"Bottle: {loot.itemName}";

            UIManager.Instance.ShowLootNotification(notificationText, loot.displayColor);
        }
    }

    IEnumerator BottleOpenEffect(BottleLoot loot)
    {
        if (activeBottle == null) yield break;

        Vector3 bottlePos = activeBottle.transform.position;

        // Destroy bottle
        Destroy(activeBottle);
        activeBottle = null;

        // MORE PARTICLES for more excitement!
        int particleCount = loot.isRare ? 100 : 40; // Rare loot = MORE PARTICLES!

        // Create explosive particle burst
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = bottlePos;

            // Vary particle sizes for more dynamic effect
            float size = loot.isRare ? Random.Range(0.08f, 0.25f) : Random.Range(0.06f, 0.15f);
            particle.transform.localScale = Vector3.one * size;
            Object.Destroy(particle.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = loot.displayColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", loot.displayColor * (loot.isRare ? 3.0f : 1.5f));

            // Add metallic shine for rare loot
            if (loot.isRare)
            {
                mat.SetFloat("_Metallic", 0.8f);
                mat.SetFloat("_Glossiness", 1.0f);
            }

            particle.GetComponent<Renderer>().material = mat;

            StartCoroutine(ParticleBurst(particle, bottlePos, loot.isRare));
        }

        // Create expanding shockwave ring for rare loot
        if (loot.isRare)
        {
            StartCoroutine(CreateShockwaveRing(bottlePos, loot.displayColor));
        }

        // Create floating text effect showing the loot name
        StartCoroutine(CreateFloatingLootText(bottlePos, loot));

        // Show loot text in console with excitement
        Debug.Log($"*** {loot.itemName} ***");
        Debug.Log($"    {loot.description}");

        yield return new WaitForSeconds(3f);

        bottleActive = false;
    }

    /// <summary>
    /// Creates an expanding shockwave ring for dramatic effect
    /// </summary>
    IEnumerator CreateShockwaveRing(Vector3 origin, Color color)
    {
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.transform.position = origin;
        ring.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);
        Object.Destroy(ring.GetComponent<Collider>());

        Material ringMat = new Material(Shader.Find("Standard"));
        ringMat.SetFloat("_Mode", 3);
        ringMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        ringMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        ringMat.EnableKeyword("_ALPHABLEND_ON");
        ringMat.color = new Color(color.r, color.g, color.b, 0.8f);
        ringMat.EnableKeyword("_EMISSION");
        ringMat.SetColor("_EmissionColor", color * 3.0f);
        ring.GetComponent<Renderer>().material = ringMat;

        float duration = 1.5f;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            // Expand outward
            float scale = Mathf.Lerp(0.1f, 8f, t);
            ring.transform.localScale = new Vector3(scale, 0.02f, scale);

            // Fade out
            Color col = ringMat.color;
            col.a = 0.8f * (1f - t);
            ringMat.color = col;

            yield return null;
        }

        Destroy(ring);
    }

    /// <summary>
    /// Creates floating text showing the loot name
    /// </summary>
    IEnumerator CreateFloatingLootText(Vector3 origin, BottleLoot loot)
    {
        // Create 3D text using multiple cubes (simplified representation)
        GameObject textContainer = new GameObject("LootText");
        textContainer.transform.position = origin + Vector3.up * 2f;

        // Create a glowing sphere as a background
        GameObject textBg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        textBg.transform.SetParent(textContainer.transform);
        textBg.transform.localPosition = Vector3.zero;
        textBg.transform.localScale = Vector3.one * (loot.isRare ? 1.5f : 1.0f);
        Object.Destroy(textBg.GetComponent<Collider>());

        Material bgMat = new Material(Shader.Find("Standard"));
        bgMat.SetFloat("_Mode", 3);
        bgMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        bgMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        bgMat.EnableKeyword("_ALPHABLEND_ON");
        bgMat.color = new Color(loot.displayColor.r, loot.displayColor.g, loot.displayColor.b, 0.3f);
        bgMat.EnableKeyword("_EMISSION");
        bgMat.SetColor("_EmissionColor", loot.displayColor * 2.0f);
        textBg.GetComponent<Renderer>().material = bgMat;

        float duration = 2.5f;
        float t = 0;
        Vector3 startPos = textContainer.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            // Float upward
            textContainer.transform.position = startPos + Vector3.up * t * 2f;

            // Rotate
            textContainer.transform.Rotate(Vector3.up * 60 * Time.deltaTime);

            // Scale pulse
            float scale = (loot.isRare ? 1.5f : 1.0f) * (1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.2f);
            textContainer.transform.localScale = Vector3.one * scale;

            // Fade out near the end
            if (t > 0.7f)
            {
                Color col = bgMat.color;
                col.a = 0.3f * (1f - (t - 0.7f) / 0.3f);
                bgMat.color = col;
            }

            yield return null;
        }

        Destroy(textContainer);
    }

    IEnumerator ParticleBurst(GameObject particle, Vector3 origin, bool isRare)
    {
        Vector3 direction = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 2f),
            Random.Range(-1f, 1f)
        ).normalized;

        // Rare loot = more explosive particles
        float speed = isRare ? Random.Range(5f, 10f) : Random.Range(3f, 6f);
        float initialScale = particle.transform.localScale.x;
        float t = 0;

        // Get particle material for effects
        Renderer renderer = particle.GetComponent<Renderer>();
        Material mat = renderer.material;

        while (t < 1f)
        {
            t += Time.deltaTime * (isRare ? 1.5f : 2f); // Rare particles last longer

            particle.transform.position = origin + direction * speed * t - Vector3.up * t * t * 2f;
            particle.transform.localScale = Vector3.one * initialScale * (1f - t);

            // Add spinning for more drama
            particle.transform.Rotate(Random.onUnitSphere * 500f * Time.deltaTime);

            // Pulse emission for rare particles
            if (isRare)
            {
                float pulse = (Mathf.Sin(Time.time * 20f) + 1f) / 2f;
                Color emissionColor = mat.GetColor("_EmissionColor");
                mat.SetColor("_EmissionColor", emissionColor * (0.5f + pulse * 0.5f));
            }

            yield return null;
        }

        Destroy(particle);
    }

    public bool IsBottleActive()
    {
        return bottleActive;
    }

    GameObject CreateQuestionMark()
    {
        GameObject qm = new GameObject("QuestionMark");

        // Golden emissive material for question mark
        Material qmMat = new Material(Shader.Find("Standard"));
        qmMat.color = new Color(1f, 0.85f, 0.2f);
        qmMat.SetFloat("_Metallic", 0.5f);
        qmMat.SetFloat("_Glossiness", 0.9f);
        qmMat.EnableKeyword("_EMISSION");
        qmMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 3.0f); // Very bright

        // Create the curve of the '?' using spheres
        // Top curve
        GameObject topCurve = new GameObject("TopCurve");
        topCurve.transform.SetParent(qm.transform);
        topCurve.transform.localPosition = new Vector3(0, 0.3f, 0);

        // Top of the curve (small sphere)
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        top.transform.SetParent(topCurve.transform);
        top.transform.localPosition = new Vector3(0, 0.15f, 0);
        top.transform.localScale = Vector3.one * 0.15f;
        Object.Destroy(top.GetComponent<Collider>());
        top.GetComponent<Renderer>().material = qmMat;

        // Right side of curve
        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        right.transform.SetParent(topCurve.transform);
        right.transform.localPosition = new Vector3(0.12f, 0.05f, 0);
        right.transform.localScale = Vector3.one * 0.15f;
        Object.Destroy(right.GetComponent<Collider>());
        right.GetComponent<Renderer>().material = qmMat;

        // Bottom right
        GameObject bottomRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bottomRight.transform.SetParent(topCurve.transform);
        bottomRight.transform.localPosition = new Vector3(0.08f, -0.05f, 0);
        bottomRight.transform.localScale = Vector3.one * 0.15f;
        Object.Destroy(bottomRight.GetComponent<Collider>());
        bottomRight.GetComponent<Renderer>().material = qmMat;

        // Center stem
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stem.transform.SetParent(qm.transform);
        stem.transform.localPosition = new Vector3(0, 0.05f, 0);
        stem.transform.localScale = Vector3.one * 0.15f;
        Object.Destroy(stem.GetComponent<Collider>());
        stem.GetComponent<Renderer>().material = qmMat;

        // Dot at bottom
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.transform.SetParent(qm.transform);
        dot.transform.localPosition = new Vector3(0, -0.2f, 0);
        dot.transform.localScale = Vector3.one * 0.12f;
        Object.Destroy(dot.GetComponent<Collider>());
        dot.GetComponent<Renderer>().material = qmMat;

        // Left side of curve
        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        left.transform.SetParent(topCurve.transform);
        left.transform.localPosition = new Vector3(-0.12f, 0.05f, 0);
        left.transform.localScale = Vector3.one * 0.15f;
        Object.Destroy(left.GetComponent<Collider>());
        left.GetComponent<Renderer>().material = qmMat;

        // Add extra glow sphere around question mark
        GameObject qmGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        qmGlow.transform.SetParent(qm.transform);
        qmGlow.transform.localPosition = Vector3.zero;
        qmGlow.transform.localScale = Vector3.one * 0.8f;
        Object.Destroy(qmGlow.GetComponent<Collider>());

        Material qmGlowMat = new Material(Shader.Find("Standard"));
        qmGlowMat.SetFloat("_Mode", 3);
        qmGlowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        qmGlowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        qmGlowMat.EnableKeyword("_ALPHABLEND_ON");
        qmGlowMat.color = new Color(1f, 0.85f, 0.2f, 0.2f);
        qmGlowMat.EnableKeyword("_EMISSION");
        qmGlowMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.2f) * 2.0f);
        qmGlow.GetComponent<Renderer>().material = qmGlowMat;

        return qm;
    }

    // ==================== PROCEDURAL AUDIO GENERATION ====================

    /// <summary>
    /// Creates a magical whirling swoosh sound with frequency sweep
    /// </summary>
    AudioClip CreateMagicalSwooshSound()
    {
        int sampleRate = 44100;
        float duration = 1.2f;
        int samples = (int)(sampleRate * duration);

        AudioClip clip = AudioClip.Create("MagicalSwoosh", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;

            // Frequency sweep from 200Hz to 2000Hz
            float frequency = Mathf.Lerp(200f, 2000f, t);

            // Add harmonics for magical feel
            float sample = Mathf.Sin(2 * Mathf.PI * frequency * t);
            sample += 0.5f * Mathf.Sin(2 * Mathf.PI * frequency * 2f * t); // 2nd harmonic
            sample += 0.3f * Mathf.Sin(2 * Mathf.PI * frequency * 3f * t); // 3rd harmonic

            // Envelope: fade in and out
            float envelope = Mathf.Sin(Mathf.PI * t);

            data[i] = sample * envelope * 0.3f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Creates a cha-ching money sound with metallic ring
    /// </summary>
    AudioClip CreateChaChing()
    {
        int sampleRate = 44100;
        float duration = 1.5f;
        int samples = (int)(sampleRate * duration);

        AudioClip clip = AudioClip.Create("ChaChing", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;

            // Multiple metallic frequencies
            float sample = 0;
            sample += Mathf.Sin(2 * Mathf.PI * 1200f * t) * Mathf.Exp(-t * 3f); // Main ring
            sample += Mathf.Sin(2 * Mathf.PI * 1800f * t) * Mathf.Exp(-t * 4f); // Harmonic
            sample += Mathf.Sin(2 * Mathf.PI * 2400f * t) * Mathf.Exp(-t * 5f); // High harmonic
            sample += Mathf.Sin(2 * Mathf.PI * 900f * t) * Mathf.Exp(-t * 2.5f); // Low ring

            data[i] = sample * 0.3f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Creates a cork pop sound
    /// </summary>
    AudioClip CreateCorkPop()
    {
        int sampleRate = 44100;
        float duration = 0.3f;
        int samples = (int)(sampleRate * duration);

        AudioClip clip = AudioClip.Create("CorkPop", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;

            // Quick burst of noise with low frequency punch
            float noise = Random.Range(-1f, 1f);
            float lowFreq = Mathf.Sin(2 * Mathf.PI * 80f * t);

            // Sharp attack, quick decay
            float envelope = Mathf.Exp(-t * 15f);

            data[i] = (noise * 0.5f + lowFreq * 0.5f) * envelope * 0.4f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Creates cascading coin sounds
    /// </summary>
    AudioClip CreateCoinShower()
    {
        int sampleRate = 44100;
        float duration = 1.8f;
        int samples = (int)(sampleRate * duration);

        AudioClip clip = AudioClip.Create("CoinShower", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        // Create multiple coin clinks at different times
        for (int coinNum = 0; coinNum < 20; coinNum++)
        {
            float coinStartTime = coinNum * 0.08f; // Stagger coins
            int coinStartSample = (int)(coinStartTime * sampleRate);

            float coinFreq = Random.Range(1000f, 2000f); // Random pitch per coin

            for (int i = coinStartSample; i < samples && i < coinStartSample + 5000; i++)
            {
                float t = (float)(i - coinStartSample) / sampleRate;

                // Metallic ring
                float sample = Mathf.Sin(2 * Mathf.PI * coinFreq * t);
                sample += 0.5f * Mathf.Sin(2 * Mathf.PI * coinFreq * 1.5f * t);

                // Quick decay
                float envelope = Mathf.Exp(-t * 8f);

                data[i] += sample * envelope * 0.08f;
            }
        }

        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Creates a triumphant fanfare for rare loot
    /// </summary>
    AudioClip CreateTriumphantFanfare()
    {
        int sampleRate = 44100;
        float duration = 2.5f;
        int samples = (int)(sampleRate * duration);

        AudioClip clip = AudioClip.Create("Fanfare", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        // Musical notes for fanfare (C, E, G, C)
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f };

        for (int noteIndex = 0; noteIndex < notes.Length; noteIndex++)
        {
            float noteStartTime = noteIndex * 0.4f;
            int noteStartSample = (int)(noteStartTime * sampleRate);
            float noteDuration = 0.6f;
            int noteSamples = (int)(noteDuration * sampleRate);

            for (int i = noteStartSample; i < samples && i < noteStartSample + noteSamples; i++)
            {
                float t = (float)(i - noteStartSample) / sampleRate;

                // Trumpet-like sound with harmonics
                float sample = 0;
                sample += 0.5f * Mathf.Sin(2 * Mathf.PI * notes[noteIndex] * t);
                sample += 0.3f * Mathf.Sin(2 * Mathf.PI * notes[noteIndex] * 2f * t);
                sample += 0.2f * Mathf.Sin(2 * Mathf.PI * notes[noteIndex] * 3f * t);

                // Envelope
                float noteT = t / noteDuration;
                float envelope = Mathf.Sin(Mathf.PI * noteT) * 0.8f;

                data[i] += sample * envelope * 0.15f;
            }
        }

        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Creates an epic jackpot sound with ascending triumph
    /// </summary>
    AudioClip CreateJackpotSound()
    {
        int sampleRate = 44100;
        float duration = 3.5f;
        int samples = (int)(sampleRate * duration);

        AudioClip clip = AudioClip.Create("Jackpot", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;

            // Ascending frequency sweep (victorious)
            float baseFreq = Mathf.Lerp(200f, 800f, progress);

            // Multiple harmonics creating a "chorus" effect
            float sample = 0;
            sample += Mathf.Sin(2 * Mathf.PI * baseFreq * t);
            sample += 0.7f * Mathf.Sin(2 * Mathf.PI * baseFreq * 1.5f * t);
            sample += 0.5f * Mathf.Sin(2 * Mathf.PI * baseFreq * 2f * t);
            sample += 0.3f * Mathf.Sin(2 * Mathf.PI * baseFreq * 3f * t);

            // Add sparkle with high frequencies
            sample += 0.2f * Mathf.Sin(2 * Mathf.PI * 2000f * t) * Mathf.Sin(20f * t);

            // Envelope: build up and sustain
            float envelope = Mathf.Min(1f, progress * 3f);

            data[i] = sample * envelope * 0.2f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Plays the bottle spawn sound (magical swoosh + cha-ching)
    /// </summary>
    void PlayBottleSpawnSound()
    {
        StartCoroutine(PlaySpawnSoundSequence());
    }

    IEnumerator PlaySpawnSoundSequence()
    {
        // Magical swoosh
        AudioClip swoosh = CreateMagicalSwooshSound();
        audioSource.PlayOneShot(swoosh);

        yield return new WaitForSeconds(0.3f);

        // Cha-ching
        AudioClip chaChing = CreateChaChing();
        audioSource.PlayOneShot(chaChing, 0.6f);
    }

    /// <summary>
    /// Plays the collection sound based on loot rarity
    /// </summary>
    void PlayCollectionSound(BottleLoot loot)
    {
        // Cork pop always plays first
        AudioClip corkPop = CreateCorkPop();
        audioSource.PlayOneShot(corkPop, 0.5f);

        // Delay before reward sound
        StartCoroutine(PlayRewardSound(loot));
    }

    IEnumerator PlayRewardSound(BottleLoot loot)
    {
        yield return new WaitForSeconds(0.2f);

        switch (loot.lootType)
        {
            case LootType.JackpotCoins:
                // JACKPOT! Epic sound
                AudioClip jackpot = CreateJackpotSound();
                audioSource.PlayOneShot(jackpot, 1.0f);
                break;

            case LootType.EpicFishingRod:
            case LootType.GoldenFishingHat:
            case LootType.GroovyMarlinRing:
                // Rare items get fanfare
                AudioClip fanfare = CreateTriumphantFanfare();
                audioSource.PlayOneShot(fanfare, 0.8f);
                break;

            case LootType.Coins:
                // Regular coins get coin shower
                if (loot.value >= 5000)
                {
                    // High value coins get fanfare too
                    AudioClip bigCoins = CreateTriumphantFanfare();
                    audioSource.PlayOneShot(bigCoins, 0.7f);
                }
                else
                {
                    AudioClip coins = CreateCoinShower();
                    audioSource.PlayOneShot(coins, 0.6f);
                }
                break;

            case LootType.XP:
                // XP gets magical swoosh
                if (loot.value >= 5000)
                {
                    AudioClip bigXp = CreateTriumphantFanfare();
                    audioSource.PlayOneShot(bigXp, 0.7f);
                }
                else
                {
                    AudioClip xpSound = CreateMagicalSwooshSound();
                    audioSource.PlayOneShot(xpSound, 0.5f);
                }
                break;
        }
    }
}
