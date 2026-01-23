#nullable enable
using Gameplay.Presentation.Board;
using UnityCoreKit.Runtime.Core.UpdateManagers;
using UnityCoreKit.Runtime.Core.UpdateManagers.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Detects mouse clicks on the game board and attempts to execute tile moves to clicked positions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This component completes the movement interaction loop by detecting clicks on both empty
    /// and occupied board cells. It works in conjunction with other movement controllers:
    /// <list type="bullet">
    /// <item><see cref="TileSelectionController"/> - Manages which tile is selected</item>
    /// <item><see cref="MovePreviewController"/> - Shows valid move destinations</item>
    /// <item><see cref="DestinationClickHandler"/> - Validates and executes moves</item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <strong>Interaction Flow:</strong>
    /// <list type="number">
    /// <item>User clicks a tile → TileSelectionController selects it</item>
    /// <item>MovePreviewController highlights valid destinations</item>
    /// <item>User clicks destination → EmptyCellClickDetector converts click to board position</item>
    /// <item>DestinationClickHandler validates and executes the move</item>
    /// <item>If successful, selection clears and highlights disappear</item>
    /// </list>
    /// </para>
    /// 
    /// <para>
    /// <strong>Execution Order:</strong>
    /// Uses <c>[DefaultExecutionOrder(-100)]</c> to run before TileSelectionController,
    /// allowing push moves to occupied cells to be attempted before the click is interpreted
    /// as a tile selection change.
    /// </para>
    /// 
    /// <para>
    /// <strong>Click Detection:</strong>
    /// - Uses Unity's new Input System (<see cref="Mouse.current"/>)
    /// - Converts screen position → world position → board cell position
    /// - Only processes clicks when a tile is already selected
    /// - Ignores clicks on the currently selected tile (allows deselection)
    /// </para>
    /// 
    /// <para>
    /// <strong>UpdateManager Integration:</strong>
    /// Implements <see cref="IUpdateObserver"/> and <see cref="ILateUpdateObserver"/>
    /// for centralized update management via UnityCoreKit's UpdateManager system.
    /// </para>
    /// </remarks>
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
            // Reset frame flag at end of frame
            moveAttemptedThisFrame = false;
        }

        /// <summary>
        /// Called each frame to detect mouse clicks.
        /// </summary>
        /// <remarks>
        /// Uses Unity's new Input System to detect left mouse button presses and
        /// delegates to <see cref="HandleClick"/> for processing.
        /// </remarks>
        public void ObservedUpdate()
        {
            // Detect mouse click using new Input System
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                HandleClick(mouse.position.ReadValue());
            }
        }

        /// <summary>
        /// Processes a mouse click, converting screen position to board coordinates
        /// and attempting to move the selected tile to the clicked position.
        /// </summary>
        /// <param name="screenPosition">Screen position of the click in pixels.</param>
        /// <remarks>
        /// <para>
        /// <strong>Click Processing Steps:</strong>
        /// <list type="number">
        /// <item>Checks if a tile is selected (exits if none)</item>
        /// <item>Converts screen position to world position via camera</item>
        /// <item>Converts world position to board cell position</item>
        /// <item>Validates click is within board bounds</item>
        /// <item>Allows deselection if clicking the selected tile</item>
        /// <item>Calls <see cref="DestinationClickHandler"/> to attempt move</item>
        /// </list>
        /// </para>
        /// <para>
        /// <strong>Supported Move Types:</strong>
        /// - Simple moves to empty cells
        /// - Push moves to occupied cells (if tile has PushObstacles rule)
        /// - Chain pushes (pushing multiple tiles in a row)
        /// </para>
        /// <para>
        /// The <see cref="DestinationClickHandler"/> validates whether the move is legal
        /// based on the selected tile's movement rules before executing.
        /// </para>
        /// </remarks>
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
