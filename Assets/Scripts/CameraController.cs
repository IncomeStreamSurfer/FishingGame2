using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 10f;

    [Header("Camera Position")]
    public float cameraDistance = 8f;    // Distance behind player
    public float cameraHeight = 4f;      // Height above player
    public float lookAtHeight = 1.2f;    // Where camera looks at on player

    [Header("Zoom Settings")]
    public float zoomSpeed = 3f;
    public float minZoom = 4f;
    public float maxZoom = 15f;

    [Header("Orbit Camera Settings")]
    public float orbitSensitivity = 5f;
    public float minPitch = -10f;
    public float maxPitch = 80f;

    // ============================================================
    // LIE DOWN CAMERA FIX - Keeps camera COMPLETELY FROZEN when lying down
    // To REVERT: Delete lines marked with "// LIE DOWN FIX" and
    // remove the lying down check in LateUpdate
    // ============================================================
    private Vector3 lastStandingForward = Vector3.forward; // LIE DOWN FIX
    private Vector3 frozenCameraPosition; // LIE DOWN FIX
    private Quaternion frozenCameraRotation; // LIE DOWN FIX
    private bool wasLyingDown = false; // LIE DOWN FIX
    private PlayerController playerController; // LIE DOWN FIX

    // Orbit camera variables
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private bool isOrbiting = false;

    // Camera is locked behind player - follows player's rotation
    public float GetCurrentYaw()
    {
        if (target != null)
        {
            return target.eulerAngles.y;
        }
        return 0f;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Auto-find player if not assigned using cached reference
        if (target == null)
        {
            if (GameCache.IsPlayerValid())
            {
                target = GameCache.Player;
                playerController = target.GetComponent<PlayerController>(); // LIE DOWN FIX
            }
        }

        // Set initial camera position immediately
        if (target != null)
        {
            lastStandingForward = target.forward; // LIE DOWN FIX

            // Initialize orbit angles to match player's current rotation
            currentYaw = target.eulerAngles.y;
            currentPitch = 20f; // Start with a slight downward angle

            Vector3 behindPlayer = -target.forward * cameraDistance;
            Vector3 abovePlayer = Vector3.up * cameraHeight;
            transform.position = target.position + behindPlayer + abovePlayer;
            transform.LookAt(target.position + Vector3.up * lookAtHeight);
        }
    }

    // LIE DOWN FIX - Returns stored forward when lying down, otherwise current forward
    Vector3 GetCameraForward()
    {
        if (playerController != null && playerController.IsLyingDown())
        {
            // When lying down, use the last standing forward direction
            return lastStandingForward;
        }
        else
        {
            // When standing, store and use current forward
            lastStandingForward = target.forward;
            return target.forward;
        }
    }

    void LateUpdate()
    {
        // Keep trying to find player if not found using cached reference
        if (target == null)
        {
            if (GameCache.IsPlayerValid())
            {
                target = GameCache.Player;
                playerController = target.GetComponent<PlayerController>(); // LIE DOWN FIX
            }
            else
            {
                return;
            }
        }

        // LIE DOWN FIX - Completely freeze camera when lying down
        bool isLyingDown = playerController != null && playerController.IsLyingDown();

        if (isLyingDown)
        {
            // Just started lying down - save current camera state
            if (!wasLyingDown)
            {
                frozenCameraPosition = transform.position;
                frozenCameraRotation = transform.rotation;
                lastStandingForward = target.forward;
                wasLyingDown = true;
            }

            // Keep camera completely frozen
            transform.position = frozenCameraPosition;
            transform.rotation = frozenCameraRotation;
            return; // Skip all other camera updates
        }
        else
        {
            wasLyingDown = false;
        }

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            cameraDistance -= scroll * zoomSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, minZoom, maxZoom);
        }

        // Camera always stays behind player (no orbit mode)
        Vector3 behindPlayer = -target.forward * cameraDistance;
        Vector3 abovePlayer = Vector3.up * cameraHeight;
        Vector3 desiredPosition = target.position + behindPlayer + abovePlayer;

        Vector3 lookAtPoint = target.position + Vector3.up * lookAtHeight;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(lookAtPoint);
    }

    public void SetYaw(float yaw)
    {
        // Not used in locked camera mode, but kept for compatibility
    }
}
