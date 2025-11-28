using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AutoSetup
{
    static AutoSetup()
    {
        EditorApplication.delayCall += RunSetup;
    }

    [MenuItem("Fishing Game/Setup Scene Now")]
    static void RunSetup()
    {
        // Delete all existing game objects
        CleanupScene();

        Debug.Log("=== SETTING UP FISHING GAME WITH REALISTIC VISUALS ===");

        // Setup lighting first
        SetupLighting();

        // Ground with grass texture (behind the dock area)
        CreateGround();

        // Water - positioned so half the dock is over water
        CreateWater();

        // Wooden dock/boardwalk extending into the water
        CreateDock();

        // Ramp from grass to dock
        CreateRamp();

        // Player with proper human proportions
        CreatePlayer();

        // Spawn NPC - greets the naked player
        CreateSpawnNPC();

        // IMPORTANT: Create MainMenu FIRST so other systems can check GameStarted
        GameObject mainMenu = new GameObject("MainMenu");
        mainMenu.AddComponent<MainMenu>();

        // Game Systems
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        GameObject fs = new GameObject("FishingSystem");
        fs.AddComponent<FishingSystem>();

        // Leveling System (OSRS-style, level 1-399, 100M XP cap)
        GameObject ls = new GameObject("LevelingSystem");
        ls.AddComponent<LevelingSystem>();

        // Quest System
        GameObject qs = new GameObject("QuestSystem");
        qs.AddComponent<QuestSystem>();

        // Bottle Event System (1/100 chance per cast)
        GameObject bes = new GameObject("BottleEventSystem");
        bes.AddComponent<BottleEventSystem>();

        // UI System
        GameObject ui = new GameObject("UIManager");
        ui.AddComponent<UIManager>();

        // Character Panel (Tab inventory)
        GameObject charPanel = new GameObject("CharacterPanel");
        charPanel.AddComponent<CharacterPanel>();

        // Fish Diary (J to toggle)
        GameObject fishDiary = new GameObject("FishDiary");
        fishDiary.AddComponent<FishDiary>();

        // Developer Panel (F12 to toggle - for testing)
        GameObject devPanel = new GameObject("DevPanel");
        devPanel.AddComponent<DevPanel>();

        // Player Health System (HP, hunger, death)
        GameObject playerHealth = new GameObject("PlayerHealth");
        playerHealth.AddComponent<PlayerHealth>();

        // Food Inventory System (fish storage, cooking, hotbar)
        GameObject foodInventory = new GameObject("FoodInventory");
        foodInventory.AddComponent<FoodInventory>();

        // BBQ Station at end of dock
        CreateBBQ();

        // Quest NPC on the dock
        CreateQuestNPC();

        // Camera setup
        SetupCamera();

        // Create realistic trees in background
        CreateTreesAroundScene();

        // Create 4 mystical portals on the beach
        CreatePortals();

        // Create clothing shop island with Granny
        CreateClothingShopIsland();

        // Horizon boats system
        GameObject boats = new GameObject("HorizonBoats");
        boats.AddComponent<HorizonBoats>();

        // Bird flock system
        GameObject birds = new GameObject("BirdFlock");
        birds.AddComponent<BirdFlock>();

        // Candy the cat - calico cat that patrols the island
        GameObject candy = new GameObject("CandyCat");
        candy.AddComponent<CandyCat>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("=== FISHING GAME READY! Press PLAY, WASD to move, HOLD LEFT CLICK to charge cast! ===");
    }

    static void CleanupScene()
    {
        string[] toDelete = { "Player", "Ground", "Water", "WaterBed", "Dock", "Ramp", "GameManager", "FishingSystem", "UIManager", "Sun", "TreesParent", "LevelingSystem", "QuestSystem", "BottleEventSystem", "QuestNPC", "PortalsParent", "CharacterPanel", "DevPanel", "MainMenu", "ClothingShopIsland", "HorizonBoats", "BirdFlock", "PlayerHealth", "FoodInventory", "BBQStation", "DockRadio", "ShoulderParrot" };
        foreach (string name in toDelete)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null) Object.DestroyImmediate(obj);
        }

        // Delete any stray objects
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj != null && (obj.name.StartsWith("Tree") || obj.name == "DockPost" || obj.name == "Plank" ||
                obj.name.StartsWith("Ramp") || obj.name.StartsWith("Branch")))
                Object.DestroyImmediate(obj);
        }
    }

    static void SetupLighting()
    {
        // Remove existing directional lights
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional)
                Object.DestroyImmediate(l.gameObject);
        }

        // Main sun - warm afternoon light
        GameObject sun = new GameObject("Sun");
        Light sunLight = sun.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.intensity = 1.3f;
        sunLight.color = new Color(1f, 0.95f, 0.85f);
        sunLight.shadows = LightShadows.Soft;
        sunLight.shadowStrength = 0.5f;
        sun.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Ambient lighting - natural outdoor feel
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.7f, 0.9f);
        RenderSettings.ambientEquatorColor = new Color(0.45f, 0.55f, 0.65f);
        RenderSettings.ambientGroundColor = new Color(0.25f, 0.3f, 0.2f);

        // Subtle fog for depth
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.65f, 0.75f, 0.85f);
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance = 100f;
    }

    static void CreateGround()
    {
        // LAYERED TERRAIN with gradients - grass > sand > water slope
        GameObject ground = new GameObject("Ground");
        ground.transform.position = Vector3.zero;

        float groundY = 1.0f; // Ground level

        // === GRASS LAYERS (center island) - multiple shades for gradient ===
        // Core grass (darkest/richest green)
        Material grassCoreMat = new Material(Shader.Find("Standard"));
        grassCoreMat.color = new Color(0.22f, 0.48f, 0.15f); // Deep lush green

        GameObject grassCore = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grassCore.name = "GrassCore";
        grassCore.transform.SetParent(ground.transform);
        grassCore.transform.localPosition = new Vector3(0, groundY + 0.02f, -9.2f);
        grassCore.transform.localScale = new Vector3(60, 0.5f, 62f);
        grassCore.GetComponent<Renderer>().sharedMaterial = grassCoreMat;

        // Mid grass (medium green)
        Material grassMidMat = new Material(Shader.Find("Standard"));
        grassMidMat.color = new Color(0.32f, 0.55f, 0.22f); // Medium green

        GameObject grassMid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grassMid.name = "GrassMid";
        grassMid.transform.SetParent(ground.transform);
        grassMid.transform.localPosition = new Vector3(0, groundY + 0.01f, -9.2f);
        grassMid.transform.localScale = new Vector3(70, 0.5f, 72f);
        grassMid.GetComponent<Renderer>().sharedMaterial = grassMidMat;

        // Outer grass (lighter, transitioning to sand)
        Material grassOuterMat = new Material(Shader.Find("Standard"));
        grassOuterMat.color = new Color(0.42f, 0.58f, 0.30f); // Light green with yellow tint

        GameObject grassOuter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grassOuter.name = "GrassOuter";
        grassOuter.transform.SetParent(ground.transform);
        grassOuter.transform.localPosition = new Vector3(0, groundY, -9.2f);
        grassOuter.transform.localScale = new Vector3(76, 0.5f, 78f);
        grassOuter.GetComponent<Renderer>().sharedMaterial = grassOuterMat;

        // Main walkable floor (base grass)
        Material grassMat = new Material(Shader.Find("Standard"));
        grassMat.color = new Color(0.35f, 0.52f, 0.25f);

        GameObject mainFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainFloor.name = "MainFloor";
        mainFloor.transform.SetParent(ground.transform);
        mainFloor.transform.localPosition = new Vector3(0, groundY - 0.01f, -9.2f);
        mainFloor.transform.localScale = new Vector3(80, 0.5f, 81.88f);
        mainFloor.GetComponent<Renderer>().sharedMaterial = grassMat;

        // === SAND/BEACH GRADIENT LAYERS ===
        // Inner sand (grass-sand transition - greenish sand)
        Material sandInnerMat = new Material(Shader.Find("Standard"));
        sandInnerMat.color = new Color(0.72f, 0.68f, 0.45f); // Yellow-green sand

        GameObject sandInner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sandInner.name = "SandInner";
        sandInner.transform.SetParent(ground.transform);
        sandInner.transform.localPosition = new Vector3(0, groundY - 0.08f, -9.2f);
        sandInner.transform.localScale = new Vector3(85, 0.5f, 87f);
        sandInner.GetComponent<Renderer>().sharedMaterial = sandInnerMat;

        // Mid sand (typical beach color)
        Material sandMidMat = new Material(Shader.Find("Standard"));
        sandMidMat.color = new Color(0.88f, 0.78f, 0.55f); // Classic sand

        GameObject sandMid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sandMid.name = "SandMid";
        sandMid.transform.SetParent(ground.transform);
        sandMid.transform.localPosition = new Vector3(0, groundY - 0.15f, -9.2f);
        sandMid.transform.localScale = new Vector3(92, 0.5f, 94f);
        sandMid.GetComponent<Renderer>().sharedMaterial = sandMidMat;

        // Outer sand (wet sand near water - darker)
        Material sandWetMat = new Material(Shader.Find("Standard"));
        sandWetMat.color = new Color(0.65f, 0.55f, 0.38f); // Wet dark sand
        sandWetMat.SetFloat("_Glossiness", 0.4f); // Slightly shiny wet look

        GameObject sandWet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sandWet.name = "SandWet";
        sandWet.transform.SetParent(ground.transform);
        sandWet.transform.localPosition = new Vector3(0, groundY - 0.25f, -9.2f);
        sandWet.transform.localScale = new Vector3(100, 0.5f, 102f);
        sandWet.GetComponent<Renderer>().sharedMaterial = sandWetMat;

        // Add grass tufts for decoration
        AddProceduralGrass(ground.transform, new Vector3(0, groundY + 0.25f, -10), 25f, 100);
        AddProceduralGrass(ground.transform, new Vector3(-15, groundY + 0.25f, 5), 10f, 40);
        AddProceduralGrass(ground.transform, new Vector3(15, groundY + 0.25f, 5), 10f, 40);

        // Add random BUSHES in the center of the island
        AddBushes(ground.transform, groundY);
    }

    static void AddBushes(Transform parent, float groundY)
    {
        // Various bush materials
        Material darkGreenMat = new Material(Shader.Find("Standard"));
        darkGreenMat.color = new Color(0.18f, 0.42f, 0.15f);

        Material lightGreenMat = new Material(Shader.Find("Standard"));
        lightGreenMat.color = new Color(0.22f, 0.48f, 0.18f);

        Material oliveGreenMat = new Material(Shader.Find("Standard"));
        oliveGreenMat.color = new Color(0.35f, 0.45f, 0.20f);

        Material blueGreenMat = new Material(Shader.Find("Standard"));
        blueGreenMat.color = new Color(0.15f, 0.38f, 0.32f);

        // Flowering bush materials
        Material pinkFlowerMat = new Material(Shader.Find("Standard"));
        pinkFlowerMat.color = new Color(0.95f, 0.55f, 0.70f);

        Material yellowFlowerMat = new Material(Shader.Find("Standard"));
        yellowFlowerMat.color = new Color(0.95f, 0.85f, 0.30f);

        Material whiteFlowerMat = new Material(Shader.Find("Standard"));
        whiteFlowerMat.color = new Color(0.95f, 0.95f, 0.90f);

        Material purpleFlowerMat = new Material(Shader.Find("Standard"));
        purpleFlowerMat.color = new Color(0.70f, 0.40f, 0.85f);

        Material[] greenMats = { darkGreenMat, lightGreenMat, oliveGreenMat, blueGreenMat };
        Material[] flowerMats = { pinkFlowerMat, yellowFlowerMat, whiteFlowerMat, purpleFlowerMat };

        // More bush positions spread across island
        Vector3[] bushPositions = {
            new Vector3(-8f, groundY + 0.4f, -15f),
            new Vector3(5f, groundY + 0.4f, -12f),
            new Vector3(-12f, groundY + 0.4f, -5f),
            new Vector3(10f, groundY + 0.4f, -8f),
            new Vector3(-5f, groundY + 0.4f, -20f),
            new Vector3(15f, groundY + 0.4f, -18f),
            new Vector3(-18f, groundY + 0.4f, -12f),
            new Vector3(0f, groundY + 0.4f, -25f),
            new Vector3(8f, groundY + 0.4f, 0f),
            new Vector3(-10f, groundY + 0.4f, 2f),
            new Vector3(20f, groundY + 0.4f, -5f),
            new Vector3(-22f, groundY + 0.4f, -20f),
            new Vector3(12f, groundY + 0.4f, -22f),
            new Vector3(-15f, groundY + 0.4f, -25f),
            new Vector3(18f, groundY + 0.4f, 5f),
            new Vector3(-6f, groundY + 0.4f, -8f),
            new Vector3(3f, groundY + 0.4f, -18f),
            new Vector3(-20f, groundY + 0.4f, 0f),
        };

        // Bush types: 0=round, 1=tall/columnar, 2=wide/spreading, 3=flowering, 4=topiary/shaped
        for (int i = 0; i < bushPositions.Length; i++)
        {
            int bushType = Random.Range(0, 5);
            Material greenMat = greenMats[Random.Range(0, greenMats.Length)];

            GameObject bush = new GameObject("Bush_" + bushType);
            bush.transform.SetParent(parent);
            bush.transform.position = bushPositions[i];

            switch (bushType)
            {
                case 0: // Round bush - classic spherical shape
                    CreateRoundBush(bush.transform, greenMat, Random.Range(0.8f, 1.6f));
                    break;
                case 1: // Tall/columnar bush - like cypress
                    CreateTallBush(bush.transform, greenMat, Random.Range(1.2f, 2.0f));
                    break;
                case 2: // Wide/spreading bush - low and wide
                    CreateWideBush(bush.transform, greenMat, Random.Range(1.0f, 1.8f));
                    break;
                case 3: // Flowering bush - green with colorful flowers
                    Material flowerMat = flowerMats[Random.Range(0, flowerMats.Length)];
                    CreateFloweringBush(bush.transform, greenMat, flowerMat, Random.Range(0.9f, 1.4f));
                    break;
                case 4: // Topiary/shaped bush - geometric shapes
                    CreateTopiaryBush(bush.transform, greenMat, Random.Range(0.8f, 1.3f));
                    break;
            }
        }
    }

    static void CreateRoundBush(Transform parent, Material mat, float size)
    {
        int numParts = Random.Range(4, 7);
        for (int j = 0; j < numParts; j++)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part.name = "BushPart";
            part.transform.SetParent(parent);
            part.transform.localPosition = new Vector3(
                Random.Range(-0.3f, 0.3f) * size,
                Random.Range(0f, 0.4f) * size,
                Random.Range(-0.3f, 0.3f) * size
            );
            float partSize = Random.Range(0.4f, 0.7f) * size;
            part.transform.localScale = new Vector3(partSize, partSize * 0.85f, partSize);
            part.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(part.GetComponent<Collider>());
        }
    }

    static void CreateTallBush(Transform parent, Material mat, float height)
    {
        // Tall columnar bush like a small cypress
        float width = height * 0.35f;

        // Main tall body
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        main.name = "TallBushMain";
        main.transform.SetParent(parent);
        main.transform.localPosition = new Vector3(0, height * 0.5f, 0);
        main.transform.localScale = new Vector3(width, height * 0.5f, width);
        main.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(main.GetComponent<Collider>());

        // Top pointed section
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        top.name = "TallBushTop";
        top.transform.SetParent(parent);
        top.transform.localPosition = new Vector3(0, height * 0.9f, 0);
        top.transform.localScale = new Vector3(width * 0.7f, height * 0.25f, width * 0.7f);
        top.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(top.GetComponent<Collider>());

        // Small bulges around sides
        for (int i = 0; i < 4; i++)
        {
            float angle = (90f * i) * Mathf.Deg2Rad;
            GameObject bulge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulge.name = "TallBushBulge";
            bulge.transform.SetParent(parent);
            bulge.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * width * 0.4f,
                height * Random.Range(0.3f, 0.7f),
                Mathf.Cos(angle) * width * 0.4f
            );
            float bulgeSize = width * Random.Range(0.3f, 0.5f);
            bulge.transform.localScale = new Vector3(bulgeSize, bulgeSize * 1.2f, bulgeSize);
            bulge.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(bulge.GetComponent<Collider>());
        }
    }

    static void CreateWideBush(Transform parent, Material mat, float width)
    {
        // Wide spreading bush, low to ground
        float height = width * 0.4f;

        // Main wide body
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        main.name = "WideBushMain";
        main.transform.SetParent(parent);
        main.transform.localPosition = new Vector3(0, height * 0.5f, 0);
        main.transform.localScale = new Vector3(width, height, width);
        main.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(main.GetComponent<Collider>());

        // Spreading side clusters
        for (int i = 0; i < 6; i++)
        {
            float angle = (60f * i + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            float dist = width * Random.Range(0.35f, 0.55f);
            GameObject cluster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cluster.name = "WideBushCluster";
            cluster.transform.SetParent(parent);
            cluster.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * dist,
                height * Random.Range(0.2f, 0.5f),
                Mathf.Cos(angle) * dist
            );
            float clusterSize = width * Random.Range(0.25f, 0.4f);
            cluster.transform.localScale = new Vector3(clusterSize, clusterSize * 0.7f, clusterSize);
            cluster.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(cluster.GetComponent<Collider>());
        }
    }

    static void CreateFloweringBush(Transform parent, Material greenMat, Material flowerMat, float size)
    {
        // Base green bush
        CreateRoundBush(parent, greenMat, size);

        // Add colorful flower clusters on top
        int numFlowers = Random.Range(8, 15);
        for (int i = 0; i < numFlowers; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(0f, size * 0.4f);
            GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flower.name = "Flower";
            flower.transform.SetParent(parent);
            flower.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * dist,
                size * Random.Range(0.3f, 0.6f),
                Mathf.Cos(angle) * dist
            );
            float flowerSize = Random.Range(0.08f, 0.15f);
            flower.transform.localScale = new Vector3(flowerSize, flowerSize * 0.6f, flowerSize);
            flower.GetComponent<Renderer>().sharedMaterial = flowerMat;
            Object.DestroyImmediate(flower.GetComponent<Collider>());
        }
    }

    static void CreateTopiaryBush(Transform parent, Material mat, float size)
    {
        // Geometric shaped bushes - random shape selection
        int shape = Random.Range(0, 3);

        switch (shape)
        {
            case 0: // Sphere on stem (lollipop)
                GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.name = "TopiaryStem";
                stem.transform.SetParent(parent);
                stem.transform.localPosition = new Vector3(0, size * 0.3f, 0);
                stem.transform.localScale = new Vector3(0.08f, size * 0.3f, 0.08f);
                stem.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(stem.GetComponent<Collider>());

                GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.name = "TopiaryBall";
                ball.transform.SetParent(parent);
                ball.transform.localPosition = new Vector3(0, size * 0.8f, 0);
                ball.transform.localScale = new Vector3(size * 0.7f, size * 0.7f, size * 0.7f);
                ball.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(ball.GetComponent<Collider>());
                break;

            case 1: // Cone/pyramid shape
                GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cone.name = "TopiaryCone";
                cone.transform.SetParent(parent);
                cone.transform.localPosition = new Vector3(0, size * 0.5f, 0);
                cone.transform.localScale = new Vector3(size * 0.8f, size * 1.2f, size * 0.8f);
                cone.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(cone.GetComponent<Collider>());

                GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tip.name = "TopiaryConeTop";
                tip.transform.SetParent(parent);
                tip.transform.localPosition = new Vector3(0, size * 1.1f, 0);
                tip.transform.localScale = new Vector3(size * 0.3f, size * 0.4f, size * 0.3f);
                tip.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(tip.GetComponent<Collider>());
                break;

            case 2: // Tiered/stacked balls
                for (int tier = 0; tier < 3; tier++)
                {
                    float tierSize = size * (1f - tier * 0.25f);
                    float tierY = tier * size * 0.4f + tierSize * 0.4f;
                    GameObject tierBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tierBall.name = "TopiaryTier" + tier;
                    tierBall.transform.SetParent(parent);
                    tierBall.transform.localPosition = new Vector3(0, tierY, 0);
                    tierBall.transform.localScale = new Vector3(tierSize * 0.6f, tierSize * 0.5f, tierSize * 0.6f);
                    tierBall.GetComponent<Renderer>().sharedMaterial = mat;
                    Object.DestroyImmediate(tierBall.GetComponent<Collider>());
                }
                break;
        }
    }

    static void CreateBridgeToShop()
    {
        // Bridge removed - simplified terrain
    }

    // Scattered islands removed - using unified floor instead

    static void AddProceduralGrass(Transform parent, Vector3 center, float radius, int count)
    {
        Material grassBladeMat = new Material(Shader.Find("Standard"));
        grassBladeMat.color = new Color(0.22f, 0.48f, 0.12f);

        Material grassBladeMat2 = new Material(Shader.Find("Standard"));
        grassBladeMat2.color = new Color(0.28f, 0.55f, 0.18f);

        for (int i = 0; i < count; i++)
        {
            // Random position within radius
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(0f, radius);
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);

            // Create grass tuft (3-5 blades)
            int bladeCount = Random.Range(3, 5);
            for (int j = 0; j < bladeCount; j++)
            {
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "GrassBlade";
                blade.transform.SetParent(parent);

                float offsetX = Random.Range(-0.08f, 0.08f);
                float offsetZ = Random.Range(-0.08f, 0.08f);
                blade.transform.localPosition = pos + new Vector3(offsetX, 0.12f, offsetZ);

                float height = Random.Range(0.15f, 0.3f);
                float width = Random.Range(0.02f, 0.04f);
                blade.transform.localScale = new Vector3(width, height, width);

                // Slight random tilt
                blade.transform.localRotation = Quaternion.Euler(
                    Random.Range(-10f, 10f),
                    Random.Range(0f, 360f),
                    Random.Range(-10f, 10f)
                );

                blade.GetComponent<Renderer>().sharedMaterial = Random.value > 0.5f ? grassBladeMat : grassBladeMat2;
                Object.DestroyImmediate(blade.GetComponent<Collider>());
            }
        }
    }

    static void CreateSimplePalmTree(Transform parent, Vector3 localPos)
    {
        GameObject tree = new GameObject("SmallPalm");
        tree.transform.SetParent(parent);
        tree.transform.localPosition = localPos;

        Material trunkMat = new Material(Shader.Find("Standard"));
        trunkMat.color = new Color(0.45f, 0.35f, 0.2f);

        Material leafMat = new Material(Shader.Find("Standard"));
        leafMat.color = new Color(0.18f, 0.45f, 0.12f);

        // Simple trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, 1.5f, 0);
        trunk.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
        trunk.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;
        Object.DestroyImmediate(trunk.GetComponent<Collider>());

        // Simple leaves (4 fronds)
        for (int i = 0; i < 4; i++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf.transform.SetParent(tree.transform);
            float angle = i * 90f;
            leaf.transform.localPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 0.8f,
                2.8f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 0.8f
            );
            leaf.transform.localScale = new Vector3(0.15f, 0.05f, 1.2f);
            leaf.transform.localRotation = Quaternion.Euler(-30, angle, 0);
            leaf.GetComponent<Renderer>().sharedMaterial = leafMat;
            Object.DestroyImmediate(leaf.GetComponent<Collider>());
        }
    }

    static void CreateRock(Transform parent, Vector3 localPos)
    {
        Material rockMat = new Material(Shader.Find("Standard"));
        rockMat.color = new Color(0.45f, 0.42f, 0.38f);

        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";
        rock.transform.SetParent(parent);
        rock.transform.localPosition = localPos;
        float size = Random.Range(0.4f, 0.9f);
        rock.transform.localScale = new Vector3(size, size * 0.5f, size * 0.7f);
        rock.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
        rock.GetComponent<Renderer>().sharedMaterial = rockMat;
        Object.DestroyImmediate(rock.GetComponent<Collider>());
    }

    static void CreateWater()
    {
        // WATER SURROUNDS THE ENTIRE ISLAND
        // Water level is below ground level (ground is at y=1.0, water at y=0.5)
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Water";
        water.transform.position = new Vector3(0, 0.5f, 0);  // Water level below island
        water.transform.localScale = new Vector3(50, 1, 50); // HUGE water plane surrounding island
        water.AddComponent<WaterEffect>();

        // Water bed (sandy bottom visible through clear water)
        GameObject waterBed = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterBed.name = "WaterBed";
        waterBed.transform.position = new Vector3(0, -1f, 0);
        waterBed.transform.localScale = new Vector3(50, 1, 50);
        Material bedMat = new Material(Shader.Find("Standard"));
        bedMat.color = new Color(0.65f, 0.58f, 0.45f); // Sandy bottom color
        waterBed.GetComponent<Renderer>().sharedMaterial = bedMat;
    }

    static void CreateDock()
    {
        // RAISED DOCK ON LEGS - extends from island out over water
        // MOVED MORE TO THE LEFT (X = -12)
        GameObject dockParent = new GameObject("Dock");
        dockParent.transform.position = new Vector3(-12f, 0, 0);  // 12 meters left

        Material woodMat = MaterialGenerator.CreateWoodMaterial();
        Material darkWood = new Material(Shader.Find("Standard"));
        darkWood.color = new Color(0.22f, 0.14f, 0.08f);

        // Dock dimensions - LONGER and HIGHER
        float dockStartZ = 8f;    // Start slightly earlier
        float dockEndZ = 58f;     // Extend further out over water (was 45)
        float dockWidth = 5f;
        float dockHeight = 2.5f;  // RAISED HIGHER above ground level (was 1.5)
        float legHeight = 3.5f;   // Visible legs going down

        // MAIN DOCK SURFACE - HAS COLLIDER for walking
        GameObject dockSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dockSurface.name = "DockSurface";
        dockSurface.transform.SetParent(dockParent.transform);
        dockSurface.transform.localPosition = new Vector3(0, dockHeight, (dockStartZ + dockEndZ) / 2f);
        dockSurface.transform.localScale = new Vector3(dockWidth, 0.3f, dockEndZ - dockStartZ);
        dockSurface.GetComponent<Renderer>().sharedMaterial = woodMat;
        // KEEP COLLIDER - player walks on this

        // Support LEGS going down into water - VISUAL ONLY
        float[] legPositions = { 12f, 22f, 32f, 42f, 52f };
        foreach (float zPos in legPositions)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                // Main vertical leg
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = "DockLeg";
                leg.transform.SetParent(dockParent.transform);
                leg.transform.localPosition = new Vector3(side * 2f, dockHeight - legHeight / 2f, zPos);
                leg.transform.localScale = new Vector3(0.3f, legHeight / 2f, 0.3f);
                leg.GetComponent<Renderer>().sharedMaterial = darkWood;
                Object.DestroyImmediate(leg.GetComponent<Collider>()); // Visual only

                // Diagonal support brace
                GameObject brace = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                brace.name = "DockBrace";
                brace.transform.SetParent(dockParent.transform);
                brace.transform.localPosition = new Vector3(side * 1.5f, dockHeight - 1.2f, zPos);
                brace.transform.localRotation = Quaternion.Euler(0, 0, side * 35f);
                brace.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);
                brace.GetComponent<Renderer>().sharedMaterial = darkWood;
                Object.DestroyImmediate(brace.GetComponent<Collider>()); // Visual only
            }
        }

        // Cross beams under dock - VISUAL ONLY
        for (int i = 0; i < 6; i++)
        {
            float z = dockStartZ + 5 + i * 8f;
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "CrossBeam";
            beam.transform.SetParent(dockParent.transform);
            beam.transform.localPosition = new Vector3(0, dockHeight - 0.3f, z);
            beam.transform.localScale = new Vector3(dockWidth + 0.5f, 0.15f, 0.2f);
            beam.GetComponent<Renderer>().sharedMaterial = darkWood;
            Object.DestroyImmediate(beam.GetComponent<Collider>()); // Visual only
        }

        // Longitudinal support beams (run length of dock)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject longBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            longBeam.name = "LongBeam";
            longBeam.transform.SetParent(dockParent.transform);
            longBeam.transform.localPosition = new Vector3(side * 1.8f, dockHeight - 0.5f, (dockStartZ + dockEndZ) / 2f);
            longBeam.transform.localScale = new Vector3(0.2f, 0.2f, dockEndZ - dockStartZ - 2f);
            longBeam.GetComponent<Renderer>().sharedMaterial = darkWood;
            Object.DestroyImmediate(longBeam.GetComponent<Collider>()); // Visual only
        }

        // Decorative rope coil at end - VISUAL ONLY
        GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.name = "RopeCoil";
        rope.transform.SetParent(dockParent.transform);
        rope.transform.localPosition = new Vector3(2f, dockHeight + 0.2f, dockEndZ - 2f);
        rope.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        Material ropeMat = new Material(Shader.Find("Standard"));
        ropeMat.color = new Color(0.55f, 0.45f, 0.30f);
        rope.GetComponent<Renderer>().sharedMaterial = ropeMat;
        Object.DestroyImmediate(rope.GetComponent<Collider>()); // Visual only

        // STAIRCASE leading up to dock from ground level
        // Ground is at 1.26f, dock is at dockHeight (2.5f)
        // Stairs at the entrance of the dock (z around dockStartZ)
        float groundLevel = 1.26f;
        float stairStartZ = dockStartZ - 3f;  // Start stairs 3m before dock entrance
        int numSteps = 5;
        float stepHeight = (dockHeight - groundLevel) / numSteps;  // Height per step
        float stepDepth = 0.6f;  // Depth of each step
        float stairWidth = dockWidth;  // Same width as dock

        for (int i = 0; i < numSteps; i++)
        {
            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "DockStair_" + i;
            step.transform.SetParent(dockParent.transform);

            // Each step is higher and further toward the dock
            float stepY = groundLevel + stepHeight * (i + 0.5f);
            float stepZ = stairStartZ + stepDepth * i;

            step.transform.localPosition = new Vector3(0, stepY, stepZ);
            step.transform.localScale = new Vector3(stairWidth, stepHeight, stepDepth);
            step.GetComponent<Renderer>().sharedMaterial = woodMat;
            // KEEP COLLIDER - player walks on stairs
        }

        // Side rails for the staircase
        for (int side = -1; side <= 1; side += 2)
        {
            // Rail post at bottom
            GameObject bottomPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottomPost.name = "StairPostBottom";
            bottomPost.transform.SetParent(dockParent.transform);
            bottomPost.transform.localPosition = new Vector3(side * (stairWidth / 2f + 0.15f), groundLevel + 0.5f, stairStartZ - 0.2f);
            bottomPost.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            bottomPost.GetComponent<Renderer>().sharedMaterial = darkWood;
            Object.DestroyImmediate(bottomPost.GetComponent<Collider>());

            // Rail post at top
            GameObject topPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topPost.name = "StairPostTop";
            topPost.transform.SetParent(dockParent.transform);
            topPost.transform.localPosition = new Vector3(side * (stairWidth / 2f + 0.15f), dockHeight + 0.5f, stairStartZ + stepDepth * numSteps - 0.2f);
            topPost.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
            topPost.GetComponent<Renderer>().sharedMaterial = darkWood;
            Object.DestroyImmediate(topPost.GetComponent<Collider>());

            // Angled handrail connecting posts
            GameObject handrail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handrail.name = "StairHandrail";
            handrail.transform.SetParent(dockParent.transform);

            float railCenterY = (groundLevel + dockHeight) / 2f + 0.5f;
            float railCenterZ = stairStartZ + (stepDepth * numSteps) / 2f - 0.2f;
            float railLength = Mathf.Sqrt(Mathf.Pow(dockHeight - groundLevel, 2) + Mathf.Pow(stepDepth * numSteps, 2));
            float railAngle = Mathf.Atan2(dockHeight - groundLevel, stepDepth * numSteps) * Mathf.Rad2Deg;

            handrail.transform.localPosition = new Vector3(side * (stairWidth / 2f + 0.15f), railCenterY, railCenterZ);
            handrail.transform.localRotation = Quaternion.Euler(-railAngle, 0, 0);
            handrail.transform.localScale = new Vector3(0.08f, 0.08f, railLength);
            handrail.GetComponent<Renderer>().sharedMaterial = darkWood;
            Object.DestroyImmediate(handrail.GetComponent<Collider>());
        }
    }

    static void CreateRamp()
    {
        // RAMP NOT NEEDED - unified floor is all one level
        // This function kept for compatibility but does nothing
    }

    static void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, 2f, -5f);  // Start on grass island
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        player.AddComponent<PlayerController>();
        player.AddComponent<FishingRodAnimator>();

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 5f;

        CapsuleCollider col = player.AddComponent<CapsuleCollider>();
        col.height = 1.8f;
        col.radius = 0.3f;
        col.center = new Vector3(0, 0, 0);

        // Human proportions: ~7.5 heads tall for adult
        // Total height ~1.8m, head ~0.24m

        Material skinMat = MaterialGenerator.CreateSkinMaterial();

        // TORSO (naked upper body)
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso";
        torso.transform.SetParent(player.transform);
        torso.transform.localPosition = new Vector3(0, 0.15f, 0);
        torso.transform.localScale = new Vector3(0.45f, 0.55f, 0.25f);
        torso.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(torso.GetComponent<Collider>());

        // HIPS/WAIST (naked)
        GameObject hips = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hips.name = "Hips";
        hips.transform.SetParent(player.transform);
        hips.transform.localPosition = new Vector3(0, -0.2f, 0);
        hips.transform.localScale = new Vector3(0.40f, 0.25f, 0.22f);
        hips.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(hips.GetComponent<Collider>());

        // LEFT LEG (naked)
        CreateLeg(player, skinMat, -0.12f);
        // RIGHT LEG (naked)
        CreateLeg(player, skinMat, 0.12f);

        // LEFT ARM (naked)
        CreateArm(player, skinMat, skinMat, -0.30f);
        // RIGHT ARM (naked)
        CreateArm(player, skinMat, skinMat, 0.30f);

        // NECK
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.name = "Neck";
        neck.transform.SetParent(player.transform);
        neck.transform.localPosition = new Vector3(0, 0.5f, 0);
        neck.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
        neck.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(neck.GetComponent<Collider>());

        // HEAD
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(player.transform);
        head.transform.localPosition = new Vector3(0, 0.72f, 0);
        head.transform.localScale = new Vector3(0.26f, 0.30f, 0.26f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // FACE DETAILS
        CreateFace(head, skinMat);

        // NO HAT - player starts bald/naked

        // FISHING ROD
        CreateFishingRod(player);
    }

    static void CreateLeg(GameObject player, Material skinMat, float xOffset)
    {
        // Upper leg (naked)
        GameObject upperLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        upperLeg.name = "UpperLeg";
        upperLeg.transform.SetParent(player.transform);
        upperLeg.transform.localPosition = new Vector3(xOffset, -0.50f, 0);
        upperLeg.transform.localScale = new Vector3(0.14f, 0.22f, 0.14f);
        upperLeg.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(upperLeg.GetComponent<Collider>());

        // Lower leg (naked)
        GameObject lowerLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        lowerLeg.name = "LowerLeg";
        lowerLeg.transform.SetParent(player.transform);
        lowerLeg.transform.localPosition = new Vector3(xOffset, -0.78f, 0);
        lowerLeg.transform.localScale = new Vector3(0.11f, 0.20f, 0.11f);
        lowerLeg.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(lowerLeg.GetComponent<Collider>());

        // Bare foot (naked)
        GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        foot.name = "Foot";
        foot.transform.SetParent(player.transform);
        foot.transform.localPosition = new Vector3(xOffset, -0.95f, 0.03f);
        foot.transform.localScale = new Vector3(0.10f, 0.08f, 0.18f);
        foot.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(foot.GetComponent<Collider>());
    }

    static void CreateArm(GameObject player, Material skinMat, Material armMat, float xOffset)
    {
        // Both arms reach forward to hold the fishing rod with two hands
        bool isRightArm = xOffset > 0;

        // Shoulder - at body sides (naked)
        GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shoulder.name = isRightArm ? "RightShoulder" : "LeftShoulder";
        shoulder.transform.SetParent(player.transform);
        shoulder.transform.localPosition = new Vector3(xOffset * 0.8f, 0.35f, 0.02f);
        shoulder.transform.localScale = new Vector3(0.13f, 0.11f, 0.11f);
        shoulder.GetComponent<Renderer>().sharedMaterial = armMat;
        Object.DestroyImmediate(shoulder.GetComponent<Collider>());

        // Upper arm - angled forward and inward toward center where rod is held (naked)
        GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        upperArm.name = isRightArm ? "RightUpperArm" : "LeftUpperArm";
        upperArm.transform.SetParent(player.transform);
        // Both arms angle forward and slightly down, converging toward rod
        upperArm.transform.localPosition = new Vector3(xOffset * 0.55f, 0.18f, 0.15f);
        upperArm.transform.localRotation = Quaternion.Euler(60, isRightArm ? -15 : 15, isRightArm ? -20 : 20);
        upperArm.transform.localScale = new Vector3(0.09f, 0.16f, 0.09f);
        upperArm.GetComponent<Renderer>().sharedMaterial = armMat;
        Object.DestroyImmediate(upperArm.GetComponent<Collider>());

        // Lower arm - extends forward to rod, both arms parallel
        GameObject lowerArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        lowerArm.name = isRightArm ? "RightLowerArm" : "LeftLowerArm";
        lowerArm.transform.SetParent(player.transform);
        // Forearms reach forward to grip rod - right hand lower, left hand higher on rod
        float handHeight = isRightArm ? -0.02f : 0.04f;
        float handForward = isRightArm ? 0.30f : 0.38f;
        lowerArm.transform.localPosition = new Vector3(xOffset * 0.25f, handHeight, handForward - 0.08f);
        lowerArm.transform.localRotation = Quaternion.Euler(75, 0, isRightArm ? 5 : -5);
        lowerArm.transform.localScale = new Vector3(0.07f, 0.14f, 0.07f);
        lowerArm.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(lowerArm.GetComponent<Collider>());

        // Hand - gripping the rod (positioned on the rod pivot area)
        GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hand.name = isRightArm ? "RightHand" : "LeftHand";
        hand.transform.SetParent(player.transform);
        // Right hand on lower handle, left hand on upper grip
        hand.transform.localPosition = new Vector3(xOffset * 0.08f, handHeight, handForward);
        hand.transform.localScale = new Vector3(0.09f, 0.06f, 0.10f);
        hand.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(hand.GetComponent<Collider>());
    }

    static void CreateFace(GameObject head, Material skinMat)
    {
        // Eyes
        CreateEye(head, new Vector3(0.06f, 0.05f, 0.45f));
        CreateEye(head, new Vector3(-0.06f, 0.05f, 0.45f));

        // Nose
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "Nose";
        nose.transform.SetParent(head.transform);
        nose.transform.localPosition = new Vector3(0, -0.05f, 0.48f);
        nose.transform.localScale = new Vector3(0.15f, 0.18f, 0.20f);
        nose.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(nose.GetComponent<Collider>());

        // Ears
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ear.name = "Ear";
            ear.transform.SetParent(head.transform);
            ear.transform.localPosition = new Vector3(side * 0.48f, 0, 0);
            ear.transform.localScale = new Vector3(0.12f, 0.20f, 0.10f);
            ear.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(ear.GetComponent<Collider>());
        }
    }

    static void CreateEye(GameObject head, Vector3 localPos)
    {
        // Eye white
        GameObject eyeWhite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eyeWhite.name = "EyeWhite";
        eyeWhite.transform.SetParent(head.transform);
        eyeWhite.transform.localPosition = localPos;
        eyeWhite.transform.localScale = new Vector3(0.12f, 0.08f, 0.06f);
        Material whiteMat = new Material(Shader.Find("Standard"));
        whiteMat.color = Color.white;
        eyeWhite.GetComponent<Renderer>().sharedMaterial = whiteMat;
        Object.DestroyImmediate(eyeWhite.GetComponent<Collider>());

        // Iris
        GameObject iris = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        iris.name = "Iris";
        iris.transform.SetParent(eyeWhite.transform);
        iris.transform.localPosition = new Vector3(0, 0, 0.35f);
        iris.transform.localScale = new Vector3(0.55f, 0.65f, 0.25f);
        Material irisMat = new Material(Shader.Find("Standard"));
        irisMat.color = new Color(0.3f, 0.5f, 0.35f);  // Green-brown eyes
        iris.GetComponent<Renderer>().sharedMaterial = irisMat;
        Object.DestroyImmediate(iris.GetComponent<Collider>());

        // Pupil
        GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pupil.name = "Pupil";
        pupil.transform.SetParent(iris.transform);
        pupil.transform.localPosition = new Vector3(0, 0, 0.3f);
        pupil.transform.localScale = new Vector3(0.5f, 0.5f, 0.3f);
        Material pupilMat = new Material(Shader.Find("Standard"));
        pupilMat.color = new Color(0.05f, 0.05f, 0.05f);
        pupil.GetComponent<Renderer>().sharedMaterial = pupilMat;
        Object.DestroyImmediate(pupil.GetComponent<Collider>());
    }

    static void CreateFishingHat(GameObject head)
    {
        Material strawMat = MaterialGenerator.CreateStrawMaterial();

        // Hat brim
        GameObject brim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        brim.name = "HatBrim";
        brim.transform.SetParent(head.transform);
        brim.transform.localPosition = new Vector3(0, 0.4f, 0);
        brim.transform.localScale = new Vector3(1.8f, 0.05f, 1.8f);
        brim.GetComponent<Renderer>().sharedMaterial = strawMat;
        Object.DestroyImmediate(brim.GetComponent<Collider>());

        // Hat crown
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crown.name = "HatCrown";
        crown.transform.SetParent(head.transform);
        crown.transform.localPosition = new Vector3(0, 0.6f, 0);
        crown.transform.localScale = new Vector3(1.0f, 0.25f, 1.0f);
        crown.GetComponent<Renderer>().sharedMaterial = strawMat;
        Object.DestroyImmediate(crown.GetComponent<Collider>());

        // Hat band
        GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.name = "HatBand";
        band.transform.SetParent(head.transform);
        band.transform.localPosition = new Vector3(0, 0.48f, 0);
        band.transform.localScale = new Vector3(1.05f, 0.06f, 1.05f);
        Material bandMat = new Material(Shader.Find("Standard"));
        bandMat.color = new Color(0.45f, 0.20f, 0.10f);
        band.GetComponent<Renderer>().sharedMaterial = bandMat;
        Object.DestroyImmediate(band.GetComponent<Collider>());
    }

    static void CreateFishingRod(GameObject player)
    {
        // Rod pivot - positioned at where hands grip, angled naturally
        GameObject rodPivot = new GameObject("RodPivot");
        rodPivot.transform.SetParent(player.transform);
        rodPivot.transform.localPosition = new Vector3(0, 0.02f, 0.37f);

        Material rodMat = new Material(Shader.Find("Standard"));
        rodMat.color = new Color(0.35f, 0.22f, 0.12f);
        rodMat.SetFloat("_Glossiness", 0.5f);

        // Handle (cork grip) - where right hand grips
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "RodHandle";
        handle.transform.SetParent(rodPivot.transform);
        handle.transform.localPosition = new Vector3(0, -0.12f, 0);
        handle.transform.localRotation = Quaternion.Euler(90, 0, 0);
        handle.transform.localScale = new Vector3(0.055f, 0.18f, 0.055f);
        Material corkMat = new Material(Shader.Find("Standard"));
        corkMat.color = new Color(0.70f, 0.55f, 0.40f);
        handle.GetComponent<Renderer>().sharedMaterial = corkMat;
        Object.DestroyImmediate(handle.GetComponent<Collider>());

        // Handle grip texture (cork rings)
        GameObject corkRing1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        corkRing1.name = "CorkRing";
        corkRing1.transform.SetParent(rodPivot.transform);
        corkRing1.transform.localPosition = new Vector3(0, -0.05f, 0);
        corkRing1.transform.localRotation = Quaternion.Euler(90, 0, 0);
        corkRing1.transform.localScale = new Vector3(0.06f, 0.015f, 0.06f);
        Material ringMat = new Material(Shader.Find("Standard"));
        ringMat.color = new Color(0.50f, 0.40f, 0.30f);
        corkRing1.GetComponent<Renderer>().sharedMaterial = ringMat;
        Object.DestroyImmediate(corkRing1.GetComponent<Collider>());

        // Rod butt (end cap)
        GameObject buttCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        buttCap.name = "RodButt";
        buttCap.transform.SetParent(rodPivot.transform);
        buttCap.transform.localPosition = new Vector3(0, -0.28f, 0);
        buttCap.transform.localRotation = Quaternion.Euler(90, 0, 0);
        buttCap.transform.localScale = new Vector3(0.045f, 0.03f, 0.045f);
        Material buttMat = new Material(Shader.Find("Standard"));
        buttMat.color = new Color(0.2f, 0.2f, 0.2f);
        buttCap.GetComponent<Renderer>().sharedMaterial = buttMat;
        Object.DestroyImmediate(buttCap.GetComponent<Collider>());

        // Reel seat (where reel mounts) - right below where left hand grips
        GameObject reelSeat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        reelSeat.name = "ReelSeat";
        reelSeat.transform.SetParent(rodPivot.transform);
        reelSeat.transform.localPosition = new Vector3(0, 0.05f, 0);
        reelSeat.transform.localRotation = Quaternion.Euler(90, 0, 0);
        reelSeat.transform.localScale = new Vector3(0.045f, 0.08f, 0.045f);
        reelSeat.GetComponent<Renderer>().sharedMaterial = buttMat;
        Object.DestroyImmediate(reelSeat.GetComponent<Collider>());

        // Upper grip area (where left hand goes)
        GameObject upperGrip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        upperGrip.name = "UpperGrip";
        upperGrip.transform.SetParent(rodPivot.transform);
        upperGrip.transform.localPosition = new Vector3(0, 0.18f, 0);
        upperGrip.transform.localRotation = Quaternion.Euler(90, 0, 0);
        upperGrip.transform.localScale = new Vector3(0.04f, 0.10f, 0.04f);
        upperGrip.GetComponent<Renderer>().sharedMaterial = corkMat;
        Object.DestroyImmediate(upperGrip.GetComponent<Collider>());

        // Rod shaft - main blank extending forward
        GameObject rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rod.name = "FishingRod";
        rod.transform.SetParent(rodPivot.transform);
        rod.transform.localPosition = new Vector3(0, 0.75f, 0);
        rod.transform.localRotation = Quaternion.Euler(90, 0, 0);
        rod.transform.localScale = new Vector3(0.025f, 0.50f, 0.025f);
        rod.GetComponent<Renderer>().sharedMaterial = rodMat;
        Object.DestroyImmediate(rod.GetComponent<Collider>());

        // Rod tip section (thinner, more flexible looking)
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tip.name = "RodTip";
        tip.transform.SetParent(rod.transform);
        tip.transform.localPosition = new Vector3(0, 0.85f, 0);
        tip.transform.localScale = new Vector3(0.6f, 0.45f, 0.6f);
        tip.GetComponent<Renderer>().sharedMaterial = rodMat;
        Object.DestroyImmediate(tip.GetComponent<Collider>());

        // Line guides (eyelets along rod)
        Material guideMat = new Material(Shader.Find("Standard"));
        guideMat.color = new Color(0.3f, 0.3f, 0.35f);
        guideMat.SetFloat("_Metallic", 0.8f);

        float[] guidePositions = { 0.3f, 0.55f, 0.75f, 0.9f };
        foreach (float pos in guidePositions)
        {
            GameObject guide = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            guide.name = "LineGuide";
            guide.transform.SetParent(rodPivot.transform);
            guide.transform.localPosition = new Vector3(0, pos, 0.015f);
            guide.transform.localRotation = Quaternion.Euler(0, 0, 0);
            guide.transform.localScale = new Vector3(0.025f, 0.008f, 0.025f);
            guide.GetComponent<Renderer>().sharedMaterial = guideMat;
            Object.DestroyImmediate(guide.GetComponent<Collider>());
        }

        // Reel - mounted below handle, on reel seat
        GameObject reel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        reel.name = "Reel";
        reel.transform.SetParent(rodPivot.transform);
        reel.transform.localPosition = new Vector3(0.06f, 0.0f, 0);
        reel.transform.localRotation = Quaternion.Euler(0, 0, 0);
        reel.transform.localScale = new Vector3(0.07f, 0.025f, 0.07f);
        Material reelMat = new Material(Shader.Find("Standard"));
        reelMat.color = new Color(0.20f, 0.20f, 0.25f);
        reelMat.SetFloat("_Metallic", 0.8f);
        reelMat.SetFloat("_Glossiness", 0.7f);
        reel.GetComponent<Renderer>().sharedMaterial = reelMat;
        Object.DestroyImmediate(reel.GetComponent<Collider>());

        // Reel body (spool)
        GameObject reelBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        reelBody.name = "ReelSpool";
        reelBody.transform.SetParent(reel.transform);
        reelBody.transform.localPosition = new Vector3(0.8f, 0, 0);
        reelBody.transform.localRotation = Quaternion.Euler(0, 0, 90);
        reelBody.transform.localScale = new Vector3(0.9f, 0.4f, 0.9f);
        reelBody.GetComponent<Renderer>().sharedMaterial = reelMat;
        Object.DestroyImmediate(reelBody.GetComponent<Collider>());

        // Reel handle
        GameObject reelHandle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        reelHandle.name = "ReelHandle";
        reelHandle.transform.SetParent(reel.transform);
        reelHandle.transform.localPosition = new Vector3(1.2f, 0, 0);
        reelHandle.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        reelHandle.GetComponent<Renderer>().sharedMaterial = reelMat;
        Object.DestroyImmediate(reelHandle.GetComponent<Collider>());
    }

    static void CreateTreesAroundScene()
    {
        GameObject treesParent = new GameObject("TreesParent");
        float groundY = 1.0f; // Match unified floor

        // Tree size categories: 0=small(3-5), 1=medium(6-10), 2=large(11-15), 3=giant(16-20)
        // Tree type: 0=oak-like, 1=pine-like, 2=palm-like

        // Trees on the left side of the dock - with grass patches
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(-35f, -10f);
            float z = Random.Range(-30f, 5f);
            int treeType = Random.Range(0, 3);
            int sizeCategory = Random.Range(0, 4);
            CreateVariedTree(treesParent.transform, new Vector3(x, groundY, z), treeType, sizeCategory);
            CreateGrassPatchUnderTree(treesParent.transform, new Vector3(x, groundY, z));
        }

        // Trees on the right side - with grass patches
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(10f, 35f);
            float z = Random.Range(-30f, 5f);
            int treeType = Random.Range(0, 3);
            int sizeCategory = Random.Range(0, 4);
            CreateVariedTree(treesParent.transform, new Vector3(x, groundY, z), treeType, sizeCategory);
            CreateGrassPatchUnderTree(treesParent.transform, new Vector3(x, groundY, z));
        }

        // Trees in the background - with grass patches (more large and giant trees for depth)
        for (int i = 0; i < 12; i++)
        {
            float x = Random.Range(-45f, 45f);
            float z = Random.Range(-50f, -25f);
            int treeType = Random.Range(0, 3);
            int sizeCategory = Random.Range(1, 4); // No small trees in background
            CreateVariedTree(treesParent.transform, new Vector3(x, groundY, z), treeType, sizeCategory);
            CreateGrassPatchUnderTree(treesParent.transform, new Vector3(x, groundY, z));
        }

        // A few scattered small/medium trees on island itself
        for (int i = 0; i < 4; i++)
        {
            float x = Random.Range(-20f, 20f);
            float z = Random.Range(-20f, -5f);
            int treeType = Random.Range(0, 3);
            int sizeCategory = Random.Range(0, 2); // Only small and medium on island
            CreateVariedTree(treesParent.transform, new Vector3(x, groundY, z), treeType, sizeCategory);
        }
    }

    static void CreateVariedTree(Transform parent, Vector3 pos, int treeType, int sizeCategory)
    {
        // Get height based on size category
        float minHeight, maxHeight;
        switch (sizeCategory)
        {
            case 0: minHeight = 3f; maxHeight = 5f; break;    // Small
            case 1: minHeight = 6f; maxHeight = 10f; break;   // Medium
            case 2: minHeight = 11f; maxHeight = 15f; break;  // Large
            case 3: minHeight = 16f; maxHeight = 22f; break;  // Giant
            default: minHeight = 6f; maxHeight = 10f; break;
        }
        float treeHeight = Random.Range(minHeight, maxHeight);

        switch (treeType)
        {
            case 0: // Oak-like tree (round canopy)
                CreateOakTree(parent, pos, treeHeight);
                break;
            case 1: // Pine-like tree (conical canopy)
                CreatePineTree(parent, pos, treeHeight);
                break;
            case 2: // Palm-like tree (tropical)
                CreateTropicalPalmTree(parent, pos, treeHeight);
                break;
        }
    }

    static void CreateOakTree(Transform parent, Vector3 pos, float treeHeight)
    {
        GameObject tree = new GameObject("OakTree");
        tree.transform.SetParent(parent);
        tree.transform.position = pos;

        Material barkMat = MaterialGenerator.CreateBarkMaterial();
        Material leavesMat = MaterialGenerator.CreateLeavesMaterial();

        float trunkBaseRadius = treeHeight * 0.04f;

        // TRUNK - tapers from bottom to top
        GameObject lowerTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lowerTrunk.name = "LowerTrunk";
        lowerTrunk.transform.SetParent(tree.transform);
        lowerTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.2f, 0);
        lowerTrunk.transform.localScale = new Vector3(trunkBaseRadius, treeHeight * 0.2f, trunkBaseRadius);
        lowerTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(lowerTrunk.GetComponent<Collider>());

        GameObject midTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        midTrunk.name = "MidTrunk";
        midTrunk.transform.SetParent(tree.transform);
        midTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.45f, 0);
        midTrunk.transform.localScale = new Vector3(trunkBaseRadius * 0.75f, treeHeight * 0.15f, trunkBaseRadius * 0.75f);
        midTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(midTrunk.GetComponent<Collider>());

        GameObject upperTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        upperTrunk.name = "UpperTrunk";
        upperTrunk.transform.SetParent(tree.transform);
        upperTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.6f, 0);
        upperTrunk.transform.localScale = new Vector3(trunkBaseRadius * 0.5f, treeHeight * 0.1f, trunkBaseRadius * 0.5f);
        upperTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(upperTrunk.GetComponent<Collider>());

        // BRANCHES
        int numBranches = Mathf.RoundToInt(treeHeight * 0.5f);
        for (int i = 0; i < numBranches; i++)
        {
            float branchHeight = treeHeight * Random.Range(0.4f, 0.7f);
            float branchAngle = Random.Range(0f, 360f);
            float branchTilt = Random.Range(15f, 45f);
            float branchLength = Random.Range(1.5f, 3f) * (treeHeight / 8f);

            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch.name = "Branch";
            branch.transform.SetParent(tree.transform);
            branch.transform.localPosition = new Vector3(0, branchHeight, 0);
            branch.transform.localRotation = Quaternion.Euler(branchTilt, branchAngle, 0);
            branch.transform.localScale = new Vector3(0.08f * (treeHeight / 8f), branchLength * 0.5f, 0.08f * (treeHeight / 8f));
            branch.GetComponent<Renderer>().sharedMaterial = barkMat;
            Object.DestroyImmediate(branch.GetComponent<Collider>());
            branch.transform.localPosition += branch.transform.up * branchLength * 0.4f;
        }

        // LEAF CANOPY - round shape for oak
        float canopyRadius = treeHeight * 0.35f;

        GameObject mainCanopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mainCanopy.name = "MainCanopy";
        mainCanopy.transform.SetParent(tree.transform);
        mainCanopy.transform.localPosition = new Vector3(0, treeHeight * 0.75f, 0);
        mainCanopy.transform.localScale = new Vector3(canopyRadius * 1.8f, canopyRadius * 1.5f, canopyRadius * 1.8f);
        mainCanopy.GetComponent<Renderer>().sharedMaterial = leavesMat;
        Object.DestroyImmediate(mainCanopy.GetComponent<Collider>());

        // Lower canopy clusters
        int numClusters = Mathf.RoundToInt(treeHeight * 0.6f);
        float canopyBaseHeight = treeHeight * 0.55f;
        for (int i = 0; i < numClusters; i++)
        {
            float angle = (360f / numClusters) * i + Random.Range(-20f, 20f);
            float rad = angle * Mathf.Deg2Rad;
            float dist = Random.Range(canopyRadius * 0.6f, canopyRadius * 1.1f);
            float height = canopyBaseHeight + Random.Range(-0.5f, 1.5f);

            GameObject cluster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cluster.name = "LeafCluster";
            cluster.transform.SetParent(tree.transform);
            cluster.transform.localPosition = new Vector3(Mathf.Sin(rad) * dist, height, Mathf.Cos(rad) * dist);
            float clusterSize = Random.Range(canopyRadius * 0.5f, canopyRadius * 0.8f);
            cluster.transform.localScale = new Vector3(clusterSize, clusterSize * 0.8f, clusterSize);
            cluster.GetComponent<Renderer>().sharedMaterial = leavesMat;
            Object.DestroyImmediate(cluster.GetComponent<Collider>());
        }

        // Top crown
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(tree.transform);
        crown.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), treeHeight * 0.9f, Random.Range(-0.3f, 0.3f));
        crown.transform.localScale = new Vector3(canopyRadius * 0.9f, canopyRadius * 1.1f, canopyRadius * 0.9f);
        crown.GetComponent<Renderer>().sharedMaterial = leavesMat;
        Object.DestroyImmediate(crown.GetComponent<Collider>());
    }

    static void CreatePineTree(Transform parent, Vector3 pos, float treeHeight)
    {
        GameObject tree = new GameObject("PineTree");
        tree.transform.SetParent(parent);
        tree.transform.position = pos;

        Material barkMat = new Material(Shader.Find("Standard"));
        barkMat.color = new Color(0.35f, 0.25f, 0.15f); // Darker pine bark

        Material pineMat = new Material(Shader.Find("Standard"));
        pineMat.color = new Color(0.12f, 0.32f, 0.18f); // Dark pine green

        float trunkBaseRadius = treeHeight * 0.03f;

        // Single straight trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, treeHeight * 0.35f, 0);
        trunk.transform.localScale = new Vector3(trunkBaseRadius, treeHeight * 0.35f, trunkBaseRadius);
        trunk.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(trunk.GetComponent<Collider>());

        // Conical leaf layers - stacked cones
        int numLayers = Mathf.RoundToInt(treeHeight * 0.4f);
        float baseWidth = treeHeight * 0.35f;
        for (int i = 0; i < numLayers; i++)
        {
            float layerY = treeHeight * 0.3f + (treeHeight * 0.65f * i / numLayers);
            float layerWidth = baseWidth * (1f - (float)i / numLayers * 0.8f);
            float layerHeight = treeHeight * 0.15f;

            GameObject layer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            layer.name = "PineLayer" + i;
            layer.transform.SetParent(tree.transform);
            layer.transform.localPosition = new Vector3(Random.Range(-0.1f, 0.1f), layerY, Random.Range(-0.1f, 0.1f));
            layer.transform.localScale = new Vector3(layerWidth, layerHeight, layerWidth);
            layer.GetComponent<Renderer>().sharedMaterial = pineMat;
            Object.DestroyImmediate(layer.GetComponent<Collider>());
        }

        // Pointed top
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        top.name = "PineTop";
        top.transform.SetParent(tree.transform);
        top.transform.localPosition = new Vector3(0, treeHeight * 0.95f, 0);
        top.transform.localScale = new Vector3(baseWidth * 0.15f, treeHeight * 0.12f, baseWidth * 0.15f);
        top.GetComponent<Renderer>().sharedMaterial = pineMat;
        Object.DestroyImmediate(top.GetComponent<Collider>());
    }

    static void CreateTropicalPalmTree(Transform parent, Vector3 pos, float treeHeight)
    {
        GameObject tree = new GameObject("PalmTree");
        tree.transform.SetParent(parent);
        tree.transform.position = pos;

        Material trunkMat = new Material(Shader.Find("Standard"));
        trunkMat.color = new Color(0.55f, 0.42f, 0.28f); // Palm trunk color

        Material leafMat = new Material(Shader.Find("Standard"));
        leafMat.color = new Color(0.18f, 0.48f, 0.15f); // Bright palm green

        // Curved trunk segments
        int numSegments = Mathf.RoundToInt(treeHeight * 0.5f);
        float segmentHeight = treeHeight / numSegments;
        float trunkRadius = treeHeight * 0.025f;
        float curve = Random.Range(-0.15f, 0.15f);

        for (int i = 0; i < numSegments; i++)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segment.name = "TrunkSegment" + i;
            segment.transform.SetParent(tree.transform);

            float xOffset = curve * i * 0.3f;
            float segY = segmentHeight * i + segmentHeight * 0.5f;
            segment.transform.localPosition = new Vector3(xOffset, segY, 0);
            segment.transform.localScale = new Vector3(trunkRadius * (1f - i * 0.03f), segmentHeight * 0.55f, trunkRadius * (1f - i * 0.03f));
            segment.GetComponent<Renderer>().sharedMaterial = trunkMat;
            Object.DestroyImmediate(segment.GetComponent<Collider>());
        }

        // Palm fronds at top
        int numFronds = Random.Range(6, 10);
        float topX = curve * numSegments * 0.3f;
        for (int i = 0; i < numFronds; i++)
        {
            float angle = (360f / numFronds) * i + Random.Range(-10f, 10f);
            float tilt = Random.Range(25f, 55f);

            GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frond.name = "PalmFrond" + i;
            frond.transform.SetParent(tree.transform);
            frond.transform.localPosition = new Vector3(topX, treeHeight * 0.95f, 0);
            frond.transform.localRotation = Quaternion.Euler(tilt, angle, 0);

            float frondLength = treeHeight * Random.Range(0.25f, 0.4f);
            frond.transform.localScale = new Vector3(0.15f, frondLength, 0.6f);
            frond.GetComponent<Renderer>().sharedMaterial = leafMat;
            Object.DestroyImmediate(frond.GetComponent<Collider>());

            // Move frond outward
            frond.transform.localPosition += frond.transform.up * frondLength * 0.4f;
        }

        // Central crown
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "PalmCrown";
        crown.transform.SetParent(tree.transform);
        crown.transform.localPosition = new Vector3(topX, treeHeight * 0.98f, 0);
        crown.transform.localScale = new Vector3(treeHeight * 0.08f, treeHeight * 0.06f, treeHeight * 0.08f);
        crown.GetComponent<Renderer>().sharedMaterial = leafMat;
        Object.DestroyImmediate(crown.GetComponent<Collider>());
    }

    static void CreateGrassPatchUnderTree(Transform parent, Vector3 pos)
    {
        // Grass visual patch under each tree (no collider)
        Material grassMat = new Material(Shader.Find("Standard"));
        grassMat.color = new Color(0.28f, 0.52f, 0.18f);

        GameObject grassPatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grassPatch.name = "GrassPatch";
        grassPatch.transform.SetParent(parent);
        grassPatch.transform.position = new Vector3(pos.x, pos.y + 0.01f, pos.z);
        grassPatch.transform.localScale = new Vector3(4f, 0.02f, 4f);
        grassPatch.GetComponent<Renderer>().sharedMaterial = grassMat;
        Object.DestroyImmediate(grassPatch.GetComponent<Collider>()); // NO COLLIDER

        // Add some grass tufts
        AddProceduralGrass(parent, pos, 1.5f, 8);
    }

    static void CreateRealisticTree(Transform parent, Vector3 pos)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.SetParent(parent);
        tree.transform.position = pos;

        Material barkMat = MaterialGenerator.CreateBarkMaterial();
        Material leavesMat = MaterialGenerator.CreateLeavesMaterial();

        float treeHeight = Random.Range(6f, 10f);
        float trunkBaseRadius = Random.Range(0.3f, 0.5f);

        // TRUNK - tapers from bottom to top - ALL VISUAL ONLY
        // Lower trunk section
        GameObject lowerTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lowerTrunk.name = "LowerTrunk";
        lowerTrunk.transform.SetParent(tree.transform);
        lowerTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.2f, 0);
        lowerTrunk.transform.localScale = new Vector3(trunkBaseRadius, treeHeight * 0.2f, trunkBaseRadius);
        lowerTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(lowerTrunk.GetComponent<Collider>()); // NO COLLIDER

        // Middle trunk (slightly thinner)
        GameObject midTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        midTrunk.name = "MidTrunk";
        midTrunk.transform.SetParent(tree.transform);
        midTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.45f, 0);
        midTrunk.transform.localScale = new Vector3(trunkBaseRadius * 0.75f, treeHeight * 0.15f, trunkBaseRadius * 0.75f);
        midTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(midTrunk.GetComponent<Collider>()); // NO COLLIDER

        // Upper trunk (thinner still)
        GameObject upperTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        upperTrunk.name = "UpperTrunk";
        upperTrunk.transform.SetParent(tree.transform);
        upperTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.6f, 0);
        upperTrunk.transform.localScale = new Vector3(trunkBaseRadius * 0.5f, treeHeight * 0.1f, trunkBaseRadius * 0.5f);
        upperTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(upperTrunk.GetComponent<Collider>()); // NO COLLIDER

        // BRANCHES - several main branches coming off trunk - VISUAL ONLY
        int numBranches = Random.Range(4, 7);
        for (int i = 0; i < numBranches; i++)
        {
            float branchHeight = treeHeight * Random.Range(0.4f, 0.7f);
            float branchAngle = Random.Range(0f, 360f);
            float branchTilt = Random.Range(15f, 45f);
            float branchLength = Random.Range(1.5f, 3f);

            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch.name = "Branch";
            branch.transform.SetParent(tree.transform);
            branch.transform.localPosition = new Vector3(0, branchHeight, 0);
            branch.transform.localRotation = Quaternion.Euler(branchTilt, branchAngle, 0);
            branch.transform.localScale = new Vector3(0.08f, branchLength * 0.5f, 0.08f);
            branch.GetComponent<Renderer>().sharedMaterial = barkMat;
            Object.DestroyImmediate(branch.GetComponent<Collider>()); // NO COLLIDER

            // Move branch outward along its direction
            branch.transform.localPosition += branch.transform.up * branchLength * 0.4f;
        }

        // LEAF CANOPY - layered clusters for natural look
        float canopyBaseHeight = treeHeight * 0.55f;
        float canopyRadius = treeHeight * 0.35f;

        // Main central canopy mass - VISUAL ONLY
        GameObject mainCanopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mainCanopy.name = "MainCanopy";
        mainCanopy.transform.SetParent(tree.transform);
        mainCanopy.transform.localPosition = new Vector3(0, treeHeight * 0.75f, 0);
        mainCanopy.transform.localScale = new Vector3(canopyRadius * 1.8f, canopyRadius * 1.5f, canopyRadius * 1.8f);
        mainCanopy.GetComponent<Renderer>().sharedMaterial = leavesMat;
        Object.DestroyImmediate(mainCanopy.GetComponent<Collider>()); // NO COLLIDER

        // Lower canopy clusters (spreading out) - VISUAL ONLY
        int numClusters = Random.Range(5, 8);
        for (int i = 0; i < numClusters; i++)
        {
            float angle = (360f / numClusters) * i + Random.Range(-20f, 20f);
            float rad = angle * Mathf.Deg2Rad;
            float dist = Random.Range(canopyRadius * 0.6f, canopyRadius * 1.1f);
            float height = canopyBaseHeight + Random.Range(-0.5f, 1.5f);

            float x = Mathf.Sin(rad) * dist;
            float z = Mathf.Cos(rad) * dist;

            GameObject cluster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cluster.name = "LeafCluster";
            cluster.transform.SetParent(tree.transform);
            cluster.transform.localPosition = new Vector3(x, height, z);

            float clusterSize = Random.Range(canopyRadius * 0.5f, canopyRadius * 0.8f);
            cluster.transform.localScale = new Vector3(clusterSize, clusterSize * 0.8f, clusterSize);
            cluster.GetComponent<Renderer>().sharedMaterial = leavesMat;
            Object.DestroyImmediate(cluster.GetComponent<Collider>()); // NO COLLIDER
        }

        // Top crown - VISUAL ONLY
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(tree.transform);
        crown.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), treeHeight * 0.9f, Random.Range(-0.3f, 0.3f));
        crown.transform.localScale = new Vector3(canopyRadius * 0.9f, canopyRadius * 1.1f, canopyRadius * 0.9f);
        crown.GetComponent<Renderer>().sharedMaterial = leavesMat;
        Object.DestroyImmediate(crown.GetComponent<Collider>()); // NO COLLIDER
    }

    static void CreateBBQ()
    {
        // BBQ Station at the end of the dock - moved to the left
        // Dock ends at z=58, surface at y=2.5
        GameObject bbq = new GameObject("BBQStation");
        bbq.transform.position = new Vector3(-14f, 2.5f, 52f);  // Moved left (was -12)
        bbq.transform.rotation = Quaternion.Euler(0, 90, 0);    // Face sideways
        bbq.AddComponent<BBQStation>();

        // Small radio to the RIGHT of BBQ - visible and interactable
        GameObject dockRadio = new GameObject("DockRadio");
        dockRadio.transform.position = new Vector3(-10.5f, 2.5f, 52f);  // Right side of dock, next to BBQ
        dockRadio.transform.rotation = Quaternion.Euler(0, -30, 0);     // Angled toward player
        dockRadio.AddComponent<DockRadio>();

        // Shoulder Parrot system (spawns when purchased from shop)
        GameObject parrotSystem = new GameObject("ShoulderParrot");
        parrotSystem.AddComponent<ShoulderParrot>();
    }

    static void CreateQuestNPC()
    {
        // WETSUIT PETE - Quest NPC wearing black wetsuit with snorkel
        // REMODELED with better human proportions
        // Dock surface is at y=2.5, Pete's model origin is at feet, so place at 2.65 to be ON the dock
        GameObject npc = new GameObject("QuestNPC");
        npc.transform.position = new Vector3(-12f, 2.65f, 18f);  // Y=2.65 - ON TOP of dock surface
        npc.transform.rotation = Quaternion.Euler(0, 180, 0);  // Facing toward shore/player approach

        // Add NPC interaction component (for clicking)
        NPCInteraction npcInteraction = npc.AddComponent<NPCInteraction>();
        npcInteraction.npcName = "Wetsuit Pete";
        npcInteraction.interactionRange = 5f;

        // Add Wetsuit Pete's quest system with speech bubbles
        npc.AddComponent<WetsuitPeteQuests>();

        // Materials
        Material skinMat = MaterialGenerator.CreateSkinMaterial();
        Material wetsuitMat = new Material(Shader.Find("Standard"));
        wetsuitMat.color = new Color(0.05f, 0.05f, 0.08f); // Black wetsuit
        wetsuitMat.SetFloat("_Glossiness", 0.7f);

        Material wetsuitAccentMat = new Material(Shader.Find("Standard"));
        wetsuitAccentMat.color = new Color(0.12f, 0.12f, 0.18f);

        // === TORSO - properly proportioned ===
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        torso.name = "NPCTorso";
        torso.transform.SetParent(npc.transform);
        torso.transform.localPosition = new Vector3(0, 0.35f, 0);
        torso.transform.localScale = new Vector3(0.38f, 0.35f, 0.22f);
        torso.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
        Object.DestroyImmediate(torso.GetComponent<Collider>());

        // Chest detail
        GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        chest.transform.SetParent(npc.transform);
        chest.transform.localPosition = new Vector3(0, 0.45f, 0.05f);
        chest.transform.localScale = new Vector3(0.35f, 0.25f, 0.18f);
        chest.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
        Object.DestroyImmediate(chest.GetComponent<Collider>());

        // Wetsuit zipper stripe
        GameObject zipper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zipper.transform.SetParent(torso.transform);
        zipper.transform.localPosition = new Vector3(0, 0, 0.52f);
        zipper.transform.localScale = new Vector3(0.08f, 0.85f, 0.02f);
        Material zipperMat = new Material(Shader.Find("Standard"));
        zipperMat.color = new Color(0.5f, 0.5f, 0.55f);
        zipperMat.SetFloat("_Metallic", 0.8f);
        zipper.GetComponent<Renderer>().sharedMaterial = zipperMat;
        Object.DestroyImmediate(zipper.GetComponent<Collider>());

        // === HIPS - wider pelvis ===
        GameObject hips = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hips.name = "NPCHips";
        hips.transform.SetParent(npc.transform);
        hips.transform.localPosition = new Vector3(0, 0f, 0);
        hips.transform.localScale = new Vector3(0.35f, 0.18f, 0.20f);
        hips.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
        Object.DestroyImmediate(hips.GetComponent<Collider>());

        // === LEGS - LONGER and more proportionate ===
        for (float xOffset = -0.11f; xOffset <= 0.11f; xOffset += 0.22f)
        {
            // Upper thigh
            GameObject upperLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperLeg.transform.SetParent(npc.transform);
            upperLeg.transform.localPosition = new Vector3(xOffset, -0.35f, 0);
            upperLeg.transform.localScale = new Vector3(0.14f, 0.25f, 0.14f);
            upperLeg.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(upperLeg.GetComponent<Collider>());

            // Lower thigh / knee area
            GameObject knee = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            knee.transform.SetParent(npc.transform);
            knee.transform.localPosition = new Vector3(xOffset, -0.58f, 0.02f);
            knee.transform.localScale = new Vector3(0.12f, 0.10f, 0.12f);
            knee.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(knee.GetComponent<Collider>());

            // Shin / calf
            GameObject lowerLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lowerLeg.transform.SetParent(npc.transform);
            lowerLeg.transform.localPosition = new Vector3(xOffset, -0.85f, 0);
            lowerLeg.transform.localScale = new Vector3(0.10f, 0.22f, 0.10f);
            lowerLeg.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(lowerLeg.GetComponent<Collider>());

            // Ankle
            GameObject ankle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ankle.transform.SetParent(npc.transform);
            ankle.transform.localPosition = new Vector3(xOffset, -1.08f, 0);
            ankle.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f);
            ankle.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(ankle.GetComponent<Collider>());

            // Diving booties
            Material bootyMat = new Material(Shader.Find("Standard"));
            bootyMat.color = new Color(0.02f, 0.02f, 0.02f);
            bootyMat.SetFloat("_Glossiness", 0.8f);
            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foot.transform.SetParent(npc.transform);
            foot.transform.localPosition = new Vector3(xOffset, -1.18f, 0.05f);
            foot.transform.localScale = new Vector3(0.09f, 0.06f, 0.20f);
            foot.GetComponent<Renderer>().sharedMaterial = bootyMat;
            Object.DestroyImmediate(foot.GetComponent<Collider>());
        }

        // === ARMS - properly proportioned and positioned ===
        for (float xOffset = -0.24f; xOffset <= 0.24f; xOffset += 0.48f)
        {
            int side = xOffset > 0 ? 1 : -1;

            // Shoulder
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shoulder.transform.SetParent(npc.transform);
            shoulder.transform.localPosition = new Vector3(xOffset, 0.55f, 0);
            shoulder.transform.localScale = new Vector3(0.14f, 0.12f, 0.12f);
            shoulder.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(shoulder.GetComponent<Collider>());

            // Upper arm
            GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperArm.transform.SetParent(npc.transform);
            upperArm.transform.localPosition = new Vector3(xOffset * 1.25f, 0.35f, 0);
            upperArm.transform.localScale = new Vector3(0.10f, 0.18f, 0.10f);
            upperArm.transform.localRotation = Quaternion.Euler(0, 0, side * -15);
            upperArm.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(upperArm.GetComponent<Collider>());

            // Elbow
            GameObject elbow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            elbow.transform.SetParent(npc.transform);
            elbow.transform.localPosition = new Vector3(xOffset * 1.35f, 0.15f, 0);
            elbow.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            elbow.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(elbow.GetComponent<Collider>());

            // Forearm
            GameObject lowerArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lowerArm.transform.SetParent(npc.transform);
            lowerArm.transform.localPosition = new Vector3(xOffset * 1.2f, -0.05f, 0.08f);
            lowerArm.transform.localScale = new Vector3(0.08f, 0.16f, 0.08f);
            lowerArm.transform.localRotation = Quaternion.Euler(40, 0, side * 10);
            lowerArm.GetComponent<Renderer>().sharedMaterial = wetsuitAccentMat;
            Object.DestroyImmediate(lowerArm.GetComponent<Collider>());

            // Wrist
            GameObject wrist = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wrist.transform.SetParent(npc.transform);
            wrist.transform.localPosition = new Vector3(xOffset * 1.0f, -0.18f, 0.18f);
            wrist.transform.localScale = new Vector3(0.06f, 0.05f, 0.06f);
            wrist.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(wrist.GetComponent<Collider>());

            // Hand
            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hand.transform.SetParent(npc.transform);
            hand.transform.localPosition = new Vector3(xOffset * 0.9f, -0.25f, 0.22f);
            hand.transform.localScale = new Vector3(0.07f, 0.10f, 0.04f);
            hand.transform.localRotation = Quaternion.Euler(20, 0, 0);
            hand.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(hand.GetComponent<Collider>());
        }

        // === NECK ===
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.transform.SetParent(npc.transform);
        neck.transform.localPosition = new Vector3(0, 0.68f, 0);
        neck.transform.localScale = new Vector3(0.10f, 0.06f, 0.10f);
        neck.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(neck.GetComponent<Collider>());

        // === HEAD - better proportioned face ===
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "NPCHead";
        head.transform.SetParent(npc.transform);
        head.transform.localPosition = new Vector3(0, 0.88f, 0);
        head.transform.localScale = new Vector3(0.22f, 0.26f, 0.24f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Face plane (flatter front of face)
        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        face.transform.SetParent(head.transform);
        face.transform.localPosition = new Vector3(0, -0.05f, 0.35f);
        face.transform.localScale = new Vector3(0.85f, 0.75f, 0.25f);
        face.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(face.GetComponent<Collider>());

        // Eyes - better positioned
        for (float ex = -0.15f; ex <= 0.15f; ex += 0.30f)
        {
            // Eye socket (subtle shadow)
            GameObject eyeSocket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeSocket.transform.SetParent(head.transform);
            eyeSocket.transform.localPosition = new Vector3(ex, 0.08f, 0.42f);
            eyeSocket.transform.localScale = new Vector3(0.20f, 0.14f, 0.08f);
            Material socketMat = new Material(Shader.Find("Standard"));
            socketMat.color = new Color(0.75f, 0.60f, 0.50f);
            eyeSocket.GetComponent<Renderer>().sharedMaterial = socketMat;
            Object.DestroyImmediate(eyeSocket.GetComponent<Collider>());

            // Eye white
            GameObject eyeWhite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeWhite.transform.SetParent(head.transform);
            eyeWhite.transform.localPosition = new Vector3(ex, 0.08f, 0.46f);
            eyeWhite.transform.localScale = new Vector3(0.14f, 0.10f, 0.06f);
            Material whiteMat = new Material(Shader.Find("Standard"));
            whiteMat.color = new Color(0.98f, 0.98f, 0.95f);
            eyeWhite.GetComponent<Renderer>().sharedMaterial = whiteMat;
            Object.DestroyImmediate(eyeWhite.GetComponent<Collider>());

            // Iris
            GameObject iris = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            iris.transform.SetParent(eyeWhite.transform);
            iris.transform.localPosition = new Vector3(0, 0, 0.4f);
            iris.transform.localScale = new Vector3(0.5f, 0.6f, 0.3f);
            Material irisMat = new Material(Shader.Find("Standard"));
            irisMat.color = new Color(0.25f, 0.50f, 0.55f);  // Blue-green eyes
            iris.GetComponent<Renderer>().sharedMaterial = irisMat;
            Object.DestroyImmediate(iris.GetComponent<Collider>());

            // Pupil
            GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.transform.SetParent(iris.transform);
            pupil.transform.localPosition = new Vector3(0, 0, 0.35f);
            pupil.transform.localScale = new Vector3(0.45f, 0.45f, 0.3f);
            Material pupilMat = new Material(Shader.Find("Standard"));
            pupilMat.color = new Color(0.05f, 0.05f, 0.05f);
            pupil.GetComponent<Renderer>().sharedMaterial = pupilMat;
            Object.DestroyImmediate(pupil.GetComponent<Collider>());

            // Eyebrow
            GameObject eyebrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eyebrow.transform.SetParent(head.transform);
            eyebrow.transform.localPosition = new Vector3(ex, 0.22f, 0.42f);
            eyebrow.transform.localScale = new Vector3(0.20f, 0.035f, 0.06f);
            eyebrow.transform.localRotation = Quaternion.Euler(0, 0, ex > 0 ? -6 : 6);
            Material browMat = new Material(Shader.Find("Standard"));
            browMat.color = new Color(0.30f, 0.22f, 0.12f);
            eyebrow.GetComponent<Renderer>().sharedMaterial = browMat;
            Object.DestroyImmediate(eyebrow.GetComponent<Collider>());
        }

        // Nose bridge
        GameObject noseBridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        noseBridge.transform.SetParent(head.transform);
        noseBridge.transform.localPosition = new Vector3(0, 0f, 0.48f);
        noseBridge.transform.localScale = new Vector3(0.06f, 0.12f, 0.08f);
        noseBridge.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(noseBridge.GetComponent<Collider>());

        // Nose tip
        GameObject noseTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        noseTip.transform.SetParent(head.transform);
        noseTip.transform.localPosition = new Vector3(0, -0.08f, 0.52f);
        noseTip.transform.localScale = new Vector3(0.10f, 0.08f, 0.08f);
        noseTip.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(noseTip.GetComponent<Collider>());

        // Nostrils
        for (float nx = -0.03f; nx <= 0.03f; nx += 0.06f)
        {
            GameObject nostril = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nostril.transform.SetParent(head.transform);
            nostril.transform.localPosition = new Vector3(nx, -0.12f, 0.50f);
            nostril.transform.localScale = new Vector3(0.03f, 0.02f, 0.02f);
            Material nostrilMat = new Material(Shader.Find("Standard"));
            nostrilMat.color = new Color(0.5f, 0.35f, 0.3f);
            nostril.GetComponent<Renderer>().sharedMaterial = nostrilMat;
            Object.DestroyImmediate(nostril.GetComponent<Collider>());
        }

        // Ears
        for (float ex = -0.50f; ex <= 0.50f; ex += 1.0f)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ear.transform.SetParent(head.transform);
            ear.transform.localPosition = new Vector3(ex, 0.02f, 0f);
            ear.transform.localScale = new Vector3(0.08f, 0.14f, 0.08f);
            ear.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(ear.GetComponent<Collider>());
        }

        // Jaw / chin area
        GameObject jaw = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        jaw.transform.SetParent(head.transform);
        jaw.transform.localPosition = new Vector3(0, -0.30f, 0.20f);
        jaw.transform.localScale = new Vector3(0.60f, 0.30f, 0.45f);
        jaw.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(jaw.GetComponent<Collider>());

        // === SNORKEL ===
        Material snorkelMat = new Material(Shader.Find("Standard"));
        snorkelMat.color = new Color(0.1f, 0.4f, 0.6f);
        snorkelMat.SetFloat("_Glossiness", 0.8f);

        // Snorkel mouthpiece
        GameObject mouthpiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mouthpiece.transform.SetParent(head.transform);
        mouthpiece.transform.localPosition = new Vector3(0, -0.20f, 0.48f);
        mouthpiece.transform.localScale = new Vector3(0.10f, 0.05f, 0.12f);
        mouthpiece.GetComponent<Renderer>().sharedMaterial = snorkelMat;
        Object.DestroyImmediate(mouthpiece.GetComponent<Collider>());

        // Snorkel tube
        GameObject snorkelTube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        snorkelTube.transform.SetParent(head.transform);
        snorkelTube.transform.localPosition = new Vector3(0.38f, 0.15f, 0.25f);
        snorkelTube.transform.localScale = new Vector3(0.06f, 0.35f, 0.06f);
        snorkelTube.transform.localRotation = Quaternion.Euler(0, 0, 12);
        snorkelTube.GetComponent<Renderer>().sharedMaterial = snorkelMat;
        Object.DestroyImmediate(snorkelTube.GetComponent<Collider>());

        // Snorkel top
        GameObject snorkelTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        snorkelTop.transform.SetParent(head.transform);
        snorkelTop.transform.localPosition = new Vector3(0.42f, 0.50f, 0.15f);
        snorkelTop.transform.localScale = new Vector3(0.06f, 0.10f, 0.06f);
        snorkelTop.transform.localRotation = Quaternion.Euler(25, 0, 0);
        snorkelTop.GetComponent<Renderer>().sharedMaterial = snorkelMat;
        Object.DestroyImmediate(snorkelTop.GetComponent<Collider>());

        // Splash guard
        GameObject splashGuard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        splashGuard.transform.SetParent(head.transform);
        splashGuard.transform.localPosition = new Vector3(0.42f, 0.58f, 0.08f);
        splashGuard.transform.localScale = new Vector3(0.10f, 0.06f, 0.10f);
        Material guardMat = new Material(Shader.Find("Standard"));
        guardMat.color = new Color(0.9f, 0.35f, 0.1f);
        splashGuard.GetComponent<Renderer>().sharedMaterial = guardMat;
        Object.DestroyImmediate(splashGuard.GetComponent<Collider>());

        // Stubble
        GameObject stubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stubble.transform.SetParent(head.transform);
        stubble.transform.localPosition = new Vector3(0, -0.28f, 0.28f);
        stubble.transform.localScale = new Vector3(0.28f, 0.15f, 0.18f);
        Material stubbleMat = new Material(Shader.Find("Standard"));
        stubbleMat.color = new Color(0.55f, 0.45f, 0.38f);
        stubble.GetComponent<Renderer>().sharedMaterial = stubbleMat;
        Object.DestroyImmediate(stubble.GetComponent<Collider>());

        // Hair
        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.28f, 0.20f, 0.12f);
        hairMat.SetFloat("_Glossiness", 0.6f);

        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.transform.SetParent(head.transform);
        hair.transform.localPosition = new Vector3(0, 0.32f, -0.05f);
        hair.transform.localScale = new Vector3(1.02f, 0.38f, 0.95f);
        hair.GetComponent<Renderer>().sharedMaterial = hairMat;
        Object.DestroyImmediate(hair.GetComponent<Collider>());

        // === QUEST MARKER ===
        GameObject questMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        questMarker.name = "QuestMarker";
        questMarker.transform.SetParent(npc.transform);
        questMarker.transform.localPosition = new Vector3(0, 1.45f, 0);
        questMarker.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
        Material markerMat = new Material(Shader.Find("Standard"));
        markerMat.color = new Color(1f, 0.9f, 0f);
        markerMat.EnableKeyword("_EMISSION");
        markerMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0f) * 0.5f);
        questMarker.GetComponent<Renderer>().sharedMaterial = markerMat;
        Object.DestroyImmediate(questMarker.GetComponent<Collider>());

        GameObject markerStick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        markerStick.transform.SetParent(npc.transform);
        markerStick.transform.localPosition = new Vector3(0, 1.28f, 0);
        markerStick.transform.localScale = new Vector3(0.05f, 0.10f, 0.05f);
        markerStick.GetComponent<Renderer>().sharedMaterial = markerMat;
        Object.DestroyImmediate(markerStick.GetComponent<Collider>());
    }

    static void CreateSpawnNPC()
    {
        // NPC who comments on player being naked - positioned near spawn
        // Player spawns at (0, 2f, -5f), so place NPC in front facing player
        GameObject npc = new GameObject("SpawnNPC");
        npc.transform.position = new Vector3(2f, 1.26f, -2f);  // To the right of spawn, on the grass
        npc.transform.rotation = Quaternion.Euler(0, -120, 0);  // Facing toward spawn location
        npc.AddComponent<SpawnNPC>();
    }

    static void SetupCamera()
    {
        if (Camera.main.GetComponent<CameraController>() == null)
            Camera.main.gameObject.AddComponent<CameraController>();

        GameObject player = GameObject.Find("Player");
        Camera.main.GetComponent<CameraController>().target = player.transform;
        Camera.main.transform.position = new Vector3(0, 7, -12);
        Camera.main.backgroundColor = new Color(0.5f, 0.7f, 0.9f);  // Sky blue
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
    }

    static void CreatePortals()
    {
        GameObject portalsParent = new GameObject("PortalsParent");

        // 4 portals on the beach, behind where player spawns (negative Z)
        // Spaced along the beach area
        Vector3[] portalPositions = {
            new Vector3(-15f, 0.5f, -25f),  // Portal 1 - Jungle (Level 50)
            new Vector3(-5f, 0.5f, -28f),   // Portal 2 - Volcanic (Level 100)
            new Vector3(5f, 0.5f, -28f),    // Portal 3 - Ice (Level 200)
            new Vector3(15f, 0.5f, -25f)    // Portal 4 - Void (Level 300)
        };

        int[] requiredLevels = { 50, 100, 200, 300 };
        string[] portalNames = { "JunglePortal", "VolcanicPortal", "IcePortal", "VoidPortal" };
        Color[] portalColors = {
            new Color(0.2f, 0.8f, 0.3f),    // Green for jungle
            new Color(1f, 0.4f, 0.1f),       // Orange/red for volcanic
            new Color(0.4f, 0.8f, 1f),       // Light blue for ice
            new Color(0.6f, 0.2f, 0.8f)      // Purple for void
        };

        for (int i = 0; i < 4; i++)
        {
            CreateSinglePortal(portalsParent.transform, portalPositions[i], portalNames[i], requiredLevels[i], portalColors[i]);
        }
    }

    static void CreateSinglePortal(Transform parent, Vector3 pos, string name, int requiredLevel, Color portalColor)
    {
        GameObject portal = new GameObject(name);
        portal.transform.SetParent(parent);
        portal.transform.position = pos;
        portal.transform.rotation = Quaternion.Euler(0, 0, 0);  // Face forward toward player

        // Add portal interaction component
        PortalInteraction portalInteraction = portal.AddComponent<PortalInteraction>();
        portalInteraction.portalName = name.Replace("Portal", " Realm");
        portalInteraction.requiredLevel = requiredLevel;

        // Portal materials
        Material stoneMat = new Material(Shader.Find("Standard"));
        stoneMat.color = new Color(0.3f, 0.25f, 0.35f);  // Dark purple-gray stone

        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.color = portalColor;
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", portalColor * 0.8f);

        Material portalSurfaceMat = new Material(Shader.Find("Standard"));
        portalSurfaceMat.SetFloat("_Mode", 3);  // Transparent
        portalSurfaceMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        portalSurfaceMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        portalSurfaceMat.EnableKeyword("_ALPHABLEND_ON");
        portalSurfaceMat.color = new Color(portalColor.r, portalColor.g, portalColor.b, 0.6f);
        portalSurfaceMat.EnableKeyword("_EMISSION");
        portalSurfaceMat.SetColor("_EmissionColor", portalColor * 0.5f);

        // Portal arch frame (stone pillars and arch)
        float archHeight = 4f;
        float archWidth = 2.5f;
        float pillarWidth = 0.4f;

        // Left pillar
        GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPillar.name = "LeftPillar";
        leftPillar.transform.SetParent(portal.transform);
        leftPillar.transform.localPosition = new Vector3(-archWidth / 2, archHeight / 2, 0);
        leftPillar.transform.localScale = new Vector3(pillarWidth, archHeight, pillarWidth);
        leftPillar.GetComponent<Renderer>().sharedMaterial = stoneMat;

        // Right pillar
        GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPillar.name = "RightPillar";
        rightPillar.transform.SetParent(portal.transform);
        rightPillar.transform.localPosition = new Vector3(archWidth / 2, archHeight / 2, 0);
        rightPillar.transform.localScale = new Vector3(pillarWidth, archHeight, pillarWidth);
        rightPillar.GetComponent<Renderer>().sharedMaterial = stoneMat;

        // Top arch (curved using multiple cubes)
        GameObject topArch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topArch.name = "TopArch";
        topArch.transform.SetParent(portal.transform);
        topArch.transform.localPosition = new Vector3(0, archHeight + 0.3f, 0);
        topArch.transform.localScale = new Vector3(archWidth + pillarWidth, 0.6f, pillarWidth);
        topArch.GetComponent<Renderer>().sharedMaterial = stoneMat;

        // Decorative capstones on pillars
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject capstone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            capstone.name = "Capstone";
            capstone.transform.SetParent(portal.transform);
            capstone.transform.localPosition = new Vector3(side * archWidth / 2, archHeight + 0.15f, 0);
            capstone.transform.localScale = new Vector3(pillarWidth + 0.15f, 0.3f, pillarWidth + 0.15f);
            capstone.GetComponent<Renderer>().sharedMaterial = stoneMat;
            Object.DestroyImmediate(capstone.GetComponent<Collider>());
        }

        // Portal surface (the swirling portal itself)
        GameObject portalSurface = GameObject.CreatePrimitive(PrimitiveType.Quad);
        portalSurface.name = "PortalSurface";
        portalSurface.transform.SetParent(portal.transform);
        portalSurface.transform.localPosition = new Vector3(0, archHeight / 2 + 0.3f, 0.05f);
        portalSurface.transform.localScale = new Vector3(archWidth - 0.3f, archHeight - 0.5f, 1);
        portalSurface.GetComponent<Renderer>().sharedMaterial = portalSurfaceMat;
        Object.DestroyImmediate(portalSurface.GetComponent<Collider>());

        // Glowing runes on pillars
        Material runeMat = new Material(Shader.Find("Standard"));
        runeMat.EnableKeyword("_EMISSION");
        runeMat.SetColor("_EmissionColor", portalColor * 1.5f);
        runeMat.color = portalColor;

        // Rune symbols (small glowing spheres along pillars)
        float[] runeHeights = { 1f, 2f, 3f };
        foreach (float h in runeHeights)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject rune = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rune.name = "Rune";
                rune.transform.SetParent(portal.transform);
                rune.transform.localPosition = new Vector3(side * archWidth / 2, h, pillarWidth / 2 + 0.05f);
                rune.transform.localScale = new Vector3(0.12f, 0.12f, 0.05f);
                rune.GetComponent<Renderer>().sharedMaterial = runeMat;
                Object.DestroyImmediate(rune.GetComponent<Collider>());
            }
        }

        // Mystical smoke/particle effect base
        for (int j = 0; j < 5; j++)
        {
            GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.name = "MysticalSmoke";
            smoke.transform.SetParent(portal.transform);
            float xOff = Random.Range(-archWidth / 2 + 0.3f, archWidth / 2 - 0.3f);
            float yOff = Random.Range(0.2f, archHeight);
            smoke.transform.localPosition = new Vector3(xOff, yOff, Random.Range(-0.1f, 0.1f));
            float smokeSize = Random.Range(0.15f, 0.35f);
            smoke.transform.localScale = new Vector3(smokeSize, smokeSize, smokeSize * 0.5f);

            Material smokeMat = new Material(Shader.Find("Standard"));
            smokeMat.SetFloat("_Mode", 3);
            smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            smokeMat.EnableKeyword("_ALPHABLEND_ON");
            smokeMat.color = new Color(portalColor.r, portalColor.g, portalColor.b, 0.3f);
            smokeMat.EnableKeyword("_EMISSION");
            smokeMat.SetColor("_EmissionColor", portalColor * 0.3f);
            smoke.GetComponent<Renderer>().sharedMaterial = smokeMat;
            Object.DestroyImmediate(smoke.GetComponent<Collider>());
        }

        // LOCK symbol if portal is locked (always start locked)
        GameObject lockSymbol = new GameObject("LockSymbol");
        lockSymbol.transform.SetParent(portal.transform);
        lockSymbol.transform.localPosition = new Vector3(0, archHeight / 2 + 0.3f, 0.15f);

        // Lock body
        GameObject lockBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lockBody.name = "LockBody";
        lockBody.transform.SetParent(lockSymbol.transform);
        lockBody.transform.localPosition = new Vector3(0, -0.15f, 0);
        lockBody.transform.localScale = new Vector3(0.5f, 0.4f, 0.15f);
        Material lockMat = new Material(Shader.Find("Standard"));
        lockMat.color = new Color(0.7f, 0.55f, 0.1f);  // Gold
        lockMat.SetFloat("_Metallic", 0.9f);
        lockMat.SetFloat("_Glossiness", 0.8f);
        lockBody.GetComponent<Renderer>().sharedMaterial = lockMat;
        Object.DestroyImmediate(lockBody.GetComponent<Collider>());

        // Lock shackle (arch on top)
        GameObject lockShackle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lockShackle.name = "LockShackle";
        lockShackle.transform.SetParent(lockSymbol.transform);
        lockShackle.transform.localPosition = new Vector3(0, 0.2f, 0);
        lockShackle.transform.localScale = new Vector3(0.35f, 0.15f, 0.15f);
        lockShackle.GetComponent<Renderer>().sharedMaterial = lockMat;
        Object.DestroyImmediate(lockShackle.GetComponent<Collider>());

        // Level requirement text marker (floating above portal)
        GameObject levelMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        levelMarker.name = "LevelMarker";
        levelMarker.transform.SetParent(portal.transform);
        levelMarker.transform.localPosition = new Vector3(0, archHeight + 1.2f, 0);
        levelMarker.transform.localScale = new Vector3(1.2f, 0.4f, 0.1f);
        Material markerMat = new Material(Shader.Find("Standard"));
        markerMat.color = new Color(0.1f, 0.1f, 0.15f);
        levelMarker.GetComponent<Renderer>().sharedMaterial = markerMat;
        Object.DestroyImmediate(levelMarker.GetComponent<Collider>());

        // Add PortalAnimator for swirling effect
        portal.AddComponent<PortalAnimator>();
    }

    static void CreateClothingShopIsland()
    {
        // Clothing shop on the main island (left side)
        GameObject shop = new GameObject("ClothingShopIsland");
        shop.transform.position = new Vector3(-25, 0, -10); // Left side of island

        float groundY = 1.25f; // On the grass floor

        // Palm trees near shop
        CreatePalmTree(shop.transform, new Vector3(-3, groundY, -3));
        CreatePalmTree(shop.transform, new Vector3(5, groundY, 2));

        // Small wooden shop structure
        CreateClothingShopBuilding(shop.transform, new Vector3(0f, groundY, 0));

        // Granny NPC with rocking chair - in front of shop
        CreateGrannyNPC(shop.transform, new Vector3(-3f, groundY, 3));

        // Add clothing shop component
        shop.AddComponent<ClothingShopNPC>();
    }

    static void CreatePalmTree(Transform parent, Vector3 localPos)
    {
        GameObject palmTree = new GameObject("PalmTree");
        palmTree.transform.SetParent(parent);
        palmTree.transform.localPosition = localPos;

        // Trunk - curved slightly
        for (int i = 0; i < 5; i++)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk" + i;
            trunk.transform.SetParent(palmTree.transform);
            float curve = Mathf.Sin(i * 0.3f) * 0.3f;
            trunk.transform.localPosition = new Vector3(curve, i * 1.2f + 0.6f, 0);
            trunk.transform.localScale = new Vector3(0.4f - i * 0.05f, 0.7f, 0.4f - i * 0.05f);
            trunk.transform.localRotation = Quaternion.Euler(0, 0, i * 3);

            Material trunkMat = new Material(Shader.Find("Standard"));
            trunkMat.color = new Color(0.5f, 0.35f, 0.2f);
            trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;
            Object.DestroyImmediate(trunk.GetComponent<Collider>());
        }

        // Palm fronds (leaves)
        Material leafMat = new Material(Shader.Find("Standard"));
        leafMat.color = new Color(0.2f, 0.5f, 0.15f);

        for (int i = 0; i < 8; i++)
        {
            GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frond.name = "Frond" + i;
            frond.transform.SetParent(palmTree.transform);

            float angle = i * 45f;
            float rad = angle * Mathf.Deg2Rad;
            float dropAngle = 30f + Random.Range(0f, 20f);

            frond.transform.localPosition = new Vector3(
                Mathf.Cos(rad) * 1.2f + 0.5f,
                6.2f - Mathf.Abs(Mathf.Sin(rad)) * 0.5f,
                Mathf.Sin(rad) * 1.2f
            );
            frond.transform.localRotation = Quaternion.Euler(dropAngle, angle, 0);
            frond.transform.localScale = new Vector3(0.15f, 0.05f, 2.5f);

            frond.GetComponent<Renderer>().sharedMaterial = leafMat;
            Object.DestroyImmediate(frond.GetComponent<Collider>());
        }

        // Coconuts
        for (int i = 0; i < 3; i++)
        {
            GameObject coconut = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coconut.name = "Coconut" + i;
            coconut.transform.SetParent(palmTree.transform);
            coconut.transform.localPosition = new Vector3(
                0.4f + Random.Range(-0.2f, 0.2f),
                5.8f,
                Random.Range(-0.3f, 0.3f)
            );
            coconut.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f);

            Material cocoMat = new Material(Shader.Find("Standard"));
            cocoMat.color = new Color(0.45f, 0.3f, 0.15f);
            coconut.GetComponent<Renderer>().sharedMaterial = cocoMat;
            Object.DestroyImmediate(coconut.GetComponent<Collider>());
        }
    }

    static void CreateClothingShopBuilding(Transform parent, Vector3 localPos)
    {
        GameObject shop = new GameObject("ClothingShop");
        shop.transform.SetParent(parent);
        shop.transform.localPosition = localPos;

        // Shop base/floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(shop.transform);
        floor.transform.localPosition = new Vector3(0, 0.1f, 0);
        floor.transform.localScale = new Vector3(4, 0.2f, 3);
        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.6f, 0.4f, 0.25f);
        floor.GetComponent<Renderer>().sharedMaterial = woodMat;

        // Back wall
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.SetParent(shop.transform);
        backWall.transform.localPosition = new Vector3(0, 1.5f, -1.4f);
        backWall.transform.localScale = new Vector3(4, 2.8f, 0.15f);
        backWall.GetComponent<Renderer>().sharedMaterial = woodMat;
        Object.DestroyImmediate(backWall.GetComponent<Collider>());

        // Side walls (partial)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject sideWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideWall.name = "SideWall";
            sideWall.transform.SetParent(shop.transform);
            sideWall.transform.localPosition = new Vector3(side * 1.9f, 1.5f, -0.5f);
            sideWall.transform.localScale = new Vector3(0.15f, 2.8f, 2);
            sideWall.GetComponent<Renderer>().sharedMaterial = woodMat;
            Object.DestroyImmediate(sideWall.GetComponent<Collider>());
        }

        // Counter
        GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        counter.name = "Counter";
        counter.transform.SetParent(shop.transform);
        counter.transform.localPosition = new Vector3(0, 0.6f, 0.5f);
        counter.transform.localScale = new Vector3(3, 0.8f, 0.6f);
        Material counterMat = new Material(Shader.Find("Standard"));
        counterMat.color = new Color(0.5f, 0.35f, 0.2f);
        counter.GetComponent<Renderer>().sharedMaterial = counterMat;
        Object.DestroyImmediate(counter.GetComponent<Collider>());

        // Roof (straw/thatch style)
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(shop.transform);
        roof.transform.localPosition = new Vector3(0, 3.2f, 0);
        roof.transform.localScale = new Vector3(5, 0.3f, 4);
        roof.transform.localRotation = Quaternion.Euler(0, 0, 5);
        Material roofMat = new Material(Shader.Find("Standard"));
        roofMat.color = new Color(0.8f, 0.7f, 0.4f);
        roof.GetComponent<Renderer>().sharedMaterial = roofMat;
        Object.DestroyImmediate(roof.GetComponent<Collider>());

        // Sign
        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "Sign";
        sign.transform.SetParent(shop.transform);
        sign.transform.localPosition = new Vector3(0, 3.8f, 0.5f);
        sign.transform.localScale = new Vector3(2.5f, 0.6f, 0.1f);
        Material signMat = new Material(Shader.Find("Standard"));
        signMat.color = new Color(0.4f, 0.25f, 0.15f);
        sign.GetComponent<Renderer>().sharedMaterial = signMat;
        Object.DestroyImmediate(sign.GetComponent<Collider>());

        // Clothing racks (colored boxes to represent clothes)
        Color[] clothColors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta };
        for (int i = 0; i < 5; i++)
        {
            GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = "ClothingDisplay" + i;
            cloth.transform.SetParent(shop.transform);
            cloth.transform.localPosition = new Vector3(-1.5f + i * 0.8f, 1.8f, -1.2f);
            cloth.transform.localScale = new Vector3(0.5f, 0.7f, 0.15f);
            Material clothMat = new Material(Shader.Find("Standard"));
            clothMat.color = clothColors[i];
            cloth.GetComponent<Renderer>().sharedMaterial = clothMat;
            Object.DestroyImmediate(cloth.GetComponent<Collider>());
        }

        // RADIO on the counter - plays Evil Bob's Island music
        GameObject radio = new GameObject("ShopRadio");
        radio.transform.SetParent(shop.transform);
        radio.transform.localPosition = new Vector3(1.0f, 1.1f, 0.5f);  // On the counter
        radio.transform.localRotation = Quaternion.Euler(0, -25f, 0);  // Angled
        radio.AddComponent<ShopRadio>();
    }

    static void CreateGrannyNPC(Transform parent, Vector3 localPos)
    {
        GameObject granny = new GameObject("GrannyNPC");
        granny.transform.SetParent(parent);
        granny.transform.localPosition = localPos;
        granny.transform.localRotation = Quaternion.Euler(0, -45, 0);

        // Rocking chair
        GameObject chair = new GameObject("RockingChair");
        chair.transform.SetParent(granny.transform);
        chair.transform.localPosition = Vector3.zero;

        Material chairMat = new Material(Shader.Find("Standard"));
        chairMat.color = new Color(0.45f, 0.3f, 0.15f);

        // Chair seat
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = "Seat";
        seat.transform.SetParent(chair.transform);
        seat.transform.localPosition = new Vector3(0, 0.4f, 0);
        seat.transform.localScale = new Vector3(0.6f, 0.08f, 0.5f);
        seat.GetComponent<Renderer>().sharedMaterial = chairMat;
        Object.DestroyImmediate(seat.GetComponent<Collider>());

        // Chair back
        GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.name = "Back";
        back.transform.SetParent(chair.transform);
        back.transform.localPosition = new Vector3(0, 0.8f, -0.22f);
        back.transform.localScale = new Vector3(0.6f, 0.8f, 0.08f);
        back.GetComponent<Renderer>().sharedMaterial = chairMat;
        Object.DestroyImmediate(back.GetComponent<Collider>());

        // Rockers
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject rocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rocker.name = "Rocker";
            rocker.transform.SetParent(chair.transform);
            rocker.transform.localPosition = new Vector3(side * 0.28f, 0.1f, 0);
            rocker.transform.localScale = new Vector3(0.06f, 0.15f, 0.7f);
            rocker.transform.localRotation = Quaternion.Euler(15, 0, 0);
            rocker.GetComponent<Renderer>().sharedMaterial = chairMat;
            Object.DestroyImmediate(rocker.GetComponent<Collider>());
        }

        // Granny model
        GameObject npcModel = new GameObject("NPCModel");
        npcModel.transform.SetParent(granny.transform);
        npcModel.transform.localPosition = new Vector3(0, 0.45f, 0);

        Material skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.9f, 0.75f, 0.6f);

        Material dressMat = new Material(Shader.Find("Standard"));
        dressMat.color = new Color(0.6f, 0.3f, 0.5f); // Purple dress

        // Body/dress
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(npcModel.transform);
        body.transform.localPosition = new Vector3(0, 0.3f, 0);
        body.transform.localScale = new Vector3(0.35f, 0.4f, 0.3f);
        body.GetComponent<Renderer>().sharedMaterial = dressMat;
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(npcModel.transform);
        head.transform.localPosition = new Vector3(0, 0.75f, 0);
        head.transform.localScale = new Vector3(0.25f, 0.28f, 0.25f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Hair bun
        GameObject hairBun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hairBun.name = "HairBun";
        hairBun.transform.SetParent(npcModel.transform);
        hairBun.transform.localPosition = new Vector3(0, 0.92f, -0.05f);
        hairBun.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.85f, 0.85f, 0.9f); // Gray/white hair
        hairBun.GetComponent<Renderer>().sharedMaterial = hairMat;
        Object.DestroyImmediate(hairBun.GetComponent<Collider>());

        // Glasses (small rectangles)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lens.name = "Glasses";
            lens.transform.SetParent(npcModel.transform);
            lens.transform.localPosition = new Vector3(side * 0.06f, 0.77f, 0.12f);
            lens.transform.localScale = new Vector3(0.08f, 0.05f, 0.02f);
            Material glassMat = new Material(Shader.Find("Standard"));
            glassMat.color = new Color(0.7f, 0.85f, 1f, 0.5f);
            lens.GetComponent<Renderer>().sharedMaterial = glassMat;
            Object.DestroyImmediate(lens.GetComponent<Collider>());
        }

        // Arms
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            arm.name = side < 0 ? "LeftArm" : "RightArm";
            arm.transform.SetParent(npcModel.transform);
            arm.transform.localPosition = new Vector3(side * 0.22f, 0.35f, 0.1f);
            arm.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
            arm.transform.localRotation = Quaternion.Euler(-30, 0, side * 20);
            arm.GetComponent<Renderer>().sharedMaterial = dressMat;
            Object.DestroyImmediate(arm.GetComponent<Collider>());

            // Hands
            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hand.name = side < 0 ? "LeftHand" : "RightHand";
            hand.transform.SetParent(npcModel.transform);
            hand.transform.localPosition = new Vector3(side * 0.15f, 0.25f, 0.25f);
            hand.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            hand.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(hand.GetComponent<Collider>());
        }

        // Knitting needles
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject needle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            needle.name = "Needle";
            needle.transform.SetParent(npcModel.transform);
            needle.transform.localPosition = new Vector3(side * 0.08f, 0.3f, 0.32f);
            needle.transform.localScale = new Vector3(0.02f, 0.15f, 0.02f);
            needle.transform.localRotation = Quaternion.Euler(60, side * 30, 0);
            Material needleMat = new Material(Shader.Find("Standard"));
            needleMat.color = new Color(0.8f, 0.8f, 0.85f);
            needleMat.SetFloat("_Metallic", 0.8f);
            needle.GetComponent<Renderer>().sharedMaterial = needleMat;
            Object.DestroyImmediate(needle.GetComponent<Collider>());
        }

        // Yarn ball
        GameObject yarn = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        yarn.name = "YarnBall";
        yarn.transform.SetParent(granny.transform);
        yarn.transform.localPosition = new Vector3(0.3f, 0.5f, 0.2f);
        yarn.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        Material yarnMat = new Material(Shader.Find("Standard"));
        yarnMat.color = new Color(0.9f, 0.4f, 0.4f); // Red yarn
        yarn.GetComponent<Renderer>().sharedMaterial = yarnMat;
        Object.DestroyImmediate(yarn.GetComponent<Collider>());
    }

}
