using UnityEngine;

/// <summary>
/// Manages casino-style sound effects for the fishing game.
/// Provides jackpot sounds, coin cascades, and game over music.
/// All sounds are procedurally generated using waveform synthesis.
/// </summary>
public class CasinoAudioManager : MonoBehaviour
{
    public static CasinoAudioManager Instance { get; private set; }

    /// <summary>
    /// Auto-create the casino audio manager when the game starts if it doesn't exist
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateInstance()
    {
        if (Instance == null)
        {
            Debug.Log("[CasinoAudioManager] Auto-creating instance at runtime");
            GameObject go = new GameObject("CasinoAudioManager");
            go.AddComponent<CasinoAudioManager>();
        }
    }

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1.0f;
    [Range(0f, 1f)] public float jackpotVolume = 1.0f;
    [Range(0f, 1f)] public float coinVolume = 0.8f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;

    // Audio sources
    private AudioSource jackpotSource;
    private AudioSource coinSource;
    private AudioSource musicSource;

    // Cached audio clips (procedurally generated)
    private AudioClip jackpotEpicClip;
    private AudioClip jackpotLegendaryClip;
    private AudioClip jackpotMythicClip;
    private AudioClip coinCascadeClip;
    private AudioClip coinCascadeLongClip;
    private AudioClip gameOverMusicClip;
    private AudioClip singleCoinClip;

    void Awake()
    {
        Debug.Log("[CasinoAudioManager] Awake() called");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[CasinoAudioManager] Instance registered and marked DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[CasinoAudioManager] Duplicate instance detected - destroying");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("[CasinoAudioManager] Start() called - initializing casino sound system");
        CreateAudioSources();
        GenerateAllAudioClips();
        Debug.Log("[CasinoAudioManager] Casino sound system initialized successfully");
    }

    void CreateAudioSources()
    {
        Debug.Log("[CasinoAudioManager] CreateAudioSources called");

        // Jackpot sounds (cash register, ka-ching)
        jackpotSource = gameObject.AddComponent<AudioSource>();
        jackpotSource.loop = false;
        jackpotSource.spatialBlend = 0f; // 2D sound
        jackpotSource.priority = 16; // High priority
        jackpotSource.playOnAwake = false;

        // Coin sounds (cascade, clinking)
        coinSource = gameObject.AddComponent<AudioSource>();
        coinSource.loop = false;
        coinSource.spatialBlend = 0f;
        coinSource.priority = 32;
        coinSource.playOnAwake = false;

        // Music source (for game over music)
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.spatialBlend = 0f;
        musicSource.priority = 64;
        musicSource.playOnAwake = false;

        Debug.Log("[CasinoAudioManager] Audio sources created");
    }

    void GenerateAllAudioClips()
    {
        Debug.Log("[CasinoAudioManager] Generating procedural audio clips...");

        // Jackpot sounds - different intensities
        jackpotEpicClip = GenerateJackpotSound(1.5f, 0.7f); // Shorter, moderate
        jackpotLegendaryClip = GenerateJackpotSound(2.0f, 0.85f); // Medium
        jackpotMythicClip = GenerateJackpotSound(3.0f, 1.0f); // Full jackpot experience

        // Coin sounds
        singleCoinClip = GenerateSingleCoinSound();
        coinCascadeClip = GenerateCoinCascadeSound(1.5f);
        coinCascadeLongClip = GenerateCoinCascadeSound(3.0f);

        // Game over music
        gameOverMusicClip = GenerateGameOverMusic();

        Debug.Log("[CasinoAudioManager] All audio clips generated");
    }

    // === PUBLIC METHODS ===

    /// <summary>
    /// Play jackpot/cash register sound based on fish rarity.
    /// Bigger sound for higher rarity catches.
    /// </summary>
    /// <param name="rarity">The rarity of the caught fish</param>
    public void PlayJackpotSound(Rarity rarity)
    {
        Debug.Log("[CasinoAudioManager] PlayJackpotSound called - rarity: " + rarity);

        AudioClip clip = null;
        float volume = jackpotVolume * masterVolume;

        switch (rarity)
        {
            case Rarity.Epic:
                clip = jackpotEpicClip;
                volume *= 0.8f;
                break;
            case Rarity.Legendary:
                clip = jackpotLegendaryClip;
                volume *= 0.9f;
                break;
            case Rarity.Mythic:
                clip = jackpotMythicClip;
                volume *= 1.0f;
                break;
            default:
                // No jackpot sound for Common, Uncommon, Rare
                Debug.Log("[CasinoAudioManager] Rarity too low for jackpot sound");
                return;
        }

        if (clip != null && jackpotSource != null)
        {
            jackpotSource.pitch = 1.0f;
            jackpotSource.volume = volume;
            jackpotSource.PlayOneShot(clip);
            Debug.Log("[CasinoAudioManager] Playing jackpot sound - volume: " + volume);
        }
    }

    /// <summary>
    /// Play jackpot sound using integer rarity (for compatibility).
    /// 3 = Epic, 4 = Legendary, 5 = Mythic
    /// </summary>
    public void PlayJackpotSound(int rarityLevel)
    {
        Rarity rarity;
        switch (rarityLevel)
        {
            case 3: rarity = Rarity.Epic; break;
            case 4: rarity = Rarity.Legendary; break;
            case 5: rarity = Rarity.Mythic; break;
            default: return; // No jackpot for lower rarities
        }
        PlayJackpotSound(rarity);
    }

    /// <summary>
    /// Play coin cascade sound (for when gold particles appear).
    /// </summary>
    /// <param name="long">If true, plays a longer cascade</param>
    public void PlayCoinCascade(bool longCascade = false)
    {
        Debug.Log("[CasinoAudioManager] PlayCoinCascade called - long: " + longCascade);

        AudioClip clip = longCascade ? coinCascadeLongClip : coinCascadeClip;

        if (clip != null && coinSource != null)
        {
            coinSource.pitch = Random.Range(0.95f, 1.05f);
            coinSource.volume = coinVolume * masterVolume;
            coinSource.PlayOneShot(clip);
            Debug.Log("[CasinoAudioManager] Playing coin cascade - volume: " + coinSource.volume);
        }
    }

    /// <summary>
    /// Play a single coin clink sound.
    /// </summary>
    public void PlaySingleCoin()
    {
        Debug.Log("[CasinoAudioManager] PlaySingleCoin called");

        if (singleCoinClip != null && coinSource != null)
        {
            coinSource.pitch = Random.Range(0.9f, 1.1f);
            coinSource.volume = coinVolume * masterVolume * 0.6f;
            coinSource.PlayOneShot(singleCoinClip);
        }
    }

    /// <summary>
    /// Play dramatic game over music when player loses (health reaches 0).
    /// </summary>
    public void PlayGameOverMusic()
    {
        Debug.Log("[CasinoAudioManager] PlayGameOverMusic called");

        if (gameOverMusicClip != null && musicSource != null)
        {
            // Stop any currently playing music
            musicSource.Stop();

            musicSource.clip = gameOverMusicClip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.loop = false;
            musicSource.Play();
            Debug.Log("[CasinoAudioManager] Playing game over music - volume: " + musicSource.volume);
        }
    }

    /// <summary>
    /// Stop the game over music if it's playing.
    /// </summary>
    public void StopGameOverMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[CasinoAudioManager] Game over music stopped");
        }
    }

    /// <summary>
    /// Check if game over music is currently playing.
    /// </summary>
    public bool IsGameOverMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying && musicSource.clip == gameOverMusicClip;
    }

    // === PROCEDURAL AUDIO GENERATION ===

    /// <summary>
    /// Generate a cash register "ka-ching" jackpot sound.
    /// </summary>
    AudioClip GenerateJackpotSound(float duration, float intensity)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Part 1: Initial "ka" - metallic click (0 to 0.1s)
            if (t < 0.1f)
            {
                float clickT = t / 0.1f;
                float clickEnv = Mathf.Exp(-clickT * 30f);

                // Metallic frequencies
                sample += Mathf.Sin(t * 3500f * Mathf.PI * 2f) * clickEnv * 0.5f;
                sample += Mathf.Sin(t * 2800f * Mathf.PI * 2f) * clickEnv * 0.4f;
                sample += Mathf.Sin(t * 4200f * Mathf.PI * 2f) * clickEnv * 0.3f;

                // Add impact noise
                sample += (Random.value - 0.5f) * clickEnv * 0.6f;
            }

            // Part 2: "ching" - bell/chime sound (0.08s onwards)
            if (t >= 0.08f)
            {
                float chingT = t - 0.08f;
                float chingEnv = Mathf.Exp(-chingT * 3f) * intensity;

                // Bell harmonics for that satisfying cash register sound
                float bellFreq = 2093f; // C7 - high bell tone
                sample += Mathf.Sin(t * bellFreq * Mathf.PI * 2f) * chingEnv * 0.4f;
                sample += Mathf.Sin(t * bellFreq * 2f * Mathf.PI * 2f) * chingEnv * 0.2f;
                sample += Mathf.Sin(t * bellFreq * 3f * Mathf.PI * 2f) * chingEnv * 0.1f;

                // Add second bell tone for richness
                float bell2Freq = 2637f; // E7
                sample += Mathf.Sin(t * bell2Freq * Mathf.PI * 2f) * chingEnv * 0.3f;
                sample += Mathf.Sin(t * bell2Freq * 1.5f * Mathf.PI * 2f) * chingEnv * 0.15f;
            }

            // Part 3: Coin shower/shimmer (for longer durations)
            if (duration > 1.5f && t > 0.3f)
            {
                float shimmerT = t - 0.3f;
                float shimmerEnv = Mathf.Sin(shimmerT / (duration - 0.3f) * Mathf.PI) * intensity;

                // Multiple high frequencies for shimmer effect
                sample += Mathf.Sin(t * 5000f * Mathf.PI * 2f + Mathf.Sin(t * 7f) * 2f) * shimmerEnv * 0.1f;
                sample += Mathf.Sin(t * 6000f * Mathf.PI * 2f + Mathf.Sin(t * 11f) * 2f) * shimmerEnv * 0.08f;
                sample += Mathf.Sin(t * 4500f * Mathf.PI * 2f + Mathf.Sin(t * 5f) * 2f) * shimmerEnv * 0.06f;
            }

            // Part 4: Triumphant chord for mythic jackpots (intensity = 1.0)
            if (intensity >= 0.95f && t > 0.5f && t < duration - 0.3f)
            {
                float chordT = t - 0.5f;
                float chordEnv = Mathf.Sin(chordT / (duration - 0.8f) * Mathf.PI) * 0.3f;

                // Major chord: C, E, G
                sample += Mathf.Sin(t * 523.25f * Mathf.PI * 2f) * chordEnv; // C5
                sample += Mathf.Sin(t * 659.25f * Mathf.PI * 2f) * chordEnv * 0.8f; // E5
                sample += Mathf.Sin(t * 783.99f * Mathf.PI * 2f) * chordEnv * 0.6f; // G5
            }

            samples[i] = Mathf.Clamp(sample * 0.5f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Jackpot_" + intensity, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generate a single coin clink/ding sound.
    /// </summary>
    AudioClip GenerateSingleCoinSound()
    {
        int sampleRate = 44100;
        float duration = 0.3f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Sharp metallic attack
            float env = Mathf.Exp(-t * 15f);

            // Coin frequencies - bright and metallic
            sample += Mathf.Sin(t * 4000f * Mathf.PI * 2f) * env * 0.5f;
            sample += Mathf.Sin(t * 5500f * Mathf.PI * 2f) * env * 0.3f;
            sample += Mathf.Sin(t * 3200f * Mathf.PI * 2f) * env * 0.2f;

            // Slight pitch bend down for realism
            float bendFreq = 4800f - t * 1000f;
            sample += Mathf.Sin(t * bendFreq * Mathf.PI * 2f) * env * 0.2f;

            samples[i] = sample * 0.4f;
        }

        AudioClip clip = AudioClip.Create("SingleCoin", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generate a cascading coins sound (many coins falling and clinking).
    /// </summary>
    AudioClip GenerateCoinCascadeSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Pre-calculate random coin hit times for consistency
        int numCoins = (int)(duration * 30); // About 30 coins per second
        float[] coinTimes = new float[numCoins];
        float[] coinFreqs = new float[numCoins];
        float[] coinVolumes = new float[numCoins];

        for (int c = 0; c < numCoins; c++)
        {
            coinTimes[c] = Random.Range(0f, duration * 0.9f);
            coinFreqs[c] = Random.Range(3000f, 6000f); // Random metallic frequencies
            coinVolumes[c] = Random.Range(0.2f, 0.5f);
        }

        // Sort by time for better processing
        System.Array.Sort(coinTimes);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Overall envelope - builds up then fades
            float overallEnv = Mathf.Sin(t / duration * Mathf.PI);
            overallEnv = Mathf.Pow(overallEnv, 0.5f); // Keep it louder longer

            // Add each coin's contribution
            for (int c = 0; c < numCoins; c++)
            {
                float coinT = t - coinTimes[c];
                if (coinT > 0f && coinT < 0.15f)
                {
                    float coinEnv = Mathf.Exp(-coinT * 25f);

                    // Primary frequency
                    sample += Mathf.Sin(t * coinFreqs[c] * Mathf.PI * 2f) * coinEnv * coinVolumes[c];

                    // Harmonic
                    sample += Mathf.Sin(t * coinFreqs[c] * 1.5f * Mathf.PI * 2f) * coinEnv * coinVolumes[c] * 0.3f;
                }
            }

            // Add subtle metallic noise bed
            float noiseMod = (Mathf.Sin(t * 50f) + 1f) * 0.5f;
            sample += (Random.value - 0.5f) * overallEnv * noiseMod * 0.1f;

            samples[i] = Mathf.Clamp(sample * overallEnv * 0.4f, -1f, 1f);
        }

        // Light high-pass filter to keep it bright
        for (int i = 1; i < sampleCount; i++)
        {
            samples[i] = samples[i] * 0.8f + (samples[i] - samples[i - 1]) * 0.2f;
        }

        AudioClip clip = AudioClip.Create("CoinCascade_" + duration, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Generate sad/dramatic game over music.
    /// </summary>
    AudioClip GenerateGameOverMusic()
    {
        int sampleRate = 44100;
        float duration = 5.0f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Sad/dramatic progression in A minor
        // Structure:
        // 0-1.5s: Descending minor phrase
        // 1.5-3s: Low dramatic drone with dissonance
        // 3-5s: Final resolution/fadeout

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Part 1: Descending sad melody (0-1.5s)
            if (t < 1.5f)
            {
                // Notes: A4, G4, F4, E4 (descending minor scale)
                float[] notes = { 440f, 392f, 349.23f, 329.63f };
                float noteLength = 1.5f / notes.Length;
                int noteIndex = Mathf.Min((int)(t / noteLength), notes.Length - 1);
                float noteT = t - noteIndex * noteLength;

                float noteEnv = Mathf.Sin(noteT / noteLength * Mathf.PI);
                noteEnv *= (1f - t / 1.5f * 0.3f); // Slight overall fade

                // Sad organ-like tone
                float freq = notes[noteIndex];
                sample += Mathf.Sin(t * freq * Mathf.PI * 2f) * noteEnv * 0.4f;
                sample += Mathf.Sin(t * freq * 2f * Mathf.PI * 2f) * noteEnv * 0.15f;
                sample += Mathf.Sin(t * freq * 0.5f * Mathf.PI * 2f) * noteEnv * 0.2f; // Sub octave

                // Add slight vibrato for emotion
                float vibrato = Mathf.Sin(t * 5f * Mathf.PI * 2f) * 0.02f;
                sample += Mathf.Sin(t * freq * (1f + vibrato) * Mathf.PI * 2f) * noteEnv * 0.1f;
            }

            // Part 2: Dramatic low drone with tension (1.5-3s)
            if (t >= 1.5f && t < 3f)
            {
                float droneT = t - 1.5f;
                float droneEnv = Mathf.Sin(droneT / 1.5f * Mathf.PI);

                // Low A minor chord with added tension
                float rootFreq = 110f; // A2
                sample += Mathf.Sin(t * rootFreq * Mathf.PI * 2f) * droneEnv * 0.35f;
                sample += Mathf.Sin(t * rootFreq * 1.2f * Mathf.PI * 2f) * droneEnv * 0.25f; // Minor third
                sample += Mathf.Sin(t * rootFreq * 1.5f * Mathf.PI * 2f) * droneEnv * 0.2f; // Fifth

                // Add dissonant tritone for dramatic tension
                float tritone = rootFreq * 1.414f; // Diminished fifth
                float tensionEnv = Mathf.Sin(droneT / 1.5f * Mathf.PI * 2f) * droneEnv * 0.5f;
                sample += Mathf.Sin(t * tritone * Mathf.PI * 2f) * tensionEnv * 0.15f;

                // Low rumble
                sample += Mathf.Sin(t * 55f * Mathf.PI * 2f) * droneEnv * 0.2f;

                // Ominous pulsing
                float pulse = (Mathf.Sin(t * 2f * Mathf.PI * 2f) + 1f) * 0.5f;
                sample *= (0.7f + pulse * 0.3f);
            }

            // Part 3: Final resolution and fadeout (3-5s)
            if (t >= 3f)
            {
                float endT = t - 3f;
                float endEnv = Mathf.Exp(-endT * 1.5f);

                // Resolve to low A
                float resolveFreq = 110f; // A2
                sample += Mathf.Sin(t * resolveFreq * Mathf.PI * 2f) * endEnv * 0.4f;
                sample += Mathf.Sin(t * resolveFreq * 2f * Mathf.PI * 2f) * endEnv * 0.15f;

                // Add fading high note for finality
                float highNote = 440f; // A4
                float highEnv = Mathf.Exp(-endT * 2f) * 0.3f;
                sample += Mathf.Sin(t * highNote * Mathf.PI * 2f) * highEnv;

                // Subtle wind-down effect
                if (endT > 1f)
                {
                    float windDown = (endT - 1f) / 1f;
                    sample *= (1f - windDown * 0.5f);
                }
            }

            // Add subtle reverb-like effect (delayed copy)
            if (i > sampleRate / 8) // 125ms delay
            {
                int delayIndex = i - sampleRate / 8;
                sample += samples[delayIndex] * 0.2f;
            }

            samples[i] = Mathf.Clamp(sample * 0.5f, -1f, 1f);
        }

        // Apply gentle low-pass filter for warmth
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 1; i < sampleCount; i++)
            {
                samples[i] = samples[i] * 0.6f + samples[i - 1] * 0.4f;
            }
        }

        AudioClip clip = AudioClip.Create("GameOverMusic", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
