using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Rules Popup - Shows game instructions when player starts a new game
/// Styled to match FishInventoryPanel and CharacterPanel
/// Auto-creates itself on scene load
/// </summary>
public class RulesPopup : MonoBehaviour
{
    public static RulesPopup Instance { get; private set; }

    private bool isOpen = false;
    private bool hasShownOnce = false;
    private bool initialized = false;

    // Cached textures (same style as CharacterPanel/FishInventoryPanel)
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    // Panel dimensions
    private float panelWidth = 220f;
    private float panelHeight = 195f;

    // Cached GUIStyles for performance (created once, reused every frame)
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedXStyle;
    private static GUIStyle cachedWarningStyle;
    private static GUIStyle cachedTipHeaderStyle;
    private static GUIStyle cachedTipStyle;
    private static GUIStyle cachedControlStyle;
    private static GUIStyle cachedKeyStyle;
    private static GUIStyle cachedFinalStyle;
    private static GUIStyle cachedHintStyle;
    private static bool stylesInitialized = false;

    // Auto-create on scene load
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("RulesPopup");
            go.AddComponent<RulesPopup>();
            DontDestroyOnLoad(go);
            Debug.Log("[RulesPopup] Auto-created and registered");
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Invoke("Initialize", 0.3f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        initialized = true;
    }

    void CreateCachedTextures()
    {
        // Match CharacterPanel/FishInventoryPanel style exactly
        CacheTexture("panelBg", new Color(0.1f, 0.1f, 0.12f, 0.95f));
        CacheTexture("panelBorder", new Color(1f, 0.85f, 0.4f, 1f)); // Gold border
        CacheTexture("closeBtn", new Color(0.8f, 0.2f, 0.2f, 1f));
        CacheTexture("divider", new Color(1f, 0.85f, 0.4f, 0.6f));
        CacheTexture("warningBg", new Color(0.5f, 0.15f, 0.1f, 0.8f));
        CacheTexture("tipBg", new Color(0.15f, 0.15f, 0.18f, 0.9f));
    }

    void InitializeStyles()
    {
        if (stylesInitialized) return;

        cachedTitleStyle = new GUIStyle();
        cachedTitleStyle.fontSize = 12;
        cachedTitleStyle.fontStyle = FontStyle.Bold;
        cachedTitleStyle.alignment = TextAnchor.MiddleCenter;
        cachedTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold

        cachedXStyle = new GUIStyle();
        cachedXStyle.fontSize = 10;
        cachedXStyle.fontStyle = FontStyle.Bold;
        cachedXStyle.alignment = TextAnchor.MiddleCenter;
        cachedXStyle.normal.textColor = Color.white;

        cachedWarningStyle = new GUIStyle();
        cachedWarningStyle.fontSize = 9;
        cachedWarningStyle.fontStyle = FontStyle.Bold;
        cachedWarningStyle.alignment = TextAnchor.MiddleCenter;
        cachedWarningStyle.normal.textColor = new Color(1f, 0.7f, 0.5f);
        cachedWarningStyle.wordWrap = true;

        cachedTipHeaderStyle = new GUIStyle();
        cachedTipHeaderStyle.fontSize = 9;
        cachedTipHeaderStyle.fontStyle = FontStyle.Bold;
        cachedTipHeaderStyle.alignment = TextAnchor.MiddleLeft;
        cachedTipHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

        cachedTipStyle = new GUIStyle();
        cachedTipStyle.fontSize = 9;
        cachedTipStyle.alignment = TextAnchor.UpperLeft;
        cachedTipStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        cachedTipStyle.wordWrap = true;

        cachedControlStyle = new GUIStyle();
        cachedControlStyle.fontSize = 9;
        cachedControlStyle.fontStyle = FontStyle.Bold;
        cachedControlStyle.alignment = TextAnchor.MiddleCenter;
        cachedControlStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);

        cachedKeyStyle = new GUIStyle();
        cachedKeyStyle.fontSize = 8;
        cachedKeyStyle.alignment = TextAnchor.MiddleCenter;
        cachedKeyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        cachedFinalStyle = new GUIStyle();
        cachedFinalStyle.fontSize = 10;
        cachedFinalStyle.fontStyle = FontStyle.Bold;
        cachedFinalStyle.alignment = TextAnchor.MiddleCenter;
        cachedFinalStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);

        cachedHintStyle = new GUIStyle();
        cachedHintStyle.fontSize = 8;
        cachedHintStyle.alignment = TextAnchor.MiddleCenter;
        cachedHintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

        stylesInitialized = true;
    }

    void CacheTexture(string name, Color color)
    {
        if (!textureCache.ContainsKey(name))
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            textureCache[name] = tex;
        }
    }

    Texture2D GetTexture(string name)
    {
        if (textureCache.TryGetValue(name, out Texture2D tex))
        {
            return tex;
        }
        return Texture2D.whiteTexture;
    }

    void Update()
    {
        // Show popup when game starts (only once per session)
        if (MainMenu.GameStarted && !hasShownOnce && initialized)
        {
            // Small delay to let other systems initialize
            Invoke("ShowPopup", 0.5f);
            hasShownOnce = true;
        }

        // Close with ESC
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePopup();
        }
    }

    public void ShowPopup()
    {
        isOpen = true;
    }

    public void ClosePopup()
    {
        isOpen = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    void OnGUI()
    {
        if (!isOpen || !initialized) return;

        // Initialize styles lazily (must be done inside OnGUI context)
        InitializeStyles();

        // Position on left side, vertically centered
        float panelX = 20f;
        float panelY = (Screen.height - panelHeight) / 2f;

        // Gold border (3px)
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("panelBorder"));

        // Panel background
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // ============ TITLE ============
        GUI.Label(new Rect(panelX, panelY + 8, panelWidth, 18), "SURVIVAL GUIDE", cachedTitleStyle);

        // ============ CLOSE BUTTON (X) ============
        float closeBtnSize = 16f;
        Rect closeRect = new Rect(panelX + panelWidth - closeBtnSize - 4, panelY + 4, closeBtnSize, closeBtnSize);
        GUI.DrawTexture(closeRect, GetTexture("closeBtn"));
        GUI.Label(closeRect, "X", cachedXStyle);

        if (GUI.Button(closeRect, "", GUIStyle.none))
        {
            ClosePopup();
        }

        // ============ DIVIDER ============
        GUI.DrawTexture(new Rect(panelX + 10, panelY + 28, panelWidth - 20, 1), GetTexture("divider"));

        // ============ CONTENT ============
        float contentY = panelY + 34;
        float padding = 10f;
        float contentWidth = panelWidth - (padding * 2);

        // Warning box
        Rect warningRect = new Rect(panelX + padding, contentY, contentWidth, 22);
        GUI.DrawTexture(warningRect, GetTexture("warningBg"));
        GUI.Label(warningRect, "Health depletes! Eat to survive.", cachedWarningStyle);

        contentY += 28;

        // Tip background
        Rect tipBgRect = new Rect(panelX + padding, contentY, contentWidth, 68);
        GUI.DrawTexture(tipBgRect, GetTexture("tipBg"));

        float tipY = contentY + 4;
        float tipPadding = 6f;

        // Tips header
        GUI.Label(new Rect(panelX + padding + tipPadding, tipY, contentWidth - tipPadding * 2, 14), "TIPS:", cachedTipHeaderStyle);
        tipY += 14;

        // Tips list
        string[] tips = new string[]
        {
            "F near water = Fish",
            "Cook at BBQ for HP",
            "Sell rare fish to NPCs",
        };

        foreach (string tip in tips)
        {
            GUI.Label(new Rect(panelX + padding + tipPadding, tipY, contentWidth - tipPadding * 2, 16), tip, cachedTipStyle);
            tipY += 16;
        }

        contentY += 74;

        // Controls section
        Rect controlsBgRect = new Rect(panelX + padding, contentY, contentWidth, 30);
        GUI.DrawTexture(controlsBgRect, GetTexture("tipBg"));

        GUI.Label(new Rect(panelX + padding, contentY + 4, contentWidth, 10), "KEYS", cachedControlStyle);
        GUI.Label(new Rect(panelX + padding, contentY + 16, contentWidth, 10), "F=Fish E=Use TAB=Stats", cachedKeyStyle);

        contentY += 36;

        // Final message
        GUI.Label(new Rect(panelX, contentY, panelWidth, 14), "Don't starve!", cachedFinalStyle);

        // ============ FOOTER HINT ============
        GUI.Label(new Rect(panelX, panelY + panelHeight - 14, panelWidth, 12), "ESC or X to close", cachedHintStyle);
    }

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}
