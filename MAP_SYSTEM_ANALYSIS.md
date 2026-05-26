# 7VNR — Map System Technical Analysis & Creation Guide

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Map Data Model](#2-map-data-model)
3. [Map Prefab Anatomy](#3-map-prefab-anatomy)
4. [Rendering Pipeline](#4-rendering-pipeline)
5. [Collision Detection System](#5-collision-detection-system)
6. [Map Transport (Navigation Between Maps)](#6-map-transport-navigation-between-maps)
7. [Camera Bounds System](#7-camera-bounds-system)
8. [Enemy Spawning on Maps](#8-enemy-spawning-on-maps)
9. [Reusable Assets in CategorizedAssets](#9-reusable-assets-in-categorizedassets)
10. [Step-by-Step Guide: Creating a New Map](#10-step-by-step-guide-creating-a-new-map)

---

## 1. Architecture Overview

The map system follows a **JSON-driven, prefab-based** architecture. Maps are Unity prefabs stored under `Assets/Resources/Map/` and loaded at runtime via `Resources.Load<GameObject>()`. Map metadata — including neighbor links, player spawn positions, and enemy placements — lives in a separate JSON file (`Assets/Resources/Data/Enemies/spawnPoints.json`).

### Key Scripts

| Script | Location | Responsibility |
|---|---|---|
| `GamePlay.cs` | `Scripts/GamePlay/` | Orchestrates map loading, player creation, and input routing |
| `MapManager.cs` | `Scripts/Controller/` | Singleton stub (currently unused; logic lives in `GamePlay`) |
| `MapTransport.cs` | `Scripts/Map/` | Attaches to each map prefab root; wires left/right transport triggers |
| `Transport.cs` | `Scripts/Map/` | Individual trigger zone that fires `OnEnterTransport` on player collision |
| `MapData.cs` | `Scripts/Map/` | Data classes: `MapJsonData`, `EnemySpawnInfo`, `MapDataWrapper` |
| `CameraFollowBounds.cs` | `Scripts/` | Orthographic camera that clamps within map edge boundaries |
| `MiniMapFollow.cs` | `Scripts/` | Secondary camera for the minimap, shares the same bounds |
| `CloudScroll.cs` | `Scripts/` | Scrolls a material's texture offset for parallax cloud effects |
| `FadeManager.cs` | `Scripts/Controller/` | Fade-in/out transitions used during scene/map loads |

### Data Flow

```
GameManager.Start()
  └─► GamePlay.Initialize()
        ├─► LoadMapData()           // reads spawnPoints.json → MapDataWrapper
        └─► LoadMap("Map/Map_0")    // first map
              ├─► Resources.Load<GameObject>(path)
              ├─► Instantiate(prefab) under mapRoot
              ├─► MapTransport.SetData()  // wire neighbour links
              ├─► CameraFollowBounds.SetEdgeParent()
              └─► EnemyManager.LoadMapEnemies()
```

---

## 2. Map Data Model

### `spawnPoints.json` Schema

Located at `Assets/Resources/Data/Enemies/spawnPoints.json`:

```json
{
  "maps": [
    {
      "mapPrefab": "Map/Map_0",          // Resources path to the prefab
      "leftMapPrefab": "Map/Map_2",      // neighbour on the left (nullable)
      "rightMapPrefab": "Map/Map_1",     // neighbour on the right (nullable)
      "playerSpawnPos": { "x": 0, "y": 0, "z": 0 },
      "enemies": [
        { "enemyName": "MayDam", "spawnPosition": { "x": -29, "y": 1 } }
      ]
    }
  ]
}
```

### C# Data Classes (`MapData.cs`)

```csharp
[Serializable]
public class EnemySpawnInfo {
    public string enemyName;       // matches key in enemies.json
    public Vector2 spawnPosition;
}

[Serializable]
public class MapJsonData {
    public string mapPrefab;          // Resources.Load path
    public string leftMapPrefab;      // left neighbor (can be null/empty)
    public string rightMapPrefab;     // right neighbor (can be null/empty)
    public Vector3 playerSpawnPos;
    public List<EnemySpawnInfo> enemies;
}

[Serializable]
public class MapDataWrapper {
    public List<MapJsonData> maps;
}
```

### Current Map Registry (6 maps)

| Prefab | Left Neighbor | Right Neighbor | Enemies |
|---|---|---|---|
| `Map/Map_0` | `Map/Map_2` | `Map/Map_1` | MayDam ×3, Gigan ×3 |
| `Map/Map_1` | `Map/Map_0` | `Map/Map_3` | Ran ×5, KhiCon ×4, KhungLongBay ×1 |
| `Map/Map_2` | _(none)_ | `Map/Map_0` | KhiCon ×5, KhungLongBay ×1, ThanLanBay ×1, OcSen ×4 |
| `Map/Map_3` | `Map/Map_1` | `Map/Map_4` | _(empty)_ |
| `Map/Map_4` | `Map/Map_3` | `Map/Map_5` | _(empty)_ |
| `Map/Map_5` | `Map/Map_4` | _(none)_ | _(empty)_ |

---

## 3. Map Prefab Anatomy

Each map is a self-contained Unity prefab (`Map_X.prefab`) with a standardized hierarchy. All GameObjects use **Layer 7** (a custom layer, likely named "Map" in the project's Tag & Layer settings).

### Hierarchy Template

```
Map_X  (root — has MapTransport component)
├── Layer_1          (z=4)     — farthest background (sky, distant mountains)
├── Layer_2          (z=3)     — mid-background (hills, buildings)
├── Layer_3          (z=2)     — near-background (trees, structures)
├── Layer_4          (z=1)     — foreground decorations rendered behind gameplay
├── Layer_Ground     (z≈-0.4)  — ground-level visual sprites
│   ├── BG                     — large background sprite (sky/landscape)
│   ├── ColorGround            — colored ground fill sprite
│   ├── Shadow                 — ground shadow overlay
│   └── Sprite_N...            — additional ground-level sprites
├── Layer_Collider   (z≈-0.4)  — physics collision container
│   ├── Ground       (tag: "Ground")  — EdgeCollider2D for main walkable surface
│   ├── ColorCollider                  — visual-only ground color sprite
│   └── Edge                           — camera bounds container
│       ├── Top          — EdgeCollider2D (camera top bound)
│       ├── Bottom       — EdgeCollider2D (camera bottom bound)
│       ├── Left         — EdgeCollider2D (camera left bound)
│       └── Right        — EdgeCollider2D (camera right bound)
├── NPC_Gohan, NPC_Bulma, ...  — NPC GameObjects with BoxCollider2D (trigger)
├── Left_Outside / Right_Outside — BoxCollider2D walls preventing player escape
├── oneway / oneway (1) ...    — EdgeCollider2D + PlatformEffector2D (one-way platforms)
├── ArrowWarp                  — visual indicators for warp/transport zones
├── WaterFall                  — animated waterfall Spine objects
├── Particle System            — leaf/snow/dust particle effects
└── NameMap                    — text label showing map name
```

### Root Component: `MapTransport`

Every map root has a `MapTransport` MonoBehaviour with serialized references:

- `leftTransport` → reference to a `Transport` GameObject on the left edge
- `rightTransport` → reference to a `Transport` GameObject on the right edge
- `edgeParent` → reference to the `Edge` GameObject (camera bounds)

---

## 4. Rendering Pipeline

### Rendering Strategy

The game uses a **2D orthographic camera** with Unity's **SpriteRenderer** pipeline and **Spine 2D** for skeletal animations. Depth ordering is achieved through a combination of:

1. **Z-position** — Background layers use higher Z values (Layer_1 at z=4, Layer_2 at z=3, etc.) pushing them behind the gameplay plane at z=0.
2. **Sorting Order** — `SpriteRenderer.sortingOrder` differentiates draw order within the same Z plane:
   - Background (BG): low sorting order
   - Ground elements: `sortingOrder = 6`
   - Shadows: `sortingOrder = -1`
   - Arrows/indicators: `sortingOrder = 99`
   - Map name: `sortingOrder = 101`
3. **Sorting Layer** — Some sprites reference specific sorting layer IDs for additional control.

### Layer Breakdown

| Layer Name | Z Offset | Content | Rendering Role |
|---|---|---|---|
| `Layer_1` | z = 4 | Sky, distant BG | Farthest parallax background |
| `Layer_2` | z = 3, y ≈ 2.89 | Mid-range BG, structures | Mid-ground decoration |
| `Layer_3` | z = 2 | Near-range BG, trees | Near decoration layer |
| `Layer_4` | z = 1 | Foreground elements | Foreground decoration |
| `Layer_Ground` | z ≈ -0.4 | Ground visuals, tilemap sprites | Walkable surface visuals |
| `Layer_Collider` | z ≈ -0.4 | Colliders, Edge bounds | Physics-only (mostly invisible) |

### Spine Animations on Maps

Some map objects (e.g., waterfalls, ambient decorations) use `SkeletonAnimation` components from the Spine-Unity runtime. These are rendered inline via Spine's `MeshRenderer` with their own sorting order. Map-specific Spine assets are stored at:

```
Assets/Resources/CategorizedAssets/Spine_Animations/Map/
```

### Parallax Scrolling

The `CloudScroll.cs` script achieves parallax cloud effects by continuously offsetting the `mainTextureOffset` of a material. This is typically attached to background quads or sprites in the far layers:

```csharp
mat.mainTextureOffset += new Vector2(scrollSpeed * Time.deltaTime, 0);
```

Parallax materials are located in `Assets/Material/` (e.g., `Parrallax_0 1.mat`).

### Particle Effects

Some maps include `ParticleSystem` components (e.g., falling leaves, dust) attached to dedicated child GameObjects. These use Unity's built-in particle renderer with custom rotation and simulation speed settings.

---

## 5. Collision Detection System

### Ground Collision

The primary walkable surface uses an **`EdgeCollider2D`** component on the `Ground` child object (tagged `"Ground"`, Layer 7). The edge points define the terrain shape:

```yaml
# Example from Map_0
EdgeCollider2D:
  m_Points:
    - {x: -40, y: 1}
    - {x: 40.89, y: 1.14}
```

The character detects the ground via **raycasting** in `Character.IsGrounded()`:

```csharp
public bool IsGrounded()
{
    Bounds bounds = boxCollider.bounds;
    Vector2 left   = new(bounds.min.x, bounds.min.y);
    Vector2 center = new(bounds.center.x, bounds.min.y);
    Vector2 right  = new(bounds.max.x, bounds.min.y);

    return Physics2D.Raycast(left, Vector2.down, groundCheckDistance, groundLayer) ||
           Physics2D.Raycast(center, Vector2.down, groundCheckDistance, groundLayer) ||
           Physics2D.Raycast(right, Vector2.down, groundCheckDistance, groundLayer);
}
```

Three downward rays (left, center, right of the capsule collider) check for ground within `groundCheckDistance`. This is used by the state machine to transition between `IdleState`, `FallState`, `JumpState`, `RunState`, and `FlyState`.

### One-Way Platforms

Some maps (e.g., Map_1, Map_3) include **one-way platforms** using:

- `EdgeCollider2D` with `m_UsedByEffector: 1`
- `PlatformEffector2D` with `m_SurfaceArc: 90`

This allows the player to jump up through the platform from below but stand on it from above.

### Map Boundary Walls

**`BoxCollider2D`** objects named `Left_Outside`, `Right_Outside`, `Top_Outside` act as invisible walls at map edges, preventing the player from walking off the map.

### Transport Trigger Zones

The `Transport.cs` script uses **`OnCollisionEnter2D`** (not trigger) to detect when a player tagged `"Player"` collides with the transport zone:

```csharp
void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player"))
    {
        OnEnterTransport?.Invoke();
    }
}
```

### NPC Interaction Zones

NPCs (e.g., `NPC_Gohan`, `NPC_Bulma`, `NPC_OoLong`) have **`BoxCollider2D`** set as triggers (`m_IsTrigger: 1`) with a tag pattern like `unknown_20008`. These are currently disabled (`m_Enabled: 0` on some) or used for click-based interaction via raycasting.

### Character Physics

- `Rigidbody2D` with gravity scale 5 (normal state), 0 (fly state)
- `CapsuleCollider2D` for body collision
- `groundLayer` LayerMask for ground detection raycasts

### Enemy Physics

- `Rigidbody2D` with gravity scale 3, freeze rotation, continuous collision detection, interpolation
- `Collider2D` on the "Enemy" layer for raycast-based targeting by the player

---

## 6. Map Transport (Navigation Between Maps)

### How Map Transitions Work

1. Player walks into a `Transport` collider (left or right edge of the map).
2. `Transport.OnCollisionEnter2D()` fires the `OnEnterTransport` Action.
3. `MapTransport` handles the event:
   - Calls `GamePlay.LoadMap(nextMapPath)` to load the new map.
   - Calls `GamePlay.SetPlayerPositionWhenEnterWaypoint(isLeft)` to reposition the player.
   - Destroys the current map GameObject.
4. `GamePlay.LoadMap()`:
   - Destroys old map, clears enemies.
   - Loads new prefab via `Resources.Load<GameObject>()`.
   - Instantiates under `GameManager.mapRoot`.
   - Looks up `MapJsonData` from the pre-loaded `spawnPoints.json`.
   - Wires `MapTransport.SetData()` with neighbor info.
   - Sets camera bounds via `CameraFollowBounds.SetEdgeParent()`.
   - Spawns enemies via `EnemyManager.LoadMapEnemies()`.
5. Player position is set relative to the new map's transport points:
   - Entering from the left → placed near the right transport (offset -5x, +5y)
   - Entering from the right → placed near the left transport (offset +5x, -5y)

---

## 7. Camera Bounds System

`CameraFollowBounds` is a singleton attached to the main orthographic camera. It constrains the viewport within the map's boundaries.

### How Bounds Are Set

The `Edge` child GameObject under `Layer_Collider` contains 4 `EdgeCollider2D` children:

| Index | Name | Purpose |
|---|---|---|
| 0 | Top | `maxY` boundary |
| 1 | Bottom | `minY` boundary |
| 2 | Left | `minX` boundary |
| 3 | Right | `maxX` boundary |

The camera reads the **positions** of these edge objects (not their collider points) to determine bounds:

```csharp
Vector3 topEdge    = edges[0].transform.position;  // Top
Vector3 bottomEdge = edges[1].transform.position;  // Bottom
Vector3 leftEdge   = edges[2].transform.position;  // Left
Vector3 rightEdge  = edges[3].transform.position;  // Right

minY = bottomEdge.y;
maxY = topEdge.y;
minX = leftEdge.x;
maxX = rightEdge.x;
```

**Important**: The order of children under `Edge` matters (indices 0–3 map to Top, Bottom, Left, Right). The camera smoothly follows the player (`Vector3.Lerp`) and clamps within `[minX + halfWidth, maxX - halfWidth]` and `[minY + halfHeight, maxY - halfHeight]`.

### Minimap

`MiniMapFollow` uses the same bounds from `CameraFollowBounds.Instance` but with a separate orthographic camera and a Z offset of -10.

---

## 8. Enemy Spawning on Maps

### Data Pipeline

1. `enemies.json` defines enemy templates (stats, prefab path, animations).
2. `spawnPoints.json` defines which enemies appear on which map and their positions.
3. `EnemyManager` loads templates at startup, then spawns per-map enemies on demand.

### Enemy Prefabs

Located at `Assets/Resources/Prefabs/EnemyPrefabs/`:

- `Gigan.prefab`
- `KhiCon.prefab`
- `KhungLongBay.prefab`
- `MayDam.prefab`
- `OcSen.prefab`
- `Ran.prefab`
- `ThanLanBay.prefab`

Each prefab has a `SkeletonAnimation` (Spine), `Rigidbody2D`, and `Collider2D`. Enemies are parented under `GameManager.EnemiesHolder` at runtime.

---

## 9. Reusable Assets in CategorizedAssets

`Assets/Resources/CategorizedAssets/` contains organized raw assets extracted from game data. The `_index.json` provides a statistical breakdown:

### Asset Categories

| Category | Spine Packages | Sprites | Textures | Description |
|---|---|---|---|---|
| `maps_environment` | 32 | 101 | 127 | Map backgrounds, decorations, environment elements |
| `mobs_npc` | 42 | 2 | 44 | Monster and NPC Spine animations |
| `characters_skins` | 35 | — | 55 | Player character Spine skins |
| `skill_effects` | 208 | 8 | 231 | Skill visual effect animations |
| `pets_mounts` | 13 | 1 | 14 | Pet and mount animations |
| `items_equipment` | 20 | 2 | 22 | Equipment item Spine data |
| `ui` | 3 | 24 | 28 | UI elements |
| `items_icons_numeric` | — | 645 | 645 | Numeric item icon sprites |
| `unknown_review` | 39 | 389 | 401 | Uncategorized assets pending review |

### Map-Relevant Reusable Assets

#### 1. Map Backgrounds (`CategorizedAssets/Map_Backgrounds/`)

Organized by map regions ("nhanh" = branch):

| Region | Maps | Asset Types |
|---|---|---|
| `nhanh2/doico` | Đồi cỏ (Grassland) | Sky, layers (1–4), tilemap, trees, rocks, fog, buildings |
| `nhanh3/bobienkame` | Bờ biển Kame (Kame Beach) | Sky, clouds, sand, water, rocks, Kame House, islands |
| `nhanh3/bobienphianam` | Bờ biển phía Nam (South Beach) | Similar beach assets |
| `nhanh3/daodancu` | Đảo dân cư (Residential Island) | Island-themed backgrounds |
| `nhanh3/daokame` | Đảo Kame (Kame Island) | Small island assets |
| `nhanh3/kamehouse` | Nhà Kame (Kame House) | Indoor/house backgrounds |
| `nhanh4/thanhphotaitho` | Thành phố Tây Thô (West Capital City) | Multi-layer city backgrounds with tilemaps |

**Naming convention for backgrounds**: Files like `Layer1.png`, `Layer2.png`, `Sky.png`, `TileMap1.png` map directly to the layer structure in map prefabs.

#### 2. Map Spine Animations (`CategorizedAssets/Spine_Animations/Map/`)

Animated map decorations (each folder contains `.skel`, `.atlas`, `.png`):

| Asset | Description |
|---|---|
| `1_hp_core`, `2_hp_orbiter` | HP station core and orbiter animations |
| `11_mp_core`, `12_mp_orbiter` | MP station core and orbiter animations |
| `11_revive` (HoiSinh) | Revival point animation |
| `1_level1_front`, `2_level1_back` | Level 1 boss area aura (front/back) |
| `11_level2_front`, `12_level2_back` | Level 2 boss area aura |
| `21_level3_front`, `22_level3_back` | Level 3 boss area aura |
| `2_level5_front`, `1_level5_back` | Level 5 area effects |
| `11_level6_back`, `12_level6_front` | Level 6 area effects |
| `31_level8_back`, `32_level8_front` | Level 8 area effects |
| `41_level9_back`, `42_level9_sau` | Level 9 area effects |
| `51_level10_back`, `52_level10_front` | Level 10 area effects |

#### 3. Mob/NPC Spine Animations (`CategorizedAssets/Spine_Animations/Mob/`, `NPC/`)

- Mob variants: `10001_mob_traidat_attack_1_blue/orange/red/violet`, `10013_mob_ocsen`, `10014_mob_doi`, `10015_mob_rua`, `10016_mob_ong`, etc.
- Super mobs: `30051_supermob_nappa`, `30052_supermob_raditz`, `30055_supermob_vegeta`
- NPCs: `50011_npc_ruabien`, `50012_npc_quylao`, `50013_npc_lunch`, etc.

#### 4. Images & Icons (`CategorizedAssets/Images_Icons/`)

- `avatar/` — Character/enemy portrait images (used for UI)
- `icon/` — Item/status icons
- `skillicon/` — Skill button icons

---

## 10. Step-by-Step Guide: Creating a New Map

### Prerequisites

- Unity Editor with the 7VNR project open
- Familiarity with Unity's 2D tools (SpriteRenderer, Collider2D, Prefab workflow)

### Step 1: Create the Map Prefab Structure

1. In Unity, create a new empty GameObject. Name it `Map_N` (e.g., `Map_6`).
2. Add the `MapTransport` script component to the root.
3. Set the root's Layer to **Layer 7**.
4. Set the root's Transform:
   - Adjust `position` and `scale` to match your map's world-space size. Reference: Map_0 uses `position: (-16.5, -10.4, 2.6)`, `scale: (1.5, 1.45, 1)`.

### Step 2: Build the Visual Layers

Create the following child GameObjects under the root:

| Child Name | Local Position | Purpose |
|---|---|---|
| `Layer_1` | `(0, 0, 4)` | Farthest background (sky) |
| `Layer_2` | `(0, 2.89, 3)` | Mid-background |
| `Layer_3` | `(0, 0, 2)` | Near-background |
| `Layer_4` | `(0, 0, 1)` | Foreground decorations |
| `Layer_Ground` | `(0, -0.4, 0)` | Ground-level visuals |

For each layer:
1. Create a child named `BG` with a `SpriteRenderer`.
2. Assign a background sprite from `CategorizedAssets/Map_Backgrounds/` for the chosen region.
3. Add additional decorative sprites as needed (trees, rocks, structures).
4. Set appropriate `sortingOrder` values on each `SpriteRenderer`.

**Tip**: Use assets from `Map_Backgrounds/nhanh2/doico/` for a grassland theme, `nhanh3/bobienkame/` for a beach theme, or `nhanh4/thanhphotaitho/` for a city theme.

### Step 3: Set Up the Collision Layer

Create `Layer_Collider` at `(0, -0.4, 0)` with the following children:

#### 3a. Ground Collider

1. Create a child named `Ground`.
2. Set tag to `"Ground"`.
3. Add an `EdgeCollider2D`.
4. Define the edge points to match your ground terrain shape. Example:
   ```
   Points: [(-40, 1), (40, 1)]  // flat ground
   ```
   For uneven terrain, add intermediate points.

#### 3b. Camera Bounds (Edge)

1. Create a child named `Edge`.
2. Add 4 child GameObjects **in this exact order**:
   - `Top` — position at `(0, Y_max, 0)` with an `EdgeCollider2D`
   - `Bottom` — position at `(0, Y_min, 0)` with an `EdgeCollider2D`
   - `Left` — position at `(X_min, 0, 0)` with an `EdgeCollider2D`
   - `Right` — position at `(X_max, 0, 0)` with an `EdgeCollider2D`
3. The camera system reads **transform.position** of these objects, so their positions define the camera boundary rectangle.

**Important**: The child order under `Edge` must be: Top (index 0), Bottom (index 1), Left (index 2), Right (index 3).

#### 3c. One-Way Platforms (Optional)

For elevated platforms the player can jump through:
1. Create a child named `oneway`.
2. Add an `EdgeCollider2D` with `m_UsedByEffector = true`.
3. Add a `PlatformEffector2D` with `surfaceArc = 90`.
4. Position the edge points to define the platform surface.

### Step 4: Add Boundary Walls

Create invisible boundary colliders to prevent the player from leaving the map:

1. `Left_Outside` — `BoxCollider2D` positioned at the far-left edge
2. `Right_Outside` — `BoxCollider2D` positioned at the far-right edge
3. `Top_Outside` (optional) — `BoxCollider2D` at the top if needed

### Step 5: Add Transport Zones

1. Create two child GameObjects (e.g., `TransportLeft`, `TransportRight`) at the left and right map edges.
2. Add a `Collider2D` to each (the `Transport.cs` script uses `OnCollisionEnter2D`, so do **not** mark it as trigger).
3. Add the `Transport` script to each.
4. In the root `MapTransport` component:
   - Assign `leftTransport` → the left Transport GameObject
   - Assign `rightTransport` → the right Transport GameObject
   - Assign `edgeParent` → the `Edge` GameObject

### Step 6: Add NPCs (Optional)

For each NPC:
1. Create a child GameObject (e.g., `NPC_MyNPC`).
2. Add a Spine `SkeletonAnimation` component with the NPC's skeleton data from `CategorizedAssets/Spine_Animations/NPC/`.
3. Add a `BoxCollider2D` (set as trigger if for click interaction).
4. Add child elements: `Visual` (spine renderer), `NameNPC` (text label), `MarkQuest`/`MarkMinimap` (indicator sprites).

### Step 7: Add Animated Decorations (Optional)

For Spine-based map decorations (waterfalls, auras):
1. Use assets from `CategorizedAssets/Spine_Animations/Map/`.
2. Create a child, add `SkeletonAnimation`, assign the `.skel` data asset.
3. Set the sorting order to fit within the layering scheme.

For particle effects:
1. Add a `Particle System` child.
2. Configure emission, shape, and rendering as needed.

For parallax clouds:
1. Add a sprite/quad with a tiling material.
2. Attach `CloudScroll.cs` and set `scrollSpeed`.

### Step 8: Save as Prefab

1. Drag the completed `Map_N` hierarchy into `Assets/Resources/Map/`.
2. This creates `Map_N.prefab` loadable via `Resources.Load<GameObject>("Map/Map_N")`.

### Step 9: Register in spawnPoints.json

Add a new entry to `Assets/Resources/Data/Enemies/spawnPoints.json`:

```json
{
  "mapPrefab": "Map/Map_6",
  "leftMapPrefab": "Map/Map_5",
  "rightMapPrefab": "",
  "playerSpawnPos": { "x": 0, "y": 5, "z": 0 },
  "enemies": [
    { "enemyName": "Gigan", "spawnPosition": { "x": 10, "y": 1 } },
    { "enemyName": "KhiCon", "spawnPosition": { "x": -15, "y": 1 } }
  ]
}
```

Also update the **neighbor map's** entry to link back to your new map. For example, if connecting to Map_5's right side, update Map_5's `rightMapPrefab` to `"Map/Map_6"`.

### Step 10: Add New Enemies (Optional)

If using new enemy types:

1. Create a Spine-based enemy prefab in `Assets/Resources/Prefabs/EnemyPrefabs/`.
2. Add an entry to `Assets/Resources/Data/Enemies/enemies.json`:
   ```json
   {
     "name": "NewEnemy",
     "prefabPath": "Prefabs/EnemyPrefabs/NewEnemy",
     "hp": 50000,
     "hpMax": 50000,
     "damage": 15,
     "speed": 10,
     "detectionRange": 5,
     "attackRange": 2,
     "attackCooldown": 1.5,
     "spawnPoint": { "x": 0, "y": 0 },
     "respawnTime": 5,
     "patrolRange": 2,
     "patrolSpeed": 2,
     "patrolWaitTime": 2,
     "animations": {
       "idle": "idle",
       "walk": "walk",
       "attack": "atk",
       "hit": "hit",
       "die": "die"
     }
   }
   ```
3. Reference the enemy by name in `spawnPoints.json`.

### Step 11: Test

1. Open the `Game.unity` scene.
2. Press Play.
3. Navigate to your new map by walking through transport zones, or temporarily set your map as the first entry in `spawnPoints.json`.
4. Verify:
   - Visual layers render correctly with proper depth ordering.
   - Ground collision works (player stands on the ground, doesn't fall through).
   - Camera stays within bounds.
   - Transport zones transition to neighbor maps correctly.
   - Enemies spawn at defined positions.
   - One-way platforms (if any) work correctly.

---

## Appendix A: Quick Reference — File Locations

| Asset Type | Path |
|---|---|
| Map prefabs | `Assets/Resources/Map/Map_*.prefab` |
| Spawn/map config | `Assets/Resources/Data/Enemies/spawnPoints.json` |
| Enemy templates | `Assets/Resources/Data/Enemies/enemies.json` |
| Enemy prefabs | `Assets/Resources/Prefabs/EnemyPrefabs/` |
| Map scripts | `Assets/Scripts/Map/` |
| Controller scripts | `Assets/Scripts/Controller/` |
| Map backgrounds | `Assets/Resources/CategorizedAssets/Map_Backgrounds/` |
| Map Spine anims | `Assets/Resources/CategorizedAssets/Spine_Animations/Map/` |
| Mob Spine anims | `Assets/Resources/CategorizedAssets/Spine_Animations/Mob/` |
| NPC Spine anims | `Assets/Resources/CategorizedAssets/Spine_Animations/NPC/` |
| Character Spine anims | `Assets/Resources/CategorizedAssets/Spine_Animations/NhanVat/` |
| UI icons | `Assets/Resources/CategorizedAssets/Images_Icons/` |
| Materials | `Assets/Material/` |
| Textures | `Assets/Texture/`, `Assets/Texture2D/` |
| Main scene | `Assets/Scenes/Game.unity` |

## Appendix B: Common Pitfalls

1. **Edge child order matters** — The camera bounds system reads children by index (0=Top, 1=Bottom, 2=Left, 3=Right). Reordering in the hierarchy will break camera clamping.
2. **Transport uses collision, not trigger** — `Transport.cs` listens to `OnCollisionEnter2D`. Ensure the transport zone collider is NOT set as a trigger.
3. **Resources path must be relative** — `Resources.Load()` paths are relative to `Assets/Resources/`. Use `"Map/Map_6"` not `"Assets/Resources/Map/Map_6"`.
4. **Layer 7 for all map objects** — All map GameObjects should be on Layer 7 to maintain consistency with physics layer masks.
5. **Ground tag** — The ground collider must be tagged `"Ground"` for the character's `IsGrounded()` raycast to detect it.
6. **Update both neighbor entries** — When connecting a new map, update both the new map's entry AND the existing neighbor's entry in `spawnPoints.json`.
7. **Enemy names must match** — The `enemyName` in `spawnPoints.json` must exactly match the `name` field in `enemies.json`.
