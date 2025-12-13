using UnityEngine;

public class PortalInteraction : MonoBehaviour
{
    public string portalName = "Unknown Realm";
    public int requiredLevel = 50;
    public float interactionRange = 3f;
    public RealmType destinationRealm = RealmType.TropicalIsland;
    public Vector3 spawnOffset = new Vector3(0, 2f, 5f); // Where player spawns in destination

    // Beta lock - prevents access regardless of level, shows "COMING SOON"
    public bool betaLocked = false;
    public string betaMessage = "COMING SOON IN FULL RELEASE!";

    private bool isUnlocked = false;
    private bool playerNearby = false;
    private Transform playerTransform;
    private GameObject lockSymbol;
    private int guiFrameSkip = 0;

    void Start()
    {
        if (GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;
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
        // Update player reference if needed
        if (playerTransform == null && GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

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
        // Beta locked portals can never be unlocked
        if (betaLocked)
        {
            isUnlocked = false;
            return;
        }

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
        // Beta locked portals cannot be entered at all
        if (betaLocked)
        {
            Debug.Log($"{portalName} is BETA LOCKED - {betaMessage}");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification(betaMessage, new Color(1f, 0.8f, 0.3f));
            }
            return;
        }

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
        // Performance: Skip frames when not actively needed
        if (!playerNearby)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        if (!MainMenu.GameStarted || !playerNearby) return;

        // Show portal info when nearby
        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        // Beta locked portals show special message
        if (betaLocked)
        {
            // Portal name in orange
            style.normal.textColor = new Color(1f, 0.7f, 0.3f);
            GUI.Label(new Rect(Screen.width / 2 - 150, 80, 300, 30), portalName, style);

            // Coming soon message
            style.fontSize = 16;
            style.normal.textColor = new Color(1f, 0.85f, 0.4f);
            GUI.Label(new Rect(Screen.width / 2 - 150, 110, 300, 25), "COMING SOON!", style);

            // Beta message
            style.fontSize = 12;
            style.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            GUI.Label(new Rect(Screen.width / 2 - 150, 135, 300, 25), betaMessage, style);

            // Beta badge
            style.fontSize = 11;
            style.normal.textColor = new Color(0.3f, 0.7f, 1f);
            GUI.Label(new Rect(Screen.width / 2 - 150, 160, 300, 20), "[BETA VERSION - Tropical Island Only]", style);
            return;
        }

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
