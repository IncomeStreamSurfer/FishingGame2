using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages simple boats on the horizon
/// Simplified for performance - no audio generation
/// </summary>
public class HorizonBoats : MonoBehaviour
{
    public static HorizonBoats Instance { get; private set; }

    private List<GameObject> activeBoats = new List<GameObject>();
    private List<Vector3> boatDirections = new List<Vector3>();
    private List<float> boatSpeeds = new List<float>();
    private List<float> boatBaseY = new List<float>();

    private float nextSpawnTime = 0f;
    private float spawnInterval = 30f;
    private int maxBoats = 6; // More boats from all angles

    private Material boatMaterial;
    private bool initialized = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Delay initialization
        Invoke("Initialize", 1f);
    }

    void Initialize()
    {
        // Create a simple unlit material (more reliable)
        boatMaterial = new Material(Shader.Find("Unlit/Color"));
        if (boatMaterial != null)
        {
            boatMaterial.color = new Color(0.3f, 0.25f, 0.2f);
        }
        initialized = true;
        nextSpawnTime = Time.time + 10f;
    }

    void Update()
    {
        if (!initialized || !MainMenu.GameStarted) return;

        // Spawn new boats periodically
        if (Time.time >= nextSpawnTime && activeBoats.Count < maxBoats)
        {
            SpawnBoat();
            nextSpawnTime = Time.time + Random.Range(spawnInterval, spawnInterval * 1.5f);
        }

        // Update boats
        for (int i = activeBoats.Count - 1; i >= 0; i--)
        {
            if (activeBoats[i] == null)
            {
                activeBoats.RemoveAt(i);
                boatDirections.RemoveAt(i);
                boatSpeeds.RemoveAt(i);
                boatBaseY.RemoveAt(i);
                continue;
            }

            // Move boat
            activeBoats[i].transform.position += boatDirections[i] * boatSpeeds[i] * Time.deltaTime;

            // Simple bob
            Vector3 pos = activeBoats[i].transform.position;
            pos.y = boatBaseY[i] + Mathf.Sin(Time.time + i) * 0.2f;
            activeBoats[i].transform.position = pos;

            // Remove if crossed the island or too far
            float distFromCenter = new Vector2(pos.x, pos.z).magnitude;
            if (distFromCenter < 60f || distFromCenter > 200f)
            {
                Destroy(activeBoats[i]);
                activeBoats.RemoveAt(i);
                boatDirections.RemoveAt(i);
                boatSpeeds.RemoveAt(i);
                boatBaseY.RemoveAt(i);
            }
        }
    }

    void SpawnBoat()
    {
        if (boatMaterial == null) return;

        // Boats spawn much farther away and from various angles
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(120f, 180f); // Much farther away
        float startX = Mathf.Cos(angle) * distance;
        float startZ = Mathf.Sin(angle) * distance;

        // Direction toward center (with some randomness)
        Vector3 toCenter = new Vector3(-startX, 0, -startZ).normalized;
        toCenter = Quaternion.Euler(0, Random.Range(-30f, 30f), 0) * toCenter;
        float direction = toCenter.x > 0 ? 1f : -1f;

        Vector3 spawnPos = new Vector3(startX, 0.5f, startZ);

        // Create simple boat (just a cube)
        GameObject boat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boat.name = "Boat";
        boat.transform.position = spawnPos;
        boat.transform.localScale = new Vector3(2f, 1f, 5f);
        boat.transform.rotation = Quaternion.Euler(0, direction > 0 ? -90f : 90f, 0);

        // Remove collider
        Collider col = boat.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Apply material safely
        Renderer rend = boat.GetComponent<Renderer>();
        if (rend != null && boatMaterial != null)
        {
            rend.sharedMaterial = boatMaterial;
        }

        activeBoats.Add(boat);
        boatDirections.Add(toCenter); // Use actual direction toward center
        boatSpeeds.Add(Random.Range(1f, 2f));
        boatBaseY.Add(0.5f);
    }

    void OnDestroy()
    {
        // Clean up boats
        foreach (var boat in activeBoats)
        {
            if (boat != null) Destroy(boat);
        }
        activeBoats.Clear();

        // Clean up material
        if (boatMaterial != null)
        {
            Destroy(boatMaterial);
        }
    }
}
