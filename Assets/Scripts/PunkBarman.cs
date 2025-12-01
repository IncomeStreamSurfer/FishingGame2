using UnityEngine;

/// <summary>
/// Punk bartender NPC in Void Realm who sells buff drinks
/// Uses OnGUI for shop interface (no Unity UI dependencies)
/// </summary>
public class PunkBarman : MonoBehaviour
{
    [Header("Shop Settings")]
    public float interactionRange = 3.5f;

    [Header("Drink Prices")]
    private const int NEON_SURGE_PRICE = 500;
    private const int VOID_TONIC_PRICE = 500;
    private const int TOXIC_COCKTAIL_PRICE = 1000;

    private GameObject player;
    private bool isPlayerNearby = false;
    private bool shopOpen = false;

    void Start()
    {
        CreatePunkBarmanModel();
        CreateBarCounter();
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Find player
        if (player == null)
            player = GameObject.Find("Player");

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        isPlayerNearby = distance <= interactionRange;

        // Open shop with F
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F) && !shopOpen)
        {
            shopOpen = true;
        }

        // Close shop with ESC or X
        if (shopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            shopOpen = false;
        }
    }

    void CreatePunkBarmanModel()
    {
        // Create main body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 1f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.75f, 0.5f);

        Material bodyMat = new Material(Shader.Find("Standard"));
        bodyMat.color = new Color(0.1f, 0.1f, 0.1f);
        body.GetComponent<Renderer>().material = bodyMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Create head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 2f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);

        Material headMat = new Material(Shader.Find("Standard"));
        headMat.color = new Color(0.9f, 0.8f, 0.7f);
        head.GetComponent<Renderer>().material = headMat;
        Object.Destroy(head.GetComponent<Collider>());

        // Create punk mohawk
        GameObject mohawk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mohawk.name = "Mohawk";
        mohawk.transform.SetParent(transform);
        mohawk.transform.localPosition = new Vector3(0, 2.5f, 0);
        mohawk.transform.localScale = new Vector3(0.15f, 0.5f, 0.3f);

        Material mohawkMat = new Material(Shader.Find("Standard"));
        mohawkMat.color = new Color(1f, 0f, 1f);
        mohawkMat.EnableKeyword("_EMISSION");
        mohawkMat.SetColor("_EmissionColor", new Color(1f, 0f, 1f) * 0.5f);
        mohawk.GetComponent<Renderer>().material = mohawkMat;
        Object.Destroy(mohawk.GetComponent<Collider>());

        // Create leather vest
        GameObject vest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vest.name = "Vest";
        vest.transform.SetParent(transform);
        vest.transform.localPosition = new Vector3(0, 1.2f, 0);
        vest.transform.localScale = new Vector3(0.55f, 0.6f, 0.3f);

        Material vestMat = new Material(Shader.Find("Standard"));
        vestMat.color = new Color(0.15f, 0.05f, 0.05f);
        vest.GetComponent<Renderer>().material = vestMat;
        Object.Destroy(vest.GetComponent<Collider>());

        // Create arms
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            arm.name = "Arm";
            arm.transform.SetParent(transform);
            arm.transform.localPosition = new Vector3(i * 0.35f, 1f, 0);
            arm.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);

            Material armMat = new Material(Shader.Find("Standard"));
            armMat.color = new Color(0.9f, 0.8f, 0.7f);
            arm.GetComponent<Renderer>().material = armMat;
            Object.Destroy(arm.GetComponent<Collider>());
        }

        // Add spiked collar
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        collar.name = "Collar";
        collar.transform.SetParent(transform);
        collar.transform.localPosition = new Vector3(0, 1.7f, 0);
        collar.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f);

        Material collarMat = new Material(Shader.Find("Standard"));
        collarMat.color = Color.black;
        collarMat.SetFloat("_Metallic", 0.8f);
        collar.GetComponent<Renderer>().material = collarMat;
        Object.Destroy(collar.GetComponent<Collider>());
    }

    void CreateBarCounter()
    {
        // Bar counter
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "BarCounter";
        bar.transform.SetParent(transform);
        bar.transform.localPosition = new Vector3(0, 0.5f, 0.8f);
        bar.transform.localScale = new Vector3(2.5f, 1f, 0.5f);

        Material barMat = new Material(Shader.Find("Standard"));
        barMat.color = new Color(0.2f, 0.05f, 0.15f);
        bar.GetComponent<Renderer>().material = barMat;

        // Bar top
        GameObject barTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barTop.name = "BarTop";
        barTop.transform.SetParent(transform);
        barTop.transform.localPosition = new Vector3(0, 1.05f, 0.8f);
        barTop.transform.localScale = new Vector3(2.7f, 0.1f, 0.6f);

        Material topMat = new Material(Shader.Find("Standard"));
        topMat.color = new Color(0.1f, 0.1f, 0.1f);
        topMat.SetFloat("_Metallic", 0.8f);
        topMat.SetFloat("_Glossiness", 0.9f);
        barTop.GetComponent<Renderer>().material = topMat;
        Object.Destroy(barTop.GetComponent<Collider>());

        // Neon lights under bar
        Material neonMat = new Material(Shader.Find("Standard"));
        neonMat.color = new Color(0.5f, 0f, 1f);
        neonMat.EnableKeyword("_EMISSION");
        neonMat.SetColor("_EmissionColor", new Color(0.5f, 0f, 1f) * 2f);

        for (int i = -1; i <= 1; i++)
        {
            GameObject light = GameObject.CreatePrimitive(PrimitiveType.Cube);
            light.name = "NeonLight";
            light.transform.SetParent(transform);
            light.transform.localPosition = new Vector3(i * 0.7f, 0.3f, 0.8f);
            light.transform.localScale = new Vector3(0.4f, 0.05f, 0.45f);
            light.GetComponent<Renderer>().material = neonMat;
            Object.Destroy(light.GetComponent<Collider>());
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Interaction prompt
        if (isPlayerNearby && !shopOpen)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 18;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(1f, 0f, 1f);

            GUI.Label(new Rect(0, Screen.height * 0.6f, Screen.width, 30), "[F] Void Bar", promptStyle);
        }

        // Shop UI
        if (shopOpen)
        {
            DrawShopUI();
        }
    }

    void DrawShopUI()
    {
        float panelW = 420f;
        float panelH = 380f;
        float panelX = (Screen.width - panelW) / 2f;
        float panelY = (Screen.height - panelH) / 2f;

        // Background
        GUI.color = new Color(0.08f, 0.02f, 0.12f, 0.95f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);

        // Border - magenta neon
        GUI.color = new Color(1f, 0f, 1f, 1f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX, panelY + panelH - 3, panelW, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX, panelY, 3, panelH), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelX + panelW - 3, panelY, 3, panelH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0f, 1f);
        GUI.Label(new Rect(panelX, panelY + 15, panelW, 30), "VOID BAR", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = 12;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.7f, 0.5f, 0.8f);
        GUI.Label(new Rect(panelX, panelY + 42, panelW, 20), "Punk's Special Drinks", subStyle);

        // Gold display
        int coins = 0;
        if (GameManager.Instance != null)
            coins = GameManager.Instance.GetCoins();

        GUIStyle goldStyle = new GUIStyle();
        goldStyle.fontSize = 16;
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.alignment = TextAnchor.MiddleRight;
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
        GUI.Label(new Rect(panelX + panelW - 130, panelY + 15, 115, 25), coins + " Gold", goldStyle);

        // X button
        GUIStyle xStyle = new GUIStyle();
        xStyle.fontSize = 18;
        xStyle.fontStyle = FontStyle.Bold;
        xStyle.alignment = TextAnchor.MiddleCenter;
        xStyle.normal.textColor = Color.white;

        GUI.color = new Color(0.8f, 0.2f, 0.2f);
        if (GUI.Button(new Rect(panelX + panelW - 35, panelY + 8, 25, 25), ""))
        {
            shopOpen = false;
        }
        GUI.color = Color.white;
        GUI.Label(new Rect(panelX + panelW - 35, panelY + 8, 25, 25), "X", xStyle);

        // Drink buttons
        float buttonY = panelY + 80;
        float buttonH = 75f;
        float buttonW = panelW - 40;
        float buttonX = panelX + 20;

        // Neon Surge
        bool xpActive = BuffManager.HasBuff(BuffType.XP) || BuffManager.HasBuff(BuffType.Both);
        DrawDrinkButton(buttonX, buttonY, buttonW, buttonH, "Neon Surge", "+25% XP for 20 min", NEON_SURGE_PRICE, coins, xpActive, BuffType.XP);

        // Void Tonic
        bool goldActive = BuffManager.HasBuff(BuffType.Gold) || BuffManager.HasBuff(BuffType.Both);
        DrawDrinkButton(buttonX, buttonY + buttonH + 10, buttonW, buttonH, "Void Tonic", "+25% Gold for 20 min", VOID_TONIC_PRICE, coins, goldActive, BuffType.Gold);

        // Toxic Cocktail
        bool bothActive = BuffManager.HasBuff(BuffType.Both);
        DrawDrinkButton(buttonX, buttonY + (buttonH + 10) * 2, buttonW, buttonH, "Toxic Cocktail", "+25% XP & Gold for 20 min", TOXIC_COCKTAIL_PRICE, coins, bothActive, BuffType.Both);

        // Instructions
        GUIStyle instrStyle = new GUIStyle();
        instrStyle.fontSize = 11;
        instrStyle.alignment = TextAnchor.MiddleCenter;
        instrStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(panelX, panelY + panelH - 35, panelW, 20), "Press ESC or X to close", instrStyle);
    }

    void DrawDrinkButton(float x, float y, float w, float h, string name, string desc, int price, int playerCoins, bool buffActive, BuffType buffType)
    {
        bool canAfford = playerCoins >= price;
        bool available = !buffActive;

        // Button background
        if (buffActive)
            GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        else if (canAfford)
            GUI.color = new Color(0.25f, 0.05f, 0.25f, 0.9f);
        else
            GUI.color = new Color(0.15f, 0.05f, 0.15f, 0.9f);

        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

        // Border
        Color borderColor = buffActive ? new Color(0.3f, 0.3f, 0.3f) : (canAfford ? new Color(0.8f, 0.3f, 0.9f) : new Color(0.4f, 0.2f, 0.4f));
        GUI.color = borderColor;
        GUI.DrawTexture(new Rect(x, y, w, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y + h - 2, w, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y, 2, h), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x + w - 2, y, 2, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Drink name
        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 16;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.normal.textColor = buffActive ? new Color(0.5f, 0.5f, 0.5f) : new Color(1f, 0.3f, 1f);
        GUI.Label(new Rect(x + 15, y + 10, w - 100, 22), name, nameStyle);

        // Description
        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 12;
        descStyle.normal.textColor = buffActive ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.8f, 0.8f, 0.8f);
        GUI.Label(new Rect(x + 15, y + 32, w - 30, 18), desc, descStyle);

        // Price or Status
        GUIStyle priceStyle = new GUIStyle();
        priceStyle.fontSize = 14;
        priceStyle.fontStyle = FontStyle.Bold;
        priceStyle.alignment = TextAnchor.MiddleRight;

        if (buffActive)
        {
            priceStyle.normal.textColor = new Color(0.3f, 0.8f, 0.3f);
            GUI.Label(new Rect(x, y + 10, w - 15, 22), "ACTIVE", priceStyle);
        }
        else
        {
            priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.3f) : new Color(0.8f, 0.3f, 0.3f);
            GUI.Label(new Rect(x, y + 10, w - 15, 22), price + "g", priceStyle);
        }

        // Buy button
        if (available && canAfford)
        {
            GUI.color = new Color(0.2f, 0.7f, 0.3f);
            if (GUI.Button(new Rect(x + w - 80, y + h - 32, 65, 22), ""))
            {
                PurchaseDrink(buffType, price, name);
            }
            GUI.color = Color.white;

            GUIStyle buyStyle = new GUIStyle();
            buyStyle.fontSize = 12;
            buyStyle.fontStyle = FontStyle.Bold;
            buyStyle.alignment = TextAnchor.MiddleCenter;
            buyStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x + w - 80, y + h - 32, 65, 22), "BUY", buyStyle);
        }
    }

    void PurchaseDrink(BuffType buffType, int cost, string drinkName)
    {
        if (GameManager.Instance == null) return;

        int coins = GameManager.Instance.GetCoins();
        if (coins < cost) return;

        // Deduct coins
        GameManager.Instance.AddCoins(-cost);

        // Apply buff (20 minutes = 1200 seconds)
        BuffManager.ApplyBuff(buffType, 1200f);

        // Show notification
        if (UIManager.Instance != null)
        {
            string message = "";
            switch (buffType)
            {
                case BuffType.XP:
                    message = "Neon Surge! +25% XP for 20 min";
                    break;
                case BuffType.Gold:
                    message = "Void Tonic! +25% Gold for 20 min";
                    break;
                case BuffType.Both:
                    message = "Toxic Cocktail! +25% XP & Gold for 20 min";
                    break;
            }
            UIManager.Instance.ShowLootNotification(message, new Color(1f, 0f, 1f));
        }
    }
}

// Buff Manager - Static class to track active buffs
public static class BuffManager
{
    private static float xpBuffEndTime = 0f;
    private static float goldBuffEndTime = 0f;
    private static float bothBuffEndTime = 0f;

    public static void ApplyBuff(BuffType buffType, float duration)
    {
        float endTime = Time.time + duration;

        switch (buffType)
        {
            case BuffType.XP:
                xpBuffEndTime = endTime;
                break;
            case BuffType.Gold:
                goldBuffEndTime = endTime;
                break;
            case BuffType.Both:
                bothBuffEndTime = endTime;
                break;
        }
    }

    public static bool HasBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.XP:
                return Time.time < xpBuffEndTime;
            case BuffType.Gold:
                return Time.time < goldBuffEndTime;
            case BuffType.Both:
                return Time.time < bothBuffEndTime;
            default:
                return false;
        }
    }

    public static float GetRemainingTime(BuffType buffType)
    {
        float endTime = 0f;

        switch (buffType)
        {
            case BuffType.XP:
                endTime = xpBuffEndTime;
                break;
            case BuffType.Gold:
                endTime = goldBuffEndTime;
                break;
            case BuffType.Both:
                endTime = bothBuffEndTime;
                break;
        }

        return Mathf.Max(0f, endTime - Time.time);
    }

    public static float GetXPMultiplier()
    {
        if (HasBuff(BuffType.Both) || HasBuff(BuffType.XP))
            return 1.25f;
        return 1f;
    }

    public static float GetGoldMultiplier()
    {
        if (HasBuff(BuffType.Both) || HasBuff(BuffType.Gold))
            return 1.25f;
        return 1f;
    }
}

public enum BuffType
{
    XP,
    Gold,
    Both
}
