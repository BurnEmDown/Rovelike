# Task 02c: Documentation - Data-Driven Tile System

**Estimated Time:** 15-20 minutes  
**Prerequisites:** Task_02b_TestDataDrivenTiles.md completed  
**Status:** Complete

---

## Context

Tasks 02 and 02b implemented and tested data-driven tile creation. Before moving to movement execution, we need to document:
- The factory pattern used for tile creation
- The data flow from ScriptableObjects to engine tiles
- The TileConfig transfer object pattern
- Design decisions around modularity and data-driven design

**Current State:**
- ✅ Data-driven tile creation implemented and tested
- ❌ Factory classes need better documentation
- ❌ TileConfig pattern not explained
- ❌ ScriptableObject → Engine flow not documented

**Goal:** Document the complete data-driven tile creation pipeline.

---

## Goals

1. Document TileFactory and EngineTileFactory classes
2. Explain the TileConfig transfer object pattern
3. Document TileDefinition ScriptableObject structure
4. Add comments explaining why concrete tile classes were removed (if applicable)
5. Document the data flow diagram

---

## Implementation Steps

### Step 1: Document TileFactory
**File:** `/Assets/Scripts/Gameplay/Game/Definitions/TileFactory.cs`

Add comprehensive XML documentation:
```csharp
/// <summary>
/// Unity-layer factory for creating engine tiles from ScriptableObject definitions.
/// 
/// <para>
/// This factory sits at the boundary between Unity (ScriptableObjects) and the engine (pure C#).
/// It translates Unity-side TileDefinition data into engine-compatible TileConfig,
/// then delegates to EngineTileFactory for actual tile instantiation.
/// </para>
/// 
/// <para>
/// Design Decision: Why Two Factories?
/// - TileFactory (Unity layer): Knows about ScriptableObjects, Unity types
/// - EngineTileFactory (Engine layer): Pure C#, no Unity dependencies, fully testable
/// This separation keeps the engine testable without Unity's playmode test harness.
/// </para>
/// </summary>
public static class TileFactory
{
    /// <summary>
    /// Creates an engine tile from a Unity ScriptableObject definition.
    /// </summary>
    /// <param name="def">The ScriptableObject tile definition containing movement rules and visual data.</param>
    /// <returns>A fully initialized ModuleTile with behaviors configured from the definition.</returns>
    /// <remarks>
    /// Data flow:
    /// 1. Extract movement rules from TileDefinition
    /// 2. Create TileConfig transfer object
    /// 3. Pass to EngineTileFactory.CreateTile()
    /// 4. Return engine tile
    /// </remarks>
    public static ModuleTile CreateTile(TileDefinition def)
    {
        // ... implementation
    }
}
```

### Step 2: Document EngineTileFactory
**File:** `/Assets/Scripts/Gameplay/Engine/Tiles/EngineTileFactory.cs`

```csharp
/// <summary>
/// Pure C# factory for creating engine-layer tiles from configuration data.
/// 
/// <para>
/// This factory has ZERO Unity dependencies, making it fully testable
/// in fast edit-mode tests. It receives TileConfig (a plain C# struct)
/// and constructs ModuleTile instances with appropriate behaviors.
/// </para>
/// 
/// <para>
/// Design Decision: Generic Tiles vs Concrete Subclasses
/// This factory creates generic ModuleTile instances configured via
/// composition (behaviors), NOT specialized subclasses (BrainTile, MotorTile).
/// This makes the system fully data-driven: changing a tile's behavior
/// requires editing a ScriptableObject, not writing code.
/// </para>
/// </summary>
public static class EngineTileFactory
{
    private static int nextId = 1;
    
    /// <summary>
    /// Creates a ModuleTile from engine-layer configuration data.
    /// Assigns a unique ID and initializes behaviors from the config.
    /// </summary>
    /// <param name="config">Configuration containing movement rules and tile metadata.</param>
    /// <returns>A fully configured ModuleTile ready for board placement.</returns>
    /// <remarks>
    /// The tile ID is auto-incremented globally. Future versions may support
    /// custom ID generation strategies or ID pooling for save/load systems.
    /// </remarks>
    public static ModuleTile CreateTile(TileConfig config)
    {
        // ... implementation
    }
}
```

### Step 3: Document TileConfig
**File:** `/Assets/Scripts/Gameplay/Engine/Tiles/TileConfig.cs`

```csharp
/// <summary>
/// Transfer object carrying tile configuration data from Unity layer to engine layer.
/// 
/// <para>
/// This struct acts as the boundary between Unity-dependent code (TileFactory)
/// and engine code (EngineTileFactory). It contains only plain C# types,
/// ensuring the engine remains Unity-free.
/// </para>
/// 
/// <para>
/// Design Pattern: Data Transfer Object (DTO)
/// TileConfig is a pure data container with no behavior. It's created by
/// TileFactory from Unity ScriptableObjects, then consumed by EngineTileFactory
/// to construct actual tile instances.
/// </para>
/// </summary>
public struct TileConfig
{
    /// <summary>
    /// String identifier for this tile type (e.g., "Brain", "Motor", "Coil").
    /// Used for lookup, serialization, and debug display.
    /// </summary>
    public string TypeKey;
    
    /// <summary>
    /// Movement behavior configuration defining how this tile can move.
    /// Includes max distance, direction constraints, and pass-through rules.
    /// </summary>
    public MovementRules MovementRules;
    
    /// <summary>
    /// (Optional) Custom ability behavior. If null, defaults to DefaultAbilityBehavior.
    /// </summary>
    public IAbilityBehavior? AbilityBehavior;
}
```

### Step 4: Document TileDefinition ScriptableObject
**File:** `/Assets/Scripts/Gameplay/Game/Definitions/TileDefinition.cs`

```csharp
/// <summary>
/// Unity ScriptableObject defining a tile type's visual and behavioral properties.
/// 
/// <para>
/// This is the designer-facing data format. All tile properties are editable
/// in the Unity Inspector without touching code. Changes made here automatically
/// affect runtime tile behavior via the factory system.
/// </para>
/// 
/// <para>
/// Architecture Note: Separation of Data and Logic
/// - TileDefinition: Data only (ScriptableObject, serializable)
/// - ModuleTile: Logic only (engine C#, behaviors)
/// - TileFactory: Translation layer between the two
/// </para>
/// </summary>
[CreateAssetMenu(fileName = "NewTileDefinition", menuName = "Rovelike/Tile Definition")]
public class TileDefinition : ScriptableObject
{
    [Header("Identity")]
    /// <summary>
    /// Unique string identifier for this tile type.
    /// Must match across all systems (factories, serialization, logic).
    /// </summary>
    public string typeKey = "";
    
    [Header("Movement")]
    /// <summary>
    /// Maximum distance this tile can move in a single action.
    /// Engine validation ensures moves don't exceed this value.
    /// </summary>
    public int maxMoveDistance = 1;
    
    /// <summary>
    /// Whether this tile can move in orthogonal directions (up, down, left, right).
    /// </summary>
    public bool allowOrthogonal = true;
    
    /// <summary>
    /// Whether this tile can move diagonally.
    /// If both orthogonal and diagonal are false, the tile cannot move.
    /// </summary>
    public bool allowDiagonal = false;
    
    // ... rest of fields
}
```

### Step 5: Add Data Flow Diagram
**File:** `/Assets/Tasks/Task_02_DataFlowDiagram.md` (new file)

```markdown
# Tile Creation Data Flow

## Overview
Tiles are created through a multi-layer pipeline ensuring separation between Unity and engine code.

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│ Unity Inspector (Designer Edits)                       │
│ - BrainDefinitionTile.asset                            │
│   • typeKey: "Brain"                                   │
│   • maxMoveDistance: 10                                │
│   • allowOrthogonal: true                              │
│   • tileSprite: brain_sprite.png                       │
└────────────────┬────────────────────────────────────────┘
                 │
                 │ Loaded via TileLibraryService
                 ▼
┌─────────────────────────────────────────────────────────┐
│ TileLibrarySO (ScriptableObject)                       │
│ - Collection of all TileDefinitions                    │
└────────────────┬────────────────────────────────────────┘
                 │
                 │ GetTileByTypeKey("Brain")
                 ▼
┌─────────────────────────────────────────────────────────┐
│ TileFactory (Unity Layer)                              │
│ CreateTile(TileDefinition def)                         │
│   1. Extract movement rules                            │
│   2. Create TileConfig struct                          │
│   3. Call EngineTileFactory.CreateTile(config)         │
└────────────────┬────────────────────────────────────────┘
                 │
                 │ TileConfig { TypeKey, MovementRules }
                 ▼
┌─────────────────────────────────────────────────────────┐
│ EngineTileFactory (Pure C# Engine Layer)               │
│ CreateTile(TileConfig config)                          │
│   1. Generate unique ID                                │
│   2. Create DefaultMovementBehavior(config.MovementRules)│
│   3. Create DefaultAbilityBehavior()                   │
│   4. Return new ModuleTile(id, typeKey, behaviors)     │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────┐
│ ModuleTile (Engine Object)                             │
│ - Pure C# game logic                                   │
│ - No Unity dependencies                                │
│ - Fully testable in edit-mode                          │
└─────────────────────────────────────────────────────────┘
```

## Why This Architecture?

### Layer Separation
- **Unity Layer**: Knows about ScriptableObjects, Sprites, Unity types
- **Engine Layer**: Pure C#, no Unity dependencies, fast tests

### Data-Driven Design
- Designers edit ScriptableObjects in Inspector
- No code changes needed to add/modify tile types
- Same code supports infinite tile variations

### Testability
- Engine tests run in milliseconds (edit-mode)
- No need to enter Play Mode to test tile logic
- Can test tile behavior with simple C# unit tests

## Example: Adding a New Tile

1. Create new ScriptableObject: `GearTileDefinition.asset`
2. Set properties in Inspector (typeKey, movement, sprite)
3. Add to TileLibrarySO
4. Done! No code required.

The factory system automatically creates engine tiles with correct behavior.
```

---

## Success Criteria

- ✅ All factory classes have comprehensive XML documentation
- ✅ TileConfig pattern is explained with design rationale
- ✅ Data flow from ScriptableObject → Engine is documented
- ✅ Design decisions (why factories, why no concrete classes) are clear
- ✅ New developers can understand how to add tiles without asking

---

## Notes

### Why Remove Concrete Tile Classes?
If concrete classes (BrainTile, MotorTile) still exist, they should be deleted after this documentation is complete. The documentation should explain:
- Old approach: Hardcoded MovementRules in BrainTile constructor
- New approach: Generic ModuleTile + data-driven configuration
- Benefit: Add/modify tiles without touching code

### Documentation Location
- Code documentation: XML comments in source files
- Architecture documentation: Separate .md files in /Assets/Tasks/
- Both are important: XML for IntelliSense, .md for high-level understanding

---

### Next Task
After documentation is complete, proceed to **Task_03_MovementExecution.md**.
