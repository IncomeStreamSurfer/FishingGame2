using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Outdoor nightclub in the Void Realm
/// Features a stage with neon lights, a punk performer, and spatial audio
/// Press R to toggle music from HARDSCAPE Resources folder
/// Pulsing neon lights react to music state
/// </summary>
public class VoidNightclub : MonoBehaviour
{
    // Track ALL music sources so we can stop others when one plays
    private static List<VoidNightclub> allNightclubs = new List<VoidNightclub>();

    [Header("Audio Settings")]
    public float maxVolume = 0.35f;  // Louder for nightclub atmosphere

    [Header("Visual Settings")]
    public Color stageColor = new Color(0.1f, 0.1f, 0.15f);  // Dark gray/black
    public Color neonPurple = new Color(0.8f, 0.2f, 1f);  // Vivid purple
    public Color neonMagenta = new Color(1f, 0.1f, 0.8f);  // Hot magenta
    public Color neonCyan = new Color(0.1f, 0.8f, 1f);  // Bright cyan
    public Color performerSkinTone = new Color(0.9f, 0.7f, 0.6f);
    public Color performerHairColor = new Color(1f, 0.2f, 0.5f);  // Punk pink
    public Color performerOutfitColor = new Color(0.15f, 0.15f, 0.2f);  // Dark outfit
    public Color indicatorOnColor = new Color(0.2f, 1f, 0.5f);  // Green LED
    public Color indicatorOffColor = new Color(0.5f, 0.1f, 0.1f);  // Red LED

    private AudioSource audioSource;
    private GameObject statusLED;
    private List<GameObject> neonLights = new List<GameObject>();
    private List<Material> neonMaterials = new List<Material>();
    private float lightPulseTime;
    private bool initialized = false;
    private bool isPlaying = false;
    private bool playerNearby = false;
    private float interactionDistance = 5f;
    private float songEndCheckDelay = 0.5f;

    // Music from HARDSCAPE folder
    private List<AudioClip> tracks = new List<AudioClip>();
    private List<string> loadedTrackNames = new List<string>();
    private int currentTrackIndex = 0;

    void Awake()
    {
        // Register this nightclub instance
        if (!allNightclubs.Contains(this))
            allNightclubs.Add(this);
    }

    void OnDestroy()
    {
        allNightclubs.Remove(this);
    }

    void Start()
    {
        Debug.Log("VoidNightclub: Starting initialization...");
        CreateNightclubVisuals();
        Invoke("SetupAudio", 0.5f);
    }

    void SetupAudio()
    {
        // Load songs from HARDSCAPE folder
        AudioClip[] hardscapeClips = Resources.LoadAll<AudioClip>("HARDSCAPE");

        if (hardscapeClips != null && hardscapeClips.Length > 0)
        {
            foreach (AudioClip clip in hardscapeClips)
            {
                tracks.Add(clip);
                loadedTrackNames.Add(clip.name);
                Debug.Log("VoidNightclub: Loaded HARDSCAPE track - " + clip.name);
            }
        }

        if (tracks.Count == 0)
        {
            Debug.LogWarning("VoidNightclub: No HARDSCAPE tracks found!");
            return;
        }

        Debug.Log("VoidNightclub: Loaded " + tracks.Count + " tracks total!");

        // Create audio source with 3D spatial audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = tracks[0];
        audioSource.loop = false;
        audioSource.volume = maxVolume;
        audioSource.spatialBlend = 1f;  // Full 3D sound
        audioSource.minDistance = 3f;  // Full volume within 3 units
        audioSource.maxDistance = 50f;   // Audible up to 50 units (nightclub carries far)
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0.3f;
        audioSource.playOnAwake = false;
        audioSource.priority = 0;

        initialized = true;
        Debug.Log("VoidNightclub: Ready! Press R to start the party.");
    }

    void CreateNightclubVisuals()
    {
        // STAGE PLATFORM
        GameObject stage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stage.name = "Stage";
        stage.transform.SetParent(transform);
        stage.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        stage.transform.localScale = new Vector3(6f, 0.5f, 4f);
        Material stageMat = CreateMaterial(stageColor);
        stageMat.SetFloat("_Metallic", 0.6f);
        stageMat.SetFloat("_Glossiness", 0.7f);
        stage.GetComponent<Renderer>().material = stageMat;
        Object.Destroy(stage.GetComponent<Collider>());

        // BACK WALL
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.SetParent(transform);
        backWall.transform.localPosition = new Vector3(0f, 2f, -1.8f);
        backWall.transform.localScale = new Vector3(6f, 3.5f, 0.2f);
        backWall.GetComponent<Renderer>().material = CreateMaterial(stageColor);
        Object.Destroy(backWall.GetComponent<Collider>());

        // NEON LIGHT STRIPS - Purple
        for (int i = -2; i <= 2; i++)
        {
            GameObject neonStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neonStrip.name = "NeonStrip_Purple_" + i;
            neonStrip.transform.SetParent(transform);
            neonStrip.transform.localPosition = new Vector3(i * 1.2f, 3.5f, -1.7f);
            neonStrip.transform.localScale = new Vector3(0.15f, 0.15f, 0.1f);
            Material neonMat = CreateEmissiveMaterial(neonPurple, 3f);
            neonStrip.GetComponent<Renderer>().material = neonMat;
            neonLights.Add(neonStrip);
            neonMaterials.Add(neonMat);
            Object.Destroy(neonStrip.GetComponent<Collider>());
        }

        // NEON LIGHT STRIPS - Cyan (lower level)
        for (int i = -2; i <= 2; i++)
        {
            GameObject neonStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            neonStrip.name = "NeonStrip_Cyan_" + i;
            neonStrip.transform.SetParent(transform);
            neonStrip.transform.localPosition = new Vector3(i * 1.2f, 1.5f, -1.7f);
            neonStrip.transform.localScale = new Vector3(0.15f, 0.15f, 0.1f);
            Material neonMat = CreateEmissiveMaterial(neonCyan, 3f);
            neonStrip.GetComponent<Renderer>().material = neonMat;
            neonLights.Add(neonStrip);
            neonMaterials.Add(neonMat);
            Object.Destroy(neonStrip.GetComponent<Collider>());
        }

        // VERTICAL NEON POLES - Magenta
        for (int i = -2; i <= 2; i += 2)
        {
            GameObject neonPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neonPole.name = "NeonPole_Magenta_" + i;
            neonPole.transform.SetParent(transform);
            neonPole.transform.localPosition = new Vector3(i * 1.3f, 2f, -1.7f);
            neonPole.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
            Material neonMat = CreateEmissiveMaterial(neonMagenta, 3f);
            neonPole.GetComponent<Renderer>().material = neonMat;
            neonLights.Add(neonPole);
            neonMaterials.Add(neonMat);
            Object.Destroy(neonPole.GetComponent<Collider>());
        }

        // PERFORMER - Humanoid with punk aesthetics
        CreatePerformer();

        // SPEAKERS
        CreateSpeaker(new Vector3(-2.5f, 1f, -1.5f));
        CreateSpeaker(new Vector3(2.5f, 1f, -1.5f));

        // STATUS LED (shows if music is playing)
        statusLED = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        statusLED.name = "StatusLED";
        statusLED.transform.SetParent(transform);
        statusLED.transform.localPosition = new Vector3(0f, 0.6f, 2f);
        statusLED.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        Material ledMat = CreateEmissiveMaterial(indicatorOffColor, 1f);
        statusLED.GetComponent<Renderer>().material = ledMat;
        Object.Destroy(statusLED.GetComponent<Collider>());

        // STAGE EDGE LIGHTS
        CreateStageEdgeLights();
    }

    void CreatePerformer()
    {
        GameObject performer = new GameObject("Performer");
        performer.transform.SetParent(transform);
        performer.transform.localPosition = new Vector3(0f, 1.2f, -0.5f);

        // Body (torso)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(performer.transform);
        body.transform.localPosition = new Vector3(0f, 0f, 0f);
        body.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
        body.GetComponent<Renderer>().material = CreateMaterial(performerOutfitColor);
        Object.Destroy(body.GetComponent<Collider>());

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(performer.transform);
        head.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        head.transform.localScale = new Vector3(0.3f, 0.35f, 0.3f);
        head.GetComponent<Renderer>().material = CreateMaterial(performerSkinTone);
        Object.Destroy(head.GetComponent<Collider>());

        // Punk hair (spiky mohawk)
        GameObject hair = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hair.name = "Hair";
        hair.transform.SetParent(performer.transform);
        hair.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        hair.transform.localScale = new Vector3(0.15f, 0.3f, 0.25f);
        Material hairMat = CreateEmissiveMaterial(performerHairColor, 1.5f);
        hair.GetComponent<Renderer>().material = hairMat;
        Object.Destroy(hair.GetComponent<Collider>());

        // Left arm
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArm.name = "LeftArm";
        leftArm.transform.SetParent(performer.transform);
        leftArm.transform.localPosition = new Vector3(-0.35f, -0.1f, 0f);
        leftArm.transform.localScale = new Vector3(0.12f, 0.4f, 0.12f);
        leftArm.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        leftArm.GetComponent<Renderer>().material = CreateMaterial(performerSkinTone);
        Object.Destroy(leftArm.GetComponent<Collider>());

        // Right arm (raised in punk gesture)
        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArm.name = "RightArm";
        rightArm.transform.SetParent(performer.transform);
        rightArm.transform.localPosition = new Vector3(0.35f, 0.2f, 0f);
        rightArm.transform.localScale = new Vector3(0.12f, 0.4f, 0.12f);
        rightArm.transform.localRotation = Quaternion.Euler(0f, 0f, -120f);
        rightArm.GetComponent<Renderer>().material = CreateMaterial(performerSkinTone);
        Object.Destroy(rightArm.GetComponent<Collider>());

        // Legs
        GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftLeg.name = "LeftLeg";
        leftLeg.transform.SetParent(performer.transform);
        leftLeg.transform.localPosition = new Vector3(-0.15f, -0.7f, 0f);
        leftLeg.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
        leftLeg.GetComponent<Renderer>().material = CreateMaterial(Color.black);
        Object.Destroy(leftLeg.GetComponent<Collider>());

        GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightLeg.name = "RightLeg";
        rightLeg.transform.SetParent(performer.transform);
        rightLeg.transform.localPosition = new Vector3(0.15f, -0.7f, 0f);
        rightLeg.transform.localScale = new Vector3(0.15f, 0.5f, 0.15f);
        rightLeg.GetComponent<Renderer>().material = CreateMaterial(Color.black);
        Object.Destroy(rightLeg.GetComponent<Collider>());

        // Add subtle animation rotation component
        PerformerAnimation anim = performer.AddComponent<PerformerAnimation>();
        anim.voidNightclub = this;
    }

    void CreateSpeaker(Vector3 localPosition)
    {
        GameObject speaker = new GameObject("Speaker");
        speaker.transform.SetParent(transform);
        speaker.transform.localPosition = localPosition;

        // Speaker box
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "SpeakerBox";
        box.transform.SetParent(speaker.transform);
        box.transform.localPosition = Vector3.zero;
        box.transform.localScale = new Vector3(0.6f, 1.2f, 0.5f);
        box.GetComponent<Renderer>().material = CreateMaterial(Color.black);
        Object.Destroy(box.GetComponent<Collider>());

        // Speaker cone (purple neon accent)
        GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone.name = "SpeakerCone";
        cone.transform.SetParent(speaker.transform);
        cone.transform.localPosition = new Vector3(0f, 0.2f, 0.26f);
        cone.transform.localScale = new Vector3(0.35f, 0.02f, 0.35f);
        cone.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Material coneMat = CreateEmissiveMaterial(neonPurple, 2f);
        cone.GetComponent<Renderer>().material = coneMat;
        neonLights.Add(cone);
        neonMaterials.Add(coneMat);
        Object.Destroy(cone.GetComponent<Collider>());

        // Lower speaker cone
        GameObject cone2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone2.name = "SpeakerCone2";
        cone2.transform.SetParent(speaker.transform);
        cone2.transform.localPosition = new Vector3(0f, -0.3f, 0.26f);
        cone2.transform.localScale = new Vector3(0.35f, 0.02f, 0.35f);
        cone2.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Material cone2Mat = CreateEmissiveMaterial(neonCyan, 2f);
        cone2.GetComponent<Renderer>().material = cone2Mat;
        neonLights.Add(cone2);
        neonMaterials.Add(cone2Mat);
        Object.Destroy(cone2.GetComponent<Collider>());
    }

    void CreateStageEdgeLights()
    {
        // Lights along front edge of stage
        for (int i = -5; i <= 5; i++)
        {
            GameObject edgeLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            edgeLight.name = "EdgeLight_" + i;
            edgeLight.transform.SetParent(transform);
            edgeLight.transform.localPosition = new Vector3(i * 0.6f, 0.55f, 1.9f);
            edgeLight.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

            // Alternate colors
            Color lightColor = (i % 2 == 0) ? neonMagenta : neonCyan;
            Material lightMat = CreateEmissiveMaterial(lightColor, 2.5f);
            edgeLight.GetComponent<Renderer>().material = lightMat;
            neonLights.Add(edgeLight);
            neonMaterials.Add(lightMat);
            Object.Destroy(edgeLight.GetComponent<Collider>());
        }
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
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Glossiness", 0.9f);
        return mat;
    }

    void Update()
    {
        if (!initialized) return;
        if (!MainMenu.GameStarted) return;
        if (audioSource == null) return;

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

        // Check if track ended, play next
        if (isPlaying && tracks.Count > 0)
        {
            if (songEndCheckDelay > 0)
            {
                songEndCheckDelay -= Time.deltaTime;
            }
            else if (!audioSource.isPlaying)
            {
                PlayNextTrack();
            }
        }

        UpdateNeonLights();
        UpdateStatusLED();
    }

    void OnGUI()
    {
        if (!initialized || !MainMenu.GameStarted) return;

        // Show interaction prompt when player is nearby
        if (playerNearby)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 18;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = neonMagenta;

            string promptText = isPlaying ? "[R] Stop Nightclub Music" : "[R] Start Nightclub Party";
            float promptY = Screen.height * 0.65f;
            GUI.Label(new Rect(0, promptY, Screen.width, 30), promptText, promptStyle);
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
                // Start at random track
                currentTrackIndex = Random.Range(0, tracks.Count);
                audioSource.clip = tracks[currentTrackIndex];
                audioSource.volume = maxVolume;
                audioSource.Play();
                songEndCheckDelay = 0.5f;
                Debug.Log("VoidNightclub: Party started! Playing " + tracks[currentTrackIndex].name);

                // Notify NPCs to start dancing
                NotifyNPCsOfMusicState(true);
            }
        }
        else
        {
            audioSource.Stop();
            Debug.Log("VoidNightclub: Party stopped.");

            // Notify NPCs to stop dancing
            NotifyNPCsOfMusicState(false);
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
                club.NotifyNPCsOfMusicState(false);
            }
        }

        // Also stop any DockRadios that might be playing
        DockRadio[] radios = FindObjectsOfType<DockRadio>();
        foreach (DockRadio radio in radios)
        {
            // Use reflection or just stop all audio sources on dock radios
            AudioSource radioAudio = radio.GetComponent<AudioSource>();
            if (radioAudio != null && radioAudio.isPlaying)
            {
                radioAudio.Stop();
            }
        }
    }

    void NotifyNPCsOfMusicState(bool musicPlaying)
    {
        // Find all VoidWanderers and tell them about music state
        VoidWanderer[] wanderers = FindObjectsOfType<VoidWanderer>();
        foreach (VoidWanderer wanderer in wanderers)
        {
            if (musicPlaying)
                wanderer.StartDancing(transform.position);
            else
                wanderer.StopDancing();
        }
    }

    void PlayNextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % tracks.Count;
        audioSource.clip = tracks[currentTrackIndex];
        audioSource.volume = maxVolume;
        audioSource.Play();
        songEndCheckDelay = 0.5f;
        Debug.Log("VoidNightclub: Now playing " + tracks[currentTrackIndex].name);
    }

    void UpdateNeonLights()
    {
        lightPulseTime += Time.deltaTime;

        // Different pulse patterns based on music state
        if (isPlaying)
        {
            // Fast, energetic pulsing when music plays
            float fastPulse = 0.5f + Mathf.Sin(lightPulseTime * 8f) * 0.5f;
            float slowPulse = 0.6f + Mathf.Sin(lightPulseTime * 3f) * 0.4f;

            for (int i = 0; i < neonMaterials.Count; i++)
            {
                if (neonMaterials[i] != null)
                {
                    Color baseColor = neonMaterials[i].color;
                    // Alternate between fast and slow pulse for variety
                    float pulse = (i % 2 == 0) ? fastPulse : slowPulse;
                    float intensity = 2f + pulse * 3f;
                    neonMaterials[i].SetColor("_EmissionColor", baseColor * intensity);
                }
            }
        }
        else
        {
            // Slow, dim pulsing when idle
            float idlePulse = 0.3f + Mathf.Sin(lightPulseTime * 1.5f) * 0.2f;

            for (int i = 0; i < neonMaterials.Count; i++)
            {
                if (neonMaterials[i] != null)
                {
                    Color baseColor = neonMaterials[i].color;
                    float intensity = 0.5f + idlePulse;
                    neonMaterials[i].SetColor("_EmissionColor", baseColor * intensity);
                }
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
            // Green pulsing LED when playing
            float pulse = 0.7f + Mathf.Sin(lightPulseTime * 5f) * 0.3f;
            rend.material.SetColor("_EmissionColor", indicatorOnColor * pulse * 3f);
            rend.material.color = indicatorOnColor;
        }
        else
        {
            // Red steady LED when stopped
            rend.material.SetColor("_EmissionColor", indicatorOffColor * 0.8f);
            rend.material.color = indicatorOffColor;
        }
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}

/// <summary>
/// Simple animation for the performer - subtle rotation and movement
/// </summary>
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

        // Subtle rotation and bob when music plays
        float rotationSpeed = 30f;
        float bobAmount = 0.05f;
        float bobSpeed = 4f;

        transform.localRotation = Quaternion.Euler(0f, animTime * rotationSpeed, 0f);

        float bob = Mathf.Sin(animTime * bobSpeed) * bobAmount;
        transform.localPosition = originalPosition + new Vector3(0f, bob, 0f);
    }
}
