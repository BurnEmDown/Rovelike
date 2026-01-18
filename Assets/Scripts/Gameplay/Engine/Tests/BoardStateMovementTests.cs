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
        public void MoveTile_ToSamePosition_KeepsTile()
        {
            // Arrange
            var pos = new CellPos { X = 0, Y = 0 };
            board.TryPlaceTile(pos, testTile);

            // Act
            board.MoveTile(pos, pos);

            // Assert
            Assert.AreEqual(testTile, board.GetTileAt(pos));
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
        public void MoveTile_EmptyOrigin_ResultsInEmptyDestination()
        {
            // Arrange
            var from = new CellPos { X = 0, Y = 0 };
            var to = new CellPos { X = 1, Y = 1 };
            // No tile at 'from'

            // Act
            board.MoveTile(from, to);

            // Assert
            Assert.IsNull(board.GetTileAt(from));
            Assert.IsNull(board.GetTileAt(to));
        }
    }
}
