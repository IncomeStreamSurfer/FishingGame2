using UnityEngine;
using System.Collections;

/// <summary>
/// Shoulder Parrot - A loyal feathered friend!
/// - Flies down from sky when purchased
/// - Sits on player's shoulder
/// - Press E to interact - "Polly wants a cracker?" "KAWWW!"
/// </summary>
public class ShoulderParrot : MonoBehaviour
{
    public static ShoulderParrot Instance { get; private set; }

    private bool isEquipped = false;
    private bool isFlying = false;
    private bool canInteract = false;
    private bool isInteracting = false;
    private float interactTimer = 0f;
    private float interactDuration = 3f;

    // Parrot visual
    private GameObject parrotModel;
    private Transform playerTransform;
    private Vector3 shoulderOffset = new Vector3(0.35f, 1.6f, 0.1f);

    // Animation
    private float idleTimer = 0f;
    private float headBobAmount = 0f;

    // Audio
    private AudioSource audioSource;

    // UI
    private Texture2D bubbleTexture;
    private bool showPlayerBubble = false;
    private bool showParrotResponse = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetupAudio();
        CreateBubbleTexture();
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.5f;
        audioSource.playOnAwake = false;
    }

    void CreateBubbleTexture()
    {
        bubbleTexture = new Texture2D(1, 1);
        bubbleTexture.SetPixel(0, 0, new Color(1f, 1f, 0.9f, 0.95f));
        bubbleTexture.Apply();
    }

    public void SpawnParrot()
    {
        if (parrotModel != null) return;

        // Find player
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        playerTransform = player.transform;

        // Create parrot model
        CreateParrotModel();

        // Start flying animation from sky
        StartCoroutine(FlyDownAnimation());
    }

    void CreateParrotModel()
    {
        parrotModel = new GameObject("ShoulderParrot");
        parrotModel.transform.SetParent(transform);

        // Body - green parrot
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "ParrotBody";
        body.transform.SetParent(parrotModel.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.12f, 0.1f, 0.15f);
        Material bodyMat = new Material(Shader.Find("Standard"));
        bodyMat.color = new Color(0.2f, 0.75f, 0.3f); // Bright green
        body.GetComponent<Renderer>().material = bodyMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "ParrotHead";
        head.transform.SetParent(parrotModel.transform);
        head.transform.localPosition = new Vector3(0, 0.06f, 0.06f);
        head.transform.localScale = new Vector3(0.09f, 0.085f, 0.09f);
        head.GetComponent<Renderer>().material = bodyMat;
        Object.Destroy(head.GetComponent<Collider>());

        // Beak
        GameObject beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beak.name = "ParrotBeak";
        beak.transform.SetParent(head.transform);
        beak.transform.localPosition = new Vector3(0, -0.1f, 0.5f);
        beak.transform.localScale = new Vector3(0.3f, 0.25f, 0.4f);
        beak.transform.localRotation = Quaternion.Euler(20, 0, 0);
        Material beakMat = new Material(Shader.Find("Standard"));
        beakMat.color = new Color(0.95f, 0.7f, 0.2f); // Orange/yellow beak
        beak.GetComponent<Renderer>().material = beakMat;
        Object.Destroy(beak.GetComponent<Collider>());

        // Eyes
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "ParrotEye";
            eye.transform.SetParent(head.transform);
            eye.transform.localPosition = new Vector3(side * 0.35f, 0.1f, 0.35f);
            eye.transform.localScale = new Vector3(0.2f, 0.25f, 0.15f);
            Material eyeMat = new Material(Shader.Find("Standard"));
            eyeMat.color = Color.black;
            eye.GetComponent<Renderer>().material = eyeMat;
            Object.Destroy(eye.GetComponent<Collider>());
        }

        // Wings
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wing.name = "ParrotWing";
            wing.transform.SetParent(parrotModel.transform);
            wing.transform.localPosition = new Vector3(side * 0.07f, 0.01f, -0.02f);
            wing.transform.localScale = new Vector3(0.02f, 0.06f, 0.1f);
            wing.transform.localRotation = Quaternion.Euler(0, 0, side * -20);
            Material wingMat = new Material(Shader.Find("Standard"));
            wingMat.color = new Color(0.15f, 0.6f, 0.25f); // Darker green
            wing.GetComponent<Renderer>().material = wingMat;
            Object.Destroy(wing.GetComponent<Collider>());
        }

        // Tail
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tail.name = "ParrotTail";
        tail.transform.SetParent(parrotModel.transform);
        tail.transform.localPosition = new Vector3(0, -0.02f, -0.12f);
        tail.transform.localScale = new Vector3(0.04f, 0.03f, 0.12f);
        tail.transform.localRotation = Quaternion.Euler(-15, 0, 0);
        Material tailMat = new Material(Shader.Find("Standard"));
        tailMat.color = new Color(0.8f, 0.2f, 0.2f); // Red tail feathers
        tail.GetComponent<Renderer>().material = tailMat;
        Object.Destroy(tail.GetComponent<Collider>());

        // Feet/claws
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foot.name = "ParrotFoot";
            foot.transform.SetParent(parrotModel.transform);
            foot.transform.localPosition = new Vector3(side * 0.03f, -0.06f, 0);
            foot.transform.localScale = new Vector3(0.015f, 0.03f, 0.025f);
            Material footMat = new Material(Shader.Find("Standard"));
            footMat.color = new Color(0.3f, 0.3f, 0.3f);
            foot.GetComponent<Renderer>().material = footMat;
            Object.Destroy(foot.GetComponent<Collider>());
        }

        parrotModel.transform.localScale = Vector3.one * 0.8f;
    }

    IEnumerator FlyDownAnimation()
    {
        isFlying = true;

        // Play excited parrot sound
        PlayParrotSound(true);

        // Start high in the sky
        Vector3 startPos = playerTransform.position + new Vector3(5f, 15f, 5f);
        parrotModel.transform.position = startPos;

        // Calculate target position on shoulder
        Vector3 targetPos = playerTransform.position + playerTransform.TransformDirection(shoulderOffset);

        float flyTime = 2f;
        float t = 0f;

        // Wing flapping during flight
        Transform leftWing = parrotModel.transform.Find("ParrotWing");
        Transform rightWing = null;
        foreach (Transform child in parrotModel.transform)
        {
            if (child.name == "ParrotWing" && child != leftWing)
            {
                rightWing = child;
                break;
            }
        }

        while (t < flyTime)
        {
            t += Time.deltaTime;
            float progress = t / flyTime;

            // Curved flight path
            Vector3 currentTarget = playerTransform.position + playerTransform.TransformDirection(shoulderOffset);
            Vector3 pos = Vector3.Lerp(startPos, currentTarget, EaseOutQuad(progress));

            // Add some wobble
            pos.x += Mathf.Sin(t * 8f) * 0.3f * (1f - progress);
            pos.y += Mathf.Sin(t * 6f) * 0.2f * (1f - progress);

            parrotModel.transform.position = pos;

            // Face direction of travel
            Vector3 dir = (currentTarget - parrotModel.transform.position).normalized;
            if (dir.magnitude > 0.1f)
            {
                parrotModel.transform.rotation = Quaternion.LookRotation(dir);
            }

            // Flap wings
            float flapAngle = Mathf.Sin(t * 25f) * 45f;
            if (leftWing != null)
                leftWing.localRotation = Quaternion.Euler(0, 0, -20 + flapAngle);

            yield return null;
        }

        // Land on shoulder
        isFlying = false;
        isEquipped = true;
        canInteract = true;

        // Reset wing positions
        if (leftWing != null)
            leftWing.localRotation = Quaternion.Euler(0, 0, -20);

        // Play landing chirp
        yield return new WaitForSeconds(0.2f);
        PlayParrotSound(false);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Your parrot has arrived! Press E to interact.", new Color(0.3f, 0.9f, 0.4f));
        }
    }

    float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        if (isEquipped && !isFlying && parrotModel != null && playerTransform != null)
        {
            // Keep parrot on shoulder
            Vector3 targetPos = playerTransform.position + playerTransform.TransformDirection(shoulderOffset);
            parrotModel.transform.position = Vector3.Lerp(parrotModel.transform.position, targetPos, Time.deltaTime * 10f);

            // Face same direction as player with slight offset
            Quaternion targetRot = playerTransform.rotation * Quaternion.Euler(0, 30, 0);
            parrotModel.transform.rotation = Quaternion.Lerp(parrotModel.transform.rotation, targetRot, Time.deltaTime * 5f);

            // Idle animation - head bob
            idleTimer += Time.deltaTime;
            headBobAmount = Mathf.Sin(idleTimer * 3f) * 0.01f;

            Transform head = parrotModel.transform.Find("ParrotHead");
            if (head != null)
            {
                head.localPosition = new Vector3(0, 0.06f + headBobAmount, 0.06f);
                // Occasional head tilt
                float tilt = Mathf.Sin(idleTimer * 1.5f) * 10f;
                head.localRotation = Quaternion.Euler(0, tilt, 0);
            }

            // Check for interaction
            if (canInteract && !isInteracting && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(ParrotInteraction());
            }
        }

        // Handle interaction timer
        if (isInteracting)
        {
            interactTimer += Time.deltaTime;
            if (interactTimer >= interactDuration)
            {
                isInteracting = false;
                showPlayerBubble = false;
                showParrotResponse = false;
            }
        }
    }

    IEnumerator ParrotInteraction()
    {
        isInteracting = true;
        interactTimer = 0f;

        // Show player speech bubble
        showPlayerBubble = true;
        showParrotResponse = false;

        yield return new WaitForSeconds(1f);

        // Parrot responds
        showParrotResponse = true;
        PlayParrotSound(false);

        yield return new WaitForSeconds(2f);

        isInteracting = false;
        showPlayerBubble = false;
        showParrotResponse = false;
    }

    void PlayParrotSound(bool excited)
    {
        StartCoroutine(GenerateParrotSound(excited));
    }

    IEnumerator GenerateParrotSound(bool excited)
    {
        int sampleRate = 44100;
        float duration = excited ? 0.8f : 0.5f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip parrotClip = AudioClip.Create("ParrotSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];

        if (excited)
        {
            // Excited squawk - multiple calls
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = (float)i / sampleCount;

                float call = 0f;

                // Two squawks
                float squawk1 = 0f;
                float squawk2 = 0f;

                if (progress < 0.4f)
                {
                    float p = progress / 0.4f;
                    float freq = Mathf.Lerp(800f, 1200f, p) + Mathf.Sin(t * 50f) * 100f;
                    squawk1 = Mathf.Sin(2 * Mathf.PI * freq * t);
                    squawk1 += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.3f;
                    squawk1 *= Mathf.Sin(p * Mathf.PI);
                }
                else if (progress > 0.5f && progress < 0.9f)
                {
                    float p = (progress - 0.5f) / 0.4f;
                    float freq = Mathf.Lerp(900f, 1400f, p) + Mathf.Sin(t * 60f) * 120f;
                    squawk2 = Mathf.Sin(2 * Mathf.PI * freq * t);
                    squawk2 += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.3f;
                    squawk2 *= Mathf.Sin(p * Mathf.PI);
                }

                call = (squawk1 + squawk2) * 0.4f;
                samples[i] = call;
            }
        }
        else
        {
            // Single "KAWWW" sound
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = (float)i / sampleCount;

                // Rising then falling frequency
                float freqCurve = Mathf.Sin(progress * Mathf.PI);
                float freq = 600f + freqCurve * 600f + Mathf.Sin(t * 40f) * 80f;

                float call = Mathf.Sin(2 * Mathf.PI * freq * t);
                call += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.25f;
                call += Mathf.Sin(2 * Mathf.PI * freq * 3f * t) * 0.1f;

                // Harsh overtones for parrot sound
                call += (Random.value * 2f - 1f) * 0.1f * freqCurve;

                // Envelope
                float envelope = Mathf.Sin(progress * Mathf.PI);
                envelope *= 1f - progress * 0.3f; // Slight decay

                samples[i] = call * envelope * 0.35f;
            }
        }

        parrotClip.SetData(samples, 0);
        audioSource.clip = parrotClip;
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.Play();

        yield return null;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Show "Press E" prompt when equipped and not interacting
        if (isEquipped && canInteract && !isInteracting)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 14;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.4f, 0.9f, 0.5f);

            GUI.Label(new Rect(0, Screen.height - 180, Screen.width, 20), "[E] Talk to Parrot", promptStyle);
        }

        // Draw speech bubbles during interaction
        if (isInteracting)
        {
            DrawInteractionBubbles();
        }
    }

    void DrawInteractionBubbles()
    {
        float bubbleWidth = 200;
        float bubbleHeight = 50;

        // Player bubble - "Polly wants a cracker?"
        if (showPlayerBubble)
        {
            float playerBubbleX = Screen.width / 2 - bubbleWidth - 20;
            float playerBubbleY = Screen.height / 2 - 100;

            GUI.DrawTexture(new Rect(playerBubbleX, playerBubbleY, bubbleWidth, bubbleHeight), bubbleTexture);

            GUIStyle textStyle = new GUIStyle();
            textStyle.fontSize = 14;
            textStyle.fontStyle = FontStyle.Italic;
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
            textStyle.wordWrap = true;

            GUI.Label(new Rect(playerBubbleX + 10, playerBubbleY + 5, bubbleWidth - 20, bubbleHeight - 10),
                "\"Polly wants a cracker?\"", textStyle);
        }

        // Parrot bubble - "KAWWW!"
        if (showParrotResponse)
        {
            float parrotBubbleX = Screen.width / 2 + 20;
            float parrotBubbleY = Screen.height / 2 - 120;
            float parrotBubbleWidth = 120;

            // Green tinted bubble for parrot
            GUI.color = new Color(0.8f, 1f, 0.8f);
            GUI.DrawTexture(new Rect(parrotBubbleX, parrotBubbleY, parrotBubbleWidth, bubbleHeight), bubbleTexture);
            GUI.color = Color.white;

            GUIStyle parrotStyle = new GUIStyle();
            parrotStyle.fontSize = 18;
            parrotStyle.fontStyle = FontStyle.Bold;
            parrotStyle.alignment = TextAnchor.MiddleCenter;
            parrotStyle.normal.textColor = new Color(0.2f, 0.6f, 0.2f);

            GUI.Label(new Rect(parrotBubbleX, parrotBubbleY + 5, parrotBubbleWidth, bubbleHeight - 10),
                "KAWWW!", parrotStyle);
        }
    }

    public void EquipParrot()
    {
        if (!isEquipped)
        {
            SpawnParrot();
        }
    }

    public void UnequipParrot()
    {
        if (isEquipped && parrotModel != null)
        {
            isEquipped = false;
            canInteract = false;
            Object.Destroy(parrotModel);
            parrotModel = null;
        }
    }

    public bool IsEquipped() => isEquipped;

    void OnDestroy()
    {
        if (bubbleTexture != null) Destroy(bubbleTexture);
    }
}
