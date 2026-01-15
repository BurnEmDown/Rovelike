# Task 05c: Documentation - Objective System

**Estimated Time:** 15-20 minutes  
**Prerequisites:** Task_05b_TestObjectiveSystem.md completed  
**Status:** Not Started

---

## Context

Tasks 05 and 05b implemented and tested the objective system that determines win conditions. This completes the minimum playable game loop. Documentation should cover:
- The IObjective interface design pattern
- Objective evaluation lifecycle
- How to add new objective types
- Integration with the game loop

**Current State:**
- ✅ Objective system implemented and tested
- ❌ IObjective extensibility pattern not documented
- ❌ Evaluation lifecycle not explained
- ❌ How to add custom objectives not documented

**Goal:** Document the objective system architecture and extensibility.

---

## Goals

1. Document IObjective interface and design pattern
2. Explain objective evaluation lifecycle and timing
3. Document how to create custom objective types
4. Add examples of potential objective variations
5. Document integration with the game loop

---

## Implementation Steps

### Step 1: Document IObjective Interface
**File:** `/Assets/Scripts/Gameplay/Engine/Objectives/IObjective.cs`

```csharp
/// <summary>
/// Represents a win condition that can be evaluated against the current board state.
/// 
/// <para>
/// Design Pattern: Strategy Pattern
/// Different objectives (move to target, collect items, survive turns) are
/// implemented as separate classes implementing this interface. The game
/// doesn't need to know which specific objective is active; it just calls
/// IsComplete() and reacts to the result.
/// </para>
/// 
/// <para>
/// Extensibility:
/// To add a new objective type:
/// 1. Create a class implementing IObjective
/// 2. Implement IsComplete(IBoardState board) with your win logic
/// 3. Provide a Description property for UI display
/// 4. Use the objective in ObjectiveEvaluator
/// No changes to existing code required!
/// </para>
/// 
/// <para>
/// Architecture Note: Pure Engine Interface
/// IObjective works with IBoardState (engine interface), not Unity types.
/// This keeps objectives testable without Unity's playmode test harness.
/// </para>
/// </summary>
public interface IObjective
{
    /// <summary>
    /// Checks if the objective has been completed based on the current board state.
    /// </summary>
    /// <param name="board">The current board state to evaluate.</param>
    /// <returns>True if objective is complete (win condition met), false otherwise.</returns>
    /// <remarks>
    /// This method should be:
    /// - Pure (no side effects, same input → same output)
    /// - Fast (called frequently, potentially after every move)
    /// - Thread-safe (if objectives are evaluated off main thread)
    /// 
    /// Evaluation Timing:
    /// - After every player move
    /// - After every AI move
    /// - On board initialization (to catch pre-won states)
    /// </remarks>
    bool IsComplete(IBoardState board);

    /// <summary>
    /// Human-readable description of what the objective requires.
    /// Displayed in UI, tutorials, and debug logs.
    /// </summary>
    /// <example>
    /// "Move Brain to (2, 2)"
    /// "Collect all 5 Energy Cells"
    /// "Survive 10 turns"
    /// </example>
    string Description { get; }
}
```

### Step 2: Document MoveTileToTargetObjective
**File:** `/Assets/Scripts/Gameplay/Engine/Objectives/MoveTileToTargetObjective.cs`

```csharp
/// <summary>
/// Objective: Move a specific tile (by TypeKey) to a target board position.
/// Commonly used for "Move Brain to target cell" win condition.
/// 
/// <para>
/// Use Case:
/// This is the primary objective for ROVE-style puzzle games. The player must
/// navigate a specific tile (usually the "Brain") to a designated goal cell,
/// often using other tiles as obstacles or stepping stones.
/// </para>
/// 
/// <para>
/// Design Note: TypeKey vs. Tile Instance
/// This objective identifies tiles by TypeKey (string), not by instance reference.
/// This allows:
/// - Serialization (save/load objectives)
/// - Level design (specify "Brain" without knowing which Brain instance)
/// - Multiple matching tiles (e.g., "Move any Robot to the goal")
/// </para>
/// </summary>
public class MoveTileToTargetObjective : IObjective
{
    private readonly string targetTileTypeKey;
    private readonly CellPos targetPosition;

    public string Description => $"Move {targetTileTypeKey} to ({targetPosition.X}, {targetPosition.Y})";

    /// <summary>
    /// Creates an objective requiring a specific tile to reach a target position.
    /// </summary>
    /// <param name="targetTileTypeKey">The TypeKey of the tile that must reach the target (e.g., "Brain").</param>
    /// <param name="targetPosition">The board position the tile must reach.</param>
    /// <example>
    /// // Create objective: Move Brain tile to cell (2, 3)
    /// var objective = new MoveTileToTargetObjective("Brain", new CellPos { X = 2, Y = 3 });
    /// </example>
    public MoveTileToTargetObjective(string targetTileTypeKey, CellPos targetPosition)
    {
        this.targetTileTypeKey = targetTileTypeKey;
        this.targetPosition = targetPosition;
    }

    /// <summary>
    /// Evaluates whether the target tile has reached the target position.
    /// </summary>
    /// <returns>True if a tile matching targetTileTypeKey is at targetPosition.</returns>
    /// <remarks>
    /// Evaluation Logic:
    /// 1. Check if targetPosition is within board bounds (out of bounds = fail)
    /// 2. Get tile at targetPosition (null = fail)
    /// 3. Check if tile.TypeKey matches targetTileTypeKey (mismatch = fail)
    /// 4. If all checks pass, objective is complete
    /// 
    /// Edge Cases:
    /// - Multiple tiles with same TypeKey: Only position matters
    /// - Tile removed mid-game: Objective fails (tile not at target)
    /// - Board resized: Out of bounds targets always fail
    /// </remarks>
    public bool IsComplete(IBoardState board)
    {
        // ... implementation
    }
}
```

### Step 3: Document ObjectiveEvaluator
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/ObjectiveEvaluator.cs`

```csharp
/// <summary>
/// Evaluates objectives and notifies the game when win conditions are met.
/// 
/// <para>
/// Responsibilities:
/// - Store current active objectives
/// - Evaluate objectives after relevant game events (moves, etc.)
/// - Emit OnObjectiveComplete event when win conditions met
/// - Provide UI access to current objectives (for HUD display)
/// </para>
/// 
/// <para>
/// Architecture Pattern: Observer Pattern
/// The evaluator observes game state changes and notifies subscribers
/// when objectives are completed. This decouples win detection from
/// game logic (MoveExecutor doesn't need to know about objectives).
/// </para>
/// 
/// <para>
/// Evaluation Timing:
/// Current: After every move (MoveExecutor triggers evaluation)
/// Future: Could add time-based evaluation, trigger-based, etc.
/// </para>
/// </summary>
public class ObjectiveEvaluator : MonoBehaviour
{
    /// <summary>
    /// Fired when all active objectives are complete (win condition met).
    /// Subscribers: UI (show win screen), Analytics (log completion), etc.
    /// </summary>
    public event System.Action? OnObjectiveComplete;
    
    private List<IObjective> objectives = new List<IObjective>();
    private IBoardState? board;
    private bool objectivesCompleted = false;

    /// <summary>
    /// Initializes the evaluator with a board reference.
    /// Must be called before evaluating objectives.
    /// </summary>
    /// <param name="boardState">The board to evaluate objectives against.</param>
    public void Init(IBoardState boardState)
    {
        this.board = boardState;
    }

    /// <summary>
    /// Adds an objective to the active objectives list.
    /// Objectives are evaluated in the order they were added.
    /// </summary>
    /// <param name="objective">The objective to add.</param>
    /// <remarks>
    /// Multiple objectives can be active simultaneously:
    /// - All must be complete for win (AND logic)
    /// - Alternative: Any complete for win (OR logic) - future feature
    /// </remarks>
    public void AddObjective(IObjective objective)
    {
        objectives.Add(objective);
        Logger.Log($"[ObjectiveEvaluator] Added objective: {objective.Description}");
    }

    /// <summary>
    /// Evaluates all active objectives against the current board state.
    /// If all are complete, fires OnObjectiveComplete event.
    /// </summary>
    /// <remarks>
    /// Evaluation Strategy:
    /// - Short-circuit: Stop at first incomplete objective (optimization)
    /// - Idempotent: Safe to call multiple times after win
    /// - No Reset: Objectives stay completed (see ResetObjectives() for restart)
    /// 
    /// Call Sites:
    /// - MoveExecutor.ExecuteMove() (after successful move)
    /// - TurnManager (if time-based objectives exist)
    /// - BoardController.Start() (to check pre-won states)
    /// </remarks>
    public void EvaluateObjectives()
    {
        if (objectivesCompleted || board == null || objectives.Count == 0)
            return;

        // Check if all objectives are complete
        bool allComplete = true;
        foreach (var objective in objectives)
        {
            if (!objective.IsComplete(board))
            {
                allComplete = false;
                break; // Short-circuit for performance
            }
        }

        if (allComplete)
        {
            objectivesCompleted = true;
            Logger.Log("[ObjectiveEvaluator] All objectives complete! Win condition met.");
            OnObjectiveComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Resets objective completion state.
    /// Used when restarting a level or starting a new game.
    /// </summary>
    public void ResetObjectives()
    {
        objectivesCompleted = false;
    }
}
```

### Step 4: Document Objective Integration
**File:** `/Assets/Tasks/Task_05_ObjectiveIntegration.md` (new file)

```markdown
# Objective System Integration

## Overview
The objective system determines when the player has won. It integrates with the game loop.

## Integration Points

### 1. Game Initialization
```csharp
// In GameController or LevelLoader:
var objectiveEvaluator = GetComponent<ObjectiveEvaluator>();
objectiveEvaluator.Init(board);

// Add objectives (could be loaded from level data):
var brainObjective = new MoveTileToTargetObjective("Brain", new CellPos { X = 4, Y = 4 });
objectiveEvaluator.AddObjective(brainObjective);

// Subscribe to win event:
objectiveEvaluator.OnObjectiveComplete += HandleWin;
```

### 2. Move Execution
```csharp
// In MoveExecutor.ExecuteMove():
public bool ExecuteMove(CellPos from, CellPos to)
{
    // Execute move...
    bool success = board.MoveTile(from, to);
    if (success)
    {
        boardPresenter.MoveView(from, to);
        
        // Evaluate objectives after every move
        objectiveEvaluator.EvaluateObjectives();
    }
    return success;
}
```

### 3. Win Handling
```csharp
// In GameController:
private void HandleWin()
{
    Debug.Log("Player won!");
    
    // Show win screen
    winScreenUI.Show();
    
    // Disable input
    inputController.enabled = false;
    
    // Play victory music
    audioManager.PlayVictory();
    
    // Record analytics
    analyticsService.LogLevelComplete(currentLevel);
}
```

## Creating Custom Objectives

### Example 1: Collect All Items
```csharp
public class CollectAllItemsObjective : IObjective
{
    private readonly string itemTypeKey;
    public string Description => $"Collect all {itemTypeKey} items";

    public CollectAllItemsObjective(string itemTypeKey)
    {
        this.itemTypeKey = itemTypeKey;
    }

    public bool IsComplete(IBoardState board)
    {
        // Check if any tiles of type itemTypeKey remain on the board
        var tiles = board.GetAllTiles();
        return !tiles.Any(t => t.TypeKey == itemTypeKey);
    }
}
```

### Example 2: Survive N Turns
```csharp
public class SurviveTurnsObjective : IObjective
{
    private readonly int targetTurns;
    private int currentTurn = 0;
    
    public string Description => $"Survive {targetTurns} turns (Current: {currentTurn})";

    public SurviveTurnsObjective(int turns)
    {
        this.targetTurns = turns;
    }

    public void IncrementTurn() => currentTurn++;

    public bool IsComplete(IBoardState board)
    {
        return currentTurn >= targetTurns;
    }
}

// Usage:
// In TurnManager.EndTurn():
// surviveObjective.IncrementTurn();
// objectiveEvaluator.EvaluateObjectives();
```

### Example 3: Chain Multiple Objectives
```csharp
public class AllObjectivesComplete : IObjective
{
    private readonly List<IObjective> subObjectives;
    public string Description => "Complete all objectives";

    public AllObjectivesComplete(params IObjective[] objectives)
    {
        this.subObjectives = new List<IObjective>(objectives);
    }

    public bool IsComplete(IBoardState board)
    {
        return subObjectives.All(obj => obj.IsComplete(board));
    }
}

// Usage:
var multiObjective = new AllObjectivesComplete(
    new MoveTileToTargetObjective("Brain", targetPos),
    new CollectAllItemsObjective("EnergyCells"),
    new SurviveTurnsObjective(10)
);
```

## Objective Data in Levels

### ScriptableObject Approach (Future)
```csharp
[CreateAssetMenu(menuName = "Rovelike/Level")]
public class LevelDefinition : ScriptableObject
{
    public BoardSize boardSize;
    public List<TileDefinition> tiles;
    public ObjectiveData[] objectives;
}

[System.Serializable]
public class ObjectiveData
{
    public enum ObjectiveType { MoveTileToTarget, CollectAll, SurviveTurns }
    public ObjectiveType type;
    public string targetTileTypeKey;
    public Vector2Int targetPosition;
    
    public IObjective CreateObjective()
    {
        return type switch
        {
            ObjectiveType.MoveTileToTarget => 
                new MoveTileToTargetObjective(targetTileTypeKey, 
                    new CellPos { X = targetPosition.x, Y = targetPosition.y }),
            // ... other types
        };
    }
}
```

## Testing Objectives

```csharp
[Test]
public void ObjectiveEvaluator_MultipleObjectives_AllMustComplete()
{
    // Arrange
    var board = new BoardState(3, 3);
    var evaluator = new ObjectiveEvaluator();
    evaluator.Init(board);
    
    var obj1 = new MoveTileToTargetObjective("Brain", new CellPos { X = 0, Y = 0 });
    var obj2 = new MoveTileToTargetObjective("Motor", new CellPos { X = 1, Y = 1 });
    evaluator.AddObjective(obj1);
    evaluator.AddObjective(obj2);
    
    bool winFired = false;
    evaluator.OnObjectiveComplete += () => winFired = true;
    
    // Act - Complete only first objective
    board.TryPlaceTile(new CellPos { X = 0, Y = 0 }, CreateTile("Brain"));
    evaluator.EvaluateObjectives();
    
    // Assert - Win should not fire yet
    Assert.IsFalse(winFired);
    
    // Act - Complete second objective
    board.TryPlaceTile(new CellPos { X = 1, Y = 1 }, CreateTile("Motor"));
    evaluator.EvaluateObjectives();
    
    // Assert - Now win should fire
    Assert.IsTrue(winFired);
}
```

## Design Principles

### Keep Objectives Pure
- No side effects (don't modify board state)
- Deterministic (same board → same result)
- Fast evaluation (called frequently)

### Separate Objective from Evaluation
- IObjective: What to check (pure logic)
- ObjectiveEvaluator: When to check (game loop integration)
- This separation allows reusing objectives in different contexts

### Make Objectives Serializable
- Use primitive types (string, int, Vector2Int)
- Avoid object references (use TypeKey, not tile instance)
- This enables save/load, level editors, networking
```

---

## Success Criteria

- ✅ IObjective interface has comprehensive documentation
- ✅ Design pattern (Strategy) is explained
- ✅ How to create custom objectives is documented with examples
- ✅ Integration with game loop is clear
- ✅ Future extensions are documented

---

## Notes

### Completion - Minimum Playable Game
This task completes the minimum playable game:
- ✅ Tiles can be moved
- ✅ Movement is visualized
- ✅ Win condition is defined
- ✅ Player can win

Next steps (future tasks):
- Level progression system
- UI/Menu system
- Save/load
- Undo/redo
- Additional tile abilities
- Sound and music
- Particle effects

### Objective System as Foundation
The objective system is designed for extensibility:
- Easy to add new objective types
- Composable (combine objectives)
- Serializable (for level editors)
- Testable (pure C# logic)

This foundation supports future game modes:
- Campaign (sequential objectives)
- Challenge mode (unique objectives per level)
- Custom levels (player-defined objectives)

---

### Next Steps
With Task 05c complete, the foundational systems are fully documented.
Future tasks will build on this foundation:
- Level system and progression
- UI and menu flow
- Additional tile abilities
- Polish and juice
