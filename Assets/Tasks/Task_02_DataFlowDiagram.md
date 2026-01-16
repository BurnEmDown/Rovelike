# Tile Creation Data Flow

## Overview
Tiles are created through a multi-layer pipeline ensuring separation between Unity and engine code. This architecture enables data-driven design where gameplay behavior is configured through Unity Inspector rather than hardcoded in C#.

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│ Unity Inspector (Designer Edits)                       │
│ - BrainDefinitionTile.asset                            │
│   • typeKey: "Brain"                                   │
│   • maxMoveDistance: 10                                │
│   • allowOrthogonal: true                              │
│   • allowDiagonal: false                               │
│   • tileSprite: brain_sprite.png                       │
└────────────────┬────────────────────────────────────────┘
                 │
                 │ Loaded via TileLibraryService
                 ▼
┌─────────────────────────────────────────────────────────┐
│ TileLibrarySO (ScriptableObject)                       │
│ - Collection of all TileDefinitions                    │
│ - GetTileByTypeKey("Brain") → TileDefinition           │
└────────────────┬────────────────────────────────────────┘
                 │
                 │ TileDefinition reference
                 ▼
┌─────────────────────────────────────────────────────────┐
│ TileFactory (Unity Layer)                              │
│ CreateTile(TileDefinition def)                         │
│   1. Extract movement rules from ScriptableObject      │
│   2. Create TileConfig struct with extracted data      │
│   3. Call EngineTileFactory.CreateTile(config)         │
└────────────────┬────────────────────────────────────────┘
                 │
                 │ TileConfig { TypeKey, MovementRules, AbilityBehavior }
                 ▼
┌─────────────────────────────────────────────────────────┐
│ EngineTileFactory (Pure C# Engine Layer)               │
│ CreateTile(TileConfig config)                          │
│   1. Generate unique tile ID (auto-increment)          │
│   2. Create DefaultMovementBehavior(MovementRules)     │
│   3. Assign ability behavior from config               │
│   4. Return new ModuleTile(id, typeKey, behaviors)     │
└────────────────┬────────────────────────────────────────┘
                 │
                 │ Returns configured tile instance
                 ▼
┌─────────────────────────────────────────────────────────┐
│ ModuleTile (Engine Object)                             │
│ - Pure C# game logic (no Unity dependencies)           │
│ - Behavior-based movement via IMovementBehavior        │
│ - Behavior-based abilities via IAbilityBehavior        │
│ - Fully testable in edit-mode unit tests               │
└─────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### Unity Layer (Game)
**Files:**
- `TileDefinition.cs` - ScriptableObject data container
- `TileFactory.cs` - Unity → Engine translation
- `TileLibrarySO.cs` - Collection of all tile definitions

**Responsibilities:**
- Exposes designer-friendly interfaces (Inspector fields)
- Loads and manages ScriptableObject assets
- Translates Unity data types → plain C# types
- Handles sprite and visual configuration

**Dependencies:** Unity Engine, ScriptableObjects, Sprites

### Engine Layer (Gameplay.Engine)
**Files:**
- `EngineTileFactory.cs` - Pure C# tile factory
- `TileConfig.cs` - Data transfer object
- `ModuleTile.cs` - Tile implementation
- `IMovementBehavior.cs` - Movement behavior interface
- `IAbilityBehavior.cs` - Ability behavior interface

**Responsibilities:**
- Implements game logic without Unity dependencies
- Creates tiles from plain C# configuration
- Manages tile behaviors via composition
- Provides testable, fast-running code

**Dependencies:** None (pure C#)

## Why This Architecture?

### 1. Layer Separation
**Unity Layer**
- Knows about ScriptableObjects, Sprites, Unity types
- Runs in Unity Editor / Runtime
- Depends on UnityEngine.dll

**Engine Layer**
- Pure C# - no Unity dependencies
- Runs anywhere (tests, servers, non-Unity clients)
- Fast edit-mode tests (milliseconds, not seconds)

### 2. Data-Driven Design
**Old Approach (Hardcoded):**
```csharp
public class BrainTile : ModuleTile
{
    public BrainTile(int id) : base(id, "Brain",
        new DefaultMovementBehavior(
            new MovementRules(10, true, false) // ❌ Hardcoded!
        ))
    { }
}
```
- Changing movement requires code edit
- Requires recompilation
- Not accessible to designers

**New Approach (Data-Driven):**
```csharp
// Designer edits BrainDefinitionTile.asset in Inspector
// maxMoveDistance: 10 → 15 (no code change!)

var def = TileLibrary.GetTileByTypeKey("Brain");
var tile = TileFactory.CreateTile(def); // ✅ Uses data!
```
- Designers edit values in Inspector
- No recompilation needed
- Immediate iteration

### 3. Testability
**Engine tests run in milliseconds:**
```csharp
[Test]
public void BrainTile_MovesUpTo10Spaces()
{
    var config = new TileConfig
    {
        TypeKey = "Brain",
        MovementRules = new MovementRules(10, true, false)
    };
    
    var tile = EngineTileFactory.CreateTile(config);
    
    Assert.AreEqual("Brain", tile.TypeKey);
}
```
- No Unity Test Runner needed
- No Play Mode required
- Fast feedback loop

### 4. Composition Over Inheritance
**Old Design (Inheritance):**
```
ModuleTile
  ├─ BrainTile (hardcoded movement)
  ├─ MotorTile (hardcoded movement)
  └─ CoilTile (hardcoded movement)
```
- Behavior embedded in class hierarchy
- Difficult to change at runtime
- Limits flexibility

**New Design (Composition):**
```
ModuleTile
  ├─ IMovementBehavior (configurable)
  └─ IAbilityBehavior (configurable)
```
- Behaviors injected via constructor
- Easy to swap at runtime
- Maximum flexibility

## Example: Adding a New Tile

### Step 1: Create ScriptableObject Asset
1. Right-click in Unity Project window
2. Create → Rovelike → Tile Definition
3. Name it `GearTileDefinition.asset`

### Step 2: Configure in Inspector
```
typeKey: "Gear"
maxMoveDistance: 4
allowOrthogonal: true
allowDiagonal: true
tileSprite: gear_sprite.png
```

### Step 3: Add to TileLibrarySO
1. Open `TileLibrarySO.asset`
2. Add `GearTileDefinition` to the tiles array

### Step 4: Use in Game
```csharp
var gearDef = TileLibrary.GetTileByTypeKey("Gear");
var gearTile = TileFactory.CreateTile(gearDef);
// Done! Tile works with configured behavior.
```

**No code changes required!**

## Data Transfer Object Pattern

### TileConfig - The Bridge Between Layers

**Purpose:** Transfer tile configuration from Unity layer → Engine layer without Unity dependencies.

**Structure:**
```csharp
public sealed class TileConfig
{
    public string TypeKey { get; set; }           // Plain C# string
    public string DisplayName { get; set; }       // Plain C# string
    public MovementRules MovementRules { get; set; } // Plain C# struct
    public IAbilityBehavior AbilityBehavior { get; set; } // Interface
}
```

**Why a separate class?**
- Engine can't reference `TileDefinition` (Unity dependency)
- `TileDefinition` is a ScriptableObject (heavy, serialization-focused)
- `TileConfig` is lightweight, plain C#, easily testable

**Flow:**
```
TileDefinition (Unity) → TileFactory → TileConfig (DTO) → EngineTileFactory → ModuleTile
```

## Comparison: Before vs After

### Before (Hardcoded)
| Aspect | Old Approach |
|--------|-------------|
| **Adding new tile** | Write new C# class, recompile |
| **Changing movement** | Edit code, recompile |
| **Testing** | Unity Play Mode tests (slow) |
| **Designer access** | None - requires programmer |
| **Runtime changes** | Not possible |

### After (Data-Driven)
| Aspect | New Approach |
|--------|-------------|
| **Adding new tile** | Create ScriptableObject asset |
| **Changing movement** | Edit Inspector values |
| **Testing** | Fast edit-mode tests |
| **Designer access** | Full control via Inspector |
| **Runtime changes** | Possible via config loading |

## Common Patterns

### Pattern 1: Loading Tiles by TypeKey
```csharp
var brainDef = TileLibrary.GetTileByTypeKey("Brain");
var brainTile = TileFactory.CreateTile(brainDef);
```

### Pattern 2: Creating Tiles in Tests
```csharp
[Test]
public void TestTileMovement()
{
    var config = new TileConfig
    {
        TypeKey = "TestTile",
        MovementRules = new MovementRules(5, true, false)
    };
    
    var tile = EngineTileFactory.CreateTile(config);
    // Test tile behavior...
}
```

### Pattern 3: Batch Creating Tiles
```csharp
var tiles = new List<ModuleTile>();
foreach (var typeKey in new[] { "Brain", "Motor", "Coil" })
{
    var def = TileLibrary.GetTileByTypeKey(typeKey);
    tiles.Add(TileFactory.CreateTile(def));
}
```

## Future Extensions

### Runtime Configuration Loading
```csharp
// Load tile definitions from JSON
var json = File.ReadAllText("tiles.json");
var config = JsonUtility.FromJson<TileConfig>(json);
var tile = EngineTileFactory.CreateTile(config);
```

### Procedural Tile Generation
```csharp
// Generate random tile configurations
var randomConfig = new TileConfig
{
    TypeKey = "Random" + Random.Range(0, 100),
    MovementRules = GenerateRandomMovementRules()
};
```

### Tile Modification
```csharp
// Modify tile behavior at runtime (power-ups, debuffs)
var modifiedRules = new MovementRules(
    tile.MovementRules.MaxSteps + 2,  // Buff: +2 range
    tile.MovementRules.AllowOrthogonal,
    tile.MovementRules.AllowDiagonal
);
```

## Troubleshooting

### "Factory returns tile with wrong movement rules"
✅ **Check:** TileDefinition ScriptableObject has correct values
✅ **Check:** TileFactory is passing rules to TileConfig correctly
✅ **Check:** EngineTileFactory is using MovementRules from config

### "Changes to ScriptableObject not reflected in runtime"
✅ **Solution:** Restart Unity (domain reload issue)
✅ **Solution:** Re-save the ScriptableObject asset

### "Tests can't find TileDefinition"
✅ **Reminder:** Engine tests should use TileConfig directly, NOT TileDefinition
✅ **Solution:** Create mock TileConfig in test instead of loading ScriptableObject

## Related Files

### Unity Layer
- `/Assets/Scripts/Gameplay/Game/Definitions/TileFactory.cs`
- `/Assets/Scripts/Gameplay/Game/Definitions/TileDefinition.cs`
- `/Assets/Scripts/Gameplay/Game/Services/TileLibraryService.cs`
- `/Assets/GameData/Libraries/TileLibrarySO.asset`

### Engine Layer
- `/Assets/Scripts/Gameplay/Engine/Tiles/EngineTileFactory.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/TileConfig.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ModuleTile.cs`
- `/Assets/Scripts/Gameplay/Engine/Moves/MovementRules.cs`

### Tests
- `/Assets/Scripts/Gameplay/Engine/Tests/EngineTileFactoryTests.cs`
- `/Assets/Scripts/Gameplay/Engine/Tests/TileConfigTests.cs`
- `/Assets/Scripts/Gameplay/Game/Tests/TileFactoryIntegrationTests.cs`
