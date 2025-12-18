using UnityEngine;
using UnityEditor;

/// <summary>
/// Automatically sets up the NightSkyOverlay component in the scene
/// Runs when Unity loads or when scripts are recompiled
/// </summary>
[InitializeOnLoad]
public class NightSkyOverlaySetup
{
    static NightSkyOverlaySetup()
    {
        EditorApplication.delayCall += SetupNightSkyOverlay;
    }

    static void SetupNightSkyOverlay()
    {
        // Only run in play mode or when a scene is loaded
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "") return;

        // Check if NightSkyOverlay already exists
        NightSkyOverlay existingOverlay = Object.FindObjectOfType<NightSkyOverlay>();
        if (existingOverlay != null)
        {
            Debug.Log("NightSkyOverlaySetup: NightSkyOverlay already exists in scene");
            return;
        }

        // Find DayNightCycle
        DayNightCycle dayNightCycle = Object.FindObjectOfType<DayNightCycle>();
        if (dayNightCycle == null)
        {
            Debug.LogWarning("NightSkyOverlaySetup: DayNightCycle not found in scene. NightSkyOverlay requires DayNightCycle to function.");
            return;
        }

        // Create NightSkyOverlay GameObject
        GameObject overlayObject = new GameObject("NightSkyOverlay");
        NightSkyOverlay overlay = overlayObject.AddComponent<NightSkyOverlay>();

        // Configure overlay settings
        overlay.dayNightCycle = dayNightCycle;
        overlay.domeDistance = 150f;
        overlay.nightBlackColor = new Color(0f, 0f, 0f, 1f);
        overlay.sunriseStartHour = 6f;
        overlay.sunriseEndHour = 8f;
        overlay.sunsetStartHour = 18f;
        overlay.sunsetEndHour = 20f;

        // Parent to DayNightCycle for organization
        overlayObject.transform.SetParent(dayNightCycle.transform);
        overlayObject.transform.localPosition = Vector3.zero;

        // Mark scene as dirty so Unity saves the changes
        EditorUtility.SetDirty(overlayObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        Debug.Log("NightSkyOverlaySetup: Successfully created and configured NightSkyOverlay component!");
        Debug.Log("The night sky will now be TRULY BLACK during nighttime (8 PM - 6 AM)");
    }

    [MenuItem("Tools/Sky System/Add Night Sky Overlay")]
    public static void AddNightSkyOverlayMenuItem()
    {
        // Manual setup through menu
        NightSkyOverlay existingOverlay = Object.FindObjectOfType<NightSkyOverlay>();
        if (existingOverlay != null)
        {
            Debug.LogWarning("NightSkyOverlay already exists in the scene!");
            Selection.activeGameObject = existingOverlay.gameObject;
            return;
        }

        DayNightCycle dayNightCycle = Object.FindObjectOfType<DayNightCycle>();
        if (dayNightCycle == null)
        {
            EditorUtility.DisplayDialog(
                "DayNightCycle Required",
                "NightSkyOverlay requires a DayNightCycle component in the scene. Please add DayNightCycle first.",
                "OK"
            );
            return;
        }

        GameObject overlayObject = new GameObject("NightSkyOverlay");
        NightSkyOverlay overlay = overlayObject.AddComponent<NightSkyOverlay>();
        overlay.dayNightCycle = dayNightCycle;
        overlayObject.transform.SetParent(dayNightCycle.transform);

        Selection.activeGameObject = overlayObject;
        Undo.RegisterCreatedObjectUndo(overlayObject, "Create Night Sky Overlay");

        Debug.Log("Night Sky Overlay created successfully!");
    }

    [MenuItem("Tools/Sky System/Remove Night Sky Overlay")]
    public static void RemoveNightSkyOverlayMenuItem()
    {
        NightSkyOverlay overlay = Object.FindObjectOfType<NightSkyOverlay>();
        if (overlay == null)
        {
            Debug.LogWarning("No NightSkyOverlay found in scene");
            return;
        }

        if (EditorUtility.DisplayDialog(
            "Remove Night Sky Overlay",
            "Are you sure you want to remove the Night Sky Overlay? The sky will revert to blue at night.",
            "Remove",
            "Cancel"))
        {
            Undo.DestroyObjectImmediate(overlay.gameObject);
            Debug.Log("Night Sky Overlay removed");
        }
    }
}
