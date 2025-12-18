# WET Debuff System - Setup Guide

## Overview
The WET debuff system adds a visual and gameplay mechanic when the player touches water. When wet, the player loses 1 extra HP every 5 seconds (on top of the normal hunger damage).

## What's Been Created

### 1. New Script: `WetDebuffSystem.cs`
Location: `Assets/Scripts/WetDebuffSystem.cs`

**Features:**
- Detects when player enters water (Y position < 0.85)
- Applies WET debuff with visual indicator
- Deals 1 HP damage every 5 seconds while wet
- Removes debuff when player exits water (Y position >= 1.0)
- Shows blue debuff bar with countdown timer
- Displays notifications when getting wet/drying off

**UI Display:**
- BLUE debuff bar appears on the right side of the screen
- Shows "WET" label in light blue
- Shows "-1 HP every 5s" description
- Countdown bar shows time until next damage tick
- Positioned below active fish buffs (if any)

### 2. Integration
- Added to `AutoSetup.cs` so it's automatically created in new scenes
- Follows same pattern as other systems (FishBuffSystem, ColdMechanic)
- Uses singleton pattern for easy access from other scripts

## How to Set Up

### Method 1: Auto Setup (Recommended)
1. Open Unity
2. Open the scene `Assets/fish.unity`
3. Go to **GameObject > Auto Setup Scene**
4. The WetDebuffSystem will be automatically created along with all other game systems

### Method 2: Manual Setup
1. Open Unity
2. Open the scene `Assets/fish.unity`
3. Right-click in Hierarchy
4. Create Empty GameObject
5. Name it "WetDebuffSystem"
6. Add Component > WetDebuffSystem script
7. Save the scene

## How to Test

### In Unity Editor:
1. Press Play
2. Move the player character into the water (walk off the dock)
3. You should see:
   - A notification: "You are WET!" (blue text)
   - A BLUE debuff bar appear on the right side showing "WET"
   - The bar has a countdown timer showing when next damage occurs
   - Every 5 seconds: "-1 HP (Wet)" notification appears
   - Your HP decreases by 1 (in addition to normal hunger loss)

4. Move the player back onto land/dock
5. You should see:
   - A notification: "You dried off!" (light blue text)
   - The WET debuff bar disappears
   - No more wet damage taken

### Expected Behavior:
- **In Water (Y < 0.85):** WET debuff active, taking extra damage
- **On Land (Y >= 1.0):** WET debuff removed, normal gameplay
- **Total Damage Rate When Wet:**
  - Hunger: 1 HP every 5 seconds (normal)
  - Wet: 1 HP every 5 seconds (extra)
  - **Total: 2 HP every 5 seconds when in water**

## Visual Reference

### UI Layout (Right Side of Screen):
```
┌─────────────────────┐
│   HP: 95/100       │  ← Health Bar
│ ┌─ECG Monitor────┐ │
│ │ ~~~~~~~~~~~    │ │  ← Heartbeat
│ └────────────────┘ │
├─────────────────────┤
│ MARLIN'S LUCK      │  ← Fish Buff (if active)
│ 2m 30s      ▓▓▓▓▓░ │
├─────────────────────┤
│ WET                │  ← NEW: Wet Debuff
│ -1 HP every 5s  ░▓ │  ← Shows countdown
└─────────────────────┘
```

## Technical Details

### Water Detection:
- Water level threshold: Y < 0.85 (same as drowning detection in PlayerHealth.cs)
- Dry threshold: Y >= 1.0 (ensures player is fully on land)
- Uses GameCache for efficient player position lookup

### Damage System:
- Damage amount: 1 HP
- Damage interval: 5 seconds
- Damage type: Regular (can be blocked by Snapper's Delight buff)
- Works alongside existing health decay (hunger)

### UI Positioning:
- Aligned with HP/ECG panel (right side, width 170px)
- Positioned below all active fish buffs
- Same style as fish buff bars for consistency
- Blue color scheme to indicate water-related debuff

## Code Architecture

### Key Methods:
- `Update()`: Checks player Y position and manages wet state
- `ApplyWetDamage()`: Deals damage when timer expires
- `DrawWetDebuff()`: Renders the UI debuff bar
- `IsWet()`: Public method for other systems to check wet state

### Integrations:
- **GameCache**: Efficient player position checking
- **PlayerHealth**: Damage application
- **UIManager**: Notifications
- **MainMenu**: Game state checking
- **FishBuffSystem**: UI positioning coordination

## Customization Options

You can easily modify these values in `WetDebuffSystem.cs`:

```csharp
// Line 14-15: Damage settings
private float wetDamageInterval = 5f;    // Change to 10f for slower damage
private float wetDamageAmount = 1f;      // Change to 2f for more damage

// Line 18-19: Water detection
private float waterLevel = 0.85f;        // Lower = easier to get wet
private float dryLevel = 1.0f;           // Higher = need to climb higher to dry
```

## Troubleshooting

### Debuff bar not showing:
- Check that WetDebuffSystem GameObject exists in scene
- Verify script is attached and enabled
- Make sure player is below Y = 0.85

### Damage not applying:
- Check that PlayerHealth.Instance is not null
- Verify MainMenu.GameStarted is true
- Check if Snapper's Delight buff is active (blocks all damage)

### UI positioning issues:
- Check Screen.width for resolution scaling
- Verify FishBuffSystem.Instance exists for proper offset calculation

## Future Enhancements

Possible additions:
- Add "dripping" particle effects when wet
- Slow movement speed while wet
- Make certain clothing prevent getting wet
- Add towels or campfires to dry off faster
- Different wet levels (damp, wet, soaked)

## Files Modified

1. **NEW:** `Assets/Scripts/WetDebuffSystem.cs` - Main system script
2. **NEW:** `Assets/Scripts/WetDebuffSystem.cs.meta` - Unity metadata
3. **MODIFIED:** `Assets/Editor/AutoSetup.cs` - Added auto-creation of WetDebuffSystem

## Related Systems

- **PlayerHealth.cs**: Takes damage from wet debuff
- **WaterEffect.cs**: Provides water surface reference
- **FishBuffSystem.cs**: Similar UI pattern, positioning
- **ColdMechanic.cs**: Similar debuff pattern (cold at night)

---

**System Status:** ✅ Ready to use
**Compatibility:** Works with all existing game systems
**Performance:** Minimal impact (simple Y position check)
