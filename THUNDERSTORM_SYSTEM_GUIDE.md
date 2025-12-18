# Thunderstorm System - Implementation Guide

## Overview
The Thunderstorm System adds dangerous weather events to the fishing game. Players must be cautious during storms as lightning can strike and kill them if they're on the dock.

## Features Implemented

### 1. Storm Timing
- **Frequency**: Occurs randomly once every 1-2 in-game days (5-10 real minutes)
- **Duration**: Each storm lasts 1-2 minutes
- **Random Variation**: All timings use randomization for unpredictability

### 2. Visual Effects
- **Sky Darkening**: Ambient light reduces from bright sunny to dark storm clouds
- **Sun Dimming**: Directional light intensity drops to 30% during storms
- **Color Shift**: Sky color shifts from blue to dark gray/blue
- **Lightning Flashes**: Random visual flashes throughout the storm (harmless)
- **Screen Flash**: Full white screen flash when player is struck by lightning

### 3. Audio Effects
All audio is procedurally generated for authentic storm sounds:
- **Thunder Rumble**: Deep rolling thunder (continuous loop during storm)
- **Heavy Rain**: Filtered white noise rain sound (continuous loop)
- **Lightning Crack**: Sharp crack followed by rumbling thunder (when struck)
- **Distant Thunder**: Softer thunder sounds for visual lightning flashes

### 4. Danger Mechanics

#### Lightning Strike System
- **Trigger**: Only occurs when player is on a dock or bridge (over water)
- **Chance**: 1 in 100 chance per second (1% per second)
- **Warning**: 2-second warning message before strike
  - Message: "Lightning strike approaching, get away from the water!!!"
  - Pulsing yellow/red warning box appears on screen
- **Strike Effect**:
  - Loud lightning crack sound
  - Full white screen flash
  - Instant death (999 damage)
  - Death message: "ZAP! You're fried. Killed by lightning."

#### Safe Zones
Player is SAFE when on land (grass, sand, ice, etc.)
Player is in DANGER on:
- Main Tropical Dock (X=-12, Z=5-60)
- Ice Realm Dock (X=500, Z=22-70)
- Jungle Realm Dock (X=988, Z=5-60)
- Bridge to Goldie's Island (X=25, Z=25-80)

If player moves to safety after warning is triggered, the lightning strike is cancelled!

### 5. Storm Progression
1. **Storm Starts**: Notification "Storm approaching..."
2. **Fade In**: Storm intensity ramps up over ~3 seconds
3. **Active Storm**: Full storm effects for 1-2 minutes
4. **Lightning**: Random chance of strike if player on dock
5. **Fade Out**: Storm intensity fades over ~3 seconds
6. **Storm Ends**: Notification "Storm passing..."
7. **Cooldown**: Next storm scheduled 5-10 minutes later

## Technical Implementation

### File Location
- **Script**: `Assets/Scripts/ThunderstormSystem.cs`
- **Meta**: `Assets/Scripts/ThunderstormSystem.cs.meta`

### Integration
The system is automatically added to the scene via `AutoSetup.cs` (line 141-142).

### Key Methods

#### Storm Management
- `StartStorm()` - Initiates a new storm
- `UpdateStorm()` - Handles active storm logic
- `EndStorm()` - Concludes the storm

#### Lightning
- `IsPlayerOnDock()` - Checks if player is in danger zone
- `ExecuteLightningStrike()` - Kills player with lightning
- `FlashLightningVisual()` - Visual-only lightning (safe)

#### Audio Generation
- `CreateThunderRumbleClip()` - Deep rolling thunder
- `CreateHeavyRainClip()` - Heavy rain sound
- `CreateLightningCrackClip()` - Sharp lightning crack
- `CreateDistantThunderClip()` - Softer distant thunder

### Procedural Audio Details

All audio is generated at runtime using sinusoidal waves and filtered noise:

**Thunder Rumble**:
- Multiple low frequencies (20Hz, 35Hz, 50Hz)
- Filtered noise for texture
- Rolling envelope for natural sound
- 4-second loop

**Heavy Rain**:
- White noise with heavy low-pass filtering
- Variations in intensity
- 2-second loop

**Lightning Crack**:
- Sharp white noise burst at start
- Secondary crack
- Rolling thunder tail
- ~1.2 seconds total

## UI Elements

### Storm Indicator
- Displays at top center of screen during active storm
- Shows remaining storm time in seconds
- Format: "THUNDERSTORM (45s)"

### Lightning Warning
- Large pulsing yellow/red box
- Lightning bolt icon (!)
- Warning text: "Lightning strike approaching, get away from the water!!!"
- Appears 2 seconds before strike

### Notifications
- "Storm approaching..." (storm start)
- "Storm passing..." (storm end)
- "Safe on land!" (if strike cancelled by reaching safety)
- "ZAP! You're fried. Killed by lightning." (death message)

## Testing the System

### Console Commands (RECOMMENDED)
The easiest way to test storms is with the developer console:

1. Press **~** or **F1** or **F12** to open the console
2. Type `storm` and press Enter - Storm starts immediately
3. Type `endstorm` to stop the storm
4. Type `time 12` to test storms during day
5. Type `time 0` to test storms at night

**Example Test Session:**
```
> storm
Thunderstorm triggered!

> time 20
Time set to 20.0 hours (8:00 PM)

> heal
Player fully healed!

> endstorm
Storm ended.
```

### Complete Storm Testing Workflow
```bash
# 1. Open console
Press ~ or F1 or F12

# 2. Start storm during day
> time 12
> storm

# 3. Test lightning on dock
Walk to dock, watch for warning

# 4. Test safe zone
Move to land, verify warning cancels

# 5. Test storm at night
> time 0
> storm

# 6. Test time-of-day variations
> time 6    # Sunrise storm
> storm
> time 18   # Sunset storm
> storm

# 7. End storm when done
> endstorm
```

### In-Game Testing (Natural Timing)
1. Start the game
2. Wait 5-10 minutes for first storm (or modify `nextStormTime` in code for faster testing)
3. When storm starts, sky darkens and thunder/rain sounds play
4. Stand on the dock during storm
5. Watch for lightning warning
6. Move to land to cancel strike, or stay on dock to get zapped

### Quick Testing (Code Modification)
To test faster without console, modify these values in `ThunderstormSystem.cs`:
```csharp
private float minTimeBetweenStorms = 30f;   // 30 seconds instead of 5 minutes
private float maxTimeBetweenStorms = 60f;   // 1 minute instead of 10 minutes
private float lightningChancePerSecond = 0.5f; // 50% chance instead of 1%
```

## Configuration Options

All timing values can be adjusted in the script:

| Variable | Default | Description |
|----------|---------|-------------|
| `minTimeBetweenStorms` | 300s (5 min) | Minimum time between storms |
| `maxTimeBetweenStorms` | 600s (10 min) | Maximum time between storms |
| `minStormDuration` | 60s (1 min) | Minimum storm length |
| `maxStormDuration` | 120s (2 min) | Maximum storm length |
| `lightningChancePerSecond` | 0.01 (1%) | Lightning strike chance per second |
| `warningTime` | 2s | Warning duration before strike |

## Integration with Existing Systems

### PlayerHealth Integration
- Uses `PlayerHealth.Instance.TakeDamage(999f)` for instant kill
- Respects existing death/respawn mechanics
- Death results in normal stat reset

### UIManager Integration
- Uses `UIManager.Instance.ShowLootNotification()` for all messages
- Follows existing notification styling

### WeatherSystem Compatibility
- Runs independently of regular rain system
- Both can be active simultaneously
- Shares sun/ambient light references

### GameCache Integration
- Uses `GameCache.IsPlayerValid()` for safe player reference checks
- Uses `GameCache.Player.position` for location detection

## Performance Considerations

- Audio clips are generated once and cached
- Textures are created at start and destroyed on cleanup
- Lightning checks only occur during active storms
- Visual lightning flashes use coroutines for efficiency

## Future Enhancement Ideas

1. **Storm Intensity Variations**: Stronger storms with higher lightning frequency
2. **Regional Storms**: Different storm types per realm (tropical vs arctic)
3. **Storm Warnings**: Weather forecast system to warn players
4. **Lightning Rod**: Craftable item to protect player on dock
5. **Storm Achievements**: "Survived 10 storms" etc.
6. **Dynamic Fish Spawns**: Rare fish only appear during storms
7. **Rain Visual Particles**: Add rain drops to match audio (currently only sky effects)

## Known Limitations

1. No rain particle effects (only audio/lighting changes)
2. Lightning strikes are instant kill (no damage reduction possible)
3. Storms affect all realms equally (no regional variation)
4. No way to predict or avoid storms besides staying on land

## Troubleshooting

### Storm not starting
- Check console for "Next storm in X seconds" message
- Verify `MainMenu.GameStarted` is true
- Check that ThunderstormSystem exists in scene hierarchy

### Lightning not striking
- Must be on dock during storm
- Check player position matches dock coordinates
- Verify `IsPlayerOnDock()` returns true

### No audio
- Check that AudioSource components are created
- Verify procedural audio clip generation succeeded
- Check system audio volume settings

### Warning not showing
- Ensure player is on dock when warning triggers
- Check GUI rendering in OnGUI()
- Verify textures are created

## Code Quality Notes

- Follows existing game coding style
- Uses singleton pattern like other systems
- Properly cleaned up resources in OnDestroy()
- Well-commented for maintainability
- Procedural audio avoids asset dependencies

## Developer Console Reference

For complete console command documentation, see:
- **`CONSOLE_CHEAT_SHEET.md`** - Quick reference for all commands
- **`CONSOLE_COMMANDS.md`** - Detailed command documentation
- **`CONSOLE_SETUP_QUICK_START.md`** - Setup guide

Key Storm Commands:
- `storm` or `lightning` - Start storm immediately
- `endstorm` - Stop storm immediately
- `time [0-24]` - Set time of day for testing different lighting conditions
- `heal` - Recover after testing lightning strikes
- `tp [x] [y] [z]` - Teleport to specific dock locations for testing
