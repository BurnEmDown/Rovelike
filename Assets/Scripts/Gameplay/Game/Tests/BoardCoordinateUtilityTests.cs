using NUnit.Framework;
using Gameplay.Utilities;
using UnityEngine;

namespace Gameplay.Game.Tests
{
    /// <summary>
    /// Tests for <see cref="BoardCoordinateUtility"/> coordinate conversion methods.
    /// Validates world-to-board and board-to-world position transformations with various configurations.
    /// </summary>
    [TestFixture]
    public class BoardCoordinateUtilityTests
    {
        /// <summary>
        /// Tests world-to-board conversion with default settings (origin at 0,0 and 1x1 cell size).
        /// Verifies that a world position at (1,1) correctly maps to board cell (1,1).
        /// </summary>
        [Test]
        public void WorldToBoardPos_CenterOfCell_ReturnsCorrectPosition()
        {
            // Arrange
            var origin = Vector3.zero;
            var cellSize = Vector2.one;
            var worldPos = new Vector3(1f, 1f, 0f);

            // Act
            var boardPos = BoardCoordinateUtility.WorldToBoardPos(worldPos, origin, cellSize);

            // Assert
            Assert.AreEqual(1, boardPos.x);
            Assert.AreEqual(1, boardPos.y);
        }

        /// <summary>
        /// Tests world-to-board conversion with a custom origin offset.
        /// Verifies that the conversion correctly accounts for board origin not being at world (0,0).
        /// </summary>
        [Test]
        public void WorldToBoardPos_WithOffset_ReturnsCorrectPosition()
        {
            // Arrange
            var origin = new Vector3(2f, 2f, 0f);
            var cellSize = Vector2.one;
            var worldPos = new Vector3(3f, 4f, 0f);

            // Act
            var boardPos = BoardCoordinateUtility.WorldToBoardPos(worldPos, origin, cellSize);

            // Assert
            Assert.AreEqual(1, boardPos.x);
            Assert.AreEqual(2, boardPos.y);
        }

        [Test]
        public void WorldToBoardPos_WithLargerCellSize_ReturnsCorrectPosition()
        {
            // Arrange
            var origin = Vector3.zero;
            var cellSize = new Vector2(2f, 2f);
            var worldPos = new Vector3(4f, 6f, 0f);

            // Act
            var boardPos = BoardCoordinateUtility.WorldToBoardPos(worldPos, origin, cellSize);

            // Assert
            Assert.AreEqual(2, boardPos.x);
            Assert.AreEqual(3, boardPos.y);
        }

        [Test]
        public void WorldToBoardPos_EdgeOfCell_RoundsToNearestCell()
        {
            // Arrange
            var origin = Vector3.zero;
            var cellSize = Vector2.one;
            var worldPos = new Vector3(0.4f, 0.6f, 0f);

            // Act
            var boardPos = BoardCoordinateUtility.WorldToBoardPos(worldPos, origin, cellSize);

            // Assert - Mathf.RoundToInt should round 0.4 to 0 and 0.6 to 1
            Assert.AreEqual(0, boardPos.x);
            Assert.AreEqual(1, boardPos.y);
        }

        [Test]
        public void BoardToWorldPos_ReturnsCorrectWorldPosition()
        {
            // Arrange
            var origin = Vector3.zero;
            var cellSize = Vector2.one;
            var boardPos = new Vector2Int(1, 2);

            // Act
            var worldPos = BoardCoordinateUtility.BoardToWorldPos(boardPos, origin, cellSize);

            // Assert
            Assert.AreEqual(1f, worldPos.x);
            Assert.AreEqual(2f, worldPos.y);
            Assert.AreEqual(0f, worldPos.z);
        }

        [Test]
        public void BoardToWorldPos_WithOffset_ReturnsCorrectWorldPosition()
        {
            // Arrange
            var origin = new Vector3(5f, 5f, 0f);
            var cellSize = Vector2.one;
            var boardPos = new Vector2Int(2, 3);

            // Act
            var worldPos = BoardCoordinateUtility.BoardToWorldPos(boardPos, origin, cellSize);

            // Assert
            Assert.AreEqual(7f, worldPos.x);
            Assert.AreEqual(8f, worldPos.y);
            Assert.AreEqual(0f, worldPos.z);
        }

        [Test]
        public void BoardToWorldPos_WithLargerCellSize_ReturnsCorrectWorldPosition()
        {
            // Arrange
            var origin = Vector3.zero;
            var cellSize = new Vector2(2f, 2f);
            var boardPos = new Vector2Int(3, 4);

            // Act
            var worldPos = BoardCoordinateUtility.BoardToWorldPos(boardPos, origin, cellSize);

            // Assert
            Assert.AreEqual(6f, worldPos.x);
            Assert.AreEqual(8f, worldPos.y);
            Assert.AreEqual(0f, worldPos.z);
        }

        [Test]
        public void WorldToBoardPos_NegativeCoordinates_ReturnsCorrectPosition()
        {
            // Arrange
            var origin = Vector3.zero;
            var cellSize = Vector2.one;
            var worldPos = new Vector3(-1f, -2f, 0f);

            // Act
            var boardPos = BoardCoordinateUtility.WorldToBoardPos(worldPos, origin, cellSize);

            // Assert
            Assert.AreEqual(-1, boardPos.x);
            Assert.AreEqual(-2, boardPos.y);
        }

        [Test]
        public void RoundTrip_BoardToWorldToBoard_ReturnsOriginalPosition()
        {
            // Arrange
            var origin = new Vector3(1f, 1f, 0f);
            var cellSize = new Vector2(1.5f, 1.5f);
            var originalBoardPos = new Vector2Int(3, 5);

            // Act - Convert to world and back
            var worldPos = BoardCoordinateUtility.BoardToWorldPos(originalBoardPos, origin, cellSize);
            var finalBoardPos = BoardCoordinateUtility.WorldToBoardPos(worldPos, origin, cellSize);

            // Assert
            Assert.AreEqual(originalBoardPos.x, finalBoardPos.x);
            Assert.AreEqual(originalBoardPos.y, finalBoardPos.y);
        }
    }
}