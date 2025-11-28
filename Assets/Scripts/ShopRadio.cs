using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A radio that plays music - Press F to toggle on/off
/// </summary>
public class ShopRadio : MonoBehaviour
{
    public static ShopRadio Instance { get; private set; }

    [Header("Audio Settings")]
    public float maxVolume = 0.2f;

    [Header("Visual Settings")]
    public Color radioColor = new Color(0.4f, 0.25f, 0.15f);
    public Color speakerColor = new Color(0.2f, 0.2f, 0.2f);
    public Color indicatorLightColor = new Color(0.2f, 1f, 0.3f);
    public Color offLightColor = new Color(0.3f, 0.1f, 0.1f);

    private AudioSource audioSource;
    private GameObject indicatorLight;
    private float lightPulseTime;
    private bool initialized = false;
    private bool isOn = false;  // Radio starts OFF
    private bool playerNearby = false;
    private float interactionDistance = 4f;  // Must be within 4 units to use radio

    // Multiple songs
    private List<AudioClip> songs = new List<AudioClip>();
    private string[] songNames = { "EvilBobsIsland", "Venomous", "ScapeOriginal", "Baroque", "Melodrama" };
    private int currentSongIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        Debug.Log("ShopRadio: Starting initialization...");
        CreateRadioVisuals();

        // Ensure there's an AudioListener on the main camera
        EnsureAudioListener();

        Invoke("SetupAudio", 0.5f);
    }

    void EnsureAudioListener()
    {
        // Check if any AudioListener exists
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            // Add to main camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.gameObject.AddComponent<AudioListener>();
                Debug.Log("ShopRadio: Added AudioListener to main camera");
            }
        }
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
                Debug.Log("ShopRadio: Loaded song - " + songName);
            }
            else
            {
                Debug.LogWarning("ShopRadio: Could not load - " + songName);
            }
        }

        if (songs.Count == 0)
        {
            Debug.LogError("ShopRadio: No songs found! Make sure audio files are in Assets/Resources/");
            return;
        }

        Debug.Log("ShopRadio: Loaded " + songs.Count + " songs!");

        // Create audio source with doppler/distance falloff
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = songs[0];
        audioSource.loop = false;
        audioSource.volume = maxVolume;
        audioSource.spatialBlend = 1f;  // 3D sound with distance falloff
        audioSource.minDistance = 2f;   // Full volume within 2 units
        audioSource.maxDistance = 50f;  // Audible up to 50 units
        audioSource.rolloffMode = AudioRolloffMode.Linear;  // Linear falloff for clear effect
        audioSource.dopplerLevel = 0.5f;  // Subtle doppler effect
        audioSource.playOnAwake = false;
        audioSource.priority = 0;

        initialized = true;
        Debug.Log("ShopRadio: Ready! Press F to toggle music on/off.");
    }

    void CreateRadioVisuals()
    {
        // Main radio body
        GameObject radioBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        radioBody.name = "RadioBody";
        radioBody.transform.SetParent(transform);
        radioBody.transform.localPosition = Vector3.zero;
        radioBody.transform.localScale = new Vector3(0.4f, 0.25f, 0.15f);
        radioBody.GetComponent<Renderer>().material = CreateMaterial(radioColor);
        Object.Destroy(radioBody.GetComponent<Collider>());

        // Speaker left
        GameObject speakerLeft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        speakerLeft.name = "SpeakerLeft";
        speakerLeft.transform.SetParent(transform);
        speakerLeft.transform.localPosition = new Vector3(-0.1f, 0f, 0.07f);
        speakerLeft.transform.localScale = new Vector3(0.1f, 0.02f, 0.1f);
        speakerLeft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        speakerLeft.GetComponent<Renderer>().material = CreateMaterial(speakerColor);
        Object.Destroy(speakerLeft.GetComponent<Collider>());

        // Speaker right
        GameObject speakerRight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        speakerRight.name = "SpeakerRight";
        speakerRight.transform.SetParent(transform);
        speakerRight.transform.localPosition = new Vector3(0.1f, 0f, 0.07f);
        speakerRight.transform.localScale = new Vector3(0.1f, 0.02f, 0.1f);
        speakerRight.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        speakerRight.GetComponent<Renderer>().material = CreateMaterial(speakerColor);
        Object.Destroy(speakerRight.GetComponent<Collider>());

        // Power LED
        indicatorLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicatorLight.name = "PowerLED";
        indicatorLight.transform.SetParent(transform);
        indicatorLight.transform.localPosition = new Vector3(0f, 0.08f, 0.07f);
        indicatorLight.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
        Material ledMat = CreateMaterial(indicatorLightColor);
        ledMat.EnableKeyword("_EMISSION");
        ledMat.SetColor("_EmissionColor", indicatorLightColor * 2f);
        indicatorLight.GetComponent<Renderer>().material = ledMat;
        Object.Destroy(indicatorLight.GetComponent<Collider>());

        // Antenna
        GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.name = "Antenna";
        antenna.transform.SetParent(transform);
        antenna.transform.localPosition = new Vector3(0.15f, 0.2f, 0f);
        antenna.transform.localScale = new Vector3(0.015f, 0.15f, 0.015f);
        antenna.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
        Material antennaMat = CreateMaterial(Color.gray);
        antennaMat.SetFloat("_Metallic", 0.8f);
        antenna.GetComponent<Renderer>().material = antennaMat;
        Object.Destroy(antenna.GetComponent<Collider>());
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
            promptStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);

            string promptText = isOn ? "[R] Turn Off Radio" : "[R] Play Radio";
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
                audioSource.clip = songs[currentSongIndex];
                audioSource.volume = maxVolume;
                audioSource.Play();
                Debug.Log("ShopRadio: ON - Playing " + songNames[currentSongIndex]);

                // Show notification
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification("Radio ON - " + songNames[currentSongIndex], new Color(0.3f, 0.8f, 0.5f));
                }
            }
        }
        else
        {
            audioSource.Stop();
            Debug.Log("ShopRadio: OFF");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Radio OFF", new Color(0.8f, 0.5f, 0.3f));
            }
        }
    }

    void PlayNextSong()
    {
        currentSongIndex = (currentSongIndex + 1) % songs.Count;
        audioSource.clip = songs[currentSongIndex];
        audioSource.volume = maxVolume;
        audioSource.Play();
        Debug.Log("ShopRadio: Now playing - " + songNames[currentSongIndex]);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("Now playing: " + songNames[currentSongIndex], new Color(0.5f, 0.7f, 0.9f));
        }
    }

    void UpdateLED()
    {
        if (indicatorLight == null) return;

        Renderer rend = indicatorLight.GetComponent<Renderer>();
        if (rend == null) return;

        if (isOn)
        {
            // Green pulsing light when on
            lightPulseTime += Time.deltaTime;
            float pulse = 0.7f + Mathf.Sin(lightPulseTime * 3f) * 0.3f;
            rend.material.SetColor("_EmissionColor", indicatorLightColor * pulse * 2f);
        }
        else
        {
            // Red dim light when off
            rend.material.SetColor("_EmissionColor", offLightColor * 0.5f);
        }
    }
}
