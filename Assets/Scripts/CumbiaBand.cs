using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A group of 7 musician NPCs that walk together in formation through the Jungle Realm
/// Plays cumbia music with 3D spatial audio and Doppler effect
/// Players can press F to interact when nearby
/// </summary>
public class CumbiaBand : MonoBehaviour
{
    [Header("Band Settings")]
    public float walkSpeed = 1.5f;
    public float interactionDistance = 5f;
    public float formationSpacing = 1.2f;

    [Header("Audio Settings")]
    public float maxVolume = 0.3f;

    [Header("Particle Settings")]
    public int noteParticleCount = 20;
    public float particleEmitRadius = 3f;

    private AudioSource audioSource;
    private List<AudioClip> songs = new List<AudioClip>();
    private List<string> loadedSongNames = new List<string>();
    private int currentSongIndex = 0;

    private bool isPlaying = false;
    private bool playerNearby = false;
    private bool initialized = false;

    // Band members
    private BandMember[] bandMembers;
    private GameObject[] musicalNotes;

    // Patrol movement
    private Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;
    private float patrolWaitTime = 0f;

    [System.Serializable]
    private class BandMember
    {
        public GameObject gameObject;
        public string instrument;
        public Color color;
        public Vector3 formationOffset;
        public GameObject instrumentObject;
    }

    void Start()
    {
        // Define jungle patrol points (X > 900)
        patrolPoints = new Vector3[]
        {
            new Vector3(920f, 1f, 0f),
            new Vector3(950f, 1f, 30f),
            new Vector3(980f, 1f, 10f),
            new Vector3(960f, 1f, -20f),
            new Vector3(930f, 1f, -10f)
        };

        // Position band at first patrol point
        transform.position = patrolPoints[0];

        CreateBandMembers();
        CreateMusicalNoteParticles();
        Invoke("SetupAudio", 0.5f);
    }

    void SetupAudio()
    {
        // Load ALL songs from Resources/CUMBIASCAPE folder
        // Note: Songs should be placed in Assets/Resources/CUMBIASCAPE/
        // For now, we'll load all available audio files as cumbia songs
        AudioClip[] allClips = Resources.LoadAll<AudioClip>("CUMBIASCAPE");

        if (allClips != null && allClips.Length > 0)
        {
            foreach (AudioClip clip in allClips)
            {
                songs.Add(clip);
                loadedSongNames.Add(clip.name);
                Debug.Log("CumbiaBand: Loaded CUMBIASCAPE song - " + clip.name);
            }
        }

        // Fallback: if CUMBIASCAPE folder is empty, load from root Resources
        if (songs.Count == 0)
        {
            Debug.LogWarning("CumbiaBand: No songs in CUMBIASCAPE folder. Loading all Resources songs as cumbia...");

            // Load all audio clips from Resources root as cumbia
            AudioClip[] rootClips = Resources.LoadAll<AudioClip>("");
            if (rootClips != null && rootClips.Length > 0)
            {
                foreach (AudioClip clip in rootClips)
                {
                    songs.Add(clip);
                    loadedSongNames.Add(clip.name);
                    Debug.Log("CumbiaBand: Loaded song - " + clip.name);
                }
            }
        }

        if (songs.Count == 0)
        {
            Debug.LogWarning("CumbiaBand: No cumbia songs found! Place audio files in Assets/Resources/CUMBIASCAPE/");
            return;
        }

        Debug.Log("CumbiaBand: Loaded " + songs.Count + " songs total!");

        // Create audio source with 3D spatial audio and Doppler effect
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = songs[0];
        audioSource.loop = false;
        audioSource.volume = maxVolume;
        audioSource.spatialBlend = 1f;          // Full 3D sound
        audioSource.minDistance = 3f;           // Full volume within 3 units
        audioSource.maxDistance = 50f;          // Audible up to 50 units
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 1.0f;        // Doppler effect enabled
        audioSource.playOnAwake = false;
        audioSource.priority = 128;

        // Don't auto-play - wait for player interaction
        currentSongIndex = Random.Range(0, songs.Count);
        audioSource.clip = songs[currentSongIndex];
        // audioSource.Play(); // Removed - music starts when player presses F
        isPlaying = false; // Start silent

        initialized = true;
        Debug.Log("CumbiaBand: Ready! Press F to start the music!");
    }

    void CreateBandMembers()
    {
        // Define 7 band members with instruments and colors
        string[] instruments = new string[] { "Guitar", "Cuica", "Percussion", "Cowbell", "Bass", "Trumpet", "Saxophone" };
        Color[] colors = new Color[]
        {
            new Color(0.9f, 0.3f, 0.2f),  // Red - Guitar
            new Color(0.2f, 0.7f, 0.9f),  // Blue - Cuica
            new Color(0.9f, 0.7f, 0.2f),  // Yellow - Percussion
            new Color(0.3f, 0.9f, 0.3f),  // Green - Cowbell
            new Color(0.7f, 0.3f, 0.9f),  // Purple - Bass
            new Color(0.9f, 0.5f, 0.1f),  // Orange - Trumpet
            new Color(0.2f, 0.9f, 0.7f)   // Cyan - Saxophone
        };

        // Formation pattern (V-shape with leader in front)
        Vector3[] formationOffsets = new Vector3[]
        {
            new Vector3(0f, 0f, 2f),       // Guitar - front center (leader)
            new Vector3(-1f, 0f, 1f),      // Cuica - left front
            new Vector3(1f, 0f, 1f),       // Percussion - right front
            new Vector3(-2f, 0f, 0f),      // Cowbell - left middle
            new Vector3(2f, 0f, 0f),       // Bass - right middle
            new Vector3(-1.5f, 0f, -1f),   // Trumpet - left back
            new Vector3(1.5f, 0f, -1f)     // Saxophone - right back
        };

        bandMembers = new BandMember[7];

        for (int i = 0; i < 7; i++)
        {
            bandMembers[i] = new BandMember();
            bandMembers[i].instrument = instruments[i];
            bandMembers[i].color = colors[i];
            bandMembers[i].formationOffset = formationOffsets[i];

            // Create humanoid figure
            GameObject member = CreateHumanoidFigure(instruments[i], colors[i]);
            member.transform.SetParent(transform);
            member.transform.localPosition = formationOffsets[i];
            member.transform.localRotation = Quaternion.identity;

            bandMembers[i].gameObject = member;

            // Create instrument representation
            bandMembers[i].instrumentObject = CreateInstrument(instruments[i], colors[i], member);
        }
    }

    GameObject CreateHumanoidFigure(string name, Color color)
    {
        GameObject humanoid = new GameObject("BandMember_" + name);

        // Body (tall rectangle)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(humanoid.transform);
        body.transform.localPosition = new Vector3(0f, 1f, 0f);
        body.transform.localScale = new Vector3(0.4f, 0.8f, 0.25f);
        Material bodyMat = CreateMaterial(color);
        body.GetComponent<Renderer>().material = bodyMat;
        Object.Destroy(body.GetComponent<Collider>());

        // Head (sphere)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(humanoid.transform);
        head.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        head.transform.localScale = Vector3.one * 0.35f;
        Color skinColor = new Color(0.8f, 0.6f, 0.4f);
        head.GetComponent<Renderer>().material = CreateMaterial(skinColor);
        Object.Destroy(head.GetComponent<Collider>());

        // Legs
        for (int i = 0; i < 2; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "Leg_" + i;
            leg.transform.SetParent(humanoid.transform);
            float xOffset = (i == 0) ? -0.12f : 0.12f;
            leg.transform.localPosition = new Vector3(xOffset, 0.4f, 0f);
            leg.transform.localScale = new Vector3(0.15f, 0.8f, 0.15f);
            leg.GetComponent<Renderer>().material = CreateMaterial(color * 0.7f);
            Object.Destroy(leg.GetComponent<Collider>());
        }

        // Arms
        for (int i = 0; i < 2; i++)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Arm_" + i;
            arm.transform.SetParent(humanoid.transform);
            float xOffset = (i == 0) ? -0.35f : 0.35f;
            arm.transform.localPosition = new Vector3(xOffset, 1.1f, 0f);
            arm.transform.localScale = new Vector3(0.12f, 0.6f, 0.12f);
            arm.GetComponent<Renderer>().material = CreateMaterial(skinColor * 0.9f);
            Object.Destroy(arm.GetComponent<Collider>());
        }

        return humanoid;
    }

    GameObject CreateInstrument(string instrumentName, Color color, GameObject parent)
    {
        GameObject instrument = new GameObject("Instrument_" + instrumentName);
        instrument.transform.SetParent(parent.transform);

        switch (instrumentName)
        {
            case "Guitar":
                // Acoustic guitar shape with body and long neck
                // Guitar body (rounded shape using sphere)
                GameObject guitarBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                guitarBody.name = "GuitarBody";
                guitarBody.transform.SetParent(instrument.transform);
                guitarBody.transform.localPosition = new Vector3(0.25f, 0.9f, 0.1f);
                guitarBody.transform.localScale = new Vector3(0.2f, 0.35f, 0.12f);
                guitarBody.GetComponent<Renderer>().material = CreateMaterial(new Color(0.6f, 0.35f, 0.1f)); // Wood brown
                Object.Destroy(guitarBody.GetComponent<Collider>());

                // Guitar neck (long thin cylinder)
                GameObject guitarNeck = GameObject.CreatePrimitive(PrimitiveType.Cube);
                guitarNeck.name = "GuitarNeck";
                guitarNeck.transform.SetParent(instrument.transform);
                guitarNeck.transform.localPosition = new Vector3(0.25f, 1.25f, 0.1f);
                guitarNeck.transform.localScale = new Vector3(0.05f, 0.45f, 0.04f);
                guitarNeck.GetComponent<Renderer>().material = CreateMaterial(new Color(0.35f, 0.2f, 0.1f)); // Dark wood
                Object.Destroy(guitarNeck.GetComponent<Collider>());

                // Guitar headstock
                GameObject guitarHead = GameObject.CreatePrimitive(PrimitiveType.Cube);
                guitarHead.name = "GuitarHeadstock";
                guitarHead.transform.SetParent(instrument.transform);
                guitarHead.transform.localPosition = new Vector3(0.25f, 1.52f, 0.1f);
                guitarHead.transform.localScale = new Vector3(0.08f, 0.1f, 0.05f);
                guitarHead.GetComponent<Renderer>().material = CreateMaterial(new Color(0.35f, 0.2f, 0.1f));
                Object.Destroy(guitarHead.GetComponent<Collider>());
                break;

            case "Bass":
                // Upright bass (double bass) - tall bass shape
                // Bass body (large rounded)
                GameObject bassBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bassBody.name = "BassBody";
                bassBody.transform.SetParent(instrument.transform);
                bassBody.transform.localPosition = new Vector3(0.3f, 0.6f, 0.1f);
                bassBody.transform.localScale = new Vector3(0.25f, 0.5f, 0.15f);
                bassBody.GetComponent<Renderer>().material = CreateMaterial(new Color(0.5f, 0.25f, 0.05f)); // Dark wood
                Object.Destroy(bassBody.GetComponent<Collider>());

                // Bass neck (very tall)
                GameObject bassNeck = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bassNeck.name = "BassNeck";
                bassNeck.transform.SetParent(instrument.transform);
                bassNeck.transform.localPosition = new Vector3(0.3f, 1.3f, 0.1f);
                bassNeck.transform.localScale = new Vector3(0.06f, 0.8f, 0.05f);
                bassNeck.GetComponent<Renderer>().material = CreateMaterial(new Color(0.3f, 0.15f, 0.05f));
                Object.Destroy(bassNeck.GetComponent<Collider>());

                // Bass scroll (top)
                GameObject bassScroll = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bassScroll.name = "BassScroll";
                bassScroll.transform.SetParent(instrument.transform);
                bassScroll.transform.localPosition = new Vector3(0.3f, 1.75f, 0.1f);
                bassScroll.transform.localScale = Vector3.one * 0.1f;
                bassScroll.GetComponent<Renderer>().material = CreateMaterial(new Color(0.3f, 0.15f, 0.05f));
                Object.Destroy(bassScroll.GetComponent<Collider>());
                break;

            case "Trumpet":
                // Trumpet with bell and valves
                // Trumpet body (main tube)
                GameObject trumpetBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trumpetBody.name = "TrumpetBody";
                trumpetBody.transform.SetParent(instrument.transform);
                trumpetBody.transform.localPosition = new Vector3(0.15f, 1.3f, 0.15f);
                trumpetBody.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
                trumpetBody.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                trumpetBody.GetComponent<Renderer>().material = CreateMaterial(new Color(0.9f, 0.8f, 0.2f)); // Gold/brass
                Object.Destroy(trumpetBody.GetComponent<Collider>());

                // Trumpet bell (flared end)
                GameObject trumpetBell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                trumpetBell.name = "TrumpetBell";
                trumpetBell.transform.SetParent(instrument.transform);
                trumpetBell.transform.localPosition = new Vector3(0.35f, 1.3f, 0.15f);
                trumpetBell.transform.localScale = new Vector3(0.15f, 0.12f, 0.15f);
                trumpetBell.GetComponent<Renderer>().material = CreateMaterial(new Color(0.9f, 0.8f, 0.2f));
                Object.Destroy(trumpetBell.GetComponent<Collider>());

                // Trumpet valves
                for (int i = 0; i < 3; i++)
                {
                    GameObject valve = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    valve.name = "TrumpetValve";
                    valve.transform.SetParent(instrument.transform);
                    valve.transform.localPosition = new Vector3(0.18f + i * 0.03f, 1.25f, 0.15f);
                    valve.transform.localScale = new Vector3(0.02f, 0.06f, 0.02f);
                    valve.GetComponent<Renderer>().material = CreateMaterial(new Color(0.9f, 0.8f, 0.2f));
                    Object.Destroy(valve.GetComponent<Collider>());
                }
                break;

            case "Saxophone":
                // Saxophone with curved body and bell
                // Sax body (curved main tube)
                GameObject saxBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                saxBody.name = "SaxBody";
                saxBody.transform.SetParent(instrument.transform);
                saxBody.transform.localPosition = new Vector3(0.2f, 1.1f, 0.15f);
                saxBody.transform.localScale = new Vector3(0.06f, 0.3f, 0.06f);
                saxBody.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
                saxBody.GetComponent<Renderer>().material = CreateMaterial(new Color(0.85f, 0.75f, 0.25f)); // Gold
                Object.Destroy(saxBody.GetComponent<Collider>());

                // Sax bell (large opening)
                GameObject saxBell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                saxBell.name = "SaxBell";
                saxBell.transform.SetParent(instrument.transform);
                saxBell.transform.localPosition = new Vector3(0.25f, 0.85f, 0.2f);
                saxBell.transform.localScale = new Vector3(0.18f, 0.15f, 0.18f);
                saxBell.GetComponent<Renderer>().material = CreateMaterial(new Color(0.85f, 0.75f, 0.25f));
                Object.Destroy(saxBell.GetComponent<Collider>());

                // Sax neck (top part with mouthpiece)
                GameObject saxNeck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                saxNeck.name = "SaxNeck";
                saxNeck.transform.SetParent(instrument.transform);
                saxNeck.transform.localPosition = new Vector3(0.15f, 1.35f, 0.18f);
                saxNeck.transform.localScale = new Vector3(0.03f, 0.12f, 0.03f);
                saxNeck.transform.localRotation = Quaternion.Euler(45f, 0f, -15f);
                saxNeck.GetComponent<Renderer>().material = CreateMaterial(new Color(0.85f, 0.75f, 0.25f));
                Object.Destroy(saxNeck.GetComponent<Collider>());
                break;

            case "Percussion":
                // Hand-held drum with drumsticks
                // Drum body
                GameObject percDrum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                percDrum.name = "PercussionDrum";
                percDrum.transform.SetParent(instrument.transform);
                percDrum.transform.localPosition = new Vector3(0.2f, 1.0f, 0.1f);
                percDrum.transform.localScale = new Vector3(0.18f, 0.12f, 0.18f);
                percDrum.GetComponent<Renderer>().material = CreateMaterial(new Color(0.8f, 0.6f, 0.2f)); // Yellow-ish
                Object.Destroy(percDrum.GetComponent<Collider>());

                // Drumstick 1
                GameObject stick1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick1.name = "Drumstick1";
                stick1.transform.SetParent(instrument.transform);
                stick1.transform.localPosition = new Vector3(0.25f, 1.15f, 0.05f);
                stick1.transform.localScale = new Vector3(0.02f, 0.15f, 0.02f);
                stick1.transform.localRotation = Quaternion.Euler(45f, 0f, 30f);
                stick1.GetComponent<Renderer>().material = CreateMaterial(new Color(0.6f, 0.4f, 0.2f)); // Wood
                Object.Destroy(stick1.GetComponent<Collider>());

                // Drumstick 2
                GameObject stick2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick2.name = "Drumstick2";
                stick2.transform.SetParent(instrument.transform);
                stick2.transform.localPosition = new Vector3(0.15f, 1.15f, 0.15f);
                stick2.transform.localScale = new Vector3(0.02f, 0.15f, 0.02f);
                stick2.transform.localRotation = Quaternion.Euler(45f, 0f, -30f);
                stick2.GetComponent<Renderer>().material = CreateMaterial(new Color(0.6f, 0.4f, 0.2f));
                Object.Destroy(stick2.GetComponent<Collider>());
                break;

            case "Cowbell":
                // Cowbell with beater/stick
                // Cowbell body (rectangular bell shape)
                GameObject cowbell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cowbell.name = "Cowbell";
                cowbell.transform.SetParent(instrument.transform);
                cowbell.transform.localPosition = new Vector3(0.2f, 1.0f, 0.1f);
                cowbell.transform.localScale = new Vector3(0.1f, 0.15f, 0.08f);
                cowbell.GetComponent<Renderer>().material = CreateMaterial(new Color(0.7f, 0.7f, 0.7f)); // Silver/metal
                Object.Destroy(cowbell.GetComponent<Collider>());

                // Cowbell beater (stick)
                GameObject beater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beater.name = "CowbellBeater";
                beater.transform.SetParent(instrument.transform);
                beater.transform.localPosition = new Vector3(0.25f, 1.05f, 0.05f);
                beater.transform.localScale = new Vector3(0.02f, 0.12f, 0.02f);
                beater.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                beater.GetComponent<Renderer>().material = CreateMaterial(new Color(0.3f, 0.2f, 0.1f)); // Wood handle
                Object.Destroy(beater.GetComponent<Collider>());

                // Handle for holding cowbell
                GameObject cowbellHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cowbellHandle.name = "CowbellHandle";
                cowbellHandle.transform.SetParent(instrument.transform);
                cowbellHandle.transform.localPosition = new Vector3(0.2f, 1.12f, 0.1f);
                cowbellHandle.transform.localScale = new Vector3(0.03f, 0.02f, 0.08f);
                cowbellHandle.GetComponent<Renderer>().material = CreateMaterial(new Color(0.5f, 0.5f, 0.5f));
                Object.Destroy(cowbellHandle.GetComponent<Collider>());
                break;

            case "Cuica":
                // Cuica (Brazilian friction drum) with stick through top
                // Cuica drum body (small barrel)
                GameObject cuicaDrum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cuicaDrum.name = "CuicaDrum";
                cuicaDrum.transform.SetParent(instrument.transform);
                cuicaDrum.transform.localPosition = new Vector3(0.2f, 1.0f, 0.1f);
                cuicaDrum.transform.localScale = new Vector3(0.15f, 0.18f, 0.15f);
                cuicaDrum.GetComponent<Renderer>().material = CreateMaterial(new Color(0.3f, 0.6f, 0.9f)); // Blue
                Object.Destroy(cuicaDrum.GetComponent<Collider>());

                // Cuica stick (friction stick protruding from top)
                GameObject cuicaStick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cuicaStick.name = "CuicaStick";
                cuicaStick.transform.SetParent(instrument.transform);
                cuicaStick.transform.localPosition = new Vector3(0.2f, 1.25f, 0.1f);
                cuicaStick.transform.localScale = new Vector3(0.02f, 0.15f, 0.02f);
                cuicaStick.GetComponent<Renderer>().material = CreateMaterial(new Color(0.6f, 0.4f, 0.2f)); // Wood
                Object.Destroy(cuicaStick.GetComponent<Collider>());

                // Cloth on stick (for friction)
                GameObject cloth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cloth.name = "FrictionCloth";
                cloth.transform.SetParent(instrument.transform);
                cloth.transform.localPosition = new Vector3(0.25f, 1.15f, 0.1f);
                cloth.transform.localScale = Vector3.one * 0.05f;
                cloth.GetComponent<Renderer>().material = CreateMaterial(new Color(0.9f, 0.9f, 0.8f)); // White cloth
                Object.Destroy(cloth.GetComponent<Collider>());
                break;
        }

        return instrument;
    }

    void CreateMusicalNoteParticles()
    {
        musicalNotes = new GameObject[noteParticleCount];

        Material noteMat = new Material(Shader.Find("Standard"));
        noteMat.color = new Color(1f, 1f, 0.3f);
        noteMat.EnableKeyword("_EMISSION");
        noteMat.SetColor("_EmissionColor", new Color(1f, 1f, 0.3f) * 1.5f);

        for (int i = 0; i < noteParticleCount; i++)
        {
            // Create simple sphere for musical note
            GameObject note = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            note.name = "MusicalNote_" + i;
            note.transform.SetParent(transform);

            // Random position around the band
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0f, particleEmitRadius);
            float height = Random.Range(1f, 3f);

            note.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                height,
                Mathf.Sin(angle) * radius
            );

            note.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);
            note.GetComponent<Renderer>().material = noteMat;
            Object.Destroy(note.GetComponent<Collider>());

            musicalNotes[i] = note;
        }
    }

    Material CreateMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Check if in Jungle Realm
        if (!IsInJungleRealm())
        {
            if (isPlaying && audioSource != null)
            {
                audioSource.Pause();
            }
            return;
        }
        else if (!isPlaying && audioSource != null && initialized)
        {
            audioSource.UnPause();
            isPlaying = true;
        }

        // Patrol movement
        UpdatePatrolMovement();

        // Check player distance for interaction
        CheckPlayerProximity();

        // Handle interaction input
        if (playerNearby && Input.GetKeyDown(KeyCode.F))
        {
            ShowInteractionDialog();
        }

        // Update musical note particles
        UpdateMusicalNoteParticles();

        // Check if song ended and play next
        if (initialized && audioSource != null && isPlaying && !audioSource.isPlaying && songs.Count > 0)
        {
            PlayNextSong();
        }
    }

    bool IsInJungleRealm()
    {
        RealmManager rm = FindObjectOfType<RealmManager>();
        if (rm != null)
        {
            return rm.CurrentRealm == RealmType.JungleRealm;
        }

        // Fallback: check if band is in jungle area (X > 900)
        return transform.position.x > 900f;
    }

    void UpdatePatrolMovement()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Wait at patrol point
        if (patrolWaitTime > 0f)
        {
            patrolWaitTime -= Time.deltaTime;
            return;
        }

        // Move towards current patrol point
        Vector3 targetPos = patrolPoints[currentPatrolIndex];
        Vector3 direction = (targetPos - transform.position).normalized;

        // Face movement direction
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        }

        // Move band
        transform.position = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);

        // Animate band members (simple bob walk cycle)
        float bobSpeed = 5f;
        float bobAmount = 0.05f;
        for (int i = 0; i < bandMembers.Length; i++)
        {
            if (bandMembers[i].gameObject != null)
            {
                Vector3 basePos = bandMembers[i].formationOffset;
                float bobOffset = Mathf.Sin(Time.time * bobSpeed + i * 0.5f) * bobAmount;
                bandMembers[i].gameObject.transform.localPosition = basePos + new Vector3(0f, bobOffset, 0f);
            }
        }

        // Check if reached patrol point
        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            patrolWaitTime = Random.Range(2f, 5f); // Wait 2-5 seconds at each point
        }
    }

    void CheckPlayerProximity()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerNearby = distance < interactionDistance;
        }
        else
        {
            playerNearby = false;
        }
    }

    void UpdateMusicalNoteParticles()
    {
        if (musicalNotes == null) return;
        if (!isPlaying || audioSource == null || !audioSource.isPlaying) return;

        float time = Time.time;

        for (int i = 0; i < musicalNotes.Length; i++)
        {
            if (musicalNotes[i] == null) continue;

            // Float upward and rotate
            Vector3 pos = musicalNotes[i].transform.localPosition;
            pos.y += Time.deltaTime * Random.Range(0.3f, 0.8f);

            // Gentle swaying
            pos.x += Mathf.Sin(time * 2f + i) * Time.deltaTime * 0.2f;
            pos.z += Mathf.Cos(time * 1.5f + i) * Time.deltaTime * 0.2f;

            // Reset if too high
            if (pos.y > 5f)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(0f, particleEmitRadius);
                pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Random.Range(1f, 2f),
                    Mathf.Sin(angle) * radius
                );
            }

            musicalNotes[i].transform.localPosition = pos;

            // Rotate for visual interest
            musicalNotes[i].transform.Rotate(Vector3.up, Time.deltaTime * 50f);

            // Pulse scale with music
            float pulse = 1f + Mathf.Sin(time * 8f + i * 0.3f) * 0.2f;
            float baseScale = Random.Range(0.08f, 0.15f);
            musicalNotes[i].transform.localScale = Vector3.one * baseScale * pulse;
        }
    }

    void ShowInteractionDialog()
    {
        // Toggle music on/off when player presses F
        if (!isPlaying)
        {
            // Start playing music
            if (songs.Count > 0 && audioSource != null)
            {
                audioSource.Play();
                isPlaying = true;
                Debug.Log("CumbiaBand: Music started!");

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Cumbia band starts playing!", new Color(0.9f, 0.7f, 0.2f));
                }
            }
        }
        else
        {
            // Stop playing music
            if (audioSource != null)
            {
                audioSource.Stop();
                isPlaying = false;
                Debug.Log("CumbiaBand: Music stopped!");

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Band takes a break...", new Color(0.9f, 0.7f, 0.2f));
                }
            }
        }
    }

    void PlayNextSong()
    {
        if (songs.Count == 0) return;

        // Play songs in RANDOM ORDER (shuffle mode)
        int previousIndex = currentSongIndex;

        // If we have more than one song, make sure we don't play the same song twice in a row
        if (songs.Count > 1)
        {
            do
            {
                currentSongIndex = Random.Range(0, songs.Count);
            }
            while (currentSongIndex == previousIndex);
        }
        else
        {
            currentSongIndex = 0;
        }

        audioSource.clip = songs[currentSongIndex];
        audioSource.Play();

        Debug.Log("CumbiaBand: Now playing (shuffled) - " + loadedSongNames[currentSongIndex]);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Now playing: " + loadedSongNames[currentSongIndex], new Color(0.9f, 0.7f, 0.3f));
        }
    }

    void OnGUI()
    {
        if (!initialized || !MainMenu.GameStarted) return;
        if (!playerNearby) return;

        // Show interaction prompt
        GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
        promptStyle.fontSize = 20;
        promptStyle.fontStyle = FontStyle.Bold;
        promptStyle.alignment = TextAnchor.MiddleCenter;
        promptStyle.normal.textColor = new Color(0.9f, 0.7f, 0.2f);

        string prompt = isPlaying ? "[F] Stop Cumbia Band" : "[F] Start Cumbia Band";
        Vector2 size = promptStyle.CalcSize(new GUIContent(prompt));
        GUI.Label(new Rect((Screen.width - size.x) / 2, Screen.height - 150, size.x, size.y), prompt, promptStyle);

        // Show currently playing song
        if (isPlaying && loadedSongNames.Count > 0)
        {
            GUIStyle songStyle = new GUIStyle(GUI.skin.label);
            songStyle.fontSize = 16;
            songStyle.alignment = TextAnchor.MiddleCenter;
            songStyle.normal.textColor = new Color(1f, 0.9f, 0.5f, 0.8f);

            string nowPlaying = "Now Playing: " + loadedSongNames[currentSongIndex];
            Vector2 songSize = songStyle.CalcSize(new GUIContent(nowPlaying));
            GUI.Label(new Rect((Screen.width - songSize.x) / 2, Screen.height - 120, songSize.x, songSize.y), nowPlaying, songStyle);
        }
    }

    void OnDestroy()
    {
        // Clean up
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
