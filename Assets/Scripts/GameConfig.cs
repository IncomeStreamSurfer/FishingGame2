using UnityEngine;

/// <summary>
/// Game Configuration - Central place for build settings
/// Toggle RELEASE_MODE to true before building for itch.io
/// </summary>
public static class GameConfig
{
    /// <summary>
    /// Set to TRUE for itch.io release builds
    /// Set to FALSE for development/testing
    ///
    /// When TRUE:
    /// - DevPanel (F12) is disabled
    /// - Console Commands (~, F1) are disabled
    /// - Debug overlays are hidden
    /// </summary>
    public const bool RELEASE_MODE = true;  // RELEASE BUILD FOR ITCH.IO

    /// <summary>
    /// Game version shown in menus
    /// </summary>
    public const string VERSION = "1.0.0";

    /// <summary>
    /// Check if dev tools should be enabled
    /// </summary>
    public static bool DevToolsEnabled => !RELEASE_MODE && Application.isEditor || !RELEASE_MODE;
}
