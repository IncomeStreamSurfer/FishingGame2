using UnityEngine;

/// <summary>
/// Manages game resolution and aspect ratio enforcement for PC play.
/// Ensures consistent UI scaling across different screen sizes with letterboxing/pillarboxing.
///
/// Recommended Default: 1920x1080 (16:9) - Most common PC gaming resolution (54.44% of Steam users)
/// Also supports: 2560x1440 (16:9) - Second most common (20.19% of Steam users)
/// </summary>
public class ResolutionManager : MonoBehaviour
{
    public static ResolutionManager Instance { get; private set; }

    // Reference resolution for UI scaling (1920x1080 is the most common PC gaming resolution)
    public static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(1920, 1080);
    public static readonly float TARGET_ASPECT_RATIO = 16f / 9f; // 1.777...

    [Header("Resolution Settings")]
    [Tooltip("The target resolution for the game. Default: 1920x1080 (most common PC gaming resolution)")]
    public Vector2Int targetResolution = new Vector2Int(1920, 1080);

    [Tooltip("Enforce 16:9 aspect ratio with letterboxing/pillarboxing")]
    public bool enforceAspectRatio = true;

    [Tooltip("Allow fullscreen mode")]
    public bool allowFullscreen = true;

    [Header("Debug")]
    [Tooltip("Show resolution info in console")]
    public bool debugMode = false;

    // Current scale factor relative to reference resolution
    private float currentScaleFactor = 1f;

    // Letterbox/pillarbox rects
    private Rect topLetterbox;
    private Rect bottomLetterbox;
    private Rect leftPillarbox;
    private Rect rightPillarbox;
    private bool hasLetterboxing = false;
    private bool hasPillarboxing = false;

    // Cached texture for letterboxing
    private Texture2D blackTexture;

    // Viewport rect (the actual game area after letterboxing/pillarboxing)
    private Rect viewportRect;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeResolution();
    }

    void InitializeResolution()
    {
        // Create black texture for letterboxing
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();

        // Set initial resolution
        ApplyResolution();

        if (debugMode)
        {
            Debug.Log($"[ResolutionManager] Initialized with target: {targetResolution.x}x{targetResolution.y}");
            Debug.Log($"[ResolutionManager] Reference resolution: {REFERENCE_RESOLUTION.x}x{REFERENCE_RESOLUTION.y}");
            Debug.Log($"[ResolutionManager] Current screen: {Screen.width}x{Screen.height}");
        }
    }

    void ApplyResolution()
    {
        // Set the resolution
        Screen.SetResolution(targetResolution.x, targetResolution.y, allowFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        // Calculate scale factor and letterboxing
        RecalculateScaling();
    }

    void Update()
    {
        // Check if screen size changed (window was resized)
        if (Screen.width != targetResolution.x || Screen.height != targetResolution.y)
        {
            RecalculateScaling();
        }
    }

    void RecalculateScaling()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenAspect = screenWidth / screenHeight;

        // Calculate scale factor relative to reference resolution
        currentScaleFactor = Mathf.Min(screenWidth / REFERENCE_RESOLUTION.x, screenHeight / REFERENCE_RESOLUTION.y);

        if (enforceAspectRatio)
        {
            if (screenAspect > TARGET_ASPECT_RATIO)
            {
                // Screen is wider than 16:9 - add pillarboxing (black bars on sides)
                hasPillarboxing = true;
                hasLetterboxing = false;

                float targetWidth = screenHeight * TARGET_ASPECT_RATIO;
                float barWidth = (screenWidth - targetWidth) / 2f;

                leftPillarbox = new Rect(0, 0, barWidth, screenHeight);
                rightPillarbox = new Rect(screenWidth - barWidth, 0, barWidth, screenHeight);
                viewportRect = new Rect(barWidth, 0, targetWidth, screenHeight);

                if (debugMode)
                {
                    Debug.Log($"[ResolutionManager] Pillarboxing applied. Bar width: {barWidth}px");
                }
            }
            else if (screenAspect < TARGET_ASPECT_RATIO)
            {
                // Screen is taller than 16:9 - add letterboxing (black bars on top/bottom)
                hasLetterboxing = true;
                hasPillarboxing = false;

                float targetHeight = screenWidth / TARGET_ASPECT_RATIO;
                float barHeight = (screenHeight - targetHeight) / 2f;

                topLetterbox = new Rect(0, 0, screenWidth, barHeight);
                bottomLetterbox = new Rect(0, screenHeight - barHeight, screenWidth, barHeight);
                viewportRect = new Rect(0, barHeight, screenWidth, targetHeight);

                if (debugMode)
                {
                    Debug.Log($"[ResolutionManager] Letterboxing applied. Bar height: {barHeight}px");
                }
            }
            else
            {
                // Perfect 16:9 aspect ratio
                hasLetterboxing = false;
                hasPillarboxing = false;
                viewportRect = new Rect(0, 0, screenWidth, screenHeight);
            }
        }
        else
        {
            hasLetterboxing = false;
            hasPillarboxing = false;
            viewportRect = new Rect(0, 0, screenWidth, screenHeight);
        }
    }

    void OnGUI()
    {
        if (!enforceAspectRatio)
            return;

        // Draw letterboxing/pillarboxing
        if (hasLetterboxing)
        {
            GUI.DrawTexture(topLetterbox, blackTexture);
            GUI.DrawTexture(bottomLetterbox, blackTexture);
        }
        else if (hasPillarboxing)
        {
            GUI.DrawTexture(leftPillarbox, blackTexture);
            GUI.DrawTexture(rightPillarbox, blackTexture);
        }
    }

    // Public API for UI scripts to use scaled coordinates

    /// <summary>
    /// Get the current scale factor relative to the reference resolution (1920x1080).
    /// Use this to scale UI elements, font sizes, etc.
    /// </summary>
    public static float GetScaleFactor()
    {
        if (Instance == null)
            return 1f;
        return Instance.currentScaleFactor;
    }

    /// <summary>
    /// Get the effective screen width (accounting for pillarboxing).
    /// Use this instead of Screen.width for UI positioning.
    /// </summary>
    public static float GetEffectiveScreenWidth()
    {
        if (Instance == null)
            return Screen.width;
        return Instance.viewportRect.width;
    }

    /// <summary>
    /// Get the effective screen height (accounting for letterboxing).
    /// Use this instead of Screen.height for UI positioning.
    /// </summary>
    public static float GetEffectiveScreenHeight()
    {
        if (Instance == null)
            return Screen.height;
        return Instance.viewportRect.height;
    }

    /// <summary>
    /// Get the viewport offset X (left edge of game area).
    /// Add this to X coordinates when using Screen.width for positioning.
    /// </summary>
    public static float GetViewportOffsetX()
    {
        if (Instance == null)
            return 0f;
        return Instance.viewportRect.x;
    }

    /// <summary>
    /// Get the viewport offset Y (top edge of game area).
    /// Add this to Y coordinates when using Screen.height for positioning.
    /// </summary>
    public static float GetViewportOffsetY()
    {
        if (Instance == null)
            return 0f;
        return Instance.viewportRect.y;
    }

    /// <summary>
    /// Scale a value relative to the reference resolution.
    /// Example: Scale(100) will return 50 if running at 960x540.
    /// </summary>
    public static float Scale(float value)
    {
        return value * GetScaleFactor();
    }

    /// <summary>
    /// Convert a screen position to viewport position (accounting for letterboxing/pillarboxing).
    /// </summary>
    public static Vector2 ScreenToViewport(Vector2 screenPos)
    {
        if (Instance == null)
            return screenPos;

        return new Vector2(
            screenPos.x - Instance.viewportRect.x,
            screenPos.y - Instance.viewportRect.y
        );
    }

    /// <summary>
    /// Convert a viewport position to screen position.
    /// </summary>
    public static Vector2 ViewportToScreen(Vector2 viewportPos)
    {
        if (Instance == null)
            return viewportPos;

        return new Vector2(
            viewportPos.x + Instance.viewportRect.x,
            viewportPos.y + Instance.viewportRect.y
        );
    }

    /// <summary>
    /// Get a Rect centered on screen, accounting for letterboxing/pillarboxing.
    /// </summary>
    public static Rect GetCenteredRect(float width, float height)
    {
        float screenWidth = GetEffectiveScreenWidth();
        float screenHeight = GetEffectiveScreenHeight();
        float offsetX = GetViewportOffsetX();
        float offsetY = GetViewportOffsetY();

        return new Rect(
            offsetX + (screenWidth - width) / 2f,
            offsetY + (screenHeight - height) / 2f,
            width,
            height
        );
    }

    /// <summary>
    /// Get the reference resolution (1920x1080).
    /// </summary>
    public static Vector2 GetReferenceResolution()
    {
        return REFERENCE_RESOLUTION;
    }

    /// <summary>
    /// Get the target aspect ratio (16:9 = 1.777...).
    /// </summary>
    public static float GetTargetAspectRatio()
    {
        return TARGET_ASPECT_RATIO;
    }

    // Debug method to print current resolution info
    [ContextMenu("Print Resolution Info")]
    public void PrintResolutionInfo()
    {
        Debug.Log("=== RESOLUTION INFO ===");
        Debug.Log($"Screen Size: {Screen.width}x{Screen.height}");
        Debug.Log($"Target Resolution: {targetResolution.x}x{targetResolution.y}");
        Debug.Log($"Reference Resolution: {REFERENCE_RESOLUTION.x}x{REFERENCE_RESOLUTION.y}");
        Debug.Log($"Scale Factor: {currentScaleFactor:F2}");
        Debug.Log($"Effective Size: {GetEffectiveScreenWidth():F0}x{GetEffectiveScreenHeight():F0}");
        Debug.Log($"Viewport Offset: ({viewportRect.x:F0}, {viewportRect.y:F0})");
        Debug.Log($"Has Letterboxing: {hasLetterboxing}");
        Debug.Log($"Has Pillarboxing: {hasPillarboxing}");
        Debug.Log($"Current Aspect Ratio: {(Screen.width / (float)Screen.height):F3} (Target: {TARGET_ASPECT_RATIO:F3})");
        Debug.Log("=====================");
    }

    // Public method to change resolution at runtime
    public void SetResolution(int width, int height, bool fullscreen)
    {
        targetResolution = new Vector2Int(width, height);
        allowFullscreen = fullscreen;
        ApplyResolution();

        if (debugMode)
        {
            PrintResolutionInfo();
        }
    }

    // Preset resolutions
    public void SetResolution1080p() => SetResolution(1920, 1080, allowFullscreen);
    public void SetResolution1440p() => SetResolution(2560, 1440, allowFullscreen);
    public void SetResolution720p() => SetResolution(1280, 720, allowFullscreen);
}
