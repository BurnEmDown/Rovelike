# Task 01: Wire Up Interaction System

**Estimated Time:** 30-60 minutes  
**Prerequisites:** None  
**Status:** Not Started

---

## Context

The `PointerClickUserInteractionSource` component exists and is fully implemented in UnityCoreKit, but it's never instantiated or attached to `TileView` instances. The `BoardPresenter` spawns `TileView` objects via pooling but doesn't initialize any interaction sources.

**Current State:**
- ✅ `PointerClickUserInteractionSource.cs` implemented
- ✅ `UserInteractionEvent` infrastructure ready
- ✅ `IEventsManager` registered in CoreServices
- ❌ No interaction source attached to TileView prefab
- ❌ No initialization in BoardPresenter
- ❌ No verification that click events emit

**Goal:** Enable tile click detection by attaching and initializing `PointerClickUserInteractionSource` on each spawned `TileView`.

---

## Goals

1. Add `PointerClickUserInteractionSource` component to TileView prefab
2. Initialize the interaction source in `BoardPresenter.SpawnTileView` method
3. Add temporary logging to verify click events are emitted
4. Confirm EventSystem exists in scene (required for Unity's pointer event system)

---

## Implementation Steps

### Step 1: Add Component to TileView Prefab
**File:** TileView prefab (locate via AssetDatabase or scene)

1. Open the TileView prefab in Unity Editor
2. Add `PointerClickUserInteractionSource` component
3. Ensure the GameObject has a Collider2D (required for physics raycasting) or Image/UI component (required for UI raycasting)
4. Save prefab

**Alternative (code-based):**
- Add component dynamically in `BoardPresenter.SpawnTileView` if prefab modification isn't desired

### Step 2: Initialize Interaction Source in BoardPresenter
**File:** `/Assets/Scripts/Gameplay/Presentation/Board/BoardPresenter.cs`

**Location:** Inside `SpawnTileView` method, after `view.Init(tile, pos)` call

**Add:**
```csharp
// Initialize interaction source for click detection
var interactionSource = view.GetComponent<PointerClickUserInteractionSource>();
if (interactionSource != null)
{
    var interactions = CoreServices.Get<IUserInteractions>();
    interactionSource.Init(interactions, view); // TileView implements IUserInteractionTarget
}
else
{
    Debug.LogWarning($"[BoardPresenter] No PointerClickUserInteractionSource found on {view.name}");
}
```

**Required using statements:**
```csharp
using UnityCoreKit.Runtime.UserInteractions;
using UnityCoreKit.Runtime.UserInteractions.Unity;
using UnityCoreKit.Runtime.Core;
```

### Step 3: Add Temporary Verification Logging
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/BoardController.cs` (or create new test controller)

**Add in `Start()` or `Awake()`:**
```csharp
// Subscribe to tile click events for verification
var interactions = CoreServices.Get<IUserInteractions>();
interactions.Subscribe(UserInteractionType.Click, OnTileClicked, this);
```

**Add method:**
```csharp
private void OnTileClicked(UserInteractionEvent evt)
{
    if (evt.Target is TileView tileView)
    {
        Debug.Log($"[BoardController] Tile clicked: {tileView.Tile.TypeKey} at ({tileView.BoardPosition.X}, {tileView.BoardPosition.Y})");
    }
}
```

**Add cleanup:**
```csharp
private void OnDestroy()
{
    var interactions = CoreServices.Get<IUserInteractions>();
    interactions.UnsubscribeAll(this); // Cleanup owner-based subscriptions
}
```

### Step 4: Verify EventSystem Exists
**Check in Scene:**
- Open GameScene or InitScene
- Verify `EventSystem` GameObject exists (Create → UI → Event System if missing)
- Ensure `Physics2DRaycaster` or `GraphicRaycaster` component exists (depending on whether tiles use UI or physics colliders)

---

## Test Checklist

### Manual Tests
- [ ] Open GameScene in Unity Editor
- [ ] Enter Play Mode
- [ ] Click on a tile view in the scene
- [ ] Verify console log appears: `[BoardController] Tile clicked: <TypeKey> at (<X>, <Y>)`
- [ ] Click multiple different tiles
- [ ] Verify each click produces a unique log with correct tile info
- [ ] Click empty board space (no tile) - verify no log appears
- [ ] Exit Play Mode - verify no errors/warnings about unsubscribed listeners

### Code Verification
- [ ] `BoardPresenter.SpawnTileView` initializes `PointerClickUserInteractionSource`
- [ ] TileView prefab has `PointerClickUserInteractionSource` component
- [ ] TileView has Collider2D (if using physics) or is in Canvas (if using UI)
- [ ] EventSystem exists in scene hierarchy
- [ ] No null reference warnings in console during tile spawn
- [ ] Interaction source `Init` is called with valid `IUserInteractions` and `IUserInteractionTarget`

### Edge Cases
- [ ] Click rapidly on same tile - verify no double-processing
- [ ] Click during board rebuild - verify no errors
- [ ] Return tile to pool and re-spawn - verify interaction still works

---

## Success Criteria

- ✅ Clicking a tile in Play Mode emits `UserInteractionEvent` with `UserInteractionType.Click`
- ✅ Console logs show correct tile TypeKey and board position
- ✅ No null reference exceptions or warnings
- ✅ All pooled/re-spawned tiles respond to clicks correctly
- ✅ Event subscriptions clean up properly on scene unload (no leaked listeners)

---

## Notes

### Architectural Decisions
- **Why initialize in BoardPresenter?** - Keeps interaction setup co-located with view spawning. Alternative would be TileView.Awake, but that requires TileView to know about CoreServices.
- **Why temporary logging?** - Verifies the plumbing works before building selection/movement logic on top.
- **Physics vs UI raycasting?** - If tiles are sprites in world space, use Collider2D + Physics2DRaycaster. If tiles are UI elements, use Image + GraphicRaycaster.

### Common Issues
- **"Nothing happens when I click"** → Check EventSystem exists, verify Collider2D/Image component on TileView
- **"Null reference on interactions.Subscribe"** → CoreServices.Init() not called before BoardPresenter runs
- **"Click detected but wrong tile"** → Z-fighting or layering issue - check tile sorting layers

### Next Task
After this task passes tests, proceed to **Task_01b_TestInteractions.md** to add unit tests for the interaction system.

### Related Files
- `/Assets/UnityCoreKit/Runtime/UserInteractions/Unity/PointerClickUserInteractionSource.cs`
- `/Assets/Scripts/Gameplay/Presentation/Tiles/TileView.cs`
- `/Assets/Scripts/Gameplay/Presentation/Board/BoardPresenter.cs`
- `/Assets/UnityCoreKit/Runtime/UserInteractions/IUserInteractions.cs`
