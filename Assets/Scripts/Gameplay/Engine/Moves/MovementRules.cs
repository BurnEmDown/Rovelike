using Gameplay.Engine.Tiles;

namespace Gameplay.Engine.Moves
{
    /// <summary>
    /// Defines the movement constraints applied to a tile during a movement action.
    /// 
    /// Movement rules represent *external* constraints on movement (typically coming
    /// from a command card, action, or special effect) rather than the tile's inherent
    /// movement pattern. They describe how far a tile may move, what directions are
    /// permitted, and how the path may interact with obstacles.
    /// 
    /// These rules are combined with tile-specific movement logic inside
    /// <see cref="IMovingTile"/> implementations to determine the final set of valid
    /// <see cref="MoveOption"/> destinations.
    /// 
    /// This class is designed to be lightweight, immutable by game logic, and easy to
    /// extend with additional constraints if needed.
    /// </summary>
    public class MovementRules
    {
        /// <summary>
        /// The maximum number of steps a tile may move during this action.
        /// A step is one tile-to-adjacent-tile movement in any allowed direction.
        /// 
        /// A value of zero prevents movement entirely.
        /// </summary>
        public int MaxSteps { get; private set; }

        /// <summary>
        /// If true, movement along orthogonal directions (up, down, left, right)
        /// is permitted.
        /// </summary>
        public bool AllowOrthogonal { get; private set; }

        /// <summary>
        /// If true, movement along diagonal directions is permitted.
        /// If both <see cref="AllowOrthogonal"/> and this flag are false, the tile
        /// cannot move unless explicit tile logic handles special cases (e.g. teleporting).
        /// </summary>
        public bool AllowDiagonal { get; private set; }

        /// <summary>
        /// Defines how the movement path interacts with obstacles on the board.
        /// The exact behavior is interpreted by tile movement logic, but the values
        /// generally control whether the tile must avoid, may pass through, must pass
        /// through, or may push obstacles.
        /// </summary>
        public ObstaclePassRule PassRule { get; private set; }

        // Extensible for future variants:
        // public bool MustEndOnObjective { get; init; }

        /// <summary>
        /// Creates a new set of movement constraints to be applied during a movement action.
        /// 
        /// <para>
        /// These parameters define constraints on movement and are combined with a tile's inherent
        /// movement logic to determine the final set of legal destinations.
        /// </para>
        /// 
        /// <para>
        /// The most common default configuration allows one orthogonal step and does not allow
        /// passing through obstacles.
        /// </para>
        /// </summary>
        /// <param name="maxSteps">
        /// The maximum number of grid steps the tile may take during this movement action.
        /// A value of 1 limits movement to adjacent cells.
        /// </param>
        /// <param name="allowOrthogonal">
        /// If <c>true</c>, movement along the four cardinal directions (up, down, left, right)
        /// is allowed.
        /// </param>
        /// <param name="allowDiagonal">
        /// If <c>true</c>, movement along diagonal directions is allowed.
        /// </param>
        /// <param name="passRule">
        /// Defines how the movement path interacts with obstacles (typically other tiles).
        /// </param>
        public MovementRules(int maxSteps = 1,
            bool allowOrthogonal = true,
            bool allowDiagonal = false,
            ObstaclePassRule passRule = ObstaclePassRule.CannotPassThrough)
        {
            MaxSteps = maxSteps;
            AllowOrthogonal = allowOrthogonal;
            AllowDiagonal = allowDiagonal;
            PassRule = passRule;
        }
    }

    /// <summary>
    /// Describes how a movement path should interact with obstacles on the board.
    /// Obstacles typically include other tiles but may be extended to include
    /// special board features depending on game rules.
    /// </summary>
    public enum ObstaclePassRule
    {
        /// <summary>
        /// The movement path must NOT pass through any obstacles.
        /// Movement is blocked by obstacles in the path.
        /// </summary>
        CannotPassThrough,

        /// <summary>
        /// The movement path MAY pass through obstacles, but is not required to.
        /// </summary>
        CanPassThrough,

        /// <summary>
        /// The movement path MUST pass through at least one obstacle.
        /// If no obstacle lies along the possible path, the move is invalid.
        /// </summary>
        MustPassThrough,

        /// <summary>
        /// The tile is permitted to enter cells containing obstacles, pushing them
        /// out of the way if the tile's movement logic supports pushing behavior.
        /// This mode requires tile-specific logic to define how pushing works.
        /// </summary>
        PushObstacles
    }
}