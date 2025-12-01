using UnityEngine;
using System.Collections;

public class VoidWanderer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float patrolRadius = 40f;
    [SerializeField] private float minPauseDuration = 2f;
    [SerializeField] private float maxPauseDuration = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float destinationThreshold = 0.5f;

    [Header("Idle Animation")]
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.1f;

    [Header("Avoidance")]
    [SerializeField] private float avoidanceCheckRadius = 2f;
    [SerializeField] private LayerMask obstacleLayer;

    private Vector3 spawnPoint;
    private Vector3 currentDestination;
    private bool isWalking = false;
    private bool isPaused = false;
    private float initialY;
    private Transform bodyTransform;

    // Dancing state
    private bool isDancing = false;
    private Vector3 danceTarget;
    private float danceTime = 0f;
    private Coroutine wanderCoroutine;

    // Visual components
    private GameObject visualModel;
    private string[] mohawkColors = { "pink", "green", "blue", "orange" };

    void Start()
    {
        spawnPoint = transform.position;
        initialY = transform.position.y;
        CreatePunkModel();
        wanderCoroutine = StartCoroutine(WanderRoutine());
    }

    public void StartDancing(Vector3 stagePosition)
    {
        if (isDancing) return;

        isDancing = true;
        danceTime = 0f;

        // Stop wandering
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }

        // Set dance target near the stage (with some random offset)
        Vector2 offset = Random.insideUnitCircle * 8f;
        danceTarget = stagePosition + new Vector3(offset.x, 0, 5f + Mathf.Abs(offset.y));
        danceTarget.y = initialY;

        isWalking = true;
        isPaused = false;
    }

    public void StopDancing()
    {
        if (!isDancing) return;

        isDancing = false;

        // Resume wandering
        if (wanderCoroutine == null)
        {
            wanderCoroutine = StartCoroutine(WanderRoutine());
        }
    }

    void CreatePunkModel()
    {
        visualModel = new GameObject("PunkModel");
        visualModel.transform.SetParent(transform);
        visualModel.transform.localPosition = Vector3.zero;

        // Body (torso with leather jacket look)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(visualModel.transform);
        body.transform.localPosition = new Vector3(0, 1f, 0);
        body.transform.localScale = new Vector3(0.6f, 0.8f, 0.4f);
        Renderer bodyRenderer = body.GetComponent<Renderer>();
        bodyRenderer.material = new Material(Shader.Find("Standard"));
        bodyRenderer.material.color = new Color(0.1f, 0.1f, 0.1f); // Dark leather
        Destroy(body.GetComponent<Collider>());
        bodyTransform = body.transform;

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(visualModel.transform);
        head.transform.localPosition = new Vector3(0, 1.9f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        Renderer headRenderer = head.GetComponent<Renderer>();
        headRenderer.material = new Material(Shader.Find("Standard"));
        headRenderer.material.color = new Color(0.8f, 0.7f, 0.6f); // Skin tone
        Destroy(head.GetComponent<Collider>());

        // Mohawk
        GameObject mohawk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mohawk.name = "Mohawk";
        mohawk.transform.SetParent(head.transform);
        mohawk.transform.localPosition = new Vector3(0, 0.3f, -0.05f);
        mohawk.transform.localScale = new Vector3(0.3f, 0.6f, 1.2f);
        mohawk.transform.localRotation = Quaternion.Euler(10, 0, 0);
        Renderer mohawkRenderer = mohawk.GetComponent<Renderer>();
        mohawkRenderer.material = new Material(Shader.Find("Standard"));

        // Random mohawk color
        string chosenColor = mohawkColors[Random.Range(0, mohawkColors.Length)];
        switch (chosenColor)
        {
            case "pink":
                mohawkRenderer.material.color = new Color(1f, 0.4f, 0.7f);
                break;
            case "green":
                mohawkRenderer.material.color = new Color(0.3f, 1f, 0.3f);
                break;
            case "blue":
                mohawkRenderer.material.color = new Color(0.3f, 0.5f, 1f);
                break;
            case "orange":
                mohawkRenderer.material.color = new Color(1f, 0.6f, 0.2f);
                break;
        }
        mohawkRenderer.material.EnableKeyword("_EMISSION");
        mohawkRenderer.material.SetColor("_EmissionColor", mohawkRenderer.material.color * 0.5f);
        Destroy(mohawk.GetComponent<Collider>());

        // Left Arm
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArm.name = "LeftArm";
        leftArm.transform.SetParent(visualModel.transform);
        leftArm.transform.localPosition = new Vector3(-0.45f, 1f, 0);
        leftArm.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
        leftArm.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Renderer leftArmRenderer = leftArm.GetComponent<Renderer>();
        leftArmRenderer.material = new Material(Shader.Find("Standard"));
        leftArmRenderer.material.color = new Color(0.15f, 0.15f, 0.15f);
        Destroy(leftArm.GetComponent<Collider>());

        // Right Arm
        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArm.name = "RightArm";
        rightArm.transform.SetParent(visualModel.transform);
        rightArm.transform.localPosition = new Vector3(0.45f, 1f, 0);
        rightArm.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
        rightArm.transform.localRotation = Quaternion.Euler(0, 0, 0);
        Renderer rightArmRenderer = rightArm.GetComponent<Renderer>();
        rightArmRenderer.material = new Material(Shader.Find("Standard"));
        rightArmRenderer.material.color = new Color(0.15f, 0.15f, 0.15f);
        Destroy(rightArm.GetComponent<Collider>());

        // Left Leg
        GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLeg.name = "LeftLeg";
        leftLeg.transform.SetParent(visualModel.transform);
        leftLeg.transform.localPosition = new Vector3(-0.2f, 0.3f, 0);
        leftLeg.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);
        Renderer leftLegRenderer = leftLeg.GetComponent<Renderer>();
        leftLegRenderer.material = new Material(Shader.Find("Standard"));
        leftLegRenderer.material.color = new Color(0.05f, 0.05f, 0.1f); // Dark pants
        Destroy(leftLeg.GetComponent<Collider>());

        // Right Leg
        GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLeg.name = "RightLeg";
        rightLeg.transform.SetParent(visualModel.transform);
        rightLeg.transform.localPosition = new Vector3(0.2f, 0.3f, 0);
        rightLeg.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);
        Renderer rightLegRenderer = rightLeg.GetComponent<Renderer>();
        rightLegRenderer.material = new Material(Shader.Find("Standard"));
        rightLegRenderer.material.color = new Color(0.05f, 0.05f, 0.1f);
        Destroy(rightLeg.GetComponent<Collider>());

        // Jacket details (studs/spikes on shoulders)
        GameObject leftStud = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftStud.name = "LeftStud";
        leftStud.transform.SetParent(body.transform);
        leftStud.transform.localPosition = new Vector3(-0.6f, 0.3f, 0);
        leftStud.transform.localScale = new Vector3(0.2f, 0.1f, 0.5f);
        Renderer leftStudRenderer = leftStud.GetComponent<Renderer>();
        leftStudRenderer.material = new Material(Shader.Find("Standard"));
        leftStudRenderer.material.color = new Color(0.5f, 0.5f, 0.5f);
        leftStudRenderer.material.SetFloat("_Metallic", 0.8f);
        Destroy(leftStud.GetComponent<Collider>());

        GameObject rightStud = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightStud.name = "RightStud";
        rightStud.transform.SetParent(body.transform);
        rightStud.transform.localPosition = new Vector3(0.6f, 0.3f, 0);
        rightStud.transform.localScale = new Vector3(0.2f, 0.1f, 0.5f);
        Renderer rightStudRenderer = rightStud.GetComponent<Renderer>();
        rightStudRenderer.material = new Material(Shader.Find("Standard"));
        rightStudRenderer.material.color = new Color(0.5f, 0.5f, 0.5f);
        rightStudRenderer.material.SetFloat("_Metallic", 0.8f);
        Destroy(rightStud.GetComponent<Collider>());

        // Add capsule collider for the NPC
        CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
        capsule.height = 2.2f;
        capsule.radius = 0.4f;
        capsule.center = new Vector3(0, 1.1f, 0);
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            // Pick a random destination within patrol radius
            currentDestination = GetRandomDestination();

            // Check if destination is safe (not over toxic puddle)
            int maxAttempts = 5;
            int attempts = 0;
            while (IsDestinationUnsafe(currentDestination) && attempts < maxAttempts)
            {
                currentDestination = GetRandomDestination();
                attempts++;
            }

            isWalking = true;
            isPaused = false;

            // Walk to destination
            while (Vector3.Distance(transform.position, currentDestination) > destinationThreshold)
            {
                MoveTowardsDestination();
                yield return null;
            }

            // Reached destination, pause
            isWalking = false;
            isPaused = true;
            float pauseDuration = Random.Range(minPauseDuration, maxPauseDuration);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    Vector3 GetRandomDestination()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 destination = spawnPoint + new Vector3(randomCircle.x, 0, randomCircle.y);
        destination.y = initialY; // Keep at same height
        return destination;
    }

    bool IsDestinationUnsafe(Vector3 destination)
    {
        // Check for trigger colliders (toxic puddles) near the destination
        Collider[] colliders = Physics.OverlapSphere(destination, avoidanceCheckRadius);
        foreach (Collider col in colliders)
        {
            if (col.isTrigger && (col.CompareTag("Hazard") || col.name.Contains("Toxic") || col.name.Contains("Puddle")))
            {
                return true;
            }
        }
        return false;
    }

    void MoveTowardsDestination()
    {
        // Move towards destination
        Vector3 direction = (currentDestination - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void Update()
    {
        if (isDancing)
        {
            UpdateDancing();
        }
        else
        {
            // Idle bobbing animation
            if (bodyTransform != null)
            {
                float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                bodyTransform.localPosition = new Vector3(0, 1f + bob, 0);
            }
        }
    }

    void UpdateDancing()
    {
        danceTime += Time.deltaTime;

        // Walk towards dance floor first
        float distToDanceSpot = Vector3.Distance(transform.position, danceTarget);
        if (distToDanceSpot > 1.5f)
        {
            // Walk to dance spot
            Vector3 direction = (danceTarget - transform.position).normalized;
            transform.position += direction * moveSpeed * 1.5f * Time.deltaTime;  // Walk faster to the party

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Simple walk bob
            if (bodyTransform != null)
            {
                float walkBob = Mathf.Sin(danceTime * 8f) * 0.05f;
                bodyTransform.localPosition = new Vector3(0, 1f + walkBob, 0);
            }
        }
        else
        {
            // At dance spot - DANCE!
            if (bodyTransform != null && visualModel != null)
            {
                // Energetic dancing animation
                float danceIntensity = 0.15f;
                float danceBobSpeed = 6f;
                float danceRotSpeed = 3f;

                // Bobbing up and down
                float bob = Mathf.Sin(danceTime * danceBobSpeed) * danceIntensity;
                bodyTransform.localPosition = new Vector3(0, 1f + bob, 0);

                // Side to side swaying
                float sway = Mathf.Sin(danceTime * danceRotSpeed) * 10f;
                visualModel.transform.localRotation = Quaternion.Euler(0, sway, 0);

                // Occasional spin
                if (Mathf.Sin(danceTime * 0.5f) > 0.9f)
                {
                    transform.Rotate(0, 180f * Time.deltaTime, 0);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw patrol radius
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);

        // Draw current destination
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentDestination, 0.5f);
            Gizmos.DrawLine(transform.position, currentDestination);
        }
    }
}
