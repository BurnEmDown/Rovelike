# Task 02: Fix Data-Driven Tile Creation

**Estimated Time:** 45-60 minutes  
**Prerequisites:** Task_01c_Documentation.md completed  
**Status:** Complete

---

## Context

Currently, concrete tile classes (BrainTile, MotorTile, CoilTile, etc.) have **hardcoded MovementRules** in their constructors, completely bypassing the `TileDefinition` ScriptableObject system. This means changing a tile's movement in the Unity Editor (e.g., editing BrainDefinitionTile.asset) has **no effect** - the hardcoded values in code override it.

**Current State (Updated):**
- ✅ TileFactory correctly translates TileDefinition → TileConfig
- ✅ EngineTileFactory creates ModuleTile with correct rules from config
- ✅ BoardController uses TileFactory to create tiles
- ✅ Concrete tile classes (BrainTile, MotorTile, CoilTile) still exist with hardcoded rules
- ⚠️ Concrete tiles are NOT being used currently (TileFactory creates generic ModuleTiles)
- ⚠️ Concrete tile classes should be deleted as they're bypassed by the factory

**Expected Flow:**
```
TileDefinition.asset → TileFactory.CreateTile() → TileConfig → EngineTileFactory.CreateTile() → ModuleTile
```

**Actual Flow:**
```
TileDefinition.asset → TileFactory.CreateTile() → TileConfig → EngineTileFactory.CreateTile() → ConcreteXYZTile (ignores config)
```

**Goal:** Complete the data-driven tile creation by removing obsolete concrete tile classes that bypass the factory system.

---

## Goals

1. ✅ ~~Remove hardcoded MovementRules from concrete tile constructors~~ (Already bypassed by factory)
2. ✅ ~~Ensure EngineTileFactory creates generic ModuleTile instances~~ (Already working)
3. ✅ ~~Verify TileFactory properly passes TileDefinition data through~~ (Already working)
4. ❌ Delete obsolete concrete tile classes (BrainTile, MotorTile, CoilTile, etc.)
5. ❌ Verify no code references the deleted concrete classes
6. ❌ Test that editing TileDefinition ScriptableObjects changes runtime behavior

---

## Implementation Steps

### Step 1: Verify Current Implementation is Data-Driven

**Good news: The factory system is already working correctly!**

Check these files to confirm:
- `/Assets/Scripts/Gameplay/Game/Definitions/TileFactory.cs` (Unity layer) ✅
- `/Assets/Scripts/Gameplay/Engine/Tiles/EngineTileFactory.cs` (Engine layer) ✅
- `/Assets/Scripts/Gameplay/Game/Controllers/BoardController.cs` (Uses TileFactory.CreateTile()) ✅

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

### Step 2: Delete Obsolete Concrete Tile Classes

**Why Delete Them?**
The concrete tile classes (BrainTile, MotorTile, etc.) are no longer used. The factory system creates generic `ModuleTile` instances configured via composition, which is more flexible and data-driven.

**Files to delete:**
```
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/BrainTile.cs
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/BrainTile.cs.meta
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/MotorTile.cs
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/MotorTile.cs.meta
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/CoilTile.cs
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/CoilTile.cs.meta
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/GripperTile.cs
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/GripperTile.cs.meta
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/LaserTile.cs
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/LaserTile.cs.meta
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/SensorTile.cs
/Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/SensorTile.cs.meta
```

**Steps:**
1. In Unity, right-click each file in the Project window
2. Select "Delete"
3. Confirm deletion
4. Unity will automatically remove the .meta files

**Or via command line:**
```bash
cd /Assets/Scripts/Gameplay/Engine/Tiles/ConcreteTiles/
rm BrainTile.cs BrainTile.cs.meta
rm MotorTile.cs MotorTile.cs.meta
rm CoilTile.cs CoilTile.cs.meta
rm GripperTile.cs GripperTile.cs.meta
rm LaserTile.cs LaserTile.cs.meta
rm SensorTile.cs SensorTile.cs.meta
```

### Step 3: Search for References to Deleted Classes

**Verify nothing references the concrete classes:**
```bash
# Search for any code referencing concrete tile types
grep -r "BrainTile\|MotorTile\|CoilTile\|GripperTile\|LaserTile\|SensorTile" Assets/Scripts/ --include="*.cs"
```

**Expected result:** No matches (or only comments/deleted files)

If you find references:
- Update code to use `ModuleTile` instead
- Use TypeKey checks instead of type checks: `if (tile.TypeKey == "Brain")` not `if (tile is BrainTile)`
public static ModuleTile CreateTile(TileConfig config)
{
    var id = nextId++;
    
    // Factory method pattern - create specific types based on TypeKey
    return config.TypeKey switch
    {
### Step 4: Test Data-Driven Behavior

**Verify the system is fully data-driven:**

1. **Check Current TileDefinition Values:**
   - Open `/Assets/GameData/Tiles/BrainDefinitionTile.asset` in Inspector
   - Note the current `maxMoveDistance` value
   
2. **Test Runtime Behavior Reflects ScriptableObject:**
   - Enter Play Mode
   - Observe tile creation logs in console
   - Verify the Brain tile has the movement rules from the ScriptableObject

3. **Test Changing ScriptableObject Updates Behavior:**
   - Exit Play Mode
   - Change BrainDefinitionTile.maxMoveDistance to a different value (e.g., 5)
   - Enter Play Mode again
   - Verify new value is used (check logs or use MovementCalculator in Task 03)
   - Change value back to original

**Validation Logging (already in BoardController):**
The BoardController already logs tile creation, so you can verify the factory is working:
```
[CreateTiles] Created tile: Brain with max distance: 10
```
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
