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

    // Parrot flying behavior
    private bool parrotFlying = false;
    private float parrotFlyTimer = 0f;
    private float nextFlyTime = 0f;
    private Vector3 parrotFlyTarget;
    private Vector3 parrotStartPos;
    private float flyProgress = 0f;
    private bool parrotReturning = false;
    private string parrotMessage = "";
    private float parrotMessageTimer = 0f;
    private string[] parrotPhrases = {
        "SQUAWK! Nice fish!",
        "Polly wants a cracker!",
        "ARRR! Shiver me timbers!",
        "Pretty bird! Pretty bird!",
        "Land ho!",
        "SQUAWK! Keep fishing!",
        "Pieces of eight!",
        "Yo ho ho!",
        "BAWK! Watch the water!"
    };

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

        // Start with underpants equipped
        Invoke("EquipStartingClothes", 0.1f);
    }

    void EquipStartingClothes()
    {
        // Player starts with white underpants
        EquipClothing("Legs", "Underpants", new Color(0.95f, 0.95f, 0.95f));
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
            // Ice Realm hats
            case "Bear Skin Hat":
                CreateBearSkinHat();
                break;
            case "Pirate Hat":
                CreatePirateHat();
                break;
            case "Beret":
                CreateBeret();
                break;
        }
    }

    void CreateStrawHat()
    {
        Material strawMat = new Material(Shader.Find("Standard"));
        strawMat.color = new Color(0.9f, 0.8f, 0.5f);

        // Hat brim - positioned to sit on top of head
        GameObject brim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        brim.name = "HatBrim";
        brim.transform.SetParent(headClothingObject.transform);
        brim.transform.localPosition = new Vector3(0, 0.35f, 0);
        brim.transform.localScale = new Vector3(0.9f, 0.025f, 0.9f);
        brim.GetComponent<Renderer>().material = strawMat;
        Object.Destroy(brim.GetComponent<Collider>());

        // Hat crown
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crown.name = "HatCrown";
        crown.transform.SetParent(headClothingObject.transform);
        crown.transform.localPosition = new Vector3(0, 0.45f, 0);
        crown.transform.localScale = new Vector3(0.5f, 0.125f, 0.5f);
        crown.GetComponent<Renderer>().material = strawMat;
        Object.Destroy(crown.GetComponent<Collider>());

        // Hat band
        Material bandMat = new Material(Shader.Find("Standard"));
        bandMat.color = new Color(0.45f, 0.20f, 0.10f);

        GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.name = "HatBand";
        band.transform.SetParent(headClothingObject.transform);
        band.transform.localPosition = new Vector3(0, 0.39f, 0);
        band.transform.localScale = new Vector3(0.525f, 0.03f, 0.525f);
        band.GetComponent<Renderer>().material = bandMat;
        Object.Destroy(band.GetComponent<Collider>());
    }

    void CreateBaseballCap()
    {
        Material capMat = new Material(Shader.Find("Standard"));
        capMat.color = new Color(0.85f, 0.15f, 0.1f); // Red

        // Cap dome - positioned to sit on head
        GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dome.name = "CapDome";
        dome.transform.SetParent(headClothingObject.transform);
        dome.transform.localPosition = new Vector3(0, 0.32f, 0);
        dome.transform.localScale = new Vector3(0.575f, 0.25f, 0.575f);
        dome.GetComponent<Renderer>().material = capMat;
        Object.Destroy(dome.GetComponent<Collider>());

        // Cap visor/brim
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.name = "CapVisor";
        visor.transform.SetParent(headClothingObject.transform);
        visor.transform.localPosition = new Vector3(0, 0.28f, 0.25f);
        visor.transform.localRotation = Quaternion.Euler(-15, 0, 0);
        visor.transform.localScale = new Vector3(0.4f, 0.025f, 0.25f);
        visor.GetComponent<Renderer>().material = capMat;
        Object.Destroy(visor.GetComponent<Collider>());

        // Cap button on top
        Material whiteMat = new Material(Shader.Find("Standard"));
        whiteMat.color = Color.white;

        GameObject button = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        button.name = "CapButton";
        button.transform.SetParent(headClothingObject.transform);
        button.transform.localPosition = new Vector3(0, 0.42f, 0);
        button.transform.localScale = Vector3.one * 0.075f;
        button.GetComponent<Renderer>().material = whiteMat;
        Object.Destroy(button.GetComponent<Collider>());
    }

    void CreateTopHat()
    {
        Material hatMat = new Material(Shader.Find("Standard"));
        hatMat.color = new Color(0.1f, 0.1f, 0.1f); // Black
        hatMat.SetFloat("_Glossiness", 0.7f);

        // Hat brim - positioned to sit on head
        GameObject brim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        brim.name = "TopHatBrim";
        brim.transform.SetParent(headClothingObject.transform);
        brim.transform.localPosition = new Vector3(0, 0.32f, 0);
        brim.transform.localScale = new Vector3(0.75f, 0.02f, 0.75f);
        brim.GetComponent<Renderer>().material = hatMat;
        Object.Destroy(brim.GetComponent<Collider>());

        // Tall crown
        GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crown.name = "TopHatCrown";
        crown.transform.SetParent(headClothingObject.transform);
        crown.transform.localPosition = new Vector3(0, 0.52f, 0);
        crown.transform.localScale = new Vector3(0.45f, 0.22f, 0.45f);
        crown.GetComponent<Renderer>().material = hatMat;
        Object.Destroy(crown.GetComponent<Collider>());

        // Satin band
        Material bandMat = new Material(Shader.Find("Standard"));
        bandMat.color = new Color(0.6f, 0.1f, 0.1f); // Dark red

        GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.name = "TopHatBand";
        band.transform.SetParent(headClothingObject.transform);
        band.transform.localPosition = new Vector3(0, 0.38f, 0);
        band.transform.localScale = new Vector3(0.475f, 0.03f, 0.475f);
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
            // Ice Realm tops
            case "Husband Beater":
                torsoRenderer.material.color = new Color(0.95f, 0.95f, 0.95f); // White
                break;
            case "Pink Fur Coat":
                CreatePinkFurCoat();
                break;
            case "Bear Skin Overcoat":
                CreateBearSkinOvercoat();
                break;
            case "HAZMAT Suit":
                CreateHazmatSuitVisual();
                break;
            default:
                torsoRenderer.material.color = itemColor;
                break;
        }
    }

    void CreateCoconutBra()
    {
        if (torso == null || torsoRenderer == null) return;

        // Apply coconut bra as texture on torso - skin with coconut colored areas
        // Create a procedural texture for the coconut bra pattern
        Texture2D braTex = CreateCoconutBraTexture();
        torsoRenderer.material.mainTexture = braTex;
        torsoRenderer.material.color = Color.white; // Let texture show through
    }

    Texture2D CreateCoconutBraTexture()
    {
        // Coconut bra texture - coconuts on FRONT (chest), straps only on BACK
        // Texture wraps around capsule: x=0-0.25 is back-left, 0.25-0.75 is front, 0.75-1.0 is back-right
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color skin = new Color(0.85f, 0.7f, 0.55f);
        Color coconut = new Color(0.55f, 0.35f, 0.2f);
        Color coconutDark = new Color(0.4f, 0.25f, 0.15f);  // Darker coconut detail
        Color rope = new Color(0.6f, 0.5f, 0.35f);

        // Fill with skin color
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, skin);
            }
        }

        // Coconuts on FRONT only (middle 50% of texture = x from 16 to 48 on 64px texture)
        // Front area: x = 16 to 48 (center of texture wrap)
        int coconutRadius = 8;
        int leftCoconutX = 24;   // Left coconut on front
        int rightCoconutX = 40;  // Right coconut on front
        int coconutY = size / 2 + 4;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Only draw coconuts in front area (x between 16 and 48)
                if (x >= 12 && x <= 52)
                {
                    // Left coconut
                    float distLeft = Mathf.Sqrt((x - leftCoconutX) * (x - leftCoconutX) + (y - coconutY) * (y - coconutY));
                    if (distLeft < coconutRadius)
                    {
                        // Add some texture variation to coconut
                        float shade = 1f - distLeft / coconutRadius * 0.3f;
                        tex.SetPixel(x, y, Color.Lerp(coconutDark, coconut, shade));
                    }

                    // Right coconut
                    float distRight = Mathf.Sqrt((x - rightCoconutX) * (x - rightCoconutX) + (y - coconutY) * (y - coconutY));
                    if (distRight < coconutRadius)
                    {
                        float shade = 1f - distRight / coconutRadius * 0.3f;
                        tex.SetPixel(x, y, Color.Lerp(coconutDark, coconut, shade));
                    }
                }
            }
        }

        // Draw rope/straps that go AROUND the torso (full width for straps at back)
        int ropeY = coconutY + coconutRadius;
        for (int x = 0; x < size; x++)
        {
            // Horizontal strap across top (goes all around)
            for (int y = ropeY - 1; y <= ropeY + 1; y++)
            {
                if (y >= 0 && y < size)
                    tex.SetPixel(x, y, rope);
            }
        }

        // Vertical straps on the BACK (x < 12 or x > 52 = back area)
        // Two diagonal straps that cross in an X pattern on back
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Back left area (x < 12)
                if (x < 14)
                {
                    // Diagonal strap going up-right
                    int targetY1 = coconutY + (int)((12 - x) * 0.8f);
                    if (Mathf.Abs(y - targetY1) < 2)
                        tex.SetPixel(x, y, rope);
                }
                // Back right area (x > 50)
                if (x > 50)
                {
                    // Diagonal strap going up-left
                    int targetY2 = coconutY + (int)((x - 52) * 0.8f);
                    if (Mathf.Abs(y - targetY2) < 2)
                        tex.SetPixel(x, y, rope);
                }
            }
        }

        // Center back strap (vertical line at x=0 and x=63 which wrap together)
        for (int y = coconutY - 5; y < size; y++)
        {
            if (y >= 0 && y < size)
            {
                tex.SetPixel(0, y, rope);
                tex.SetPixel(1, y, rope);
                tex.SetPixel(62, y, rope);
                tex.SetPixel(63, y, rope);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    void CreateLumberjackShirt()
    {
        if (torso == null || torsoRenderer == null) return;

        // Apply red/black checkerboard texture to torso
        Texture2D lumberjackTex = CreateLumberjackTexture();
        torsoRenderer.material.mainTexture = lumberjackTex;
        torsoRenderer.material.color = Color.white; // Let texture show through
    }

    Texture2D CreateLumberjackTexture()
    {
        int size = 64;
        int checkSize = 8; // Size of each checker square
        Texture2D tex = new Texture2D(size, size);
        Color red = new Color(0.75f, 0.12f, 0.08f);
        Color black = new Color(0.1f, 0.08f, 0.05f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Determine which checker square we're in
                int checkX = x / checkSize;
                int checkY = y / checkSize;

                // Alternate colors based on position
                bool isRed = (checkX + checkY) % 2 == 0;
                tex.SetPixel(x, y, isRed ? red : black);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    void CreateTuxedoTop()
    {
        if (torso == null || torsoRenderer == null) return;

        // Apply tuxedo texture to torso - black jacket with white shirt front and tie
        Texture2D tuxedoTex = CreateTuxedoTexture();
        torsoRenderer.material.mainTexture = tuxedoTex;
        torsoRenderer.material.color = Color.white; // Let texture show through
    }

    Texture2D CreateTuxedoTexture()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color black = new Color(0.08f, 0.08f, 0.08f);
        Color white = new Color(0.95f, 0.95f, 0.95f);
        Color darkGray = new Color(0.05f, 0.05f, 0.05f);
        Color lapelGray = new Color(0.12f, 0.12f, 0.12f);

        // Fill with black (jacket)
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, black);
            }
        }

        // White shirt front (center strip)
        int shirtLeft = size / 2 - 8;
        int shirtRight = size / 2 + 8;
        for (int y = 0; y < size; y++)
        {
            for (int x = shirtLeft; x < shirtRight; x++)
            {
                tex.SetPixel(x, y, white);
            }
        }

        // Black tie (center of white shirt)
        int tieLeft = size / 2 - 3;
        int tieRight = size / 2 + 3;
        for (int y = 0; y < size - 10; y++)
        {
            for (int x = tieLeft; x < tieRight; x++)
            {
                tex.SetPixel(x, y, darkGray);
            }
        }

        // Tie knot (wider at top)
        int knotTop = size - 12;
        for (int y = knotTop; y < knotTop + 6; y++)
        {
            for (int x = size / 2 - 5; x < size / 2 + 5; x++)
            {
                tex.SetPixel(x, y, darkGray);
            }
        }

        // Lapels (diagonal darker areas on sides of shirt)
        for (int y = size / 3; y < size; y++)
        {
            // Left lapel
            int lapelX = shirtLeft - (size - y) / 4;
            for (int x = Mathf.Max(0, lapelX - 4); x < lapelX + 4 && x < shirtLeft; x++)
            {
                if (x >= 0) tex.SetPixel(x, y, lapelGray);
            }

            // Right lapel
            lapelX = shirtRight + (size - y) / 4;
            for (int x = shirtRight; x < lapelX + 4 && x < size; x++)
            {
                tex.SetPixel(x, y, lapelGray);
            }
        }

        // Buttons on shirt (3 small dots)
        Color buttonColor = new Color(0.8f, 0.8f, 0.8f);
        int[] buttonYs = { size / 4, size / 2, size * 3 / 4 };
        foreach (int by in buttonYs)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int px = size / 2 + dx;
                    int py = by + dy;
                    if (px >= 0 && px < size && py >= 0 && py < size)
                        tex.SetPixel(px, py, buttonColor);
                }
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    void RemoveTopClothing()
    {
        if (topClothingObject != null)
        {
            Destroy(topClothingObject);
            topClothingObject = null;
        }

        // Clear any texture on torso
        if (torsoRenderer != null)
        {
            torsoRenderer.material.mainTexture = null;
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
            case "Underpants":
                // Underpants only cover hips, legs stay skin color
                pantsColor = itemColor;
                hipsRenderer.material.color = pantsColor;
                foreach (Renderer r in legRenderers)
                {
                    if (r != null) r.material.color = skinColor;
                }
                return; // Early return - don't apply pants color to legs
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
            // Ice Realm pants
            case "Black Leather Hotpants":
                pantsColor = new Color(0.08f, 0.08f, 0.08f);
                CreateLeatherShine();
                break;
            case "Whale Bladder Pants":
                pantsColor = new Color(0.9f, 0.7f, 0.7f);
                break;
            case "Pink Leather Pants":
                pantsColor = new Color(1f, 0.3f, 0.6f);
                CreateLeatherShine();
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

        // Initialize fly timer
        if (nextFlyTime == 0f)
        {
            nextFlyTime = Time.time + Random.Range(20f, 40f);
        }

        // Check for ESC to dismiss message
        if (!string.IsNullOrEmpty(parrotMessage) && Input.GetKeyDown(KeyCode.Escape))
        {
            parrotMessage = "";
            parrotMessageTimer = 0f;
        }

        // Update message timer
        if (parrotMessageTimer > 0f)
        {
            parrotMessageTimer -= Time.deltaTime;
            if (parrotMessageTimer <= 0f)
            {
                parrotMessage = "";
            }
        }

        // Check if it's time to fly away
        if (!parrotFlying && Time.time >= nextFlyTime)
        {
            StartParrotFlight();
        }

        if (parrotFlying)
        {
            AnimateParrotFlight();
        }
        else
        {
            // Normal shoulder animation
            AnimateParrotOnShoulder();
        }
    }

    void AnimateParrotOnShoulder()
    {
        // Gentle bobbing and head turning
        float bob = Mathf.Sin(Time.time * 2f) * 0.01f;
        float headTurn = Mathf.Sin(Time.time * 0.5f) * 15f;

        parrotObject.transform.SetParent(torso);
        parrotObject.transform.localPosition = new Vector3(0.35f, 0.35f + bob, 0.05f);
        parrotObject.transform.localRotation = Quaternion.identity;

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

    void StartParrotFlight()
    {
        parrotFlying = true;
        parrotReturning = false;
        flyProgress = 0f;
        parrotStartPos = parrotObject.transform.position;

        // Unparent from player for world-space flight
        parrotObject.transform.SetParent(null);

        // Pick a random direction to fly
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(8f, 15f);
        float height = Random.Range(5f, 10f);
        parrotFlyTarget = parrotStartPos + new Vector3(Mathf.Cos(angle) * distance, height, Mathf.Sin(angle) * distance);

        // Squawk when flying away
        parrotMessage = "SQUAWK! I'll be back!";
        parrotMessageTimer = 2f;
    }

    void AnimateParrotFlight()
    {
        Transform wing = parrotObject.transform.Find("ParrotWing");

        if (!parrotReturning)
        {
            // Flying away
            flyProgress += Time.deltaTime * 0.5f;

            if (flyProgress >= 1f)
            {
                // Start returning after a delay
                parrotReturning = true;
                flyProgress = 0f;
                parrotStartPos = parrotFlyTarget;
            }
            else
            {
                // Fly to target with arc
                float arc = Mathf.Sin(flyProgress * Mathf.PI) * 3f;
                Vector3 newPos = Vector3.Lerp(parrotStartPos, parrotFlyTarget, flyProgress);
                newPos.y += arc;
                parrotObject.transform.position = newPos;

                // Face flying direction
                Vector3 dir = (parrotFlyTarget - parrotStartPos).normalized;
                if (dir.magnitude > 0.1f)
                {
                    parrotObject.transform.rotation = Quaternion.LookRotation(dir);
                }

                // Constant wing flapping while flying
                if (wing != null)
                {
                    float flap = Mathf.Sin(Time.time * 20f) * 45f;
                    wing.localRotation = Quaternion.Euler(0, -20, 15 + flap);
                }
            }
        }
        else
        {
            // Flying back to player
            flyProgress += Time.deltaTime * 0.6f;

            Vector3 playerShoulderPos = torso != null ? torso.position + torso.right * 0.35f + torso.up * 0.35f + torso.forward * 0.05f : transform.position;

            if (flyProgress >= 1f)
            {
                // Land on shoulder
                parrotFlying = false;
                parrotObject.transform.SetParent(torso);
                parrotObject.transform.localPosition = new Vector3(0.35f, 0.35f, 0.05f);
                parrotObject.transform.localRotation = Quaternion.identity;

                // Schedule next flight
                nextFlyTime = Time.time + Random.Range(30f, 60f);

                // Say something when landing
                parrotMessage = parrotPhrases[Random.Range(0, parrotPhrases.Length)];
                parrotMessageTimer = 3f;
            }
            else
            {
                // Fly back to player
                float arc = Mathf.Sin(flyProgress * Mathf.PI) * 2f;
                Vector3 newPos = Vector3.Lerp(parrotStartPos, playerShoulderPos, flyProgress);
                newPos.y += arc;
                parrotObject.transform.position = newPos;

                // Face player
                Vector3 dir = (playerShoulderPos - parrotObject.transform.position).normalized;
                if (dir.magnitude > 0.1f)
                {
                    parrotObject.transform.rotation = Quaternion.LookRotation(dir);
                }

                // Wing flapping
                if (wing != null)
                {
                    float flap = Mathf.Sin(Time.time * 20f) * 45f;
                    wing.localRotation = Quaternion.Euler(0, -20, 15 + flap);
                }
            }
        }
    }

    void OnGUI()
    {
        // Draw parrot speech bubble
        if (!string.IsNullOrEmpty(parrotMessage) && parrotObject != null && MainMenu.GameStarted)
        {
            Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(parrotObject.transform.position + Vector3.up * 0.5f) : Vector3.zero;

            if (screenPos.z > 0)
            {
                float bubbleWidth = 180;
                float bubbleHeight = 50;
                float bubbleX = screenPos.x - bubbleWidth / 2;
                float bubbleY = Screen.height - screenPos.y - bubbleHeight - 20;

                // Speech bubble background
                GUI.color = new Color(1f, 1f, 0.8f, 0.95f);
                GUI.DrawTexture(new Rect(bubbleX, bubbleY, bubbleWidth, bubbleHeight), Texture2D.whiteTexture);
                GUI.color = new Color(0.3f, 0.3f, 0.2f);
                GUI.DrawTexture(new Rect(bubbleX + 2, bubbleY + 2, bubbleWidth - 4, bubbleHeight - 4), Texture2D.whiteTexture);
                GUI.color = new Color(1f, 1f, 0.8f);
                GUI.DrawTexture(new Rect(bubbleX + 4, bubbleY + 4, bubbleWidth - 8, bubbleHeight - 8), Texture2D.whiteTexture);

                // Message text
                GUIStyle msgStyle = new GUIStyle();
                msgStyle.fontSize = 11;
                msgStyle.fontStyle = FontStyle.Bold;
                msgStyle.alignment = TextAnchor.MiddleCenter;
                msgStyle.normal.textColor = new Color(0.2f, 0.15f, 0.1f);
                msgStyle.wordWrap = true;
                GUI.color = Color.white;
                GUI.Label(new Rect(bubbleX + 8, bubbleY + 8, bubbleWidth - 16, bubbleHeight - 16), parrotMessage, msgStyle);

                // ESC hint
                GUIStyle hintStyle = new GUIStyle();
                hintStyle.fontSize = 9;
                hintStyle.alignment = TextAnchor.MiddleCenter;
                hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.4f);
                GUI.Label(new Rect(bubbleX, bubbleY + bubbleHeight + 2, bubbleWidth, 14), "[ESC to dismiss]", hintStyle);
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

    // ==================== ICE REALM CLOTHING ====================

    void CreateBearSkinHat()
    {
        // White fluffy polar bear hat with 2 ears
        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = new Color(0.95f, 0.95f, 0.98f);

        // Main fluffy hat body - positioned to sit on head
        GameObject hatBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hatBody.name = "BearHatBody";
        hatBody.transform.SetParent(headClothingObject.transform);
        hatBody.transform.localPosition = new Vector3(0, 0.32f, 0);
        hatBody.transform.localScale = new Vector3(0.65f, 0.35f, 0.6f);
        hatBody.GetComponent<Renderer>().material = furMat;
        Object.Destroy(hatBody.GetComponent<Collider>());

        // Left ear
        GameObject leftEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEar.name = "LeftEar";
        leftEar.transform.SetParent(headClothingObject.transform);
        leftEar.transform.localPosition = new Vector3(-0.2f, 0.52f, 0);
        leftEar.transform.localScale = new Vector3(0.15f, 0.12f, 0.1f);
        leftEar.GetComponent<Renderer>().material = furMat;
        Object.Destroy(leftEar.GetComponent<Collider>());

        // Right ear
        GameObject rightEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEar.name = "RightEar";
        rightEar.transform.SetParent(headClothingObject.transform);
        rightEar.transform.localPosition = new Vector3(0.2f, 0.52f, 0);
        rightEar.transform.localScale = new Vector3(0.15f, 0.12f, 0.1f);
        rightEar.GetComponent<Renderer>().material = furMat;
        Object.Destroy(rightEar.GetComponent<Collider>());

        // Fluffy tufts
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            GameObject tuft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tuft.name = "FurTuft";
            tuft.transform.SetParent(headClothingObject.transform);
            tuft.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.25f,
                0.27f + Random.Range(0f, 0.1f),
                Mathf.Sin(angle) * 0.22f
            );
            tuft.transform.localScale = Vector3.one * Random.Range(0.08f, 0.12f);
            tuft.GetComponent<Renderer>().material = furMat;
            Object.Destroy(tuft.GetComponent<Collider>());
        }
    }

    void CreatePirateHat()
    {
        // Black tricorn pirate hat with skull
        Material blackMat = new Material(Shader.Find("Standard"));
        blackMat.color = new Color(0.08f, 0.08f, 0.1f);

        Material goldMat = new Material(Shader.Find("Standard"));
        goldMat.color = new Color(0.85f, 0.7f, 0.3f);
        goldMat.SetFloat("_Metallic", 0.5f);

        // Main hat body - positioned to sit on head
        GameObject hatBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hatBody.name = "PirateHatBody";
        hatBody.transform.SetParent(headClothingObject.transform);
        hatBody.transform.localPosition = new Vector3(0, 0.37f, 0);
        hatBody.transform.localScale = new Vector3(0.6f, 0.15f, 0.55f);
        hatBody.GetComponent<Renderer>().material = blackMat;
        Object.Destroy(hatBody.GetComponent<Collider>());

        // Tricorn peaks
        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            GameObject peak = GameObject.CreatePrimitive(PrimitiveType.Cube);
            peak.name = "TricornPeak";
            peak.transform.SetParent(headClothingObject.transform);
            peak.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.2f,
                0.47f,
                Mathf.Sin(angle) * 0.2f
            );
            peak.transform.localRotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 30);
            peak.transform.localScale = new Vector3(0.25f, 0.08f, 0.15f);
            peak.GetComponent<Renderer>().material = blackMat;
            Object.Destroy(peak.GetComponent<Collider>());
        }

        // Gold trim
        GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trim.name = "GoldTrim";
        trim.transform.SetParent(headClothingObject.transform);
        trim.transform.localPosition = new Vector3(0, 0.29f, 0);
        trim.transform.localScale = new Vector3(0.62f, 0.02f, 0.57f);
        trim.GetComponent<Renderer>().material = goldMat;
        Object.Destroy(trim.GetComponent<Collider>());
    }

    void CreateBeret()
    {
        // French beret - burgundy red
        Material beretMat = new Material(Shader.Find("Standard"));
        beretMat.color = new Color(0.6f, 0.1f, 0.15f);

        // Main beret body (flat and round) - positioned to sit on head
        GameObject beretBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beretBody.name = "BeretBody";
        beretBody.transform.SetParent(headClothingObject.transform);
        beretBody.transform.localPosition = new Vector3(0.05f, 0.32f, 0);
        beretBody.transform.localRotation = Quaternion.Euler(0, 0, 15);
        beretBody.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);
        beretBody.GetComponent<Renderer>().material = beretMat;
        Object.Destroy(beretBody.GetComponent<Collider>());

        // Little stem on top
        Material blackMat = new Material(Shader.Find("Standard"));
        blackMat.color = new Color(0.1f, 0.1f, 0.1f);

        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stem.name = "BeretStem";
        stem.transform.SetParent(headClothingObject.transform);
        stem.transform.localPosition = new Vector3(0.05f, 0.39f, 0);
        stem.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        stem.GetComponent<Renderer>().material = blackMat;
        Object.Destroy(stem.GetComponent<Collider>());
    }

    void CreatePinkFurCoat()
    {
        // Luxurious pink fur coat with fluffy texture
        Material pinkFurMat = new Material(Shader.Find("Standard"));
        pinkFurMat.color = new Color(1f, 0.6f, 0.7f);

        // Change torso color
        torsoRenderer.material.color = new Color(1f, 0.6f, 0.7f);

        topClothingObject = new GameObject("PinkFurCoat");
        topClothingObject.transform.SetParent(torso);
        topClothingObject.transform.localPosition = Vector3.zero;

        // Fluffy collar
        Material lightPinkMat = new Material(Shader.Find("Standard"));
        lightPinkMat.color = new Color(1f, 0.75f, 0.82f);

        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        collar.name = "FurCollar";
        collar.transform.SetParent(topClothingObject.transform);
        collar.transform.localPosition = new Vector3(0, 0.4f, 0);
        collar.transform.localScale = new Vector3(0.7f, 0.12f, 0.5f);
        collar.GetComponent<Renderer>().material = lightPinkMat;
        Object.Destroy(collar.GetComponent<Collider>());

        // Fluffy tufts around collar and body (hairs sticking out)
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30f * Mathf.Deg2Rad;
            GameObject tuft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tuft.name = "FurTuft";
            tuft.transform.SetParent(topClothingObject.transform);
            tuft.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.35f,
                0.35f + Random.Range(-0.1f, 0.1f),
                Mathf.Sin(angle) * 0.25f
            );
            tuft.transform.localScale = Vector3.one * Random.Range(0.06f, 0.1f);
            tuft.GetComponent<Renderer>().material = i % 2 == 0 ? lightPinkMat : pinkFurMat;
            Object.Destroy(tuft.GetComponent<Collider>());
        }

        // Fluffy cuffs
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject cuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cuff.name = "FurCuff";
            cuff.transform.SetParent(topClothingObject.transform);
            cuff.transform.localPosition = new Vector3(side * 0.45f, -0.1f, 0);
            cuff.transform.localScale = new Vector3(0.2f, 0.08f, 0.2f);
            cuff.GetComponent<Renderer>().material = lightPinkMat;
            Object.Destroy(cuff.GetComponent<Collider>());
        }
    }

    void CreateLeatherShine()
    {
        // Add glossy/shiny effect to current pants
        if (hipsRenderer != null)
        {
            hipsRenderer.material.SetFloat("_Glossiness", 0.8f);
            hipsRenderer.material.SetFloat("_Metallic", 0.1f);
        }
        foreach (Renderer r in legRenderers)
        {
            if (r != null && !r.name.Contains("Foot"))
            {
                r.material.SetFloat("_Glossiness", 0.8f);
                r.material.SetFloat("_Metallic", 0.1f);
            }
        }
    }

    void CreateBearSkinOvercoat()
    {
        // Long white polar bear fur overcoat - covers entire body
        Color furColor = new Color(0.97f, 0.97f, 0.99f);
        Color lightFur = new Color(0.92f, 0.93f, 0.95f);

        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = furColor;

        Material lightFurMat = new Material(Shader.Find("Standard"));
        lightFurMat.color = lightFur;

        // Change torso color to white fur
        torsoRenderer.material.color = furColor;

        // Change legs to white fur too (full body coverage)
        if (hipsRenderer != null) hipsRenderer.material.color = furColor;
        foreach (Renderer r in legRenderers)
        {
            if (r != null && !r.name.Contains("Foot"))
            {
                r.material.color = furColor;
            }
        }

        topClothingObject = new GameObject("BearSkinOvercoat");
        topClothingObject.transform.SetParent(torso);
        topClothingObject.transform.localPosition = Vector3.zero;

        // Big fluffy collar
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        collar.name = "FurCollar";
        collar.transform.SetParent(topClothingObject.transform);
        collar.transform.localPosition = new Vector3(0, 0.45f, 0);
        collar.transform.localScale = new Vector3(0.8f, 0.15f, 0.6f);
        collar.GetComponent<Renderer>().material = lightFurMat;
        Object.Destroy(collar.GetComponent<Collider>());

        // Fluffy tufts around collar
        for (int i = 0; i < 10; i++)
        {
            float angle = i * 36f * Mathf.Deg2Rad;
            GameObject tuft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tuft.name = "CollarTuft";
            tuft.transform.SetParent(topClothingObject.transform);
            tuft.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.4f,
                0.45f + Random.Range(-0.05f, 0.1f),
                Mathf.Sin(angle) * 0.3f
            );
            tuft.transform.localScale = Vector3.one * Random.Range(0.08f, 0.14f);
            tuft.GetComponent<Renderer>().material = i % 2 == 0 ? furMat : lightFurMat;
            Object.Destroy(tuft.GetComponent<Collider>());
        }

        // Long coat tails extending down over legs
        GameObject leftTail = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftTail.name = "CoatTailLeft";
        leftTail.transform.SetParent(topClothingObject.transform);
        leftTail.transform.localPosition = new Vector3(-0.15f, -0.7f, 0);
        leftTail.transform.localScale = new Vector3(0.35f, 0.5f, 0.25f);
        leftTail.GetComponent<Renderer>().material = furMat;
        Object.Destroy(leftTail.GetComponent<Collider>());

        GameObject rightTail = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightTail.name = "CoatTailRight";
        rightTail.transform.SetParent(topClothingObject.transform);
        rightTail.transform.localPosition = new Vector3(0.15f, -0.7f, 0);
        rightTail.transform.localScale = new Vector3(0.35f, 0.5f, 0.25f);
        rightTail.GetComponent<Renderer>().material = furMat;
        Object.Destroy(rightTail.GetComponent<Collider>());

        // Back of coat (longer)
        GameObject backTail = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        backTail.name = "CoatBack";
        backTail.transform.SetParent(topClothingObject.transform);
        backTail.transform.localPosition = new Vector3(0, -0.6f, -0.15f);
        backTail.transform.localScale = new Vector3(0.55f, 0.55f, 0.2f);
        backTail.GetComponent<Renderer>().material = furMat;
        Object.Destroy(backTail.GetComponent<Collider>());

        // Fluffy cuffs
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject cuff = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cuff.name = "FurCuff";
            cuff.transform.SetParent(topClothingObject.transform);
            cuff.transform.localPosition = new Vector3(side * 0.5f, -0.15f, 0);
            cuff.transform.localScale = new Vector3(0.22f, 0.1f, 0.22f);
            cuff.GetComponent<Renderer>().material = lightFurMat;
            Object.Destroy(cuff.GetComponent<Collider>());

            // Cuff tufts
            for (int t = 0; t < 4; t++)
            {
                float tAngle = t * 90f * Mathf.Deg2Rad;
                GameObject cuffTuft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cuffTuft.name = "CuffTuft";
                cuffTuft.transform.SetParent(topClothingObject.transform);
                cuffTuft.transform.localPosition = new Vector3(
                    side * 0.5f + Mathf.Cos(tAngle) * 0.12f,
                    -0.15f,
                    Mathf.Sin(tAngle) * 0.12f
                );
                cuffTuft.transform.localScale = Vector3.one * 0.06f;
                cuffTuft.GetComponent<Renderer>().material = furMat;
                Object.Destroy(cuffTuft.GetComponent<Collider>());
            }
        }

        // Fluffy hem at bottom
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            GameObject hemTuft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hemTuft.name = "HemTuft";
            hemTuft.transform.SetParent(topClothingObject.transform);
            hemTuft.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 0.3f,
                -1.1f + Random.Range(-0.05f, 0.05f),
                Mathf.Sin(angle) * 0.2f
            );
            hemTuft.transform.localScale = Vector3.one * Random.Range(0.06f, 0.1f);
            hemTuft.GetComponent<Renderer>().material = i % 2 == 0 ? furMat : lightFurMat;
            Object.Destroy(hemTuft.GetComponent<Collider>());
        }
    }

    void CreateHazmatSuitVisual()
    {
        // Yellow HAZMAT suit with dark visor and warning stripes
        Color hazmatYellow = new Color(0.95f, 0.85f, 0.1f);
        Color hazmatDark = new Color(0.7f, 0.6f, 0.05f);
        Color blackStripe = new Color(0.1f, 0.1f, 0.1f);
        Color visorColor = new Color(0.2f, 0.3f, 0.35f);

        Material hazmatMat = new Material(Shader.Find("Standard"));
        hazmatMat.color = hazmatYellow;

        Material hazmatDarkMat = new Material(Shader.Find("Standard"));
        hazmatDarkMat.color = hazmatDark;

        Material stripeMat = new Material(Shader.Find("Standard"));
        stripeMat.color = blackStripe;

        Material visorMat = new Material(Shader.Find("Standard"));
        visorMat.color = visorColor;
        visorMat.SetFloat("_Metallic", 0.8f);
        visorMat.SetFloat("_Glossiness", 0.9f);

        // Change torso to hazmat yellow
        torsoRenderer.material.color = hazmatYellow;

        // Change legs to hazmat yellow (full body suit)
        if (hipsRenderer != null) hipsRenderer.material.color = hazmatYellow;
        foreach (Renderer r in legRenderers)
        {
            if (r != null && !r.name.Contains("Foot"))
            {
                r.material.color = hazmatYellow;
            }
        }

        topClothingObject = new GameObject("HazmatSuit");
        topClothingObject.transform.SetParent(torso);
        topClothingObject.transform.localPosition = Vector3.zero;

        // Helmet/Hood
        GameObject helmet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        helmet.name = "HazmatHelmet";
        helmet.transform.SetParent(headClothingObject != null ? headClothingObject.transform : topClothingObject.transform);
        if (head != null)
        {
            helmet.transform.SetParent(head);
            helmet.transform.localPosition = new Vector3(0, 0.1f, 0);
        }
        else
        {
            helmet.transform.SetParent(topClothingObject.transform);
            helmet.transform.localPosition = new Vector3(0, 0.9f, 0);
        }
        helmet.transform.localScale = new Vector3(0.55f, 0.5f, 0.5f);
        helmet.GetComponent<Renderer>().material = hazmatMat;
        Object.Destroy(helmet.GetComponent<Collider>());

        // Visor (dark face shield)
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visor.name = "HazmatVisor";
        visor.transform.SetParent(helmet.transform);
        visor.transform.localPosition = new Vector3(0, 0, 0.4f);
        visor.transform.localScale = new Vector3(0.65f, 0.7f, 0.4f);
        visor.GetComponent<Renderer>().material = visorMat;
        Object.Destroy(visor.GetComponent<Collider>());

        // Breathing apparatus tubes
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tube.name = "BreathingTube";
            tube.transform.SetParent(helmet.transform);
            tube.transform.localPosition = new Vector3(side * 0.35f, -0.25f, 0.2f);
            tube.transform.localRotation = Quaternion.Euler(0, 0, side * 15f);
            tube.transform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
            tube.GetComponent<Renderer>().material = hazmatDarkMat;
            Object.Destroy(tube.GetComponent<Collider>());
        }

        // Warning stripes on torso
        for (int i = 0; i < 3; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "WarningStripe";
            stripe.transform.SetParent(topClothingObject.transform);
            stripe.transform.localPosition = new Vector3(0, 0.1f - i * 0.15f, 0.26f);
            stripe.transform.localScale = new Vector3(0.5f, 0.04f, 0.02f);
            stripe.GetComponent<Renderer>().material = stripeMat;
            Object.Destroy(stripe.GetComponent<Collider>());
        }

        // Gloves (dark rubber)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject glove = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glove.name = "HazmatGlove";
            glove.transform.SetParent(topClothingObject.transform);
            glove.transform.localPosition = new Vector3(side * 0.45f, -0.3f, 0);
            glove.transform.localScale = new Vector3(0.15f, 0.12f, 0.18f);
            glove.GetComponent<Renderer>().material = hazmatDarkMat;
            Object.Destroy(glove.GetComponent<Collider>());
        }

        // Oxygen tank on back
        GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tank.name = "OxygenTank";
        tank.transform.SetParent(topClothingObject.transform);
        tank.transform.localPosition = new Vector3(0, 0.1f, -0.25f);
        tank.transform.localScale = new Vector3(0.2f, 0.25f, 0.15f);
        tank.GetComponent<Renderer>().material = hazmatDarkMat;
        Object.Destroy(tank.GetComponent<Collider>());

        // Tank valve
        GameObject valve = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        valve.name = "TankValve";
        valve.transform.SetParent(tank.transform);
        valve.transform.localPosition = new Vector3(0, 0.55f, 0);
        valve.transform.localRotation = Quaternion.Euler(90, 0, 0);
        valve.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);

        Material valveMat = new Material(Shader.Find("Standard"));
        valveMat.color = new Color(0.7f, 0.7f, 0.7f);
        valveMat.SetFloat("_Metallic", 0.9f);
        valve.GetComponent<Renderer>().material = valveMat;
        Object.Destroy(valve.GetComponent<Collider>());

        // Boots (dark rubber)
        foreach (Renderer r in legRenderers)
        {
            if (r != null && r.name.Contains("Foot"))
            {
                r.material.color = hazmatDark;
            }
        }

        Debug.Log("HAZMAT Suit equipped - Toxic immunity active!");
    }
}
