using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Drop Rates Sign on the beach
/// - Shows all fish drop rates and % chances when F is pressed
/// - Displays bottle drop rates and special item chances
/// </summary>
public class DropRatesSign : MonoBehaviour
{
    private bool playerNearby = false;
    private bool showingRates = false;
    private const float INTERACTION_DISTANCE = 4f;

    private Texture2D signTexture;

    // Cached UI textures and styles
    private static Texture2D cachedBgTex;
    private static Texture2D cachedBoxBgTex;
    private static Texture2D cachedBorderTex;
    private static Texture2D cachedHeaderBgTex;
    private static GUIStyle cachedPromptStyle;
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedSectionStyle;
    private static GUIStyle cachedItemStyle;
    private static GUIStyle cachedPercentStyle;
    private static GUIStyle cachedCloseStyle;
    private static GUIStyle cachedXButtonStyle;
    private static bool stylesInitialized = false;

    // Draggable window support
    private DraggableWindow window;
    private float scrollPos = 0f;

    void Start()
    {
        CreateSignTexture();
        ApplyTexture();
        InitializeCachedUI();

        // Initialize draggable window
        float panelWidth = 380f;
        float panelHeight = 500f;
        Rect initialRect = new Rect(
            (Screen.width - panelWidth) / 2f,
            (Screen.height - panelHeight) / 2f,
            panelWidth,
            panelHeight
        );
        window = new DraggableWindow(initialRect, new Vector2(320, 400), new Vector2(500, 650));
    }

    void InitializeCachedUI()
    {
        if (stylesInitialized) return;

        cachedBgTex = new Texture2D(1, 1);
        cachedBgTex.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        cachedBgTex.Apply();

        cachedBoxBgTex = new Texture2D(1, 1);
        cachedBoxBgTex.SetPixel(0, 0, new Color(0.1f, 0.15f, 0.2f, 0.95f));
        cachedBoxBgTex.Apply();

        cachedBorderTex = new Texture2D(1, 1);
        cachedBorderTex.SetPixel(0, 0, new Color(0.3f, 0.5f, 0.7f));
        cachedBorderTex.Apply();

        cachedHeaderBgTex = new Texture2D(1, 1);
        cachedHeaderBgTex.SetPixel(0, 0, new Color(0.15f, 0.25f, 0.35f, 0.95f));
        cachedHeaderBgTex.Apply();

        stylesInitialized = true;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check player proximity
        if (GameCache.IsPlayerValid())
        {
            float distance = Vector3.Distance(transform.position, GameCache.Player.position);
            playerNearby = distance < INTERACTION_DISTANCE;

            // F to view rates
            if (playerNearby && Input.GetKeyDown(KeyCode.F) && !FishInventoryPanel.Instance.IsOpen())
            {
                showingRates = !showingRates;
                if (showingRates)
                    scrollPos = 0f;
            }
        }

        // Close with ESC
        if (showingRates && Input.GetKeyDown(KeyCode.Escape))
        {
            showingRates = false;
        }
    }

    void CreateSignTexture()
    {
        int width = 128;
        int height = 96;
        signTexture = new Texture2D(width, height);

        Color woodColor = new Color(0.4f, 0.3f, 0.25f);
        Color darkBorder = new Color(0.2f, 0.12f, 0.08f);

        // Fill with wood background
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.3f) * 0.12f;
                Color pixelColor = new Color(
                    woodColor.r + noise,
                    woodColor.g + noise,
                    woodColor.b + noise
                );
                signTexture.SetPixel(x, y, pixelColor);
            }
        }

        // Add dark border
        int borderWidth = 4;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < borderWidth || x >= width - borderWidth ||
                    y < borderWidth || y >= height - borderWidth)
                {
                    signTexture.SetPixel(x, y, darkBorder);
                }
            }
        }

        // Draw title text
        Color textColor = new Color(0.9f, 0.85f, 0.7f);
        DrawText("DROPS", 38, 65, textColor);
        DrawText("PRESS F", 30, 20, textColor);

        signTexture.Apply();
        signTexture.filterMode = FilterMode.Point;
    }

    void DrawText(string text, int startX, int startY, Color color)
    {
        int charWidth = 6;
        for (int i = 0; i < text.Length; i++)
        {
            DrawChar(text[i], startX + i * charWidth, startY, color);
        }
    }

    void DrawChar(char c, int x, int y, Color color)
    {
        bool[,] pixels = GetCharPixels(c);
        if (pixels == null) return;

        for (int py = 0; py < 7; py++)
        {
            for (int px = 0; px < 5; px++)
            {
                if (pixels[py, px])
                {
                    int texX = x + px;
                    int texY = y + (6 - py);
                    if (texX >= 0 && texX < signTexture.width &&
                        texY >= 0 && texY < signTexture.height)
                    {
                        signTexture.SetPixel(texX, texY, color);
                    }
                }
            }
        }
    }

    bool[,] GetCharPixels(char c)
    {
        switch (char.ToUpper(c))
        {
            case 'D': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false}};
            case 'R': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false},{true,false,true,false,false},{true,false,false,true,false},{true,false,false,false,true}};
            case 'O': return new bool[,] {{false,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{false,true,true,true,false}};
            case 'P': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false}};
            case 'S': return new bool[,] {{false,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{false,true,true,true,false},{false,false,false,false,true},{false,false,false,false,true},{true,true,true,true,false}};
            case 'E': return new bool[,] {{true,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,true}};
            case 'F': return new bool[,] {{true,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false}};
            case ' ': return new bool[,] {{false,false,false,false,false},{false,false,false,false,false},{false,false,false,false,false},{false,false,false,false,false},{false,false,false,false,false},{false,false,false,false,false},{false,false,false,false,false}};
            default: return null;
        }
    }

    void ApplyTexture()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && signTexture != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = signTexture;
            mat.color = new Color(1f, 1f, 1f);
            mat.SetFloat("_Glossiness", 0.15f);
            rend.material = mat;
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Initialize styles lazily
        if (cachedPromptStyle == null)
        {
            cachedPromptStyle = new GUIStyle();
            cachedPromptStyle.fontSize = 14;
            cachedPromptStyle.fontStyle = FontStyle.Bold;
            cachedPromptStyle.normal.textColor = Color.white;
            cachedPromptStyle.alignment = TextAnchor.MiddleCenter;

            cachedTitleStyle = new GUIStyle();
            cachedTitleStyle.fontSize = 18;
            cachedTitleStyle.fontStyle = FontStyle.Bold;
            cachedTitleStyle.normal.textColor = new Color(0.4f, 0.8f, 1f);
            cachedTitleStyle.alignment = TextAnchor.MiddleCenter;

            cachedSectionStyle = new GUIStyle();
            cachedSectionStyle.fontSize = 13;
            cachedSectionStyle.fontStyle = FontStyle.Bold;
            cachedSectionStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            cachedItemStyle = new GUIStyle();
            cachedItemStyle.fontSize = 11;
            cachedItemStyle.normal.textColor = new Color(0.9f, 0.9f, 0.85f);

            cachedPercentStyle = new GUIStyle();
            cachedPercentStyle.fontSize = 11;
            cachedPercentStyle.fontStyle = FontStyle.Bold;
            cachedPercentStyle.alignment = TextAnchor.MiddleRight;
            cachedPercentStyle.normal.textColor = new Color(0.5f, 1f, 0.6f);

            cachedCloseStyle = new GUIStyle();
            cachedCloseStyle.fontSize = 10;
            cachedCloseStyle.alignment = TextAnchor.MiddleCenter;
            cachedCloseStyle.normal.textColor = new Color(0.7f, 0.7f, 0.6f);

            cachedXButtonStyle = new GUIStyle();
            cachedXButtonStyle.fontSize = 16;
            cachedXButtonStyle.fontStyle = FontStyle.Bold;
            cachedXButtonStyle.alignment = TextAnchor.MiddleCenter;
            cachedXButtonStyle.normal.textColor = Color.white;
        }

        // Show "Press F to View Drop Rates" prompt
        if (playerNearby && !showingRates && !FishInventoryPanel.Instance.IsOpen())
        {
            float promptWidth = 200;
            float promptHeight = 30;
            float promptX = (Screen.width - promptWidth) / 2;
            float promptY = Screen.height * 0.7f;

            if (cachedBgTex != null)
                GUI.DrawTexture(new Rect(promptX, promptY, promptWidth, promptHeight), cachedBgTex);
            GUI.Label(new Rect(promptX, promptY, promptWidth, promptHeight), "[F] View Drop Rates", cachedPromptStyle);
        }

        // Show rates panel
        if (showingRates)
        {
            DrawRatesPanel();
        }
    }

    void DrawRatesPanel()
    {
        if (window == null) return;

        // Handle dragging and resizing
        window.UpdateWindow();

        // Get window rect
        Rect rect = window.WindowRect;
        float panelX = rect.x;
        float panelY = rect.y;
        float panelWidth = rect.width;
        float panelHeight = rect.height;

        // Border and background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), cachedBorderTex);
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), cachedBoxBgTex);

        // Header
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 45), cachedHeaderBgTex);
        GUI.Label(new Rect(panelX, panelY + 10, panelWidth, 30), "DROP RATES & CHANCES", cachedTitleStyle);

        // Red X close button
        Texture2D redTex = new Texture2D(1, 1);
        redTex.SetPixel(0, 0, new Color(0.8f, 0.2f, 0.2f));
        redTex.Apply();
        GUI.DrawTexture(new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22), redTex);
        if (GUI.Button(new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22), "X", cachedXButtonStyle))
        {
            showingRates = false;
        }
        Object.Destroy(redTex);

        // Content area with scrolling
        float contentY = panelY + 55;
        float listHeight = panelHeight - 85;
        Rect listArea = new Rect(panelX + 15, contentY, panelWidth - 30, listHeight);

        // Calculate total content height
        float totalContentHeight = 850f; // Approximate height of all content (including ultra rare events section)
        float maxScroll = Mathf.Max(0, totalContentHeight - listHeight);

        if (listArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                scrollPos += Event.current.delta.y * 25f;
                scrollPos = Mathf.Clamp(scrollPos, 0, maxScroll);
                Event.current.Use();
            }
        }

        GUI.BeginGroup(listArea);

        float y = -scrollPos;

        // FISH RARITY DROP RATES
        GUI.Label(new Rect(0, y, listArea.width, 20), "FISH RARITY DROP RATES", cachedSectionStyle);
        y += 25;

        DrawDropItem(0, y, listArea.width, "Common Fish", "62%", new Color(0.7f, 0.7f, 0.7f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Uncommon Fish", "32%", new Color(0.3f, 0.9f, 0.3f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Rare Fish", "5%", new Color(0.4f, 0.6f, 1f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Epic Fish", "1%", new Color(0.8f, 0.4f, 1f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Legendary Fish", "0.1%", new Color(1f, 0.75f, 0.2f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Golden Starfish", "0.01%", new Color(1f, 0.35f, 0.35f)); y += 30;

        // GOLD FIND (instead of fish)
        GUI.Label(new Rect(0, y, listArea.width, 20), "GOLD FIND (15% of casts)", cachedSectionStyle);
        y += 25;

        DrawDropItem(0, y, listArea.width, "Small Gold (1-50g)", "99%", new Color(1f, 0.9f, 0.3f)); y += 20;
        DrawDropItem(0, y, listArea.width, "RARE CHEST (1000g!)", "1%", new Color(1f, 0.7f, 0.1f)); y += 30;

        // ULTRA RARE EVENTS
        GUI.Label(new Rect(0, y, listArea.width, 20), "ULTRA RARE EVENTS", cachedSectionStyle);
        y += 25;

        DrawDropItem(0, y, listArea.width, "Million Gold Jackpot", "1 in 100,000", new Color(1f, 0.85f, 0.2f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Shoulder Parrot Pet", "1 in 500,000", new Color(0.4f, 1f, 0.5f)); y += 30;

        // ROD BONUS
        GUI.Label(new Rect(0, y, listArea.width, 20), "ROD RARITY BONUSES", cachedSectionStyle);
        y += 25;

        DrawDropItem(0, y, listArea.width, "Basic Rod", "+0%", Color.gray); y += 20;
        DrawDropItem(0, y, listArea.width, "Bronze Rod (Lvl 10)", "+5%", new Color(0.8f, 0.5f, 0.2f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Silver Rod (Lvl 40)", "+10%", new Color(0.75f, 0.75f, 0.8f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Golden Rod (Lvl 100)", "+15%", new Color(1f, 0.85f, 0.2f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Legendary Rod (Lvl 175)", "+20%", new Color(0.8f, 0.4f, 1f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Epic Rod (Lvl 250)", "+25%", new Color(1f, 0.5f, 0.3f)); y += 30;

        // BOTTLE EVENT
        GUI.Label(new Rect(0, y, listArea.width, 20), "BOTTLE EVENT (1% per cast)", cachedSectionStyle);
        y += 25;

        DrawDropItem(0, y, listArea.width, "Coins (10-10,000)", "~42%", new Color(1f, 0.9f, 0.3f)); y += 20;
        DrawDropItem(0, y, listArea.width, "XP (10-10,000)", "~42%", new Color(0.3f, 1f, 0.5f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Groovy Marlin Ring (+10 Lvl)", "10%", new Color(0.3f, 0.8f, 1f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Golden Fishing Hat", "5%", new Color(1f, 0.85f, 0.2f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Epic Fishing Rod", "1%", new Color(0.6f, 0.2f, 0.8f)); y += 20;
        DrawDropItem(0, y, listArea.width, "JACKPOT (1,000,000 coins!)", "0.05%", new Color(1f, 0.85f, 0f)); y += 30;

        // SPECIAL MECHANICS
        GUI.Label(new Rect(0, y, listArea.width, 20), "SPECIAL MECHANICS", cachedSectionStyle);
        y += 25;

        DrawDropItem(0, y, listArea.width, "Cast Distance affects rarity", "", new Color(0.8f, 0.8f, 0.6f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Farther cast = better fish", "", new Color(0.8f, 0.8f, 0.6f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Hold click to charge cast", "", new Color(0.8f, 0.8f, 0.6f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Special fish glow at night", "", new Color(0.8f, 0.8f, 0.6f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Ice Realm has unique fish", "", new Color(0.8f, 0.8f, 0.6f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Jungle has rare species", "", new Color(0.8f, 0.8f, 0.6f)); y += 30;

        // XP REWARDS
        GUI.Label(new Rect(0, y, listArea.width, 20), "XP PER FISH CAUGHT", cachedSectionStyle);
        y += 25;

        DrawDropItem(0, y, listArea.width, "Common", "5 XP (x5 = 25)", new Color(0.7f, 0.7f, 0.7f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Uncommon", "25 XP (x5 = 125)", new Color(0.3f, 0.9f, 0.3f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Rare", "100 XP (x5 = 500)", new Color(0.4f, 0.6f, 1f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Epic", "300 XP (x5 = 1,500)", new Color(0.8f, 0.4f, 1f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Legendary", "1,000 XP (x5 = 5,000)", new Color(1f, 0.75f, 0.2f)); y += 20;
        DrawDropItem(0, y, listArea.width, "Mythic", "2,000 XP (x5 = 10,000)", new Color(1f, 0.35f, 0.35f)); y += 20;

        GUI.EndGroup();

        // Scroll indicator
        if (maxScroll > 0)
        {
            float scrollBarHeight = listHeight * (listHeight / totalContentHeight);
            float scrollBarY = contentY + (scrollPos / maxScroll) * (listHeight - scrollBarHeight);
            Texture2D scrollTex = new Texture2D(1, 1);
            scrollTex.SetPixel(0, 0, new Color(0.4f, 0.6f, 0.8f));
            scrollTex.Apply();
            GUI.DrawTexture(new Rect(panelX + panelWidth - 8, scrollBarY, 4, scrollBarHeight), scrollTex);
            Object.Destroy(scrollTex);
        }

        // Close hint at bottom
        GUI.Label(new Rect(panelX, panelY + panelHeight - 25, panelWidth, 20), "Press F or ESC to close", cachedCloseStyle);

        // Draw resize handle
        window.DrawResizeHandle();
    }

    void DrawDropItem(float x, float y, float width, string name, string percent, Color nameColor)
    {
        cachedItemStyle.normal.textColor = nameColor;
        GUI.Label(new Rect(x, y, width - 80, 18), name, cachedItemStyle);
        GUI.Label(new Rect(x + width - 80, y, 75, 18), percent, cachedPercentStyle);
    }

    void OnDestroy()
    {
        if (signTexture != null)
        {
            Destroy(signTexture);
        }
    }
}
