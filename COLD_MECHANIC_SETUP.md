# Cold Mechanic Setup Guide

## Overview
A nighttime cold mechanic has been implemented for the Tropical Island realm. When the sun goes down, players who aren't wearing proper clothing will get cold and lose health over time.

## Features Implemented

### 1. Core Cold System (`ColdMechanic.cs`)
- **Nighttime Detection**: Automatically detects when it's nighttime (6 PM to 6 AM) using the DayNightCycle system
- **Clothing Check**: Verifies if the player is wearing both a top AND legs (underpants don't count as protection)
- **Cold Damage**: Deals 5 HP every 10 seconds when cold
- **Realm-Specific**: Only active in the Tropical Island realm (RealmType.TropicalIsland)

### 2. Visual Warning System
- **"Too cold!" Warning**: Displays a pulsing blue warning box in the center of the screen
- **Pulsing Effect**: The warning pulses to draw attention
- **Helpful Message**: Tells players to "Wear clothes or wait for dawn!"

### 3. Heart Rate Monitor Integration
- **Elevated BPM**: Adds +25 BPM to the heart rate monitor when the player is cold
- **Seamless Integration**: Works alongside existing BPM systems (health-based BPM, attack BPM boost)
- **Updates in Real-Time**: BPM increases when cold, returns to normal when warm

### 4. Death Message
- **Custom Death Message**: If the player dies from cold, shows: "You froze to death in the cold night..."
- **Integration**: Uses the existing PlayerHealth death message system

## Files Modified

### New Files
1. `Assets/Scripts/ColdMechanic.cs` - Main cold mechanic system
2. `Assets/Scripts/ColdMechanic.cs.meta` - Unity metadata file
3. `COLD_MECHANIC_SETUP.md` - This setup guide

### Modified Files
1. `Assets/Scripts/PlayerHealth.cs`
   - Added cold BPM boost calculation to the Update() method
   - Now checks ColdMechanic.Instance for BPM boost

2. `Assets/Scripts/GameCache.cs`
   - Added ColdMechanic to cached managers
   - Added Cold property for quick access

## Setup Instructions

### Step 1: Add ColdMechanic to the Scene
1. Open your Unity scene (likely `Assets/fish.unity`)
2. Create a new empty GameObject in the hierarchy
3. Name it "ColdMechanic"
4. Add the `ColdMechanic` component to this GameObject
5. The script will automatically initialize itself as a singleton

### Step 2: Verify Dependencies
Make sure these systems exist in your scene:
- `DayNightCycle` - For nighttime detection
- `PlayerHealth` - For damage and BPM
- `PlayerClothingVisuals` - For clothing checks
- `RealmManager` - For realm detection
- `UIManager` - For notifications
- `GameCache` - For caching (should auto-refresh)

### Step 3: Test the System

#### Test Scenario 1: Basic Cold Damage
1. Start the game in the Tropical Island realm
2. Remove all clothing (or wear only underpants)
3. Use the Dev Panel or time skip to set time to 6 PM (18:00)
4. Wait and observe:
   - "Too cold!" warning appears
   - Health decreases by 5 HP every 10 seconds
   - BPM increases by 25
   - Blue notification shows "-5 HP (Cold)"

#### Test Scenario 2: Clothing Protection
1. Start cold (nighttime, no clothes)
2. Put on a shirt and pants from the Clothing Shop
3. Observe:
   - "Too cold!" warning disappears
   - Health stops decreasing
   - BPM returns to normal

#### Test Scenario 3: Dawn Relief
1. Start cold (nighttime, no clothes)
2. Wait until 6 AM or use Dev Panel to advance time
3. Observe:
   - "Too cold!" warning disappears at dawn
   - Cold damage stops
   - BPM returns to normal

#### Test Scenario 4: Death Message
1. Start with low health (e.g., 20 HP)
2. Get cold at night without clothes
3. Let health reach 0
4. Observe: Death screen shows "You froze to death in the cold night..."

## Technical Details

### Clothing Detection Logic
```csharp
// Player needs BOTH top AND legs to stay warm
bool hasTop = topItem != "None";
bool hasLegs = legsItem != "None" && legsItem != "Underpants";
bool isWearingClothes = hasTop && hasLegs;
```

### Nighttime Detection
```csharp
// Night is 6 PM (18:00) to 6 AM (6:00)
bool isNight = DayNightCycle.Instance.IsNight();
```

### BPM Calculation
```csharp
// Base BPM (health-based) + Attack Boost + Cold Boost
currentBPM = baseBPM + attackBPMBoost + coldBoost;
```

### Realm Check
```csharp
// Only active in Tropical Island
if (GameCache.GetCurrentRealm() != RealmType.TropicalIsland)
{
    // Mechanic is disabled
}
```

## Configuration

You can adjust these values in `ColdMechanic.cs`:

```csharp
private float coldDamageInterval = 10f;  // Seconds between damage ticks
private float coldDamageAmount = 5f;     // HP lost per tick
private int coldBPMBoost = 25;           // BPM increase when cold
```

## Troubleshooting

### Issue: Cold warning doesn't appear
- Check that you're in the Tropical Island realm
- Verify it's nighttime (6 PM - 6 AM)
- Make sure you're not wearing both top and legs

### Issue: BPM doesn't increase
- Check that ColdMechanic is in the scene and enabled
- Verify GameCache has refreshed (should happen automatically)
- Look for ColdMechanic.Instance in the debugger

### Issue: No damage being taken
- Check that PlayerHealth.Instance exists
- Verify the game has started (MainMenu.GameStarted = true)
- Check console for "Cold damage: 5 HP" messages

### Issue: Wearing clothes but still cold
- Must wear BOTH a top (shirt/coat) AND legs (pants, not underpants)
- Underpants alone don't provide warmth
- Check PlayerClothingVisuals.Instance for current equipped items

## Future Enhancements

Possible additions to consider:
1. Different cold levels based on clothing quality (light shirt vs warm coat)
2. Campfire system to warm up without clothes
3. Temperature gauge UI element
4. Frostbite visual effects
5. Shivering animation when cold
6. Ice Realm integration (different cold mechanics)
7. Seasonal variations (colder in winter)

## Integration with Other Systems

### Works With:
- **Fish Buff System**: Snapper's Delight health protection works against cold damage
- **Max Health Buff**: Cold damage respects health protection buffs
- **Day/Night Cycle**: Automatically syncs with game time
- **Clothing System**: Fully integrated with existing clothing visuals
- **Death System**: Uses custom death messages
- **Realm System**: Only active in appropriate realm

### Compatible With Future Features:
- Additional realms can have their own temperature mechanics
- Different clothing items can have different warmth ratings
- Weather system can affect cold intensity

## Code Quality Notes

- **Singleton Pattern**: Uses standard Unity singleton pattern
- **Performance**: Minimal overhead, only checks conditions in Update()
- **Caching**: Uses cached textures to avoid GC
- **Null Safety**: Checks for null references before accessing systems
- **Debug Logging**: Includes console logs for debugging
- **Clean Integration**: Doesn't modify core PlayerHealth logic, uses existing TakeDamage API

## Credits

This cold mechanic was designed to integrate seamlessly with the existing game systems while providing a new survival challenge for players in the Tropical Island realm.
