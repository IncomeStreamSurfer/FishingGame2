using UnityEngine;

/// <summary>
/// Sky Diagnostics - Press F9 to log camera and sky rendering status
/// This helps diagnose why the sky appears differently in Scene vs Game view
/// </summary>
public class SkyDiagnostics : MonoBehaviour
{
    void Awake()
    {
        // Disable in release mode
        if (GameConfig.RELEASE_MODE)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (GameConfig.RELEASE_MODE) return;

        // Press F9 to log diagnostics
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LogDiagnostics();
        }
    }

    void LogDiagnostics()
    {
        Debug.Log("=== SKY DIAGNOSTICS ===");

        // Camera diagnostics
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"Main Camera: {mainCam.name}");
            Debug.Log($"  Clear Flags: {mainCam.clearFlags}");
            Debug.Log($"  Culling Mask: {LayerMaskToString(mainCam.cullingMask)}");
            Debug.Log($"  Background Color: {mainCam.backgroundColor}");
            Debug.Log($"  Position: {mainCam.transform.position}");
            Debug.Log($"  Rotation: {mainCam.transform.eulerAngles}");
        }
        else
        {
            Debug.LogError("No Main Camera found!");
        }

        // Render Settings
        Debug.Log($"RenderSettings.skybox: {RenderSettings.skybox?.name ?? "null"}");
        Debug.Log($"RenderSettings.ambientMode: {RenderSettings.ambientMode}");
        Debug.Log($"RenderSettings.ambientIntensity: {RenderSettings.ambientIntensity}");
        Debug.Log($"RenderSettings.ambientLight: {RenderSettings.ambientLight}");
        Debug.Log($"RenderSettings.fog: {RenderSettings.fog}");

        // SkyboxManager
        if (SkyboxManager.Instance != null)
        {
            Debug.Log($"SkyboxManager exists: useProceduralSkybox={SkyboxManager.Instance.useProceduralSkybox}");
            Debug.Log($"  Stars: {SkyboxManager.Instance.enableStars}");
        }
        else
        {
            Debug.LogWarning("SkyboxManager not found!");
        }

        // NightSkyOverlay
        if (NightSkyOverlay.Instance != null)
        {
            Debug.Log($"NightSkyOverlay exists: Alpha={NightSkyOverlay.Instance.GetNightOverlayAlpha():F2}");
        }
        else
        {
            Debug.LogWarning("NightSkyOverlay not found!");
        }

        // DayNightCycle
        DayNightCycle dayNight = FindObjectOfType<DayNightCycle>();
        if (dayNight != null)
        {
            Debug.Log($"DayNightCycle: Hour={dayNight.GetCurrentHour():F1}, Daylight={dayNight.GetDaylightIntensity():F2}");
        }
        else
        {
            Debug.LogWarning("DayNightCycle not found!");
        }

        // Check for sky-related objects
        GameObject[] skyObjects = GameObject.FindGameObjectsWithTag("Untagged");
        int skyCount = 0;
        foreach (var obj in skyObjects)
        {
            if (obj.name.Contains("Sky") || obj.name.Contains("Star") || obj.name.Contains("Moon") ||
                obj.name.Contains("Cloud") || obj.name.Contains("NightSky"))
            {
                skyCount++;
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Debug.Log($"  Sky Object: {obj.name}, Active={obj.activeSelf}, " +
                             $"Layer={LayerMask.LayerToName(obj.layer)}, " +
                             $"RenderQueue={renderer.material?.renderQueue ?? 0}");
                }
            }
        }
        Debug.Log($"Found {skyCount} sky-related objects");

        Debug.Log("=== END DIAGNOSTICS ===");
    }

    string LayerMaskToString(int layerMask)
    {
        if (layerMask == -1) return "Everything";
        if (layerMask == 0) return "Nothing";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < 32; i++)
        {
            if ((layerMask & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(layerName);
                }
            }
        }
        return sb.ToString();
    }
}
