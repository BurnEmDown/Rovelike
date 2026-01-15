# Task 05: Simple Objective System

**Estimated Time:** 45-60 minutes  
**Prerequisites:** Task_04b_VisualTestVerification.md completed  
**Status:** Not Started

---

## Context

The game now has a complete movement system (Tasks 01-04), but there's no win condition. Players can move tiles indefinitely with no goal. This task implements a simple objective system:
- **Win Condition:** Move the Brain tile to a target cell
- **Objective Evaluation:** Check after each move
- **UI Feedback:** Simple win message (console log MVP, can add UI later)

This completes the **minimum playable game loop**: Start → Move tiles → Win.

**Current State:**
- ✅ Tiles can be moved
- ✅ BoardState tracks all tiles
- ❌ No objectives defined
- ❌ No win condition checking
- ❌ No game-over state

**Goal:** Implement simple "Move Brain to target cell" objective with win detection.

---

## Goals

1. Create `IObjective` interface for extensible objective system
2. Implement `MoveTileToTargetObjective` (Brain → specific cell)
3. Create `ObjectiveEvaluator` to check win conditions after moves
4. Add simple win notification (console log MVP)
5. (Optional) Add basic UI win screen

---

## Implementation Steps

### Step 1: Create IObjective Interface
**File:** `/Assets/Scripts/Gameplay/Engine/Objectives/IObjective.cs`

```csharp
using Gameplay.Engine.Board;

namespace Gameplay.Engine.Objectives
{
    /// <summary>
    /// Represents a win condition that can be evaluated against the current board state.
    /// </summary>
    public interface IObjective
    {
        /// <summary>
        /// Checks if the objective has been completed based on the current board state.
        /// </summary>
        /// <param name="board">The current board state to evaluate.</param>
        /// <returns>True if objective is complete (win condition met), false otherwise.</returns>
        bool IsComplete(IBoardState board);

        /// <summary>
        /// Human-readable description of what the objective requires.
        /// </summary>
        string Description { get; }
    }
}
```

### Step 2: Create MoveTileToTargetObjective
**File:** `/Assets/Scripts/Gameplay/Engine/Objectives/MoveTileToTargetObjective.cs`

```csharp
using Gameplay.Engine.Board;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Engine.Objectives
{
    /// <summary>
    /// Objective: Move a specific tile (by TypeKey) to a target board position.
    /// Commonly used for "Move Brain to target cell" win condition.
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
        public MoveTileToTargetObjective(string targetTileTypeKey, CellPos targetPosition)
        {
            this.targetTileTypeKey = targetTileTypeKey;
            this.targetPosition = targetPosition;
        }

        public bool IsComplete(IBoardState board)
        {
            // Check if target position is in bounds
            if (!board.IsInsideBounds(targetPosition))
                return false;

            // Get tile at target position
            var tile = board.GetTileAt(targetPosition);
            
            // Check if it's the correct tile type
            return tile != null && tile.TypeKey == targetTileTypeKey;
        }
    }
}
```

### Step 3: Create ObjectiveEvaluator
**File:** `/Assets/Scripts/Gameplay/Game/Controllers/ObjectiveEvaluator.cs`

```csharp
using System.Collections.Generic;
using Gameplay.Engine.Board;
using Gameplay.Engine.Objectives;
using UnityEngine;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Evaluates game objectives and triggers win/loss events.
    /// Checks objectives after each significant game action (e.g., after a move).
    /// </summary>
    public class ObjectiveEvaluator : MonoBehaviour
    {
        private readonly List<IObjective> objectives = new();
        private IBoardState? board;
        private bool gameWon = false;

        // Events
        public event System.Action<IObjective>? OnObjectiveCompleted;
        public event System.Action? OnAllObjectivesCompleted;

        public void Init(IBoardState boardState)
        {
            board = boardState;
        }

        /// <summary>
        /// Registers an objective to be evaluated.
        /// </summary>
        public void AddObjective(IObjective objective)
        {
            objectives.Add(objective);
            Debug.Log($"[ObjectiveEvaluator] Objective added: {objective.Description}");
        }

        /// <summary>
        /// Evaluates all objectives. Call this after any board-changing action (e.g., move).
        /// </summary>
        public void EvaluateObjectives()
        {
            if (board == null || gameWon)
                return;

            foreach (var objective in objectives)
            {
                if (objective.IsComplete(board))
                {
                    Debug.Log($"[ObjectiveEvaluator] ✓ Objective complete: {objective.Description}");
                    OnObjectiveCompleted?.Invoke(objective);
                }
            }

            // Check if all objectives complete
            if (AreAllObjectivesComplete())
            {
                gameWon = true;
                Debug.Log("[ObjectiveEvaluator] ★ ALL OBJECTIVES COMPLETE - YOU WIN! ★");
                OnAllObjectivesCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Checks if all registered objectives are complete.
        /// </summary>
        public bool AreAllObjectivesComplete()
        {
            if (board == null || objectives.Count == 0)
                return false;

            foreach (var objective in objectives)
            {
                if (!objective.IsComplete(board))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns current game won state.
        /// </summary>
        public bool IsGameWon() => gameWon;

        /// <summary>
        /// Resets evaluator state (for level restart).
        /// </summary>
        public void Reset()
        {
            objectives.Clear();
            gameWon = false;
        }
    }
}
```

### Step 4: Integrate with MoveExecutor
**File:** Update `/Assets/Scripts/Gameplay/Game/Controllers/MoveExecutor.cs`

**Add field:**
```csharp
private readonly ObjectiveEvaluator? objectiveEvaluator;
```

**Update constructor:**
```csharp
public MoveExecutor(IBoardState boardState, BoardPresenter boardPresenter, ObjectiveEvaluator? objectiveEvaluator = null)
{
    this.boardState = boardState;
    this.boardPresenter = boardPresenter;
    this.objectiveEvaluator = objectiveEvaluator;
}
```

**Update ExecuteMove method (at end, after successful move):**
```csharp
public bool ExecuteMove(CellPos from, CellPos to)
{
    // ... existing validation and move logic ...

    Debug.Log($"[MoveExecutor] Moved {tile.TypeKey} from ({from.X},{from.Y}) to ({to.X},{to.Y})");
    
    // Evaluate objectives after move
    objectiveEvaluator?.EvaluateObjectives(); // ADD THIS

    return true;
}
```

### Step 5: Wire Up in BoardController
**File:** Update `/Assets/Scripts/Gameplay/Game/Controllers/BoardController.cs`

**Add field:**
```csharp
[Header("Objectives")]
[SerializeField] private ObjectiveEvaluator objectiveEvaluator = null!;
[SerializeField] private int targetCellX = 2;
[SerializeField] private int targetCellY = 1;
```

**Update InitializeGameSystems() method:**
```csharp
private void InitializeGameSystems()
{
    // ... existing initialization ...

    // Initialize objectives
    InitializeObjectives();

    // Create move executor WITH objective evaluator
    moveExecutor = new MoveExecutor(board, boardPresenter, objectiveEvaluator);

    // ... rest of initialization ...
}

private void InitializeObjectives()
{
    objectiveEvaluator.Init(board);

    // Create "Move Brain to target" objective
    var targetPos = new CellPos { X = targetCellX, Y = targetCellY };
    var brainObjective = new MoveTileToTargetObjective("Brain", targetPos);
    
    objectiveEvaluator.AddObjective(brainObjective);

    // Subscribe to win event
    objectiveEvaluator.OnAllObjectivesCompleted += HandleWin;

    Debug.Log($"[BoardController] Objective: {brainObjective.Description}");
}

private void HandleWin()
{
    Debug.Log("[BoardController] ★★★ GAME WON ★★★");
    
    // Optional: Show UI, disable input, play effects, etc.
    // For MVP, just log to console
}

private void OnDestroy()
{
    if (objectiveEvaluator != null)
        objectiveEvaluator.OnAllObjectivesCompleted -= HandleWin;
}
```

**In Unity Inspector:**
1. Create empty GameObject: "ObjectiveEvaluator"
2. Add `ObjectiveEvaluator` component
3. Assign to `objectiveEvaluator` field in BoardController
4. Set `targetCellX` and `targetCellY` to desired win position (e.g., 2, 1)

### Step 6: Add Visual Target Indicator (Optional)
**File:** Create `/Assets/Scripts/Gameplay/Presentation/Board/TargetCellIndicator.cs`

```csharp
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Presentation.Board
{
    /// <summary>
    /// Visualizes the target cell where a tile must be moved to win.
    /// </summary>
    public class TargetCellIndicator : MonoBehaviour
    {
        [SerializeField] private GameObject indicatorPrefab = null!;
        [SerializeField] private Transform indicatorParent = null!;

        private GameObject? activeIndicator;

        /// <summary>
        /// Shows a target indicator at the specified board position.
        /// </summary>
        public void ShowTarget(CellPos targetPosition, Vector3 worldPosition)
        {
            HideTarget();

            if (indicatorPrefab == null)
            {
                Debug.LogWarning("[TargetCellIndicator] Indicator prefab not assigned!");
                return;
            }

            activeIndicator = Instantiate(indicatorPrefab, worldPosition, Quaternion.identity, indicatorParent);
            activeIndicator.name = $"TargetIndicator_({targetPosition.X},{targetPosition.Y})";
        }

        /// <summary>
        /// Hides the target indicator.
        /// </summary>
        public void HideTarget()
        {
            if (activeIndicator != null)
            {
                Destroy(activeIndicator);
                activeIndicator = null;
            }
        }
    }
}
```

**Unity Setup for Target Indicator:**
1. Create prefab: "TargetIndicator"
   - Add SpriteRenderer with star/flag icon
   - Color: Gold (R:1, G:0.8, B:0, A:0.8)
   - Add optional pulse animation
2. Create GameObject: "TargetCellIndicator" in scene
3. Add component, assign prefab
4. In BoardController, show target at start:
```csharp
// In InitializeObjectives():
var targetIndicator = FindObjectOfType<TargetCellIndicator>();
if (targetIndicator != null)
{
    var worldPos = boardPresenter.BoardToWorld(targetPos);
    targetIndicator.ShowTarget(targetPos, worldPos);
}
```

### Step 7: Add Simple Win UI (Optional)
**File:** Create `/Assets/Scripts/Gameplay/Presentation/UI/WinScreen.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Presentation.UI
{
    /// <summary>
    /// Simple win screen UI that appears when objectives are complete.
    /// </summary>
    public class WinScreen : MonoBehaviour
    {
        [SerializeField] private GameObject winPanel = null!;
        [SerializeField] private Text winText = null!;

        private void Awake()
        {
            Hide();
        }

        public void Show()
        {
            if (winPanel != null)
                winPanel.SetActive(true);

            if (winText != null)
                winText.text = "YOU WIN!";

            Debug.Log("[WinScreen] Win screen shown");
        }

        public void Hide()
        {
            if (winPanel != null)
                winPanel.SetActive(false);
        }
    }
}
```

**Unity UI Setup:**
1. Create Canvas (if not exists)
2. Create UI → Panel (name: "WinPanel")
   - Semi-transparent background
   - Center anchors
3. Add UI → Text child (name: "WinText")
   - Text: "YOU WIN!"
   - Large font size (48+)
   - Center alignment
4. Add WinScreen component to WinPanel
5. Assign references in inspector
6. In BoardController.HandleWin():
```csharp
private void HandleWin()
{
    Debug.Log("[BoardController] ★★★ GAME WON ★★★");
    
    var winScreen = FindObjectOfType<WinScreen>();
    winScreen?.Show();
}
```

---

## Test Checklist

### Manual Testing
- [ ] Enter Play Mode
- [ ] Console shows objective: "Move Brain to (X, Y)"
- [ ] (Optional) Gold star/flag visible at target cell
- [ ] Move Brain tile to target position
- [ ] Console logs "Objective complete" immediately after move
- [ ] Console logs "ALL OBJECTIVES COMPLETE - YOU WIN!"
- [ ] (Optional) Win UI appears
- [ ] Moving other tiles does NOT trigger win
- [ ] Moving Brain to wrong cell does NOT trigger win

### Code Verification
- [ ] ObjectiveEvaluator.EvaluateObjectives() called after each move
- [ ] MoveTileToTargetObjective.IsComplete() returns correct boolean
- [ ] OnAllObjectivesCompleted event fires exactly once
- [ ] Game win state persists (gameWon flag prevents re-evaluation)

### Edge Cases
- [ ] Move Brain to target on first move - win triggers immediately
- [ ] Move Brain away from target - win doesn't trigger
- [ ] Move Brain to target, then move again - win doesn't trigger twice
- [ ] Target cell out of bounds - objective never completes (graceful)

---

## Success Criteria

- ✅ IObjective interface created and documented
- ✅ MoveTileToTargetObjective implemented correctly
- ✅ ObjectiveEvaluator tracks and evaluates objectives
- ✅ Objectives evaluated after each move
- ✅ Win condition detected correctly (Brain at target)
- ✅ Win event fires exactly once
- ✅ Console logs clear win message
- ✅ (Optional) Target cell visually indicated
- ✅ (Optional) Win UI appears

---

## Notes

### Design Decisions
- **Single Objective MVP:** Only "Move Brain to target" for simplicity
- **Hardcoded Target:** Target position set in inspector (no level definition system yet)
- **Console Win UI:** MVP uses Debug.Log, can add proper UI later
- **No Loss Condition:** Infinite moves allowed (can add move limit later)

### Future Extensions (Not This Task)
- **Multiple Objectives:** Combine objectives with AND/OR logic
- **Complex Objectives:** "Move 3 tiles to target", "Connect two tiles", etc.
- **Objective Definitions:** Load from ScriptableObject or JSON
- **Move Counter:** Track moves, add par/optimal move count
- **Loss Conditions:** Move limit, time limit, blocked state detection
- **Level Progression:** Load next level after win

### Architectural Notes
- **IObjective** - Extensible interface for future objective types
- **ObjectiveEvaluator** - Centralized evaluation logic, event-driven
- **MoveExecutor Integration** - Evaluates after each move automatically
- **Engine Layer** - IObjective and implementations live in Engine (Unity-free)

### Common Issues
- **"Win triggers multiple times"** → Check gameWon flag is set correctly
- **"Win doesn't trigger"** → Verify TypeKey matches exactly (case-sensitive)
- **"Target cell wrong"** → Check BoardPresenter.BoardToWorld() calculation
- **"Objective evaluator not assigned"** → Check inspector references

### Next Task
After this task works, proceed to **Task_05b_TestObjectiveSystem.md** to add unit tests for objectives.

### Related Files
- `/Assets/Scripts/Gameplay/Engine/Objectives/IObjective.cs`
- `/Assets/Scripts/Gameplay/Engine/Objectives/MoveTileToTargetObjective.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/ObjectiveEvaluator.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/MoveExecutor.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/BoardController.cs`
