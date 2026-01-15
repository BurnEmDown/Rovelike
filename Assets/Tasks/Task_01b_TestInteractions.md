# Task 01b: Unit Test - Interaction System

**Estimated Time:** 30-45 minutes  
**Prerequisites:** Task_01_WireUpInteractions.md completed  
**Status:** Not Started

---

## Context

Task 01 implemented the user interaction system for tile clicks. Now we need to verify this system works correctly through automated tests, ensuring:
- Events are emitted when PointerClickUserInteractionSource is triggered
- TileView correctly implements IUserInteractionTarget
- Subscriptions and unsubscriptions work properly
- No memory leaks from event listeners

**Current State:**
- ✅ Interaction system works in Play Mode (verified manually in Task 01)
- ❌ No automated tests exist
- ❌ No test assembly defined

**Goal:** Create unit tests for the interaction system to ensure reliability and catch regressions.

---

## Goals

1. Create test assembly definition for Presentation layer tests
2. Write tests for PointerClickUserInteractionSource initialization
3. Write tests for UserInteractionEvent emission
4. Write tests for TileView as IUserInteractionTarget
5. Write tests for subscription cleanup

---

## Implementation Steps

### Step 1: Create Test Assembly
**Directory:** `/Assets/Scripts/Gameplay/Presentation/Tests/`

**Create file:** `Presentation.Tests.asmdef`
```json
{
    "name": "Presentation.Tests",
    "rootNamespace": "Gameplay.Presentation.Tests",
    "references": [
        "Gameplay.Presentation",
        "Gameplay.Engine",
        "Gameplay.Game",
        "UnityCoreKit.Runtime",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ]
}
```

### Step 2: Create TileViewTests.cs
**File:** `/Assets/Scripts/Gameplay/Presentation/Tests/TileViewTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Presentation.Tiles;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using UnityCoreKit.Runtime.UserInteractions;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Presentation.Tests
{
    [TestFixture]
    public class TileViewTests
    {
        private GameObject tileViewGameObject;
        private TileView tileView;
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

            // Create TileView GameObject
            tileViewGameObject = new GameObject("TestTileView");
            tileView = tileViewGameObject.AddComponent<TileView>();
            tileViewGameObject.AddComponent<SpriteRenderer>(); // Required by TileView
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(tileViewGameObject);
        }

        [Test]
        public void Init_SetsTileReference()
        {
            // Arrange
            var pos = new CellPos { X = 1, Y = 2 };

            // Act
            tileView.Init(testTile, pos);

            // Assert
            Assert.AreEqual(testTile, tileView.Tile);
            Assert.AreEqual(pos, tileView.BoardPosition);
        }

        [Test]
        public void Init_SetsGameObjectName()
        {
            // Arrange
            var pos = new CellPos { X = 0, Y = 0 };

            // Act
            tileView.Init(testTile, pos);

            // Assert
            Assert.AreEqual("TileView_TestTile_1", tileViewGameObject.name);
        }

        [Test]
        public void IUserInteractionTarget_InteractionKey_ReturnsCorrectKey()
        {
            // Arrange
            var pos = new CellPos { X = 0, Y = 0 };
            tileView.Init(testTile, pos);

            // Act
            IUserInteractionTarget target = tileView;

            // Assert
            Assert.AreEqual("Tile", target.InteractionKey);
        }

        [Test]
        public void IUserInteractionTarget_Model_ReturnsTile()
        {
            // Arrange
            var pos = new CellPos { X = 0, Y = 0 };
            tileView.Init(testTile, pos);

            // Act
            IUserInteractionTarget target = tileView;

            // Assert
            Assert.AreEqual(testTile, target.Model);
        }

        [Test]
        public void SetBoardPosition_UpdatesPosition()
        {
            // Arrange
            var initialPos = new CellPos { X = 0, Y = 0 };
            var newPos = new CellPos { X = 2, Y = 3 };
            tileView.Init(testTile, initialPos);

            // Act
            tileView.SetBoardPosition(newPos);

            // Assert
            Assert.AreEqual(newPos, tileView.BoardPosition);
        }
    }
}
```

### Step 3: Create UserInteractionTests.cs
**File:** `/Assets/Scripts/Gameplay/Presentation/Tests/UserInteractionTests.cs`

```csharp
using NUnit.Framework;
using UnityCoreKit.Runtime.UserInteractions;
using UnityCoreKit.Runtime.UserInteractions.Unity;
using UnityCoreKit.Runtime.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay.Presentation.Tests
{
    [TestFixture]
    public class UserInteractionTests
    {
        private GameObject sourceGameObject;
        private PointerClickUserInteractionSource interactionSource;
        private MockUserInteractions mockInteractions;
        private MockInteractionTarget mockTarget;

        [SetUp]
        public void Setup()
        {
            // Create mock services
            mockInteractions = new MockUserInteractions();
            mockTarget = new MockInteractionTarget();

            // Create interaction source
            sourceGameObject = new GameObject("InteractionSource");
            interactionSource = sourceGameObject.AddComponent<PointerClickUserInteractionSource>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(sourceGameObject);
        }

        [Test]
        public void Init_StoresReferences()
        {
            // Act
            interactionSource.Init(mockInteractions, mockTarget);

            // Assert - verify by triggering interaction
            var pointerData = new PointerEventData(EventSystem.current);
            interactionSource.OnPointerClick(pointerData);

            Assert.AreEqual(1, mockInteractions.EmittedEvents.Count);
        }

        [Test]
        public void OnPointerClick_EmitsUserInteractionEvent()
        {
            // Arrange
            interactionSource.Init(mockInteractions, mockTarget);
            var pointerData = new PointerEventData(EventSystem.current);

            // Act
            interactionSource.OnPointerClick(pointerData);

            // Assert
            Assert.AreEqual(1, mockInteractions.EmittedEvents.Count);
            var evt = mockInteractions.EmittedEvents[0];
            Assert.AreEqual(UserInteractionType.Click, evt.Type);
            Assert.AreEqual(mockTarget, evt.Target);
        }

        [Test]
        public void OnPointerClick_WithoutInit_DoesNotCrash()
        {
            // Arrange - no Init call
            var pointerData = new PointerEventData(EventSystem.current);

            // Act & Assert - should not throw
            Assert.DoesNotThrow(() => interactionSource.OnPointerClick(pointerData));
        }

        // Mock implementations for testing
        private class MockUserInteractions : IUserInteractions
        {
            public List<UserInteractionEvent> EmittedEvents = new List<UserInteractionEvent>();

            public void Emit(UserInteractionEvent evt)
            {
                EmittedEvents.Add(evt);
            }

            public void Subscribe(UserInteractionType type, System.Action<UserInteractionEvent> handler, object owner)
            {
                // Not needed for these tests
            }

            public void Unsubscribe(UserInteractionType type, System.Action<UserInteractionEvent> handler, object owner)
            {
                // Not needed for these tests
            }

            public void UnsubscribeAll(object owner)
            {
                // Not needed for these tests
            }
        }

        private class MockInteractionTarget : IUserInteractionTarget
        {
            public string InteractionKey => "Mock";
            public object Model => this;
        }
    }
}
```

### Step 4: Create EventsManagerTests.cs (Subscription Cleanup)
**File:** `/Assets/Scripts/Gameplay/Presentation/Tests/EventsManagerTests.cs`

```csharp
using NUnit.Framework;
using UnityCoreKit.Runtime.Core.Services;
using UnityCoreKit.Runtime.UserInteractions;

namespace Gameplay.Presentation.Tests
{
    [TestFixture]
    public class EventsManagerSubscriptionTests
    {
        private EventsManager eventsManager;
        private MockUserInteractions interactions;

        [SetUp]
        public void Setup()
        {
            eventsManager = new EventsManager();
            interactions = new MockUserInteractions(eventsManager);
        }

        [Test]
        public void Subscribe_ReceivesEvent()
        {
            // Arrange
            int callCount = 0;
            var owner = new object();
            interactions.Subscribe(UserInteractionType.Click, evt => callCount++, owner);

            // Act
            interactions.Emit(new UserInteractionEvent(UserInteractionType.Click, null));

            // Assert
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void UnsubscribeAll_RemovesAllSubscriptionsForOwner()
        {
            // Arrange
            int callCount = 0;
            var owner = new object();
            interactions.Subscribe(UserInteractionType.Click, evt => callCount++, owner);
            interactions.Subscribe(UserInteractionType.Click, evt => callCount++, owner);

            // Act
            interactions.UnsubscribeAll(owner);
            interactions.Emit(new UserInteractionEvent(UserInteractionType.Click, null));

            // Assert
            Assert.AreEqual(0, callCount, "Handlers should not be called after UnsubscribeAll");
        }

        [Test]
        public void UnsubscribeAll_DoesNotAffectOtherOwners()
        {
            // Arrange
            int owner1Count = 0;
            int owner2Count = 0;
            var owner1 = new object();
            var owner2 = new object();

            interactions.Subscribe(UserInteractionType.Click, evt => owner1Count++, owner1);
            interactions.Subscribe(UserInteractionType.Click, evt => owner2Count++, owner2);

            // Act
            interactions.UnsubscribeAll(owner1);
            interactions.Emit(new UserInteractionEvent(UserInteractionType.Click, null));

            // Assert
            Assert.AreEqual(0, owner1Count, "Owner1 handlers should be removed");
            Assert.AreEqual(1, owner2Count, "Owner2 handlers should still work");
        }

        // Mock implementation wrapping real EventsManager
        private class MockUserInteractions : IUserInteractions
        {
            private readonly EventsManager eventsManager;

            public MockUserInteractions(EventsManager eventsManager)
            {
                this.eventsManager = eventsManager;
            }

            public void Emit(UserInteractionEvent evt)
            {
                eventsManager.Emit("UserInteraction_" + evt.Type, evt);
            }

            public void Subscribe(UserInteractionType type, System.Action<UserInteractionEvent> handler, object owner)
            {
                eventsManager.Subscribe("UserInteraction_" + type, handler, owner);
            }

            public void Unsubscribe(UserInteractionType type, System.Action<UserInteractionEvent> handler, object owner)
            {
                eventsManager.Unsubscribe("UserInteraction_" + type, handler, owner);
            }

            public void UnsubscribeAll(object owner)
            {
                eventsManager.UnsubscribeAll(owner);
            }
        }
    }
}
```

### Step 5: Run Tests in Unity
1. Open Unity Test Runner (Window → General → Test Runner)
2. Click "Run All" in EditMode tab
3. Verify all tests pass
4. Fix any failing tests

---

## Test Checklist

### Setup Verification
- [ ] `Presentation.Tests.asmdef` compiles without errors
- [ ] Test files appear in Unity Test Runner
- [ ] All required assemblies are referenced correctly

### Test Execution
- [ ] All TileViewTests pass (5 tests)
- [ ] All UserInteractionTests pass (3 tests)
- [ ] All EventsManagerSubscriptionTests pass (3 tests)
- [ ] No warnings in console during test execution
- [ ] Tests run in <5 seconds total

### Coverage Check
- [ ] TileView initialization tested
- [ ] IUserInteractionTarget interface tested
- [ ] PointerClickUserInteractionSource emission tested
- [ ] Event subscription tested
- [ ] Event cleanup tested
- [ ] Owner-based unsubscription tested

---

## Success Criteria

- ✅ All unit tests pass (11 total tests minimum)
- ✅ Test assembly compiles without errors
- ✅ Tests execute in Unity Test Runner
- ✅ Code coverage includes:
  - TileView.Init()
  - TileView.SetBoardPosition()
  - TileView as IUserInteractionTarget
  - PointerClickUserInteractionSource.OnPointerClick()
  - EventsManager subscription lifecycle

---

## Notes

### Why These Tests Matter
- **TileViewTests** - Ensures view correctly holds engine state without mutation
- **UserInteractionTests** - Verifies input plumbing works independently of Unity's EventSystem
- **EventsManagerTests** - Prevents memory leaks from dangling event subscriptions

### Test Limitations
- These are **unit tests** (fast, isolated)
- **Not covered:** Integration with Unity's EventSystem (requires Play Mode tests)
- **Not covered:** Actual mouse/touch input (requires manual testing)
- **Not covered:** Pool manager interaction with TileView

### Common Test Failures
- **"Reference not set"** → Check SetUp creates all required objects
- **"Assembly not found"** → Verify .asmdef references are correct
- **"Test times out"** → Remove any `yield return` or async code (use Edit Mode tests only)

### Next Task
After tests pass, proceed to **Task_01c_Documentation.md** to document the interaction system.

### Related Files
- `/Assets/Scripts/Gameplay/Presentation/Tiles/TileView.cs`
- `/Assets/UnityCoreKit/Runtime/UserInteractions/Unity/PointerClickUserInteractionSource.cs`
- `/Assets/UnityCoreKit/Runtime/Core/Services/EventsManager.cs`
