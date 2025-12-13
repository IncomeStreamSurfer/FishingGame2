using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Shooting star effect system for nighttime atmosphere
/// Spawns random shooting stars across the night sky
/// Only active when DayNightCycle.Instance.IsNight() returns true
/// </summary>
public class ShootingStars : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Minimum time between shooting stars (seconds)")]
    public float minSpawnInterval = 10f;

    [Tooltip("Maximum time between shooting stars (seconds)")]
    public float maxSpawnInterval = 30f;

    [Header("Shooting Star Settings")]
    [Tooltip("How long each shooting star lasts (seconds)")]
    public float starLifetime = 1.5f;

    [Tooltip("How fast the star moves across the sky")]
    public float starSpeed = 50f;

    [Tooltip("Size of the shooting star")]
    public Vector3 starSize = new Vector3(0.8f, 0.1f, 0.1f);

    [Tooltip("Color of the shooting star")]
    public Color starColor = new Color(1f, 0.95f, 0.8f);

    [Tooltip("Emission intensity for glow effect")]
    public float emissionIntensity = 3f;

    [Header("Spawn Area")]
    [Tooltip("Distance from player/origin to spawn stars")]
    public float spawnDistance = 80f;

    [Tooltip("Minimum height above origin to spawn stars")]
    public float minHeight = 40f;

    [Tooltip("Maximum height above origin to spawn stars")]
    public float maxHeight = 60f;

    [Tooltip("Minimum angle (degrees) of shooting star trajectory")]
    public float minAngle = 45f;

    [Tooltip("Maximum angle (degrees) of shooting star trajectory")]
    public float maxAngle = 60f;

    // Active shooting stars
    private List<ShootingStar> activeStars = new List<ShootingStar>();

    // Shared materials for performance
    private Material starMaterial;
    private Material trailMaterial;

    // Spawn coroutine
    private Coroutine spawnCoroutine;

    // Night tracking
    private bool wasNight = false;

    private class ShootingStar
    {
        public GameObject starObject;
        public GameObject trailObject;
        public Vector3 velocity;
        public float lifetime;
        public float age;
    }

    void Start()
    {
        CreateMaterials();

        // Check if it's already night and start if so
        if (DayNightCycle.Instance != null && DayNightCycle.Instance.IsNight())
        {
            StartSpawning();
            wasNight = true;
        }
    }

    void CreateMaterials()
    {
        // Create star material (bright emissive)
        starMaterial = new Material(Shader.Find("Standard"));
        starMaterial.color = starColor;
        starMaterial.EnableKeyword("_EMISSION");
        starMaterial.SetColor("_EmissionColor", starColor * emissionIntensity);
        starMaterial.SetFloat("_Metallic", 0f);
        starMaterial.SetFloat("_Glossiness", 1f);

        // Create trail material (transparent with emission)
        trailMaterial = new Material(Shader.Find("Standard"));
        trailMaterial.SetFloat("_Mode", 3); // Transparent
        trailMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        trailMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        trailMaterial.SetInt("_ZWrite", 0);
        trailMaterial.EnableKeyword("_ALPHABLEND_ON");
        trailMaterial.renderQueue = 3000;
        trailMaterial.color = new Color(starColor.r, starColor.g, starColor.b, 0.4f);
        trailMaterial.EnableKeyword("_EMISSION");
        trailMaterial.SetColor("_EmissionColor", starColor * (emissionIntensity * 0.5f));
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check day/night transition
        CheckNightCycle();

        // Update active shooting stars
        UpdateShootingStars();
    }

    void CheckNightCycle()
    {
        if (DayNightCycle.Instance == null) return;

        bool isNight = DayNightCycle.Instance.IsNight();

        // Night just started
        if (isNight && !wasNight)
        {
            StartSpawning();
        }
        // Day just started
        else if (!isNight && wasNight)
        {
            StopSpawning();
        }

        wasNight = isNight;
    }

    void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }

    void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Wait random interval between stars
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Double-check we're still at night
            if (DayNightCycle.Instance != null && DayNightCycle.Instance.IsNight())
            {
                SpawnShootingStar();
            }
        }
    }

    void SpawnShootingStar()
    {
        ShootingStar star = new ShootingStar();

        // Random spawn position in the upper sky
        // Spawn at a random angle around the horizon
        float horizonAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float height = Random.Range(minHeight, maxHeight);

        Vector3 spawnPos = new Vector3(
            Mathf.Cos(horizonAngle) * spawnDistance,
            height,
            Mathf.Sin(horizonAngle) * spawnDistance
        );

        // Create the main star object (elongated cube)
        star.starObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        star.starObject.name = "ShootingStar";
        star.starObject.transform.SetParent(transform);
        star.starObject.transform.position = spawnPos;
        star.starObject.transform.localScale = starSize;
        Destroy(star.starObject.GetComponent<Collider>());
        star.starObject.GetComponent<Renderer>().material = starMaterial;

        // Create trail object (slightly larger, more transparent)
        star.trailObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        star.trailObject.name = "ShootingStarTrail";
        star.trailObject.transform.SetParent(star.starObject.transform);
        star.trailObject.transform.localPosition = Vector3.zero;
        star.trailObject.transform.localScale = new Vector3(1.5f, 1.2f, 1.2f);
        Destroy(star.trailObject.GetComponent<Collider>());
        star.trailObject.GetComponent<Renderer>().material = trailMaterial;

        // Calculate velocity (downward at angle)
        float trajectoryAngle = Random.Range(minAngle, maxAngle) * Mathf.Deg2Rad;

        // Random horizontal direction (but generally moving across the sky)
        float horizontalAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Velocity components
        float horizontalSpeed = Mathf.Cos(trajectoryAngle) * starSpeed;
        float verticalSpeed = -Mathf.Sin(trajectoryAngle) * starSpeed;

        star.velocity = new Vector3(
            Mathf.Cos(horizontalAngle) * horizontalSpeed,
            verticalSpeed,
            Mathf.Sin(horizontalAngle) * horizontalSpeed
        );

        // Orient the star to face its movement direction
        if (star.velocity.magnitude > 0.01f)
        {
            star.starObject.transform.rotation = Quaternion.LookRotation(star.velocity.normalized);
        }

        // Set lifetime
        star.lifetime = starLifetime;
        star.age = 0f;

        activeStars.Add(star);
    }

    void UpdateShootingStars()
    {
        for (int i = activeStars.Count - 1; i >= 0; i--)
        {
            ShootingStar star = activeStars[i];

            if (star.starObject == null)
            {
                activeStars.RemoveAt(i);
                continue;
            }

            // Update age
            star.age += Time.deltaTime;

            // Move the star
            star.starObject.transform.position += star.velocity * Time.deltaTime;

            // Fade out based on lifetime
            float lifetimePercent = star.age / star.lifetime;

            // Fade in quickly at start, fade out at end
            float alpha = 1f;
            if (lifetimePercent < 0.1f)
            {
                // Fade in over first 10% of lifetime
                alpha = lifetimePercent / 0.1f;
            }
            else if (lifetimePercent > 0.7f)
            {
                // Fade out over last 30% of lifetime
                alpha = 1f - ((lifetimePercent - 0.7f) / 0.3f);
            }

            // Update star material emission
            Renderer starRenderer = star.starObject.GetComponent<Renderer>();
            if (starRenderer != null && starRenderer.material != null)
            {
                Color emissionColor = starColor * emissionIntensity * alpha;
                starRenderer.material.SetColor("_EmissionColor", emissionColor);
            }

            // Update trail material transparency
            if (star.trailObject != null)
            {
                Renderer trailRenderer = star.trailObject.GetComponent<Renderer>();
                if (trailRenderer != null && trailRenderer.material != null)
                {
                    Color trailColor = new Color(starColor.r, starColor.g, starColor.b, 0.4f * alpha);
                    trailRenderer.material.color = trailColor;
                    Color trailEmission = starColor * (emissionIntensity * 0.5f) * alpha;
                    trailRenderer.material.SetColor("_EmissionColor", trailEmission);
                }
            }

            // Remove if lifetime expired
            if (star.age >= star.lifetime)
            {
                if (star.trailObject != null)
                    Destroy(star.trailObject);
                if (star.starObject != null)
                    Destroy(star.starObject);
                activeStars.RemoveAt(i);
            }
        }
    }

    void OnDestroy()
    {
        // Clean up all active shooting stars
        foreach (ShootingStar star in activeStars)
        {
            if (star.trailObject != null)
                Destroy(star.trailObject);
            if (star.starObject != null)
                Destroy(star.starObject);
        }
        activeStars.Clear();

        // Clean up materials
        if (starMaterial != null)
            Destroy(starMaterial);
        if (trailMaterial != null)
            Destroy(trailMaterial);
    }
}
