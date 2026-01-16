using Gameplay.Engine.Moves;
using UnityEngine;

namespace Gameplay.Game.Definitions
{
    /// <summary>
    /// Unity ScriptableObject defining a tile type's visual and behavioral properties.
    /// 
    /// <para>
    /// This is the designer-facing data format. All tile properties are editable
    /// in the Unity Inspector without touching code. Changes made here automatically
    /// affect runtime tile behavior via the factory system.
    /// </para>
    /// 
    /// <para>
    /// <b>Architecture Note: Separation of Data and Logic</b><br/>
    /// • <b>TileDefinition</b>: Data only (ScriptableObject, serializable)<br/>
    /// • <b>ModuleTile</b>: Logic only (engine C#, behaviors)<br/>
    /// • <b>TileFactory</b>: Translation layer between the two
    /// </para>
    /// 
    /// <para>
    /// To add a new tile type, create a new asset from this template, configure its
    /// properties in the Inspector, and add it to the <see cref="TileLibrarySO"/>.
    /// </para>
    /// </summary>
    [CreateAssetMenu(menuName = "Rovelike/Tile Definition")]
    public class TileDefinition : ScriptableObject
    {
        [Header("Identity")]
        /// <summary>
        /// Internal numeric ID for serialization.
        /// </summary>
        public int id;
        
        /// <summary>
        /// Unique string identifier for this tile type (e.g., "Brain", "Motor", "Coil").
        /// Must match across all systems (factories, serialization, logic).
        /// </summary>
        public string typeKey;
        //public string displayName;

        [Header("Movement")]
        /// <summary>
        /// Maximum distance this tile can move in a single action.
        /// Engine validation ensures moves don't exceed this value.
        /// </summary>
        public int maxMoveDistance = 1;
        
        /// <summary>
        /// Whether this tile can move in orthogonal directions (up, down, left, right).
        /// </summary>
        public bool allowOrthogonal = true;
        
        /// <summary>
        /// Whether this tile can move diagonally.
        /// If both orthogonal and diagonal are false, the tile cannot move.
        /// </summary>
        public bool allowDiagonal = false;
        
        /// <summary>
        /// Defines how this tile interacts with obstacles during movement.
        /// See <see cref="ObstaclePassRule"/> for available options.
        /// </summary>
        public ObstaclePassRule passRule = ObstaclePassRule.CannotPassThrough;

        //[Header("Ability")]
        //public AbilityKind abilityKind; // enum or type key for which ability behavior to use

        //[Header("Art")]
        /// <summary>
        /// Sprite used to visually represent this tile on the board.
        /// </summary>
        public Sprite tileSprite;

        /// <summary>
        /// Temporary color for visual testing. Will be replaced with sprite-based rendering.
        /// </summary>
        public Color color; // temp for testing
        // maybe color, animation, SFX, etc.
    }
}