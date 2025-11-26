using UnityEngine;
using System.Collections;

public class FishingRodAnimator : MonoBehaviour
{
    private Transform rodPivot;
    private Transform rodTip;

    // Bobber and line
    private GameObject bobber;
    private LineRenderer fishingLine;
    private GameObject lineObject;

    // State
    private bool isLineOut = false;
    private bool fishBiting = false;
    private bool waitingForBite = false;
    private float biteTimer = 0f;
    private float nextBiteTime = 0f;

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
            Transform rod = rodPivot.Find("FishingRod");
            if (rod != null)
            {
                rodTip = rod.Find("RodTip");
            }
        }

        // Create fishing line renderer
        CreateFishingLine();

        // Set initial idle position
        if (rodPivot != null)
        {
            rodPivot.localRotation = idleRotation;
        }
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
            chargeTime = Mathf.Min(chargeTime, maxChargeTime);
            currentCastPower = chargeTime / maxChargeTime;

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
        float meterWidth = 200;
        float meterHeight = 20;
        float meterX = (Screen.width - meterWidth) / 2;
        float meterY = Screen.height - 150;

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
        chargeStyle.fontSize = 14;
        chargeStyle.fontStyle = FontStyle.Bold;
        chargeStyle.alignment = TextAnchor.MiddleCenter;
        chargeStyle.normal.textColor = Color.white;

        string powerText = currentCastPower < 0.5f ? "SHORT CAST" :
                          currentCastPower < 0.8f ? "MEDIUM CAST" : "POWER CAST!";
        GUI.Label(new Rect(meterX, meterY - 25, meterWidth, 22), powerText, chargeStyle);

        // Distance preview
        float previewDist = Mathf.Lerp(minCastDistance, maxCastDistance, currentCastPower);
        chargeStyle.fontSize = 12;
        chargeStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        GUI.Label(new Rect(meterX, meterY + meterHeight + 5, meterWidth, 20), $"Distance: {previewDist:F0}m", chargeStyle);

        // Bonus hint
        if (currentCastPower > 0.7f)
        {
            chargeStyle.normal.textColor = new Color(1f, 0.8f, 0.3f);
            GUI.Label(new Rect(meterX, meterY + meterHeight + 25, meterWidth, 20), "Bigger fish in deep water!", chargeStyle);
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
        if (!isLineOut && rodPivot != null && !isCharging)
        {
            // Start charging
            isCharging = true;
            chargeTime = 0f;
            currentCastPower = 0f;
        }
    }

    IEnumerator ChargedCastCoroutine(float power)
    {
        // Calculate cast distance based on power
        float castDistance = Mathf.Lerp(minCastDistance, maxCastDistance, power);
        LastCastDistance = castDistance;

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
        // More splash rings for power casts
        int numRings = 3 + Mathf.RoundToInt(power * 3);
        float splashSize = 0.2f + power * 0.3f;

        for (int i = 0; i < numRings; i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Splash";
            ring.transform.position = new Vector3(pos.x, 0.25f, pos.z);
            ring.transform.localScale = new Vector3(splashSize, 0.01f, splashSize);
            Object.Destroy(ring.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.color = new Color(1f, 1f, 1f, 0.6f);
            ring.GetComponent<Renderer>().material = mat;

            StartCoroutine(ExpandSplash(ring, i * 0.08f, power));
        }
    }

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
