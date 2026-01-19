using System.Collections.Generic;
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
        /// Handles push moves if destination is occupied and push is valid.
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

            // Check if destination is occupied
            var destinationTile = boardState.GetTileAt(to);
            if (destinationTile != null)
            {
                // Destination occupied - attempt push
                return ExecutePushMove(from, to);
            }

            // Simple move to empty cell
            // Execute move on engine
            boardState.MoveTile(from, to);

            // Update view
            boardPresenter.MoveView(from, to);

            Debug.Log($"[MoveExecutor] Moved {tile.TypeKey} from ({from.X},{from.Y}) to ({to.X},{to.Y})");
            return true;
        }

        /// <summary>
        /// Executes a push move where the moving tile pushes one or more tiles in a chain.
        /// </summary>
        private bool ExecutePushMove(CellPos from, CellPos to)
        {
            // Calculate push direction
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;

            // Normalize direction (in case of multi-step move)
            if (dx != 0) dx = dx / System.Math.Abs(dx);
            if (dy != 0) dy = dy / System.Math.Abs(dy);

            // Find all tiles in the push chain
            var tilesToPush = new List<CellPos>();
            var currentPos = to;
            
            while (boardState.IsInsideBounds(currentPos))
            {
                var tileAtPos = boardState.GetTileAt(currentPos);
                if (tileAtPos == null)
                {
                    // Found empty space - this is where the push chain ends
                    break;
                }
                
                tilesToPush.Add(currentPos);
                currentPos = new CellPos { X = currentPos.X + dx, Y = currentPos.Y + dy };
            }

            // Validate we found an empty space for the last tile in the chain
            var finalDestination = new CellPos { X = to.X + dx * tilesToPush.Count, Y = to.Y + dy * tilesToPush.Count };
            
            if (!boardState.IsInsideBounds(finalDestination))
            {
                Debug.LogWarning($"[MoveExecutor] Cannot push: chain pushes out of bounds at ({finalDestination.X},{finalDestination.Y})");
                return false;
            }

            var tileAtFinalDestination = boardState.GetTileAt(finalDestination);
            if (tileAtFinalDestination != null)
            {
                Debug.LogWarning($"[MoveExecutor] Cannot push: no empty space for push chain");
                return false;
            }

            var movingTile = boardState.GetTileAt(from);

            // Execute push chain: move tiles from back to front to avoid overwrites
            for (int i = tilesToPush.Count - 1; i >= 0; i--)
            {
                var pushFrom = tilesToPush[i];
                var pushTo = new CellPos { X = pushFrom.X + dx, Y = pushFrom.Y + dy };
                
                var pushedTile = boardState.GetTileAt(pushFrom);
                boardState.MoveTile(pushFrom, pushTo);
                boardPresenter.MoveView(pushFrom, pushTo);
                
                Debug.Log($"[MoveExecutor] Pushed {pushedTile?.TypeKey} from ({pushFrom.X},{pushFrom.Y}) to ({pushTo.X},{pushTo.Y})");
            }
            
            // Finally, move the pushing tile
            boardState.MoveTile(from, to);
            boardPresenter.MoveView(from, to);

            Debug.Log($"[MoveExecutor] Push move complete: {movingTile?.TypeKey} pushed {tilesToPush.Count} tile(s) from ({from.X},{from.Y}) to ({to.X},{to.Y})");
            return true;
        }
    }
}
