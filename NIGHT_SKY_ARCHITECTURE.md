# Night Sky Overlay - System Architecture

## Visual Rendering Stack

```
┌─────────────────────────────────────────────────────────────┐
│                        VIEWER (CAMERA)                       │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
                    ┌─────────┴─────────┐
                    │   What you see    │
                    └─────────┬─────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  LAYER 5: CLOUDS (Queue 2900)                               │
│  • White fluffy spheres                                      │
│  • Move across sky                                           │
│  • Render IN FRONT of everything                            │
└─────────────────────────────────────────────────────────────┘
                              ▲
┌─────────────────────────────────────────────────────────────┐
│  LAYER 4: STARS (Queue 2800)                                │
│  • Small emissive spheres                                    │
│  • Twinkling effect                                          │
│  • Visible against BLACK sky at night                       │
└─────────────────────────────────────────────────────────────┘
                              ▲
┌─────────────────────────────────────────────────────────────┐
│  LAYER 3: MOON (Queue 2750)                                 │
│  • Large emissive sphere                                     │
│  • White glow                                                │
│  • Visible against BLACK sky at night                       │
└─────────────────────────────────────────────────────────────┘
                              ▲
┌─────────────────────────────────────────────────────────────┐
│  LAYER 2: NIGHT BLACK OVERLAY (Queue 2600) ⭐ NEW!          │
│  • Large inverted dome                                       │
│  • Pure black with animated alpha:                          │
│    - Alpha 0.0 (transparent) during day → Blue sky visible  │
│    - Alpha 1.0 (opaque) at night → Covers blue, shows black │
│  • Smooth fade transitions at sunrise/sunset                │
└─────────────────────────────────────────────────────────────┘
                              ▲
┌─────────────────────────────────────────────────────────────┐
│  LAYER 1: PROCEDURAL SKYBOX (Queue 2000)                    │
│  • Unity's built-in Skybox/Procedural shader                │
│  • Always renders BLUE sky with atmosphere                   │
│  • Can't be made pure black (shader limitation)             │
│  • Gets covered by black overlay at night                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Time-Based Blending System

```
                    SUNRISE              SUNSET
                   6 AM - 8 AM        6 PM - 8 PM
                        │                  │
        ────────────────┼──────────────────┼────────────────
MIDNIGHT               6 AM               6 PM           MIDNIGHT
  │                     │                  │                │
  │◄──── NIGHT ────────►│◄──── DAY ──────►│◄──── NIGHT ───►│
  │                     │                  │                │
  │  BLACK (α=1.0)      │   BLUE (α=0.0)   │  BLACK (α=1.0) │
  │                     │                  │                │
  └─────────────────────┴──────────────────┴────────────────┘

Timeline Legend:
  α = Overlay Alpha
  α=1.0 = Fully opaque black (covers blue sky)
  α=0.0 = Fully transparent (blue sky visible)

Sunrise Transition (6 AM → 8 AM):
  Hour 6.0: α = 1.00 (black)
  Hour 6.5: α = 0.75 (darkish)
  Hour 7.0: α = 0.50 (blending)
  Hour 7.5: α = 0.25 (mostly blue)
  Hour 8.0: α = 0.00 (blue)

Sunset Transition (6 PM → 8 PM):
  Hour 18.0: α = 0.00 (blue)
  Hour 18.5: α = 0.25 (mostly blue)
  Hour 19.0: α = 0.50 (blending)
  Hour 19.5: α = 0.75 (darkish)
  Hour 20.0: α = 1.00 (black)
```

---

## Component Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      UNITY SCENE                             │
│                                                              │
│  ┌────────────────────────────────────────────────────┐     │
│  │  DayNightCycle GameObject                          │     │
│  │  • Tracks time of day (0-24 hours)                 │     │
│  │  • Updates sun/moon positions                       │     │
│  │  • Controls lighting                                │     │
│  │                                                      │     │
│  │  ┌────────────────────────────────────────────┐    │     │
│  │  │  NightSkyOverlay (child) ⭐ NEW            │    │     │
│  │  │  • Reads current hour from parent          │    │     │
│  │  │  • Calculates overlay alpha                │    │     │
│  │  │  • Updates black dome material             │    │     │
│  │  │                                             │    │     │
│  │  │  ┌──────────────────────────────────┐     │    │     │
│  │  │  │  BlackDome GameObject            │     │    │     │
│  │  │  │  • Inverted sphere primitive     │     │    │     │
│  │  │  │  • Transparent black material    │     │    │     │
│  │  │  │  • Render queue 2600             │     │    │     │
│  │  │  └──────────────────────────────────┘     │    │     │
│  │  └────────────────────────────────────────────┘    │     │
│  └────────────────────────────────────────────────────┘     │
│                                                              │
│  ┌────────────────────────────────────────────────────┐     │
│  │  SkyboxManager GameObject                          │     │
│  │  • Creates procedural skybox                       │     │
│  │  • Creates stars (queue 2800)                      │     │
│  │  • Creates moon (queue 2750)                       │     │
│  │  • Creates clouds (queue 2900)                     │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
```

---

## Data Flow Diagram

```
┌─────────────────┐
│  Time Passes    │  (Time.deltaTime)
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────┐
│  DayNightCycle                      │
│  • currentTimeOfDay += timeSpeed    │
│  • Provides GetCurrentHour()        │
└────────┬────────────────────────────┘
         │
         ├──────────────────┐
         │                  │
         ▼                  ▼
┌─────────────────┐  ┌──────────────────────────────┐
│  Sun/Moon       │  │  NightSkyOverlay             │
│  • Position     │  │  • Reads current hour        │
│  • Lighting     │  │  • Calculates alpha:         │
└─────────────────┘  │    if (6 AM - 8 AM)          │
                     │      α = sunrise fade        │
                     │    else if (8 AM - 6 PM)     │
                     │      α = 0 (transparent)     │
                     │    else if (6 PM - 8 PM)     │
                     │      α = sunset fade         │
                     │    else                       │
                     │      α = 1 (opaque)          │
                     └───────────┬──────────────────┘
                                 │
                                 ▼
                     ┌──────────────────────────────┐
                     │  Black Dome Material         │
                     │  material.color.a = α        │
                     │  • α = 1.0 → BLACK sky       │
                     │  • α = 0.0 → BLUE sky        │
                     └──────────────────────────────┘
```

---

## Render Queue Priority System

```
Lower Number = Renders First (back)
Higher Number = Renders Last (front)

Queue 0-1999: Background
  └── Not used in this system

Queue 2000: Geometry/Skybox
  └── ☁️ Procedural Skybox (ALWAYS BLUE)
      • Unity's Skybox/Procedural shader
      • Atmospheric scattering
      • Can't be pure black

Queue 2001-2599: Custom geometry
  └── Not used in this system

Queue 2600: ⭐ NIGHT BLACK OVERLAY (NEW!)
  └── 🌑 Black Dome
      • Covers blue skybox
      • Transparent during day (sky visible)
      • Opaque at night (black visible)

Queue 2601-2749: Reserved
  └── Not used in this system

Queue 2750: Moon
  └── 🌕 Moon Sphere
      • Emissive white material
      • Renders IN FRONT of black dome
      • Always visible when above horizon

Queue 2800: Stars
  └── ⭐ Star Spheres
      • Emissive materials
      • Twinkling effect
      • Render IN FRONT of black dome
      • Clearly visible against black

Queue 2900: Clouds
  └── ☁️ Cloud Spheres
      • Semi-transparent white
      • Move across sky
      • Render IN FRONT of everything sky-related

Queue 3000+: Other transparent objects
  └── Sun glow, effects, etc.
```

---

## Material Configuration Deep Dive

### Black Dome Material

```
Material: "BlackDomeMaterial"
Shader: Standard (Transparent Mode)

Base Properties:
  • Color: RGB(0, 0, 0) - Pure black
  • Alpha: 0.0 to 1.0 (animated based on time)
  • Metallic: 0.0 (no reflections)
  • Smoothness: 0.0 (completely matte)

Transparency Settings:
  • Rendering Mode: Transparent
  • Source Blend: SrcAlpha
  • Destination Blend: OneMinusSrcAlpha
  • Z Write: Off (allow objects to render through)
  • Alpha Blend: On

Render Settings:
  • Render Queue: 2600
  • Culling: Front faces (inverted dome)
  • Double Sided: No (single sided)

Blending Formula:
  finalColor = (black * α) + (skyboxColor * (1-α))

  When α = 0.0:
    finalColor = skyboxColor (blue visible)
  When α = 1.0:
    finalColor = black (completely black)
  When α = 0.5:
    finalColor = 50% black, 50% blue (blending)
```

---

## GameObject Hierarchy

```
Scene Root
│
├── DayNightCycle
│   ├── Sun
│   │   ├── SunSphere
│   │   └── SunGlow
│   ├── Moon
│   │   ├── MoonSphere
│   │   └── MoonGlow
│   ├── Stars
│   │   ├── Star_0
│   │   ├── Star_1
│   │   └── ... (100 stars)
│   ├── SkyDome (may be disabled)
│   ├── AmbientLight
│   │
│   └── NightSkyOverlay ⭐ NEW
│       └── NightSkyOverlay_BlackDome
│           • MeshFilter (Sphere)
│           • MeshRenderer (BlackDomeMaterial)
│
└── SkyboxManager
    ├── Cloud_0
    ├── Cloud_1
    ├── ... (6 clouds)
    ├── Star_0 (if not using DayNightCycle stars)
    └── Moon (if not using DayNightCycle moon)
```

---

## Transition State Machine

```
                    ┌─────────────┐
                    │   NIGHT     │
           ┌───────►│  α = 1.0    │◄──────┐
           │        │  SKY: BLACK │       │
           │        └──────┬──────┘       │
           │               │              │
    20:00  │        6:00   │              │ 0:00-6:00
    (8 PM) │        (6 AM) │              │ (midnight-dawn)
           │               │              │
           │               ▼              │
           │        ┌─────────────┐       │
           │        │  SUNRISE    │       │
           │        │  TRANSITION │       │
           │        │  α: 1.0→0.0 │       │
           │        │  SKY: B→Bl  │       │
           │        └──────┬──────┘       │
           │               │              │
           │        8:00   │              │
           │        (8 AM) │              │
           │               │              │
           │               ▼              │
           │        ┌─────────────┐       │
           │   18:00│     DAY     │       │
    ┌──────┘   (6PM)│  α = 0.0    │       │
    │               │  SKY: BLUE  │       │
    │               └──────┬──────┘       │
    │                      │              │
    │                      │ 8:00-18:00   │
    │                      │ (8 AM-6 PM)  │
    │                      │              │
    │                      ▼              │
    │               ┌─────────────┐       │
    │               │   SUNSET    │       │
    │               │  TRANSITION │       │
    │               │  α: 0.0→1.0 │       │
    │               │  SKY: Bl→B  │       │
    └───────────────┤             │       │
                    └─────────────┘       │
                           │              │
                           └──────────────┘

Legend:
  α = Overlay Alpha
  B = Black
  Bl = Blue
  B→Bl = Black fading to Blue
  Bl→B = Blue fading to Black
```

---

## Performance Profile

### Per Frame Operations

```
Frame Start
  │
  ├─ DayNightCycle.Update()
  │  └─ currentTimeOfDay += timeSpeed * deltaTime
  │
  ├─ NightSkyOverlay.Update()
  │  ├─ hour = dayNightCycle.GetCurrentHour()
  │  ├─ alpha = CalculateBlackAlpha(hour)
  │  │  └─ Simple if/else checks + Mathf.Lerp
  │  └─ material.color.a = alpha
  │     └─ Single float assignment
  │
  └─ Render Pipeline
     ├─ Draw Skybox (Queue 2000)
     ├─ Draw Black Dome (Queue 2600) - if alpha > 0.001
     ├─ Draw Moon (Queue 2750) - if visible
     ├─ Draw Stars (Queue 2800) - if visible
     └─ Draw Clouds (Queue 2900)

Frame End

Overhead:
  • CPU: ~0.05ms (negligible)
  • GPU: 1 additional draw call when overlay visible
  • Memory: <1 KB (one material, one mesh)
```

---

## Edge Cases & Handling

### 1. DayNightCycle Missing
```
Handling: Component logs warning and disables itself
Result: No black overlay (fallback to blue sky)
```

### 2. Time Exactly at Transition Boundary
```
Time = 6:00:00 AM (exactly)
Alpha = 1.0 (still black)

Time = 6:00:01 AM (one second later)
Alpha = 0.9995 (starting to fade)
```

### 3. Very Fast Time Speed
```
If day length = 60 seconds (fast-forward):
  • Transitions still smooth (uses delta time)
  • No visible popping or artifacts
  • Alpha updates every frame
```

### 4. Paused Game
```
If Time.timeScale = 0:
  • DayNightCycle doesn't advance
  • Overlay stays at current alpha
  • Sky appearance frozen
```

### 5. Scene Reload
```
On scene load:
  • Auto-setup runs again
  • Creates overlay if missing
  • Preserves existing if already present
```

---

## Integration Points

### System Interactions

```
┌──────────────────────┐
│  NightSkyOverlay     │
└──────────────────────┘
         │
         │ Reads time
         ▼
┌──────────────────────┐
│  DayNightCycle       │
└──────────────────────┘

┌──────────────────────┐
│  SkyboxManager       │
└──────────────────────┘
         │
         │ Renders skybox
         ▼
  [Blue procedural sky]
         │
         │ Gets covered by
         ▼
┌──────────────────────┐
│  Black Dome          │
│  (when α > 0)        │
└──────────────────────┘
         │
         │ Stars/moon render in front
         ▼
  [Black night sky with celestial objects]
```

---

## Summary

The Night Sky Overlay system works by:

1. **Creating a large inverted dome** around the scene
2. **Rendering it AFTER the skybox** (queue 2600) to cover the blue
3. **But BEFORE stars/moon** (queues 2750-2800) so they're visible
4. **Animating the dome's alpha** from transparent (day) to opaque (night)
5. **Using smooth transitions** via Mathf.SmoothStep for natural blending

This architecture ensures:
- ✅ True black sky at night (not blue)
- ✅ Normal blue sky during day
- ✅ Smooth sunrise/sunset transitions
- ✅ Visible stars and moon against black
- ✅ No modification of existing systems
- ✅ Minimal performance impact

**The result: A properly functioning day/night sky system with dramatic black nights!**
