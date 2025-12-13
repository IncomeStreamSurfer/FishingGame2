using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages accessory/ring equipment system
/// Handles inventory, equipping, and accessory effects
/// </summary>
public class AccessorySystem : MonoBehaviour
{
    public static AccessorySystem Instance { get; private set; }

    // Owned accessories
    private List<AccessoryItem> ownedAccessories = new List<AccessoryItem>();

    // Currently equipped accessories by slot
    private Dictionary<string, AccessoryItem> equippedAccessories = new Dictionary<string, AccessoryItem>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddAccessory(AccessoryItem item)
    {
        if (!ownedAccessories.Exists(a => a.name == item.name))
        {
            ownedAccessories.Add(item);
            Debug.Log($"Added accessory: {item.name}");
        }
    }

    public void EquipAccessory(AccessoryItem item)
    {
        if (!ownedAccessories.Exists(a => a.name == item.name))
        {
            Debug.LogWarning($"Cannot equip {item.name} - not owned!");
            return;
        }

        // Equip to slot (replaces any existing accessory in that slot)
        equippedAccessories[item.slot] = item;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLootNotification($"Equipped: {item.name}", new Color(0.4f, 0.9f, 0.4f));
        }

        Debug.Log($"Equipped {item.name} in {item.slot} slot");
    }

    public void UnequipAccessory(string slot)
    {
        if (equippedAccessories.ContainsKey(slot))
        {
            string itemName = equippedAccessories[slot].name;
            equippedAccessories.Remove(slot);
            Debug.Log($"Unequipped {itemName} from {slot} slot");
        }
    }

    public bool HasAccessory(string accessoryName)
    {
        return ownedAccessories.Exists(a => a.name == accessoryName);
    }

    public bool IsEquipped(string accessoryName)
    {
        foreach (var kvp in equippedAccessories)
        {
            if (kvp.Value.name == accessoryName)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasEffect(AccessoryEffect effect)
    {
        foreach (var kvp in equippedAccessories)
        {
            if (kvp.Value.effect == effect)
            {
                return true;
            }
        }
        return false;
    }

    public AccessoryItem GetEquippedInSlot(string slot)
    {
        return equippedAccessories.ContainsKey(slot) ? equippedAccessories[slot] : null;
    }

    public List<AccessoryItem> GetOwnedAccessories()
    {
        return new List<AccessoryItem>(ownedAccessories);
    }

    public Dictionary<string, AccessoryItem> GetEquippedAccessories()
    {
        return new Dictionary<string, AccessoryItem>(equippedAccessories);
    }

    void OnGUI()
    {
        if (!MainMenu.GameStarted) return;

        // Show equipped ring indicator in jungle realm
        if (GameCache.IsPlayerValid() && GameCache.Player.position.x > 900f)
        {
            DrawAccessoryHUD();
        }
    }

    void DrawAccessoryHUD()
    {
        // Small accessory slot display (similar to weapon hotbar)
        float slotSize = 50;
        float startX = 20;
        float startY = Screen.height - slotSize - 200; // Below weapon hotbar

        AccessoryItem ringItem = GetEquippedInSlot("Ring");

        if (ringItem != null)
        {
            // Ring slot background
            Rect ringSlot = new Rect(startX, startY, slotSize, slotSize);

            // Border
            GUI.color = new Color(0.8f, 0.7f, 0.3f, 0.9f);
            GUI.DrawTexture(new Rect(ringSlot.x - 2, ringSlot.y - 2, ringSlot.width + 4, ringSlot.height + 4), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Background
            GUI.color = new Color(0.25f, 0.2f, 0.15f, 0.8f);
            GUI.DrawTexture(ringSlot, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Label
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 9;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.normal.textColor = new Color(0.8f, 0.7f, 0.4f);
            GUI.Label(new Rect(ringSlot.x, ringSlot.y - 16, ringSlot.width, 14), "RING", labelStyle);

            // Icon or text
            GUIStyle iconStyle = new GUIStyle();
            iconStyle.fontSize = 8;
            iconStyle.alignment = TextAnchor.MiddleCenter;
            iconStyle.normal.textColor = new Color(0.3f, 0.8f, 0.3f);
            iconStyle.wordWrap = true;
            iconStyle.fontStyle = FontStyle.Bold;

            GUI.Label(ringSlot, "SNAKE\nCHARM", iconStyle);

            // Name below
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.fontSize = 8;
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = new Color(0.3f, 0.9f, 0.3f);
            GUI.Label(new Rect(ringSlot.x, ringSlot.y + ringSlot.height + 2, ringSlot.width, 12), ringItem.name, nameStyle);
        }
    }
}

public enum AccessoryEffect
{
    None,
    SnakeImmunity,
    FireResistance,
    IceResistance,
    SpeedBoost,
    DamageBoost,
    ToxicImmunity,
    ZombieSafety
}

[System.Serializable]
public class AccessoryItem
{
    public string name;
    public string slot; // "Ring", "Necklace", "Bracelet", etc.
    public int price;
    public string description;
    public AccessoryEffect effect;

    public AccessoryItem()
    {
        effect = AccessoryEffect.None;
    }
}
