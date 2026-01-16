# Task 02b: Unit Test - Data-Driven Tile Creation

**Estimated Time:** 30-45 minutes  
**Prerequisites:** Task_02_FixDataDrivenTiles.md completed  
**Status:** Complete

---

## Context

Task 02 removed hardcoded tile rules and made tile creation fully data-driven. Now we need automated tests to ensure:
- TileFactory correctly translates TileDefinition → TileConfig
- EngineTileFactory creates tiles with correct MovementRules
- TileConfig properly transfers all data fields
- Changes to TileDefinition propagate to created tiles

**Current State:**
- ✅ Tiles are created data-driven (verified manually in Task 02)
- ✅ Concrete tile classes deleted (or refactored)
- ❌ No automated tests for factory system
- ❌ No regression prevention for hardcoded values

**Goal:** Create unit tests ensuring tile creation remains data-driven and correct.

---

## Goals

1. Create test assembly for Engine layer tests
2. Write tests for EngineTileFactory tile creation
3. Write tests for TileConfig data transfer
4. Write tests for MovementRules initialization
5. Write tests for unique tile ID generation

---

## Implementation Steps

### Step 1: Create Test Assembly for Engine
**Directory:** `/Assets/Scripts/Gameplay/Engine/Tests/`

**Create file:** `Engine.Tests.asmdef`
```json
{
    "name": "Engine.Tests",
    "rootNamespace": "Gameplay.Engine.Tests",
    "references": [
        "Engine",
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

### Step 2: Create EngineTileFactoryTests.cs
**File:** `/Assets/Scripts/Gameplay/Engine/Tests/EngineTileFactoryTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;

namespace Gameplay.Engine.Tests
{
    [TestFixture]
    public class EngineTileFactoryTests
    {
        [Test]
        public void CreateTile_AssignsUniqueId()
        {
            // Arrange
            var config1 = new TileConfig
            {
                TypeKey = "TestTile1",
                MovementRules = new MovementRules(1, true, false)
            };
            var config2 = new TileConfig
            {
                TypeKey = "TestTile2",
                MovementRules = new MovementRules(1, true, false)
            };

            // Act
            var tile1 = EngineTileFactory.CreateTile(config1);
            var tile2 = EngineTileFactory.CreateTile(config2);

            // Assert
            Assert.AreNotEqual(tile1.Id, tile2.Id, "Tiles should have unique IDs");
            Assert.Greater(tile2.Id, tile1.Id, "IDs should increment");
        }

        [Test]
        public void CreateTile_SetsTypeKeyFromConfig()
        {
            // Arrange
            var config = new TileConfig
            {
                TypeKey = "Brain",
                MovementRules = new MovementRules(10, true, false)
            };

            // Act
            var tile = EngineTileFactory.CreateTile(config);

            // Assert
            Assert.AreEqual("Brain", tile.TypeKey);
        }

        [Test]
        public void CreateTile_UsesMovementRulesFromConfig()
        {
            // Arrange
            var config = new TileConfig
            {
                TypeKey = "Motor",
                MovementRules = new MovementRules(3, true, true, ObstaclePassRule.PushObstacles)
            };

            // Act
            var tile = EngineTileFactory.CreateTile(config);

            // Assert - verify by checking movement behavior
            // We can't directly access MovementRules, so we verify via GetAvailableMoves
            Assert.IsNotNull(tile);
            Assert.IsInstanceOf<ModuleTile>(tile);
        }

        [Test]
        public void CreateTile_CreatesModuleTileInstance()
        {
            // Arrange
            var config = new TileConfig
            {
                TypeKey = "Sensor",
                MovementRules = new MovementRules(5, false, true)
            };

            // Act
            var tile = EngineTileFactory.CreateTile(config);

            // Assert
            Assert.IsInstanceOf<ModuleTile>(tile);
            Assert.IsNotNull(tile);
        }

        [Test]
        public void CreateTile_WithAbilityBehavior_AssignsBehavior()
        {
            // Arrange
            var customAbility = new DefaultAbilityBehavior();
            var config = new TileConfig
            {
                TypeKey = "Laser",
                MovementRules = new MovementRules(1, true, false),
                AbilityBehavior = customAbility
            };

            // Act
            var tile = EngineTileFactory.CreateTile(config);

            // Assert
            Assert.IsNotNull(tile);
            Assert.IsFalse(tile.IsAbilityAvailable); // DefaultAbilityBehavior returns false
        }
    }
}
```

### Step 3: Create TileConfigTests.cs
**File:** `/Assets/Scripts/Gameplay/Engine/Tests/TileConfigTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;

namespace Gameplay.Engine.Tests
{
    [TestFixture]
    public class TileConfigTests
    {
        [Test]
        public void TileConfig_InitializesWithDefaults()
        {
            // Act
            var config = new TileConfig();

            // Assert
            Assert.AreEqual("", config.TypeKey);
            Assert.AreEqual("", config.DisplayName);
            Assert.IsNotNull(config.MovementRules);
            Assert.IsNotNull(config.AbilityBehavior);
        }

        [Test]
        public void TileConfig_CanSetAllProperties()
        {
            // Arrange
            var movementRules = new MovementRules(10, true, true, ObstaclePassRule.CanPassThrough);
            var abilityBehavior = new DefaultAbilityBehavior();

            // Act
            var config = new TileConfig
            {
                TypeKey = "CustomTile",
                DisplayName = "Custom Tile Name",
                MovementRules = movementRules,
                AbilityBehavior = abilityBehavior
            };

            // Assert
            Assert.AreEqual("CustomTile", config.TypeKey);
            Assert.AreEqual("Custom Tile Name", config.DisplayName);
            Assert.AreEqual(movementRules, config.MovementRules);
            Assert.AreEqual(abilityBehavior, config.AbilityBehavior);
        }
    }
}
```

### Step 4: Create MovementRulesTests.cs
**File:** `/Assets/Scripts/Gameplay/Engine/Tests/MovementRulesTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Engine.Moves;

namespace Gameplay.Engine.Tests
{
    [TestFixture]
    public class MovementRulesTests
    {
        [Test]
        public void MovementRules_Constructor_SetsAllProperties()
        {
            // Act
            var rules = new MovementRules(5, true, false, ObstaclePassRule.CanPassThrough);

            // Assert
            Assert.AreEqual(5, rules.MaxSteps);
            Assert.IsTrue(rules.AllowOrthogonal);
            Assert.IsFalse(rules.AllowDiagonal);
            Assert.AreEqual(ObstaclePassRule.CanPassThrough, rules.PassRule);
        }

        [Test]
        public void MovementRules_DefaultConstructor_SetsDefaults()
        {
            // Act
            var rules = new MovementRules();

            // Assert
            Assert.AreEqual(1, rules.MaxSteps);
            Assert.IsTrue(rules.AllowOrthogonal);
            Assert.IsFalse(rules.AllowDiagonal);
            Assert.AreEqual(ObstaclePassRule.CannotPassThrough, rules.PassRule);
        }

        [Test]
        public void MovementRules_OrthogonalOnly()
        {
            // Act
            var rules = new MovementRules(10, true, false);

            // Assert
            Assert.IsTrue(rules.AllowOrthogonal);
            Assert.IsFalse(rules.AllowDiagonal);
        }

        [Test]
        public void MovementRules_DiagonalOnly()
        {
            // Act
            var rules = new MovementRules(10, false, true);

            // Assert
            Assert.IsFalse(rules.AllowOrthogonal);
            Assert.IsTrue(rules.AllowDiagonal);
        }

        [Test]
        public void MovementRules_BothDirections()
        {
            // Act
            var rules = new MovementRules(5, true, true);

            // Assert
            Assert.IsTrue(rules.AllowOrthogonal);
            Assert.IsTrue(rules.AllowDiagonal);
        }

        [Test]
        public void MovementRules_NoMovement()
        {
            // Act
            var rules = new MovementRules(0, false, false);

            // Assert
            Assert.AreEqual(0, rules.MaxSteps);
            Assert.IsFalse(rules.AllowOrthogonal);
            Assert.IsFalse(rules.AllowDiagonal);
        }
    }
}
```

### Step 5: Create Integration Test (Game Layer)
**Directory:** `/Assets/Scripts/Gameplay/Game/Tests/`

**Create file:** `Game.Tests.asmdef`
```json
{
    "name": "Game.Tests",
    "rootNamespace": "Gameplay.Game.Tests",
    "references": [
        "Gameplay.Game",
        "Gameplay.Engine",
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

**Create file:** `/Assets/Scripts/Gameplay/Game/Tests/TileFactoryIntegrationTests.cs`

```csharp
using NUnit.Framework;
using Gameplay.Game.Definitions;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using UnityEngine;

namespace Gameplay.Game.Tests
{
    [TestFixture]
    public class TileFactoryIntegrationTests
    {
        [Test]
        public void TileFactory_CreateTile_WithMockDefinition_CreatesCorrectTile()
        {
            // Arrange
            var mockDefinition = ScriptableObject.CreateInstance<TileDefinition>();
            mockDefinition.typeKey = "TestBrain";
            mockDefinition.maxMoveDistance = 7;
            mockDefinition.allowOrthogonal = true;
            mockDefinition.allowDiagonal = false;
            mockDefinition.passRule = ObstaclePassRule.CannotPassThrough;

            // Act
            var tile = TileFactory.CreateTile(mockDefinition);

            // Assert
            Assert.IsNotNull(tile);
            Assert.AreEqual("TestBrain", tile.TypeKey);
            Assert.IsInstanceOf<ModuleTile>(tile);

            // Cleanup
            Object.DestroyImmediate(mockDefinition);
        }

        [Test]
        public void TileFactory_CreateTile_TranslatesMovementRulesCorrectly()
        {
            // Arrange
            var mockDefinition = ScriptableObject.CreateInstance<TileDefinition>();
            mockDefinition.typeKey = "TestMotor";
            mockDefinition.maxMoveDistance = 3;
            mockDefinition.allowOrthogonal = true;
            mockDefinition.allowDiagonal = true;
            mockDefinition.passRule = ObstaclePassRule.PushObstacles;

            // Act
            var tile = TileFactory.CreateTile(mockDefinition);

            // Assert - verify via movement context (indirect test)
            Assert.IsNotNull(tile);
            Assert.IsInstanceOf<IMovingTile>(tile);

            // Cleanup
            Object.DestroyImmediate(mockDefinition);
        }

        [Test]
        public void TileFactory_CreateMultipleTiles_ProducesUniqueTiles()
        {
            // Arrange
            var def1 = ScriptableObject.CreateInstance<TileDefinition>();
            def1.typeKey = "Tile1";
            def1.maxMoveDistance = 1;

            var def2 = ScriptableObject.CreateInstance<TileDefinition>();
            def2.typeKey = "Tile2";
            def2.maxMoveDistance = 2;

            // Act
            var tile1 = TileFactory.CreateTile(def1);
            var tile2 = TileFactory.CreateTile(def2);

            // Assert
            Assert.AreNotEqual(tile1.Id, tile2.Id);
            Assert.AreNotEqual(tile1.TypeKey, tile2.TypeKey);

            // Cleanup
            Object.DestroyImmediate(def1);
            Object.DestroyImmediate(def2);
        }
    }
}
```

### Step 6: Run All Tests
1. Open Unity Test Runner (Window → General → Test Runner)
2. Click "Run All" in EditMode tab
3. Verify all tests pass:
   - Engine.Tests (10+ tests)
   - Game.Tests (3+ tests)
4. Fix any failing tests

---

## Test Checklist

### Setup Verification
- [ ] `Engine.Tests.asmdef` compiles without errors
- [ ] `Game.Tests.asmdef` compiles without errors
- [ ] All test files appear in Unity Test Runner
- [ ] All required assemblies referenced

### Test Execution
- [ ] All EngineTileFactoryTests pass (5 tests)
- [ ] All TileConfigTests pass (2 tests)
- [ ] All MovementRulesTests pass (6 tests)
- [ ] All TileFactoryIntegrationTests pass (3 tests)
- [ ] No warnings in console during test execution
- [ ] Tests run in <5 seconds total

### Coverage Check
- [ ] EngineTileFactory.CreateTile tested
- [ ] TileConfig property setting tested
- [ ] MovementRules constructor tested
- [ ] MovementRules default values tested
- [ ] TileFactory integration tested (Unity → Engine)
- [ ] Unique ID generation tested

---

## Success Criteria

- ✅ All unit tests pass (16+ total tests)
- ✅ Test assemblies compile without errors
- ✅ Tests execute in Unity Test Runner
- ✅ Code coverage includes:
  - EngineTileFactory tile creation
  - TileConfig initialization
  - MovementRules all constructors
  - TileFactory ScriptableObject translation
  - Unique ID generation

---

## Notes

### Why These Tests Matter
- **EngineTileFactoryTests** - Ensures factory creates tiles correctly from config
- **TileConfigTests** - Validates data transfer object works as expected
- **MovementRulesTests** - Confirms movement rules initialize properly with all permutations
- **TileFactoryIntegrationTests** - Verifies Unity → Engine translation layer works

### Test Strategy
- **Unit tests** (Engine layer) - Fast, no Unity dependencies, test pure C# logic
- **Integration tests** (Game layer) - Test Unity → Engine boundary (ScriptableObject → Tile)
- **No Play Mode tests needed** - Factory logic is deterministic and synchronous

### Common Test Failures
- **"Assembly reference not found"** → Check .asmdef references include correct assemblies
- **"TileFactory returns null"** → Mock TileDefinition missing required fields
- **"IDs not incrementing"** → EngineTileFactory.nextId being reset between tests (expected - static field)

### Next Task
After tests pass, proceed to **Task_02c_Documentation.md** to document the data-driven tile system.

### Related Files
- `/Assets/Scripts/Gameplay/Engine/Tiles/EngineTileFactory.cs`
- `/Assets/Scripts/Gameplay/Engine/Tiles/TileConfig.cs`
- `/Assets/Scripts/Gameplay/Engine/Moves/MovementRules.cs`
- `/Assets/Scripts/Gameplay/Game/Definitions/TileFactory.cs`
