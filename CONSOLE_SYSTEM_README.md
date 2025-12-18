# Developer Console System - Complete Package

A comprehensive in-game console for testing and debugging Fish or Die game mechanics.

## What's Included

### Core System
- **`Assets/Scripts/ConsoleCommands.cs`** - Main console system (721 lines)
  - Command parsing and execution
  - GUI rendering
  - Command history
  - Integration with all major game systems

### Editor Tools
- **`Assets/Editor/ConsoleSetup.cs`** - Unity Editor integration
  - Menu items: `Tools > Add Developer Console to Scene`
  - Quick documentation access
  - One-click scene setup

### Documentation
- **`CONSOLE_COMMANDS.md`** - Complete command reference (detailed)
- **`CONSOLE_SETUP_QUICK_START.md`** - 2-minute setup guide
- **`CONSOLE_CHEAT_SHEET.md`** - Quick reference card
- **`CONSOLE_SYSTEM_README.md`** - This file

## Quick Start (30 Seconds)

1. In Unity, go to: **Tools > Add Developer Console to Scene**
2. Press Play
3. Press **~** or **F1** or **F12**
4. Type `help` and press Enter

Done! Console is ready to use.

## Features

### Command Categories
✅ **Weather Control**
- Trigger/end thunderstorms instantly
- Test storm mechanics without waiting

✅ **Time Control**
- Set exact time of day (0-24 hours)
- Quick shortcuts: day, night, sunrise, sunset
- Test day/night transitions

✅ **Player Management**
- Heal/kill player
- Give coins/gold
- Teleport anywhere
- Test death/respawn system

✅ **Utility**
- Command history (Up/Down arrows)
- Auto-completing command recall
- Clear console output
- Help system

### User Experience
- 🎨 Color-coded output (green=commands, red=errors, yellow=headers)
- 📜 Scrollable output log (last 20 lines)
- ⌨️ Command history (last 50 commands)
- 🎯 Auto-focus input field
- 🖥️ Responsive UI that scales with screen size
- 🔍 Clear error messages

## All Available Commands

### Weather & Time
| Command | Description |
|---------|-------------|
| `storm` / `lightning` | Trigger thunderstorm |
| `endstorm` | End current storm |
| `time [0-24]` | Set time of day |
| `day` / `noon` | Set to 12:00 PM |
| `night` / `midnight` | Set to 12:00 AM |
| `sunrise` | Set to 6:00 AM |
| `sunset` | Set to 6:00 PM |

### Player
| Command | Description |
|---------|-------------|
| `heal [amount]` | Restore health (full if no amount) |
| `kill` | Kill player instantly |
| `coins [amount]` | Give coins (show balance if no amount) |
| `gold [amount]` | Alias for coins |
| `tp x y z` | Teleport to coordinates |
| `teleport x y z` | Alias for tp |
| `spawn` | Teleport to spawn point |
| `god` | Toggle god mode (not fully implemented) |
| `speed [mult]` | Set movement speed (not implemented) |

### Utility
| Command | Description |
|---------|-------------|
| `help` | Show all commands |
| `clear` | Clear console output |

## Integration Points

The console seamlessly integrates with:

- **ThunderstormSystem** - Start/end storms via reflection
- **DayNightCycle** - Control time of day
- **PlayerHealth** - Heal, damage, kill player
- **GameManager** - Add coins, check balance
- **GameCache** - Access player transform

## Technical Architecture

### Design Patterns
- **Singleton**: Global access via `ConsoleCommands.Instance`
- **Command Pattern**: Dictionary-based command dispatch
- **Observer**: Integrates with existing game systems

### Key Features
- **Reflection**: Calls private methods in game systems
- **DontDestroyOnLoad**: Persists across scene changes
- **Cached Textures**: Optimized GUI rendering
- **Command History**: Ring buffer for last 50 commands

### Performance
- Minimal overhead when closed
- Efficient texture caching
- No GC allocations during command entry
- Only renders when visible

## Usage Examples

### Example 1: Test Storm System
```
> clear
> time 12
> storm
> heal
> spawn
> endstorm
```

### Example 2: Test Death/Respawn
```
> coins 10000
> kill
(wait for respawn)
> coins
Current coins: 10000  (coins preserved!)
```

### Example 3: Test Day/Night Cycle
```
> sunrise
(observe sunrise)
> time 12
(observe noon)
> sunset
(observe sunset)
> night
(observe stars)
```

### Example 4: Quick Testing Setup
```
> clear
> coins 100000
> heal
> day
> spawn
Ready for testing!
```

## Adding New Commands

1. Open `Assets/Scripts/ConsoleCommands.cs`

2. Add command to dictionary in `InitializeCommands()`:
```csharp
{ "mycommand", CmdMyCommand },
```

3. Implement handler:
```csharp
void CmdMyCommand(string[] args)
{
    if (args.Length == 0)
    {
        LogOutput("Usage: mycommand [arg]");
        return;
    }

    // Your logic here
    LogOutput("Command executed successfully!");
}
```

4. Add to help text in `CmdHelp()`:
```csharp
LogOutput("  mycommand [arg] - Description here");
```

## Tips & Best Practices

### Testing
1. Always `clear` console before a test for clean logs
2. Use `heal` to prevent death during storm tests
3. Give yourself `coins 100000` for unrestricted testing
4. Use `spawn` to quickly reset position

### Development
1. Check `startVisible = true` in Inspector during dev
2. Use command history (Up arrow) to repeat tests
3. Chain commands in your test scenarios
4. Add custom commands for your specific needs

### Debugging
1. Watch console output for errors
2. Use `time` without args to check current time
3. Use `tp` without args to check position
4. Use `coins` without args to check balance

## Troubleshooting

### Console won't open
- ✅ Check GameObject is active in Hierarchy
- ✅ Verify script is attached to GameObject
- ✅ Try both ~ and F1 keys
- ✅ Check Unity console for errors

### "System not found" errors
- ✅ Ensure game has started (past main menu)
- ✅ Verify required systems exist in scene
- ✅ Check `MainMenu.GameStarted` is true

### Commands not working
- ✅ Type `help` to see available commands
- ✅ Check spelling and arguments
- ✅ Read error messages in console
- ✅ Verify game systems are initialized

## File Locations

```
FishingGame2/
├── Assets/
│   ├── Scripts/
│   │   └── ConsoleCommands.cs          ← Main system
│   └── Editor/
│       └── ConsoleSetup.cs             ← Editor tools
├── CONSOLE_COMMANDS.md                  ← Full reference
├── CONSOLE_SETUP_QUICK_START.md         ← Setup guide
├── CONSOLE_CHEAT_SHEET.md               ← Quick reference
└── CONSOLE_SYSTEM_README.md             ← This file
```

## Version History

**v1.0** - Initial Release
- Core console system
- 20+ commands
- Weather, time, player commands
- Command history
- GUI rendering
- Editor integration
- Complete documentation

## Future Enhancements

Potential additions:
- God mode implementation
- Speed multiplier
- Spawn specific fish
- Toggle UI elements
- Save/load positions
- Weather presets
- Time speed control
- Debug visualization toggles

## Support

### Documentation
1. **Quick Reference**: `CONSOLE_CHEAT_SHEET.md`
2. **Full Guide**: `CONSOLE_COMMANDS.md`
3. **Setup Help**: `CONSOLE_SETUP_QUICK_START.md`

### In-Game Help
- Type `help` for command list
- Press `?` for quick shortcuts (not implemented)
- Check error messages for guidance

## Credits

Created for Fish or Die game testing and development.

- **System**: ConsoleCommands.cs
- **Integration**: ThunderstormSystem, DayNightCycle, PlayerHealth, GameManager
- **Design**: Command pattern with reflection-based integration

---

**Ready to test!** Press ~ or F1 to open the console.
