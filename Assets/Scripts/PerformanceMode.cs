using UnityEngine;

/// <summary>
/// GLOBAL PERFORMANCE MODE FLAG
///
/// Set PERFORMANCE_MODE = true to disable all heavy systems.
/// All scripts should check this flag before creating primitives.
///
/// This is checked at the START of Start() methods, before any work is done.
/// </summary>
public static class PerformanceMode
{
    // ═══════════════════════════════════════════════════════════════
    // SET THIS TO TRUE TO DISABLE ALL HEAVY VISUAL SYSTEMS
    // ═══════════════════════════════════════════════════════════════
    public const bool ENABLED = true;  // <-- CHANGE TO false TO RE-ENABLE SYSTEMS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Call this at the start of any heavy system's Start() method.
    /// Returns true if the system should skip initialization.
    /// </summary>
    public static bool ShouldSkip(MonoBehaviour script)
    {
        if (ENABLED)
        {
            Debug.Log($"[PERF] Skipping heavy system: {script.GetType().Name}");
            script.enabled = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Quick check without disabling.
    /// </summary>
    public static bool IsEnabled => ENABLED;
}
