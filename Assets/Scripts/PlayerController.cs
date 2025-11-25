using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public LayerMask groundLayer;
    private bool isGrounded = true;

    [Header("Water Death")]
    public float waterDeathHeight = 0.3f;
    private bool isDead = false;

    private Vector3 moveDirection;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleRotation();
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

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    void HandleRotation()
    {
        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
        Debug.Log("THE FISHERMAN DROWNED! He couldn't swim...");

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

        // Wait a moment
        yield return new WaitForSeconds(1f);

        // Respawn
        Respawn();
    }

    void Respawn()
    {
        isDead = false;
        transform.position = new Vector3(0, 2f, 5f); // Back on dock
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

        Debug.Log("Fisherman respawned! Be more careful near the water!");
    }

    public bool IsDead()
    {
        return isDead;
    }
}
