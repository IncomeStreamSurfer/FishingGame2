# Resolution & UI Scaling Guide for PC Play

## Executive Summary

This game now supports optimized PC play with a **fixed 16:9 aspect ratio** and proper UI scaling. The recommended default resolution is **1920x1080 (Full HD)**, which is used by 54.44% of PC gamers according to the December 2025 Steam Hardware Survey.

## Recommended Default Resolution

**1920x1080 (16:9 aspect ratio)**

### Why 1920x1080?

Based on Steam Hardware Survey data from 2025:

- **54.44%** of PC gamers use 1920x1080 (Full HD)
- **20.19%** use 2560x1440 (2K)
- **5.09%** use 2560x1600 (16:10 ultrawide)
- **4.49%** use 3840x2160 (4K)

The 16:9 aspect ratio (1920x1080, 2560x1440, 1280x720, etc.) covers **74%+ of all PC gamers**, making it the clear standard.

### Other Common Resolutions Supported

- **2560x1440** (2K) - Second most common, also 16:9
- **1280x720** (HD) - Budget systems, also 16:9
- **3840x2160** (4K) - High-end systems, also 16:9

All these resolutions share the same 16:9 aspect ratio, ensuring consistent UI presentation.

## What Has Been Implemented

### 1. ResolutionManager.cs

A comprehensive resolution management system that:

- ✅ Sets default resolution to 1920x1080 (configurable)
- ✅ Enforces 16:9 aspect ratio with letterboxing/pillarboxing
- ✅ Provides UI scaling helpers for consistent presentation
- ✅ Handles window resizing gracefully
- ✅ Supports fullscreen and windowed modes
- ✅ Adds black bars (letterboxing/pillarboxing) for non-16:9 displays

### 2. Letterboxing & Pillarboxing

**Letterboxing** (black bars on top/bottom):
- Occurs when the screen is taller than 16:9 (e.g., 4:3, 5:4 displays)
- Maintains the intended game view without stretching

**Pillarboxing** (black bars on left/right):
- Occurs when the screen is wider than 16:9 (e.g., 21:9 ultrawide)
- Prevents UI from spreading too wide

### 3. UI Scaling System

The ResolutionManager provides helper methods to replace direct `Screen.width`/`Screen.height` usage:

```csharp
// Instead of this:
float x = Screen.width / 2;
float y = Screen.height / 2;

// Use this:
float x = ResolutionManager.GetEffectiveScreenWidth() / 2 + ResolutionManager.GetViewportOffsetX();
float y = ResolutionManager.GetEffectiveScreenHeight() / 2 + ResolutionManager.GetViewportOffsetY();

// Or for centered elements, use the convenience method:
Rect centered = ResolutionManager.GetCenteredRect(400, 300);
```

## How to Use ResolutionManager

### Setup (Required)

1. **Add ResolutionManager to your main scene:**
   - Create an empty GameObject in your scene
   - Name it "ResolutionManager"
   - Attach the `ResolutionManager.cs` script to it
   - The script is already set to persist across scenes (DontDestroyOnLoad)

2. **Configure settings in Inspector:**
   - **Target Resolution**: Set to 1920x1080 (default)
   - **Enforce Aspect Ratio**: Keep enabled (recommended)
   - **Allow Fullscreen**: Enable for fullscreen support
   - **Debug Mode**: Enable to see resolution info in console

### Migration Guide for Existing UI Scripts

Your codebase has **extensive use of Screen.width/height** in UI scripts. Here's how to migrate:

#### Pattern 1: Centered Panels

**Before:**
```csharp
float panelX = (Screen.width - panelWidth) / 2;
float panelY = (Screen.height - panelHeight) / 2;
GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "");
```

**After:**
```csharp
Rect panelRect = ResolutionManager.GetCenteredRect(panelWidth, panelHeight);
GUI.Box(panelRect, "");
```

#### Pattern 2: Fullscreen Overlays

**Before:**
```csharp
GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);
```

**After:**
```csharp
GUI.DrawTexture(new Rect(
    ResolutionManager.GetViewportOffsetX(),
    ResolutionManager.GetViewportOffsetY(),
    ResolutionManager.GetEffectiveScreenWidth(),
    ResolutionManager.GetEffectiveScreenHeight()
), overlayTexture);
```

#### Pattern 3: Anchored to Corners

**Before (top-right):**
```csharp
GUI.Label(new Rect(Screen.width - 110, 35, 100, 25), "Time", style);
```

**After:**
```csharp
float x = ResolutionManager.GetViewportOffsetX() + ResolutionManager.GetEffectiveScreenWidth() - 110;
float y = ResolutionManager.GetViewportOffsetY() + 35;
GUI.Label(new Rect(x, y, 100, 25), "Time", style);
```

#### Pattern 4: Relative Positioning

**Before:**
```csharp
float promptY = Screen.height * 0.7f;
GUI.Label(new Rect(0, promptY, Screen.width, 30), "Press E", style);
```

**After:**
```csharp
float promptY = ResolutionManager.GetViewportOffsetY() + ResolutionManager.GetEffectiveScreenHeight() * 0.7f;
float promptX = ResolutionManager.GetViewportOffsetX();
GUI.Label(new Rect(promptX, promptY, ResolutionManager.GetEffectiveScreenWidth(), 30), "Press E", style);
```

#### Pattern 5: Font Scaling (Optional but Recommended)

**Before:**
```csharp
GUIStyle style = new GUIStyle();
style.fontSize = 24;
```

**After (scales with resolution):**
```csharp
GUIStyle style = new GUIStyle();
style.fontSize = Mathf.RoundToInt(24 * ResolutionManager.GetScaleFactor());
```

### Scripts That Need Updating

Based on the codebase analysis, these scripts use `Screen.width/height` and should be updated:

**High Priority (UI-heavy):**
- UIManager.cs
- FishInventoryPanel.cs
- MainMenu.cs
- PauseMenu.cs
- CharacterPanel.cs
- FishDiary.cs
- ClothingShopNPC.cs
- FoodInventory.cs

**Medium Priority (NPC/Shop dialogs):**
- GoldieBanksNPC.cs
- IceRealmShopNPC.cs
- JungleShopNPC.cs
- WeaponShopNPC.cs
- OrangutanVendor.cs
- BjorkHuntsman.cs
- ChefNPC.cs
- FishConnoisseurNPC.cs

**Lower Priority (prompts/notifications):**
- BBQStation.cs
- CandyCat.cs
- DockRadio.cs
- FrostZoneTip.cs
- JungleTip.cs
- ReadableSign.cs
- PortalInteraction.cs

**Special Cases:**
- DraggableWindow.cs - Needs viewport clamping
- PlayerController.cs - Death screen overlay
- PlayerHealth.cs - Health UI and death screen
- FishingSystem.cs - Fishing UI popups
- FishingRodAnimator.cs - Fishing meter

## Testing Checklist

After implementing ResolutionManager and updating UI scripts:

- [ ] Test at 1920x1080 (primary target)
- [ ] Test at 2560x1440 (common high-res)
- [ ] Test at 1280x720 (common low-res)
- [ ] Test fullscreen mode
- [ ] Test windowed mode
- [ ] Test window resizing behavior
- [ ] Verify letterboxing appears on 4:3 displays
- [ ] Verify pillarboxing appears on 21:9 ultrawide displays
- [ ] Verify all UI elements are properly centered
- [ ] Verify no UI elements are cut off
- [ ] Verify font sizes are readable at all resolutions

## Advanced Features

### Runtime Resolution Changes

```csharp
// Change to 1440p
ResolutionManager.Instance.SetResolution1440p();

// Change to 720p
ResolutionManager.Instance.SetResolution720p();

// Custom resolution
ResolutionManager.Instance.SetResolution(1600, 900, true);
```

### Debug Information

Enable "Debug Mode" in the ResolutionManager inspector, or call:

```csharp
ResolutionManager.Instance.PrintResolutionInfo();
```

Output shows:
- Current screen size
- Target resolution
- Reference resolution
- Scale factor
- Effective viewport size
- Letterboxing/pillarboxing status

### Helper Methods Reference

```csharp
// Get scaled dimensions
ResolutionManager.GetEffectiveScreenWidth()   // Width accounting for pillarboxing
ResolutionManager.GetEffectiveScreenHeight()  // Height accounting for letterboxing

// Get viewport offsets (black bar sizes)
ResolutionManager.GetViewportOffsetX()        // Left edge offset
ResolutionManager.GetViewportOffsetY()        // Top edge offset

// Scale values based on resolution
ResolutionManager.GetScaleFactor()            // Current scale vs. 1920x1080
ResolutionManager.Scale(100)                  // Scale a specific value

// Position helpers
ResolutionManager.GetCenteredRect(w, h)       // Get centered rect
ResolutionManager.ScreenToViewport(pos)       // Convert screen to viewport
ResolutionManager.ViewportToScreen(pos)       // Convert viewport to screen

// Constants
ResolutionManager.REFERENCE_RESOLUTION        // Vector2(1920, 1080)
ResolutionManager.TARGET_ASPECT_RATIO         // 16/9 = 1.777...
```

## Why This Approach?

### Unity UGUI Canvas Scaler Limitations

Unity's built-in Canvas Scaler only works with Unity UI (uGUI), but this game uses **OnGUI/IMGUI** for all UI elements. OnGUI requires manual scaling and positioning, which is why ResolutionManager was created.

### Benefits of This System

1. **Consistent Presentation**: UI looks the same across all 16:9 resolutions
2. **No Stretching**: Aspect ratio enforcement prevents distorted UI
3. **Future-Proof**: Easy to support new resolutions (8K, etc.)
4. **Flexible**: Can disable aspect ratio enforcement if needed
5. **Performance**: Minimal overhead, black bars are just simple texture draws

## Aspect Ratio Statistics

From Steam Hardware Survey (2025):

- **16:9**: ~74% of users (most common)
- **16:10**: ~6% of users (growing, gaming laptops)
- **21:9**: ~2% of users (ultrawide monitors)
- **Other**: ~18% (4:3, 5:4, custom)

By targeting 16:9, you cover the vast majority of PC gamers while providing a graceful fallback (letterboxing/pillarboxing) for other aspect ratios.

## Next Steps

1. **Add ResolutionManager to your main scene** (required)
2. **Optionally migrate existing UI scripts** using the patterns above
3. **Test at different resolutions** to verify everything works
4. **Consider adding resolution options** to your settings menu

## Example Implementation

See `ResolutionManagerExample.cs` for complete before/after code examples and a quick reference guide.

## Sources

- [Steam Hardware & Software Survey: November 2025](https://store.steampowered.com/hwsurvey/)
- [Steam Hardware Survey Resolution Data](https://store.steampowered.com/hwsurvey/resolution/?platform=pc)
- Unity Documentation on Screen Space UI
