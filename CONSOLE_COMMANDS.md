# Developer Console Commands

A comprehensive debug console for testing and debugging game mechanics in Fish or Die.

## Setup

1. **Script Created**: `Assets/Scripts/ConsoleCommands.cs`
2. **Add to Scene**: Create an empty GameObject named "ConsoleCommands" and attach the ConsoleCommands.cs script
3. **Optional**: Check "Start Visible" in the inspector to show console on game start

## Usage

### Opening/Closing
- Press **~** (tilde/backtick) or **F1** or **F12** to toggle the console
- Press **Escape** to close the console
- Press **Enter** to execute a command

### Command History
- **Up Arrow**: Navigate to previous commands
- **Down Arrow**: Navigate to next commands
- The console remembers your last 50 commands

## Available Commands

### Weather & Environment

#### `storm` or `lightning`
Immediately triggers a thunderstorm.
```
> storm
Thunderstorm triggered!
```

#### `endstorm`
Ends the current storm immediately.
```
> endstorm
Storm ended.
```

#### `time [hour]`
Sets the time of day. Hour must be between 0-24.
```
> time 14
Time set to 14.0 hours (2:00 PM)

> time
Current time: 8.5 hours (8:30 AM)
```

#### `day` or `noon`
Sets time to noon (12:00).
```
> day
Time set to 12.0 hours (12:00 PM)
```

#### `night` or `midnight`
Sets time to midnight (0:00).
```
> night
Time set to 0.0 hours (12:00 AM)
```

#### `sunrise`
Sets time to sunrise (6:00 AM).
```
> sunrise
Time set to 6.0 hours (6:00 AM)
```

#### `sunset`
Sets time to sunset (6:00 PM).
```
> sunset
Time set to 18.0 hours (6:00 PM)
```

### Player Commands

#### `heal [amount]`
Restores player health. Without an amount, fully heals the player.
```
> heal
Player fully healed!

> heal 50
Healed 50 HP
```

#### `kill`
Instantly kills the player (for testing death/respawn).
```
> kill
Player killed.
```

#### `coins [amount]` or `gold [amount]`
Gives the player coins. Without an amount, shows current coins.
```
> coins 1000
Added 1000 coins. Total: 1500

> coins
Current coins: 1500
```

#### `tp [x] [y] [z]` or `teleport [x] [y] [z]`
Teleports player to specified coordinates. Without coordinates, shows current position.
```
> tp 10 2 20
Teleported to 10.0, 2.0, 20.0

> tp
Current position: 0.0, 2.0, -5.0
```

#### `spawn`
Teleports player to spawn point (0, 2, -5).
```
> spawn
Teleported to spawn point (0, 2, -5)
```

#### `god`
Toggle god mode (not fully implemented yet - currently just heals player).
```
> god
God mode not implemented yet. Player healed to full.
```

#### `speed [multiplier]`
Set player movement speed (not implemented yet).
```
> speed 2
Speed command not implemented yet.
```

### Utility Commands

#### `help`
Shows all available commands with descriptions.
```
> help
=== DEVELOPER CONSOLE COMMANDS ===
[Full command list displayed]
```

#### `clear`
Clears the console output log.
```
> clear
Console cleared.
```

## Examples & Testing Scenarios

### Testing Thunderstorms
```
> storm           # Trigger a storm
> time 12         # Set to daytime to see storm effects clearly
> endstorm        # End the storm when done testing
```

### Testing Day/Night Cycle
```
> sunrise         # Start at sunrise
> time 12         # Jump to noon
> sunset          # Jump to sunset
> night           # Jump to night
```

### Testing Death & Respawn
```
> kill            # Kill player
# Wait for respawn...
> coins 5000      # Verify coins persist after death
```

### Testing Low Health Warning
```
> heal 5          # Set health to 5 HP
# Observe low health warning UI
> heal            # Restore to full
```

### Quick Testing Setup
```
> coins 10000     # Give yourself lots of money
> heal            # Full health
> day             # Set to daytime
> spawn           # Go to spawn
```

## Technical Details

### Architecture
- **Singleton Pattern**: `ConsoleCommands.Instance` for global access
- **Reflection**: Uses reflection to call private methods in ThunderstormSystem (StartStorm, EndStorm)
- **Command Dictionary**: Easily extensible command system
- **DontDestroyOnLoad**: Console persists across scene changes

### Integration Points
The console integrates with these game systems:
- **ThunderstormSystem**: Trigger/end storms
- **DayNightCycle**: Control time of day
- **PlayerHealth**: Heal, kill, get health info
- **GameManager**: Add/check coins
- **GameCache**: Access player transform for teleportation

### Adding New Commands

To add a new command, edit `ConsoleCommands.cs`:

1. Add command to dictionary in `InitializeCommands()`:
```csharp
{ "mycommand", CmdMyCommand },
```

2. Implement the command handler:
```csharp
void CmdMyCommand(string[] args)
{
    // Your command logic here
    LogOutput("Command executed!");
}
```

3. Add help text in `CmdHelp()`:
```csharp
LogOutput("  mycommand [args] - Description of command");
```

## Known Limitations

1. **God Mode**: Not fully implemented - currently just heals player
2. **Speed Command**: Placeholder - needs player movement script integration
3. **Storm Reflection**: Uses reflection to access private methods - may break if ThunderstormSystem is refactored

## Tips

- The console auto-focuses the input field when opened
- Command history is limited to last 50 commands
- Output log shows last 20 lines (scrollable)
- Commands are case-insensitive
- The console uses color-coding:
  - **Bright Green**: User input (commands you typed)
  - **Red**: Errors
  - **Yellow**: Section headers
  - **Gray**: Normal output

## Troubleshooting

### "ERROR: ThunderstormSystem not found in scene"
- Make sure ThunderstormSystem is in the scene
- Check that MainMenu.GameStarted is true

### "ERROR: DayNightCycle not found in scene"
- Ensure DayNightCycle is in the scene
- Verify it has initialized properly

### "ERROR: Player not found"
- Make sure the player GameObject exists
- Check that GameCache has been initialized

### Console doesn't open
- Check that ConsoleCommands script is attached to a GameObject
- Verify the GameObject is active in the scene
- Try both ~ and F1 keys
