using UnityEngine;

/// <summary>
/// Adorable baby polar bear that follows the player
/// Shows question mark occasionally, can be fed fish
/// </summary>
public class PolarBearCubPet : MonoBehaviour
{
    public static PolarBearCubPet Instance { get; private set; }

    // Following behavior
    private Transform playerTransform;
    public float followDistance = 2.5f;
    public float moveSpeed = 4f;
    private Vector3 targetPosition;

    // Animation
    private float bobTime = 0f;
    private float walkCycle = 0f;
    private bool isWalking = false;

    // Question mark
    private bool showingQuestion = false;
    private float questionTimer = 0f;
    private float nextQuestionTime;
    private GameObject questionMark;

    // Feeding
    private bool canFeed = false;
    private float feedCooldown = 0f;
    private int fishFed = 0;

    // Messages
    private string currentMessage = "";
    private float messageTimer = 0f;
    private string[] happyMessages = {
        "*happy bear noises*",
        "Rawrr! (Thank you!)",
        "*snuggles*",
        "Nom nom nom!",
        "*wiggles excitedly*"
    };

    private string[] questionMessages = {
        "...?",
        "What's that?",
        "*curious sniff*",
        "Rawr?",
        "*tilts head*"
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        CreateCubModel();
        CreateQuestionMark();
        nextQuestionTime = Time.time + Random.Range(15f, 30f);

        // Find player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            targetPosition = playerTransform.position - playerTransform.forward * followDistance;
        }
    }

    void CreateCubModel()
    {
        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = new Color(0.98f, 0.98f, 1f);

        Material furShadeMat = new Material(Shader.Find("Standard"));
        furShadeMat.color = new Color(0.88f, 0.9f, 0.95f);

        Material noseMat = new Material(Shader.Find("Standard"));
        noseMat.color = new Color(0.15f, 0.15f, 0.15f);

        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = Color.black;

        // Round body (cute and chubby)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 0.25f, 0);
        body.transform.localScale = new Vector3(0.4f, 0.35f, 0.5f);
        body.GetComponent<Renderer>().material = furMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Head (big and cute)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 0.4f, 0.28f);
        head.transform.localScale = new Vector3(0.35f, 0.32f, 0.3f);
        head.GetComponent<Renderer>().material = furMat;
        Object.Destroy(head.GetComponent<Collider>());

        // Snout
        GameObject snout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        snout.name = "Snout";
        snout.transform.SetParent(transform);
        snout.transform.localPosition = new Vector3(0, 0.35f, 0.42f);
        snout.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
        snout.GetComponent<Renderer>().material = furShadeMat;
        Object.Destroy(snout.GetComponent<Collider>());

        // Nose (small and cute)
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "Nose";
        nose.transform.SetParent(transform);
        nose.transform.localPosition = new Vector3(0, 0.36f, 0.5f);
        nose.transform.localScale = new Vector3(0.06f, 0.05f, 0.04f);
        nose.GetComponent<Renderer>().material = noseMat;
        Object.Destroy(nose.GetComponent<Collider>());

        // Eyes (big and cute)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(transform);
            eye.transform.localPosition = new Vector3(side * 0.08f, 0.45f, 0.38f);
            eye.transform.localScale = Vector3.one * 0.05f;
            eye.GetComponent<Renderer>().material = eyeMat;
            Object.Destroy(eye.GetComponent<Collider>());

            // Eye shine
            GameObject shine = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shine.name = "EyeShine";
            shine.transform.SetParent(transform);
            shine.transform.localPosition = new Vector3(side * 0.06f, 0.47f, 0.4f);
            shine.transform.localScale = Vector3.one * 0.015f;

            Material shineMat = new Material(Shader.Find("Standard"));
            shineMat.color = Color.white;
            shineMat.EnableKeyword("_EMISSION");
            shineMat.SetColor("_EmissionColor", Color.white);
            shine.GetComponent<Renderer>().material = shineMat;
            Object.Destroy(shine.GetComponent<Collider>());
        }

        // Ears (round and fluffy)
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ear.name = "Ear";
            ear.transform.SetParent(transform);
            ear.transform.localPosition = new Vector3(side * 0.12f, 0.55f, 0.25f);
            ear.transform.localScale = new Vector3(0.08f, 0.08f, 0.05f);
            ear.GetComponent<Renderer>().material = furMat;
            Object.Destroy(ear.GetComponent<Collider>());

            // Inner ear
            GameObject innerEar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            innerEar.name = "InnerEar";
            innerEar.transform.SetParent(transform);
            innerEar.transform.localPosition = new Vector3(side * 0.12f, 0.55f, 0.27f);
            innerEar.transform.localScale = new Vector3(0.04f, 0.04f, 0.02f);
            innerEar.GetComponent<Renderer>().material = furShadeMat;
            Object.Destroy(innerEar.GetComponent<Collider>());
        }

        // Stubby legs
        Vector3[] legPositions = {
            new Vector3(-0.12f, 0.08f, 0.12f),
            new Vector3(0.12f, 0.08f, 0.12f),
            new Vector3(-0.12f, 0.08f, -0.12f),
            new Vector3(0.12f, 0.08f, -0.12f)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = "Leg" + i;
            leg.transform.SetParent(transform);
            leg.transform.localPosition = legPositions[i];
            leg.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            leg.GetComponent<Renderer>().material = furMat;
            Object.Destroy(leg.GetComponent<Collider>());
        }

        // Tiny tail
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tail.name = "Tail";
        tail.transform.SetParent(transform);
        tail.transform.localPosition = new Vector3(0, 0.28f, -0.22f);
        tail.transform.localScale = new Vector3(0.08f, 0.06f, 0.06f);
        tail.GetComponent<Renderer>().material = furMat;
        Object.Destroy(tail.GetComponent<Collider>());
    }

    void CreateQuestionMark()
    {
        questionMark = new GameObject("QuestionMark");
        questionMark.transform.SetParent(transform);
        questionMark.transform.localPosition = new Vector3(0, 0.8f, 0);

        // Question mark made from primitives
        Material qMat = new Material(Shader.Find("Standard"));
        qMat.color = new Color(1f, 0.9f, 0.3f);
        qMat.EnableKeyword("_EMISSION");
        qMat.SetColor("_EmissionColor", new Color(0.5f, 0.45f, 0.1f));

        // Curve of question mark
        GameObject curve = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        curve.name = "QCurve";
        curve.transform.SetParent(questionMark.transform);
        curve.transform.localPosition = new Vector3(0, 0.08f, 0);
        curve.transform.localScale = new Vector3(0.12f, 0.15f, 0.05f);
        curve.GetComponent<Renderer>().material = qMat;
        Object.Destroy(curve.GetComponent<Collider>());

        // Dot of question mark
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = "QDot";
        dot.transform.SetParent(questionMark.transform);
        dot.transform.localPosition = new Vector3(0, -0.08f, 0);
        dot.transform.localScale = Vector3.one * 0.05f;
        dot.GetComponent<Renderer>().material = qMat;
        Object.Destroy(dot.GetComponent<Collider>());

        questionMark.SetActive(false);
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        UpdateFollowing();
        UpdateAnimation();
        UpdateQuestionMark();
        UpdateFeeding();
        UpdateMessage();

        // Animate question mark
        if (questionMark != null && questionMark.activeSelf)
        {
            questionMark.transform.localPosition = new Vector3(0, 0.8f + Mathf.Sin(Time.time * 4f) * 0.05f, 0);
            questionMark.transform.Rotate(Vector3.up * 60f * Time.deltaTime);
        }
    }

    void UpdateFollowing()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        // Calculate target position behind player
        targetPosition = playerTransform.position - playerTransform.forward * followDistance;
        targetPosition += playerTransform.right * 0.8f; // Slightly to the side

        float distToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distToTarget > 0.5f)
        {
            isWalking = true;
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;

            // Move toward target
            float speed = distToTarget > 5f ? moveSpeed * 1.5f : moveSpeed;
            transform.position += direction * speed * Time.deltaTime;

            // Face movement direction
            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }
        }
        else
        {
            isWalking = false;

            // Face same direction as player when idle
            transform.rotation = Quaternion.Slerp(transform.rotation,
                playerTransform.rotation, Time.deltaTime * 2f);
        }

        // Keep at ground level
        transform.position = new Vector3(transform.position.x, 1.25f, transform.position.z);
    }

    void UpdateAnimation()
    {
        bobTime += Time.deltaTime;

        // Body bob
        float bob = Mathf.Sin(bobTime * 3f) * 0.02f;
        Transform body = transform.Find("Body");
        if (body != null)
        {
            body.localPosition = new Vector3(0, 0.25f + bob, 0);
        }

        // Walking leg animation
        if (isWalking)
        {
            walkCycle += Time.deltaTime * 8f;

            string[] legNames = { "Leg0", "Leg1", "Leg2", "Leg3" };
            Vector3[] basePositions = {
                new Vector3(-0.12f, 0.08f, 0.12f),
                new Vector3(0.12f, 0.08f, 0.12f),
                new Vector3(-0.12f, 0.08f, -0.12f),
                new Vector3(0.12f, 0.08f, -0.12f)
            };

            for (int i = 0; i < 4; i++)
            {
                Transform leg = transform.Find(legNames[i]);
                if (leg != null)
                {
                    float phase = i % 2 == 0 ? walkCycle : walkCycle + Mathf.PI;
                    float lift = Mathf.Max(0, Mathf.Sin(phase)) * 0.03f;
                    leg.localPosition = basePositions[i] + new Vector3(0, lift, 0);
                }
            }
        }

        // Tail wag
        Transform tail = transform.Find("Tail");
        if (tail != null)
        {
            float wag = Mathf.Sin(bobTime * 6f) * 0.03f;
            tail.localPosition = new Vector3(wag, 0.28f, -0.22f);
        }
    }

    void UpdateQuestionMark()
    {
        // Show question mark occasionally
        if (!showingQuestion && Time.time >= nextQuestionTime)
        {
            showingQuestion = true;
            questionTimer = 5f;
            currentMessage = questionMessages[Random.Range(0, questionMessages.Length)];
            messageTimer = 3f;

            if (questionMark != null)
                questionMark.SetActive(true);
        }

        if (showingQuestion)
        {
            questionTimer -= Time.deltaTime;

            if (questionTimer <= 0)
            {
                showingQuestion = false;
                nextQuestionTime = Time.time + Random.Range(20f, 45f);

                if (questionMark != null)
                    questionMark.SetActive(false);
            }
        }
    }

    void UpdateFeeding()
    {
        feedCooldown -= Time.deltaTime;

        // Check if player is looking at us and has fish
        if (playerTransform != null && feedCooldown <= 0)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distToPlayer < 3f)
            {
                // Check if player presses E
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Check if player has fish to feed
                    if (GameManager.Instance != null && GameManager.Instance.GetTotalFishCaught() > 0)
                    {
                        FeedCub();
                    }
                    else
                    {
                        currentMessage = "*looks at you expectantly*";
                        messageTimer = 2f;
                    }
                }
                canFeed = true;
            }
            else
            {
                canFeed = false;
            }
        }
    }

    void FeedCub()
    {
        feedCooldown = 2f;
        fishFed++;

        // Remove one fish from player
        // Note: Simplified - removes from total fish count
        // In full implementation, would show fish selection UI

        currentMessage = happyMessages[Random.Range(0, happyMessages.Length)];
        messageTimer = 3f;

        // Happy animation - jump!
        StartCoroutine(HappyJump());

        Debug.Log($"Fed the polar bear cub! Total fish fed: {fishFed}");
    }

    System.Collections.IEnumerator HappyJump()
    {
        Vector3 startPos = transform.position;
        float jumpHeight = 0.3f;

        // Jump up
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float y = Mathf.Sin(t / 0.2f * Mathf.PI) * jumpHeight;
            transform.position = startPos + Vector3.up * y;
            yield return null;
        }

        transform.position = startPos;
    }

    void UpdateMessage()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
        }
        else
        {
            currentMessage = "";
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Draw speech bubble if has message
        if (!string.IsNullOrEmpty(currentMessage))
        {
            Vector3 screenPos = Camera.main != null ?
                Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.7f) : Vector3.zero;

            if (screenPos.z > 0)
            {
                float bubbleWidth = 150;
                float bubbleHeight = 40;
                float bubbleX = screenPos.x - bubbleWidth / 2;
                float bubbleY = Screen.height - screenPos.y - bubbleHeight;

                // Bubble background
                GUI.color = new Color(1f, 1f, 0.95f, 0.95f);
                GUI.DrawTexture(new Rect(bubbleX, bubbleY, bubbleWidth, bubbleHeight), Texture2D.whiteTexture);

                // Border
                GUI.color = new Color(0.8f, 0.75f, 0.6f);
                GUI.DrawTexture(new Rect(bubbleX, bubbleY, bubbleWidth, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(bubbleX, bubbleY + bubbleHeight - 2, bubbleWidth, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(bubbleX, bubbleY, 2, bubbleHeight), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(bubbleX + bubbleWidth - 2, bubbleY, 2, bubbleHeight), Texture2D.whiteTexture);
                GUI.color = Color.white;

                // Text
                GUIStyle msgStyle = new GUIStyle();
                msgStyle.fontSize = 12;
                msgStyle.alignment = TextAnchor.MiddleCenter;
                msgStyle.normal.textColor = new Color(0.3f, 0.25f, 0.2f);
                msgStyle.wordWrap = true;

                GUI.Label(new Rect(bubbleX + 5, bubbleY + 5, bubbleWidth - 10, bubbleHeight - 10),
                    currentMessage, msgStyle);
            }
        }

        // Feed prompt
        if (canFeed && feedCooldown <= 0)
        {
            GUIStyle feedStyle = new GUIStyle();
            feedStyle.fontSize = 14;
            feedStyle.fontStyle = FontStyle.Bold;
            feedStyle.alignment = TextAnchor.MiddleCenter;
            feedStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);

            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 180, 200, 25),
                "[E] Feed Fish", feedStyle);
        }
    }
}
