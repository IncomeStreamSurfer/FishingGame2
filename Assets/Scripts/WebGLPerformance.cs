using UnityEngine;

/// <summary>
/// WebGL Performance Manager - Reduces OnGUI overhead
/// All OnGUI scripts should check ShouldRenderGUI() before drawing
/// </summary>
public class WebGLPerformance : MonoBehaviour
{
    public static WebGLPerformance Instance { get; private set; }

    // Frame skipping for OnGUI
    private static int frameCounter = 0;
    private static int guiSkipFrames = 2; // Only render GUI every N frames in WebGL

    // Check if running in WebGL
    public static bool IsWebGL { get; private set; }

    // Target FPS
    public static int TargetFPS = 30;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            #if UNITY_WEBGL
            IsWebGL = true;
            guiSkipFrames = 3; // Skip more frames in WebGL
            Application.targetFrameRate = TargetFPS;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.antiAliasing = 0;
            Debug.Log("[WebGLPerformance] WebGL mode - GUI skip: " + guiSkipFrames + ", Target FPS: " + TargetFPS);
            #else
            IsWebGL = false;
            guiSkipFrames = 1; // Normal rendering in editor/standalone
            #endif
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        frameCounter++;
    }

    /// <summary>
    /// Check if GUI should be rendered this frame
    /// Call at the start of OnGUI() and return early if false
    /// </summary>
    public static bool ShouldRenderGUI()
    {
        return frameCounter % guiSkipFrames == 0;
    }

    /// <summary>
    /// For critical UI that must always render (popups, menus)
    /// </summary>
    public static bool ShouldRenderCriticalGUI()
    {
        return frameCounter % 2 == 0; // Still skip every other frame
    }

    /// <summary>
    /// For non-essential UI that can skip more frames
    /// </summary>
    public static bool ShouldRenderLowPriorityGUI()
    {
        return frameCounter % 4 == 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("WebGLPerformance");
            go.AddComponent<WebGLPerformance>();
        }
    }
}
