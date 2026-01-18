# Task 04: Implement Empty Cell Click Detection

**Estimated Time:** 30-45 minutes  
**Prerequisites:** Task_03c_Documentation.md completed  
**Status:** Not Started

---

## Context

Currently, the movement system is implemented but tiles cannot actually move because there's no way to detect clicks on empty board cells. The `DestinationClickHandler` has the validation logic but lacks a mechanism to detect when the player clicks on an empty destination.

**Current State:**
- ✅ TileSelectionController selects tiles on click
- ✅ MovePreviewController shows valid move destinations
- ✅ MoveExecutor can execute moves
- ✅ DestinationClickHandler has `TryMoveSelectedTileTo()` method
- ❌ No way to detect clicks on empty board cells
- ❌ Tiles cannot actually move in manual testing

**Goal:** Implement a system to detect clicks on empty board cells and trigger tile movement.

---

## Goals

1. Add colliders to board grid for empty cell detection
2. Create a component that converts empty cell clicks to board positions
3. Wire up empty cell clicks to call `DestinationClickHandler.TryMoveSelectedTileTo()`
4. Test complete move flow: select tile → see highlights → click destination → tile moves

---

## Implementation Options

### Option A: Screen-to-Board Raycasting (Recommended)
Convert screen click position to board coordinates without requiring colliders on every cell.

**Pros:**
- No GameObject per cell
- Better performance
- Cleaner hierarchy

**Cons:**
- Requires math to convert screen → world → board
- Need to handle camera properly

### Option B: Invisible Collider Grid
Create colliders for each board cell that detect clicks.

**Pros:**
- Uses Unity's built-in physics system
- Simple to implement

**Cons:**
- Creates many GameObjects (width × height)
- More overhead

### Option C: Click on Highlights (MVP)
Make the highlight objects themselves clickable.

**Pros:**
- Very simple
- Reuses existing highlights

**Cons:**
- Only valid destinations are clickable
- Less intuitive UX

---

## Implementation Steps (Option A - Recommended)

### Step 1: Create BoardCoordinateUtility

**File:** `/Assets/Scripts/Gameplay/Presentation/Board/BoardCoordinateUtility.cs`

Create a static utility class for coordinate conversions:

```csharp
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Presentation.Board
{
    /// <summary>
    /// Utility class for converting between world coordinates and board cell positions.
    /// </summary>
    public static class BoardCoordinateUtility
    {
        /// <summary>
        /// Converts world position to board cell position.
        /// </summary>
        /// <param name="worldPosition">Position in world space</param>
        /// <param name="origin">World position of board cell (0,0)</param>
        /// <param name="cellSize">Size of each board cell in world units</param>
        /// <returns>Board cell position</returns>
        public static CellPos WorldToBoardPos(Vector3 worldPosition, Vector3 origin, Vector2 cellSize)
        {
            var localPos = worldPosition - origin;
            int x = Mathf.RoundToInt(localPos.x / cellSize.x);
            int y = Mathf.RoundToInt(localPos.y / cellSize.y);
            
            return new CellPos { X = x, Y = y };
        }

        /// <summary>
        /// Converts board cell position to world position (center of cell).
        /// </summary>
        /// <param name="boardPos">Board cell position</param>
        /// <param name="origin">World position of board cell (0,0)</param>
        /// <param name="cellSize">Size of each board cell in world units</param>
        /// <returns>World position at center of cell</returns>
        public static Vector3 BoardToWorldPos(CellPos boardPos, Vector3 origin, Vector2 cellSize)
        {
            return origin + new Vector3(
                boardPos.X * cellSize.x,
                boardPos.Y * cellSize.y,
                0
            );
        }

        /// <summary>
        /// Converts screen position to world position using the main camera.
        /// </summary>
        /// <param name="screenPosition">Screen space position</param>
        /// <returns>World position</returns>
        public static Vector3 ScreenToWorldPos(Vector3 screenPosition)
        {
            return Camera.main.ScreenToWorldPoint(screenPosition);
        }
    }
}
```

### Step 2: Add Conversion Methods to BoardPresenter

**File:** `/Assets/Scripts/Gameplay/Presentation/Board/BoardPresenter.cs`

Add these methods that use the utility class:

```csharp
/// <summary>
/// Converts screen position to board cell position.
/// </summary>
public CellPos? ScreenToBoardPos(Vector3 screenPosition)
{
    var worldPos = BoardCoordinateUtility.ScreenToWorldPos(screenPosition);
    return WorldToBoardPos(worldPos);
}

/// <summary>
/// Converts world position to board cell position.
/// Returns null if outside board bounds.
/// </summary>
public CellPos? WorldToBoardPos(Vector3 worldPosition)
{
    var pos = BoardCoordinateUtility.WorldToBoardPos(worldPosition, origin, cellSize);
    
    if (board != null && board.IsInsideBounds(pos))
        return pos;
    
    return null;
}

/// <summary>
/// Converts board position to world position (center of cell).
/// </summary>
public Vector3 BoardToWorldPos(CellPos boardPos)
{
    return BoardCoordinateUtility.BoardToWorldPos(boardPos, origin, cellSize);
}
```

### Step 3: Create EmptyCellClickDetector Component

**File:** `/Assets/Scripts/Gameplay/Game/Controllers/EmptyCellClickDetector.cs`

```csharp
using Gameplay.Presentation.Board;
using Gameplay.Presentation.Tiles;
using UnityCoreKit.Runtime.Core;
using UnityCoreKit.Runtime.Core.Services;
using UnityCoreKit.Runtime.UserInteractions;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Detects clicks on empty board cells and converts them to board positions.
    /// </summary>
    public class EmptyCellClickDetector : MonoBehaviour
    {
        [SerializeField] private BoardPresenter boardPresenter = null!;
        [SerializeField] private DestinationClickHandler destinationClickHandler = null!;

        private IUserInteractions? interactions;

        private void Awake()
        {
            interactions = CoreServices.Get<IUserInteractions>();
            interactions.Subscribe(this, OnClick);
        }

        private void OnDestroy()
        {
            interactions?.UnsubscribeAll(this);
        }

        private void OnClick(UserInteractionEvent evt)
        {
            // If clicking on a tile, ignore (let TileSelectionController handle it)
            if (evt.Target is TileView)
                return;

            // Convert screen position to board position
            var boardPos = boardPresenter.ScreenToBoardPos(evt.WorldPosition);
            
            if (boardPos.HasValue)
            {
                Debug.Log($"[EmptyCellClickDetector] Clicked board cell: ({boardPos.Value.X}, {boardPos.Value.Y})");
                destinationClickHandler.TryMoveSelectedTileTo(boardPos.Value);
            }
        }
    }
}
```

### Step 4: Add EmptyCellClickDetector to BoardController

**Update BoardController:**
1. Add `EmptyCellClickDetector` component
2. Assign `boardPresenter` and `destinationClickHandler` references in Inspector

### Step 5: Test Complete Flow

**Manual Test:**
1. Enter Play Mode
2. Click a tile → Should select and show highlights
3. Click an empty cell (highlighted or not):
   - If highlighted → Tile should move there
   - If not highlighted → Should log warning "Move not valid"
4. After successful move → Selection should clear, highlights should disappear

---

## Alternative: Option C Implementation (Quick MVP)

### Make Highlights Clickable

**Update MovePreviewController.CreateHighlight():**

```csharp
private void CreateHighlight(CellPos pos)
{
    if (highlightPrefab == null)
    {
        Debug.LogWarning("[MovePreviewController] Highlight prefab not assigned!");
        return;
    }

    var worldPos = BoardToWorld(pos);
    var highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity, highlightParent);

    // Apply color
    var spriteRenderer = highlight.GetComponent<SpriteRenderer>();
    if (spriteRenderer != null)
    {
        spriteRenderer.color = highlightColor;
    }

    // Add collider if not present
    if (highlight.GetComponent<Collider2D>() == null)
    {
        var collider = highlight.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
    }

    // Add click detection component
    var clickable = highlight.AddComponent<HighlightClickHandler>();
    clickable.Init(pos, destinationClickHandler);

    activeHighlights.Add(highlight);
}
```

**Create HighlightClickHandler:**

```csharp
public class HighlightClickHandler : MonoBehaviour, IPointerClickHandler
{
    private CellPos destination;
    private DestinationClickHandler? clickHandler;

    public void Init(CellPos dest, DestinationClickHandler handler)
    {
        destination = dest;
        clickHandler = handler;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickHandler?.TryMoveSelectedTileTo(destination);
    }
}
```

---

## Test Checklist

### Setup Verification
- [ ] BoardPresenter has position conversion methods
- [ ] EmptyCellClickDetector component added to scene
- [ ] EmptyCellClickDetector references assigned
- [ ] EventSystem exists in scene

### Manual Testing
- [ ] Click tile → Selection works
- [ ] Highlights appear at valid destinations
- [ ] Click highlighted empty cell → Tile moves
- [ ] Click non-highlighted empty cell → Warning logged, no move
- [ ] After move → Selection clears, highlights disappear
- [ ] Click outside board → Nothing happens, no errors

### Edge Cases
- [ ] Click tile with no valid moves → No highlights, no errors
- [ ] Click destination occupied by another tile → Move rejected
- [ ] Click very fast on multiple cells → No crashes or duplicate moves
- [ ] Deselect tile (click same tile) → Highlights clear, clicks do nothing

---

## Success Criteria

- ✅ Empty board cells are clickable
- ✅ Clicks on valid destinations execute moves
- ✅ Clicks on invalid destinations show warnings
- ✅ Complete flow works: select → preview → click → move
- ✅ Selection and highlights clear after move
- ✅ No errors or crashes during rapid clicking

---

## Notes

### Why Option A is Recommended
- **Performance**: No extra GameObjects or colliders
- **Flexibility**: Easy to adjust cell size or board offset
- **Simplicity**: One component handles all empty clicks
- **Reusability**: Coordinate conversion logic in utility class can be used elsewhere

### Utility Class Benefits
- **Testability**: Pure static methods are easy to unit test
- **Separation of Concerns**: BoardPresenter handles bounds checking, utility handles math
- **Configurability**: Supports different origins and cell sizes (not hardcoded)
- **Reuse**: Other systems can convert coordinates without needing BoardPresenter reference

### Camera Assumptions
The implementation assumes:
- Orthographic camera (for ScreenToWorldPos conversion)
- Board cells on z=0 plane

The utility class supports configurable:
- ✅ Cell size (via `cellSize` parameter)
- ✅ Board origin (via `origin` parameter)
- ✅ Different board layouts and scaling

### Future Improvements
- Add visual feedback for invalid clicks (red flash)
- Add hover preview (ghost tile at mouse position)
- Support touch input for mobile
- Support gamepad/keyboard movement selection

---

## Next Task

After this task works, proceed to **Task_04b_TestEmptyCellClick.md** for automated tests.

---

## Related Files

- `/Assets/Scripts/Gameplay/Presentation/Board/BoardCoordinateUtility.cs` (NEW)
- `/Assets/Scripts/Gameplay/Presentation/Board/BoardPresenter.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/EmptyCellClickDetector.cs` (NEW)
- `/Assets/Scripts/Gameplay/Game/Controllers/DestinationClickHandler.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/TileSelectionController.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/MovePreviewController.cs`
