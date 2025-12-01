using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// HAZMAT suit vendor in the Void Realm
/// Sells protective suit against toxic slime puddles
/// Features purple/cyan neon theme matching void realm aesthetics
/// </summary>
public class HazmatVendorNPC : MonoBehaviour
{
    public static HazmatVendorNPC Instance { get; private set; }

    private bool shopOpen = false;
    private bool playerNearby = false;
    private float interactionDistance = 4f;

    // Shop item - HAZMAT Suit
    private AccessoryItem hazmatSuit;

    // UI Textures
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D hazmatIcon;

    // Void realm neon colors
    private Color neonPurple = new Color(0.8f, 0.2f, 1f);
    private Color neonCyan = new Color(0.1f, 0.8f, 1f);
    private Color neonMagenta = new Color(1f, 0.1f, 0.8f);
    private Color darkBg = new Color(0.1f, 0.1f, 0.15f);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateVendorModel();
        InitializeShop();
        CreateUITextures();
        CreateHazmatIcon();
    }

    void CreateVendorModel()
    {
        // HAZMAT suit colors
        Material hazmatYellow = new Material(Shader.Find("Standard"));
        hazmatYellow.color = new Color(0.95f, 0.85f, 0.1f); // Bright yellow

        Material hazmatYellowDark = new Material(Shader.Find("Standard"));
        hazmatYellowDark.color = new Color(0.7f, 0.6f, 0.05f);

        Material visor = new Material(Shader.Find("Standard"));
        visor.color = new Color(0.2f, 0.3f, 0.35f, 0.8f); // Dark tinted visor
        visor.SetFloat("_Metallic", 0.8f);
        visor.SetFloat("_Glossiness", 0.9f);

        Material gloves = new Material(Shader.Find("Standard"));
        gloves.color = new Color(0.3f, 0.3f, 0.35f); // Dark rubber

        // Body (hazmat suit torso)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 0.8f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.7f, 0.5f);
        body.GetComponent<Renderer>().material = hazmatYellow;
        Object.Destroy(body.GetComponent<Collider>());

        // Head (helmet/hood)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 1.4f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);
        head.GetComponent<Renderer>().material = hazmatYellow;
        Object.Destroy(head.GetComponent<Collider>());

        // Visor (face shield)
        GameObject visorObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visorObj.name = "Visor";
        visorObj.transform.SetParent(head.transform);
        visorObj.transform.localPosition = new Vector3(0, 0, 0.5f);
        visorObj.transform.localScale = new Vector3(0.7f, 0.8f, 0.5f);
        visorObj.GetComponent<Renderer>().material = visor;
        Object.Destroy(visorObj.GetComponent<Collider>());

        // Breathing apparatus (tubes on sides)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tube.name = "BreathingTube";
            tube.transform.SetParent(head.transform);
            tube.transform.localPosition = new Vector3(side * 0.4f, -0.3f, 0.3f);
            tube.transform.localRotation = Quaternion.Euler(0, 0, side * 15f);
            tube.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
            tube.GetComponent<Renderer>().material = gloves;
            Object.Destroy(tube.GetComponent<Collider>());
        }

        // Arms (protective sleeves)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            arm.name = "Arm";
            arm.transform.SetParent(transform);
            arm.transform.localPosition = new Vector3(side * 0.35f, 0.75f, 0);
            arm.transform.localRotation = Quaternion.Euler(0, 0, side * 80f);
            arm.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
            arm.GetComponent<Renderer>().material = hazmatYellow;
            Object.Destroy(arm.GetComponent<Collider>());

            // Gloves
            GameObject glove = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glove.name = "Glove";
            glove.transform.SetParent(arm.transform);
            glove.transform.localPosition = new Vector3(0, -1.1f, 0);
            glove.transform.localScale = new Vector3(1.3f, 1f, 1.3f);
            glove.GetComponent<Renderer>().material = gloves;
            Object.Destroy(glove.GetComponent<Collider>());
        }

        // Legs (protective pants)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = "Leg";
            leg.transform.SetParent(transform);
            leg.transform.localPosition = new Vector3(side * 0.15f, 0.35f, 0);
            leg.transform.localScale = new Vector3(0.18f, 0.4f, 0.18f);
            leg.GetComponent<Renderer>().material = hazmatYellowDark;
            Object.Destroy(leg.GetComponent<Collider>());

            // Boots
            GameObject boot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boot.name = "Boot";
            boot.transform.SetParent(leg.transform);
            boot.transform.localPosition = new Vector3(0, -1.1f, 0.15f);
            boot.transform.localScale = new Vector3(1.2f, 0.5f, 1.5f);
            boot.GetComponent<Renderer>().material = gloves;
            Object.Destroy(boot.GetComponent<Collider>());
        }

        // Oxygen tank (backpack)
        GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tank.name = "OxygenTank";
        tank.transform.SetParent(transform);
        tank.transform.localPosition = new Vector3(0, 0.9f, -0.25f);
        tank.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        tank.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
        tank.GetComponent<Renderer>().material = gloves;
        Object.Destroy(tank.GetComponent<Collider>());

        // Warning stripes on suit
        for (int i = 0; i < 3; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "WarningStripe";
            stripe.transform.SetParent(body.transform);
            stripe.transform.localPosition = new Vector3(0, -0.3f + i * 0.3f, 0.5f);
            stripe.transform.localScale = new Vector3(0.8f, 0.08f, 0.01f);
            Material stripeMat = new Material(Shader.Find("Standard"));
            stripeMat.color = new Color(0.15f, 0.15f, 0.15f); // Black stripes
            stripe.GetComponent<Renderer>().material = stripeMat;
            Object.Destroy(stripe.GetComponent<Collider>());
        }

        // Neon glow accent (void realm aesthetic)
        GameObject neonAccent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        neonAccent.name = "NeonAccent";
        neonAccent.transform.SetParent(body.transform);
        neonAccent.transform.localPosition = new Vector3(0, 0.5f, 0.52f);
        neonAccent.transform.localScale = new Vector3(0.6f, 0.15f, 0.01f);
        Material neonMat = new Material(Shader.Find("Standard"));
        neonMat.color = neonCyan;
        neonMat.EnableKeyword("_EMISSION");
        neonMat.SetColor("_EmissionColor", neonCyan * 2f);
        neonAccent.GetComponent<Renderer>().material = neonMat;
        Object.Destroy(neonAccent.GetComponent<Collider>());
    }

    void InitializeShop()
    {
        hazmatSuit = new AccessoryItem
        {
            name = "HAZMAT Suit",
            slot = "Outfit",
            price = 25000,
            description = "Heavy-duty protective suit. Grants complete immunity to toxic slime hazards.",
            effect = AccessoryEffect.ToxicImmunity
        };
    }

    void CreateUITextures()
    {
        // Dark void-themed panel
        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0.08f, 0.08f, 0.12f, 0.95f));
        panelTexture.Apply();

        // Purple-tinted button
        buttonTexture = new Texture2D(1, 1);
        buttonTexture.SetPixel(0, 0, new Color(0.2f, 0.15f, 0.25f, 0.9f));
        buttonTexture.Apply();

        // Cyan-tinted hover
        buttonHoverTexture = new Texture2D(1, 1);
        buttonHoverTexture.SetPixel(0, 0, new Color(0.15f, 0.25f, 0.3f, 0.95f));
        buttonHoverTexture.Apply();
    }

    void CreateHazmatIcon()
    {
        hazmatIcon = new Texture2D(24, 24);
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < 24; x++)
            for (int y = 0; y < 24; y++)
                hazmatIcon.SetPixel(x, y, clear);

        Color yellow = new Color(0.95f, 0.85f, 0.1f);
        Color yellowDark = new Color(0.7f, 0.6f, 0.05f);
        Color visorColor = new Color(0.2f, 0.3f, 0.35f);
        Color black = new Color(0.15f, 0.15f, 0.15f);

        // Helmet
        FillRect(hazmatIcon, 8, 14, 8, 8, yellow);
        FillRect(hazmatIcon, 10, 18, 4, 3, visorColor);

        // Body/suit
        FillRect(hazmatIcon, 7, 4, 10, 10, yellow);

        // Warning stripes
        FillRect(hazmatIcon, 8, 10, 8, 1, black);
        FillRect(hazmatIcon, 8, 7, 8, 1, black);

        // Arms
        FillRect(hazmatIcon, 4, 8, 3, 6, yellow);
        FillRect(hazmatIcon, 17, 8, 3, 6, yellow);

        // Gloves (dark)
        FillRect(hazmatIcon, 4, 8, 3, 2, yellowDark);
        FillRect(hazmatIcon, 17, 8, 3, 2, yellowDark);

        // Breathing apparatus
        hazmatIcon.SetPixel(9, 16, black);
        hazmatIcon.SetPixel(15, 16, black);

        // Highlight
        hazmatIcon.SetPixel(11, 20, Color.white);
        hazmatIcon.SetPixel(12, 20, Color.white);

        hazmatIcon.Apply();
        hazmatIcon.filterMode = FilterMode.Point;
    }

    void FillRect(Texture2D tex, int x, int y, int w, int h, Color col)
    {
        for (int px = x; px < x + w && px < 24; px++)
            for (int py = y; py < y + h && py < 24; py++)
                if (px >= 0 && py >= 0)
                    tex.SetPixel(px, py, col);
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerNearby = distance < interactionDistance;

        if (playerNearby && !shopOpen && Input.GetKeyDown(KeyCode.F))
        {
            OpenShop();
        }

        if (shopOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }

        // Idle animation - slight bob
        if (!shopOpen)
        {
            float bob = Mathf.Sin(Time.time * 1.2f) * 0.05f;
            transform.position = new Vector3(transform.position.x, 0.5f + bob, transform.position.z);
        }
    }

    void OpenShop()
    {
        shopOpen = true;
    }

    void CloseShop()
    {
        shopOpen = false;
    }

    void TryPurchaseHazmat()
    {
        // Check if already owned
        if (AccessorySystem.Instance != null && AccessorySystem.Instance.HasAccessory("HAZMAT Suit"))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Already owned!", new Color(0.9f, 0.7f, 0.3f));
            }
            return;
        }

        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;

        if (playerGold >= hazmatSuit.price)
        {
            GameManager.Instance.AddCoins(-hazmatSuit.price);

            if (AccessorySystem.Instance != null)
            {
                AccessorySystem.Instance.AddAccessory(hazmatSuit);
                AccessorySystem.Instance.EquipAccessory(hazmatSuit);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"Purchased {hazmatSuit.name}!", neonCyan);
            }

            Debug.Log($"Purchased and equipped {hazmatSuit.name}!");
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Not enough gold!", new Color(0.9f, 0.3f, 0.3f));
            }
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        if (playerNearby && !shopOpen)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 18;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = neonCyan;

            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 150, 300, 30),
                "[F] HAZMAT Vendor", promptStyle);
        }

        if (shopOpen)
        {
            DrawShopUI();
        }
    }

    void DrawShopUI()
    {
        float panelWidth = 500;
        float panelHeight = 350;
        Rect panelRect = new Rect(
            Screen.width / 2 - panelWidth / 2,
            Screen.height / 2 - panelHeight / 2,
            panelWidth,
            panelHeight
        );

        GUI.DrawTexture(panelRect, panelTexture);

        // Neon border (purple/cyan alternating)
        GUI.color = neonPurple;
        GUI.DrawTexture(new Rect(panelRect.x - 3, panelRect.y - 3, panelRect.width + 6, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 3, panelRect.y + panelRect.height, panelRect.width + 6, 3), Texture2D.whiteTexture);
        GUI.color = neonCyan;
        GUI.DrawTexture(new Rect(panelRect.x - 3, panelRect.y, 3, panelRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x + panelRect.width, panelRect.y, 3, panelRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title with neon glow effect
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 24;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = neonCyan;

        // Glow effect
        GUI.color = new Color(neonCyan.r, neonCyan.g, neonCyan.b, 0.3f);
        GUI.Label(new Rect(panelRect.x - 1, panelRect.y + 14, panelRect.width, 35), "HAZMAT VENDOR", titleStyle);
        GUI.Label(new Rect(panelRect.x + 1, panelRect.y + 14, panelRect.width, 35), "HAZMAT VENDOR", titleStyle);
        GUI.Label(new Rect(panelRect.x, panelRect.y + 13, panelRect.width, 35), "HAZMAT VENDOR", titleStyle);
        GUI.Label(new Rect(panelRect.x, panelRect.y + 15, panelRect.width, 35), "HAZMAT VENDOR", titleStyle);
        GUI.color = Color.white;
        GUI.Label(new Rect(panelRect.x, panelRect.y + 14, panelRect.width, 35), "HAZMAT VENDOR", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = 11;
        subStyle.fontStyle = FontStyle.Italic;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = neonMagenta;

        GUI.Label(new Rect(panelRect.x, panelRect.y + 48, panelRect.width, 20),
            "Protection from toxic hazards", subStyle);

        // Gold display
        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUIStyle goldStyle = new GUIStyle();
        goldStyle.fontSize = 14;
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.alignment = TextAnchor.MiddleRight;
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        GUI.Label(new Rect(panelRect.x + panelRect.width - 160, panelRect.y + 17, 150, 25),
            $"Gold: {playerGold:N0}", goldStyle);

        // Close button (X)
        GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
        closeStyle.fontSize = 14;
        closeStyle.fontStyle = FontStyle.Bold;
        closeStyle.normal.textColor = neonPurple;

        if (GUI.Button(new Rect(panelRect.x + panelRect.width - 35, panelRect.y + 8, 28, 28), "X", closeStyle))
        {
            CloseShop();
        }

        // HAZMAT Suit item display
        float itemY = panelRect.y + 90;
        Rect itemRect = new Rect(panelRect.x + 25, itemY, panelRect.width - 50, 150);

        bool owned = AccessorySystem.Instance != null && AccessorySystem.Instance.HasAccessory(hazmatSuit.name);
        bool equipped = AccessorySystem.Instance != null && AccessorySystem.Instance.IsEquipped(hazmatSuit.name);

        GUI.DrawTexture(itemRect, equipped ? buttonHoverTexture : buttonTexture);

        // Neon border on item
        GUI.color = owned ? neonCyan : neonPurple;
        GUI.DrawTexture(new Rect(itemRect.x - 2, itemRect.y - 2, itemRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(itemRect.x - 2, itemRect.y + itemRect.height, itemRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(itemRect.x - 2, itemRect.y, 2, itemRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(itemRect.x + itemRect.width, itemRect.y, 2, itemRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Icon
        if (hazmatIcon != null)
        {
            GUI.DrawTexture(new Rect(itemRect.x + 20, itemRect.y + 20, 80, 80), hazmatIcon);
        }

        // Name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 20;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleLeft;
        nameStyle.normal.textColor = equipped ? neonCyan : Color.white;

        string displayName = hazmatSuit.name;
        if (equipped) displayName += " [EQUIPPED]";
        else if (owned) displayName += " [OWNED]";

        GUI.Label(new Rect(itemRect.x + 115, itemRect.y + 20, 300, 30), displayName, nameStyle);

        // Slot
        GUIStyle slotStyle = new GUIStyle();
        slotStyle.fontSize = 12;
        slotStyle.normal.textColor = neonMagenta;
        GUI.Label(new Rect(itemRect.x + 115, itemRect.y + 50, 200, 20), $"[{hazmatSuit.slot}]", slotStyle);

        // Description
        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 13;
        descStyle.wordWrap = true;
        descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.9f);
        GUI.Label(new Rect(itemRect.x + 115, itemRect.y + 75, itemRect.width - 130, 60), hazmatSuit.description, descStyle);

        // Price/Button
        if (!owned)
        {
            bool canAfford = playerGold >= hazmatSuit.price;

            GUIStyle priceStyle = new GUIStyle(GUI.skin.button);
            priceStyle.fontSize = 18;
            priceStyle.fontStyle = FontStyle.Bold;
            priceStyle.alignment = TextAnchor.MiddleCenter;
            priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);

            Rect buyButton = new Rect(itemRect.x + 20, itemRect.y + itemRect.height - 45, 150, 38);

            GUI.enabled = canAfford;
            if (GUI.Button(buyButton, $"BUY - {hazmatSuit.price}g", priceStyle))
            {
                TryPurchaseHazmat();
            }
            GUI.enabled = true;
        }
        else if (owned)
        {
            GUIStyle soldStyle = new GUIStyle();
            soldStyle.fontSize = 16;
            soldStyle.fontStyle = FontStyle.Bold;
            soldStyle.alignment = TextAnchor.MiddleCenter;
            soldStyle.normal.textColor = neonCyan;

            GUI.Label(new Rect(itemRect.x + 20, itemRect.y + itemRect.height - 40, 150, 35),
                "SOLD OUT", soldStyle);
        }

        // Instructions
        GUIStyle instrStyle = new GUIStyle();
        instrStyle.fontSize = 11;
        instrStyle.alignment = TextAnchor.MiddleCenter;
        instrStyle.normal.textColor = new Color(0.5f, 0.5f, 0.6f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + panelHeight - 35, panelRect.width, 25),
            "[X] Close | [ESC] Exit", instrStyle);
    }
}
