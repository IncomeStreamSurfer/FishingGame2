using UnityEngine;

/// <summary>
/// Creates a waterfall visual and audio effect
/// Includes cascading water, mist, and splashing sounds
/// </summary>
public class WaterfallEffect : MonoBehaviour
{
    [Header("Settings")]
    public float height = 15f;
    public float width = 8f;

    private AudioSource waterfallAudio;
    private GameObject[] waterStreams;
    private GameObject[] mistParticles;
    private float animTimer = 0f;

    void Start()
    {
        // PERFORMANCE: Skip if performance mode enabled
        if (PerformanceMode.ShouldSkip(this)) return;

        CreateWaterfall();
        CreateMist();
        CreateAudio();
        CreatePool();
    }

    void CreateWaterfall()
    {
        // Materials
        Material waterMat = new Material(Shader.Find("Standard"));
        waterMat.color = new Color(0.4f, 0.7f, 0.9f, 0.7f);
        waterMat.SetFloat("_Mode", 3); // Transparent
        waterMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        waterMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        waterMat.SetInt("_ZWrite", 0);
        waterMat.DisableKeyword("_ALPHATEST_ON");
        waterMat.EnableKeyword("_ALPHABLEND_ON");
        waterMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        waterMat.renderQueue = 3000;

        Material foamMat = new Material(Shader.Find("Standard"));
        foamMat.color = new Color(0.9f, 0.95f, 1f, 0.8f);

        Material rockMat = new Material(Shader.Find("Standard"));
        rockMat.color = new Color(0.35f, 0.32f, 0.28f);

        // Rock face behind waterfall
        GameObject rockFace = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rockFace.name = "RockFace";
        rockFace.transform.SetParent(transform);
        rockFace.transform.localPosition = new Vector3(0, height / 2, -1f);
        rockFace.transform.localScale = new Vector3(width + 4, height + 2, 2f);
        rockFace.GetComponent<Renderer>().material = rockMat;
        Object.Destroy(rockFace.GetComponent<Collider>());

        // Add rock texture details
        for (int i = 0; i < 15; i++)
        {
            GameObject rockDetail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rockDetail.name = "RockDetail";
            rockDetail.transform.SetParent(rockFace.transform);
            rockDetail.transform.localPosition = new Vector3(
                Random.Range(-0.4f, 0.4f),
                Random.Range(-0.4f, 0.4f),
                0.51f
            );
            rockDetail.transform.localScale = new Vector3(
                Random.Range(0.1f, 0.3f),
                Random.Range(0.1f, 0.2f),
                0.05f
            );
            rockDetail.transform.localRotation = Quaternion.Euler(
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f),
                Random.Range(-20f, 20f)
            );
            Material detailMat = new Material(Shader.Find("Standard"));
            detailMat.color = new Color(
                0.3f + Random.Range(-0.05f, 0.05f),
                0.28f + Random.Range(-0.05f, 0.05f),
                0.25f + Random.Range(-0.05f, 0.05f)
            );
            rockDetail.GetComponent<Renderer>().material = detailMat;
            Object.Destroy(rockDetail.GetComponent<Collider>());
        }

        // Water streams
        int streamCount = 5;
        waterStreams = new GameObject[streamCount];

        for (int i = 0; i < streamCount; i++)
        {
            float xOffset = (i - streamCount / 2) * (width / streamCount);

            GameObject stream = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stream.name = "WaterStream" + i;
            stream.transform.SetParent(transform);
            stream.transform.localPosition = new Vector3(xOffset + Random.Range(-0.5f, 0.5f), height / 2, 0);
            stream.transform.localScale = new Vector3(
                width / streamCount * 0.8f,
                height,
                0.3f + Random.Range(0f, 0.2f)
            );

            Material streamMat = new Material(waterMat);
            streamMat.color = new Color(
                0.4f + Random.Range(-0.05f, 0.05f),
                0.7f + Random.Range(-0.05f, 0.05f),
                0.9f,
                0.6f + Random.Range(-0.1f, 0.1f)
            );
            stream.GetComponent<Renderer>().material = streamMat;
            Object.Destroy(stream.GetComponent<Collider>());

            waterStreams[i] = stream;
        }

        // Top ledge
        GameObject ledge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ledge.name = "Ledge";
        ledge.transform.SetParent(transform);
        ledge.transform.localPosition = new Vector3(0, height + 0.5f, 0.5f);
        ledge.transform.localScale = new Vector3(width + 2, 1f, 2f);
        ledge.GetComponent<Renderer>().material = rockMat;

        // Foam at base
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            GameObject foam = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foam.name = "Foam";
            foam.transform.SetParent(transform);
            foam.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * width * 0.4f,
                0.3f,
                1f + Mathf.Sin(angle) * 1.5f
            );
            foam.transform.localScale = new Vector3(
                Random.Range(0.8f, 1.5f),
                0.3f,
                Random.Range(0.8f, 1.5f)
            );
            foam.GetComponent<Renderer>().material = foamMat;
            Object.Destroy(foam.GetComponent<Collider>());
        }
    }

    void CreateMist()
    {
        Material mistMat = new Material(Shader.Find("Standard"));
        mistMat.color = new Color(0.9f, 0.95f, 1f, 0.3f);
        mistMat.SetFloat("_Mode", 3);
        mistMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mistMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mistMat.SetInt("_ZWrite", 0);
        mistMat.EnableKeyword("_ALPHABLEND_ON");
        mistMat.renderQueue = 3100;

        int mistCount = 12;
        mistParticles = new GameObject[mistCount];

        for (int i = 0; i < mistCount; i++)
        {
            GameObject mist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mist.name = "Mist" + i;
            mist.transform.SetParent(transform);
            mist.transform.localPosition = new Vector3(
                Random.Range(-width / 2, width / 2),
                Random.Range(0f, 3f),
                Random.Range(0f, 3f)
            );
            float size = Random.Range(1f, 3f);
            mist.transform.localScale = new Vector3(size, size * 0.5f, size);
            mist.GetComponent<Renderer>().material = mistMat;
            Object.Destroy(mist.GetComponent<Collider>());

            mistParticles[i] = mist;
        }
    }

    void CreatePool()
    {
        Material poolMat = new Material(Shader.Find("Standard"));
        poolMat.color = new Color(0.2f, 0.5f, 0.6f, 0.8f);
        poolMat.SetFloat("_Glossiness", 0.9f);

        GameObject pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pool.name = "Pool";
        pool.transform.SetParent(transform);
        pool.transform.localPosition = new Vector3(0, 0.1f, 2f);
        pool.transform.localScale = new Vector3(width * 1.2f, 0.2f, width * 0.8f);
        pool.GetComponent<Renderer>().material = poolMat;

        // Pool can be fished in
        pool.tag = "Untagged"; // Could set to "Water" if tag exists
        pool.name = "WaterfallPool_Water";
    }

    void CreateAudio()
    {
        waterfallAudio = gameObject.AddComponent<AudioSource>();
        waterfallAudio.loop = true;
        waterfallAudio.spatialBlend = 1f;
        waterfallAudio.maxDistance = 40f;
        waterfallAudio.rolloffMode = AudioRolloffMode.Linear;
        waterfallAudio.volume = 0.6f;
        waterfallAudio.clip = GenerateWaterfallSound();
        waterfallAudio.Play();
    }

    AudioClip GenerateWaterfallSound()
    {
        int sampleRate = 22050;
        int samples = sampleRate * 4;
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;

            // Rushing water (filtered white noise)
            float water = Random.Range(-1f, 1f);

            // Low frequency rumble
            water += Mathf.Sin(t * 30f) * 0.2f;
            water += Mathf.Sin(t * 50f) * 0.15f;
            water += Mathf.Sin(t * 80f) * 0.1f;

            // Occasional splashes
            if (Random.value < 0.005f)
            {
                water += Random.Range(-0.5f, 0.5f);
            }

            // Smooth it out
            data[i] = water * 0.4f;
        }

        // Simple low-pass filter
        for (int i = 1; i < samples; i++)
        {
            data[i] = data[i] * 0.3f + data[i - 1] * 0.7f;
        }

        AudioClip clip = AudioClip.Create("Waterfall", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    void Update()
    {
        animTimer += Time.deltaTime;

        // Animate water streams
        if (waterStreams != null)
        {
            for (int i = 0; i < waterStreams.Length; i++)
            {
                if (waterStreams[i] != null)
                {
                    Vector3 scale = waterStreams[i].transform.localScale;
                    scale.z = 0.3f + Mathf.Sin(animTimer * 3f + i) * 0.1f;
                    waterStreams[i].transform.localScale = scale;

                    // Slight position wobble
                    Vector3 pos = waterStreams[i].transform.localPosition;
                    pos.x += Mathf.Sin(animTimer * 2f + i * 0.5f) * 0.01f;
                    waterStreams[i].transform.localPosition = pos;
                }
            }
        }

        // Animate mist
        if (mistParticles != null)
        {
            for (int i = 0; i < mistParticles.Length; i++)
            {
                if (mistParticles[i] != null)
                {
                    Vector3 pos = mistParticles[i].transform.localPosition;

                    // Float upward and drift
                    pos.y += Time.deltaTime * 0.5f;
                    pos.x += Mathf.Sin(animTimer + i) * Time.deltaTime * 0.3f;
                    pos.z += Time.deltaTime * 0.2f;

                    // Reset when too high
                    if (pos.y > 5f)
                    {
                        pos.y = 0f;
                        pos.x = Random.Range(-width / 2, width / 2);
                        pos.z = Random.Range(0f, 2f);
                    }

                    mistParticles[i].transform.localPosition = pos;

                    // Fade based on height
                    Renderer r = mistParticles[i].GetComponent<Renderer>();
                    if (r != null)
                    {
                        Color c = r.material.color;
                        c.a = 0.3f * (1f - pos.y / 5f);
                        r.material.color = c;
                    }
                }
            }
        }
    }
}
