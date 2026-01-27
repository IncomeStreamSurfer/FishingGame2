using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// PERFORMANCE NUCLEAR OPTION
/// This script runs BEFORE everything else and disables all heavy systems.
///
/// HOW TO USE:
/// 1. This script auto-runs before scene loads
/// 2. Check the console for "DISABLED" messages
/// 3. Once FPS is good, you can selectively re-enable systems
///
/// DELETE THIS FILE when done debugging!
/// </summary>
public static class PerformanceNuke
{
    // List of MonoBehaviour types to DISABLE (add more as needed)
    private static readonly string[] SYSTEMS_TO_DISABLE = new string[]
    {
        // Already disabled but make sure
        "BeachCritters",
        "VolumetricLighting",
        "IslandCritters",
        "HorizonBoats",

        // Heavy visual effects
        "WaterfallEffect",
        "SnowParticles",
        "ShootingStars",
        "NightSkyOverlay",
        "BirdFlock",

        // NPCs with complex models
        "SpawnNPC",
        "GoldieBanksNPC",
        "FishConnoisseurNPC",
        "TutorialCat",
        "OrangutanVendor",
        "RenaCumbiaQueen",
        "CandyCat",
        "BjorkHuntsman",
        "PolarBearAI",
        "PolarBearCubPet",
        "ShoulderParrot",
        "SnakeAI",

        // Heavy environment systems
        "CoconutManager",
        "DockTorch",
        "BBQStation",
        "BeachTowel",
        "BottleEventSystem",
        "DockRadio",
        "ShopRadio",

        // Other potentially heavy systems
        "WeedBagCollectible",
        "FloatingIsland",
        "PortalAnimator",

        // Keep CookingFire disabled too
        "CookingFire",
    };

    private static bool hasRun = false;
    private static int disabledCount = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void NukeHeavySystems()
    {
        if (hasRun) return;
        hasRun = true;

        Debug.Log("╔══════════════════════════════════════════════════════════════╗");
        Debug.Log("║          PERFORMANCE NUKE - DISABLING HEAVY SYSTEMS          ║");
        Debug.Log("╚══════════════════════════════════════════════════════════════╝");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void DisableAfterSceneLoad()
    {
        disabledCount = 0;

        // Find and disable all heavy MonoBehaviours
        MonoBehaviour[] allBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            if (behaviour == null) continue;

            string typeName = behaviour.GetType().Name;

            foreach (string systemName in SYSTEMS_TO_DISABLE)
            {
                if (typeName == systemName)
                {
                    behaviour.enabled = false;
                    disabledCount++;
                    Debug.Log($"   [NUKE] DISABLED: {typeName} on {behaviour.gameObject.name}");
                    break;
                }
            }
        }

        Debug.Log("╔══════════════════════════════════════════════════════════════╗");
        Debug.Log($"║   PERFORMANCE NUKE COMPLETE: {disabledCount} systems disabled            ║");
        Debug.Log("╚══════════════════════════════════════════════════════════════╝");

        // Also count remaining primitives
        CountPrimitives();

        // AggressiveCulling DISABLED - was causing player/tree parts to disappear
        // CreateCullingSystem();
    }

    static void CountPrimitives()
    {
        Renderer[] allRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int primitiveCount = 0;

        foreach (var r in allRenderers)
        {
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                string meshName = mf.sharedMesh.name;
                if (meshName.Contains("Cube") || meshName.Contains("Sphere") ||
                    meshName.Contains("Cylinder") || meshName.Contains("Capsule") ||
                    meshName.Contains("Quad") || meshName.Contains("Plane"))
                {
                    primitiveCount++;
                }
            }
        }

        if (primitiveCount > 50)
        {
            Debug.LogWarning($"   [NUKE] ⚠️ Still {primitiveCount} primitives in scene!");
            Debug.LogWarning($"   [NUKE] Some systems created primitives BEFORE we could disable them.");
            Debug.LogWarning($"   [NUKE] Need to modify those scripts directly.");
        }
        else
        {
            Debug.Log($"   [NUKE] ✓ Only {primitiveCount} primitives - acceptable!");
        }
    }
}
