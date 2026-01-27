using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Food Inventory System
/// - Stores caught fish
/// - Hotbar with 4 slots for cooked fish
/// - Cooking and consumption for HP
/// </summary>
public class FoodInventory : MonoBehaviour
{
    public static FoodInventory Instance { get; private set; }

    // Raw fish inventory
    public List<InventoryFish> rawFish = new List<InventoryFish>();

    // Cooked fish hotbar (4 slots)
    public InventoryFish[] hotbar = new InventoryFish[4];

    // Lunch Box - holds up to 10 fish, gives 10 min max health when consumed
    public int lunchBoxCount = 0;
    public int lunchBoxFishCount = 0;
    public const int LUNCHBOX_CAPACITY = 10;

    // UI State
    private bool inventoryOpen = false;
    private bool lunchBoxOpen = false;
    private float inventoryScrollPos = 0f;

    // Draggable/Resizable Lunch Box window
    private DraggableWindow lunchBoxWindow;

    // Health values per fish rarity
    private Dictionary<Rarity, int> fishHealthValues = new Dictionary<Rarity, int>()
    {
        { Rarity.Common, 1 },
        { Rarity.Uncommon, 10 },
        { Rarity.Rare, 20 },
        { Rarity.Epic, 35 },
        { Rarity.Legendary, 50 },
        { Rarity.Mythic, 100 }
    };

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;

    // Cached GUIStyles for performance (avoid allocating every frame)
    private static GUIStyle cachedHintStyle;
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedXButtonStyle;
    private static GUIStyle cachedCountStyle;
    private static GUIStyle cachedStatusStyle;
    private static GUIStyle cachedBtnStyle;
    private static GUIStyle cachedConsumeStyle;
    private static GUIStyle cachedBoxCountStyle;
    private static GUIStyle cachedLabelStyle;
    private static GUIStyle cachedHpStyle;
    private static GUIStyle cachedNumStyle;
    private static GUIStyle cachedInstrStyle;
    private static GUIStyle cachedEmptyStyle;
    private static GUIStyle cachedNameStyle;
    private static GUIStyle cachedInfoStyle;
    private static bool stylesInitialized = false;

    // Audio
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        CreateCachedTextures();
        SetupAudio();

        // Initialize draggable lunch box window (right side of screen, medium size)
        float lunchBoxWidth = 300;
        float lunchBoxHeight = 220;
        float lunchBoxX = Screen.width - lunchBoxWidth - 20;
        float lunchBoxY = Screen.height / 2 - lunchBoxHeight / 2;
        lunchBoxWindow = new DraggableWindow(
            new Rect(lunchBoxX, lunchBoxY, lunchBoxWidth, lunchBoxHeight),
            new Vector2(250, 180),  // Min size
            new Vector2(450, 350)   // Max size
        );

        initialized = true;
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;  // 2D sound
        audioSource.volume = 0.4f;
        audioSource.playOnAwake = false;
    }

    void PlayMunchSound()
    {
        StartCoroutine(GenerateMunchSound());
    }

    System.Collections.IEnumerator GenerateMunchSound()
    {
        // Crunchy munching sound
        int sampleRate = 44100;
        float duration = 0.5f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip munchClip = AudioClip.Create("MunchSound", sampleCount, 1, sampleRate, false);

        float[] samples = new float[sampleCount];
        System.Random rand = new System.Random();

        // Multiple munch sounds (3 bites)
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / sampleCount;

            // Determine which bite we're in (3 bites)
            int bite = (int)(progress * 3);
            float biteProgress = (progress * 3) % 1f;

            // Each bite has a crunch sound
            float crunch = 0f;
            if (biteProgress < 0.3f)
            {
                // Initial crunch
                float crunchProgress = biteProgress / 0.3f;
                float crunchEnvelope = Mathf.Sin(crunchProgress * Mathf.PI);

                // Crunchy noise
                crunch = (float)(rand.NextDouble() * 2 - 1) * crunchEnvelope * 0.4f;

                // Low frequency thud
                crunch += Mathf.Sin(2 * Mathf.PI * 150f * t) * crunchEnvelope * 0.2f;

                // Mid-range crackle
                if (rand.NextDouble() < 0.1f)
                {
                    crunch += (float)(rand.NextDouble() - 0.5f) * 0.3f;
                }
            }
            else if (biteProgress < 0.5f)
            {
                // Chewing sound - quieter
                float chewProgress = (biteProgress - 0.3f) / 0.2f;
                float chewEnvelope = Mathf.Sin(chewProgress * Mathf.PI) * 0.3f;
                crunch = (float)(rand.NextDouble() * 2 - 1) * chewEnvelope * 0.15f;
            }

            samples[i] = crunch * 0.6f;
        }

        munchClip.SetData(samples, 0);
        audioSource.clip = munchClip;
        audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        audioSource.Play();

        yield return null;
    }

    void CreateCachedTextures()
    {
        CacheTexture("slotBg", new Color(0.12f, 0.1f, 0.08f, 0.95f));
        CacheTexture("slotEmpty", new Color(0.08f, 0.08f, 0.08f, 0.8f));
        CacheTexture("slotHover", new Color(0.2f, 0.18f, 0.12f, 0.95f));
        CacheTexture("slotSelected", new Color(0.3f, 0.25f, 0.15f, 0.95f));
        CacheTexture("border", new Color(0.4f, 0.35f, 0.25f, 0.9f));
        CacheTexture("rawFish", new Color(0.5f, 0.6f, 0.7f, 1f));
        CacheTexture("cookedFish", new Color(0.8f, 0.5f, 0.3f, 1f));
        CacheTexture("invBg", new Color(0.1f, 0.08f, 0.06f, 0.95f));
        CacheTexture("cookBtn", new Color(0.7f, 0.3f, 0.1f, 1f));
        CacheTexture("white", Color.white);
    }

    void CacheTexture(string name, Color color)
    {
        if (!textureCache.ContainsKey(name))
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            textureCache[name] = tex;
        }
    }

    Texture2D GetTexture(string name)
    {
        return textureCache.TryGetValue(name, out Texture2D tex) ? tex : Texture2D.whiteTexture;
    }

    void Update()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        // Toggle lunch box with L key
        if (Input.GetKeyDown(KeyCode.L) && lunchBoxCount > 0)
        {
            lunchBoxOpen = !lunchBoxOpen;
        }

        // Close lunch box with ESC
        if (lunchBoxOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            lunchBoxOpen = false;
        }

        // Consume hotbar items with number keys 1-4
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ConsumeFromHotbar(i);
            }
        }
    }

    public void AddLunchBox()
    {
        lunchBoxCount++;
        Debug.Log($"Added Lunch Box! Total: {lunchBoxCount}");
    }

    public void AddFishToLunchBox()
    {
        if (lunchBoxCount <= 0) return;
        if (lunchBoxFishCount >= LUNCHBOX_CAPACITY)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Lunch Box is full! (10/10)", new Color(1f, 0.5f, 0.2f));
            }
            return;
        }

        // Add a cooked fish from hotbar if available
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] != null)
            {
                lunchBoxFishCount++;
                hotbar[i] = null;
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowLootNotification($"Added fish to Lunch Box ({lunchBoxFishCount}/{LUNCHBOX_CAPACITY})", new Color(0.7f, 0.5f, 0.3f));
                }
                return;
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification("No cooked fish to add! Cook some first.", new Color(1f, 0.5f, 0.2f));
        }
    }

    public void ConsumeLunchBox()
    {
        if (lunchBoxCount <= 0) return;
        if (lunchBoxFishCount < LUNCHBOX_CAPACITY)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"Lunch Box not full! ({lunchBoxFishCount}/{LUNCHBOX_CAPACITY})", new Color(1f, 0.5f, 0.2f));
            }
            return;
        }

        // Consume the lunch box
        lunchBoxCount--;
        lunchBoxFishCount = 0;
        lunchBoxOpen = false;

        // Apply 10 minute max health buff
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.ApplyMaxHealthBuff(600f); // 10 minutes
            PlayMunchSound();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Lunch Box consumed! MAX HEALTH FOR 10 MINS!", new Color(0.3f, 1f, 0.5f));
            }
        }
    }

    public void AddRawFish(FishData fish)
    {
        InventoryFish invFish = new InventoryFish();
        invFish.fishName = fish.fishName;
        invFish.fishId = fish.id;
        invFish.rarity = fish.rarity;
        invFish.color = fish.fishColor;
        invFish.healthValue = fishHealthValues.ContainsKey(fish.rarity) ? fishHealthValues[fish.rarity] : 5;
        invFish.isCooked = false;

        rawFish.Add(invFish);
        Debug.Log($"Added {fish.fishName} to food inventory (heals {invFish.healthValue} HP when cooked)");
    }

    public bool CookFish(int rawFishIndex)
    {
        if (rawFishIndex < 0 || rawFishIndex >= rawFish.Count) return false;

        // Check if BBQ is open
        if (BBQStation.Instance == null || !BBQStation.Instance.IsOpen())
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Need to use a BBQ to cook!", new Color(1f, 0.5f, 0.2f));
            }
            return false;
        }

        // Find empty hotbar slot
        int emptySlot = -1;
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] == null)
            {
                emptySlot = i;
                break;
            }
        }

        if (emptySlot == -1)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Hotbar full! Eat some food first.", new Color(1f, 0.5f, 0.2f));
            }
            return false;
        }

        // Cook the fish
        InventoryFish fish = rawFish[rawFishIndex];
        fish.isCooked = true;
        hotbar[emptySlot] = fish;
        rawFish.RemoveAt(rawFishIndex);

        // Award XP for cooking
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.AwardFishXP(fish.rarity);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"Cooked {fish.fishName}! (+{fish.healthValue} HP)", new Color(1f, 0.7f, 0.3f));
        }

        Debug.Log($"Cooked {fish.fishName} - moved to hotbar slot {emptySlot + 1}");
        return true;
    }

    public void ConsumeFromHotbar(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbar.Length) return;
        if (hotbar[slotIndex] == null) return;

        InventoryFish fish = hotbar[slotIndex];

        // Play munching sound
        PlayMunchSound();

        // Heal player
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.Heal(fish.healthValue);
        }

        // 5% chance to get poisoned when eating fish
        float poisonChance = UnityEngine.Random.Range(0f, 1f);
        if (poisonChance < 0.05f)
        {
            // Apply poison debuff (10 seconds, 1 damage per second)
            if (FishBuffSystem.Instance != null)
            {
                FishBuffSystem.Instance.ApplyPoison();
            }
        }
        else
        {
            // Show normal notification only if not poisoned
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"Ate {fish.fishName}! +{fish.healthValue} HP", new Color(0.4f, 1f, 0.4f));
            }
        }

        Debug.Log($"Consumed {fish.fishName} for +{fish.healthValue} HP");

        // Remove from hotbar
        hotbar[slotIndex] = null;
    }

    public void ClearInventory()
    {
        rawFish.Clear();
        for (int i = 0; i < hotbar.Length; i++)
        {
            hotbar[i] = null;
        }
    }

    public int GetRawFishCount() => rawFish.Count;
    public int GetCookedFishCount()
    {
        int count = 0;
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i] != null) count++;
        }
        return count;
    }

    void InitializeCachedStyles()
    {
        cachedHintStyle = new GUIStyle();
        cachedHintStyle.fontSize = 10;
        cachedHintStyle.fontStyle = FontStyle.Bold;
        cachedHintStyle.alignment = TextAnchor.MiddleCenter;
        cachedHintStyle.normal.textColor = new Color(0.7f, 0.5f, 0.3f);

        cachedTitleStyle = new GUIStyle();
        cachedTitleStyle.fontStyle = FontStyle.Bold;
        cachedTitleStyle.alignment = TextAnchor.MiddleCenter;
        cachedTitleStyle.normal.textColor = new Color(0.8f, 0.5f, 0.2f);

        cachedXButtonStyle = new GUIStyle();
        cachedXButtonStyle.fontSize = 12;
        cachedXButtonStyle.fontStyle = FontStyle.Bold;
        cachedXButtonStyle.alignment = TextAnchor.MiddleCenter;
        cachedXButtonStyle.normal.textColor = Color.white;

        cachedCountStyle = new GUIStyle();
        cachedCountStyle.fontStyle = FontStyle.Bold;
        cachedCountStyle.alignment = TextAnchor.MiddleCenter;
        cachedCountStyle.normal.textColor = Color.white;

        cachedStatusStyle = new GUIStyle();
        cachedStatusStyle.alignment = TextAnchor.MiddleCenter;
        cachedStatusStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

        cachedBtnStyle = new GUIStyle();
        cachedBtnStyle.fontStyle = FontStyle.Bold;
        cachedBtnStyle.alignment = TextAnchor.MiddleCenter;
        cachedBtnStyle.normal.textColor = Color.white;

        cachedConsumeStyle = new GUIStyle();
        cachedConsumeStyle.fontStyle = FontStyle.Bold;
        cachedConsumeStyle.alignment = TextAnchor.MiddleCenter;

        cachedBoxCountStyle = new GUIStyle();
        cachedBoxCountStyle.fontSize = 9;
        cachedBoxCountStyle.alignment = TextAnchor.MiddleCenter;
        cachedBoxCountStyle.normal.textColor = new Color(0.8f, 0.6f, 0.3f);

        cachedLabelStyle = new GUIStyle();
        cachedLabelStyle.fontSize = 10;
        cachedLabelStyle.fontStyle = FontStyle.Bold;
        cachedLabelStyle.alignment = TextAnchor.MiddleCenter;
        cachedLabelStyle.normal.textColor = new Color(0.9f, 0.85f, 0.75f);

        cachedHpStyle = new GUIStyle();
        cachedHpStyle.fontSize = 9;
        cachedHpStyle.alignment = TextAnchor.MiddleCenter;
        cachedHpStyle.normal.textColor = new Color(0.5f, 0.9f, 0.5f);

        cachedNumStyle = new GUIStyle();
        cachedNumStyle.fontSize = 10;
        cachedNumStyle.fontStyle = FontStyle.Bold;
        cachedNumStyle.alignment = TextAnchor.LowerRight;
        cachedNumStyle.normal.textColor = Color.white;

        cachedInstrStyle = new GUIStyle();
        cachedInstrStyle.fontSize = 10;
        cachedInstrStyle.alignment = TextAnchor.UpperCenter;
        cachedInstrStyle.normal.textColor = new Color(0.7f, 0.65f, 0.55f);

        cachedEmptyStyle = new GUIStyle();
        cachedEmptyStyle.fontSize = 12;
        cachedEmptyStyle.alignment = TextAnchor.MiddleCenter;
        cachedEmptyStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

        cachedNameStyle = new GUIStyle();
        cachedNameStyle.fontSize = 11;
        cachedNameStyle.fontStyle = FontStyle.Bold;

        cachedInfoStyle = new GUIStyle();
        cachedInfoStyle.fontSize = 9;
        cachedInfoStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

        stylesInitialized = true;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        // Initialize cached styles once (must be inside OnGUI)
        if (!stylesInitialized)
        {
            InitializeCachedStyles();
        }

        // Always draw hotbar at bottom of screen
        DrawHotbar();

        // Draw full inventory when BBQ is open
        if (BBQStation.Instance != null && BBQStation.Instance.IsOpen())
        {
            DrawFishInventory();
        }

        // Draw lunch box UI when open
        if (lunchBoxOpen && lunchBoxCount > 0)
        {
            DrawLunchBoxUI();
        }

        // Show lunch box hint if player has one
        if (lunchBoxCount > 0 && !lunchBoxOpen)
        {
            DrawLunchBoxHint();
        }
    }

    void DrawLunchBoxHint()
    {
        GUI.Label(new Rect(Screen.width - 150, Screen.height - 30, 140, 20), $"[L] Lunch Box ({lunchBoxFishCount}/{LUNCHBOX_CAPACITY})", cachedHintStyle);
    }

    void DrawLunchBoxUI()
    {
        // Update window (handles dragging and resizing)
        lunchBoxWindow.UpdateWindow();

        Rect rect = lunchBoxWindow.WindowRect;
        float panelWidth = rect.width;
        float panelHeight = rect.height;
        float panelX = rect.x;
        float panelY = rect.y;

        // Background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("border"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("invBg"));

        // Title bar (draggable area)
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 25), GetOrCreateColorTexture(new Color(0.15f, 0.1f, 0.08f, 1f)));

        // Title (use cached style, just update fontSize)
        cachedTitleStyle.fontSize = Mathf.Max(12, (int)(panelWidth * 0.045f));
        GUI.Label(new Rect(panelX, panelY + 3, panelWidth, 20), "LUNCH BOX", cachedTitleStyle);

        // Red X close button (use cached style)
        GUI.DrawTexture(new Rect(panelX + panelWidth - 24, panelY + 4, 18, 18), GetOrCreateColorTexture(new Color(0.8f, 0.2f, 0.2f)));
        if (GUI.Button(new Rect(panelX + panelWidth - 24, panelY + 4, 18, 18), "X", cachedXButtonStyle))
        {
            lunchBoxOpen = false;
        }

        // Content area starts below title bar
        float contentY = panelY + 30;
        float contentHeight = panelHeight - 30;

        // Lunch box icon (brown box) - scaled based on window size
        float boxWidth = Mathf.Min(60, panelWidth * 0.25f);
        float boxHeight = boxWidth * 0.67f;
        Texture2D boxTex = GetOrCreateColorTexture(new Color(0.6f, 0.4f, 0.2f));
        GUI.DrawTexture(new Rect(panelX + panelWidth/2 - boxWidth/2, contentY + 10, boxWidth, boxHeight), boxTex);

        // Fish count display (use cached style, update dynamic properties)
        bool isFull = lunchBoxFishCount >= LUNCHBOX_CAPACITY;
        cachedCountStyle.fontSize = Mathf.Max(14, (int)(panelWidth * 0.06f));
        cachedCountStyle.normal.textColor = isFull ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.9f, 0.5f);
        GUI.Label(new Rect(panelX, contentY + boxHeight + 20, panelWidth, 30), $"{lunchBoxFishCount} / {LUNCHBOX_CAPACITY} fish", cachedCountStyle);

        // Status text (use cached style)
        cachedStatusStyle.fontSize = Mathf.Max(9, (int)(panelWidth * 0.035f));
        cachedStatusStyle.normal.textColor = isFull ? new Color(0.3f, 1f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
        cachedStatusStyle.wordWrap = true;
        string statusText = isFull ? "READY TO CONSUME!" : "Add cooked fish from hotbar";
        GUI.Label(new Rect(panelX + 10, contentY + boxHeight + 50, panelWidth - 20, 30), statusText, cachedStatusStyle);

        // Buttons - scaled and positioned relative to window size
        float btnWidth = Mathf.Max(80, panelWidth * 0.35f);
        float btnHeight = Mathf.Max(25, panelHeight * 0.12f);
        float btnY = panelY + panelHeight - btnHeight - 35;

        // Add Fish button (use cached style)
        Texture2D addBtnTex = GetOrCreateColorTexture(new Color(0.3f, 0.5f, 0.7f));
        Rect addBtnRect = new Rect(panelX + 15, btnY, btnWidth, btnHeight);
        GUI.DrawTexture(addBtnRect, addBtnTex);

        cachedBtnStyle.fontSize = Mathf.Max(10, (int)(panelWidth * 0.038f));
        GUI.Label(addBtnRect, "Add Fish", cachedBtnStyle);

        if (GUI.Button(addBtnRect, "", GUIStyle.none))
        {
            AddFishToLunchBox();
        }

        // Consume button (use cached style)
        Texture2D consumeBtnTex = GetOrCreateColorTexture(isFull ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.3f, 0.3f, 0.3f));
        Rect consumeBtnRect = new Rect(panelX + panelWidth - btnWidth - 15, btnY, btnWidth, btnHeight);
        GUI.DrawTexture(consumeBtnRect, consumeBtnTex);

        cachedConsumeStyle.fontSize = Mathf.Max(10, (int)(panelWidth * 0.038f));
        cachedConsumeStyle.normal.textColor = isFull ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(consumeBtnRect, "Consume", cachedConsumeStyle);

        if (isFull && GUI.Button(consumeBtnRect, "", GUIStyle.none))
        {
            ConsumeLunchBox();
        }

        // Boxes remaining count (use cached style)
        if (lunchBoxCount > 1)
        {
            cachedBoxCountStyle.fontSize = Mathf.Max(8, (int)(panelWidth * 0.032f));
            GUI.Label(new Rect(panelX, panelY + panelHeight - 18, panelWidth, 14), $"({lunchBoxCount} lunch boxes owned)", cachedBoxCountStyle);
        }

        // Draw resize handle
        lunchBoxWindow.DrawResizeHandle();
    }

    void DrawHotbar()
    {
        float slotSize = 50;
        float spacing = 5;
        float totalWidth = (slotSize * 4) + (spacing * 3);  // 4 slots = 215px wide
        // Position at bottom left, with some margin from edge
        float startX = 15;  // 15px from left edge - ensures all 4 slots visible
        float startY = Screen.height - slotSize - 15;

        // Label (use cached style)
        cachedLabelStyle.normal.textColor = new Color(0.8f, 0.7f, 0.5f);
        GUI.Label(new Rect(startX, startY - 16, totalWidth, 14), "FOOD [1-4]", cachedLabelStyle);

        for (int i = 0; i < 4; i++)
        {
            float x = startX + i * (slotSize + spacing);
            Rect slotRect = new Rect(x, startY, slotSize, slotSize);

            // Border
            GUI.DrawTexture(new Rect(x - 2, startY - 2, slotSize + 4, slotSize + 4), GetTexture("border"));

            // Slot background
            bool hasItem = hotbar[i] != null;
            GUI.DrawTexture(slotRect, hasItem ? GetTexture("slotBg") : GetTexture("slotEmpty"));

            if (hasItem)
            {
                // Fish pixel art sprite
                float imgSize = slotSize - 16;
                Texture2D fishTex = null;
                if (FishSprites.Instance != null)
                    fishTex = FishSprites.Instance.GetFishTexture(hotbar[i].fishId);
                if (fishTex == null)
                    fishTex = GetOrCreateColorTexture(hotbar[i].color);  // Fallback to color
                GUI.DrawTexture(new Rect(x + 8, startY + 8, imgSize, imgSize), fishTex);

                // Health value (use cached style)
                cachedHpStyle.fontSize = 10;
                cachedHpStyle.alignment = TextAnchor.LowerRight;
                cachedHpStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);
                GUI.Label(new Rect(x, startY, slotSize - 3, slotSize - 3), $"+{hotbar[i].healthValue}", cachedHpStyle);
            }

            // Slot number (use cached style)
            cachedNumStyle.fontSize = 9;
            cachedNumStyle.alignment = TextAnchor.UpperLeft;
            cachedNumStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(x + 3, startY + 2, 15, 12), $"{i + 1}", cachedNumStyle);

            // Click to consume
            if (hasItem && GUI.Button(slotRect, "", GUIStyle.none))
            {
                ConsumeFromHotbar(i);
            }
        }
    }

    void DrawFishInventory()
    {
        float panelWidth = 300;
        float panelHeight = 350;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("border"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("invBg"));

        // Title (use cached style)
        cachedTitleStyle.fontSize = 16;
        cachedTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + 8, panelWidth, 24), "RAW FISH - Click to Cook", cachedTitleStyle);

        // Instruction (use cached style)
        cachedInstrStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(panelX, panelY + 30, panelWidth, 16), "Click a fish to cook it on the BBQ", cachedInstrStyle);

        // Fish list
        float listY = panelY + 55;
        float listHeight = panelHeight - 65;
        float itemHeight = 45;

        // Scrollable area
        float totalHeight = rawFish.Count * itemHeight;
        float maxScroll = Mathf.Max(0, totalHeight - listHeight);

        Rect scrollArea = new Rect(panelX + 10, listY, panelWidth - 20, listHeight);
        if (scrollArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                inventoryScrollPos += Event.current.delta.y * 20f;
                inventoryScrollPos = Mathf.Clamp(inventoryScrollPos, 0, maxScroll);
                Event.current.Use();
            }
        }

        GUI.BeginGroup(scrollArea);

        if (rawFish.Count == 0)
        {
            // Use cached style
            GUI.Label(new Rect(0, listHeight / 2 - 20, scrollArea.width, 40), "No raw fish.\nGo fishing to catch some!", cachedEmptyStyle);
        }
        else
        {
            float itemY = -inventoryScrollPos;
            for (int i = 0; i < rawFish.Count; i++)
            {
                if (itemY + itemHeight < 0 || itemY > listHeight)
                {
                    itemY += itemHeight;
                    continue;
                }

                InventoryFish fish = rawFish[i];
                Rect itemRect = new Rect(0, itemY, scrollArea.width - 10, itemHeight - 5);

                // Item background
                bool hover = new Rect(scrollArea.x, listY + itemY, scrollArea.width - 10, itemHeight - 5).Contains(Event.current.mousePosition);
                GUI.DrawTexture(itemRect, hover ? GetTexture("slotHover") : GetTexture("slotBg"));

                // Fish pixel art sprite
                float imgSize = 30;
                Texture2D fishTex = null;
                if (FishSprites.Instance != null)
                    fishTex = FishSprites.Instance.GetFishTexture(fish.fishId);
                if (fishTex == null)
                    fishTex = GetOrCreateColorTexture(fish.color);  // Fallback to color
                GUI.DrawTexture(new Rect(itemRect.x + 5, itemRect.y + 5, imgSize, imgSize), fishTex);

                // Fish name (use cached style, update color)
                cachedNameStyle.normal.textColor = GetRarityColor(fish.rarity);
                GUI.Label(new Rect(itemRect.x + 42, itemRect.y + 5, 150, 16), fish.fishName, cachedNameStyle);

                // Rarity and HP value (use cached style)
                GUI.Label(new Rect(itemRect.x + 42, itemRect.y + 22, 100, 14), fish.rarity.ToString(), cachedInfoStyle);

                // HP value when cooked (use cached style)
                cachedHpStyle.fontSize = 10;
                cachedHpStyle.alignment = TextAnchor.MiddleRight;
                cachedHpStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 60, itemRect.y + 5, 55, 30), $"+{fish.healthValue} HP", cachedHpStyle);

                // Click to cook
                if (GUI.Button(itemRect, "", GUIStyle.none))
                {
                    CookFish(i);
                }

                itemY += itemHeight;
            }
        }

        GUI.EndGroup();
    }

    Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return new Color(0.7f, 0.7f, 0.7f);
            case Rarity.Uncommon: return new Color(0.3f, 0.9f, 0.3f);
            case Rarity.Rare: return new Color(0.3f, 0.5f, 1f);
            case Rarity.Epic: return new Color(0.7f, 0.3f, 0.9f);
            case Rarity.Legendary: return new Color(1f, 0.7f, 0.2f);
            case Rarity.Mythic: return new Color(1f, 0.3f, 0.3f);
            default: return Color.white;
        }
    }

    Texture2D GetOrCreateColorTexture(Color color)
    {
        string key = $"color_{color.r:F2}_{color.g:F2}_{color.b:F2}";
        if (!textureCache.ContainsKey(key))
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pixels = new Color[4];
            for (int i = 0; i < 4; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            textureCache[key] = tex;
        }
        return textureCache[key];
    }

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }
}

[System.Serializable]
public class InventoryFish
{
    public string fishName;
    public string fishId;
    public Rarity rarity;
    public Color color;
    public int healthValue;
    public bool isCooked;
}
