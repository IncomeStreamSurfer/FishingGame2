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
        initialized = true;
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

        // Toggle with F key
        if (Input.GetKeyDown(KeyCode.F))
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

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !initialized || !isOpen) return;

        DrawFishInventory();
    }

    void DrawFishInventory()
    {
        if (FishingSystem.Instance == null || GameManager.Instance == null) return;

        float panelWidth = 350;
        float panelHeight = 450;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Border and background
        GUI.DrawTexture(new Rect(panelX - 3, panelY - 3, panelWidth + 6, panelHeight + 6), GetTexture("border"));
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), GetTexture("panelBg"));

        // Header
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 40), GetTexture("headerBg"));

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 18;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);
        GUI.Label(new Rect(panelX, panelY + 5, panelWidth, 30), "FISH INVENTORY", titleStyle);

        // Close hint
        GUIStyle closeStyle = new GUIStyle();
        closeStyle.fontSize = 10;
        closeStyle.alignment = TextAnchor.MiddleRight;
        closeStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
        GUI.Label(new Rect(panelX, panelY + 8, panelWidth - 10, 20), "[F] or [ESC] Close", closeStyle);

        // Get fish sorted by value
        List<FishDisplayData> fishList = GetSortedFishList();

        // Stats header
        GUIStyle statsStyle = new GUIStyle();
        statsStyle.fontSize = 11;
        statsStyle.alignment = TextAnchor.MiddleCenter;
        statsStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        int totalFish = fishList.Sum(f => f.count);
        int totalValue = fishList.Sum(f => f.coinValue * f.count);
        GUI.Label(new Rect(panelX, panelY + 42, panelWidth, 18), $"Total: {totalFish} fish | Worth: {totalValue}g", statsStyle);

        // Fish list area
        float listY = panelY + 65;
        float listHeight = panelHeight - 75;
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
            GUIStyle emptyStyle = new GUIStyle();
            emptyStyle.fontSize = 14;
            emptyStyle.alignment = TextAnchor.MiddleCenter;
            emptyStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(new Rect(0, listHeight / 2 - 30, listArea.width, 60), "No fish caught yet!\n\nGo fishing to fill your inventory.", emptyStyle);
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

                // Fish color/image (square)
                float imgSize = 36;
                Texture2D fishTex = GetOrCreateColorTexture(fish.color);
                GUI.DrawTexture(new Rect(itemRect.x + 8, itemRect.y + (itemHeight - imgSize) / 2 - 2, imgSize, imgSize), fishTex);

                // Fish name
                GUIStyle nameStyle = new GUIStyle();
                nameStyle.fontSize = 13;
                nameStyle.fontStyle = FontStyle.Bold;
                nameStyle.normal.textColor = GetRarityColor(fish.rarity);
                GUI.Label(new Rect(itemRect.x + 52, itemRect.y + 6, 180, 20), fish.name, nameStyle);

                // Rarity
                GUIStyle rarityStyle = new GUIStyle();
                rarityStyle.fontSize = 10;
                rarityStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f);
                GUI.Label(new Rect(itemRect.x + 52, itemRect.y + 24, 100, 16), fish.rarity.ToString(), rarityStyle);

                // Count
                GUIStyle countStyle = new GUIStyle();
                countStyle.fontSize = 14;
                countStyle.fontStyle = FontStyle.Bold;
                countStyle.alignment = TextAnchor.MiddleRight;
                countStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 100, itemRect.y + 4, 40, 20), $"x{fish.count}", countStyle);

                // Value
                GUIStyle valueStyle = new GUIStyle();
                valueStyle.fontSize = 12;
                valueStyle.fontStyle = FontStyle.Bold;
                valueStyle.alignment = TextAnchor.MiddleRight;
                valueStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 60, itemRect.y + 4, 55, 20), $"{fish.coinValue}g", valueStyle);

                // Total value for this stack
                GUIStyle totalStyle = new GUIStyle();
                totalStyle.fontSize = 10;
                totalStyle.alignment = TextAnchor.MiddleRight;
                totalStyle.normal.textColor = new Color(0.6f, 0.6f, 0.5f);
                int stackValue = fish.coinValue * fish.count;
                GUI.Label(new Rect(itemRect.x + itemRect.width - 80, itemRect.y + 24, 75, 16), $"({stackValue}g total)", totalStyle);

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

    List<FishDisplayData> GetSortedFishList()
    {
        List<FishDisplayData> result = new List<FishDisplayData>();

        if (FishingSystem.Instance == null || GameManager.Instance == null)
            return result;

        var fishDatabase = FishingSystem.Instance.fishDatabase;
        var inventory = GameManager.Instance.fishInventory;

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
                    count = count
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
}
