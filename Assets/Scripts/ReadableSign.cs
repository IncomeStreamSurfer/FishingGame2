using UnityEngine;

/// <summary>
/// Readable sign that displays a message when player presses E near it
/// </summary>
public class ReadableSign : MonoBehaviour
{
    public string signTitle = "NOTICE";
    public string signMessage = "Welcome to the island!";
    public Color titleColor = new Color(0.9f, 0.2f, 0.2f);
    public Color backgroundColor = new Color(0.95f, 0.85f, 0.3f);

    private bool playerNearby = false;
    private bool showingMessage = false;
    private float messageTimer = 0f;
    private const float MESSAGE_DURATION = 5f;
    private const float INTERACTION_DISTANCE = 3f;

    private Texture2D signTexture;
    private int guiFrameSkip = 0;

    // Cached UI textures and styles (created once, reused every frame)
    private static Texture2D cachedBgTex;
    private static Texture2D cachedBoxBgTex;
    private static Texture2D cachedBorderTex;
    private static GUIStyle cachedPromptStyle;
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedMsgStyle;
    private static bool stylesInitialized = false;

    void Start()
    {
        CreateSignTexture();
        ApplyTexture();
        InitializeCachedUI();
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
        cachedBorderTex.SetPixel(0, 0, new Color(0.95f, 0.85f, 0.3f));
        cachedBorderTex.Apply();

        stylesInitialized = true;
    }

    void Update()
    {
        // Check player proximity
        if (GameCache.IsPlayerValid())
        {
            float distance = Vector3.Distance(transform.position, GameCache.Player.position);
            playerNearby = distance < INTERACTION_DISTANCE;

            // E to read
            if (playerNearby && Input.GetKeyDown(KeyCode.E) && !showingMessage)
            {
                showingMessage = true;
                messageTimer = MESSAGE_DURATION;
            }
        }

        // Timer for message display
        if (showingMessage)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
            {
                showingMessage = false;
            }
        }
    }

    void CreateSignTexture()
    {
        int width = 128;
        int height = 80;
        signTexture = new Texture2D(width, height);

        Color black = new Color(0.1f, 0.1f, 0.1f);

        // Fill with background color
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                signTexture.SetPixel(x, y, backgroundColor);
            }
        }

        // Add colored border
        int borderWidth = 4;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < borderWidth || x >= width - borderWidth ||
                    y < borderWidth || y >= height - borderWidth)
                {
                    signTexture.SetPixel(x, y, titleColor);
                }
            }
        }

        // Draw title and abbreviated message on sign
        string shortTitle = signTitle.Length > 10 ? signTitle.Substring(0, 10) : signTitle;
        int titleX = (width - shortTitle.Length * 6) / 2;
        DrawText(shortTitle, titleX, 50, black);
        DrawText("PRESS E", 30, 20, black);

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
            case 'B': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false}};
            case 'C': return new bool[,] {{false,true,true,true,false},{true,false,false,false,true},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,true},{false,true,true,true,false}};
            case 'D': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false}};
            case 'E': return new bool[,] {{true,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,true}};
            case 'F': return new bool[,] {{true,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false}};
            case 'G': return new bool[,] {{false,true,true,true,false},{true,false,false,false,true},{true,false,false,false,false},{true,false,true,true,true},{true,false,false,false,true},{true,false,false,false,true},{false,true,true,true,false}};
            case 'H': return new bool[,] {{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true}};
            case 'I': return new bool[,] {{true,true,true,true,true},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{true,true,true,true,true}};
            case 'K': return new bool[,] {{true,false,false,false,true},{true,false,false,true,false},{true,false,true,false,false},{true,true,false,false,false},{true,false,true,false,false},{true,false,false,true,false},{true,false,false,false,true}};
            case 'L': return new bool[,] {{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false},{true,true,true,true,true}};
            case 'M': return new bool[,] {{true,false,false,false,true},{true,true,false,true,true},{true,false,true,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true}};
            case 'N': return new bool[,] {{true,false,false,false,true},{true,true,false,false,true},{true,false,true,false,true},{true,false,false,true,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true}};
            case 'O': return new bool[,] {{false,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{false,true,true,true,false}};
            case 'P': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false},{true,false,false,false,false},{true,false,false,false,false},{true,false,false,false,false}};
            case 'R': return new bool[,] {{true,true,true,true,false},{true,false,false,false,true},{true,false,false,false,true},{true,true,true,true,false},{true,false,true,false,false},{true,false,false,true,false},{true,false,false,false,true}};
            case 'S': return new bool[,] {{false,true,true,true,true},{true,false,false,false,false},{true,false,false,false,false},{false,true,true,true,false},{false,false,false,false,true},{false,false,false,false,true},{true,true,true,true,false}};
            case 'T': return new bool[,] {{true,true,true,true,true},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false}};
            case 'U': return new bool[,] {{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{false,true,true,true,false}};
            case 'W': return new bool[,] {{true,false,false,false,true},{true,false,false,false,true},{true,false,false,false,true},{true,false,true,false,true},{true,false,true,false,true},{true,true,false,true,true},{true,false,false,false,true}};
            case 'Y': return new bool[,] {{true,false,false,false,true},{true,false,false,false,true},{false,true,false,true,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false},{false,false,true,false,false}};
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
            mat.SetFloat("_Glossiness", 0.2f);
            rend.material = mat;
        }
    }

    void OnGUI()
    {
        // Performance: Skip frames when not actively needed
        if (!showingMessage && !playerNearby)
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
            cachedTitleStyle.fontSize = 18;
            cachedTitleStyle.fontStyle = FontStyle.Bold;
            cachedTitleStyle.normal.textColor = titleColor;
            cachedTitleStyle.alignment = TextAnchor.MiddleCenter;

            cachedMsgStyle = new GUIStyle();
            cachedMsgStyle.fontSize = 14;
            cachedMsgStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
            cachedMsgStyle.alignment = TextAnchor.MiddleCenter;
            cachedMsgStyle.wordWrap = true;
        }

        // Show "Press E to read" prompt
        if (playerNearby && !showingMessage)
        {
            float promptWidth = 150;
            float promptHeight = 30;
            float promptX = (Screen.width - promptWidth) / 2;
            float promptY = Screen.height * 0.7f;

            // Use cached textures
            if (cachedBgTex != null)
                GUI.DrawTexture(new Rect(promptX, promptY, promptWidth, promptHeight), cachedBgTex);
            GUI.Label(new Rect(promptX, promptY, promptWidth, promptHeight), "Press E to read", cachedPromptStyle);
        }

        // Show full message when reading
        if (showingMessage)
        {
            float boxWidth = 400;
            float boxHeight = 150;
            float boxX = (Screen.width - boxWidth) / 2;
            float boxY = Screen.height * 0.3f;

            // Fade based on timer
            float alpha = Mathf.Clamp01(messageTimer / 0.5f);
            GUI.color = new Color(1, 1, 1, alpha);

            // Use cached textures
            if (cachedBoxBgTex != null)
                GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, boxHeight), cachedBoxBgTex);

            // Border using cached texture
            if (cachedBorderTex != null)
            {
                GUI.DrawTexture(new Rect(boxX, boxY, boxWidth, 4), cachedBorderTex);
                GUI.DrawTexture(new Rect(boxX, boxY + boxHeight - 4, boxWidth, 4), cachedBorderTex);
                GUI.DrawTexture(new Rect(boxX, boxY, 4, boxHeight), cachedBorderTex);
                GUI.DrawTexture(new Rect(boxX + boxWidth - 4, boxY, 4, boxHeight), cachedBorderTex);
            }

            // Title and message using cached styles
            GUI.Label(new Rect(boxX, boxY + 15, boxWidth, 30), signTitle, cachedTitleStyle);
            GUI.Label(new Rect(boxX + 20, boxY + 50, boxWidth - 40, 80), signMessage, cachedMsgStyle);

            GUI.color = Color.white;
        }
    }

    void OnDestroy()
    {
        if (signTexture != null)
        {
            Destroy(signTexture);
        }
    }
}
