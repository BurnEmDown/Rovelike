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
