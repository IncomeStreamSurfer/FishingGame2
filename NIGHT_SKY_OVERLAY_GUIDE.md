# Night Sky Overlay System - True Black Night Sky

## Problem Solved

The sky was appearing BLUE at night instead of BLACK. This happened because Unity's procedural skybox shader (`Skybox/Procedural`) always generates atmospheric scattering, preventing true black even when black colors are set.

## Solution

The **NightSkyOverlay** system creates a large black dome that fades in/out based on time of day, ensuring the sky is TRULY BLACK at night while keeping stars and moon visible.

---

## How It Works

### The Rendering Stack (Back to Front)
1. **Procedural Skybox** (render queue 2000) - Blue sky with atmospheric effects
2. **NightSkyOverlay Black Dome** (render queue 2600) - Black overlay that fades in at night
3. **Moon** (render queue 2750) - Visible against black sky
4. **Stars** (render queue 2800) - Visible against black sky
5. **Clouds** (render queue 2900) - In front of everything

### The Blending Schedule

- **6 AM - 8 AM (Sunrise)**: Black fades from opaque to transparent using smooth interpolation
- **8 AM - 6 PM (Day)**: Fully transparent - blue procedural sky is visible
- **6 PM - 8 PM (Sunset)**: Black fades from transparent to opaque
- **8 PM - 6 AM (Night)**: Fully opaque black - creates dramatic starlight and moonlight

---

## Automatic Setup

The `NightSkyOverlaySetup.cs` editor script **automatically** creates and configures the overlay when Unity loads or recompiles scripts.

### What It Does:
1. Detects if `DayNightCycle` exists in the scene
2. Creates a `NightSkyOverlay` GameObject
3. Configures it with proper settings
4. Parents it to `DayNightCycle` for organization

---

## Manual Setup (if needed)

### Option 1: Unity Menu
1. Go to **Tools > Sky System > Add Night Sky Overlay**
2. The component will be created and configured automatically

### Option 2: Manual Creation
1. Create an empty GameObject in your scene
2. Name it "NightSkyOverlay"
3. Add the `NightSkyOverlay` component
4. Assign the `DayNightCycle` reference in the Inspector
5. Configure settings (or use defaults)

---

## Component Settings

### Integration
- **Day Night Cycle**: Reference to the DayNightCycle component (required)

### Overlay Settings
- **Dome Distance**: How far the black dome is from center (default: 150)
- **Night Black Color**: The color of the night overlay (default: pure black RGB 0,0,0)

### Transition Times
- **Sunrise Start Hour**: When black starts fading out (default: 6 AM)
- **Sunrise End Hour**: When black is fully transparent (default: 8 AM)
- **Sunset Start Hour**: When black starts fading in (default: 6 PM)
- **Sunset End Hour**: When black is fully opaque (default: 8 PM)

---

## Public API

### Methods

```csharp
// Get current opacity of the night overlay (0-1)
float alpha = NightSkyOverlay.Instance.GetNightOverlayAlpha();

// Check if night overlay is active
bool isActive = NightSkyOverlay.Instance.IsNightOverlayActive();

// Manually set overlay alpha (for testing/debugging)
NightSkyOverlay.Instance.SetOverlayAlpha(0.5f);

// Change the black color (advanced customization)
NightSkyOverlay.Instance.SetBlackColor(new Color(0.05f, 0.0f, 0.1f));
```

---

## Technical Details

### Material Configuration
- **Shader**: Standard (Transparent mode)
- **Blend Mode**: SrcAlpha / OneMinusSrcAlpha (standard alpha blending)
- **Render Queue**: 2600 (after skybox, before celestial objects)
- **Z Write**: Disabled (allows objects to render through it)

### Performance Optimization
- The dome GameObject is automatically disabled when fully transparent (alpha < 0.001)
- Uses a single material instance
- Minimal per-frame overhead (just alpha updates)

### Smooth Transitions
- Uses `Mathf.SmoothStep()` for fade transitions
- Creates natural-looking sunrise/sunset blending
- No visible popping or harsh transitions

---

## Integration with Existing Systems

### Works With:
- **DayNightCycle.cs**: Uses time-of-day data for blending
- **SkyboxManager.cs**: Renders over the procedural skybox
- **Stars & Moon**: Render in front of the black overlay

### Render Queue Hierarchy:
```
2000: Procedural Skybox (blue sky)
2600: Night Black Overlay ← NEW
2750: Moon
2800: Stars
2900: Clouds
3000: Sun glow
```

---

## Customization Examples

### Make Night Sky Deep Blue Instead of Black
```csharp
NightSkyOverlay.Instance.SetBlackColor(new Color(0.02f, 0.02f, 0.1f));
```

### Faster Sunrise (1 hour instead of 2)
```csharp
// In Inspector or code:
overlay.sunriseStartHour = 6.5f;
overlay.sunriseEndHour = 7.5f;
```

### Longer Night (black until 7 AM)
```csharp
overlay.sunriseStartHour = 7f;
overlay.sunriseEndHour = 9f;
```

---

## Troubleshooting

### Sky still looks blue at night
1. Check that `NightSkyOverlay` component exists in the scene
2. Verify `dayNightCycle` reference is assigned in Inspector
3. Check current time using the on-screen time display
4. Make sure the black dome GameObject is active
5. Check material render queue is 2600 (should be automatic)

### Stars/Moon not visible
1. The overlay might be rendering IN FRONT of stars
2. Check render queues: Moon=2750, Stars=2800, Black Dome should be 2600
3. If using custom materials, ensure stars/moon have higher render queue values

### Harsh transitions
1. Increase the transition duration by adjusting sunrise/sunset time ranges
2. Example: `sunriseStartHour = 5f` and `sunriseEndHour = 9f` for 4-hour sunrise

### Dome too small/large
1. Adjust `domeDistance` in Inspector
2. Should be larger than all scene objects but smaller than far clip plane
3. Default 150 units works for most scenes

---

## Removal

If you want to remove the Night Sky Overlay:

### Option 1: Unity Menu
1. Go to **Tools > Sky System > Remove Night Sky Overlay**
2. Confirm the removal

### Option 2: Manual
1. Find the `NightSkyOverlay` GameObject in the Hierarchy
2. Delete it

**Note**: After removal, the sky will revert to the procedural skybox's natural behavior (blue-ish at night).

---

## Files Created

### Runtime Scripts:
- `Assets/Scripts/NightSkyOverlay.cs` - Main overlay component
- `Assets/Scripts/NightSkyOverlay.cs.meta` - Unity metadata

### Editor Scripts:
- `Assets/Editor/NightSkyOverlaySetup.cs` - Automatic setup system
- `Assets/Editor/NightSkyOverlaySetup.cs.meta` - Unity metadata

### Documentation:
- `NIGHT_SKY_OVERLAY_GUIDE.md` - This file

---

## Benefits

1. **True Black Sky**: Guaranteed pure black at night (no blue tint)
2. **Visible Celestial Objects**: Stars and moon render correctly against black
3. **Smooth Transitions**: Natural sunrise/sunset blending
4. **Easy Customization**: Inspector controls for all settings
5. **Automatic Setup**: Zero manual configuration required
6. **Performance Friendly**: Minimal overhead, auto-disables when not needed
7. **Non-Destructive**: Doesn't modify existing SkyboxManager or DayNightCycle

---

## Credits

Created to solve the blue night sky issue in the Fishing Game 2 project.

**System Design**: Black overlay dome with render queue management
**Implementation**: NightSkyOverlay component with automatic editor setup
**Integration**: Works seamlessly with existing DayNightCycle and SkyboxManager
