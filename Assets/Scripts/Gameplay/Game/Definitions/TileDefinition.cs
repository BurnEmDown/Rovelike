using Gameplay.Engine.Moves;
using UnityEngine;

namespace Gameplay.Game.Definitions
{
    [CreateAssetMenu(menuName = "Rovelike/Tile Definition")]
    public class TileDefinition : ScriptableObject
    {
        [Header("Identity")]
        public int id;
        public string typeKey;
        //public string displayName;

        [Header("Movement")]
        public int maxMoveDistance = 1;
        public bool allowOrthogonal = true;
        public bool allowDiagonal = false;
        public ObstaclePassRule passRule = ObstaclePassRule.CannotPassThrough;

        //[Header("Ability")]
        //public AbilityKind abilityKind; // enum or type key for which ability behavior to use

        //[Header("Art")]
        public Sprite tileSprite;

        public Color color; // temp for testing
        // maybe color, animation, SFX, etc.
    }
}