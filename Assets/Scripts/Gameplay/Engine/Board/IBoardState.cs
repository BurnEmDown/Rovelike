#nullable enable
using System.Collections.Generic;
using static Gameplay.Engine.Board.Structs;
using Gameplay.Engine.Tiles;

namespace Gameplay.Engine.Board
{
    /// <summary>
    /// Represents a read/write view of the logical tile board used by the puzzle engine.
    /// 
    /// The board is a 2D grid of fixed width and height. Each cell may contain zero or one
    /// <see cref="Tile"/> instance. Implementations of this interface are responsible for
    /// maintaining tile placement and ensuring board invariants (e.g., no overlapping tiles).
    ///
    /// This interface is intended for engine-level operations, including:
    ///  - Allowing movement logic to inspect tile positions.
    ///  - Allowing game controllers to place or move tiles.
    ///  - Supporting objective logic that evaluates board configuration.
    ///
    /// It does *not* expose any Unity-specific details; rendering and MonoBehaviour logic
    /// exist in higher layers of the application.
    /// </summary>
    public interface IBoardState
    {
        /// <summary>
        /// Width of the board (number of columns).
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height of the board (number of rows).
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Retrieves the tile located at the given board position.
        /// Returns <c>null</c> if the position is empty. <br/>
        /// This method assumes <paramref name="pos"/> is inside the board.
        /// Callers should check <see cref="IsInsideBounds"/> when necessary.
        /// </summary>
        /// <param name="pos">The grid coordinate to query.</param>
        /// <returns>The tile at the given position, or <c>null</c> if empty.</returns>
        Tile? GetTileAt(CellPos pos);

        /// <summary>
        /// Returns a snapshot of all tile positions currently occupied on the board.
        /// The returned collection does not update automatically if the board changes.
        /// </summary>
        /// <returns>A read-only list of positions containing tiles.</returns>
        IReadOnlyList<CellPos> GetAllTilePositions();

        /// <summary>
        /// Returns a snapshot of all tile instances currently placed on the board.
        /// The returned list is a snapshot; modifying it does not change the board.
        /// </summary>
        /// <returns>A read-only list of all tiles on the board.</returns>
        IReadOnlyList<Tile> GetAllTiles();

        /// <summary>
        /// Attempts to place a tile at a specific position on the board.
        /// Placement fails if either: <br/>
        /// 1. The position is outside the bounds of the board. <br/>
        /// 2. The cell is already occupied. <br/>
        /// On success, the board begins tracking the tile at the specified position.
        /// </summary>
        /// <param name="pos">The target position for the tile.</param>
        /// <param name="tile">The tile instance to place.</param>
        /// <returns><c>true</c> if placement succeeds; otherwise <c>false</c>.</returns>
        bool TryPlaceTile(CellPos pos, Tile tile);

        /// <summary>
        /// Moves a tile from one position to another on the board.
        /// This operation assumes the origin contains a tile and the destination is valid.
        /// </summary>
        /// <param name="from">The position to move the tile from.</param>
        /// <param name="to">The position to move the tile to.</param>
        void MoveTile(CellPos from, CellPos to);

        /// <summary>
        /// Determines whether a position lies within the board's boundaries.
        /// A position is considered valid if: <br/>
        /// <c>0 &lt;= x &lt; Width</c> and <c>0 &lt;= y &lt; Height</c>.
        /// </summary>
        /// <param name="pos">The position to test.</param>
        /// <returns><c>true</c> if inside bounds; otherwise <c>false</c>.</returns>
        bool IsInsideBounds(CellPos pos);
    }
}