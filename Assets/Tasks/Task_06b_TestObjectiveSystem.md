# Task 05b: Unit Test - Objective System

**Estimated Time:** 30-45 minutes  
**Prerequisites:** Task_05_SimpleObjective.md completed  
**Status:** Not Started

---

## Context

Task 05 implemented the objective system with win condition detection. Now we need automated tests to ensure:
- MoveTileToTargetObjective correctly evaluates win conditions
- ObjectiveEvaluator properly tracks and evaluates objectives
- Win events fire correctly
- Edge cases are handled (out of bounds, wrong tile, multiple evaluations)

**Current State:**
- ✅ Objective system works manually (verified in Task 05)
- ✅ MoveTileToTargetObjective implemented
- ✅ ObjectiveEvaluator implemented
- ❌ No automated tests
- ❌ No regression prevention

**Goal:** Create unit tests for the objective system.

---

## Goals

1. Create tests for MoveTileToTargetObjective evaluation
2. Create tests for ObjectiveEvaluator state management
3. Create tests for win event lifecycle
4. Create tests for edge cases (invalid targets, wrong tiles)
5. Create integration tests for complete objective flow

---

## Implementation Steps

### Step 1: Create MoveTileToTargetObjectiveTests.cs
**File:** `/Assets/Scripts/Gameplay/Engine/Tests/MoveTileToTargetObjectiveTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Engine.Objectives;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Engine.Tests
{
    [TestFixture]
    public class MoveTileToTargetObjectiveTests
    {
        private BoardState board;
        private ModuleTile brainTile;
        private ModuleTile motorTile;

        [SetUp]
        public void Setup()
        {
            board = new BoardState(3, 3);
            
            brainTile = new ModuleTile(
                1, "Brain",
                new DefaultMovementBehavior(new MovementRules(10, true, false)),
                new DefaultAbilityBehavior()
            );

            motorTile = new ModuleTile(
                2, "Motor",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior()
            );
        }

        [Test]
        public void IsComplete_CorrectTileAtTarget_ReturnsTrue()
        {
            // Arrange
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);
            board.TryPlaceTile(targetPos, brainTile);

            // Act
            bool result = objective.IsComplete(board);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsComplete_WrongTileAtTarget_ReturnsFalse()
        {
            // Arrange
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);
            board.TryPlaceTile(targetPos, motorTile); // Wrong tile

            // Act
            bool result = objective.IsComplete(board);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsComplete_EmptyTargetCell_ReturnsFalse()
        {
            // Arrange
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);
            // No tile at target

            // Act
            bool result = objective.IsComplete(board);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsComplete_TargetOutOfBounds_ReturnsFalse()
        {
            // Arrange
            var targetPos = new CellPos { X = 10, Y = 10 }; // Out of bounds
            var objective = new MoveTileToTargetObjective("Brain", targetPos);

            // Act
            bool result = objective.IsComplete(board);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsComplete_CorrectTileWrongPosition_ReturnsFalse()
        {
            // Arrange
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);
            board.TryPlaceTile(new CellPos { X = 0, Y = 0 }, brainTile); // Wrong position

            // Act
            bool result = objective.IsComplete(board);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Description_ReturnsCorrectString()
        {
            // Arrange
            var targetPos = new CellPos { X = 2, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);

            // Act
            string description = objective.Description;

            // Assert
            Assert.AreEqual("Move Brain to (2, 1)", description);
        }

        [Test]
        public void IsComplete_CaseSensitiveTypeKey_ReturnsFalse()
        {
            // Arrange
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("brain", targetPos); // Lowercase
            
            var brainTileUppercase = new ModuleTile(
                1, "Brain", // Uppercase
                new DefaultMovementBehavior(new MovementRules(10, true, false)),
                new DefaultAbilityBehavior()
            );
            board.TryPlaceTile(targetPos, brainTileUppercase);

            // Act
            bool result = objective.IsComplete(board);

            // Assert
            Assert.IsFalse(result, "TypeKey comparison should be case-sensitive");
        }
    }
}
```

### Step 2: Create ObjectiveEvaluatorTests.cs
**File:** `/Assets/Scripts/Gameplay/Game/Tests/ObjectiveEvaluatorTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Game.Controllers;
using Gameplay.Engine.Objectives;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Tests
{
    [TestFixture]
    public class ObjectiveEvaluatorTests
    {
        private GameObject evaluatorGameObject;
        private ObjectiveEvaluator evaluator;
        private BoardState board;
        private ModuleTile brainTile;

        [SetUp]
        public void Setup()
        {
            board = new BoardState(3, 3);
            
            brainTile = new ModuleTile(
                1, "Brain",
                new DefaultMovementBehavior(new MovementRules(10, true, false)),
                new DefaultAbilityBehavior()
            );

            evaluatorGameObject = new GameObject("TestObjectiveEvaluator");
            evaluator = evaluatorGameObject.AddComponent<ObjectiveEvaluator>();
            evaluator.Init(board);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(evaluatorGameObject);
        }

        [Test]
        public void AddObjective_AddsObjectiveToList()
        {
            // Arrange
            var objective = new MoveTileToTargetObjective("Brain", new CellPos { X = 1, Y = 1 });

            // Act
            evaluator.AddObjective(objective);

            // Assert - indirect verification via evaluation
            board.TryPlaceTile(new CellPos { X = 1, Y = 1 }, brainTile);
            Assert.IsTrue(evaluator.AreAllObjectivesComplete());
        }

        [Test]
        public void AreAllObjectivesComplete_NoObjectives_ReturnsFalse()
        {
            // Act
            bool result = evaluator.AreAllObjectivesComplete();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void AreAllObjectivesComplete_ObjectiveIncomplete_ReturnsFalse()
        {
            // Arrange
            var objective = new MoveTileToTargetObjective("Brain", new CellPos { X = 1, Y = 1 });
            evaluator.AddObjective(objective);
            // Brain not at target

            // Act
            bool result = evaluator.AreAllObjectivesComplete();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void AreAllObjectivesComplete_ObjectiveComplete_ReturnsTrue()
        {
            // Arrange
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);
            evaluator.AddObjective(objective);
            board.TryPlaceTile(targetPos, brainTile);

            // Act
            bool result = evaluator.AreAllObjectivesComplete();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EvaluateObjectives_ObjectiveComplete_FiresEvent()
        {
            // Arrange
            bool eventFired = false;
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);
            
            evaluator.AddObjective(objective);
            evaluator.OnAllObjectivesCompleted += () => eventFired = true;
            
            board.TryPlaceTile(targetPos, brainTile);

            // Act
            evaluator.EvaluateObjectives();

            // Assert
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void EvaluateObjectives_CalledTwice_EventFiresOnce()
        {
            // Arrange
            int eventCount = 0;
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);
            
            evaluator.AddObjective(objective);
            evaluator.OnAllObjectivesCompleted += () => eventCount++;
            
            board.TryPlaceTile(targetPos, brainTile);

            // Act
            evaluator.EvaluateObjectives();
            evaluator.EvaluateObjectives(); // Second call

            // Assert
            Assert.AreEqual(1, eventCount, "Event should fire only once");
        }

        [Test]
        public void IsGameWon_BeforeWin_ReturnsFalse()
        {
            // Assert
            Assert.IsFalse(evaluator.IsGameWon());
        }

        [Test]
        public void IsGameWon_AfterWin_ReturnsTrue()
        {
            // Arrange
            var targetPos = new CellPos { X = 1, Y = 1 };
            var objective = new MoveTileToTargetObjective("Brain", targetPos);
            evaluator.AddObjective(objective);
            board.TryPlaceTile(targetPos, brainTile);

            // Act
            evaluator.EvaluateObjectives();

            // Assert
            Assert.IsTrue(evaluator.IsGameWon());
        }

        [Test]
        public void Reset_ClearsObjectivesAndState()
        {
            // Arrange
            var objective = new MoveTileToTargetObjective("Brain", new CellPos { X = 1, Y = 1 });
            evaluator.AddObjective(objective);

            // Act
            evaluator.Reset();

            // Assert
            Assert.IsFalse(evaluator.IsGameWon());
            Assert.IsFalse(evaluator.AreAllObjectivesComplete());
        }

        [Test]
        public void OnObjectiveCompleted_FiresForEachObjective()
        {
            // Arrange
            int completedCount = 0;
            var objective = new MoveTileToTargetObjective("Brain", new CellPos { X = 1, Y = 1 });
            
            evaluator.AddObjective(objective);
            evaluator.OnObjectiveCompleted += (obj) => completedCount++;
            
            board.TryPlaceTile(new CellPos { X = 1, Y = 1 }, brainTile);

            // Act
            evaluator.EvaluateObjectives();

            // Assert
            Assert.AreEqual(1, completedCount);
        }
    }
}
```

### Step 3: Create MoveExecutor Integration Test with Objectives
**File:** Add to `/Assets/Scripts/Gameplay/Game/Tests/MoveExecutorTests.cs`

```csharp
// Add to existing MoveExecutorTests class

[Test]
public void ExecuteMove_WithObjectiveEvaluator_TriggersEvaluation()
{
    // Arrange
    var evaluatorGameObject = new GameObject("TestEvaluator");
    var objectiveEvaluator = evaluatorGameObject.AddComponent<ObjectiveEvaluator>();
    objectiveEvaluator.Init(board);

    var targetPos = new CellPos { X = 1, Y = 1 };
    var objective = new MoveTileToTargetObjective("TestTile", targetPos);
    objectiveEvaluator.AddObjective(objective);

    bool eventFired = false;
    objectiveEvaluator.OnAllObjectivesCompleted += () => eventFired = true;

    var executorWithEvaluator = new MoveExecutor(board, boardPresenter, objectiveEvaluator);

    var from = new CellPos { X = 0, Y = 0 };

    // Act
    executorWithEvaluator.ExecuteMove(from, targetPos);

    // Assert
    Assert.IsTrue(eventFired, "Objective should be evaluated and completed after move");

    // Cleanup
    Object.DestroyImmediate(evaluatorGameObject);
}

[Test]
public void ExecuteMove_ObjectiveNotMet_NoEventFired()
{
    // Arrange
    var evaluatorGameObject = new GameObject("TestEvaluator");
    var objectiveEvaluator = evaluatorGameObject.AddComponent<ObjectiveEvaluator>();
    objectiveEvaluator.Init(board);

    var targetPos = new CellPos { X = 2, Y = 2 }; // Different from move destination
    var objective = new MoveTileToTargetObjective("TestTile", targetPos);
    objectiveEvaluator.AddObjective(objective);

    bool eventFired = false;
    objectiveEvaluator.OnAllObjectivesCompleted += () => eventFired = true;

    var executorWithEvaluator = new MoveExecutor(board, boardPresenter, objectiveEvaluator);

    var from = new CellPos { X = 0, Y = 0 };
    var to = new CellPos { X = 1, Y = 1 }; // Not the target

    // Act
    executorWithEvaluator.ExecuteMove(from, to);

    // Assert
    Assert.IsFalse(eventFired, "Objective should not be complete");

    // Cleanup
    Object.DestroyImmediate(evaluatorGameObject);
}
```

### Step 4: Run All Tests
1. Open Unity Test Runner
2. Run all tests in EditMode
3. Verify:
   - MoveTileToTargetObjectiveTests (7 tests)
   - ObjectiveEvaluatorTests (9 tests)
   - MoveExecutor integration tests (2 additional tests)

---

## Test Checklist

### Setup Verification
- [ ] All test files compile without errors
- [ ] Tests appear in Unity Test Runner
- [ ] Required assemblies referenced

### Test Execution
- [ ] All MoveTileToTargetObjectiveTests pass (7 tests)
- [ ] All ObjectiveEvaluatorTests pass (9 tests)
- [ ] MoveExecutor integration tests pass (2 tests)
- [ ] No warnings during test execution
- [ ] Tests run in <5 seconds

### Coverage Check
- [ ] MoveTileToTargetObjective.IsComplete tested (success path)
- [ ] MoveTileToTargetObjective edge cases tested (wrong tile, out of bounds, empty cell)
- [ ] ObjectiveEvaluator.AddObjective tested
- [ ] ObjectiveEvaluator.EvaluateObjectives tested
- [ ] Win event lifecycle tested (fires once, not multiple times)
- [ ] ObjectiveEvaluator.Reset tested
- [ ] MoveExecutor + ObjectiveEvaluator integration tested

---

## Success Criteria

- ✅ All unit tests pass (18+ total new tests)
- ✅ MoveTileToTargetObjective evaluation logic covered
- ✅ ObjectiveEvaluator state management covered
- ✅ Win event lifecycle covered
- ✅ Edge cases tested (invalid targets, wrong tiles, case sensitivity)
- ✅ Integration between MoveExecutor and ObjectiveEvaluator tested

---

## Notes

### Testing Strategy
- **Engine Layer Tests** - MoveTileToTargetObjective is pure logic, easily testable
- **Game Layer Tests** - ObjectiveEvaluator requires MonoBehaviour but minimal Unity dependencies
- **Integration Tests** - Verify MoveExecutor → ObjectiveEvaluator coordination

### What's Not Tested (Acceptable)
- **UI Interaction** - Win screen display (requires visual/Play Mode tests)
- **TargetCellIndicator** - Visual indicator rendering (requires Play Mode)
- **Multiple Objectives** - Deferred until multiple objective feature implemented

### Test Patterns Used
- **Arrange-Act-Assert** - Standard unit test structure
- **Event Testing** - Verify events fire with correct parameters
- **State Testing** - Check internal state (IsGameWon, AreAllObjectivesComplete)
- **Edge Case Testing** - Boundary conditions, invalid inputs

### Common Test Failures
- **"Event fires multiple times"** → Check gameWon flag in ObjectiveEvaluator
- **"TypeKey mismatch"** → Verify case sensitivity in MoveTileToTargetObjective
- **"Objective not evaluated"** → Check ObjectiveEvaluator.Init() called with valid board
- **"NullReferenceException"** → Ensure MonoBehaviour created in Setup

### Next Steps
After tests pass, proceed to **Task_05c_Documentation.md** to document the objective system.

Once Task_05c is complete, you will have a **complete minimum playable game** that is fully documented:
- ✅ Board visualization
- ✅ Tile selection
- ✅ Move preview
- ✅ Move execution
- ✅ Win condition
- ✅ Comprehensive test coverage

**Suggested next features** (beyond these 5 tasks):
- Level definitions (ScriptableObject-based level data)
- Multiple objectives (AND/OR logic)
- Move counter / par system
- Level progression (load next level after win)
- Abilities implementation (actual tile abilities, not stub)
- Undo system (command pattern)

### Related Files
- `/Assets/Scripts/Gameplay/Engine/Objectives/MoveTileToTargetObjective.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/ObjectiveEvaluator.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/MoveExecutor.cs`
