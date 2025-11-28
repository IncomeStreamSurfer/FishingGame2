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

    // Health values per fish rarity
    private Dictionary<Rarity, int> fishHealthValues = new Dictionary<Rarity, int>()
    {
        { Rarity.Common, 5 },
        { Rarity.Uncommon, 10 },
        { Rarity.Rare, 20 },
        { Rarity.Epic, 35 },
        { Rarity.Legendary, 50 },
        { Rarity.Mythic, 100 }
    };

    // Cached textures
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private bool initialized = false;

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

        // Show notification
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"Ate {fish.fishName}! +{fish.healthValue} HP", new Color(0.4f, 1f, 0.4f));
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

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !initialized) return;

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
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 10;
        hintStyle.fontStyle = FontStyle.Bold;
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.normal.textColor = new Color(0.7f, 0.5f, 0.3f);
        GUI.Label(new Rect(Screen.width - 150, Screen.height - 30, 140, 20), $"[L] Lunch Box ({lunchBoxFishCount}/{LUNCHBOX_CAPACITY})", hintStyle);
    }

    void DrawLunchBoxUI()
    {
        float panelWidth = 250;
        float panelHeight = 180;
        float panelX = Screen.width - panelWidth - 20;
        float panelY = Screen.height / 2 - panelHeight / 2;

        // Background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("border"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("invBg"));

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.8f, 0.5f, 0.2f);
        GUI.Label(new Rect(panelX, panelY + 10, panelWidth, 24), "LUNCH BOX", titleStyle);

        // Close hint
        GUIStyle closeStyle = new GUIStyle();
        closeStyle.fontSize = 9;
        closeStyle.alignment = TextAnchor.MiddleRight;
        closeStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + 10, panelWidth - 10, 20), "[L] Close", closeStyle);

        // Lunch box icon (brown box)
        Texture2D boxTex = GetOrCreateColorTexture(new Color(0.6f, 0.4f, 0.2f));
        GUI.DrawTexture(new Rect(panelX + panelWidth/2 - 30, panelY + 45, 60, 40), boxTex);

        // Fish count display
        GUIStyle countStyle = new GUIStyle();
        countStyle.fontSize = 20;
        countStyle.fontStyle = FontStyle.Bold;
        countStyle.alignment = TextAnchor.MiddleCenter;
        bool isFull = lunchBoxFishCount >= LUNCHBOX_CAPACITY;
        countStyle.normal.textColor = isFull ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.9f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + 90, panelWidth, 30), $"{lunchBoxFishCount} / {LUNCHBOX_CAPACITY} fish", countStyle);

        // Status text
        GUIStyle statusStyle = new GUIStyle();
        statusStyle.fontSize = 10;
        statusStyle.alignment = TextAnchor.MiddleCenter;
        statusStyle.normal.textColor = isFull ? new Color(0.3f, 1f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
        string statusText = isFull ? "READY TO CONSUME!" : "Add cooked fish from hotbar";
        GUI.Label(new Rect(panelX, panelY + 115, panelWidth, 16), statusText, statusStyle);

        // Buttons
        float btnWidth = 100;
        float btnHeight = 30;
        float btnY = panelY + 140;

        // Add Fish button
        Texture2D addBtnTex = GetOrCreateColorTexture(new Color(0.3f, 0.5f, 0.7f));
        Rect addBtnRect = new Rect(panelX + 20, btnY, btnWidth, btnHeight);
        GUI.DrawTexture(addBtnRect, addBtnTex);

        GUIStyle btnStyle = new GUIStyle();
        btnStyle.fontSize = 11;
        btnStyle.fontStyle = FontStyle.Bold;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.normal.textColor = Color.white;
        GUI.Label(addBtnRect, "Add Fish", btnStyle);

        if (GUI.Button(addBtnRect, "", GUIStyle.none))
        {
            AddFishToLunchBox();
        }

        // Consume button (only enabled when full)
        Texture2D consumeBtnTex = GetOrCreateColorTexture(isFull ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.3f, 0.3f, 0.3f));
        Rect consumeBtnRect = new Rect(panelX + panelWidth - btnWidth - 20, btnY, btnWidth, btnHeight);
        GUI.DrawTexture(consumeBtnRect, consumeBtnTex);

        GUIStyle consumeStyle = new GUIStyle();
        consumeStyle.fontSize = 11;
        consumeStyle.fontStyle = FontStyle.Bold;
        consumeStyle.alignment = TextAnchor.MiddleCenter;
        consumeStyle.normal.textColor = isFull ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(consumeBtnRect, "Consume", consumeStyle);

        if (isFull && GUI.Button(consumeBtnRect, "", GUIStyle.none))
        {
            ConsumeLunchBox();
        }

        // Boxes remaining count
        if (lunchBoxCount > 1)
        {
            GUIStyle boxCountStyle = new GUIStyle();
            boxCountStyle.fontSize = 9;
            boxCountStyle.alignment = TextAnchor.MiddleCenter;
            boxCountStyle.normal.textColor = new Color(0.6f, 0.5f, 0.4f);
            GUI.Label(new Rect(panelX, panelY + panelHeight - 15, panelWidth, 14), $"({lunchBoxCount} lunch boxes owned)", boxCountStyle);
        }
    }

    void DrawHotbar()
    {
        float slotSize = 50;
        float spacing = 5;
        float totalWidth = (slotSize * 4) + (spacing * 3);
        float startX = (Screen.width - totalWidth) / 2 - 170; // Left of the main action bar
        float startY = Screen.height - slotSize - 15;

        // Label
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 10;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = new Color(0.8f, 0.7f, 0.5f);
        GUI.Label(new Rect(startX, startY - 16, totalWidth, 14), "FOOD [1-4]", labelStyle);

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
                // Cooked fish icon
                GUI.DrawTexture(new Rect(x + 8, startY + 8, slotSize - 16, slotSize - 16), GetTexture("cookedFish"));

                // Health value
                GUIStyle hpStyle = new GUIStyle();
                hpStyle.fontSize = 10;
                hpStyle.fontStyle = FontStyle.Bold;
                hpStyle.alignment = TextAnchor.LowerRight;
                hpStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);
                GUI.Label(new Rect(x, startY, slotSize - 3, slotSize - 3), $"+{hotbar[i].healthValue}", hpStyle);
            }

            // Slot number
            GUIStyle numStyle = new GUIStyle();
            numStyle.fontSize = 9;
            numStyle.fontStyle = FontStyle.Bold;
            numStyle.alignment = TextAnchor.UpperLeft;
            numStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(x + 3, startY + 2, 15, 12), $"{i + 1}", numStyle);

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

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + 8, panelWidth, 24), "RAW FISH - Click to Cook", titleStyle);

        // Instruction
        GUIStyle instrStyle = new GUIStyle();
        instrStyle.fontSize = 10;
        instrStyle.alignment = TextAnchor.MiddleCenter;
        instrStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        GUI.Label(new Rect(panelX, panelY + 30, panelWidth, 16), "Click a fish to cook it on the BBQ", instrStyle);

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
            GUIStyle emptyStyle = new GUIStyle();
            emptyStyle.fontSize = 12;
            emptyStyle.alignment = TextAnchor.MiddleCenter;
            emptyStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(new Rect(0, listHeight / 2 - 20, scrollArea.width, 40), "No raw fish.\nGo fishing to catch some!", emptyStyle);
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

                // Fish color swatch
                Texture2D swatchTex = GetOrCreateColorTexture(fish.color);
                GUI.DrawTexture(new Rect(itemRect.x + 5, itemRect.y + 5, 30, 30), swatchTex);

                // Fish name
                GUIStyle nameStyle = new GUIStyle();
                nameStyle.fontSize = 11;
                nameStyle.fontStyle = FontStyle.Bold;
                nameStyle.normal.textColor = GetRarityColor(fish.rarity);
                GUI.Label(new Rect(itemRect.x + 42, itemRect.y + 5, 150, 16), fish.fishName, nameStyle);

                // Rarity and HP value
                GUIStyle infoStyle = new GUIStyle();
                infoStyle.fontSize = 9;
                infoStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
                GUI.Label(new Rect(itemRect.x + 42, itemRect.y + 22, 100, 14), fish.rarity.ToString(), infoStyle);

                // HP value when cooked
                GUIStyle hpStyle = new GUIStyle();
                hpStyle.fontSize = 10;
                hpStyle.fontStyle = FontStyle.Bold;
                hpStyle.alignment = TextAnchor.MiddleRight;
                hpStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 60, itemRect.y + 5, 55, 30), $"+{fish.healthValue} HP", hpStyle);

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
