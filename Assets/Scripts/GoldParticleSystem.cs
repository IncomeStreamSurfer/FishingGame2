using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gold coin particle system for dramatic catch celebrations
/// Spawns erupting gold coins from the water when Epic, Legendary, or Mythic fish are caught
/// Coins burst upward, arc through the air, and fade out as they fall
/// </summary>
public class GoldParticleSystem : MonoBehaviour
{
    public static GoldParticleSystem Instance { get; private set; }

    [Header("Coin Settings")]
    [Tooltip("Base number of coins for Epic rarity")]
    public int epicCoinCount = 30;

    [Tooltip("Base number of coins for Legendary rarity")]
    public int legendaryCoinCount = 60;

    [Tooltip("Base number of coins for Mythic rarity")]
    public int mythicCoinCount = 120;

    [Header("Burst Physics")]
    [Tooltip("Minimum upward burst velocity")]
    public float minBurstForce = 8f;

    [Tooltip("Maximum upward burst velocity")]
    public float maxBurstForce = 14f;

    [Tooltip("Horizontal spread radius of the burst")]
    public float horizontalSpread = 3f;

    [Tooltip("Gravity applied to falling coins")]
    public float gravity = 15f;

    [Header("Coin Appearance")]
    [Tooltip("Size of each coin (diameter)")]
    public float coinSize = 0.18f;

    [Tooltip("Coin thickness")]
    public float coinThickness = 0.04f;

    [Tooltip("Primary gold color")]
    public Color goldColor = new Color(1f, 0.85f, 0.2f);

    [Tooltip("Emission intensity for coin glow")]
    public float emissionIntensity = 1.2f;

    [Header("Animation")]
    [Tooltip("How long coins last before fading")]
    public float coinLifetime = 2.5f;

    [Tooltip("Coin spin speed (degrees per second)")]
    public float spinSpeed = 720f;

    [Tooltip("Water surface Y position")]
    public float waterLevel = 0.75f;

    // Active coins
    private List<GoldCoin> activeCoins = new List<GoldCoin>();

    // Shared material for performance
    private Material coinMaterial;
    private Material coinMaterialTransparent;

    // Audio
    private AudioSource audioSource;
    private AudioClip coinBurstClip;
    private AudioClip coinShowerClip;

    private class GoldCoin
    {
        public GameObject obj;
        public Vector3 velocity;
        public float age;
        public float lifetime;
        public float spinAxis; // Random spin axis variation
        public bool isFading;
    }

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

        CreateMaterials();
        CreateAudio();
    }

    void CreateMaterials()
    {
        // Create opaque gold material with emission
        coinMaterial = new Material(Shader.Find("Standard"));
        coinMaterial.color = goldColor;
        coinMaterial.SetFloat("_Metallic", 0.95f);
        coinMaterial.SetFloat("_Glossiness", 0.85f);
        coinMaterial.EnableKeyword("_EMISSION");
        coinMaterial.SetColor("_EmissionColor", goldColor * emissionIntensity);

        // Create transparent version for fading
        coinMaterialTransparent = new Material(Shader.Find("Standard"));
        coinMaterialTransparent.color = goldColor;
        coinMaterialTransparent.SetFloat("_Metallic", 0.95f);
        coinMaterialTransparent.SetFloat("_Glossiness", 0.85f);
        coinMaterialTransparent.EnableKeyword("_EMISSION");
        coinMaterialTransparent.SetColor("_EmissionColor", goldColor * emissionIntensity);

        // Setup for transparency
        coinMaterialTransparent.SetFloat("_Mode", 3); // Transparent mode
        coinMaterialTransparent.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        coinMaterialTransparent.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        coinMaterialTransparent.SetInt("_ZWrite", 0);
        coinMaterialTransparent.DisableKeyword("_ALPHATEST_ON");
        coinMaterialTransparent.EnableKeyword("_ALPHABLEND_ON");
        coinMaterialTransparent.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        coinMaterialTransparent.renderQueue = 3000;
    }

    void CreateAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = 0.6f;

        // Generate coin burst sound
        coinBurstClip = GenerateCoinBurstSound();

        // Generate coin shower/rain sound
        coinShowerClip = GenerateCoinShowerSound();
    }

    AudioClip GenerateCoinBurstSound()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Water splash burst at start
            if (t < 0.15f)
            {
                float splashEnv = Mathf.Exp(-t * 20f);
                sample += Random.Range(-1f, 1f) * splashEnv * 0.5f;

                // Low whoosh
                sample += Mathf.Sin(2f * Mathf.PI * 80f * t) * splashEnv * 0.3f;
            }

            // Multiple metallic coin impacts
            for (int c = 0; c < 15; c++)
            {
                float coinTime = c * 0.04f + Random.Range(0f, 0.02f);
                if (t >= coinTime && t < coinTime + 0.08f)
                {
                    float coinT = t - coinTime;
                    float freq = Random.Range(2500f, 4500f);
                    float impact = Mathf.Sin(2f * Mathf.PI * freq * coinT) * Mathf.Exp(-coinT * 25f);

                    // Add harmonics for bell-like quality
                    impact += Mathf.Sin(2f * Mathf.PI * freq * 2.4f * coinT) * Mathf.Exp(-coinT * 30f) * 0.3f;

                    sample += impact * 0.15f;
                }
            }

            samples[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("CoinBurst", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateCoinShowerSound()
    {
        int sampleRate = 44100;
        float duration = 2.0f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Many overlapping coin clinks
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Envelope: ramp up then fade
            float envelope = Mathf.Sin(Mathf.PI * t / duration);

            // Random coin clinks throughout
            for (int c = 0; c < 80; c++)
            {
                float coinTime = c * 0.025f;
                if (t >= coinTime && t < coinTime + 0.06f)
                {
                    float coinT = t - coinTime;
                    float freq = Random.Range(3000f, 5000f);
                    float clink = Mathf.Sin(2f * Mathf.PI * freq * coinT) * Mathf.Exp(-coinT * 35f);
                    sample += clink * 0.08f * envelope;
                }
            }

            // Gentle metallic shimmer undertone
            sample += Mathf.Sin(2f * Mathf.PI * 6000f * t) * 0.02f * envelope * Random.Range(0.5f, 1f);

            samples[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        // Smooth fade out
        int fadeLength = sampleRate / 4;
        for (int i = 0; i < fadeLength; i++)
        {
            float fade = 1f - ((float)i / fadeLength);
            samples[sampleCount - 1 - i] *= fade;
        }

        AudioClip clip = AudioClip.Create("CoinShower", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void Update()
    {
        UpdateCoins();
    }

    void UpdateCoins()
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            GoldCoin coin = activeCoins[i];

            if (coin.obj == null)
            {
                activeCoins.RemoveAt(i);
                continue;
            }

            // Update age
            coin.age += Time.deltaTime;

            // Apply gravity
            coin.velocity.y -= gravity * Time.deltaTime;

            // Move coin
            coin.obj.transform.position += coin.velocity * Time.deltaTime;

            // Spin the coin
            coin.obj.transform.Rotate(
                Vector3.up * spinSpeed * Time.deltaTime +
                Vector3.right * spinSpeed * coin.spinAxis * Time.deltaTime * 0.3f
            );

            // Check for fade start (when falling below water or near end of life)
            float fadeStartTime = coin.lifetime * 0.6f;
            bool shouldFade = coin.age > fadeStartTime || coin.obj.transform.position.y < waterLevel;

            if (shouldFade && !coin.isFading)
            {
                coin.isFading = true;
                // Switch to transparent material
                Renderer renderer = coin.obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = new Material(coinMaterialTransparent);
                }
            }

            // Handle fading
            if (coin.isFading)
            {
                float fadeProgress = Mathf.Clamp01((coin.age - fadeStartTime) / (coin.lifetime - fadeStartTime));

                // Also fade faster if below water
                if (coin.obj.transform.position.y < waterLevel)
                {
                    float belowWaterDepth = waterLevel - coin.obj.transform.position.y;
                    fadeProgress = Mathf.Max(fadeProgress, belowWaterDepth * 2f);
                }

                float alpha = 1f - fadeProgress;

                Renderer renderer = coin.obj.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    Color color = renderer.material.color;
                    color.a = alpha;
                    renderer.material.color = color;

                    Color emissionColor = goldColor * emissionIntensity * alpha;
                    renderer.material.SetColor("_EmissionColor", emissionColor);
                }
            }

            // Remove if lifetime expired or too far below water
            if (coin.age >= coin.lifetime || coin.obj.transform.position.y < waterLevel - 2f)
            {
                Destroy(coin.obj);
                activeCoins.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Spawns a dramatic gold coin burst from the specified position
    /// Call this when catching Epic, Legendary, or Mythic fish
    /// </summary>
    /// <param name="position">World position to spawn coins from (typically water surface where fish was caught)</param>
    /// <param name="rarity">Fish rarity - determines number of coins (Epic/Legendary/Mythic)</param>
    public void SpawnGoldBurst(Vector3 position, Rarity rarity)
    {
        // Only spawn for Epic and higher
        if (rarity < Rarity.Epic)
        {
            return;
        }

        // Determine coin count based on rarity
        int coinCount = GetCoinCountForRarity(rarity);

        // Ensure spawn position is at water level for eruption effect
        Vector3 spawnPos = position;
        spawnPos.y = waterLevel;

        // Play appropriate sound
        PlayBurstSound(rarity);

        // Spawn coins with staggered timing for dramatic effect
        StartCoroutine(SpawnCoinsStaggered(spawnPos, coinCount, rarity));
    }

    int GetCoinCountForRarity(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Epic:
                return epicCoinCount + Random.Range(-5, 6);
            case Rarity.Legendary:
                return legendaryCoinCount + Random.Range(-10, 11);
            case Rarity.Mythic:
                return mythicCoinCount + Random.Range(-15, 16);
            default:
                return 0;
        }
    }

    void PlayBurstSound(Rarity rarity)
    {
        if (audioSource == null) return;

        // Play initial burst
        audioSource.PlayOneShot(coinBurstClip, rarity == Rarity.Mythic ? 1f : 0.7f);

        // Play shower sound for higher rarities
        if (rarity >= Rarity.Legendary)
        {
            float delay = 0.2f;
            StartCoroutine(PlayDelayedSound(coinShowerClip, delay, rarity == Rarity.Mythic ? 0.8f : 0.5f));
        }
    }

    IEnumerator PlayDelayedSound(AudioClip clip, float delay, float volume)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    IEnumerator SpawnCoinsStaggered(Vector3 position, int totalCoins, Rarity rarity)
    {
        // Spawn coins in waves for dramatic eruption effect
        int wavesCount = rarity == Rarity.Mythic ? 5 : (rarity == Rarity.Legendary ? 3 : 2);
        int coinsPerWave = totalCoins / wavesCount;
        float waveDelay = 0.08f;

        // Burst force scales with rarity
        float rarityMultiplier = rarity == Rarity.Mythic ? 1.3f : (rarity == Rarity.Legendary ? 1.15f : 1f);

        for (int wave = 0; wave < wavesCount; wave++)
        {
            // Each subsequent wave has slightly less force (like diminishing fountain)
            float waveForceMultiplier = 1f - (wave * 0.15f);

            for (int i = 0; i < coinsPerWave; i++)
            {
                SpawnSingleCoin(position, rarityMultiplier * waveForceMultiplier);
            }

            yield return new WaitForSeconds(waveDelay);
        }

        // Spawn any remaining coins
        int remaining = totalCoins - (wavesCount * coinsPerWave);
        for (int i = 0; i < remaining; i++)
        {
            SpawnSingleCoin(position, rarityMultiplier * 0.7f);
        }
    }

    void SpawnSingleCoin(Vector3 position, float forceMultiplier)
    {
        GoldCoin coin = new GoldCoin();

        // Create coin as a cylinder (disc shape)
        coin.obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coin.obj.name = "GoldCoinParticle";
        coin.obj.transform.SetParent(transform);
        coin.obj.transform.localScale = new Vector3(coinSize, coinThickness, coinSize);

        // Position at spawn point with slight random offset
        Vector3 spawnOffset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(0f, 0.2f),
            Random.Range(-0.3f, 0.3f)
        );
        coin.obj.transform.position = position + spawnOffset;

        // Random initial rotation
        coin.obj.transform.rotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f)
        );

        // Remove collider
        Collider collider = coin.obj.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        // Apply material
        Renderer renderer = coin.obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = coinMaterial;
        }

        // Calculate burst velocity
        // Primarily upward with random horizontal spread
        float burstForce = Random.Range(minBurstForce, maxBurstForce) * forceMultiplier;
        float horizontalAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float horizontalForce = Random.Range(0f, horizontalSpread);

        coin.velocity = new Vector3(
            Mathf.Cos(horizontalAngle) * horizontalForce,
            burstForce,
            Mathf.Sin(horizontalAngle) * horizontalForce
        );

        // Random spin axis variation
        coin.spinAxis = Random.Range(-1f, 1f);

        // Set lifetime with slight variation
        coin.lifetime = coinLifetime + Random.Range(-0.3f, 0.5f);
        coin.age = 0f;
        coin.isFading = false;

        activeCoins.Add(coin);
    }

    /// <summary>
    /// Spawns a massive celebratory gold burst (for special events like jackpots)
    /// </summary>
    /// <param name="position">World position to spawn coins from</param>
    /// <param name="coinCount">Number of coins to spawn</param>
    public void SpawnMassiveBurst(Vector3 position, int coinCount)
    {
        Vector3 spawnPos = position;
        spawnPos.y = Mathf.Max(spawnPos.y, waterLevel);

        // Play multiple sounds
        if (audioSource != null)
        {
            audioSource.PlayOneShot(coinBurstClip, 1f);
            StartCoroutine(PlayDelayedSound(coinShowerClip, 0.1f, 1f));
            StartCoroutine(PlayDelayedSound(coinShowerClip, 0.4f, 0.8f));
        }

        // Spawn in multiple massive waves
        StartCoroutine(SpawnMassiveWaves(spawnPos, coinCount));
    }

    IEnumerator SpawnMassiveWaves(Vector3 position, int totalCoins)
    {
        int waves = 8;
        int coinsPerWave = totalCoins / waves;

        for (int wave = 0; wave < waves; wave++)
        {
            float waveMultiplier = 1.2f - (wave * 0.1f);

            for (int i = 0; i < coinsPerWave; i++)
            {
                SpawnSingleCoin(position, waveMultiplier);
            }

            yield return new WaitForSeconds(0.06f);
        }
    }

    void OnDestroy()
    {
        // Clean up all active coins
        foreach (GoldCoin coin in activeCoins)
        {
            if (coin.obj != null)
            {
                Destroy(coin.obj);
            }
        }
        activeCoins.Clear();

        // Clean up materials
        if (coinMaterial != null)
            Destroy(coinMaterial);
        if (coinMaterialTransparent != null)
            Destroy(coinMaterialTransparent);
    }
}
