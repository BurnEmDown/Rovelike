using System.Collections.Generic;
using Gameplay.Engine.Board;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Engine.Moves
{
    /// <summary>
    /// Provides generic movement calculation logic for tiles based on a set of
    /// <see cref="MovementRules"/>. This helper performs directional raycasts
    /// from a tile's origin and determines all valid <see cref="MoveOption"/>
    /// destinations allowed by the current rule set.
    ///
    /// <para>
    /// This class is designed as reusable logic shared across all tiles that move
    /// according to directional step-based movement. Tile-specific behavior may be
    /// implemented by overriding <c>GetAvailableMoves</c> inside individual tile types,
    /// or by adjusting the <see cref="MovementRules"/> passed into this method.
    /// </para>
    ///
    /// <para>
    /// The algorithm proceeds by:
    /// <list type="number">
    ///     <item><description>Building a list of allowed movement directions.</description></item>
    ///     <item><description>Raycasting outward up to <c>MaxSteps</c> in each direction.</description></item>
    ///     <item><description>Evaluating each cell using <see cref="ObstaclePassRule"/> logic.</description></item>
    ///     <item><description>Recording reachable empty cells as <see cref="MoveOption"/> results.</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// This method does not modify the board. It only determines possible destinations.
    /// Applying moves is the responsibility of game control logic (e.g. <c>BoardState</c>).
    /// </para>
    /// </summary>
    public static class MovementCalculator
    {
        /// <summary>
        /// Calculates all legal movement destinations for a tile located at
        /// <paramref name="origin"/> using the specified <paramref name="rules"/>.
        /// 
        /// <para>
        /// The board is treated as read-only. Tiles may be inspected but not moved.
        /// </para>
        ///
        /// <para>
        /// Behavior varies depending on the obstacle rule:
        /// </para>
        ///
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///     <see cref="ObstaclePassRule.CannotPassThrough"/>:
        ///     Movement is blocked immediately when an obstacle is encountered.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <description>
        ///     <see cref="ObstaclePassRule.CanPassThrough"/>:
        ///     Movement may continue past obstacles, but the tile may not end its
        ///     movement on the obstacle itself.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <description>
        ///     <see cref="ObstaclePassRule.MustPassThrough"/>:
        ///     The tile may only stop on empty cells after at least one obstacle
        ///     has been encountered along the movement path.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <description>
        ///     <see cref="ObstaclePassRule.PushObstacles"/>:
        ///     Placeholder for future logic where tiles may push obstructing tiles.
        ///     Currently unimplemented.
        ///     </description>
        ///   </item>
        /// </list>
        ///
        /// <para>
        /// The resulting list may be empty if no legal destinations exist under the
        /// current constraints.
        /// </para>
        /// </summary>
        ///
        /// <param name="context">Given context for the move.</param>
        ///
        /// <returns>
        /// A read-only list of <see cref="MoveOption"/> objects, each representing a
        /// legal destination cell. The caller is responsible for deciding which move
        /// to perform and executing the actual board state mutation.
        /// </returns>
        public static IReadOnlyList<MoveOption> GetMoves(MoveContext context)
        {
            var results = new List<MoveOption>();
            
            IBoardState board = context.Board;
            CellPos origin = context.CurrentPosition;
            MovementRules rules = context.Rules;

            // Build direction list based on rules
            var directions = new List<(int dx, int dy)>();

            if (rules.AllowOrthogonal)
            {
                directions.Add((1, 0));
                directions.Add((-1, 0));
                directions.Add((0, 1));
                directions.Add((0, -1));
            }

            if (rules.AllowDiagonal)
            {
                directions.Add((1, 1));
                directions.Add((1, -1));
                directions.Add((-1, 1));
                directions.Add((-1, -1));
            }

            // For each allowed direction, raycast up to MaxSteps
            foreach (var (dx, dy) in directions)
            {
                bool passedObstacle = false;

                for (int step = 1; step <= rules.MaxSteps; step++)
                {
                    var pos = new CellPos();
                    pos.X = origin.X + dx * step;
                    pos.Y = origin.Y + dy * step;

                    if (!board.IsInsideBounds(pos))
                        break;

                    var tileAtPos = board.GetTileAt(pos);
                    bool isObstacle = tileAtPos != null;

                    // Handle obstacle interaction rules
                    if (isObstacle)
                    {
                        switch (rules.PassRule)
                        {
                            case ObstaclePassRule.CannotPassThrough:
                                // cannot enter or go beyond this tile
                                goto EndDirectionLoop;

                            case ObstaclePassRule.CanPassThrough:
                                // can move through this cell but not into it,
                                // continue towards next positions in the same direction
                                continue;

                            case ObstaclePassRule.MustPassThrough:
                                // mark that we passed an obstacle; we may only accept positions 
                                // if at least one obstacle has been encountered
                                passedObstacle = true;
                                continue;

                            case ObstaclePassRule.PushObstacles:
                                // can push the existing obstacle 
                                results.Add(new MoveOption(pos));
                                goto EndDirectionLoop;

                            default:
                                goto EndDirectionLoop;
                        }
                    }
                    else
                    {
                        // empty cell
                        if (rules.PassRule == ObstaclePassRule.MustPassThrough)
                        {
                            if (passedObstacle)
                                results.Add(new MoveOption(pos));
                        }
                        else
                        {
                            results.Add(new MoveOption(pos));
                        }
                    }
                } // end of step loop

                EndDirectionLoop: ; // label target
            }

            return results;
        }
    }
}