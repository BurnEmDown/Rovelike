using System.Collections.Generic;
using Gameplay.Engine.Abilities;
using Gameplay.Engine.Moves;

namespace Gameplay.Engine.Tiles
{
    /// <summary>
    /// An class for tiles that support both movement and ability
    /// functionality through composable behaviors.
    ///
    /// <para>
    /// <see cref="ModuleTile"/> represents a unit on the board that has two
    /// capabilities:
    /// </para>
    ///
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///         Movement via an injected <see cref="IMovementBehavior"/>, which
    ///         determines how the tile computes legal movement options.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///         Ability usage via an injected <see cref="IAbilityBehavior"/>, which
    ///         determines whether the tile can use its ability and what ability
    ///         options are available.
    ///         </description>
    ///     </item>
    /// </list>
    ///
    /// </summary>
    public class ModuleTile : Tile, IMovingTile, IAbilityTile, IReadOnlyModuleTile
    {
        /// <summary>
        /// The movement behavior associated with this tile. It determines how the
        /// tile computes available movement destinations.
        /// </summary>
        protected readonly IMovementBehavior movementBehavior;

        /// <summary>
        /// Gets the movement behavior that determines how this tile can move.
        /// </summary>
        public IMovementBehavior MovementBehavior => movementBehavior;

        /// <summary>
        /// The ability behavior associated with this tile. It determines whether
        /// the tile's ability is available and what actions it may perform.
        /// </summary>
        protected readonly IAbilityBehavior abilityBehavior;
    
        /// <summary>
        /// Constructs a new module tile with the specified movement and ability
        /// behaviors.
        ///
        /// <para>
        /// Concrete tile types supply specific behavior implementations through
        /// dependency injection, enabling maximal flexibility in defining unique
        /// tile behaviors without forcing them into a rigid class hierarchy.
        /// </para>
        /// </summary>
        /// <param name="id">The unique identifier of the tile.</param>
        /// <param name="typeKey">
        /// A string representing the tile's type or classification.
        /// </param>
        /// <param name="movementBehavior">
        /// The movement behavior used to compute valid movement options.
        /// </param>
        /// <param name="abilityBehavior">
        /// The ability behavior used to compute available ability options.
        /// </param>
        public ModuleTile(
            int id,
            string typeKey,
            IMovementBehavior movementBehavior,
            IAbilityBehavior abilityBehavior)
            : base(id, typeKey)
        {
            this.movementBehavior = movementBehavior;
            this.abilityBehavior = abilityBehavior;
        }

        /// <summary>
        /// Computes all legal movement destinations for this tile using its
        /// associated <see cref="IMovementBehavior"/> and the supplied
        /// <see cref="MoveContext"/>.
        ///
        /// <para>
        /// This method does not modify board state; it only enumerates possible
        /// movement actions.
        /// </para>
        /// </summary>
        /// <param name="context">
        /// A snapshot of the board, tile position, and movement rule modifiers.
        /// </param>
        /// <returns>
        /// A read-only list of <see cref="MoveOption"/> objects representing all
        /// legal destinations for the tile.
        /// </returns>
        public IReadOnlyList<MoveOption> GetAvailableMoves(MoveContext context)
            => movementBehavior.GetAvailableMoves(context);

        /// <summary>
        /// Indicates whether the tile's ability is currently available for use,
        /// as determined by its <see cref="IAbilityBehavior"/>.
        /// </summary>
        public bool IsAbilityAvailable => abilityBehavior.IsAbilityAvailable;

        /// <summary>
        /// Computes all valid ability options for this tile using its associated
        /// <see cref="IAbilityBehavior"/> and the supplied
        /// <see cref="AbilityContext"/>.
        ///
        /// <para>
        /// The actual execution of an ability option is handled externally,
        /// typically by the game controller.
        /// </para>
        /// </summary>
        /// <param name="context">
        /// A snapshot of relevant board and game state used to determine available
        /// ability options.
        /// </param>
        /// <returns>
        /// A read-only list of <see cref="AbilityOption"/> objects representing all
        /// ability actions currently available to the tile.
        /// </returns>
        public IReadOnlyList<AbilityOption> GetAbilityOptions(AbilityContext context)
            => abilityBehavior.GetAbilityOptions(context);

        // Ability execution can be delegated as well:
        // public void UseAbility(AbilityContext context, AbilityOption option)
        //    => abilityBehavior.UseAbility(context, option);
    }
}