using UnityEngine;

/// <summary>
/// Camera Settings Enforcer - Ensures camera is configured correctly for skybox rendering
/// This script automatically fixes camera settings if they get reset or configured incorrectly.
/// Attach this to the Main Camera or any object in the scene.
/// </summary>
public class CameraSettingsEnforcer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Automatically enforce skybox clear flags")]
    public bool enforceSkyclearFlags = true;

    [Tooltip("Log when settings are corrected")]
    public bool logCorrections = true;

    private Camera mainCamera;
    private bool hasLoggedInitialState = false;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("CameraSettingsEnforcer: No Main Camera found!");
            enabled = false;
            return;
        }

        // Log initial state
        LogCameraState("Initial Camera State");
        hasLoggedInitialState = true;

        // Enforce correct settings on start
        EnforceSettings();
    }

    void Update()
    {
        if (mainCamera == null) return;
        if (!enforceSkyclearFlags) return;

        // Check and fix settings every frame (very cheap check)
        EnforceSettings();
    }

    void EnforceSettings()
    {
        bool needsCorrection = false;

        // Check and fix clear flags
        if (mainCamera.clearFlags != CameraClearFlags.Skybox)
        {
            if (logCorrections)
            {
                Debug.LogWarning($"CameraSettingsEnforcer: Correcting clear flags from {mainCamera.clearFlags} to Skybox");
            }
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            needsCorrection = true;
        }

        // Log if corrections were made
        if (needsCorrection && logCorrections && hasLoggedInitialState)
        {
            LogCameraState("After Correction");
        }
    }

    void LogCameraState(string context)
    {
        if (mainCamera == null) return;

        Debug.Log($"=== {context} ===");
        Debug.Log($"Camera: {mainCamera.name}");
        Debug.Log($"Clear Flags: {mainCamera.clearFlags}");
        Debug.Log($"Culling Mask: {mainCamera.cullingMask}");
        Debug.Log($"Background Color: {mainCamera.backgroundColor}");
        Debug.Log($"Depth: {mainCamera.depth}");
        Debug.Log($"Skybox Material: {RenderSettings.skybox?.name ?? "null"}");
    }

    // Public method to manually trigger a check
    public void CheckAndFix()
    {
        EnforceSettings();
        LogCameraState("Manual Check");
    }
}
