using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 12f;  // Higher jump!
    public LayerMask groundLayer;
    private bool isGrounded = true;

    [Header("Water Death")]
    public float waterDeathHeight = 0.3f;
    private bool isDead = false;
    private bool showDeathScreen = false;

    private Vector3 moveDirection;
    private Rigidbody rb;
    private CameraController cameraController;

    // Cached texture for death screen
    private Texture2D deathOverlayTexture;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraController = Camera.main?.GetComponent<CameraController>();

        // Create death overlay texture once
        deathOverlayTexture = new Texture2D(2, 2);
        Color[] pixels = new Color[4];
        for (int i = 0; i < 4; i++) pixels[i] = new Color(0.6f, 0, 0, 0.7f);
        deathOverlayTexture.SetPixels(pixels);
        deathOverlayTexture.Apply();
    }

    void Update()
    {
        if (isDead) return;

        HandleWoWMovement();
        CheckGrounded();

        // Left mouse button to fish (only starts cast, rod animator handles the rest)
        if (Input.GetMouseButtonDown(0))
        {
            FishingRodAnimator rodAnimator = GetComponent<FishingRodAnimator>();
            // Only start fishing if line is not already out
            if (rodAnimator != null && !rodAnimator.IsLineOut())
            {
                if (FishingSystem.Instance != null && FishingSystem.Instance.CanFish())
                {
                    FishingSystem.Instance.StartFishing();
                }
            }
            // If line is out, the rod animator handles the click for reeling in
        }

        // Space to jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Check if fallen in water
        CheckWaterDeath();
    }

    void HandleWoWMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D for turning
        float vertical = Input.GetAxisRaw("Vertical");     // W/S for forward/back

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        // Locked camera style: A/D always turns character, W/S moves forward/back
        // Camera follows behind, so player always sees character's back

        // Turn left/right with A/D
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            transform.Rotate(Vector3.up, horizontal * rotationSpeed * 12f * Time.deltaTime);
        }

        // Move forward/backward with W/S
        if (Mathf.Abs(vertical) > 0.1f)
        {
            Vector3 moveDir = transform.forward * vertical;
            transform.position += moveDir * currentSpeed * Time.deltaTime;
            moveDirection = moveDir;
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    void CheckGrounded()
    {
        // Raycast down to check if on ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.2f);
    }

    void Jump()
    {
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void CheckWaterDeath()
    {
        // If player falls below water level, they die
        if (transform.position.y < waterDeathHeight && !isDead)
        {
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        isDead = true;
        showDeathScreen = true;
        Debug.Log("THE FISHERMAN DROWNED! He couldn't swim...");

        // Reset player's gold and fish - PERMANENT LOSS
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetOnDeath();
        }

        // Disable movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
        }

        // Sink and spin animation
        float sinkTime = 0f;
        Vector3 startPos = transform.position;

        while (sinkTime < 2f)
        {
            sinkTime += Time.deltaTime;

            // Sink down
            transform.position = new Vector3(
                startPos.x,
                startPos.y - sinkTime * 0.5f,
                startPos.z
            );

            // Spin while sinking
            transform.Rotate(Vector3.up * 180 * Time.deltaTime);
            transform.Rotate(Vector3.right * 90 * Time.deltaTime);

            yield return null;
        }

        // Show death screen for 3 seconds
        yield return new WaitForSeconds(3f);

        // Respawn
        Respawn();
    }

    void Respawn()
    {
        isDead = false;
        showDeathScreen = false;
        transform.position = new Vector3(0, 2f, -5f); // Back on dock, on beach side
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

        Debug.Log("Fisherman respawned! Your gold and fish are gone forever!");
    }

    void OnGUI()
    {
        if (showDeathScreen && deathOverlayTexture != null)
        {
            // Red overlay - using cached texture
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), deathOverlayTexture);

            // Death message
            GUIStyle deathStyle = new GUIStyle(GUI.skin.label);
            deathStyle.fontSize = 48;
            deathStyle.fontStyle = FontStyle.Bold;
            deathStyle.alignment = TextAnchor.MiddleCenter;
            deathStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(0, Screen.height / 2 - 80, Screen.width, 60), "YOU CAN'T SWIM!", deathStyle);

            deathStyle.fontSize = 36;
            deathStyle.normal.textColor = new Color(1f, 0.8f, 0.8f);
            GUI.Label(new Rect(0, Screen.height / 2 - 20, Screen.width, 50), "You're Dead!", deathStyle);

            deathStyle.fontSize = 20;
            deathStyle.normal.textColor = new Color(1f, 0.6f, 0.6f);
            GUI.Label(new Rect(0, Screen.height / 2 + 40, Screen.width, 30), "Your gold and fish have been lost forever...", deathStyle);
        }
    }

    void OnDestroy()
    {
        if (deathOverlayTexture != null)
        {
            Destroy(deathOverlayTexture);
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}
