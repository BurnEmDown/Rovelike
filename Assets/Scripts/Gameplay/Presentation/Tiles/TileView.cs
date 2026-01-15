using GameEvents;
using Gameplay.Engine.Tiles;
using Gameplay.Game.Definitions;
using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityCoreKit.Runtime.UserInteractions;
using UnityCoreKit.Runtime.UserInteractions.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Presentation.Tiles
{
    
    /// <summary>
    /// Unity view for rendering a tile and forwarding user interaction.
    /// Holds a read-only reference to the engine tile model and its current board position.
    /// </summary>
    public class TileView : MonoBehaviour, IUserInteractionTarget
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PointerClickUserInteractionSource interactionSource;
        
        private IReadOnlyModuleTile moduleTile;
        private CellPos boardPosition;

        // IUserInteractionTarget
        public string InteractionKey => "Tile";
        public object Model => moduleTile;
        
        /// <summary>
        /// The read-only tile model represented by this view.
        /// </summary>
        public IReadOnlyModuleTile Tile => moduleTile;
        
        /// <summary>
        /// The tile's current board coordinate as known by this view.
        /// </summary>
        public CellPos BoardPosition => boardPosition;

        private void OnEnable()
        {
            // Initialize interaction source when tile view is enabled (spawned from pool)
            if (interactionSource != null)
            {
                var interactions = CoreServices.Get<IUserInteractions>();
                interactionSource.Init(interactions, this);
            }
        }

        private void OnDisable()
        {
            moduleTile = null;
            boardPosition = default;
        }
        
        /// <summary>
        /// Initializes the view with a read-only tile model and a board position.
        /// Visuals may be applied separately via <see cref="SetVisuals"/>.
        /// </summary>
        public void Init(IReadOnlyModuleTile tile, CellPos position)
        {
            moduleTile = tile;
            boardPosition = position;
            
            gameObject.name = $"TileView_{tile.TypeKey}_{tile.Id}";
        }
        
        /// <summary>
        /// Applies Unity-side visuals for this tile view from the provided definition.
        /// </summary>
        public void SetVisuals(TileDefinition definition)
        {
            if (spriteRenderer == null || definition == null)
                return;

            spriteRenderer.sprite = definition.tileSprite;
            spriteRenderer.color = definition.color;
        }
        
        /// <summary>
        /// Updates the cached board position. Call this after the tile is moved on the engine board.
        /// </summary>
        public void SetBoardPosition(CellPos newPosition)
        {
            boardPosition = newPosition;
        }
    }
}