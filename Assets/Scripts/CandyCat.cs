using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Candy the Cat - A black, white, and ginger mixed fur cat who patrols the island
/// Press E near her to feed her a fish!
/// </summary>
public class CandyCat : MonoBehaviour
{
    public static CandyCat Instance { get; private set; }

    [Header("Patrol Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float patrolRadius = 35f;
    public float waypointReachDistance = 1.5f;
    public float idleTimeMin = 3f;
    public float idleTimeMax = 8f;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public int fishFedCount = 0;

    // Cat parts
    private GameObject body;
    private GameObject head;
    private GameObject tail;
    private GameObject[] legs = new GameObject[4];
    private GameObject[] ears = new GameObject[2];

    // State
    private enum CatState { Idle, Walking, Running, Eating, Sitting, Sleeping }
    private CatState currentState = CatState.Idle;
    private Vector3 targetPosition;
    private float stateTimer = 0f;
    private float walkAnimTime = 0f;
    private Transform playerTransform;
    private bool isNearPlayer = false;

    // Materials for fur pattern
    private Material blackFurMat;
    private Material whiteFurMat;
    private Material gingerFurMat;

    void Awake()
    {
        // Ensure only ONE cat exists
        if (Instance != null && Instance != this)
        {
            Debug.Log("Duplicate CandyCat destroyed - only one cat allowed!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        CreateCatModel();
        SetNewPatrolTarget();

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        StartCoroutine(CatBehaviorLoop());
    }

    void CreateCatModel()
    {
        // Create fur materials - Calico/Tortoiseshell pattern (black, white, ginger)
        blackFurMat = new Material(Shader.Find("Standard"));
        blackFurMat.color = new Color(0.1f, 0.1f, 0.1f);
        blackFurMat.SetFloat("_Glossiness", 0.3f);

        whiteFurMat = new Material(Shader.Find("Standard"));
        whiteFurMat.color = new Color(0.95f, 0.93f, 0.90f);
        whiteFurMat.SetFloat("_Glossiness", 0.35f);

        gingerFurMat = new Material(Shader.Find("Standard"));
        gingerFurMat.color = new Color(0.85f, 0.45f, 0.15f);
        gingerFurMat.SetFloat("_Glossiness", 0.3f);

        // Pink for nose/inner ears
        Material pinkMat = new Material(Shader.Find("Standard"));
        pinkMat.color = new Color(0.95f, 0.7f, 0.75f);

        // Body - elongated sphere with patches
        body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "CatBody";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 0.25f, 0);
        body.transform.localScale = new Vector3(0.3f, 0.25f, 0.5f);
        body.transform.localRotation = Quaternion.Euler(0, 0, 90);
        body.GetComponent<Renderer>().material = whiteFurMat; // Base white
        Destroy(body.GetComponent<Collider>());

        // Body patches (black and ginger spots)
        CreateFurPatch(body.transform, new Vector3(0.1f, 0, 0.15f), 0.12f, blackFurMat);
        CreateFurPatch(body.transform, new Vector3(-0.08f, 0.05f, -0.1f), 0.1f, gingerFurMat);
        CreateFurPatch(body.transform, new Vector3(0.05f, -0.05f, -0.2f), 0.08f, blackFurMat);
        CreateFurPatch(body.transform, new Vector3(-0.1f, 0, 0.2f), 0.09f, gingerFurMat);

        // Head
        head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "CatHead";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 0.35f, 0.35f);
        head.transform.localScale = new Vector3(0.28f, 0.24f, 0.26f);
        head.GetComponent<Renderer>().material = whiteFurMat;
        Destroy(head.GetComponent<Collider>());

        // Face patches
        CreateFurPatch(head.transform, new Vector3(0.08f, 0.02f, 0.08f), 0.08f, blackFurMat); // Right eye patch
        CreateFurPatch(head.transform, new Vector3(-0.06f, 0.04f, 0.06f), 0.07f, gingerFurMat); // Left forehead

        // Ears
        for (int i = 0; i < 2; i++)
        {
            float side = i == 0 ? -1 : 1;
            ears[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ears[i].name = "CatEar" + i;
            ears[i].transform.SetParent(head.transform);
            ears[i].transform.localPosition = new Vector3(side * 0.35f, 0.4f, -0.1f);
            ears[i].transform.localScale = new Vector3(0.25f, 0.35f, 0.15f);
            ears[i].transform.localRotation = Quaternion.Euler(0, 0, side * -15f);
            ears[i].GetComponent<Renderer>().material = i == 0 ? blackFurMat : gingerFurMat;
            Destroy(ears[i].GetComponent<Collider>());

            // Inner ear (pink)
            GameObject innerEar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            innerEar.name = "InnerEar";
            innerEar.transform.SetParent(ears[i].transform);
            innerEar.transform.localPosition = new Vector3(0, 0, 0.3f);
            innerEar.transform.localScale = new Vector3(0.6f, 0.7f, 0.3f);
            innerEar.GetComponent<Renderer>().material = pinkMat;
            Destroy(innerEar.GetComponent<Collider>());
        }

        // Nose
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "CatNose";
        nose.transform.SetParent(head.transform);
        nose.transform.localPosition = new Vector3(0, -0.1f, 0.45f);
        nose.transform.localScale = new Vector3(0.15f, 0.1f, 0.1f);
        nose.GetComponent<Renderer>().material = pinkMat;
        Destroy(nose.GetComponent<Collider>());

        // Eyes
        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = new Color(0.4f, 0.7f, 0.3f); // Green eyes
        eyeMat.EnableKeyword("_EMISSION");
        eyeMat.SetColor("_EmissionColor", new Color(0.2f, 0.4f, 0.15f) * 0.3f);

        for (int i = 0; i < 2; i++)
        {
            float side = i == 0 ? -1 : 1;
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "CatEye" + i;
            eye.transform.SetParent(head.transform);
            eye.transform.localPosition = new Vector3(side * 0.25f, 0.1f, 0.35f);
            eye.transform.localScale = new Vector3(0.12f, 0.14f, 0.08f);
            eye.GetComponent<Renderer>().material = eyeMat;
            Destroy(eye.GetComponent<Collider>());

            // Pupil
            GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.name = "Pupil";
            pupil.transform.SetParent(eye.transform);
            pupil.transform.localPosition = new Vector3(0, 0, 0.4f);
            pupil.transform.localScale = new Vector3(0.4f, 0.6f, 0.3f);
            pupil.GetComponent<Renderer>().material = blackFurMat;
            Destroy(pupil.GetComponent<Collider>());
        }

        // Legs
        Material[] legMats = { blackFurMat, whiteFurMat, gingerFurMat, whiteFurMat };
        Vector3[] legPositions = {
            new Vector3(-0.12f, 0.1f, 0.18f),  // Front left
            new Vector3(0.12f, 0.1f, 0.18f),   // Front right
            new Vector3(-0.12f, 0.1f, -0.18f), // Back left
            new Vector3(0.12f, 0.1f, -0.18f)   // Back right
        };

        for (int i = 0; i < 4; i++)
        {
            legs[i] = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            legs[i].name = "CatLeg" + i;
            legs[i].transform.SetParent(transform);
            legs[i].transform.localPosition = legPositions[i];
            legs[i].transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
            legs[i].GetComponent<Renderer>().material = legMats[i];
            Destroy(legs[i].GetComponent<Collider>());

            // Paw
            GameObject paw = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            paw.name = "Paw";
            paw.transform.SetParent(legs[i].transform);
            paw.transform.localPosition = new Vector3(0, -0.8f, 0);
            paw.transform.localScale = new Vector3(1.3f, 0.5f, 1.5f);
            paw.GetComponent<Renderer>().material = whiteFurMat; // White paws
            Destroy(paw.GetComponent<Collider>());
        }

        // Tail
        tail = new GameObject("CatTail");
        tail.transform.SetParent(transform);
        tail.transform.localPosition = new Vector3(0, 0.3f, -0.35f);

        // Tail segments for fluffy look
        Material[] tailMats = { gingerFurMat, blackFurMat, whiteFurMat };
        for (int i = 0; i < 5; i++)
        {
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            segment.name = "TailSegment" + i;
            segment.transform.SetParent(tail.transform);
            float t = i / 4f;
            segment.transform.localPosition = new Vector3(0, t * 0.15f + Mathf.Sin(t * Mathf.PI) * 0.1f, -t * 0.35f);
            float size = 0.1f - t * 0.015f;
            segment.transform.localScale = new Vector3(size, size, size * 1.2f);
            segment.GetComponent<Renderer>().material = tailMats[i % 3];
            Destroy(segment.GetComponent<Collider>());
        }

        // Add collider to main transform for interaction
        CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 0.25f, 0);
        col.radius = 0.25f;
        col.height = 0.6f;
        col.isTrigger = true;
    }

    void CreateFurPatch(Transform parent, Vector3 localPos, float size, Material mat)
    {
        GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        patch.name = "FurPatch";
        patch.transform.SetParent(parent);
        patch.transform.localPosition = localPos;
        patch.transform.localScale = Vector3.one * size * 2f;
        patch.GetComponent<Renderer>().material = mat;
        Destroy(patch.GetComponent<Collider>());
    }

    IEnumerator CatBehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (!MainMenu.GameStarted) continue;

            // Find player if needed
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            // Check distance to player
            if (playerTransform != null)
            {
                float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                isNearPlayer = distToPlayer < interactionDistance;
            }

            // State machine
            stateTimer -= 0.1f;

            switch (currentState)
            {
                case CatState.Idle:
                    if (stateTimer <= 0)
                    {
                        // Choose next action
                        float rand = Random.value;
                        if (rand < 0.5f)
                        {
                            currentState = CatState.Walking;
                            SetNewPatrolTarget();
                        }
                        else if (rand < 0.7f)
                        {
                            currentState = CatState.Sitting;
                            stateTimer = Random.Range(4f, 10f);
                        }
                        else if (rand < 0.85f)
                        {
                            currentState = CatState.Sleeping;
                            stateTimer = Random.Range(8f, 15f);
                        }
                        else
                        {
                            stateTimer = Random.Range(idleTimeMin, idleTimeMax);
                        }
                    }
                    break;

                case CatState.Walking:
                case CatState.Running:
                    // Move toward target
                    Vector3 direction = (targetPosition - transform.position).normalized;
                    direction.y = 0;

                    float speed = currentState == CatState.Running ? runSpeed : walkSpeed;
                    transform.position += direction * speed * 0.1f;

                    // Face movement direction
                    if (direction.magnitude > 0.1f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.15f);
                    }

                    // Check if reached target
                    float distToTarget = Vector3.Distance(transform.position, targetPosition);
                    if (distToTarget < waypointReachDistance)
                    {
                        currentState = CatState.Idle;
                        stateTimer = Random.Range(idleTimeMin, idleTimeMax);
                    }
                    break;

                case CatState.Sitting:
                case CatState.Sleeping:
                    if (stateTimer <= 0)
                    {
                        currentState = CatState.Idle;
                        stateTimer = Random.Range(1f, 3f);
                    }
                    break;

                case CatState.Eating:
                    if (stateTimer <= 0)
                    {
                        currentState = CatState.Idle;
                        stateTimer = Random.Range(3f, 6f);

                        // Happy after eating!
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.ShowLootNotification("Candy purrs happily!", new Color(1f, 0.8f, 0.9f));
                        }
                    }
                    break;
            }
        }
    }

    void SetNewPatrolTarget()
    {
        // Pick random point on island
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(5f, patrolRadius);

        targetPosition = new Vector3(
            Mathf.Cos(angle) * dist,
            1.0f, // Ground level
            Mathf.Sin(angle) * dist - 10f // Offset for island center
        );
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Animate based on state
        AnimateCat();

        // Check for feed input
        if (isNearPlayer && Input.GetKeyDown(KeyCode.E))
        {
            TryFeedCat();
        }

        // Keep on ground
        transform.position = new Vector3(transform.position.x, 1.0f, transform.position.z);
    }

    void AnimateCat()
    {
        walkAnimTime += Time.deltaTime;

        switch (currentState)
        {
            case CatState.Walking:
            case CatState.Running:
                // Leg animation
                float legSpeed = currentState == CatState.Running ? 12f : 6f;
                for (int i = 0; i < 4; i++)
                {
                    if (legs[i] == null) continue;
                    float phase = i < 2 ? 0 : Mathf.PI; // Front/back offset
                    float side = i % 2 == 0 ? 1 : -1;
                    float swing = Mathf.Sin(walkAnimTime * legSpeed + phase + side * 0.5f) * 15f;
                    legs[i].transform.localRotation = Quaternion.Euler(swing, 0, 0);
                }

                // Tail sway
                if (tail != null)
                {
                    float tailSway = Mathf.Sin(walkAnimTime * 4f) * 20f;
                    tail.transform.localRotation = Quaternion.Euler(30f, tailSway, 0);
                }

                // Head bob
                if (head != null)
                {
                    float bob = Mathf.Sin(walkAnimTime * legSpeed * 0.5f) * 0.02f;
                    head.transform.localPosition = new Vector3(0, 0.35f + bob, 0.35f);
                }
                break;

            case CatState.Idle:
                // Gentle breathing
                if (body != null)
                {
                    float breath = 1f + Mathf.Sin(walkAnimTime * 2f) * 0.02f;
                    body.transform.localScale = new Vector3(0.3f * breath, 0.25f, 0.5f);
                }

                // Tail gentle wave
                if (tail != null)
                {
                    float wave = Mathf.Sin(walkAnimTime * 1.5f) * 10f;
                    tail.transform.localRotation = Quaternion.Euler(20f, wave, 0);
                }

                // Occasional ear twitch
                if (Random.value < 0.002f && ears[0] != null)
                {
                    StartCoroutine(EarTwitch());
                }
                break;

            case CatState.Sitting:
                // Sitting pose - legs tucked
                for (int i = 0; i < 4; i++)
                {
                    if (legs[i] == null) continue;
                    if (i < 2) // Front legs
                        legs[i].transform.localRotation = Quaternion.Euler(-30f, 0, 0);
                    else // Back legs tucked under
                        legs[i].transform.localRotation = Quaternion.Euler(60f, 0, 0);
                }

                // Tail wrapped
                if (tail != null)
                    tail.transform.localRotation = Quaternion.Euler(0, 60f, 0);
                break;

            case CatState.Sleeping:
                // Curled up pose
                if (body != null)
                    transform.localRotation = Quaternion.Euler(0, transform.eulerAngles.y, 15f);

                // Slow breathing
                if (body != null)
                {
                    float sleepBreath = 1f + Mathf.Sin(walkAnimTime * 1f) * 0.03f;
                    body.transform.localScale = new Vector3(0.3f * sleepBreath, 0.25f, 0.5f);
                }
                break;

            case CatState.Eating:
                // Head bobbing down (eating)
                if (head != null)
                {
                    float eatBob = Mathf.Sin(walkAnimTime * 8f) * 0.05f;
                    head.transform.localPosition = new Vector3(0, 0.3f + eatBob, 0.4f);
                    head.transform.localRotation = Quaternion.Euler(20f, 0, 0);
                }
                break;
        }
    }

    IEnumerator EarTwitch()
    {
        if (ears[0] == null) yield break;

        int earIndex = Random.Range(0, 2);
        Quaternion originalRot = ears[earIndex].transform.localRotation;

        ears[earIndex].transform.localRotation = originalRot * Quaternion.Euler(0, 0, 20f);
        yield return new WaitForSeconds(0.1f);
        ears[earIndex].transform.localRotation = originalRot;
    }

    void TryFeedCat()
    {
        // Check if player has fish
        if (GameManager.Instance != null && GameManager.Instance.GetTotalFishCaught() > fishFedCount)
        {
            // Feed the cat!
            fishFedCount++;
            currentState = CatState.Eating;
            stateTimer = 4f;

            // Face player
            if (playerTransform != null)
            {
                Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            Debug.Log("Fed Candy a fish! Total fish fed: " + fishFedCount);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("You fed Candy a fish! Meow~", new Color(1f, 0.7f, 0.8f));
            }

            // Give bonus for feeding cat
            if (fishFedCount % 5 == 0)
            {
                // Every 5 fish, give bonus
                GameManager.Instance.AddCoins(50);
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Candy's friendship bonus: +50 gold!", new Color(1f, 0.85f, 0.2f));
                }
            }
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Catch a fish first to feed Candy!", new Color(0.8f, 0.6f, 0.5f));
            }
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (!isNearPlayer) return;

        // Show interaction prompt
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = new Color(1f, 0.9f, 0.8f);

        float promptY = Screen.height * 0.65f;

        // Cat name and status
        string status = currentState == CatState.Sleeping ? "(sleeping)" :
                       currentState == CatState.Eating ? "(eating)" :
                       currentState == CatState.Sitting ? "(relaxing)" : "";

        GUI.Label(new Rect(0, promptY, Screen.width, 25), $"Candy the Cat {status}", style);

        style.fontSize = 12;
        style.normal.textColor = new Color(0.9f, 0.8f, 0.7f);
        GUI.Label(new Rect(0, promptY + 22, Screen.width, 20), "Press E to feed fish", style);

        style.fontSize = 10;
        style.normal.textColor = new Color(0.7f, 0.6f, 0.5f);
        GUI.Label(new Rect(0, promptY + 40, Screen.width, 18), $"Fish fed: {fishFedCount}", style);
    }
}
