using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages simple birds flying overhead - simplified for performance
/// </summary>
public class BirdFlock : MonoBehaviour
{
    public static BirdFlock Instance { get; private set; }

    private List<GameObject> activeBirds = new List<GameObject>();
    private List<Vector3> birdDirections = new List<Vector3>();
    private List<float> birdSpeeds = new List<float>();
    private List<float> birdBaseY = new List<float>();

    private float nextFlockTime = 0f;
    private int maxBirds = 10;

    private Material birdMaterial;
    private bool initialized = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Delay initialization
        Invoke("Initialize", 1.5f);
    }

    void Initialize()
    {
        // Create a simple unlit material
        birdMaterial = new Material(Shader.Find("Unlit/Color"));
        if (birdMaterial != null)
        {
            birdMaterial.color = new Color(0.9f, 0.9f, 0.9f);
        }
        initialized = true;
        nextFlockTime = Time.time + Random.Range(10f, 20f);
    }

    void Update()
    {
        if (!initialized || !MainMenu.GameStarted) return;

        // Spawn new flocks periodically
        if (Time.time >= nextFlockTime && activeBirds.Count < maxBirds)
        {
            int count = Mathf.Min(Random.Range(2, 5), maxBirds - activeBirds.Count);
            if (count > 0) SpawnFlock(count);
            nextFlockTime = Time.time + Random.Range(30f, 60f);
        }

        // Update birds
        for (int i = activeBirds.Count - 1; i >= 0; i--)
        {
            if (activeBirds[i] == null)
            {
                activeBirds.RemoveAt(i);
                birdDirections.RemoveAt(i);
                birdSpeeds.RemoveAt(i);
                birdBaseY.RemoveAt(i);
                continue;
            }

            // Move bird
            activeBirds[i].transform.position += birdDirections[i] * birdSpeeds[i] * Time.deltaTime;

            // Simple bob
            Vector3 pos = activeBirds[i].transform.position;
            pos.y = birdBaseY[i] + Mathf.Sin(Time.time * 2f + i) * 0.3f;
            activeBirds[i].transform.position = pos;

            // Remove if too far
            if (Mathf.Abs(pos.x) > 120f)
            {
                Destroy(activeBirds[i]);
                activeBirds.RemoveAt(i);
                birdDirections.RemoveAt(i);
                birdSpeeds.RemoveAt(i);
                birdBaseY.RemoveAt(i);
            }
        }
    }

    void SpawnFlock(int count)
    {
        if (birdMaterial == null) return;

        float startX = Random.value > 0.5f ? -80f : 80f;
        float startZ = Random.Range(-10f, 50f);
        float startY = Random.Range(15f, 30f);
        float direction = startX > 0 ? -1f : 1f;

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(-1f, 1f),
                Random.Range(-5f, 5f)
            );

            SpawnBird(new Vector3(startX, startY, startZ) + offset, direction);
        }
    }

    void SpawnBird(Vector3 position, float direction)
    {
        if (birdMaterial == null) return;

        // Create simple bird (just a small cube)
        GameObject bird = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bird.name = "Bird";
        bird.transform.position = position;
        bird.transform.localScale = new Vector3(0.3f, 0.1f, 0.5f);
        bird.transform.rotation = Quaternion.Euler(0, direction > 0 ? -90f : 90f, 0);

        // Remove collider
        Collider col = bird.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Apply material safely
        Renderer rend = bird.GetComponent<Renderer>();
        if (rend != null && birdMaterial != null)
        {
            rend.sharedMaterial = birdMaterial;
        }

        activeBirds.Add(bird);
        birdDirections.Add(new Vector3(direction, 0, 0));
        birdSpeeds.Add(Random.Range(6f, 10f));
        birdBaseY.Add(position.y);
    }

    void OnDestroy()
    {
        foreach (var bird in activeBirds)
        {
            if (bird != null) Destroy(bird);
        }
        activeBirds.Clear();

        if (birdMaterial != null)
        {
            Destroy(birdMaterial);
        }
    }
}
