using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Creates volumetric lighting effects - god rays, light shafts, and atmospheric scattering
/// </summary>
public class VolumetricLighting : MonoBehaviour
{
    public static VolumetricLighting Instance { get; private set; }

    [Header("God Ray Settings")]
    public int numRays = 8;
    public float rayLength = 30f;
    public float rayWidth = 2f;

    [Header("Dust Particles")]
    public int numDustMotes = 100;

    private List<GameObject> godRays = new List<GameObject>();
    private List<GameObject> dustMotes = new List<GameObject>();
    private Material rayMaterial;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateRayMaterial();
        Invoke("CreateGodRays", 1f);
        Invoke("CreateDustMotes", 1.2f);
    }

    void CreateRayMaterial()
    {
        rayMaterial = new Material(Shader.Find("Standard"));
        rayMaterial.SetFloat("_Mode", 3); // Transparent
        rayMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        rayMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive
        rayMaterial.SetInt("_ZWrite", 0);
        rayMaterial.EnableKeyword("_ALPHABLEND_ON");
        rayMaterial.renderQueue = 3200;
        rayMaterial.color = new Color(1f, 0.95f, 0.8f, 0.05f);
    }

    void CreateGodRays()
    {
        for (int i = 0; i < numRays; i++)
        {
            GameObject ray = CreateSingleRay(i);
            godRays.Add(ray);
        }
    }

    GameObject CreateSingleRay(int index)
    {
        GameObject rayObj = new GameObject("GodRay_" + index);
        rayObj.transform.SetParent(transform);

        // Random position spread
        float angle = (index / (float)numRays) * 360f + Random.Range(-20f, 20f);
        float dist = Random.Range(5f, 25f);
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * dist;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * dist;

        rayObj.transform.position = new Vector3(x, 15f, z);

        // Create cone/beam shape using stretched cube
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "Beam";
        beam.transform.SetParent(rayObj.transform);
        beam.transform.localPosition = Vector3.zero;

        float width = rayWidth * Random.Range(0.7f, 1.3f);
        float length = rayLength * Random.Range(0.8f, 1.2f);
        beam.transform.localScale = new Vector3(width, length, width * 0.3f);

        // Angle the ray down from sky
        beam.transform.localRotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-10f, 10f));

        Destroy(beam.GetComponent<Collider>());

        // Create unique material for each ray (so we can fade individually)
        Material mat = new Material(rayMaterial);
        beam.GetComponent<Renderer>().material = mat;

        return rayObj;
    }

    void CreateDustMotes()
    {
        Material dustMat = new Material(Shader.Find("Standard"));
        dustMat.SetFloat("_Mode", 3);
        dustMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        dustMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        dustMat.EnableKeyword("_ALPHABLEND_ON");
        dustMat.renderQueue = 3100;
        // Golden/warm dust motes - not white
        dustMat.color = new Color(1f, 0.9f, 0.65f, 0.25f);
        dustMat.EnableKeyword("_EMISSION");
        dustMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.5f) * 0.25f);

        for (int i = 0; i < numDustMotes; i++)
        {
            GameObject mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mote.name = "DustMote";
            mote.transform.SetParent(transform);

            // Random position in air
            mote.transform.position = new Vector3(
                Random.Range(-30f, 30f),
                Random.Range(2f, 10f),
                Random.Range(-30f, 30f)
            );

            float size = Random.Range(0.02f, 0.06f);
            mote.transform.localScale = Vector3.one * size;

            Destroy(mote.GetComponent<Collider>());

            Material moteMat = new Material(dustMat);
            mote.GetComponent<Renderer>().material = moteMat;

            dustMotes.Add(mote);
        }
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        UpdateGodRays();
        UpdateDustMotes();
    }

    void UpdateGodRays()
    {
        if (DayNightCycle.Instance == null) return;

        float daylight = DayNightCycle.Instance.GetDaylightIntensity();
        Color sunColor = DayNightCycle.Instance.GetSunColor();
        Vector3 sunDir = DayNightCycle.Instance.GetSunDirection();

        // Only show god rays during day with good sun angle
        float rayVisibility = Mathf.Clamp01(daylight - 0.3f) * Mathf.Clamp01(sunDir.y + 0.2f);

        for (int i = 0; i < godRays.Count; i++)
        {
            GameObject ray = godRays[i];
            if (ray == null) continue;

            // Position rays based on sun direction
            float angle = (i / (float)godRays.Count) * 360f;
            float dist = 15f + Mathf.Sin(Time.time * 0.2f + i) * 5f;
            float x = sunDir.x * 20f + Mathf.Cos(angle * Mathf.Deg2Rad) * dist;
            float z = sunDir.z * 20f + Mathf.Sin(angle * Mathf.Deg2Rad) * dist;

            ray.transform.position = new Vector3(x, 20f, z);

            // Rotate to align with sun direction
            Transform beam = ray.transform.Find("Beam");
            if (beam != null)
            {
                Vector3 rayDir = -sunDir;
                rayDir.y = Mathf.Min(rayDir.y, -0.3f); // Always somewhat downward
                beam.rotation = Quaternion.LookRotation(rayDir) * Quaternion.Euler(90, 0, 0);

                // Update material
                Renderer r = beam.GetComponent<Renderer>();
                if (r != null)
                {
                    // Shimmer effect
                    float shimmer = 0.03f + Mathf.Sin(Time.time * 2f + i * 0.5f) * 0.02f;
                    Color rayColor = new Color(sunColor.r, sunColor.g * 0.95f, sunColor.b * 0.8f, shimmer * rayVisibility);
                    r.material.color = rayColor;
                }
            }

            ray.SetActive(rayVisibility > 0.1f);
        }
    }

    void UpdateDustMotes()
    {
        if (DayNightCycle.Instance == null) return;

        float daylight = DayNightCycle.Instance.GetDaylightIntensity();
        Color sunColor = DayNightCycle.Instance.GetSunColor();

        // Motes more visible in god rays during day
        float moteVisibility = Mathf.Clamp01(daylight);

        foreach (GameObject mote in dustMotes)
        {
            if (mote == null) continue;

            // Gentle floating motion
            Vector3 pos = mote.transform.position;
            float uniqueOffset = mote.GetHashCode() * 0.0001f;

            pos.x += Mathf.Sin(Time.time * 0.3f + uniqueOffset) * 0.01f;
            pos.y += Mathf.Sin(Time.time * 0.5f + uniqueOffset * 2f) * 0.005f;
            pos.z += Mathf.Cos(Time.time * 0.4f + uniqueOffset) * 0.008f;

            // Wrap around
            if (pos.x > 35f) pos.x = -35f;
            if (pos.x < -35f) pos.x = 35f;
            if (pos.z > 35f) pos.z = -35f;
            if (pos.z < -35f) pos.z = 35f;
            if (pos.y > 12f) pos.y = 2f;
            if (pos.y < 2f) pos.y = 12f;

            mote.transform.position = pos;

            // Update visibility
            Renderer r = mote.GetComponent<Renderer>();
            if (r != null)
            {
                float sparkle = 0.2f + Mathf.Sin(Time.time * 5f + uniqueOffset * 10f) * 0.15f;
                Color moteColor = new Color(sunColor.r, sunColor.g, sunColor.b * 0.9f, sparkle * moteVisibility);
                r.material.color = moteColor;

                // Brighter emission when in sunlight
                if (moteVisibility > 0.5f)
                {
                    r.material.SetColor("_EmissionColor", sunColor * sparkle * 0.5f);
                }
            }
        }
    }

    void OnDestroy()
    {
        foreach (var ray in godRays)
        {
            if (ray != null) Destroy(ray);
        }
        foreach (var mote in dustMotes)
        {
            if (mote != null) Destroy(mote);
        }
    }
}
