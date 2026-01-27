using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Aggressive distance-based culling system.
/// Disables renderers on objects far from the player to improve FPS.
/// This is a runtime optimization that doesn't modify the scene.
/// </summary>
public class AggressiveCulling : MonoBehaviour
{
    public static AggressiveCulling Instance { get; private set; }

    [Header("Culling Settings")]
    [Tooltip("Objects beyond this distance will be hidden")]
    public float cullDistance = 40f;

    [Tooltip("How often to update culling (seconds)")]
    public float updateInterval = 0.5f;

    [Tooltip("Max objects to process per frame")]
    public int maxObjectsPerFrame = 500;

    private Transform playerTransform;
    private List<RendererData> allRenderers = new List<RendererData>();
    private int currentIndex = 0;
    private float lastUpdateTime = 0f;
    private bool initialized = false;

    // Track stats
    private int hiddenCount = 0;
    private int visibleCount = 0;

    private struct RendererData
    {
        public Renderer renderer;
        public Vector3 position;
        public bool wasEnabled;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // DISABLED - was causing player arms and trees to disappear
        // The jungle realm cleanup should be enough for FPS now
        Debug.Log("[AggressiveCulling] DISABLED - causing rendering issues");
        enabled = false;
        return;

        // Invoke("Initialize", 3f); // Wait for scene to fully load
    }

    void Initialize()
    {
        Debug.Log("[AggressiveCulling] Initializing distance-based culling system...");

        // Find player
        if (GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        // Collect all renderers in scene
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer rend in renderers)
        {
            // Skip UI elements, particles, and essential objects
            if (rend.gameObject.layer == LayerMask.NameToLayer("UI")) continue;
            if (rend.GetComponent<ParticleSystemRenderer>() != null) continue;

            // Skip player and camera objects
            string name = rend.gameObject.name.ToLower();
            if (name.Contains("player") || name.Contains("camera") || name.Contains("canvas")) continue;
            if (name.Contains("water") || name.Contains("terrain") || name.Contains("ground")) continue;

            // Skip important NPCs (they handle their own visibility)
            if (name.Contains("goldie") || name.Contains("connoisseur") || name.Contains("orangutan")) continue;
            if (name.Contains("tutorial") || name.Contains("poll")) continue;

            allRenderers.Add(new RendererData
            {
                renderer = rend,
                position = rend.transform.position,
                wasEnabled = rend.enabled
            });
        }

        initialized = true;
        Debug.Log($"[AggressiveCulling] Tracking {allRenderers.Count} renderers for distance culling");
    }

    void Update()
    {
        if (!initialized || !MainMenu.GameStarted) return;

        // Update player reference
        if (playerTransform == null && GameCache.IsPlayerValid())
            playerTransform = GameCache.Player;

        if (playerTransform == null) return;

        // Throttle updates
        if (Time.time - lastUpdateTime < updateInterval) return;
        lastUpdateTime = Time.time;

        Vector3 playerPos = playerTransform.position;
        float cullDistSqr = cullDistance * cullDistance;

        hiddenCount = 0;
        visibleCount = 0;

        // Process a batch of renderers
        int processed = 0;
        int startIndex = currentIndex;

        while (processed < maxObjectsPerFrame && allRenderers.Count > 0)
        {
            if (currentIndex >= allRenderers.Count)
                currentIndex = 0;

            RendererData data = allRenderers[currentIndex];

            if (data.renderer != null)
            {
                float distSqr = (data.position - playerPos).sqrMagnitude;
                bool shouldBeVisible = distSqr < cullDistSqr;

                if (data.renderer.enabled != shouldBeVisible)
                {
                    data.renderer.enabled = shouldBeVisible;
                }

                if (shouldBeVisible) visibleCount++;
                else hiddenCount++;
            }

            currentIndex++;
            processed++;

            // Full loop completed
            if (currentIndex == startIndex) break;
        }
    }

    // Show debug info
    void OnGUI()
    {
        if (!initialized) return;

        // Only show if shift+C is held (toggle with existing debug keys)
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.C))
        {
            GUI.Label(new Rect(10, 150, 300, 25), $"Culling: {hiddenCount} hidden, {visibleCount} visible");
            GUI.Label(new Rect(10, 175, 300, 25), $"Cull Distance: {cullDistance}m");
        }
    }

    /// <summary>
    /// Adjust cull distance at runtime
    /// </summary>
    public void SetCullDistance(float distance)
    {
        cullDistance = Mathf.Max(10f, distance);
        Debug.Log($"[AggressiveCulling] Cull distance set to {cullDistance}m");
    }

    /// <summary>
    /// Force immediate full update
    /// </summary>
    public void ForceUpdate()
    {
        if (!initialized || playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        float cullDistSqr = cullDistance * cullDistance;

        foreach (RendererData data in allRenderers)
        {
            if (data.renderer == null) continue;

            float distSqr = (data.position - playerPos).sqrMagnitude;
            data.renderer.enabled = distSqr < cullDistSqr;
        }
    }

    void OnDestroy()
    {
        // Restore all renderers
        foreach (RendererData data in allRenderers)
        {
            if (data.renderer != null)
                data.renderer.enabled = data.wasEnabled;
        }
        allRenderers.Clear();
    }
}
