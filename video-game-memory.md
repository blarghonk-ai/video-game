# Crash & Build — Game Memory

A father & son car building and crash physics game built with Unity + Claude Code.

## Project Info
- **Engine**: Unity 2023 LTS with URP (3D URP template)
- **Output**: Mac standalone .app
- **Repo**: https://github.com/blarghonk-ai/video-game
- **Unity project name**: CrashAndBuild

## Game Modes
- Quick Drive, Builder Mode, Crash Test, Free Roam

## Controls
- WASD / Arrow keys = drive | Space = handbrake | R = reset | C = cycle camera | Esc = pause

## Physics Values
| Parameter | Value |
|-----------|-------|
| Car Mass | 1200 kg |
| Wheel Mass | 30 kg |
| Motor Torque | 400 Nm |
| Max Steer Angle | 35° |
| Suspension Distance | 0.2 m |
| Spring Force | 25000 |
| Joint Break Force | 15000 |
| Rigidbody Drag | 0.1 |

## Key Scripts
| Script | Responsibility | Phase |
|--------|---------------|-------|
| CarController.cs | Wheel physics, steering, acceleration, braking | 1 |
| CarPartBreaker.cs | Joint break detection, part detachment on impact | 1 |
| CameraController.cs | Chase, hood, orbit, and crash camera modes | 1 |
| CarBuilder.cs | Drag-drop parts, snap logic, part validation | 2 |
| CarSaveLoad.cs | JSON serialization of car designs | 2 |
| PropPhysics.cs | Destructible props in world | 3 |
| AudioManager.cs | Engine sound pitch scaling, crash sound triggers | 4 |
| VFXManager.cs | Smoke, sparks, dust particle triggers | 4 |
| HUDController.cs | Speedometer, damage meter, impact force display | 5 |
| MenuManager.cs | Main menu, world select, car select navigation | 5 |
| CrashAnalyzer.cs | Impact force calculation, crash result display | 6 |
| ReplayRecorder.cs | Record last 10s of physics state | 7 |
| ReplayPlayer.cs | Playback + slow-motion toggle | 7 |

## Development Phases
- [ ] **Phase 1** — Physics Sandbox: one car driving + crashing on flat ground
- [ ] **Phase 2** — Car Builder Mode: drag/drop parts, snap system, save/load JSON
- [ ] **Phase 3** — World Building: 4 worlds from CC0 assets
- [ ] **Phase 4** — Audio & VFX: engine sounds, crash sounds, particles
- [ ] **Phase 5** — UI & HUD: menus, speedometer, damage meter, crash cam
- [ ] **Phase 6** — Car Variety: more cars, paint colors, fantasy parts
- [ ] **Phase 7** — Crash Scoring & Replay system
- [ ] **Phase 8** — Polish pass

## Worlds
1. **Downtown City** — streets, intersections, parking lots (Kenney City Kit)
2. **Race Circuit** — track, barriers, grandstands (Kenney Racing Pack)
3. **Crash Test Facility** — ramps, walls, drop zones, barrels
4. **Open Off-Road** — hills, dirt paths, jumps, open terrain

## Folder Structure (Assets/)
```
Cars/
  Models/       — .fbx car body models
  Parts/        — Modular builder parts
  Materials/    — Car paint and surface materials
Worlds/
  City/
  RaceTrack/
  CrashTest/
  OffRoad/
Audio/
  Engine/
  Crash/
  Music/
  UI/
UI/
  Sprites/
  Fonts/
Scripts/
  Car/
  Camera/
  UI/
  World/
  Save/
Prefabs/
  Cars/
  Props/
  VFX/
Scenes/
  MainMenu
  CarBuilder
  City
  RaceTrack
  CrashTest
  OffRoad
```

## Asset Sources
### Cars
- Kenney Car Kit — kenney.nl/assets/car-kit
- OGA Low Poly Vehicles Pack — opengameart.org/content/free-low-poly-vehicles-pack
- OGA Car Kit 45+ models — opengameart.org/content/car-kit
- Sketchfab CC0 Concept Cars — sketchfab.com/tags/cc0

### Environments
- Kenney 3D Road Tiles — kenney.nl/assets/3d-road-tiles
- Kenney City Kit Roads — kenney.nl/assets/city-kit-roads
- Kenney City Kit Commercial — kenney.nl/assets/city-kit-commercial
- Kenney City Kit Suburban — kenney.nl/assets/city-kit-suburban
- Kenney City Kit Industrial — kenney.nl/assets/city-kit-industrial
- Kenney Racing Pack — kenney.nl/assets/racing-pack
- OGA Racing Kit — opengameart.org/content/racing-kit
- OGA Modular Racetrack — opengameart.org/content/modular-racetrack-3d-models

### Audio (all CC0)
- Kenney Impact Sounds — kenney.nl/assets/impact-sounds
- Kenney Interface Sounds — kenney.nl/assets/interface-sounds
- Kenney UI Audio — kenney.nl/assets/ui-audio
- OGA Car Engine Loop — opengameart.org/content/car-engine-loop-96khz-4s
- freesound.org (CC0 filter): tire screech, glass break, car horn
- Crash combo: freesound.org user HoBoTrails, sound #426021

### UI
- Kenney UI Pack — kenney.nl (search: UI pack)

## Save System
- Car designs saved as JSON, local only
- Up to 20 saved car slots
- Stores: part list, positions, rotations, color choices
- Auto-save after every builder session

## Session Notes
<!-- Add dated notes here as you build -->
