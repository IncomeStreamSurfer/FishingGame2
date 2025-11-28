using UnityEngine;
using System.Collections;

public class FishingRodAnimator : MonoBehaviour
{
    private Transform rodPivot;
    private Transform rodTip;
    private Transform mainRod;

    // Bobber and line
    private GameObject bobber;
    private LineRenderer fishingLine;
    private GameObject lineObject;

    // Rod cosmetic visuals
    private int currentRodVisual = -1;
    private GameObject rodGlowEffect;
    private GameObject rodSmokeEffect;
    private Material rodMaterial;
    private Material tipMaterial;

    // State
    private bool isLineOut = false;
    private bool fishBiting = false;
    private bool waitingForBite = false;
    private float biteTimer = 0f;
    private float nextBiteTime = 0f;

    // Audio
    private AudioSource audioSource;

    // Charged casting
    private bool isCharging = false;
    private float chargeTime = 0f;
    private float maxChargeTime = 2f;
    private float minCastDistance = 6f;
    private float maxCastDistance = 25f;
    private float currentCastPower = 0f;

    // Cast distance affects fish rarity
    public float LastCastDistance { get; private set; }

    // Positions
    private Vector3 bobberTargetPos;
    private Vector3 bobberStartPos;

    // Idle rod rotation (held naturally at waist level, tip pointing slightly up and forward)
    private Quaternion idleRotation = Quaternion.Euler(45, 0, 0);
    // Cast back (rod lifted over shoulder behind head)
    private Quaternion castBackRotation = Quaternion.Euler(-100, 0, 0);
    // Cast forward (rod thrusts forward and down in casting motion)
    private Quaternion castForwardRotation = Quaternion.Euler(65, 0, 0);
    // Waiting position (rod held at comfortable angle over water, tip up slightly)
    private Quaternion waitingRotation = Quaternion.Euler(40, 0, 0);

    void Start()
    {
        rodPivot = transform.Find("RodPivot");

        // Find rod tip for line attachment
        if (rodPivot != null)
        {
            mainRod = rodPivot.Find("FishingRod");
            if (mainRod != null)
            {
                rodTip = mainRod.Find("RodTip");

                // Cache the rod materials for cosmetic updates
                Renderer rodRenderer = mainRod.GetComponent<Renderer>();
                if (rodRenderer != null)
                {
                    rodMaterial = rodRenderer.material;
                }
                if (rodTip != null)
                {
                    Renderer tipRenderer = rodTip.GetComponent<Renderer>();
                    if (tipRenderer != null)
                    {
                        tipMaterial = tipRenderer.material;
                    }
                }
            }
        }

        // Create fishing line renderer
        CreateFishingLine();

        // Set up audio
        SetupAudio();

        // Set initial idle position
        if (rodPivot != null)
        {
            rodPivot.localRotation = idleRotation;
        }
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.3f;
        audioSource.playOnAwake = false;
    }

    void PlayCastSound(float power)
    {
        StartCoroutine(GenerateCastSound(power));
    }

    System.Collections.IEnumerator GenerateCastSound(float power)
    {
        // Line whistle and reel unwind sound
        int sampleRate = 44100;
        float duration = 0.4f + power * 0.3f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip castClip = AudioClip.Create("CastSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Whistle - descending pitch
            float whistleFreq = Mathf.Lerp(2000f, 800f, progress) * (0.8f + power * 0.4f);
            float whistle = Mathf.Sin(2 * Mathf.PI * whistleFreq * t) * 0.15f;

            // Reel clicking sound
            float clickFreq = 30f + power * 20f;
            float click = (Mathf.Sin(2 * Mathf.PI * clickFreq * t) > 0.8f) ? 0.2f : 0f;

            // Line swoosh (filtered noise)
            float noise = (Random.value * 2f - 1f) * 0.1f;

            // Envelope
            float envelope = Mathf.Sin(progress * Mathf.PI);

            samples[i] = (whistle + click + noise) * envelope * 0.4f;
        }

        castClip.SetData(samples, 0);
        audioSource.clip = castClip;
        audioSource.pitch = 0.9f + power * 0.2f;
        audioSource.volume = 0.25f;
        audioSource.Play();

        yield return new WaitForSeconds(duration);
    }

    void PlaySplashSound(float power)
    {
        StartCoroutine(GenerateSplashSound(power));
    }

    System.Collections.IEnumerator GenerateSplashSound(float power)
    {
        // Water splash/plop sound
        int sampleRate = 44100;
        float duration = 0.3f + power * 0.2f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip splashClip = AudioClip.Create("SplashSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Low thud for impact
            float thud = Mathf.Sin(2 * Mathf.PI * 80f * t) * Mathf.Exp(-t * 15f) * 0.5f;

            // Water splash noise
            float splash = (Random.value * 2f - 1f) * Mathf.Exp(-t * 8f) * 0.4f;

            // Bubble sounds
            float bubbleFreq = 400f + Mathf.Sin(t * 50f) * 100f;
            float bubbles = Mathf.Sin(2 * Mathf.PI * bubbleFreq * t) * Mathf.Exp(-t * 6f) * 0.2f;

            samples[i] = (thud + splash + bubbles) * (0.5f + power * 0.5f);
        }

        splashClip.SetData(samples, 0);
        audioSource.clip = splashClip;
        audioSource.pitch = 0.9f + Random.Range(-0.1f, 0.1f);
        audioSource.volume = 0.35f;
        audioSource.Play();

        yield return new WaitForSeconds(duration);
    }

    void CreateFishingLine()
    {
        lineObject = new GameObject("FishingLine");
        lineObject.transform.SetParent(transform);

        fishingLine = lineObject.AddComponent<LineRenderer>();
        fishingLine.startWidth = 0.015f;
        fishingLine.endWidth = 0.01f;
        fishingLine.positionCount = 2;

        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        lineMat.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark fishing line
        fishingLine.material = lineMat;

        fishingLine.enabled = false;
    }

    void Update()
    {
        // Update rod cosmetics based on selected rod in UIManager
        UpdateRodCosmetics();

        // Update fishing line position
        if (isLineOut && bobber != null && fishingLine != null)
        {
            Vector3 tipPos = GetRodTipPosition();
            fishingLine.SetPosition(0, tipPos);
            fishingLine.SetPosition(1, bobber.transform.position + Vector3.up * 0.1f);
        }

        // Handle charging cast
        if (isCharging)
        {
            chargeTime += Time.deltaTime;
            // Loop the charge bar using ping-pong effect
            float loopedTime = Mathf.PingPong(chargeTime, maxChargeTime);
            currentCastPower = loopedTime / maxChargeTime;

            // Animate rod pulling back more as charge increases
            if (rodPivot != null)
            {
                float chargeAngle = Mathf.Lerp(45f, -100f, currentCastPower);
                rodPivot.localRotation = Quaternion.Euler(chargeAngle, 0, 0);
            }

            // Release to cast
            if (Input.GetMouseButtonUp(0))
            {
                isCharging = false;
                StartCoroutine(ChargedCastCoroutine(currentCastPower));
            }
        }

        // Handle fish biting animation
        if (waitingForBite && bobber != null)
        {
            biteTimer += Time.deltaTime;

            if (!fishBiting && biteTimer >= nextBiteTime)
            {
                // Fish starts biting!
                fishBiting = true;
                StartCoroutine(BobberBiteAnimation());
            }
        }

        // Check for reel in click
        if (fishBiting && Input.GetMouseButtonDown(0))
        {
            // Player clicked while fish is biting - catch it!
            ReelInFish(true);
        }
        else if (waitingForBite && !fishBiting && Input.GetMouseButtonDown(0))
        {
            // Player clicked too early - reel in empty
            ReelInFish(false);
        }
    }

    void UpdateRodCosmetics()
    {
        if (UIManager.Instance == null) return;

        int selectedRod = UIManager.Instance.GetSelectedRodIndex();
        if (selectedRod == currentRodVisual) return; // No change

        currentRodVisual = selectedRod;
        ApplyRodCosmetics(selectedRod);
    }

    void ApplyRodCosmetics(int rodIndex)
    {
        if (UIManager.Instance == null) return;

        Color rodColor = UIManager.Instance.GetRodColor(rodIndex);
        float metallic = UIManager.Instance.GetRodMetallic(rodIndex);
        float glossiness = UIManager.Instance.GetRodGlossiness(rodIndex);
        bool hasGlow = UIManager.Instance.GetRodHasGlow(rodIndex);
        bool hasSmoke = UIManager.Instance.GetRodHasSmoke(rodIndex);

        // Apply color and metallic/glossiness to rod material
        if (rodMaterial != null)
        {
            rodMaterial.color = rodColor;
            rodMaterial.SetFloat("_Metallic", metallic);
            rodMaterial.SetFloat("_Glossiness", glossiness);

            // Enable emission for glowing rods
            if (hasGlow)
            {
                rodMaterial.EnableKeyword("_EMISSION");
                Color glowColor = rodColor * 0.5f; // Subtle glow
                rodMaterial.SetColor("_EmissionColor", glowColor);
            }
            else
            {
                rodMaterial.DisableKeyword("_EMISSION");
            }
        }

        // Apply to tip as well
        if (tipMaterial != null)
        {
            tipMaterial.color = rodColor;
            tipMaterial.SetFloat("_Metallic", metallic);
            tipMaterial.SetFloat("_Glossiness", glossiness);

            if (hasGlow)
            {
                tipMaterial.EnableKeyword("_EMISSION");
                Color glowColor = rodColor * 0.5f;
                tipMaterial.SetColor("_EmissionColor", glowColor);
            }
            else
            {
                tipMaterial.DisableKeyword("_EMISSION");
            }
        }

        // Handle glow effect (aura around rod)
        if (hasGlow)
        {
            CreateRodGlow(rodColor);
        }
        else
        {
            DestroyRodGlow();
        }

        // Handle smoke effect (Epic rod only)
        if (hasSmoke)
        {
            CreateRodSmoke(rodColor);
        }
        else
        {
            DestroyRodSmoke();
        }

        Debug.Log($"Applied cosmetics for {UIManager.Instance.GetRodName(rodIndex)}");
    }

    void CreateRodGlow(Color glowColor)
    {
        DestroyRodGlow();

        if (mainRod == null) return;

        // Create glow effect using a larger transparent cylinder around the rod
        rodGlowEffect = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rodGlowEffect.name = "RodGlow";
        rodGlowEffect.transform.SetParent(mainRod.transform);
        rodGlowEffect.transform.localPosition = Vector3.zero;
        rodGlowEffect.transform.localRotation = Quaternion.identity;
        rodGlowEffect.transform.localScale = new Vector3(2.5f, 1.02f, 2.5f); // Slightly larger than rod

        Object.Destroy(rodGlowEffect.GetComponent<Collider>());

        // Create glowing transparent material
        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.SetFloat("_Mode", 3); // Transparent
        glowMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glowMat.SetInt("_ZWrite", 0);
        glowMat.EnableKeyword("_ALPHABLEND_ON");
        glowMat.renderQueue = 3100;

        Color transparentGlow = new Color(glowColor.r, glowColor.g, glowColor.b, 0.25f);
        glowMat.color = transparentGlow;
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", glowColor * 0.8f);

        rodGlowEffect.GetComponent<Renderer>().material = glowMat;

        // Animate the glow
        StartCoroutine(AnimateRodGlow());
    }

    IEnumerator AnimateRodGlow()
    {
        while (rodGlowEffect != null)
        {
            // Pulse the glow
            float pulse = 0.2f + Mathf.Sin(Time.time * 3f) * 0.1f;
            if (rodGlowEffect != null)
            {
                Material mat = rodGlowEffect.GetComponent<Renderer>().material;
                Color c = mat.color;
                c.a = pulse;
                mat.color = c;

                // Slight scale pulse
                float scalePulse = 2.5f + Mathf.Sin(Time.time * 2f) * 0.3f;
                rodGlowEffect.transform.localScale = new Vector3(scalePulse, 1.02f, scalePulse);
            }
            yield return null;
        }
    }

    void DestroyRodGlow()
    {
        if (rodGlowEffect != null)
        {
            Object.Destroy(rodGlowEffect);
            rodGlowEffect = null;
        }
    }

    void CreateRodSmoke(Color smokeColor)
    {
        DestroyRodSmoke();

        if (mainRod == null) return;

        // Create smoke effect container
        rodSmokeEffect = new GameObject("RodSmoke");
        rodSmokeEffect.transform.SetParent(mainRod.transform);
        rodSmokeEffect.transform.localPosition = new Vector3(0, 0.5f, 0);

        // Start smoke particle spawner
        StartCoroutine(SpawnSmokeParticles(smokeColor));
    }

    IEnumerator SpawnSmokeParticles(Color smokeColor)
    {
        while (rodSmokeEffect != null)
        {
            // Spawn a smoke puff
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = "SmokePuff";
            puff.transform.position = rodSmokeEffect.transform.position + Random.insideUnitSphere * 0.1f;
            puff.transform.localScale = Vector3.one * Random.Range(0.03f, 0.06f);
            Object.Destroy(puff.GetComponent<Collider>());

            Material puffMat = new Material(Shader.Find("Standard"));
            puffMat.SetFloat("_Mode", 3);
            puffMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            puffMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            puffMat.EnableKeyword("_ALPHABLEND_ON");
            puffMat.renderQueue = 3100;

            Color puffColor = new Color(smokeColor.r, smokeColor.g, smokeColor.b, 0.5f);
            puffMat.color = puffColor;
            puffMat.EnableKeyword("_EMISSION");
            puffMat.SetColor("_EmissionColor", smokeColor * 0.6f);

            puff.GetComponent<Renderer>().material = puffMat;

            StartCoroutine(AnimateSmokePuff(puff, puffMat, smokeColor));

            yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
        }
    }

    IEnumerator AnimateSmokePuff(GameObject puff, Material mat, Color baseColor)
    {
        float lifetime = Random.Range(0.6f, 1.2f);
        float t = 0;
        Vector3 velocity = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(0.5f, 1f),
            Random.Range(-0.3f, 0.3f)
        );

        Vector3 startScale = puff.transform.localScale;

        while (t < lifetime && puff != null)
        {
            t += Time.deltaTime;
            float progress = t / lifetime;

            // Rise and drift
            puff.transform.position += velocity * Time.deltaTime;
            velocity *= 0.98f; // Slow down

            // Grow and fade
            float scale = 1f + progress * 2f;
            puff.transform.localScale = startScale * scale;

            Color c = mat.color;
            c.a = 0.5f * (1f - progress);
            mat.color = c;

            yield return null;
        }

        if (puff != null)
        {
            Object.Destroy(puff);
        }
    }

    void DestroyRodSmoke()
    {
        if (rodSmokeEffect != null)
        {
            // Destroy all child smoke puffs
            foreach (Transform child in rodSmokeEffect.transform)
            {
                Object.Destroy(child.gameObject);
            }
            Object.Destroy(rodSmokeEffect);
            rodSmokeEffect = null;
        }
    }

    void OnGUI()
    {
        // Show charge meter when charging
        if (!MainMenu.GameStarted) return;

        if (isCharging)
        {
            DrawChargeMeter();
        }
    }

    void DrawChargeMeter()
    {
        float meterWidth = 170;
        float meterHeight = 16;
        float meterX = (Screen.width - meterWidth) / 2;
        float meterY = Screen.height - 130;

        // Background
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(meterX - 2, meterY - 2, meterWidth + 4, meterHeight + 4), Texture2D.whiteTexture);

        // Fill based on charge
        Color chargeColor = Color.Lerp(Color.yellow, Color.red, currentCastPower);
        GUI.color = chargeColor;
        GUI.DrawTexture(new Rect(meterX, meterY, meterWidth * currentCastPower, meterHeight), Texture2D.whiteTexture);

        // Border
        GUI.color = Color.white;

        // Text
        GUIStyle chargeStyle = new GUIStyle(GUI.skin.label);
        chargeStyle.fontSize = 11;
        chargeStyle.fontStyle = FontStyle.Bold;
        chargeStyle.alignment = TextAnchor.MiddleCenter;
        chargeStyle.normal.textColor = Color.white;

        string powerText = currentCastPower < 0.5f ? "SHORT CAST" :
                          currentCastPower < 0.8f ? "MEDIUM CAST" : "POWER CAST!";
        GUI.Label(new Rect(meterX, meterY - 20, meterWidth, 18), powerText, chargeStyle);

        // Distance preview
        float previewDist = Mathf.Lerp(minCastDistance, maxCastDistance, currentCastPower);
        chargeStyle.fontSize = 10;
        chargeStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        GUI.Label(new Rect(meterX, meterY + meterHeight + 3, meterWidth, 16), $"Distance: {previewDist:F0}m", chargeStyle);

        // Bonus hint
        if (currentCastPower > 0.7f)
        {
            chargeStyle.normal.textColor = new Color(1f, 0.8f, 0.3f);
            GUI.Label(new Rect(meterX, meterY + meterHeight + 18, meterWidth, 16), "Bigger fish in deep water!", chargeStyle);
        }

        GUI.color = Color.white;
    }

    Vector3 GetRodTipPosition()
    {
        if (rodTip != null)
        {
            return rodTip.position;
        }
        else if (rodPivot != null)
        {
            // Estimate tip position
            return rodPivot.position + rodPivot.forward * 2f + rodPivot.up * 1.5f;
        }
        return transform.position + Vector3.forward * 2f + Vector3.up * 2f;
    }

    public void CastLine()
    {
        // Only start charging if not already charging and not fishing
        if (!isLineOut && rodPivot != null && !isCharging && !rodBroken)
        {
            // Start charging
            isCharging = true;
            chargeTime = 0f;
            currentCastPower = 0f;
        }
    }

    // Returns true if currently in charging state
    public bool IsCharging()
    {
        return isCharging;
    }

    IEnumerator ChargedCastCoroutine(float power)
    {
        // Calculate cast distance based on power
        float castDistance = Mathf.Lerp(minCastDistance, maxCastDistance, power);
        LastCastDistance = castDistance;

        // Play cast sound
        PlayCastSound(power);

        // Quick forward cast animation (already pulled back during charge)
        float t = 0;
        Quaternion startRot = rodPivot.localRotation;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            rodPivot.localRotation = Quaternion.Slerp(startRot, castForwardRotation, EaseOutCubic(t));

            // Spawn bobber partway through cast
            if (t > 0.5f && bobber == null)
            {
                SpawnBobberCharged(castDistance, power);
            }

            yield return null;
        }

        // Move to waiting position
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            rodPivot.localRotation = Quaternion.Slerp(castForwardRotation, waitingRotation, EaseOutCubic(t));
            yield return null;
        }

        // Start waiting for fish
        isLineOut = true;
        waitingForBite = true;
        biteTimer = 0f;

        // Random time until fish bites (faster bite for power casts, but better fish)
        float speedBonus = UIManager.Instance != null ? UIManager.Instance.GetSpeedBonus() : 0f;
        float baseBiteTime = Mathf.Lerp(2f, 4f, power); // Longer wait for bigger cast, but better fish
        nextBiteTime = baseBiteTime * (1f - speedBonus);

        // Trigger bottle event check (1/100 chance per cast)
        if (BottleEventSystem.Instance != null)
        {
            BottleEventSystem.Instance.OnLineCast();
        }
    }

    void SpawnBobberCharged(float castDistance, float power)
    {
        // IMPORTANT: Destroy any existing bobber first to prevent duplicates
        if (bobber != null)
        {
            Object.Destroy(bobber);
            bobber = null;
        }

        // Calculate where bobber should land
        Vector3 castDirection = transform.forward;
        bobberTargetPos = transform.position + castDirection * castDistance;
        bobberTargetPos.y = 0.35f; // Water surface level

        // Start bobber at rod tip
        bobberStartPos = GetRodTipPosition();

        // Create bobber (same as before)
        CreateBobberObject();

        // Enable fishing line
        fishingLine.enabled = true;

        // Animate bobber flying to water with more arc for power casts
        StartCoroutine(BobberFlyAnimationCharged(power));
    }

    void CreateBobberObject()
    {
        bobber = new GameObject("Bobber");

        // Bobber body (red and white)
        GameObject bobberTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bobberTop.name = "BobberTop";
        bobberTop.transform.SetParent(bobber.transform);
        bobberTop.transform.localPosition = new Vector3(0, 0.08f, 0);
        bobberTop.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        Object.Destroy(bobberTop.GetComponent<Collider>());
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = new Color(0.9f, 0.15f, 0.1f);
        bobberTop.GetComponent<Renderer>().material = redMat;

        GameObject bobberBottom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bobberBottom.name = "BobberBottom";
        bobberBottom.transform.SetParent(bobber.transform);
        bobberBottom.transform.localPosition = new Vector3(0, -0.02f, 0);
        bobberBottom.transform.localScale = new Vector3(0.1f, 0.14f, 0.1f);
        Object.Destroy(bobberBottom.GetComponent<Collider>());
        Material whiteMat = new Material(Shader.Find("Standard"));
        whiteMat.color = Color.white;
        bobberBottom.GetComponent<Renderer>().material = whiteMat;

        // Small stick on top
        GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stick.name = "BobberStick";
        stick.transform.SetParent(bobber.transform);
        stick.transform.localPosition = new Vector3(0, 0.18f, 0);
        stick.transform.localScale = new Vector3(0.02f, 0.06f, 0.02f);
        Object.Destroy(stick.GetComponent<Collider>());
        Material stickMat = new Material(Shader.Find("Standard"));
        stickMat.color = new Color(0.3f, 0.2f, 0.1f);
        stick.GetComponent<Renderer>().material = stickMat;

        bobber.transform.position = bobberStartPos;
    }

    IEnumerator BobberFlyAnimationCharged(float power)
    {
        float t = 0;
        float arcHeight = 3f + power * 4f; // Higher arc for power casts
        float flySpeed = 1.5f + power * 1f; // Faster for power casts

        while (t < 1)
        {
            t += Time.deltaTime * flySpeed;

            // Arc trajectory
            Vector3 pos = Vector3.Lerp(bobberStartPos, bobberTargetPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            bobber.transform.position = pos;

            // Spin while flying
            bobber.transform.Rotate(Vector3.up * 180 * Time.deltaTime);

            yield return null;
        }

        // Bigger splash for power casts
        CreateSplash(bobberTargetPos, power);

        // Bobber settles in water
        bobber.transform.position = bobberTargetPos;

        // Start gentle bobbing animation
        StartCoroutine(BobberIdleAnimation());
    }

    IEnumerator CastCoroutine()
    {
        // Wind back
        float t = 0;
        Quaternion startRot = rodPivot.localRotation;
        while (t < 1)
        {
            t += Time.deltaTime * 3f;
            rodPivot.localRotation = Quaternion.Slerp(startRot, castBackRotation, EaseOutCubic(t));
            yield return null;
        }

        // Small pause at back
        yield return new WaitForSeconds(0.15f);

        // Cast forward quickly
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            rodPivot.localRotation = Quaternion.Slerp(castBackRotation, castForwardRotation, EaseOutCubic(t));

            // Spawn bobber partway through cast
            if (t > 0.6f && bobber == null)
            {
                SpawnBobber();
            }

            yield return null;
        }

        // Move to waiting position
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            rodPivot.localRotation = Quaternion.Slerp(castForwardRotation, waitingRotation, EaseOutCubic(t));
            yield return null;
        }

        // Start waiting for fish
        isLineOut = true;
        waitingForBite = true;
        biteTimer = 0f;

        // Random time until fish bites (affected by rod speed bonus)
        float speedBonus = UIManager.Instance != null ? UIManager.Instance.GetSpeedBonus() : 0f;
        nextBiteTime = Random.Range(2f, 5f) * (1f - speedBonus);

        // Trigger bottle event check (1/100 chance per cast)
        if (BottleEventSystem.Instance != null)
        {
            BottleEventSystem.Instance.OnLineCast();
        }
    }

    void SpawnBobber()
    {
        // IMPORTANT: Destroy any existing bobber first to prevent duplicates
        if (bobber != null)
        {
            Object.Destroy(bobber);
            bobber = null;
        }

        // Calculate where bobber should land (in front of player, in the water)
        Vector3 castDirection = transform.forward;
        float castDistance = Random.Range(6f, 10f);
        bobberTargetPos = transform.position + castDirection * castDistance;
        bobberTargetPos.y = 0.35f; // Water surface level

        // Start bobber at rod tip
        bobberStartPos = GetRodTipPosition();

        // Create bobber
        bobber = new GameObject("Bobber");

        // Bobber body (red and white)
        GameObject bobberTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bobberTop.name = "BobberTop";
        bobberTop.transform.SetParent(bobber.transform);
        bobberTop.transform.localPosition = new Vector3(0, 0.08f, 0);
        bobberTop.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        Object.Destroy(bobberTop.GetComponent<Collider>());
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = new Color(0.9f, 0.15f, 0.1f);
        bobberTop.GetComponent<Renderer>().material = redMat;

        GameObject bobberBottom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bobberBottom.name = "BobberBottom";
        bobberBottom.transform.SetParent(bobber.transform);
        bobberBottom.transform.localPosition = new Vector3(0, -0.02f, 0);
        bobberBottom.transform.localScale = new Vector3(0.1f, 0.14f, 0.1f);
        Object.Destroy(bobberBottom.GetComponent<Collider>());
        Material whiteMat = new Material(Shader.Find("Standard"));
        whiteMat.color = Color.white;
        bobberBottom.GetComponent<Renderer>().material = whiteMat;

        // Small stick on top
        GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stick.name = "BobberStick";
        stick.transform.SetParent(bobber.transform);
        stick.transform.localPosition = new Vector3(0, 0.18f, 0);
        stick.transform.localScale = new Vector3(0.02f, 0.06f, 0.02f);
        Object.Destroy(stick.GetComponent<Collider>());
        Material stickMat = new Material(Shader.Find("Standard"));
        stickMat.color = new Color(0.3f, 0.2f, 0.1f);
        stick.GetComponent<Renderer>().material = stickMat;

        bobber.transform.position = bobberStartPos;

        // Enable fishing line
        fishingLine.enabled = true;

        // Animate bobber flying to water
        StartCoroutine(BobberFlyAnimation());
    }

    IEnumerator BobberFlyAnimation()
    {
        float t = 0;
        float arcHeight = 3f;

        while (t < 1)
        {
            t += Time.deltaTime * 2f;

            // Arc trajectory
            Vector3 pos = Vector3.Lerp(bobberStartPos, bobberTargetPos, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            bobber.transform.position = pos;

            // Spin slightly while flying
            bobber.transform.Rotate(Vector3.up * 180 * Time.deltaTime);

            yield return null;
        }

        // Splash effect when landing
        CreateSplash(bobberTargetPos);

        // Bobber settles in water
        bobber.transform.position = bobberTargetPos;

        // Start gentle bobbing animation
        StartCoroutine(BobberIdleAnimation());
    }

    IEnumerator BobberIdleAnimation()
    {
        while (isLineOut && bobber != null && !fishBiting)
        {
            // Gentle bobbing motion
            float bobAmount = Mathf.Sin(Time.time * 2f) * 0.03f;
            float swayX = Mathf.Sin(Time.time * 0.8f) * 0.02f;
            float swayZ = Mathf.Cos(Time.time * 0.6f) * 0.02f;

            bobber.transform.position = bobberTargetPos + new Vector3(swayX, bobAmount, swayZ);

            // Slight rotation
            bobber.transform.rotation = Quaternion.Euler(
                Mathf.Sin(Time.time * 1.5f) * 5f,
                Time.time * 10f,
                Mathf.Cos(Time.time * 1.2f) * 5f
            );

            yield return null;
        }
    }

    IEnumerator BobberBiteAnimation()
    {
        Debug.Log("Fish is biting! Click to reel in!");

        float biteDuration = 3f; // Player has 3 seconds to react
        float biteTime = 0f;

        while (fishBiting && biteTime < biteDuration && bobber != null)
        {
            biteTime += Time.deltaTime;

            // Aggressive bobbing - dipping under water
            float dipAmount = Mathf.Sin(Time.time * 12f) * 0.15f;
            float strongDip = Mathf.Sin(Time.time * 3f) * 0.1f;

            // Occasional strong pull under
            if (Random.value < 0.02f)
            {
                dipAmount -= 0.2f;
            }

            Vector3 bitePos = bobberTargetPos + new Vector3(
                Mathf.Sin(Time.time * 8f) * 0.08f,
                dipAmount + strongDip - 0.1f,
                Mathf.Cos(Time.time * 6f) * 0.08f
            );

            bobber.transform.position = bitePos;

            // Erratic rotation
            bobber.transform.rotation = Quaternion.Euler(
                Mathf.Sin(Time.time * 15f) * 20f,
                Time.time * 50f,
                Mathf.Cos(Time.time * 12f) * 20f
            );

            yield return null;
        }

        // Fish got away if we're still here
        if (fishBiting)
        {
            Debug.Log("The fish got away! Too slow!");
            ReelInFish(false);
        }
    }

    void ReelInFish(bool caughtFish)
    {
        fishBiting = false;
        waitingForBite = false;

        StartCoroutine(ReelInCoroutine(caughtFish));
    }

    IEnumerator ReelInCoroutine(bool caughtFish)
    {
        if (bobber == null) yield break;

        // Reel animation on rod
        StartCoroutine(RodReelAnimation());

        // Bobber comes back to player
        Vector3 startPos = bobber.transform.position;
        Vector3 endPos = GetRodTipPosition();
        float t = 0;

        while (t < 1 && bobber != null)
        {
            t += Time.deltaTime * 3f;

            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            // Small arc
            pos.y += Mathf.Sin(t * Mathf.PI) * 0.5f;

            bobber.transform.position = pos;

            yield return null;
        }

        // Destroy bobber
        if (bobber != null)
        {
            Object.Destroy(bobber);
            bobber = null;
        }

        // Hide fishing line
        fishingLine.enabled = false;
        isLineOut = false;

        // Trigger catch in fishing system
        if (caughtFish && FishingSystem.Instance != null)
        {
            FishingSystem.Instance.CompleteCatch();
        }
        else
        {
            Debug.Log("Nothing caught this time.");
            if (FishingSystem.Instance != null)
            {
                FishingSystem.Instance.ResetFishing();
            }
        }

        // Return rod to idle
        yield return new WaitForSeconds(0.5f);

        t = 0;
        Quaternion currentRot = rodPivot.localRotation;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            rodPivot.localRotation = Quaternion.Slerp(currentRot, idleRotation, EaseOutCubic(t));
            yield return null;
        }
    }

    IEnumerator RodReelAnimation()
    {
        // Bob the rod up and down like reeling
        for (int i = 0; i < 3; i++)
        {
            float t = 0;
            Quaternion upRot = Quaternion.Euler(10, 15, 0);
            Quaternion downRot = Quaternion.Euler(35, 15, 5);

            while (t < 1)
            {
                t += Time.deltaTime * 6f;
                rodPivot.localRotation = Quaternion.Slerp(waitingRotation, upRot, t);
                yield return null;
            }
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 6f;
                rodPivot.localRotation = Quaternion.Slerp(upRot, downRot, t);
                yield return null;
            }
        }
    }

    void CreateSplash(Vector3 pos, float power = 0.5f)
    {
        // Play splash sound
        PlaySplashSound(power);

        // Create water ripples spreading outward
        int numRipples = 4 + Mathf.RoundToInt(power * 3);

        // Initial splash droplets shooting up
        int numDroplets = 5 + Mathf.RoundToInt(power * 8);
        for (int i = 0; i < numDroplets; i++)
        {
            StartCoroutine(SpawnSplashDroplet(pos, power, i * 0.02f));
        }

        // Concentric ripple rings
        for (int i = 0; i < numRipples; i++)
        {
            StartCoroutine(SpawnRippleRing(pos, i * 0.15f, power));
        }

        // Central splash disturbance
        GameObject centralSplash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        centralSplash.name = "CentralSplash";
        centralSplash.transform.position = new Vector3(pos.x, 0.3f, pos.z);
        centralSplash.transform.localScale = Vector3.one * (0.15f + power * 0.2f);
        Object.Destroy(centralSplash.GetComponent<Collider>());

        Material splashMat = new Material(Shader.Find("Standard"));
        splashMat.SetFloat("_Mode", 3);
        splashMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        splashMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        splashMat.EnableKeyword("_ALPHABLEND_ON");
        splashMat.color = new Color(0.8f, 0.9f, 1f, 0.7f);
        centralSplash.GetComponent<Renderer>().material = splashMat;

        StartCoroutine(AnimateCentralSplash(centralSplash));
    }

    IEnumerator SpawnSplashDroplet(Vector3 pos, float power, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject droplet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        droplet.name = "SplashDroplet";
        droplet.transform.position = pos + Vector3.up * 0.1f;
        droplet.transform.localScale = Vector3.one * Random.Range(0.03f, 0.06f + power * 0.03f);
        Object.Destroy(droplet.GetComponent<Collider>());

        Material dropMat = new Material(Shader.Find("Standard"));
        dropMat.SetFloat("_Mode", 3);
        dropMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        dropMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        dropMat.EnableKeyword("_ALPHABLEND_ON");
        dropMat.color = new Color(0.7f, 0.85f, 1f, 0.8f);
        dropMat.SetFloat("_Glossiness", 0.9f);
        droplet.GetComponent<Renderer>().material = dropMat;

        // Random upward velocity
        Vector3 velocity = new Vector3(
            Random.Range(-1f, 1f) * (0.5f + power),
            Random.Range(2f, 4f) * (0.5f + power),
            Random.Range(-1f, 1f) * (0.5f + power)
        );

        float t = 0;
        while (t < 1.5f && droplet != null)
        {
            t += Time.deltaTime;
            velocity.y -= 9.8f * Time.deltaTime; // Gravity
            droplet.transform.position += velocity * Time.deltaTime;

            // Fade out
            Color c = dropMat.color;
            c.a = Mathf.Lerp(0.8f, 0f, t / 1.5f);
            dropMat.color = c;

            // Stop at water surface
            if (droplet.transform.position.y < 0.25f)
            {
                break;
            }

            yield return null;
        }

        if (droplet != null) Object.Destroy(droplet);
    }

    IEnumerator SpawnRippleRing(Vector3 pos, float delay, float power)
    {
        yield return new WaitForSeconds(delay);

        // Create a torus-like ripple ring using a flattened cylinder
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "WaterRipple";
        ring.transform.position = new Vector3(pos.x, 0.26f, pos.z);
        ring.transform.localScale = new Vector3(0.1f, 0.005f, 0.1f);
        Object.Destroy(ring.GetComponent<Collider>());

        Material rippleMat = new Material(Shader.Find("Standard"));
        rippleMat.SetFloat("_Mode", 3);
        rippleMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        rippleMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        rippleMat.EnableKeyword("_ALPHABLEND_ON");
        rippleMat.renderQueue = 3100;
        rippleMat.color = new Color(0.9f, 0.95f, 1f, 0.5f);
        ring.GetComponent<Renderer>().material = rippleMat;

        float maxScale = 2f + power * 2.5f;
        float duration = 1.5f + power * 0.5f;
        float t = 0;

        while (t < duration && ring != null)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            // Expand outward
            float scale = Mathf.Lerp(0.1f, maxScale, EaseOutQuad(progress));
            ring.transform.localScale = new Vector3(scale, 0.005f, scale);

            // Slight wave height
            float waveHeight = Mathf.Sin(progress * Mathf.PI * 2f) * 0.02f * (1f - progress);
            ring.transform.position = new Vector3(pos.x, 0.26f + waveHeight, pos.z);

            // Fade out
            Color c = rippleMat.color;
            c.a = Mathf.Lerp(0.5f, 0f, progress);
            rippleMat.color = c;

            yield return null;
        }

        if (ring != null) Object.Destroy(ring);
    }

    IEnumerator AnimateCentralSplash(GameObject splash)
    {
        Material mat = splash.GetComponent<Renderer>().material;
        Vector3 startScale = splash.transform.localScale;
        float t = 0;

        while (t < 0.4f && splash != null)
        {
            t += Time.deltaTime;
            float progress = t / 0.4f;

            // Expand then shrink
            float scaleMult = 1f + Mathf.Sin(progress * Mathf.PI) * 0.5f;
            splash.transform.localScale = startScale * scaleMult;

            // Rise slightly then fall
            float height = 0.3f + Mathf.Sin(progress * Mathf.PI) * 0.15f;
            Vector3 pos = splash.transform.position;
            pos.y = height;
            splash.transform.position = pos;

            // Fade out
            Color c = mat.color;
            c.a = Mathf.Lerp(0.7f, 0f, progress);
            mat.color = c;

            yield return null;
        }

        if (splash != null) Object.Destroy(splash);
    }

    float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    // Legacy splash for compatibility
    IEnumerator ExpandSplash(GameObject ring, float delay, float power = 0.5f)
    {
        yield return new WaitForSeconds(delay);

        float t = 0;
        Material mat = ring.GetComponent<Renderer>().material;
        float maxScale = 1.5f + power * 1.5f;

        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            float scale = Mathf.Lerp(0.2f, maxScale, t);
            ring.transform.localScale = new Vector3(scale, 0.01f, scale);

            Color c = mat.color;
            c.a = Mathf.Lerp(0.6f, 0f, t);
            mat.color = c;

            yield return null;
        }

        Object.Destroy(ring);
    }

    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public bool IsLineOut()
    {
        return isLineOut;
    }

    public bool IsFishBiting()
    {
        return fishBiting;
    }

    // Called when a humpback whale breaks the rod!
    private bool rodBroken = false;

    public void BreakRod()
    {
        if (rodBroken) return;
        rodBroken = true;

        StartCoroutine(RodBreakAnimation());
    }

    IEnumerator RodBreakAnimation()
    {
        // Stop any current fishing
        isLineOut = false;
        fishBiting = false;
        waitingForBite = false;

        // Destroy bobber if exists
        if (bobber != null)
        {
            Object.Destroy(bobber);
            bobber = null;
        }

        // Hide fishing line
        if (fishingLine != null)
        {
            fishingLine.enabled = false;
        }

        // Find the rod parts
        if (rodPivot != null)
        {
            // Shake the rod violently
            float shakeTime = 0.5f;
            float t = 0;
            while (t < shakeTime)
            {
                t += Time.deltaTime;
                float shake = Mathf.Sin(t * 60f) * 15f * (1f - t / shakeTime);
                rodPivot.localRotation = Quaternion.Euler(30 + shake, shake * 0.5f, shake * 0.3f);
                yield return null;
            }

            // Break effect - spawn broken rod pieces
            Transform rod = rodPivot.Find("FishingRod");
            if (rod != null)
            {
                // Get rod position before hiding
                Vector3 breakPos = rod.position;
                Quaternion breakRot = rod.rotation;

                // Hide original rod
                rod.gameObject.SetActive(false);

                // Spawn broken pieces
                Material brokenMat = new Material(Shader.Find("Standard"));
                brokenMat.color = new Color(0.45f, 0.3f, 0.15f);

                // Lower piece
                GameObject lowerPiece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lowerPiece.name = "BrokenRodLower";
                lowerPiece.transform.position = breakPos;
                lowerPiece.transform.rotation = breakRot;
                lowerPiece.transform.localScale = new Vector3(0.06f, 0.5f, 0.06f);
                lowerPiece.GetComponent<Renderer>().material = brokenMat;
                Object.Destroy(lowerPiece.GetComponent<Collider>());

                // Upper piece (flies off)
                GameObject upperPiece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                upperPiece.name = "BrokenRodUpper";
                upperPiece.transform.position = breakPos + Vector3.up * 0.8f + Vector3.forward * 0.3f;
                upperPiece.transform.rotation = breakRot * Quaternion.Euler(45, 30, 0);
                upperPiece.transform.localScale = new Vector3(0.04f, 0.6f, 0.04f);
                upperPiece.GetComponent<Renderer>().material = brokenMat;
                Object.Destroy(upperPiece.GetComponent<Collider>());

                // Animate upper piece flying into water
                StartCoroutine(FlyingBrokenPiece(upperPiece));

                // Make lower piece drop
                Rigidbody lowerRb = lowerPiece.AddComponent<Rigidbody>();
                lowerRb.mass = 0.5f;

                // Cleanup after delay
                Object.Destroy(lowerPiece, 5f);
            }
        }

        // Wait then restore rod (player gets a new one automatically after a few seconds)
        yield return new WaitForSeconds(4f);

        // Restore the rod
        if (rodPivot != null)
        {
            Transform rod = rodPivot.Find("FishingRod");
            if (rod != null)
            {
                rod.gameObject.SetActive(true);
            }
            rodPivot.localRotation = idleRotation;
        }

        rodBroken = false;

        // Show message
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Got a new fishing rod!", Color.green);
        }
    }

    IEnumerator FlyingBrokenPiece(GameObject piece)
    {
        Vector3 startPos = piece.transform.position;
        Vector3 velocity = new Vector3(Random.Range(-2f, 2f), 3f, Random.Range(2f, 5f));
        Vector3 angularVel = new Vector3(Random.Range(-500f, 500f), Random.Range(-500f, 500f), Random.Range(-500f, 500f));

        float t = 0;
        while (t < 2f && piece != null)
        {
            t += Time.deltaTime;
            velocity.y -= 15f * Time.deltaTime; // Gravity

            piece.transform.position += velocity * Time.deltaTime;
            piece.transform.Rotate(angularVel * Time.deltaTime);

            // Stop when hitting water
            if (piece.transform.position.y < 0.3f)
            {
                // Small splash
                if (WaterEffect.Instance != null)
                {
                    WaterEffect.Instance.CreateSplash(piece.transform.position);
                }
                break;
            }

            yield return null;
        }

        if (piece != null)
        {
            Object.Destroy(piece);
        }
    }

    public bool IsRodBroken()
    {
        return rodBroken;
    }
}
