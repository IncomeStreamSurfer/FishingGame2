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

    // Performance: Frame skip for OnGUI
    private int guiFrameSkip = 0;

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
        // Consistent UI style
        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.12f, 0.95f));
        panelTexture.Apply();

        buttonTexture = new Texture2D(1, 1);
        buttonTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.22f, 0.9f));
        buttonTexture.Apply();

        buttonHoverTexture = new Texture2D(1, 1);
        buttonHoverTexture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.32f, 0.95f));
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
        Texture2D tex = new Texture2D(32, 32);
        Color clear = new Color(0, 0, 0, 0);
        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
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
        for (int px = x; px < x + w && px < 32; px++)
            for (int py = y; py < y + h && py < 32; py++)
                if (px >= 0 && py >= 0)
                    tex.SetPixel(px, py, col);
    }

    Texture2D CreateKnifeIcon()
    {
        Texture2D tex = CreateTexture();
        Color blade = new Color(0.6f, 0.6f, 0.65f);
        Color bladeShine = new Color(0.8f, 0.8f, 0.85f);
        Color handle = new Color(0.4f, 0.25f, 0.15f);

        // Blade (diagonal) - scaled up
        for (int i = 0; i < 16; i++)
        {
            tex.SetPixel(8 + i, 8 + i, blade);
            tex.SetPixel(9 + i, 8 + i, blade);
            tex.SetPixel(10 + i, 8 + i, blade);
            tex.SetPixel(8 + i, 9 + i, bladeShine);
            tex.SetPixel(8 + i, 10 + i, bladeShine);
        }

        // Handle - scaled up
        FillRect(tex, 3, 3, 7, 7, handle);
        tex.SetPixel(4, 4, new Color(0.5f, 0.35f, 0.2f));
        tex.SetPixel(5, 5, new Color(0.5f, 0.35f, 0.2f));

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateSpearIcon()
    {
        Texture2D tex = CreateTexture();
        Color shaft = new Color(0.5f, 0.35f, 0.2f);
        Color tip = new Color(0.5f, 0.5f, 0.55f);
        Color tipShine = new Color(0.7f, 0.7f, 0.75f);

        // Wooden shaft - scaled up
        for (int i = 0; i < 24; i++)
        {
            tex.SetPixel(4 + i, 4 + i, shaft);
            tex.SetPixel(5 + i, 4 + i, shaft);
            tex.SetPixel(6 + i, 4 + i, shaft);
        }

        // Metal tip - scaled up
        FillRect(tex, 24, 24, 6, 6, tip);
        FillRect(tex, 26, 26, 4, 4, tip);
        tex.SetPixel(29, 29, tipShine);
        tex.SetPixel(30, 30, tipShine);
        tex.SetPixel(28, 28, tipShine);

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

        // Thin blade - scaled up
        for (int i = 0; i < 22; i++)
        {
            tex.SetPixel(10 + i, 10 + i, blade);
            tex.SetPixel(11 + i, 10 + i, blade);
            if (i % 4 == 0) tex.SetPixel(10 + i, 10 + i, bladeShine);
        }

        // Guard (curved) - scaled up
        FillRect(tex, 5, 8, 8, 3, guard);
        FillRect(tex, 8, 5, 3, 8, guard);

        // Handle - scaled up
        FillRect(tex, 2, 2, 6, 6, handle);

        FinalizeTexture(tex);
        return tex;
    }

    Texture2D CreateLanceIcon()
    {
        Texture2D tex = CreateTexture();
        Color shaft = new Color(0.6f, 0.4f, 0.25f);
        Color gold = new Color(1f, 0.85f, 0.3f);
        Color goldShine = new Color(1f, 0.95f, 0.6f);

        // Thick shaft - scaled up
        for (int i = 0; i < 22; i++)
        {
            tex.SetPixel(3 + i, 3 + i, shaft);
            tex.SetPixel(4 + i, 3 + i, shaft);
            tex.SetPixel(5 + i, 3 + i, shaft);
            tex.SetPixel(3 + i, 4 + i, shaft);
            tex.SetPixel(3 + i, 5 + i, shaft);
        }

        // Golden tip (large) - scaled up
        FillRect(tex, 22, 22, 8, 8, gold);
        FillRect(tex, 24, 24, 6, 6, gold);
        FillRect(tex, 26, 26, 5, 5, goldShine);
        tex.SetPixel(29, 29, goldShine);
        tex.SetPixel(30, 30, goldShine);
        tex.SetPixel(31, 31, goldShine);

        // Gold bands on shaft - scaled up
        tex.SetPixel(8, 8, gold);
        tex.SetPixel(9, 9, gold);
        tex.SetPixel(10, 10, gold);
        tex.SetPixel(14, 14, gold);
        tex.SetPixel(15, 15, gold);
        tex.SetPixel(16, 16, gold);

        FinalizeTexture(tex);
        return tex;
    }
    #endregion

    void Update()
    {
        if (!MainMenu.GameStarted) return;

        // Use cached player reference
        if (!GameCache.IsPlayerValid()) return;

        float distance = Vector3.Distance(transform.position, GameCache.Player.position);
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

    // Unlock a weapon without purchasing (for quest rewards)
    public void UnlockWeapon(string weaponName)
    {
        if (!ownedWeapons.Contains(weaponName))
        {
            ownedWeapons.Add(weaponName);

            // Auto-equip the weapon
            WeaponData weapon = weapons.Find(w => w.name == weaponName);
            if (weapon != null)
            {
                EquipWeapon(weapon);
            }

            Debug.Log($"Weapon unlocked: {weaponName}");
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

        // Performance: Skip frames when not actively interacting
        if (!playerNearby && !shopOpen)
        {
            guiFrameSkip++;
            if (guiFrameSkip % 3 != 0) return; // Skip 2 out of 3 frames
        }

        if (playerNearby && !shopOpen)
        {
            GUIStyle promptStyle = new GUIStyle();
            promptStyle.fontSize = 18;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            promptStyle.normal.textColor = new Color(0.9f, 0.75f, 0.5f);

            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 150, 300, 30),
                "[E] Talk to Pik", promptStyle);
            return; // Early return - don't process shop UI
        }

        if (shopOpen)
        {
            DrawShopUI();
        }
    }

    void DrawShopUI()
    {
        // 35% smaller (500 -> 325, 400 -> 260)
        float panelWidth = 325;
        float panelHeight = 260;
        Rect panelRect = new Rect(
            Screen.width / 2 - panelWidth / 2,
            Screen.height / 2 - panelHeight / 2,
            panelWidth,
            panelHeight
        );

        GUI.DrawTexture(panelRect, panelTexture);

        // Border - gold
        GUI.color = new Color(1f, 0.85f, 0.4f);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y - 2, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y + panelRect.height, panelRect.width + 4, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x - 2, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panelRect.x + panelRect.width, panelRect.y, 2, panelRect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Title - smaller
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.fontSize = 14; // Smaller
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold

        GUI.Label(new Rect(panelRect.x, panelRect.y + 10, panelRect.width, 20), "PIK'S WEAPONS", titleStyle);

        // X close button (top-right)
        GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
        closeStyle.fontSize = 10;
        closeStyle.fontStyle = FontStyle.Bold;
        if (GUI.Button(new Rect(panelRect.x + panelWidth - 22, panelRect.y + 4, 18, 18), "X", closeStyle))
        {
            CloseShop();
        }

        // Character Stats button
        GUIStyle statsStyle = new GUIStyle(GUI.skin.button);
        statsStyle.fontSize = 8;
        statsStyle.fontStyle = FontStyle.Bold;
        if (GUI.Button(new Rect(panelRect.x + panelWidth - 70, panelRect.y + 4, 45, 18), "STATS", statsStyle))
        {
            if (CharacterPanel.Instance != null) CharacterPanel.Instance.Toggle();
        }

        // Subtitle - smaller
        GUIStyle subStyle = new GUIStyle();
        subStyle.fontSize = 8; // Smaller
        subStyle.fontStyle = FontStyle.Italic;
        subStyle.alignment = TextAnchor.MiddleCenter;
        subStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f); // Light gray/cream

        GUI.Label(new Rect(panelRect.x, panelRect.y + 29, panelRect.width, 14),
            "\"Sharp blades for dangerous lands...\"", subStyle);

        // Gold display - smaller
        int playerGold = GameManager.Instance != null ? GameManager.Instance.GetCoins() : 0;
        GUIStyle goldStyle = new GUIStyle();
        goldStyle.fontSize = 10; // Smaller
        goldStyle.fontStyle = FontStyle.Bold;
        goldStyle.alignment = TextAnchor.MiddleRight;
        goldStyle.normal.textColor = new Color(1f, 0.85f, 0.4f); // Gold

        GUI.Label(new Rect(panelRect.x + panelWidth - 100, panelRect.y + 10, 90, 16),
            $"Gold: {playerGold:N0}", goldStyle);

        // Weapons list - smaller
        float itemY = panelRect.y + 52;

        GUIStyle nameStyle = new GUIStyle();
        nameStyle.fontSize = 10; // Smaller
        nameStyle.fontStyle = FontStyle.Bold;
        nameStyle.alignment = TextAnchor.MiddleLeft;

        GUIStyle statStyle = new GUIStyle();
        statStyle.fontSize = 8; // Smaller
        statStyle.alignment = TextAnchor.MiddleLeft;
        statStyle.normal.textColor = new Color(0.7f, 0.8f, 0.7f);

        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 8; // Smaller
        descStyle.wordWrap = true;
        descStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f); // Light gray/cream

        GUIStyle priceStyle = new GUIStyle();
        priceStyle.fontSize = 10; // Smaller
        priceStyle.fontStyle = FontStyle.Bold;
        priceStyle.alignment = TextAnchor.MiddleRight;

        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponData weapon = weapons[i];
            Rect itemRect = new Rect(panelRect.x + 10, itemY + i * 46, panelRect.width - 20, 42); // Smaller

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

            // Icon - smaller
            if (weaponIcons.ContainsKey(weapon.name))
            {
                GUI.DrawTexture(new Rect(itemRect.x + 5, itemRect.y + 5, 32, 32), weaponIcons[weapon.name]);
            }

            // Name - smaller
            bool owned = ownedWeapons.Contains(weapon.name);
            string displayName = owned ? $"{weapon.name} [OWNED]" : weapon.name;
            GUI.Label(new Rect(itemRect.x + 42, itemRect.y + 3, 170, 14), displayName, nameStyle);

            // Stats - smaller
            string stats = $"DMG: {weapon.damage} | Spd: {(1f / weapon.attackSpeed):F1}/s | Rng: {weapon.range:F1}m";
            GUI.Label(new Rect(itemRect.x + 42, itemRect.y + 17, 200, 12), stats, statStyle);

            // Description - smaller
            GUI.Label(new Rect(itemRect.x + 42, itemRect.y + 29, itemRect.width - 120, 13), weapon.description, descStyle);

            // Price - smaller
            if (!owned)
            {
                bool canAfford = playerGold >= weapon.price;
                priceStyle.normal.textColor = canAfford ? new Color(1f, 0.85f, 0.4f) : new Color(0.8f, 0.3f, 0.3f);
                GUI.Label(new Rect(itemRect.x + itemRect.width - 70, itemRect.y + 14, 65, 14),
                    $"{weapon.price:N0}g", priceStyle);
            }

            // Left mouse button click to select and buy/equip
            if (GUI.Button(itemRect, "", GUIStyle.none))
            {
                selectedWeaponIndex = i;
                TryPurchaseWeapon(weapon);
            }
        }

        // Instructions - smaller
        GUIStyle instrStyle = new GUIStyle();
        instrStyle.fontSize = 8; // Smaller
        instrStyle.alignment = TextAnchor.MiddleCenter;
        instrStyle.normal.textColor = new Color(0.5f, 0.45f, 0.4f);

        GUI.Label(new Rect(panelRect.x, panelRect.y + panelHeight - 20, panelRect.width, 16),
            "[W/S/Click] Sel | [Enter/Click] Buy | [ESC] Close", instrStyle);
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
