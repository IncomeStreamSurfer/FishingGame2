# Sky Rendering Architecture

## Overview

This document explains how the sky rendering system works and how the components interact.

---

## System Architecture

```
Main Camera (Game View)
    ↓
    [Clear Flags = Skybox] ← THE FIX
    ↓
    Renders in this order:
    ↓
1. Skybox (Background)
   ├─ RenderSettings.skybox
   ├─ Managed by SkyboxManager
   └─ Procedural material with day/night colors
    ↓
2. NightSkyOverlay Black Dome
   ├─ Render Queue: 2100
   ├─ Fades in/out based on time
   └─ Makes night sky truly black
    ↓
3. Clouds
   ├─ Render Queue: 2150
   ├─ Float around the scene
   └─ Change color with time of day
    ↓
4. Stars
   ├─ Render Queue: 2200
   ├─ Emissive spheres
   └─ Twinkle and fade with time
    ↓
5. Moon
   ├─ Render Queue: 2250
   ├─ Emissive sphere with light
   └─ Rises/sets opposite to sun
    ↓
6. Scene Objects (terrain, water, player, etc.)
    ↓
7. Transparent Objects
    ↓
8. UI
```

---

## Component Relationships

```
DayNightCycle (Time System)
    ├─ Tracks current hour (0-24)
    ├─ Updates sun position and directional light
    └─ Provides time info to other systems
         ↓
         ↓ (reads time)
         ↓
    ┌────┴─────┬─────────────────┐
    ↓          ↓                 ↓
SkyboxManager  NightSkyOverlay  WeatherSystem
    │          │
    │          └─ Creates black dome
    │          └─ Fades alpha based on time
    │
    └─ Updates skybox colors
    └─ Creates/manages clouds
    └─ Creates/manages stars
    └─ Creates/manages moon
```

---

## The Bug and The Fix

### Before (Broken):

```
Scene View Camera           Main Camera (Game View)
├─ Clear Flags: Skybox  ✓  ├─ Clear Flags: SolidColor  ✗
└─ Shows: Skybox           └─ Shows: Blue solid color

Result: Scene view works, Game view broken
```

### After (Fixed):

```
Scene View Camera           Main Camera (Game View)
├─ Clear Flags: Skybox  ✓  ├─ Clear Flags: Skybox  ✓
└─ Shows: Skybox           └─ Shows: Skybox

Result: Both views work correctly
```

---

## Render Order Details

### Render Queue Numbers:

| Queue | Purpose | Component |
|-------|---------|-----------|
| 1000-1999 | Geometry | Opaque objects |
| 2000 | Skybox | Unity's skybox |
| 2100 | Night Overlay | Black dome |
| 2150 | Clouds | Cloud spheres |
| 2200 | Stars | Star spheres |
| 2250 | Moon | Moon sphere |
| 3000+ | Transparent | Water, effects |

**Key Point**: Higher queue numbers render LATER (on top of lower numbers)

---

## Time-Based Sky States

### Daytime (8 AM - 6 PM):
```
Skybox: Blue (procedural)
NightOverlay: Alpha = 0 (invisible)
Clouds: White/gray, visible
Stars: Inactive (hidden)
Moon: Below horizon (hidden)
```

### Sunrise (6 AM - 8 AM):
```
Skybox: Pink/orange gradient
NightOverlay: Alpha fading from 1→0
Clouds: Orange/pink tinted
Stars: Fading out
Moon: Setting below horizon
```

### Sunset (6 PM - 8 PM):
```
Skybox: Orange/red/purple gradient
NightOverlay: Alpha fading from 0→1
Clouds: Orange/red tinted
Stars: Fading in
Moon: Rising above horizon
```

### Night (8 PM - 6 AM):
```
Skybox: Black (procedural, low exposure)
NightOverlay: Alpha = 1 (fully black)
Clouds: Dark blue-gray
Stars: Fully visible, twinkling
Moon: Above horizon, emitting light
```

---

## Component Responsibilities

### SkyboxManager
- Creates and updates procedural skybox material
- Spawns 100 stars at random positions
- Spawns 1 moon that follows time-based arc
- Spawns 6 clouds that drift across sky
- Updates all colors based on time of day
- Manages render queues

### NightSkyOverlay
- Creates large black dome around scene
- Fades dome alpha based on time
- Ensures night sky is truly black
- Render queue positions dome behind stars

### DayNightCycle
- Tracks game time (24-hour cycle)
- Moves directional light (sun)
- Provides time queries to other systems
- Updates ambient lighting

### CameraSettingsEnforcer (NEW)
- Monitors camera clear flags
- Auto-corrects if changed
- Prevents future bugs

### SkyDiagnostics (NEW)
- Press F9 for diagnostics
- Logs all sky system state
- Helps debug rendering issues

---

## Data Flow

```
Time Update (DayNightCycle)
    ↓
    hour = 14.5 (2:30 PM)
    ↓
┌───┴────┬─────────────┬──────────────┐
↓        ↓             ↓              ↓
SkyboxManager          NightSkyOverlay WeatherSystem
│                      │
├─ Calculate sky color ├─ Calculate alpha
│  (based on hour)     │  (based on hour)
│                      │
├─ Update RenderSettings.skybox
│  ├─ SkyTint color
│  ├─ Ground color
│  ├─ Exposure
│  └─ Atmosphere thickness
│
├─ Update clouds
│  ├─ Position (drift)
│  └─ Color (time-based)
│
├─ Update stars
│  ├─ Visibility (night only)
│  ├─ Twinkle effect
│  └─ Brightness
│
└─ Update moon
   ├─ Position (arc path)
   ├─ Visibility (night only)
   └─ Light intensity
```

---

## Camera Configuration

### Required Settings:

```csharp
Camera mainCamera = Camera.main;

// CRITICAL: Must be Skybox to see sky
mainCamera.clearFlags = CameraClearFlags.Skybox;

// Should include all layers
mainCamera.cullingMask = -1; // Everything

// Fallback color (only used if skybox fails)
mainCamera.backgroundColor = new Color(0.5f, 0.7f, 0.9f);

// Normal camera settings
mainCamera.fieldOfView = 60f;
mainCamera.nearClipPlane = 0.1f;
mainCamera.farClipPlane = 1000f;
```

---

## Debugging Flow

### When sky doesn't render:

1. **Check Camera Clear Flags**
   ```csharp
   if (Camera.main.clearFlags != CameraClearFlags.Skybox)
       Debug.LogError("Camera not set to render skybox!");
   ```

2. **Check Skybox Material**
   ```csharp
   if (RenderSettings.skybox == null)
       Debug.LogError("No skybox material assigned!");
   ```

3. **Check Sky Systems Exist**
   ```csharp
   if (SkyboxManager.Instance == null)
       Debug.LogError("SkyboxManager missing!");
   if (NightSkyOverlay.Instance == null)
       Debug.LogError("NightSkyOverlay missing!");
   ```

4. **Press F9 for Full Diagnostics**
   - SkyDiagnostics logs everything
   - Check Console for detailed state

---

## Performance Optimization

### Frame Skipping:
```csharp
// Stars only update every 3rd frame
if (frameCounter % 3 == 0)
    UpdateStars();

// Debug logging only every 60 frames
if (frameCounter % 60 == 0)
    LogState();
```

### Culling:
```csharp
// Disable objects when not visible
star.SetActive(starVisibility > 0.01f);
moon.SetActive(moonHeight > 0f);
blackDome.SetActive(blackAlpha > 0.001f);
```

### Material Sharing:
- All stars share same shader
- All clouds share same shader
- Procedural skybox reuses one material

---

## Configuration Files

### Scene File (`Assets/fish.unity`):
```yaml
Camera:
  m_ClearFlags: 1        # 1 = Skybox
  m_BackGroundColor: {r: 0.5, g: 0.7, b: 0.9, a: 1}
  m_cullingMask: -1      # Everything
```

### AutoSetup (`Assets/Editor/AutoSetup.cs`):
```csharp
Camera.main.clearFlags = CameraClearFlags.Skybox;
```

---

## Testing Checklist

- [ ] Scene view shows skybox
- [ ] Game view shows skybox
- [ ] Both views match
- [ ] Blue sky during day
- [ ] Black sky at night
- [ ] Stars visible at night
- [ ] Moon visible at night
- [ ] Sunrise/sunset transitions work
- [ ] F9 diagnostics work
- [ ] Console shows no errors

---

## Common Mistakes

### ❌ Wrong:
```csharp
Camera.main.clearFlags = CameraClearFlags.SolidColor;
RenderSettings.skybox = null;
star.transform.localScale = Vector3.zero; // Invisible
```

### ✓ Correct:
```csharp
Camera.main.clearFlags = CameraClearFlags.Skybox;
RenderSettings.skybox = proceduralSkyboxMaterial;
star.transform.localScale = Vector3.one * 0.2f; // Visible
```

---

## Summary

The sky rendering system consists of multiple layers:
1. Procedural skybox (background)
2. Black overlay dome (for night)
3. Clouds (atmosphere)
4. Stars (night sky)
5. Moon (night light)

All of these rely on the camera's Clear Flags being set to Skybox mode. When set to SolidColor, the camera ignores all sky objects and just shows a flat color.

The fix was simple: Change Clear Flags from SolidColor to Skybox in both the scene file and setup code.
