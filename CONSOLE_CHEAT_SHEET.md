# Developer Console - Quick Reference Cheat Sheet

## Opening Console
```
~  or  F1  or  F12  →  Toggle console
ESC                 →  Close console
ENTER               →  Run command
↑ ↓                 →  Command history
```

## Most Used Commands

### Weather
| Command | Effect |
|---------|--------|
| `storm` | Start thunderstorm NOW |
| `endstorm` | Stop storm NOW |

### Time Control
| Command | Time | Description |
|---------|------|-------------|
| `time 6` | 6:00 AM | Sunrise |
| `time 12` | 12:00 PM | Noon |
| `time 18` | 6:00 PM | Sunset |
| `time 0` | 12:00 AM | Midnight |
| `day` | 12:00 PM | Quick noon |
| `night` | 12:00 AM | Quick midnight |
| `sunrise` | 6:00 AM | Quick sunrise |
| `sunset` | 6:00 PM | Quick sunset |

### Player
| Command | Effect |
|---------|--------|
| `heal` | Full heal |
| `heal 50` | Heal 50 HP |
| `kill` | Die instantly |
| `coins 1000` | Get 1000 coins |
| `spawn` | Back to spawn |

### Teleport
| Command | Effect |
|---------|--------|
| `tp 10 2 20` | Go to (10, 2, 20) |
| `spawn` | Go to spawn point |
| `tp` | Show current position |

### Utility
| Command | Effect |
|---------|--------|
| `help` | Show all commands |
| `clear` | Clear console |

## Common Test Scenarios

### Test Storm During Day
```
time 12
storm
```

### Test Storm at Night
```
time 0
storm
```

### Test Death/Respawn
```
coins 5000
kill
(wait for respawn)
(verify coins kept)
```

### Test Low Health
```
heal 5
(see warning)
heal
```

### Quick Money for Testing
```
coins 100000
```

### Test Time Transitions
```
time 5.5
(watch sunrise)
time 17.5
(watch sunset)
```

### Perfect Storm Testing
```
clear
time 20
storm
heal
coins 10000
```

## Pro Tips

1. **Use Up Arrow**: Quickly repeat last command
2. **Chain Commands**: Clear console before tests for clean logs
3. **Time Values**: Use decimals like `time 6.5` for 6:30 AM
4. **Quick Heal**: Just type `h` then `↑` to repeat
5. **Money Shortcut**: `gold` works same as `coins`

## Command Aliases

| Long | Short | Description |
|------|-------|-------------|
| `lightning` | `storm` | Both trigger storm |
| `teleport` | `tp` | Both teleport |
| `gold` | `coins` | Both give money |
| `midnight` | `night` | Both set to 0:00 |
| `noon` | `day` | Both set to 12:00 |

## Testing Checklist

### Storm System
- [ ] `storm` - Storm starts
- [ ] `time 12` - Storm visible in daylight
- [ ] `time 0` - Storm visible at night
- [ ] Stand on dock during storm
- [ ] Lightning warning appears
- [ ] Move to land (warning cancels)
- [ ] `endstorm` - Storm ends

### Day/Night Cycle
- [ ] `sunrise` - Sun rises
- [ ] `noon` - Sun at peak
- [ ] `sunset` - Sun sets
- [ ] `night` - Dark with stars
- [ ] `time 6.5` - Smooth sunrise transition
- [ ] `time 18.5` - Smooth sunset transition

### Player Health
- [ ] `heal 5` - Low health warning
- [ ] `heal` - Full heal
- [ ] `kill` - Death screen
- [ ] Respawn works
- [ ] `coins 5000` then `kill` - Coins persist

### Economy
- [ ] `coins` - Show current amount
- [ ] `coins 1000` - Add money
- [ ] Buy something
- [ ] `coins` - Verify amount changed

## Color Legend

In the console output:
- 🟢 **Bright Green** = Your commands
- 🔴 **Red** = Errors
- 🟡 **Yellow** = Section headers
- ⚪ **Gray** = System messages

## Example Session

```
> help
=== DEVELOPER CONSOLE COMMANDS ===
[...]

> time 6
Time set to 6.0 hours (6:00 AM)

> storm
Thunderstorm triggered!

> coins 5000
Added 5000 coins. Total: 5000

> heal
Player fully healed!

> clear
Console cleared.
```

## Keyboard Shortcuts Summary

```
┌──────────────────┬──────────────────────┐
│ Key              │ Action               │
├──────────────────┼──────────────────────┤
│ ~ or F1 or F12   │ Open/Close Console   │
│ Enter            │ Run Command          │
│ Escape           │ Close Console        │
│ Up Arrow         │ Previous Command     │
│ Down Arrow       │ Next Command         │
└──────────────────┴──────────────────────┘
```

## Need Help?

- Type `help` in console for full command list
- See `CONSOLE_COMMANDS.md` for detailed documentation
- See `CONSOLE_SETUP_QUICK_START.md` for setup instructions

---

**Print this page or keep it on a second monitor while testing!**
