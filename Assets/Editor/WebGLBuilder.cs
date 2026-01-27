using UnityEditor;
using UnityEngine;
using System.Linq;

public class WebGLBuilder
{
    [MenuItem("Build/Build WebGL")]
    public static void BuildWebGL()
    {
        PerformBuild();
    }

    public static void PerformBuild()
    {
        Debug.Log("Starting WebGL Build with performance optimizations...");

        // Set WebGL-specific performance settings
        PlayerSettings.WebGL.memorySize = 512; // Increase memory for smoother performance
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;

        // Set quality for WebGL - use lower settings for better FPS
        QualitySettings.SetQualityLevel(1); // Medium quality
        QualitySettings.vSyncCount = 0; // Disable VSync for higher FPS
        QualitySettings.antiAliasing = 0; // Disable AA for performance
        QualitySettings.shadows = ShadowQuality.Disable; // Disable shadows
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;

        Debug.Log("Applied WebGL performance settings: Shadows OFF, AA OFF, VSync OFF");

        // Get all scenes in build settings
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            // If no scenes in build settings, find all scenes in Assets
            scenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .ToArray();

            if (scenes.Length == 0)
            {
                // Last resort - find any scene
                scenes = AssetDatabase.FindAssets("t:Scene")
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Take(1)
                    .ToArray();
            }
        }

        Debug.Log($"Building with {scenes.Length} scene(s): {string.Join(", ", scenes)}");

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "WebGLBuild",
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"WebGL Build completed successfully! Output: WebGLBuild/");
            Debug.Log($"Build size: {report.summary.totalSize / (1024 * 1024)} MB");
        }
        else
        {
            Debug.LogError($"WebGL Build failed: {report.summary.result}");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == UnityEngine.LogType.Error)
                        Debug.LogError(msg.content);
                }
            }
        }
    }
}
