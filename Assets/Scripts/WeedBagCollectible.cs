using UnityEngine;

/// <summary>
/// Hidden weed bag collectible for Goldie Banks' quest
/// Glows green, can be picked up when player gets close
/// </summary>
public class WeedBagCollectible : MonoBehaviour
{
    private bool collected = false;
    private float interactionDistance = 2f;
    private float bobTimer = 0f;
    private Vector3 startPos;
    private GameObject glowEffect;

    void Start()
    {
        startPos = transform.position;
        CreateVisual();
    }

    void CreateVisual()
    {
        // Bag
        Material bagMat = new Material(Shader.Find("Standard"));
        bagMat.color = new Color(0.4f, 0.3f, 0.2f); // Brown bag

        GameObject bag = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bag.transform.SetParent(transform);
        bag.transform.localPosition = Vector3.zero;
        bag.transform.localScale = new Vector3(0.25f, 0.35f, 0.15f);
        bag.GetComponent<Renderer>().sharedMaterial = bagMat;
        Object.Destroy(bag.GetComponent<Collider>());

        // Green glow effect
        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.color = new Color(0.2f, 0.8f, 0.3f, 0.3f);
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 0.2f) * 2f);

        glowEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glowEffect.transform.SetParent(transform);
        glowEffect.transform.localPosition = Vector3.zero;
        glowEffect.transform.localScale = Vector3.one * 0.5f;
        glowEffect.GetComponent<Renderer>().sharedMaterial = glowMat;
        Object.Destroy(glowEffect.GetComponent<Collider>());
    }

    void Update()
    {
        if (!MainMenu.GameStarted || collected) return;

        // Bob up and down
        bobTimer += Time.deltaTime;
        transform.position = startPos + Vector3.up * Mathf.Sin(bobTimer * 2f) * 0.1f;

        // Rotate
        transform.Rotate(Vector3.up * 30f * Time.deltaTime);

        // Pulse glow
        if (glowEffect != null)
        {
            float pulse = 0.4f + Mathf.Sin(bobTimer * 3f) * 0.15f;
            glowEffect.transform.localScale = Vector3.one * pulse;
        }

        // Check for player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < interactionDistance)
            {
                // Show prompt
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Collect();
                }
            }
        }
    }

    void Collect()
    {
        collected = true;

        // Notify Goldie Banks
        if (GoldieBanksNPC.Instance != null)
        {
            GoldieBanksNPC.Instance.OnWeedBagFound();
        }

        // Play sound and show notification
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Found Goldie's Bag! Return to him.", new Color(0.4f, 0.8f, 0.3f));
        }

        // Destroy the bag
        Destroy(gameObject);
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || collected) return;

        // Check if Goldie's quest is active
        if (GoldieBanksNPC.Instance == null || !GoldieBanksNPC.Instance.questActive) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < interactionDistance)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 16;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.4f, 0.9f, 0.4f);

            GUI.Label(new Rect(0, Screen.height * 0.6f, Screen.width, 30), "Press E to pick up Goldie's Bag", promptStyle);
        }
    }
}
