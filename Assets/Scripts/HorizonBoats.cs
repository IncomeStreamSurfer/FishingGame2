using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns and manages boats, planes with ad banners, and clouds on the horizon
/// </summary>
public class HorizonBoats : MonoBehaviour
{
    public static HorizonBoats Instance { get; private set; }

    // Boats
    private List<GameObject> activeBoats = new List<GameObject>();
    private List<Vector3> boatDirections = new List<Vector3>();
    private List<float> boatSpeeds = new List<float>();
    private List<float> boatBaseY = new List<float>();
    private float nextBoatSpawnTime = 0f;
    private float boatSpawnInterval = 15f;
    private int maxBoats = 12;

    // Planes with ad banners (occasional)
    private List<GameObject> activePlanes = new List<GameObject>();
    private List<Vector3> planeDirections = new List<Vector3>();
    private List<float> planeSpeeds = new List<float>();
    private float nextPlaneSpawnTime = 0f;
    private float planeSpawnInterval = 90f;  // Less frequent - every 90-180 seconds
    private int maxPlanes = 1;  // Only 1 plane at a time

    // Clouds
    private List<GameObject> activeClouds = new List<GameObject>();
    private List<Vector3> cloudDirections = new List<Vector3>();
    private List<float> cloudSpeeds = new List<float>();
    private float nextCloudSpawnTime = 0f;
    private float cloudSpawnInterval = 20f;
    private int maxClouds = 8;

    // Ad messages for planes
    private string[] adMessages = {
        "EAT AT JOE'S",
        "FISH MORE!",
        "VISIT PETE'S",
        "SALE TODAY!",
        "FRESH FISH",
        "ISLAND TOURS",
        "DRINK BANKS",
        "BBQ TONIGHT",
        "CATCH OF DAY",
        "GO FISHING!"
    };

    // Materials
    private Dictionary<string, Material> materials = new Dictionary<string, Material>();
    private bool initialized = false;

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
        CreateMaterials();
        initialized = true;
        nextBoatSpawnTime = Time.time + 5f;
        nextPlaneSpawnTime = Time.time + 20f;
        nextCloudSpawnTime = Time.time + 2f;

        // Spawn a few initial boats
        for (int i = 0; i < 4; i++)
        {
            SpawnBoat();
        }

        // Spawn initial clouds
        for (int i = 0; i < 5; i++)
        {
            SpawnCloud(true);
        }
    }

    void CreateMaterials()
    {
        // Boat materials
        materials["wood_dark"] = CreateMaterial(new Color(0.35f, 0.25f, 0.15f));
        materials["wood_light"] = CreateMaterial(new Color(0.55f, 0.4f, 0.25f));
        materials["white"] = CreateMaterial(new Color(0.95f, 0.95f, 0.95f));
        materials["red"] = CreateMaterial(new Color(0.8f, 0.2f, 0.15f));
        materials["blue"] = CreateMaterial(new Color(0.2f, 0.35f, 0.7f));
        materials["gray"] = CreateMaterial(new Color(0.4f, 0.4f, 0.45f));
        materials["black"] = CreateMaterial(new Color(0.15f, 0.15f, 0.15f));
        materials["yellow"] = CreateMaterial(new Color(0.9f, 0.8f, 0.2f));
        materials["sail"] = CreateMaterial(new Color(1f, 0.98f, 0.9f));
        materials["cloud"] = CreateMaterial(new Color(1f, 1f, 1f, 0.9f));
        materials["banner_red"] = CreateMaterial(new Color(0.9f, 0.15f, 0.1f));
        materials["banner_yellow"] = CreateMaterial(new Color(1f, 0.9f, 0.2f));
        materials["banner_blue"] = CreateMaterial(new Color(0.2f, 0.5f, 0.9f));

        // New boat-specific materials
        materials["orange_hull"] = CreateMaterial(new Color(0.85f, 0.5f, 0.2f));  // Orange/brown sailboat
        materials["sail_cream"] = CreateMaterial(new Color(1f, 0.98f, 0.85f));    // Cream colored sails
        materials["navy_blue"] = CreateMaterial(new Color(0.1f, 0.15f, 0.35f));   // Dark navy blue
        materials["dark_red"] = CreateMaterial(new Color(0.5f, 0.15f, 0.12f));    // Dark red/maroon waterline
        materials["container_red"] = CreateMaterial(new Color(0.65f, 0.2f, 0.18f)); // Container red
        materials["container_teal"] = CreateMaterial(new Color(0.2f, 0.45f, 0.45f)); // Teal containers
        materials["cream"] = CreateMaterial(new Color(0.95f, 0.9f, 0.8f));        // Cream/tan bridge
    }

    Material CreateMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = color;
        return mat;
    }

    void Update()
    {
        if (!initialized || !MainMenu.GameStarted) return;

        // Spawn boats
        if (Time.time >= nextBoatSpawnTime && activeBoats.Count < maxBoats)
        {
            SpawnBoat();
            nextBoatSpawnTime = Time.time + Random.Range(boatSpawnInterval * 0.5f, boatSpawnInterval * 1.5f);
        }

        // Spawn planes
        if (Time.time >= nextPlaneSpawnTime && activePlanes.Count < maxPlanes)
        {
            SpawnPlane();
            nextPlaneSpawnTime = Time.time + Random.Range(planeSpawnInterval, planeSpawnInterval * 2f);
        }

        // Spawn clouds
        if (Time.time >= nextCloudSpawnTime && activeClouds.Count < maxClouds)
        {
            SpawnCloud(false);
            nextCloudSpawnTime = Time.time + Random.Range(cloudSpawnInterval * 0.5f, cloudSpawnInterval * 1.5f);
        }

        UpdateBoats();
        UpdatePlanes();
        UpdateClouds();
    }

    void UpdateBoats()
    {
        for (int i = activeBoats.Count - 1; i >= 0; i--)
        {
            if (activeBoats[i] == null)
            {
                RemoveBoat(i);
                continue;
            }

            // Move boat
            activeBoats[i].transform.position += boatDirections[i] * boatSpeeds[i] * Time.deltaTime;

            // Bob on water
            Vector3 pos = activeBoats[i].transform.position;
            pos.y = boatBaseY[i] + Mathf.Sin(Time.time * 1.5f + i * 0.7f) * 0.3f;
            activeBoats[i].transform.position = pos;

            // Gentle rock
            float rock = Mathf.Sin(Time.time * 0.8f + i) * 3f;
            activeBoats[i].transform.rotation = Quaternion.Euler(rock, activeBoats[i].transform.eulerAngles.y, rock * 0.5f);

            // Remove if too far
            float distFromCenter = new Vector2(pos.x, pos.z).magnitude;
            if (distFromCenter < 50f || distFromCenter > 250f)
            {
                Destroy(activeBoats[i]);
                RemoveBoat(i);
            }
        }
    }

    void UpdatePlanes()
    {
        for (int i = activePlanes.Count - 1; i >= 0; i--)
        {
            if (activePlanes[i] == null)
            {
                RemovePlane(i);
                continue;
            }

            // Move plane
            activePlanes[i].transform.position += planeDirections[i] * planeSpeeds[i] * Time.deltaTime;

            // Gentle sway
            float sway = Mathf.Sin(Time.time * 0.5f + i) * 0.5f;
            Vector3 pos = activePlanes[i].transform.position;
            pos.y += sway * Time.deltaTime;
            activePlanes[i].transform.position = pos;

            // Remove if too far
            if (Mathf.Abs(pos.x) > 300f || Mathf.Abs(pos.z) > 300f)
            {
                Destroy(activePlanes[i]);
                RemovePlane(i);
            }
        }
    }

    void UpdateClouds()
    {
        for (int i = activeClouds.Count - 1; i >= 0; i--)
        {
            if (activeClouds[i] == null)
            {
                RemoveCloud(i);
                continue;
            }

            // Move cloud
            activeClouds[i].transform.position += cloudDirections[i] * cloudSpeeds[i] * Time.deltaTime;

            // Remove if too far
            Vector3 pos = activeClouds[i].transform.position;
            if (Mathf.Abs(pos.x) > 200f || Mathf.Abs(pos.z) > 200f)
            {
                Destroy(activeClouds[i]);
                RemoveCloud(i);
            }
        }
    }

    void RemoveBoat(int i)
    {
        activeBoats.RemoveAt(i);
        boatDirections.RemoveAt(i);
        boatSpeeds.RemoveAt(i);
        boatBaseY.RemoveAt(i);
    }

    void RemovePlane(int i)
    {
        activePlanes.RemoveAt(i);
        planeDirections.RemoveAt(i);
        planeSpeeds.RemoveAt(i);
    }

    void RemoveCloud(int i)
    {
        activeClouds.RemoveAt(i);
        cloudDirections.RemoveAt(i);
        cloudSpeeds.RemoveAt(i);
    }

    void SpawnBoat()
    {
        // Random spawn position on horizon
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(100f, 180f);
        Vector3 spawnPos = new Vector3(Mathf.Cos(angle) * distance, 0.5f, Mathf.Sin(angle) * distance);

        // Direction tangent to circle (sailing around the island)
        Vector3 tangent = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
        if (Random.value > 0.5f) tangent = -tangent;

        // Add some variation
        tangent = Quaternion.Euler(0, Random.Range(-30f, 30f), 0) * tangent;

        // Choose boat type
        int boatType = Random.Range(0, 6);
        GameObject boat = CreateBoat(boatType, spawnPos);

        if (boat != null)
        {
            // Face direction of travel
            boat.transform.rotation = Quaternion.LookRotation(tangent);

            activeBoats.Add(boat);
            boatDirections.Add(tangent);
            boatSpeeds.Add(Random.Range(1.5f, 4f));
            boatBaseY.Add(spawnPos.y);
        }
    }

    GameObject CreateBoat(int type, Vector3 position)
    {
        GameObject boat = new GameObject("Boat_" + type);
        boat.transform.position = position;

        switch (type)
        {
            case 0:
                CreateSailboat(boat);
                break;
            case 1:
                CreateYacht(boat);
                break;
            case 2:
                CreateFishingBoat(boat);
                break;
            case 3:
                CreateCargoShip(boat);
                break;
            case 4:
                CreateSpeedboat(boat);
                break;
            case 5:
                CreateRowboat(boat);
                break;
        }

        return boat;
    }

    void CreateSailboat(GameObject parent)
    {
        float scale = Random.Range(0.8f, 1.3f);

        // Hull - orange/brown wooden boat shape
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.transform.SetParent(parent.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localScale = new Vector3(1.2f * scale, 0.5f * scale, 3.5f * scale);
        hull.GetComponent<Renderer>().material = materials["orange_hull"];
        Object.Destroy(hull.GetComponent<Collider>());

        // Hull bottom (darker)
        GameObject hullBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hullBottom.transform.SetParent(parent.transform);
        hullBottom.transform.localPosition = new Vector3(0, -0.15f * scale, 0);
        hullBottom.transform.localScale = new Vector3(1.0f * scale, 0.25f * scale, 3.2f * scale);
        hullBottom.GetComponent<Renderer>().material = materials["wood_dark"];
        Object.Destroy(hullBottom.GetComponent<Collider>());

        // Small cabin with windows
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.transform.SetParent(parent.transform);
        cabin.transform.localPosition = new Vector3(0, 0.45f * scale, -0.3f * scale);
        cabin.transform.localScale = new Vector3(0.9f * scale, 0.4f * scale, 1.2f * scale);
        cabin.GetComponent<Renderer>().material = materials["orange_hull"];
        Object.Destroy(cabin.GetComponent<Collider>());

        // Cabin windows (dark strip)
        GameObject windows = GameObject.CreatePrimitive(PrimitiveType.Cube);
        windows.transform.SetParent(parent.transform);
        windows.transform.localPosition = new Vector3(0, 0.5f * scale, -0.3f * scale);
        windows.transform.localScale = new Vector3(0.92f * scale, 0.15f * scale, 1.22f * scale);
        windows.GetComponent<Renderer>().material = materials["black"];
        Object.Destroy(windows.GetComponent<Collider>());

        // Mast (tall, thin, gray)
        GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mast.transform.SetParent(parent.transform);
        mast.transform.localPosition = new Vector3(0, 2.5f * scale, 0.3f * scale);
        mast.transform.localScale = new Vector3(0.06f * scale, 2.5f * scale, 0.06f * scale);
        mast.GetComponent<Renderer>().material = materials["gray"];
        Object.Destroy(mast.GetComponent<Collider>());

        // Main sail (large triangular - simulated with rotated cube)
        GameObject mainSail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainSail.transform.SetParent(parent.transform);
        mainSail.transform.localPosition = new Vector3(0, 2.2f * scale, -0.4f * scale);
        mainSail.transform.localRotation = Quaternion.Euler(0, 0, -5);
        mainSail.transform.localScale = new Vector3(0.03f * scale, 3.5f * scale, 2.2f * scale);
        mainSail.GetComponent<Renderer>().material = materials["sail_cream"];
        Object.Destroy(mainSail.GetComponent<Collider>());

        // Jib sail (front triangular sail)
        GameObject jibSail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        jibSail.transform.SetParent(parent.transform);
        jibSail.transform.localPosition = new Vector3(0, 1.8f * scale, 1.2f * scale);
        jibSail.transform.localRotation = Quaternion.Euler(0, 0, 10);
        jibSail.transform.localScale = new Vector3(0.03f * scale, 2.5f * scale, 1.5f * scale);
        jibSail.GetComponent<Renderer>().material = materials["sail_cream"];
        Object.Destroy(jibSail.GetComponent<Collider>());

        // Back railing
        GameObject railing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        railing.transform.SetParent(parent.transform);
        railing.transform.localPosition = new Vector3(0, 0.5f * scale, -1.6f * scale);
        railing.transform.localScale = new Vector3(0.8f * scale, 0.4f * scale, 0.05f * scale);
        railing.GetComponent<Renderer>().material = materials["gray"];
        Object.Destroy(railing.GetComponent<Collider>());

        // Railing posts
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.transform.SetParent(parent.transform);
            post.transform.localPosition = new Vector3(side * 0.35f * scale, 0.35f * scale, -1.6f * scale);
            post.transform.localScale = new Vector3(0.04f * scale, 0.2f * scale, 0.04f * scale);
            post.GetComponent<Renderer>().material = materials["gray"];
            Object.Destroy(post.GetComponent<Collider>());
        }
    }

    void CreateYacht(GameObject parent)
    {
        float scale = Random.Range(1f, 1.5f);

        // Hull (sleek)
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hull.transform.SetParent(parent.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localRotation = Quaternion.Euler(0, 0, 90);
        hull.transform.localScale = new Vector3(1f * scale, 3f * scale, 1.2f * scale);
        hull.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(hull.GetComponent<Collider>());

        // Cabin
        GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabin.transform.SetParent(parent.transform);
        cabin.transform.localPosition = new Vector3(0, 0.8f * scale, -0.5f * scale);
        cabin.transform.localScale = new Vector3(1f * scale, 0.8f * scale, 2f * scale);
        cabin.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(cabin.GetComponent<Collider>());

        // Windows (blue stripe)
        GameObject windows = GameObject.CreatePrimitive(PrimitiveType.Cube);
        windows.transform.SetParent(parent.transform);
        windows.transform.localPosition = new Vector3(0, 0.9f * scale, -0.5f * scale);
        windows.transform.localScale = new Vector3(1.02f * scale, 0.2f * scale, 2.02f * scale);
        windows.GetComponent<Renderer>().material = materials["blue"];
        Object.Destroy(windows.GetComponent<Collider>());
    }

    void CreateFishingBoat(GameObject parent)
    {
        float scale = Random.Range(0.7f, 1.1f);

        // Hull - white top section
        GameObject hullTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hullTop.transform.SetParent(parent.transform);
        hullTop.transform.localPosition = new Vector3(0, 0.1f * scale, 0);
        hullTop.transform.localScale = new Vector3(1.4f * scale, 0.4f * scale, 3.2f * scale);
        hullTop.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(hullTop.GetComponent<Collider>());

        // Hull - dark blue bottom section
        GameObject hullBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hullBottom.transform.SetParent(parent.transform);
        hullBottom.transform.localPosition = new Vector3(0, -0.2f * scale, 0);
        hullBottom.transform.localScale = new Vector3(1.3f * scale, 0.35f * scale, 3.0f * scale);
        hullBottom.GetComponent<Renderer>().material = materials["navy_blue"];
        Object.Destroy(hullBottom.GetComponent<Collider>());

        // Center console
        GameObject console = GameObject.CreatePrimitive(PrimitiveType.Cube);
        console.transform.SetParent(parent.transform);
        console.transform.localPosition = new Vector3(0, 0.45f * scale, 0.2f * scale);
        console.transform.localScale = new Vector3(0.5f * scale, 0.5f * scale, 0.6f * scale);
        console.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(console.GetComponent<Collider>());

        // T-top frame (4 poles)
        for (int i = 0; i < 4; i++)
        {
            float xOff = (i % 2 == 0) ? -0.25f : 0.25f;
            float zOff = (i < 2) ? 0.4f : -0.1f;
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.transform.SetParent(parent.transform);
            pole.transform.localPosition = new Vector3(xOff * scale, 0.9f * scale, zOff * scale);
            pole.transform.localScale = new Vector3(0.04f * scale, 0.5f * scale, 0.04f * scale);
            pole.GetComponent<Renderer>().material = materials["gray"];
            Object.Destroy(pole.GetComponent<Collider>());
        }

        // T-top canopy (dark)
        GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        canopy.transform.SetParent(parent.transform);
        canopy.transform.localPosition = new Vector3(0, 1.4f * scale, 0.15f * scale);
        canopy.transform.localScale = new Vector3(0.9f * scale, 0.08f * scale, 0.9f * scale);
        canopy.GetComponent<Renderer>().material = materials["black"];
        Object.Destroy(canopy.GetComponent<Collider>());

        // Rod holders on top of canopy
        for (int i = 0; i < 4; i++)
        {
            GameObject rodHolder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rodHolder.transform.SetParent(parent.transform);
            rodHolder.transform.localPosition = new Vector3(-0.25f * scale + i * 0.17f * scale, 1.55f * scale, 0.3f * scale);
            rodHolder.transform.localRotation = Quaternion.Euler(-20, 0, 0);
            rodHolder.transform.localScale = new Vector3(0.04f * scale, 0.12f * scale, 0.04f * scale);
            rodHolder.GetComponent<Renderer>().material = materials["white"];
            Object.Destroy(rodHolder.GetComponent<Collider>());
        }

        // Outboard motor
        GameObject motor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        motor.transform.SetParent(parent.transform);
        motor.transform.localPosition = new Vector3(0, 0f * scale, -1.7f * scale);
        motor.transform.localScale = new Vector3(0.3f * scale, 0.6f * scale, 0.4f * scale);
        motor.GetComponent<Renderer>().material = materials["gray"];
        Object.Destroy(motor.GetComponent<Collider>());

        // Motor lower unit
        GameObject motorLower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        motorLower.transform.SetParent(parent.transform);
        motorLower.transform.localPosition = new Vector3(0, -0.4f * scale, -1.75f * scale);
        motorLower.transform.localScale = new Vector3(0.15f * scale, 0.5f * scale, 0.2f * scale);
        motorLower.GetComponent<Renderer>().material = materials["black"];
        Object.Destroy(motorLower.GetComponent<Collider>());

        // Front bow rail
        GameObject bowRail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bowRail.transform.SetParent(parent.transform);
        bowRail.transform.localPosition = new Vector3(0, 0.4f * scale, 1.4f * scale);
        bowRail.transform.localRotation = Quaternion.Euler(0, 0, 90);
        bowRail.transform.localScale = new Vector3(0.03f * scale, 0.5f * scale, 0.03f * scale);
        bowRail.GetComponent<Renderer>().material = materials["gray"];
        Object.Destroy(bowRail.GetComponent<Collider>());
    }

    void CreateCargoShip(GameObject parent)
    {
        float scale = Random.Range(1.5f, 2.5f);

        // Hull - dark blue main section
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.transform.SetParent(parent.transform);
        hull.transform.localPosition = new Vector3(0, 0.3f * scale, 0);
        hull.transform.localScale = new Vector3(2.8f * scale, 1.2f * scale, 12f * scale);
        hull.GetComponent<Renderer>().material = materials["navy_blue"];
        Object.Destroy(hull.GetComponent<Collider>());

        // Hull - red waterline (bottom)
        GameObject hullBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hullBottom.transform.SetParent(parent.transform);
        hullBottom.transform.localPosition = new Vector3(0, -0.4f * scale, 0);
        hullBottom.transform.localScale = new Vector3(2.5f * scale, 0.5f * scale, 11.5f * scale);
        hullBottom.GetComponent<Renderer>().material = materials["dark_red"];
        Object.Destroy(hullBottom.GetComponent<Collider>());

        // Stacked containers - multiple rows
        int containerRows = 3;  // front to back rows
        int containerCols = 5;  // left to right stacks
        int containerHeight = 3; // stacked height

        for (int row = 0; row < containerRows; row++)
        {
            for (int col = 0; col < containerCols; col++)
            {
                for (int h = 0; h < containerHeight; h++)
                {
                    // Skip some containers randomly for variety
                    if (h == containerHeight - 1 && Random.value > 0.6f) continue;

                    GameObject container = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    container.transform.SetParent(parent.transform);

                    float xPos = (-1f + col * 0.5f) * scale;
                    float yPos = (1.2f + h * 0.55f) * scale;
                    float zPos = (1f + row * 2.2f) * scale;

                    container.transform.localPosition = new Vector3(xPos, yPos, zPos);
                    container.transform.localScale = new Vector3(0.45f * scale, 0.5f * scale, 2f * scale);

                    // Mostly red/maroon containers like the reference
                    Material contMat = Random.value > 0.15f ? materials["container_red"] : materials["container_teal"];
                    container.GetComponent<Renderer>().material = contMat;
                    Object.Destroy(container.GetComponent<Collider>());
                }
            }
        }

        // Bridge/superstructure - cream/tan colored, at the back
        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.transform.SetParent(parent.transform);
        bridge.transform.localPosition = new Vector3(0, 1.8f * scale, -4.5f * scale);
        bridge.transform.localScale = new Vector3(2f * scale, 2f * scale, 1.5f * scale);
        bridge.GetComponent<Renderer>().material = materials["cream"];
        Object.Destroy(bridge.GetComponent<Collider>());

        // Bridge upper deck
        GameObject bridgeUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridgeUpper.transform.SetParent(parent.transform);
        bridgeUpper.transform.localPosition = new Vector3(0, 3f * scale, -4.5f * scale);
        bridgeUpper.transform.localScale = new Vector3(1.5f * scale, 0.8f * scale, 1.2f * scale);
        bridgeUpper.GetComponent<Renderer>().material = materials["cream"];
        Object.Destroy(bridgeUpper.GetComponent<Collider>());

        // Bridge windows
        GameObject bridgeWindows = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridgeWindows.transform.SetParent(parent.transform);
        bridgeWindows.transform.localPosition = new Vector3(0, 3.2f * scale, -4.0f * scale);
        bridgeWindows.transform.localScale = new Vector3(1.4f * scale, 0.4f * scale, 0.1f * scale);
        bridgeWindows.GetComponent<Renderer>().material = materials["black"];
        Object.Destroy(bridgeWindows.GetComponent<Collider>());

        // Smoke stack
        GameObject stack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stack.transform.SetParent(parent.transform);
        stack.transform.localPosition = new Vector3(0, 3.5f * scale, -4.8f * scale);
        stack.transform.localScale = new Vector3(0.3f * scale, 0.6f * scale, 0.3f * scale);
        stack.GetComponent<Renderer>().material = materials["cream"];
        Object.Destroy(stack.GetComponent<Collider>());

        // Antenna/mast
        GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.transform.SetParent(parent.transform);
        antenna.transform.localPosition = new Vector3(0, 4f * scale, -4.5f * scale);
        antenna.transform.localScale = new Vector3(0.05f * scale, 0.5f * scale, 0.05f * scale);
        antenna.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(antenna.GetComponent<Collider>());
    }

    void CreateSpeedboat(GameObject parent)
    {
        float scale = Random.Range(0.6f, 0.9f);

        // Hull (sleek, low)
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.transform.SetParent(parent.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localScale = new Vector3(1f * scale, 0.4f * scale, 2.5f * scale);
        hull.GetComponent<Renderer>().material = Random.value > 0.5f ? materials["red"] : materials["white"];
        Object.Destroy(hull.GetComponent<Collider>());

        // Windshield
        GameObject windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
        windshield.transform.SetParent(parent.transform);
        windshield.transform.localPosition = new Vector3(0, 0.4f * scale, 0.2f * scale);
        windshield.transform.localRotation = Quaternion.Euler(-30, 0, 0);
        windshield.transform.localScale = new Vector3(0.8f * scale, 0.4f * scale, 0.1f * scale);
        windshield.GetComponent<Renderer>().material = materials["blue"];
        Object.Destroy(windshield.GetComponent<Collider>());
    }

    void CreateRowboat(GameObject parent)
    {
        float scale = Random.Range(0.4f, 0.6f);

        // Hull (small)
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hull.transform.SetParent(parent.transform);
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localRotation = Quaternion.Euler(0, 0, 90);
        hull.transform.localScale = new Vector3(0.4f * scale, 1.2f * scale, 0.5f * scale);
        hull.GetComponent<Renderer>().material = materials["wood_light"];
        Object.Destroy(hull.GetComponent<Collider>());

        // Oars
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject oar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            oar.transform.SetParent(parent.transform);
            oar.transform.localPosition = new Vector3(side * 0.6f * scale, 0.2f * scale, 0);
            oar.transform.localRotation = Quaternion.Euler(0, 0, side * 45);
            oar.transform.localScale = new Vector3(0.03f * scale, 0.8f * scale, 0.03f * scale);
            oar.GetComponent<Renderer>().material = materials["wood_dark"];
            Object.Destroy(oar.GetComponent<Collider>());
        }
    }

    void SpawnPlane()
    {
        // Spawn from edge of sky
        bool fromLeft = Random.value > 0.5f;
        float startX = fromLeft ? -250f : 250f;
        float startZ = Random.Range(-100f, 100f);
        float height = Random.Range(40f, 70f);

        Vector3 spawnPos = new Vector3(startX, height, startZ);
        Vector3 direction = new Vector3(fromLeft ? 1f : -1f, 0, Random.Range(-0.2f, 0.2f)).normalized;

        GameObject plane = CreatePlaneWithBanner(spawnPos, direction);

        if (plane != null)
        {
            activePlanes.Add(plane);
            planeDirections.Add(direction);
            planeSpeeds.Add(Random.Range(15f, 25f));
        }
    }

    GameObject CreatePlaneWithBanner(Vector3 position, Vector3 direction)
    {
        GameObject planeParent = new GameObject("AdvertPlane");
        planeParent.transform.position = position;
        planeParent.transform.rotation = Quaternion.LookRotation(direction);

        // Plane body (fuselage) - small prop plane
        GameObject fuselage = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        fuselage.transform.SetParent(planeParent.transform);
        fuselage.transform.localPosition = Vector3.zero;
        fuselage.transform.localRotation = Quaternion.Euler(0, 0, 90);
        fuselage.transform.localScale = new Vector3(0.8f, 1.8f, 0.8f);
        fuselage.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(fuselage.GetComponent<Collider>());

        // Nose cone
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.transform.SetParent(planeParent.transform);
        nose.transform.localPosition = new Vector3(0, 0, 1.6f);
        nose.transform.localScale = new Vector3(0.5f, 0.5f, 0.4f);
        nose.GetComponent<Renderer>().material = materials["black"];
        Object.Destroy(nose.GetComponent<Collider>());

        // Wings (wide)
        GameObject wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wings.transform.SetParent(planeParent.transform);
        wings.transform.localPosition = new Vector3(0, 0, 0.3f);
        wings.transform.localScale = new Vector3(7f, 0.12f, 1.2f);
        wings.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(wings.GetComponent<Collider>());

        // Twin engines on wings
        for (int side = -1; side <= 1; side += 2)
        {
            // Engine nacelle
            GameObject engine = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            engine.transform.SetParent(planeParent.transform);
            engine.transform.localPosition = new Vector3(side * 1.8f, -0.15f, 0.5f);
            engine.transform.localRotation = Quaternion.Euler(0, 0, 90);
            engine.transform.localScale = new Vector3(0.35f, 0.6f, 0.35f);
            engine.GetComponent<Renderer>().material = materials["gray"];
            Object.Destroy(engine.GetComponent<Collider>());

            // Propeller disc
            GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            prop.transform.SetParent(planeParent.transform);
            prop.transform.localPosition = new Vector3(side * 1.8f, -0.15f, 1.0f);
            prop.transform.localRotation = Quaternion.Euler(90, 0, 0);
            prop.transform.localScale = new Vector3(0.6f, 0.02f, 0.6f);
            prop.GetComponent<Renderer>().material = materials["black"];
            Object.Destroy(prop.GetComponent<Collider>());
        }

        // Tail fin (vertical) - smaller
        GameObject tailFin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tailFin.transform.SetParent(planeParent.transform);
        tailFin.transform.localPosition = new Vector3(0, 0.6f, -1.5f);
        tailFin.transform.localScale = new Vector3(0.08f, 1f, 0.7f);
        tailFin.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(tailFin.GetComponent<Collider>());

        // Tail wings (horizontal)
        GameObject tailWings = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tailWings.transform.SetParent(planeParent.transform);
        tailWings.transform.localPosition = new Vector3(0, 0.1f, -1.5f);
        tailWings.transform.localScale = new Vector3(2.5f, 0.08f, 0.6f);
        tailWings.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(tailWings.GetComponent<Collider>());

        // Landing gear (small, retracted look)
        GameObject gear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gear.transform.SetParent(planeParent.transform);
        gear.transform.localPosition = new Vector3(0, -0.5f, 0.5f);
        gear.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        gear.GetComponent<Renderer>().material = materials["black"];
        Object.Destroy(gear.GetComponent<Collider>());

        // Tow rope (thin line from tail)
        GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.transform.SetParent(planeParent.transform);
        rope.transform.localPosition = new Vector3(0, -0.3f, -5f);
        rope.transform.localRotation = Quaternion.Euler(90, 0, 0);
        rope.transform.localScale = new Vector3(0.03f, 3f, 0.03f);
        rope.GetComponent<Renderer>().material = materials["gray"];
        Object.Destroy(rope.GetComponent<Collider>());

        // Banner - ribbon style with forked tail
        CreateRibbonBanner(planeParent);

        return planeParent;
    }

    void CreateRibbonBanner(GameObject planeParent)
    {
        // Main banner body - long white ribbon
        GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        banner.name = "Banner";
        banner.transform.SetParent(planeParent.transform);
        banner.transform.localPosition = new Vector3(0, -0.5f, -14f);
        banner.transform.localScale = new Vector3(15f, 2.5f, 0.08f);
        banner.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(banner.GetComponent<Collider>());

        // Forked tail - left ribbon
        GameObject forkLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        forkLeft.transform.SetParent(planeParent.transform);
        forkLeft.transform.localPosition = new Vector3(-2f, -0.8f, -22f);
        forkLeft.transform.localRotation = Quaternion.Euler(0, -15, 0);
        forkLeft.transform.localScale = new Vector3(4f, 1.5f, 0.06f);
        forkLeft.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(forkLeft.GetComponent<Collider>());

        // Forked tail - right ribbon
        GameObject forkRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        forkRight.transform.SetParent(planeParent.transform);
        forkRight.transform.localPosition = new Vector3(2f, -0.8f, -22f);
        forkRight.transform.localRotation = Quaternion.Euler(0, 15, 0);
        forkRight.transform.localScale = new Vector3(4f, 1.5f, 0.06f);
        forkRight.GetComponent<Renderer>().material = materials["white"];
        Object.Destroy(forkRight.GetComponent<Collider>());

        // Add banner sway animation component
        BannerSway sway = banner.AddComponent<BannerSway>();
    }

    void SpawnCloud(bool randomPosition)
    {
        float spawnX, spawnZ;

        if (randomPosition)
        {
            // Random position for initial spawn
            spawnX = Random.Range(-150f, 150f);
            spawnZ = Random.Range(-150f, 150f);
        }
        else
        {
            // Spawn from edge
            bool fromX = Random.value > 0.5f;
            if (fromX)
            {
                spawnX = Random.value > 0.5f ? -180f : 180f;
                spawnZ = Random.Range(-100f, 100f);
            }
            else
            {
                spawnX = Random.Range(-100f, 100f);
                spawnZ = Random.value > 0.5f ? -180f : 180f;
            }
        }

        float height = Random.Range(25f, 50f);
        Vector3 spawnPos = new Vector3(spawnX, height, spawnZ);

        // Direction - generally toward opposite side with variation
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        if (direction.magnitude < 0.1f) direction = Vector3.right;

        GameObject cloud = CreateCloud(spawnPos);

        if (cloud != null)
        {
            activeClouds.Add(cloud);
            cloudDirections.Add(direction);
            cloudSpeeds.Add(Random.Range(1f, 3f));
        }
    }

    GameObject CreateCloud(Vector3 position)
    {
        GameObject cloudParent = new GameObject("Cloud");
        cloudParent.transform.position = position;

        // Create fluffy cloud from multiple spheres
        int numPuffs = Random.Range(4, 8);
        float cloudScale = Random.Range(0.7f, 1.5f);

        for (int i = 0; i < numPuffs; i++)
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.transform.SetParent(cloudParent.transform);

            float xOff = Random.Range(-4f, 4f) * cloudScale;
            float yOff = Random.Range(-0.5f, 0.5f) * cloudScale;
            float zOff = Random.Range(-2f, 2f) * cloudScale;
            puff.transform.localPosition = new Vector3(xOff, yOff, zOff);

            float puffScale = Random.Range(2f, 4f) * cloudScale;
            puff.transform.localScale = new Vector3(puffScale, puffScale * 0.6f, puffScale);

            puff.GetComponent<Renderer>().material = materials["cloud"];
            Object.Destroy(puff.GetComponent<Collider>());
        }

        return cloudParent;
    }

    void OnDestroy()
    {
        // Clean up all objects
        foreach (var boat in activeBoats) if (boat != null) Destroy(boat);
        foreach (var plane in activePlanes) if (plane != null) Destroy(plane);
        foreach (var cloud in activeClouds) if (cloud != null) Destroy(cloud);

        activeBoats.Clear();
        activePlanes.Clear();
        activeClouds.Clear();

        // Clean up materials
        foreach (var mat in materials.Values) if (mat != null) Destroy(mat);
        materials.Clear();
    }
}

/// <summary>
/// Makes the banner sway while being towed
/// </summary>
public class BannerSway : MonoBehaviour
{
    private float swayOffset;

    void Start()
    {
        swayOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Gentle sway animation
        float sway = Mathf.Sin(Time.time * 2f + swayOffset) * 5f;
        float wave = Mathf.Sin(Time.time * 3f + swayOffset) * 2f;

        transform.localRotation = Quaternion.Euler(sway, wave, sway * 0.5f);
    }
}
