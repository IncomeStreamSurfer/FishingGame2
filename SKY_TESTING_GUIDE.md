# Sky Rendering - Testing Guide

## Quick Test (1 minute)

1. **Open Unity** and load `Assets/fish.unity`
2. **Select Main Camera** in Hierarchy
3. **Check Inspector** - Verify "Clear Flags" shows "Skybox"
4. **Press Play**
5. **Look at the sky** - Should see:
   - Blue sky (during day)
   - Stars and moon (at night)
   - Colorful transitions (sunrise/sunset)

## Detailed Test (5 minutes)

### Before Starting
1. Open Unity
2. Load `Assets/fish.unity`
3. Open Console window (Window > General > Console)

### Test 1: Camera Settings Verification
1. Select "Main Camera" in Hierarchy
2. In Inspector, verify:
   - Clear Flags: **Skybox** (NOT Solid Color)
   - Culling Mask: **Everything**
   - Background: Any color (fallback only)

### Test 2: Scene View vs Game View
1. **Scene View**: Look at the sky - should show skybox
2. **Game View**: Look at the sky - should ALSO show skybox
3. If they look different, the fix didn't work

### Test 3: Runtime Diagnostics
1. Press Play
2. Press **F9** key
3. Check Console for diagnostics output:
   - Should show "Main Camera: Main Camera"
   - Should show "Clear Flags: Skybox"
   - Should show "RenderSettings.skybox: [Material Name]"
   - Should show "SkyboxManager exists: true"
   - Should show "NightSkyOverlay exists: true"

### Test 4: Day/Night Cycle
1. Press Play
2. Wait for game to start
3. Use time controls (if available) or wait
4. Verify sky changes:
   - **6 AM - 8 AM**: Sunrise (pink/orange)
   - **8 AM - 6 PM**: Blue sky
   - **6 PM - 8 PM**: Sunset (orange/red/purple)
   - **8 PM - 6 AM**: Black with stars

### Test 5: Stars and Moon
1. Set time to night (20:00 / 8 PM)
2. Look up at the sky
3. Should see:
   - Black background (not blue)
   - White stars twinkling
   - Large white moon
   - Moon should cast light on the scene

### Test 6: Auto-Correction
1. Press Play
2. While playing, select Main Camera
3. In Inspector, change Clear Flags to "Solid Color"
4. Watch Console - should see warning:
   - "CameraSettingsEnforcer: Correcting clear flags..."
5. Sky should continue to render correctly

## Common Issues and Solutions

### Issue: Sky is still blue in Game view
**Solution:**
1. Check Main Camera Clear Flags (should be Skybox)
2. Press F9 and check diagnostics
3. Verify `Assets/fish.unity` has `m_ClearFlags: 1`
4. Ensure CameraSettingsEnforcer is active

### Issue: No stars or moon at night
**Solution:**
1. Press F9 to check if SkyboxManager exists
2. Check Console for "Created X procedural stars" message
3. Verify `enableStars` and `enableMoon` are true on SkyboxManager
4. Set time to night (20:00) to trigger visibility

### Issue: Sky is completely black all the time
**Solution:**
1. Check if NightSkyOverlay alpha is stuck at 1.0
2. Press F9 to see NightSkyOverlay status
3. Verify DayNightCycle is running (check time updates)
4. Check if MainMenu.GameStarted is true

### Issue: F9 diagnostics don't work
**Solution:**
1. Ensure SkyDiagnostics.cs is in `Assets/Scripts/`
2. Create an empty GameObject in the scene
3. Add SkyDiagnostics component to it
4. Try F9 again

### Issue: Scene looks different after reopening
**Solution:**
1. Unity may have saved incorrect camera settings
2. Select Main Camera
3. Set Clear Flags to Skybox
4. Save scene (Ctrl+S)

## Expected Console Messages

### On Start (first few seconds):
```
SkyboxManager initialized - Skybox system active
Created 6 procedural clouds
Created 100 procedural stars
Created moon with directional light
Disabled procedural SkyDome - using Unity Skybox instead
NightSkyOverlay: Created black dome overlay for true black night sky

CameraSettingsEnforcer: Initial Camera State
Camera: Main Camera
Clear Flags: Skybox
Culling Mask: [All Layers]
Background Color: RGBA(0.500, 0.700, 0.900, 1.000)
Skybox Material: [Material Name]

SkyboxManager: Camera clear flags = Skybox
SkyboxManager: Camera culling mask = Everything
SkyboxManager: RenderSettings.skybox = [Material Name]
```

### During Gameplay (every ~1 second):
```
SkyboxManager Update: Hour=12.5, Skybox=[Material Name], Stars=100 active=0, Moon active=False
```

### When pressing F9:
```
=== SKY DIAGNOSTICS ===
Main Camera: Main Camera
  Clear Flags: Skybox
  Culling Mask: Everything
  ...
=== END DIAGNOSTICS ===
```

## Files Modified/Created

### Modified:
- `Assets/fish.unity` - Camera clear flags changed to Skybox
- `Assets/Editor/AutoSetup.cs` - Camera initialization fixed
- `Assets/Scripts/SkyboxManager.cs` - Added debug logging
- `Assets/Scripts/NightSkyOverlay.cs` - Added render logging

### Created:
- `Assets/Scripts/SkyDiagnostics.cs` - Press F9 for diagnostics
- `Assets/Scripts/CameraSettingsEnforcer.cs` - Auto-fixes camera settings
- `SKY_FIX_SUMMARY.md` - Detailed fix explanation
- `SKY_TESTING_GUIDE.md` - This file

## Performance Notes

All diagnostic logging can be disabled without affecting functionality:
- SkyboxManager logging runs every 60 frames
- NightSkyOverlay logging only when dome is visible
- SkyDiagnostics only runs when F9 is pressed
- CameraSettingsEnforcer check is very cheap (single boolean comparison)

## Reverting Changes

If you need to revert to solid color sky:
1. Select Main Camera
2. Set Clear Flags to "Solid Color"
3. Disable CameraSettingsEnforcer component
4. Save scene

## Contact/Support

If issues persist:
1. Run F9 diagnostics
2. Copy ALL console output
3. Take screenshots of Scene view AND Game view
4. Check that all files listed above exist
