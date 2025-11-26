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

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("=== FISHING GAME READY! Press PLAY, WASD to move, HOLD LEFT CLICK to charge cast! ===");
    }

    static void CleanupScene()
    {
        string[] toDelete = { "Player", "Ground", "Water", "WaterBed", "Dock", "Ramp", "GameManager", "FishingSystem", "UIManager", "Sun", "TreesParent", "LevelingSystem", "QuestSystem", "BottleEventSystem", "QuestNPC", "PortalsParent", "CharacterPanel", "DevPanel", "MainMenu", "ClothingShopIsland", "HorizonBoats", "BirdFlock" };
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
        // Main island where dock is located - FLAT surfaces
        CreateMainIsland();

        // Bridge to clothing shop island
        CreateBridgeToShop();

        // Create scattered smaller islands around the area
        CreateScatteredIslands();
    }

    static void CreateMainIsland()
    {
        GameObject mainIsland = new GameObject("MainIsland");
        mainIsland.transform.position = Vector3.zero;

        Material grassMat = new Material(Shader.Find("Standard"));
        grassMat.color = new Color(0.28f, 0.52f, 0.18f); // Lush green grass

        Material sandMat = new Material(Shader.Find("Standard"));
        sandMat.color = new Color(0.9f, 0.82f, 0.65f); // Light sandy beach

        float groundY = 1.0f; // FLAT ground level - player spawns here

        // === GRASS AREA (where player spawns) - FLAT ===
        GameObject grassArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grassArea.name = "GrassGround";
        grassArea.transform.SetParent(mainIsland.transform);
        grassArea.transform.localPosition = new Vector3(0, groundY - 0.1f, -15);
        grassArea.transform.localScale = new Vector3(40, 0.2f, 30); // Wide flat grass area
        grassArea.GetComponent<Renderer>().sharedMaterial = grassMat;

        // === SAND AREA (transition to water) - FLAT ===
        GameObject sandArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sandArea.name = "SandBeach";
        sandArea.transform.SetParent(mainIsland.transform);
        sandArea.transform.localPosition = new Vector3(0, groundY - 0.15f, 5);
        sandArea.transform.localScale = new Vector3(45, 0.2f, 20); // Beach area
        sandArea.GetComponent<Renderer>().sharedMaterial = sandMat;

        // === BEACH EDGE (slopes gently into water) ===
        GameObject beachEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beachEdge.name = "BeachEdge";
        beachEdge.transform.SetParent(mainIsland.transform);
        beachEdge.transform.localPosition = new Vector3(0, groundY - 0.4f, 18);
        beachEdge.transform.localRotation = Quaternion.Euler(8, 0, 0); // Gentle slope into water
        beachEdge.transform.localScale = new Vector3(40, 0.2f, 12);
        beachEdge.GetComponent<Renderer>().sharedMaterial = sandMat;

        // === GRASS-SAND TRANSITION (smooth edge) ===
        GameObject transition = GameObject.CreatePrimitive(PrimitiveType.Cube);
        transition.name = "GrassSandTransition";
        transition.transform.SetParent(mainIsland.transform);
        transition.transform.localPosition = new Vector3(0, groundY - 0.12f, -2);
        transition.transform.localScale = new Vector3(42, 0.2f, 8);
        // Mix color between grass and sand
        Material transitionMat = new Material(Shader.Find("Standard"));
        transitionMat.color = new Color(0.5f, 0.55f, 0.35f); // Grass-sand mix
        transition.GetComponent<Renderer>().sharedMaterial = transitionMat;

        // === SIDE EXTENSIONS (wider beach) ===
        // Left side beach extension
        GameObject leftBeach = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftBeach.name = "LeftBeach";
        leftBeach.transform.SetParent(mainIsland.transform);
        leftBeach.transform.localPosition = new Vector3(-25, groundY - 0.2f, 10);
        leftBeach.transform.localScale = new Vector3(15, 0.2f, 25);
        leftBeach.GetComponent<Renderer>().sharedMaterial = sandMat;

        // Right side beach extension (towards shop)
        GameObject rightBeach = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightBeach.name = "RightBeach";
        rightBeach.transform.SetParent(mainIsland.transform);
        rightBeach.transform.localPosition = new Vector3(25, groundY - 0.2f, 10);
        rightBeach.transform.localScale = new Vector3(15, 0.2f, 25);
        rightBeach.GetComponent<Renderer>().sharedMaterial = sandMat;

        // Add procedural grass tufts to grass area only
        AddProceduralGrass(mainIsland.transform, new Vector3(0, groundY, -15), 15f, 100);

        // Add some rocks on the beach
        CreateRock(mainIsland.transform, new Vector3(-12, groundY, 8));
        CreateRock(mainIsland.transform, new Vector3(15, groundY, 6));
        CreateRock(mainIsland.transform, new Vector3(-8, groundY, 3));
    }

    static void CreateBridgeToShop()
    {
        // Wooden bridge connecting main island to clothing shop island
        GameObject bridge = new GameObject("BridgeToShop");
        bridge.transform.position = Vector3.zero;

        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.5f, 0.38f, 0.25f); // Brown wood

        Material railMat = new Material(Shader.Find("Standard"));
        railMat.color = new Color(0.4f, 0.3f, 0.2f); // Darker wood for rails

        float bridgeY = 0.85f; // Slightly below ground level
        float bridgeStartX = 28f;
        float bridgeEndX = 55f;
        float bridgeZ = 20f;

        // Main bridge walkway - FLAT
        GameObject walkway = GameObject.CreatePrimitive(PrimitiveType.Cube);
        walkway.name = "BridgeWalkway";
        walkway.transform.SetParent(bridge.transform);
        walkway.transform.localPosition = new Vector3((bridgeStartX + bridgeEndX) / 2, bridgeY, bridgeZ);
        walkway.transform.localScale = new Vector3(bridgeEndX - bridgeStartX, 0.15f, 3f);
        walkway.GetComponent<Renderer>().sharedMaterial = woodMat;

        // Bridge planks (visual detail)
        for (int i = 0; i < 15; i++)
        {
            GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.name = "BridgePlank" + i;
            plank.transform.SetParent(bridge.transform);
            float x = bridgeStartX + 2 + i * 1.8f;
            plank.transform.localPosition = new Vector3(x, bridgeY + 0.08f, bridgeZ);
            plank.transform.localScale = new Vector3(1.6f, 0.05f, 2.8f);
            plank.GetComponent<Renderer>().sharedMaterial = woodMat;
            Object.DestroyImmediate(plank.GetComponent<Collider>()); // Visual only
        }

        // Bridge support posts
        for (int i = 0; i < 5; i++)
        {
            float x = bridgeStartX + 3 + i * 6f;

            // Left post
            GameObject leftPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftPost.name = "BridgePostL" + i;
            leftPost.transform.SetParent(bridge.transform);
            leftPost.transform.localPosition = new Vector3(x, bridgeY - 0.8f, bridgeZ - 1.3f);
            leftPost.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);
            leftPost.GetComponent<Renderer>().sharedMaterial = railMat;
            Object.DestroyImmediate(leftPost.GetComponent<Collider>());

            // Right post
            GameObject rightPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightPost.name = "BridgePostR" + i;
            rightPost.transform.SetParent(bridge.transform);
            rightPost.transform.localPosition = new Vector3(x, bridgeY - 0.8f, bridgeZ + 1.3f);
            rightPost.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);
            rightPost.GetComponent<Renderer>().sharedMaterial = railMat;
            Object.DestroyImmediate(rightPost.GetComponent<Collider>());
        }

        // Railings
        // Left railing
        GameObject leftRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftRail.name = "LeftRailing";
        leftRail.transform.SetParent(bridge.transform);
        leftRail.transform.localPosition = new Vector3((bridgeStartX + bridgeEndX) / 2, bridgeY + 0.5f, bridgeZ - 1.4f);
        leftRail.transform.localScale = new Vector3(bridgeEndX - bridgeStartX - 2, 0.08f, 0.1f);
        leftRail.GetComponent<Renderer>().sharedMaterial = railMat;
        Object.DestroyImmediate(leftRail.GetComponent<Collider>());

        // Right railing
        GameObject rightRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightRail.name = "RightRailing";
        rightRail.transform.SetParent(bridge.transform);
        rightRail.transform.localPosition = new Vector3((bridgeStartX + bridgeEndX) / 2, bridgeY + 0.5f, bridgeZ + 1.4f);
        rightRail.transform.localScale = new Vector3(bridgeEndX - bridgeStartX - 2, 0.08f, 0.1f);
        rightRail.GetComponent<Renderer>().sharedMaterial = railMat;
        Object.DestroyImmediate(rightRail.GetComponent<Collider>());

        // Railing posts
        for (int i = 0; i < 8; i++)
        {
            float x = bridgeStartX + 2 + i * 3.5f;

            GameObject leftRailPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftRailPost.name = "RailPostL" + i;
            leftRailPost.transform.SetParent(bridge.transform);
            leftRailPost.transform.localPosition = new Vector3(x, bridgeY + 0.3f, bridgeZ - 1.4f);
            leftRailPost.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            leftRailPost.GetComponent<Renderer>().sharedMaterial = railMat;
            Object.DestroyImmediate(leftRailPost.GetComponent<Collider>());

            GameObject rightRailPost = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightRailPost.name = "RailPostR" + i;
            rightRailPost.transform.SetParent(bridge.transform);
            rightRailPost.transform.localPosition = new Vector3(x, bridgeY + 0.3f, bridgeZ + 1.4f);
            rightRailPost.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            rightRailPost.GetComponent<Renderer>().sharedMaterial = railMat;
            Object.DestroyImmediate(rightRailPost.GetComponent<Collider>());
        }
    }

    static void CreateScatteredIslands()
    {
        // Small islands scattered around - all with FLAT walkable surfaces
        Vector3[] islandPositions = new Vector3[]
        {
            new Vector3(-45, 0, 35),   // Left side
            new Vector3(-35, 0, 55),   // Far left
            new Vector3(0, 0, 50),     // Center far
        };

        float[] islandSizes = new float[] { 10f, 8f, 12f };

        for (int i = 0; i < islandPositions.Length; i++)
        {
            CreateSmallIsland(islandPositions[i], islandSizes[i], "Island_" + i);
        }
    }

    static void CreateSmallIsland(Vector3 position, float size, string name)
    {
        GameObject island = new GameObject(name);
        island.transform.position = position;

        Material sandMat = new Material(Shader.Find("Standard"));
        sandMat.color = new Color(0.9f, 0.82f, 0.65f);

        Material grassMat = new Material(Shader.Find("Standard"));
        grassMat.color = new Color(0.3f, 0.55f, 0.2f);

        float groundY = 0.9f;

        // Island base - FLAT cube instead of cylinder
        GameObject islandBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        islandBase.name = "Base";
        islandBase.transform.SetParent(island.transform);
        islandBase.transform.localPosition = new Vector3(0, groundY - 0.1f, 0);
        islandBase.transform.localScale = new Vector3(size, 0.2f, size);
        islandBase.GetComponent<Renderer>().sharedMaterial = sandMat;

        // Grassy center - FLAT
        GameObject grassTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grassTop.name = "GrassTop";
        grassTop.transform.SetParent(island.transform);
        grassTop.transform.localPosition = new Vector3(0, groundY - 0.05f, 0);
        grassTop.transform.localScale = new Vector3(size * 0.7f, 0.15f, size * 0.7f);
        grassTop.GetComponent<Renderer>().sharedMaterial = grassMat;

        // Add some grass tufts
        int grassCount = Mathf.RoundToInt(size * 2);
        AddProceduralGrass(island.transform, new Vector3(0, groundY, 0), size * 0.3f, grassCount);

        // Maybe add a palm tree
        if (Random.value > 0.3f)
        {
            CreateSimplePalmTree(island.transform, new Vector3(Random.Range(-size * 0.2f, size * 0.2f), groundY, Random.Range(-size * 0.2f, size * 0.2f)));
        }
    }

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
        // Large water plane covering entire play area
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Water";
        water.transform.position = new Vector3(0, 0.1f, 40);  // Water level below ground
        water.transform.localScale = new Vector3(25, 1, 25);
        water.AddComponent<WaterEffect>();

        // Water bed (sandy bottom visible through clear water)
        GameObject waterBed = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterBed.name = "WaterBed";
        waterBed.transform.position = new Vector3(0, -2f, 40);
        waterBed.transform.localScale = new Vector3(25, 1, 25);
        Material bedMat = new Material(Shader.Find("Standard"));
        bedMat.color = new Color(0.65f, 0.58f, 0.45f); // Sandy bottom color
        waterBed.GetComponent<Renderer>().sharedMaterial = bedMat;
    }

    static void CreateDock()
    {
        GameObject dockParent = new GameObject("Dock");
        dockParent.transform.position = Vector3.zero;

        Material woodMat = MaterialGenerator.CreateWoodMaterial();

        // Dock dimensions - extends from land (z=-2) into water (z=18)
        // Water starts around z=5, so half the dock (z=5 to z=18) is over water
        float dockStartZ = -2f;
        float dockEndZ = 18f;
        float dockWidth = 5f;
        float dockHeight = 1.2f;
        float plankWidth = 0.6f;

        int numPlanks = Mathf.CeilToInt((dockEndZ - dockStartZ) / plankWidth);

        // Create individual planks
        for (int i = 0; i < numPlanks; i++)
        {
            GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.name = "Plank";
            plank.transform.SetParent(dockParent.transform);

            float z = dockStartZ + i * plankWidth + plankWidth * 0.5f;
            plank.transform.position = new Vector3(0, dockHeight, z);
            plank.transform.localScale = new Vector3(dockWidth, 0.12f, plankWidth - 0.04f);

            // Slight variation for natural look
            plank.transform.rotation = Quaternion.Euler(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.2f, 0.2f)
            );

            plank.GetComponent<Renderer>().sharedMaterial = woodMat;
        }

        // Support structure
        Material darkWood = new Material(Shader.Find("Standard"));
        darkWood.color = new Color(0.22f, 0.14f, 0.08f);

        // Main support beams running length of dock
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "SupportBeam";
            beam.transform.SetParent(dockParent.transform);
            beam.transform.position = new Vector3(side * 2f, dockHeight - 0.2f, (dockStartZ + dockEndZ) / 2f);
            beam.transform.localScale = new Vector3(0.25f, 0.25f, dockEndZ - dockStartZ);
            beam.GetComponent<Renderer>().sharedMaterial = darkWood;
        }

        // Vertical support posts (pilings)
        float[] postPositions = { 0f, 5f, 10f, 15f };
        foreach (float zPos in postPositions)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "DockPost";
                post.transform.SetParent(dockParent.transform);

                float postHeight = zPos > 4f ? 2.5f : 1.5f;  // Taller posts in water
                float postY = zPos > 4f ? -0.5f : 0f;

                post.transform.position = new Vector3(side * 1.8f, postY, zPos);
                post.transform.localScale = new Vector3(0.2f, postHeight, 0.2f);
                post.GetComponent<Renderer>().sharedMaterial = darkWood;
            }
        }

        // Cross braces for structure
        for (int i = 0; i < 3; i++)
        {
            GameObject brace = GameObject.CreatePrimitive(PrimitiveType.Cube);
            brace.name = "CrossBrace";
            brace.transform.SetParent(dockParent.transform);
            brace.transform.position = new Vector3(0, dockHeight - 0.3f, 2f + i * 5f);
            brace.transform.localScale = new Vector3(4f, 0.15f, 0.15f);
            brace.GetComponent<Renderer>().sharedMaterial = darkWood;
        }

        // Decorative rope coil
        GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.name = "RopeCoil";
        rope.transform.SetParent(dockParent.transform);
        rope.transform.position = new Vector3(2f, dockHeight + 0.15f, 1f);
        rope.transform.localScale = new Vector3(0.4f, 0.12f, 0.4f);
        Material ropeMat = new Material(Shader.Find("Standard"));
        ropeMat.color = new Color(0.55f, 0.45f, 0.30f);
        rope.GetComponent<Renderer>().sharedMaterial = ropeMat;
    }

    static void CreateRamp()
    {
        Material woodMat = MaterialGenerator.CreateWoodMaterial();

        // Ramp from ground to dock
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Ramp";
        ramp.transform.position = new Vector3(0, 0.6f, -4f);
        ramp.transform.localScale = new Vector3(3.5f, 0.15f, 4f);
        ramp.transform.rotation = Quaternion.Euler(-18, 0, 0);
        ramp.GetComponent<Renderer>().sharedMaterial = woodMat;

        // Ramp side rails
        Material railMat = new Material(Shader.Find("Standard"));
        railMat.color = new Color(0.28f, 0.18f, 0.10f);

        for (int side = -1; side <= 1; side += 2)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "RampRail";
            rail.transform.position = new Vector3(side * 1.6f, 0.8f, -4f);
            rail.transform.localScale = new Vector3(0.08f, 0.5f, 4.2f);
            rail.transform.rotation = Quaternion.Euler(-18, 0, 0);
            rail.GetComponent<Renderer>().sharedMaterial = railMat;
        }
    }

    static void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0, 2f, -5f);  // Start on beach side of dock (facing water)
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
        Material shirtMat = MaterialGenerator.CreateFabricMaterial(new Color(0.15f, 0.30f, 0.55f)); // Blue shirt
        Material pantsMat = MaterialGenerator.CreateFabricMaterial(new Color(0.22f, 0.18f, 0.12f)); // Brown pants

        // TORSO (upper body with shirt)
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "Torso";
        torso.transform.SetParent(player.transform);
        torso.transform.localPosition = new Vector3(0, 0.15f, 0);
        torso.transform.localScale = new Vector3(0.45f, 0.55f, 0.25f);
        torso.GetComponent<Renderer>().sharedMaterial = shirtMat;
        Object.DestroyImmediate(torso.GetComponent<Collider>());

        // HIPS/WAIST
        GameObject hips = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hips.name = "Hips";
        hips.transform.SetParent(player.transform);
        hips.transform.localPosition = new Vector3(0, -0.2f, 0);
        hips.transform.localScale = new Vector3(0.40f, 0.25f, 0.22f);
        hips.GetComponent<Renderer>().sharedMaterial = pantsMat;
        Object.DestroyImmediate(hips.GetComponent<Collider>());

        // LEFT LEG
        CreateLeg(player, pantsMat, -0.12f);
        // RIGHT LEG
        CreateLeg(player, pantsMat, 0.12f);

        // LEFT ARM
        CreateArm(player, skinMat, shirtMat, -0.30f);
        // RIGHT ARM
        CreateArm(player, skinMat, shirtMat, 0.30f);

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

        // FISHING HAT
        CreateFishingHat(head);

        // FISHING ROD
        CreateFishingRod(player);
    }

    static void CreateLeg(GameObject player, Material pantsMat, float xOffset)
    {
        // Upper leg
        GameObject upperLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        upperLeg.name = "UpperLeg";
        upperLeg.transform.SetParent(player.transform);
        upperLeg.transform.localPosition = new Vector3(xOffset, -0.50f, 0);
        upperLeg.transform.localScale = new Vector3(0.14f, 0.22f, 0.14f);
        upperLeg.GetComponent<Renderer>().sharedMaterial = pantsMat;
        Object.DestroyImmediate(upperLeg.GetComponent<Collider>());

        // Lower leg
        GameObject lowerLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        lowerLeg.name = "LowerLeg";
        lowerLeg.transform.SetParent(player.transform);
        lowerLeg.transform.localPosition = new Vector3(xOffset, -0.78f, 0);
        lowerLeg.transform.localScale = new Vector3(0.11f, 0.20f, 0.11f);
        lowerLeg.GetComponent<Renderer>().sharedMaterial = pantsMat;
        Object.DestroyImmediate(lowerLeg.GetComponent<Collider>());

        // Foot/boot
        Material bootMat = new Material(Shader.Find("Standard"));
        bootMat.color = new Color(0.15f, 0.12f, 0.08f);

        GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        foot.name = "Foot";
        foot.transform.SetParent(player.transform);
        foot.transform.localPosition = new Vector3(xOffset, -0.95f, 0.03f);
        foot.transform.localScale = new Vector3(0.10f, 0.08f, 0.18f);
        foot.GetComponent<Renderer>().sharedMaterial = bootMat;
        Object.DestroyImmediate(foot.GetComponent<Collider>());
    }

    static void CreateArm(GameObject player, Material skinMat, Material shirtMat, float xOffset)
    {
        // Both arms reach forward to hold the fishing rod with two hands
        bool isRightArm = xOffset > 0;

        // Shoulder - at body sides
        GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shoulder.name = isRightArm ? "RightShoulder" : "LeftShoulder";
        shoulder.transform.SetParent(player.transform);
        shoulder.transform.localPosition = new Vector3(xOffset * 0.8f, 0.35f, 0.02f);
        shoulder.transform.localScale = new Vector3(0.13f, 0.11f, 0.11f);
        shoulder.GetComponent<Renderer>().sharedMaterial = shirtMat;
        Object.DestroyImmediate(shoulder.GetComponent<Collider>());

        // Upper arm - angled forward and inward toward center where rod is held
        GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        upperArm.name = isRightArm ? "RightUpperArm" : "LeftUpperArm";
        upperArm.transform.SetParent(player.transform);
        // Both arms angle forward and slightly down, converging toward rod
        upperArm.transform.localPosition = new Vector3(xOffset * 0.55f, 0.18f, 0.15f);
        upperArm.transform.localRotation = Quaternion.Euler(60, isRightArm ? -15 : 15, isRightArm ? -20 : 20);
        upperArm.transform.localScale = new Vector3(0.09f, 0.16f, 0.09f);
        upperArm.GetComponent<Renderer>().sharedMaterial = shirtMat;
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

        // Trees on the left side of the dock
        for (int i = 0; i < 6; i++)
        {
            float x = Random.Range(-35f, -10f);
            float z = Random.Range(-30f, 5f);
            CreateRealisticTree(treesParent.transform, new Vector3(x, 0, z));
        }

        // Trees on the right side
        for (int i = 0; i < 6; i++)
        {
            float x = Random.Range(10f, 35f);
            float z = Random.Range(-30f, 5f);
            CreateRealisticTree(treesParent.transform, new Vector3(x, 0, z));
        }

        // Trees in the background
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(-40f, 40f);
            float z = Random.Range(-45f, -25f);
            CreateRealisticTree(treesParent.transform, new Vector3(x, 0, z));
        }
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

        // TRUNK - tapers from bottom to top
        // Lower trunk section
        GameObject lowerTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lowerTrunk.name = "LowerTrunk";
        lowerTrunk.transform.SetParent(tree.transform);
        lowerTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.2f, 0);
        lowerTrunk.transform.localScale = new Vector3(trunkBaseRadius, treeHeight * 0.2f, trunkBaseRadius);
        lowerTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;

        // Middle trunk (slightly thinner)
        GameObject midTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        midTrunk.name = "MidTrunk";
        midTrunk.transform.SetParent(tree.transform);
        midTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.45f, 0);
        midTrunk.transform.localScale = new Vector3(trunkBaseRadius * 0.75f, treeHeight * 0.15f, trunkBaseRadius * 0.75f);
        midTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;

        // Upper trunk (thinner still)
        GameObject upperTrunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        upperTrunk.name = "UpperTrunk";
        upperTrunk.transform.SetParent(tree.transform);
        upperTrunk.transform.localPosition = new Vector3(0, treeHeight * 0.6f, 0);
        upperTrunk.transform.localScale = new Vector3(trunkBaseRadius * 0.5f, treeHeight * 0.1f, trunkBaseRadius * 0.5f);
        upperTrunk.GetComponent<Renderer>().sharedMaterial = barkMat;

        // BRANCHES - several main branches coming off trunk
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

            // Move branch outward along its direction
            branch.transform.localPosition += branch.transform.up * branchLength * 0.4f;
        }

        // LEAF CANOPY - layered clusters for natural look
        float canopyBaseHeight = treeHeight * 0.55f;
        float canopyRadius = treeHeight * 0.35f;

        // Main central canopy mass
        GameObject mainCanopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mainCanopy.name = "MainCanopy";
        mainCanopy.transform.SetParent(tree.transform);
        mainCanopy.transform.localPosition = new Vector3(0, treeHeight * 0.75f, 0);
        mainCanopy.transform.localScale = new Vector3(canopyRadius * 1.8f, canopyRadius * 1.5f, canopyRadius * 1.8f);
        mainCanopy.GetComponent<Renderer>().sharedMaterial = leavesMat;

        // Lower canopy clusters (spreading out)
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
        }

        // Top crown
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        crown.name = "Crown";
        crown.transform.SetParent(tree.transform);
        crown.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), treeHeight * 0.9f, Random.Range(-0.3f, 0.3f));
        crown.transform.localScale = new Vector3(canopyRadius * 0.9f, canopyRadius * 1.1f, canopyRadius * 0.9f);
        crown.GetComponent<Renderer>().sharedMaterial = leavesMat;
    }

    static void CreateQuestNPC()
    {
        // WETSUIT PETE - Quest NPC wearing black wetsuit with snorkel
        GameObject npc = new GameObject("QuestNPC");
        npc.transform.position = new Vector3(-2f, 2f, 3f);  // Standing on dock, to the side
        npc.transform.rotation = Quaternion.Euler(0, 45, 0);  // Facing toward player area

        // Add NPC interaction component
        NPCInteraction npcInteraction = npc.AddComponent<NPCInteraction>();
        npcInteraction.npcName = "Wetsuit Pete";
        npcInteraction.interactionRange = 4f;

        // Materials
        Material skinMat = MaterialGenerator.CreateSkinMaterial();
        Material wetsuitMat = new Material(Shader.Find("Standard"));
        wetsuitMat.color = new Color(0.05f, 0.05f, 0.08f); // Black wetsuit
        wetsuitMat.SetFloat("_Glossiness", 0.7f); // Shiny rubber look

        Material wetsuitAccentMat = new Material(Shader.Find("Standard"));
        wetsuitAccentMat.color = new Color(0.1f, 0.1f, 0.15f); // Slightly lighter accent

        // Torso - wetsuit
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "NPCTorso";
        torso.transform.SetParent(npc.transform);
        torso.transform.localPosition = new Vector3(0, 0.15f, 0);
        torso.transform.localScale = new Vector3(0.45f, 0.55f, 0.25f);
        torso.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
        Object.DestroyImmediate(torso.GetComponent<Collider>());

        // Wetsuit zipper stripe
        GameObject zipper = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zipper.transform.SetParent(torso.transform);
        zipper.transform.localPosition = new Vector3(0, 0, 0.51f);
        zipper.transform.localScale = new Vector3(0.1f, 0.9f, 0.02f);
        Material zipperMat = new Material(Shader.Find("Standard"));
        zipperMat.color = new Color(0.4f, 0.4f, 0.45f);
        zipperMat.SetFloat("_Metallic", 0.8f);
        zipper.GetComponent<Renderer>().sharedMaterial = zipperMat;
        Object.DestroyImmediate(zipper.GetComponent<Collider>());

        // Hips - wetsuit
        GameObject hips = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hips.name = "NPCHips";
        hips.transform.SetParent(npc.transform);
        hips.transform.localPosition = new Vector3(0, -0.2f, 0);
        hips.transform.localScale = new Vector3(0.40f, 0.25f, 0.22f);
        hips.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
        Object.DestroyImmediate(hips.GetComponent<Collider>());

        // Legs - wetsuit
        for (float xOffset = -0.12f; xOffset <= 0.12f; xOffset += 0.24f)
        {
            GameObject upperLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperLeg.transform.SetParent(npc.transform);
            upperLeg.transform.localPosition = new Vector3(xOffset, -0.50f, 0);
            upperLeg.transform.localScale = new Vector3(0.14f, 0.22f, 0.14f);
            upperLeg.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(upperLeg.GetComponent<Collider>());

            GameObject lowerLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lowerLeg.transform.SetParent(npc.transform);
            lowerLeg.transform.localPosition = new Vector3(xOffset, -0.78f, 0);
            lowerLeg.transform.localScale = new Vector3(0.11f, 0.20f, 0.11f);
            lowerLeg.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(lowerLeg.GetComponent<Collider>());

            // Diving booties (black rubber)
            Material bootyMat = new Material(Shader.Find("Standard"));
            bootyMat.color = new Color(0.02f, 0.02f, 0.02f);
            bootyMat.SetFloat("_Glossiness", 0.8f);
            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foot.transform.SetParent(npc.transform);
            foot.transform.localPosition = new Vector3(xOffset, -0.95f, 0.03f);
            foot.transform.localScale = new Vector3(0.10f, 0.08f, 0.18f);
            foot.GetComponent<Renderer>().sharedMaterial = bootyMat;
            Object.DestroyImmediate(foot.GetComponent<Collider>());
        }

        // Arms - wetsuit sleeves
        for (float xOffset = -0.30f; xOffset <= 0.30f; xOffset += 0.60f)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shoulder.transform.SetParent(npc.transform);
            shoulder.transform.localPosition = new Vector3(xOffset, 0.35f, 0);
            shoulder.transform.localScale = new Vector3(0.12f, 0.10f, 0.10f);
            shoulder.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(shoulder.GetComponent<Collider>());

            GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperArm.transform.SetParent(npc.transform);
            upperArm.transform.localPosition = new Vector3(xOffset * 1.15f, 0.18f, 0);
            upperArm.transform.localScale = new Vector3(0.09f, 0.15f, 0.09f);
            upperArm.transform.localRotation = Quaternion.Euler(0, 0, xOffset > 0 ? -30 : 30);
            upperArm.GetComponent<Renderer>().sharedMaterial = wetsuitMat;
            Object.DestroyImmediate(upperArm.GetComponent<Collider>());

            GameObject lowerArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lowerArm.transform.SetParent(npc.transform);
            lowerArm.transform.localPosition = new Vector3(xOffset * 0.5f, 0.02f, 0.12f);
            lowerArm.transform.localScale = new Vector3(0.07f, 0.14f, 0.07f);
            lowerArm.transform.localRotation = Quaternion.Euler(60, 0, 0);
            lowerArm.GetComponent<Renderer>().sharedMaterial = wetsuitAccentMat;
            Object.DestroyImmediate(lowerArm.GetComponent<Collider>());

            // Bare hands
            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hand.transform.SetParent(npc.transform);
            hand.transform.localPosition = new Vector3(xOffset * 0.3f, 0.05f, 0.22f);
            hand.transform.localScale = new Vector3(0.08f, 0.10f, 0.05f);
            hand.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(hand.GetComponent<Collider>());
        }

        // Neck - visible above wetsuit
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.transform.SetParent(npc.transform);
        neck.transform.localPosition = new Vector3(0, 0.5f, 0);
        neck.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
        neck.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(neck.GetComponent<Collider>());

        // Head with detailed features
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "NPCHead";
        head.transform.SetParent(npc.transform);
        head.transform.localPosition = new Vector3(0, 0.72f, 0);
        head.transform.localScale = new Vector3(0.26f, 0.30f, 0.26f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Eyes with detail
        for (float ex = -0.06f; ex <= 0.06f; ex += 0.12f)
        {
            // Eye socket shadow
            GameObject eyeSocket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeSocket.transform.SetParent(head.transform);
            eyeSocket.transform.localPosition = new Vector3(ex, 0.05f, 0.42f);
            eyeSocket.transform.localScale = new Vector3(0.18f, 0.12f, 0.08f);
            Material socketMat = new Material(Shader.Find("Standard"));
            socketMat.color = new Color(0.7f, 0.55f, 0.45f);
            eyeSocket.GetComponent<Renderer>().sharedMaterial = socketMat;
            Object.DestroyImmediate(eyeSocket.GetComponent<Collider>());

            GameObject eyeWhite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeWhite.transform.SetParent(head.transform);
            eyeWhite.transform.localPosition = new Vector3(ex, 0.05f, 0.45f);
            eyeWhite.transform.localScale = new Vector3(0.14f, 0.10f, 0.08f);
            Material whiteMat = new Material(Shader.Find("Standard"));
            whiteMat.color = Color.white;
            eyeWhite.GetComponent<Renderer>().sharedMaterial = whiteMat;
            Object.DestroyImmediate(eyeWhite.GetComponent<Collider>());

            GameObject iris = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            iris.transform.SetParent(eyeWhite.transform);
            iris.transform.localPosition = new Vector3(0, 0, 0.35f);
            iris.transform.localScale = new Vector3(0.55f, 0.65f, 0.25f);
            Material irisMat = new Material(Shader.Find("Standard"));
            irisMat.color = new Color(0.2f, 0.5f, 0.6f);  // Blue eyes
            iris.GetComponent<Renderer>().sharedMaterial = irisMat;
            Object.DestroyImmediate(iris.GetComponent<Collider>());

            // Pupil
            GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.transform.SetParent(iris.transform);
            pupil.transform.localPosition = new Vector3(0, 0, 0.3f);
            pupil.transform.localScale = new Vector3(0.5f, 0.5f, 0.3f);
            Material pupilMat = new Material(Shader.Find("Standard"));
            pupilMat.color = Color.black;
            pupil.GetComponent<Renderer>().sharedMaterial = pupilMat;
            Object.DestroyImmediate(pupil.GetComponent<Collider>());

            // Eyebrow
            GameObject eyebrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eyebrow.transform.SetParent(head.transform);
            eyebrow.transform.localPosition = new Vector3(ex, 0.18f, 0.44f);
            eyebrow.transform.localScale = new Vector3(0.18f, 0.04f, 0.08f);
            eyebrow.transform.localRotation = Quaternion.Euler(0, 0, ex > 0 ? -8 : 8);
            Material browMat = new Material(Shader.Find("Standard"));
            browMat.color = new Color(0.35f, 0.25f, 0.15f);
            eyebrow.GetComponent<Renderer>().sharedMaterial = browMat;
            Object.DestroyImmediate(eyebrow.GetComponent<Collider>());
        }

        // Nose
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.transform.SetParent(head.transform);
        nose.transform.localPosition = new Vector3(0, -0.05f, 0.48f);
        nose.transform.localScale = new Vector3(0.08f, 0.15f, 0.12f);
        nose.transform.localRotation = Quaternion.Euler(-10, 0, 0);
        nose.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(nose.GetComponent<Collider>());

        // Nose tip
        GameObject noseTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        noseTip.transform.SetParent(head.transform);
        noseTip.transform.localPosition = new Vector3(0, -0.12f, 0.52f);
        noseTip.transform.localScale = new Vector3(0.10f, 0.08f, 0.08f);
        noseTip.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(noseTip.GetComponent<Collider>());

        // Ears
        for (float ex = -0.48f; ex <= 0.48f; ex += 0.96f)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ear.transform.SetParent(head.transform);
            ear.transform.localPosition = new Vector3(ex, 0f, 0f);
            ear.transform.localScale = new Vector3(0.12f, 0.18f, 0.10f);
            ear.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(ear.GetComponent<Collider>());
        }

        // SNORKEL in mouth!
        Material snorkelMat = new Material(Shader.Find("Standard"));
        snorkelMat.color = new Color(0.1f, 0.4f, 0.6f); // Blue snorkel
        snorkelMat.SetFloat("_Glossiness", 0.8f);

        // Snorkel mouthpiece
        GameObject mouthpiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mouthpiece.transform.SetParent(head.transform);
        mouthpiece.transform.localPosition = new Vector3(0, -0.22f, 0.45f);
        mouthpiece.transform.localScale = new Vector3(0.12f, 0.06f, 0.15f);
        mouthpiece.GetComponent<Renderer>().sharedMaterial = snorkelMat;
        Object.DestroyImmediate(mouthpiece.GetComponent<Collider>());

        // Snorkel tube going up the side
        GameObject snorkelTube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        snorkelTube.transform.SetParent(head.transform);
        snorkelTube.transform.localPosition = new Vector3(0.35f, 0.1f, 0.25f);
        snorkelTube.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
        snorkelTube.transform.localRotation = Quaternion.Euler(0, 0, 15);
        snorkelTube.GetComponent<Renderer>().sharedMaterial = snorkelMat;
        Object.DestroyImmediate(snorkelTube.GetComponent<Collider>());

        // Snorkel top curve
        GameObject snorkelTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        snorkelTop.transform.SetParent(head.transform);
        snorkelTop.transform.localPosition = new Vector3(0.42f, 0.45f, 0.15f);
        snorkelTop.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
        snorkelTop.transform.localRotation = Quaternion.Euler(30, 0, 0);
        snorkelTop.GetComponent<Renderer>().sharedMaterial = snorkelMat;
        Object.DestroyImmediate(snorkelTop.GetComponent<Collider>());

        // Snorkel splash guard
        GameObject splashGuard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        splashGuard.transform.SetParent(head.transform);
        splashGuard.transform.localPosition = new Vector3(0.42f, 0.55f, 0.05f);
        splashGuard.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
        Material guardMat = new Material(Shader.Find("Standard"));
        guardMat.color = new Color(0.9f, 0.3f, 0.1f); // Orange splash guard
        splashGuard.GetComponent<Renderer>().sharedMaterial = guardMat;
        Object.DestroyImmediate(splashGuard.GetComponent<Collider>());

        // Short stubble beard (5 o'clock shadow)
        GameObject stubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stubble.transform.SetParent(head.transform);
        stubble.transform.localPosition = new Vector3(0, -0.28f, 0.25f);
        stubble.transform.localScale = new Vector3(0.30f, 0.18f, 0.20f);
        Material stubbleMat = new Material(Shader.Find("Standard"));
        stubbleMat.color = new Color(0.5f, 0.4f, 0.35f);
        stubble.GetComponent<Renderer>().sharedMaterial = stubbleMat;
        Object.DestroyImmediate(stubble.GetComponent<Collider>());

        // Short wet hair
        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.25f, 0.18f, 0.1f); // Dark brown wet hair
        hairMat.SetFloat("_Glossiness", 0.7f);

        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.transform.SetParent(head.transform);
        hair.transform.localPosition = new Vector3(0, 0.35f, -0.05f);
        hair.transform.localScale = new Vector3(1.05f, 0.4f, 1.0f);
        hair.GetComponent<Renderer>().sharedMaterial = hairMat;
        Object.DestroyImmediate(hair.GetComponent<Collider>());

        // Quest marker above head (yellow exclamation mark effect)
        GameObject questMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        questMarker.name = "QuestMarker";
        questMarker.transform.SetParent(npc.transform);
        questMarker.transform.localPosition = new Vector3(0, 1.3f, 0);
        questMarker.transform.localScale = new Vector3(0.2f, 0.25f, 0.2f);
        Material markerMat = new Material(Shader.Find("Standard"));
        markerMat.color = new Color(1f, 0.9f, 0f);
        markerMat.EnableKeyword("_EMISSION");
        markerMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0f) * 0.5f);
        questMarker.GetComponent<Renderer>().sharedMaterial = markerMat;
        Object.DestroyImmediate(questMarker.GetComponent<Collider>());

        // Exclamation mark stick
        GameObject markerStick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        markerStick.transform.SetParent(npc.transform);
        markerStick.transform.localPosition = new Vector3(0, 1.1f, 0);
        markerStick.transform.localScale = new Vector3(0.06f, 0.12f, 0.06f);
        markerStick.GetComponent<Renderer>().sharedMaterial = markerMat;
        Object.DestroyImmediate(markerStick.GetComponent<Collider>());
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
        // Large island with clothing shop - FLAT surfaces, connected by bridge
        GameObject island = new GameObject("ClothingShopIsland");
        island.transform.position = new Vector3(60, 0, 20); // Position to match bridge end

        Material sandMat = new Material(Shader.Find("Standard"));
        sandMat.color = new Color(0.9f, 0.82f, 0.65f);

        Material grassMat = new Material(Shader.Find("Standard"));
        grassMat.color = new Color(0.3f, 0.55f, 0.2f);

        float groundY = 0.85f; // Match bridge height

        // === MAIN ISLAND BASE - FLAT ===
        GameObject islandBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        islandBase.name = "IslandBase";
        islandBase.transform.SetParent(island.transform);
        islandBase.transform.localPosition = new Vector3(0, groundY - 0.1f, 0);
        islandBase.transform.localScale = new Vector3(22, 0.2f, 18);
        islandBase.GetComponent<Renderer>().sharedMaterial = sandMat;

        // === GRASSY CENTER - FLAT ===
        GameObject grassTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grassTop.name = "GrassTop";
        grassTop.transform.SetParent(island.transform);
        grassTop.transform.localPosition = new Vector3(2, groundY - 0.05f, 0);
        grassTop.transform.localScale = new Vector3(16, 0.15f, 14);
        grassTop.GetComponent<Renderer>().sharedMaterial = grassMat;

        // === BRIDGE LANDING AREA - connects to bridge ===
        GameObject bridgeLanding = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridgeLanding.name = "BridgeLanding";
        bridgeLanding.transform.SetParent(island.transform);
        bridgeLanding.transform.localPosition = new Vector3(-10, groundY - 0.1f, 0);
        bridgeLanding.transform.localScale = new Vector3(6, 0.2f, 6);
        bridgeLanding.GetComponent<Renderer>().sharedMaterial = sandMat;

        // Add procedural grass
        AddProceduralGrass(island.transform, new Vector3(2, groundY, 0), 6f, 50);

        // Palm trees
        CreatePalmTree(island.transform, new Vector3(-3, groundY, -5));
        CreatePalmTree(island.transform, new Vector3(6, groundY, 4));

        // Small wooden shop structure
        CreateClothingShopBuilding(island.transform, new Vector3(4f, groundY, -2));

        // Granny NPC with rocking chair - in front of shop
        CreateGrannyNPC(island.transform, new Vector3(0f, groundY, 2));

        // Add clothing shop component
        island.AddComponent<ClothingShopNPC>();

        // Add some decorative rocks
        CreateRock(island.transform, new Vector3(-4, groundY, 5));
        CreateRock(island.transform, new Vector3(7, groundY, -4));
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
