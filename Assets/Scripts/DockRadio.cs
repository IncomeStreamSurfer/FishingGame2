using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Small radio on the dock near the BBQ
/// Press F to toggle on/off - plays the same songs as shop radio
/// Has 3D spatial audio with distance falloff (doppler effect)
/// </summary>
public class DockRadio : MonoBehaviour
{
    public static DockRadio Instance { get; private set; }

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

    // Multiple songs (same as shop radio)
    private List<AudioClip> songs = new List<AudioClip>();
    private string[] songNames = { "EvilBobsIsland", "Venomous", "ScapeOriginal", "Baroque", "Melodrama" };
    private int currentSongIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        Debug.Log("DockRadio: Starting initialization...");
        CreateRadioVisuals();
        Invoke("SetupAudio", 0.5f);
    }

    void SetupAudio()
    {
        // Load all songs from Resources
        foreach (string songName in songNames)
        {
            AudioClip clip = Resources.Load<AudioClip>(songName);
            if (clip != null)
            {
                songs.Add(clip);
            }
        }

        if (songs.Count == 0)
        {
            Debug.LogWarning("DockRadio: No songs found!");
            return;
        }

        Debug.Log("DockRadio: Loaded " + songs.Count + " songs!");

        // Create audio source with 3D spatial audio (doppler effect)
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = songs[0];
        audioSource.loop = false;
        audioSource.volume = maxVolume;
        audioSource.spatialBlend = 1f;  // Full 3D sound
        audioSource.minDistance = 1.5f;  // Full volume within 1.5 units
        audioSource.maxDistance = 30f;   // Audible up to 30 units
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0.5f;
        audioSource.playOnAwake = false;
        audioSource.priority = 0;

        initialized = true;
        Debug.Log("DockRadio: Ready! Press F to toggle music.");
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

        // Check distance to player
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerNearby = distance < interactionDistance;
        }

        // Press F only when near radio to toggle
        if (playerNearby && Input.GetKeyDown(KeyCode.R))
        {
            ToggleRadio();
        }

        // Check if song ended, play next
        if (isOn && !audioSource.isPlaying && songs.Count > 0)
        {
            PlayNextSong();
        }

        UpdateLED();
    }

    void OnGUI()
    {
        if (!initialized || !MainMenu.GameStarted) return;

        // Show interaction prompt when player is nearby
        if (playerNearby)
        {
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 16;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.6f, 0.9f, 1f);

            string promptText = isOn ? "[F] Turn Off Dock Radio" : "[F] Play Dock Radio";
            float promptY = Screen.height * 0.65f;
            GUI.Label(new Rect(0, promptY, Screen.width, 30), promptText, promptStyle);
        }
    }

    void ToggleRadio()
    {
        isOn = !isOn;

        if (isOn)
        {
            if (songs.Count > 0)
            {
                // Start at random song for variety
                currentSongIndex = Random.Range(0, songs.Count);
                audioSource.clip = songs[currentSongIndex];
                audioSource.volume = maxVolume;
                audioSource.Play();
                Debug.Log("DockRadio: ON - Playing " + songNames[currentSongIndex]);

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Dock Radio ON - " + songNames[currentSongIndex], new Color(0.4f, 0.8f, 1f));
                }
            }
        }
        else
        {
            audioSource.Stop();
            Debug.Log("DockRadio: OFF");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Dock Radio OFF", new Color(0.8f, 0.6f, 0.4f));
            }
        }
    }

    void PlayNextSong()
    {
        currentSongIndex = (currentSongIndex + 1) % songs.Count;
        audioSource.clip = songs[currentSongIndex];
        audioSource.volume = maxVolume;
        audioSource.Play();
        Debug.Log("DockRadio: Now playing - " + songNames[currentSongIndex]);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Now playing: " + songNames[currentSongIndex], new Color(0.5f, 0.8f, 1f));
        }
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
