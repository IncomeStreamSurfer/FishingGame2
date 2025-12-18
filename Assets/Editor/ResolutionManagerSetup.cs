using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to quickly add ResolutionManager to the current scene.
/// Menu: Tools > Setup Resolution Manager
/// </summary>
public class ResolutionManagerSetup : Editor
{
    [MenuItem("Tools/Setup Resolution Manager")]
    static void SetupResolutionManager()
    {
        // Check if ResolutionManager already exists in scene
        ResolutionManager existing = FindObjectOfType<ResolutionManager>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog(
                "Resolution Manager Already Exists",
                "A ResolutionManager is already present in this scene.\n\n" +
                "GameObject: " + existing.gameObject.name,
                "OK"
            );
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Create new GameObject with ResolutionManager
        GameObject resManagerObj = new GameObject("ResolutionManager");
        ResolutionManager resManager = resManagerObj.AddComponent<ResolutionManager>();

        // Set default values
        resManager.targetResolution = new Vector2Int(1920, 1080);
        resManager.enforceAspectRatio = true;
        resManager.allowFullscreen = true;
        resManager.debugMode = false;

        // Select it in hierarchy
        Selection.activeGameObject = resManagerObj;

        // Show success dialog
        bool openGuide = EditorUtility.DisplayDialog(
            "Resolution Manager Setup Complete",
            "ResolutionManager has been added to the scene!\n\n" +
            "Default Settings:\n" +
            "  • Resolution: 1920x1080 (16:9)\n" +
            "  • Aspect Ratio: Enforced with letterboxing\n" +
            "  • Fullscreen: Enabled\n\n" +
            "The ResolutionManager will persist across scenes (DontDestroyOnLoad).\n\n" +
            "Would you like to open the full integration guide?",
            "Open Guide",
            "Close"
        );

        if (openGuide)
        {
            string guidePath = Application.dataPath + "/../RESOLUTION_GUIDE.md";
            if (System.IO.File.Exists(guidePath))
            {
                System.Diagnostics.Process.Start(guidePath);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Guide Not Found",
                    "Could not find RESOLUTION_GUIDE.md in the project root.\n\n" +
                    "Please see ResolutionManager.cs and ResolutionManagerExample.cs for usage instructions.",
                    "OK"
                );
            }
        }

        Debug.Log("[ResolutionManagerSetup] Added ResolutionManager to scene with default 1920x1080 resolution.");
    }

    [MenuItem("Tools/Resolution Manager Info")]
    static void ShowResolutionInfo()
    {
        ResolutionManager rm = FindObjectOfType<ResolutionManager>();
        if (rm == null)
        {
            EditorUtility.DisplayDialog(
                "No Resolution Manager Found",
                "ResolutionManager is not present in the current scene.\n\n" +
                "Use 'Tools > Setup Resolution Manager' to add it.",
                "OK"
            );
            return;
        }

        string info = "=== RESOLUTION MANAGER INFO ===\n\n";
        info += $"Current Screen: {Screen.width}x{Screen.height}\n";
        info += $"Target Resolution: {rm.targetResolution.x}x{rm.targetResolution.y}\n";
        info += $"Enforce Aspect Ratio: {rm.enforceAspectRatio}\n";
        info += $"Allow Fullscreen: {rm.allowFullscreen}\n";
        info += $"Debug Mode: {rm.debugMode}\n\n";
        info += $"Reference Resolution: {ResolutionManager.REFERENCE_RESOLUTION.x}x{ResolutionManager.REFERENCE_RESOLUTION.y}\n";
        info += $"Target Aspect Ratio: {ResolutionManager.TARGET_ASPECT_RATIO:F3} (16:9)\n\n";

        if (Application.isPlaying)
        {
            info += $"Scale Factor: {ResolutionManager.GetScaleFactor():F2}\n";
            info += $"Effective Size: {ResolutionManager.GetEffectiveScreenWidth():F0}x{ResolutionManager.GetEffectiveScreenHeight():F0}\n";
            info += $"Viewport Offset: ({ResolutionManager.GetViewportOffsetX():F0}, {ResolutionManager.GetViewportOffsetY():F0})\n";
        }
        else
        {
            info += "(Enter Play Mode to see runtime info)";
        }

        EditorUtility.DisplayDialog("Resolution Manager Info", info, "OK");
        Debug.Log(info);
    }

    [MenuItem("Tools/Test Resolutions/1920x1080 (Full HD)")]
    static void TestResolution1080p()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Not Playing", "You must be in Play Mode to test resolutions.", "OK");
            return;
        }

        ResolutionManager rm = FindObjectOfType<ResolutionManager>();
        if (rm != null)
        {
            rm.SetResolution1080p();
            Debug.Log("[ResolutionTest] Set to 1920x1080");
        }
    }

    [MenuItem("Tools/Test Resolutions/2560x1440 (2K)")]
    static void TestResolution1440p()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Not Playing", "You must be in Play Mode to test resolutions.", "OK");
            return;
        }

        ResolutionManager rm = FindObjectOfType<ResolutionManager>();
        if (rm != null)
        {
            rm.SetResolution1440p();
            Debug.Log("[ResolutionTest] Set to 2560x1440");
        }
    }

    [MenuItem("Tools/Test Resolutions/1280x720 (HD)")]
    static void TestResolution720p()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Not Playing", "You must be in Play Mode to test resolutions.", "OK");
            return;
        }

        ResolutionManager rm = FindObjectOfType<ResolutionManager>();
        if (rm != null)
        {
            rm.SetResolution720p();
            Debug.Log("[ResolutionTest] Set to 1280x720");
        }
    }

    [MenuItem("Tools/Test Resolutions/Print Debug Info")]
    static void PrintDebugInfo()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Not Playing", "You must be in Play Mode to see debug info.", "OK");
            return;
        }

        ResolutionManager rm = FindObjectOfType<ResolutionManager>();
        if (rm != null)
        {
            rm.PrintResolutionInfo();
        }
        else
        {
            Debug.LogError("[ResolutionTest] No ResolutionManager found in scene!");
        }
    }
}
