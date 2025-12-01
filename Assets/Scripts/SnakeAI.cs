using UnityEngine;

/// <summary>
/// Snake enemy AI for the Jungle Realm
/// Slithers through the jungle, attacks players who get too close
/// Can be defeated with weapons
/// </summary>
public class SnakeAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 60f;
    private float currentHealth;
    public int attackDamage = 15;
    public float attackCooldown = 4f;
    public int venomDamage = 2; // Poison tick damage

    [Header("Movement")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 5f;
    public float aggroRadius = 6f;
    public float attackRadius = 1.5f;
    public float patrolRadius = 25f;

    [Header("Safe Zones")]
    public float hutSafeRadius = 8f;
    private Vector3 hutPosition;
    public float elevationSafeThreshold = 0.5f; // Min height difference to be safe from attacks
    private GameObject[] boulderFormations;

    // State
    private enum SnakeState { Patrol, Chase, Attack, Returning, Dead }
    private SnakeState currentState = SnakeState.Patrol;

    private Vector3 patrolTarget;
    private Vector3 homePosition;
    private Transform playerTransform;
    private float lastAttackTime;
    private float slitherTimer = 0f;

    // Visual
    private GameObject[] bodySegments;
    private Material snakeMat;
    private Material patternMat;
    private float damageFlashTimer;
    private bool isDead = false;

    // Audio
    private AudioSource audioSource;
    private bool hasHissed = false;

    // Loot
    public static int totalSnakesKilled = 0;

    void Start()
    {
        currentHealth = maxHealth;
        homePosition = transform.position;
        patrolTarget = GetRandomPatrolPoint();

        // Find hut position (safe zone)
        hutPosition = new Vector3(1000f, 1f, 0f); // Center of jungle

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        CreateSnakeModel();
        CreateBoulderFormations();
    }

    void CreateSnakeModel()
    {
        // Create a segmented snake body
        snakeMat = new Material(Shader.Find("Standard"));
        snakeMat.color = new Color(0.2f, 0.5f, 0.15f); // Green snake

        patternMat = new Material(Shader.Find("Standard"));
        patternMat.color = new Color(0.35f, 0.25f, 0.1f); // Brown pattern

        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = new Color(0.9f, 0.8f, 0.1f); // Yellow eyes

        Material tongueMat = new Material(Shader.Find("Standard"));
        tongueMat.color = new Color(0.8f, 0.2f, 0.2f); // Red tongue

        // Create body segments
        int segmentCount = 8;
        bodySegments = new GameObject[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            float size = 0.35f - (i * 0.03f); // Taper toward tail

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            segment.name = "BodySegment" + i;
            segment.transform.SetParent(transform);
            segment.transform.localPosition = new Vector3(0, 0.2f, -i * 0.25f);
            segment.transform.localScale = new Vector3(size, size * 0.6f, size);
            segment.GetComponent<Renderer>().material = (i % 2 == 0) ? snakeMat : patternMat;

            if (i > 0) Object.Destroy(segment.GetComponent<Collider>());
            bodySegments[i] = segment;
        }

        // Head (first segment is larger)
        bodySegments[0].transform.localScale = new Vector3(0.4f, 0.3f, 0.5f);

        // Eyes
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye";
            eye.transform.SetParent(bodySegments[0].transform);
            eye.transform.localPosition = new Vector3(side * 0.3f, 0.3f, 0.3f);
            eye.transform.localScale = Vector3.one * 0.25f;
            eye.GetComponent<Renderer>().material = eyeMat;
            Object.Destroy(eye.GetComponent<Collider>());

            // Pupil
            GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pupil.name = "Pupil";
            pupil.transform.SetParent(eye.transform);
            pupil.transform.localPosition = new Vector3(0, 0, 0.4f);
            pupil.transform.localScale = Vector3.one * 0.4f;
            Material pupilMat = new Material(Shader.Find("Standard"));
            pupilMat.color = Color.black;
            pupil.GetComponent<Renderer>().material = pupilMat;
            Object.Destroy(pupil.GetComponent<Collider>());
        }

        // Forked tongue
        GameObject tongue = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tongue.name = "Tongue";
        tongue.transform.SetParent(bodySegments[0].transform);
        tongue.transform.localPosition = new Vector3(0, 0, 0.6f);
        tongue.transform.localScale = new Vector3(0.05f, 0.02f, 0.3f);
        tongue.GetComponent<Renderer>().material = tongueMat;
        Object.Destroy(tongue.GetComponent<Collider>());

        // Tongue forks
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject fork = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fork.name = "TongueFork";
            fork.transform.SetParent(tongue.transform);
            fork.transform.localPosition = new Vector3(side * 0.3f, 0, 0.4f);
            fork.transform.localRotation = Quaternion.Euler(0, side * 30, 0);
            fork.transform.localScale = new Vector3(0.8f, 1f, 0.5f);
            fork.GetComponent<Renderer>().material = tongueMat;
            Object.Destroy(fork.GetComponent<Collider>());
        }
    }

    void CreateBoulderFormations()
    {
        // Only create boulders once (first snake spawned)
        if (GameObject.Find("JungleBoulders") != null) return;

        GameObject boulderParent = new GameObject("JungleBoulders");
        boulderParent.transform.position = new Vector3(1000f, 1f, 0f); // Center of jungle

        // Material for boulders
        Material rockMat = new Material(Shader.Find("Standard"));
        rockMat.color = new Color(0.4f, 0.35f, 0.3f); // Gray-brown rock

        Material mossMat = new Material(Shader.Find("Standard"));
        mossMat.color = new Color(0.2f, 0.4f, 0.15f); // Mossy green

        // Create 6 boulder formations scattered around jungle
        Vector3[] formationPositions = {
            new Vector3(1005f, 1f, 5f),   // Northeast
            new Vector3(1012f, 1f, -3f),  // East
            new Vector3(995f, 1f, 8f),    // Northwest
            new Vector3(988f, 1f, -5f),   // West
            new Vector3(1000f, 1f, 10f),  // North
            new Vector3(1003f, 1f, -8f)   // South
        };

        boulderFormations = new GameObject[formationPositions.Length];

        for (int f = 0; f < formationPositions.Length; f++)
        {
            GameObject formation = new GameObject("BoulderFormation" + f);
            formation.transform.SetParent(boulderParent.transform);
            formation.transform.position = formationPositions[f];
            boulderFormations[f] = formation;

            // Create 3-5 boulders per formation
            int boulderCount = Random.Range(3, 6);
            for (int i = 0; i < boulderCount; i++)
            {
                GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                boulder.name = "Boulder" + i;
                boulder.transform.SetParent(formation.transform);

                // Random offset within formation
                float offsetX = Random.Range(-2f, 2f);
                float offsetZ = Random.Range(-2f, 2f);
                boulder.transform.position = formationPositions[f] + new Vector3(offsetX, 0f, offsetZ);

                // Random size and shape
                float size = Random.Range(0.8f, 1.8f);
                boulder.transform.localScale = new Vector3(
                    size * Random.Range(0.8f, 1.2f),
                    size * Random.Range(0.6f, 1.0f),
                    size * Random.Range(0.8f, 1.2f)
                );

                // Position boulders slightly above ground for climbability
                boulder.transform.position = new Vector3(
                    boulder.transform.position.x,
                    boulder.transform.localScale.y * 0.4f + 1f, // Elevated
                    boulder.transform.position.z
                );

                // Apply material (some with moss)
                Renderer r = boulder.GetComponent<Renderer>();
                r.material = (Random.value < 0.3f) ? mossMat : rockMat;

                // Random rotation for natural look
                boulder.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 30f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 30f)
                );

                // Tag as safe zone
                boulder.tag = "SafeRock";
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        FindPlayer();
        UpdateSlither();

        switch (currentState)
        {
            case SnakeState.Patrol:
                UpdatePatrol();
                break;
            case SnakeState.Chase:
                UpdateChase();
                break;
            case SnakeState.Attack:
                UpdateAttack();
                break;
            case SnakeState.Returning:
                UpdateReturning();
                break;
        }

        UpdateDamageFlash();
    }

    void UpdateSlither()
    {
        slitherTimer += Time.deltaTime * 8f;

        // Make body segments slither
        for (int i = 1; i < bodySegments.Length; i++)
        {
            if (bodySegments[i] != null)
            {
                float offset = Mathf.Sin(slitherTimer + i * 0.5f) * 0.1f;
                Vector3 pos = bodySegments[i].transform.localPosition;
                pos.x = offset;
                bodySegments[i].transform.localPosition = pos;
            }
        }

        // Flick tongue
        Transform tongue = transform.Find("BodySegment0/Tongue");
        if (tongue != null)
        {
            float tongueFlick = Mathf.Sin(slitherTimer * 3f);
            tongue.localPosition = new Vector3(0, 0, 0.6f + tongueFlick * 0.1f);
        }
    }

    void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    Vector3 GetRandomPatrolPoint()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(5f, patrolRadius);
        return homePosition + new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);
    }

    void UpdatePatrol()
    {
        Vector3 direction = (patrolTarget - transform.position).normalized;
        direction.y = 0;

        transform.position += direction * patrolSpeed * Time.deltaTime;
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        float distToTarget = Vector3.Distance(transform.position, patrolTarget);
        if (distToTarget < 2f)
        {
            patrolTarget = GetRandomPatrolPoint();
        }

        // Check for player
        if (playerTransform != null)
        {
            // Don't attack if player is playing dead
            PlayerController pc = playerTransform.GetComponent<PlayerController>();
            if (pc != null && pc.IsLyingDown()) return;

            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distToPlayer < aggroRadius && !IsPlayerInSafeZone())
            {
                currentState = SnakeState.Chase;
                hasHissed = false;
            }
        }
    }

    void UpdateChase()
    {
        if (playerTransform == null)
        {
            currentState = SnakeState.Returning;
            return;
        }

        // Stop if player plays dead
        PlayerController pc = playerTransform.GetComponent<PlayerController>();
        if (pc != null && pc.IsLyingDown())
        {
            currentState = SnakeState.Returning;
            return;
        }

        if (!hasHissed)
        {
            PlayHissSound();
            hasHissed = true;
        }

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float distFromHome = Vector3.Distance(transform.position, homePosition);

        if (IsPlayerInSafeZone() || distFromHome > patrolRadius * 1.5f || distToPlayer > aggroRadius * 1.5f)
        {
            currentState = SnakeState.Returning;
            return;
        }

        if (distToPlayer <= attackRadius)
        {
            currentState = SnakeState.Attack;
            return;
        }

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * chaseSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * 8f);
    }

    void UpdateAttack()
    {
        if (playerTransform == null)
        {
            currentState = SnakeState.Returning;
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distToPlayer > attackRadius * 1.2f)
        {
            currentState = SnakeState.Chase;
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }

        // Face player
        Vector3 lookDir = (playerTransform.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
        }
    }

    void PerformAttack()
    {
        // Check if player is elevated (on rocks, docks, platforms)
        if (playerTransform != null && IsPlayerElevated())
        {
            // Snake cannot reach elevated player - just hiss in frustration
            PlayHissSound();
            return;
        }

        // Check if player has Snake Charm equipped (immunity)
        if (AccessorySystem.Instance != null && AccessorySystem.Instance.HasEffect(AccessoryEffect.SnakeImmunity))
        {
            // Snake Charm deflects the attack - play special sound
            PlayDeflectedSound();

            // Visual feedback - snake recoils
            StartCoroutine(RecoilAnimation());
            return;
        }

        PlayHissSound();

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(attackDamage);
            // Apply venom (could add DOT here)
        }

        // Strike animation
        StartCoroutine(StrikeAnimation());
    }

    System.Collections.IEnumerator StrikeAnimation()
    {
        Vector3 originalPos = transform.position;
        Vector3 strikePos = originalPos + transform.forward * 0.5f;

        float t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(originalPos, strikePos, t / 0.15f);
            yield return null;
        }

        t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(strikePos, originalPos, t / 0.15f);
            yield return null;
        }

        transform.position = originalPos;
    }

    void UpdateReturning()
    {
        float distFromHome = Vector3.Distance(transform.position, homePosition);

        if (distFromHome < 3f)
        {
            currentState = SnakeState.Patrol;
            patrolTarget = GetRandomPatrolPoint();
            hasHissed = false;
            return;
        }

        Vector3 direction = (homePosition - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * patrolSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * 3f);
    }

    bool IsPlayerInSafeZone()
    {
        if (playerTransform == null) return false;
        float distToHut = Vector3.Distance(playerTransform.position, hutPosition);
        return distToHut < hutSafeRadius;
    }

    bool IsPlayerElevated()
    {
        if (playerTransform == null) return false;

        // Check if player is significantly higher than the snake
        float heightDifference = playerTransform.position.y - transform.position.y;
        if (heightDifference > elevationSafeThreshold)
        {
            return true;
        }

        // Check if player is on a boulder/rock
        Collider[] nearbyColliders = Physics.OverlapSphere(playerTransform.position, 1.5f);
        foreach (Collider col in nearbyColliders)
        {
            if (col.CompareTag("SafeRock") || col.CompareTag("Dock") || col.CompareTag("Platform"))
            {
                return true;
            }
        }

        return false;
    }

    void PlayHissSound()
    {
        if (audioSource == null) return;

        AudioClip hiss = GenerateHissSound();
        audioSource.PlayOneShot(hiss, 0.7f);
    }

    AudioClip GenerateHissSound()
    {
        int sampleRate = 22050;
        int samples = sampleRate / 2;
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Pow(1f - t, 0.5f);

            // White noise filtered for hiss
            float noise = Random.Range(-1f, 1f);
            // High pass filter simulation
            float hiss = noise * envelope * 0.4f;

            data[i] = hiss;
        }

        AudioClip clip = AudioClip.Create("Hiss", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    void PlayDeflectedSound()
    {
        if (audioSource == null) return;

        AudioClip deflect = GenerateDeflectSound();
        audioSource.PlayOneShot(deflect, 0.6f);

        // Show visual feedback to player
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Snake Charm protected you!", new Color(0.3f, 0.8f, 0.3f));
        }
    }

    AudioClip GenerateDeflectSound()
    {
        // Magical "ding" sound for deflection
        int sampleRate = 22050;
        int samples = sampleRate / 4;
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 8f);

            // Bell-like tone (multiple harmonics)
            float freq1 = 800f * 2f * Mathf.PI / sampleRate;
            float freq2 = 1200f * 2f * Mathf.PI / sampleRate;
            float freq3 = 1600f * 2f * Mathf.PI / sampleRate;

            float tone = Mathf.Sin(i * freq1) * 0.5f +
                        Mathf.Sin(i * freq2) * 0.3f +
                        Mathf.Sin(i * freq3) * 0.2f;

            data[i] = tone * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create("Deflect", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    System.Collections.IEnumerator RecoilAnimation()
    {
        // Snake recoils backward when charm deflects
        Vector3 originalPos = transform.position;
        Vector3 recoilPos = originalPos - transform.forward * 1f;

        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(originalPos, recoilPos, t / 0.2f);
            yield return null;
        }

        // Return to original position
        t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(recoilPos, originalPos, t / 0.3f);
            yield return null;
        }

        transform.position = originalPos;

        // Return to patrol after being deflected
        currentState = SnakeState.Returning;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        damageFlashTimer = 0.2f;

        PlayHissSound();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        currentState = SnakeState.Dead;
        totalSnakesKilled++;

        // Notify Rena for quest progress
        RenaCumbiaQueen rena = FindObjectOfType<RenaCumbiaQueen>();
        if (rena != null)
        {
            rena.OnSnakeKilled();
        }

        // Drop loot
        int goldDrop = Random.Range(30, 80);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(goldDrop);
        }

        // Drop snake skin
        if (Random.value < 0.4f)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"+{goldDrop}g + Snake Skin!", new Color(0.3f, 0.6f, 0.2f));
            }
            // Could add to inventory here
        }
        else if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"+{goldDrop}g", new Color(1f, 0.85f, 0.3f));
        }

        // Death animation
        StartCoroutine(DeathSequence());
    }

    System.Collections.IEnumerator DeathSequence()
    {
        float t = 0;
        Vector3 startScale = transform.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime;
            transform.localScale = startScale * (1f - t);
            transform.Rotate(0, 360 * Time.deltaTime, 0);
            yield return null;
        }

        Destroy(gameObject);
    }

    void UpdateDamageFlash()
    {
        if (damageFlashTimer > 0)
        {
            damageFlashTimer -= Time.deltaTime;
            float flash = Mathf.Sin(damageFlashTimer * 30f) > 0 ? 1f : 0f;

            foreach (var segment in bodySegments)
            {
                if (segment != null)
                {
                    Renderer r = segment.GetComponent<Renderer>();
                    if (r != null)
                    {
                        r.material.color = Color.Lerp(r.material.color, Color.red, flash * 0.5f);
                    }
                }
            }
        }
    }

    void OnGUI()
    {
        if (isDead || !MainMenu.GameStarted) return;

        // Health bar when damaged
        if (currentHealth < maxHealth && playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist < 15f)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1f);
                if (screenPos.z > 0)
                {
                    float barWidth = 50;
                    float barHeight = 6;
                    float barX = screenPos.x - barWidth / 2;
                    float barY = Screen.height - screenPos.y - 20;

                    // Background
                    GUI.color = Color.black;
                    GUI.DrawTexture(new Rect(barX - 1, barY - 1, barWidth + 2, barHeight + 2), Texture2D.whiteTexture);

                    // Health fill
                    float healthPercent = currentHealth / maxHealth;
                    GUI.color = Color.Lerp(Color.red, new Color(0.2f, 0.8f, 0.2f), healthPercent);
                    GUI.DrawTexture(new Rect(barX, barY, barWidth * healthPercent, barHeight), Texture2D.whiteTexture);

                    GUI.color = Color.white;
                }
            }
        }
    }
}
