# Console Commands - Quick Setup Guide

Get the developer console running in your scene in 2 minutes.

## Step 1: Add to Scene (30 seconds)

1. Open your Unity scene (e.g., `fish.unity`)
2. Right-click in Hierarchy
3. Create Empty GameObject
4. Rename it to "ConsoleCommands"
5. Drag `Assets/Scripts/ConsoleCommands.cs` onto the GameObject

## Step 2: Configure (Optional)

In the Inspector for the ConsoleCommands GameObject:

- **Start Visible**: Check this if you want the console to show on game start
  - ✅ Good for testing sessions
  - ❌ Leave unchecked for normal gameplay

## Step 3: Test It!

1. Press Play
2. Press **~** (tilde) or **F1** or **F12**
3. Console should appear!
4. Type `help` and press Enter
5. Try some commands:
   ```
   storm
   time 20
   heal
   coins 1000
   ```

## That's It!

You're ready to test game mechanics. See `CONSOLE_COMMANDS.md` for the full command reference.

## Quick Commands Reference Card

**Must-Know Commands:**
- `help` - Show all commands
- `storm` - Trigger thunderstorm
- `time [hour]` - Set time (0-24)
- `day` / `night` - Quick time shortcuts
- `heal` - Restore health
- `coins [amount]` - Give money
- `spawn` - Teleport to spawn
- `clear` - Clear console

**Shortcuts:**
- `~` or `F1` or `F12` - Open/close console
- `Enter` - Execute command
- `Up/Down` - Navigate command history
- `Esc` - Close console

## Troubleshooting

### Console won't open?
- Check the GameObject is active in Hierarchy
- Try all three keys: `~`, `F1`, and `F12`
- Make sure script is attached to GameObject

### "ThunderstormSystem not found"?
- Storm commands only work if ThunderstormSystem exists in the scene
- Check if the system is set up in your scene

### "Player not found"?
- Make sure game has started (past main menu)
- Verify player GameObject exists in scene

## Advanced: Auto-Setup Script

If you want to auto-add the console to scenes, create this editor script:

**File**: `Assets/Editor/ConsoleSetup.cs`

```csharp
using UnityEngine;
using UnityEditor;

public class ConsoleSetup
{
    [MenuItem("Tools/Add Developer Console to Scene")]
    public static void AddConsoleToScene()
    {
        // Check if already exists
        if (GameObject.Find("ConsoleCommands") != null)
        {
            Debug.LogWarning("ConsoleCommands already exists in scene!");
            return;
        }

        // Create GameObject
        GameObject consoleObj = new GameObject("ConsoleCommands");
        consoleObj.AddComponent<ConsoleCommands>();

        Debug.Log("Developer Console added to scene! Press ~ or F1 or F12 to open.");
    }
}
```

Then use: **Tools > Add Developer Console to Scene** from Unity menu.
