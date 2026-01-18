using System.Collections.Generic;
using Gameplay.Engine.Board;
using Gameplay.Engine.Moves;
using Gameplay.Engine.Tiles;
using Gameplay.Presentation.Tiles;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Visualizes available move destinations for the selected tile.
    /// Shows highlight indicators at each valid destination cell.
    /// </summary>
    public class MovePreviewController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TileSelectionController selectionController = null!;

        [Header("Visual Settings")]
        [SerializeField] private GameObject highlightPrefab = null!;
        [SerializeField] private Transform? highlightParent;
        [SerializeField] private Color highlightColor = new Color(0, 1, 0, 0.3f);

        private readonly List<GameObject> activeHighlights = new();
        private IBoardState? board;

        private void Awake()
        {
            // Create highlight parent if not assigned
            if (highlightParent == null)
            {
                var parentGO = new GameObject("MoveHighlights");
                highlightParent = parentGO.transform;
                highlightParent.SetParent(transform);
            }

            if (selectionController == null)
            {
                Debug.LogError("[MovePreviewController] TileSelectionController not assigned!");
                return;
            }

            selectionController.OnTileSelected += ShowMovePreview;
            selectionController.OnTileDeselected += ClearMovePreview;
        }

        private void OnDestroy()
        {
            if (selectionController != null)
            {
                selectionController.OnTileSelected -= ShowMovePreview;
                selectionController.OnTileDeselected -= ClearMovePreview;
            }

            ClearMovePreview();
        }

        public void Init(IBoardState boardState)
        {
            board = boardState;
        }

        private void ShowMovePreview(TileView tileView)
        {
            ClearMovePreview();

            if (board == null)
            {
                Debug.LogWarning("[MovePreviewController] Board not initialized!");
                return;
            }

            // Get available moves from engine
            var tile = tileView.Tile;
            var currentPos = tileView.BoardPosition;

            // Cast to IMovingTile to access movement behavior
            if (tile is not IMovingTile movingTile)
            {
                Debug.LogWarning($"[MovePreviewController] Tile {tile.TypeKey} is not a moving tile!");
                return;
            }

            // Get the tile's actual movement rules from its behavior
            var movementRules = movingTile.MovementBehavior.MovementRules;
            var context = new MoveContext(board, currentPos, movementRules);
            var moves = movingTile.GetAvailableMoves(context);

            Debug.Log($"[MovePreviewController] Showing {moves.Count} possible moves for {tile.TypeKey}");

            // Create highlight at each destination
            foreach (var move in moves)
            {
                CreateHighlight(move.Destination);
            }
        }

        private void ClearMovePreview()
        {
            foreach (var highlight in activeHighlights)
            {
                if (highlight != null)
                    Destroy(highlight);
            }

            activeHighlights.Clear();
        }

        private void CreateHighlight(CellPos pos)
        {
            if (highlightPrefab == null)
            {
                Debug.LogWarning("[MovePreviewController] Highlight prefab not assigned!");
                return;
            }

            // Instantiate highlight at world position
            var worldPos = BoardToWorld(pos);
            var highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity, highlightParent);

            // Apply color
            var spriteRenderer = highlight.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }

            activeHighlights.Add(highlight);
        }

        // TODO: Get this from BoardPresenter instead of duplicating
        private Vector3 BoardToWorld(CellPos pos)
        {
            // Temporary: assumes 1x1 cell size at origin
            return new Vector3(pos.X, pos.Y, 0);
        }
    }
}
