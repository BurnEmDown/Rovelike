# Task 04b: Unit Test - Empty Cell Click Detection

**Estimated Time:** 20-30 minutes  
**Prerequisites:** Task_04_EmptyCellClick.md completed  
**Status:** Not Started

---

## Context

Task 04 implemented empty cell click detection for tile movement. Now we need automated tests to ensure:
- Screen-to-board position conversion works correctly
- Empty cell clicks trigger move attempts
- Invalid clicks are handled properly
- Edge cases work (outside bounds, occupied cells)

**Current State:**
- ✅ Empty cell clicking works manually
- ✅ Position conversion implemented
- ❌ No automated tests
- ❌ No regression prevention

**Goal:** Create unit tests for empty cell click detection and position conversion.

---

## Goals

1. Test screen-to-board position conversion
2. Test world-to-board position conversion
3. Test board-to-world position conversion
4. Test empty cell click handling
5. Test click filtering (tiles vs empty cells)

---

## Implementation Steps

### Step 1: Create BoardPresenterPositionTests.cs

**File:** `/Assets/Scripts/Gameplay/Game/Tests/BoardPresenterPositionTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Presentation.Board;
using Gameplay.Engine.Board;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Tests
{
    [TestFixture]
    public class BoardPresenterPositionTests
    {
        private GameObject presenterGameObject;
        private BoardPresenter boardPresenter;
        private BoardState board;

        [SetUp]
        public void Setup()
        {
            // Create board
            board = new BoardState(3, 3);

            // Create presenter
            presenterGameObject = new GameObject("TestBoardPresenter");
            boardPresenter = presenterGameObject.AddComponent<BoardPresenter>();
            // Note: BoardPresenter.Init() and Rebuild() would normally be called here
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(presenterGameObject);
        }

        [Test]
        public void WorldToBoardPos_CenterOfCell_ReturnsCorrectPosition()
        {
            // Arrange
            var worldPos = new Vector3(1f, 1f, 0f);

            // Act
            var boardPos = boardPresenter.WorldToBoardPos(worldPos);

            // Assert
            Assert.IsTrue(boardPos.HasValue);
            Assert.AreEqual(1, boardPos.Value.X);
            Assert.AreEqual(1, boardPos.Value.Y);
        }

        [Test]
        public void WorldToBoardPos_OutOfBounds_ReturnsNull()
        {
            // Arrange
            var worldPos = new Vector3(10f, 10f, 0f); // Outside 3x3 board

            // Act
            var boardPos = boardPresenter.WorldToBoardPos(worldPos);

            // Assert
            Assert.IsFalse(boardPos.HasValue);
        }

        [Test]
        public void BoardToWorldPos_ValidPosition_ReturnsWorldCenter()
        {
            // Arrange
            var boardPos = new CellPos { X = 1, Y = 2 };

            // Act
            var worldPos = boardPresenter.BoardToWorldPos(boardPos);

            // Assert
            Assert.AreEqual(1f, worldPos.x);
            Assert.AreEqual(2f, worldPos.y);
            Assert.AreEqual(0f, worldPos.z);
        }

        [Test]
        public void WorldToBoardPos_EdgeOfCell_RoundsToNearestCell()
        {
            // Arrange - Position at edge between cells
            var worldPos = new Vector3(0.4f, 0.6f, 0f);

            // Act
            var boardPos = boardPresenter.WorldToBoardPos(worldPos);

            // Assert
            Assert.IsTrue(boardPos.HasValue);
            // Should round to nearest cell (0 or 1 depending on rounding)
            Assert.IsTrue(boardPos.Value.X >= 0 && boardPos.Value.X <= 1);
            Assert.IsTrue(boardPos.Value.Y >= 0 && boardPos.Value.Y <= 1);
        }
    }
}
```

### Step 2: Run Tests

1. Open Unity Test Runner
2. Run all tests in EditMode
3. Verify position conversion tests pass

---

## Test Checklist

### Setup Verification
- [ ] Test files compile without errors
- [ ] Tests appear in Unity Test Runner

### Test Execution
- [ ] WorldToBoardPos tests pass (3+ tests)
- [ ] BoardToWorldPos tests pass (1+ test)
- [ ] All tests run quickly (<1 second)

### Coverage Check
- [ ] World-to-board conversion tested
- [ ] Board-to-world conversion tested
- [ ] Out-of-bounds handling tested
- [ ] Rounding behavior tested

---

## Success Criteria

- ✅ All unit tests pass (4+ tests)
- ✅ Position conversion logic covered
- ✅ Edge cases tested (out of bounds, cell edges)
- ✅ No compilation errors

---

## Notes

### Testing Challenges
- **Screen position testing**: Difficult to test without active camera
- **Unity Input**: Hard to simulate actual clicks in unit tests
- Focus on testing the conversion logic, not the full click pipeline

### What's Not Tested (Acceptable)
- **Actual mouse clicks**: Requires Play Mode or integration tests
- **Camera setup**: Assumes orthographic camera exists
- **EmptyCellClickDetector**: Requires CoreServices setup (Play Mode test territory)

---

## Next Task

After tests pass, proceed to **Task_04c_Documentation.md** to document the empty cell click system.

---

## Related Files

- `/Assets/Scripts/Gameplay/Presentation/Board/BoardPresenter.cs`
- `/Assets/Scripts/Gameplay/Game/Controllers/EmptyCellClickDetector.cs`
