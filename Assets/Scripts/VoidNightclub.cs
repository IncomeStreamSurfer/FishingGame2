using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Outdoor nightclub in the Void Realm
/// Press F to toggle music - plays ONE song at a time
/// Features lasers, fog machine, and dancing NPCs
/// </summary>
public class VoidNightclub : MonoBehaviour
{
    // Track ALL music sources so we can stop others when one plays
    private static List<VoidNightclub> allNightclubs = new List<VoidNightclub>();

    [Header("Audio Settings")]
    public float maxVolume = 0.35f;

    [Header("Visual Settings")]
    public Color stageColor = new Color(0.1f, 0.1f, 0.15f);
    public Color neonPurple = new Color(0.8f, 0.2f, 1f);
    public Color neonMagenta = new Color(1f, 0.1f, 0.8f);
    public Color neonCyan = new Color(0.1f, 0.8f, 1f);
    public Color laserGreen = new Color(0.2f, 1f, 0.3f);
    public Color laserRed = new Color(1f, 0.2f, 0.2f);

    private AudioSource audioSource;
    private GameObject statusLED;
    private List<GameObject> neonLights = new List<GameObject>();
    private List<Material> neonMaterials = new List<Material>();
    private List<GameObject> laserBeams = new List<GameObject>();
    private List<Material> laserMaterials = new List<Material>();
    private GameObject fogEffect;
    private float lightPulseTime;
    private bool initialized = false;
    private bool isPlaying = false;
    private bool playerNearby = false;
    private float interactionDistance = 6f;

    // Music from HARDSCAPE folder
    private List<AudioClip> tracks = new List<AudioClip>();
    private int currentTrackIndex = 0;

    void Awake()
    {
        if (!allNightclubs.Contains(this))
            allNightclubs.Add(this);
    }

    void OnDestroy()
    {
        allNightclubs.Remove(this);
    }

    void Start()
    {
        CreateNightclubVisuals();
        CreateLasers();
        CreateFogMachine();
        Invoke("SetupAudio", 0.5f);
    }

    void SetupAudio()
    {
        AudioClip[] hardscapeClips = Resources.LoadAll<AudioClip>("HARDSCAPE");

        if (hardscapeClips != null && hardscapeClips.Length > 0)
        {
            foreach (AudioClip clip in hardscapeClips)
            {
                tracks.Add(clip);
            }
        }

        if (tracks.Count == 0)
        {
            Debug.LogWarning("VoidNightclub: No HARDSCAPE tracks found!");
            return;
        }

        // Create audio source - LOOP enabled for single song
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = tracks[0];
        audioSource.loop = true;  // Loop the same song
        audioSource.volume = maxVolume;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 3f;
        audioSource.maxDistance = 50f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;  // No doppler effect
        audioSource.playOnAwake = false;

        initialized = true;
    }

    void CreateNightclubVisuals()
    {
        // STAGE PLATFORM
        GameObject stage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stage.name = "Stage";
        stage.transform.SetParent(transform);
        stage.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        stage.transform.localScale = new Vector3(8f, 0.5f, 5f);
        Material stageMat = CreateMaterial(stageColor);
        stageMat.SetFloat("_Metallic", 0.7f);
        stage.GetComponent<Renderer>().material = stageMat;

        // BACK WALL
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.SetParent(transform);
        backWall.transform.localPosition = new Vector3(0f, 3f, -2.2f);
        backWall.transform.localScale = new Vector3(8f, 5.5f, 0.2f);
        backWall.GetComponent<Renderer>().material = CreateMaterial(stageColor);
        Object.Destroy(backWall.GetComponent<Collider>());

        // NEON LIGHT STRIPS
        for (int i = -3; i <= 3; i++)
        {
            GameObject neonStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neonStrip.name = "NeonStrip_" + i;
            neonStrip.transform.SetParent(transform);
            neonStrip.transform.localPosition = new Vector3(i * 1.0f, 5f, -2.1f);
            neonStrip.transform.localScale = new Vector3(0.2f, 0.2f, 0.1f);
            Material neonMat = CreateEmissiveMaterial((i % 2 == 0) ? neonPurple : neonCyan, 2f);
            neonStrip.GetComponent<Renderer>().material = neonMat;
            neonLights.Add(neonStrip);
            neonMaterials.Add(neonMat);
            Object.Destroy(neonStrip.GetComponent<Collider>());
        }

        // PERFORMER
        CreatePerformer();

        // SPEAKERS
        CreateSpeaker(new Vector3(-3.5f, 1.2f, -1.8f));
        CreateSpeaker(new Vector3(3.5f, 1.2f, -1.8f));

        // STATUS LED
        statusLED = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        statusLED.name = "StatusLED";
        statusLED.transform.SetParent(transform);
        statusLED.transform.localPosition = new Vector3(0f, 0.6f, 2.5f);
        statusLED.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        Material ledMat = CreateEmissiveMaterial(new Color(0.5f, 0.1f, 0.1f), 1f);
        statusLED.GetComponent<Renderer>().material = ledMat;
        Object.Destroy(statusLED.GetComponent<Collider>());
    }

    void CreateLasers()
    {
        // Create multiple laser beams that will animate when music plays
        Color[] laserColors = { laserGreen, laserRed, neonPurple, neonCyan, neonMagenta };

        for (int i = 0; i < 8; i++)
        {
            GameObject laser = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            laser.name = "Laser_" + i;
            laser.transform.SetParent(transform);

            // Position lasers along the back wall
            float xPos = -3f + (i * 0.85f);
            laser.transform.localPosition = new Vector3(xPos, 4f, -2f);
            laser.transform.localScale = new Vector3(0.03f, 3f, 0.03f);
            laser.transform.localRotation = Quaternion.Euler(0, 0, 30f);

            Material laserMat = CreateEmissiveMaterial(laserColors[i % laserColors.Length], 5f);
            laser.GetComponent<Renderer>().material = laserMat;
            laserBeams.Add(laser);
            laserMaterials.Add(laserMat);
            Object.Destroy(laser.GetComponent<Collider>());

            // Start hidden
            laser.SetActive(false);
        }
    }

    void CreateFogMachine()
    {
        // Create fog/smoke effect at ground level
        fogEffect = new GameObject("FogMachine");
        fogEffect.transform.SetParent(transform);
        fogEffect.transform.localPosition = new Vector3(0f, 0.6f, 3f);

        // Create multiple fog "clouds" as flat scaled spheres
        for (int i = 0; i < 12; i++)
        {
            GameObject cloud = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cloud.name = "FogCloud_" + i;
            cloud.transform.SetParent(fogEffect.transform);

            float x = Random.Range(-6f, 6f);
            float z = Random.Range(-2f, 8f);
            cloud.transform.localPosition = new Vector3(x, Random.Range(-0.2f, 0.3f), z);
            cloud.transform.localScale = new Vector3(
                Random.Range(1.5f, 3f),
                Random.Range(0.3f, 0.6f),
                Random.Range(1.5f, 3f)
            );

            Material fogMat = new Material(Shader.Find("Standard"));
            fogMat.SetFloat("_Mode", 3); // Transparent
            fogMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fogMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fogMat.EnableKeyword("_ALPHABLEND_ON");
            fogMat.color = new Color(0.7f, 0.7f, 0.8f, 0.15f);
            fogMat.renderQueue = 3000;
            cloud.GetComponent<Renderer>().material = fogMat;
            Object.Destroy(cloud.GetComponent<Collider>());
        }

        fogEffect.SetActive(false);
    }

    void CreatePerformer()
    {
        GameObject performer = new GameObject("Performer");
        performer.transform.SetParent(transform);
        performer.transform.localPosition = new Vector3(0f, 1.2f, -0.5f);

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(performer.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
        body.GetComponent<Renderer>().material = CreateMaterial(new Color(0.15f, 0.15f, 0.2f));
        Object.Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(performer.transform);
        head.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        head.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
        head.GetComponent<Renderer>().material = CreateMaterial(new Color(0.9f, 0.7f, 0.6f));
        Object.Destroy(head.GetComponent<Collider>());

        // Mohawk
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hair.transform.SetParent(performer.transform);
        hair.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        hair.transform.localScale = new Vector3(0.15f, 0.3f, 0.25f);
        hair.GetComponent<Renderer>().material = CreateEmissiveMaterial(new Color(1f, 0.2f, 0.5f), 1.5f);
        Object.Destroy(hair.GetComponent<Collider>());

        performer.AddComponent<PerformerAnimation>().voidNightclub = this;
    }

    void CreateSpeaker(Vector3 pos)
    {
        GameObject speaker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        speaker.transform.SetParent(transform);
        speaker.transform.localPosition = pos;
        speaker.transform.localScale = new Vector3(0.8f, 1.5f, 0.6f);
        speaker.GetComponent<Renderer>().material = CreateMaterial(Color.black);
        Object.Destroy(speaker.GetComponent<Collider>());

        GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone.transform.SetParent(speaker.transform);
        cone.transform.localPosition = new Vector3(0f, 0.15f, 0.51f);
        cone.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);
        cone.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Material coneMat = CreateEmissiveMaterial(neonPurple, 2f);
        cone.GetComponent<Renderer>().material = coneMat;
        neonLights.Add(cone);
        neonMaterials.Add(coneMat);
        Object.Destroy(cone.GetComponent<Collider>());
    }

    Material CreateMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }

    Material CreateEmissiveMaterial(Color color, float intensity)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        return mat;
    }

    void Update()
    {
        if (!initialized) return;
        if (!MainMenu.GameStarted) return;

        // Check distance to player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerNearby = distance < interactionDistance;
        }

        // Press R to toggle music
        if (playerNearby && Input.GetKeyDown(KeyCode.R))
        {
            ToggleMusic();
        }

        if (isPlaying)
        {
            UpdateLasers();
            UpdateFog();
        }

        UpdateNeonLights();
        UpdateStatusLED();
    }

    void OnGUI()
    {
        if (!initialized || !MainMenu.GameStarted) return;

        if (playerNearby)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 18;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = neonMagenta;

            string promptText = isPlaying ? "[R] Stop Party" : "[R] Start Party";
            GUI.Label(new Rect(0, Screen.height * 0.65f, Screen.width, 30), promptText, promptStyle);
        }
    }

    void ToggleMusic()
    {
        isPlaying = !isPlaying;

        if (isPlaying)
        {
            // Stop ALL other music sources first
            StopAllOtherMusic();

            if (tracks.Count > 0)
            {
                // Pick random track and LOOP it
                currentTrackIndex = Random.Range(0, tracks.Count);
                audioSource.clip = tracks[currentTrackIndex];
                audioSource.loop = true;
                audioSource.Play();

                // Activate effects
                ActivateLasers(true);
                ActivateFog(true);

                // Notify NPCs to dance
                NotifyNPCsOfMusicState(true);

                Debug.Log("VoidNightclub: Party started!");
            }
        }
        else
        {
            audioSource.Stop();

            // Deactivate effects
            ActivateLasers(false);
            ActivateFog(false);

            // Stop NPCs dancing
            NotifyNPCsOfMusicState(false);

            Debug.Log("VoidNightclub: Party stopped.");
        }
    }

    void StopAllOtherMusic()
    {
        // Stop other nightclubs
        foreach (VoidNightclub club in allNightclubs)
        {
            if (club != this && club.isPlaying && club.audioSource != null)
            {
                club.audioSource.Stop();
                club.isPlaying = false;
                club.ActivateLasers(false);
                club.ActivateFog(false);
                club.NotifyNPCsOfMusicState(false);
            }
        }

        // Stop any DockRadios
        DockRadio[] radios = FindObjectsOfType<DockRadio>();
        foreach (DockRadio radio in radios)
        {
            AudioSource radioAudio = radio.GetComponent<AudioSource>();
            if (radioAudio != null && radioAudio.isPlaying)
            {
                radioAudio.Stop();
            }
        }
    }

    void ActivateLasers(bool active)
    {
        foreach (GameObject laser in laserBeams)
        {
            if (laser != null)
                laser.SetActive(active);
        }
    }

    void ActivateFog(bool active)
    {
        if (fogEffect != null)
            fogEffect.SetActive(active);
    }

    void UpdateLasers()
    {
        lightPulseTime += Time.deltaTime;

        for (int i = 0; i < laserBeams.Count; i++)
        {
            if (laserBeams[i] != null)
            {
                // Rotate lasers
                float rotSpeed = 30f + (i * 10f);
                float rotAngle = Mathf.Sin(lightPulseTime * 2f + i) * 45f;
                float tiltAngle = Mathf.Cos(lightPulseTime * 1.5f + i * 0.5f) * 30f;
                laserBeams[i].transform.localRotation = Quaternion.Euler(tiltAngle, 0, 30f + rotAngle);

                // Pulse intensity
                if (i < laserMaterials.Count && laserMaterials[i] != null)
                {
                    float pulse = 3f + Mathf.Sin(lightPulseTime * 8f + i) * 2f;
                    Color baseColor = laserMaterials[i].color;
                    laserMaterials[i].SetColor("_EmissionColor", baseColor * pulse);
                }
            }
        }
    }

    void UpdateFog()
    {
        if (fogEffect == null) return;

        // Animate fog clouds
        foreach (Transform cloud in fogEffect.transform)
        {
            // Slow drift
            float drift = Mathf.Sin(Time.time * 0.3f + cloud.GetSiblingIndex()) * 0.01f;
            cloud.localPosition += new Vector3(drift, 0, drift * 0.5f);

            // Pulse size slightly
            float pulse = 1f + Mathf.Sin(Time.time + cloud.GetSiblingIndex()) * 0.1f;
            Vector3 baseScale = cloud.localScale;
            cloud.localScale = new Vector3(baseScale.x, 0.4f * pulse, baseScale.z);
        }
    }

    void UpdateNeonLights()
    {
        float pulse;
        if (isPlaying)
        {
            pulse = 2f + Mathf.Sin(lightPulseTime * 8f) * 3f;
        }
        else
        {
            pulse = 0.5f + Mathf.Sin(lightPulseTime * 1.5f) * 0.3f;
        }

        for (int i = 0; i < neonMaterials.Count; i++)
        {
            if (neonMaterials[i] != null)
            {
                Color baseColor = neonMaterials[i].color;
                neonMaterials[i].SetColor("_EmissionColor", baseColor * pulse);
            }
        }
    }

    void UpdateStatusLED()
    {
        if (statusLED == null) return;
        Renderer rend = statusLED.GetComponent<Renderer>();
        if (rend == null) return;

        if (isPlaying)
        {
            float pulse = 0.7f + Mathf.Sin(lightPulseTime * 5f) * 0.3f;
            rend.material.color = new Color(0.2f, 1f, 0.5f);
            rend.material.SetColor("_EmissionColor", new Color(0.2f, 1f, 0.5f) * pulse * 3f);
        }
        else
        {
            rend.material.color = new Color(0.5f, 0.1f, 0.1f);
            rend.material.SetColor("_EmissionColor", new Color(0.5f, 0.1f, 0.1f) * 0.5f);
        }
    }

    void NotifyNPCsOfMusicState(bool musicPlaying)
    {
        VoidWanderer[] wanderers = FindObjectsOfType<VoidWanderer>();
        foreach (VoidWanderer wanderer in wanderers)
        {
            if (musicPlaying)
                wanderer.StartDancing(transform.position);
            else
                wanderer.StopDancing();
        }
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}

public class PerformerAnimation : MonoBehaviour
{
    public VoidNightclub voidNightclub;
    private float animTime;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (voidNightclub == null || !voidNightclub.IsPlaying()) return;

        animTime += Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0f, animTime * 30f, 0f);
        float bob = Mathf.Sin(animTime * 4f) * 0.05f;
        transform.localPosition = originalPosition + new Vector3(0f, bob, 0f);
    }
}
