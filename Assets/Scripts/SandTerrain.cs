using UnityEngine;

/// <summary>
/// Procedural Sand Terrain System
/// Creates a beautiful sandy island with smooth edges that slope into the water
/// Features: Dynamic sand coloring, gentle dunes, wet sand near water, beach details
/// </summary>
public class SandTerrain : MonoBehaviour
{
    [Header("Island Size")]
    public float islandRadius = 50f;
    public float islandHeight = 1.5f;

    [Header("Sand Colors - Vibrant Brown/Beige Tones")]
    public Color drySandColor = new Color(0.82f, 0.68f, 0.45f);      // Rich golden brown sand
    public Color wetSandColor = new Color(0.58f, 0.42f, 0.28f);      // Deep chocolate wet sand
    public Color lightSandColor = new Color(0.90f, 0.80f, 0.58f);    // Warm golden highlights

    [Header("Terrain Settings")]
    public int meshResolution = 64;        // Vertices per side
    public float edgeSlopeWidth = 15f;     // How wide the slope into water is
    public float duneHeight = 0.4f;        // Maximum dune height variation
    public int duneCount = 5;              // Number of major dunes

    [Header("Beach Details")]
    public bool addShells = true;
    public bool addRocks = true;
    public bool addDriftwood = true;
    public bool addFootprints = false;

    // Components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Material sandMaterial;

    void Start()
    {
        GenerateTerrain();
        if (addShells) AddSeashells();
        if (addRocks) AddBeachRocks();
        if (addDriftwood) AddDriftwood();
    }

    public void GenerateTerrain()
    {
        // Setup components
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        // Create the sand material
        CreateSandMaterial();

        // Generate the mesh
        Mesh sandMesh = GenerateSandMesh();
        meshFilter.mesh = sandMesh;
        meshCollider.sharedMesh = sandMesh;
        meshRenderer.material = sandMaterial;
    }

    void CreateSandMaterial()
    {
        sandMaterial = new Material(Shader.Find("Standard"));
        sandMaterial.color = drySandColor;
        sandMaterial.SetFloat("_Glossiness", 0.05f);  // Matte sand
        sandMaterial.SetFloat("_Metallic", 0f);

        // Enable vertex colors for variation
        sandMaterial.EnableKeyword("_VERTEXCOLOR");
    }

    Mesh GenerateSandMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "SandTerrainMesh";

        int vertCount = meshResolution * meshResolution;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        Color[] colors = new Color[vertCount];

        // Generate dune noise seeds
        float[] duneSeeds = new float[duneCount];
        Vector2[] duneCenters = new Vector2[duneCount];
        float[] duneRadii = new float[duneCount];

        for (int i = 0; i < duneCount; i++)
        {
            duneSeeds[i] = Random.Range(0f, 100f);
            duneCenters[i] = new Vector2(
                Random.Range(-islandRadius * 0.6f, islandRadius * 0.6f),
                Random.Range(-islandRadius * 0.6f, islandRadius * 0.6f)
            );
            duneRadii[i] = Random.Range(8f, 20f);
        }

        // Generate vertices
        for (int z = 0; z < meshResolution; z++)
        {
            for (int x = 0; x < meshResolution; x++)
            {
                int index = z * meshResolution + x;

                // Position in world space (centered)
                float worldX = (x / (float)(meshResolution - 1) - 0.5f) * islandRadius * 2f;
                float worldZ = (z / (float)(meshResolution - 1) - 0.5f) * islandRadius * 2f;

                // Distance from center
                float distFromCenter = Mathf.Sqrt(worldX * worldX + worldZ * worldZ);

                // Calculate height
                float height = CalculateHeight(worldX, worldZ, distFromCenter, duneCenters, duneRadii, duneSeeds);

                vertices[index] = new Vector3(worldX, height, worldZ);
                uvs[index] = new Vector2(x / (float)(meshResolution - 1), z / (float)(meshResolution - 1));

                // Color based on height and distance (wet sand near edges)
                colors[index] = CalculateSandColor(height, distFromCenter);
            }
        }

        // Generate triangles
        int[] triangles = new int[(meshResolution - 1) * (meshResolution - 1) * 6];
        int triIndex = 0;

        for (int z = 0; z < meshResolution - 1; z++)
        {
            for (int x = 0; x < meshResolution - 1; x++)
            {
                int bottomLeft = z * meshResolution + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + meshResolution;
                int topRight = topLeft + 1;

                // First triangle
                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomRight;

                // Second triangle
                triangles[triIndex++] = bottomRight;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = topRight;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    float CalculateHeight(float x, float z, float distFromCenter, Vector2[] duneCenters, float[] duneRadii, float[] duneSeeds)
    {
        // Base island shape - flat in center, slopes at edges
        float edgeFactor = Mathf.Clamp01((islandRadius - distFromCenter) / edgeSlopeWidth);
        float baseHeight = islandHeight * SmoothStep(edgeFactor);

        // If outside island radius, slope down into water
        if (distFromCenter > islandRadius - edgeSlopeWidth)
        {
            // Smooth ramp down
            float slopeFactor = 1f - edgeFactor;
            baseHeight = islandHeight * (1f - slopeFactor * slopeFactor) - slopeFactor * 1.5f;
        }

        // Add dune variation (only on the island itself)
        float duneOffset = 0f;
        if (edgeFactor > 0.3f)
        {
            for (int i = 0; i < duneCenters.Length; i++)
            {
                float duneDist = Vector2.Distance(new Vector2(x, z), duneCenters[i]);
                if (duneDist < duneRadii[i])
                {
                    float duneFactor = 1f - (duneDist / duneRadii[i]);
                    duneFactor = duneFactor * duneFactor; // Smooth falloff
                    duneOffset += duneHeight * duneFactor * Mathf.PerlinNoise(x * 0.1f + duneSeeds[i], z * 0.1f);
                }
            }

            // Small-scale sand ripples
            float ripples = Mathf.PerlinNoise(x * 0.5f, z * 0.5f) * 0.05f;
            duneOffset += ripples * edgeFactor;
        }

        return baseHeight + duneOffset;
    }

    float SmoothStep(float t)
    {
        // Hermite smoothstep for natural-looking slopes
        return t * t * (3f - 2f * t);
    }

    Color CalculateSandColor(float height, float distFromCenter)
    {
        // Wet sand near water's edge
        float wetFactor = Mathf.Clamp01((distFromCenter - (islandRadius - edgeSlopeWidth)) / edgeSlopeWidth);

        // Height-based variation (higher = lighter, sun-bleached)
        float heightFactor = Mathf.Clamp01((height - islandHeight * 0.5f) / (islandHeight * 0.5f));

        // Mix colors
        Color baseColor = Color.Lerp(drySandColor, lightSandColor, heightFactor * 0.3f);
        Color finalColor = Color.Lerp(baseColor, wetSandColor, wetFactor);

        // Add subtle random variation
        float variation = Random.Range(-0.02f, 0.02f);
        finalColor.r += variation;
        finalColor.g += variation;
        finalColor.b += variation;

        return finalColor;
    }

    void AddSeashells()
    {
        Material[] shellMats = new Material[3];

        // White/cream shell
        shellMats[0] = new Material(Shader.Find("Standard"));
        shellMats[0].color = new Color(0.95f, 0.93f, 0.88f);
        shellMats[0].SetFloat("_Glossiness", 0.6f);

        // Tan/brown shell
        shellMats[1] = new Material(Shader.Find("Standard"));
        shellMats[1].color = new Color(0.82f, 0.72f, 0.58f);
        shellMats[1].SetFloat("_Glossiness", 0.5f);

        // Pink shell
        shellMats[2] = new Material(Shader.Find("Standard"));
        shellMats[2].color = new Color(0.95f, 0.82f, 0.80f);
        shellMats[2].SetFloat("_Glossiness", 0.65f);

        // Scatter shells
        for (int i = 0; i < 80; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(5f, islandRadius - 5f);
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;
            float y = GetHeightAt(x, z) + 0.02f;

            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shell.name = "Shell";
            shell.transform.SetParent(transform);
            shell.transform.localPosition = new Vector3(x, y, z);

            float size = Random.Range(0.08f, 0.2f);
            shell.transform.localScale = new Vector3(size, size * 0.25f, size * 0.7f);
            shell.transform.localRotation = Quaternion.Euler(
                Random.Range(-8f, 8f),
                Random.Range(0f, 360f),
                Random.Range(-8f, 8f)
            );

            shell.GetComponent<Renderer>().sharedMaterial = shellMats[Random.Range(0, 3)];
            Object.Destroy(shell.GetComponent<Collider>());
        }

        // Add starfish
        Material starfishMat = new Material(Shader.Find("Standard"));
        starfishMat.color = new Color(0.9f, 0.35f, 0.25f);

        for (int i = 0; i < 6; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(10f, islandRadius - 10f);
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;
            float y = GetHeightAt(x, z) + 0.02f;

            CreateStarfish(new Vector3(x, y, z), starfishMat);
        }
    }

    void CreateStarfish(Vector3 pos, Material mat)
    {
        GameObject starfish = new GameObject("Starfish");
        starfish.transform.SetParent(transform);
        starfish.transform.localPosition = pos;
        starfish.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        // 5 arms
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f * Mathf.Deg2Rad;
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm";
            arm.transform.SetParent(starfish.transform);
            arm.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.1f, 0, Mathf.Sin(angle) * 0.1f);
            arm.transform.localScale = new Vector3(0.04f, 0.02f, 0.15f);
            arm.transform.localRotation = Quaternion.Euler(0, -i * 72f, 0);
            arm.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(arm.GetComponent<Collider>());
        }

        // Center
        GameObject center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        center.transform.SetParent(starfish.transform);
        center.transform.localPosition = Vector3.zero;
        center.transform.localScale = new Vector3(0.1f, 0.015f, 0.1f);
        center.GetComponent<Renderer>().sharedMaterial = mat;
        Object.Destroy(center.GetComponent<Collider>());
    }

    void AddBeachRocks()
    {
        Material rockMat1 = new Material(Shader.Find("Standard"));
        rockMat1.color = new Color(0.55f, 0.52f, 0.48f);

        Material rockMat2 = new Material(Shader.Find("Standard"));
        rockMat2.color = new Color(0.42f, 0.40f, 0.38f);

        // Scattered individual rocks
        for (int i = 0; i < 35; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(8f, islandRadius - 3f);
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;
            float y = GetHeightAt(x, z);

            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "BeachRock";
            rock.transform.SetParent(transform);
            rock.transform.localPosition = new Vector3(x, y, z);

            float size = Random.Range(0.2f, 0.6f);
            rock.transform.localScale = new Vector3(size, size * 0.5f, size * 0.75f);
            rock.transform.localRotation = Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(0f, 360f),
                Random.Range(-15f, 15f)
            );

            rock.GetComponent<Renderer>().sharedMaterial = Random.value > 0.5f ? rockMat1 : rockMat2;
            Object.Destroy(rock.GetComponent<Collider>());
        }

        // Rock clusters (2-4 rocks together)
        for (int c = 0; c < 8; c++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(15f, islandRadius - 8f);
            float cx = Mathf.Cos(angle) * dist;
            float cz = Mathf.Sin(angle) * dist;

            int clusterSize = Random.Range(2, 5);
            for (int i = 0; i < clusterSize; i++)
            {
                float ox = Random.Range(-1f, 1f);
                float oz = Random.Range(-1f, 1f);
                float x = cx + ox;
                float z = cz + oz;
                float y = GetHeightAt(x, z);

                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = "ClusterRock";
                rock.transform.SetParent(transform);
                rock.transform.localPosition = new Vector3(x, y, z);

                float size = Random.Range(0.3f, 0.8f);
                rock.transform.localScale = new Vector3(size, size * 0.6f, size * 0.8f);
                rock.transform.localRotation = Quaternion.Euler(
                    Random.Range(-10f, 10f),
                    Random.Range(0f, 360f),
                    Random.Range(-10f, 10f)
                );

                rock.GetComponent<Renderer>().sharedMaterial = Random.value > 0.5f ? rockMat1 : rockMat2;
                Object.Destroy(rock.GetComponent<Collider>());
            }
        }
    }

    void AddDriftwood()
    {
        Material driftwoodMat = new Material(Shader.Find("Standard"));
        driftwoodMat.color = new Color(0.58f, 0.50f, 0.42f);
        driftwoodMat.SetFloat("_Glossiness", 0.1f);

        for (int i = 0; i < 15; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(islandRadius * 0.5f, islandRadius - 5f);
            float x = Mathf.Cos(angle) * dist;
            float z = Mathf.Sin(angle) * dist;
            float y = GetHeightAt(x, z) + 0.05f;

            GameObject wood = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wood.name = "Driftwood";
            wood.transform.SetParent(transform);
            wood.transform.localPosition = new Vector3(x, y, z);

            float length = Random.Range(0.6f, 2f);
            float thickness = Random.Range(0.08f, 0.18f);
            wood.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
            wood.transform.localRotation = Quaternion.Euler(
                88f + Random.Range(-5f, 5f),
                Random.Range(0f, 360f),
                0
            );

            wood.GetComponent<Renderer>().sharedMaterial = driftwoodMat;
            Object.Destroy(wood.GetComponent<Collider>());
        }
    }

    // Get height at a specific point (for placing objects)
    public float GetHeightAt(float x, float z)
    {
        float dist = Mathf.Sqrt(x * x + z * z);
        float edgeFactor = Mathf.Clamp01((islandRadius - dist) / edgeSlopeWidth);
        float height = islandHeight * SmoothStep(edgeFactor);

        if (dist > islandRadius - edgeSlopeWidth)
        {
            float slopeFactor = 1f - edgeFactor;
            height = islandHeight * (1f - slopeFactor * slopeFactor) - slopeFactor * 1.5f;
        }

        return height;
    }
}
