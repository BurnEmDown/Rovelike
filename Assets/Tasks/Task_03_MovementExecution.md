# Task 03: Implement Movement Execution

**Estimated Time:** 60 minutes  
**Prerequisites:** Task_02c_Documentation.md completed  
**Status:** Not Started

---

## Context

Currently, movement **calculation** is fully implemented (`MovementCalculator.GetAvailableMoves`), but movement **execution** doesn't exist. Tiles can compute where they can move, but there's no system to:
1. Select a tile via click
2. Show move preview (highlight valid destinations)
3. Execute the move on destination click
4. Update both engine state and view

This task creates the **first complete gameplay loop**: Click tile → See moves → Click destination → Tile moves.

**Current State:**
- ✅ `MovementCalculator.GetAvailableMoves` works
- ✅ `BoardState.MoveTile` exists
- ✅ `BoardPresenter.MoveView` exists
- ✅ Tile click events emit (Task 01)
- ❌ No tile selection state tracking
- ❌ No move preview visualization
- ❌ No coordination between engine + presentation layers

**Goal:** Implement complete move execution: selection → preview → execution → view update.

---

## Goals

1. Create `TileSelectionController` to track selected tile state
2. Create `MovePreviewController` to visualize valid move destinations
3. Create `MoveExecutor` to coordinate `BoardState.MoveTile` + `BoardPresenter.MoveView`
4. Wire up the complete interaction flow: tile click → preview → destination click → move

---

## Implementation Steps

### Step 1: Create TileSelectionController
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/TileSelectionController.cs`

```csharp
using Gameplay.Engine.Tiles;
using Gameplay.Presentation.Tiles;
using UnityCoreKit.Runtime.UserInteractions;
using UnityCoreKit.Runtime.Core;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Manages tile selection state in response to user click interactions.
    /// Allows selecting one tile at a time, deselecting via second click.
    /// </summary>
    public class TileSelectionController : MonoBehaviour
    {
        private TileView? selectedTileView;
        private IUserInteractions? interactions;

        // Events for other systems to react to selection changes
        public event System.Action<TileView>? OnTileSelected;
        public event System.Action? OnTileDeselected;

        private void Awake()
        {
            interactions = CoreServices.Get<IUserInteractions>();
            interactions.Subscribe(UserInteractionType.Click, HandleTileClick, this);
        }

        private void OnDestroy()
        {
            interactions?.UnsubscribeAll(this);
        }

        private void HandleTileClick(UserInteractionEvent evt)
        {
            if (evt.Target is not TileView tileView)
                return;

            // If clicking the already-selected tile, deselect it
            if (selectedTileView == tileView)
            {
                DeselectTile();
                return;
            }

            // Otherwise, select the clicked tile
            SelectTile(tileView);
        }

        private void SelectTile(TileView tileView)
        {
            // Deselect previous tile if any
            if (selectedTileView != null)
            {
                DeselectTile();
            }

            selectedTileView = tileView;
            Debug.Log($"[TileSelectionController] Selected: {tileView.Tile.TypeKey} at ({tileView.BoardPosition.X}, {tileView.BoardPosition.Y})");

            OnTileSelected?.Invoke(tileView);
        }

        private void DeselectTile()
        {
            if (selectedTileView == null)
                return;

            Debug.Log($"[TileSelectionController] Deselected: {selectedTileView.Tile.TypeKey}");
            selectedTileView = null;

            OnTileDeselected?.Invoke();
        }

        public TileView? GetSelectedTile() => selectedTileView;
        
        public void ClearSelection() => DeselectTile();
    }
}
```

### Step 2: Create MovePreviewController
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/MovePreviewController.cs`

```csharp
using System.Collections.Generic;
using Gameplay.Engine.Board;
using Gameplay.Engine.Moves;
using Gameplay.Presentation.Tiles;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Visualizes available move destinations for the selected tile.
    /// Shows highlight indicators at each valid destination cell.
    /// </summary>
    public class MovePreviewController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TileSelectionController selectionController = null!;

        [Header("Visual Settings")]
        [SerializeField] private GameObject highlightPrefab = null!; // Simple sprite or quad
        [SerializeField] private Transform highlightParent = null!;
        [SerializeField] private Color highlightColor = new Color(0, 1, 0, 0.3f); // Semi-transparent green

        private readonly List<GameObject> activeHighlights = new();
        private IBoardState? board;

        private void Awake()
        {
            if (selectionController == null)
            {
                Debug.LogError("[MovePreviewController] TileSelectionController not assigned!");
                return;
            }

            selectionController.OnTileSelected += ShowMovePreview;
            selectionController.OnTileDeselected += ClearMovePreview;
        }

        private void OnDestroy()
        {
            if (selectionController != null)
            {
                selectionController.OnTileSelected -= ShowMovePreview;
                selectionController.OnTileDeselected -= ClearMovePreview;
            }

            ClearMovePreview();
        }

        public void Init(IBoardState boardState)
        {
            board = boardState;
        }

        private void ShowMovePreview(TileView tileView)
        {
            ClearMovePreview();

            if (board == null)
            {
                Debug.LogWarning("[MovePreviewController] Board not initialized!");
                return;
            }

            // Get available moves from engine
            var tile = tileView.Tile;
            var currentPos = tileView.BoardPosition;

            // Use tile's movement rules (from TileDefinition)
            var context = new MoveContext(board, currentPos, new MovementRules(100, true, true)); // TODO: Get from tile behavior
            var moves = tile.GetAvailableMoves(context);

            Debug.Log($"[MovePreviewController] Showing {moves.Count} possible moves for {tile.TypeKey}");

            // Create highlight at each destination
            foreach (var move in moves)
            {
                CreateHighlight(move.Destination);
            }
        }

        private void ClearMovePreview()
        {
            foreach (var highlight in activeHighlights)
            {
                if (highlight != null)
                    Destroy(highlight);
            }

            activeHighlights.Clear();
        }

        private void CreateHighlight(CellPos pos)
        {
            if (highlightPrefab == null)
            {
                Debug.LogWarning("[MovePreviewController] Highlight prefab not assigned!");
                return;
            }

            // Instantiate highlight at world position
            var worldPos = BoardToWorld(pos);
            var highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity, highlightParent);

            // Apply color (assumes SpriteRenderer or similar)
            var spriteRenderer = highlight.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }

            activeHighlights.Add(highlight);
        }

        // TODO: Get this from BoardPresenter instead of duplicating
        private Vector3 BoardToWorld(CellPos pos)
        {
            // Temporary: assumes 1x1 cell size at origin
            return new Vector3(pos.X, pos.Y, 0);
        }
    }
}
```

### Step 3: Create MoveExecutor
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/MoveExecutor.cs`

```csharp
using Gameplay.Engine.Board;
using Gameplay.Presentation.Board;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Executes tile movement by coordinating engine state updates (BoardState) 
    /// with presentation layer updates (BoardPresenter).
    /// </summary>
    public class MoveExecutor
    {
        private readonly IBoardState boardState;
        private readonly BoardPresenter boardPresenter;

        public MoveExecutor(IBoardState boardState, BoardPresenter boardPresenter)
        {
            this.boardState = boardState;
            this.boardPresenter = boardPresenter;
        }

        /// <summary>
        /// Executes a tile move from one position to another.
        /// Updates both engine state and view.
        /// </summary>
        /// <returns>True if move succeeded, false if invalid.</returns>
        public bool ExecuteMove(CellPos from, CellPos to)
        {
            // Validate positions are in bounds
            if (!boardState.IsInsideBounds(from) || !boardState.IsInsideBounds(to))
            {
                Debug.LogWarning($"[MoveExecutor] Invalid move: ({from.X},{from.Y}) → ({to.X},{to.Y}) out of bounds");
                return false;
            }

            // Get tile at origin
            var tile = boardState.GetTileAt(from);
            if (tile == null)
            {
                Debug.LogWarning($"[MoveExecutor] No tile at origin ({from.X},{from.Y})");
                return false;
            }

            // Validate destination is empty (TODO: Add more complex validation later)
            var destinationTile = boardState.GetTileAt(to);
            if (destinationTile != null)
            {
                Debug.LogWarning($"[MoveExecutor] Destination ({to.X},{to.Y}) is occupied");
                return false;
            }

            // Execute move on engine
            boardState.MoveTile(from, to);

            // Update view
            boardPresenter.MoveView(from, to);

            Debug.Log($"[MoveExecutor] Moved {tile.TypeKey} from ({from.X},{from.Y}) to ({to.X},{to.Y})");
            return true;
        }
    }
}
```

### Step 4: Create DestinationClickHandler
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/DestinationClickHandler.cs`

```csharp
using Gameplay.Engine.Board;
using Gameplay.Engine.Moves;
using Gameplay.Presentation.Tiles;
using UnityCoreKit.Runtime.UserInteractions;
using UnityCoreKit.Runtime.Core;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Handles clicks on empty board cells (destinations) to execute tile movement.
    /// </summary>
    public class DestinationClickHandler : MonoBehaviour
    {
        [SerializeField] private TileSelectionController selectionController = null!;
        [SerializeField] private MovePreviewController movePreviewController = null!;

        private MoveExecutor? moveExecutor;
        private IBoardState? board;
        private IUserInteractions? interactions;

        private void Awake()
        {
            interactions = CoreServices.Get<IUserInteractions>();
            // Subscribe to all clicks - we'll filter for destination clicks
            interactions.Subscribe(UserInteractionType.Click, HandleClick, this);
        }

        private void OnDestroy()
        {
            interactions?.UnsubscribeAll(this);
        }

        public void Init(IBoardState boardState, MoveExecutor executor)
        {
            board = boardState;
            moveExecutor = executor;
        }

        private void HandleClick(UserInteractionEvent evt)
        {
            // Only process if we have a selected tile
            var selectedTile = selectionController.GetSelectedTile();
            if (selectedTile == null)
                return;

            // If clicking another tile, let TileSelectionController handle it
            if (evt.Target is TileView)
                return;

            // TODO: Implement destination cell click detection
            // For now, this requires either:
            // 1. Colliders on empty cells (expensive)
            // 2. Raycasting from screen to board grid (better)
            // 3. Clicking on highlight objects themselves (simple MVP)

            // MVP: Handle click on highlight objects
            // This will be implemented after highlight prefabs are created
        }

        // Alternative: Direct method call from external system
        public void TryMoveSelectedTileTo(CellPos destination)
        {
            var selectedTile = selectionController.GetSelectedTile();
            if (selectedTile == null || moveExecutor == null)
                return;

            var from = selectedTile.BoardPosition;
            
            // Validate move is in available moves list
            if (!IsValidMove(selectedTile, destination))
            {
                Debug.LogWarning($"[DestinationClickHandler] Move to ({destination.X},{destination.Y}) is not valid");
                return;
            }

            // Execute move
            bool success = moveExecutor.ExecuteMove(from, destination);
            
            if (success)
            {
                // Clear selection after successful move
                selectionController.ClearSelection();
            }
        }

        private bool IsValidMove(TileView tileView, CellPos destination)
        {
            if (board == null)
                return false;

            var context = new MoveContext(board, tileView.BoardPosition, new MovementRules(100, true, true));
            var moves = tileView.Tile.GetAvailableMoves(context);

            foreach (var move in moves)
            {
                if (move.Destination.X == destination.X && move.Destination.Y == destination.Y)
                    return true;
            }

            return false;
        }
    }
}
```

### Step 5: Wire Everything Together in GameController
**File:** Update `/Assets/Scripts/Gameplay/Game/Controllers/BoardController.cs`

Add these fields:
```csharp
[Header("Controllers")]
[SerializeField] private TileSelectionController selectionController = null!;
[SerializeField] private MovePreviewController movePreviewController = null!;
[SerializeField] private DestinationClickHandler destinationClickHandler = null!;
[SerializeField] private BoardPresenter boardPresenter = null!;

private MoveExecutor? moveExecutor;
```

Update `Start()`:
```csharp
private void Start()
{
    CreateAndFillBoard();
    InitializeGameSystems();
}

private void InitializeGameSystems()
{
    // Initialize BoardPresenter
    var poolManager = CoreServices.Get<IPoolManager>();
    boardPresenter.Init(poolManager);
    boardPresenter.Rebuild(board);

    // Initialize move preview
    movePreviewController.Init(board);

    // Create move executor
    moveExecutor = new MoveExecutor(board, boardPresenter);

    // Initialize destination handler
    destinationClickHandler.Init(board, moveExecutor);

    Debug.Log("[BoardController] All game systems initialized");
}
```

### Step 6: Create Highlight Prefab
**In Unity Editor:**
1. Create new GameObject: "MoveHighlight"
2. Add SpriteRenderer component
3. Set sprite to white square (Unity default sprites)
4. Set color to semi-transparent green (R:0, G:1, B:0, A:0.3)
5. Set sorting layer to be above tiles
6. Save as prefab: `/Assets/Prefabs/MoveHighlight.prefab`
7. Assign to MovePreviewController in inspector

---

## Test Checklist

### Manual Testing
- [ ] Enter Play Mode
- [ ] Click a tile - verify it logs "Selected: ..."
- [ ] Verify green highlights appear at valid move destinations
- [ ] Click same tile again - verify deselection + highlights disappear
- [ ] Select different tile - verify previous deselects, new highlights appear
- [ ] Click empty cell with tile selected - verify move executes (if destination handler complete)
- [ ] After move, verify tile view moves to new position
- [ ] Verify BoardState internal data updated (check via debug log)

### Code Verification
- [ ] TileSelectionController subscribes/unsubscribes correctly
- [ ] Selection events (OnTileSelected, OnTileDeselected) fire
- [ ] MovePreviewController creates highlights at correct positions
- [ ] Highlights clear on deselection
- [ ] MoveExecutor calls both BoardState.MoveTile and BoardPresenter.MoveView
- [ ] DestinationClickHandler validates moves before execution

### Edge Cases
- [ ] Select tile with 0 valid moves - no highlights, no errors
- [ ] Select tile at board edge - only valid moves highlighted
- [ ] Try to move to occupied cell - move rejected with warning
- [ ] Try to move to out-of-bounds position - move rejected
- [ ] Rapidly click multiple tiles - selection state consistent

---

## Success Criteria

- ✅ Clicking a tile selects it and shows visual feedback
- ✅ Valid move destinations are highlighted
- ✅ Clicking selected tile again deselects it
- ✅ Highlights clear on deselection
- ✅ Clicking a valid destination executes the move
- ✅ Tile view animates/moves to new position
- ✅ BoardState internal data is updated
- ✅ Selection clears after successful move
- ✅ Invalid moves are rejected with warnings
- ✅ No memory leaks from event subscriptions

---

## Notes

### Incomplete: Destination Click Detection
This task leaves destination clicking **partially implemented**. The `DestinationClickHandler` has two approaches:

**Approach 1 (Recommended for MVP):** Click highlights themselves
- Add collider to MoveHighlight prefab
- Make highlights implement IUserInteractionTarget
- Handle click on highlight → extract destination from highlight position

**Approach 2 (Better UX):** Raycast screen to board grid
- Convert screen click → world position → board cell
- Check if cell is in available moves list
- Requires BoardPresenter.WorldToBoard() method

Choose **Approach 1** for this task to get basic functionality working quickly.

### Visual Feedback (Minimal MVP)
- No tile selection visual (outline/glow) implemented yet
- No move animation (instant teleport)
- These are addressed in **Task_04_VisualFeedback.md**

### Architecture Notes
- **TileSelectionController** - Owns selection state, publishes events
- **MovePreviewController** - Subscribes to selection, manages highlights
- **MoveExecutor** - Pure logic, no Unity dependencies (easily testable)
- **DestinationClickHandler** - Coordinates user input → move execution

### Common Issues
- **"Highlights don't appear"** → Check MovePreviewController.Init() was called with valid board
- **"Highlights wrong position"** → BoardToWorld() calculation doesn't match BoardPresenter
- **"Move doesn't execute"** → Check MoveExecutor.Init() was called
- **"Tile view doesn't move"** → BoardPresenter.MoveView() not called or view not found in dictionary

### Next Task
After this task works, proceed to **Task_03b_TestMovementExecution.md** to add unit tests.

### Related Files
- `/Assets/Scripts/Gameplay/Engine/Board/BoardState.cs`
- `/Assets/Scripts/Gameplay/Presentation/Board/BoardPresenter.cs`
- `/Assets/Scripts/Gameplay/Engine/Moves/MovementCalculator.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/BoardController.cs`
