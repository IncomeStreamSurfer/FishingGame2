using UnityEngine;
using System.Collections;

/// <summary>
/// Polar Bear enemy AI for the Ice Realm
/// Patrols the map, attacks players who get too close
/// Cannot attack players on docks (safe zones)
/// </summary>
public class PolarBearAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;
    public int attackDamage = 10;
    public float attackCooldown = 5f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float aggroRadius = 8f;
    public float attackRadius = 2f;
    public float patrolRadius = 30f;

    [Header("Safe Zones")]
    public float dockSafeRadius = 12f;
    private Vector3 dockPosition;

    // State
    private enum BearState { Patrol, Chase, Attack, Returning, Dead }
    private BearState currentState = BearState.Patrol;

    private Vector3 patrolTarget;
    private Vector3 homePosition;
    private Transform playerTransform;
    private float lastAttackTime;
    private float stateTimer;

    // Visual
    private Renderer[] bearRenderers;
    private Color originalColor;
    private float damageFlashTimer;
    private bool isDead = false;

    // Audio
    private AudioSource audioSource;
    private bool hasRoared = false;

    // Loot
    public static int totalBearsKilled = 0;

    void Start()
    {
        currentHealth = maxHealth;
        homePosition = transform.position;
        patrolTarget = GetRandomPatrolPoint();

        // Find dock position (dock area is safe zone)
        // Dock is at Ice Realm origin (500,0,0) + local position (0,0,25) + extends to Z=65
        dockPosition = new Vector3(500f, 1f, 45f); // Center of dock area

        // Get renderers for damage flash
        bearRenderers = GetComponentsInChildren<Renderer>();
        if (bearRenderers.Length > 0)
        {
            originalColor = bearRenderers[0].material.color;
        }

        // Audio source for roar
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 30f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        CreateBearModel();
    }

    void CreateBearModel()
    {
        // Create polar bear from primitives - big fluffy realistic bear
        Material furMat = new Material(Shader.Find("Standard"));
        furMat.color = new Color(0.97f, 0.97f, 0.95f); // Creamy white like real polar bear

        Material furUnder = new Material(Shader.Find("Standard"));
        furUnder.color = new Color(0.92f, 0.91f, 0.88f); // Slightly darker undertone

        Material noseMat = new Material(Shader.Find("Standard"));
        noseMat.color = new Color(0.1f, 0.1f, 0.1f);

        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = new Color(0.05f, 0.05f, 0.05f);

        // Main body - large barrel shape
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 1.0f, 0);
        body.transform.localRotation = Quaternion.Euler(90, 0, 0);
        body.transform.localScale = new Vector3(1.4f, 1.6f, 1.2f);
        body.GetComponent<Renderer>().material = furMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Fluffy chest/belly bulge
        GameObject chest = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        chest.name = "Chest";
        chest.transform.SetParent(transform);
        chest.transform.localPosition = new Vector3(0, 0.85f, 0.5f);
        chest.transform.localScale = new Vector3(1.3f, 1.1f, 1.0f);
        chest.GetComponent<Renderer>().material = furMat;
        Object.Destroy(chest.GetComponent<Collider>());

        // Rear haunches
        GameObject rear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rear.name = "Rear";
        rear.transform.SetParent(transform);
        rear.transform.localPosition = new Vector3(0, 0.95f, -0.6f);
        rear.transform.localScale = new Vector3(1.2f, 1.0f, 1.0f);
        rear.GetComponent<Renderer>().material = furMat;
        Object.Destroy(rear.GetComponent<Collider>());

        // Neck
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        neck.name = "Neck";
        neck.transform.SetParent(transform);
        neck.transform.localPosition = new Vector3(0, 1.2f, 1.1f);
        neck.transform.localRotation = Quaternion.Euler(60, 0, 0);
        neck.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
        neck.GetComponent<Renderer>().material = furMat;
        Object.Destroy(neck.GetComponent<Collider>());

        // Head - elongated bear skull shape
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 1.3f, 1.5f);
        head.transform.localScale = new Vector3(0.7f, 0.6f, 0.8f);
        head.GetComponent<Renderer>().material = furMat;
        Object.Destroy(head.GetComponent<Collider>());

        // Snout - elongated
        GameObject snout = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        snout.name = "Snout";
        snout.transform.SetParent(transform);
        snout.transform.localPosition = new Vector3(0, 1.15f, 2.0f);
        snout.transform.localRotation = Quaternion.Euler(90, 0, 0);
        snout.transform.localScale = new Vector3(0.35f, 0.35f, 0.3f);
        snout.GetComponent<Renderer>().material = furMat;
        Object.Destroy(snout.GetComponent<Collider>());

        // Nose - black
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.name = "Nose";
        nose.transform.SetParent(transform);
        nose.transform.localPosition = new Vector3(0, 1.18f, 2.35f);
        nose.transform.localScale = new Vector3(0.15f, 0.12f, 0.12f);
        nose.GetComponent<Renderer>().material = noseMat;
        Object.Destroy(nose.GetComponent<Collider>());

        // Eyes - small and black
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(transform);
            eye.transform.localPosition = new Vector3(side * 0.2f, 1.4f, 1.75f);
            eye.transform.localScale = Vector3.one * 0.08f;
            eye.GetComponent<Renderer>().material = eyeMat;
            Object.Destroy(eye.GetComponent<Collider>());
        }

        // Small round ears
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ear.name = "Ear";
            ear.transform.SetParent(transform);
            ear.transform.localPosition = new Vector3(side * 0.28f, 1.55f, 1.35f);
            ear.transform.localScale = new Vector3(0.12f, 0.14f, 0.08f);
            ear.GetComponent<Renderer>().material = furMat;
            Object.Destroy(ear.GetComponent<Collider>());
        }

        // Front legs - thick and powerful
        Vector3[] frontLegPos = { new Vector3(-0.45f, 0.5f, 0.7f), new Vector3(0.45f, 0.5f, 0.7f) };
        foreach (Vector3 legPos in frontLegPos)
        {
            // Upper leg
            GameObject upperLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperLeg.name = "FrontUpperLeg";
            upperLeg.transform.SetParent(transform);
            upperLeg.transform.localPosition = legPos;
            upperLeg.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
            upperLeg.GetComponent<Renderer>().material = furMat;
            Object.Destroy(upperLeg.GetComponent<Collider>());

            // Paw - big and flat
            GameObject paw = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            paw.name = "FrontPaw";
            paw.transform.SetParent(transform);
            paw.transform.localPosition = legPos + new Vector3(0, -0.45f, 0.1f);
            paw.transform.localScale = new Vector3(0.28f, 0.12f, 0.32f);
            paw.GetComponent<Renderer>().material = furUnder;
            Object.Destroy(paw.GetComponent<Collider>());
        }

        // Back legs - thick haunches
        Vector3[] backLegPos = { new Vector3(-0.4f, 0.5f, -0.5f), new Vector3(0.4f, 0.5f, -0.5f) };
        foreach (Vector3 legPos in backLegPos)
        {
            // Upper leg/haunch
            GameObject upperLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            upperLeg.name = "BackUpperLeg";
            upperLeg.transform.SetParent(transform);
            upperLeg.transform.localPosition = legPos;
            upperLeg.transform.localScale = new Vector3(0.38f, 0.5f, 0.38f);
            upperLeg.GetComponent<Renderer>().material = furMat;
            Object.Destroy(upperLeg.GetComponent<Collider>());

            // Paw
            GameObject paw = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            paw.name = "BackPaw";
            paw.transform.SetParent(transform);
            paw.transform.localPosition = legPos + new Vector3(0, -0.5f, 0.05f);
            paw.transform.localScale = new Vector3(0.25f, 0.1f, 0.3f);
            paw.GetComponent<Renderer>().material = furUnder;
            Object.Destroy(paw.GetComponent<Collider>());
        }

        // Small tail
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tail.name = "Tail";
        tail.transform.SetParent(transform);
        tail.transform.localPosition = new Vector3(0, 1.1f, -1.1f);
        tail.transform.localScale = new Vector3(0.18f, 0.15f, 0.15f);
        tail.GetComponent<Renderer>().material = furMat;
        Object.Destroy(tail.GetComponent<Collider>());

        // Add collider to main object
        CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0, 1.0f, 0.5f);
        col.radius = 0.7f;
        col.height = 3f;
        col.direction = 2; // Z-axis

        // Update renderer references
        bearRenderers = GetComponentsInChildren<Renderer>();
        if (bearRenderers.Length > 0)
        {
            originalColor = furMat.color;
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateDamageFlash();
        FindPlayer();

        switch (currentState)
        {
            case BearState.Patrol:
                UpdatePatrol();
                break;
            case BearState.Chase:
                UpdateChase();
                break;
            case BearState.Attack:
                UpdateAttack();
                break;
            case BearState.Returning:
                UpdateReturning();
                break;
        }
    }

    void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    void UpdatePatrol()
    {
        // Move toward patrol target
        Vector3 direction = (patrolTarget - transform.position).normalized;
        direction.y = 0;

        transform.position += direction * patrolSpeed * Time.deltaTime;
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * 3f);
        }

        // Check if reached patrol point
        float distToTarget = Vector3.Distance(transform.position, patrolTarget);
        if (distToTarget < 2f)
        {
            patrolTarget = GetRandomPatrolPoint();
        }

        // Check for player - but don't attack if player has bear cub pet
        if (playerTransform != null)
        {
            // Bears won't attack players who have a bear cub pet (they recognize family)
            if (PolarBearCubPet.Instance != null)
            {
                return; // Player is friend, not foe
            }

            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distToPlayer < aggroRadius && !IsPlayerInSafeZone())
            {
                currentState = BearState.Chase;
                hasRoared = false;
            }
        }
    }

    void UpdateChase()
    {
        if (playerTransform == null)
        {
            currentState = BearState.Returning;
            return;
        }

        // Stop chasing if player has bear cub pet
        if (PolarBearCubPet.Instance != null)
        {
            currentState = BearState.Returning;
            return;
        }

        // Growl when starting chase
        if (!hasRoared)
        {
            PlayRoarSound();
            hasRoared = true;
        }

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float distFromHome = Vector3.Distance(transform.position, homePosition);

        // Check if player escaped to safe zone
        if (IsPlayerInSafeZone())
        {
            currentState = BearState.Returning;
            return;
        }

        // Check if too far from home
        if (distFromHome > patrolRadius * 1.5f)
        {
            currentState = BearState.Returning;
            return;
        }

        // Check if player too far
        if (distToPlayer > aggroRadius * 1.5f)
        {
            currentState = BearState.Returning;
            return;
        }

        // Move toward player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;

        transform.position += direction * chaseSpeed * Time.deltaTime;
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        // Check if close enough to attack
        if (distToPlayer < attackRadius)
        {
            currentState = BearState.Attack;
        }
    }

    void UpdateAttack()
    {
        if (playerTransform == null || IsPlayerInSafeZone())
        {
            currentState = BearState.Returning;
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Face player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Attack if cooldown ready
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
        }

        // Return to chase if player moved away
        if (distToPlayer > attackRadius * 1.5f)
        {
            currentState = BearState.Chase;
        }
    }

    void UpdateReturning()
    {
        Vector3 direction = (homePosition - transform.position).normalized;
        direction.y = 0;

        transform.position += direction * patrolSpeed * Time.deltaTime;
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * 3f);
        }

        float distToHome = Vector3.Distance(transform.position, homePosition);
        if (distToHome < 3f)
        {
            currentState = BearState.Patrol;
            patrolTarget = GetRandomPatrolPoint();
        }

        // Re-aggro if player gets close again
        if (playerTransform != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distToPlayer < aggroRadius * 0.7f && !IsPlayerInSafeZone())
            {
                currentState = BearState.Chase;
                hasRoared = false;
            }
        }
    }

    void PerformAttack()
    {
        lastAttackTime = Time.time;

        // Random attack type
        bool isPounce = Random.value > 0.5f;

        if (isPounce)
        {
            Debug.Log("Polar Bear POUNCES!");
        }
        else
        {
            Debug.Log("Polar Bear CLAWS!");
        }

        // Play roar/attack sound
        PlayRoarSound();

        // Deal damage to player
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(attackDamage);

            // Show damage notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"-{attackDamage} HP (Bear Attack!)",
                    new Color(1f, 0.3f, 0.3f));
            }
        }

        // Attack animation (simple lunge forward)
        StartCoroutine(AttackAnimation());
    }

    IEnumerator AttackAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 lungePos = transform.position + transform.forward * 0.5f;

        // Lunge forward
        float t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, lungePos, t / 0.15f);
            yield return null;
        }

        // Return
        t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(lungePos, startPos, t / 0.15f);
            yield return null;
        }

        transform.position = startPos;
    }

    bool IsPlayerInSafeZone()
    {
        if (playerTransform == null) return true;

        // Check if player is near the center dock area
        float distToDock = Vector3.Distance(playerTransform.position, dockPosition);
        return distToDock < dockSafeRadius;
    }

    Vector3 GetRandomPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 point = homePosition + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Keep within ice realm bounds (roughly)
        point.x = Mathf.Clamp(point.x, 450f, 550f);
        point.z = Mathf.Clamp(point.z, -40f, 40f);
        point.y = 1.25f; // Ground level

        return point;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        damageFlashTimer = 0.3f;

        // Play hurt sound
        PlayRoarSound();

        Debug.Log($"Polar Bear took {damage} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Become aggressive
            if (currentState == BearState.Patrol)
            {
                currentState = BearState.Chase;
                hasRoared = false;
            }
        }
    }

    void UpdateDamageFlash()
    {
        if (damageFlashTimer > 0)
        {
            damageFlashTimer -= Time.deltaTime;

            // Flash red
            Color flashColor = Color.Lerp(originalColor, Color.red, damageFlashTimer / 0.3f);

            foreach (Renderer r in bearRenderers)
            {
                if (r != null && r.material != null)
                {
                    // Don't change black parts (eyes, nose)
                    if (r.material.color.r > 0.5f)
                    {
                        r.material.color = flashColor;
                    }
                }
            }
        }
        else
        {
            // Reset to original color
            foreach (Renderer r in bearRenderers)
            {
                if (r != null && r.material != null)
                {
                    if (r.material.color.r > 0.3f && r.name != "Nose" && r.name != "Eye")
                    {
                        r.material.color = originalColor;
                    }
                }
            }
        }
    }

    void Die()
    {
        isDead = true;
        currentState = BearState.Dead;
        totalBearsKilled++;

        Debug.Log("Polar Bear defeated! Dropping Bear Skin...");

        // Drop loot
        DropBearSkin();

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Death animation
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        // Fall over
        float t = 0;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, transform.eulerAngles.y, 90);

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // Fade out and destroy after delay
        yield return new WaitForSeconds(5f);

        // Respawn bear after delay
        yield return new WaitForSeconds(60f); // 1 minute respawn

        // Reset and respawn
        isDead = false;
        currentHealth = maxHealth;
        transform.position = homePosition;
        transform.rotation = Quaternion.identity;
        currentState = BearState.Patrol;
        patrolTarget = GetRandomPatrolPoint();

        Collider respawnCol = GetComponent<Collider>();
        if (respawnCol != null) respawnCol.enabled = true;

        Debug.Log("Polar Bear respawned!");
    }

    void DropBearSkin()
    {
        // Add bear skin to player's inventory
        if (BjorkHuntsman.Instance != null)
        {
            BjorkHuntsman.Instance.AddBearSkin();
        }

        // Show loot notification
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("+ Polar Bear Skin", new Color(0.9f, 0.85f, 0.8f));
        }
    }

    void PlayRoarSound()
    {
        // Create procedural growl sound
        if (audioSource != null)
        {
            AudioClip growl = CreateGrowlClip();
            audioSource.PlayOneShot(growl, 0.9f);
        }
    }

    // Separate growl for when bear spots player
    public void PlayGrowl()
    {
        if (audioSource != null)
        {
            AudioClip growl = CreateGrowlClip();
            audioSource.PlayOneShot(growl, 0.7f);
        }
    }

    AudioClip CreateGrowlClip()
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.8f); // 0.8 second growl
        float[] data = new float[samples];

        // Randomize base frequencies for variety
        float baseFreq = 60f + Random.Range(-10f, 10f);

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;

            // Growl envelope - starts strong, sustains, then fades
            float envelope;
            if (progress < 0.1f)
            {
                envelope = progress / 0.1f; // Quick attack
            }
            else if (progress < 0.7f)
            {
                envelope = 1f - (progress - 0.1f) * 0.2f; // Slow decay during sustain
            }
            else
            {
                envelope = (1f - progress) / 0.3f * 0.8f; // Release
            }

            // Multiple low frequencies for deep growl
            float freq1 = baseFreq + Mathf.Sin(t * 3f) * 8f; // Wobble
            float freq2 = baseFreq * 2f + Mathf.Sin(t * 5f) * 12f;
            float freq3 = baseFreq * 3f;
            float freq4 = baseFreq * 0.5f; // Sub-bass

            // Main growl tones
            float wave = Mathf.Sin(2 * Mathf.PI * freq1 * t) * 0.35f;
            wave += Mathf.Sin(2 * Mathf.PI * freq2 * t) * 0.25f;
            wave += Mathf.Sin(2 * Mathf.PI * freq3 * t) * 0.15f;
            wave += Mathf.Sin(2 * Mathf.PI * freq4 * t) * 0.25f; // Deep sub

            // Add rumble/texture (filtered noise)
            float noise = (Random.value - 0.5f) * 2f;
            // Simple low-pass by averaging
            wave += noise * 0.2f * envelope;

            // Vibrato for more realistic growl
            float vibrato = Mathf.Sin(t * 25f) * 0.15f;
            wave *= (1f + vibrato);

            // Apply envelope
            data[i] = wave * envelope * 0.6f;

            // Soft clip for warmth
            data[i] = Mathf.Clamp(data[i], -0.8f, 0.8f);
        }

        // Smooth the audio
        for (int i = 1; i < samples - 1; i++)
        {
            data[i] = data[i] * 0.6f + data[i - 1] * 0.2f + data[i + 1] * 0.2f;
        }

        AudioClip clip = AudioClip.Create("BearGrowl", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (isDead) return;

        // Draw health bar above bear if damaged
        if (currentHealth < maxHealth)
        {
            Vector3 screenPos = Camera.main != null ?
                Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f) : Vector3.zero;

            if (screenPos.z > 0)
            {
                float barWidth = 60;
                float barHeight = 8;
                float barX = screenPos.x - barWidth / 2;
                float barY = Screen.height - screenPos.y;

                // Background
                GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                GUI.DrawTexture(new Rect(barX - 1, barY - 1, barWidth + 2, barHeight + 2), Texture2D.whiteTexture);

                // Health fill
                float healthPercent = currentHealth / maxHealth;
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercent);
                GUI.color = healthColor;
                GUI.DrawTexture(new Rect(barX, barY, barWidth * healthPercent, barHeight), Texture2D.whiteTexture);

                GUI.color = Color.white;
            }
        }
    }
}
