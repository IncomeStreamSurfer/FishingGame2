using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Fishing Pool System - Spawns special fishing pools in the water
/// Landing your bobber in a pool gives +50% chance for rare fish
/// Pools last 1 minute and spawn every 2 minutes
/// </summary>
public class FishingPoolSystem : MonoBehaviour
{
    public static FishingPoolSystem Instance { get; private set; }

    [System.Serializable]
    public class FishingPool
    {
        public Vector3 position;
        public float radius;
        public float timeRemaining;
        public GameObject visualObject;
    }

    // Active pools
    public List<FishingPool> activePools = new List<FishingPool>();

    // Spawn settings
    private float poolSpawnTimer = 0f;
    private float poolSpawnInterval = 120f; // 2 minutes between spawns
    private float poolDuration = 60f; // 1 minute duration
    private float poolRadius = 4f; // Size of the pool

    // Water area bounds (where pools can spawn) - around the dock, not under it
    // Dock is at X = -12, width ~5 units, extends from Z ~5 to Z ~55
    private float waterHeight = 0.77f; // Just above water surface (water is at 0.75)

    // Visual settings
    private Material poolMaterial;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        CreatePoolMaterial();
        // Spawn first pool shortly after game starts
        poolSpawnTimer = poolSpawnInterval - 10f;
    }

    void CreatePoolMaterial()
    {
        poolMaterial = new Material(Shader.Find("Standard"));
        poolMaterial.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        poolMaterial.SetFloat("_Mode", 3); // Transparent
        poolMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        poolMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        poolMaterial.SetInt("_ZWrite", 0);
        poolMaterial.DisableKeyword("_ALPHATEST_ON");
        poolMaterial.EnableKeyword("_ALPHABLEND_ON");
        poolMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        poolMaterial.renderQueue = 3000;
        poolMaterial.EnableKeyword("_EMISSION");
        poolMaterial.SetColor("_EmissionColor", new Color(0.1f, 0.4f, 0.6f) * 0.5f);
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Update spawn timer
        poolSpawnTimer += Time.deltaTime;
        if (poolSpawnTimer >= poolSpawnInterval)
        {
            SpawnPool();
            poolSpawnTimer = 0f;
        }

        // Update existing pools
        for (int i = activePools.Count - 1; i >= 0; i--)
        {
            FishingPool pool = activePools[i];
            pool.timeRemaining -= Time.deltaTime;

            // Animate the pool visual
            if (pool.visualObject != null)
            {
                AnimatePool(pool);
            }

            // Remove expired pools
            if (pool.timeRemaining <= 0)
            {
                if (pool.visualObject != null)
                {
                    Destroy(pool.visualObject);
                }
                activePools.RemoveAt(i);
                Debug.Log("Fishing pool expired");
            }
        }
    }

    void SpawnPool()
    {
        // Spawn pools around the dock, not under it
        // Dock is at X = -12, extends from Z ~5 to Z ~55, width ~5 units
        // Player fishes from the end of the dock (around Z = 50-55)

        float x, z;
        int spawnZone = Random.Range(0, 3);

        switch (spawnZone)
        {
            case 0: // In front of dock (main fishing area)
                x = Random.Range(-20f, -4f);
                z = Random.Range(58f, 75f);
                break;
            case 1: // Left side of dock
                x = Random.Range(-30f, -17f);
                z = Random.Range(40f, 60f);
                break;
            default: // Right side of dock
                x = Random.Range(-7f, 5f);
                z = Random.Range(40f, 60f);
                break;
        }

        Vector3 spawnPos = new Vector3(x, waterHeight, z);

        // Create pool data
        FishingPool newPool = new FishingPool
        {
            position = spawnPos,
            radius = poolRadius,
            timeRemaining = poolDuration,
            visualObject = CreatePoolVisual(spawnPos)
        };

        activePools.Add(newPool);

        Debug.Log($"Fishing pool spawned at {spawnPos}! Fish here for bonus rare catches!");

        // Notify player if they're in the game
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("A fishing pool has appeared!", new Color(0.3f, 0.8f, 1f));
        }
    }

    GameObject CreatePoolVisual(Vector3 position)
    {
        // Create parent object
        GameObject poolObj = new GameObject("FishingPool");
        poolObj.transform.position = position;

        // Main swirling ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "PoolRing";
        ring.transform.SetParent(poolObj.transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(poolRadius * 2f, 0.05f, poolRadius * 2f);
        Destroy(ring.GetComponent<Collider>());

        // Apply material
        Renderer rend = ring.GetComponent<Renderer>();
        rend.material = poolMaterial;

        // Inner glow circle
        GameObject innerGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        innerGlow.name = "InnerGlow";
        innerGlow.transform.SetParent(poolObj.transform);
        innerGlow.transform.localPosition = new Vector3(0, 0.02f, 0);
        innerGlow.transform.localScale = new Vector3(poolRadius * 1.2f, 0.03f, poolRadius * 1.2f);
        Destroy(innerGlow.GetComponent<Collider>());

        Material innerMat = new Material(poolMaterial);
        innerMat.color = new Color(0.4f, 0.9f, 1f, 0.3f);
        innerMat.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 0.8f) * 0.8f);
        innerGlow.GetComponent<Renderer>().material = innerMat;

        // Create swirling fish silhouettes (small dark shapes)
        for (int i = 0; i < 5; i++)
        {
            GameObject fishShadow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fishShadow.name = "FishShadow";
            fishShadow.transform.SetParent(poolObj.transform);
            float angle = i * (360f / 5f) * Mathf.Deg2Rad;
            float dist = poolRadius * 0.5f;
            fishShadow.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, -0.1f, Mathf.Sin(angle) * dist);
            fishShadow.transform.localScale = new Vector3(0.4f, 0.15f, 0.8f);
            Destroy(fishShadow.GetComponent<Collider>());

            Material fishMat = new Material(Shader.Find("Standard"));
            fishMat.color = new Color(0.1f, 0.2f, 0.3f, 0.5f);
            fishMat.SetFloat("_Mode", 3);
            fishMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fishMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fishMat.SetInt("_ZWrite", 0);
            fishMat.EnableKeyword("_ALPHABLEND_ON");
            fishMat.renderQueue = 3000;
            fishShadow.GetComponent<Renderer>().material = fishMat;
        }

        // Bubbles indicator
        for (int i = 0; i < 8; i++)
        {
            GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bubble.name = "Bubble";
            bubble.transform.SetParent(poolObj.transform);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(0.5f, poolRadius * 0.8f);
            bubble.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, Random.Range(0f, 0.3f), Mathf.Sin(angle) * dist);
            bubble.transform.localScale = Vector3.one * Random.Range(0.1f, 0.25f);
            Destroy(bubble.GetComponent<Collider>());

            Material bubbleMat = new Material(Shader.Find("Standard"));
            bubbleMat.color = new Color(0.8f, 0.95f, 1f, 0.6f);
            bubbleMat.SetFloat("_Mode", 3);
            bubbleMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            bubbleMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            bubbleMat.SetInt("_ZWrite", 0);
            bubbleMat.EnableKeyword("_ALPHABLEND_ON");
            bubbleMat.renderQueue = 3000;
            bubble.GetComponent<Renderer>().material = bubbleMat;
        }

        return poolObj;
    }

    void AnimatePool(FishingPool pool)
    {
        if (pool.visualObject == null) return;

        float time = Time.time;

        // Rotate the whole pool slowly
        pool.visualObject.transform.Rotate(Vector3.up, 20f * Time.deltaTime);

        // Pulse the scale slightly
        float pulse = 1f + Mathf.Sin(time * 2f) * 0.05f;

        // Animate fish shadows swimming in circles
        int fishIndex = 0;
        foreach (Transform child in pool.visualObject.transform)
        {
            if (child.name == "FishShadow")
            {
                float angle = (time * 0.5f + fishIndex * (360f / 5f)) * Mathf.Deg2Rad;
                float dist = poolRadius * (0.4f + Mathf.Sin(time + fishIndex) * 0.15f);
                child.localPosition = new Vector3(Mathf.Cos(angle) * dist, -0.1f, Mathf.Sin(angle) * dist);

                // Face direction of movement
                Vector3 moveDir = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
                if (moveDir.magnitude > 0.1f)
                    child.rotation = Quaternion.LookRotation(moveDir);

                fishIndex++;
            }
            else if (child.name == "Bubble")
            {
                // Bubbles rise and respawn
                Vector3 pos = child.localPosition;
                pos.y += Time.deltaTime * 0.3f;
                if (pos.y > 0.5f)
                {
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float dist = Random.Range(0.5f, poolRadius * 0.8f);
                    pos = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
                }
                child.localPosition = pos;
            }
        }

        // Fade out effect when pool is about to expire (last 10 seconds)
        if (pool.timeRemaining < 10f)
        {
            float alpha = pool.timeRemaining / 10f;
            foreach (Transform child in pool.visualObject.transform)
            {
                Renderer rend = child.GetComponent<Renderer>();
                if (rend != null && rend.material != null)
                {
                    Color col = rend.material.color;
                    col.a = Mathf.Lerp(0f, col.a, alpha);
                    rend.material.color = col;
                }
            }
        }
    }

    /// <summary>
    /// Check if a position is inside any active fishing pool
    /// </summary>
    public bool IsInFishingPool(Vector3 position)
    {
        foreach (FishingPool pool in activePools)
        {
            float dist = Vector3.Distance(new Vector3(position.x, 0, position.z),
                                          new Vector3(pool.position.x, 0, pool.position.z));
            if (dist <= pool.radius)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get the fishing pool bonus multiplier (1.5 = +50% for rare fish)
    /// </summary>
    public float GetPoolBonusMultiplier(Vector3 bobberPosition)
    {
        if (IsInFishingPool(bobberPosition))
        {
            return 1.5f; // +50% bonus
        }
        return 1f; // No bonus
    }

    void OnDestroy()
    {
        // Cleanup pools
        foreach (FishingPool pool in activePools)
        {
            if (pool.visualObject != null)
            {
                Destroy(pool.visualObject);
            }
        }
        activePools.Clear();

        if (poolMaterial != null)
        {
            Destroy(poolMaterial);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
