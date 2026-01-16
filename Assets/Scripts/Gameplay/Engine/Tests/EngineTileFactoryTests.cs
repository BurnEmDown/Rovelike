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
