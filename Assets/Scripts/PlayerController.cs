using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 35f;  // CRAZY HIGH jump!
    public LayerMask groundLayer;
    private bool isGrounded = true;

    [Header("Water Death")]
    public float waterDeathHeight = 0.3f;
    private bool isDead = false;
    private bool showDeathScreen = false;

    [Header("Lie Down")]
    private bool isLyingDown = false;
    private float lieDownTransition = 0f;
    private Quaternion standingRotation;
    private Quaternion lyingRotation;

    [Header("Stomp Attack")]
    public float stompDamage = 10f;
    public float stompRadius = 3.5f;
    public float stompCooldown = 2f;
    private float lastStompTime = 0f;
    private bool isStomping = false;
    private float stompAnimTimer = 0f;
    private AudioSource audioSource;

    private Vector3 moveDirection;
    private Rigidbody rb;
    private CameraController cameraController;
    private FishingRodAnimator cachedRodAnimator; // Cache GetComponent result

    // Cached texture for death screen
    private Texture2D deathOverlayTexture;
    private int guiFrameSkip = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraController = Camera.main?.GetComponent<CameraController>();
        cachedRodAnimator = GetComponent<FishingRodAnimator>(); // Cache once

        // Setup audio source for stomp
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 0.5f;

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
        // Don't cast if any UI window is open or if lying down
        if (Input.GetMouseButtonDown(0) && !IsAnyUIOpen() && !isLyingDown)
        {
            // Use cached rod animator reference
            // Only start fishing if line is not already out AND not already charging
            if (cachedRodAnimator != null && !cachedRodAnimator.IsLineOut() && !cachedRodAnimator.IsCharging())
            {
                if (FishingSystem.Instance != null && FishingSystem.Instance.CanFish())
                {
                    FishingSystem.Instance.StartFishing();
                }
            }
            // If line is out or charging, the rod animator handles the click
        }

        // Space to jump (can't jump while lying down)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isLyingDown)
        {
            Jump();
        }

        // CTRL to lie down / stand up
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            ToggleLieDown();
        }

        // G to stomp (Jungle Realm only)
        if (Input.GetKeyDown(KeyCode.G) && !isLyingDown && !isStomping)
        {
            if (IsInJungleRealm())
            {
                TryStompAttack();
            }
        }

        // Handle lie down transition animation
        UpdateLieDownTransition();

        // Handle stomp animation
        UpdateStompAnimation();

        // Water damage is now handled by PlayerHealth.cs (5hp/sec drowning)
        // Removed instant water death - player can swim but takes damage
    }

    bool IsInJungleRealm()
    {
        // Use cached realm reference for performance
        return GameCache.IsInRealm(RealmType.JungleRealm);
    }

    void TryStompAttack()
    {
        if (Time.time - lastStompTime < stompCooldown)
        {
            // Still on cooldown
            return;
        }

        lastStompTime = Time.time;
        isStomping = true;
        stompAnimTimer = 0f;

        // Play stomp sound
        PlayStompSound();

        // Create visual impact effect
        CreateStompParticles();

        // Deal damage to all snakes within radius
        DamageNearbySnakes();
    }

    void UpdateStompAnimation()
    {
        if (!isStomping) return;

        stompAnimTimer += Time.deltaTime;

        if (stompAnimTimer < 0.15f)
        {
            // Quick jump up
            float jumpHeight = (stompAnimTimer / 0.15f) * 0.5f;
            Vector3 pos = transform.position;
            pos.y = 1f + jumpHeight;
            transform.position = pos;
        }
        else if (stompAnimTimer < 0.3f)
        {
            // Slam down
            float slamProgress = (stompAnimTimer - 0.15f) / 0.15f;
            Vector3 pos = transform.position;
            pos.y = 1.5f - (slamProgress * 0.5f);
            transform.position = pos;
        }
        else if (stompAnimTimer >= 0.5f)
        {
            // Animation complete
            isStomping = false;
            Vector3 pos = transform.position;
            pos.y = 1f;
            transform.position = pos;
        }
    }

    void PlayStompSound()
    {
        if (audioSource == null) return;

        AudioClip stompClip = GenerateStompSound();
        audioSource.PlayOneShot(stompClip, 0.8f);
    }

    AudioClip GenerateStompSound()
    {
        // Generate a deep impact sound
        int sampleRate = 22050;
        int samples = sampleRate / 4; // 0.25 second sound
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Pow(1f - t, 2f); // Quick decay

            // Low frequency thump (50-100 Hz)
            float freq = 80f - (t * 30f); // Pitch drops
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);

            // Add some noise for impact texture
            float noise = Random.Range(-0.3f, 0.3f);

            data[i] = (wave * 0.7f + noise * 0.3f) * envelope;
        }

        AudioClip clip = AudioClip.Create("Stomp", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    void CreateStompParticles()
    {
        // Use shared materials from GameCache instead of creating new ones each stomp
        Material dirtMat = GameCache.GetSharedMaterial("stompDirt", new Color(0.4f, 0.3f, 0.2f));

        // Reduced particle count for better performance (15 -> 8)
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.name = "StompParticle";

            // Random position around player's feet
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(0.5f, 2f);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * dist, 0.1f, Mathf.Sin(angle) * dist);
            particle.transform.position = transform.position + offset;

            // Random size
            float size = Random.Range(0.1f, 0.3f);
            particle.transform.localScale = new Vector3(size, size, size);

            // Use shared material
            particle.GetComponent<Renderer>().sharedMaterial = dirtMat;

            // Remove collider
            Destroy(particle.GetComponent<Collider>());

            // Add upward velocity
            Rigidbody particleRb = particle.AddComponent<Rigidbody>();
            particleRb.useGravity = true;
            Vector3 velocity = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(2f, 5f),
                Random.Range(-2f, 2f)
            );
            particleRb.linearVelocity = velocity;
            particleRb.angularVelocity = Random.insideUnitSphere * 10f;

            // Destroy after 1 second
            Destroy(particle, 1f);
        }

        // Create shockwave ring effect (subtle ground ripple)
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "StompRing";
        ring.transform.position = transform.position + Vector3.up * 0.05f;
        ring.transform.localScale = new Vector3(0.2f, 0.005f, 0.2f);

        Material ringMat = GameCache.GetSharedMaterial("stompRing", new Color(0.6f, 0.5f, 0.3f, 0.5f));
        ring.GetComponent<Renderer>().sharedMaterial = ringMat;
        Destroy(ring.GetComponent<Collider>());

        // Animate ring expansion
        StartCoroutine(ExpandRing(ring));
    }

    System.Collections.IEnumerator ExpandRing(GameObject ring)
    {
        float timer = 0f;
        float duration = 0.4f;
        Vector3 startScale = ring.transform.localScale;
        // Subtle ripple - only expands to 1.2 units diameter (much smaller than damage radius)
        Vector3 endScale = new Vector3(1.2f, 0.005f, 1.2f);

        Material mat = ring.GetComponent<Renderer>().material;
        Color startColor = mat.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            ring.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            // Fade out
            Color col = startColor;
            col.a = startColor.a * (1f - t);
            mat.color = col;

            yield return null;
        }

        Destroy(ring);
    }

    void DamageNearbySnakes()
    {
        // Use cached snake list from GameCache instead of expensive FindObjectsOfType
        int snakesHit = 0;
        foreach (SnakeAI snake in GameCache.Snakes)
        {
            if (snake == null) continue;
            float distance = Vector3.Distance(transform.position, snake.transform.position);

            if (distance <= stompRadius)
            {
                snake.TakeDamage(stompDamage);
                snakesHit++;
            }
        }

        if (snakesHit > 0)
        {
            Debug.Log($"STOMP hit {snakesHit} snake(s) for {stompDamage} damage each!");

            // Show notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"STOMPED {snakesHit} snake(s)!", new Color(0.8f, 0.4f, 0.2f));
            }
        }
    }

    void ToggleLieDown()
    {
        isLyingDown = !isLyingDown;

        if (isLyingDown)
        {
            // Store current rotation and set target lying rotation
            standingRotation = transform.rotation;
            // Lie face down on stomach
            lyingRotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
            Debug.Log("Lying down to relax...");
        }
        else
        {
            Debug.Log("Standing back up!");
        }
    }

    void UpdateLieDownTransition()
    {
        float transitionSpeed = 3f;

        if (isLyingDown)
        {
            // Transition to lying down
            lieDownTransition = Mathf.MoveTowards(lieDownTransition, 1f, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Slerp(standingRotation, lyingRotation, lieDownTransition);

            // Lower the player slightly when lying down
            if (lieDownTransition > 0.5f && rb != null)
            {
                // Keep player at ground level
            }
        }
        else if (lieDownTransition > 0f)
        {
            // Transition back to standing
            lieDownTransition = Mathf.MoveTowards(lieDownTransition, 0f, Time.deltaTime * transitionSpeed);

            // Restore upright rotation
            Quaternion targetStanding = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Slerp(lyingRotation, targetStanding, 1f - lieDownTransition);
        }
    }

    void HandleWoWMovement()
    {
        // Can't move while lying down
        if (isLyingDown)
        {
            moveDirection = Vector3.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D for turning
        float vertical = Input.GetAxisRaw("Vertical");     // W/S for forward/back

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        // Snubnose Speed buff - +25% movement speed
        if (FishBuffSystem.Instance != null)
        {
            currentSpeed *= FishBuffSystem.Instance.GetSpeedMultiplier();
        }

        // WoW-style mouse movement: Both mouse buttons held = move forward in camera direction
        bool leftMouseHeld = Input.GetMouseButton(0);
        bool rightMouseHeld = Input.GetMouseButton(1);
        bool bothMouseButtons = leftMouseHeld && rightMouseHeld;

        if (bothMouseButtons)
        {
            // Move forward in the direction the camera is facing
            if (cameraController != null && Camera.main != null)
            {
                // Get the camera's forward direction, but keep it horizontal (no up/down movement)
                Vector3 cameraForward = Camera.main.transform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                // Move the player in that direction
                transform.position += cameraForward * currentSpeed * Time.deltaTime;
                moveDirection = cameraForward;

                // Optionally rotate the player to face the movement direction
                if (cameraForward.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
        else
        {
            // Normal keyboard movement

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
        // Performance: Skip frames when not actively needed
        if (!showDeathScreen)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        if (!MainMenu.GameStarted) return;

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

    public bool IsLyingDown()
    {
        return isLyingDown;
    }

    // Check if any UI window is currently open (blocks fishing)
    bool IsAnyUIOpen()
    {
        // Check all UI panels that could be open
        if (CharacterPanel.Instance != null && CharacterPanel.Instance.IsOpen()) return true;
        if (FishDiary.Instance != null && FishDiary.Instance.IsOpen()) return true;
        if (FishInventoryPanel.Instance != null && FishInventoryPanel.Instance.IsOpen()) return true;
        if (BBQStation.Instance != null && BBQStation.Instance.IsOpen()) return true;
        if (ClothingShopNPC.Instance != null && ClothingShopNPC.Instance.IsShopOpen()) return true;
        if (WetsuitPeteQuests.Instance != null && WetsuitPeteQuests.Instance.IsDialogueOpen()) return true;
        if (GoldieBanksNPC.Instance != null && GoldieBanksNPC.Instance.IsDialogueOpen()) return true;
        if (IceRealmShopNPC.Instance != null && IceRealmShopNPC.Instance.IsShopOpen()) return true;
        if (WeaponShopNPC.Instance != null && WeaponShopNPC.Instance.IsShopOpen()) return true;
        if (PauseMenu.IsPaused) return true;
        return false;
    }
}
