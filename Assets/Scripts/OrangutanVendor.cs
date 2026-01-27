using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Orangutan weapon vendor in the Jungle Realm
/// Sells the Snake Charm ring which grants immunity to snake damage
/// </summary>
public class OrangutanVendor : MonoBehaviour
{
    public static OrangutanVendor Instance { get; private set; }

    private bool shopOpen = false;
    private bool playerNearby = false;
    private float interactionDistance = 4f;

    // Performance: Frame skip for OnGUI
    private int guiFrameSkip = 0;

    // Shop item - Snake Charm
    private AccessoryItem snakeCharm;

    // UI Textures
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D snakeCharmIcon;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // PERFORMANCE: Skip if performance mode enabled
        if (PerformanceMode.ShouldSkip(this)) return;

        CreateOrangutanModel();
        InitializeShop();
        CreateUITextures();
        CreateSnakeCharmIcon();
    }

    void CreateOrangutanModel()
    {
        // Orange/brown materials
        Material orangeFur = new Material(Shader.Find("Standard"));
        orangeFur.color = new Color(0.85f, 0.5f, 0.25f);

        Material darkFur = new Material(Shader.Find("Standard"));
        darkFur.color = new Color(0.4f, 0.25f, 0.15f);

        Material face = new Material(Shader.Find("Standard"));
        face.color = new Color(0.3f, 0.2f, 0.15f);

        // Body (round)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 0.6f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.6f, 0.4f);
        body.GetComponent<Renderer>().material = orangeFur;
        Object.Destroy(body.GetComponent<Collider>());

        // Head (round)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 1.1f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);
        head.GetComponent<Renderer>().material = orangeFur;
        Object.Destroy(head.GetComponent<Collider>());

        // Face (dark)
        GameObject faceObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        faceObj.name = "Face";
        faceObj.transform.SetParent(head.transform);
        faceObj.transform.localPosition = new Vector3(0, 0, 0.4f);
        faceObj.transform.localScale = new Vector3(0.7f, 0.8f, 0.5f);
        faceObj.GetComponent<Renderer>().material = face;
        Object.Destroy(faceObj.GetComponent<Collider>());

        // Eyes (yellow)
        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = new Color(0.9f, 0.8f, 0.4f);

        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(faceObj.transform);
            eye.transform.localPosition = new Vector3(side * 0.25f, 0.15f, 0.8f);
            eye.transform.localScale = Vector3.one * 0.3f;
            eye.GetComponent<Renderer>().material = eyeMat;
            Object.Destroy(eye.GetComponent<Collider>());

            // Pupil
            GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.name = "Pupil";
            pupil.transform.SetParent(eye.transform);
            pupil.transform.localPosition = new Vector3(0, 0, 0.5f);
            pupil.transform.localScale = Vector3.one * 0.5f;
            Material pupilMat = new Material(Shader.Find("Standard"));
            pupilMat.color = Color.black;
            pupil.GetComponent<Renderer>().material = pupilMat;
            Object.Destroy(pupil.GetComponent<Collider>());
        }

        // Ears (small)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ear.name = "Ear";
            ear.transform.SetParent(head.transform);
            ear.transform.localPosition = new Vector3(side * 0.45f, 0.3f, 0);
            ear.transform.localScale = new Vector3(0.15f, 0.2f, 0.1f);
            ear.GetComponent<Renderer>().material = darkFur;
            Object.Destroy(ear.GetComponent<Collider>());
        }

        // Mouth area
        GameObject mouth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mouth.name = "Mouth";
        mouth.transform.SetParent(faceObj.transform);
        mouth.transform.localPosition = new Vector3(0, -0.3f, 0.7f);
        mouth.transform.localScale = new Vector3(0.5f, 0.4f, 0.4f);
        Material mouthMat = new Material(Shader.Find("Standard"));
        mouthMat.color = new Color(0.25f, 0.15f, 0.1f);
        mouth.GetComponent<Renderer>().material = mouthMat;
        Object.Destroy(mouth.GetComponent<Collider>());

        // Long arms (dangling)
        for (int side = -1; side <= 1; side += 2)
        {
            // Upper arm
            GameObject upperArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperArm.name = "UpperArm";
            upperArm.transform.SetParent(transform);
            upperArm.transform.localPosition = new Vector3(side * 0.3f, 0.7f, 0);
            upperArm.transform.localRotation = Quaternion.Euler(0, 0, side * 20f);
            upperArm.transform.localScale = new Vector3(0.12f, 0.35f, 0.12f);
            upperArm.GetComponent<Renderer>().material = orangeFur;
            Object.Destroy(upperArm.GetComponent<Collider>());

            // Lower arm (long)
            GameObject lowerArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lowerArm.name = "LowerArm";
            lowerArm.transform.SetParent(transform);
            lowerArm.transform.localPosition = new Vector3(side * 0.35f, 0.15f, 0);
            lowerArm.transform.localRotation = Quaternion.Euler(0, 0, side * 10f);
            lowerArm.transform.localScale = new Vector3(0.1f, 0.45f, 0.1f);
            lowerArm.GetComponent<Renderer>().material = orangeFur;
            Object.Destroy(lowerArm.GetComponent<Collider>());

            // Hand (dark)
            GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hand.name = "Hand";
            hand.transform.SetParent(lowerArm.transform);
            hand.transform.localPosition = new Vector3(0, -1.1f, 0);
            hand.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
            hand.GetComponent<Renderer>().material = darkFur;
            Object.Destroy(hand.GetComponent<Collider>());
        }

        // Short legs
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = "Leg";
            leg.transform.SetParent(transform);
            leg.transform.localPosition = new Vector3(side * 0.15f, 0.25f, 0);
            leg.transform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
            leg.GetComponent<Renderer>().material = darkFur;
            Object.Destroy(leg.GetComponent<Collider>());

            // Foot
            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foot.name = "Foot";
            foot.transform.SetParent(leg.transform);
            foot.transform.localPosition = new Vector3(0, -1.1f, 0.3f);
            foot.transform.localScale = new Vector3(1.3f, 0.8f, 1.5f);
            foot.GetComponent<Renderer>().material = darkFur;
            Object.Destroy(foot.GetComponent<Collider>());
        }
    }

    void InitializeShop()
    {
        snakeCharm = new AccessoryItem
        {
            name = "Snake Charm",
            slot = "Ring",
            price = 750,
            description = "Ancient charm that wards off serpents. Grants immunity to snake damage.",
            effect = AccessoryEffect.SnakeImmunity
        };
    }

    void CreateUITextures()
    {
        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0.25f, 0.2f, 0.15f, 0.95f));
        panelTexture.Apply();

        buttonTexture = new Texture2D(1, 1);
        buttonTexture.SetPixel(0, 0, new Color(0.4f, 0.3f, 0.2f, 0.9f));
        buttonTexture.Apply();

        buttonHoverTexture = new Texture2D(1, 1);
        buttonHoverTexture.SetPixel(0, 0, new Color(0.55f, 0.4f, 0.25f, 0.95f));
        buttonHoverTexture.Apply();
    }

    void CreateSnakeCharmIcon()
    {
        snakeCharmIcon = new Texture2D(24, 24);
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < 24; x++)
            for (int y = 0; y < 24; y++)
                snakeCharmIcon.SetPixel(x, y, clear);

        // Ring circle (golden)
        Color gold = new Color(1f, 0.85f, 0.3f);
        Color goldShine = new Color(1f, 0.95f, 0.6f);
        Color emerald = new Color(0.2f, 0.8f, 0.3f);
        Color emeraldShine = new Color(0.4f, 1f, 0.5f);

        // Draw ring circle
        DrawCircle(snakeCharmIcon, 12, 12, 8, gold);
        DrawCircle(snakeCharmIcon, 12, 12, 6, clear); // Inner hollow

        // Shine on ring
        snakeCharmIcon.SetPixel(9, 16, goldShine);
        snakeCharmIcon.SetPixel(10, 17, goldShine);

        // Emerald gem (snake eye)
        FillCircle(snakeCharmIcon, 12, 12, 4, emerald);
        snakeCharmIcon.SetPixel(13, 13, emeraldShine);
        snakeCharmIcon.SetPixel(14, 14, emeraldShine);

        // Snake symbol (simplified)
        snakeCharmIcon.SetPixel(11, 12, Color.black);
        snakeCharmIcon.SetPixel(13, 12, Color.black);

        snakeCharmIcon.Apply();
        snakeCharmIcon.filterMode = FilterMode.Point;
    }

    void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color col)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius && x * x + y * y > (radius - 2) * (radius - 2))
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < 24 && py >= 0 && py < 24)
                    {
                        tex.SetPixel(px, py, col);
                    }
                }
            }
        }
    }

    void FillCircle(Texture2D tex, int cx, int cy, int radius, Color col)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < 24 && py >= 0 && py < 24)
                    {
                        tex.SetPixel(px, py, col);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Use cached player reference
        if (!GameCache.IsPlayerValid()) return;

        float distance = Vector3.Distance(transform.position, GameCache.Player.position);
        playerNearby = distance < interactionDistance;

        if (playerNearby && !shopOpen && Input.GetKeyDown(KeyCode.E))
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

        // Simple idle animation - sway back and forth
        if (!shopOpen)
        {
            float sway = Mathf.Sin(Time.time * 1.5f) * 5f;
            transform.rotation = Quaternion.Euler(0, sway, 0);
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

    void TryPurchaseSnakeCharm()
    {
        // Check if already owned
        if (AccessorySystem.Instance != null && AccessorySystem.Instance.HasAccessory("Snake Charm"))
        {
            // Already owned, just equip it
            AccessorySystem.Instance.EquipAccessory(snakeCharm);
            return;
        }

        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;

        if (playerGold >= snakeCharm.price)
        {
            GameManager.Instance.AddCoins(-snakeCharm.price);

            if (AccessorySystem.Instance != null)
            {
                AccessorySystem.Instance.AddAccessory(snakeCharm);
                AccessorySystem.Instance.EquipAccessory(snakeCharm);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"Purchased {snakeCharm.name}!", new Color(0.3f, 0.8f, 0.3f));
            }

            Debug.Log($"Purchased {snakeCharm.name}!");
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

        // Performance: Skip frames when not actively interacting
        if (!playerNearby && !shopOpen)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return; // Skip 2 out of 3 frames
        }

        if (playerNearby && !shopOpen)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 18;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.9f, 0.7f, 0.4f);

            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 150, 300, 30),
                "[E] Talk to Orangutan", promptStyle);
            return; // Early return - don't process shop UI
        }

        if (shopOpen)
        {
            DrawShopUI();
        }
    }

    void DrawShopUI()
    {
        float panelWidth = 450;
        float panelHeight = 320;
        Rect panelRect = new Rect(
            Screen.width / 2 - panelWidth / 2,
            Screen.height / 2 - panelHeight / 2,
            panelWidth,
            panelHeight
        );

        GUI.DrawTexture(panelRect, panelTexture);

        // Border
        GUI.color = new Color(0.85f, 0.6f, 0.3f);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y - 2, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y + panelRect.height, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x + panelRect.width, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.8f, 0.4f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + 15, panelRect.width, 30), "ORANGUTAN VENDOR", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = 12;
        subStyle.fontStyle = FontStyle.Italic;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.8f, 0.7f, 0.5f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + 45, panelRect.width, 20),
            "*Grunts and gestures to a mysterious ring*", subStyle);

        // Gold display
        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUIStyle goldStyle = new GUIStyle();
        goldStyle.fontSize = 14;
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.alignment = TextAnchor.MiddleRight;
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        GUI.Label(new Rect(panelRect.x + panelRect.width - 150, panelRect.y + 15, 140, 25),
            $"Gold: {playerGold:N0}", goldStyle);

        // Snake Charm item display
        float itemY = panelRect.y + 90;
        Rect itemRect = new Rect(panelRect.x + 20, itemY, panelRect.width - 40, 130);

        bool owned = AccessorySystem.Instance != null && AccessorySystem.Instance.HasAccessory(snakeCharm.name);
        bool equipped = AccessorySystem.Instance != null && AccessorySystem.Instance.IsEquipped(snakeCharm.name);

        GUI.DrawTexture(itemRect, equipped ? buttonHoverTexture : buttonTexture);

        // Icon
        if (snakeCharmIcon != null)
        {
            GUI.DrawTexture(new Rect(itemRect.x + 15, itemRect.y + 15, 64, 64), snakeCharmIcon);
        }

        // Name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 18;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleLeft;
        nameStyle.normal.textColor = equipped ? new Color(0.3f, 1f, 0.3f) : Color.white;

        string displayName = owned ? (equipped ? $"{snakeCharm.name} [EQUIPPED]" : $"{snakeCharm.name} [OWNED]") : snakeCharm.name;
        GUI.Label(new Rect(itemRect.x + 95, itemRect.y + 15, 280, 25), displayName, nameStyle);

        // Slot
        GUIStyle slotStyle = new GUIStyle();
        slotStyle.fontSize = 12;
        slotStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(itemRect.x + 95, itemRect.y + 40, 200, 20), $"[{snakeCharm.slot}]", slotStyle);

        // Description
        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 13;
        descStyle.wordWrap = true;
        descStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
        GUI.Label(new Rect(itemRect.x + 95, itemRect.y + 60, itemRect.width - 110, 55), snakeCharm.description, descStyle);

        // Price/Button
        if (!owned)
        {
            bool canAfford = playerGold >= snakeCharm.price;

            GUIStyle priceStyle = new GUIStyle();
            priceStyle.fontSize = 16;
            priceStyle.fontStyle = FontStyle.Bold;
            priceStyle.alignment = TextAnchor.MiddleCenter;
            priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);

            Rect buyButton = new Rect(itemRect.x + itemRect.width - 110, itemRect.y + itemRect.height - 40, 100, 35);

            GUI.enabled = canAfford;
            if (GUI.Button(buyButton, ""))
            {
                TryPurchaseSnakeCharm();
            }
            GUI.enabled = true;

            GUI.Label(buyButton, $"{snakeCharm.price}g", priceStyle);
        }
        else if (!equipped)
        {
            GUIStyle equipStyle = new GUIStyle(GUI.skin.button);
            equipStyle.fontSize = 14;
            equipStyle.fontStyle = FontStyle.Bold;

            Rect equipButton = new Rect(itemRect.x + itemRect.width - 110, itemRect.y + itemRect.height - 40, 100, 35);

            if (GUI.Button(equipButton, "Equip", equipStyle))
            {
                if (AccessorySystem.Instance != null)
                {
                    AccessorySystem.Instance.EquipAccessory(snakeCharm);
                }
            }
        }

        // Click anywhere on item to buy/equip
        if (GUI.Button(itemRect, "", GUIStyle.none))
        {
            if (!owned)
            {
                TryPurchaseSnakeCharm();
            }
            else if (!equipped && AccessorySystem.Instance != null)
            {
                AccessorySystem.Instance.EquipAccessory(snakeCharm);
            }
        }

        // Instructions
        GUIStyle instrStyle = new GUIStyle();
        instrStyle.fontSize = 11;
        instrStyle.alignment = TextAnchor.MiddleCenter;
        instrStyle.normal.textColor = new Color(0.5f, 0.45f, 0.4f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + panelHeight - 30, panelRect.width, 25),
            "[Click] Buy/Equip | [ESC] Close", instrStyle);
    }
}
