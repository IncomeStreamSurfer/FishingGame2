using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// TEMPORARY DIAGNOSTIC SCRIPT - Delete after finding the bottleneck
/// Place on a GameObject in your scene (or it will auto-create)
/// Shows exactly which systems are slow during startup
/// </summary>
public class StartupProfiler : MonoBehaviour
{
    private static StartupProfiler instance;
    private static Stopwatch totalTimer = new Stopwatch();
    private static bool hasRun = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        if (hasRun) return;
        hasRun = true;

        totalTimer.Start();
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("   STARTUP PROFILER - Tracking initialization times...");
        Debug.Log("═══════════════════════════════════════════════════════════");

        // Create a persistent object to track scene load completion
        GameObject profiler = new GameObject("[StartupProfiler]");
        instance = profiler.AddComponent<StartupProfiler>();
        DontDestroyOnLoad(profiler);
    }

    void Start()
    {
        // This runs after all other Start() methods due to default execution order
        Invoke("ReportResults", 0.1f);
    }

    void ReportResults()
    {
        totalTimer.Stop();

        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log($"   TOTAL STARTUP TIME: {totalTimer.ElapsedMilliseconds} ms ({totalTimer.ElapsedMilliseconds / 1000f:F2} seconds)");
        Debug.Log("═══════════════════════════════════════════════════════════");

        // Count objects in scene
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        var allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        var allColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        Debug.Log($"   GameObjects in scene: {allObjects.Length}");
        Debug.Log($"   Renderers: {allRenderers.Length}");
        Debug.Log($"   Colliders: {allColliders.Length}");
        Debug.Log($"   MonoBehaviours: {allMonoBehaviours.Length}");
        Debug.Log("═══════════════════════════════════════════════════════════");

        // Count primitives (likely culprits)
        int primitiveCount = 0;
        foreach (var r in allRenderers)
        {
            if (r.GetComponent<MeshFilter>() != null)
            {
                var mesh = r.GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null && (mesh.name.Contains("Cube") || mesh.name.Contains("Sphere") ||
                    mesh.name.Contains("Cylinder") || mesh.name.Contains("Capsule") || mesh.name.Contains("Quad")))
                {
                    primitiveCount++;
                }
            }
        }
        Debug.Log($"   PRIMITIVES (CreatePrimitive objects): {primitiveCount}");

        if (primitiveCount > 100)
        {
            Debug.LogWarning($"   ⚠️ HIGH PRIMITIVE COUNT! {primitiveCount} primitives detected.");
            Debug.LogWarning("   This is likely causing your performance issues.");
        }

        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("   Copy these numbers and send to Dave!");
        Debug.Log("═══════════════════════════════════════════════════════════");
    }
}
