using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 8, -10);
    public float smoothSpeed = 5f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 3f;
    public float minZoom = 4f;
    public float maxZoom = 20f;
    private float currentZoom = 10f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 3f;
    public float minVerticalAngle = 10f;
    public float maxVerticalAngle = 80f;

    private float currentYaw = 0f;      // Horizontal rotation
    private float currentPitch = 35f;   // Vertical rotation (angle from horizontal)

    void Start()
    {
        currentZoom = offset.magnitude;

        // Initialize rotation from current offset
        Vector3 flatOffset = new Vector3(offset.x, 0, offset.z);
        if (flatOffset.magnitude > 0.01f)
        {
            currentYaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        }
        currentPitch = Mathf.Clamp(35f, minVerticalAngle, maxVerticalAngle);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // Right-click to rotate camera 360 degrees around player
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentYaw += mouseX * rotationSpeed;
            currentPitch -= mouseY * rotationSpeed;
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        }

        // Calculate camera position based on spherical coordinates
        float pitchRad = currentPitch * Mathf.Deg2Rad;
        float yawRad = currentYaw * Mathf.Deg2Rad;

        // Spherical to Cartesian conversion
        float horizontalDist = currentZoom * Mathf.Cos(pitchRad);
        float verticalDist = currentZoom * Mathf.Sin(pitchRad);

        Vector3 cameraOffset = new Vector3(
            horizontalDist * Mathf.Sin(yawRad),
            verticalDist,
            horizontalDist * Mathf.Cos(yawRad)
        );

        Vector3 desiredPosition = target.position + cameraOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }
}
