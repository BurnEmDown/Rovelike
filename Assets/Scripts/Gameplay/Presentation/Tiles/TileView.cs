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
    /// 
    /// <para>
    /// Implements <see cref="IUserInteractionTarget"/> to participate in the application's
    /// event-based interaction system. When clicked, the attached 
    /// <see cref="PointerClickUserInteractionSource"/> emits a <see cref="UserInteractionEvent"/>
    /// with this TileView as the target.
    /// </para>
    /// 
    /// <para>
    /// Architecture Note: TileView manages its own interaction initialization in <see cref="OnEnable"/>
    /// using an Inspector-assigned reference to avoid expensive GetComponent calls. This follows
    /// best practices for pooled objects that are frequently enabled/disabled.
    /// </para>
    /// </summary>
    public class TileView : MonoBehaviour, IUserInteractionTarget
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Header("Selection Visual")]
        [Tooltip("Child GameObject that displays when this tile is selected (e.g., outline or glow effect).")]
        [SerializeField] private GameObject selectionIndicator;
        
        [Tooltip("Interaction source component that converts Unity click events to UserInteractionEvents. Assigned in Inspector.")]
        [SerializeField] private PointerClickUserInteractionSource interactionSource;
        
        private IReadOnlyModuleTile moduleTile;
        private CellPos boardPosition;

        /// <summary>
        /// IUserInteractionTarget key identifying this as a tile interaction target.
        /// Used by event handlers to filter interaction types.
        /// </summary>
        public string InteractionKey => "Tile";
        
        /// <summary>
        /// The underlying engine model represented by this view.
        /// Event handlers can cast this to <see cref="IReadOnlyModuleTile"/> for logic.
        /// </summary>
        public object Model => moduleTile;
        
        /// <summary>
        /// The read-only tile model represented by this view.
        /// </summary>
        public IReadOnlyModuleTile Tile => moduleTile;
        
        /// <summary>
        /// The tile's current board coordinate as known by this view.
        /// </summary>
        public CellPos BoardPosition => boardPosition;

        /// <summary>
        /// Initializes the interaction source when this tile view is enabled (spawned from pool).
        /// </summary>
        /// <remarks>
        /// Design Decision: Self-Initialization Pattern
        /// TileView initializes its own interaction source rather than having external code
        /// (like BoardPresenter) manage it. This approach:
        /// - Eliminates expensive GetComponent calls (uses Inspector-assigned reference)
        /// - Follows single responsibility principle (TileView owns its interaction setup)
        /// - Automatically handles pooling (re-initializes each time enabled)
        /// - Keeps BoardPresenter decoupled from the interaction system
        /// 
        /// The interaction source must be re-initialized each time the view is enabled because:
        /// - Service references may have changed
        /// - Target binding must be refreshed for the pooled instance
        /// </remarks>
        private void OnEnable()
        {
            // Initialize interaction source when tile view is enabled (spawned from pool)
            if (interactionSource != null)
            {
                var interactions = CoreServices.Get<IUserInteractions>();
                interactionSource.Init(interactions, this);
            }
        }

        /// <summary>
        /// Cleans up references when this tile view is disabled (returned to pool).
        /// </summary>
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
        
        /// <summary>
        /// Shows visual indication that this tile is selected.
        /// </summary>
        /// <remarks>
        /// Activates the selection indicator GameObject (typically an outline or glow effect).
        /// Called by <see cref="TileSelectionController"/> when this tile becomes selected.
        /// </remarks>
        public void ShowSelection()
        {
            if (selectionIndicator != null)
                selectionIndicator.SetActive(true);
        }

        /// <summary>
        /// Hides the selection visual indication.
        /// </summary>
        /// <remarks>
        /// Deactivates the selection indicator GameObject.
        /// Called by <see cref="TileSelectionController"/> when this tile is deselected
        /// or when another tile is selected.
        /// </remarks>
        public void HideSelection()
        {
            if (selectionIndicator != null)
                selectionIndicator.SetActive(false);
        }
    }
}