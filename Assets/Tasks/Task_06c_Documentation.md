# Task 04c: Documentation - Visual Feedback System

**Estimated Time:** 15-20 minutes  
**Prerequisites:** Task_04b_VisualTestVerification.md completed  
**Status:** Not Started

---

## Context

Tasks 04 and 04b implemented and tested visual feedback for selection, movement animation, and highlights. Visual systems require different documentation approaches than logic systems:
- Animation implementation details
- Visual design decisions and rationale
- Performance considerations (pooling, tweening)
- Future enhancement paths

**Current State:**
- ✅ Visual feedback system implemented and tested
- ❌ Animation implementation not documented
- ❌ Visual design decisions not explained
- ❌ Performance patterns not documented

**Goal:** Document the visual feedback implementation and design decisions.

---

## Goals

1. Document TileView selection visual implementation
2. Explain movement animation approach (instant vs. tweened)
3. Document highlight visual design and pooling
4. Add comments on performance considerations
5. Document visual design guidelines for consistency

---

## Implementation Steps

### Step 1: Document TileView Selection Visual
**File:** `/Assets/Scripts/Gameplay/Presentation/Tiles/TileView.cs`

```csharp
/// <summary>
/// Unity view for rendering a tile and forwarding user interaction.
/// Holds a read-only reference to the engine tile model and its current board position.
/// 
/// <para>
/// Visual Feedback:
/// - Selection indicator (yellow outline) toggled via ShowSelection/HideSelection
/// - Sprite rendering via SetVisuals()
/// - Position updates via SetBoardPosition() + transform.position
/// </para>
/// </summary>
public class TileView : MonoBehaviour, IUserInteractionTarget
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Selection Visual")]
    [Tooltip("Child GameObject with outline sprite. Activated when tile is selected.")]
    [SerializeField] private GameObject selectionIndicator = null!;
    
    // ... existing code ...
    
    /// <summary>
    /// Shows visual indication that this tile is selected.
    /// Activates the selection indicator child object (typically a yellow/white outline).
    /// </summary>
    /// <remarks>
    /// Design Decision: GameObject activation vs. Material property
    /// - GameObject.SetActive(true) is simple and performant for small tile counts
    /// - Alternative: Material property block for better batching (future optimization)
    /// 
    /// Visual Design:
    /// - Color: Yellow/white (high contrast, indicates "active")
    /// - Shape: Outline matching tile bounds
    /// - Timing: Instant (no fade-in for responsiveness)
    /// </remarks>
    public void ShowSelection()
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(true);
    }
    
    /// <summary>
    /// Hides selection visual.
    /// Deactivates the selection indicator child object.
    /// </summary>
    public void HideSelection()
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
    }
}
```

### Step 2: Document Movement Animation
**File:** `/Assets/Scripts/Gameplay/Presentation/Board/BoardPresenter.cs`

```csharp
/// <summary>
/// Updates a single view mapping and moves the view in world space.
/// </summary>
/// <param name="from">The tile's previous board position.</param>
/// <param name="to">The tile's new board position.</param>
/// <remarks>
/// Current Implementation: Instant Teleport
/// The tile's transform.position is set immediately to the new world position.
/// This is simple and works but lacks visual polish.
/// 
/// Future Enhancement: Tweened Movement
/// For better visual feedback, consider:
/// - DOTween/LeanTween for smooth position interpolation
/// - Duration: 0.2-0.3 seconds for snappy feel
/// - Easing: EaseOutQuad for natural deceleration
/// - Callback: Notify MoveExecutor when animation completes
/// 
/// Example (DOTween):
/// view.transform.DOMove(BoardToWorld(to), 0.25f)
///     .SetEase(Ease.OutQuad)
///     .OnComplete(() => OnMoveAnimationComplete(view));
/// 
/// Considerations:
/// - Blocking vs. non-blocking moves (can player click during animation?)
/// - Queued moves (what if multiple moves happen rapidly?)
/// - Animation cancellation (what if tile moves again mid-animation?)
/// </remarks>
public void MoveView(CellPos from, CellPos to)
{
    if (!viewsByPos.TryGetValue(from, out var view) || view == null)
        return;

    viewsByPos.Remove(from);
    viewsByPos[to] = view;

    view.SetBoardPosition(to);
    view.transform.position = BoardToWorld(to); // TODO: Replace with tweened animation
}
```

### Step 3: Document Highlight System
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/MovePreviewController.cs`

```csharp
/// <summary>
/// Visualizes valid move destinations when a tile is selected.
/// 
/// <para>
/// Highlight Implementation:
/// - Prefab: Simple sprite (circle or square) with semi-transparent color
/// - Pooling: Highlights are pooled for performance (frequent spawn/destroy)
/// - Positioning: Placed at board cell centers via BoardPresenter.BoardToWorld()
/// - Lifecycle: Spawned on selection, destroyed on deselection
/// </para>
/// 
/// <para>
/// Visual Design Guidelines:
/// - Color: Cyan/green (distinct from tile colors and selection yellow)
/// - Alpha: 0.5-0.7 (visible but not obstructing tile view)
/// - Size: Slightly smaller than cell size (clear visual hierarchy)
/// - Animation (optional): Gentle pulse (scale or alpha fade) for "aliveness"
/// </para>
/// 
/// <para>
/// Performance Considerations:
/// - Pooling is essential: Tiles may have 10+ valid moves
/// - Selection changes happen frequently (every click)
/// - Avoid Instantiate/Destroy spam: Use object pooling
/// - Batching: Use same material for all highlights (single draw call)
/// </para>
/// </summary>
public class MovePreviewController : MonoBehaviour
{
    /// <summary>
    /// Spawns highlight visuals at all valid move destinations.
    /// </summary>
    /// <remarks>
    /// Spawn Strategy:
    /// 1. Query engine for valid moves (MovementCalculator)
    /// 2. For each destination:
    ///    a. Get highlight from pool (PoolManager)
    ///    b. Position at BoardPresenter.BoardToWorld(destination)
    ///    c. Activate GameObject
    /// 3. Store references for cleanup
    /// 
    /// Alternative Approach (not implemented):
    /// - Pre-create highlight objects for all board cells
    /// - Toggle visibility based on valid moves
/// - Pros: No spawn/despawn overhead
    /// - Cons: Memory overhead for large boards (e.g., 10x10 = 100 objects)
    /// </remarks>
    private void ShowMovePreview(TileView tileView)
    {
        ClearMovePreview(); // Clean up previous highlights

        if (board == null)
        {
            Debug.LogWarning("[MovePreviewController] Board not initialized!");
            return;
        }

        // Get available moves from engine
        var tile = tileView.Tile;
        var moves = MovementCalculator.GetAvailableMoves(tile, board, tileView.BoardPosition);

        // Spawn highlights at each valid destination
        foreach (var destination in moves)
        {
            // TODO: Get highlight from pool
            // TODO: Position at BoardPresenter.BoardToWorld(destination)
            // TODO: Store reference in activeHighlights list
        }
    }
    
    /// <summary>
    /// Clears all active highlight visuals.
    /// Returns highlight objects to the pool for reuse.
    /// </summary>
    /// <remarks>
    /// Cleanup is critical to avoid:
    /// - Memory leaks (highlights not returned to pool)
    /// - Visual bugs (old highlights lingering on screen)
    /// - Performance issues (accumulating active objects)
    /// 
    /// Cleanup Triggers:
    /// - Tile deselected (OnTileDeselected event)
    /// - New tile selected (before showing new highlights)
    /// - Component destroyed (OnDestroy)
    /// </remarks>
    private void ClearMovePreview()
    {
        // Return all highlights to pool
        foreach (var highlight in activeHighlights)
        {
            // TODO: Return to pool
        }
        activeHighlights.Clear();
    }
}
```

### Step 4: Document Visual Design Decisions
**File:** `/Assets/Tasks/Task_04_VisualDesignGuidelines.md` (new file)

```markdown
# Visual Feedback Design Guidelines

## Overview
Visual feedback makes the game responsive and learnable. These guidelines ensure consistency.

## Selection Visual

### Purpose
Indicate which tile is currently selected and ready to move.

### Design
- **Shape**: Outline matching tile bounds (circle or square)
- **Color**: Yellow (#FFFF00) or White (#FFFFFF)
- **Alpha**: 0.8 (visible but not overpowering)
- **Animation**: None (instant feedback for responsiveness)
- **Layer**: Above tile sprite but below UI

### Rationale
- Yellow/white is universally recognized as "active" or "selected"
- Instant appearance (no fade-in) provides immediate feedback
- Outline shape preserves tile visibility (not covering the tile art)

## Move Highlight

### Purpose
Show all valid move destinations for the selected tile.

### Design
- **Shape**: Filled circle or square matching cell size
- **Color**: Cyan (#00FFFF) or Green (#00FF00)
- **Alpha**: 0.5-0.6 (visible but not obstructing view)
- **Animation**: Optional gentle pulse (scale 0.9 → 1.0, 1 second loop)
- **Layer**: Below tiles (highlights are background, tiles are foreground)

### Rationale
- Cyan/green is distinct from selection yellow and tile colors
- Semi-transparent to show board underneath
- Pulse animation adds "aliveness" without being distracting
- Multiple highlights create a clear "movement zone" visual

## Movement Animation

### Purpose (Future)
Visually communicate that a tile is moving from one cell to another.

### Design (Not Yet Implemented)
- **Duration**: 0.2-0.3 seconds (fast enough to feel responsive)
- **Easing**: EaseOutQuad (natural deceleration, not linear)
- **Path**: Straight line (no arc or bounce for clarity)
- **Blocking**: Non-blocking (player can select other tiles during animation)

### Rationale
- Short duration maintains game pace
- EaseOut feels natural (objects don't stop instantly in real world)
- Straight path is clearest for grid-based movement
- Non-blocking prevents feeling of "waiting" for animations

## Color Palette

| Element | Color | Hex | Alpha | Purpose |
|---------|-------|-----|-------|---------|
| Selection | Yellow | #FFFF00 | 0.8 | Active/Selected |
| Move Highlight | Cyan | #00FFFF | 0.6 | Valid Destination |
| Error Flash | Red | #FF0000 | 0.5 | Invalid Action |
| Success Flash | Green | #00FF00 | 0.5 | Goal Reached |

## Performance Guidelines

### Object Pooling
- **Required For**: Highlights (frequent spawn/destroy)
- **Optional For**: Particles, VFX
- **Not Needed For**: Tile visuals (persistent)

### Batching
- Use same material for all highlights (single draw call)
- Use sprite atlases to batch tile renders
- Minimize unique materials (each breaks a batch)

### Animation
- Prefer transform tweening over material property changes
- Use DOTween/LeanTween for performance and ease-of-use
- Avoid Update() loops for animations (use tweening callbacks)

## Accessibility Considerations

### Color Blindness
- Don't rely solely on color to convey information
- Add shape or pattern differences (e.g., dotted vs. solid outlines)
- Provide colorblind modes with alternative palettes

### Motion Sensitivity
- Allow disabling animations in settings
- Provide "reduced motion" mode
- Never use flashing effects (seizure risk)

## Future Enhancements

1. **Particle Effects**: Dust clouds on tile movement
2. **Sound Design**: Click, move, invalid action sounds
3. **Juice**: Screen shake, impact effects, combo animations
4. **Themes**: Day/night modes, seasonal skins
```

---

## Success Criteria

- ✅ All visual methods have XML documentation with design rationale
- ✅ Animation approach (current and future) is documented
- ✅ Visual design guidelines exist for consistency
- ✅ Performance considerations are documented
- ✅ Accessibility considerations are noted

---

## Notes

### Visual Documentation vs. Logic Documentation
Visual systems benefit from:
- Screenshots (show, don't tell)
- Color swatches (precise color values)
- Animation timing diagrams
- Before/after comparisons

### Placeholder Art
Current implementation may use programmer art (simple sprites).
Documentation should describe the *intent* and *principles*, not specific assets.
Real art can be swapped later without changing the documented design.

---

### Next Task
After documentation is complete, proceed to **Task_05_SimpleObjective.md**.
