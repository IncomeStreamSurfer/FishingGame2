# Quick Start: Resolution Manager

## TL;DR

**Recommended Default Resolution: 1920x1080 (16:9)**
- Used by 54.44% of PC gamers (Steam Survey 2025)
- 16:9 aspect ratio covers 74%+ of all PC gamers
- Provides consistent UI presentation

## Setup (2 Minutes)

### Method 1: Unity Editor Menu (Easiest)

1. Open your main scene
2. Go to **Tools > Setup Resolution Manager**
3. Click "Close" or open the guide
4. Done! ResolutionManager is now in your scene

### Method 2: Manual Setup

1. Create empty GameObject named "ResolutionManager"
2. Attach `ResolutionManager.cs` script
3. Set these values in Inspector:
   - Target Resolution: **1920x1080**
   - Enforce Aspect Ratio: **✓ Enabled**
   - Allow Fullscreen: **✓ Enabled**
   - Debug Mode: **✗ Disabled** (enable to see logs)

## What It Does

✅ Sets game to 1920x1080 by default
✅ Enforces 16:9 aspect ratio with letterboxing/pillarboxing
✅ Provides helpers for UI scripts to scale properly
✅ Handles window resizing gracefully

## Migration Cheat Sheet

Your game uses OnGUI/IMGUI extensively. Here's how to update it:

### Replace Screen.width/height

```csharp
// OLD → NEW
Screen.width                  → ResolutionManager.GetEffectiveScreenWidth()
Screen.height                 → ResolutionManager.GetEffectiveScreenHeight()

// For positions, also add viewport offset:
x = Screen.width / 2          → x = ResolutionManager.GetViewportOffsetX() +
                                    ResolutionManager.GetEffectiveScreenWidth() / 2

y = Screen.height - 100       → y = ResolutionManager.GetViewportOffsetY() +
                                    ResolutionManager.GetEffectiveScreenHeight() - 100
```

### Common Patterns

**Centered Panel:**
```csharp
// OLD
float x = (Screen.width - panelWidth) / 2;
float y = (Screen.height - panelHeight) / 2;

// NEW (Easy way)
Rect panel = ResolutionManager.GetCenteredRect(panelWidth, panelHeight);
```

**Fullscreen Overlay:**
```csharp
// OLD
GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), tex);

// NEW
GUI.DrawTexture(new Rect(
    ResolutionManager.GetViewportOffsetX(),
    ResolutionManager.GetViewportOffsetY(),
    ResolutionManager.GetEffectiveScreenWidth(),
    ResolutionManager.GetEffectiveScreenHeight()
), tex);
```

**Top-Right Corner:**
```csharp
// OLD
float x = Screen.width - 110;
float y = 35;

// NEW
float x = ResolutionManager.GetViewportOffsetX() +
          ResolutionManager.GetEffectiveScreenWidth() - 110;
float y = ResolutionManager.GetViewportOffsetY() + 35;
```

## Testing

In Unity Editor:
- **Tools > Test Resolutions > 1920x1080** (most common)
- **Tools > Test Resolutions > 2560x1440** (high-res)
- **Tools > Test Resolutions > 1280x720** (low-res)
- **Tools > Test Resolutions > Print Debug Info**

## Files to Update (Optional)

High priority UI files that use Screen.width/height:
- `UIManager.cs` - Main UI manager
- `FishInventoryPanel.cs` - Fish inventory
- `MainMenu.cs` - Main menu
- `PauseMenu.cs` - Pause menu
- `CharacterPanel.cs` - Character panel

See `RESOLUTION_GUIDE.md` for complete list and detailed examples.

## Example Files

- `ResolutionManager.cs` - The main script (already working, just add to scene)
- `ResolutionManagerExample.cs` - Before/after code examples
- `PicketSignText_Migrated_Example.cs` - Real migration example
- `RESOLUTION_GUIDE.md` - Complete documentation

## Do I Have To Migrate Everything?

**No!** The ResolutionManager works immediately:
- It sets the default resolution to 1920x1080
- It adds letterboxing/pillarboxing for aspect ratio enforcement
- Your existing UI will work, but might not be perfectly centered on non-16:9 screens

**Migrating UI scripts is optional** but recommended for:
- Perfect centering on all screen sizes
- Scaling for different resolutions
- Better support for ultrawide/portrait displays

Start with high-priority files (menus, inventory) and migrate others as needed.

## Support

Questions? See:
- `RESOLUTION_GUIDE.md` - Full documentation
- `ResolutionManagerExample.cs` - Code examples
- Steam Hardware Survey - [Resolution data](https://store.steampowered.com/hwsurvey/resolution/)

---

**Remember:** 1920x1080 @ 16:9 is the standard. You're good to go!
