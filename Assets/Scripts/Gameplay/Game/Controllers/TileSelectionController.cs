using Gameplay.Presentation.Tiles;
using UnityCoreKit.Runtime.Core;
using UnityCoreKit.Runtime.Core.Services;
using UnityCoreKit.Runtime.UserInteractions;
using UnityEngine;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Manages tile selection state in response to user click interactions.
    /// Allows selecting one tile at a time, deselecting via second click.
    /// </summary>
    public class TileSelectionController : MonoBehaviour
    {
        [SerializeField] private DestinationClickHandler? destinationClickHandler;
        
        private TileView? selectedTileView;
        private IUserInteractions? interactions;

        // Events for other systems to react to selection changes
        public event System.Action<TileView>? OnTileSelected;
        public event System.Action? OnTileDeselected;

        private void Awake()
        {
            interactions = CoreServices.Get<IUserInteractions>();
            interactions.Subscribe(this, OnTileClick);
        }

        private void OnDestroy()
        {
            interactions?.UnsubscribeAll(this);
        }

        private void OnTileClick(UserInteractionEvent evt)
        {
            if (evt.Target is not TileView tileView)
                return;

            // If clicking the already-selected tile, deselect it
            if (selectedTileView == tileView)
            {
                DeselectTile();
                return;
            }

            // If we have a selected tile and destination handler, check if this click is a valid move destination
            if (selectedTileView != null && destinationClickHandler != null)
            {
                // Try to move to the clicked tile's position (could be a push)
                bool moveAttempted = destinationClickHandler.TryMoveSelectedTileTo(tileView.BoardPosition);
                
                if (moveAttempted)
                {
                    Debug.Log($"[TileSelectionController] Move attempted to ({tileView.BoardPosition.X}, {tileView.BoardPosition.Y}), not selecting");
                    return; // Don't select if a move was attempted
                }
            }

            // Otherwise, select the clicked tile
            SelectTile(tileView);
        }

        private void SelectTile(TileView tileView)
        {
            // Deselect previous tile if any
            if (selectedTileView != null)
            {
                DeselectTile();
            }

            selectedTileView = tileView;
            Debug.Log($"[TileSelectionController] Selected: {tileView.Tile.TypeKey} at ({tileView.BoardPosition.X}, {tileView.BoardPosition.Y})");

            OnTileSelected?.Invoke(tileView);
        }

        private void DeselectTile()
        {
            if (selectedTileView == null)
                return;

            Debug.Log($"[TileSelectionController] Deselected: {selectedTileView.Tile.TypeKey}");
            selectedTileView = null;

            OnTileDeselected?.Invoke();
        }

        public TileView? GetSelectedTile() => selectedTileView;
        
        public void ClearSelection() => DeselectTile();

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        /// <summary>
        /// Directly selects a tile (for testing only).
        /// In production, selection happens via OnTileClick.
        /// </summary>
        public void SelectTileForTest(TileView tileView)
        {
            SelectTile(tileView);
        }
#endif
    }
}
