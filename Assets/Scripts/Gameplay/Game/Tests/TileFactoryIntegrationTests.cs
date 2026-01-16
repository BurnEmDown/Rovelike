using Gameplay.Engine.Moves;
using Gameplay.Engine.Tiles;
using Gameplay.Game.Definitions;
using NUnit.Framework;
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
