using UnityEngine;

public class PortalInteraction : MonoBehaviour
{
    public string portalName = "Unknown Realm";
    public int requiredLevel = 50;
    public float interactionRange = 3f;
    public RealmType destinationRealm = RealmType.TropicalIsland;
    public Vector3 spawnOffset = new Vector3(0, 2f, 5f); // Where player spawns in destination

    private bool isUnlocked = false;
    private bool playerNearby = false;
    private Transform playerTransform;
    private GameObject lockSymbol;

    void Start()
    {
        playerTransform = GameObject.Find("Player")?.transform;
        lockSymbol = transform.Find("LockSymbol")?.gameObject;

        // Subscribe to level up events
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.OnLevelUp += CheckUnlock;
        }

        // Check initial unlock status
        CheckUnlockStatus();
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        playerNearby = distance <= interactionRange;

        // Check for interaction
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryEnterPortal();
        }
    }

    void CheckUnlock(int oldLevel, int newLevel)
    {
        CheckUnlockStatus();
    }

    void CheckUnlockStatus()
    {
        // Auto-unlock portals with level 0 requirement (return portals)
        if (requiredLevel <= 0 && !isUnlocked)
        {
            UnlockPortal();
            return;
        }

        if (LevelingSystem.Instance != null)
        {
            int playerLevel = LevelingSystem.Instance.GetLevel();
            if (playerLevel >= requiredLevel && !isUnlocked)
            {
                UnlockPortal();
            }
        }
    }

    void UnlockPortal()
    {
        isUnlocked = true;
        Debug.Log($"{portalName} has been UNLOCKED! You can now enter!");

        // Hide lock symbol
        if (lockSymbol != null)
        {
            lockSymbol.SetActive(false);
        }

        // Make portal surface more vibrant when unlocked
        Transform portalSurface = transform.Find("PortalSurface");
        if (portalSurface != null)
        {
            Renderer rend = portalSurface.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                c.a = 0.9f;
                rend.material.color = c;
                rend.material.SetColor("_EmissionColor", rend.material.GetColor("_EmissionColor") * 2f);
            }
        }
    }

    void TryEnterPortal()
    {
        if (isUnlocked)
        {
            Debug.Log($"Entering {portalName}...");

            // Use RealmManager to teleport
            if (RealmManager.Instance != null)
            {
                RealmManager.Instance.TravelToRealm(destinationRealm, spawnOffset);
            }
            else
            {
                Debug.LogWarning("RealmManager not found!");
            }
        }
        else
        {
            int playerLevel = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;
            Debug.Log($"{portalName} is LOCKED! Requires level {requiredLevel}. You are level {playerLevel}.");
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !playerNearby) return;

        // Show portal info when nearby
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        // Portal name
        style.normal.textColor = isUnlocked ? Color.green : Color.red;
        GUI.Label(new Rect(Screen.width / 2 - 150, 80, 300, 30), portalName, style);

        // Level requirement
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        int playerLevel = LevelingSystem.Instance != null ? LevelingSystem.Instance.GetLevel() : 1;

        if (isUnlocked)
        {
            style.normal.textColor = Color.cyan;
            GUI.Label(new Rect(Screen.width / 2 - 150, 105, 300, 25), "Press E to Enter", style);
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 2 - 150, 105, 300, 25), $"Required: Level {requiredLevel} (You: {playerLevel})", style);

            // Show lock message
            style.normal.textColor = new Color(1f, 0.6f, 0.6f);
            GUI.Label(new Rect(Screen.width / 2 - 150, 130, 300, 25), "LOCKED", style);
        }
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    void OnDestroy()
    {
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.OnLevelUp -= CheckUnlock;
        }
    }
}
