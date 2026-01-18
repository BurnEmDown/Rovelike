using NUnit.Framework;
using Gameplay.Game.Controllers;
using Gameplay.Presentation.Tiles;
using Gameplay.Engine.Tiles;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Abilities;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Tests
{
    [TestFixture]
    public class TileSelectionControllerTests
    {
        private GameObject controllerGameObject;
        private TileSelectionController selectionController;
        private GameObject tileView1GameObject;
        private TileView tileView1;
        private GameObject tileView2GameObject;
        private TileView tileView2;

        [SetUp]
        public void Setup()
        {
            // Create controller
            controllerGameObject = new GameObject("SelectionController");
            selectionController = controllerGameObject.AddComponent<TileSelectionController>();

            // Create test tiles
            var tile1 = new ModuleTile(1, "Tile1",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior());
            var tile2 = new ModuleTile(2, "Tile2",
                new DefaultMovementBehavior(new MovementRules(1, true, false)),
                new DefaultAbilityBehavior());

            // Create TileViews
            tileView1GameObject = new GameObject("TileView1");
            tileView1 = tileView1GameObject.AddComponent<TileView>();
            tileView1GameObject.AddComponent<SpriteRenderer>();
            tileView1.Init(tile1, new CellPos { X = 0, Y = 0 });

            tileView2GameObject = new GameObject("TileView2");
            tileView2 = tileView2GameObject.AddComponent<TileView>();
            tileView2GameObject.AddComponent<SpriteRenderer>();
            tileView2.Init(tile2, new CellPos { X = 1, Y = 1 });
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(controllerGameObject);
            Object.DestroyImmediate(tileView1GameObject);
            Object.DestroyImmediate(tileView2GameObject);
        }

        [Test]
        public void GetSelectedTile_Initially_ReturnsNull()
        {
            // Assert
            Assert.IsNull(selectionController.GetSelectedTile());
        }

        [Test]
        public void SelectTileForTest_SelectsTile()
        {
            // Act
            selectionController.SelectTileForTest(tileView1);

            // Assert
            Assert.AreEqual(tileView1, selectionController.GetSelectedTile());
        }

        [Test]
        public void OnTileSelected_Event_FiresWhenTileSelected()
        {
            // Arrange
            TileView? selectedTile = null;
            selectionController.OnTileSelected += (tile) => selectedTile = tile;

            // Act
            selectionController.SelectTileForTest(tileView1);

            // Assert
            Assert.AreEqual(tileView1, selectedTile);
            Assert.AreEqual(tileView1, selectionController.GetSelectedTile());
        }

        [Test]
        public void SelectTile_Twice_KeepsLatestSelection()
        {
            // Act
            selectionController.SelectTileForTest(tileView1);
            selectionController.SelectTileForTest(tileView2);

            // Assert
            Assert.AreEqual(tileView2, selectionController.GetSelectedTile());
        }

        [Test]
        public void ClearSelection_WithSelection_ClearsTile()
        {
            // Arrange
            selectionController.SelectTileForTest(tileView1);

            // Act
            selectionController.ClearSelection();

            // Assert
            Assert.IsNull(selectionController.GetSelectedTile());
        }

        [Test]
        public void OnTileDeselected_Event_FiresWhenCleared()
        {
            // Arrange
            bool deselectedFired = false;
            selectionController.OnTileDeselected += () => deselectedFired = true;
            selectionController.SelectTileForTest(tileView1);

            // Act
            selectionController.ClearSelection();

            // Assert
            Assert.IsTrue(deselectedFired);
        }

        [Test]
        public void SelectTile_WhenAlreadySelected_DeselectsPrevious()
        {
            // Arrange
            int deselectedCount = 0;
            selectionController.OnTileDeselected += () => deselectedCount++;
            selectionController.SelectTileForTest(tileView1);

            // Act
            selectionController.SelectTileForTest(tileView2);

            // Assert
            Assert.AreEqual(1, deselectedCount, "Should deselect previous tile");
            Assert.AreEqual(tileView2, selectionController.GetSelectedTile());
        }
    }
}
