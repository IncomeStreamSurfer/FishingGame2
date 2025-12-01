using UnityEngine;

/// <summary>
/// Ambient jungle/rainforest sounds for the Jungle Realm
/// Includes birds, insects, rain, and wildlife
/// Only plays when player is in the Jungle Realm
/// </summary>
public class JungleSounds : MonoBehaviour
{
    private AudioSource ambientSource;
    private AudioSource birdSource;
    private AudioSource insectSource;
    private AudioSource rainSource;

    private float nextBirdCall = 0f;
    private float nextInsectBurst = 0f;

    private bool isPlaying = false;
    private const float JUNGLE_X_START = 900f; // Jungle realm starts at X > 900

    void Start()
    {
        // Main ambient layer (constant background) - FURTHER REDUCED VOLUME
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.volume = 0.04f; // Was 0.08f
        ambientSource.spatialBlend = 0f;
        ambientSource.clip = GenerateJungleAmbience();

        // Bird calls (occasional) - FURTHER REDUCED VOLUME
        birdSource = gameObject.AddComponent<AudioSource>();
        birdSource.spatialBlend = 0f;
        birdSource.volume = 0.03f; // Was 0.06f

        // Insect sounds - FURTHER REDUCED VOLUME
        insectSource = gameObject.AddComponent<AudioSource>();
        insectSource.loop = true;
        insectSource.volume = 0.02f; // Was 0.04f
        insectSource.spatialBlend = 0f;
        insectSource.clip = GenerateInsectChirps();

        // Light rain - FURTHER REDUCED VOLUME
        rainSource = gameObject.AddComponent<AudioSource>();
        rainSource.loop = true;
        rainSource.volume = 0.025f; // Was 0.05f
        rainSource.spatialBlend = 0f;
        rainSource.clip = GenerateLightRain();

        nextBirdCall = Time.time + Random.Range(5f, 15f); // Longer initial delay
    }

    void Update()
    {
        // Check if player is in jungle realm
        bool inJungle = IsPlayerInJungle();

        if (inJungle && !isPlaying)
        {
            StartJungleSounds();
        }
        else if (!inJungle && isPlaying)
        {
            StopJungleSounds();
        }

        // Random bird calls (only if in jungle) - with occasional silence
        if (isPlaying && Time.time > nextBirdCall)
        {
            // 20% chance of silence instead of bird call
            if (Random.value > 0.2f)
            {
                PlayBirdCall();
            }
            // Much longer and more varied intervals between sounds
            nextBirdCall = Time.time + Random.Range(8f, 25f);
        }
    }

    bool IsPlayerInJungle()
    {
        // Check via RealmManager first
        RealmManager rm = FindObjectOfType<RealmManager>();
        if (rm != null)
        {
            return rm.CurrentRealm == RealmType.JungleRealm;
        }

        // Fallback: check player X position
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            return player.transform.position.x > JUNGLE_X_START;
        }

        return false;
    }

    void StartJungleSounds()
    {
        isPlaying = true;
        ambientSource.Play();
        insectSource.Play();
        rainSource.Play();
    }

    void StopJungleSounds()
    {
        isPlaying = false;
        ambientSource.Stop();
        insectSource.Stop();
        rainSource.Stop();
    }

    void PlayBirdCall()
    {
        int callType = Random.Range(0, 6); // Expanded to 6 types for more variety
        AudioClip clip = null;

        switch (callType)
        {
            case 0:
                clip = GenerateTropicalBirdCall();
                break;
            case 1:
                clip = GenerateParrotSquawk();
                break;
            case 2:
                clip = GenerateMonkeyHowl();
                break;
            case 3:
                clip = GenerateToucanCall();
                break;
            case 4:
                clip = GenerateMacawScreech();
                break;
            case 5:
                clip = GenerateDistantBirdChirp();
                break;
        }

        if (clip != null)
        {
            // More varied pitch and volume for natural sound
            birdSource.pitch = Random.Range(0.8f, 1.2f);
            birdSource.PlayOneShot(clip, Random.Range(0.15f, 0.45f));
        }
    }

    AudioClip GenerateJungleAmbience()
    {
        int sampleRate = 22050;
        int samples = sampleRate * 8; // 8 seconds loop
        float[] data = new float[samples];

        // More randomization for variety
        float rumbleOffset = Random.Range(0f, 10f);
        float rustleSpeed = Random.Range(0.3f, 0.7f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;

            // Layered nature sounds
            float ambient = 0f;

            // Low rumble (distant sounds) - more varied frequencies
            ambient += Mathf.Sin((t + rumbleOffset) * Random.Range(15f, 25f)) * 0.1f;
            ambient += Mathf.Sin((t + rumbleOffset) * Random.Range(30f, 45f) + 1.5f) * 0.08f;

            // Rustling leaves - more randomized
            float rustleFreq = Random.Range(150f, 250f) + Mathf.Sin(t * rustleSpeed) * Random.Range(40f, 80f);
            ambient += (Random.Range(-1f, 1f) * Random.Range(0.03f, 0.07f)) * Mathf.Sin(t * rustleFreq);

            // Dripping water - less frequent
            if (Random.value < 0.0005f)
            {
                ambient += Mathf.Sin(t * Random.Range(700f, 900f)) * 0.3f * Mathf.Exp(-((i % 1000) / 200f));
            }

            // Wind through trees - more varied
            float wind = Mathf.PerlinNoise(t * Random.Range(0.2f, 0.4f), rumbleOffset) * Random.Range(0.08f, 0.12f);
            ambient += Random.Range(-1f, 1f) * wind;

            data[i] = ambient * 0.5f;
        }

        AudioClip clip = AudioClip.Create("JungleAmbience", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateInsectChirps()
    {
        int sampleRate = 22050;
        int samples = sampleRate * 4;
        float[] data = new float[samples];

        // More randomization for varied insect sounds
        float chirpSpeed = Random.Range(6f, 12f);
        float cicadaPulse = Random.Range(0.2f, 0.5f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float insect = 0f;

            // Cricket chirps - more varied frequency
            float cricketFreq = Random.Range(3500f, 4500f) + Mathf.Sin(t * Random.Range(10f, 20f)) * Random.Range(400f, 700f);
            float chirpEnv = Mathf.Abs(Mathf.Sin(t * chirpSpeed));
            insect += Mathf.Sin(t * cricketFreq) * chirpEnv * Random.Range(0.08f, 0.12f);

            // Cicada drone - more varied
            float cicadaFreq = Random.Range(2800f, 3400f) + Mathf.Sin(t * Random.Range(1.5f, 3f)) * Random.Range(150f, 300f);
            float cicadaEnv = 0.5f + 0.5f * Mathf.Sin(t * cicadaPulse);
            insect += Mathf.Sin(t * cicadaFreq) * cicadaEnv * Random.Range(0.03f, 0.07f);

            // Random clicks - less frequent for more natural sound
            if (Random.value < 0.001f)
            {
                insect += Random.Range(-0.15f, 0.15f);
            }

            data[i] = insect;
        }

        AudioClip clip = AudioClip.Create("InsectChirps", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateLightRain()
    {
        int sampleRate = 22050;
        int samples = sampleRate * 5;
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;

            // White noise base
            float rain = Random.Range(-1f, 1f) * 0.15f;

            // Occasional droplet impacts
            if (Random.value < 0.01f)
            {
                int dropLength = Random.Range(50, 200);
                for (int j = 0; j < dropLength && i + j < samples; j++)
                {
                    float dropEnv = Mathf.Exp(-j / 30f);
                    float freq = Random.Range(800f, 2000f);
                    data[i + j] += Mathf.Sin(j * freq / sampleRate * Mathf.PI * 2f) * dropEnv * 0.2f;
                }
            }

            data[i] += rain;
        }

        AudioClip clip = AudioClip.Create("LightRain", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateTropicalBirdCall()
    {
        int sampleRate = 22050;
        int samples = sampleRate / 2;
        float[] data = new float[samples];

        // More varied base frequency and warble
        float baseFreq = Random.Range(1200f, 2800f);
        float warbleSpeed = Random.Range(30f, 60f);
        float warbleDepth = Random.Range(200f, 500f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float env = Mathf.Sin(t * Mathf.PI) * (1f - t * Random.Range(0.4f, 0.6f));

            // More varied warbling frequency
            float freq = baseFreq + Mathf.Sin(t * warbleSpeed) * warbleDepth;
            freq += Mathf.Sin(t * Random.Range(60f, 100f)) * Random.Range(80f, 150f);

            float bird = Mathf.Sin(i * freq / sampleRate * Mathf.PI * 2f) * env;

            data[i] = bird * 0.4f;
        }

        AudioClip clip = AudioClip.Create("BirdCall", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateParrotSquawk()
    {
        int sampleRate = 22050;
        int samples = sampleRate / 3;
        float[] data = new float[samples];

        // More varied squawk characteristics
        float startFreq = Random.Range(700f, 1000f);
        float freqRange = Random.Range(500f, 800f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float env = Mathf.Pow(1f - t, Random.Range(0.25f, 0.4f));

            // Harsh squawk with more variation
            float freq = startFreq + t * freqRange;
            float squawk = Mathf.Sin(i * freq / sampleRate * Mathf.PI * 2f);
            squawk += Mathf.Sin(i * freq * 2f / sampleRate * Mathf.PI * 2f) * Random.Range(0.4f, 0.6f);
            squawk += Mathf.Sin(i * freq * 3f / sampleRate * Mathf.PI * 2f) * Random.Range(0.2f, 0.4f);

            // Add noise for harshness - more varied
            squawk += Random.Range(-0.25f, 0.25f);

            data[i] = squawk * env * 0.3f;
        }

        AudioClip clip = AudioClip.Create("ParrotSquawk", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateMonkeyHowl()
    {
        int sampleRate = 22050;
        int samples = sampleRate;
        float[] data = new float[samples];

        // More varied howl characteristics
        float baseFreq = Random.Range(350f, 500f);
        float pitchRange = Random.Range(500f, 700f);
        float vibratoSpeed = Random.Range(25f, 35f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;

            // Rising then falling pitch - more varied
            float pitchCurve = Mathf.Sin(t * Mathf.PI);
            float freq = baseFreq + pitchCurve * pitchRange;

            // Volume envelope - more varied
            float env = Mathf.Sin(t * Mathf.PI * Random.Range(0.7f, 0.9f));
            env *= 1f - Mathf.Pow(t, Random.Range(1.8f, 2.2f));

            // Howl with harmonics - more variation
            float howl = Mathf.Sin(i * freq / sampleRate * Mathf.PI * 2f);
            howl += Mathf.Sin(i * freq * 1.5f / sampleRate * Mathf.PI * 2f) * Random.Range(0.3f, 0.5f);
            howl += Mathf.Sin(i * freq * 2f / sampleRate * Mathf.PI * 2f) * Random.Range(0.15f, 0.25f);

            // Vibrato - more varied
            howl *= 1f + Mathf.Sin(t * vibratoSpeed) * Random.Range(0.08f, 0.12f);

            data[i] = howl * env * 0.25f;
        }

        AudioClip clip = AudioClip.Create("MonkeyHowl", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateToucanCall()
    {
        int sampleRate = 22050;
        int samples = sampleRate / 2;
        float[] data = new float[samples];

        // Deep, resonant toucan call
        float baseFreq = Random.Range(500f, 700f);
        float callPattern = Random.Range(3f, 5f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;

            // Multi-part call envelope
            float env = Mathf.Sin(t * callPattern * Mathf.PI);
            env *= Mathf.Exp(-t * Random.Range(1.5f, 2.5f));

            // Deep resonant tone with harmonics
            float freq = baseFreq + Mathf.Sin(t * 10f) * Random.Range(50f, 100f);
            float toucan = Mathf.Sin(i * freq / sampleRate * Mathf.PI * 2f);
            toucan += Mathf.Sin(i * freq * 2f / sampleRate * Mathf.PI * 2f) * 0.3f;
            toucan += Mathf.Sin(i * freq * 0.5f / sampleRate * Mathf.PI * 2f) * 0.2f;

            data[i] = toucan * env * 0.35f;
        }

        AudioClip clip = AudioClip.Create("ToucanCall", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateMacawScreech()
    {
        int sampleRate = 22050;
        int samples = sampleRate * 3 / 4; // Longer screech
        float[] data = new float[samples];

        // High-pitched, piercing macaw screech
        float startFreq = Random.Range(2000f, 2500f);
        float endFreq = Random.Range(1500f, 2000f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;

            // Descending screech
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            freq += Mathf.Sin(t * Random.Range(20f, 40f)) * Random.Range(100f, 200f);

            // Sharp attack, sustained, then decay
            float env = 1f;
            if (t < 0.1f)
                env = t / 0.1f; // Attack
            else if (t > 0.7f)
                env = (1f - t) / 0.3f; // Decay

            // Harsh, bright tone
            float screech = Mathf.Sin(i * freq / sampleRate * Mathf.PI * 2f);
            screech += Mathf.Sin(i * freq * 2.5f / sampleRate * Mathf.PI * 2f) * 0.4f;
            screech += Random.Range(-0.2f, 0.2f) * 0.3f; // Noise

            data[i] = screech * env * 0.3f;
        }

        AudioClip clip = AudioClip.Create("MacawScreech", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateDistantBirdChirp()
    {
        int sampleRate = 22050;
        int samples = sampleRate / 4; // Short chirp
        float[] data = new float[samples];

        // Soft, distant chirping
        float freq = Random.Range(2500f, 3500f);
        int chirps = Random.Range(2, 4);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;

            // Multiple quick chirps
            float chirpEnv = 0f;
            for (int c = 0; c < chirps; c++)
            {
                float chirpTime = (float)c / chirps;
                float chirpWidth = 0.15f;
                if (t > chirpTime && t < chirpTime + chirpWidth)
                {
                    float localT = (t - chirpTime) / chirpWidth;
                    chirpEnv = Mathf.Max(chirpEnv, Mathf.Sin(localT * Mathf.PI));
                }
            }

            // Soft, distant quality - higher frequency, lower volume
            float varyFreq = freq + Mathf.Sin(t * 100f) * Random.Range(100f, 200f);
            float chirp = Mathf.Sin(i * varyFreq / sampleRate * Mathf.PI * 2f);

            data[i] = chirp * chirpEnv * 0.2f; // Quieter for distant effect
        }

        AudioClip clip = AudioClip.Create("DistantChirp", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
