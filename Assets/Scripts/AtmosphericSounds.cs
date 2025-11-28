using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Atmospheric ambient sounds - tropical birds and ocean breeze
/// Uses 3D positioned audio sources for surround sound panning
/// </summary>
public class AtmosphericSounds : MonoBehaviour
{
    public static AtmosphericSounds Instance { get; private set; }

    // Audio sources for different ambient sounds
    private List<AudioSource> birdSources = new List<AudioSource>();
    private AudioSource breezeSource;
    private AudioSource wavesSource;

    // Bird sound timing
    private float nextBirdCallTime = 0f;
    private float birdCallInterval = 4f;

    // Player reference
    private Transform playerTransform;

    // Breeze audio clip (generated)
    private AudioClip breezeClip;
    private AudioClip wavesClip;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Invoke("Initialize", 1f);
    }

    void Initialize()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Create ambient breeze (low volume, continuous)
        CreateBreezeSound();

        // Create ocean waves ambient
        CreateWavesSound();

        // Create bird audio sources positioned around the scene
        CreateBirdSources();

        nextBirdCallTime = Time.time + Random.Range(2f, 5f);
    }

    void CreateBreezeSound()
    {
        // Create breeze audio source on this object (follows listener)
        GameObject breezeObj = new GameObject("BreezeAmbient");
        breezeObj.transform.SetParent(transform);
        breezeObj.transform.localPosition = Vector3.zero;

        breezeSource = breezeObj.AddComponent<AudioSource>();
        breezeSource.spatialBlend = 0f;  // 2D sound - always present
        breezeSource.loop = true;
        breezeSource.volume = 0.04f;  // 50% of original
        breezeSource.playOnAwake = false;

        // Generate breeze clip
        breezeClip = GenerateBreezeClip();
        breezeSource.clip = breezeClip;
        breezeSource.Play();
    }

    AudioClip GenerateBreezeClip()
    {
        int sampleRate = 44100;
        float duration = 5f;  // 5 second loop
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Breeze", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];

        // Use a seed for consistent random but varied noise
        System.Random rng = new System.Random(12345);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Very slow modulation for wind gusts
            float gustMod = 0.5f + 0.5f * Mathf.Sin(t * 0.3f) * Mathf.Sin(t * 0.17f);

            // Filtered noise (low frequency emphasis for wind)
            float noise = ((float)rng.NextDouble() * 2f - 1f);

            // Low-pass filter simulation via averaging
            if (i > 0)
            {
                noise = samples[i - 1] * 0.95f + noise * 0.05f;
            }

            // Add some subtle whistling
            float whistle = Mathf.Sin(2 * Mathf.PI * 800f * t) * 0.02f * Mathf.Sin(t * 2f);

            samples[i] = (noise * gustMod + whistle) * 0.5f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    void CreateWavesSound()
    {
        // Ocean waves - positioned at water level around player
        GameObject wavesObj = new GameObject("WavesAmbient");
        wavesObj.transform.SetParent(transform);
        wavesObj.transform.localPosition = Vector3.zero;

        wavesSource = wavesObj.AddComponent<AudioSource>();
        wavesSource.spatialBlend = 0.3f;  // Slight spatial
        wavesSource.loop = true;
        wavesSource.volume = 0.06f;  // 50% of original
        wavesSource.playOnAwake = false;

        // Generate waves clip
        wavesClip = GenerateWavesClip();
        wavesSource.clip = wavesClip;
        wavesSource.Play();
    }

    AudioClip GenerateWavesClip()
    {
        int sampleRate = 44100;
        float duration = 8f;  // 8 second loop
        int sampleCount = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Waves", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        System.Random rng = new System.Random(54321);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            // Wave rhythm - slow cycle
            float waveRhythm = Mathf.Sin(t * 0.5f) * 0.5f + 0.5f;
            waveRhythm *= Mathf.Sin(t * 0.3f + 1f) * 0.3f + 0.7f;

            // White noise base
            float noise = ((float)rng.NextDouble() * 2f - 1f);

            // Filter for water sound
            if (i > 0)
            {
                noise = samples[i - 1] * 0.9f + noise * 0.1f;
            }

            // Wave crash peaks
            float crashPhase = (t % 4f) / 4f;  // 4 second wave cycle
            float crash = 0f;
            if (crashPhase > 0.4f && crashPhase < 0.6f)
            {
                crash = Mathf.Sin((crashPhase - 0.4f) * Mathf.PI / 0.2f) * 0.3f;
            }

            samples[i] = (noise * waveRhythm + crash * ((float)rng.NextDouble() * 0.5f + 0.5f)) * 0.6f;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    void CreateBirdSources()
    {
        // Create several bird audio sources positioned around the scene
        // These will be 3D spatial audio for surround panning
        for (int i = 0; i < 5; i++)
        {
            GameObject birdObj = new GameObject("BirdSource_" + i);
            birdObj.transform.SetParent(transform);

            // Position birds in a circle around origin at varying heights
            float angle = (i / 5f) * Mathf.PI * 2f;
            float radius = Random.Range(15f, 35f);
            float height = Random.Range(5f, 15f);
            birdObj.transform.position = new Vector3(
                Mathf.Cos(angle) * radius,
                height,
                Mathf.Sin(angle) * radius
            );

            AudioSource birdSource = birdObj.AddComponent<AudioSource>();
            birdSource.spatialBlend = 1f;  // Full 3D spatial
            birdSource.minDistance = 5f;
            birdSource.maxDistance = 50f;
            birdSource.rolloffMode = AudioRolloffMode.Linear;
            birdSource.volume = 0.125f;  // 50% of original
            birdSource.playOnAwake = false;
            birdSource.dopplerLevel = 0f;  // No doppler for ambient

            birdSources.Add(birdSource);
        }
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Update bird source positions relative to player for surround effect
        UpdateBirdPositions();

        // Trigger random bird calls
        if (Time.time >= nextBirdCallTime)
        {
            PlayRandomBirdCall();
            nextBirdCallTime = Time.time + Random.Range(birdCallInterval * 0.5f, birdCallInterval * 1.5f);
        }
    }

    void UpdateBirdPositions()
    {
        if (playerTransform == null) return;

        // Slowly move bird sources around player to create dynamic soundscape
        for (int i = 0; i < birdSources.Count; i++)
        {
            if (birdSources[i] == null) continue;

            // Orbit around player slowly
            float orbitSpeed = 0.02f + i * 0.005f;
            float angle = Time.time * orbitSpeed + (i * Mathf.PI * 2f / birdSources.Count);
            float radius = 20f + i * 5f;
            float height = 8f + Mathf.Sin(Time.time * 0.1f + i) * 3f;

            Vector3 newPos = playerTransform.position + new Vector3(
                Mathf.Cos(angle) * radius,
                height,
                Mathf.Sin(angle) * radius
            );

            birdSources[i].transform.position = Vector3.Lerp(
                birdSources[i].transform.position,
                newPos,
                Time.deltaTime * 0.5f
            );
        }
    }

    void PlayRandomBirdCall()
    {
        if (birdSources.Count == 0) return;

        // Pick a random bird source
        int index = Random.Range(0, birdSources.Count);
        AudioSource source = birdSources[index];

        if (source == null || source.isPlaying) return;

        // Generate a random bird call
        AudioClip birdClip = GenerateBirdCall();
        source.clip = birdClip;
        source.pitch = Random.Range(0.8f, 1.3f);
        source.Play();
    }

    AudioClip GenerateBirdCall()
    {
        int sampleRate = 44100;
        int birdType = Random.Range(0, 4);  // Different bird types

        float duration;
        float[] samples;

        switch (birdType)
        {
            case 0:
                // Tropical chirp (short, high)
                duration = Random.Range(0.3f, 0.6f);
                samples = GenerateTropicalChirp(sampleRate, duration);
                break;
            case 1:
                // Parrot-like squawk
                duration = Random.Range(0.4f, 0.8f);
                samples = GenerateParrotSquawk(sampleRate, duration);
                break;
            case 2:
                // Songbird trill
                duration = Random.Range(0.8f, 1.5f);
                samples = GenerateSongbirdTrill(sampleRate, duration);
                break;
            default:
                // Seagull call
                duration = Random.Range(0.5f, 1.0f);
                samples = GenerateSeagullCall(sampleRate, duration);
                break;
        }

        AudioClip clip = AudioClip.Create("BirdCall", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    float[] GenerateTropicalChirp(int sampleRate, float duration)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float baseFreq = Random.Range(2000f, 3500f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Quick frequency sweep up then down
            float freqMod = 1f + Mathf.Sin(progress * Mathf.PI) * 0.3f;
            float freq = baseFreq * freqMod;

            // Sharp attack, quick decay
            float envelope = Mathf.Sin(progress * Mathf.PI) * Mathf.Exp(-progress * 3f);

            // Add harmonics
            float sound = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.6f;
            sound += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.3f;
            sound += Mathf.Sin(2 * Mathf.PI * freq * 3f * t) * 0.1f;

            samples[i] = sound * envelope;
        }

        return samples;
    }

    float[] GenerateParrotSquawk(int sampleRate, float duration)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float baseFreq = Random.Range(800f, 1200f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Raspy frequency modulation
            float freqMod = 1f + Mathf.Sin(t * 80f) * 0.15f;
            float freq = baseFreq * freqMod * (1f + progress * 0.3f);

            // Harsh envelope
            float envelope = Mathf.Clamp01(1f - Mathf.Abs(progress - 0.3f) * 2f);

            // Square-ish wave for harsh sound
            float sound = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * freq * t)) * 0.3f;
            sound += Mathf.Sin(2 * Mathf.PI * freq * t) * 0.4f;

            // Add noise for raspiness
            sound += (Random.value * 2f - 1f) * 0.1f * envelope;

            samples[i] = sound * envelope;
        }

        return samples;
    }

    float[] GenerateSongbirdTrill(int sampleRate, float duration)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float baseFreq = Random.Range(1500f, 2500f);
        int numNotes = Random.Range(4, 8);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Trill between notes
            int noteIndex = (int)(progress * numNotes);
            float noteProgress = (progress * numNotes) % 1f;

            // Each note has different pitch
            float noteMod = 1f + Mathf.Sin(noteIndex * 1.5f) * 0.2f;
            float freq = baseFreq * noteMod;

            // Note envelope
            float noteEnv = Mathf.Sin(noteProgress * Mathf.PI);

            // Overall envelope
            float overallEnv = Mathf.Sin(progress * Mathf.PI);

            float sound = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.5f;
            sound += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.25f;

            samples[i] = sound * noteEnv * overallEnv;
        }

        return samples;
    }

    float[] GenerateSeagullCall(int sampleRate, float duration)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float baseFreq = Random.Range(600f, 900f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Rising then falling pitch
            float pitchCurve;
            if (progress < 0.4f)
            {
                pitchCurve = 1f + (progress / 0.4f) * 0.5f;
            }
            else
            {
                pitchCurve = 1.5f - ((progress - 0.4f) / 0.6f) * 0.3f;
            }

            float freq = baseFreq * pitchCurve;

            // Wavering vibrato
            freq *= 1f + Mathf.Sin(t * 40f) * 0.05f;

            // Envelope with quick attack
            float envelope = Mathf.Sin(progress * Mathf.PI);
            if (progress < 0.1f) envelope = progress / 0.1f;

            float sound = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.5f;
            sound += Mathf.Sin(2 * Mathf.PI * freq * 1.5f * t) * 0.2f;

            // Slight noise for realistic texture
            sound += (Random.value * 2f - 1f) * 0.05f * envelope;

            samples[i] = sound * envelope;
        }

        return samples;
    }

    void OnDestroy()
    {
        // Clean up audio clips
        if (breezeClip != null) Destroy(breezeClip);
        if (wavesClip != null) Destroy(wavesClip);
    }
}
