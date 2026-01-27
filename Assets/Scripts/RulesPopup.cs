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

    // Panel dimensions - 50% smaller than original
    private float panelWidth = 160f;
    private float panelHeight = 140f;

    // Draggable/resizable window support
    private DraggableWindow window;

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

        // Initialize draggable/resizable window
        Rect initialRect = new Rect(20f, (Screen.height - panelHeight) / 2f, panelWidth, panelHeight);
        window = new DraggableWindow(initialRect, new Vector2(120, 100), new Vector2(300, 280));

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

        // Font sizes reduced for smaller panel
        cachedTitleStyle = new GUIStyle();
        cachedTitleStyle.fontSize = 10;
        cachedTitleStyle.fontStyle = FontStyle.Bold;
        cachedTitleStyle.alignment = TextAnchor.MiddleCenter;
        cachedTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold

        cachedXStyle = new GUIStyle();
        cachedXStyle.fontSize = 9;
        cachedXStyle.fontStyle = FontStyle.Bold;
        cachedXStyle.alignment = TextAnchor.MiddleCenter;
        cachedXStyle.normal.textColor = Color.white;

        cachedWarningStyle = new GUIStyle();
        cachedWarningStyle.fontSize = 8;
        cachedWarningStyle.fontStyle = FontStyle.Bold;
        cachedWarningStyle.alignment = TextAnchor.MiddleCenter;
        cachedWarningStyle.normal.textColor = new Color(1f, 0.7f, 0.5f);
        cachedWarningStyle.wordWrap = true;

        cachedTipHeaderStyle = new GUIStyle();
        cachedTipHeaderStyle.fontSize = 8;
        cachedTipHeaderStyle.fontStyle = FontStyle.Bold;
        cachedTipHeaderStyle.alignment = TextAnchor.MiddleLeft;
        cachedTipHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

        cachedTipStyle = new GUIStyle();
        cachedTipStyle.fontSize = 8;
        cachedTipStyle.alignment = TextAnchor.UpperLeft;
        cachedTipStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
        cachedTipStyle.wordWrap = true;

        cachedControlStyle = new GUIStyle();
        cachedControlStyle.fontSize = 8;
        cachedControlStyle.fontStyle = FontStyle.Bold;
        cachedControlStyle.alignment = TextAnchor.MiddleCenter;
        cachedControlStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);

        cachedKeyStyle = new GUIStyle();
        cachedKeyStyle.fontSize = 7;
        cachedKeyStyle.alignment = TextAnchor.MiddleCenter;
        cachedKeyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        cachedFinalStyle = new GUIStyle();
        cachedFinalStyle.fontSize = 9;
        cachedFinalStyle.fontStyle = FontStyle.Bold;
        cachedFinalStyle.alignment = TextAnchor.MiddleCenter;
        cachedFinalStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);

        cachedHintStyle = new GUIStyle();
        cachedHintStyle.fontSize = 7;
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
        if (!isOpen || !initialized || window == null) return;

        // Initialize styles lazily (must be done inside OnGUI context)
        InitializeStyles();

        // Handle dragging and resizing
        window.UpdateWindow();

        // Get window rect
        Rect rect = window.WindowRect;
        float panelX = rect.x;
        float panelY = rect.y;
        float pWidth = rect.width;
        float pHeight = rect.height;

        // Gold border (2px)
        GUI.DrawTexture(new Rect(panelX - 2, panelY - 2, pWidth + 4, pHeight + 4), GetTexture("panelBorder"));

        // Panel background
        GUI.DrawTexture(new Rect(panelX, panelY, pWidth, pHeight), GetTexture("panelBg"));

        // ============ TITLE ============
        GUI.Label(new Rect(panelX, panelY + 5, pWidth, 14), "SURVIVAL GUIDE", cachedTitleStyle);

        // ============ CLOSE BUTTON (X) ============
        float closeBtnSize = 14f;
        Rect closeRect = new Rect(panelX + pWidth - closeBtnSize - 3, panelY + 3, closeBtnSize, closeBtnSize);
        GUI.DrawTexture(closeRect, GetTexture("closeBtn"));
        GUI.Label(closeRect, "X", cachedXStyle);

        if (GUI.Button(closeRect, "", GUIStyle.none))
        {
            ClosePopup();
        }

        // ============ DIVIDER ============
        GUI.DrawTexture(new Rect(panelX + 6, panelY + 20, pWidth - 12, 1), GetTexture("divider"));

        // ============ CONTENT ============
        float contentY = panelY + 24;
        float padding = 6f;
        float contentWidth = pWidth - (padding * 2);

        // Warning box
        Rect warningRect = new Rect(panelX + padding, contentY, contentWidth, 16);
        GUI.DrawTexture(warningRect, GetTexture("warningBg"));
        GUI.Label(warningRect, "Eat to survive!", cachedWarningStyle);

        contentY += 20;

        // Tip background
        float tipHeight = Mathf.Max(50, pHeight - 80);
        Rect tipBgRect = new Rect(panelX + padding, contentY, contentWidth, tipHeight);
        GUI.DrawTexture(tipBgRect, GetTexture("tipBg"));

        float tipY = contentY + 3;
        float tipPadding = 4f;

        // Tips header
        GUI.Label(new Rect(panelX + padding + tipPadding, tipY, contentWidth - tipPadding * 2, 12), "TIPS:", cachedTipHeaderStyle);
        tipY += 12;

        // Tips list
        string[] tips = new string[]
        {
            "Hold LMB for power cast",
            "F = fish inventory",
            "Cook special fish",
            "Sell rare fish to NPCs",
        };

        foreach (string tip in tips)
        {
            GUI.Label(new Rect(panelX + padding + tipPadding, tipY, contentWidth - tipPadding * 2, 11), tip, cachedTipStyle);
            tipY += 11;
        }

        // ============ FOOTER HINT ============
        GUI.Label(new Rect(panelX, panelY + pHeight - 12, pWidth, 10), "ESC to close | drag to move", cachedHintStyle);

        // Draw resize handle
        window.DrawResizeHandle();
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
