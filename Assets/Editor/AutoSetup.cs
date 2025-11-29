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

    [MenuItem("Fishing Game/Reset Scene View Camera")]
    static void ResetSceneViewCamera()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            // Position camera to see the main game area
            sceneView.pivot = new Vector3(0, 5, 0);
            sceneView.rotation = Quaternion.Euler(45, 0, 0);
            sceneView.size = 30f;
            sceneView.Repaint();
            Debug.Log("Scene view camera reset to game area");
        }
    }

    [MenuItem("Fishing Game/Focus On Player")]
    static void FocusOnPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.pivot = player.transform.position;
                sceneView.rotation = Quaternion.Euler(45, 0, 0);
                sceneView.size = 15f;
                sceneView.Repaint();
                Debug.Log("Scene view focused on player");
            }
        }
        else
        {
            Debug.LogWarning("No Player found in scene");
        }
    }

    [MenuItem("Fishing Game/Setup Scene Now")]
    static void RunSetup()
    {
        // Reset scene view camera first
        ResetSceneViewCamera();

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

        // Pause Menu (ESC to open - Save/Load/Quit)
        GameObject pauseMenu = new GameObject("PauseMenu");
        pauseMenu.AddComponent<PauseMenu>();

        // Game Systems
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        GameObject fs = new GameObject("FishingSystem");
        fs.AddComponent<FishingSystem>();

        // Tutorial Cat (appears at level 2 with tip)
        GameObject tutCat = new GameObject("TutorialCat");
        tutCat.AddComponent<TutorialCat>();

        // Fish Sprites (pixel art for inventory)
        GameObject fishSprites = new GameObject("FishSprites");
        fishSprites.AddComponent<FishSprites>();

        GameObject rodSprites = new GameObject("RodSprites");
        rodSprites.AddComponent<RodSprites>();

        // Clothing Sprites (pixel art for shop/wardrobe)
        GameObject clothingSprites = new GameObject("ClothingSprites");
        clothingSprites.AddComponent<ClothingSprites>();

        // Leveling System (OSRS-style, level 1-399, 100M XP cap)
        GameObject ls = new GameObject("LevelingSystem");
        ls.AddComponent<LevelingSystem>();

        // Quest System
        GameObject qs = new GameObject("QuestSystem");
        qs.AddComponent<QuestSystem>();

        // Realm Manager (handles portal teleportation between realms)
        GameObject rm = new GameObject("RealmManager");
        rm.AddComponent<RealmManager>();

        // Bottle Event System (1/100 chance per cast)
        GameObject bes = new GameObject("BottleEventSystem");
        bes.AddComponent<BottleEventSystem>();

        // Weather System (occasional rain, mostly sunny)
        GameObject weather = new GameObject("WeatherSystem");
        weather.AddComponent<WeatherSystem>();

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

        // Fish Inventory Panel (F to toggle - shows caught fish sorted by value)
        GameObject fishInvPanel = new GameObject("FishInventoryPanel");
        fishInvPanel.AddComponent<FishInventoryPanel>();

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

        // Goldie Banks - Rastafarian who walks on the beach
        CreateGoldieBanks();

        // Camera setup
        SetupCamera();

        // Create realistic trees in background
        CreateTreesAroundScene();

        // Create 4 mystical portals on the beach
        CreatePortals();

        // Create the Ice Realm (at offset position)
        CreateIceRealm();

        // Create clothing shop island with Granny
        CreateClothingShopIsland();

        // Horizon boats system
        GameObject boats = new GameObject("HorizonBoats");
        boats.AddComponent<HorizonBoats>();

        // Atmospheric sounds (birds, breeze) with surround panning
        GameObject atmosphere = new GameObject("AtmosphericSounds");
        atmosphere.AddComponent<AtmosphericSounds>();

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
        string[] toDelete = { "Player", "Ground", "Water", "WaterBed", "Dock", "Ramp", "GameManager", "FishingSystem", "UIManager", "Sun", "TreesParent", "LevelingSystem", "QuestSystem", "BottleEventSystem", "QuestNPC", "PortalsParent", "CharacterPanel", "DevPanel", "FishInventoryPanel", "MainMenu", "ClothingShopIsland", "HorizonBoats", "AtmosphericSounds", "BirdFlock", "PlayerHealth", "FoodInventory", "BBQStation", "DockRadio", "ShoulderParrot", "Bobber", "FishingLine", "FishSprites", "RodSprites", "TutorialCat", "GoldieBanks", "GoldieIsland", "BridgeToShop", "SpawnNPC", "WetsuitPete", "SmallIslands", "WeedBagCollectible", "PauseMenu", "WeatherSystem", "ClothingSprites", "FishDiary", "RealmManager", "IceRealm" };
        foreach (string name in toDelete)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null) Object.DestroyImmediate(obj);
        }

        // Delete any stray objects including bobbers and water effects
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj != null && (obj.name.StartsWith("Tree") || obj.name == "DockPost" || obj.name == "Plank" ||
                obj.name.StartsWith("Ramp") || obj.name.StartsWith("Branch") ||
                obj.name == "Bobber" || obj.name == "BobberTop" || obj.name == "BobberBottom" ||
                obj.name == "SplashRing" || obj.name == "WaterDroplet" || obj.name == "FootRipple" ||
                obj.name.StartsWith("ShimmerRipple") || obj.name.StartsWith("WaveRidge") ||
                obj.name.StartsWith("WaveCrest") || obj.name.StartsWith("WaterSparkle") ||
                obj.name.StartsWith("WaterFoam") || obj.name.StartsWith("DepthRing") ||
                obj.name == "Bush" || obj.name == "BushPart"))
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

        // Add seaweed on the sand/beach areas
        AddSeaweed(ground.transform, groundY);

        // Add palm trees scattered on sand
        AddPalmTreesOnSand(ground.transform, groundY);

        // Add warning signs on sand
        AddWarningSigns(ground.transform, groundY);

        // Add more bushes on outer grass areas
        AddOuterBushes(ground.transform, groundY);

        // Add small islands around the main island
        AddSmallIslands(ground.transform);
    }

    static void AddSeaweed(Transform parent, float groundY)
    {
        Material seaweedMat = new Material(Shader.Find("Standard"));
        seaweedMat.color = new Color(0.15f, 0.35f, 0.2f); // Dark green seaweed

        Material seaweedMat2 = new Material(Shader.Find("Standard"));
        seaweedMat2.color = new Color(0.25f, 0.4f, 0.15f); // Lighter seaweed

        // Place seaweed around the beach areas
        for (int i = 0; i < 60; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(38f, 48f); // On the sand area
            Vector3 pos = new Vector3(Mathf.Cos(angle) * dist, groundY - 0.1f, Mathf.Sin(angle) * dist - 9f);

            // Create seaweed strand
            GameObject seaweed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seaweed.name = "Seaweed";
            seaweed.transform.SetParent(parent);
            seaweed.transform.localPosition = pos + new Vector3(0, 0.15f, 0);
            seaweed.transform.localScale = new Vector3(0.08f, Random.Range(0.2f, 0.4f), 0.04f);
            seaweed.transform.localRotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
            seaweed.GetComponent<Renderer>().sharedMaterial = Random.value > 0.5f ? seaweedMat : seaweedMat2;
            Object.DestroyImmediate(seaweed.GetComponent<Collider>());
        }
    }

    static void AddPalmTreesOnSand(Transform parent, float groundY)
    {
        // Scatter palm trees around the sand/beach areas
        Vector3[] palmPositions = new Vector3[]
        {
            new Vector3(-35f, groundY - 0.1f, 10f),
            new Vector3(38f, groundY - 0.1f, -25f),
            new Vector3(-42f, groundY - 0.1f, -30f),
            new Vector3(30f, groundY - 0.1f, 15f),
            new Vector3(-25f, groundY - 0.1f, 25f),
            new Vector3(45f, groundY - 0.1f, 5f),
            new Vector3(-48f, groundY - 0.1f, 0f),
            new Vector3(35f, groundY - 0.1f, -40f),
            new Vector3(-30f, groundY - 0.1f, -45f),
            new Vector3(40f, groundY - 0.1f, 20f),
            new Vector3(-38f, groundY - 0.1f, 35f),
            new Vector3(28f, groundY - 0.1f, -35f),
        };

        foreach (var pos in palmPositions)
        {
            CreateSimplePalmTree(parent, pos);
        }
    }

    static void AddWarningSigns(Transform parent, float groundY)
    {
        // Warning signs on sand saying "CAUTION. FAST CURRENT"
        Material postMat = new Material(Shader.Find("Standard"));
        postMat.color = new Color(0.5f, 0.35f, 0.2f); // Wood

        Material signMat = new Material(Shader.Find("Standard"));
        signMat.color = new Color(0.95f, 0.85f, 0.3f); // Yellow warning sign

        Vector3[] signPositions = new Vector3[]
        {
            new Vector3(-32f, groundY - 0.1f, 5f),
            new Vector3(33f, groundY - 0.1f, -20f),
            new Vector3(-28f, groundY - 0.1f, -35f),
            new Vector3(38f, groundY - 0.1f, 10f),
            new Vector3(0f, groundY - 0.1f, 35f),
            new Vector3(-40f, groundY - 0.1f, -15f),
        };

        foreach (var pos in signPositions)
        {
            GameObject sign = new GameObject("WarningSign");
            sign.transform.SetParent(parent);
            sign.transform.localPosition = pos;
            sign.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Post
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "SignPost";
            post.transform.SetParent(sign.transform);
            post.transform.localPosition = new Vector3(0, 0.5f, 0);
            post.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            post.GetComponent<Renderer>().sharedMaterial = postMat;
            Object.DestroyImmediate(post.GetComponent<Collider>());

            // Sign board
            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "SignBoard";
            board.transform.SetParent(sign.transform);
            board.transform.localPosition = new Vector3(0, 1.1f, 0);
            board.transform.localScale = new Vector3(0.8f, 0.5f, 0.05f);
            board.GetComponent<Renderer>().sharedMaterial = signMat;
            Object.DestroyImmediate(board.GetComponent<Collider>());

            // Add text component (WarningSignText script)
            board.AddComponent<WarningSignText>();
        }

        // Add 10 more readable signs with different messages
        AddReadableSigns(parent, groundY);
    }

    static void AddReadableSigns(Transform parent, float groundY)
    {
        // Different sign messages
        string[][] signData = new string[][]
        {
            new string[] { "DANGER", "Deep water ahead! Strong currents may pull you under. Swim at your own risk!" },
            new string[] { "FISHING TIP", "Cast further out for rare fish! Hold the cast button longer for more distance." },
            new string[] { "NOTICE", "Sell your fish to Wetsuit Pete for gold! Find him near the dock." },
            new string[] { "WARNING", "Jellyfish spotted in these waters. Watch your step near the shore!" },
            new string[] { "ISLAND RULES", "No littering. Respect the wildlife. Take only fish, leave only footprints." },
            new string[] { "TREASURE", "Rumor has it bottles wash up with treasure inside. Keep your eyes peeled!" },
            new string[] { "LEGEND", "They say a Golden Starfish lives in these waters. Only the luckiest catch it!" },
            new string[] { "TIP", "Visit the clothing shop for hats and gear. Some items boost your luck!" },
            new string[] { "HISTORY", "This island was founded by Captain Barnacle in 1842. Fish have thrived since." },
            new string[] { "BEWARE", "The Rastafarian on the far island has special quests. Seek him out!" }
        };

        Vector3[] signPositions = new Vector3[]
        {
            new Vector3(20f, groundY - 0.1f, 30f),
            new Vector3(-15f, groundY - 0.1f, 25f),
            new Vector3(25f, groundY - 0.1f, -15f),
            new Vector3(-20f, groundY - 0.1f, -25f),
            new Vector3(10f, groundY - 0.1f, -30f),
            new Vector3(-35f, groundY - 0.1f, 15f),
            new Vector3(15f, groundY - 0.1f, 20f),
            new Vector3(-25f, groundY - 0.1f, 5f),
            new Vector3(30f, groundY - 0.1f, -5f),
            new Vector3(-10f, groundY - 0.1f, -15f)
        };

        Material postMat = new Material(Shader.Find("Standard"));
        postMat.color = new Color(0.4f, 0.3f, 0.2f); // Dark wood

        // Different sign colors
        Color[] signColors = new Color[]
        {
            new Color(0.95f, 0.85f, 0.3f),  // Yellow
            new Color(0.3f, 0.8f, 0.4f),    // Green
            new Color(0.4f, 0.6f, 0.9f),    // Blue
            new Color(0.9f, 0.6f, 0.3f),    // Orange
            new Color(0.85f, 0.85f, 0.85f), // White
            new Color(0.9f, 0.7f, 0.9f),    // Pink
            new Color(1f, 0.85f, 0.2f),     // Gold
            new Color(0.6f, 0.9f, 0.9f),    // Cyan
            new Color(0.8f, 0.7f, 0.5f),    // Tan
            new Color(0.7f, 0.5f, 0.8f)     // Purple
        };

        for (int i = 0; i < signPositions.Length; i++)
        {
            GameObject sign = new GameObject("ReadableSign_" + i);
            sign.transform.SetParent(parent);
            sign.transform.localPosition = signPositions[i];
            sign.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Post
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "SignPost";
            post.transform.SetParent(sign.transform);
            post.transform.localPosition = new Vector3(0, 0.6f, 0);
            post.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);
            post.GetComponent<Renderer>().sharedMaterial = postMat;
            Object.DestroyImmediate(post.GetComponent<Collider>());

            // Sign board
            Material signMat = new Material(Shader.Find("Standard"));
            signMat.color = signColors[i];

            GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "SignBoard";
            board.transform.SetParent(sign.transform);
            board.transform.localPosition = new Vector3(0, 1.3f, 0);
            board.transform.localScale = new Vector3(0.9f, 0.6f, 0.06f);
            board.GetComponent<Renderer>().sharedMaterial = signMat;
            Object.DestroyImmediate(board.GetComponent<Collider>());

            // Add readable sign component with message
            ReadableSign readableSign = board.AddComponent<ReadableSign>();
            readableSign.signTitle = signData[i][0];
            readableSign.signMessage = signData[i][1];
            readableSign.backgroundColor = signColors[i];
            readableSign.titleColor = new Color(0.2f, 0.1f, 0.1f);
        }
    }

    static void AddOuterBushes(Transform parent, float groundY)
    {
        // More bushes scattered further from center on grass areas
        Material bushMat1 = new Material(Shader.Find("Standard"));
        bushMat1.color = new Color(0.2f, 0.45f, 0.18f);

        Material bushMat2 = new Material(Shader.Find("Standard"));
        bushMat2.color = new Color(0.25f, 0.5f, 0.2f);

        Material bushMat3 = new Material(Shader.Find("Standard"));
        bushMat3.color = new Color(0.18f, 0.4f, 0.22f);

        // Outer bush positions - further from center
        Vector3[] outerBushPositions = new Vector3[]
        {
            new Vector3(-25f, groundY + 0.3f, -30f),
            new Vector3(28f, groundY + 0.3f, -28f),
            new Vector3(-30f, groundY + 0.3f, 8f),
            new Vector3(25f, groundY + 0.3f, 12f),
            new Vector3(-18f, groundY + 0.3f, -35f),
            new Vector3(22f, groundY + 0.3f, -35f),
            new Vector3(-35f, groundY + 0.3f, -8f),
            new Vector3(32f, groundY + 0.3f, -12f),
            new Vector3(-28f, groundY + 0.3f, 15f),
            new Vector3(30f, groundY + 0.3f, 5f),
            new Vector3(-22f, groundY + 0.3f, 20f),
            new Vector3(18f, groundY + 0.3f, 18f),
            new Vector3(-32f, groundY + 0.3f, -22f),
            new Vector3(35f, groundY + 0.3f, -20f),
            new Vector3(-12f, groundY + 0.3f, -38f),
            new Vector3(15f, groundY + 0.3f, -40f),
        };

        Material[] mats = { bushMat1, bushMat2, bushMat3 };

        foreach (var pos in outerBushPositions)
        {
            GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = "OuterBush";
            bush.transform.SetParent(parent);
            bush.transform.localPosition = pos;
            float size = Random.Range(0.8f, 1.8f);
            bush.transform.localScale = new Vector3(size, size * 0.7f, size);
            bush.GetComponent<Renderer>().sharedMaterial = mats[Random.Range(0, mats.Length)];
            Object.DestroyImmediate(bush.GetComponent<Collider>());

            // Add smaller spheres for variety
            if (Random.value > 0.5f)
            {
                GameObject bush2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bush2.name = "OuterBush2";
                bush2.transform.SetParent(bush.transform);
                bush2.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), 0.2f, Random.Range(-0.3f, 0.3f));
                bush2.transform.localScale = Vector3.one * Random.Range(0.5f, 0.8f);
                bush2.GetComponent<Renderer>().sharedMaterial = mats[Random.Range(0, mats.Length)];
                Object.DestroyImmediate(bush2.GetComponent<Collider>());
            }
        }
    }

    static void AddSmallIslands(Transform parent)
    {
        Material sandMat = new Material(Shader.Find("Standard"));
        sandMat.color = new Color(0.88f, 0.78f, 0.55f);

        Material grassMat = new Material(Shader.Find("Standard"));
        grassMat.color = new Color(0.25f, 0.5f, 0.2f);

        // Small islands scattered ALL AROUND the main island
        // Raised above water level (water is at Y=0.75)
        Vector3[] islandPositions = new Vector3[]
        {
            // North
            new Vector3(0f, 1.0f, 95f),
            new Vector3(-40f, 0.95f, 85f),
            new Vector3(45f, 1.0f, 90f),
            // Northeast
            new Vector3(75f, 1.0f, 70f),
            new Vector3(95f, 0.95f, 45f),
            // East
            new Vector3(100f, 1.0f, 0f),
            new Vector3(90f, 1.0f, -35f),
            // Southeast
            new Vector3(80f, 0.95f, -75f),
            new Vector3(55f, 1.0f, -90f),
            // South
            new Vector3(0f, 1.0f, -100f),
            new Vector3(-45f, 0.95f, -95f),
            // Southwest
            new Vector3(-80f, 1.0f, -70f),
            new Vector3(-95f, 1.0f, -40f),
            // West
            new Vector3(-100f, 0.95f, 0f),
            new Vector3(-90f, 1.0f, 40f),
            // Northwest
            new Vector3(-75f, 1.0f, 75f),
            new Vector3(-50f, 0.95f, 90f)
        };

        foreach (var pos in islandPositions)
        {
            float size = Random.Range(4f, 8f);

            // Sand base - raised above water
            GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            island.name = "SmallIsland";
            island.transform.SetParent(parent);
            island.transform.localPosition = pos;
            island.transform.localScale = new Vector3(size, 0.5f, size); // Taller base
            island.GetComponent<Renderer>().sharedMaterial = sandMat;

            // Grass top - smaller and centered to avoid water overlap
            GameObject grass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            grass.name = "IslandGrass";
            grass.transform.SetParent(island.transform);
            grass.transform.localPosition = new Vector3(0, 0.2f, 0);
            grass.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f); // Smaller grass circle
            grass.GetComponent<Renderer>().sharedMaterial = grassMat;
            Object.DestroyImmediate(grass.GetComponent<Collider>());

            // EVERY small island gets a palm tree
            CreateSimplePalmTree(island.transform, new Vector3(0, 0.5f, 0));
        }
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
        // Create wooden bridge from main island to Goldie's island
        // Bridge sits ON TOP of sand/water and leads to the island
        GameObject bridge = new GameObject("WoodenBridge");

        // Bridge starts at the edge of main island
        float bridgeStartX = 25f;
        float bridgeStartZ = 30f;
        float groundY = 1.5f;
        float bridgeY = 1.8f; // Bridge deck height
        float bridgeLength = 40f;
        float bridgeWidth = 2.5f;

        bridge.transform.position = new Vector3(bridgeStartX, bridgeY, bridgeStartZ);

        // ========== STAIRCASE LEADING UP TO BRIDGE ==========
        Material stairMat = new Material(Shader.Find("Standard"));
        stairMat.color = new Color(0.4f, 0.3f, 0.18f); // Wood color

        GameObject staircase = new GameObject("BridgeStaircase");
        staircase.transform.SetParent(bridge.transform);
        staircase.transform.localPosition = new Vector3(0, 0, -3f); // In front of bridge start

        // Create 4 steps going up
        int numSteps = 4;
        float stepHeight = (bridgeY - groundY) / numSteps;
        float stepDepth = 0.6f;

        for (int i = 0; i < numSteps; i++)
        {
            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "Step_" + i;
            step.transform.SetParent(staircase.transform);
            float yPos = -(bridgeY - groundY) + (i + 0.5f) * stepHeight;
            float zPos = -i * stepDepth;
            step.transform.localPosition = new Vector3(0, yPos, zPos);
            step.transform.localScale = new Vector3(bridgeWidth, stepHeight * 0.9f, stepDepth);
            step.GetComponent<Renderer>().sharedMaterial = stairMat;
        }

        // Side rails for staircase
        Material railMat = new Material(Shader.Find("Standard"));
        railMat.color = new Color(0.35f, 0.25f, 0.12f);

        for (int side = 0; side < 2; side++)
        {
            float x = side == 0 ? -bridgeWidth / 2 - 0.1f : bridgeWidth / 2 + 0.1f;

            // Stair rail posts
            for (int i = 0; i < 3; i++)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "StairRailPost";
                post.transform.SetParent(staircase.transform);
                float yPos = -(bridgeY - groundY) + (i + 1) * stepHeight + 0.4f;
                float zPos = -i * stepDepth - stepDepth / 2;
                post.transform.localPosition = new Vector3(x, yPos, zPos);
                post.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
                post.GetComponent<Renderer>().sharedMaterial = railMat;
                Object.DestroyImmediate(post.GetComponent<Collider>());
            }
        }

        Material plankMat = new Material(Shader.Find("Standard"));
        plankMat.color = new Color(0.45f, 0.32f, 0.18f); // Weathered wood

        // Support posts under the bridge (so it looks like it's elevated)
        Material supportMat = new Material(Shader.Find("Standard"));
        supportMat.color = new Color(0.3f, 0.2f, 0.1f); // Dark wood

        // Bridge planks
        int plankCount = 50;
        for (int i = 0; i < plankCount; i++)
        {
            GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.name = "BridgePlank";
            plank.transform.SetParent(bridge.transform);
            float z = ((float)i / plankCount) * bridgeLength;
            plank.transform.localPosition = new Vector3(0, 0, z);
            plank.transform.localScale = new Vector3(bridgeWidth, 0.15f, bridgeLength / plankCount * 0.9f);
            plank.GetComponent<Renderer>().sharedMaterial = plankMat;
        }

        // Support posts under bridge
        for (int i = 0; i < 8; i++)
        {
            float z = i * (bridgeLength / 7);
            GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
            support.name = "BridgeSupport";
            support.transform.SetParent(bridge.transform);
            support.transform.localPosition = new Vector3(0, -0.5f, z);
            support.transform.localScale = new Vector3(0.3f, 1.2f, 0.3f);
            support.GetComponent<Renderer>().sharedMaterial = supportMat;
            Object.DestroyImmediate(support.GetComponent<Collider>());
        }

        // Rails on sides
        for (int side = 0; side < 2; side++)
        {
            float x = side == 0 ? -bridgeWidth / 2 - 0.1f : bridgeWidth / 2 + 0.1f;

            // Rail posts
            for (int i = 0; i < 8; i++)
            {
                float z = i * (bridgeLength / 7);
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "RailPost";
                post.transform.SetParent(bridge.transform);
                post.transform.localPosition = new Vector3(x, 0.5f, z);
                post.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
                post.GetComponent<Renderer>().sharedMaterial = railMat;
                Object.DestroyImmediate(post.GetComponent<Collider>());
            }

            // Top rail
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "TopRail";
            rail.transform.SetParent(bridge.transform);
            rail.transform.localPosition = new Vector3(x, 1f, bridgeLength / 2);
            rail.transform.localScale = new Vector3(0.08f, 0.08f, bridgeLength);
            rail.GetComponent<Renderer>().sharedMaterial = railMat;
            Object.DestroyImmediate(rail.GetComponent<Collider>());
        }

        // Create Goldie's island at the end of the bridge - positioned to connect with bridge
        float islandX = bridgeStartX;
        float islandZ = bridgeStartZ + bridgeLength + 7f; // Island center is 7 units past bridge end (island radius is ~7)
        CreateGoldieIsland(new Vector3(islandX, 1.6f, islandZ)); // Same height as bridge deck
    }

    static void CreateGoldieIsland(Vector3 position)
    {
        GameObject island = new GameObject("GoldieIsland");
        island.transform.position = position;

        // Island ground - small sandy island raised above water
        Material sandMat = new Material(Shader.Find("Standard"));
        sandMat.color = new Color(0.85f, 0.75f, 0.5f);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ground.name = "IslandGround";
        ground.transform.SetParent(island.transform);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale = new Vector3(14f, 0.6f, 14f); // Larger island
        ground.GetComponent<Renderer>().sharedMaterial = sandMat;

        // Small shack on the side, not blocking Goldie
        CreateSmallShack(island.transform, new Vector3(3f, 0.6f, 3f));

        // Bushes - positioned around the edges, not blocking center
        Material bushMat = new Material(Shader.Find("Standard"));
        bushMat.color = new Color(0.2f, 0.45f, 0.15f);

        Vector3[] bushPositions = new Vector3[]
        {
            new Vector3(4.5f, 0.4f, -1f),
            new Vector3(-4.5f, 0.3f, -2f),
            new Vector3(4f, 0.35f, 3f),
            new Vector3(-4f, 0.4f, 2f),
            new Vector3(-3f, 0.35f, 4f)
        };

        foreach (var pos in bushPositions)
        {
            GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = "Bush";
            bush.transform.SetParent(island.transform);
            bush.transform.localPosition = pos;
            bush.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f) * Random.Range(0.8f, 1.2f);
            bush.GetComponent<Renderer>().sharedMaterial = bushMat;
            Object.DestroyImmediate(bush.GetComponent<Collider>());
        }

        // Goldie Banks now walks on the beach instead of being on this island
        // The island remains as scenery at the end of the bridge

        // Add a palm tree for atmosphere
        CreatePalmTree(island.transform, new Vector3(-2f, 0.5f, 3f));
        CreatePalmTree(island.transform, new Vector3(2f, 0.5f, -2f));
    }

    static void CreateSmallShack(Transform parent, Vector3 localPos)
    {
        GameObject shack = new GameObject("Shack");
        shack.transform.SetParent(parent);
        shack.transform.localPosition = localPos;

        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.4f, 0.28f, 0.15f);

        Material roofMat = new Material(Shader.Find("Standard"));
        roofMat.color = new Color(0.3f, 0.35f, 0.2f); // Thatched roof green

        // Walls
        GameObject walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
        walls.transform.SetParent(shack.transform);
        walls.transform.localPosition = new Vector3(0, 0.75f, 0);
        walls.transform.localScale = new Vector3(2.5f, 1.5f, 2f);
        walls.GetComponent<Renderer>().sharedMaterial = woodMat;

        // Roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.transform.SetParent(shack.transform);
        roof.transform.localPosition = new Vector3(0, 1.8f, 0);
        roof.transform.localScale = new Vector3(3f, 0.3f, 2.5f);
        roof.transform.localRotation = Quaternion.Euler(0, 0, 5f);
        roof.GetComponent<Renderer>().sharedMaterial = roofMat;
        Object.DestroyImmediate(roof.GetComponent<Collider>());

        // Door opening (black cube)
        Material doorMat = new Material(Shader.Find("Standard"));
        doorMat.color = new Color(0.05f, 0.05f, 0.05f);

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.transform.SetParent(shack.transform);
        door.transform.localPosition = new Vector3(0, 0.5f, 1.01f);
        door.transform.localScale = new Vector3(0.6f, 1f, 0.1f);
        door.GetComponent<Renderer>().sharedMaterial = doorMat;
        Object.DestroyImmediate(door.GetComponent<Collider>());
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
        // Water level raised slightly higher
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Water";
        water.transform.position = new Vector3(0, 0.75f, 0);  // Raised water level
        water.transform.localScale = new Vector3(80, 1, 80); // MASSIVE water plane to cover distant islands
        water.AddComponent<WaterEffect>();

        // Water bed (sandy bottom visible through clear water)
        GameObject waterBed = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterBed.name = "WaterBed";
        waterBed.transform.position = new Vector3(0, -0.5f, 0);  // Raised to match
        waterBed.transform.localScale = new Vector3(80, 1, 80);
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
        player.AddComponent<PlayerClothingVisuals>();

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

        // === PALM TREES SCATTERED ON THE SANDY BEACH AREAS ===
        // Beach is around the outer edges of the island
        Vector3[] beachPalmPositions = new Vector3[]
        {
            // Front beach (near water/dock area)
            new Vector3(-25f, groundY, 18f),
            new Vector3(-18f, groundY, 22f),
            new Vector3(-8f, groundY, 25f),
            new Vector3(8f, groundY, 24f),
            new Vector3(18f, groundY, 20f),
            new Vector3(28f, groundY, 15f),
            // Right side beach
            new Vector3(32f, groundY, 5f),
            new Vector3(35f, groundY, -8f),
            new Vector3(30f, groundY, -18f),
            // Back beach
            new Vector3(22f, groundY, -28f),
            new Vector3(10f, groundY, -32f),
            new Vector3(-5f, groundY, -30f),
            new Vector3(-18f, groundY, -28f),
            // Left side beach
            new Vector3(-30f, groundY, -15f),
            new Vector3(-35f, groundY, -5f),
            new Vector3(-32f, groundY, 8f),
        };

        foreach (var pos in beachPalmPositions)
        {
            // Randomize position slightly
            Vector3 randomizedPos = pos + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            float height = Random.Range(4f, 8f);
            CreateTropicalPalmTree(treesParent.transform, randomizedPos, height);
        }

        // === BUSHES SCATTERED AROUND THE ISLAND ===
        CreateScatteredBushes(treesParent.transform, groundY);
    }

    static void CreateScatteredBushes(Transform parent, float groundY)
    {
        Material bushMat = new Material(Shader.Find("Standard"));
        bushMat.color = new Color(0.18f, 0.42f, 0.12f); // Dark green bush

        Material bushMat2 = new Material(Shader.Find("Standard"));
        bushMat2.color = new Color(0.25f, 0.50f, 0.18f); // Medium green bush

        Material bushMat3 = new Material(Shader.Find("Standard"));
        bushMat3.color = new Color(0.35f, 0.55f, 0.25f); // Light green bush

        Material[] bushMats = { bushMat, bushMat2, bushMat3 };

        // Scatter bushes around the island
        for (int i = 0; i < 40; i++)
        {
            // Random position on the island
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(8f, 35f);
            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance - 5f; // Offset toward back

            // Skip if too close to dock area
            if (x > -15f && x < 5f && z > 10f) continue;

            Vector3 pos = new Vector3(x, groundY, z);
            CreateBush(parent, pos, bushMats[Random.Range(0, bushMats.Length)]);
        }

        // Extra bushes near trees and palm trees
        for (int i = 0; i < 25; i++)
        {
            float x = Random.Range(-35f, 35f);
            float z = Random.Range(-35f, 25f);

            // Skip dock area
            if (x > -15f && x < 5f && z > 10f) continue;

            Vector3 pos = new Vector3(x, groundY, z);
            CreateBush(parent, pos, bushMats[Random.Range(0, bushMats.Length)]);
        }
    }

    static void CreateBush(Transform parent, Vector3 pos, Material mat)
    {
        GameObject bush = new GameObject("Bush");
        bush.transform.SetParent(parent);
        bush.transform.position = pos;

        // Main bush body - cluster of spheres
        int sphereCount = Random.Range(3, 6);
        float baseSize = Random.Range(0.6f, 1.2f);

        for (int i = 0; i < sphereCount; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "BushPart";
            sphere.transform.SetParent(bush.transform);

            float offsetX = Random.Range(-0.4f, 0.4f) * baseSize;
            float offsetZ = Random.Range(-0.4f, 0.4f) * baseSize;
            float offsetY = Random.Range(0.2f, 0.5f) * baseSize;
            float size = Random.Range(0.5f, 0.9f) * baseSize;

            sphere.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            sphere.transform.localScale = new Vector3(size, size * 0.7f, size);
            sphere.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(sphere.GetComponent<Collider>());
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

    static void CreateGoldieBanks()
    {
        // Goldie Banks - Rastafarian who walks along the beach
        // He smokes, has dreadlocks, and gives repeatable quests to find his lost bag
        GameObject goldie = new GameObject("GoldieBanks");
        goldie.transform.position = new Vector3(15f, 1.6f, 20f); // Starting position on the beach
        goldie.AddComponent<GoldieBanksNPC>();
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
        RealmType[] destinations = {
            RealmType.JungleRealm,
            RealmType.VolcanicRealm,
            RealmType.IceRealm,
            RealmType.VoidRealm
        };

        for (int i = 0; i < 4; i++)
        {
            CreateSinglePortal(portalsParent.transform, portalPositions[i], portalNames[i], requiredLevels[i], portalColors[i], destinations[i]);
        }
    }

    static void CreateSinglePortal(Transform parent, Vector3 pos, string name, int requiredLevel, Color portalColor, RealmType destination = RealmType.TropicalIsland)
    {
        GameObject portal = new GameObject(name);
        portal.transform.SetParent(parent);
        portal.transform.position = pos;
        portal.transform.rotation = Quaternion.Euler(0, 0, 0);  // Face forward toward player

        // Add portal interaction component
        PortalInteraction portalInteraction = portal.AddComponent<PortalInteraction>();
        portalInteraction.portalName = name.Replace("Portal", " Realm");
        portalInteraction.requiredLevel = requiredLevel;
        portalInteraction.destinationRealm = destination;
        portalInteraction.spawnOffset = new Vector3(0, 2f, 5f); // Spawn near return portal

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

        // Granny NPC with rocking chair - in front of shop (offset left for visibility)
        CreateGrannyNPC(shop.transform, new Vector3(-4.5f, groundY, 3));

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

        // Clothing rack with hangers and clothes
        GameObject clothingRack = new GameObject("ClothingRack");
        clothingRack.transform.SetParent(shop.transform);
        clothingRack.transform.localPosition = new Vector3(0, 0, -1.2f);

        Material metalMat = new Material(Shader.Find("Standard"));
        metalMat.color = new Color(0.3f, 0.3f, 0.35f);
        metalMat.SetFloat("_Metallic", 0.8f);
        metalMat.SetFloat("_Glossiness", 0.6f);

        // Rack horizontal bar (top)
        GameObject rackBar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rackBar.name = "RackBar";
        rackBar.transform.SetParent(clothingRack.transform);
        rackBar.transform.localPosition = new Vector3(0, 2.0f, 0);
        rackBar.transform.localRotation = Quaternion.Euler(0, 0, 90);
        rackBar.transform.localScale = new Vector3(0.04f, 1.6f, 0.04f);
        rackBar.GetComponent<Renderer>().sharedMaterial = metalMat;
        Object.DestroyImmediate(rackBar.GetComponent<Collider>());

        // Rack vertical posts (left and right)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "RackPost";
            post.transform.SetParent(clothingRack.transform);
            post.transform.localPosition = new Vector3(side * 1.5f, 1.0f, 0);
            post.transform.localScale = new Vector3(0.05f, 1.0f, 0.05f);
            post.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(post.GetComponent<Collider>());

            // Rack feet (angled supports)
            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            foot.name = "RackFoot";
            foot.transform.SetParent(clothingRack.transform);
            foot.transform.localPosition = new Vector3(side * 1.5f, 0.15f, 0.2f);
            foot.transform.localRotation = Quaternion.Euler(30, 0, 0);
            foot.transform.localScale = new Vector3(0.03f, 0.25f, 0.03f);
            foot.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(foot.GetComponent<Collider>());

            GameObject foot2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            foot2.name = "RackFoot2";
            foot2.transform.SetParent(clothingRack.transform);
            foot2.transform.localPosition = new Vector3(side * 1.5f, 0.15f, -0.2f);
            foot2.transform.localRotation = Quaternion.Euler(-30, 0, 0);
            foot2.transform.localScale = new Vector3(0.03f, 0.25f, 0.03f);
            foot2.GetComponent<Renderer>().sharedMaterial = metalMat;
            Object.DestroyImmediate(foot2.GetComponent<Collider>());
        }

        // Clothes hangers with clothing
        Color[] clothColors = {
            new Color(0.85f, 0.15f, 0.1f),   // Red shirt
            new Color(0.15f, 0.35f, 0.7f),   // Blue shirt
            new Color(0.2f, 0.6f, 0.25f),    // Green
            new Color(0.9f, 0.8f, 0.3f),     // Yellow
            new Color(0.6f, 0.2f, 0.5f)      // Purple
        };
        string[] clothTypes = { "TShirt", "Dress", "TShirt", "Blouse", "Jacket" };

        for (int i = 0; i < 5; i++)
        {
            float xPos = -1.2f + i * 0.6f;
            GameObject hangerGroup = new GameObject("HangerWithClothes" + i);
            hangerGroup.transform.SetParent(clothingRack.transform);
            hangerGroup.transform.localPosition = new Vector3(xPos, 1.95f, 0);

            // Hanger (triangle shape with hook)
            Material hangerMat = new Material(Shader.Find("Standard"));
            hangerMat.color = new Color(0.85f, 0.75f, 0.6f); // Wooden hanger color

            // Hanger hook
            GameObject hook = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hook.name = "Hook";
            hook.transform.SetParent(hangerGroup.transform);
            hook.transform.localPosition = new Vector3(0, 0.08f, 0);
            hook.transform.localScale = new Vector3(0.015f, 0.08f, 0.015f);
            hook.GetComponent<Renderer>().sharedMaterial = hangerMat;
            Object.DestroyImmediate(hook.GetComponent<Collider>());

            // Hanger bar (horizontal)
            GameObject hangerBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hangerBar.name = "HangerBar";
            hangerBar.transform.SetParent(hangerGroup.transform);
            hangerBar.transform.localPosition = new Vector3(0, -0.02f, 0);
            hangerBar.transform.localScale = new Vector3(0.35f, 0.025f, 0.02f);
            hangerBar.GetComponent<Renderer>().sharedMaterial = hangerMat;
            Object.DestroyImmediate(hangerBar.GetComponent<Collider>());

            // Hanger shoulders (angled parts)
            for (int shoulder = -1; shoulder <= 1; shoulder += 2)
            {
                GameObject shoulderPart = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shoulderPart.name = "HangerShoulder";
                shoulderPart.transform.SetParent(hangerGroup.transform);
                shoulderPart.transform.localPosition = new Vector3(shoulder * 0.1f, -0.04f, 0);
                shoulderPart.transform.localRotation = Quaternion.Euler(0, 0, shoulder * -25);
                shoulderPart.transform.localScale = new Vector3(0.12f, 0.02f, 0.015f);
                shoulderPart.GetComponent<Renderer>().sharedMaterial = hangerMat;
                Object.DestroyImmediate(shoulderPart.GetComponent<Collider>());
            }

            // Create clothing item based on type
            Material clothMat = new Material(Shader.Find("Standard"));
            clothMat.color = clothColors[i];

            if (clothTypes[i] == "TShirt" || clothTypes[i] == "Blouse")
            {
                // T-shirt body
                GameObject shirtBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shirtBody.name = "ShirtBody";
                shirtBody.transform.SetParent(hangerGroup.transform);
                shirtBody.transform.localPosition = new Vector3(0, -0.25f, 0);
                shirtBody.transform.localScale = new Vector3(0.3f, 0.35f, 0.06f);
                shirtBody.GetComponent<Renderer>().sharedMaterial = clothMat;
                Object.DestroyImmediate(shirtBody.GetComponent<Collider>());

                // Sleeves
                for (int sleeve = -1; sleeve <= 1; sleeve += 2)
                {
                    GameObject sleeveObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    sleeveObj.name = "Sleeve";
                    sleeveObj.transform.SetParent(hangerGroup.transform);
                    sleeveObj.transform.localPosition = new Vector3(sleeve * 0.2f, -0.12f, 0);
                    sleeveObj.transform.localRotation = Quaternion.Euler(0, 0, sleeve * 45);
                    sleeveObj.transform.localScale = new Vector3(0.12f, 0.1f, 0.05f);
                    sleeveObj.GetComponent<Renderer>().sharedMaterial = clothMat;
                    Object.DestroyImmediate(sleeveObj.GetComponent<Collider>());
                }
            }
            else if (clothTypes[i] == "Dress")
            {
                // Dress top
                GameObject dressTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dressTop.name = "DressTop";
                dressTop.transform.SetParent(hangerGroup.transform);
                dressTop.transform.localPosition = new Vector3(0, -0.15f, 0);
                dressTop.transform.localScale = new Vector3(0.28f, 0.2f, 0.05f);
                dressTop.GetComponent<Renderer>().sharedMaterial = clothMat;
                Object.DestroyImmediate(dressTop.GetComponent<Collider>());

                // Dress skirt (flared)
                GameObject dressSkirt = GameObject.CreatePrimitive(PrimitiveType.Cube);
                dressSkirt.name = "DressSkirt";
                dressSkirt.transform.SetParent(hangerGroup.transform);
                dressSkirt.transform.localPosition = new Vector3(0, -0.42f, 0);
                dressSkirt.transform.localScale = new Vector3(0.38f, 0.35f, 0.06f);
                dressSkirt.GetComponent<Renderer>().sharedMaterial = clothMat;
                Object.DestroyImmediate(dressSkirt.GetComponent<Collider>());
            }
            else if (clothTypes[i] == "Jacket")
            {
                // Jacket body
                GameObject jacketBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jacketBody.name = "JacketBody";
                jacketBody.transform.SetParent(hangerGroup.transform);
                jacketBody.transform.localPosition = new Vector3(0, -0.28f, 0);
                jacketBody.transform.localScale = new Vector3(0.32f, 0.42f, 0.08f);
                jacketBody.GetComponent<Renderer>().sharedMaterial = clothMat;
                Object.DestroyImmediate(jacketBody.GetComponent<Collider>());

                // Jacket collar
                Material collarMat = new Material(Shader.Find("Standard"));
                collarMat.color = clothColors[i] * 0.7f;
                for (int collar = -1; collar <= 1; collar += 2)
                {
                    GameObject collarPart = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    collarPart.name = "Collar";
                    collarPart.transform.SetParent(hangerGroup.transform);
                    collarPart.transform.localPosition = new Vector3(collar * 0.08f, -0.1f, 0.04f);
                    collarPart.transform.localRotation = Quaternion.Euler(0, 0, collar * -20);
                    collarPart.transform.localScale = new Vector3(0.08f, 0.1f, 0.03f);
                    collarPart.GetComponent<Renderer>().sharedMaterial = collarMat;
                    Object.DestroyImmediate(collarPart.GetComponent<Collider>());
                }

                // Jacket sleeves (longer)
                for (int sleeve = -1; sleeve <= 1; sleeve += 2)
                {
                    GameObject sleeveObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    sleeveObj.name = "JacketSleeve";
                    sleeveObj.transform.SetParent(hangerGroup.transform);
                    sleeveObj.transform.localPosition = new Vector3(sleeve * 0.22f, -0.2f, 0);
                    sleeveObj.transform.localRotation = Quaternion.Euler(0, 0, sleeve * 30);
                    sleeveObj.transform.localScale = new Vector3(0.1f, 0.25f, 0.07f);
                    sleeveObj.GetComponent<Renderer>().sharedMaterial = clothMat;
                    Object.DestroyImmediate(sleeveObj.GetComponent<Collider>());
                }
            }
        }

        // Potted flower decoration
        GameObject pottedFlower = new GameObject("PottedFlower");
        pottedFlower.transform.SetParent(shop.transform);
        pottedFlower.transform.localPosition = new Vector3(-1.6f, 0.2f, 0.8f);

        // Terracotta pot
        Material potMat = new Material(Shader.Find("Standard"));
        potMat.color = new Color(0.75f, 0.4f, 0.25f);

        GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pot.name = "Pot";
        pot.transform.SetParent(pottedFlower.transform);
        pot.transform.localPosition = Vector3.zero;
        pot.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
        pot.GetComponent<Renderer>().sharedMaterial = potMat;
        Object.DestroyImmediate(pot.GetComponent<Collider>());

        // Pot rim
        GameObject potRim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        potRim.name = "PotRim";
        potRim.transform.SetParent(pottedFlower.transform);
        potRim.transform.localPosition = new Vector3(0, 0.18f, 0);
        potRim.transform.localScale = new Vector3(0.35f, 0.04f, 0.35f);
        potRim.GetComponent<Renderer>().sharedMaterial = potMat;
        Object.DestroyImmediate(potRim.GetComponent<Collider>());

        // Soil
        Material soilMat = new Material(Shader.Find("Standard"));
        soilMat.color = new Color(0.3f, 0.2f, 0.1f);
        GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        soil.name = "Soil";
        soil.transform.SetParent(pottedFlower.transform);
        soil.transform.localPosition = new Vector3(0, 0.15f, 0);
        soil.transform.localScale = new Vector3(0.25f, 0.05f, 0.25f);
        soil.GetComponent<Renderer>().sharedMaterial = soilMat;
        Object.DestroyImmediate(soil.GetComponent<Collider>());

        // Flower stem
        Material stemMat = new Material(Shader.Find("Standard"));
        stemMat.color = new Color(0.2f, 0.5f, 0.15f);
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.name = "Stem";
        stem.transform.SetParent(pottedFlower.transform);
        stem.transform.localPosition = new Vector3(0, 0.45f, 0);
        stem.transform.localScale = new Vector3(0.03f, 0.3f, 0.03f);
        stem.GetComponent<Renderer>().sharedMaterial = stemMat;
        Object.DestroyImmediate(stem.GetComponent<Collider>());

        // Leaves
        for (int leaf = 0; leaf < 3; leaf++)
        {
            GameObject leafObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leafObj.name = "Leaf" + leaf;
            leafObj.transform.SetParent(pottedFlower.transform);
            float leafAngle = leaf * 120f * Mathf.Deg2Rad;
            leafObj.transform.localPosition = new Vector3(Mathf.Cos(leafAngle) * 0.08f, 0.3f + leaf * 0.08f, Mathf.Sin(leafAngle) * 0.08f);
            leafObj.transform.localScale = new Vector3(0.12f, 0.03f, 0.06f);
            leafObj.transform.localRotation = Quaternion.Euler(0, -leaf * 120, 20);
            leafObj.GetComponent<Renderer>().sharedMaterial = stemMat;
            Object.DestroyImmediate(leafObj.GetComponent<Collider>());
        }

        // Flower petals (hibiscus style - tropical!)
        Material petalMat = new Material(Shader.Find("Standard"));
        petalMat.color = new Color(1f, 0.4f, 0.5f); // Pink hibiscus
        for (int petal = 0; petal < 5; petal++)
        {
            GameObject petalObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            petalObj.name = "Petal" + petal;
            petalObj.transform.SetParent(pottedFlower.transform);
            float angle = petal * 72f * Mathf.Deg2Rad;
            petalObj.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.08f, 0.72f, Mathf.Sin(angle) * 0.08f);
            petalObj.transform.localScale = new Vector3(0.1f, 0.06f, 0.12f);
            petalObj.transform.localRotation = Quaternion.Euler(30, -petal * 72, 0);
            petalObj.GetComponent<Renderer>().sharedMaterial = petalMat;
            Object.DestroyImmediate(petalObj.GetComponent<Collider>());
        }

        // Flower center (yellow)
        Material centerMat = new Material(Shader.Find("Standard"));
        centerMat.color = new Color(1f, 0.9f, 0.2f);
        GameObject flowerCenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flowerCenter.name = "FlowerCenter";
        flowerCenter.transform.SetParent(pottedFlower.transform);
        flowerCenter.transform.localPosition = new Vector3(0, 0.72f, 0);
        flowerCenter.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        flowerCenter.GetComponent<Renderer>().sharedMaterial = centerMat;
        Object.DestroyImmediate(flowerCenter.GetComponent<Collider>());

        // Stamen (center spike)
        GameObject stamen = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stamen.name = "Stamen";
        stamen.transform.SetParent(pottedFlower.transform);
        stamen.transform.localPosition = new Vector3(0, 0.82f, 0);
        stamen.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);
        Material stamenMat = new Material(Shader.Find("Standard"));
        stamenMat.color = new Color(0.9f, 0.3f, 0.35f);
        stamen.GetComponent<Renderer>().sharedMaterial = stamenMat;
        Object.DestroyImmediate(stamen.GetComponent<Collider>());

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

    // ==================== ICE REALM ====================
    static void CreateIceRealm()
    {
        // Ice Realm is positioned at X offset of 500
        Vector3 realmOrigin = RealmManager.IceRealmOrigin;

        GameObject iceRealm = new GameObject("IceRealm");
        iceRealm.transform.position = realmOrigin;

        // Ice/Snow materials
        Material snowMat = new Material(Shader.Find("Standard"));
        snowMat.color = new Color(0.95f, 0.97f, 1f);
        snowMat.SetFloat("_Glossiness", 0.2f);

        Material iceMat = new Material(Shader.Find("Standard"));
        iceMat.color = new Color(0.7f, 0.85f, 0.95f);
        iceMat.SetFloat("_Glossiness", 0.8f);
        iceMat.SetFloat("_Metallic", 0.1f);

        Material darkIceMat = new Material(Shader.Find("Standard"));
        darkIceMat.color = new Color(0.4f, 0.6f, 0.75f);
        darkIceMat.SetFloat("_Glossiness", 0.9f);

        Material frozenWaterMat = new Material(Shader.Find("Standard"));
        frozenWaterMat.color = new Color(0.3f, 0.5f, 0.7f, 0.9f);
        frozenWaterMat.SetFloat("_Mode", 3);
        frozenWaterMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        frozenWaterMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        frozenWaterMat.EnableKeyword("_ALPHABLEND_ON");
        frozenWaterMat.SetFloat("_Glossiness", 0.95f);

        // === SNOWY GROUND (ring shape with hole in center for ice lake) ===
        // Create ground as multiple pieces around the central lake
        float groundY = 1.25f;
        float lakeRadius = 12f;
        float mapRadius = 38f;

        // Main ground (large area)
        GameObject snowGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
        snowGround.name = "SnowGround";
        snowGround.transform.SetParent(iceRealm.transform);
        snowGround.transform.localPosition = new Vector3(0, groundY, 0);
        snowGround.transform.localScale = new Vector3(mapRadius * 2, 0.5f, mapRadius * 2);
        snowGround.GetComponent<Renderer>().sharedMaterial = snowMat;

        // === CENTRAL ICE LAKE (fishing hole) ===
        CreateCentralIceLake(iceRealm.transform, Vector3.zero, lakeRadius, frozenWaterMat, darkIceMat);

        // === DISTANT MOUNTAINS (enclosing feel) ===
        CreateDistantMountains(iceRealm.transform, snowMat, iceMat);

        // === FROZEN WATER AROUND EDGES ===
        GameObject frozenWater = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frozenWater.name = "FrozenWater";
        frozenWater.transform.SetParent(iceRealm.transform);
        frozenWater.transform.localPosition = new Vector3(0, 0.3f, 0);
        frozenWater.transform.localScale = new Vector3(200, 0.3f, 200);
        frozenWater.GetComponent<Renderer>().sharedMaterial = frozenWaterMat;
        Object.DestroyImmediate(frozenWater.GetComponent<Collider>());

        // Invisible floor for edges
        GameObject iceFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        iceFloor.name = "IceFloor";
        iceFloor.transform.SetParent(iceRealm.transform);
        iceFloor.transform.localPosition = new Vector3(0, 0.4f, 0);
        iceFloor.transform.localScale = new Vector3(200, 0.1f, 200);
        iceFloor.GetComponent<Renderer>().enabled = false;

        // === ICE FORMATIONS scattered around ===
        CreateIceFormation(iceRealm.transform, new Vector3(-28f, groundY, 18f), iceMat, 5f);
        CreateIceFormation(iceRealm.transform, new Vector3(30f, groundY, -22f), iceMat, 6f);
        CreateIceFormation(iceRealm.transform, new Vector3(-18f, groundY, -28f), darkIceMat, 4f);
        CreateIceFormation(iceRealm.transform, new Vector3(25f, groundY, 25f), iceMat, 5f);

        // === DEAD TREES (leafless, skeletal) ===
        CreateDeadTree(iceRealm.transform, new Vector3(-22f, groundY, -18f), 7f);
        CreateDeadTree(iceRealm.transform, new Vector3(18f, groundY, -25f), 8f);
        CreateDeadTree(iceRealm.transform, new Vector3(-30f, groundY, 8f), 6f);
        CreateDeadTree(iceRealm.transform, new Vector3(28f, groundY, 15f), 9f);
        CreateDeadTree(iceRealm.transform, new Vector3(-15f, groundY, 28f), 7f);
        CreateDeadTree(iceRealm.transform, new Vector3(8f, groundY, -30f), 8f);
        // Trees for hiding weapon shop
        CreateDeadTree(iceRealm.transform, new Vector3(-32f, groundY, -25f), 10f);
        CreateDeadTree(iceRealm.transform, new Vector3(-28f, groundY, -28f), 8f);
        CreateDeadTree(iceRealm.transform, new Vector3(-35f, groundY, -22f), 9f);
        // Trees for hiding Bjork
        CreateDeadTree(iceRealm.transform, new Vector3(30f, groundY, -28f), 9f);
        CreateDeadTree(iceRealm.transform, new Vector3(33f, groundY, -25f), 7f);

        // === SNOW-COVERED PINE TREES ===
        CreateSnowyPineTree(iceRealm.transform, new Vector3(-25f, groundY, 5f), 8f);
        CreateSnowyPineTree(iceRealm.transform, new Vector3(20f, groundY, -8f), 10f);
        CreateSnowyPineTree(iceRealm.transform, new Vector3(-12f, groundY, -32f), 7f);
        CreateSnowyPineTree(iceRealm.transform, new Vector3(32f, groundY, 5f), 9f);

        // === RETURN PORTAL (edge of map, not center) ===
        CreateReturnPortal(iceRealm.transform, new Vector3(0, groundY - 0.75f, -35f), "TropicalPortal", RealmType.TropicalIsland);

        // === IGLOO SHOP (CENTER OF MAP - native old woman inside with fire) ===
        CreateIglooShop(iceRealm.transform, new Vector3(0, groundY, 0));

        // === ICE REALM DOCK (facing out to sea with BBQ and radio) ===
        CreateIceRealmDock(iceRealm.transform, groundY);

        // === WEAPON SHOP (Pik, hidden behind dead trees) ===
        CreateWeaponShop(iceRealm.transform, new Vector3(-30f, groundY, -26f));

        // === BJORK THE HUNTSMAN (hidden behind trees with fire) ===
        CreateBjorkNPC(iceRealm.transform, new Vector3(32f, groundY, -26f));

        // === POLAR BEARS (3 enemies patrolling) ===
        CreatePolarBear(iceRealm.transform, new Vector3(-20f, groundY, 15f));
        CreatePolarBear(iceRealm.transform, new Vector3(22f, groundY, -15f));
        CreatePolarBear(iceRealm.transform, new Vector3(0f, groundY, -25f));

        // === SNOW PARTICLE SYSTEM ===
        GameObject snowSystem = new GameObject("SnowParticles");
        snowSystem.transform.SetParent(iceRealm.transform);
        snowSystem.transform.localPosition = Vector3.zero;
        snowSystem.AddComponent<SnowParticles>();

        // === WIND AMBIENCE ===
        GameObject windObj = new GameObject("WindAmbience");
        windObj.transform.SetParent(iceRealm.transform);
        windObj.AddComponent<WindAmbience>();

        // === WEAPON SYSTEM (for Ice Realm combat) ===
        GameObject weaponSystem = new GameObject("WeaponSystem");
        weaponSystem.transform.SetParent(iceRealm.transform);
        weaponSystem.AddComponent<WeaponSystem>();

        Debug.Log("Ice Realm created with central ice lake, shops, NPCs, and polar bears!");
    }

    static void CreateCentralIceLake(Transform parent, Vector3 center, float radius, Material waterMat, Material iceMat)
    {
        GameObject lake = new GameObject("CentralIceLake");
        lake.transform.SetParent(parent);
        lake.transform.localPosition = center;

        // Cut a circular hole in the ground (we'll visually cover it)
        // Water surface (fishable area)
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water.name = "LakeWater";
        water.transform.SetParent(lake.transform);
        water.transform.localPosition = new Vector3(0, 0.5f, 0);
        water.transform.localScale = new Vector3(radius * 2, 0.3f, radius * 2);
        water.GetComponent<Renderer>().sharedMaterial = waterMat;
        Object.DestroyImmediate(water.GetComponent<Collider>());

        // Ice rim around the lake
        int rimSegments = 16;
        for (int i = 0; i < rimSegments; i++)
        {
            float angle = i * (360f / rimSegments) * Mathf.Deg2Rad;
            float nextAngle = (i + 1) * (360f / rimSegments) * Mathf.Deg2Rad;

            GameObject iceBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            iceBlock.name = "IceRim" + i;
            iceBlock.transform.SetParent(lake.transform);

            float x = Mathf.Cos(angle) * (radius + 0.5f);
            float z = Mathf.Sin(angle) * (radius + 0.5f);
            iceBlock.transform.localPosition = new Vector3(x, 1f, z);
            iceBlock.transform.localScale = new Vector3(3f, 1.5f + Random.Range(0f, 0.5f), 2f);
            iceBlock.transform.localRotation = Quaternion.Euler(
                Random.Range(-5f, 5f),
                -angle * Mathf.Rad2Deg + 90,
                Random.Range(-5f, 5f)
            );
            iceBlock.GetComponent<Renderer>().sharedMaterial = iceMat;
        }

        // Floating ice chunks in the water
        for (int i = 0; i < 8; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(2f, radius - 2f);

            GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunk.name = "IceChunk" + i;
            chunk.transform.SetParent(lake.transform);
            chunk.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * dist,
                0.7f,
                Mathf.Sin(angle) * dist
            );
            chunk.transform.localScale = new Vector3(
                Random.Range(0.5f, 1.5f),
                Random.Range(0.3f, 0.6f),
                Random.Range(0.5f, 1.5f)
            );
            chunk.transform.localRotation = Quaternion.Euler(
                Random.Range(-10f, 10f),
                Random.Range(0f, 360f),
                Random.Range(-10f, 10f)
            );
            chunk.GetComponent<Renderer>().sharedMaterial = iceMat;
            Object.DestroyImmediate(chunk.GetComponent<Collider>());
        }
    }

    static void CreateDistantMountains(Transform parent, Material snowMat, Material iceMat)
    {
        GameObject mountains = new GameObject("DistantMountains");
        mountains.transform.SetParent(parent);
        mountains.transform.localPosition = Vector3.zero;

        // Create mountains in a ring around the map
        float mountainDistance = 80f;
        int mountainCount = 12;

        for (int i = 0; i < mountainCount; i++)
        {
            float angle = i * (360f / mountainCount) * Mathf.Deg2Rad;
            float angleVariation = Random.Range(-15f, 15f) * Mathf.Deg2Rad;

            GameObject mountain = new GameObject("Mountain" + i);
            mountain.transform.SetParent(mountains.transform);

            float x = Mathf.Cos(angle + angleVariation) * (mountainDistance + Random.Range(-10f, 10f));
            float z = Mathf.Sin(angle + angleVariation) * (mountainDistance + Random.Range(-10f, 10f));
            mountain.transform.localPosition = new Vector3(x, 0, z);

            // Create mountain shape (multiple peaks)
            int peaks = Random.Range(1, 4);
            for (int p = 0; p < peaks; p++)
            {
                float peakHeight = Random.Range(25f, 50f);
                float peakWidth = Random.Range(15f, 30f);

                GameObject peak = GameObject.CreatePrimitive(PrimitiveType.Cube);
                peak.name = "Peak" + p;
                peak.transform.SetParent(mountain.transform);
                peak.transform.localPosition = new Vector3(p * 8f - (peaks - 1) * 4f, peakHeight / 2, 0);
                peak.transform.localScale = new Vector3(peakWidth, peakHeight, peakWidth * 0.8f);
                peak.transform.localRotation = Quaternion.Euler(0, Random.Range(-20f, 20f), 0);

                // Gradient color - darker at base, whiter at top
                Material peakMat = new Material(Shader.Find("Standard"));
                float whiteness = 0.7f + (peakHeight / 50f) * 0.3f;
                peakMat.color = new Color(whiteness, whiteness + 0.02f, whiteness + 0.05f);
                peak.GetComponent<Renderer>().sharedMaterial = peakMat;
                Object.DestroyImmediate(peak.GetComponent<Collider>());

                // Snow cap
                GameObject snowCap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                snowCap.name = "SnowCap";
                snowCap.transform.SetParent(mountain.transform);
                snowCap.transform.localPosition = new Vector3(p * 8f - (peaks - 1) * 4f, peakHeight * 0.85f, 0);
                snowCap.transform.localScale = new Vector3(peakWidth * 0.7f, peakHeight * 0.3f, peakWidth * 0.6f);
                snowCap.transform.localRotation = peak.transform.localRotation;
                snowCap.GetComponent<Renderer>().sharedMaterial = snowMat;
                Object.DestroyImmediate(snowCap.GetComponent<Collider>());
            }
        }
    }

    static void CreateDeadTree(Transform parent, Vector3 localPos, float height)
    {
        GameObject tree = new GameObject("DeadTree");
        tree.transform.SetParent(parent);
        tree.transform.localPosition = localPos;

        Material barkMat = new Material(Shader.Find("Standard"));
        barkMat.color = new Color(0.25f, 0.2f, 0.15f);

        // Main trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, height * 0.4f, 0);
        trunk.transform.localScale = new Vector3(0.25f, height * 0.4f, 0.25f);
        trunk.transform.localRotation = Quaternion.Euler(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        trunk.GetComponent<Renderer>().sharedMaterial = barkMat;
        Object.DestroyImmediate(trunk.GetComponent<Collider>());

        // Dead branches (skeletal)
        int branchCount = Random.Range(4, 7);
        for (int i = 0; i < branchCount; i++)
        {
            float branchY = height * (0.3f + i * 0.12f);
            float branchAngle = i * 60f + Random.Range(-20f, 20f);
            float branchLength = Random.Range(1f, 2.5f);

            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            branch.name = "Branch" + i;
            branch.transform.SetParent(tree.transform);

            float rad = branchAngle * Mathf.Deg2Rad;
            branch.transform.localPosition = new Vector3(
                Mathf.Cos(rad) * branchLength * 0.5f,
                branchY,
                Mathf.Sin(rad) * branchLength * 0.5f
            );
            branch.transform.localScale = new Vector3(0.08f, branchLength * 0.5f, 0.08f);
            branch.transform.localRotation = Quaternion.Euler(
                -60 + Random.Range(-20f, 20f),
                branchAngle,
                0
            );
            branch.GetComponent<Renderer>().sharedMaterial = barkMat;
            Object.DestroyImmediate(branch.GetComponent<Collider>());
        }
    }

    static void CreateIglooShop(Transform parent, Vector3 localPos)
    {
        // BIG SHELTER for Elder NPC (not an igloo - open shelter with fire)
        GameObject shelter = new GameObject("ElderShelter");
        shelter.transform.SetParent(parent);
        shelter.transform.localPosition = localPos;

        // Materials
        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = new Color(0.5f, 0.42f, 0.35f);
        furMat.SetFloat("_Glossiness", 0.1f);

        Material poleMat = new Material(Shader.Find("Standard"));
        poleMat.color = new Color(0.35f, 0.25f, 0.18f);

        Material darkPoleMat = new Material(Shader.Find("Standard"));
        darkPoleMat.color = new Color(0.25f, 0.18f, 0.12f);

        Material snowMat = new Material(Shader.Find("Standard"));
        snowMat.color = new Color(0.95f, 0.97f, 1f);

        Material stoneMat = new Material(Shader.Find("Standard"));
        stoneMat.color = new Color(0.35f, 0.35f, 0.4f);

        // === LARGE SHELTER STRUCTURE ===
        float shelterRadius = 5f;
        float shelterHeight = 4.5f;

        // Main support poles (8 poles in a circle, leaning inward)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "ShelterPole";
            pole.transform.SetParent(shelter.transform);
            pole.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * shelterRadius * 0.6f,
                shelterHeight / 2f,
                Mathf.Sin(angle) * shelterRadius * 0.6f
            );
            pole.transform.localRotation = Quaternion.Euler(
                Mathf.Sin(angle) * 12f,
                -angle * Mathf.Rad2Deg,
                Mathf.Cos(angle) * 12f
            );
            pole.transform.localScale = new Vector3(0.2f, shelterHeight / 2f, 0.2f);
            pole.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? poleMat : darkPoleMat;
            Object.DestroyImmediate(pole.GetComponent<Collider>());
        }

        // Central support pole
        GameObject centerPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        centerPole.name = "CenterPole";
        centerPole.transform.SetParent(shelter.transform);
        centerPole.transform.localPosition = new Vector3(0, shelterHeight / 2f + 0.5f, 0);
        centerPole.transform.localScale = new Vector3(0.25f, shelterHeight / 2f + 0.5f, 0.25f);
        centerPole.GetComponent<Renderer>().sharedMaterial = darkPoleMat;
        Object.DestroyImmediate(centerPole.GetComponent<Collider>());

        // Large fur/hide roof covering
        GameObject roofMain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        roofMain.name = "RoofMain";
        roofMain.transform.SetParent(shelter.transform);
        roofMain.transform.localPosition = new Vector3(0, shelterHeight - 0.5f, 0);
        roofMain.transform.localScale = new Vector3(shelterRadius * 2.2f, shelterHeight * 0.7f, shelterRadius * 2.2f);
        roofMain.GetComponent<Renderer>().sharedMaterial = furMat;
        Object.DestroyImmediate(roofMain.GetComponent<Collider>());

        // Lower roof sections (draped furs)
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            GameObject roofSection = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofSection.name = "RoofDrape";
            roofSection.transform.SetParent(shelter.transform);
            roofSection.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * shelterRadius * 0.8f,
                shelterHeight * 0.4f,
                Mathf.Sin(angle) * shelterRadius * 0.8f
            );
            roofSection.transform.localRotation = Quaternion.Euler(
                Random.Range(-15f, 15f),
                -angle * Mathf.Rad2Deg,
                Random.Range(-10f, 10f)
            );
            roofSection.transform.localScale = new Vector3(2.5f, 2f, 0.15f);
            roofSection.GetComponent<Renderer>().sharedMaterial = furMat;
            Object.DestroyImmediate(roofSection.GetComponent<Collider>());
        }

        // Snow accumulation on roof
        for (int i = 0; i < 5; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(1f, 3f);
            GameObject snowPatch = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            snowPatch.name = "RoofSnow";
            snowPatch.transform.SetParent(shelter.transform);
            snowPatch.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * dist,
                shelterHeight + Random.Range(-0.5f, 0.5f),
                Mathf.Sin(angle) * dist
            );
            snowPatch.transform.localScale = new Vector3(
                Random.Range(0.8f, 1.5f),
                Random.Range(0.3f, 0.5f),
                Random.Range(0.8f, 1.5f)
            );
            snowPatch.GetComponent<Renderer>().sharedMaterial = snowMat;
            Object.DestroyImmediate(snowPatch.GetComponent<Collider>());
        }

        // === LARGE CENTRAL FIRE ===
        GameObject fire = new GameObject("CentralFire");
        fire.transform.SetParent(shelter.transform);
        fire.transform.localPosition = new Vector3(0, 0.3f, 0);

        // Large fire pit ring (stones)
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f * Mathf.Deg2Rad;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = "FireStone";
            stone.transform.SetParent(fire.transform);
            stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * 1.2f, 0, Mathf.Sin(angle) * 1.2f);
            stone.transform.localScale = new Vector3(
                Random.Range(0.35f, 0.5f),
                Random.Range(0.25f, 0.35f),
                Random.Range(0.35f, 0.5f)
            );
            stone.GetComponent<Renderer>().sharedMaterial = stoneMat;
            Object.DestroyImmediate(stone.GetComponent<Collider>());
        }

        // Fire flames - large roaring fire
        Material fireMat = new Material(Shader.Find("Standard"));
        fireMat.color = new Color(1f, 0.5f, 0.1f);
        fireMat.EnableKeyword("_EMISSION");
        fireMat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * 5f);

        Material fireOrangeMat = new Material(Shader.Find("Standard"));
        fireOrangeMat.color = new Color(1f, 0.3f, 0f);
        fireOrangeMat.EnableKeyword("_EMISSION");
        fireOrangeMat.SetColor("_EmissionColor", new Color(1f, 0.2f, 0f) * 4f);

        Material fireYellowMat = new Material(Shader.Find("Standard"));
        fireYellowMat.color = new Color(1f, 0.8f, 0.2f);
        fireYellowMat.EnableKeyword("_EMISSION");
        fireYellowMat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.1f) * 6f);

        // Main large flame
        GameObject mainFlame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mainFlame.name = "MainFlame";
        mainFlame.transform.SetParent(fire.transform);
        mainFlame.transform.localPosition = new Vector3(0, 1f, 0);
        mainFlame.transform.localScale = new Vector3(1.2f, 2f, 1.2f);
        mainFlame.GetComponent<Renderer>().sharedMaterial = fireMat;
        Object.DestroyImmediate(mainFlame.GetComponent<Collider>());

        // Inner bright flame
        GameObject innerFlame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerFlame.name = "InnerFlame";
        innerFlame.transform.SetParent(fire.transform);
        innerFlame.transform.localPosition = new Vector3(0, 0.8f, 0);
        innerFlame.transform.localScale = new Vector3(0.7f, 1.4f, 0.7f);
        innerFlame.GetComponent<Renderer>().sharedMaterial = fireYellowMat;
        Object.DestroyImmediate(innerFlame.GetComponent<Collider>());

        // Multiple side flames for roaring effect
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f * Mathf.Deg2Rad;
            GameObject sideFlame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sideFlame.name = "SideFlame";
            sideFlame.transform.SetParent(fire.transform);
            sideFlame.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.5f,
                0.6f + Random.Range(0f, 0.3f),
                Mathf.Sin(angle) * 0.5f
            );
            sideFlame.transform.localScale = new Vector3(0.5f, 1.2f, 0.5f);
            sideFlame.GetComponent<Renderer>().sharedMaterial = fireOrangeMat;
            Object.DestroyImmediate(sideFlame.GetComponent<Collider>());
        }

        // Bright fire light
        GameObject lightObj = new GameObject("FireLight");
        lightObj.transform.SetParent(fire.transform);
        lightObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        Light fireLight = lightObj.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = new Color(1f, 0.6f, 0.3f);
        fireLight.intensity = 6f;
        fireLight.range = 20f;

        // Large logs in fire
        Material logMat = new Material(Shader.Find("Standard"));
        logMat.color = new Color(0.25f, 0.15f, 0.1f);
        for (int i = 0; i < 6; i++)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.name = "FireLog";
            log.transform.SetParent(fire.transform);
            log.transform.localPosition = new Vector3(0, 0.2f, 0);
            log.transform.localRotation = Quaternion.Euler(75f, i * 60f, 0);
            log.transform.localScale = new Vector3(0.15f, 0.8f, 0.15f);
            log.GetComponent<Renderer>().sharedMaterial = logMat;
            Object.DestroyImmediate(log.GetComponent<Collider>());
        }

        // === DECORATIONS ===
        // Fur rugs/mats on ground
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad + 30f * Mathf.Deg2Rad;
            GameObject furRug = GameObject.CreatePrimitive(PrimitiveType.Cube);
            furRug.name = "FurRug";
            furRug.transform.SetParent(shelter.transform);
            furRug.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 2.5f,
                0.08f,
                Mathf.Sin(angle) * 2.5f
            );
            furRug.transform.localRotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg + Random.Range(-15f, 15f), 0);
            furRug.transform.localScale = new Vector3(1.8f, 0.15f, 2.5f);
            furRug.GetComponent<Renderer>().sharedMaterial = furMat;
            Object.DestroyImmediate(furRug.GetComponent<Collider>());
        }

        // Hanging items (dried fish, furs)
        for (int i = 0; i < 4; i++)
        {
            float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
            GameObject hanging = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hanging.name = "HangingItem";
            hanging.transform.SetParent(shelter.transform);
            hanging.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 3f,
                shelterHeight * 0.6f,
                Mathf.Sin(angle) * 3f
            );
            hanging.transform.localRotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, Random.Range(-10f, 10f));
            hanging.transform.localScale = new Vector3(0.4f, 1f, 0.1f);
            hanging.GetComponent<Renderer>().sharedMaterial = furMat;
            Object.DestroyImmediate(hanging.GetComponent<Collider>());
        }

        // Add shop NPC component
        shelter.AddComponent<IceRealmShopNPC>();

        // Create the native elder NPC - sitting by the fire
        CreateIceRealmShopkeeper(shelter.transform, new Vector3(2f, 0, 0));
    }

    static void CreateIceRealmShopkeeper(Transform parent, Vector3 localPos)
    {
        GameObject elder = new GameObject("FrostElder");
        elder.transform.SetParent(parent);
        elder.transform.localPosition = localPos;
        elder.transform.localRotation = Quaternion.Euler(0, 0, 0); // Face the entrance

        Material skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.7f, 0.55f, 0.45f);

        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = new Color(0.5f, 0.4f, 0.35f);

        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.8f, 0.8f, 0.85f);

        // Body (fur clothing)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(elder.transform);
        body.transform.localPosition = new Vector3(0, 0.7f, 0);
        body.transform.localScale = new Vector3(0.4f, 0.5f, 0.3f);
        body.GetComponent<Renderer>().sharedMaterial = furMat;
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(elder.transform);
        head.transform.localPosition = new Vector3(0, 1.35f, 0);
        head.transform.localScale = new Vector3(0.28f, 0.32f, 0.28f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // White hair
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.name = "Hair";
        hair.transform.SetParent(elder.transform);
        hair.transform.localPosition = new Vector3(0, 1.45f, -0.05f);
        hair.transform.localScale = new Vector3(0.3f, 0.15f, 0.28f);
        hair.GetComponent<Renderer>().sharedMaterial = hairMat;
        Object.DestroyImmediate(hair.GetComponent<Collider>());

        // Hood
        GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hood.name = "Hood";
        hood.transform.SetParent(elder.transform);
        hood.transform.localPosition = new Vector3(0, 1.48f, -0.08f);
        hood.transform.localScale = new Vector3(0.38f, 0.22f, 0.35f);
        hood.GetComponent<Renderer>().sharedMaterial = furMat;
        Object.DestroyImmediate(hood.GetComponent<Collider>());

        // Eyes
        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = new Color(0.3f, 0.35f, 0.4f);
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(elder.transform);
            eye.transform.localPosition = new Vector3(side * 0.06f, 1.38f, 0.12f);
            eye.transform.localScale = Vector3.one * 0.04f;
            eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
            Object.DestroyImmediate(eye.GetComponent<Collider>());
        }
    }

    static void CreateIceRealmDock(Transform parent, float groundY)
    {
        // Ice Realm dock - facing out to frozen sea, with BBQ and radio like tropical
        GameObject dockParent = new GameObject("IceRealmDock");
        dockParent.transform.SetParent(parent);
        dockParent.transform.localPosition = new Vector3(0, 0, 25f); // Face out to sea (+Z direction)

        // Wood materials with more variation for detail
        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.38f, 0.28f, 0.20f); // Weathered wood
        woodMat.SetFloat("_Glossiness", 0.1f);

        Material darkWood = new Material(Shader.Find("Standard"));
        darkWood.color = new Color(0.22f, 0.14f, 0.08f);
        darkWood.SetFloat("_Glossiness", 0.05f);

        Material lightWood = new Material(Shader.Find("Standard"));
        lightWood.color = new Color(0.45f, 0.35f, 0.25f);
        lightWood.SetFloat("_Glossiness", 0.15f);

        Material snowMat = new Material(Shader.Find("Standard"));
        snowMat.color = new Color(0.95f, 0.97f, 1f);
        snowMat.SetFloat("_Glossiness", 0.3f);

        Material iceMat = new Material(Shader.Find("Standard"));
        iceMat.color = new Color(0.7f, 0.85f, 0.95f, 0.8f);
        iceMat.SetFloat("_Glossiness", 0.9f);
        iceMat.SetFloat("_Metallic", 0.1f);

        // Dock dimensions
        float dockStartZ = 0f;
        float dockEndZ = 40f;
        float dockWidth = 5f;
        float dockHeight = groundY + 1f;
        float legHeight = 3.5f;

        // MAIN DOCK SURFACE - HAS COLLIDER for walking
        GameObject dockSurface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dockSurface.name = "DockSurface";
        dockSurface.transform.SetParent(dockParent.transform);
        dockSurface.transform.localPosition = new Vector3(0, dockHeight, (dockStartZ + dockEndZ) / 2f);
        dockSurface.transform.localScale = new Vector3(dockWidth, 0.3f, dockEndZ - dockStartZ);
        dockSurface.GetComponent<Renderer>().sharedMaterial = woodMat;
        // KEEP COLLIDER

        // === INDIVIDUAL PLANKS for detail ===
        float plankWidth = 0.4f;
        int numPlanks = (int)((dockEndZ - dockStartZ) / plankWidth);
        for (int i = 0; i < numPlanks; i += 3) // Every 3rd position
        {
            float z = dockStartZ + 1f + i * plankWidth;
            // Alternate plank colors for weathered look
            Material plankMat = (i % 6 == 0) ? lightWood : ((i % 6 == 3) ? darkWood : woodMat);

            // Plank line
            GameObject plankLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plankLine.name = "PlankLine";
            plankLine.transform.SetParent(dockParent.transform);
            plankLine.transform.localPosition = new Vector3(0, dockHeight + 0.16f, z);
            plankLine.transform.localScale = new Vector3(dockWidth - 0.1f, 0.02f, 0.05f);
            plankLine.GetComponent<Renderer>().sharedMaterial = darkWood;
            Object.DestroyImmediate(plankLine.GetComponent<Collider>());
        }

        // === RAILINGS on both sides ===
        for (int side = -1; side <= 1; side += 2)
        {
            float railX = side * (dockWidth / 2f - 0.1f);

            // Railing posts
            for (float z = dockStartZ + 5f; z < dockEndZ - 2f; z += 5f)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "RailingPost";
                post.transform.SetParent(dockParent.transform);
                post.transform.localPosition = new Vector3(railX, dockHeight + 0.5f, z);
                post.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
                post.GetComponent<Renderer>().sharedMaterial = darkWood;
                Object.DestroyImmediate(post.GetComponent<Collider>());

                // Post cap
                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cap.name = "PostCap";
                cap.transform.SetParent(dockParent.transform);
                cap.transform.localPosition = new Vector3(railX, dockHeight + 1.0f, z);
                cap.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
                cap.GetComponent<Renderer>().sharedMaterial = lightWood;
                Object.DestroyImmediate(cap.GetComponent<Collider>());
            }

            // Top rail
            GameObject topRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topRail.name = "TopRail";
            topRail.transform.SetParent(dockParent.transform);
            topRail.transform.localPosition = new Vector3(railX, dockHeight + 0.9f, (dockStartZ + dockEndZ) / 2f);
            topRail.transform.localScale = new Vector3(0.08f, 0.06f, dockEndZ - dockStartZ - 8f);
            topRail.GetComponent<Renderer>().sharedMaterial = woodMat;
            Object.DestroyImmediate(topRail.GetComponent<Collider>());

            // Bottom rail
            GameObject bottomRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bottomRail.name = "BottomRail";
            bottomRail.transform.SetParent(dockParent.transform);
            bottomRail.transform.localPosition = new Vector3(railX, dockHeight + 0.4f, (dockStartZ + dockEndZ) / 2f);
            bottomRail.transform.localScale = new Vector3(0.06f, 0.04f, dockEndZ - dockStartZ - 8f);
            bottomRail.GetComponent<Renderer>().sharedMaterial = darkWood;
            Object.DestroyImmediate(bottomRail.GetComponent<Collider>());
        }

        // === SNOW ACCUMULATION on dock ===
        // Scattered snow piles
        for (int i = 0; i < 8; i++)
        {
            float z = dockStartZ + 5f + i * 4.5f;
            int snowSide = (i % 2 == 0) ? -1 : 1;

            GameObject snowPile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            snowPile.name = "SnowPile";
            snowPile.transform.SetParent(dockParent.transform);
            snowPile.transform.localPosition = new Vector3(snowSide * Random.Range(1.5f, 2.2f), dockHeight + 0.15f, z + Random.Range(-1f, 1f));
            snowPile.transform.localScale = new Vector3(Random.Range(0.3f, 0.6f), 0.2f, Random.Range(0.4f, 0.8f));
            snowPile.GetComponent<Renderer>().sharedMaterial = snowMat;
            Object.DestroyImmediate(snowPile.GetComponent<Collider>());
        }

        // === ICICLES hanging from dock ===
        for (float z = dockStartZ + 3f; z < dockEndZ - 3f; z += 4f)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                int numIcicles = Random.Range(2, 5);
                for (int i = 0; i < numIcicles; i++)
                {
                    GameObject icicle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    icicle.name = "Icicle";
                    icicle.transform.SetParent(dockParent.transform);
                    float icicleLen = Random.Range(0.3f, 0.8f);
                    icicle.transform.localPosition = new Vector3(
                        side * (dockWidth / 2f - 0.1f) + Random.Range(-0.1f, 0.1f),
                        dockHeight - 0.15f - icicleLen / 2f,
                        z + Random.Range(-0.5f, 0.5f)
                    );
                    icicle.transform.localScale = new Vector3(0.04f, icicleLen, 0.04f);
                    icicle.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
                    icicle.GetComponent<Renderer>().sharedMaterial = iceMat;
                    Object.DestroyImmediate(icicle.GetComponent<Collider>());
                }
            }
        }

        // Support LEGS with more detail
        float[] legPositions = { 5f, 15f, 25f, 35f };
        foreach (float zPos in legPositions)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                // Main leg
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = "DockLeg";
                leg.transform.SetParent(dockParent.transform);
                leg.transform.localPosition = new Vector3(side * 2f, dockHeight - legHeight / 2f, zPos);
                leg.transform.localScale = new Vector3(0.35f, legHeight / 2f, 0.35f);
                leg.GetComponent<Renderer>().sharedMaterial = darkWood;
                Object.DestroyImmediate(leg.GetComponent<Collider>());

                // Diagonal brace
                GameObject brace = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                brace.name = "DockBrace";
                brace.transform.SetParent(dockParent.transform);
                brace.transform.localPosition = new Vector3(side * 1.5f, dockHeight - 1.2f, zPos);
                brace.transform.localRotation = Quaternion.Euler(0, 0, side * 35f);
                brace.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);
                brace.GetComponent<Renderer>().sharedMaterial = darkWood;
                Object.DestroyImmediate(brace.GetComponent<Collider>());

                // Cross brace
                GameObject crossBrace = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crossBrace.name = "CrossBrace";
                crossBrace.transform.SetParent(dockParent.transform);
                crossBrace.transform.localPosition = new Vector3(side * 1.2f, dockHeight - 2f, zPos);
                crossBrace.transform.localRotation = Quaternion.Euler(0, 0, side * -25f);
                crossBrace.transform.localScale = new Vector3(0.08f, 0.8f, 0.08f);
                crossBrace.GetComponent<Renderer>().sharedMaterial = lightWood;
                Object.DestroyImmediate(crossBrace.GetComponent<Collider>());

                // Ice on legs
                GameObject legIce = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                legIce.name = "LegIce";
                legIce.transform.SetParent(dockParent.transform);
                legIce.transform.localPosition = new Vector3(side * 2f, dockHeight - legHeight + 0.3f, zPos);
                legIce.transform.localScale = new Vector3(0.45f, 0.3f, 0.45f);
                legIce.GetComponent<Renderer>().sharedMaterial = iceMat;
                Object.DestroyImmediate(legIce.GetComponent<Collider>());
            }
        }

        // Cross beams under dock
        for (int i = 0; i < 8; i++)
        {
            float z = dockStartZ + 3 + i * 5f;
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "CrossBeam";
            beam.transform.SetParent(dockParent.transform);
            beam.transform.localPosition = new Vector3(0, dockHeight - 0.25f, z);
            beam.transform.localScale = new Vector3(dockWidth + 0.3f, 0.12f, 0.18f);
            beam.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? darkWood : woodMat;
            Object.DestroyImmediate(beam.GetComponent<Collider>());
        }

        // STAIRCASE from ground to dock with details
        float stairStartZ = dockStartZ - 3f;
        int numSteps = 4;
        float stepHeight = (dockHeight - groundY) / numSteps;
        float stepDepth = 0.6f;

        for (int i = 0; i < numSteps; i++)
        {
            GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
            step.name = "DockStair_" + i;
            step.transform.SetParent(dockParent.transform);
            float stepY = groundY + stepHeight * (i + 0.5f);
            float stepZ = stairStartZ + stepDepth * i;
            step.transform.localPosition = new Vector3(0, stepY, stepZ);
            step.transform.localScale = new Vector3(dockWidth, stepHeight, stepDepth);
            step.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? woodMat : lightWood;
            // KEEP COLLIDER

            // Snow on step edge
            if (i > 0)
            {
                GameObject stepSnow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stepSnow.name = "StepSnow";
                stepSnow.transform.SetParent(dockParent.transform);
                stepSnow.transform.localPosition = new Vector3(0, stepY + stepHeight / 2f + 0.05f, stepZ + stepDepth / 2f - 0.1f);
                stepSnow.transform.localScale = new Vector3(dockWidth * 0.9f, 0.08f, 0.15f);
                stepSnow.GetComponent<Renderer>().sharedMaterial = snowMat;
                Object.DestroyImmediate(stepSnow.GetComponent<Collider>());
            }
        }

        // === BBQ at end of dock (uses BBQStation component - creates its own model) ===
        GameObject bbq = new GameObject("IceRealmBBQ");
        bbq.transform.SetParent(dockParent.transform);
        bbq.transform.localPosition = new Vector3(-1.5f, dockHeight + 0.1f, dockEndZ - 3f);
        bbq.AddComponent<BBQStation>(); // Same BBQ as tropical - E to open

        // === RADIO next to BBQ (uses DockRadio component - creates its own model) ===
        GameObject radio = new GameObject("IceRealmRadio");
        radio.transform.SetParent(dockParent.transform);
        radio.transform.localPosition = new Vector3(1.5f, dockHeight + 0.1f, dockEndZ - 2f);
        radio.transform.localRotation = Quaternion.Euler(0, -30, 0);
        radio.AddComponent<DockRadio>(); // Same radio as tropical - F to toggle

        // Rope coil
        GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.name = "RopeCoil";
        rope.transform.SetParent(dockParent.transform);
        rope.transform.localPosition = new Vector3(2f, dockHeight + 0.2f, dockEndZ - 5f);
        rope.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        Material ropeMat = new Material(Shader.Find("Standard"));
        ropeMat.color = new Color(0.55f, 0.45f, 0.30f);
        rope.GetComponent<Renderer>().sharedMaterial = ropeMat;
        Object.DestroyImmediate(rope.GetComponent<Collider>());

        // Ice fishing hole at end of dock
        Material waterMat = new Material(Shader.Find("Standard"));
        waterMat.color = new Color(0.3f, 0.5f, 0.7f, 0.8f);
        waterMat.SetFloat("_Mode", 3);
        waterMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        waterMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        waterMat.EnableKeyword("_ALPHABLEND_ON");

        GameObject fishingHole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fishingHole.name = "FishingHole";
        fishingHole.transform.SetParent(dockParent.transform);
        fishingHole.transform.localPosition = new Vector3(0, dockHeight - 0.1f, dockEndZ - 1f);
        fishingHole.transform.localScale = new Vector3(2f, 0.1f, 2f);
        fishingHole.GetComponent<Renderer>().sharedMaterial = waterMat;
        fishingHole.tag = "Water"; // For fishing detection
        Object.DestroyImmediate(fishingHole.GetComponent<Collider>());

        // Ice around fishing hole
        Material iceMat = new Material(Shader.Find("Standard"));
        iceMat.color = new Color(0.8f, 0.9f, 1f);
        iceMat.SetFloat("_Glossiness", 0.9f);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            GameObject iceChunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            iceChunk.name = "IceChunk";
            iceChunk.transform.SetParent(dockParent.transform);
            iceChunk.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 1.2f,
                dockHeight + 0.1f,
                dockEndZ - 1f + Mathf.Sin(angle) * 1.2f
            );
            iceChunk.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
            iceChunk.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
            iceChunk.GetComponent<Renderer>().sharedMaterial = iceMat;
            Object.DestroyImmediate(iceChunk.GetComponent<Collider>());
        }
    }

    static void CreateWeaponShop(Transform parent, Vector3 localPos)
    {
        GameObject shop = new GameObject("WeaponShop");
        shop.transform.SetParent(parent);
        shop.transform.localPosition = localPos;

        // BIGGER shelter structure
        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = new Color(0.45f, 0.38f, 0.3f); // Animal hide color

        Material poleMat = new Material(Shader.Find("Standard"));
        poleMat.color = new Color(0.35f, 0.25f, 0.15f);

        Material snowMat = new Material(Shader.Find("Standard"));
        snowMat.color = new Color(0.95f, 0.95f, 1f);

        // Large shelter structure - 6 poles in bigger circle
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole" + i;
            pole.transform.SetParent(shop.transform);
            pole.transform.localPosition = new Vector3(Mathf.Cos(angle) * 3f, 2.5f, Mathf.Sin(angle) * 3f);
            pole.transform.localScale = new Vector3(0.15f, 2.5f, 0.15f);
            pole.transform.localRotation = Quaternion.Euler(12, -angle * Mathf.Rad2Deg, 0);
            pole.GetComponent<Renderer>().sharedMaterial = poleMat;
            Object.DestroyImmediate(pole.GetComponent<Collider>());
        }

        // Large fur tent cover
        GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cover.name = "TentCover";
        cover.transform.SetParent(shop.transform);
        cover.transform.localPosition = new Vector3(0, 3f, 0);
        cover.transform.localScale = new Vector3(8f, 4f, 8f);
        cover.GetComponent<Renderer>().sharedMaterial = furMat;
        Object.DestroyImmediate(cover.GetComponent<Collider>());

        // Snow on top of shelter
        GameObject snowCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        snowCap.name = "SnowCap";
        snowCap.transform.SetParent(shop.transform);
        snowCap.transform.localPosition = new Vector3(0, 4f, 0);
        snowCap.transform.localScale = new Vector3(4f, 1.5f, 4f);
        snowCap.GetComponent<Renderer>().sharedMaterial = snowMat;
        Object.DestroyImmediate(snowCap.GetComponent<Collider>());

        // Ground fur/hide mats
        for (int i = 0; i < 3; i++)
        {
            GameObject furMat2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            furMat2.name = "FurMat";
            furMat2.transform.SetParent(shop.transform);
            furMat2.transform.localPosition = new Vector3(i * 1.5f - 1.5f, 0.05f, 0);
            furMat2.transform.localScale = new Vector3(1.4f, 0.1f, 2f);
            furMat2.transform.localRotation = Quaternion.Euler(0, Random.Range(-10f, 10f), 0);
            furMat2.GetComponent<Renderer>().sharedMaterial = furMat;
            Object.DestroyImmediate(furMat2.GetComponent<Collider>());
        }

        // Weapon rack (wall of spears/knives) - bigger
        CreateWeaponRack(shop.transform, new Vector3(0, 0, -2.5f));

        // Campfire inside shelter
        GameObject fire = new GameObject("ShelterFire");
        fire.transform.SetParent(shop.transform);
        fire.transform.localPosition = new Vector3(-2f, 0.3f, 0);

        Material fireMat = new Material(Shader.Find("Standard"));
        fireMat.color = new Color(1f, 0.5f, 0.1f);
        fireMat.EnableKeyword("_EMISSION");
        fireMat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0.1f) * 3f);

        GameObject flames = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flames.name = "Flames";
        flames.transform.SetParent(fire.transform);
        flames.transform.localPosition = Vector3.zero;
        flames.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
        flames.GetComponent<Renderer>().sharedMaterial = fireMat;
        Object.DestroyImmediate(flames.GetComponent<Collider>());

        GameObject fireLight = new GameObject("FireLight");
        fireLight.transform.SetParent(fire.transform);
        Light fl = fireLight.AddComponent<Light>();
        fl.type = LightType.Point;
        fl.color = new Color(1f, 0.6f, 0.3f);
        fl.intensity = 2f;
        fl.range = 8f;

        // Add NPC component
        shop.AddComponent<WeaponShopNPC>();

        // Create Pik NPC - positioned visibly in front
        CreatePikNPC(shop.transform, new Vector3(1.5f, 0, 2f));
    }

    static void CreateWeaponRack(Transform parent, Vector3 localPos)
    {
        GameObject rack = new GameObject("WeaponRack");
        rack.transform.SetParent(parent);
        rack.transform.localPosition = localPos;

        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.4f, 0.28f, 0.18f);

        Material metalMat = new Material(Shader.Find("Standard"));
        metalMat.color = new Color(0.5f, 0.5f, 0.55f);
        metalMat.SetFloat("_Metallic", 0.7f);

        // Rack frame
        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "RackFrame";
        frame.transform.SetParent(rack.transform);
        frame.transform.localPosition = new Vector3(0, 1.2f, 0);
        frame.transform.localScale = new Vector3(3f, 2f, 0.15f);
        frame.GetComponent<Renderer>().sharedMaterial = woodMat;
        Object.DestroyImmediate(frame.GetComponent<Collider>());

        // Display weapons
        string[] weaponTypes = { "knife", "spear", "rapier", "lance" };
        for (int i = 0; i < 4; i++)
        {
            float x = -1.2f + i * 0.8f;
            CreateDisplayWeapon(rack.transform, new Vector3(x, 1.2f, 0.1f), weaponTypes[i], metalMat, woodMat);
        }
    }

    static void CreateDisplayWeapon(Transform parent, Vector3 pos, string type, Material metalMat, Material woodMat)
    {
        GameObject weapon = new GameObject("Display_" + type);
        weapon.transform.SetParent(parent);
        weapon.transform.localPosition = pos;
        weapon.transform.localRotation = Quaternion.Euler(0, 0, -30);

        switch (type)
        {
            case "knife":
                GameObject kBlade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                kBlade.transform.SetParent(weapon.transform);
                kBlade.transform.localPosition = new Vector3(0, 0.15f, 0);
                kBlade.transform.localScale = new Vector3(0.03f, 0.25f, 0.06f);
                kBlade.GetComponent<Renderer>().sharedMaterial = metalMat;
                Object.DestroyImmediate(kBlade.GetComponent<Collider>());

                GameObject kHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                kHandle.transform.SetParent(weapon.transform);
                kHandle.transform.localPosition = new Vector3(0, -0.05f, 0);
                kHandle.transform.localScale = new Vector3(0.04f, 0.1f, 0.04f);
                kHandle.GetComponent<Renderer>().sharedMaterial = woodMat;
                Object.DestroyImmediate(kHandle.GetComponent<Collider>());
                break;

            case "spear":
                GameObject sShaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                sShaft.transform.SetParent(weapon.transform);
                sShaft.transform.localPosition = Vector3.zero;
                sShaft.transform.localScale = new Vector3(0.03f, 0.5f, 0.03f);
                sShaft.GetComponent<Renderer>().sharedMaterial = woodMat;
                Object.DestroyImmediate(sShaft.GetComponent<Collider>());

                GameObject sTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sTip.transform.SetParent(weapon.transform);
                sTip.transform.localPosition = new Vector3(0, 0.55f, 0);
                sTip.transform.localScale = new Vector3(0.04f, 0.12f, 0.02f);
                sTip.GetComponent<Renderer>().sharedMaterial = metalMat;
                Object.DestroyImmediate(sTip.GetComponent<Collider>());
                break;

            case "rapier":
                GameObject rBlade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rBlade.transform.SetParent(weapon.transform);
                rBlade.transform.localPosition = new Vector3(0, 0.25f, 0);
                rBlade.transform.localScale = new Vector3(0.015f, 0.45f, 0.03f);
                rBlade.GetComponent<Renderer>().sharedMaterial = metalMat;
                Object.DestroyImmediate(rBlade.GetComponent<Collider>());

                Material goldMat = new Material(Shader.Find("Standard"));
                goldMat.color = new Color(0.9f, 0.75f, 0.3f);
                GameObject rGuard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rGuard.transform.SetParent(weapon.transform);
                rGuard.transform.localPosition = new Vector3(0, 0f, 0);
                rGuard.transform.localScale = new Vector3(0.12f, 0.02f, 0.06f);
                rGuard.GetComponent<Renderer>().sharedMaterial = goldMat;
                Object.DestroyImmediate(rGuard.GetComponent<Collider>());
                break;

            case "lance":
                GameObject lShaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lShaft.transform.SetParent(weapon.transform);
                lShaft.transform.localPosition = Vector3.zero;
                lShaft.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
                lShaft.GetComponent<Renderer>().sharedMaterial = woodMat;
                Object.DestroyImmediate(lShaft.GetComponent<Collider>());

                Material lGoldMat = new Material(Shader.Find("Standard"));
                lGoldMat.color = new Color(1f, 0.85f, 0.3f);
                lGoldMat.SetFloat("_Metallic", 0.9f);
                lGoldMat.EnableKeyword("_EMISSION");
                lGoldMat.SetColor("_EmissionColor", new Color(0.4f, 0.3f, 0.1f));

                GameObject lTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lTip.transform.SetParent(weapon.transform);
                lTip.transform.localPosition = new Vector3(0, 0.7f, 0);
                lTip.transform.localScale = new Vector3(0.08f, 0.2f, 0.03f);
                lTip.GetComponent<Renderer>().sharedMaterial = lGoldMat;
                Object.DestroyImmediate(lTip.GetComponent<Collider>());
                break;
        }
    }

    static void CreatePikNPC(Transform parent, Vector3 localPos)
    {
        GameObject pik = new GameObject("PikNPC");
        pik.transform.SetParent(parent);
        pik.transform.localPosition = localPos;
        pik.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face outward

        Material skinMat = new Material(Shader.Find("Standard"));
        skinMat.color = new Color(0.65f, 0.5f, 0.4f);

        Material clothMat = new Material(Shader.Find("Standard"));
        clothMat.color = new Color(0.35f, 0.3f, 0.25f);

        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = new Color(0.4f, 0.35f, 0.28f);

        // Body with fur vest
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(pik.transform);
        body.transform.localPosition = new Vector3(0, 0.6f, 0);
        body.transform.localScale = new Vector3(0.35f, 0.45f, 0.28f);
        body.GetComponent<Renderer>().sharedMaterial = furMat;
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Fur collar
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        collar.name = "FurCollar";
        collar.transform.SetParent(pik.transform);
        collar.transform.localPosition = new Vector3(0, 1.0f, 0);
        collar.transform.localScale = new Vector3(0.38f, 0.15f, 0.3f);
        collar.GetComponent<Renderer>().sharedMaterial = furMat;
        Object.DestroyImmediate(collar.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(pik.transform);
        head.transform.localPosition = new Vector3(0, 1.25f, 0);
        head.transform.localScale = new Vector3(0.26f, 0.28f, 0.26f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Black hair - messy
        Material hairMat = new Material(Shader.Find("Standard"));
        hairMat.color = new Color(0.1f, 0.08f, 0.05f);

        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.name = "Hair";
        hair.transform.SetParent(pik.transform);
        hair.transform.localPosition = new Vector3(0, 1.38f, -0.02f);
        hair.transform.localScale = new Vector3(0.28f, 0.15f, 0.26f);
        hair.GetComponent<Renderer>().sharedMaterial = hairMat;
        Object.DestroyImmediate(hair.GetComponent<Collider>());

        // Eyes
        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = Color.black;
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(pik.transform);
            eye.transform.localPosition = new Vector3(side * 0.06f, 1.28f, 0.12f);
            eye.transform.localScale = Vector3.one * 0.035f;
            eye.GetComponent<Renderer>().sharedMaterial = eyeMat;
            Object.DestroyImmediate(eye.GetComponent<Collider>());
        }

        // Right arm holding torch
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        arm.name = "RightArm";
        arm.transform.SetParent(pik.transform);
        arm.transform.localPosition = new Vector3(-0.25f, 0.85f, 0.1f);
        arm.transform.localRotation = Quaternion.Euler(0, 0, -45);
        arm.transform.localScale = new Vector3(0.08f, 0.25f, 0.08f);
        arm.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(arm.GetComponent<Collider>());

        // === FLAMING TORCH ===
        GameObject torch = new GameObject("FlamingTorch");
        torch.transform.SetParent(pik.transform);
        torch.transform.localPosition = new Vector3(-0.4f, 1.3f, 0.15f);

        // Torch handle
        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.35f, 0.22f, 0.12f);

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "TorchHandle";
        handle.transform.SetParent(torch.transform);
        handle.transform.localPosition = Vector3.zero;
        handle.transform.localRotation = Quaternion.Euler(0, 0, -30);
        handle.transform.localScale = new Vector3(0.06f, 0.35f, 0.06f);
        handle.GetComponent<Renderer>().sharedMaterial = woodMat;
        Object.DestroyImmediate(handle.GetComponent<Collider>());

        // Torch head (wrapped cloth)
        Material clothWrap = new Material(Shader.Find("Standard"));
        clothWrap.color = new Color(0.25f, 0.2f, 0.15f);

        GameObject torchHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        torchHead.name = "TorchHead";
        torchHead.transform.SetParent(torch.transform);
        torchHead.transform.localPosition = new Vector3(-0.15f, 0.35f, 0);
        torchHead.transform.localScale = new Vector3(0.12f, 0.15f, 0.12f);
        torchHead.GetComponent<Renderer>().sharedMaterial = clothWrap;
        Object.DestroyImmediate(torchHead.GetComponent<Collider>());

        // Torch flame
        Material flameMat = new Material(Shader.Find("Standard"));
        flameMat.color = new Color(1f, 0.6f, 0.1f);
        flameMat.EnableKeyword("_EMISSION");
        flameMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f) * 4f);

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "TorchFlame";
        flame.transform.SetParent(torch.transform);
        flame.transform.localPosition = new Vector3(-0.15f, 0.5f, 0);
        flame.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
        flame.GetComponent<Renderer>().sharedMaterial = flameMat;
        Object.DestroyImmediate(flame.GetComponent<Collider>());

        // Inner bright flame
        Material innerFlameMat = new Material(Shader.Find("Standard"));
        innerFlameMat.color = new Color(1f, 0.9f, 0.4f);
        innerFlameMat.EnableKeyword("_EMISSION");
        innerFlameMat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.3f) * 5f);

        GameObject innerFlame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        innerFlame.name = "InnerFlame";
        innerFlame.transform.SetParent(torch.transform);
        innerFlame.transform.localPosition = new Vector3(-0.15f, 0.48f, 0);
        innerFlame.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);
        innerFlame.GetComponent<Renderer>().sharedMaterial = innerFlameMat;
        Object.DestroyImmediate(innerFlame.GetComponent<Collider>());

        // Torch light
        GameObject torchLight = new GameObject("TorchLight");
        torchLight.transform.SetParent(torch.transform);
        torchLight.transform.localPosition = new Vector3(-0.15f, 0.5f, 0);
        Light tl = torchLight.AddComponent<Light>();
        tl.type = LightType.Point;
        tl.color = new Color(1f, 0.6f, 0.3f);
        tl.intensity = 3f;
        tl.range = 10f;
    }

    static void CreateBjorkNPC(Transform parent, Vector3 localPos)
    {
        GameObject bjork = new GameObject("BjorkHuntsman");
        bjork.transform.SetParent(parent);
        bjork.transform.localPosition = localPos;

        // Add the NPC component (creates its own model and fire)
        bjork.AddComponent<BjorkHuntsman>();
    }

    static void CreatePolarBear(Transform parent, Vector3 localPos)
    {
        GameObject bear = new GameObject("PolarBear");
        bear.transform.SetParent(parent);
        bear.transform.localPosition = localPos;

        // Add AI component (creates its own model)
        bear.AddComponent<PolarBearAI>();
    }

    static void CreateIceFormation(Transform parent, Vector3 localPos, Material iceMat, float height)
    {
        GameObject formation = new GameObject("IceFormation");
        formation.transform.SetParent(parent);
        formation.transform.localPosition = localPos;

        // Main ice spike
        GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spike.name = "IceSpike";
        spike.transform.SetParent(formation.transform);
        spike.transform.localPosition = new Vector3(0, height / 2, 0);
        spike.transform.localScale = new Vector3(1.5f, height, 1.5f);
        spike.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));
        spike.GetComponent<Renderer>().sharedMaterial = iceMat;

        // Smaller surrounding spikes
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            float dist = Random.Range(1f, 2f);
            float spikeHeight = height * Random.Range(0.4f, 0.7f);

            GameObject smallSpike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            smallSpike.name = "SmallIceSpike";
            smallSpike.transform.SetParent(formation.transform);
            smallSpike.transform.localPosition = new Vector3(Mathf.Cos(angle) * dist, spikeHeight / 2, Mathf.Sin(angle) * dist);
            smallSpike.transform.localScale = new Vector3(0.8f, spikeHeight, 0.8f);
            smallSpike.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
            smallSpike.GetComponent<Renderer>().sharedMaterial = iceMat;
        }
    }

    static void CreateSnowyPineTree(Transform parent, Vector3 localPos, float height)
    {
        GameObject tree = new GameObject("SnowyPineTree");
        tree.transform.SetParent(parent);
        tree.transform.localPosition = localPos;

        // Brown trunk
        Material trunkMat = new Material(Shader.Find("Standard"));
        trunkMat.color = new Color(0.35f, 0.25f, 0.15f);

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, height * 0.15f, 0);
        trunk.transform.localScale = new Vector3(0.4f, height * 0.15f, 0.4f);
        trunk.GetComponent<Renderer>().sharedMaterial = trunkMat;
        Object.DestroyImmediate(trunk.GetComponent<Collider>());

        // Snow-covered green foliage (cone layers)
        Material snowyGreenMat = new Material(Shader.Find("Standard"));
        snowyGreenMat.color = new Color(0.2f, 0.4f, 0.25f); // Dark green

        Material snowCapMat = new Material(Shader.Find("Standard"));
        snowCapMat.color = new Color(0.95f, 0.97f, 1f); // Snow white

        float coneY = height * 0.3f;
        float coneSize = height * 0.4f;

        for (int i = 0; i < 3; i++)
        {
            // Green cone layer
            GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = "FoliageLayer" + i;
            cone.transform.SetParent(tree.transform);
            cone.transform.localPosition = new Vector3(0, coneY, 0);
            cone.transform.localScale = new Vector3(coneSize, height * 0.12f, coneSize);
            cone.GetComponent<Renderer>().sharedMaterial = snowyGreenMat;
            Object.DestroyImmediate(cone.GetComponent<Collider>());

            // Snow cap on top of each layer
            GameObject snowCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            snowCap.name = "SnowCap" + i;
            snowCap.transform.SetParent(tree.transform);
            snowCap.transform.localPosition = new Vector3(0, coneY + height * 0.06f, 0);
            snowCap.transform.localScale = new Vector3(coneSize * 0.9f, 0.05f, coneSize * 0.9f);
            snowCap.GetComponent<Renderer>().sharedMaterial = snowCapMat;
            Object.DestroyImmediate(snowCap.GetComponent<Collider>());

            coneY += height * 0.18f;
            coneSize *= 0.7f;
        }
    }

    static void CreateReturnPortal(Transform parent, Vector3 localPos, string name, RealmType destination)
    {
        Color portalColor = new Color(1f, 0.9f, 0.4f); // Golden for return portal

        // Create the portal using the same style but pointing back to tropical
        GameObject portal = new GameObject(name);
        portal.transform.SetParent(parent);
        portal.transform.localPosition = localPos;
        portal.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face the opposite direction

        // Add portal interaction
        PortalInteraction portalInteraction = portal.AddComponent<PortalInteraction>();
        portalInteraction.portalName = "Return to Tropical Island";
        portalInteraction.requiredLevel = 0; // No level requirement for return
        portalInteraction.destinationRealm = destination;
        portalInteraction.spawnOffset = new Vector3(5f, 2f, -25f); // Spawn near ice portal on tropical island

        // Portal materials
        Material stoneMat = new Material(Shader.Find("Standard"));
        stoneMat.color = new Color(0.5f, 0.55f, 0.6f); // Lighter stone for ice realm

        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.color = portalColor;
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", portalColor * 0.8f);

        Material portalSurfaceMat = new Material(Shader.Find("Standard"));
        portalSurfaceMat.SetFloat("_Mode", 3);
        portalSurfaceMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        portalSurfaceMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        portalSurfaceMat.EnableKeyword("_ALPHABLEND_ON");
        portalSurfaceMat.color = new Color(portalColor.r, portalColor.g, portalColor.b, 0.6f);
        portalSurfaceMat.EnableKeyword("_EMISSION");
        portalSurfaceMat.SetColor("_EmissionColor", portalColor * 0.5f);

        float archWidth = 2.5f;
        float archHeight = 3.5f;
        float pillarDepth = 0.4f;

        // Portal pillars
        GameObject leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPillar.name = "LeftPillar";
        leftPillar.transform.SetParent(portal.transform);
        leftPillar.transform.localPosition = new Vector3(-archWidth / 2, archHeight / 2, 0);
        leftPillar.transform.localScale = new Vector3(pillarDepth, archHeight, pillarDepth);
        leftPillar.GetComponent<Renderer>().sharedMaterial = stoneMat;
        Object.DestroyImmediate(leftPillar.GetComponent<Collider>());

        GameObject rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPillar.name = "RightPillar";
        rightPillar.transform.SetParent(portal.transform);
        rightPillar.transform.localPosition = new Vector3(archWidth / 2, archHeight / 2, 0);
        rightPillar.transform.localScale = new Vector3(pillarDepth, archHeight, pillarDepth);
        rightPillar.GetComponent<Renderer>().sharedMaterial = stoneMat;
        Object.DestroyImmediate(rightPillar.GetComponent<Collider>());

        // Top arch
        GameObject topArch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topArch.name = "TopArch";
        topArch.transform.SetParent(portal.transform);
        topArch.transform.localPosition = new Vector3(0, archHeight + 0.2f, 0);
        topArch.transform.localScale = new Vector3(archWidth + pillarDepth, 0.4f, pillarDepth);
        topArch.GetComponent<Renderer>().sharedMaterial = stoneMat;
        Object.DestroyImmediate(topArch.GetComponent<Collider>());

        // Portal surface
        GameObject portalSurface = GameObject.CreatePrimitive(PrimitiveType.Quad);
        portalSurface.name = "PortalSurface";
        portalSurface.transform.SetParent(portal.transform);
        portalSurface.transform.localPosition = new Vector3(0, archHeight / 2 + 0.3f, 0.05f);
        portalSurface.transform.localScale = new Vector3(archWidth - 0.3f, archHeight - 0.5f, 1);
        portalSurface.GetComponent<Renderer>().sharedMaterial = portalSurfaceMat;
        Object.DestroyImmediate(portalSurface.GetComponent<Collider>());

        // Glowing runes on pillars
        for (int pillar = 0; pillar < 2; pillar++)
        {
            float pillarX = (pillar == 0) ? -archWidth / 2 : archWidth / 2;
            for (int r = 0; r < 3; r++)
            {
                GameObject rune = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rune.name = "Rune";
                rune.transform.SetParent(portal.transform);
                rune.transform.localPosition = new Vector3(pillarX, 0.8f + r * 1.0f, pillarDepth / 2 + 0.02f);
                rune.transform.localScale = new Vector3(0.15f, 0.3f, 0.02f);
                rune.GetComponent<Renderer>().sharedMaterial = glowMat;
                Object.DestroyImmediate(rune.GetComponent<Collider>());
            }
        }

        // Add portal animator
        portal.AddComponent<PortalAnimator>();

        // "HOME" text above portal
        GameObject homeMarker = new GameObject("HomeMarker");
        homeMarker.transform.SetParent(portal.transform);
        homeMarker.transform.localPosition = new Vector3(0, archHeight + 1f, 0);
    }

    static void CreateIceFishingDock(Transform parent, Vector3 localPos)
    {
        GameObject dock = new GameObject("IceFishingDock");
        dock.transform.SetParent(parent);
        dock.transform.localPosition = localPos;

        Material woodMat = new Material(Shader.Find("Standard"));
        woodMat.color = new Color(0.45f, 0.35f, 0.25f); // Weathered wood

        Material iceMat = new Material(Shader.Find("Standard"));
        iceMat.color = new Color(0.7f, 0.85f, 0.95f);
        iceMat.SetFloat("_Glossiness", 0.8f);

        // Wooden platform
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Platform";
        platform.transform.SetParent(dock.transform);
        platform.transform.localPosition = Vector3.zero;
        platform.transform.localScale = new Vector3(8f, 0.3f, 6f);
        platform.GetComponent<Renderer>().sharedMaterial = woodMat;

        // Ice hole in front
        GameObject iceHole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        iceHole.name = "IceHole";
        iceHole.transform.SetParent(dock.transform);
        iceHole.transform.localPosition = new Vector3(0, -0.3f, 4f);
        iceHole.transform.localScale = new Vector3(2f, 0.5f, 2f);

        Material darkWaterMat = new Material(Shader.Find("Standard"));
        darkWaterMat.color = new Color(0.1f, 0.2f, 0.4f);
        iceHole.GetComponent<Renderer>().sharedMaterial = darkWaterMat;
        Object.DestroyImmediate(iceHole.GetComponent<Collider>());

        // Ice blocks around the hole
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            GameObject iceBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            iceBlock.name = "IceBlock";
            iceBlock.transform.SetParent(dock.transform);
            iceBlock.transform.localPosition = new Vector3(Mathf.Cos(angle) * 1.5f, -0.1f, 4f + Mathf.Sin(angle) * 1.5f);
            iceBlock.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
            iceBlock.transform.localRotation = Quaternion.Euler(Random.Range(-10f, 10f), Random.Range(0f, 360f), Random.Range(-10f, 10f));
            iceBlock.GetComponent<Renderer>().sharedMaterial = iceMat;
            Object.DestroyImmediate(iceBlock.GetComponent<Collider>());
        }

        // Lantern post
        GameObject lanternPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lanternPost.name = "LanternPost";
        lanternPost.transform.SetParent(dock.transform);
        lanternPost.transform.localPosition = new Vector3(-3f, 1f, 0);
        lanternPost.transform.localScale = new Vector3(0.15f, 2f, 0.15f);
        lanternPost.GetComponent<Renderer>().sharedMaterial = woodMat;
        Object.DestroyImmediate(lanternPost.GetComponent<Collider>());

        // Lantern
        Material lanternMat = new Material(Shader.Find("Standard"));
        lanternMat.color = new Color(1f, 0.8f, 0.3f);
        lanternMat.EnableKeyword("_EMISSION");
        lanternMat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.2f) * 2f);

        GameObject lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lantern.name = "Lantern";
        lantern.transform.SetParent(dock.transform);
        lantern.transform.localPosition = new Vector3(-3f, 2.2f, 0);
        lantern.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
        lantern.GetComponent<Renderer>().sharedMaterial = lanternMat;
        Object.DestroyImmediate(lantern.GetComponent<Collider>());
    }

}

