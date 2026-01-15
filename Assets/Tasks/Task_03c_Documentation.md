# Task 03c: Documentation - Movement Execution System

**Estimated Time:** 20-25 minutes  
**Prerequisites:** Task_03b_TestMovementExecution.md completed  
**Status:** Not Started

---

## Context

Tasks 03 and 03b implemented and tested the complete movement execution system including selection, preview, and execution. This is a complex multi-component system that requires thorough documentation covering:
- Component responsibilities and interactions
- Event-driven architecture patterns
- Coordination between engine and presentation layers
- State management patterns

**Current State:**
- ✅ Movement execution system implemented and tested
- ❌ Component interaction patterns not documented
- ❌ State management approach not explained
- ❌ Engine-Presentation coordination not documented

**Goal:** Document the movement execution architecture and component responsibilities.

---

## Goals

1. Document all movement execution controllers and their responsibilities
2. Explain the event-driven coordination pattern
3. Document the engine-presentation synchronization approach
4. Add architecture diagrams showing component interactions
5. Document state management (selection state, preview state)

---

## Implementation Steps

### Step 1: Document TileSelectionController
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/TileSelectionController.cs`

```csharp
/// <summary>
/// Manages tile selection state in response to user click interactions.
/// 
/// <para>
/// Responsibilities:
/// - Track currently selected tile (single-selection model)
/// - Emit selection/deselection events for other systems to react to
/// - Handle selection toggle (clicking selected tile deselects it)
/// - Provide read-only access to current selection state
/// </para>
/// 
/// <para>
/// Architecture Pattern: Event-Driven State Management
/// This controller doesn't know about move previews, execution, or any other
/// game systems. It simply manages selection state and broadcasts changes via
/// events. Other controllers (MovePreviewController, DestinationClickHandler)
/// subscribe to these events and react accordingly.
/// </para>
/// 
/// <para>
/// Lifecycle:
/// - Awake: Subscribe to click interactions
/// - HandleTileClick: Update selection state based on user input
/// - OnDestroy: Unsubscribe (ownership-based cleanup)
/// </para>
/// </summary>
public class TileSelectionController : MonoBehaviour
{
    /// <summary>
    /// Fired when a tile is selected. Subscribers receive the selected TileView.
    /// Example subscribers: MovePreviewController (show valid moves)
    /// </summary>
    public event System.Action<TileView>? OnTileSelected;
    
    /// <summary>
    /// Fired when selection is cleared (tile deselected or moved).
    /// Example subscribers: MovePreviewController (clear highlights)
    /// </summary>
    public event System.Action? OnTileDeselected;
    
    // ... implementation
}
```

### Step 2: Document MovePreviewController
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/MovePreviewController.cs`

```csharp
/// <summary>
/// Visualizes valid move destinations when a tile is selected.
/// 
/// <para>
/// Responsibilities:
/// - Query engine for valid moves when tile selected
/// - Spawn highlight visuals at valid destination cells
/// - Clean up highlights when tile deselected
/// - Handle highlight lifecycle (pooling, cleanup)
/// </para>
/// 
/// <para>
/// Architecture Pattern: Reactive Controller
/// This controller reacts to TileSelectionController events:
/// - OnTileSelected → Calculate moves, show highlights
/// - OnTileDeselected → Clear all highlights
/// It does NOT maintain selection state itself; it observes state changes.
/// </para>
/// 
/// <para>
/// Engine-Presentation Coordination:
/// 1. Calls engine's MovementCalculator.GetAvailableMoves() (pure logic)
/// 2. Spawns Unity GameObjects (highlights) at calculated positions
/// This demonstrates the pattern: engine does calculation, presentation does visualization.
/// </para>
/// </summary>
public class MovePreviewController : MonoBehaviour
{
    /// <summary>
    /// Subscribes to selection controller events.
    /// This establishes the reactive relationship: selection changes → preview updates.
    /// </summary>
    private void Awake()
    {
        selectionController.OnTileSelected += ShowMovePreview;
        selectionController.OnTileDeselected += ClearMovePreview;
    }
    
    /// <summary>
    /// Shows highlights at all valid move destinations for the selected tile.
    /// </summary>
    /// <param name="tileView">The selected tile to calculate moves for.</param>
    /// <remarks>
    /// Flow:
    /// 1. Get engine tile from view (IReadOnlyModuleTile)
    /// 2. Call MovementCalculator.GetAvailableMoves(tile, board)
    /// 3. For each valid destination, spawn a highlight GameObject
    /// 4. Position highlights using BoardPresenter.BoardToWorld()
    /// </remarks>
    private void ShowMovePreview(TileView tileView)
    {
        // ... implementation
    }
    
    // ... rest of implementation
}
```

### Step 3: Document MoveExecutor
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/MoveExecutor.cs`

```csharp
/// <summary>
/// Coordinates tile movement between engine state and presentation visuals.
/// 
/// <para>
/// Responsibilities:
/// - Validate moves before execution
/// - Update engine BoardState (authoritative data)
/// - Update presentation BoardPresenter (visual sync)
/// - Ensure engine and presentation stay in sync
/// </para>
/// 
/// <para>
/// Architecture Pattern: Engine-Presentation Synchronization
/// The engine (BoardState) is the source of truth. Presentation (BoardPresenter)
/// is a view of that truth. MoveExecutor ensures both are updated atomically:
/// 1. Validate move is legal (MovementCalculator)
/// 2. Update BoardState.MoveTile() (engine mutation)
/// 3. Update BoardPresenter.MoveView() (visual update)
/// If either fails, both roll back (future: transaction pattern).
/// </para>
/// 
/// <para>
/// Why a Separate Executor?
/// - Single Responsibility: Move logic is isolated from selection/preview
/// - Testability: Can unit test move execution without UI
/// - Reusability: AI, replay, networking can use same executor
/// </para>
/// </summary>
public class MoveExecutor
{
    /// <summary>
    /// Executes a tile move, updating both engine state and presentation.
    /// </summary>
    /// <param name="from">Source cell position (must contain a tile).</param>
    /// <param name="to">Destination cell position (must be empty and valid).</param>
    /// <returns>True if move succeeded, false if invalid or failed.</returns>
    /// <remarks>
    /// Validation:
    /// - Source cell must contain a tile
    /// - Destination must be in tile's valid moves (MovementCalculator)
    /// - Destination must be empty
    /// 
    /// Execution Order:
    /// 1. Validate move
    /// 2. Call BoardState.MoveTile() (engine)
    /// 3. Call BoardPresenter.MoveView() (presentation)
    /// 
    /// Future Improvements:
    /// - Add animation support (async execution)
    /// - Add move history (undo/redo)
    /// - Add transaction rollback on partial failure
    /// </remarks>
    public bool ExecuteMove(CellPos from, CellPos to)
    {
        // ... implementation
    }
}
```

### Step 4: Document DestinationClickHandler
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/DestinationClickHandler.cs`

```csharp
/// <summary>
/// Handles destination cell clicks to execute tile movement.
/// 
/// <para>
/// Responsibilities:
/// - Listen for click events on empty cells or highlights
/// - Validate clicked position is a valid move destination
/// - Call MoveExecutor to perform the move
/// - Clear selection after successful move
/// </para>
/// 
/// <para>
/// Architecture Challenge: Empty Cell Clicks
/// Unity's EventSystem requires a collider/UI element to detect clicks.
/// Empty board cells have no objects. Solutions:
/// 1. Colliders on every cell (expensive, 100+ colliders)
/// 2. Raycasting from screen to board grid (complex)
/// 3. Click handlers on highlight objects (simple MVP) ← Current approach
/// 
/// Current Implementation:
/// Highlights have colliders and PointerClickUserInteractionSource.
/// Clicking a highlight triggers this handler via the interaction system.
/// </para>
/// </summary>
public class DestinationClickHandler : MonoBehaviour
{
    /// <summary>
    /// Attempts to move the currently selected tile to the clicked destination.
    /// </summary>
    /// <param name="destination">The board cell position that was clicked.</param>
    /// <remarks>
    /// Validation Flow:
    /// 1. Check if a tile is selected (via TileSelectionController)
    /// 2. Check if destination is in the tile's valid moves
    /// 3. Call MoveExecutor.ExecuteMove()
    /// 4. On success, clear selection (via TileSelectionController.ClearSelection())
    /// </remarks>
    public void TryMoveSelectedTileTo(CellPos destination)
    {
        // ... implementation
    }
}
```

### Step 5: Add Component Interaction Diagram
**File:** `/Assets/Tasks/Task_03_ComponentInteractionDiagram.md` (new file)

```markdown
# Movement Execution Component Interaction

## Overview
The movement system uses event-driven architecture to coordinate multiple controllers.

## Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    User Input Layer                         │
└────────────┬────────────────────────────────────────────────┘
             │
             │ Click Tile
             ▼
┌─────────────────────────────────────────────────────────────┐
│ TileSelectionController                                     │
│ - Manages selection state                                   │
│ - Emits OnTileSelected / OnTileDeselected events           │
└────────┬──────────────────────┬─────────────────────────────┘
         │                      │
         │ OnTileSelected       │ OnTileDeselected
         ▼                      ▼
┌─────────────────────┐  ┌──────────────────────────────────┐
│ MovePreviewController│  │ (Highlights cleared)              │
│ - Query engine      │  └──────────────────────────────────┘
│ - Show highlights   │
└────────┬────────────┘
         │
         │ MovementCalculator.GetAvailableMoves()
         ▼
┌─────────────────────────────────────────────────────────────┐
│ Engine Layer (BoardState, MovementCalculator)              │
│ - Pure logic, no Unity dependencies                        │
│ - Returns list of valid CellPos destinations               │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ Valid moves returned
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ Presentation Layer (BoardPresenter, Highlights)            │
│ - Spawn highlight GameObjects at valid positions           │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ User clicks highlight
                              ▼
┌─────────────────────────────────────────────────────────────┐
│ DestinationClickHandler                                     │
│ - Validate move is in available moves                       │
│ - Call MoveExecutor.ExecuteMove()                           │
└────────────┬────────────────────────────────────────────────┘
             │
             │ ExecuteMove(from, to)
             ▼
┌─────────────────────────────────────────────────────────────┐
│ MoveExecutor                                                │
│ 1. BoardState.MoveTile() ← Engine update                    │
│ 2. BoardPresenter.MoveView() ← Visual update                │
└─────────────────────────────────────────────────────────────┘
```

## Event Flow: Complete Move Sequence

1. **User clicks tile**
   - PointerClickUserInteractionSource publishes UserInteractionEvent
   - TileSelectionController.HandleTileClick() receives event
   
2. **Tile selection**
   - TileSelectionController stores selected tile
   - Fires OnTileSelected event
   
3. **Move preview**
   - MovePreviewController.ShowMovePreview() triggered by event
   - Queries MovementCalculator.GetAvailableMoves(tile, board)
   - Spawns highlight objects at valid destinations
   
4. **User clicks destination**
   - Highlight's PointerClickUserInteractionSource publishes event
   - DestinationClickHandler.HandleClick() receives event
   
5. **Move execution**
   - DestinationClickHandler validates move
   - Calls MoveExecutor.ExecuteMove(from, to)
   - MoveExecutor updates BoardState (engine)
   - MoveExecutor updates BoardPresenter (visual)
   
6. **Cleanup**
   - DestinationClickHandler calls TileSelectionController.ClearSelection()
   - TileSelectionController fires OnTileDeselected
   - MovePreviewController.ClearMovePreview() destroys highlights

## Design Principles

### Single Responsibility
- **TileSelectionController**: Selection state only
- **MovePreviewController**: Visualization only
- **MoveExecutor**: Engine-presentation sync only
- **DestinationClickHandler**: User input to action mapping only

### Event-Driven Communication
Controllers communicate via events, not direct method calls:
- Reduces coupling
- Makes testing easier (mock events)
- Allows adding new features without modifying existing controllers

### Engine-Presentation Separation
- Engine (BoardState): Source of truth, pure logic
- Presentation (BoardPresenter): View of truth, Unity visuals
- MoveExecutor: Synchronization bridge

## Testing Strategy

- **TileSelectionController**: Test state changes, event emission
- **MovePreviewController**: Test highlight spawn/clear
- **MoveExecutor**: Test engine-presentation sync
- **Integration**: Test complete click→move→clear flow
```

---

## Success Criteria

- ✅ All controllers have comprehensive XML documentation
- ✅ Component responsibilities are clearly explained
- ✅ Event-driven architecture pattern is documented
- ✅ Engine-presentation synchronization is explained
- ✅ Interaction diagram shows complete flow
- ✅ Design principles and patterns are documented

---

## Notes

### Why So Many Controllers?
Each controller has a single, clear responsibility. This makes:
- Testing easier (test one thing at a time)
- Debugging simpler (clear component boundaries)
- Extension safer (add features without breaking existing code)

### Event-Driven vs Direct Calls
Event-driven architecture adds complexity but provides:
- Loose coupling (controllers don't know about each other)
- Easy extensibility (add subscribers without modifying publishers)
- Better testability (mock events, test in isolation)

---

### Next Task
After documentation is complete, proceed to **Task_04_VisualFeedback.md**.
