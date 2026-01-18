using Gameplay.Engine.Board;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Tiles;
using Gameplay.Presentation.Tiles;
using UnityCoreKit.Runtime.Core;
using UnityCoreKit.Runtime.Core.Services;
using UnityCoreKit.Runtime.UserInteractions;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Handles clicks on empty board cells (destinations) to execute tile movement.
    /// </summary>
    public class DestinationClickHandler : MonoBehaviour
    {
        [SerializeField] private TileSelectionController selectionController = null!;
        [SerializeField] private MovePreviewController movePreviewController = null!;

        private MoveExecutor? moveExecutor;
        private IBoardState? board;
        private IUserInteractions? interactions;

        private void Awake()
        {
            interactions = CoreServices.Get<IUserInteractions>();
            interactions.Subscribe(this, OnClick);
        }

        private void OnDestroy()
        {
            interactions?.UnsubscribeAll(this);
        }

        public void Init(IBoardState boardState, MoveExecutor executor)
        {
            board = boardState;
            moveExecutor = executor;
        }

        private void OnClick(UserInteractionEvent evt)
        {
            // Only process if we have a selected tile
            var selectedTile = selectionController.GetSelectedTile();
            if (selectedTile == null)
                return;

            // If clicking another tile, let TileSelectionController handle it
            if (evt.Target is TileView)
                return;

            // TODO: Implement destination cell click detection
            // For now, requires either:
            // 1. Colliders on empty cells (expensive)
            // 2. Raycasting from screen to board grid (better)
            // 3. Clicking on highlight objects themselves (simple MVP)
        }

        /// <summary>
        /// Attempts to move the currently selected tile to the specified destination.
        /// </summary>
        public void TryMoveSelectedTileTo(CellPos destination)
        {
            var selectedTile = selectionController.GetSelectedTile();
            if (selectedTile == null || moveExecutor == null)
                return;

            var from = selectedTile.BoardPosition;
            
            // Validate move is in available moves list
            if (!IsValidMove(selectedTile, destination))
            {
                Debug.LogWarning($"[DestinationClickHandler] Move to ({destination.X},{destination.Y}) is not valid");
                return;
            }

            // Execute move
            bool success = moveExecutor.ExecuteMove(from, destination);
            
            if (success)
            {
                // Clear selection after successful move
                selectionController.ClearSelection();
            }
        }

        private bool IsValidMove(TileView tileView, CellPos destination)
        {
            if (board == null)
                return false;

            // Cast to IMovingTile to access movement behavior
            if (tileView.Tile is not IMovingTile movingTile)
                return false;

            // Get the tile's actual movement rules from its behavior
            var movementRules = movingTile.MovementBehavior.MovementRules;
            var context = new MoveContext(board, tileView.BoardPosition, movementRules);
            var moves = movingTile.GetAvailableMoves(context);

            foreach (var move in moves)
            {
                if (move.Destination.X == destination.X && move.Destination.Y == destination.Y)
                    return true;
            }

            return false;
        }
    }
}
