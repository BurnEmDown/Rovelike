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
