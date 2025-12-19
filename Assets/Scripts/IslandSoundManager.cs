using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all island sound effects including ambient sounds, weather, interactions, and celebrations.
/// Uses procedural audio generation for all sounds.
/// </summary>
public class IslandSoundManager : MonoBehaviour
{
    public static IslandSoundManager Instance { get; private set; }

    /// <summary>
    /// Auto-create the sound manager when the game starts if it doesn't exist
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateInstance()
    {
        if (Instance == null)
        {
            Debug.Log("[IslandSoundManager] Auto-creating instance at runtime");
            GameObject go = new GameObject("IslandSoundManager");
            go.AddComponent<IslandSoundManager>();
        }
    }

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1.0f;
    [Range(0f, 1f)] public float ambientVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;
    [Range(0f, 1f)] public float voiceVolume = 1.0f;

    // Audio sources
    private AudioSource ambientSource;
    private AudioSource waveSource;
    private AudioSource birdSource;
    private AudioSource rainSource;
    private AudioSource sfxSource;
    private AudioSource voiceSource;
    private AudioSource celebrationSource;

    // Cached audio clips (procedurally generated)
    private AudioClip waveClip;
    private AudioClip birdChirpClip;
    private AudioClip birdCallClip;
    private AudioClip windClip;
    private AudioClip rainClip;
    private AudioClip rainHeavyClip;
    private AudioClip splashSmallClip;
    private AudioClip splashMediumClip;
    private AudioClip splashLargeClip;
    private AudioClip bobberSplashClip;
    private AudioClip[] npcVoiceClips;
    private AudioClip fanfareClip;
    private AudioClip chimeClip;
    private AudioClip trumpetClip;
    private AudioClip jingleClip;
    private AudioClip legendaryFanfareClip;

    // State
    private float birdTimer = 0f;
    private float nextBirdTime = 3f;
    private bool isRaining = false;

    void Awake()
    {
        Debug.Log("[IslandSoundManager] Awake() called");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep alive across scenes
            Debug.Log("[IslandSoundManager] Instance registered and marked DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[IslandSoundManager] Duplicate instance detected - destroying");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("[IslandSoundManager] Start() called - initializing sound system");
        EnsureAudioListener();
        CreateAudioSources();
        GenerateAllAudioClips();
        StartAmbientSounds();

        // Play a test beep to verify audio is working
        PlayTestBeep();

        Debug.Log("[IslandSoundManager] Sound system initialized successfully");
    }

    void EnsureAudioListener()
    {
        // Check for multiple audio listeners (causes problems)
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        Debug.Log("[IslandSoundManager] Found " + listeners.Length + " AudioListener(s) in scene");

        if (listeners.Length == 0)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.gameObject.AddComponent<AudioListener>();
                Debug.Log("[IslandSoundManager] Added AudioListener to Main Camera");
            }
            else
            {
                // Add to ourselves as fallback
                gameObject.AddComponent<AudioListener>();
                Debug.Log("[IslandSoundManager] Added AudioListener to IslandSoundManager (no main camera found)");
            }
        }
        else if (listeners.Length > 1)
        {
            Debug.LogWarning("[IslandSoundManager] WARNING: Multiple AudioListeners detected! This can cause audio issues.");
            foreach (var l in listeners)
            {
                Debug.LogWarning("  - AudioListener on: " + l.gameObject.name);
            }
        }
        else
        {
            Debug.Log("[IslandSoundManager] AudioListener found on: " + listeners[0].gameObject.name);
        }
    }

    void PlayTestBeep()
    {
        Debug.Log("[IslandSoundManager] PlayTestBeep called");

        // Generate a simple beep to test if audio is working at all
        int sampleRate = 44100;
        float duration = 0.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float frequency = 880f; // A5 note - higher pitch, easier to hear
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Sin(t / duration * Mathf.PI); // Smooth envelope
            samples[i] = Mathf.Sin(t * frequency * Mathf.PI * 2f) * envelope * 0.8f;
        }

        AudioClip testBeep = AudioClip.Create("TestBeep", sampleCount, 1, sampleRate, false);
        bool success = testBeep.SetData(samples, 0);
        Debug.Log("[IslandSoundManager] TestBeep clip created - SetData success: " + success + ", length: " + testBeep.length);

        // Method 1: Regular AudioSource
        AudioSource testSource = gameObject.AddComponent<AudioSource>();
        testSource.clip = testBeep;
        testSource.volume = 1.0f;
        testSource.spatialBlend = 0f; // 2D sound
        testSource.Play();
        Debug.Log("[IslandSoundManager] TestSource.isPlaying: " + testSource.isPlaying);

        // Method 2: Also try PlayClipAtPoint as backup
        AudioSource.PlayClipAtPoint(testBeep, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 1.0f);

        Debug.Log("[IslandSoundManager] TEST BEEP PLAYED via both methods - if you don't hear this, procedural audio may not work!");

        // Destroy the test source after it finishes
        Destroy(testSource, duration + 0.5f);
    }

    void CreateAudioSources()
    {
        Debug.Log("[IslandSoundManager] CreateAudioSources called");

        // Ambient ocean waves (looping)
        waveSource = gameObject.AddComponent<AudioSource>();
        waveSource.loop = true;
        waveSource.spatialBlend = 0f; // 2D sound
        waveSource.priority = 128;
        waveSource.playOnAwake = false;
        Debug.Log("[IslandSoundManager] Created waveSource");

        // Bird sounds (one-shot)
        birdSource = gameObject.AddComponent<AudioSource>();
        birdSource.loop = false;
        birdSource.spatialBlend = 0f;
        birdSource.priority = 100;

        // Rain (looping when active)
        rainSource = gameObject.AddComponent<AudioSource>();
        rainSource.loop = true;
        rainSource.spatialBlend = 0f;
        rainSource.priority = 64;

        // General SFX (splashes, etc)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.priority = 50;

        // NPC voices
        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f;
        voiceSource.priority = 32;

        // Celebrations (fanfares, jingles)
        celebrationSource = gameObject.AddComponent<AudioSource>();
        celebrationSource.loop = false;
        celebrationSource.spatialBlend = 0f;
        celebrationSource.priority = 16;
    }

    void GenerateAllAudioClips()
    {
        // Ambient sounds
        waveClip = GenerateWaveSound(10f);
        birdChirpClip = GenerateBirdChirp();
        birdCallClip = GenerateBirdCall();
        windClip = GenerateWindSound(8f);

        // Rain
        rainClip = GenerateRainSound(5f, 0.3f);
        rainHeavyClip = GenerateRainSound(5f, 0.6f);

        // Splashes
        splashSmallClip = GenerateSplashSound(0.3f, 0.4f);
        splashMediumClip = GenerateSplashSound(0.5f, 0.6f);
        splashLargeClip = GenerateSplashSound(0.8f, 0.8f);
        bobberSplashClip = GenerateBobberSplash();

        // NPC voices
        npcVoiceClips = new AudioClip[]
        {
            GenerateVoiceSound("hi"),
            GenerateVoiceSound("hello"),
            GenerateVoiceSound("hmm"),
            GenerateVoiceSound("mhm"),
            GenerateVoiceSound("ooh"),
            GenerateVoiceSound("ah"),
            GenerateVoiceSound("hey"),
            GenerateVoiceSound("yo")
        };

        // Celebrations
        chimeClip = GenerateChimeSound();
        jingleClip = GenerateJingleSound();
        fanfareClip = GenerateFanfareSound();
        trumpetClip = GenerateTrumpetSound();
        legendaryFanfareClip = GenerateLegendaryFanfare();
    }

    void StartAmbientSounds()
    {
        Debug.Log("[IslandSoundManager] Starting tropical ambient sounds...");

        // Start ocean waves
        if (waveClip != null && waveSource != null)
        {
            waveSource.clip = waveClip;
            waveSource.volume = 0.5f; // Good volume for ambient
            waveSource.loop = true;
            waveSource.Play();
            Debug.Log("[IslandSoundManager] Ocean waves started - volume: " + waveSource.volume);
        }

        // Play an initial bird sound to confirm audio works
        if (birdChirpClip != null && birdSource != null)
        {
            birdSource.PlayOneShot(birdChirpClip, 0.6f);
            Debug.Log("[IslandSoundManager] Initial bird chirp played");
        }
    }

    private bool gameStartedLogged = false;

    void Update()
    {
        // Keep waves playing
        if (waveSource != null && waveClip != null && !waveSource.isPlaying)
        {
            waveSource.clip = waveClip;
            waveSource.loop = true;
            waveSource.volume = 0.5f;
            waveSource.Play();
        }

        if (!MainMenu.GameStarted) return;

        // Log once when game starts
        if (!gameStartedLogged)
        {
            gameStartedLogged = true;
            Debug.Log("[IslandSoundManager] Game started - tropical sounds active!");
        }

        UpdateBirdSounds();
        UpdateRainState();
    }

    void UpdateBirdSounds()
    {
        birdTimer += Time.deltaTime;
        if (birdTimer >= nextBirdTime)
        {
            birdTimer = 0f;
            nextBirdTime = Random.Range(2f, 6f); // Frequent tropical birds

            // Random bird sound
            AudioClip clip = Random.value > 0.5f ? birdChirpClip : birdCallClip;
            if (clip != null && birdSource != null)
            {
                birdSource.pitch = Random.Range(0.85f, 1.2f);
                birdSource.PlayOneShot(clip, Random.Range(0.4f, 0.7f));
            }
        }
    }

    void UpdateRainState()
    {
        // Check weather system for rain
        WeatherSystem weather = FindObjectOfType<WeatherSystem>();
        if (weather != null)
        {
            bool nowRaining = weather.IsRaining();
            if (nowRaining != isRaining)
            {
                isRaining = nowRaining;
                if (isRaining)
                {
                    StartRainSound(weather.GetRainIntensity());
                }
                else
                {
                    StopRainSound();
                }
            }
        }
    }

    // === PUBLIC METHODS FOR PLAYING SOUNDS ===

    public void PlaySplash(Vector3 position, float size = 0.5f)
    {
        Debug.Log("[IslandSoundManager] PlaySplash called - size: " + size);
        AudioClip clip;
        if (size < 0.3f)
            clip = splashSmallClip;
        else if (size < 0.7f)
            clip = splashMediumClip;
        else
            clip = splashLargeClip;

        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.volume = sfxVolume * masterVolume;
            sfxSource.PlayOneShot(clip);
            Debug.Log("[IslandSoundManager] Playing splash sound - volume: " + sfxSource.volume);
        }
    }

    public void PlayBobberSplash()
    {
        Debug.Log("[IslandSoundManager] PlayBobberSplash called");
        if (bobberSplashClip != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.volume = sfxVolume * masterVolume;
            sfxSource.PlayOneShot(bobberSplashClip);
            Debug.Log("[IslandSoundManager] Playing bobber splash - volume: " + sfxSource.volume);
        }
    }

    public void PlayNPCVoice()
    {
        Debug.Log("[IslandSoundManager] PlayNPCVoice called");
        if (npcVoiceClips != null && npcVoiceClips.Length > 0)
        {
            AudioClip clip = npcVoiceClips[Random.Range(0, npcVoiceClips.Length)];
            if (clip != null)
            {
                voiceSource.pitch = Random.Range(0.85f, 1.15f);
                voiceSource.volume = voiceVolume * masterVolume;
                voiceSource.PlayOneShot(clip);
                Debug.Log("[IslandSoundManager] Playing NPC voice - volume: " + voiceSource.volume);
            }
        }
    }

    public void PlayNPCVoice(string type)
    {
        Debug.Log("[IslandSoundManager] PlayNPCVoice called with type: " + type);
        int index = 0;
        switch (type.ToLower())
        {
            case "hi": index = 0; break;
            case "hello": index = 1; break;
            case "hmm": index = 2; break;
            case "mhm": index = 3; break;
            case "ooh": index = 4; break;
            case "ah": index = 5; break;
            case "hey": index = 6; break;
            case "yo": index = 7; break;
            default: index = Random.Range(0, npcVoiceClips.Length); break;
        }

        if (npcVoiceClips != null && index < npcVoiceClips.Length && npcVoiceClips[index] != null)
        {
            voiceSource.pitch = Random.Range(0.9f, 1.1f);
            voiceSource.volume = voiceVolume * masterVolume;
            voiceSource.PlayOneShot(npcVoiceClips[index]);
            Debug.Log("[IslandSoundManager] Playing NPC voice '" + type + "' - volume: " + voiceSource.volume);
        }
    }

    public void PlayCelebration(int rarity)
    {
        Debug.Log("[IslandSoundManager] PlayCelebration called - rarity: " + rarity);
        AudioClip clip;
        float volume = sfxVolume * masterVolume;

        switch (rarity)
        {
            case 1: // Uncommon
                clip = chimeClip;
                volume *= 0.8f;
                break;
            case 2: // Rare
                clip = jingleClip;
                volume *= 0.9f;
                break;
            case 3: // Epic
                clip = fanfareClip;
                volume *= 1.0f;
                break;
            case 4: // Legendary
                clip = trumpetClip;
                volume *= 1.0f;
                break;
            case 5: // Mythic/Jackpot
                clip = legendaryFanfareClip;
                volume *= 1.0f;
                break;
            default:
                clip = chimeClip;
                volume *= 0.7f;
                break;
        }

        if (clip != null)
        {
            celebrationSource.volume = volume;
            celebrationSource.PlayOneShot(clip);
            Debug.Log("[IslandSoundManager] Playing celebration - volume: " + volume);
        }
    }

    public void PlayChime()
    {
        if (chimeClip != null)
        {
            celebrationSource.volume = sfxVolume * masterVolume * 0.6f;
            celebrationSource.PlayOneShot(chimeClip);
        }
    }

    public void PlayFanfare()
    {
        if (fanfareClip != null)
        {
            celebrationSource.volume = sfxVolume * masterVolume;
            celebrationSource.PlayOneShot(fanfareClip);
        }
    }

    public void PlayJackpotFanfare()
    {
        if (legendaryFanfareClip != null)
        {
            celebrationSource.volume = sfxVolume * masterVolume;
            celebrationSource.PlayOneShot(legendaryFanfareClip);
        }
    }

    public void StartRainSound(float intensity = 0.5f)
    {
        AudioClip clip = intensity > 0.5f ? rainHeavyClip : rainClip;
        if (clip != null && !rainSource.isPlaying)
        {
            rainSource.clip = clip;
            rainSource.volume = ambientVolume * masterVolume * intensity;
            rainSource.Play();
        }
    }

    public void StopRainSound()
    {
        if (rainSource.isPlaying)
        {
            rainSource.Stop();
        }
    }

    // === PROCEDURAL AUDIO GENERATION ===

    AudioClip GenerateWaveSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Generate ocean wave sound using filtered noise with modulation
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // White noise base
            float noise = Random.Range(-1f, 1f);

            // Slow modulation for wave rhythm (crashing waves effect)
            float waveRhythm1 = (Mathf.Sin(t * Mathf.PI * 2f * 0.15f) + 1f) * 0.5f; // ~7 second wave cycle
            float waveRhythm2 = (Mathf.Sin(t * Mathf.PI * 2f * 0.08f + 1.5f) + 1f) * 0.5f; // ~12 second cycle
            float waveRhythm3 = (Mathf.Sin(t * Mathf.PI * 2f * 0.25f + 0.7f) + 1f) * 0.5f; // ~4 second cycle

            // Combine rhythms for natural variation
            float envelope = (waveRhythm1 * 0.4f + waveRhythm2 * 0.35f + waveRhythm3 * 0.25f);

            // Add some low frequency rumble (audible range)
            float rumble = Mathf.Sin(t * Mathf.PI * 2f * 60f) * 0.1f; // 60Hz rumble
            rumble += Mathf.Sin(t * Mathf.PI * 2f * 40f) * 0.08f; // 40Hz sub

            samples[i] = (noise * 0.6f + rumble) * envelope * 0.7f;
        }

        // Apply simple low-pass filter for ocean sound character
        for (int pass = 0; pass < 3; pass++)
        {
            for (int i = 1; i < sampleCount; i++)
            {
                samples[i] = samples[i] * 0.25f + samples[i - 1] * 0.75f;
            }
        }

        AudioClip clip = AudioClip.Create("Waves", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateBirdChirp()
    {
        int sampleRate = 44100;
        float duration = 0.3f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 15f);

            // Rising chirp frequency
            float freq = 2000f + t * 3000f;
            float chirp = Mathf.Sin(t * freq * Mathf.PI * 2f);

            samples[i] = chirp * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("BirdChirp", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateBirdCall()
    {
        int sampleRate = 44100;
        float duration = 0.6f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Two-tone bird call
            float note1 = Mathf.Sin(t * 1800f * Mathf.PI * 2f);
            float note2 = Mathf.Sin(t * 2200f * Mathf.PI * 2f);

            // Alternate between notes
            float mix = t < 0.2f ? note1 : (t < 0.4f ? note2 : note1);

            // Envelope
            float envelope = Mathf.Sin(t / duration * Mathf.PI);

            samples[i] = mix * envelope * 0.35f;
        }

        AudioClip clip = AudioClip.Create("BirdCall", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateWindSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Filtered noise for wind
            float noise = Random.value - 0.5f;
            float modulation = (Mathf.Sin(t * 0.5f) + Mathf.Sin(t * 0.3f)) * 0.5f + 0.5f;

            samples[i] = noise * modulation * 0.2f;
        }

        // Simple low-pass filter simulation
        for (int i = 1; i < sampleCount; i++)
        {
            samples[i] = samples[i] * 0.3f + samples[i - 1] * 0.7f;
        }

        AudioClip clip = AudioClip.Create("Wind", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateRainSound(float duration, float intensity)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Rain is essentially filtered noise with occasional drip sounds
            float noise = (Random.value - 0.5f) * intensity;

            // Add occasional "drip" impacts
            if (Random.value < 0.001f * intensity)
            {
                noise += Random.Range(0.3f, 0.5f) * (Random.value > 0.5f ? 1f : -1f);
            }

            samples[i] = noise * 0.4f;
        }

        // Low-pass filter
        for (int i = 1; i < sampleCount; i++)
        {
            samples[i] = samples[i] * 0.4f + samples[i - 1] * 0.6f;
        }

        AudioClip clip = AudioClip.Create("Rain", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateSplashSound(float duration, float intensity)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Quick burst of noise that decays
            float envelope = Mathf.Exp(-t * 6f / duration);
            float noise = Random.Range(-1f, 1f);

            // Add some low frequency thump for impact
            float thump = Mathf.Sin(t * 120f * Mathf.PI * 2f) * Mathf.Exp(-t * 15f);

            // Add water bubble sounds
            float bubble1 = Mathf.Sin(t * 400f * Mathf.PI * 2f) * Mathf.Exp(-t * 25f) * 0.3f;
            float bubble2 = Mathf.Sin(t * 600f * Mathf.PI * 2f) * Mathf.Exp(-t * 30f) * 0.2f;

            samples[i] = (noise * 0.5f + thump * 0.4f + bubble1 + bubble2) * envelope * intensity;
        }

        // Light filter
        for (int i = 1; i < sampleCount; i++)
        {
            samples[i] = samples[i] * 0.6f + samples[i - 1] * 0.4f;
        }

        AudioClip clip = AudioClip.Create("Splash", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateBobberSplash()
    {
        int sampleRate = 44100;
        float duration = 0.25f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Small plop sound
            float envelope = Mathf.Exp(-t * 15f);
            float plop = Mathf.Sin(t * 400f * Mathf.PI * 2f) * Mathf.Exp(-t * 30f);
            float bubble = Mathf.Sin(t * 800f * Mathf.PI * 2f) * Mathf.Exp(-t * 20f) * 0.3f;
            float noise = (Random.value - 0.5f) * envelope * 0.4f;

            samples[i] = (plop + bubble + noise) * 0.5f;
        }

        AudioClip clip = AudioClip.Create("BobberSplash", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateVoiceSound(string type)
    {
        int sampleRate = 44100;
        float duration = 0.3f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float baseFreq = 150f; // Base voice frequency
        float[] formants;

        // Different formant patterns for different sounds
        switch (type.ToLower())
        {
            case "hi":
                formants = new float[] { 400f, 2000f, 2800f };
                duration = 0.25f;
                break;
            case "hello":
                formants = new float[] { 500f, 1800f, 2500f };
                duration = 0.4f;
                break;
            case "hmm":
                formants = new float[] { 300f, 1000f, 2200f };
                duration = 0.5f;
                break;
            case "mhm":
                formants = new float[] { 250f, 900f, 2000f };
                duration = 0.45f;
                break;
            case "ooh":
                formants = new float[] { 350f, 800f, 2300f };
                duration = 0.35f;
                break;
            case "ah":
                formants = new float[] { 700f, 1200f, 2500f };
                duration = 0.3f;
                break;
            default:
                formants = new float[] { 400f, 1500f, 2500f };
                break;
        }

        sampleCount = (int)(sampleRate * duration);
        samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Fundamental frequency with slight vibrato
            float fundamental = Mathf.Sin(t * baseFreq * Mathf.PI * 2f * (1f + Mathf.Sin(t * 5f) * 0.02f));

            // Add formants (simplified)
            float voice = fundamental;
            foreach (float formant in formants)
            {
                voice += Mathf.Sin(t * formant * Mathf.PI * 2f) * 0.15f;
            }

            // Envelope
            float envelope = Mathf.Sin(t / duration * Mathf.PI);

            samples[i] = voice * envelope * 0.25f;
        }

        AudioClip clip = AudioClip.Create("Voice_" + type, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateChimeSound()
    {
        int sampleRate = 44100;
        float duration = 1.0f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] notes = { 523.25f, 659.25f, 783.99f }; // C5, E5, G5 - major chord

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            foreach (float note in notes)
            {
                float envelope = Mathf.Exp(-t * 3f);
                sample += Mathf.Sin(t * note * Mathf.PI * 2f) * envelope;
            }

            samples[i] = sample * 0.2f;
        }

        AudioClip clip = AudioClip.Create("Chime", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateJingleSound()
    {
        int sampleRate = 44100;
        float duration = 1.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Ascending arpeggio
        float[] notes = { 392f, 523.25f, 659.25f, 783.99f, 1046.5f }; // G4, C5, E5, G5, C6
        float noteTime = duration / notes.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Min((int)(t / noteTime), notes.Length - 1);
            float noteT = t - noteIndex * noteTime;

            float envelope = Mathf.Exp(-noteT * 4f);
            float sample = Mathf.Sin(t * notes[noteIndex] * Mathf.PI * 2f) * envelope;

            // Add harmonics
            sample += Mathf.Sin(t * notes[noteIndex] * 2f * Mathf.PI * 2f) * envelope * 0.3f;

            samples[i] = sample * 0.3f;
        }

        AudioClip clip = AudioClip.Create("Jingle", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateFanfareSound()
    {
        int sampleRate = 44100;
        float duration = 2.0f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Trumpet-like fanfare pattern
        float[] notes = { 392f, 392f, 523.25f, 659.25f, 783.99f, 1046.5f };
        float[] noteDurations = { 0.2f, 0.2f, 0.3f, 0.3f, 0.4f, 0.6f };

        float currentTime = 0f;
        int noteIndex = 0;
        float noteStartTime = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Advance note if needed
            while (noteIndex < notes.Length - 1 && t > currentTime + noteDurations[noteIndex])
            {
                currentTime += noteDurations[noteIndex];
                noteIndex++;
                noteStartTime = currentTime;
            }

            float noteT = t - noteStartTime;
            float envelope = Mathf.Sin(Mathf.Clamp01(noteT / noteDurations[noteIndex]) * Mathf.PI);

            // Brass-like sound (fundamental + odd harmonics)
            float freq = notes[noteIndex];
            float sample = Mathf.Sin(t * freq * Mathf.PI * 2f);
            sample += Mathf.Sin(t * freq * 3f * Mathf.PI * 2f) * 0.3f;
            sample += Mathf.Sin(t * freq * 5f * Mathf.PI * 2f) * 0.1f;

            samples[i] = sample * envelope * 0.25f;
        }

        AudioClip clip = AudioClip.Create("Fanfare", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateTrumpetSound()
    {
        int sampleRate = 44100;
        float duration = 1.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Royal trumpet flourish
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f };
        float noteTime = 0.25f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Min((int)(t / noteTime), notes.Length - 1);
            float noteT = t - noteIndex * noteTime;

            // Attack-sustain-release envelope
            float attack = Mathf.Clamp01(noteT / 0.05f);
            float release = noteT > noteTime - 0.05f ? Mathf.Clamp01((noteTime - noteT) / 0.05f) : 1f;
            float envelope = attack * release;

            // Bright brass tone
            float freq = notes[noteIndex];
            float sample = Mathf.Sin(t * freq * Mathf.PI * 2f);
            sample += Mathf.Sin(t * freq * 2f * Mathf.PI * 2f) * 0.5f;
            sample += Mathf.Sin(t * freq * 3f * Mathf.PI * 2f) * 0.25f;
            sample += Mathf.Sin(t * freq * 4f * Mathf.PI * 2f) * 0.125f;

            samples[i] = sample * envelope * 0.2f;
        }

        AudioClip clip = AudioClip.Create("Trumpet", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateLegendaryFanfare()
    {
        int sampleRate = 44100;
        float duration = 3.5f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Epic multi-part fanfare
        // Part 1: Building tension (0-1s)
        // Part 2: Main fanfare (1-2.5s)
        // Part 3: Triumphant finish (2.5-3.5s)

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            if (t < 1.0f)
            {
                // Building drums/timpani
                float drumFreq = 80f + t * 40f;
                float drumEnv = Mathf.Abs(Mathf.Sin(t * 4f * Mathf.PI)) * (1f - t);
                sample = Mathf.Sin(t * drumFreq * Mathf.PI * 2f) * drumEnv * 0.5f;
            }
            else if (t < 2.5f)
            {
                // Main fanfare with full orchestra feel
                float fanfareT = t - 1.0f;
                float[] chordNotes = { 261.63f, 329.63f, 392f, 523.25f, 659.25f }; // C major spread

                foreach (float note in chordNotes)
                {
                    float env = Mathf.Sin(fanfareT / 1.5f * Mathf.PI);
                    sample += Mathf.Sin(t * note * Mathf.PI * 2f) * env * 0.15f;
                    sample += Mathf.Sin(t * note * 2f * Mathf.PI * 2f) * env * 0.05f;
                }
            }
            else
            {
                // Triumphant high note finish
                float finishT = t - 2.5f;
                float highNote = 1046.5f; // High C
                float env = Mathf.Exp(-finishT * 2f);
                sample = Mathf.Sin(t * highNote * Mathf.PI * 2f) * env * 0.4f;
                sample += Mathf.Sin(t * highNote * 0.5f * Mathf.PI * 2f) * env * 0.2f;

                // Add shimmering harmonics
                sample += Mathf.Sin(t * highNote * 3f * Mathf.PI * 2f) * env * 0.1f;
            }

            samples[i] = sample;
        }

        AudioClip clip = AudioClip.Create("LegendaryFanfare", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
