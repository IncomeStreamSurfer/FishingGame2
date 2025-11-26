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
    private float spawnInterval = 45f;
    private int maxBoats = 2;

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

            // Remove if too far
            if (Mathf.Abs(pos.x) > 150f)
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

        float horizonZ = 70f + Random.Range(0f, 30f);
        float startX = Random.value > 0.5f ? -100f : 100f;
        float direction = startX > 0 ? -1f : 1f;

        Vector3 spawnPos = new Vector3(startX, 0.5f, horizonZ);

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
        boatDirections.Add(new Vector3(direction, 0, 0));
        boatSpeeds.Add(Random.Range(1.5f, 3f));
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
