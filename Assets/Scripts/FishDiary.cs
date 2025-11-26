using UnityEngine;
using System.Collections.Generic;

public class FishDiary : MonoBehaviour
{
    public static FishDiary Instance { get; private set; }

    private bool isOpen = false;
    private bool initialized = false;
    private Vector2 scrollPosition = Vector2.zero;

    // Cached textures
    private Texture2D backgroundTex;
    private Texture2D entryBgTex;
    private Texture2D headerTex;

    // Cached styles
    private GUIStyle titleStyle;
    private GUIStyle fishNameStyle;
    private GUIStyle fishCountStyle;
    private GUIStyle rarityStyle;
    private GUIStyle instructionStyle;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Invoke("Initialize", 0.5f);
    }

    void Initialize()
    {
        // Create textures
        backgroundTex = new Texture2D(2, 2);
        Color[] bgPixels = new Color[4];
        for (int i = 0; i < 4; i++) bgPixels[i] = new Color(0.1f, 0.15f, 0.2f, 0.95f);
        backgroundTex.SetPixels(bgPixels);
        backgroundTex.Apply();

        entryBgTex = new Texture2D(2, 2);
        Color[] entryPixels = new Color[4];
        for (int i = 0; i < 4; i++) entryPixels[i] = new Color(0.15f, 0.2f, 0.25f, 0.8f);
        entryBgTex.SetPixels(entryPixels);
        entryBgTex.Apply();

        headerTex = new Texture2D(2, 2);
        Color[] headerPixels = new Color[4];
        for (int i = 0; i < 4; i++) headerPixels[i] = new Color(0.2f, 0.3f, 0.4f, 0.9f);
        headerTex.SetPixels(headerPixels);
        headerTex.Apply();

        initialized = true;
    }

    void InitStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle();
        titleStyle.fontSize = 32;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.9f, 0.85f, 0.6f);

        fishNameStyle = new GUIStyle();
        fishNameStyle.fontSize = 18;
        fishNameStyle.fontStyle = FontStyle.Bold;
        fishNameStyle.alignment = TextAnchor.MiddleLeft;
        fishNameStyle.normal.textColor = Color.white;

        fishCountStyle = new GUIStyle();
        fishCountStyle.fontSize = 16;
        fishCountStyle.alignment = TextAnchor.MiddleRight;
        fishCountStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

        rarityStyle = new GUIStyle();
        rarityStyle.fontSize = 14;
        rarityStyle.alignment = TextAnchor.MiddleLeft;
        rarityStyle.normal.textColor = Color.gray;

        instructionStyle = new GUIStyle();
        instructionStyle.fontSize = 14;
        instructionStyle.alignment = TextAnchor.MiddleCenter;
        instructionStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Toggle fish diary with J key
        if (Input.GetKeyDown(KeyCode.J))
        {
            isOpen = !isOpen;
        }

        // Close with Escape
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            isOpen = false;
        }
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted || !initialized || !isOpen) return;

        InitStyles();

        float panelWidth = 500;
        float panelHeight = 600;
        float panelX = (Screen.width - panelWidth) / 2;
        float panelY = (Screen.height - panelHeight) / 2;

        // Background
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), backgroundTex);

        // Header
        GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 60), headerTex);
        GUI.Label(new Rect(panelX, panelY + 10, panelWidth, 40), "FISH DIARY", titleStyle);

        // Total fish caught
        int totalCaught = GameManager.Instance != null ? GameManager.Instance.GetTotalFishCaught() : 0;
        int uniqueTypes = GameManager.Instance != null ? GameManager.Instance.fishInventory.Count : 0;
        int totalTypes = FishingSystem.Instance != null ? FishingSystem.Instance.fishDatabase.Count : 0;

        GUI.Label(new Rect(panelX, panelY + 65, panelWidth, 25),
            $"Total Caught: {totalCaught}  |  Species Discovered: {uniqueTypes}/{totalTypes}", instructionStyle);

        // Fish list area
        float listY = panelY + 95;
        float listHeight = panelHeight - 130;

        // Scrollable fish list
        Rect scrollViewRect = new Rect(panelX + 10, listY, panelWidth - 20, listHeight);

        // Calculate content height
        float entryHeight = 60;
        float contentHeight = 0;

        if (FishingSystem.Instance != null)
        {
            contentHeight = FishingSystem.Instance.fishDatabase.Count * entryHeight + 20;
        }

        Rect contentRect = new Rect(0, 0, panelWidth - 40, contentHeight);

        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);

        float yOffset = 5;

        if (FishingSystem.Instance != null)
        {
            foreach (FishData fish in FishingSystem.Instance.fishDatabase)
            {
                bool hasCaught = GameManager.Instance != null &&
                                 GameManager.Instance.fishInventory.ContainsKey(fish.id);
                int count = hasCaught ? GameManager.Instance.fishInventory[fish.id] : 0;

                // Entry background
                GUI.DrawTexture(new Rect(5, yOffset, panelWidth - 50, entryHeight - 5), entryBgTex);

                // Fish color indicator
                Texture2D fishColorTex = new Texture2D(1, 1);
                fishColorTex.SetPixel(0, 0, hasCaught ? fish.fishColor : Color.gray);
                fishColorTex.Apply();
                GUI.DrawTexture(new Rect(10, yOffset + 5, 40, 40), fishColorTex);

                // Fish name (or ??? if not caught)
                fishNameStyle.normal.textColor = hasCaught ? Color.white : new Color(0.4f, 0.4f, 0.4f);
                string displayName = hasCaught ? fish.fishName : "???";
                GUI.Label(new Rect(60, yOffset + 5, 200, 25), displayName, fishNameStyle);

                // Rarity
                rarityStyle.normal.textColor = GetRarityColor(fish.rarity);
                string rarityText = hasCaught ? fish.rarity.ToString() : "Unknown";
                GUI.Label(new Rect(60, yOffset + 30, 150, 20), rarityText, rarityStyle);

                // Coin value
                if (hasCaught)
                {
                    GUI.Label(new Rect(200, yOffset + 30, 100, 20), $"Value: {fish.coinValue}g", rarityStyle);
                }

                // Count
                fishCountStyle.normal.textColor = hasCaught ? new Color(0.5f, 1f, 0.5f) : Color.gray;
                GUI.Label(new Rect(panelWidth - 150, yOffset + 15, 80, 30), $"x{count}", fishCountStyle);

                yOffset += entryHeight;
            }
        }

        GUI.EndScrollView();

        // Instructions at bottom
        GUI.Label(new Rect(panelX, panelY + panelHeight - 30, panelWidth, 25),
            "Press J to close", instructionStyle);
    }

    Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.gray;
            case Rarity.Uncommon: return Color.green;
            case Rarity.Rare: return new Color(0.3f, 0.5f, 1f);
            case Rarity.Epic: return new Color(0.7f, 0.3f, 0.9f);
            case Rarity.Legendary: return new Color(1f, 0.8f, 0f);
            case Rarity.Mythic: return new Color(1f, 0.3f, 0.3f);
            default: return Color.white;
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    void OnDestroy()
    {
        if (backgroundTex != null) Destroy(backgroundTex);
        if (entryBgTex != null) Destroy(entryBgTex);
        if (headerTex != null) Destroy(headerTex);
    }
}
