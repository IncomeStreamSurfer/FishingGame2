using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PunkBarman : MonoBehaviour
{
    [Header("Shop Settings")]
    public float interactionRange = 3f;
    public KeyCode interactionKey = KeyCode.F;

    [Header("Drink Prices")]
    private const int NEON_SURGE_PRICE = 500;
    private const int VOID_TONIC_PRICE = 500;
    private const int TOXIC_COCKTAIL_PRICE = 1000;

    [Header("UI References")]
    public GameObject shopUI;
    public TextMeshProUGUI promptText;
    public Button neonSurgeButton;
    public Button voidTonicButton;
    public Button toxicCocktailButton;
    public Button closeButton;
    public TextMeshProUGUI neonSurgePriceText;
    public TextMeshProUGUI voidTonicPriceText;
    public TextMeshProUGUI toxicCocktailPriceText;
    public TextMeshProUGUI playerCoinsText;

    private GameObject player;
    private bool isPlayerNearby = false;
    private bool shopOpen = false;

    void Start()
    {
        CreatePunkBarmanModel();
        CreateBarCounter();
        CreateShopUI();

        player = GameObject.FindGameObjectWithTag("Player");

        if (shopUI != null)
            shopUI.SetActive(false);

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        isPlayerNearby = distance <= interactionRange;

        if (promptText != null)
            promptText.gameObject.SetActive(isPlayerNearby && !shopOpen);

        if (isPlayerNearby && Input.GetKeyDown(interactionKey) && !shopOpen)
        {
            OpenShop();
        }
        else if (shopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }

        if (shopOpen)
        {
            UpdateShopUI();
        }
    }

    void CreatePunkBarmanModel()
    {
        // Create main body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.parent = transform;
        body.transform.localPosition = new Vector3(0, 1f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.75f, 0.5f);

        Renderer bodyRenderer = body.GetComponent<Renderer>();
        bodyRenderer.material = new Material(Shader.Find("Standard"));
        bodyRenderer.material.color = new Color(0.1f, 0.1f, 0.1f); // Dark clothing

        // Create head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.parent = transform;
        head.transform.localPosition = new Vector3(0, 2f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.45f, 0.4f);

        Renderer headRenderer = head.GetComponent<Renderer>();
        headRenderer.material = new Material(Shader.Find("Standard"));
        headRenderer.material.color = new Color(0.9f, 0.8f, 0.7f); // Skin tone

        // Create punk mohawk
        GameObject mohawk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mohawk.name = "Mohawk";
        mohawk.transform.parent = transform;
        mohawk.transform.localPosition = new Vector3(0, 2.5f, 0);
        mohawk.transform.localScale = new Vector3(0.15f, 0.5f, 0.3f);

        Renderer mohawkRenderer = mohawk.GetComponent<Renderer>();
        mohawkRenderer.material = new Material(Shader.Find("Standard"));
        mohawkRenderer.material.color = new Color(1f, 0f, 1f); // Bright magenta
        mohawkRenderer.material.SetFloat("_Metallic", 0.3f);

        // Create leather vest
        GameObject vest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vest.name = "Vest";
        vest.transform.parent = transform;
        vest.transform.localPosition = new Vector3(0, 1.2f, 0);
        vest.transform.localScale = new Vector3(0.55f, 0.6f, 0.3f);

        Renderer vestRenderer = vest.GetComponent<Renderer>();
        vestRenderer.material = new Material(Shader.Find("Standard"));
        vestRenderer.material.color = new Color(0.15f, 0.05f, 0.05f); // Dark leather

        // Create arms
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            arm.name = "Arm_" + (i < 0 ? "L" : "R");
            arm.transform.parent = transform;
            arm.transform.localPosition = new Vector3(i * 0.35f, 1f, 0);
            arm.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);

            Renderer armRenderer = arm.GetComponent<Renderer>();
            armRenderer.material = new Material(Shader.Find("Standard"));
            armRenderer.material.color = new Color(0.9f, 0.8f, 0.7f);
        }

        // Add punk accessories (spiked collar)
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        collar.name = "Collar";
        collar.transform.parent = transform;
        collar.transform.localPosition = new Vector3(0, 1.7f, 0);
        collar.transform.localScale = new Vector3(0.3f, 0.05f, 0.3f);

        Renderer collarRenderer = collar.GetComponent<Renderer>();
        collarRenderer.material = new Material(Shader.Find("Standard"));
        collarRenderer.material.color = Color.black;
        collarRenderer.material.SetFloat("_Metallic", 0.8f);

        // Destroy unnecessary colliders (keep only one on parent)
        Destroy(body.GetComponent<Collider>());
        Destroy(head.GetComponent<Collider>());
        Destroy(mohawk.GetComponent<Collider>());
        Destroy(vest.GetComponent<Collider>());
        Destroy(collar.GetComponent<Collider>());

        // Add main collider to parent
        CapsuleCollider mainCollider = gameObject.AddComponent<CapsuleCollider>();
        mainCollider.height = 2.5f;
        mainCollider.radius = 0.5f;
        mainCollider.center = new Vector3(0, 1.25f, 0);
    }

    void CreateBarCounter()
    {
        // Create bar counter
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "BarCounter";
        bar.transform.parent = transform;
        bar.transform.localPosition = new Vector3(0, 0.5f, 0.6f);
        bar.transform.localScale = new Vector3(2f, 1f, 0.5f);

        Renderer barRenderer = bar.GetComponent<Renderer>();
        barRenderer.material = new Material(Shader.Find("Standard"));
        barRenderer.material.color = new Color(0.2f, 0.05f, 0.15f); // Dark purple wood
        barRenderer.material.SetFloat("_Metallic", 0.4f);

        // Create bar top
        GameObject barTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barTop.name = "BarTop";
        barTop.transform.parent = transform;
        barTop.transform.localPosition = new Vector3(0, 1.05f, 0.6f);
        barTop.transform.localScale = new Vector3(2.2f, 0.1f, 0.6f);

        Renderer barTopRenderer = barTop.GetComponent<Renderer>();
        barTopRenderer.material = new Material(Shader.Find("Standard"));
        barTopRenderer.material.color = new Color(0.1f, 0.1f, 0.1f);
        barTopRenderer.material.SetFloat("_Metallic", 0.8f);
        barTopRenderer.material.SetFloat("_Smoothness", 0.9f);

        // Add neon lights under bar
        for (int i = -1; i <= 1; i++)
        {
            GameObject light = GameObject.CreatePrimitive(PrimitiveType.Cube);
            light.name = "NeonLight_" + i;
            light.transform.parent = transform;
            light.transform.localPosition = new Vector3(i * 0.6f, 0.3f, 0.6f);
            light.transform.localScale = new Vector3(0.3f, 0.05f, 0.4f);

            Renderer lightRenderer = light.GetComponent<Renderer>();
            lightRenderer.material = new Material(Shader.Find("Standard"));
            lightRenderer.material.EnableKeyword("_EMISSION");
            lightRenderer.material.color = new Color(0.5f, 0f, 1f); // Purple neon
            lightRenderer.material.SetColor("_EmissionColor", new Color(1f, 0f, 2f));

            Destroy(light.GetComponent<Collider>());
        }
    }

    void CreateShopUI()
    {
        // Create shop UI canvas
        GameObject canvasObj = new GameObject("PunkBarShopCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        shopUI = canvasObj;

        // Create background panel
        GameObject panelObj = new GameObject("ShopPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panel = panelObj.AddComponent<Image>();
        panel.color = new Color(0.1f, 0, 0.15f, 0.95f); // Dark purple background

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(600, 500);

        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "VOID BAR - PUNK'S SPECIAL DRINKS";
        title.fontSize = 28;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(1f, 0f, 1f); // Magenta
        title.fontStyle = FontStyles.Bold;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(580, 50);
        titleRect.anchoredPosition = new Vector2(0, 200);

        // Create player coins display
        GameObject coinsObj = new GameObject("PlayerCoins");
        coinsObj.transform.SetParent(panelObj.transform, false);
        playerCoinsText = coinsObj.AddComponent<TextMeshProUGUI>();
        playerCoinsText.text = "Your Gold: 0";
        playerCoinsText.fontSize = 20;
        playerCoinsText.alignment = TextAlignmentOptions.Center;
        playerCoinsText.color = Color.yellow;

        RectTransform coinsRect = coinsObj.GetComponent<RectTransform>();
        coinsRect.sizeDelta = new Vector2(400, 40);
        coinsRect.anchoredPosition = new Vector2(0, 150);

        // Create drink buttons
        CreateDrinkButton("Neon Surge", "XP +25% for 20 min", NEON_SURGE_PRICE, new Vector2(0, 70),
            out neonSurgeButton, out neonSurgePriceText, () => PurchaseDrink(BuffType.XP, NEON_SURGE_PRICE, "Neon Surge"));

        CreateDrinkButton("Void Tonic", "Gold +25% for 20 min", VOID_TONIC_PRICE, new Vector2(0, -10),
            out voidTonicButton, out voidTonicPriceText, () => PurchaseDrink(BuffType.Gold, VOID_TONIC_PRICE, "Void Tonic"));

        CreateDrinkButton("Toxic Cocktail", "XP & Gold +25% for 20 min", TOXIC_COCKTAIL_PRICE, new Vector2(0, -90),
            out toxicCocktailButton, out toxicCocktailPriceText, () => PurchaseDrink(BuffType.Both, TOXIC_COCKTAIL_PRICE, "Toxic Cocktail"));

        // Create close button
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(panelObj.transform, false);
        closeButton = closeObj.AddComponent<Button>();
        Image closeImg = closeObj.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0f, 0f);

        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.sizeDelta = new Vector2(200, 50);
        closeRect.anchoredPosition = new Vector2(0, -180);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "CLOSE [ESC]";
        closeText.fontSize = 20;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
        closeText.fontStyle = FontStyles.Bold;

        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.sizeDelta = new Vector2(200, 50);

        closeButton.onClick.AddListener(CloseShop);

        // Create prompt text (world space)
        GameObject promptObj = new GameObject("InteractPrompt");
        promptObj.transform.SetParent(transform);
        promptObj.transform.localPosition = new Vector3(0, 3f, 0);

        Canvas promptCanvas = promptObj.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.WorldSpace;
        promptObj.AddComponent<CanvasScaler>();

        RectTransform promptCanvasRect = promptObj.GetComponent<RectTransform>();
        promptCanvasRect.sizeDelta = new Vector2(2, 0.5f);
        promptCanvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        GameObject promptTextObj = new GameObject("Text");
        promptTextObj.transform.SetParent(promptObj.transform, false);
        promptText = promptTextObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "[F] TALK TO BARTENDER";
        promptText.fontSize = 48;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = new Color(1f, 0f, 1f); // Magenta
        promptText.fontStyle = FontStyles.Bold;

        RectTransform promptTextRect = promptTextObj.GetComponent<RectTransform>();
        promptTextRect.sizeDelta = new Vector2(200, 50);
    }

    void CreateDrinkButton(string drinkName, string description, int price, Vector2 position,
        out Button button, out TextMeshProUGUI priceText, System.Action onClick)
    {
        GameObject buttonObj = new GameObject(drinkName + "Button");
        buttonObj.transform.SetParent(shopUI.transform.GetChild(0), false);
        button = buttonObj.AddComponent<Button>();
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = new Color(0.2f, 0f, 0.2f); // Dark purple

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(500, 70);
        buttonRect.anchoredPosition = position;

        // Drink name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = drinkName;
        nameText.fontSize = 22;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = new Color(1f, 0f, 1f); // Magenta
        nameText.fontStyle = FontStyles.Bold;

        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(300, 30);
        nameRect.anchoredPosition = new Vector2(-80, 10);

        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = description;
        descText.fontSize = 16;
        descText.alignment = TextAlignmentOptions.Left;
        descText.color = new Color(0.8f, 0.8f, 0.8f);

        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.sizeDelta = new Vector2(300, 25);
        descRect.anchoredPosition = new Vector2(-80, -15);

        // Price
        GameObject priceObj = new GameObject("Price");
        priceObj.transform.SetParent(buttonObj.transform, false);
        priceText = priceObj.AddComponent<TextMeshProUGUI>();
        priceText.text = price + "g";
        priceText.fontSize = 24;
        priceText.alignment = TextAlignmentOptions.Right;
        priceText.color = Color.yellow;
        priceText.fontStyle = FontStyles.Bold;

        RectTransform priceRect = priceObj.GetComponent<RectTransform>();
        priceRect.sizeDelta = new Vector2(100, 40);
        priceRect.anchoredPosition = new Vector2(180, 0);

        button.onClick.AddListener(() => onClick());

        // Add hover effect
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0f, 0.2f);
        colors.highlightedColor = new Color(0.4f, 0f, 0.4f);
        colors.pressedColor = new Color(0.6f, 0f, 0.6f);
        button.colors = colors;
    }

    void OpenShop()
    {
        shopOpen = true;
        if (shopUI != null)
            shopUI.SetActive(true);

        if (player != null)
        {
            var playerController = player.GetComponent<FirstPersonController>();
            if (playerController != null)
                playerController.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateShopUI();
    }

    void CloseShop()
    {
        shopOpen = false;
        if (shopUI != null)
            shopUI.SetActive(false);

        if (player != null)
        {
            var playerController = player.GetComponent<FirstPersonController>();
            if (playerController != null)
                playerController.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdateShopUI()
    {
        if (GameManager.Instance != null)
        {
            int coins = GameManager.Instance.GetCoins();
            if (playerCoinsText != null)
                playerCoinsText.text = "Your Gold: " + coins;

            // Update button interactability based on coins and active buffs
            if (neonSurgeButton != null)
                neonSurgeButton.interactable = coins >= NEON_SURGE_PRICE && !BuffManager.HasBuff(BuffType.XP) && !BuffManager.HasBuff(BuffType.Both);

            if (voidTonicButton != null)
                voidTonicButton.interactable = coins >= VOID_TONIC_PRICE && !BuffManager.HasBuff(BuffType.Gold) && !BuffManager.HasBuff(BuffType.Both);

            if (toxicCocktailButton != null)
                toxicCocktailButton.interactable = coins >= TOXIC_COCKTAIL_PRICE && !BuffManager.HasBuff(BuffType.Both);
        }
    }

    void PurchaseDrink(BuffType buffType, int cost, string drinkName)
    {
        if (GameManager.Instance == null) return;

        int coins = GameManager.Instance.GetCoins();
        if (coins < cost)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowLootNotification("Not enough gold!", Color.red);
            return;
        }

        // Check if buff is already active
        if (buffType == BuffType.XP && (BuffManager.HasBuff(BuffType.XP) || BuffManager.HasBuff(BuffType.Both)))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowLootNotification("XP buff already active!", new Color(1f, 0.5f, 0f));
            return;
        }

        if (buffType == BuffType.Gold && (BuffManager.HasBuff(BuffType.Gold) || BuffManager.HasBuff(BuffType.Both)))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowLootNotification("Gold buff already active!", new Color(1f, 0.5f, 0f));
            return;
        }

        if (buffType == BuffType.Both && BuffManager.HasBuff(BuffType.Both))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowLootNotification("Toxic Cocktail already active!", new Color(1f, 0.5f, 0f));
            return;
        }

        // Deduct coins
        GameManager.Instance.AddCoins(-cost);

        // Apply buff
        BuffManager.ApplyBuff(buffType, 1200f); // 20 minutes = 1200 seconds

        // Show notification
        if (UIManager.Instance != null)
        {
            string message = "";
            Color color = new Color(1f, 0f, 1f); // Magenta

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

            UIManager.Instance.ShowLootNotification(message, color);
        }

        UpdateShopUI();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, interactionRange);
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
