#nullable enable
using Gameplay.Presentation.Board;
using UnityCoreKit.Runtime.Core.UpdateManagers;
using UnityCoreKit.Runtime.Core.UpdateManagers.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Detects clicks on board cells (empty or occupied) and attempts to execute moves.
    /// Runs before TileSelectionController to handle push moves to occupied cells.
    /// </summary>
    [DefaultExecutionOrder(-100)] // Execute before TileSelectionController
    public class EmptyCellClickDetector : MonoBehaviour, IUpdateObserver, ILateUpdateObserver
    {
        [SerializeField] private BoardPresenter boardPresenter = null!;
        [SerializeField] private DestinationClickHandler destinationClickHandler = null!;
        [SerializeField] private TileSelectionController selectionController = null!;

        private Camera? mainCamera;
        private bool moveAttemptedThisFrame;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            UpdateManager.RegisterObserver(this);
            LateUpdateManager.RegisterObserver(this);
        }

        private void OnDisable()
        {
            UpdateManager.UnregisterObserver(this);
            LateUpdateManager.UnregisterObserver(this);
        }

        public void ObservedLateUpdate()
        {
            // Reset flag at end of frame
            moveAttemptedThisFrame = false;
        }

        public void ObservedUpdate()
        {
            // Detect mouse click using new Input System
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                HandleClick(mouse.position.ReadValue());
            }
        }

        private void HandleClick(Vector2 screenPosition)
        {
            // Only process if we have a selected tile
            var selectedTile = selectionController.GetSelectedTile();
            if (selectedTile == null)
            {
                Debug.Log("[EmptyCellClickDetector] No tile selected, ignoring click");
                return;
            }

            // Convert screen position to board position first
            var worldPos = mainCamera!.ScreenToWorldPoint(screenPosition);
            var boardPos = boardPresenter.WorldToBoardPos(worldPos);
            
            if (!boardPos.HasValue)
            {
                Debug.Log("[EmptyCellClickDetector] Click outside board bounds");
                return;
            }

            Debug.Log($"[EmptyCellClickDetector] Click at board=({boardPos.Value.X}, {boardPos.Value.Y})");

            // Check if clicking the selected tile (deselect)
            // Check if clicking the selected tile (deselect)
            if (boardPos.Value.X == selectedTile.BoardPosition.X && 
                boardPos.Value.Y == selectedTile.BoardPosition.Y)
            {
                Debug.Log("[EmptyCellClickDetector] Clicked selected tile, letting TileSelectionController handle deselect");
                return; // Let TileSelectionController handle deselection
            }

            // Try to move to this destination (empty or occupied with push)
            Debug.Log($"[EmptyCellClickDetector] Attempting move to ({boardPos.Value.X}, {boardPos.Value.Y})");
            destinationClickHandler.TryMoveSelectedTileTo(boardPos.Value);
        }
    }
}
