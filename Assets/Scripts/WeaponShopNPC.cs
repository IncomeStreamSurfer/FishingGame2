using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Weapon Shop NPC - Pik the native weapon seller
/// Hidden behind dead trees, sells weapons for combat
/// </summary>
public class WeaponShopNPC : MonoBehaviour
{
    public static WeaponShopNPC Instance { get; private set; }

    private bool shopOpen = false;
    private bool playerNearby = false;
    private float interactionDistance = 4f;
    private int selectedWeaponIndex = 0;

    // Weapon inventory
    private List<WeaponData> weapons = new List<WeaponData>();
    private List<string> ownedWeapons = new List<string>();

    // UI Textures
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Dictionary<string, Texture2D> weaponIcons = new Dictionary<string, Texture2D>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeWeapons();
        CreateUITextures();
        CreateWeaponIcons();
    }

    void InitializeWeapons()
    {
        weapons.Add(new WeaponData("Dull Knife", 50, 12, 1.0f, 1.5f,
            "A basic knife. Gets the job done.", WeaponType.Knife));

        weapons.Add(new WeaponData("Spear", 250, 12, 1.0f, 3f,
            "Long reach but same damage as knife.", WeaponType.Spear));

        weapons.Add(new WeaponData("Rapier", 1000, 10, 0.5f, 2f,
            "Much faster attacks but slightly less damage.", WeaponType.Rapier));

        weapons.Add(new WeaponData("Lance", 10000, 30, 1.2f, 4f,
            "Massive golden lance. Devastating damage!", WeaponType.Lance));
    }

    void CreateUITextures()
    {
        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0.2f, 0.15f, 0.1f, 0.95f));
        panelTexture.Apply();

        buttonTexture = new Texture2D(1, 1);
        buttonTexture.SetPixel(0, 0, new Color(0.35f, 0.25f, 0.15f, 0.9f));
        buttonTexture.Apply();

        buttonHoverTexture = new Texture2D(1, 1);
        buttonHoverTexture.SetPixel(0, 0, new Color(0.5f, 0.35f, 0.2f, 0.95f));
        buttonHoverTexture.Apply();
    }

    void CreateWeaponIcons()
    {
        weaponIcons["Dull Knife"] = CreateKnifeIcon();
        weaponIcons["Spear"] = CreateSpearIcon();
        weaponIcons["Rapier"] = CreateRapierIcon();
        weaponIcons["Lance"] = CreateLanceIcon();
    }

    #region Weapon Icon Creation
    Texture2D CreateTexture()
    {
        Texture2D tex = new Texture2D(24, 24);
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < 24; x++)
            for (int y = 0; y < 24; y++)
                tex.SetPixel(x, y, clear);
        return tex;
    }

    void FinalizeTexture(Texture2D tex)
    {
        tex.Apply();
        tex.filterMode = FilterMode.Point;
    }

    void FillRect(Texture2D tex, int x, int y, int w, int h, Color col)
    {
        for (int px = x; px < x + w && px < 24; px++)
            for (int py = y; py < y + h && py < 24; py++)
                if (px >= 0 && py >= 0)
                    tex.SetPixel(px, py, col);
    }

    Texture2D CreateKnifeIcon()
    {
        Texture2D tex = CreateTexture();
        Color blade = new Color(0.6f, 0.6f, 0.65f);
        Color bladeShine = new Color(0.8f, 0.8f, 0.85f);
        Color handle = new Color(0.4f, 0.25f, 0.15f);

        // Blade (diagonal)
        for (int i = 0; i < 12; i++)
        {
            tex.SetPixel(6 + i, 6 + i, blade);
            tex.SetPixel(7 + i, 6 + i, blade);
            tex.SetPixel(6 + i, 7 + i, bladeShine);
        }

        // Handle
        FillRect(tex, 2, 2, 5, 5, handle);
        tex.SetPixel(3, 3, new Color(0.5f, 0.35f, 0.2f));

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateSpearIcon()
    {
        Texture2D tex = CreateTexture();
        Color shaft = new Color(0.5f, 0.35f, 0.2f);
        Color tip = new Color(0.5f, 0.5f, 0.55f);
        Color tipShine = new Color(0.7f, 0.7f, 0.75f);

        // Wooden shaft
        for (int i = 0; i < 18; i++)
        {
            tex.SetPixel(3 + i, 3 + i, shaft);
            tex.SetPixel(4 + i, 3 + i, shaft);
        }

        // Metal tip
        FillRect(tex, 18, 18, 4, 4, tip);
        FillRect(tex, 20, 20, 3, 3, tip);
        tex.SetPixel(22, 22, tipShine);
        tex.SetPixel(21, 21, tipShine);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateRapierIcon()
    {
        Texture2D tex = CreateTexture();
        Color blade = new Color(0.75f, 0.75f, 0.8f);
        Color bladeShine = new Color(0.9f, 0.9f, 0.95f);
        Color guard = new Color(0.8f, 0.7f, 0.3f);
        Color handle = new Color(0.3f, 0.2f, 0.1f);

        // Thin blade
        for (int i = 0; i < 16; i++)
        {
            tex.SetPixel(8 + i, 8 + i, blade);
            if (i % 3 == 0) tex.SetPixel(8 + i, 8 + i, bladeShine);
        }

        // Guard (curved)
        FillRect(tex, 4, 6, 6, 2, guard);
        FillRect(tex, 6, 4, 2, 6, guard);

        // Handle
        FillRect(tex, 2, 2, 4, 4, handle);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateLanceIcon()
    {
        Texture2D tex = CreateTexture();
        Color shaft = new Color(0.6f, 0.4f, 0.25f);
        Color gold = new Color(1f, 0.85f, 0.3f);
        Color goldShine = new Color(1f, 0.95f, 0.6f);

        // Thick shaft
        for (int i = 0; i < 16; i++)
        {
            tex.SetPixel(2 + i, 2 + i, shaft);
            tex.SetPixel(3 + i, 2 + i, shaft);
            tex.SetPixel(2 + i, 3 + i, shaft);
        }

        // Golden tip (large)
        FillRect(tex, 16, 16, 6, 6, gold);
        FillRect(tex, 18, 18, 5, 5, gold);
        FillRect(tex, 20, 20, 4, 4, goldShine);
        tex.SetPixel(22, 22, goldShine);
        tex.SetPixel(23, 23, goldShine);

        // Gold bands on shaft
        tex.SetPixel(6, 6, gold);
        tex.SetPixel(7, 7, gold);
        tex.SetPixel(10, 10, gold);
        tex.SetPixel(11, 11, gold);

        FinalizeTexture(tex);
        return tex;
    }
    #endregion

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        playerNearby = distance < interactionDistance;

        if (playerNearby && !shopOpen && Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }

        if (shopOpen)
        {
            HandleInput();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }
    }

    void OpenShop()
    {
        shopOpen = true;
        selectedWeaponIndex = 0;
    }

    void CloseShop()
    {
        shopOpen = false;
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedWeaponIndex = (selectedWeaponIndex - 1 + weapons.Count) % weapons.Count;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedWeaponIndex = (selectedWeaponIndex + 1) % weapons.Count;
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            TryPurchaseWeapon(weapons[selectedWeaponIndex]);
        }
    }

    void TryPurchaseWeapon(WeaponData weapon)
    {
        if (ownedWeapons.Contains(weapon.name))
        {
            // Equip it
            EquipWeapon(weapon);
            return;
        }

        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;

        if (playerGold >= weapon.price)
        {
            GameManager.Instance.AddCoins(-weapon.price);
            ownedWeapons.Add(weapon.name);
            EquipWeapon(weapon);
            Debug.Log($"Purchased {weapon.name}!");
        }
    }

    void EquipWeapon(WeaponData weapon)
    {
        if (WeaponSystem.Instance != null)
        {
            WeaponSystem.Instance.EquipWeapon(weapon);
        }
    }

    public bool IsShopOpen()
    {
        return shopOpen;
    }

    public Texture2D GetWeaponIcon(string weaponName)
    {
        return weaponIcons.ContainsKey(weaponName) ? weaponIcons[weaponName] : null;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        if (playerNearby && !shopOpen)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 18;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.9f, 0.75f, 0.5f);

            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 150, 300, 30),
                "[E] Talk to Pik", promptStyle);
        }

        if (shopOpen)
        {
            DrawShopUI();
        }
    }

    void DrawShopUI()
    {
        float panelWidth = 500;
        float panelHeight = 400;
        Rect panelRect = new Rect(
            Screen.width / 2 - panelWidth / 2,
            Screen.height / 2 - panelHeight / 2,
            panelWidth,
            panelHeight
        );

        GUI.DrawTexture(panelRect, panelTexture);

        // Border
        GUI.color = new Color(0.7f, 0.5f, 0.3f);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y - 2, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y + panelRect.height, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x + panelRect.width, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(0.95f, 0.8f, 0.5f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + 15, panelRect.width, 30), "PIK'S WEAPONS", titleStyle);

        // Subtitle
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = 12;
        subStyle.fontStyle = FontStyle.Italic;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.7f, 0.6f, 0.5f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + 45, panelRect.width, 20),
            "\"Sharp blades for dangerous lands...\"", subStyle);

        // Gold display
        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUIStyle goldStyle = new GUIStyle();
        goldStyle.fontSize = 14;
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.alignment = TextAnchor.MiddleRight;
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);

        GUI.Label(new Rect(panelRect.x + panelRect.width - 150, panelRect.y + 15, 140, 25),
            $"Gold: {playerGold:N0}", goldStyle);

        // Weapons list
        float itemY = panelRect.y + 80;

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 16;
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleLeft;

        GUIStyle statStyle = new GUIStyle();
        statStyle.fontSize = 12;
        statStyle.alignment = TextAnchor.MiddleLeft;
        statStyle.normal.textColor = new Color(0.7f, 0.8f, 0.7f);

        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 11;
        descStyle.wordWrap = true;
        descStyle.normal.textColor = new Color(0.6f, 0.55f, 0.5f);

        GUIStyle priceStyle = new GUIStyle();
        priceStyle.fontSize = 14;
        priceStyle.fontStyle = FontStyle.Bold;
        priceStyle.alignment = TextAnchor.MiddleRight;

        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponData weapon = weapons[i];
            Rect itemRect = new Rect(panelRect.x + 15, itemY + i * 70, panelRect.width - 30, 65);

            // Selection highlight
            if (i == selectedWeaponIndex)
            {
                GUI.DrawTexture(itemRect, buttonHoverTexture);
                nameStyle.normal.textColor = Color.white;
            }
            else
            {
                GUI.DrawTexture(itemRect, buttonTexture);
                nameStyle.normal.textColor = new Color(0.85f, 0.75f, 0.6f);
            }

            // Icon
            if (weaponIcons.ContainsKey(weapon.name))
            {
                GUI.DrawTexture(new Rect(itemRect.x + 8, itemRect.y + 8, 48, 48), weaponIcons[weapon.name]);
            }

            // Name
            bool owned = ownedWeapons.Contains(weapon.name);
            string displayName = owned ? $"{weapon.name} [OWNED]" : weapon.name;
            GUI.Label(new Rect(itemRect.x + 65, itemRect.y + 5, 250, 22), displayName, nameStyle);

            // Stats
            string stats = $"DMG: {weapon.damage} | Speed: {(1f / weapon.attackSpeed):F1}/s | Range: {weapon.range:F1}m";
            GUI.Label(new Rect(itemRect.x + 65, itemRect.y + 26, 300, 18), stats, statStyle);

            // Description
            GUI.Label(new Rect(itemRect.x + 65, itemRect.y + 44, itemRect.width - 180, 20), weapon.description, descStyle);

            // Price
            if (!owned)
            {
                bool canAfford = playerGold >= weapon.price;
                priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.3f) : new Color(0.8f, 0.3f, 0.3f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 100, itemRect.y + 22, 90, 22),
                    $"{weapon.price:N0}g", priceStyle);
            }

            // Left mouse button click to select and buy/equip
            if (GUI.Button(itemRect, "", GUIStyle.none))
            {
                selectedWeaponIndex = i;
                TryPurchaseWeapon(weapon);
            }
        }

        // Instructions
        GUIStyle instrStyle = new GUIStyle();
        instrStyle.fontSize = 11;
        instrStyle.alignment = TextAnchor.MiddleCenter;
        instrStyle.normal.textColor = new Color(0.5f, 0.45f, 0.4f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + panelHeight - 30, panelRect.width, 25),
            "[W/S or Click] Select | [Enter or Click] Buy/Equip | [ESC] Close", instrStyle);
    }
}

public enum WeaponType
{
    None,
    Knife,
    Spear,
    Rapier,
    Lance
}

[System.Serializable]
public class WeaponData
{
    public string name;
    public int price;
    public int damage;
    public float attackSpeed; // Seconds between attacks
    public float range;
    public string description;
    public WeaponType type;

    public WeaponData(string name, int price, int damage, float attackSpeed, float range, string description, WeaponType type)
    {
        this.name = name;
        this.price = price;
        this.damage = damage;
        this.attackSpeed = attackSpeed;
        this.range = range;
        this.description = description;
        this.type = type;
    }
}
