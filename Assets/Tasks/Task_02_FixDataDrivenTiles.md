# Task 02: Fix Data-Driven Tile Creation

**Estimated Time:** 45-60 minutes  
**Prerequisites:** Task_01b_TestInteractions.md completed  
**Status:** Not Started

---

## Context

Currently, concrete tile classes (BrainTile, MotorTile, CoilTile, etc.) have **hardcoded MovementRules** in their constructors, completely bypassing the `TileDefinition` ScriptableObject system. This means changing a tile's movement in the Unity Editor (e.g., editing BrainDefinitionTile.asset) has **no effect** - the hardcoded values in code override it.

**Current State:**
```csharp
// BrainTile.cs - WRONG: Hardcoded rules
public BrainTile(int id, string typeKey) : base(id, typeKey,
    new DefaultMovementBehavior(new MovementRules(10, true, false)), // ← Hardcoded!
    new DefaultAbilityBehavior())
{
}
```

**Expected Flow:**
```
TileDefinition.asset → TileFactory.CreateTile() → TileConfig → EngineTileFactory.CreateTile() → ModuleTile
```

**Actual Flow:**
```
TileDefinition.asset → TileFactory.CreateTile() → TileConfig → EngineTileFactory.CreateTile() → ConcreteXYZTile (ignores config)
```

**Goal:** Make tile creation fully data-driven so ScriptableObject edits change runtime behavior without code changes.

---

## Goals

1. Remove hardcoded MovementRules from concrete tile constructors
2. Ensure EngineTileFactory creates generic ModuleTile instances (not concrete subclasses)
3. Verify TileFactory properly passes TileDefinition data through to engine
4. Test that editing a TileDefinition.asset changes runtime behavior

---

## Implementation Steps

### Step 1: Audit Current Factory Flow

**Read these files to understand current state:**
- `/Assets/Scripts/Gameplay/Game/Definitions/TileFactory.cs` (Unity layer)
- `/Assets/Scripts/Gameplay/Engine/Tiles/EngineTileFactory.cs` (Engine layer)
- `/Assets/Scripts/Gameplay/Engine/Tiles/TileConfig.cs` (Data transfer object)
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/*.cs` (Concrete tile classes)

**Current TileFactory.CreateTile() (correct):**
```csharp
public static ModuleTile CreateTile(TileDefinition def)
{
    var config = new TileConfig
    {
        TypeKey = def.typeKey,
        MovementRules = new MovementRules(
            def.maxMoveDistance,
            def.allowOrthogonal,
            def.allowDiagonal,
            def.passRule
        ),
    };

    return EngineTileFactory.CreateTile(config);
}
```

**Current EngineTileFactory.CreateTile() (correct):**
```csharp
public static ModuleTile CreateTile(TileConfig config)
{
    var id = nextId++;
    var movementBehavior = new DefaultMovementBehavior(config.MovementRules);

    return new ModuleTile(
        id,
        config.TypeKey,
        movementBehavior,
        config.AbilityBehavior
    );
}
```

**Problem:** The factory is correct, but nothing uses it for concrete tiles!

### Step 2: Identify Concrete Tile Usage

**Search for concrete tile instantiation:**
```bash
# Find where BrainTile, MotorTile, etc. are instantiated
grep -r "new BrainTile\|new MotorTile\|new CoilTile" Assets/Scripts/
```

**Expected finding:** These concrete classes are likely **never instantiated** - they exist but are unused dead code.

### Step 3: Delete Concrete Tile Classes (Recommended Approach)

**Files to delete:**
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/BrainTile.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/BrainTile.cs.meta`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/MotorTile.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/MotorTile.cs.meta`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/CoilTile.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/CoilTile.cs.meta`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/GripperTile.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/GripperTile.cs.meta`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/LaserTile.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/LaserTile.cs.meta`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/SensorTile.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/SensorTile.cs.meta`

**Alternative Approach (if concrete classes are needed later):**

If you decide concrete classes provide value (e.g., type-specific ability logic in future), update constructors to accept TileConfig:

```csharp
// BrainTile.cs - FIXED version
public class BrainTile : ModuleTile
{
    public BrainTile(int id, TileConfig config) 
        : base(
            id, 
            config.TypeKey,
            new DefaultMovementBehavior(config.MovementRules), // ← Use config, not hardcode
            config.AbilityBehavior)
    {
    }
}
```

Then update EngineTileFactory to instantiate concrete types based on TypeKey:
```csharp
public static ModuleTile CreateTile(TileConfig config)
{
    var id = nextId++;
    
    // Factory method pattern - create specific types based on TypeKey
    return config.TypeKey switch
    {
        "Brain" => new BrainTile(id, config),
        "Motor" => new MotorTile(id, config),
        "Coil" => new CoilTile(id, config),
        // ... etc
        _ => new ModuleTile(
            id,
            config.TypeKey,
            new DefaultMovementBehavior(config.MovementRules),
            config.AbilityBehavior)
    };
}
```

**Recommendation:** Use the **delete approach** unless you have a clear future need for concrete types.

### Step 4: Verify TileDefinition Assets

**Check each TileDefinition.asset:**
1. Open `/Assets/GameData/Tiles/BrainDefinitionTile.asset` in Inspector
2. Verify fields are set:
   - typeKey = "Brain"
   - maxMoveDistance = some value (e.g., 10)
   - allowOrthogonal = true/false
   - allowDiagonal = true/false
   - passRule = enum value

**Example expected values (from ROVE reverse-engineering):**
- Brain: 10 steps, orthogonal only, CannotPassThrough
- Motor: 1 step, orthogonal only, PushObstacles
- Coil: 10 steps, orthogonal+diagonal, MustPassThrough
- Gripper: 1 step, orthogonal+diagonal, CannotPassThrough
- Sensor: 10 steps, diagonal only, CanPassThrough
- Laser: (special behavior - implement later)

### Step 5: Test Data-Driven Behavior

**Manual Test:**
1. Open BrainDefinitionTile.asset
2. Change maxMoveDistance from 10 to 3
3. Enter Play Mode
4. Create a Brain tile on the board
5. Use movement calculation (will implement preview in Task 03)
6. Verify tile can only move 3 steps, not 10

**Code Test (add to BoardController for verification):**
```csharp
private void Start()
{
    CreateAndFillBoard();
    
    // Verify data-driven creation
    var brainDef = GameServices.TileLibrary.GetTileByTypeKey("Brain");
    var brainTile = TileFactory.CreateTile(brainDef);
    
    // Access movement behavior to check rules
    var moves = brainTile.GetAvailableMoves(new MoveContext(
        board, 
        new CellPos { X = 0, Y = 0 }, 
        new MovementRules(100, true, true) // Override rules for testing
    ));
    
    Debug.Log($"[DataDrivenTest] Brain tile created with ID {brainTile.Id}, TypeKey {brainTile.TypeKey}");
    Debug.Log($"[DataDrivenTest] Available moves: {moves.Count}");
}
```

---

## Test Checklist

### Code Verification
- [ ] All concrete tile classes deleted (BrainTile.cs, MotorTile.cs, etc.)
- [ ] EngineTileFactory.CreateTile returns generic ModuleTile
- [ ] TileFactory.CreateTile passes MovementRules from TileDefinition to TileConfig
- [ ] No compilation errors after deleting concrete classes
- [ ] No references to concrete tile types in codebase (use grep/search)

### Data Verification
- [ ] All 6 TileDefinition.asset files have valid movement rules
- [ ] typeKey matches between TileDefinition and TileLibrarySO
- [ ] maxMoveDistance is non-negative integer
- [ ] At least one of allowOrthogonal/allowDiagonal is true (unless tile can't move)

### Runtime Verification
- [ ] Edit BrainDefinitionTile.asset maxMoveDistance to 2
- [ ] Enter Play Mode
- [ ] Create Brain tile
- [ ] Call GetAvailableMoves with current board state
- [ ] Verify move count matches edited value (≤2 steps away)
- [ ] Change back to original value, verify behavior updates

### Edge Cases
- [ ] Tile with maxMoveDistance = 0 produces no moves
- [ ] Tile with allowOrthogonal=false, allowDiagonal=true only moves diagonally
- [ ] Tile with different passRule values behaves correctly

---

## Success Criteria

- ✅ No concrete tile classes exist (or they accept TileConfig if kept)
- ✅ EngineTileFactory creates generic ModuleTile instances
- ✅ Editing TileDefinition.asset changes runtime tile behavior
- ✅ No hardcoded MovementRules anywhere in concrete tile constructors
- ✅ All tiles created via TileFactory use ScriptableObject data
- ✅ No compilation errors or warnings

---

## Notes

### Why This Matters
- **Designer Empowerment** - Game designers can tweak tile movement without touching code
- **Rapid Iteration** - Test balance changes without recompiling
- **Data Consistency** - Single source of truth (ScriptableObject), not duplicated in code
- **Future-Proofing** - Supports runtime loading from JSON/remote config later

### Architectural Decision: Delete vs Refactor Concrete Classes

**Delete Approach (Recommended):**
- ✅ Simpler - fewer classes to maintain
- ✅ Faster iteration - edit data, not code
- ✅ Matches current architecture (composition over inheritance)
- ❌ Lose type safety (can't write `BrainTile` in method signatures)

**Refactor Approach (Alternative):**
- ✅ Type safety - `BrainTile` is a distinct type
- ✅ Future-proofing - can add tile-specific logic later
- ❌ More code to maintain
- ❌ Requires factory switch statement

**Decision:** Delete unless you have a specific reason for concrete types.

### Common Issues
- **"Tile still uses old hardcoded value"** → Domain reload issue. Restart Unity Editor.
- **"TileFactory returns null"** → Check TileLibraryService is initialized before CreateTile call
- **"MovementRules all zero"** → TileDefinition.asset fields not saved. Re-edit and save.

### Next Task
After this task passes, proceed to **Task_02b_TestDataDrivenTiles.md** to add unit tests for the factory system.

### Related Files
- `/Assets/Scripts/Gameplay/Game/Definitions/TileFactory.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/EngineTileFactory.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/TileConfig.cs`
- `/Assets/GameData/Tiles/*.asset` (all TileDefinition assets)
- `/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/*.cs` (to be deleted)
