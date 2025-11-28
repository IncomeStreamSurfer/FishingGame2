using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages visual clothing items on the player character
/// Updates the 3D model when items are equipped from ClothingShopNPC
/// </summary>
public class PlayerClothingVisuals : MonoBehaviour
{
    public static PlayerClothingVisuals Instance { get; private set; }

    // Current clothing state
    private string currentHeadItem = "None";
    private string currentTopItem = "None";
    private string currentLegsItem = "None";
    private string currentAccessory = "None";

    // Visual GameObjects for clothing
    private GameObject headClothingObject;
    private GameObject topClothingObject;
    private GameObject legsClothingObject;
    private GameObject accessoryObject;
    private GameObject caneObject;
    private GameObject parrotObject;

    // References to body parts
    private Transform torso;
    private Transform head;
    private Transform hips;
    private List<Transform> legParts = new List<Transform>();
    private Renderer torsoRenderer;
    private Renderer hipsRenderer;
    private List<Renderer> legRenderers = new List<Renderer>();

    // Original materials
    private Material skinMaterial;
    private Color skinColor;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Find body parts
        torso = transform.Find("Torso");
        head = transform.Find("Head");
        hips = transform.Find("Hips");

        // Find all leg parts
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Leg") || child.name == "Foot")
            {
                legParts.Add(child);
                Renderer r = child.GetComponent<Renderer>();
                if (r != null) legRenderers.Add(r);
            }
        }

        if (torso != null)
        {
            torsoRenderer = torso.GetComponent<Renderer>();
            if (torsoRenderer != null)
            {
                skinMaterial = new Material(torsoRenderer.material);
                skinColor = skinMaterial.color;
            }
        }

        if (hips != null)
        {
            hipsRenderer = hips.GetComponent<Renderer>();
        }
    }

    void Update()
    {
        // Animate parrot if present
        if (parrotObject != null)
        {
            AnimateParrot();
        }

        // Animate cane if present (walking stick motion)
        if (caneObject != null)
        {
            AnimateCane();
        }
    }

    /// <summary>
    /// Called by ClothingShopNPC when an item is equipped
    /// </summary>
    public void EquipClothing(string slot, string itemName, Color itemColor)
    {
        Debug.Log($"PlayerClothingVisuals: Equipping {itemName} in {slot} slot");

        switch (slot)
        {
            case "Head":
                currentHeadItem = itemName;
                ApplyHeadClothing(itemName, itemColor);
                break;
            case "Top":
                currentTopItem = itemName;
                ApplyTopClothing(itemName, itemColor);
                break;
            case "Legs":
                currentLegsItem = itemName;
                ApplyLegsClothing(itemName, itemColor);
                break;
            case "Accessory":
                currentAccessory = itemName;
                ApplyAccessory(itemName, itemColor);
                break;
        }
    }

    // ==================== HEAD CLOTHING ====================
    void ApplyHeadClothing(string itemName, Color itemColor)
    {
        RemoveHeadClothing();

        if (itemName == "None" || head == null) return;

        headClothingObject = new GameObject("HeadClothing");
        headClothingObject.transform.SetParent(head.transform);
        headClothingObject.transform.localPosition = Vector3.zero;
        headClothingObject.transform.localRotation = Quaternion.identity;

        switch (itemName)
        {
            case "Straw Hat":
                CreateStrawHat();
                break;
            case "Baseball Cap":
                CreateBaseballCap();
                break;
            case "Fancy Top Hat":
                CreateTopHat();
                break;
        }
    }

    void CreateStrawHat()
    {
        Material strawMat = new Material(Shader.Find("Standard"));
        strawMat.color = new Color(0.9f, 0.8f, 0.5f);

        // Hat brim
        GameObject brim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        brim.name = "HatBrim";
        brim.transform.SetParent(headClothingObject.transform);
        brim.transform.localPosition = new Vector3(0, 0.4f, 0);
        brim.transform.localScale = new Vector3(1.8f, 0.05f, 1.8f);
        brim.GetComponent<Renderer>().material = strawMat;
        Object.Destroy(brim.GetComponent<Collider>());

        // Hat crown
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crown.name = "HatCrown";
        crown.transform.SetParent(headClothingObject.transform);
        crown.transform.localPosition = new Vector3(0, 0.6f, 0);
        crown.transform.localScale = new Vector3(1.0f, 0.25f, 1.0f);
        crown.GetComponent<Renderer>().material = strawMat;
        Object.Destroy(crown.GetComponent<Collider>());

        // Hat band
        Material bandMat = new Material(Shader.Find("Standard"));
        bandMat.color = new Color(0.45f, 0.20f, 0.10f);

        GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.name = "HatBand";
        band.transform.SetParent(headClothingObject.transform);
        band.transform.localPosition = new Vector3(0, 0.48f, 0);
        band.transform.localScale = new Vector3(1.05f, 0.06f, 1.05f);
        band.GetComponent<Renderer>().material = bandMat;
        Object.Destroy(band.GetComponent<Collider>());
    }

    void CreateBaseballCap()
    {
        Material capMat = new Material(Shader.Find("Standard"));
        capMat.color = new Color(0.85f, 0.15f, 0.1f); // Red

        // Cap dome
        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "CapDome";
        dome.transform.SetParent(headClothingObject.transform);
        dome.transform.localPosition = new Vector3(0, 0.35f, 0);
        dome.transform.localScale = new Vector3(1.15f, 0.5f, 1.15f);
        dome.GetComponent<Renderer>().material = capMat;
        Object.Destroy(dome.GetComponent<Collider>());

        // Cap visor/brim (front only)
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "CapVisor";
        visor.transform.SetParent(headClothingObject.transform);
        visor.transform.localPosition = new Vector3(0, 0.25f, 0.5f);
        visor.transform.localRotation = Quaternion.Euler(-15, 0, 0);
        visor.transform.localScale = new Vector3(0.8f, 0.05f, 0.5f);
        visor.GetComponent<Renderer>().material = capMat;
        Object.Destroy(visor.GetComponent<Collider>());

        // Cap button on top
        Material whiteMat = new Material(Shader.Find("Standard"));
        whiteMat.color = Color.white;

        GameObject button = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        button.name = "CapButton";
        button.transform.SetParent(headClothingObject.transform);
        button.transform.localPosition = new Vector3(0, 0.55f, 0);
        button.transform.localScale = Vector3.one * 0.15f;
        button.GetComponent<Renderer>().material = whiteMat;
        Object.Destroy(button.GetComponent<Collider>());
    }

    void CreateTopHat()
    {
        Material hatMat = new Material(Shader.Find("Standard"));
        hatMat.color = new Color(0.1f, 0.1f, 0.1f); // Black
        hatMat.SetFloat("_Glossiness", 0.7f);

        // Hat brim
        GameObject brim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        brim.name = "TopHatBrim";
        brim.transform.SetParent(headClothingObject.transform);
        brim.transform.localPosition = new Vector3(0, 0.35f, 0);
        brim.transform.localScale = new Vector3(1.5f, 0.04f, 1.5f);
        brim.GetComponent<Renderer>().material = hatMat;
        Object.Destroy(brim.GetComponent<Collider>());

        // Tall crown
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crown.name = "TopHatCrown";
        crown.transform.SetParent(headClothingObject.transform);
        crown.transform.localPosition = new Vector3(0, 0.7f, 0);
        crown.transform.localScale = new Vector3(0.9f, 0.4f, 0.9f);
        crown.GetComponent<Renderer>().material = hatMat;
        Object.Destroy(crown.GetComponent<Collider>());

        // Satin band
        Material bandMat = new Material(Shader.Find("Standard"));
        bandMat.color = new Color(0.6f, 0.1f, 0.1f); // Dark red

        GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.name = "TopHatBand";
        band.transform.SetParent(headClothingObject.transform);
        band.transform.localPosition = new Vector3(0, 0.45f, 0);
        band.transform.localScale = new Vector3(0.95f, 0.06f, 0.95f);
        band.GetComponent<Renderer>().material = bandMat;
        Object.Destroy(band.GetComponent<Collider>());
    }

    void RemoveHeadClothing()
    {
        if (headClothingObject != null)
        {
            Destroy(headClothingObject);
            headClothingObject = null;
        }
    }

    // ==================== TOP CLOTHING ====================
    void ApplyTopClothing(string itemName, Color itemColor)
    {
        RemoveTopClothing();

        if (torsoRenderer == null) return;

        if (itemName == "None")
        {
            // Show skin
            torsoRenderer.material.color = skinColor;
            return;
        }

        switch (itemName)
        {
            case "Coconut Bra":
                CreateCoconutBra();
                torsoRenderer.material.color = skinColor; // Skin visible with bra
                break;
            case "Red T-Shirt":
                torsoRenderer.material.color = new Color(0.85f, 0.15f, 0.1f);
                break;
            case "Blue Shirt":
                torsoRenderer.material.color = new Color(0.15f, 0.35f, 0.65f);
                break;
            case "Lumberjack Shirt":
                CreateLumberjackShirt();
                break;
            case "Fancy Tuxedo":
                CreateTuxedoTop();
                break;
            default:
                torsoRenderer.material.color = itemColor;
                break;
        }
    }

    void CreateCoconutBra()
    {
        if (torso == null) return;

        topClothingObject = new GameObject("CoconutBra");
        topClothingObject.transform.SetParent(torso.transform);
        topClothingObject.transform.localPosition = Vector3.zero;

        Material coconutMat = new Material(Shader.Find("Standard"));
        coconutMat.color = new Color(0.55f, 0.35f, 0.2f);

        // Two coconut halves
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject coconut = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coconut.name = "Coconut";
            coconut.transform.SetParent(topClothingObject.transform);
            coconut.transform.localPosition = new Vector3(side * 0.22f, 0.15f, 0.52f);
            coconut.transform.localScale = new Vector3(0.4f, 0.35f, 0.25f);
            coconut.GetComponent<Renderer>().material = coconutMat;
            Object.Destroy(coconut.GetComponent<Collider>());
        }

        // String/rope
        Material ropeMat = new Material(Shader.Find("Standard"));
        ropeMat.color = new Color(0.6f, 0.5f, 0.35f);

        GameObject rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.name = "BraRope";
        rope.transform.SetParent(topClothingObject.transform);
        rope.transform.localPosition = new Vector3(0, 0.15f, 0.4f);
        rope.transform.localRotation = Quaternion.Euler(0, 0, 90);
        rope.transform.localScale = new Vector3(0.03f, 0.45f, 0.03f);
        rope.GetComponent<Renderer>().material = ropeMat;
        Object.Destroy(rope.GetComponent<Collider>());
    }

    void CreateLumberjackShirt()
    {
        if (torso == null || torsoRenderer == null) return;

        // Base red color
        torsoRenderer.material.color = new Color(0.75f, 0.12f, 0.08f);

        topClothingObject = new GameObject("LumberjackPattern");
        topClothingObject.transform.SetParent(torso.transform);
        topClothingObject.transform.localPosition = Vector3.zero;
        topClothingObject.transform.localRotation = Quaternion.identity;

        Material blackMat = new Material(Shader.Find("Standard"));
        blackMat.color = new Color(0.1f, 0.08f, 0.05f);

        // Horizontal stripes
        for (int i = 0; i < 3; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "HStripe" + i;
            stripe.transform.SetParent(topClothingObject.transform);
            stripe.transform.localPosition = new Vector3(0, -0.15f + i * 0.15f, 0.01f);
            stripe.transform.localScale = new Vector3(1.05f, 0.03f, 1.02f);
            stripe.GetComponent<Renderer>().material = blackMat;
            Object.Destroy(stripe.GetComponent<Collider>());
        }

        // Vertical stripes
        for (int i = 0; i < 2; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "VStripe" + i;
            stripe.transform.SetParent(topClothingObject.transform);
            stripe.transform.localPosition = new Vector3(-0.15f + i * 0.3f, 0, 0.015f);
            stripe.transform.localScale = new Vector3(0.03f, 1.05f, 1.01f);
            stripe.GetComponent<Renderer>().material = blackMat;
            Object.Destroy(stripe.GetComponent<Collider>());
        }
    }

    void CreateTuxedoTop()
    {
        if (torso == null || torsoRenderer == null) return;

        // Black jacket
        torsoRenderer.material.color = new Color(0.08f, 0.08f, 0.08f);

        topClothingObject = new GameObject("TuxedoTop");
        topClothingObject.transform.SetParent(torso.transform);
        topClothingObject.transform.localPosition = Vector3.zero;

        // White shirt front (visible under jacket)
        Material whiteMat = new Material(Shader.Find("Standard"));
        whiteMat.color = new Color(0.95f, 0.95f, 0.95f);

        GameObject shirtFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shirtFront.name = "ShirtFront";
        shirtFront.transform.SetParent(topClothingObject.transform);
        shirtFront.transform.localPosition = new Vector3(0, 0, 0.52f);
        shirtFront.transform.localScale = new Vector3(0.35f, 0.9f, 0.02f);
        shirtFront.GetComponent<Renderer>().material = whiteMat;
        Object.Destroy(shirtFront.GetComponent<Collider>());

        // Black tie
        Material tieMat = new Material(Shader.Find("Standard"));
        tieMat.color = new Color(0.05f, 0.05f, 0.05f);

        GameObject tie = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tie.name = "Tie";
        tie.transform.SetParent(topClothingObject.transform);
        tie.transform.localPosition = new Vector3(0, -0.1f, 0.54f);
        tie.transform.localScale = new Vector3(0.12f, 0.6f, 0.02f);
        tie.GetComponent<Renderer>().material = tieMat;
        Object.Destroy(tie.GetComponent<Collider>());

        // Tie knot
        GameObject knot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        knot.name = "TieKnot";
        knot.transform.SetParent(topClothingObject.transform);
        knot.transform.localPosition = new Vector3(0, 0.25f, 0.54f);
        knot.transform.localScale = new Vector3(0.15f, 0.1f, 0.03f);
        knot.GetComponent<Renderer>().material = tieMat;
        Object.Destroy(knot.GetComponent<Collider>());

        // Lapels
        Material lapelMat = new Material(Shader.Find("Standard"));
        lapelMat.color = new Color(0.06f, 0.06f, 0.06f);
        lapelMat.SetFloat("_Glossiness", 0.5f);

        for (int side = -1; side <= 1; side += 2)
        {
            GameObject lapel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lapel.name = "Lapel";
            lapel.transform.SetParent(topClothingObject.transform);
            lapel.transform.localPosition = new Vector3(side * 0.25f, 0.1f, 0.53f);
            lapel.transform.localRotation = Quaternion.Euler(0, 0, side * -15);
            lapel.transform.localScale = new Vector3(0.2f, 0.5f, 0.02f);
            lapel.GetComponent<Renderer>().material = lapelMat;
            Object.Destroy(lapel.GetComponent<Collider>());
        }
    }

    void RemoveTopClothing()
    {
        if (topClothingObject != null)
        {
            Destroy(topClothingObject);
            topClothingObject = null;
        }
    }

    // ==================== LEGS CLOTHING ====================
    void ApplyLegsClothing(string itemName, Color itemColor)
    {
        RemoveLegsClothing();

        if (hipsRenderer == null) return;

        if (itemName == "None")
        {
            // Show skin
            hipsRenderer.material.color = skinColor;
            foreach (Renderer r in legRenderers)
            {
                if (r != null) r.material.color = skinColor;
            }
            return;
        }

        Color pantsColor = itemColor;

        switch (itemName)
        {
            case "Red Pants":
                pantsColor = new Color(0.8f, 0.15f, 0.1f);
                break;
            case "Green Pants":
                pantsColor = new Color(0.2f, 0.5f, 0.2f);
                break;
            case "Black Pants":
                pantsColor = new Color(0.12f, 0.12f, 0.12f);
                break;
            case "Blue Jeans":
                pantsColor = new Color(0.2f, 0.35f, 0.6f);
                break;
            case "Fancy Tuxedo":
                pantsColor = new Color(0.08f, 0.08f, 0.08f);
                CreateTuxedoPants();
                break;
        }

        // Apply color to hips and legs
        hipsRenderer.material.color = pantsColor;
        foreach (Renderer r in legRenderers)
        {
            if (r != null && !r.name.Contains("Foot"))
            {
                r.material.color = pantsColor;
            }
        }
    }

    void CreateTuxedoPants()
    {
        if (hips == null) return;

        legsClothingObject = new GameObject("TuxedoPants");
        legsClothingObject.transform.SetParent(hips.transform);
        legsClothingObject.transform.localPosition = Vector3.zero;

        // Side stripe on pants (fancy tuxedo style)
        Material stripeMat = new Material(Shader.Find("Standard"));
        stripeMat.color = new Color(0.2f, 0.2f, 0.2f);
        stripeMat.SetFloat("_Glossiness", 0.6f);

        // Note: The stripe would need to follow the leg geometry
        // For simplicity, we just color the pants darker with sheen
    }

    void RemoveLegsClothing()
    {
        if (legsClothingObject != null)
        {
            Destroy(legsClothingObject);
            legsClothingObject = null;
        }
    }

    // ==================== ACCESSORIES ====================
    void ApplyAccessory(string itemName, Color itemColor)
    {
        RemoveAccessory();

        if (itemName == "None") return;

        switch (itemName)
        {
            case "Pimp Cane":
                CreatePimpCane();
                break;
            case "Shoulder Parrot":
                CreateShoulderParrot();
                break;
        }
    }

    void CreatePimpCane()
    {
        // Find the right hand to attach the cane
        Transform rightHand = transform.Find("RightHand");
        if (rightHand == null)
        {
            // Fallback - attach to player
            rightHand = transform;
        }

        caneObject = new GameObject("PimpCane");
        caneObject.transform.SetParent(transform); // Parent to player, not hand (for animation)
        caneObject.transform.localPosition = new Vector3(0.4f, -0.5f, 0.2f);
        caneObject.transform.localRotation = Quaternion.Euler(0, 0, -10);

        // Gold handle (fancy ball top)
        Material goldMat = new Material(Shader.Find("Standard"));
        goldMat.color = new Color(0.95f, 0.8f, 0.2f);
        goldMat.SetFloat("_Metallic", 0.9f);
        goldMat.SetFloat("_Glossiness", 0.8f);

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        handle.name = "CaneHandle";
        handle.transform.SetParent(caneObject.transform);
        handle.transform.localPosition = new Vector3(0, 0.5f, 0);
        handle.transform.localScale = Vector3.one * 0.12f;
        handle.GetComponent<Renderer>().material = goldMat;
        Object.Destroy(handle.GetComponent<Collider>());

        // Decorative collar under handle
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        collar.name = "CaneCollar";
        collar.transform.SetParent(caneObject.transform);
        collar.transform.localPosition = new Vector3(0, 0.42f, 0);
        collar.transform.localScale = new Vector3(0.08f, 0.03f, 0.08f);
        collar.GetComponent<Renderer>().material = goldMat;
        Object.Destroy(collar.GetComponent<Collider>());

        // Main cane shaft (black with gold accents)
        Material shaftMat = new Material(Shader.Find("Standard"));
        shaftMat.color = new Color(0.1f, 0.1f, 0.1f);
        shaftMat.SetFloat("_Glossiness", 0.7f);

        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "CaneShaft";
        shaft.transform.SetParent(caneObject.transform);
        shaft.transform.localPosition = new Vector3(0, 0, 0);
        shaft.transform.localScale = new Vector3(0.04f, 0.4f, 0.04f);
        shaft.GetComponent<Renderer>().material = shaftMat;
        Object.Destroy(shaft.GetComponent<Collider>());

        // Gold tip at bottom
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tip.name = "CaneTip";
        tip.transform.SetParent(caneObject.transform);
        tip.transform.localPosition = new Vector3(0, -0.42f, 0);
        tip.transform.localScale = new Vector3(0.05f, 0.03f, 0.05f);
        tip.GetComponent<Renderer>().material = goldMat;
        Object.Destroy(tip.GetComponent<Collider>());

        // Gold rings on shaft
        for (int i = 0; i < 2; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "CaneRing" + i;
            ring.transform.SetParent(caneObject.transform);
            ring.transform.localPosition = new Vector3(0, 0.2f - i * 0.25f, 0);
            ring.transform.localScale = new Vector3(0.05f, 0.015f, 0.05f);
            ring.GetComponent<Renderer>().material = goldMat;
            Object.Destroy(ring.GetComponent<Collider>());
        }

        Debug.Log("Pimp Cane equipped! Walk with style!");
    }

    void AnimateCane()
    {
        if (caneObject == null) return;

        // Get player velocity for walking animation
        Rigidbody rb = GetComponent<Rigidbody>();
        float speed = rb != null ? rb.linearVelocity.magnitude : 0f;

        // Base position
        float baseX = 0.4f;
        float baseY = -0.5f;
        float baseZ = 0.2f;

        if (speed > 0.5f)
        {
            // Walking animation - cane swings with walking motion
            float walkCycle = Time.time * 4f;
            float swing = Mathf.Sin(walkCycle) * 15f;
            float forward = Mathf.Cos(walkCycle) * 0.1f;

            caneObject.transform.localPosition = new Vector3(baseX, baseY, baseZ + forward);
            caneObject.transform.localRotation = Quaternion.Euler(swing, 0, -10 + swing * 0.3f);
        }
        else
        {
            // Standing - gentle idle motion
            float idle = Mathf.Sin(Time.time * 0.5f) * 2f;
            caneObject.transform.localPosition = new Vector3(baseX, baseY, baseZ);
            caneObject.transform.localRotation = Quaternion.Euler(idle, 0, -10);
        }
    }

    void CreateShoulderParrot()
    {
        if (torso == null) return;

        parrotObject = new GameObject("ShoulderParrot");
        parrotObject.transform.SetParent(torso.transform);
        parrotObject.transform.localPosition = new Vector3(0.35f, 0.35f, 0.05f);

        // Parrot body (green)
        Material greenMat = new Material(Shader.Find("Standard"));
        greenMat.color = new Color(0.2f, 0.75f, 0.25f);
        greenMat.SetFloat("_Glossiness", 0.6f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "ParrotBody";
        body.transform.SetParent(parrotObject.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.12f, 0.12f, 0.18f);
        body.GetComponent<Renderer>().material = greenMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Parrot head
        GameObject parrotHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        parrotHead.name = "ParrotHead";
        parrotHead.transform.SetParent(parrotObject.transform);
        parrotHead.transform.localPosition = new Vector3(0, 0.08f, 0.06f);
        parrotHead.transform.localScale = new Vector3(0.09f, 0.09f, 0.09f);
        parrotHead.GetComponent<Renderer>().material = greenMat;
        Object.Destroy(parrotHead.GetComponent<Collider>());

        // Beak
        Material beakMat = new Material(Shader.Find("Standard"));
        beakMat.color = new Color(1f, 0.7f, 0.1f);

        GameObject beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beak.name = "ParrotBeak";
        beak.transform.SetParent(parrotObject.transform);
        beak.transform.localPosition = new Vector3(0, 0.08f, 0.12f);
        beak.transform.localRotation = Quaternion.Euler(45, 0, 0);
        beak.transform.localScale = new Vector3(0.03f, 0.04f, 0.04f);
        beak.GetComponent<Renderer>().material = beakMat;
        Object.Destroy(beak.GetComponent<Collider>());

        // Eyes
        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = Color.black;

        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "ParrotEye";
            eye.transform.SetParent(parrotObject.transform);
            eye.transform.localPosition = new Vector3(side * 0.03f, 0.1f, 0.1f);
            eye.transform.localScale = Vector3.one * 0.02f;
            eye.GetComponent<Renderer>().material = eyeMat;
            Object.Destroy(eye.GetComponent<Collider>());
        }

        // Tail feathers (blue and red)
        Material blueMat = new Material(Shader.Find("Standard"));
        blueMat.color = new Color(0.2f, 0.4f, 0.9f);

        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = new Color(0.9f, 0.2f, 0.15f);

        GameObject blueTail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blueTail.name = "BlueTail";
        blueTail.transform.SetParent(parrotObject.transform);
        blueTail.transform.localPosition = new Vector3(0, -0.03f, -0.12f);
        blueTail.transform.localRotation = Quaternion.Euler(-30, 0, 0);
        blueTail.transform.localScale = new Vector3(0.06f, 0.02f, 0.15f);
        blueTail.GetComponent<Renderer>().material = blueMat;
        Object.Destroy(blueTail.GetComponent<Collider>());

        GameObject redTail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        redTail.name = "RedTail";
        redTail.transform.SetParent(parrotObject.transform);
        redTail.transform.localPosition = new Vector3(0, -0.05f, -0.1f);
        redTail.transform.localRotation = Quaternion.Euler(-25, 0, 0);
        redTail.transform.localScale = new Vector3(0.04f, 0.02f, 0.12f);
        redTail.GetComponent<Renderer>().material = redMat;
        Object.Destroy(redTail.GetComponent<Collider>());

        // Wing
        GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wing.name = "ParrotWing";
        wing.transform.SetParent(parrotObject.transform);
        wing.transform.localPosition = new Vector3(0.05f, 0, -0.02f);
        wing.transform.localRotation = Quaternion.Euler(0, -20, 15);
        wing.transform.localScale = new Vector3(0.02f, 0.08f, 0.1f);
        wing.GetComponent<Renderer>().material = greenMat;
        Object.Destroy(wing.GetComponent<Collider>());

        Debug.Log("Shoulder Parrot equipped! Squawk!");
    }

    void AnimateParrot()
    {
        if (parrotObject == null) return;

        // Gentle bobbing and head turning
        float bob = Mathf.Sin(Time.time * 2f) * 0.01f;
        float headTurn = Mathf.Sin(Time.time * 0.5f) * 15f;

        parrotObject.transform.localPosition = new Vector3(0.35f, 0.35f + bob, 0.05f);

        Transform parrotHead = parrotObject.transform.Find("ParrotHead");
        if (parrotHead != null)
        {
            parrotHead.localRotation = Quaternion.Euler(0, headTurn, 0);
        }

        // Occasional wing flap
        Transform wing = parrotObject.transform.Find("ParrotWing");
        if (wing != null)
        {
            float flapCycle = Time.time % 5f;
            if (flapCycle < 0.3f)
            {
                float flap = Mathf.Sin(flapCycle * 30f) * 20f;
                wing.localRotation = Quaternion.Euler(0, -20, 15 + flap);
            }
        }
    }

    void RemoveAccessory()
    {
        if (caneObject != null)
        {
            Destroy(caneObject);
            caneObject = null;
        }
        if (parrotObject != null)
        {
            Destroy(parrotObject);
            parrotObject = null;
        }
        if (accessoryObject != null)
        {
            Destroy(accessoryObject);
            accessoryObject = null;
        }
    }

    // Getters for current equipment
    public string GetCurrentHeadItem() => currentHeadItem;
    public string GetCurrentTopItem() => currentTopItem;
    public string GetCurrentLegsItem() => currentLegsItem;
    public string GetCurrentAccessory() => currentAccessory;
}
