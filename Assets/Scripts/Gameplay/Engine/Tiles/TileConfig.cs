using Gameplay.Engine.Abilities;
using Gameplay.Engine.Moves;
using UnityEngine;

namespace Gameplay.Engine.Tiles
{
    /// <summary>
    /// Transfer object carrying tile configuration data from Unity layer to engine layer.
    /// 
    /// <para>
    /// This class acts as the boundary between Unity-dependent code (<c>TileFactory</c>)
    /// and engine code (<c>EngineTileFactory</c>). TileFactory creates instances from
    /// ScriptableObjects, and EngineTileFactory consumes them to construct tiles.
    /// </para>
    /// </summary>
    public sealed class TileConfig
    {
        /// <summary>
        /// String identifier for this tile type (e.g., "Brain", "Motor", "Coil").
        /// Used for lookup, serialization, and debug display.
        /// </summary>
        public string TypeKey { get; set; } = "";
        
        /// <summary>
        /// Human-readable display name for UI. If empty, falls back to TypeKey.
        /// </summary>
        public string DisplayName { get; set; } = "";
        
        /// <summary>
        /// Temporary color for visual testing. Will be replaced with sprite-based rendering.
        /// </summary>
        public Color color; // temp for testing

        /// <summary>
        /// Movement behavior configuration defining how this tile can move.
        /// Includes max distance, direction constraints, and pass-through rules.
        /// </summary>
        public MovementRules MovementRules { get; set; } = new();
        
        /// <summary>
        /// Custom ability behavior. Defaults to <see cref="DefaultAbilityBehavior"/> if not specified.
        /// </summary>
        public IAbilityBehavior AbilityBehavior { get; set; } = new DefaultAbilityBehavior();
    }
}