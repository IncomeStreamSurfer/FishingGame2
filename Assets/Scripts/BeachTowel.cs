using UnityEngine;

/// <summary>
/// Beach Towel - Rest spot where player can sleep to regain health
/// Press E to interact, hold CTRL to sleep and regenerate health
/// </summary>
public class BeachTowel : MonoBehaviour
{
    public static BeachTowel Instance { get; private set; }

    // Interaction
    private bool playerNearby = false;
    private bool showingPopup = false;
    private bool isSleeping = false;
    private float interactionDistance = 3f;

    // Health regen
    private float regenRate = 5f; // HP per second while sleeping

    // Cached references
    private Transform playerTransform;

    // GUI
    private Texture2D popupBgTex;
    private Texture2D towelColorTex;
    private GUIStyle labelStyle;
    private bool guiInitialized = false;

    // Static colors
    private static readonly Color towelColor = new Color(0.9f, 0.3f, 0.4f); // Red/pink towel
    private static readonly Color stripeColor = new Color(1f, 1f, 1f); // White stripes
    private static readonly Color promptColor = new Color(1f, 0.9f, 0.6f);
    private static readonly Color textColor = new Color(0.9f, 0.9f, 0.85f);
    private static readonly Color sleepingColor = new Color(0.5f, 0.8f, 1f);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        CreateTowelVisuals();
    }

    void CreateTowelVisuals()
    {
        // Main towel body - flat rectangle
        GameObject towelBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        towelBase.name = "TowelBase";
        towelBase.transform.SetParent(transform);
        towelBase.transform.localPosition = new Vector3(0, 0.02f, 0);
        towelBase.transform.localScale = new Vector3(1.2f, 0.04f, 2f);
        Destroy(towelBase.GetComponent<Collider>());

        Material towelMat = new Material(Shader.Find("Standard"));
        towelMat.color = towelColor;
        towelMat.SetFloat("_Glossiness", 0.1f);
        towelBase.GetComponent<Renderer>().material = towelMat;

        // White stripes on towel
        for (int i = 0; i < 3; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "Stripe_" + i;
            stripe.transform.SetParent(transform);
            stripe.transform.localPosition = new Vector3(0, 0.045f, -0.6f + i * 0.6f);
            stripe.transform.localScale = new Vector3(1.1f, 0.02f, 0.15f);
            Destroy(stripe.GetComponent<Collider>());

            Material stripeMat = new Material(Shader.Find("Standard"));
            stripeMat.color = stripeColor;
            stripeMat.SetFloat("_Glossiness", 0.1f);
            stripe.GetComponent<Renderer>().material = stripeMat;
        }

        // Small pillow at one end
        GameObject pillow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillow.name = "Pillow";
        pillow.transform.SetParent(transform);
        pillow.transform.localPosition = new Vector3(0, 0.1f, 0.85f);
        pillow.transform.localScale = new Vector3(0.6f, 0.12f, 0.3f);
        Destroy(pillow.GetComponent<Collider>());

        Material pillowMat = new Material(Shader.Find("Standard"));
        pillowMat.color = new Color(1f, 0.95f, 0.8f); // Cream colored pillow
        pillowMat.SetFloat("_Glossiness", 0.2f);
        pillow.GetComponent<Renderer>().material = pillowMat;

        // Folded corner effect
        GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corner.name = "FoldedCorner";
        corner.transform.SetParent(transform);
        corner.transform.localPosition = new Vector3(-0.5f, 0.05f, -0.9f);
        corner.transform.localScale = new Vector3(0.2f, 0.03f, 0.2f);
        corner.transform.localRotation = Quaternion.Euler(0, 15f, 10f);
        Destroy(corner.GetComponent<Collider>());
        corner.GetComponent<Renderer>().material = towelMat;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Get player reference
        if (playerTransform == null && GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        if (playerTransform == null) return;

        // Check distance
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        playerNearby = dist <= interactionDistance;

        // Close popup if player walks away
        if (!playerNearby)
        {
            showingPopup = false;
            isSleeping = false;
        }

        // E to toggle popup
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            showingPopup = !showingPopup;
            if (!showingPopup) isSleeping = false;
        }

        // ESC to close popup
        if (showingPopup && Input.GetKeyDown(KeyCode.Escape))
        {
            showingPopup = false;
            isSleeping = false;
        }

        // CTRL to sleep (only when popup is showing)
        if (showingPopup && playerNearby)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                isSleeping = true;
                RegenerateHealth();
            }
            else
            {
                isSleeping = false;
            }
        }
    }

    void RegenerateHealth()
    {
        if (PlayerHealth.Instance == null) return;

        float currentHealth = PlayerHealth.Instance.GetCurrentHealth();
        float maxHealth = PlayerHealth.Instance.GetMaxHealth();

        if (currentHealth < maxHealth)
        {
            PlayerHealth.Instance.Heal(regenRate * Time.deltaTime);
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (!playerNearby) return;

        // Initialize GUI once
        if (!guiInitialized)
        {
            popupBgTex = new Texture2D(1, 1);
            popupBgTex.SetPixel(0, 0, new Color(0.08f, 0.06f, 0.12f, 0.95f));
            popupBgTex.Apply();

            towelColorTex = new Texture2D(1, 1);
            towelColorTex.SetPixel(0, 0, towelColor);
            towelColorTex.Apply();

            labelStyle = new GUIStyle(GUI.skin.label);
            guiInitialized = true;
        }

        // Show interaction prompt when not showing popup
        if (!showingPopup)
        {
            DrawInteractionPrompt();
        }
        else
        {
            DrawPopupWindow();
        }
    }

    void DrawInteractionPrompt()
    {
        labelStyle.fontSize = 16;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = promptColor;

        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 120, 200, 30), "Beach Towel", labelStyle);

        labelStyle.fontSize = 14;
        labelStyle.fontStyle = FontStyle.Normal;
        labelStyle.normal.textColor = Color.gray;
        GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 95, 200, 25), "[E] Rest", labelStyle);
    }

    void DrawPopupWindow()
    {
        float w = 360;
        float h = 140;
        float x = (Screen.width - w) / 2;
        float y = (Screen.height - h) / 2;

        // Background
        GUI.DrawTexture(new Rect(x, y, w, h), popupBgTex);

        // Border
        GUI.color = towelColor;
        GUI.DrawTexture(new Rect(x, y, w, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y + h - 3, w, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y, 3, h), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x + w - 3, y, 3, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        labelStyle.fontSize = 20;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = promptColor;
        GUI.Label(new Rect(x, y + 15, w, 30), "Beach Towel", labelStyle);

        // Message
        labelStyle.fontSize = 14;
        labelStyle.fontStyle = FontStyle.Normal;
        labelStyle.wordWrap = true;
        labelStyle.normal.textColor = textColor;
        GUI.Label(new Rect(x + 20, y + 50, w - 40, 40),
            "Feeling low? Press CTRL to sleep and regain that strength!", labelStyle);

        // Status
        labelStyle.fontSize = 13;
        labelStyle.fontStyle = FontStyle.Bold;

        if (isSleeping)
        {
            // Sleeping status with pulsing effect
            float pulse = 0.7f + Mathf.Sin(Time.time * 3f) * 0.3f;
            labelStyle.normal.textColor = new Color(sleepingColor.r, sleepingColor.g, sleepingColor.b, pulse);

            float currentHealth = PlayerHealth.Instance != null ? PlayerHealth.Instance.GetCurrentHealth() : 0;
            float maxHealth = PlayerHealth.Instance != null ? PlayerHealth.Instance.GetMaxHealth() : 100;

            if (currentHealth >= maxHealth)
            {
                labelStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);
                GUI.Label(new Rect(x, y + h - 35, w, 25), "Fully Rested!", labelStyle);
            }
            else
            {
                GUI.Label(new Rect(x, y + h - 35, w, 25),
                    $"Zzz... Sleeping... HP: {Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}", labelStyle);
            }
        }
        else
        {
            labelStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(x, y + h - 35, w, 25), "[Hold CTRL to sleep]", labelStyle);
        }

        // Close hint
        labelStyle.fontSize = 10;
        labelStyle.fontStyle = FontStyle.Normal;
        labelStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        labelStyle.alignment = TextAnchor.LowerRight;
        GUI.Label(new Rect(x, y + h - 20, w - 10, 15), "[ESC] Close", labelStyle);
    }

    void OnDestroy()
    {
        if (popupBgTex != null) Destroy(popupBgTex);
        if (towelColorTex != null) Destroy(towelColorTex);
        if (Instance == this) Instance = null;
    }
}
