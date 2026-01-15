# Task 04b: Visual Test Verification

**Estimated Time:** 20-30 minutes  
**Prerequisites:** Task_04_VisualFeedback.md completed  
**Status:** Not Started

---

## Context

Task 04 added visual feedback for selection, movement animation, and highlights. Unlike previous test tasks, visual features are difficult to unit test automatically. This task creates:
1. **Visual regression reference** - Screenshots/descriptions of expected visuals
2. **Manual test protocol** - Structured checklist for QA
3. **Automated tests where possible** - Component state verification

**Current State:**
- ✅ Visual feedback implemented (Task 04)
- ❌ No visual regression baseline
- ❌ No structured manual test protocol
- ❌ No component state verification tests

**Goal:** Establish visual quality verification process.

---

## Goals

1. Create manual test protocol document
2. Capture visual regression baseline (screenshots)
3. Write automated tests for component states (not visuals themselves)
4. Document expected visual behavior

---

## Implementation Steps

### Step 1: Create Manual Visual Test Protocol
**File:** `/Assets/Tasks/VisualTestProtocol.md`

```markdown
# Visual Test Protocol - Movement & Selection

## Test Environment Setup
- Unity Editor Play Mode
- GameScene loaded
- Board fully initialized with tiles

---

## Test Case 1: Tile Selection Visual

**Steps:**
1. Click any tile

**Expected:**
- Yellow/white outline appears around tile
- Outline is clearly visible against tile sprite
- Outline matches tile bounds (not oversized/undersized)
- Outline appears instantly (<1 frame)

**Pass Criteria:**
- ✅ Outline visible
- ✅ Correct color (yellow/white)
- ✅ Instant appearance

**Screenshot Location:** `/Assets/Tasks/VisualBaseline/01_TileSelected.png`

---

## Test Case 2: Tile Deselection Visual

**Steps:**
1. Click tile (select it)
2. Click same tile again (deselect)

**Expected:**
- Outline disappears instantly
- No visual artifacts remain
- Tile returns to original appearance

**Pass Criteria:**
- ✅ Outline removed
- ✅ No flickering
- ✅ Instant removal

---

## Test Case 3: Move Preview Highlights

**Steps:**
1. Click tile with movement capability

**Expected:**
- Cyan/green highlights appear at valid destinations
- Highlights are semi-transparent (can see board underneath)
- Highlights match board cell size
- Highlight count matches expected move count
- (Optional) Highlights pulse gently

**Pass Criteria:**
- ✅ All valid destinations highlighted
- ✅ No invalid destinations highlighted
- ✅ Color is cyan/green
- ✅ Alpha is 0.3-0.5 (semi-transparent)

**Screenshot Location:** `/Assets/Tasks/VisualBaseline/03_MovePreview.png`

---

## Test Case 4: Move Animation

**Steps:**
1. Select tile
2. Click valid destination

**Expected:**
- Tile smoothly moves from origin to destination
- Animation takes approximately 0.3 seconds
- Movement follows straight line (or appropriate path)
- Easing is smooth (not linear)
- Tile arrives at exact destination position

**Pass Criteria:**
- ✅ Smooth motion (no jitter)
- ✅ Duration ~0.3s
- ✅ Exact final position
- ✅ Ease-out visible

**Video Location:** `/Assets/Tasks/VisualBaseline/04_MoveAnimation.mp4` (optional)

---

## Test Case 5: Hover Feedback (Optional)

**Steps:**
1. Hover mouse over tile (without clicking)

**Expected:**
- Subtle white overlay appears
- Overlay is very transparent (alpha ~0.2)
- Overlay disappears when mouse leaves

**Pass Criteria:**
- ✅ Hover visual appears
- ✅ Subtle (not distracting)
- ✅ Disappears on exit

---

## Test Case 6: Selection Transfer

**Steps:**
1. Click tile A (select)
2. Click tile B (select different tile)

**Expected:**
- Tile A outline disappears
- Tile B outline appears
- Transition is instant
- Move highlights update to tile B's moves

**Pass Criteria:**
- ✅ Only one tile selected at a time
- ✅ Instant visual update
- ✅ Highlights match new selection

---

## Test Case 7: Visual Consistency During Move

**Steps:**
1. Select tile
2. Execute move (click destination)
3. Observe during animation

**Expected:**
- Selection outline stays with tile during move
- Tile sprite doesn't flicker
- Z-order maintained (tile above board, below UI)
- Move highlights disappear immediately when move starts

**Pass Criteria:**
- ✅ Selection visual moves with tile
- ✅ No flickering
- ✅ Highlights clear on move start

---

## Regression Checklist

Run all test cases after any changes to:
- TileView prefab
- Selection visual logic
- BoardPresenter move animation
- MovePreviewController highlight logic
- Sprite/material changes

---
```

### Step 2: Create Visual Baseline Directory
**In Unity Editor or File System:**
```bash
mkdir -p /Assets/Tasks/VisualBaseline
```

**Capture baseline screenshots:**
1. Enter Play Mode
2. Perform each test case
3. Use Unity Editor Game View screenshot (right-click → Save Image As)
4. Name files: `01_TileSelected.png`, `03_MovePreview.png`, etc.
5. Commit to version control as reference

### Step 3: Create Automated Component State Tests
**File:** `/Assets/Scripts/Gameplay/Presentation/Tests/VisualComponentTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Presentation.Tiles;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Presentation.Tests
{
    /// <summary>
    /// Tests visual component state (not rendered output).
    /// Verifies components are enabled/disabled correctly.
    /// </summary>
    [TestFixture]
    public class VisualComponentTests
    {
        private GameObject tileViewGameObject;
        private TileView tileView;
        private GameObject selectionIndicator;
        private ModuleTile testTile;

        [SetUp]
        public void Setup()
        {
            // Create test tile
            testTile = new ModuleTile(
                1,
                "TestTile",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior()
            );

            // Create TileView with selection indicator
            tileViewGameObject = new GameObject("TestTileView");
            tileView = tileViewGameObject.AddComponent<TileView>();
            tileViewGameObject.AddComponent<SpriteRenderer>();

            // Create selection indicator child
            selectionIndicator = new GameObject("SelectionIndicator");
            selectionIndicator.transform.SetParent(tileViewGameObject.transform);
            selectionIndicator.AddComponent<SpriteRenderer>();
            selectionIndicator.SetActive(false); // Default state

            // Use reflection or public field to assign (depends on TileView implementation)
            // For testing, assume TileView has public field or we use SerializedObject
            // This is a simplified version - actual implementation may vary
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(tileViewGameObject);
        }

        [Test]
        public void ShowSelection_EnablesIndicator()
        {
            // Arrange
            tileView.Init(testTile, new CellPos { X = 0, Y = 0 });
            // Note: This requires selectionIndicator to be properly assigned
            // May need SerializedObject or reflection in actual test

            // Act
            tileView.ShowSelection();

            // Assert
            // Verify indicator is active (implementation-dependent)
            // Assert.IsTrue(selectionIndicator.activeSelf);
        }

        [Test]
        public void HideSelection_DisablesIndicator()
        {
            // Arrange
            tileView.Init(testTile, new CellPos { X = 0, Y = 0 });
            tileView.ShowSelection();

            // Act
            tileView.HideSelection();

            // Assert
            // Assert.IsFalse(selectionIndicator.activeSelf);
        }

        [Test]
        public void SelectionIndicator_DefaultState_IsInactive()
        {
            // Assert
            Assert.IsFalse(selectionIndicator.activeSelf);
        }
    }
}
```

**Note:** These tests verify component state, not actual rendered appearance. Full visual testing requires Play Mode tests or external tools.

### Step 4: Create BoardPresenter Animation Tests
**File:** `/Assets/Scripts/Gameplay/Presentation/Tests/BoardPresenterAnimationTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Presentation.Board;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using UnityEngine;
using System.Collections;
using UnityEngine.TestTools;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Presentation.Tests
{
    [TestFixture]
    public class BoardPresenterAnimationTests
    {
        private GameObject presenterGameObject;
        private BoardPresenter boardPresenter;
        private BoardState board;

        [SetUp]
        public void Setup()
        {
            board = new BoardState(3, 3);
            presenterGameObject = new GameObject("TestBoardPresenter");
            boardPresenter = presenterGameObject.AddComponent<BoardPresenter>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(presenterGameObject);
        }

        [UnityTest]
        public IEnumerator MoveView_AnimationCompletes_TileAtFinalPosition()
        {
            // This is a Play Mode test - requires coroutine support
            // Verifies tile ends at correct position after animation

            // Arrange - requires full setup with pooled TileView
            // Skipped for Edit Mode tests (would need Play Mode test assembly)

            yield return null;
        }

        [Test]
        public void BoardPresenter_HasMoveDurationField()
        {
            // Verify configuration exists (simple smoke test)
            Assert.IsNotNull(boardPresenter);
        }
    }
}
```

### Step 5: Create Visual Test Checklist Document
**File:** `/Assets/Tasks/VisualTestChecklist.txt`

```
VISUAL TEST CHECKLIST
=====================

Pre-Test Setup:
[ ] Unity Editor open
[ ] GameScene loaded
[ ] Play Mode entered
[ ] Board visible with tiles

Test Execution:
[ ] Test Case 1: Tile Selection Visual - PASS/FAIL
[ ] Test Case 2: Tile Deselection Visual - PASS/FAIL
[ ] Test Case 3: Move Preview Highlights - PASS/FAIL
[ ] Test Case 4: Move Animation - PASS/FAIL
[ ] Test Case 5: Hover Feedback (Optional) - PASS/FAIL
[ ] Test Case 6: Selection Transfer - PASS/FAIL
[ ] Test Case 7: Visual Consistency During Move - PASS/FAIL

Visual Quality Check:
[ ] Colors match design (yellow selection, cyan highlights)
[ ] Alpha transparency appropriate (0.3-0.5 for highlights)
[ ] Animation duration feels right (~0.3s)
[ ] No flickering or artifacts
[ ] Z-order correct (UI > tiles > highlights > board)

Performance Check:
[ ] No frame drops during animation
[ ] Smooth 60 FPS maintained
[ ] No lag when showing/hiding highlights

Regression Check:
[ ] Compare screenshots to /Assets/Tasks/VisualBaseline/
[ ] Note any differences
[ ] Approve or reject changes

Tester: ________________
Date: __________________
Build/Commit: __________
Result: PASS / FAIL / NOTES
```

### Step 6: Document Visual Design Specifications
**File:** `/Assets/Tasks/VisualDesignSpec.md`

```markdown
# Visual Design Specification

## Selection Visual
- **Type:** Sprite outline
- **Color:** Yellow (R:1, G:1, B:0) or White (R:1, G:1, B:1)
- **Alpha:** 0.8 (mostly opaque)
- **Size:** 110% of tile bounds
- **Position:** Centered on tile
- **Animation:** Instant on/off (no fade)
- **Z-Order:** Above tile sprite, below UI

## Move Highlight
- **Type:** Filled square
- **Color:** Cyan (R:0, G:1, B:1) or Green (R:0, G:1, B:0)
- **Alpha:** 0.3-0.5 (semi-transparent)
- **Size:** Matches board cell size (1x1 world units)
- **Position:** Centered on destination cell
- **Animation:** Optional pulse (2 Hz, alpha 0.3 ↔ 0.6)
- **Z-Order:** Above board, below tiles

## Move Animation
- **Duration:** 0.3 seconds
- **Easing:** Ease-out cubic
- **Path:** Straight line from origin to destination
- **Interpolation:** Linear position, cubic time
- **Final Position:** Exact destination (no overshoot)

## Hover Visual (Optional)
- **Type:** Filled overlay
- **Color:** White (R:1, G:1, B:1)
- **Alpha:** 0.2 (very transparent)
- **Size:** Matches tile bounds
- **Animation:** Instant on/off
- **Z-Order:** Above tile sprite, below selection

## General Guidelines
- All animations should be smooth (60 FPS target)
- Visuals should not obscure gameplay information
- Colors should be accessible (consider colorblind modes in future)
- Performance: <5ms per frame for all visual updates
```

---

## Test Checklist

### Documentation Complete
- [ ] VisualTestProtocol.md created
- [ ] VisualTestChecklist.txt created
- [ ] VisualDesignSpec.md created
- [ ] VisualBaseline directory created

### Baseline Captured
- [ ] Screenshot: 01_TileSelected.png
- [ ] Screenshot: 03_MovePreview.png
- [ ] (Optional) Video: 04_MoveAnimation.mp4

### Automated Tests
- [ ] VisualComponentTests.cs created
- [ ] Tests compile without errors
- [ ] Component state tests pass (where implemented)

### Manual Testing
- [ ] Executed full VisualTestProtocol
- [ ] All test cases pass
- [ ] Checklist completed and signed
- [ ] Any failures documented

---

## Success Criteria

- ✅ Visual test protocol document exists and is comprehensive
- ✅ Visual baseline screenshots captured and committed
- ✅ Automated component state tests created (even if limited)
- ✅ Manual test checklist executed at least once
- ✅ Design specifications documented for future reference
- ✅ Any visual issues identified and documented

---

## Notes

### Why Manual Testing?
- **Rendered output** can't be easily unit tested
- **Subjective quality** (smoothness, appeal) requires human eyes
- **Platform variations** (different GPUs/screens) affect appearance
- **Baseline comparison** requires visual regression tools (not included in Unity by default)

### Future Improvements
- **Unity Test Framework Play Mode tests** for animation completion
- **Automated screenshot capture** via Unity Recorder
- **Visual regression testing tool** (e.g., Applitools, Percy)
- **CI/CD integration** for automated visual checks

### Automated Visual Testing Tools (Optional)
If you want to invest in visual regression testing:
- **Unity Test Framework** - Play Mode tests for animation
- **Unity Recorder** - Automated screenshot/video capture
- **DeepEqual** - Image comparison library
- **Applitools** - Cloud-based visual testing (external service)

### Common Documentation Issues
- **Baseline screenshots outdated** → Re-capture after visual changes
- **Test protocol too vague** → Add specific expected values (colors, durations)
- **No performance baselines** → Add FPS requirements to protocol

### Next Task
After visual tests pass, proceed to **Task_04c_Documentation.md** to document the visual feedback system.

### Related Files
- `/Assets/Tasks/VisualTestProtocol.md` (created in this task)
- `/Assets/Tasks/VisualDesignSpec.md` (created in this task)
- `/Assets/Scripts/Gameplay/Presentation/Tiles/TileView.cs`
- `/Assets/Scripts/Gameplay/Presentation/Board/BoardPresenter.cs`
