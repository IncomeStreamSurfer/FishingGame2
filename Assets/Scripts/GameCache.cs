using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized cache for commonly accessed game objects
/// CRITICAL: This eliminates expensive GameObject.Find() and FindObjectOfType() calls
/// All scripts should use GameCache instead of Find() calls
/// </summary>
public class GameCache : MonoBehaviour
{
    public static GameCache Instance { get; private set; }

    // Core cached references
    public static Transform Player { get; private set; }
    public static GameObject PlayerObject { get; private set; }
    public static PlayerController PlayerCtrl { get; private set; }
    public static PlayerHealth PlayerHP { get; private set; }
    public static CharacterController PlayerCharController { get; private set; }

    // Managers
    public static RealmManager Realm { get; private set; }
    public static UIManager UI { get; private set; }
    public static FishingSystem Fishing { get; private set; }
    public static DayNightCycle DayNight { get; private set; }
    public static FishInventoryPanel FishInventory { get; private set; }
    public static FoodInventory Food { get; private set; }
    public static WeaponSystem Weapons { get; private set; }
    public static AccessorySystem Accessories { get; private set; }

    // NPCs - cached by name for quick lookup
    public static ClothingShopNPC ClothingShop { get; private set; }
    public static GoldieBanksNPC GoldieBanks { get; private set; }
    public static IceRealmShopNPC IceShop { get; private set; }
    public static JungleShopNPC JungleShop { get; private set; }
    public static WeaponShopNPC WeaponShop { get; private set; }
    public static OrangutanVendor OrangutanShop { get; private set; }
    public static TutorialCat TutCat { get; private set; }
    public static CandyCat CandyCatNPC { get; private set; }
    public static RenaCumbiaQueen Rena { get; private set; }
    public static WetsuitPeteQuests WetsuitPete { get; private set; }

    // Camera
    public static Camera MainCamera { get; private set; }
    public static CameraController CamController { get; private set; }

    // Audio
    public static DockRadio[] DockRadios { get; private set; }
    public static ShopRadio[] ShopRadios { get; private set; }

    // AI entities - cached lists for iteration
    public static List<SnakeAI> Snakes { get; private set; } = new List<SnakeAI>();
    public static List<PolarBearAI> PolarBears { get; private set; } = new List<PolarBearAI>();

    // Named object cache for Find() replacement
    private static Dictionary<string, GameObject> namedObjects = new Dictionary<string, GameObject>();

    // Shared materials cache (to avoid creating duplicates)
    private static Dictionary<string, Material> sharedMaterials = new Dictionary<string, Material>();

    // Shared textures cache
    private static Dictionary<Color, Texture2D> colorTextures = new Dictionary<Color, Texture2D>();

    // GUIStyle cache
    private static Dictionary<string, GUIStyle> guiStyles = new Dictionary<string, GUIStyle>();

    // Initialization flags
    private static bool isInitialized = false;
    private float refreshTimer = 0f;
    private const float REFRESH_INTERVAL = 5f; // Refresh less frequently - only for null checks

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
            EnsurePerformanceConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Second pass to catch late-initialized objects
        RefreshCache();
    }

    void Update()
    {
        // Only refresh if something is null
        refreshTimer += Time.deltaTime;
        if (refreshTimer >= REFRESH_INTERVAL)
        {
            refreshTimer = 0f;
            RefreshNullOnly();
        }
    }

    private void Initialize()
    {
        if (isInitialized) return;

        // Cache player immediately
        CachePlayer();

        // Cache camera
        MainCamera = Camera.main;
        if (MainCamera != null)
            CamController = MainCamera.GetComponent<CameraController>();

        isInitialized = true;
    }

    private void EnsurePerformanceConfig()
    {
        // Create PerformanceConfig if it doesn't exist
        if (PerformanceConfig.Instance == null)
        {
            GameObject perfGO = new GameObject("PerformanceConfig");
            perfGO.AddComponent<PerformanceConfig>();
            Debug.Log("GameCache: Created PerformanceConfig automatically");
        }
    }

    private void CachePlayer()
    {
        if (Player != null) return;

        PlayerObject = GameObject.Find("Player");
        if (PlayerObject != null)
        {
            Player = PlayerObject.transform;
            PlayerCtrl = PlayerObject.GetComponent<PlayerController>();
            PlayerHP = PlayerObject.GetComponent<PlayerHealth>();
            PlayerCharController = PlayerObject.GetComponent<CharacterController>();
        }
    }

    public void RefreshCache()
    {
        CachePlayer();

        // Cache all managers
        if (Realm == null) Realm = FindObjectOfType<RealmManager>();
        if (UI == null) UI = FindObjectOfType<UIManager>();
        if (Fishing == null) Fishing = FindObjectOfType<FishingSystem>();
        if (DayNight == null) DayNight = FindObjectOfType<DayNightCycle>();
        if (FishInventory == null) FishInventory = FindObjectOfType<FishInventoryPanel>();
        if (Food == null) Food = FindObjectOfType<FoodInventory>();
        if (Weapons == null) Weapons = FindObjectOfType<WeaponSystem>();
        if (Accessories == null) Accessories = FindObjectOfType<AccessorySystem>();

        // Cache NPCs
        if (ClothingShop == null) ClothingShop = FindObjectOfType<ClothingShopNPC>();
        if (GoldieBanks == null) GoldieBanks = FindObjectOfType<GoldieBanksNPC>();
        if (IceShop == null) IceShop = FindObjectOfType<IceRealmShopNPC>();
        if (JungleShop == null) JungleShop = FindObjectOfType<JungleShopNPC>();
        if (WeaponShop == null) WeaponShop = FindObjectOfType<WeaponShopNPC>();
        if (OrangutanShop == null) OrangutanShop = FindObjectOfType<OrangutanVendor>();
        if (TutCat == null) TutCat = FindObjectOfType<TutorialCat>();
        if (CandyCatNPC == null) CandyCatNPC = FindObjectOfType<CandyCat>();
        if (Rena == null) Rena = FindObjectOfType<RenaCumbiaQueen>();
        if (WetsuitPete == null) WetsuitPete = FindObjectOfType<WetsuitPeteQuests>();

        // Cache radios
        DockRadios = FindObjectsOfType<DockRadio>();
        ShopRadios = FindObjectsOfType<ShopRadio>();

        // Refresh AI lists
        RefreshAILists();
    }

    private void RefreshNullOnly()
    {
        // Only refresh things that are null - much cheaper than full refresh
        if (Player == null) CachePlayer();
        if (MainCamera == null) MainCamera = Camera.main;
    }

    public static void RefreshAILists()
    {
        // Clear and repopulate AI lists
        Snakes.Clear();
        Snakes.AddRange(FindObjectsOfType<SnakeAI>());

        PolarBears.Clear();
        PolarBears.AddRange(FindObjectsOfType<PolarBearAI>());
    }

    // Register/Unregister methods for AI entities (call from their Awake/OnDestroy)
    public static void RegisterSnake(SnakeAI snake)
    {
        if (!Snakes.Contains(snake))
            Snakes.Add(snake);
    }

    public static void UnregisterSnake(SnakeAI snake)
    {
        Snakes.Remove(snake);
    }

    public static void RegisterPolarBear(PolarBearAI bear)
    {
        if (!PolarBears.Contains(bear))
            PolarBears.Add(bear);
    }

    public static void UnregisterPolarBear(PolarBearAI bear)
    {
        PolarBears.Remove(bear);
    }

    // ===== HELPER METHODS =====

    // Get player position safely without null checks everywhere
    public static Vector3 GetPlayerPosition()
    {
        if (Player != null)
            return Player.position;
        return Vector3.zero;
    }

    public static bool IsPlayerValid()
    {
        return Player != null;
    }

    public static float DistanceToPlayer(Vector3 position)
    {
        if (Player == null) return float.MaxValue;
        return Vector3.Distance(position, Player.position);
    }

    public static bool IsPlayerInRange(Vector3 position, float range)
    {
        if (Player == null) return false;
        return Vector3.SqrMagnitude(Player.position - position) <= range * range;
    }

    // Realm helpers
    public static RealmType GetCurrentRealm()
    {
        if (Realm != null)
            return Realm.CurrentRealm;
        return RealmType.TropicalIsland;
    }

    public static bool IsInRealm(RealmType type)
    {
        return GetCurrentRealm() == type;
    }

    // ===== NAMED OBJECT CACHE (replaces GameObject.Find) =====

    public static GameObject FindCached(string name)
    {
        if (namedObjects.TryGetValue(name, out GameObject obj) && obj != null)
            return obj;

        // Cache miss - do the expensive lookup once
        obj = GameObject.Find(name);
        if (obj != null)
            namedObjects[name] = obj;
        return obj;
    }

    public static void RegisterNamedObject(string name, GameObject obj)
    {
        namedObjects[name] = obj;
    }

    public static void ClearNamedObject(string name)
    {
        namedObjects.Remove(name);
    }

    // ===== SHARED MATERIALS CACHE =====

    public static Material GetSharedMaterial(string key, Color color)
    {
        if (sharedMaterials.TryGetValue(key, out Material mat) && mat != null)
            return mat;

        mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        sharedMaterials[key] = mat;
        return mat;
    }

    public static Material GetSharedMaterial(string key, Color color, float emission)
    {
        string fullKey = key + "_e" + emission.ToString("F1");
        if (sharedMaterials.TryGetValue(fullKey, out Material mat) && mat != null)
            return mat;

        mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * emission);
        sharedMaterials[fullKey] = mat;
        return mat;
    }

    // ===== TEXTURE CACHE =====

    public static Texture2D GetColorTexture(Color color)
    {
        // Round color to reduce unique textures
        Color roundedColor = new Color(
            Mathf.Round(color.r * 20) / 20f,
            Mathf.Round(color.g * 20) / 20f,
            Mathf.Round(color.b * 20) / 20f,
            Mathf.Round(color.a * 20) / 20f
        );

        if (colorTextures.TryGetValue(roundedColor, out Texture2D tex) && tex != null)
            return tex;

        tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, roundedColor);
        tex.Apply();
        colorTextures[roundedColor] = tex;
        return tex;
    }

    // ===== GUI STYLE CACHE =====

    public static GUIStyle GetCachedStyle(string key)
    {
        if (guiStyles.TryGetValue(key, out GUIStyle style))
            return style;
        return null;
    }

    public static void CacheStyle(string key, GUIStyle style)
    {
        guiStyles[key] = style;
    }

    public static GUIStyle GetOrCreateStyle(string key, System.Func<GUIStyle> creator)
    {
        if (guiStyles.TryGetValue(key, out GUIStyle style))
            return style;

        style = creator();
        guiStyles[key] = style;
        return style;
    }

    // ===== CLEANUP =====

    void OnDestroy()
    {
        // Clean up created textures
        foreach (var tex in colorTextures.Values)
        {
            if (tex != null)
                Destroy(tex);
        }
        colorTextures.Clear();

        // Clean up created materials
        foreach (var mat in sharedMaterials.Values)
        {
            if (mat != null)
                Destroy(mat);
        }
        sharedMaterials.Clear();

        guiStyles.Clear();
        namedObjects.Clear();

        isInitialized = false;
        Instance = null;
    }
}
