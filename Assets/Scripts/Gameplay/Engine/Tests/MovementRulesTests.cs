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
