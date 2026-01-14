using Gameplay.Engine.Abilities;
using Gameplay.Engine.Moves;
using UnityEngine;

namespace Gameplay.Engine.Tiles
{
    public sealed class TileConfig
    {
        public string TypeKey { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public Color color; // temp for testing

        public MovementRules MovementRules { get; set; } = new();
        public IAbilityBehavior AbilityBehavior { get; set; } = new DefaultAbilityBehavior();
    }
}