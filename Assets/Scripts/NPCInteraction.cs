using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public static NPCInteraction Instance { get; private set; }

    public float interactionRange = 3f;
    public string npcName = "Old Captain";

    private bool isDialogOpen = false;
    private GameObject questMarker;

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
        // Find quest marker child object
        questMarker = transform.Find("QuestMarker")?.gameObject;

        // Add a collider for click detection if not present
        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.height = 2f;
            col.radius = 0.5f;
            col.center = new Vector3(0, 0, 0);
        }
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Update quest marker visibility
        UpdateQuestMarker();

        // Check for left click on NPC
        if (Input.GetMouseButtonDown(0) && !isDialogOpen)
        {
            CheckForNPCClick();
        }

        // Close dialog with Escape
        if (Input.GetKeyDown(KeyCode.Escape) && isDialogOpen)
        {
            CloseDialog();
        }
    }

    void UpdateQuestMarker()
    {
        if (questMarker == null) return;

        // Show marker if there's a pending quest, hide if player has active quest
        bool showMarker = QuestSystem.Instance != null &&
                         QuestSystem.Instance.HasPendingQuest() &&
                         !QuestSystem.Instance.HasActiveQuest();

        questMarker.SetActive(showMarker);

        // Bobbing animation for quest marker
        if (showMarker)
        {
            float bob = Mathf.Sin(Time.time * 3f) * 0.1f;
            questMarker.transform.localPosition = new Vector3(0, 1.3f + bob, 0);

            // Also animate the stick below it
            Transform stick = transform.Find("QuestMarker")?.GetComponent<Transform>();
            // Rotation for visibility
            questMarker.transform.Rotate(Vector3.up * 60 * Time.deltaTime);
        }
    }

    void CheckForNPCClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Check if we clicked on this NPC
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                // Check if player is close enough
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    float distance = Vector3.Distance(player.transform.position, transform.position);
                    if (distance <= interactionRange)
                    {
                        OpenDialog();
                    }
                    else
                    {
                        Debug.Log("Too far from NPC. Get closer!");
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.ShowLootNotification("Get closer to the NPC!", new Color(1f, 0.8f, 0.3f));
                        }
                    }
                }
            }
        }
    }

    public void OpenDialog()
    {
        isDialogOpen = true;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenNPCDialog(npcName);
        }
        Debug.Log($"Opened dialog with {npcName}");
    }

    public void CloseDialog()
    {
        isDialogOpen = false;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseNPCDialog();
        }
    }

    public bool IsDialogOpen()
    {
        return isDialogOpen;
    }

    // Visual indicator when player is in range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
