using UnityEngine;

/// <summary>
/// Optimizes performance by disabling realm GameObjects when player is not in that realm.
/// Only the current realm is active - all others are disabled to save CPU/GPU cycles.
/// </summary>
public class RealmActivator : MonoBehaviour
{
    public static RealmActivator Instance { get; private set; }

    // Parent objects for each realm
    private GameObject tropicalRealm;
    private GameObject iceRealm;
    private GameObject jungleRealm;
    private GameObject volcanicRealm;

    private RealmType lastRealm = RealmType.TropicalIsland;
    private bool initialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Delay initialization to ensure realms are created
        Invoke("Initialize", 1f);
    }

    void Initialize()
    {
        // Find realm parent objects
        tropicalRealm = GameObject.Find("TropicalIsland");
        iceRealm = GameObject.Find("IceRealm");
        jungleRealm = GameObject.Find("JungleRealm");
        volcanicRealm = GameObject.Find("VolcanicRealm");

        // Log what we found
        Debug.Log($"RealmActivator: Found realms - Tropical:{tropicalRealm != null}, Ice:{iceRealm != null}, Jungle:{jungleRealm != null}, Volcanic:{volcanicRealm != null}");

        // Initial activation based on current realm
        RealmType currentRealm = RealmType.TropicalIsland;
        if (GameCache.Realm != null)
        {
            currentRealm = GameCache.Realm.CurrentRealm;
        }

        ActivateRealm(currentRealm);
        lastRealm = currentRealm;
        initialized = true;

        Debug.Log($"RealmActivator: Initialized! Active realm: {currentRealm}");
    }

    void Update()
    {
        if (!initialized) return;
        if (!MainMenu.GameStarted) return;

        // Check if realm changed
        RealmType current = GameCache.GetCurrentRealm();
        if (current != lastRealm)
        {
            Debug.Log($"RealmActivator: Realm changed from {lastRealm} to {current}");
            ActivateRealm(current);
            lastRealm = current;
        }
    }

    void ActivateRealm(RealmType realm)
    {
        // Disable all realms first
        SetRealmActive(tropicalRealm, false);
        SetRealmActive(iceRealm, false);
        SetRealmActive(jungleRealm, false);
        SetRealmActive(volcanicRealm, false);

        // Enable only the current realm
        switch (realm)
        {
            case RealmType.TropicalIsland:
                SetRealmActive(tropicalRealm, true);
                break;
            case RealmType.IceRealm:
                SetRealmActive(iceRealm, true);
                break;
            case RealmType.JungleRealm:
                SetRealmActive(jungleRealm, true);
                break;
            case RealmType.VolcanicRealm:
                SetRealmActive(volcanicRealm, true);
                break;
        }
    }

    void SetRealmActive(GameObject realm, bool active)
    {
        if (realm != null)
        {
            realm.SetActive(active);
        }
    }

    // Public method to force refresh (useful after teleporting)
    public void RefreshActiveRealm()
    {
        if (!initialized) return;

        RealmType current = GameCache.GetCurrentRealm();
        ActivateRealm(current);
        lastRealm = current;
    }
}
