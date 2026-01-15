# Task 03b: Unit Test - Movement Execution

**Estimated Time:** 45-60 minutes  
**Prerequisites:** Task_03_MovementExecution.md completed  
**Status:** Not Started

---

## Context

Task 03 implemented the movement execution system with selection, preview, and move execution. Now we need automated tests to ensure:
- TileSelectionController tracks selection state correctly
- MoveExecutor coordinates engine + presentation updates
- Selection events fire properly
- Move validation works
- Edge cases are handled (out of bounds, occupied cells, etc.)

**Current State:**
- ✅ Movement execution works manually (verified in Task 03)
- ✅ Selection controller implemented
- ✅ MoveExecutor implemented
- ❌ No automated tests
- ❌ No regression prevention

**Goal:** Create unit tests for the movement execution system.

---

## Goals

1. Create tests for TileSelectionController state management
2. Create tests for MoveExecutor validation and execution
3. Create tests for selection event lifecycle
4. Create tests for move validation logic
5. Create integration tests for complete move flow

---

## Implementation Steps

### Step 1: Create MoveExecutorTests.cs
**File:** `/Assets/Scripts/Gameplay/Game/Tests/MoveExecutorTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Game.Controllers;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using Gameplay.Presentation.Board;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Tests
{
    [TestFixture]
    public class MoveExecutorTests
    {
        private BoardState board;
        private GameObject presenterGameObject;
        private BoardPresenter boardPresenter;
        private MoveExecutor moveExecutor;
        private ModuleTile testTile;

        [SetUp]
        public void Setup()
        {
            // Create test board
            board = new BoardState(3, 3);

            // Create test tile
            testTile = new ModuleTile(
                1,
                "TestTile",
                new DefaultMovementBehavior(new MovementRules(10, true, true)),
                new DefaultAbilityBehavior()
            );

            // Place tile at (0, 0)
            board.TryPlaceTile(new CellPos { X = 0, Y = 0 }, testTile);

            // Create mock BoardPresenter
            presenterGameObject = new GameObject("TestBoardPresenter");
            boardPresenter = presenterGameObject.AddComponent<BoardPresenter>();

            // Create MoveExecutor
            moveExecutor = new MoveExecutor(board, boardPresenter);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(presenterGameObject);
        }

        [Test]
        public void ExecuteMove_ValidMove_ReturnsTrue()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 1, Y = 1 };

            // Act
            bool result = moveExecutor.ExecuteMove(from, to);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void ExecuteMove_ValidMove_UpdatesBoardState()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 1, Y = 1 };

            // Act
            moveExecutor.ExecuteMove(from, to);

            // Assert
            Assert.IsNull(board.GetTileAt(from), "Origin should be empty");
            Assert.AreEqual(testTile, board.GetTileAt(to), "Destination should contain tile");
        }

        [Test]
        public void ExecuteMove_OutOfBounds_ReturnsFalse()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 10, Y = 10 }; // Out of bounds

            // Act
            bool result = moveExecutor.ExecuteMove(from, to);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(testTile, board.GetTileAt(from), "Tile should not move");
        }

        [Test]
        public void ExecuteMove_NoTileAtOrigin_ReturnsFalse()
        {
            // Arrange
            var from = new CellPos { X = 2, Y = 2 }; // Empty cell
            var to = new CellPos { X = 1, Y = 1 };

            // Act
            bool result = moveExecutor.ExecuteMove(from, to);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void ExecuteMove_DestinationOccupied_ReturnsFalse()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 1, Y = 1 };

            // Place another tile at destination
            var blockingTile = new ModuleTile(
                2,
                "BlockingTile",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior()
            );
            board.TryPlaceTile(to, blockingTile);

            // Act
            bool result = moveExecutor.ExecuteMove(from, to);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(testTile, board.GetTileAt(from), "Origin tile should not move");
            Assert.AreEqual(blockingTile, board.GetTileAt(to), "Destination should still have blocking tile");
        }

        [Test]
        public void ExecuteMove_FromOutOfBounds_ReturnsFalse()
        {
            // Arrange
            var from = new CellPos { X = -1, Y = 0 }; // Out of bounds
            var to = new CellPos { X = 1, Y = 1 };

            // Act
            bool result = moveExecutor.ExecuteMove(from, to);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
```

### Step 2: Create TileSelectionControllerTests.cs (Play Mode Test)
**File:** `/Assets/Scripts/Gameplay/Game/Tests/TileSelectionControllerTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Game.Controllers;
using Gameplay.Presentation.Tiles;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using UnityCoreKit.Runtime.UserInteractions;
using UnityCoreKit.Runtime.Core;
using UnityCoreKit.Runtime.Core.Services;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Tests
{
    [TestFixture]
    public class TileSelectionControllerTests
    {
        private GameObject controllerGameObject;
        private TileSelectionController selectionController;
        private GameObject tileView1GameObject;
        private TileView tileView1;
        private GameObject tileView2GameObject;
        private TileView tileView2;
        private EventsManager eventsManager;
        private UserInteractions userInteractions;

        [SetUp]
        public void Setup()
        {
            // Setup CoreServices with EventsManager
            eventsManager = new EventsManager();
            userInteractions = new UserInteractions(eventsManager);
            
            // Mock CoreServices (simplified - in real test you'd use DI or test service locator)
            // For now, assume CoreServices.Get<IUserInteractions>() works

            // Create controller
            controllerGameObject = new GameObject("SelectionController");
            selectionController = controllerGameObject.AddComponent<TileSelectionController>();

            // Create test tiles
            var tile1 = new ModuleTile(1, "Tile1",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior());
            var tile2 = new ModuleTile(2, "Tile2",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior());

            // Create TileViews
            tileView1GameObject = new GameObject("TileView1");
            tileView1 = tileView1GameObject.AddComponent<TileView>();
            tileView1GameObject.AddComponent<SpriteRenderer>();
            tileView1.Init(tile1, new CellPos { X = 0, Y = 0 });

            tileView2GameObject = new GameObject("TileView2");
            tileView2 = tileView2GameObject.AddComponent<TileView>();
            tileView2GameObject.AddComponent<SpriteRenderer>();
            tileView2.Init(tile2, new CellPos { X = 1, Y = 1 });
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(controllerGameObject);
            Object.DestroyImmediate(tileView1GameObject);
            Object.DestroyImmediate(tileView2GameObject);
        }

        [Test]
        public void GetSelectedTile_Initially_ReturnsNull()
        {
            // Assert
            Assert.IsNull(selectionController.GetSelectedTile());
        }

        [Test]
        public void OnTileSelected_Event_FiresWhenTileSelected()
        {
            // Arrange
            TileView? selectedTile = null;
            selectionController.OnTileSelected += (tile) => selectedTile = tile;

            // Act
            // Note: This requires emitting a UserInteractionEvent, which needs CoreServices setup
            // For unit tests, we'd need to make HandleTileClick public or use reflection
            // Alternatively, create a public SelectTile method for testing

            // For now, this demonstrates the test structure
            // Actual implementation may require refactoring for testability

            // Assert
            // Assert.AreEqual(tileView1, selectedTile);
        }

        [Test]
        public void ClearSelection_WithSelection_ClearsTile()
        {
            // Arrange
            // (Assume tile1 is selected via some mechanism)

            // Act
            selectionController.ClearSelection();

            // Assert
            Assert.IsNull(selectionController.GetSelectedTile());
        }
    }
}
```

**Note:** The above test is incomplete because TileSelectionController doesn't expose selection methods publicly. See Step 3 for refactoring recommendations.

### Step 3: Refactor TileSelectionController for Testability

**Add to TileSelectionController.cs:**
```csharp
// Add this method for testing purposes
#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
/// <summary>
/// Directly selects a tile (for testing only).
/// In production, selection happens via HandleTileClick.
/// </summary>
public void SelectTileForTest(TileView tileView)
{
    SelectTile(tileView);
}
#endif
```

**Update TileSelectionControllerTests.cs:**
```csharp
[Test]
public void OnTileSelected_Event_FiresWhenTileSelected()
{
    // Arrange
    TileView? selectedTile = null;
    selectionController.OnTileSelected += (tile) => selectedTile = tile;

    // Act
    selectionController.SelectTileForTest(tileView1);

    // Assert
    Assert.AreEqual(tileView1, selectedTile);
    Assert.AreEqual(tileView1, selectionController.GetSelectedTile());
}

[Test]
public void SelectTile_Twice_KeepsLatestSelection()
{
    // Act
    selectionController.SelectTileForTest(tileView1);
    selectionController.SelectTileForTest(tileView2);

    // Assert
    Assert.AreEqual(tileView2, selectionController.GetSelectedTile());
}

[Test]
public void ClearSelection_WithSelection_ClearsTile()
{
    // Arrange
    selectionController.SelectTileForTest(tileView1);

    // Act
    selectionController.ClearSelection();

    // Assert
    Assert.IsNull(selectionController.GetSelectedTile());
}

[Test]
public void OnTileDeselected_Event_FiresWhenCleared()
{
    // Arrange
    bool deselectedFired = false;
    selectionController.OnTileDeselected += () => deselectedFired = true;
    selectionController.SelectTileForTest(tileView1);

    // Act
    selectionController.ClearSelection();

    // Assert
    Assert.IsTrue(deselectedFired);
}
```

### Step 4: Create BoardStateMovementTests.cs
**File:** `/Assets/Scripts/Gameplay/Engine/Tests/BoardStateMovementTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Engine.Tests
{
    [TestFixture]
    public class BoardStateMovementTests
    {
        private BoardState board;
        private ModuleTile testTile;

        [SetUp]
        public void Setup()
        {
            board = new BoardState(3, 3);
            testTile = new ModuleTile(
                1,
                "TestTile",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior()
            );
        }

        [Test]
        public void MoveTile_ValidMove_UpdatesPositions()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 1, Y = 1 };
            board.TryPlaceTile(from, testTile);

            // Act
            board.MoveTile(from, to);

            // Assert
            Assert.IsNull(board.GetTileAt(from));
            Assert.AreEqual(testTile, board.GetTileAt(to));
        }

        [Test]
        public void MoveTile_ToOccupiedCell_OverwritesTile()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 1, Y = 1 };
            var otherTile = new ModuleTile(
                2,
                "OtherTile",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior()
            );

            board.TryPlaceTile(from, testTile);
            board.TryPlaceTile(to, otherTile);

            // Act
            board.MoveTile(from, to);

            // Assert
            Assert.IsNull(board.GetTileAt(from));
            Assert.AreEqual(testTile, board.GetTileAt(to));
        }

        [Test]
        public void MoveTile_OutOfBounds_ThrowsException()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 10, Y = 10 };
            board.TryPlaceTile(from, testTile);

            // Act & Assert
            Assert.Throws<System.ArgumentOutOfRangeException>(() => board.MoveTile(from, to));
        }

        [Test]
        public void MoveTile_EmptyOrigin_DoesNotCrash()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 1, Y = 1 };
            // No tile at 'from'

            // Act & Assert - should not crash, just move null
            Assert.DoesNotThrow(() => board.MoveTile(from, to));
            Assert.IsNull(board.GetTileAt(to));
        }
    }
}
```

### Step 5: Run All Tests
1. Open Unity Test Runner
2. Run all tests in EditMode
3. Verify:
   - MoveExecutorTests (6+ tests)
   - TileSelectionControllerTests (4+ tests)
   - BoardStateMovementTests (4+ tests)

---

## Test Checklist

### Setup Verification
- [ ] All test files compile without errors
- [ ] Tests appear in Unity Test Runner
- [ ] Required assemblies referenced

### Test Execution
- [ ] All MoveExecutorTests pass (6 tests)
- [ ] All TileSelectionControllerTests pass (4 tests)
- [ ] All BoardStateMovementTests pass (5 tests)
- [ ] No warnings during test execution
- [ ] Tests run in <5 seconds

### Coverage Check
- [ ] MoveExecutor.ExecuteMove tested (success path)
- [ ] MoveExecutor.ExecuteMove tested (failure paths)
- [ ] TileSelectionController selection state tested
- [ ] Selection events tested
- [ ] BoardState.MoveTile edge cases tested
- [ ] Move validation tested

---

## Success Criteria

- ✅ All unit tests pass (15+ total)
- ✅ MoveExecutor validation logic covered
- ✅ TileSelectionController state management covered
- ✅ BoardState movement operations covered
- ✅ Edge cases tested (out of bounds, occupied cells, null tiles)
- ✅ Event lifecycle tested

---

## Notes

### Testing Challenges
- **Unity MonoBehaviours** - Some tests require GameObject setup (slower)
- **CoreServices** - Tests that depend on CoreServices need mock setup or refactoring
- **Event System** - Testing event subscriptions requires careful setup/teardown

### Testability Improvements
- Added `SelectTileForTest` method wrapped in `#if UNITY_INCLUDE_TESTS`
- This allows direct testing without simulating full user interaction flow
- Alternative: Extract selection logic into testable service class

### What's Not Tested (Acceptable)
- **MovePreviewController** - Requires GameObject/highlight instantiation (integration test territory)
- **DestinationClickHandler** - Requires full Unity event system (Play Mode test)
- **Visual highlight rendering** - Requires Play Mode or visual regression tests

### Common Test Failures
- **"CoreServices not initialized"** → Tests need service locator mocking
- **"NullReferenceException on event"** → Check event subscriptions in Setup
- **"GameObject destroyed"** → Ensure TearDown properly cleans up

### Next Task
After tests pass, proceed to **Task_04_VisualFeedback.md** to add selection highlights and move animations.

### Related Files
- `/Assets/Scripts/Gameplay/Game/Controllers/TileSelectionController.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/MoveExecutor.cs`
- `/Assets/Scripts/Gameplay/Engine/Board/BoardState.cs`
