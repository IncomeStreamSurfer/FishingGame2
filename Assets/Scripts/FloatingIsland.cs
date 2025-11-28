using UnityEngine;

/// <summary>
/// Makes a floating island bob gently and drift slowly
/// Attach to any island GameObject for ambient movement
/// </summary>
public class FloatingIsland : MonoBehaviour
{
    [Header("Bobbing Motion")]
    public float bobHeight = 0.3f;        // How high it bobs
    public float bobSpeed = 0.8f;         // How fast it bobs
    public float bobPhase = 0f;           // Random offset for variation

    [Header("Drifting Motion")]
    public float driftRadius = 2f;        // How far it drifts
    public float driftSpeed = 0.15f;      // How fast it drifts

    [Header("Rotation")]
    public float rotateSpeed = 5f;        // Degrees per second

    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        startPosition = transform.position;
        timeOffset = bobPhase + Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float time = Time.time + timeOffset;

        // Bobbing up and down
        float bobOffset = Mathf.Sin(time * bobSpeed) * bobHeight;

        // Drifting in a figure-8 pattern
        float driftX = Mathf.Sin(time * driftSpeed) * driftRadius;
        float driftZ = Mathf.Sin(time * driftSpeed * 2f) * driftRadius * 0.5f;

        // Apply movement
        transform.position = startPosition + new Vector3(driftX, bobOffset, driftZ);

        // Gentle rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
