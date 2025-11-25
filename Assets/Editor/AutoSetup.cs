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

        // Quest NPC on the dock
        CreateQuestNPC();

        // Camera setup
        SetupCamera();

        // Create realistic trees in background
        CreateTreesAroundScene();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("=== FISHING GAME READY! Press PLAY, WASD to move, LEFT CLICK to fish, SPACE to jump! ===");
    }

    static void CleanupScene()
    {
        string[] toDelete = { "Player", "Ground", "Water", "WaterBed", "Dock", "Ramp", "GameManager", "FishingSystem", "UIManager", "Sun", "TreesParent", "LevelingSystem", "QuestSystem", "BottleEventSystem", "QuestNPC" };
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
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, 0, -20);
        ground.transform.localScale = new Vector3(12, 1, 6);
        ground.GetComponent<Renderer>().sharedMaterial = MaterialGenerator.CreateGrassMaterial();
    }

    static void CreateWater()
    {
        // Water plane - positioned so it starts at dock middle (z = 5) and extends forward
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Water";
        water.transform.position = new Vector3(0, 0.2f, 25);  // Water level at 0.2
        water.transform.localScale = new Vector3(25, 1, 20);
        water.AddComponent<WaterEffect>();

        // Water bed (dark sandy bottom)
        GameObject waterBed = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterBed.name = "WaterBed";
        waterBed.transform.position = new Vector3(0, -3f, 25);
        waterBed.transform.localScale = new Vector3(25, 1, 20);
        Material bedMat = new Material(Shader.Find("Standard"));
        bedMat.color = new Color(0.12f, 0.15f, 0.1f);
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
        player.transform.position = new Vector3(0, 2f, 8);  // Start on dock over water
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
        // Shoulder
        GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shoulder.name = "Shoulder";
        shoulder.transform.SetParent(player.transform);
        shoulder.transform.localPosition = new Vector3(xOffset, 0.35f, 0);
        shoulder.transform.localScale = new Vector3(0.12f, 0.10f, 0.10f);
        shoulder.GetComponent<Renderer>().sharedMaterial = shirtMat;
        Object.DestroyImmediate(shoulder.GetComponent<Collider>());

        // Upper arm (with sleeve)
        GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        upperArm.name = "UpperArm";
        upperArm.transform.SetParent(player.transform);
        upperArm.transform.localPosition = new Vector3(xOffset * 1.15f, 0.18f, 0);
        upperArm.transform.localScale = new Vector3(0.09f, 0.15f, 0.09f);
        upperArm.GetComponent<Renderer>().sharedMaterial = shirtMat;
        Object.DestroyImmediate(upperArm.GetComponent<Collider>());

        // Lower arm (skin)
        GameObject lowerArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        lowerArm.name = "LowerArm";
        lowerArm.transform.SetParent(player.transform);
        lowerArm.transform.localPosition = new Vector3(xOffset * 1.2f, -0.02f, 0);
        lowerArm.transform.localScale = new Vector3(0.07f, 0.14f, 0.07f);
        lowerArm.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(lowerArm.GetComponent<Collider>());

        // Hand
        GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hand.name = "Hand";
        hand.transform.SetParent(player.transform);
        hand.transform.localPosition = new Vector3(xOffset * 1.25f, -0.18f, 0);
        hand.transform.localScale = new Vector3(0.08f, 0.10f, 0.05f);
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
        // Rod pivot centered in front of player (held with both hands)
        GameObject rodPivot = new GameObject("RodPivot");
        rodPivot.transform.SetParent(player.transform);
        rodPivot.transform.localPosition = new Vector3(0, 0.1f, 0.3f);

        Material rodMat = new Material(Shader.Find("Standard"));
        rodMat.color = new Color(0.35f, 0.22f, 0.12f);
        rodMat.SetFloat("_Glossiness", 0.5f);

        // Handle (cork) - longer for two-handed grip
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "RodHandle";
        handle.transform.SetParent(rodPivot.transform);
        handle.transform.localPosition = new Vector3(0, 0, 0);
        handle.transform.localRotation = Quaternion.Euler(90, 0, 0);
        handle.transform.localScale = new Vector3(0.06f, 0.25f, 0.06f);
        Material corkMat = new Material(Shader.Find("Standard"));
        corkMat.color = new Color(0.65f, 0.50f, 0.35f);
        handle.GetComponent<Renderer>().sharedMaterial = corkMat;
        Object.DestroyImmediate(handle.GetComponent<Collider>());

        // Rod shaft - extends forward from handle
        GameObject rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rod.name = "FishingRod";
        rod.transform.SetParent(rodPivot.transform);
        rod.transform.localPosition = new Vector3(0, 0.9f, 0);
        rod.transform.localRotation = Quaternion.Euler(90, 0, 0);
        rod.transform.localScale = new Vector3(0.03f, 0.7f, 0.03f);
        rod.GetComponent<Renderer>().sharedMaterial = rodMat;
        Object.DestroyImmediate(rod.GetComponent<Collider>());

        // Rod tip (thinner)
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tip.name = "RodTip";
        tip.transform.SetParent(rod.transform);
        tip.transform.localPosition = new Vector3(0, 0.9f, 0);
        tip.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
        tip.GetComponent<Renderer>().sharedMaterial = rodMat;
        Object.DestroyImmediate(tip.GetComponent<Collider>());

        // Reel - mounted on handle
        GameObject reel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        reel.name = "Reel";
        reel.transform.SetParent(rodPivot.transform);
        reel.transform.localPosition = new Vector3(0.05f, 0.15f, 0);
        reel.transform.localRotation = Quaternion.Euler(0, 0, 0);
        reel.transform.localScale = new Vector3(0.08f, 0.03f, 0.08f);
        Material reelMat = new Material(Shader.Find("Standard"));
        reelMat.color = new Color(0.25f, 0.25f, 0.30f);
        reelMat.SetFloat("_Metallic", 0.7f);
        reelMat.SetFloat("_Glossiness", 0.6f);
        reel.GetComponent<Renderer>().sharedMaterial = reelMat;
        Object.DestroyImmediate(reel.GetComponent<Collider>());

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
        GameObject npc = new GameObject("QuestNPC");
        npc.transform.position = new Vector3(-2f, 2f, 3f);  // Standing on dock, to the side
        npc.transform.rotation = Quaternion.Euler(0, 45, 0);  // Facing toward player area

        // Add NPC interaction component
        NPCInteraction npcInteraction = npc.AddComponent<NPCInteraction>();
        npcInteraction.npcName = "Old Captain";
        npcInteraction.interactionRange = 4f;

        Material skinMat = MaterialGenerator.CreateSkinMaterial();
        Material vestMat = MaterialGenerator.CreateFabricMaterial(new Color(0.55f, 0.35f, 0.15f)); // Brown vest
        Material pantsMat = MaterialGenerator.CreateFabricMaterial(new Color(0.15f, 0.15f, 0.20f)); // Dark pants

        // Torso with vest
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torso.name = "NPCTorso";
        torso.transform.SetParent(npc.transform);
        torso.transform.localPosition = new Vector3(0, 0.15f, 0);
        torso.transform.localScale = new Vector3(0.45f, 0.55f, 0.25f);
        torso.GetComponent<Renderer>().sharedMaterial = vestMat;
        Object.DestroyImmediate(torso.GetComponent<Collider>());

        // Hips
        GameObject hips = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hips.name = "NPCHips";
        hips.transform.SetParent(npc.transform);
        hips.transform.localPosition = new Vector3(0, -0.2f, 0);
        hips.transform.localScale = new Vector3(0.40f, 0.25f, 0.22f);
        hips.GetComponent<Renderer>().sharedMaterial = pantsMat;
        Object.DestroyImmediate(hips.GetComponent<Collider>());

        // Legs
        for (float xOffset = -0.12f; xOffset <= 0.12f; xOffset += 0.24f)
        {
            GameObject upperLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperLeg.transform.SetParent(npc.transform);
            upperLeg.transform.localPosition = new Vector3(xOffset, -0.50f, 0);
            upperLeg.transform.localScale = new Vector3(0.14f, 0.22f, 0.14f);
            upperLeg.GetComponent<Renderer>().sharedMaterial = pantsMat;
            Object.DestroyImmediate(upperLeg.GetComponent<Collider>());

            GameObject lowerLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lowerLeg.transform.SetParent(npc.transform);
            lowerLeg.transform.localPosition = new Vector3(xOffset, -0.78f, 0);
            lowerLeg.transform.localScale = new Vector3(0.11f, 0.20f, 0.11f);
            lowerLeg.GetComponent<Renderer>().sharedMaterial = pantsMat;
            Object.DestroyImmediate(lowerLeg.GetComponent<Collider>());

            Material bootMat = new Material(Shader.Find("Standard"));
            bootMat.color = new Color(0.15f, 0.12f, 0.08f);
            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foot.transform.SetParent(npc.transform);
            foot.transform.localPosition = new Vector3(xOffset, -0.95f, 0.03f);
            foot.transform.localScale = new Vector3(0.10f, 0.08f, 0.18f);
            foot.GetComponent<Renderer>().sharedMaterial = bootMat;
            Object.DestroyImmediate(foot.GetComponent<Collider>());
        }

        // Arms (crossed in front)
        Material shirtMat = MaterialGenerator.CreateFabricMaterial(new Color(0.8f, 0.75f, 0.65f)); // Light shirt
        for (float xOffset = -0.30f; xOffset <= 0.30f; xOffset += 0.60f)
        {
            GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shoulder.transform.SetParent(npc.transform);
            shoulder.transform.localPosition = new Vector3(xOffset, 0.35f, 0);
            shoulder.transform.localScale = new Vector3(0.12f, 0.10f, 0.10f);
            shoulder.GetComponent<Renderer>().sharedMaterial = vestMat;
            Object.DestroyImmediate(shoulder.GetComponent<Collider>());

            GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperArm.transform.SetParent(npc.transform);
            upperArm.transform.localPosition = new Vector3(xOffset * 1.15f, 0.18f, 0);
            upperArm.transform.localScale = new Vector3(0.09f, 0.15f, 0.09f);
            upperArm.transform.localRotation = Quaternion.Euler(0, 0, xOffset > 0 ? -30 : 30);
            upperArm.GetComponent<Renderer>().sharedMaterial = shirtMat;
            Object.DestroyImmediate(upperArm.GetComponent<Collider>());

            GameObject lowerArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lowerArm.transform.SetParent(npc.transform);
            lowerArm.transform.localPosition = new Vector3(xOffset * 0.5f, 0.02f, 0.12f);
            lowerArm.transform.localScale = new Vector3(0.07f, 0.14f, 0.07f);
            lowerArm.transform.localRotation = Quaternion.Euler(60, 0, 0);
            lowerArm.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(lowerArm.GetComponent<Collider>());

            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hand.transform.SetParent(npc.transform);
            hand.transform.localPosition = new Vector3(xOffset * 0.3f, 0.05f, 0.22f);
            hand.transform.localScale = new Vector3(0.08f, 0.10f, 0.05f);
            hand.GetComponent<Renderer>().sharedMaterial = skinMat;
            Object.DestroyImmediate(hand.GetComponent<Collider>());
        }

        // Neck
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.transform.SetParent(npc.transform);
        neck.transform.localPosition = new Vector3(0, 0.5f, 0);
        neck.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
        neck.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(neck.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "NPCHead";
        head.transform.SetParent(npc.transform);
        head.transform.localPosition = new Vector3(0, 0.72f, 0);
        head.transform.localScale = new Vector3(0.26f, 0.30f, 0.26f);
        head.GetComponent<Renderer>().sharedMaterial = skinMat;
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Eyes
        for (float ex = -0.06f; ex <= 0.06f; ex += 0.12f)
        {
            GameObject eyeWhite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeWhite.transform.SetParent(head.transform);
            eyeWhite.transform.localPosition = new Vector3(ex, 0.05f, 0.45f);
            eyeWhite.transform.localScale = new Vector3(0.12f, 0.08f, 0.06f);
            Material whiteMat = new Material(Shader.Find("Standard"));
            whiteMat.color = Color.white;
            eyeWhite.GetComponent<Renderer>().sharedMaterial = whiteMat;
            Object.DestroyImmediate(eyeWhite.GetComponent<Collider>());

            GameObject iris = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            iris.transform.SetParent(eyeWhite.transform);
            iris.transform.localPosition = new Vector3(0, 0, 0.35f);
            iris.transform.localScale = new Vector3(0.55f, 0.65f, 0.25f);
            Material irisMat = new Material(Shader.Find("Standard"));
            irisMat.color = new Color(0.4f, 0.3f, 0.2f);  // Brown eyes
            iris.GetComponent<Renderer>().sharedMaterial = irisMat;
            Object.DestroyImmediate(iris.GetComponent<Collider>());
        }

        // Beard (Quest giver is an old fisherman)
        GameObject beard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beard.transform.SetParent(head.transform);
        beard.transform.localPosition = new Vector3(0, -0.25f, 0.3f);
        beard.transform.localScale = new Vector3(0.35f, 0.40f, 0.25f);
        Material beardMat = new Material(Shader.Find("Standard"));
        beardMat.color = new Color(0.7f, 0.7f, 0.7f);  // Gray beard
        beard.GetComponent<Renderer>().sharedMaterial = beardMat;
        Object.DestroyImmediate(beard.GetComponent<Collider>());

        // Captain's hat
        Material hatMat = new Material(Shader.Find("Standard"));
        hatMat.color = new Color(0.15f, 0.18f, 0.25f);  // Dark navy

        GameObject hatBrim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hatBrim.transform.SetParent(head.transform);
        hatBrim.transform.localPosition = new Vector3(0, 0.35f, 0.05f);
        hatBrim.transform.localScale = new Vector3(1.5f, 0.04f, 1.5f);
        hatBrim.GetComponent<Renderer>().sharedMaterial = hatMat;
        Object.DestroyImmediate(hatBrim.GetComponent<Collider>());

        GameObject hatCrown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hatCrown.transform.SetParent(head.transform);
        hatCrown.transform.localPosition = new Vector3(0, 0.55f, 0);
        hatCrown.transform.localScale = new Vector3(1.1f, 0.22f, 1.1f);
        hatCrown.GetComponent<Renderer>().sharedMaterial = hatMat;
        Object.DestroyImmediate(hatCrown.GetComponent<Collider>());

        // Gold anchor emblem on hat
        GameObject emblem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        emblem.transform.SetParent(hatCrown.transform);
        emblem.transform.localPosition = new Vector3(0, -0.3f, 0.45f);
        emblem.transform.localScale = new Vector3(0.15f, 0.15f, 0.05f);
        Material goldMat = new Material(Shader.Find("Standard"));
        goldMat.color = new Color(1f, 0.85f, 0.2f);
        goldMat.SetFloat("_Metallic", 0.8f);
        emblem.GetComponent<Renderer>().sharedMaterial = goldMat;
        Object.DestroyImmediate(emblem.GetComponent<Collider>());

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
        Camera.main.GetComponent<CameraController>().offset = new Vector3(0, 5, -7);
        Camera.main.transform.position = new Vector3(0, 7, -3);
        Camera.main.backgroundColor = new Color(0.5f, 0.7f, 0.9f);  // Sky blue
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
    }
}
