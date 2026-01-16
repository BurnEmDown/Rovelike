using Gameplay.Engine.Moves;

namespace Gameplay.Engine.Tiles
{
    /// <summary>
    /// Provides a pure C# factory for creating fully configured
    /// <see cref="ModuleTile"/> instances from a supplied <see cref="TileConfig"/>.
    ///
    /// <para>
    /// This factory is part of the engine layer and contains no Unity dependencies,
    /// allowing tiles to be constructed in unit tests or other non-Unity contexts.
    /// The Unity-facing <c>TileFactory</c> in the game layer simply adapts
    /// <c>TileDefinition</c> ScriptableObjects into <see cref="TileConfig"/> objects
    /// and delegates creation to this class.
    /// </para>
    ///
    /// <para>
    /// The factory ensures that each tile receives a unique, engine-level ID and
    /// initializes its movement and ability behaviors based on the provided config.
    /// Tiles are configured via composition using behavior objects, making the system
    /// fully data-driven.
    /// </para>
    /// </summary>
    public static class EngineTileFactory
    {
        /// <summary>
        /// Counter used to generate unique tile identifiers.
        /// </summary>
        private static int nextId = 1;

        /// <summary>
        /// Creates a new <see cref="ModuleTile"/> instance using the settings
        /// specified in a <see cref="TileConfig"/> object.
        ///
        /// <para>
        /// The resulting tile is fully configured with:
        /// </para>
        /// <list type="bullet">
        ///     <item><description>
        ///     A unique identifier assigned by the factory.
        ///     </description></item>
        ///     <item><description>
        ///     Type metadata provided by <see cref="TileConfig.TypeKey"/>.
        ///     </description></item>
        ///     <item><description>
        ///     A movement behavior constructed from
        ///     <see cref="TileConfig.MovementRules"/>.
        ///     </description></item>
        ///     <item><description>
        ///     An ability behavior copied directly from
        ///     <see cref="TileConfig.AbilityBehavior"/>.
        ///     </description></item>
        /// </list>
        /// </summary>
        /// <param name="config">
        /// Configuration object describing the tile's movement rules, ability
        /// behavior, and metadata. Must not be <c>null</c>.
        /// </param>
        ///
        /// <returns>
        /// A fully constructed <see cref="ModuleTile"/> instance ready for use in
        /// board logic or simulation.
        /// </returns>
        public static ModuleTile CreateTile(TileConfig config)
        {
            var id = nextId++;
            var movementBehavior = new DefaultMovementBehavior(config.MovementRules);

            return new ModuleTile(
                id,
                config.TypeKey,
                // config.DisplayName, (optional)
                movementBehavior,
                config.AbilityBehavior
            );
        }
    }
}