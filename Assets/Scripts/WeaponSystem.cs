using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles weapon equipping, hotbar, and combat in the Ice Realm
/// Player can switch between fishing rod and weapons using number keys
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    public static WeaponSystem Instance { get; private set; }

    // Current weapon state
    private WeaponData equippedWeapon;
    private bool weaponMode = false; // false = fishing rod, true = weapon
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    // Weapon model on player
    private GameObject weaponModel;
    private Transform weaponHand;

    // Owned weapons
    private List<WeaponData> ownedWeapons = new List<WeaponData>();

    // UI
    private Texture2D hotbarTexture;
    private Texture2D selectedTexture;

    // Attack animation
    private float attackAnimTime = 0f;
    private Quaternion attackStartRot;
    private Quaternion attackEndRot;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        CreateUITextures();

        // Find weapon hand (create if not found)
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            weaponHand = player.transform.Find("WeaponHand");
            if (weaponHand == null)
            {
                GameObject hand = new GameObject("WeaponHand");
                hand.transform.SetParent(player.transform);
                hand.transform.localPosition = new Vector3(0.4f, 0.8f, 0.3f);
                weaponHand = hand.transform;
            }
        }
    }

    void CreateUITextures()
    {
        hotbarTexture = new Texture2D(1, 1);
        hotbarTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.25f, 0.8f));
        hotbarTexture.Apply();

        selectedTexture = new Texture2D(1, 1);
        selectedTexture.SetPixel(0, 0, new Color(0.4f, 0.5f, 0.6f, 0.9f));
        selectedTexture.Apply();
    }

    void Update()
    {
        if (!MainMenu.GameStarted) return;
        if (!IsInIceRealm()) return;

        HandleInput();
        UpdateAttackAnimation();
    }

    bool IsInIceRealm()
    {
        // Check if player is in ice realm (X > 400)
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            return player.transform.position.x > 400f;
        }
        return false;
    }

    void HandleInput()
    {
        // Number keys 1-4 for weapon slots, 0 or ` for fishing rod
        if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            SwitchToFishingRod();
        }

        for (int i = 0; i < ownedWeapons.Count && i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipWeapon(ownedWeapons[i]);
            }
        }

        // Left click to attack (if weapon equipped and not fishing)
        if (weaponMode && equippedWeapon != null && Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    void SwitchToFishingRod()
    {
        weaponMode = false;
        equippedWeapon = null;
        RemoveWeaponModel();

        // Re-enable fishing rod animator
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            FishingRodAnimator rodAnim = player.GetComponent<FishingRodAnimator>();
            if (rodAnim != null)
            {
                rodAnim.enabled = true;
            }
        }

        Debug.Log("Switched to Fishing Rod");
    }

    public void EquipWeapon(WeaponData weapon)
    {
        if (weapon == null) return;

        weaponMode = true;
        equippedWeapon = weapon;

        // Disable fishing rod
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            FishingRodAnimator rodAnim = player.GetComponent<FishingRodAnimator>();
            if (rodAnim != null)
            {
                rodAnim.enabled = false;
                // Hide the fishing rod visually
                Transform rodPivot = player.transform.Find("RodPivot");
                if (rodPivot != null)
                {
                    rodPivot.gameObject.SetActive(false);
                }
            }
        }

        // Create weapon model
        CreateWeaponModel(weapon);

        Debug.Log($"Equipped {weapon.name}!");
    }

    public void AddOwnedWeapon(WeaponData weapon)
    {
        if (!ownedWeapons.Exists(w => w.name == weapon.name))
        {
            ownedWeapons.Add(weapon);
        }
    }

    void CreateWeaponModel(WeaponData weapon)
    {
        RemoveWeaponModel();

        if (weaponHand == null) return;

        weaponModel = new GameObject("WeaponModel");
        weaponModel.transform.SetParent(weaponHand);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localRotation = Quaternion.Euler(-45, 0, 0);

        Material bladeMat = new Material(Shader.Find("Standard"));
        Material handleMat = new Material(Shader.Find("Standard"));

        switch (weapon.type)
        {
            case WeaponType.Knife:
                CreateKnifeModel(bladeMat, handleMat);
                break;
            case WeaponType.Spear:
                CreateSpearModel(bladeMat, handleMat);
                break;
            case WeaponType.Rapier:
                CreateRapierModel(bladeMat, handleMat);
                break;
            case WeaponType.Lance:
                CreateLanceModel(bladeMat, handleMat);
                break;
        }
    }

    void CreateKnifeModel(Material bladeMat, Material handleMat)
    {
        bladeMat.color = new Color(0.6f, 0.6f, 0.65f);
        bladeMat.SetFloat("_Metallic", 0.7f);
        bladeMat.SetFloat("_Glossiness", 0.6f);

        handleMat.color = new Color(0.4f, 0.25f, 0.15f);

        // Blade
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(weaponModel.transform);
        blade.transform.localPosition = new Vector3(0, 0.15f, 0);
        blade.transform.localScale = new Vector3(0.03f, 0.25f, 0.08f);
        blade.GetComponent<Renderer>().material = bladeMat;
        Object.Destroy(blade.GetComponent<Collider>());

        // Handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Handle";
        handle.transform.SetParent(weaponModel.transform);
        handle.transform.localPosition = new Vector3(0, -0.08f, 0);
        handle.transform.localScale = new Vector3(0.04f, 0.12f, 0.05f);
        handle.GetComponent<Renderer>().material = handleMat;
        Object.Destroy(handle.GetComponent<Collider>());
    }

    void CreateSpearModel(Material bladeMat, Material handleMat)
    {
        bladeMat.color = new Color(0.5f, 0.5f, 0.55f);
        bladeMat.SetFloat("_Metallic", 0.6f);

        handleMat.color = new Color(0.5f, 0.35f, 0.2f);

        // Shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(weaponModel.transform);
        shaft.transform.localPosition = new Vector3(0, 0.5f, 0);
        shaft.transform.localScale = new Vector3(0.04f, 0.8f, 0.04f);
        shaft.GetComponent<Renderer>().material = handleMat;
        Object.Destroy(shaft.GetComponent<Collider>());

        // Tip
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.name = "Tip";
        tip.transform.SetParent(weaponModel.transform);
        tip.transform.localPosition = new Vector3(0, 1.35f, 0);
        tip.transform.localScale = new Vector3(0.06f, 0.2f, 0.02f);
        tip.GetComponent<Renderer>().material = bladeMat;
        Object.Destroy(tip.GetComponent<Collider>());
    }

    void CreateRapierModel(Material bladeMat, Material handleMat)
    {
        bladeMat.color = new Color(0.75f, 0.75f, 0.8f);
        bladeMat.SetFloat("_Metallic", 0.9f);
        bladeMat.SetFloat("_Glossiness", 0.8f);

        handleMat.color = new Color(0.3f, 0.2f, 0.1f);

        Material guardMat = new Material(Shader.Find("Standard"));
        guardMat.color = new Color(0.8f, 0.7f, 0.3f);
        guardMat.SetFloat("_Metallic", 0.8f);

        // Thin blade
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(weaponModel.transform);
        blade.transform.localPosition = new Vector3(0, 0.4f, 0);
        blade.transform.localScale = new Vector3(0.015f, 0.7f, 0.04f);
        blade.GetComponent<Renderer>().material = bladeMat;
        Object.Destroy(blade.GetComponent<Collider>());

        // Guard
        GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        guard.name = "Guard";
        guard.transform.SetParent(weaponModel.transform);
        guard.transform.localPosition = new Vector3(0, 0.02f, 0);
        guard.transform.localScale = new Vector3(0.15f, 0.02f, 0.08f);
        guard.GetComponent<Renderer>().material = guardMat;
        Object.Destroy(guard.GetComponent<Collider>());

        // Handle
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "Handle";
        handle.transform.SetParent(weaponModel.transform);
        handle.transform.localPosition = new Vector3(0, -0.08f, 0);
        handle.transform.localScale = new Vector3(0.03f, 0.08f, 0.03f);
        handle.GetComponent<Renderer>().material = handleMat;
        Object.Destroy(handle.GetComponent<Collider>());
    }

    void CreateLanceModel(Material bladeMat, Material handleMat)
    {
        handleMat.color = new Color(0.6f, 0.4f, 0.25f);

        Material goldMat = new Material(Shader.Find("Standard"));
        goldMat.color = new Color(1f, 0.85f, 0.3f);
        goldMat.SetFloat("_Metallic", 0.95f);
        goldMat.SetFloat("_Glossiness", 0.9f);
        goldMat.EnableKeyword("_EMISSION");
        goldMat.SetColor("_EmissionColor", new Color(0.5f, 0.4f, 0.1f));

        // Thick shaft
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(weaponModel.transform);
        shaft.transform.localPosition = new Vector3(0, 0.7f, 0);
        shaft.transform.localScale = new Vector3(0.06f, 1.2f, 0.06f);
        shaft.GetComponent<Renderer>().material = handleMat;
        Object.Destroy(shaft.GetComponent<Collider>());

        // Golden tip (large)
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.name = "GoldenTip";
        tip.transform.SetParent(weaponModel.transform);
        tip.transform.localPosition = new Vector3(0, 2f, 0);
        tip.transform.localScale = new Vector3(0.1f, 0.4f, 0.04f);
        tip.GetComponent<Renderer>().material = goldMat;
        Object.Destroy(tip.GetComponent<Collider>());

        // Gold bands
        for (int i = 0; i < 3; i++)
        {
            GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            band.name = "GoldBand" + i;
            band.transform.SetParent(weaponModel.transform);
            band.transform.localPosition = new Vector3(0, 0.3f + i * 0.5f, 0);
            band.transform.localScale = new Vector3(0.08f, 0.02f, 0.08f);
            band.GetComponent<Renderer>().material = goldMat;
            Object.Destroy(band.GetComponent<Collider>());
        }
    }

    void RemoveWeaponModel()
    {
        if (weaponModel != null)
        {
            Destroy(weaponModel);
            weaponModel = null;
        }

        // Show fishing rod again
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Transform rodPivot = player.transform.Find("RodPivot");
            if (rodPivot != null)
            {
                rodPivot.gameObject.SetActive(true);
            }
        }
    }

    void TryAttack()
    {
        if (isAttacking) return;
        if (Time.time - lastAttackTime < equippedWeapon.attackSpeed) return;

        lastAttackTime = Time.time;
        StartAttack();

        // Check for polar bear hits
        CheckHitEnemies();
    }

    void StartAttack()
    {
        isAttacking = true;
        attackAnimTime = 0f;
        attackStartPos = weaponModel != null ? weaponModel.transform.localPosition : Vector3.zero;
    }

    // Thrust animation position
    private Vector3 attackStartPos;

    void UpdateAttackAnimation()
    {
        if (!isAttacking || weaponModel == null) return;

        attackAnimTime += Time.deltaTime * 10f; // Faster thrust

        if (attackAnimTime < 0.4f)
        {
            // THRUST FORWARD
            float thrust = attackAnimTime / 0.4f;
            weaponModel.transform.localPosition = attackStartPos + Vector3.forward * thrust * 0.8f;
            // Tilt weapon forward during thrust
            weaponModel.transform.localRotation = Quaternion.Euler(-45 - thrust * 30f, 0, 0);
        }
        else if (attackAnimTime < 0.8f)
        {
            // Return from thrust
            float returnT = (attackAnimTime - 0.4f) / 0.4f;
            weaponModel.transform.localPosition = attackStartPos + Vector3.forward * (1f - returnT) * 0.8f;
            weaponModel.transform.localRotation = Quaternion.Euler(-45 - (1f - returnT) * 30f, 0, 0);
        }
        else
        {
            weaponModel.transform.localPosition = attackStartPos;
            weaponModel.transform.localRotation = Quaternion.Euler(-45, 0, 0);
            isAttacking = false;
        }
    }

    void CheckHitEnemies()
    {
        if (equippedWeapon == null) return;

        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        // Find all polar bears
        PolarBearAI[] bears = FindObjectsOfType<PolarBearAI>();

        foreach (PolarBearAI bear in bears)
        {
            float distance = Vector3.Distance(player.transform.position, bear.transform.position);

            if (distance <= equippedWeapon.range)
            {
                // Check if facing the bear (roughly)
                Vector3 toBear = (bear.transform.position - player.transform.position).normalized;
                float dot = Vector3.Dot(player.transform.forward, toBear);

                if (dot > 0.3f) // Facing roughly toward bear
                {
                    bear.TakeDamage(equippedWeapon.damage);
                    Debug.Log($"Hit polar bear for {equippedWeapon.damage} damage!");
                }
            }
        }
    }

    public bool IsWeaponMode()
    {
        return weaponMode;
    }

    public WeaponData GetEquippedWeapon()
    {
        return equippedWeapon;
    }

    public List<WeaponData> GetOwnedWeapons()
    {
        return ownedWeapons;
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;
        if (!IsInIceRealm()) return;

        DrawWeaponHotbar();
    }

    void DrawWeaponHotbar()
    {
        // Single MAIN HAND slot - positioned higher on screen (above tropical hotbar)
        float slotSize = 60;
        float startX = 20;
        float startY = Screen.height - slotSize - 100; // Higher position

        // Main hand slot background
        Rect mainSlot = new Rect(startX, startY, slotSize, slotSize);

        // Border for main hand
        GUI.color = new Color(0.6f, 0.7f, 0.8f, 0.9f);
        GUI.DrawTexture(new Rect(mainSlot.x - 2, mainSlot.y - 2, mainSlot.width + 4, mainSlot.height + 4), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.DrawTexture(mainSlot, selectedTexture);

        // "MAIN HAND" label
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 10;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
        GUI.Label(new Rect(mainSlot.x, mainSlot.y - 18, mainSlot.width, 16), "MAIN HAND", labelStyle);

        // Show current equipped item
        if (weaponMode && equippedWeapon != null)
        {
            // Show weapon icon
            Texture2D icon = WeaponShopNPC.Instance != null ?
                WeaponShopNPC.Instance.GetWeaponIcon(equippedWeapon.name) : null;

            if (icon != null)
            {
                GUI.DrawTexture(new Rect(mainSlot.x + 10, mainSlot.y + 10, 40, 40), icon);
            }
            else
            {
                GUIStyle weapStyle = new GUIStyle();
                weapStyle.fontSize = 9;
                weapStyle.alignment = TextAnchor.MiddleCenter;
                weapStyle.normal.textColor = Color.white;
                weapStyle.wordWrap = true;
                GUI.Label(mainSlot, equippedWeapon.name, weapStyle);
            }

            // Weapon name below
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = 9;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = new Color(1f, 0.9f, 0.7f);
            GUI.Label(new Rect(mainSlot.x, mainSlot.y + mainSlot.height + 2, mainSlot.width, 14), equippedWeapon.name, nameStyle);
        }
        else
        {
            // Show fishing rod
            GUIStyle rodStyle = new GUIStyle();
            rodStyle.fontSize = 11;
            rodStyle.fontStyle = FontStyle.Bold;
            rodStyle.alignment = TextAnchor.MiddleCenter;
            rodStyle.normal.textColor = new Color(0.7f, 0.5f, 0.3f);
            GUI.Label(mainSlot, "FISHING\nROD", rodStyle);

            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = 9;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = new Color(0.7f, 0.5f, 0.3f);
            GUI.Label(new Rect(mainSlot.x, mainSlot.y + mainSlot.height + 2, mainSlot.width, 14), "Fishing Rod", nameStyle);
        }

        // Controls hint
        GUIStyle hintStyle = new GUIStyle();
        hintStyle.fontSize = 9;
        hintStyle.alignment = TextAnchor.MiddleLeft;
        hintStyle.normal.textColor = new Color(0.5f, 0.6f, 0.7f);

        string hintText = "[` or 0] Rod";
        if (ownedWeapons.Count > 0)
        {
            hintText += "  [1-4] Weapons";
        }
        GUI.Label(new Rect(startX, startY + slotSize + 18, 200, 14), hintText, hintStyle);

        // If weapons owned, show small indicator
        if (ownedWeapons.Count > 0)
        {
            GUIStyle countStyle = new GUIStyle();
            countStyle.fontSize = 8;
            countStyle.alignment = TextAnchor.MiddleRight;
            countStyle.normal.textColor = new Color(0.6f, 0.7f, 0.8f);
            GUI.Label(new Rect(startX, startY + slotSize + 32, slotSize, 12), $"{ownedWeapons.Count} weapons", countStyle);
        }
    }
}
