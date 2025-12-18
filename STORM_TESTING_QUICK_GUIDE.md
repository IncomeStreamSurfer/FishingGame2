# Thunderstorm Testing - Quick Guide

Quick reference for testing the thunderstorm system using console commands.

## Open Console
Press any of these keys:
- **~** (tilde/backtick)
- **F1**
- **F12**

## Essential Storm Commands

### Start/Stop Storm
```
storm       - Start thunderstorm immediately
endstorm    - Stop storm immediately
```

### Time Control
```
time 0      - Midnight (test storms in darkness)
time 6      - Sunrise (test dawn storms)
time 12     - Noon (test day storms)
time 18     - Sunset (test dusk storms)
time 20     - Evening (test twilight storms)
```

### Shortcuts
```
day         - Quick noon
night       - Quick midnight
sunrise     - Quick 6 AM
sunset      - Quick 6 PM
```

### Utility
```
heal        - Restore health after lightning strike
clear       - Clear console output
help        - Show all commands
```

## Testing Scenarios

### 1. Basic Storm Test
```
> storm
[Storm starts immediately]
> endstorm
[Storm stops]
```

### 2. Day vs Night Storm
```
> time 12
> storm
[Test storm in daylight]

> endstorm
> time 0
> storm
[Test storm at night]
```

### 3. Lightning Strike Test
```
> storm
[Walk onto the dock]
[Wait for lightning warning]
[Move to land to cancel OR stay to get struck]
> heal
[Heal after being struck]
> endstorm
```

### 4. Different Times of Day
```
> time 5.5
> storm
[Watch sunrise during storm]

> endstorm
> time 17.5
> storm
[Watch sunset during storm]
```

### 5. Rapid Testing Workflow
```
> clear
> time 12
> storm
[Test specific feature]
> endstorm
> heal
```

## Dock Locations for Lightning Testing

Teleport to these locations to test lightning strikes:

### Main Tropical Dock
```
> tp -12 3 30
[Center of main dock]
```

### Ice Realm Dock
```
> tp 500 3 45
[Center of ice dock]
```

### Jungle Realm Dock
```
> tp 988 3 30
[Center of jungle dock]
```

### Bridge to Goldie's Island
```
> tp 25 2 50
[Center of bridge]
```

## Complete Test Session Example

```bash
# Open console with ~ or F1 or F12

# Clear previous output
> clear

# Test storm at noon
> time 12
> storm

# Teleport to main dock
> tp -12 3 30

# Wait for lightning warning (should appear within 10-20 seconds)
# Move to land when warning shows to cancel strike

# Test storm at night
> endstorm
> time 0
> storm

# Stay on dock to get struck by lightning
# [Wait for warning and strike]

# Heal after being struck
> heal

# End storm
> endstorm

# Test sunrise storm
> time 6
> storm

# Watch effects
# [Storm during sunrise is beautiful]

> endstorm
```

## What to Look For

### Visual Effects
- Sky darkens from blue to gray
- Sun dims significantly
- Random white flashes (visual lightning)
- Full white flash when struck

### Audio Effects
- Deep rolling thunder (looping)
- Heavy rain sounds (looping)
- Sharp lightning crack when struck
- Distant thunder after visual flashes

### UI Elements
- "THUNDERSTORM (XXs)" at top of screen
- Storm start notification: "Storm approaching..."
- Storm end notification: "Storm passing..."
- Lightning warning: Red/yellow pulsing box with "!" icon
- Death message: "ZAP! You're fried. Killed by lightning."

### Gameplay Mechanics
- Lightning only strikes when on dock/bridge
- 2-second warning before strike
- Warning cancels if you reach land
- Strike = instant death (999 damage)
- Normal respawn after lightning death

## Troubleshooting

### Storm won't start
```
> storm
ERROR: ThunderstormSystem not found in scene
```
**Fix**: Add ThunderstormSystem to the scene (should be auto-added by AutoSetup.cs)

### Time won't change
```
> time 12
ERROR: DayNightCycle not found in scene
```
**Fix**: Ensure DayNightCycle system exists in scene

### Console won't open
- Try all three keys: `~`, `F1`, `F12`
- Check ConsoleCommands GameObject exists in Hierarchy
- Verify script is attached to GameObject
- Make sure you're in Play mode

## Pro Tips

1. **Use Up Arrow**: Quickly repeat last command
2. **Clear Often**: Type `clear` before each test for clean output
3. **Heal Before Testing**: Use `heal` before testing lightning strikes
4. **Time Shortcuts**: Use `day`, `night`, `sunrise`, `sunset` instead of typing time values
5. **Command Aliases**: Both `storm` and `lightning` work the same way

## Testing Checklist

Test each of these scenarios:

- [ ] Storm starts with `storm` command
- [ ] Storm stops with `endstorm` command
- [ ] Storm visible during day (`time 12`)
- [ ] Storm visible at night (`time 0`)
- [ ] Lightning warning appears on dock
- [ ] Warning cancels when moving to land
- [ ] Lightning strikes and kills player on dock
- [ ] Player respawns normally after lightning death
- [ ] Storm effects (audio/visual) work properly
- [ ] Time commands work (`day`, `night`, `time X`)

## Quick Reference Card

```
┌──────────────────┬────────────────────────────────┐
│ Command          │ Effect                         │
├──────────────────┼────────────────────────────────┤
│ ~ or F1 or F12   │ Toggle console                 │
│ storm            │ Start thunderstorm             │
│ endstorm         │ Stop thunderstorm              │
│ time 12          │ Set to noon                    │
│ time 0           │ Set to midnight                │
│ day              │ Quick noon                     │
│ night            │ Quick midnight                 │
│ heal             │ Restore health                 │
│ tp X Y Z         │ Teleport to position           │
│ clear            │ Clear console                  │
│ help             │ Show all commands              │
└──────────────────┴────────────────────────────────┘
```

## Need More Help?

- Full command list: Type `help` in console
- Detailed docs: See `CONSOLE_COMMANDS.md`
- Setup guide: See `CONSOLE_SETUP_QUICK_START.md`
- Complete reference: See `CONSOLE_CHEAT_SHEET.md`
- Storm system details: See `THUNDERSTORM_SYSTEM_GUIDE.md`

---

**Happy Testing!** The console makes testing storms much faster than waiting 5-10 minutes between natural storms.
