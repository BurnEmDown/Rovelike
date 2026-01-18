using System.Collections.Generic;
using Gameplay.Engine.Moves;

namespace Gameplay.Engine.Tiles
{
    public interface IMovingTile
    {
        /// <summary>
        /// Gets the movement behavior that determines how this tile can move.
        /// </summary>
        IMovementBehavior MovementBehavior { get; }
        
        IReadOnlyList<MoveOption> GetAvailableMoves(MoveContext context);
    }
}