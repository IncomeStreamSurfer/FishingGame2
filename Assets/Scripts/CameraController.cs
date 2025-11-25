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

        // Auto-find player if not assigned
        if (target == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Set initial camera position immediately
        if (target != null)
        {
            Vector3 behindPlayer = -target.forward * cameraDistance;
            Vector3 abovePlayer = Vector3.up * cameraHeight;
            transform.position = target.position + behindPlayer + abovePlayer;
            transform.LookAt(target.position + Vector3.up * lookAtHeight);
        }
    }

    void LateUpdate()
    {
        // Keep trying to find player if not found
        if (target == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                return;
            }
        }

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            cameraDistance -= scroll * zoomSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, minZoom, maxZoom);
        }

        // Camera is LOCKED behind the player - always sees the back of the character
        // Camera position is relative to player's facing direction
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
