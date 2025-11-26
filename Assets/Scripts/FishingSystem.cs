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
        // Common fish
        fishDatabase.Add(new FishData { id = "sardine", fishName = "Sardine", rarity = Rarity.Common, coinValue = 5, weight = 50f, fishColor = Color.gray });
        fishDatabase.Add(new FishData { id = "anchovy", fishName = "Anchovy", rarity = Rarity.Common, coinValue = 5, weight = 50f, fishColor = new Color(0.6f, 0.6f, 0.7f) });
        fishDatabase.Add(new FishData { id = "minnow", fishName = "Minnow", rarity = Rarity.Common, coinValue = 3, weight = 60f, fishColor = new Color(0.7f, 0.75f, 0.8f) });
        fishDatabase.Add(new FishData { id = "cod", fishName = "Cod", rarity = Rarity.Common, coinValue = 8, weight = 45f, fishColor = new Color(0.6f, 0.55f, 0.4f) });

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

    FishData GetRandomFish()
    {
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
