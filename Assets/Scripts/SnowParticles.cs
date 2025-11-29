using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Snow particle system for Ice Realm
/// Light, constant snowfall with wind effects
/// </summary>
public class SnowParticles : MonoBehaviour
{
    [Header("Snow Settings")]
    public int maxSnowflakes = 200;
    public float spawnRadius = 40f;
    public float spawnHeight = 25f;
    public float fallSpeed = 2f;
    public float windStrength = 0.5f;

    private List<Snowflake> snowflakes = new List<Snowflake>();
    private Transform playerTransform;

    // Wind
    private Vector3 windDirection;
    private float windChangeTimer;

    // Snowflake visual
    private Material snowMat;

    private class Snowflake
    {
        public GameObject obj;
        public float size;
        public float swayOffset;
        public float swaySpeed;
        public Vector3 velocity;
    }

    void Start()
    {
        // Create snow material
        snowMat = new Material(Shader.Find("Standard"));
        snowMat.color = new Color(1f, 1f, 1f, 0.85f);
        snowMat.SetFloat("_Mode", 3); // Transparent
        snowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        snowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        snowMat.EnableKeyword("_ALPHABLEND_ON");
        snowMat.renderQueue = 3000;

        // Initial wind
        windDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        windChangeTimer = Random.Range(5f, 15f);

        // Create initial snowflakes
        for (int i = 0; i < maxSnowflakes; i++)
        {
            SpawnSnowflake(true);
        }
    }

    void Update()
    {
        UpdateWind();
        UpdateSnowflakes();
        FindPlayer();
    }

    void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    void UpdateWind()
    {
        windChangeTimer -= Time.deltaTime;

        if (windChangeTimer <= 0)
        {
            // Change wind direction gradually
            Vector3 newWind = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            windDirection = Vector3.Lerp(windDirection, newWind, 0.3f).normalized;
            windChangeTimer = Random.Range(8f, 20f);
        }
    }

    void UpdateSnowflakes()
    {
        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;

        for (int i = snowflakes.Count - 1; i >= 0; i--)
        {
            Snowflake flake = snowflakes[i];

            if (flake.obj == null)
            {
                snowflakes.RemoveAt(i);
                continue;
            }

            // Calculate movement
            float sway = Mathf.Sin(Time.time * flake.swaySpeed + flake.swayOffset) * 0.5f;

            Vector3 movement = new Vector3(
                windDirection.x * windStrength + sway * 0.3f,
                -fallSpeed * (0.8f + flake.size * 0.4f), // Larger flakes fall faster
                windDirection.z * windStrength + sway * 0.3f
            );

            flake.obj.transform.position += movement * Time.deltaTime;

            // Rotate slightly
            flake.obj.transform.Rotate(Vector3.up * 30f * Time.deltaTime);

            // Check if below ground or too far from player
            float distFromCenter = Vector3.Distance(
                new Vector3(flake.obj.transform.position.x, 0, flake.obj.transform.position.z),
                new Vector3(center.x, 0, center.z)
            );

            if (flake.obj.transform.position.y < 1f || distFromCenter > spawnRadius * 1.5f)
            {
                // Respawn at top
                RespawnSnowflake(flake, center);
            }
        }

        // Maintain snowflake count
        while (snowflakes.Count < maxSnowflakes)
        {
            SpawnSnowflake(false);
        }
    }

    void SpawnSnowflake(bool randomHeight)
    {
        Vector3 center = playerTransform != null ? playerTransform.position : transform.position;

        Snowflake flake = new Snowflake();

        // Random position around player
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(0f, spawnRadius);

        float spawnY = randomHeight ?
            Random.Range(center.y + 2f, center.y + spawnHeight) :
            center.y + spawnHeight;

        Vector3 spawnPos = new Vector3(
            center.x + Mathf.Cos(angle) * dist,
            spawnY,
            center.z + Mathf.Sin(angle) * dist
        );

        // Create snowflake (small quad)
        flake.obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flake.obj.name = "Snowflake";
        flake.obj.transform.SetParent(transform);
        flake.obj.transform.position = spawnPos;
        flake.obj.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);

        // Random rotation to face camera better
        flake.obj.transform.rotation = Quaternion.Euler(
            Random.Range(-20f, 20f),
            Random.Range(0f, 360f),
            Random.Range(-20f, 20f)
        );

        flake.obj.GetComponent<Renderer>().material = snowMat;
        Object.Destroy(flake.obj.GetComponent<Collider>());

        // Set properties
        flake.size = flake.obj.transform.localScale.x;
        flake.swayOffset = Random.Range(0f, Mathf.PI * 2f);
        flake.swaySpeed = Random.Range(1f, 3f);

        snowflakes.Add(flake);
    }

    void RespawnSnowflake(Snowflake flake, Vector3 center)
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(0f, spawnRadius);

        flake.obj.transform.position = new Vector3(
            center.x + Mathf.Cos(angle) * dist,
            center.y + spawnHeight + Random.Range(0f, 5f),
            center.z + Mathf.Sin(angle) * dist
        );

        // New random size
        float newSize = Random.Range(0.03f, 0.08f);
        flake.obj.transform.localScale = Vector3.one * newSize;
        flake.size = newSize;
        flake.swayOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void OnDestroy()
    {
        // Clean up snowflakes
        foreach (Snowflake flake in snowflakes)
        {
            if (flake.obj != null)
            {
                Destroy(flake.obj);
            }
        }
        snowflakes.Clear();
    }
}

/// <summary>
/// Wind sound effect for Ice Realm atmosphere
/// </summary>
public class WindAmbience : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 0.15f;
        audioSource.spatialBlend = 0f; // 2D sound

        // Generate wind sound
        AudioClip windClip = CreateWindClip();
        audioSource.clip = windClip;
        audioSource.Play();
    }

    AudioClip CreateWindClip()
    {
        int sampleRate = 44100;
        int samples = sampleRate * 10; // 10 seconds loop
        float[] data = new float[samples];

        // Multiple noise layers for wind texture
        float[] noiseOctaves = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;

            // Base white noise
            float noise = (Random.value - 0.5f) * 2f;

            // Slow modulation (wind gusts)
            float gust1 = Mathf.Sin(t * 0.3f) * 0.5f + 0.5f;
            float gust2 = Mathf.Sin(t * 0.7f + 1.5f) * 0.3f + 0.7f;
            float gust3 = Mathf.Sin(t * 0.15f + 3f) * 0.4f + 0.6f;

            float gustEnvelope = gust1 * gust2 * gust3;

            // Low frequency rumble
            float lowRumble = Mathf.Sin(t * 20f) * 0.1f;
            lowRumble += Mathf.Sin(t * 35f) * 0.05f;

            // Combine
            data[i] = (noise * 0.3f + lowRumble) * gustEnvelope;

            // Light high-pass filter simulation (reduce bass)
            if (i > 0)
            {
                data[i] = data[i] * 0.7f + data[i - 1] * 0.3f;
            }
        }

        // Smooth the looping point
        int fadeLen = sampleRate / 4;
        for (int i = 0; i < fadeLen; i++)
        {
            float fade = (float)i / fadeLen;
            data[i] = data[i] * fade;
            data[samples - 1 - i] = data[samples - 1 - i] * fade;
        }

        AudioClip clip = AudioClip.Create("WindAmbience", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
