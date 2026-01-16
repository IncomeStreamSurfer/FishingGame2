using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Casino-style Level Up Animation System
/// Creates an exciting, dopamine-inducing celebration when the player levels up.
/// Features: gold particles, sparkles, animated text, screen flash, slot machine numbers
/// </summary>
public class LevelUpAnimation : MonoBehaviour
{
    public static LevelUpAnimation Instance { get; private set; }

    /// <summary>
    /// Auto-create the level up animation manager when the game starts if it doesn't exist
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateInstance()
    {
        if (Instance == null)
        {
            Debug.Log("[LevelUpAnimation] Auto-creating instance at runtime");
            GameObject go = new GameObject("LevelUpAnimation");
            go.AddComponent<LevelUpAnimation>();
        }
    }

    [Header("Animation Settings")]
    [Tooltip("Total duration of the level up animation")]
    public float animationDuration = 2.5f;

    [Tooltip("Number of gold coins to spawn")]
    public int goldCoinCount = 40;

    [Tooltip("Number of sparkle particles")]
    public int sparkleCount = 60;

    [Header("Visual Settings")]
    public Color primaryGoldColor = new Color(1f, 0.85f, 0.2f);
    public Color secondaryGoldColor = new Color(1f, 0.95f, 0.5f);
    public Color flashColor = new Color(1f, 0.95f, 0.7f, 0.6f);

    // Animation state
    private bool isAnimating = false;
    private float animationTimer = 0f;
    private int fromLevel = 0;
    private int toLevel = 0;

    // Slot machine number spinning
    private float displayedNumber = 0f;
    private bool spinningComplete = false;
    private float spinStartTime = 0f;
    private float spinDuration = 1.2f;

    // Screen flash
    private float screenFlashAlpha = 0f;
    private Texture2D flashTexture;

    // Particle systems
    private List<CasinoParticle> particles = new List<CasinoParticle>();
    private List<SparkleParticle> sparkles = new List<SparkleParticle>();
    private List<RisingCoin> risingCoins = new List<RisingCoin>();

    // Text animation
    private float textScale = 0f;
    private float textBounce = 0f;
    private float textGlow = 0f;
    private float textShake = 0f;

    // Glow ring animation
    private float ringScale = 0f;
    private float ringAlpha = 0f;

    // Audio
    private AudioSource audioSource;
    private AudioClip levelUpFanfareClip;
    private AudioClip coinShowerClip;
    private AudioClip slotMachineClip;

    // Cached GUI styles
    private GUIStyle levelTextStyle;
    private GUIStyle numberStyle;
    private GUIStyle subTextStyle;

    private class CasinoParticle
    {
        public Vector2 position;
        public Vector2 velocity;
        public float size;
        public float rotation;
        public float rotationSpeed;
        public float alpha;
        public float lifetime;
        public float age;
        public Color color;
        public bool isCoin; // true = coin shape, false = diamond/star
    }

    private class SparkleParticle
    {
        public Vector2 position;
        public float size;
        public float alpha;
        public float lifetime;
        public float age;
        public float twinklePhase;
        public Color color;
    }

    private class RisingCoin
    {
        public Vector2 position;
        public float yVelocity;
        public float size;
        public float alpha;
        public float wobble;
        public float wobbleSpeed;
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
    }

    void Start()
    {
        CreateFlashTexture();
        CreateAudio();
        InitializeStyles();

        // Subscribe to level up events
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.OnLevelUp += TriggerLevelUpAnimation;
        }
    }

    void CreateFlashTexture()
    {
        flashTexture = new Texture2D(1, 1);
        flashTexture.SetPixel(0, 0, Color.white);
        flashTexture.Apply();
    }

    void InitializeStyles()
    {
        // Will be fully initialized in OnGUI for proper styling
        levelTextStyle = new GUIStyle();
        numberStyle = new GUIStyle();
        subTextStyle = new GUIStyle();
    }

    void CreateAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.8f;

        // Generate procedural audio
        levelUpFanfareClip = GenerateLevelUpFanfare();
        coinShowerClip = GenerateCoinShowerSound();
        slotMachineClip = GenerateSlotMachineSound();
    }

    AudioClip GenerateLevelUpFanfare()
    {
        int sampleRate = 44100;
        float duration = 2.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Part 1: Rising triumphant arpeggio (0-0.8s)
            if (t < 0.8f)
            {
                // C major arpeggio going up: C5, E5, G5, C6
                float[] arpeggioFreqs = { 523.25f, 659.25f, 783.99f, 1046.5f };
                int noteIndex = Mathf.Min((int)(t / 0.2f), arpeggioFreqs.Length - 1);
                float noteT = t - noteIndex * 0.2f;

                float noteEnv = Mathf.Sin(noteT / 0.2f * Mathf.PI);
                noteEnv *= Mathf.Min(1f, (0.8f - t) / 0.2f + 0.5f);

                float freq = arpeggioFreqs[noteIndex];
                sample += Mathf.Sin(t * freq * Mathf.PI * 2f) * noteEnv * 0.35f;
                sample += Mathf.Sin(t * freq * 2f * Mathf.PI * 2f) * noteEnv * 0.15f;
                sample += Mathf.Sin(t * freq * 3f * Mathf.PI * 2f) * noteEnv * 0.08f;
            }

            // Part 2: Big triumphant chord hit (0.7s-1.5s)
            if (t >= 0.7f && t < 1.5f)
            {
                float chordT = t - 0.7f;
                float chordEnv = Mathf.Exp(-chordT * 2f);

                // Big C major chord with octave doubling
                sample += Mathf.Sin(t * 261.63f * Mathf.PI * 2f) * chordEnv * 0.3f; // C4
                sample += Mathf.Sin(t * 329.63f * Mathf.PI * 2f) * chordEnv * 0.25f; // E4
                sample += Mathf.Sin(t * 392f * Mathf.PI * 2f) * chordEnv * 0.25f; // G4
                sample += Mathf.Sin(t * 523.25f * Mathf.PI * 2f) * chordEnv * 0.2f; // C5
                sample += Mathf.Sin(t * 659.25f * Mathf.PI * 2f) * chordEnv * 0.15f; // E5

                // Add some sparkle
                sample += Mathf.Sin(t * 1046.5f * Mathf.PI * 2f) * chordEnv * 0.1f; // C6
            }

            // Part 3: Sparkle/shimmer overlay (0.3s-2.2s)
            if (t >= 0.3f && t < 2.2f)
            {
                float shimmerT = (t - 0.3f) / 1.9f;
                float shimmerEnv = Mathf.Sin(shimmerT * Mathf.PI) * 0.15f;

                // High frequency shimmer
                sample += Mathf.Sin(t * 4000f * Mathf.PI * 2f + Mathf.Sin(t * 8f) * 3f) * shimmerEnv;
                sample += Mathf.Sin(t * 5000f * Mathf.PI * 2f + Mathf.Sin(t * 11f) * 3f) * shimmerEnv * 0.7f;
            }

            // Part 4: Victory sustain (1.2s-2.5s)
            if (t >= 1.2f)
            {
                float sustainT = t - 1.2f;
                float sustainEnv = Mathf.Exp(-sustainT * 1.5f) * 0.4f;

                // Sustained major chord
                sample += Mathf.Sin(t * 523.25f * Mathf.PI * 2f) * sustainEnv; // C5
                sample += Mathf.Sin(t * 659.25f * Mathf.PI * 2f) * sustainEnv * 0.7f; // E5
                sample += Mathf.Sin(t * 783.99f * Mathf.PI * 2f) * sustainEnv * 0.5f; // G5

                // Gentle vibrato
                float vibrato = Mathf.Sin(t * 5f * Mathf.PI * 2f) * 0.02f;
                sample += Mathf.Sin(t * 523.25f * (1f + vibrato) * Mathf.PI * 2f) * sustainEnv * 0.2f;
            }

            // Part 5: Coin shower sounds throughout (0.4s-2.0s)
            if (t >= 0.4f && t < 2.0f)
            {
                float coinT = t - 0.4f;
                // Random metallic clinks
                for (int c = 0; c < 30; c++)
                {
                    float coinTime = c * 0.05f;
                    if (coinT >= coinTime && coinT < coinTime + 0.08f)
                    {
                        float ct = coinT - coinTime;
                        float coinEnv = Mathf.Exp(-ct * 30f) * 0.1f;
                        float coinFreq = 3000f + (c % 7) * 500f;
                        sample += Mathf.Sin(t * coinFreq * Mathf.PI * 2f) * coinEnv;
                    }
                }
            }

            samples[i] = Mathf.Clamp(sample * 0.6f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("LevelUpFanfare", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateCoinShowerSound()
    {
        int sampleRate = 44100;
        float duration = 1.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            float env = Mathf.Sin(t / duration * Mathf.PI);

            // Many overlapping coin clinks
            for (int c = 0; c < 50; c++)
            {
                float coinTime = c * 0.03f;
                if (t >= coinTime && t < coinTime + 0.06f)
                {
                    float ct = t - coinTime;
                    float coinEnv = Mathf.Exp(-ct * 35f);
                    float freq = 3500f + (c * 137) % 2000;
                    sample += Mathf.Sin(t * freq * Mathf.PI * 2f) * coinEnv * 0.12f;
                }
            }

            samples[i] = Mathf.Clamp(sample * env, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("CoinShower", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateSlotMachineSound()
    {
        int sampleRate = 44100;
        float duration = 1.2f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Clicking/ratcheting sound that slows down
            float clickRate = Mathf.Lerp(40f, 5f, t / duration);
            float clickPhase = t * clickRate;
            float click = (clickPhase % 1f < 0.1f) ? 1f : 0f;

            if (click > 0)
            {
                float clickT = (clickPhase % 1f) / 0.1f;
                float clickEnv = Mathf.Exp(-clickT * 20f);
                sample += Mathf.Sin(t * 800f * Mathf.PI * 2f) * clickEnv * 0.3f;
                sample += (Random.value - 0.5f) * clickEnv * 0.2f;
            }

            // Reel spinning undertone
            float spinEnv = 1f - (t / duration);
            sample += Mathf.Sin(t * 150f * Mathf.PI * 2f) * spinEnv * 0.1f;

            samples[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("SlotMachine", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Triggers the casino-style level up animation
    /// </summary>
    public void TriggerLevelUpAnimation(int oldLevel, int newLevel)
    {
        if (isAnimating) return;

        fromLevel = oldLevel;
        toLevel = newLevel;
        isAnimating = true;
        animationTimer = 0f;
        spinningComplete = false;
        spinStartTime = 0f;
        displayedNumber = oldLevel;

        // Reset animation values
        textScale = 0f;
        textBounce = 0f;
        textGlow = 0f;
        textShake = 0f;
        ringScale = 0f;
        ringAlpha = 0f;
        screenFlashAlpha = 0f;

        // Clear old particles
        particles.Clear();
        sparkles.Clear();
        risingCoins.Clear();

        // Play sounds
        Debug.Log("[LevelUpAnimation] SOUND CUE: Level Up Fanfare Playing!");
        if (audioSource != null && levelUpFanfareClip != null)
        {
            audioSource.PlayOneShot(levelUpFanfareClip, 0.8f);
        }

        // Start spawning particles
        StartCoroutine(SpawnParticleWaves());

        Debug.Log($"[LevelUpAnimation] CASINO LEVEL UP! {oldLevel} -> {newLevel}");
    }

    IEnumerator SpawnParticleWaves()
    {
        // Wave 1: Initial burst of coins from center
        SpawnCoinBurst(goldCoinCount / 2);
        yield return new WaitForSeconds(0.1f);

        // Play slot machine sound
        if (audioSource != null && slotMachineClip != null)
        {
            audioSource.PlayOneShot(slotMachineClip, 0.5f);
        }

        // Wave 2: Sparkles appearing
        SpawnSparkles(sparkleCount / 2);
        yield return new WaitForSeconds(0.2f);

        // Wave 3: More coins
        SpawnCoinBurst(goldCoinCount / 2);
        yield return new WaitForSeconds(0.1f);

        // Play coin shower
        Debug.Log("[LevelUpAnimation] SOUND CUE: Coin Shower!");
        if (audioSource != null && coinShowerClip != null)
        {
            audioSource.PlayOneShot(coinShowerClip, 0.6f);
        }

        // Wave 4: Rising coins from bottom
        SpawnRisingCoins(15);
        yield return new WaitForSeconds(0.2f);

        // Wave 5: Final sparkle burst
        SpawnSparkles(sparkleCount / 2);
    }

    void SpawnCoinBurst(int count)
    {
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        for (int i = 0; i < count; i++)
        {
            CasinoParticle p = new CasinoParticle();
            p.position = center + new Vector2(Random.Range(-50f, 50f), Random.Range(-50f, 50f));

            // Burst outward in all directions
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(200f, 500f);
            p.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

            p.size = Random.Range(15f, 30f);
            p.rotation = Random.Range(0f, 360f);
            p.rotationSpeed = Random.Range(-360f, 360f);
            p.alpha = 1f;
            p.lifetime = Random.Range(1.5f, 2.5f);
            p.age = 0f;
            p.isCoin = Random.value > 0.3f;
            p.color = Random.value > 0.5f ? primaryGoldColor : secondaryGoldColor;

            particles.Add(p);
        }
    }

    void SpawnSparkles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SparkleParticle s = new SparkleParticle();
            s.position = new Vector2(
                Random.Range(Screen.width * 0.2f, Screen.width * 0.8f),
                Random.Range(Screen.height * 0.2f, Screen.height * 0.8f)
            );
            s.size = Random.Range(3f, 12f);
            s.alpha = 0f;
            s.lifetime = Random.Range(0.8f, 1.5f);
            s.age = 0f;
            s.twinklePhase = Random.Range(0f, Mathf.PI * 2f);
            s.color = Random.value > 0.3f ? Color.white : secondaryGoldColor;

            sparkles.Add(s);
        }
    }

    void SpawnRisingCoins(int count)
    {
        for (int i = 0; i < count; i++)
        {
            RisingCoin rc = new RisingCoin();
            rc.position = new Vector2(
                Random.Range(Screen.width * 0.1f, Screen.width * 0.9f),
                Screen.height + 50f
            );
            rc.yVelocity = Random.Range(-300f, -500f);
            rc.size = Random.Range(20f, 40f);
            rc.alpha = 1f;
            rc.wobble = 0f;
            rc.wobbleSpeed = Random.Range(5f, 10f);

            risingCoins.Add(rc);
        }
    }

    void Update()
    {
        if (!isAnimating) return;

        animationTimer += Time.deltaTime;

        // Update screen flash
        UpdateScreenFlash();

        // Update text animations
        UpdateTextAnimation();

        // Update glow ring
        UpdateGlowRing();

        // Update slot machine number
        UpdateSlotMachine();

        // Update particles
        UpdateParticles();
        UpdateSparkles();
        UpdateRisingCoins();

        // Check if animation is complete
        if (animationTimer >= animationDuration)
        {
            isAnimating = false;
            particles.Clear();
            sparkles.Clear();
            risingCoins.Clear();
        }
    }

    void UpdateScreenFlash()
    {
        // Big flash at start, then pulses
        if (animationTimer < 0.1f)
        {
            screenFlashAlpha = Mathf.Lerp(0f, 0.7f, animationTimer / 0.1f);
        }
        else if (animationTimer < 0.4f)
        {
            screenFlashAlpha = Mathf.Lerp(0.7f, 0f, (animationTimer - 0.1f) / 0.3f);
        }
        else
        {
            // Gentle pulses
            float pulse = Mathf.Sin(animationTimer * 8f) * 0.5f + 0.5f;
            screenFlashAlpha = pulse * 0.1f * (1f - animationTimer / animationDuration);
        }
    }

    void UpdateTextAnimation()
    {
        // Text scale: Pop in with overshoot, then settle
        if (animationTimer < 0.3f)
        {
            float t = animationTimer / 0.3f;
            // Elastic ease out
            textScale = 1f - Mathf.Pow(2f, -10f * t) * Mathf.Cos(t * Mathf.PI * 3f);
            textScale = Mathf.Clamp(textScale, 0f, 1.5f);
        }
        else if (animationTimer < 0.5f)
        {
            textScale = Mathf.Lerp(1.3f, 1f, (animationTimer - 0.3f) / 0.2f);
        }
        else
        {
            textScale = 1f;
        }

        // Bouncing effect
        textBounce = Mathf.Sin(animationTimer * 12f) * Mathf.Max(0f, 1f - animationTimer / 1.5f) * 15f;

        // Glow pulsing
        textGlow = Mathf.Sin(animationTimer * 6f) * 0.5f + 0.5f;

        // Shake effect (decreases over time)
        float shakeIntensity = Mathf.Max(0f, 1f - animationTimer / 0.8f);
        textShake = (Random.value - 0.5f) * 10f * shakeIntensity;
    }

    void UpdateGlowRing()
    {
        // Ring expands outward
        if (animationTimer < 1.5f)
        {
            ringScale = animationTimer / 1.5f;
            ringAlpha = Mathf.Sin(ringScale * Mathf.PI);
        }
        else
        {
            ringAlpha = 0f;
        }
    }

    void UpdateSlotMachine()
    {
        if (spinningComplete) return;

        if (spinStartTime == 0f)
        {
            spinStartTime = animationTimer;
        }

        float spinProgress = (animationTimer - spinStartTime) / spinDuration;

        if (spinProgress >= 1f)
        {
            displayedNumber = toLevel;
            spinningComplete = true;
        }
        else
        {
            // Slot machine easing - fast at start, slows down dramatically at end
            float eased = 1f - Mathf.Pow(1f - spinProgress, 4f);

            // During spin, cycle through numbers rapidly, slowing down
            float cycleSpeed = Mathf.Lerp(30f, 0f, eased);
            int cycleRange = toLevel - fromLevel + 10;
            float cycleOffset = Mathf.Sin(animationTimer * cycleSpeed) * cycleRange;

            displayedNumber = Mathf.Lerp(fromLevel + cycleOffset, toLevel, eased);
        }
    }

    void UpdateParticles()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            CasinoParticle p = particles[i];
            p.age += Time.deltaTime;

            if (p.age >= p.lifetime)
            {
                particles.RemoveAt(i);
                continue;
            }

            // Apply gravity
            p.velocity.y -= 400f * Time.deltaTime;

            // Apply drag
            p.velocity *= 0.98f;

            // Move
            p.position += p.velocity * Time.deltaTime;

            // Rotate
            p.rotation += p.rotationSpeed * Time.deltaTime;

            // Fade out near end of life
            float lifeProgress = p.age / p.lifetime;
            if (lifeProgress > 0.7f)
            {
                p.alpha = 1f - (lifeProgress - 0.7f) / 0.3f;
            }
        }
    }

    void UpdateSparkles()
    {
        for (int i = sparkles.Count - 1; i >= 0; i--)
        {
            SparkleParticle s = sparkles[i];
            s.age += Time.deltaTime;

            if (s.age >= s.lifetime)
            {
                sparkles.RemoveAt(i);
                continue;
            }

            // Twinkle effect
            float lifeProgress = s.age / s.lifetime;
            float fadeIn = Mathf.Min(1f, s.age / 0.2f);
            float fadeOut = lifeProgress > 0.7f ? 1f - (lifeProgress - 0.7f) / 0.3f : 1f;
            float twinkle = Mathf.Sin(s.age * 15f + s.twinklePhase) * 0.5f + 0.5f;

            s.alpha = fadeIn * fadeOut * twinkle;
        }
    }

    void UpdateRisingCoins()
    {
        for (int i = risingCoins.Count - 1; i >= 0; i--)
        {
            RisingCoin rc = risingCoins[i];

            // Move up
            rc.position.y += rc.yVelocity * Time.deltaTime;

            // Slow down
            rc.yVelocity *= 0.98f;

            // Wobble side to side
            rc.wobble += rc.wobbleSpeed * Time.deltaTime;
            rc.position.x += Mathf.Sin(rc.wobble) * 2f;

            // Fade when past top of screen or slowed down
            if (rc.position.y < -50f || Mathf.Abs(rc.yVelocity) < 50f)
            {
                rc.alpha -= Time.deltaTime * 2f;
            }

            if (rc.alpha <= 0f)
            {
                risingCoins.RemoveAt(i);
            }
        }
    }

    void OnGUI()
    {
        if (!isAnimating) return;

        // Draw screen flash overlay
        DrawScreenFlash();

        // Draw glow ring
        DrawGlowRing();

        // Draw particles
        DrawParticles();
        DrawSparkles();
        DrawRisingCoins();

        // Draw main text
        DrawLevelUpText();
    }

    void DrawScreenFlash()
    {
        if (screenFlashAlpha <= 0f) return;

        Color flashCol = flashColor;
        flashCol.a = screenFlashAlpha;
        GUI.color = flashCol;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), flashTexture);
        GUI.color = Color.white;
    }

    void DrawGlowRing()
    {
        if (ringAlpha <= 0f) return;

        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        float maxRadius = Mathf.Min(Screen.width, Screen.height) * 0.4f;
        float currentRadius = maxRadius * ringScale;

        // Draw multiple concentric rings for glow effect
        for (int r = 0; r < 5; r++)
        {
            float ringR = currentRadius - r * 15f;
            if (ringR <= 0) continue;

            Color ringColor = primaryGoldColor;
            ringColor.a = ringAlpha * (1f - r * 0.2f) * 0.5f;
            GUI.color = ringColor;

            // Draw ring as a series of small rectangles (simplified circle)
            int segments = 36;
            for (int s = 0; s < segments; s++)
            {
                float angle = s * (360f / segments) * Mathf.Deg2Rad;
                float x = center.x + Mathf.Cos(angle) * ringR - 4;
                float y = center.y + Mathf.Sin(angle) * ringR - 4;
                GUI.DrawTexture(new Rect(x, y, 8, 8), flashTexture);
            }
        }
        GUI.color = Color.white;
    }

    void DrawParticles()
    {
        foreach (CasinoParticle p in particles)
        {
            if (p.alpha <= 0f) continue;

            Color col = p.color;
            col.a = p.alpha;
            GUI.color = col;

            // Save matrix for rotation
            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(p.rotation, p.position);

            if (p.isCoin)
            {
                // Draw coin as golden circle (using texture)
                Rect coinRect = new Rect(
                    p.position.x - p.size / 2,
                    p.position.y - p.size / 2,
                    p.size,
                    p.size
                );
                GUI.DrawTexture(coinRect, flashTexture);

                // Inner highlight
                Color highlight = Color.white;
                highlight.a = p.alpha * 0.5f;
                GUI.color = highlight;
                Rect innerRect = new Rect(
                    p.position.x - p.size / 4,
                    p.position.y - p.size / 4,
                    p.size / 2,
                    p.size / 2
                );
                GUI.DrawTexture(innerRect, flashTexture);
            }
            else
            {
                // Draw diamond/star shape
                float halfSize = p.size / 2;
                // Diamond points
                GUI.DrawTexture(new Rect(p.position.x - 2, p.position.y - halfSize, 4, p.size), flashTexture);
                GUI.DrawTexture(new Rect(p.position.x - halfSize, p.position.y - 2, p.size, 4), flashTexture);
            }

            GUI.matrix = oldMatrix;
        }
        GUI.color = Color.white;
    }

    void DrawSparkles()
    {
        foreach (SparkleParticle s in sparkles)
        {
            if (s.alpha <= 0f) continue;

            Color col = s.color;
            col.a = s.alpha;
            GUI.color = col;

            // Draw sparkle as a cross/star
            float size = s.size;
            GUI.DrawTexture(new Rect(s.position.x - 1, s.position.y - size, 2, size * 2), flashTexture);
            GUI.DrawTexture(new Rect(s.position.x - size, s.position.y - 1, size * 2, 2), flashTexture);

            // Diagonal lines for star effect
            float diagSize = size * 0.7f;
            GUI.DrawTexture(new Rect(s.position.x - 1, s.position.y - diagSize, 2, diagSize * 2), flashTexture);
        }
        GUI.color = Color.white;
    }

    void DrawRisingCoins()
    {
        foreach (RisingCoin rc in risingCoins)
        {
            if (rc.alpha <= 0f) continue;

            Color col = primaryGoldColor;
            col.a = rc.alpha;
            GUI.color = col;

            Rect coinRect = new Rect(
                rc.position.x - rc.size / 2,
                rc.position.y - rc.size / 2,
                rc.size,
                rc.size
            );
            GUI.DrawTexture(coinRect, flashTexture);

            // Highlight
            Color highlight = secondaryGoldColor;
            highlight.a = rc.alpha * 0.7f;
            GUI.color = highlight;
            Rect innerRect = new Rect(
                rc.position.x - rc.size / 4,
                rc.position.y - rc.size / 4,
                rc.size / 2,
                rc.size / 2
            );
            GUI.DrawTexture(innerRect, flashTexture);
        }
        GUI.color = Color.white;
    }

    void DrawLevelUpText()
    {
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Calculate animated position
        float yOffset = textBounce;
        float xOffset = textShake;

        // "LEVEL UP!" text
        GUIStyle mainStyle = new GUIStyle();
        mainStyle.fontSize = (int)(48 * textScale);
        mainStyle.fontStyle = FontStyle.Bold;
        mainStyle.alignment = TextAnchor.MiddleCenter;

        // Glow/outline effect - draw multiple times with offset
        Color glowColor = primaryGoldColor;
        glowColor.a = textGlow * 0.5f;

        for (int gx = -2; gx <= 2; gx++)
        {
            for (int gy = -2; gy <= 2; gy++)
            {
                if (gx == 0 && gy == 0) continue;
                mainStyle.normal.textColor = glowColor;
                GUI.Label(new Rect(xOffset + gx * 2, center.y - 80 + yOffset + gy * 2, Screen.width, 60),
                    "LEVEL UP!", mainStyle);
            }
        }

        // Main text with gradient effect (gold to white)
        Color mainColor = Color.Lerp(primaryGoldColor, Color.white, textGlow * 0.3f);
        mainStyle.normal.textColor = mainColor;
        GUI.Label(new Rect(xOffset, center.y - 80 + yOffset, Screen.width, 60), "LEVEL UP!", mainStyle);

        // Level number with slot machine effect
        GUIStyle numberStyleLocal = new GUIStyle();
        numberStyleLocal.fontSize = (int)(72 * textScale);
        numberStyleLocal.fontStyle = FontStyle.Bold;
        numberStyleLocal.alignment = TextAnchor.MiddleCenter;

        // Number glow
        Color numGlow = secondaryGoldColor;
        numGlow.a = textGlow * 0.6f;
        for (int gx = -3; gx <= 3; gx++)
        {
            for (int gy = -3; gy <= 3; gy++)
            {
                if (gx == 0 && gy == 0) continue;
                numberStyleLocal.normal.textColor = numGlow;
                GUI.Label(new Rect(xOffset + gx * 2, center.y - 20 + yOffset + gy * 2, Screen.width, 80),
                    Mathf.RoundToInt(displayedNumber).ToString(), numberStyleLocal);
            }
        }

        // Main number
        numberStyleLocal.normal.textColor = Color.white;
        GUI.Label(new Rect(xOffset, center.y - 20 + yOffset, Screen.width, 80),
            Mathf.RoundToInt(displayedNumber).ToString(), numberStyleLocal);

        // Subtitle text
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = (int)(24 * Mathf.Min(1f, textScale));
        subStyle.fontStyle = FontStyle.Bold;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(1f, 1f, 1f, 0.8f);

        // Only show subtitle after spin completes
        if (spinningComplete)
        {
            string subText = GetLevelUpFlavorText();
            GUI.Label(new Rect(xOffset, center.y + 60 + yOffset, Screen.width, 40), subText, subStyle);
        }
    }

    string GetLevelUpFlavorText()
    {
        // Casino-style congratulatory messages
        string[] messages = {
            "JACKPOT!",
            "BIG WINNER!",
            "YOU'RE ON FIRE!",
            "INCREDIBLE!",
            "LEGENDARY!",
            "UNSTOPPABLE!",
            "FISHING MASTER!",
            "PURE GOLD!",
            "AMAZING!",
            "CHAMPION!"
        };

        // Pick based on level to be consistent
        return messages[toLevel % messages.Length];
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.OnLevelUp -= TriggerLevelUpAnimation;
        }

        // Clean up textures
        if (flashTexture != null)
        {
            Destroy(flashTexture);
        }

        // Clean up audio clips
        if (levelUpFanfareClip != null) Destroy(levelUpFanfareClip);
        if (coinShowerClip != null) Destroy(coinShowerClip);
        if (slotMachineClip != null) Destroy(slotMachineClip);
    }

    /// <summary>
    /// Manually trigger the animation (for testing)
    /// </summary>
    public void TestAnimation()
    {
        int currentLevel = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;
        TriggerLevelUpAnimation(currentLevel, currentLevel + 1);
    }
}
