using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Fish Inventory Panel
/// - Shows all caught fish sorted by value (highest first)
/// - Displays fish image, name, count, and value
/// - Toggle with F key
/// </summary>
public class FishInventoryPanel : MonoBehaviour
{
    public static FishInventoryPanel Instance { get; private set; }

    private bool isOpen = false;
    private float scrollPos = 0f;
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;
    private int guiFrameSkip = 0;

    // Draggable window support
    private DraggableWindow window;

    // Sell mode - enabled when near NPC and pressing E
    public bool sellModeEnabled = false;
    public string currentNPCName = "";
    private AudioSource audioSource;

    // Player stats tracking (stored in PlayerPrefs)
    private float biggestFishWeight = 0f;
    private int mostValuableCatch = 0;
    private int totalFishCaught = 0;
    private int totalGoldEarned = 0;

    // Cached GUIStyles for performance (created once, reused every frame)
    private static GUIStyle cachedTitleStyle;
    private static GUIStyle cachedSellBannerStyle;
    private static GUIStyle cachedXButtonStyle;
    private static GUIStyle cachedStatsStyle;
    private static GUIStyle cachedEmptyStyle;
    private static GUIStyle cachedNameStyleCommon;
    private static GUIStyle cachedNameStyleRare;
    private static GUIStyle cachedRarityStyle;
    private static GUIStyle cachedCountStyle;
    private static GUIStyle cachedValueStyle;
    private static GUIStyle cachedTotalStyle;
    private static GUIStyle cachedSellBtnStyle;
    private static GUIStyle cachedCookBtnStyle;
    private static GUIStyle cachedMakeBuffBtnStyle;
    private static GUIStyle cachedHintStyle;
    private static GUIStyle cachedTabStyle;
    private static GUIStyle cachedTabActiveStyle;
    private static bool stylesInitialized = false;

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
        LoadStats();
        // Initialize draggable window (350x450)
        float panelWidth = 350f;
        float panelHeight = 450f;
        Rect initialRect = new Rect(
            (Screen.width - panelWidth) / 2f,
            (Screen.height - panelHeight) / 2f,
            panelWidth,
            panelHeight
        );
        window = new DraggableWindow(initialRect, new Vector2(300, 350), new Vector2(600, 700));
        initialized = true;
    }

    void LoadStats()
    {
        biggestFishWeight = PlayerPrefs.GetFloat("BiggestFishWeight", 0f);
        mostValuableCatch = PlayerPrefs.GetInt("MostValuableCatch", 0);
        totalFishCaught = PlayerPrefs.GetInt("TotalFishCaught", 0);
        totalGoldEarned = PlayerPrefs.GetInt("TotalGoldEarned", 0);
    }

    void SaveStats()
    {
        PlayerPrefs.SetFloat("BiggestFishWeight", biggestFishWeight);
        PlayerPrefs.SetInt("MostValuableCatch", mostValuableCatch);
        PlayerPrefs.SetInt("TotalFishCaught", totalFishCaught);
        PlayerPrefs.SetInt("TotalGoldEarned", totalGoldEarned);
        PlayerPrefs.Save();
    }

    public void UpdateStats(float fishWeight, int fishValue)
    {
        if (fishWeight > biggestFishWeight)
            biggestFishWeight = fishWeight;
        if (fishValue > mostValuableCatch)
            mostValuableCatch = fishValue;
        totalFishCaught++;
        SaveStats();
    }

    public void AddGoldEarned(int gold)
    {
        totalGoldEarned += gold;
        SaveStats();
    }

    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.5f;
        audioSource.playOnAwake = false;
    }

    void PlayCoinSound()
    {
        // Simple coin sound
        int sampleRate = 44100;
        float duration = 0.15f;
        int sampleCount = (int)(sampleRate * duration);
        AudioClip coinClip = AudioClip.Create("CoinSound", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / sampleCount;
            samples[i] = Mathf.Sin(2 * Mathf.PI * 1200f * t) * envelope * 0.3f;
            samples[i] += Mathf.Sin(2 * Mathf.PI * 1800f * t) * envelope * 0.2f;
        }
        coinClip.SetData(samples, 0);
        audioSource.clip = coinClip;
        audioSource.Play();
    }

    // Called by NPCs to enable sell mode
    public void EnableSellMode(string npcName)
    {
        sellModeEnabled = true;
        currentNPCName = npcName;
    }

    public void DisableSellMode()
    {
        sellModeEnabled = false;
        currentNPCName = "";
    }

    // Toggle the panel open/closed (called by UI button click)
    public void TogglePanel()
    {
        isOpen = !isOpen;
        if (isOpen)
        {
            scrollPos = 0f;
        }
    }

    // Open the panel directly
    public void OpenPanel()
    {
        isOpen = true;
        scrollPos = 0f;
    }

    // Close the panel directly
    public void ClosePanel()
    {
        isOpen = false;
    }

    void CreateCachedTextures()
    {
        CacheTexture("panelBg", new Color(0.08f, 0.06f, 0.04f, 0.95f));
        CacheTexture("border", new Color(0.4f, 0.35f, 0.25f, 0.9f));
        CacheTexture("itemBg", new Color(0.12f, 0.1f, 0.08f, 0.95f));
        CacheTexture("itemHover", new Color(0.18f, 0.15f, 0.1f, 0.95f));
        CacheTexture("headerBg", new Color(0.15f, 0.12f, 0.08f, 0.95f));
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

    void Update()
    {
        if (!MainMenu.GameStarted || !initialized) return;

        // Toggle with F key - but NOT when near interactable objects
        if (Input.GetKeyDown(KeyCode.F) && !IsNearInteractable())
        {
            isOpen = !isOpen;
            scrollPos = 0f;
        }

        // Close with ESC
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = false;
        }
    }

    bool IsNearInteractable()
    {
        // Use cached player reference
        if (!GameCache.IsPlayerValid()) return false;

        Vector3 playerPos = GameCache.Player.position;
        float interactRange = 5f;

        // Check all NPCs that can enable sell mode - F key is handled by UIManager for these
        if (GameCache.ClothingShop != null && Vector3.Distance(playerPos, GameCache.ClothingShop.transform.position) < interactRange)
            return true;
        if (GameCache.WetsuitPete != null && Vector3.Distance(playerPos, GameCache.WetsuitPete.transform.position) < interactRange)
            return true;
        if (GameCache.GoldieBanks != null && Vector3.Distance(playerPos, GameCache.GoldieBanks.transform.position) < interactRange)
            return true;
        if (GameCache.TutCat != null && Vector3.Distance(playerPos, GameCache.TutCat.transform.position) < interactRange)
            return true;

        return false;
    }

    void OnGUI()
    {
        // Performance: Skip frames when not actively needed
        if (!isOpen)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return;
        }

        if (!MainMenu.GameStarted || !initialized || !isOpen) return;

        DrawFishInventory();
    }

    void InitializeStyles()
    {
        if (stylesInitialized) return;

        cachedTitleStyle = new GUIStyle();
        cachedTitleStyle.fontSize = 18;
        cachedTitleStyle.fontStyle = FontStyle.Bold;
        cachedTitleStyle.alignment = TextAnchor.MiddleCenter;

        cachedSellBannerStyle = new GUIStyle();
        cachedSellBannerStyle.fontSize = 12;
        cachedSellBannerStyle.fontStyle = FontStyle.Bold;
        cachedSellBannerStyle.alignment = TextAnchor.MiddleCenter;
        cachedSellBannerStyle.normal.textColor = new Color(0.8f, 1f, 0.8f);

        cachedXButtonStyle = new GUIStyle();
        cachedXButtonStyle.fontSize = 16;
        cachedXButtonStyle.fontStyle = FontStyle.Bold;
        cachedXButtonStyle.alignment = TextAnchor.MiddleCenter;
        cachedXButtonStyle.normal.textColor = Color.white;

        cachedStatsStyle = new GUIStyle();
        cachedStatsStyle.fontSize = 11;
        cachedStatsStyle.alignment = TextAnchor.MiddleCenter;
        cachedStatsStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

        cachedEmptyStyle = new GUIStyle();
        cachedEmptyStyle.fontSize = 14;
        cachedEmptyStyle.alignment = TextAnchor.MiddleCenter;
        cachedEmptyStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

        cachedNameStyleCommon = new GUIStyle();
        cachedNameStyleCommon.fontSize = 10;
        cachedNameStyleCommon.fontStyle = FontStyle.Normal;

        cachedNameStyleRare = new GUIStyle();
        cachedNameStyleRare.fontSize = 13;
        cachedNameStyleRare.fontStyle = FontStyle.Bold;

        cachedRarityStyle = new GUIStyle();
        cachedRarityStyle.fontSize = 10;
        cachedRarityStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f);

        cachedCountStyle = new GUIStyle();
        cachedCountStyle.fontSize = 14;
        cachedCountStyle.fontStyle = FontStyle.Bold;
        cachedCountStyle.alignment = TextAnchor.MiddleRight;
        cachedCountStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        cachedValueStyle = new GUIStyle();
        cachedValueStyle.fontSize = 12;
        cachedValueStyle.fontStyle = FontStyle.Bold;
        cachedValueStyle.alignment = TextAnchor.MiddleRight;
        cachedValueStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        cachedTotalStyle = new GUIStyle();
        cachedTotalStyle.fontSize = 10;
        cachedTotalStyle.alignment = TextAnchor.MiddleRight;
        cachedTotalStyle.normal.textColor = new Color(0.6f, 0.6f, 0.5f);

        cachedSellBtnStyle = new GUIStyle();
        cachedSellBtnStyle.fontSize = 9;
        cachedSellBtnStyle.fontStyle = FontStyle.Bold;
        cachedSellBtnStyle.alignment = TextAnchor.MiddleCenter;
        cachedSellBtnStyle.normal.textColor = Color.white;

        cachedCookBtnStyle = new GUIStyle();
        cachedCookBtnStyle.fontSize = 9;
        cachedCookBtnStyle.fontStyle = FontStyle.Bold;
        cachedCookBtnStyle.alignment = TextAnchor.MiddleCenter;
        cachedCookBtnStyle.normal.textColor = Color.white;

        cachedMakeBuffBtnStyle = new GUIStyle();
        cachedMakeBuffBtnStyle.fontSize = 8;
        cachedMakeBuffBtnStyle.fontStyle = FontStyle.Bold;
        cachedMakeBuffBtnStyle.alignment = TextAnchor.MiddleCenter;
        cachedMakeBuffBtnStyle.normal.textColor = Color.white;

        cachedTabStyle = new GUIStyle();
        cachedTabStyle.fontSize = 11;
        cachedTabStyle.fontStyle = FontStyle.Bold;
        cachedTabStyle.alignment = TextAnchor.MiddleCenter;
        cachedTabStyle.normal.textColor = new Color(0.7f, 0.7f, 0.6f);

        cachedTabActiveStyle = new GUIStyle();
        cachedTabActiveStyle.fontSize = 11;
        cachedTabActiveStyle.fontStyle = FontStyle.Bold;
        cachedTabActiveStyle.alignment = TextAnchor.MiddleCenter;
        cachedTabActiveStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

        cachedHintStyle = new GUIStyle();
        cachedHintStyle.fontSize = 10;
        cachedHintStyle.alignment = TextAnchor.MiddleCenter;
        cachedHintStyle.normal.textColor = new Color(0.8f, 0.6f, 0.4f);
        cachedHintStyle.wordWrap = true;

        stylesInitialized = true;
    }

    void DrawFishInventory()
    {
        if (FishingSystem.Instance == null || GameManager.Instance == null || window == null) return;

        // Initialize styles lazily (must be done inside OnGUI context)
        InitializeStyles();

        // Handle dragging and resizing
        window.UpdateWindow();

        // Get window rect
        Rect rect = window.WindowRect;
        float panelX = rect.x;
        float panelY = rect.y;
        float panelWidth = rect.width;
        float panelHeight = rect.height;

        // Border and background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("border"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Header
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 40), GetTexture("headerBg"));

        // Update title color dynamically
        cachedTitleStyle.normal.textColor = sellModeEnabled ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.85f, 0.4f);
        string title = sellModeEnabled ? $"SELL FISH TO {currentNPCName.ToUpper()}" : "FISH INVENTORY";
        GUI.Label(new Rect(panelX, panelY + 5, panelWidth, 30), title, cachedTitleStyle);

        // Show "SELL FISH" banner when sell mode is enabled
        if (sellModeEnabled)
        {
            GUI.DrawTexture(new Rect(panelX + 10, panelY + 38, panelWidth - 20, 22), GetOrCreateColorTexture(new Color(0.15f, 0.5f, 0.25f)));
            GUI.Label(new Rect(panelX + 10, panelY + 38, panelWidth - 20, 22), "Click any fish to SELL for gold!", cachedSellBannerStyle);
        }

        // Red X close button
        Rect closeButtonRect = new Rect(panelX + panelWidth - 28, panelY + 8, 22, 22);
        GUI.DrawTexture(closeButtonRect, GetOrCreateColorTexture(new Color(0.8f, 0.2f, 0.2f)));
        GUI.Label(closeButtonRect, "X", cachedXButtonStyle);
        if (GUI.Button(closeButtonRect, "", GUIStyle.none))
        {
            isOpen = false;
        }

        // Draw fish inventory content
        DrawInventoryTab(panelX, panelY, panelWidth, panelHeight);

        // Draw resize handle
        window.DrawResizeHandle();
    }

    // Check if fish can be made into a buff
    bool IsBuffFish(string fishId)
    {
        string[] buffFishIds = { "red_snapper", "blue_marlin", "rainbow_trout", "sunshore_od", "icelandic_snubnose", "seahorse" };
        foreach (string id in buffFishIds)
        {
            if (fishId == id) return true;
        }
        return false;
    }

    void DrawInventoryTab(float panelX, float panelY, float panelWidth, float panelHeight)
    {
        // Get fish sorted by value
        List<FishDisplayData> fishList = GetSortedFishList();

        // Stats header
        int totalFish = fishList.Sum(f => f.count);
        int totalValue = fishList.Sum(f => f.coinValue * f.count);
        GUI.Label(new Rect(panelX, panelY + 68, panelWidth, 18), $"Total: {totalFish} fish | Worth: {totalValue}g", cachedStatsStyle);

        // Check if near BBQ for cooking
        bool nearBBQ = BBQStation.IsPlayerNearBBQ();

        // Check if near Chef for making buffs
        bool nearChef = ChefNPC.IsPlayerNearChef();
        bool hasCompletedQuest = ChefNPC.HasCompletedFirstQuest();

        // Hint messages when actions are unavailable
        if (!sellModeEnabled && !nearBBQ && fishList.Count > 0)
        {
            GUI.Label(new Rect(panelX + 10, panelY + 86, panelWidth - 20, 30), "Go to an NPC to sell fish or a BBQ to cook!", cachedHintStyle);
        }

        // Fish list area (offset more when sell banner is shown)
        float listY = sellModeEnabled ? panelY + 108 : panelY + 90;
        float listHeight = sellModeEnabled ? panelHeight - 95 : panelHeight - 75;
        float itemHeight = 50;

        Rect listArea = new Rect(panelX + 10, listY, panelWidth - 20, listHeight);

        // Scrolling
        float totalContentHeight = fishList.Count * itemHeight;
        float maxScroll = Mathf.Max(0, totalContentHeight - listHeight);

        if (listArea.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                scrollPos += Event.current.delta.y * 25f;
                scrollPos = Mathf.Clamp(scrollPos, 0, maxScroll);
                Event.current.Use();
            }
        }

        GUI.BeginGroup(listArea);

        if (fishList.Count == 0)
        {
            GUI.Label(new Rect(0, listHeight / 2 - 30, listArea.width, 60), "No fish caught yet!\n\nGo fishing to fill your inventory.", cachedEmptyStyle);
        }
        else
        {
            float itemY = -scrollPos;
            for (int i = 0; i < fishList.Count; i++)
            {
                // Skip items outside visible area
                if (itemY + itemHeight < 0 || itemY > listHeight)
                {
                    itemY += itemHeight;
                    continue;
                }

                FishDisplayData fish = fishList[i];
                Rect itemRect = new Rect(0, itemY, listArea.width, itemHeight - 4);

                // Hover detection
                Rect globalItemRect = new Rect(listArea.x, listY + itemY, listArea.width, itemHeight - 4);
                bool hover = globalItemRect.Contains(Event.current.mousePosition);

                // Item background
                GUI.DrawTexture(itemRect, hover ? GetTexture("itemHover") : GetTexture("itemBg"));

                // Fish pixel art sprite
                float imgSize = 36;
                Texture2D fishTex = null;
                if (FishSprites.Instance != null)
                    fishTex = FishSprites.Instance.GetFishTexture(fish.id);
                if (fishTex == null)
                    fishTex = GetOrCreateColorTexture(fish.color);  // Fallback to color
                GUI.DrawTexture(new Rect(itemRect.x + 8, itemRect.y + (itemHeight - imgSize) / 2 - 2, imgSize, imgSize), fishTex);

                // Fish name - use cached styles, update color dynamically
                GUIStyle nameStyle = fish.rarity == Rarity.Common ? cachedNameStyleCommon : cachedNameStyleRare;
                nameStyle.normal.textColor = GetRarityColor(fish.rarity);
                GUI.Label(new Rect(itemRect.x + 52, itemRect.y + (fish.rarity == Rarity.Common ? 8 : 6), 180, 20), fish.name, nameStyle);

                // Rarity
                GUI.Label(new Rect(itemRect.x + 52, itemRect.y + 24, 100, 16), fish.rarity.ToString(), cachedRarityStyle);

                // Count
                GUI.Label(new Rect(itemRect.x + itemRect.width - 100, itemRect.y + 4, 40, 20), $"x{fish.count}", cachedCountStyle);

                // Value
                GUI.Label(new Rect(itemRect.x + itemRect.width - 60, itemRect.y + 4, 55, 20), $"{fish.coinValue}g", cachedValueStyle);

                // Total value for this stack
                int stackValue = fish.coinValue * fish.count;
                GUI.Label(new Rect(itemRect.x + itemRect.width - 80, itemRect.y + 24, 75, 16), $"({stackValue}g total)", cachedTotalStyle);

                // Special fish glow indicator
                if (fish.isSpecialFish && fish.glowIntensity > 0)
                {
                    // Draw glow border around special fish
                    Color glowCol = new Color(fish.glowColor.r, fish.glowColor.g, fish.glowColor.b, 0.4f);
                    GUI.DrawTexture(new Rect(itemRect.x, itemRect.y, itemRect.width, 2), GetOrCreateColorTexture(glowCol));
                    GUI.DrawTexture(new Rect(itemRect.x, itemRect.y + itemRect.height - 2, itemRect.width, 2), GetOrCreateColorTexture(glowCol));
                    GUI.DrawTexture(new Rect(itemRect.x, itemRect.y, 2, itemRect.height), GetOrCreateColorTexture(glowCol));
                    GUI.DrawTexture(new Rect(itemRect.x + itemRect.width - 2, itemRect.y, 2, itemRect.height), GetOrCreateColorTexture(glowCol));
                }

                // SELL button when in sell mode
                if (sellModeEnabled && fish.coinValue > 0)
                {
                    Rect sellBtnRect = new Rect(itemRect.x + 160, itemRect.y + 14, 32, 18);
                    // Special fish get golden sell button
                    Color btnColor = fish.isSpecialFish ? new Color(0.7f, 0.5f, 0.2f) : new Color(0.2f, 0.6f, 0.3f);
                    GUI.DrawTexture(sellBtnRect, GetOrCreateColorTexture(btnColor));
                    GUI.Label(sellBtnRect, "SELL", cachedSellBtnStyle);

                    if (GUI.Button(sellBtnRect, "", GUIStyle.none))
                    {
                        SellFish(fish.id, fish.coinValue, fish.isSpecialFish);
                    }
                }
                // MAKE BUFF button when near Chef, quest completed, and fish is a buff fish
                else if (nearChef && hasCompletedQuest && !sellModeEnabled && fish.isSpecialFish && IsBuffFish(fish.id))
                {
                    Rect buffBtnRect = new Rect(itemRect.x + 150, itemRect.y + 14, 50, 18);
                    Color btnColor = new Color(0.6f, 0.3f, 0.8f); // Purple color for buff
                    GUI.DrawTexture(buffBtnRect, GetOrCreateColorTexture(btnColor));
                    GUI.Label(buffBtnRect, "MAKE BUFF", cachedMakeBuffBtnStyle);

                    if (GUI.Button(buffBtnRect, "", GUIStyle.none))
                    {
                        MakeBuffFromFish(fish.id);
                    }
                }
                // COOK button when near BBQ and not in sell mode
                else if (nearBBQ && !sellModeEnabled && !fish.isSpecialFish)
                {
                    Rect cookBtnRect = new Rect(itemRect.x + 160, itemRect.y + 14, 36, 18);
                    Color btnColor = new Color(0.8f, 0.4f, 0.1f);
                    GUI.DrawTexture(cookBtnRect, GetOrCreateColorTexture(btnColor));
                    GUI.Label(cookBtnRect, "COOK", cachedCookBtnStyle);

                    if (GUI.Button(cookBtnRect, "", GUIStyle.none))
                    {
                        CookFish(fish.id);
                    }
                }

                itemY += itemHeight;
            }
        }

        GUI.EndGroup();

        // Scroll indicator
        if (maxScroll > 0)
        {
            float scrollBarHeight = listHeight * (listHeight / totalContentHeight);
            float scrollBarY = listY + (scrollPos / maxScroll) * (listHeight - scrollBarHeight);
            GUI.DrawTexture(new Rect(panelX + panelWidth - 8, scrollBarY, 4, scrollBarHeight), GetOrCreateColorTexture(new Color(0.5f, 0.45f, 0.35f)));
        }
    }

    void MakeBuffFromFish(string fishId)
    {
        if (FishingSystem.Instance == null || FishBuffSystem.Instance == null) return;

        // Find the fish in special inventory
        var specialInv = FishingSystem.Instance.specialFishInventory;
        int fishIndex = specialInv.FindIndex(f => f.id == fishId);
        if (fishIndex < 0)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Fish not found!", new Color(1f, 0.4f, 0.2f));
            }
            return;
        }

        // Get the buff associated with this fish
        FishBuff buffData = FishBuffSystem.Instance.GetBuffByFishId(fishId);
        if (buffData == null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Invalid buff fish!", new Color(1f, 0.4f, 0.2f));
            }
            return;
        }

        // Remove fish from special inventory
        FishData fish = specialInv[fishIndex];
        specialInv.RemoveAt(fishIndex);

        // Add buff to inventory
        FishBuffSystem.Instance.AddBuffToInventory(buffData.type, 1);

        // Show notification
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"Created {buffData.buffName}!", buffData.bowlColor);
        }

        Debug.Log($"Made buff {buffData.buffName} from fish {fishId}");
    }

    void CookFish(string fishId)
    {
        if (GameManager.Instance == null || FoodInventory.Instance == null || FishingSystem.Instance == null) return;

        // Check if fish exists in inventory
        if (!GameManager.Instance.fishInventory.ContainsKey(fishId)) return;
        if (GameManager.Instance.fishInventory[fishId] <= 0) return;

        // Get the fish data
        FishData fishData = FishingSystem.Instance.GetFishById(fishId);
        if (fishData == null) return;

        // Check if hotbar has space
        if (FoodInventory.Instance.GetCookedFishCount() >= 4)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification("Hotbar full! Eat some fish first.", new Color(1f, 0.4f, 0.2f));
            }
            return;
        }

        // Remove one fish from inventory
        GameManager.Instance.fishInventory[fishId]--;
        if (GameManager.Instance.fishInventory[fishId] <= 0)
        {
            GameManager.Instance.fishInventory.Remove(fishId);
        }

        // Directly add cooked fish to hotbar (bypasses BBQ.IsOpen check)
        // Find empty hotbar slot
        int emptySlot = -1;
        for (int i = 0; i < FoodInventory.Instance.hotbar.Length; i++)
        {
            if (FoodInventory.Instance.hotbar[i] == null)
            {
                emptySlot = i;
                break;
            }
        }

        if (emptySlot >= 0)
        {
            // Create cooked fish item for hotbar
            InventoryFish cookedFish = new InventoryFish();
            cookedFish.fishId = fishData.id;
            cookedFish.fishName = fishData.fishName;
            cookedFish.color = fishData.fishColor;
            cookedFish.healthValue = Mathf.Max(5, fishData.coinValue / 2); // HP = half coin value, min 5
            cookedFish.isCooked = true;
            FoodInventory.Instance.hotbar[emptySlot] = cookedFish;

            // Show notification
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"Cooked {fishData.fishName}! [Slot {emptySlot + 1}]", new Color(1f, 0.7f, 0.3f));
            }
        }

        // Award XP for cooking
        if (LevelingSystem.Instance != null)
        {
            LevelingSystem.Instance.AwardFishXP(fishData.rarity);
        }

        Debug.Log($"Cooked fish {fishId} - added to hotbar slot {emptySlot + 1}");
    }

    List<FishDisplayData> GetSortedFishList()
    {
        List<FishDisplayData> result = new List<FishDisplayData>();

        if (FishingSystem.Instance == null || GameManager.Instance == null)
            return result;

        var fishDatabase = FishingSystem.Instance.fishDatabase;
        var inventory = GameManager.Instance.fishInventory;

        // Add normal fish from GameManager inventory
        foreach (var kvp in inventory)
        {
            string fishId = kvp.Key;
            int count = kvp.Value;

            if (count <= 0) continue;

            // Find fish data
            FishData fishData = fishDatabase.Find(f => f.id == fishId);
            if (fishData != null)
            {
                result.Add(new FishDisplayData
                {
                    id = fishId,
                    name = fishData.fishName,
                    rarity = fishData.rarity,
                    coinValue = fishData.coinValue,
                    color = fishData.fishColor,
                    count = count,
                    isSpecialFish = false
                });
            }
        }

        // Add special fish from FishingSystem special inventory
        var specialInventory = FishingSystem.Instance.specialFishInventory;
        Dictionary<string, int> specialCounts = new Dictionary<string, int>();
        foreach (var fish in specialInventory)
        {
            if (!specialCounts.ContainsKey(fish.id))
                specialCounts[fish.id] = 0;
            specialCounts[fish.id]++;
        }

        foreach (var kvp in specialCounts)
        {
            string fishId = kvp.Key;
            int count = kvp.Value;

            FishData fishData = fishDatabase.Find(f => f.id == fishId);
            if (fishData != null)
            {
                result.Add(new FishDisplayData
                {
                    id = fishId,
                    name = fishData.fishName,
                    rarity = fishData.rarity,
                    coinValue = fishData.sellToNPC, // Special fish use sellToNPC value
                    color = fishData.fishColor,
                    count = count,
                    isSpecialFish = true,
                    glowColor = fishData.glowColor,
                    glowIntensity = fishData.glowIntensity
                });
            }
        }

        // Sort by coin value descending (highest value first)
        result.Sort((a, b) => b.coinValue.CompareTo(a.coinValue));

        return result;
    }

    Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return new Color(0.75f, 0.75f, 0.75f);
            case Rarity.Uncommon: return new Color(0.3f, 0.9f, 0.3f);
            case Rarity.Rare: return new Color(0.4f, 0.6f, 1f);
            case Rarity.Epic: return new Color(0.8f, 0.4f, 1f);
            case Rarity.Legendary: return new Color(1f, 0.75f, 0.2f);
            case Rarity.Mythic: return new Color(1f, 0.35f, 0.35f);
            default: return Color.white;
        }
    }

    void SellFish(string fishId, int coinValue, bool isSpecial = false)
    {
        if (GameManager.Instance == null) return;

        // Trout's Fortune buff - +50% gold
        if (FishBuffSystem.Instance != null)
        {
            coinValue = (int)(coinValue * FishBuffSystem.Instance.GetGoldMultiplier());
        }

        if (isSpecial)
        {
            // Sell from special fish inventory
            if (FishingSystem.Instance == null) return;
            var specialInv = FishingSystem.Instance.specialFishInventory;
            int idx = specialInv.FindIndex(f => f.id == fishId);
            if (idx < 0) return;

            FishData fish = specialInv[idx];
            specialInv.RemoveAt(idx);

            // Add coins
            GameManager.Instance.AddCoins(coinValue);

            // Track gold earned
            AddGoldEarned(coinValue);

            // Award XP for selling
            if (LevelingSystem.Instance != null)
            {
                LevelingSystem.Instance.AwardFishXP(fish.rarity);
            }

            // Play coin sound
            PlayCoinSound();

            // Show notification with gold AND XP
            int xpAwarded = LevelingSystem.GetFishXP(fish.rarity);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLootNotification($"Sold {fish.fishName}: +{coinValue}g +{xpAwarded}XP!", new Color(1f, 0.7f, 0.9f));
            }

            Debug.Log($"Sold SPECIAL fish {fishId} for {coinValue}g and {xpAwarded}XP");
        }
        else
        {
            // Sell from normal fish inventory
            if (!GameManager.Instance.fishInventory.ContainsKey(fishId)) return;
            if (GameManager.Instance.fishInventory[fishId] <= 0) return;

            // Get fish data for rarity before removing
            FishData fishData = null;
            if (FishingSystem.Instance != null)
            {
                fishData = FishingSystem.Instance.fishDatabase.Find(f => f.id == fishId);
            }

            // Remove one fish from inventory
            GameManager.Instance.fishInventory[fishId]--;
            if (GameManager.Instance.fishInventory[fishId] <= 0)
            {
                GameManager.Instance.fishInventory.Remove(fishId);
            }

            // Add coins
            GameManager.Instance.AddCoins(coinValue);

            // Track gold earned
            AddGoldEarned(coinValue);

            // Award XP for selling
            if (LevelingSystem.Instance != null && fishData != null)
            {
                LevelingSystem.Instance.AwardFishXP(fishData.rarity);
            }

            // Play coin sound
            PlayCoinSound();

            // Show notification with gold AND XP
            int xpAwarded = fishData != null ? LevelingSystem.GetFishXP(fishData.rarity) : 5;
            if (UIManager.Instance != null)
            {
                string fishName = fishData != null ? fishData.fishName : "fish";
                UIManager.Instance.ShowLootNotification($"Sold {fishName}: +{coinValue}g +{xpAwarded}XP!", new Color(1f, 0.85f, 0.3f));
            }

            Debug.Log($"Sold fish {fishId} for {coinValue}g and {xpAwarded}XP");
        }
    }

    void OnDestroy()
    {
        foreach (var tex in textureCache.Values)
        {
            if (tex != null) Destroy(tex);
        }
        textureCache.Clear();
    }

    public bool IsOpen() => isOpen;
}

public class FishDisplayData
{
    public string id;
    public string name;
    public Rarity rarity;
    public int coinValue;
    public Color color;
    public int count;
    public bool isSpecialFish;
    public Color glowColor;
    public float glowIntensity;
}
