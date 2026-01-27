using UnityEngine;

/// <summary>
/// IMMEDIATELY disables all non-Tropical realm objects at startup.
/// This runs BEFORE the first frame to prevent any rendering of other realms.
/// Only the main Tropical Island should be active - other realms are disabled.
/// </summary>
public static class DisableOtherRealms
{
    // Realm positions (from RealmManager)
    // TropicalIsland = (0, 0, 0)
    // IceRealm = (500, 0, 0)
    // JungleRealm = (1000, 0, 0)
    // VolcanicRealm = (1500, 0, 0)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void DisableAllOtherRealmObjects()
    {
        Debug.Log("[DisableOtherRealms] Disabling all non-Tropical realm objects...");

        int jungleDisabled = 0;
        int iceDisabled = 0;
        int volcanicDisabled = 0;
        int positionDisabled = 0;

        // Find ALL GameObjects
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;

            string name = obj.name;

            // Disable Jungle realm objects
            if (name.Contains("Jungle") || name.Contains("jungle"))
            {
                obj.SetActive(false);
                jungleDisabled++;
                continue;
            }

            // Disable Ice realm objects (but not things like "Nice" or "Price")
            if (name.StartsWith("Ice") || name.Contains("IceRealm") || name.Contains("_Ice") ||
                name.Contains("Frost") || name.Contains("Snow") || name.Contains("Frozen") ||
                name.Contains("Glacier") || name.Contains("Arctic") || name.Contains("Polar"))
            {
                obj.SetActive(false);
                iceDisabled++;
                continue;
            }

            // Disable Volcanic realm objects
            if (name.Contains("Volcanic") || name.Contains("volcanic") || name.Contains("Lava") ||
                name.Contains("Magma") || name.Contains("Volcano"))
            {
                obj.SetActive(false);
                volcanicDisabled++;
                continue;
            }
        }

        // Disable realm parent objects if they exist
        DisableRealmParent("JungleRealm");
        DisableRealmParent("IceRealm");
        DisableRealmParent("VolcanicRealm");

        // Disable objects by position (any object far from tropical island origin)
        foreach (GameObject obj in allObjects)
        {
            if (obj == null || !obj.activeInHierarchy) continue;

            Vector3 pos = obj.transform.position;

            // Tropical island is at origin, safe zone is roughly -200 to +200
            // Anything beyond that is another realm
            if (pos.x > 400f || pos.x < -200f)
            {
                obj.SetActive(false);
                positionDisabled++;
            }
        }

        Debug.Log($"[DisableOtherRealms] ✓ Disabled realms:");
        Debug.Log($"   - Jungle: {jungleDisabled} objects");
        Debug.Log($"   - Ice: {iceDisabled} objects");
        Debug.Log($"   - Volcanic: {volcanicDisabled} objects");
        Debug.Log($"   - By position: {positionDisabled} objects");
        Debug.Log($"[DisableOtherRealms] Total: {jungleDisabled + iceDisabled + volcanicDisabled + positionDisabled} objects disabled");
        Debug.Log("[DisableOtherRealms] Only Tropical Island will render - FPS saved!");
    }

    static void DisableRealmParent(string realmName)
    {
        GameObject realm = GameObject.Find(realmName);
        if (realm != null)
        {
            realm.SetActive(false);
            Debug.Log($"[DisableOtherRealms] Disabled {realmName} parent object");
        }
    }
}
