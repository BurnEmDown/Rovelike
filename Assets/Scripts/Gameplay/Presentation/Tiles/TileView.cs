using Gameplay.Engine.Tiles;
using Gameplay.Game.Definitions;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Presentation.Tiles
{
    
    /// <summary>
    /// Unity view for rendering a tile and forwarding user interaction.
    /// Holds a read-only reference to the engine tile model and its current board position.
    /// </summary>
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private IReadOnlyModuleTile moduleTile;
        private CellPos boardPosition;
        
        /// <summary>
        /// The read-only tile model represented by this view.
        /// </summary>
        public IReadOnlyModuleTile Tile => moduleTile;
        
        /// <summary>
        /// The tile's current board coordinate as known by this view.
        /// </summary>
        public CellPos BoardPosition => boardPosition;

        
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