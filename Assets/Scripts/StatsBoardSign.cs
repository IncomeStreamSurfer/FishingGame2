using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stats Board Sign on the beach
/// - Shows player stats when F is pressed
/// - Displays all fish caught with counts, total gold earned, total XP gained
/// - Styled like a wooden board/sign
/// </summary>
public class StatsBoardSign : MonoBehaviour
{
    private bool playerNearby = false;
    private bool showingStats = false;
    private const float INTERACTION_DISTANCE = 4f;

    private Texture2D signTexture;
    private int guiFrameSkip = 0;

    // Cached UI textures and styles (created once, reused every frame)
    private static Texture2D cachedBgTex;
    private static Texture2D cachedBoxBgTex;
    private static Texture2D cachedBorderTex;
    private static Texture2D cachedHeaderBgTex;
    private static GUIStyle cachedPromptStyle;
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedStatLabelStyle;
    private static GUIStyle cachedStatValueStyle;
    private static GUIStyle cachedFishNameStyle;
    private static GUIStyle cachedFishCountStyle;
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

        // Initialize draggable window (400x500)
        float panelWidth = 400f;
        float panelHeight = 500f;
        Rect initialRect = new Rect(
            (Screen.width - panelWidth) / 2f,
            (Screen.height - panelHeight) / 2f,
            panelWidth,
            panelHeight
        );
        window = new DraggableWindow(initialRect, new Vector2(350, 400), new Vector2(600, 700));
    }

    void InitializeCachedUI()
    {
        if (stylesInitialized) return;

        // Cache textures
        cachedBgTex = new Texture2D(1, 1);
        cachedBgTex.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        cachedBgTex.Apply();

        cachedBoxBgTex = new Texture2D(1, 1);
        cachedBoxBgTex.SetPixel(0, 0, new Color(0.15f, 0.1f, 0.05f, 0.95f));
        cachedBoxBgTex.Apply();

        cachedBorderTex = new Texture2D(1, 1);
        cachedBorderTex.SetPixel(0, 0, new Color(0.6f, 0.4f, 0.2f));
        cachedBorderTex.Apply();

        cachedHeaderBgTex = new Texture2D(1, 1);
        cachedHeaderBgTex.SetPixel(0, 0, new Color(0.2f, 0.15f, 0.08f, 0.95f));
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

            // F to view stats
            if (playerNearby && Input.GetKeyDown(KeyCode.F) && !FishInventoryPanel.Instance.IsOpen())
            {
                showingStats = !showingStats;
                if (showingStats)
                    scrollPos = 0f;
            }
        }

        // Close with ESC
        if (showingStats && Input.GetKeyDown(KeyCode.Escape))
        {
            showingStats = false;
        }
    }

    void CreateSignTexture()
    {
        int width = 128;
        int height = 96;
        signTexture = new Texture2D(width, height);

        Color woodColor = new Color(0.5f, 0.35f, 0.2f);
        Color darkBorder = new Color(0.25f, 0.15f, 0.08f);

        // Fill with wood background
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Add wood grain texture variation
                float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.3f) * 0.15f;
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
        DrawText("SCORES", 36, 65, textColor);
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
            case 'A': return new bool[,] {{false,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true}};
            case 'E': return new bool[,] {{true,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,true}};
            case 'F': return new bool[,] {{true,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false}};
            case 'P': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false}};
            case 'R': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false},{true,false,true,false,false},{true,false,false,true,false},{true,false,false,false,true}};
            case 'S': return new bool[,] {{false,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{false,true,true,true,false},{false,false,false,false,true},{false,false,false,false,true},{true,true,true,true,false}};
            case 'T': return new bool[,] {{true,true,true,true,true},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false}};
            case 'C': return new bool[,] {{false,true,true,true,false},{true,false,false,false,true},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,true},{false,true,true,true,false}};
            case 'O': return new bool[,] {{false,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{false,true,true,true,false}};
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
        // Performance: Skip frames when not actively needed
        if (!showingStats && !playerNearby)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        if (!MainMenu.GameStarted) return;

        // Initialize styles lazily (can't do in Start because GUI.skin not ready)
        if (cachedPromptStyle == null)
        {
            cachedPromptStyle = new GUIStyle();
            cachedPromptStyle.fontSize = 14;
            cachedPromptStyle.fontStyle = FontStyle.Bold;
            cachedPromptStyle.normal.textColor = Color.white;
            cachedPromptStyle.alignment = TextAnchor.MiddleCenter;

            cachedTitleStyle = new GUIStyle();
            cachedTitleStyle.fontSize = 20;
            cachedTitleStyle.fontStyle = FontStyle.Bold;
            cachedTitleStyle.normal.textColor = new Color(0.9f, 0.7f, 0.3f);
            cachedTitleStyle.alignment = TextAnchor.MiddleCenter;

            cachedStatLabelStyle = new GUIStyle();
            cachedStatLabelStyle.fontSize = 12;
            cachedStatLabelStyle.fontStyle = FontStyle.Bold;
            cachedStatLabelStyle.normal.textColor = new Color(0.9f, 0.8f, 0.5f);

            cachedStatValueStyle = new GUIStyle();
            cachedStatValueStyle.fontSize = 14;
            cachedStatValueStyle.fontStyle = FontStyle.Bold;
            cachedStatValueStyle.normal.textColor = new Color(0.4f, 1f, 0.6f);

            cachedFishNameStyle = new GUIStyle();
            cachedFishNameStyle.fontSize = 11;
            cachedFishNameStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);

            cachedFishCountStyle = new GUIStyle();
            cachedFishCountStyle.fontSize = 11;
            cachedFishCountStyle.fontStyle = FontStyle.Bold;
            cachedFishCountStyle.alignment = TextAnchor.MiddleRight;
            cachedFishCountStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

            cachedCloseStyle = new GUIStyle();
            cachedCloseStyle.fontSize = 11;
            cachedCloseStyle.alignment = TextAnchor.MiddleCenter;
            cachedCloseStyle.normal.textColor = new Color(0.7f, 0.7f, 0.6f);

            cachedXButtonStyle = new GUIStyle();
            cachedXButtonStyle.fontSize = 16;
            cachedXButtonStyle.fontStyle = FontStyle.Bold;
            cachedXButtonStyle.alignment = TextAnchor.MiddleCenter;
            cachedXButtonStyle.normal.textColor = Color.white;
        }

        // Show "Press F to View Stats" prompt
        if (playerNearby && !showingStats && !FishInventoryPanel.Instance.IsOpen())
        {
            float promptWidth = 180;
            float promptHeight = 30;
            float promptX = (Screen.width - promptWidth) / 2;
            float promptY = Screen.height * 0.7f;

            if (cachedBgTex != null)
                GUI.DrawTexture(new Rect(promptX, promptY, promptWidth, promptHeight), cachedBgTex);
            GUI.Label(new Rect(promptX, promptY, promptWidth, promptHeight), "[F] View Stats", cachedPromptStyle);
        }

        // Show stats panel
        if (showingStats)
        {
            DrawStatsPanel();
        }
    }

    void DrawStatsPanel()
    {
        if (FishingSystem.Instance == null || GameManager.Instance == null || window == null) return;

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
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 50), cachedHeaderBgTex);
        GUI.Label(new Rect(panelX, panelY + 10, panelWidth, 30), "PLAYER STATS", cachedTitleStyle);

        // Red X close button
        GUI.DrawTexture(new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22), CreateColorTexture(new Color(0.8f, 0.2f, 0.2f)));
        if (GUI.Button(new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22), "X", cachedXButtonStyle))
        {
            showingStats = false;
        }

        float contentY = panelY + 60;

        // Get stats from FishInventoryPanel (PlayerPrefs)
        float biggestFishWeight = PlayerPrefs.GetFloat("BiggestFishWeight", 0f);
        int mostValuableCatch = PlayerPrefs.GetInt("MostValuableCatch", 0);
        int totalFishCaught = PlayerPrefs.GetInt("TotalFishCaught", 0);
        int totalGoldEarned = PlayerPrefs.GetInt("TotalGoldEarned", 0);

        // Get current XP from LevelingSystem
        long currentXP = 0;
        if (LevelingSystem.Instance != null)
        {
            currentXP = LevelingSystem.Instance.GetCurrentXP();
        }

        // Summary stats
        GUI.Label(new Rect(panelX + 20, contentY, 200, 20), "Total Fish Caught:", cachedStatLabelStyle);
        GUI.Label(new Rect(panelX + 220, contentY, 160, 20), totalFishCaught.ToString(), cachedStatValueStyle);
        contentY += 25;

        GUI.Label(new Rect(panelX + 20, contentY, 200, 20), "Total Gold Earned:", cachedStatLabelStyle);
        GUI.Label(new Rect(panelX + 220, contentY, 160, 20), totalGoldEarned + "g", cachedStatValueStyle);
        contentY += 25;

        GUI.Label(new Rect(panelX + 20, contentY, 200, 20), "Total XP Gained:", cachedStatLabelStyle);
        GUI.Label(new Rect(panelX + 220, contentY, 160, 20), currentXP.ToString(), cachedStatValueStyle);
        contentY += 25;

        GUI.Label(new Rect(panelX + 20, contentY, 200, 20), "Biggest Fish:", cachedStatLabelStyle);
        GUI.Label(new Rect(panelX + 220, contentY, 160, 20), biggestFishWeight.ToString("F1") + " kg", cachedStatValueStyle);
        contentY += 25;

        GUI.Label(new Rect(panelX + 20, contentY, 200, 20), "Most Valuable Catch:", cachedStatLabelStyle);
        GUI.Label(new Rect(panelX + 220, contentY, 160, 20), mostValuableCatch + "g", cachedStatValueStyle);
        contentY += 35;

        // Fish caught list header
        GUI.DrawTexture(new Rect(panelX + 10, contentY, panelWidth - 20, 2), cachedBorderTex);
        contentY += 10;
        GUI.Label(new Rect(panelX + 20, contentY, panelWidth - 40, 20), "Fish Inventory:", cachedStatLabelStyle);
        contentY += 25;

        // Fish list area
        float listY = contentY;
        float listHeight = panelHeight - (contentY - panelY) - 30;
        Rect listArea = new Rect(panelX + 20, listY, panelWidth - 40, listHeight);

        // Get fish list from inventory
        List<FishStatsData> fishList = GetFishStatsList();

        float itemHeight = 25;
        float totalContentHeight = fishList.Count * itemHeight;
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

        if (fishList.Count == 0)
        {
            GUI.Label(new Rect(0, listHeight / 2 - 20, listArea.width, 40), "No fish caught yet!", cachedFishNameStyle);
        }
        else
        {
            float itemY = -scrollPos;
            for (int i = 0; i < fishList.Count; i++)
            {
                // Skip items outside visible area
                if (itemY + itemHeight < 0 || itemY > listHeight)
                {
                    itemY += itemHeight;
                    continue;
                }

                FishStatsData fish = fishList[i];

                // Fish name
                GUI.Label(new Rect(0, itemY, listArea.width - 80, itemHeight), fish.name, cachedFishNameStyle);

                // Count
                GUI.Label(new Rect(listArea.width - 80, itemY, 75, itemHeight), $"x{fish.count}", cachedFishCountStyle);

                itemY += itemHeight;
            }
        }

        GUI.EndGroup();

        // Scroll indicator
        if (maxScroll > 0)
        {
            float scrollBarHeight = listHeight * (listHeight / totalContentHeight);
            float scrollBarY = listY + (scrollPos / maxScroll) * (listHeight - scrollBarHeight);
            GUI.DrawTexture(new Rect(panelX + panelWidth - 8, scrollBarY, 4, scrollBarHeight), CreateColorTexture(new Color(0.5f, 0.45f, 0.35f)));
        }

        // Close hint at bottom
        GUI.Label(new Rect(panelX, panelY + panelHeight - 25, panelWidth, 20), "Press F or ESC to close", cachedCloseStyle);

        // Draw resize handle
        window.DrawResizeHandle();
    }

    List<FishStatsData> GetFishStatsList()
    {
        List<FishStatsData> result = new List<FishStatsData>();

        if (FishingSystem.Instance == null || GameManager.Instance == null)
            return result;

        var fishDatabase = FishingSystem.Instance.fishDatabase;
        var inventory = GameManager.Instance.fishInventory;

        // Add normal fish from GameManager inventory
        foreach (var kvp in inventory)
        {
            string fishId = kvp.Key;
            int count = kvp.Value;

            if (count <= 0) continue;

            // Find fish data
            FishData fishData = fishDatabase.Find(f => f.id == fishId);
            if (fishData != null)
            {
                result.Add(new FishStatsData
                {
                    name = fishData.fishName,
                    count = count
                });
            }
        }

        // Add special fish from FishingSystem special inventory
        var specialInventory = FishingSystem.Instance.specialFishInventory;
        Dictionary<string, int> specialCounts = new Dictionary<string, int>();
        foreach (var fish in specialInventory)
        {
            if (!specialCounts.ContainsKey(fish.id))
                specialCounts[fish.id] = 0;
            specialCounts[fish.id]++;
        }

        foreach (var kvp in specialCounts)
        {
            string fishId = kvp.Key;
            int count = kvp.Value;

            FishData fishData = fishDatabase.Find(f => f.id == fishId);
            if (fishData != null)
            {
                result.Add(new FishStatsData
                {
                    name = fishData.fishName + " (Special)",
                    count = count
                });
            }
        }

        // Sort alphabetically
        result.Sort((a, b) => a.name.CompareTo(b.name));

        return result;
    }

    Texture2D CreateColorTexture(Color color)
    {
        Texture2D tex = new Texture2D(2, 2);
        Color[] pixels = new Color[4];
        for (int i = 0; i < 4; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    void OnDestroy()
    {
        if (signTexture != null)
        {
            Destroy(signTexture);
        }
    }
}

public class FishStatsData
{
    public string name;
    public int count;
}
