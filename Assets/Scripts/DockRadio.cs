using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Small radio on the dock near the BBQ
/// Press F to toggle on/off - plays the same songs as shop radio
/// Has 3D spatial audio with distance falloff (doppler effect)
/// </summary>
public class DockRadio : MonoBehaviour
{
    // Track ALL radio instances so we can stop others when one plays
    private static List<DockRadio> allRadios = new List<DockRadio>();
    private static DockRadio currentlyPlaying = null;

    [Header("Audio Settings")]
    public float maxVolume = 0.175f;  // 50% reduced volume

    [Header("Visual Settings")]
    public Color radioColor = new Color(0.3f, 0.35f, 0.4f);  // Blueish gray
    public Color speakerColor = new Color(0.15f, 0.15f, 0.15f);
    public Color indicatorLightColor = new Color(0.3f, 0.8f, 1f);  // Cyan
    public Color offLightColor = new Color(0.4f, 0.2f, 0.1f);

    private AudioSource audioSource;
    private GameObject indicatorLight;
    private float lightPulseTime;
    private bool initialized = false;
    private bool isOn = false;
    private bool playerNearby = false;
    private float interactionDistance = 3.5f;
    private int guiFrameSkip = 0;
    private float songEndCheckDelay = 0.5f;  // Delay before checking if song ended

    // Multiple songs - loaded based on current realm
    private List<AudioClip> songs = new List<AudioClip>();
    private List<string> loadedSongNames = new List<string>();
    private string[] songNames = { "EvilBobsIsland", "Venomous", "ScapeOriginal", "Baroque", "Melodrama" }; // Fallback only
    private int currentSongIndex = 0;
    private RealmType lastLoadedRealm = RealmType.TropicalIsland;

    void Awake()
    {
        // Register this radio instance
        if (!allRadios.Contains(this))
            allRadios.Add(this);
    }

    void OnDestroy()
    {
        allRadios.Remove(this);
        if (currentlyPlaying == this)
            currentlyPlaying = null;
    }

    void Start()
    {
        Debug.Log("DockRadio: Starting initialization...");
        CreateRadioVisuals();
        Invoke("SetupAudio", 0.5f);
    }

    void SetupAudio()
    {
        LoadSongsForCurrentRealm();

        // Create audio source with 3D spatial audio (doppler effect)
        audioSource = gameObject.AddComponent<AudioSource>();
        if (songs.Count > 0)
            audioSource.clip = songs[0];
        audioSource.loop = true;  // Loop single song
        audioSource.volume = maxVolume;
        audioSource.spatialBlend = 1f;  // Full 3D sound
        audioSource.minDistance = 1.5f;  // Full volume within 1.5 units
        audioSource.maxDistance = 30f;   // Audible up to 30 units
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;  // No doppler effect
        audioSource.playOnAwake = false;
        audioSource.priority = 0;

        initialized = true;
        Debug.Log("DockRadio: Ready! Press R to toggle music.");

        // Auto-start music when game loads
        Invoke("AutoStartMusic", 0.5f);
    }

    void AutoStartMusic()
    {
        // Check if we can play
        if (audioSource == null)
        {
            Debug.LogWarning("DockRadio: AudioSource is null, cannot auto-start");
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("DockRadio: GameObject is not active, cannot auto-start");
            return;
        }

        if (!isOn && songs.Count > 0)
        {
            // Ensure audio source component is enabled
            if (!audioSource.enabled)
            {
                audioSource.enabled = true;
                Debug.Log("DockRadio: Re-enabled audio source");
            }

            // Double-check it's enabled now
            if (!audioSource.enabled)
            {
                Debug.LogError("DockRadio: Failed to enable audio source!");
                return;
            }

            isOn = true;
            currentSongIndex = Random.Range(0, songs.Count);
            audioSource.clip = songs[currentSongIndex];
            audioSource.volume = maxVolume;
            audioSource.Play();
            currentlyPlaying = this;
            Debug.Log("DockRadio: Auto-started - Playing " + songs[currentSongIndex].name);
        }
    }

    void LoadSongsForCurrentRealm()
    {
        // Get current realm
        RealmType currentRealm = GameCache.GetCurrentRealm();

        // Don't reload if same realm
        if (songs.Count > 0 && currentRealm == lastLoadedRealm)
            return;

        // Clear existing songs
        songs.Clear();
        loadedSongNames.Clear();
        lastLoadedRealm = currentRealm;

        // Determine folder based on realm:
        // Tropical Island -> dubscape
        // Jungle -> cumbiascape
        // Ice -> hardscape
        string folderName = GetMusicFolderForRealm(currentRealm);

        Debug.Log("DockRadio: Loading music from " + folderName + " for realm " + currentRealm);

        AudioClip[] clips = Resources.LoadAll<AudioClip>(folderName);

        if (clips != null && clips.Length > 0)
        {
            foreach (AudioClip clip in clips)
            {
                songs.Add(clip);
                loadedSongNames.Add(clip.name);
                Debug.Log("DockRadio: Loaded " + clip.name + " from " + folderName);
            }
        }

        // Fallback if folder empty
        if (songs.Count == 0)
        {
            Debug.LogWarning("DockRadio: No songs in " + folderName + "! Trying fallback...");
            foreach (string songName in songNames)
            {
                AudioClip clip = Resources.Load<AudioClip>(songName);
                if (clip != null)
                {
                    songs.Add(clip);
                    loadedSongNames.Add(clip.name);
                }
            }
        }

        Debug.Log("DockRadio: Loaded " + songs.Count + " songs for " + currentRealm);
    }

    string GetMusicFolderForRealm(RealmType realm)
    {
        switch (realm)
        {
            case RealmType.TropicalIsland:
                return "dubscape";
            case RealmType.JungleRealm:
                return "CUMBIASCAPE";
            case RealmType.IceRealm:
                return "HARDSCAPE";
            case RealmType.VolcanicRealm:
                return "dubscape";  // Default to dubscape for volcanic
            default:
                return "dubscape";
        }
    }

    void CreateRadioVisuals()
    {
        // Smaller, portable radio design

        // Main body - compact boombox style
        GameObject radioBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        radioBody.name = "RadioBody";
        radioBody.transform.SetParent(transform);
        radioBody.transform.localPosition = Vector3.zero;
        radioBody.transform.localScale = new Vector3(0.3f, 0.18f, 0.12f);
        Material bodyMat = CreateMaterial(radioColor);
        bodyMat.SetFloat("_Metallic", 0.3f);
        bodyMat.SetFloat("_Glossiness", 0.4f);
        radioBody.GetComponent<Renderer>().material = bodyMat;
        Object.Destroy(radioBody.GetComponent<Collider>());

        // Speaker grille (center)
        GameObject speaker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        speaker.name = "Speaker";
        speaker.transform.SetParent(transform);
        speaker.transform.localPosition = new Vector3(0f, 0f, 0.055f);
        speaker.transform.localScale = new Vector3(0.12f, 0.02f, 0.12f);
        speaker.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        speaker.GetComponent<Renderer>().material = CreateMaterial(speakerColor);
        Object.Destroy(speaker.GetComponent<Collider>());

        // Power LED
        indicatorLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicatorLight.name = "PowerLED";
        indicatorLight.transform.SetParent(transform);
        indicatorLight.transform.localPosition = new Vector3(-0.1f, 0.06f, 0.055f);
        indicatorLight.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        Material ledMat = CreateMaterial(offLightColor);
        ledMat.EnableKeyword("_EMISSION");
        ledMat.SetColor("_EmissionColor", offLightColor * 0.5f);
        indicatorLight.GetComponent<Renderer>().material = ledMat;
        Object.Destroy(indicatorLight.GetComponent<Collider>());

        // Handle on top
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Handle";
        handle.transform.SetParent(transform);
        handle.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        handle.transform.localScale = new Vector3(0.15f, 0.02f, 0.03f);
        handle.GetComponent<Renderer>().material = CreateMaterial(Color.gray);
        Object.Destroy(handle.GetComponent<Collider>());

        // Handle supports
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
            support.name = "HandleSupport";
            support.transform.SetParent(transform);
            support.transform.localPosition = new Vector3(i * 0.07f, 0.1f, 0f);
            support.transform.localScale = new Vector3(0.015f, 0.04f, 0.02f);
            support.GetComponent<Renderer>().material = CreateMaterial(Color.gray);
            Object.Destroy(support.GetComponent<Collider>());
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
        if (!initialized) return;
        if (!MainMenu.GameStarted) return;
        if (audioSource == null) return;

        // Check distance to player using cached reference
        if (GameCache.IsPlayerValid())
        {
            float distance = Vector3.Distance(transform.position, GameCache.Player.position);
            playerNearby = distance < interactionDistance;
        }

        // Press F only when near radio to toggle
        if (playerNearby && Input.GetKeyDown(KeyCode.R))
        {
            ToggleRadio();
        }

        // Song loops automatically now (loop=true)

        UpdateLED();
    }

    void OnGUI()
    {
        // Performance: Skip frames when not actively needed
        if (!playerNearby && !isOn)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        if (!initialized || !MainMenu.GameStarted) return;

        // Show interaction prompt when player is nearby
        if (playerNearby)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 16;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.6f, 0.9f, 1f);

            string promptText = isOn ? "[R] Turn Off Dock Radio" : "[R] Play Dock Radio";
            float promptY = Screen.height * 0.65f;
            GUI.Label(new Rect(0, promptY, Screen.width, 30), promptText, promptStyle);
        }

    }

    void ToggleRadio()
    {
        isOn = !isOn;

        if (isOn)
        {
            // Stop ALL other radios first
            StopAllOtherRadios();

            // Reload songs if realm changed
            RealmType currentRealm = GameCache.GetCurrentRealm();
            if (currentRealm != lastLoadedRealm)
            {
                LoadSongsForCurrentRealm();
            }

            if (songs.Count > 0)
            {
                // Start at random song for variety
                currentSongIndex = Random.Range(0, songs.Count);
                audioSource.clip = songs[currentSongIndex];
                audioSource.volume = maxVolume;
                audioSource.Play();
                currentlyPlaying = this;
                songEndCheckDelay = 0.5f;  // Reset delay
                Debug.Log("DockRadio: ON - Playing " + songs[currentSongIndex].name + " (" + GetMusicFolderForRealm(currentRealm) + ")");
            }
        }
        else
        {
            audioSource.Stop();
            if (currentlyPlaying == this)
                currentlyPlaying = null;
            Debug.Log("DockRadio: OFF");
        }
    }

    void StopAllOtherRadios()
    {
        foreach (DockRadio radio in allRadios)
        {
            if (radio != this && radio.isOn && radio.audioSource != null)
            {
                radio.audioSource.Stop();
                radio.isOn = false;
            }
        }
    }

    void PlayNextSong()
    {
        currentSongIndex = (currentSongIndex + 1) % songs.Count;
        audioSource.clip = songs[currentSongIndex];
        audioSource.volume = maxVolume;
        audioSource.Play();
        songEndCheckDelay = 0.5f;  // Reset delay after starting new song
        Debug.Log("DockRadio: Now playing " + songs[currentSongIndex].name);
    }

    void UpdateLED()
    {
        if (indicatorLight == null) return;

        Renderer rend = indicatorLight.GetComponent<Renderer>();
        if (rend == null) return;

        if (isOn)
        {
            // Cyan pulsing light when on
            lightPulseTime += Time.deltaTime;
            float pulse = 0.7f + Mathf.Sin(lightPulseTime * 4f) * 0.3f;
            rend.material.SetColor("_EmissionColor", indicatorLightColor * pulse * 2f);
            rend.material.color = indicatorLightColor;
        }
        else
        {
            // Orange dim light when off
            rend.material.SetColor("_EmissionColor", offLightColor * 0.5f);
            rend.material.color = offLightColor;
        }
    }
}
