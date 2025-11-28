using UnityEngine;
using System.Collections;

/// <summary>
/// Tutorial cat that appears at level 2 to teach the player about selling fish
/// Appears out of thin air, gives tip, then disappears into smoke
/// </summary>
public class TutorialCat : MonoBehaviour
{
    public static TutorialCat Instance { get; private set; }

    private bool hasShownLevel2Tip = false;
    private int lastCheckedLevel = 0;

    // Cat visual
    private GameObject catObject;
    private bool catVisible = false;
    private bool showingMessage = false;
    private float messageTimer = 0f;
    private string catMessage = "";

    // Smoke particles
    private GameObject smokeEffect;

    // Audio
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.4f;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check for level up
        if (LevelingSystem.Instance != null)
        {
            int currentLevel = LevelingSystem.Instance.GetLevel();

            // Check if player just reached level 2
            if (currentLevel >= 2 && !hasShownLevel2Tip)
            {
                hasShownLevel2Tip = true;
                StartCoroutine(SpawnCatWithTip());
            }

            lastCheckedLevel = currentLevel;
        }

        // ESC to dismiss cat message
        if (showingMessage && Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(DismissCat());
        }

        // Update message timer
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f && showingMessage)
            {
                StartCoroutine(DismissCat());
            }
        }
    }

    IEnumerator SpawnCatWithTip()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject player = GameObject.Find("Player");
        if (player == null) yield break;

        // Spawn position near player
        Vector3 spawnPos = player.transform.position + player.transform.right * 2f;
        spawnPos.y = player.transform.position.y;

        // Create smoke poof effect
        CreateSmokeEffect(spawnPos);
        yield return new WaitForSeconds(0.3f);

        // Create the cat
        CreateCat(spawnPos);
        catVisible = true;

        // Play meow sound!
        PlayMeow();

        // Wait a moment, then show message
        yield return new WaitForSeconds(0.5f);

        showingMessage = true;
        catMessage = "You know, you can sell your fish for gold if you press F while talking to any resident on this island!";
        messageTimer = 8f;
    }

    void CreateSmokeEffect(Vector3 position)
    {
        smokeEffect = new GameObject("CatSmoke");
        smokeEffect.transform.position = position + Vector3.up * 0.5f;

        // Create multiple smoke puffs
        for (int i = 0; i < 8; i++)
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.transform.SetParent(smokeEffect.transform);

            float angle = (i / 8f) * Mathf.PI * 2f;
            puff.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.3f, Random.Range(-0.2f, 0.3f), Mathf.Sin(angle) * 0.3f);
            puff.transform.localScale = Vector3.one * Random.Range(0.3f, 0.5f);

            Material smokeMat = new Material(Shader.Find("Standard"));
            smokeMat.color = new Color(0.7f, 0.7f, 0.75f, 0.6f);
            puff.GetComponent<Renderer>().material = smokeMat;
            Object.Destroy(puff.GetComponent<Collider>());

            StartCoroutine(AnimateSmokePuff(puff, 0.8f));
        }
    }

    IEnumerator AnimateSmokePuff(GameObject puff, float duration)
    {
        float elapsed = 0f;
        Vector3 startScale = puff.transform.localScale;
        Vector3 startPos = puff.transform.localPosition;

        while (elapsed < duration && puff != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Expand and rise
            puff.transform.localScale = startScale * (1f + t * 2f);
            puff.transform.localPosition = startPos + Vector3.up * t * 1.5f;

            // Fade out
            Renderer r = puff.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a = 0.6f * (1f - t);
                r.material.color = c;
            }

            yield return null;
        }

        if (puff != null) Destroy(puff);
    }

    void CreateCat(Vector3 position)
    {
        catObject = new GameObject("TutorialCat");
        catObject.transform.position = position;

        // Face the player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Vector3 lookDir = player.transform.position - position;
            lookDir.y = 0;
            if (lookDir.magnitude > 0.1f)
            {
                catObject.transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        // Materials
        Material orangeFur = new Material(Shader.Find("Standard"));
        orangeFur.color = new Color(0.9f, 0.6f, 0.2f);

        Material whiteFur = new Material(Shader.Find("Standard"));
        whiteFur.color = new Color(0.95f, 0.95f, 0.9f);

        Material pinkMat = new Material(Shader.Find("Standard"));
        pinkMat.color = new Color(1f, 0.7f, 0.75f);

        Material blackMat = new Material(Shader.Find("Standard"));
        blackMat.color = Color.black;

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "CatBody";
        body.transform.SetParent(catObject.transform);
        body.transform.localPosition = new Vector3(0, 0.35f, 0);
        body.transform.localRotation = Quaternion.Euler(90, 0, 0);
        body.transform.localScale = new Vector3(0.3f, 0.4f, 0.25f);
        body.GetComponent<Renderer>().material = orangeFur;
        Object.Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "CatHead";
        head.transform.SetParent(catObject.transform);
        head.transform.localPosition = new Vector3(0, 0.55f, 0.25f);
        head.transform.localScale = new Vector3(0.35f, 0.3f, 0.3f);
        head.GetComponent<Renderer>().material = orangeFur;
        Object.Destroy(head.GetComponent<Collider>());

        // Ears
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ear.name = "CatEar";
            ear.transform.SetParent(catObject.transform);
            ear.transform.localPosition = new Vector3(side * 0.12f, 0.72f, 0.2f);
            ear.transform.localRotation = Quaternion.Euler(0, 0, side * -15);
            ear.transform.localScale = new Vector3(0.08f, 0.12f, 0.05f);
            ear.GetComponent<Renderer>().material = orangeFur;
            Object.Destroy(ear.GetComponent<Collider>());

            // Inner ear (pink)
            GameObject innerEar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            innerEar.transform.SetParent(ear.transform);
            innerEar.transform.localPosition = new Vector3(0, 0, 0.3f);
            innerEar.transform.localScale = new Vector3(0.6f, 0.7f, 0.3f);
            innerEar.GetComponent<Renderer>().material = pinkMat;
            Object.Destroy(innerEar.GetComponent<Collider>());
        }

        // Eyes
        for (int side = -1; side <= 1; side += 2)
        {
            // Eye white
            GameObject eyeWhite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeWhite.transform.SetParent(catObject.transform);
            eyeWhite.transform.localPosition = new Vector3(side * 0.08f, 0.58f, 0.38f);
            eyeWhite.transform.localScale = new Vector3(0.08f, 0.1f, 0.05f);
            eyeWhite.GetComponent<Renderer>().material = whiteFur;
            Object.Destroy(eyeWhite.GetComponent<Collider>());

            // Pupil (vertical slit)
            GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pupil.transform.SetParent(catObject.transform);
            pupil.transform.localPosition = new Vector3(side * 0.08f, 0.58f, 0.41f);
            pupil.transform.localScale = new Vector3(0.02f, 0.07f, 0.02f);
            pupil.GetComponent<Renderer>().material = blackMat;
            Object.Destroy(pupil.GetComponent<Collider>());
        }

        // Nose
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        nose.transform.SetParent(catObject.transform);
        nose.transform.localPosition = new Vector3(0, 0.52f, 0.42f);
        nose.transform.localScale = new Vector3(0.04f, 0.03f, 0.03f);
        nose.GetComponent<Renderer>().material = pinkMat;
        Object.Destroy(nose.GetComponent<Collider>());

        // Tail
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        tail.name = "CatTail";
        tail.transform.SetParent(catObject.transform);
        tail.transform.localPosition = new Vector3(0, 0.35f, -0.35f);
        tail.transform.localRotation = Quaternion.Euler(-45, 0, 0);
        tail.transform.localScale = new Vector3(0.08f, 0.25f, 0.08f);
        tail.GetComponent<Renderer>().material = orangeFur;
        Object.Destroy(tail.GetComponent<Collider>());

        // Legs
        for (int i = 0; i < 4; i++)
        {
            float xOff = (i % 2 == 0) ? -0.1f : 0.1f;
            float zOff = (i < 2) ? 0.15f : -0.15f;

            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.transform.SetParent(catObject.transform);
            leg.transform.localPosition = new Vector3(xOff, 0.12f, zOff);
            leg.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
            leg.GetComponent<Renderer>().material = orangeFur;
            Object.Destroy(leg.GetComponent<Collider>());
        }

        // Animate cat idle
        StartCoroutine(AnimateCatIdle());
    }

    IEnumerator AnimateCatIdle()
    {
        float time = 0f;
        Transform tail = catObject?.transform.Find("CatTail");

        while (catObject != null && catVisible)
        {
            time += Time.deltaTime;

            // Tail swish
            if (tail != null)
            {
                float swish = Mathf.Sin(time * 3f) * 20f;
                tail.localRotation = Quaternion.Euler(-45, swish, 0);
            }

            yield return null;
        }
    }

    IEnumerator DismissCat()
    {
        showingMessage = false;
        catMessage = "";

        if (catObject != null)
        {
            // Create disappear smoke
            CreateSmokeEffect(catObject.transform.position);

            yield return new WaitForSeconds(0.2f);

            // Destroy cat
            Destroy(catObject);
            catObject = null;
            catVisible = false;
        }

        // Clean up smoke after a moment
        yield return new WaitForSeconds(1f);
        if (smokeEffect != null)
        {
            Destroy(smokeEffect);
            smokeEffect = null;
        }
    }

    void OnGUI()
    {
        if (!showingMessage || !catVisible || catObject == null || !MainMenu.GameStarted) return;

        // Get cat screen position
        Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(catObject.transform.position + Vector3.up * 1.2f) : Vector3.zero;

        if (screenPos.z > 0)
        {
            float bubbleWidth = 280;
            float bubbleHeight = 80;
            float bubbleX = screenPos.x - bubbleWidth / 2;
            float bubbleY = Screen.height - screenPos.y - bubbleHeight - 10;

            // Clamp to screen
            bubbleX = Mathf.Clamp(bubbleX, 10, Screen.width - bubbleWidth - 10);
            bubbleY = Mathf.Clamp(bubbleY, 10, Screen.height - bubbleHeight - 30);

            // Speech bubble background (orange theme for cat)
            GUI.color = new Color(0.95f, 0.85f, 0.7f, 0.98f);
            GUI.DrawTexture(new Rect(bubbleX, bubbleY, bubbleWidth, bubbleHeight), Texture2D.whiteTexture);
            GUI.color = new Color(0.6f, 0.4f, 0.2f);
            GUI.DrawTexture(new Rect(bubbleX + 2, bubbleY + 2, bubbleWidth - 4, bubbleHeight - 4), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.95f, 0.85f);
            GUI.DrawTexture(new Rect(bubbleX + 4, bubbleY + 4, bubbleWidth - 8, bubbleHeight - 8), Texture2D.whiteTexture);

            // Cat label
            GUIStyle catLabelStyle = new GUIStyle();
            catLabelStyle.fontSize = 10;
            catLabelStyle.fontStyle = FontStyle.Bold;
            catLabelStyle.normal.textColor = new Color(0.8f, 0.5f, 0.2f);
            GUI.color = Color.white;
            GUI.Label(new Rect(bubbleX + 10, bubbleY + 6, 100, 16), "Island Cat:", catLabelStyle);

            // Message text
            GUIStyle msgStyle = new GUIStyle();
            msgStyle.fontSize = 12;
            msgStyle.alignment = TextAnchor.UpperLeft;
            msgStyle.normal.textColor = new Color(0.25f, 0.2f, 0.15f);
            msgStyle.wordWrap = true;
            GUI.Label(new Rect(bubbleX + 10, bubbleY + 22, bubbleWidth - 20, bubbleHeight - 32), catMessage, msgStyle);

            // ESC hint
            GUIStyle hintStyle = new GUIStyle();
            hintStyle.fontSize = 9;
            hintStyle.alignment = TextAnchor.MiddleCenter;
            hintStyle.normal.textColor = new Color(0.5f, 0.4f, 0.3f);
            GUI.Label(new Rect(bubbleX, bubbleY + bubbleHeight + 2, bubbleWidth, 14), "[ESC to dismiss]", hintStyle);
        }
    }

    void PlayMeow()
    {
        if (audioSource == null) return;

        // Generate a procedural cat meow sound
        int sampleRate = 44100;
        float duration = 0.6f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip meowClip = AudioClip.Create("CatMeow", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Cat meow has rising then falling pitch
            float pitchCurve;
            if (progress < 0.3f)
            {
                // Rise
                pitchCurve = 400f + (progress / 0.3f) * 400f;
            }
            else if (progress < 0.5f)
            {
                // Hold high
                pitchCurve = 800f;
            }
            else
            {
                // Fall
                pitchCurve = 800f - ((progress - 0.5f) / 0.5f) * 300f;
            }

            // Add vibrato for realistic cat sound
            float vibrato = Mathf.Sin(t * 30f) * 20f;
            float freq = pitchCurve + vibrato;

            // Envelope - quick attack, sustain, decay
            float envelope;
            if (progress < 0.1f)
                envelope = progress / 0.1f;
            else if (progress < 0.6f)
                envelope = 1f;
            else
                envelope = 1f - ((progress - 0.6f) / 0.4f);

            // Main tone with harmonics
            float sound = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.5f;
            sound += Mathf.Sin(2 * Mathf.PI * freq * 2f * t) * 0.25f;
            sound += Mathf.Sin(2 * Mathf.PI * freq * 3f * t) * 0.1f;

            // Add slight nasality/raspiness
            sound += Mathf.Sin(2 * Mathf.PI * freq * 1.5f * t) * 0.15f;

            samples[i] = sound * envelope * 0.6f;
        }

        meowClip.SetData(samples, 0);
        audioSource.clip = meowClip;
        audioSource.pitch = Random.Range(0.9f, 1.1f);  // Slight pitch variation
        audioSource.Play();
    }
}
