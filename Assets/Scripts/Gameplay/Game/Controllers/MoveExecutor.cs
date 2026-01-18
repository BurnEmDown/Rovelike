using Gameplay.Engine.Board;
using Gameplay.Presentation.Board;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Executes tile movement by coordinating engine state updates (BoardState) 
    /// with presentation layer updates (BoardPresenter).
    /// </summary>
    public class MoveExecutor
    {
        private readonly IBoardState boardState;
        private readonly BoardPresenter boardPresenter;

        public MoveExecutor(IBoardState boardState, BoardPresenter boardPresenter)
        {
            this.boardState = boardState;
            this.boardPresenter = boardPresenter;
        }

        /// <summary>
        /// Executes a tile move from one position to another.
        /// Updates both engine state and view.
        /// </summary>
        /// <returns>True if move succeeded, false if invalid.</returns>
        public bool ExecuteMove(CellPos from, CellPos to)
        {
            // Validate positions are in bounds
            if (!boardState.IsInsideBounds(from) || !boardState.IsInsideBounds(to))
            {
                Debug.LogWarning($"[MoveExecutor] Invalid move: ({from.X},{from.Y}) → ({to.X},{to.Y}) out of bounds");
                return false;
            }

            // Get tile at origin
            var tile = boardState.GetTileAt(from);
            if (tile == null)
            {
                Debug.LogWarning($"[MoveExecutor] No tile at origin ({from.X},{from.Y})");
                return false;
            }

            // Validate destination is empty
            var destinationTile = boardState.GetTileAt(to);
            if (destinationTile != null)
            {
                Debug.LogWarning($"[MoveExecutor] Destination ({to.X},{to.Y}) is occupied");
                return false;
            }

            // Execute move on engine
            boardState.MoveTile(from, to);

            // Update view
            boardPresenter.MoveView(from, to);

            Debug.Log($"[MoveExecutor] Moved {tile.TypeKey} from ({from.X},{from.Y}) to ({to.X},{to.Y})");
            return true;
        }
    }
}
