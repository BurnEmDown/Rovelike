using System.Collections.Generic;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Game.Definitions;
using Gameplay.Game.Services;
using Gameplay.Presentation.Board;
using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;
using Logger = UnityCoreKit.Runtime.Core.Utils.Logs.Logger;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Orchestrates board initialization for the Unity/game layer.
    ///
    /// <para>
    /// Responsibilities:
    /// </para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///         Retrieves <see cref="TileDefinition"/> assets from the global tile library
    ///         (<see cref="GameServices.TileLibrary"/>).
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///         Creates engine-level <see cref="ModuleTile"/> instances via <see cref="TileFactory"/>.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///         Creates a new engine-level <see cref="BoardState"/> and places the created tiles
    ///         on random unique positions within the board bounds.
    ///         </description>
    ///     </item>
    /// </list>
    ///
    /// <para>
    /// This controller currently performs a "smoke test" style initialization:
    /// it creates <c>width * height</c> tiles and fills the board completely.
    /// Later this can be expanded to support level setups, objectives, drafting,
    /// deterministic seeding, undo/redo, and visual spawning (TileView).
    /// </para>
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        /// <summary>
        /// Board width (number of columns). Corresponds to the X-axis size in <see cref="BoardState"/>.
        /// </summary>
        [SerializeField] private int width = 8;

        /// <summary>
        /// Board height (number of rows). Corresponds to the Y-axis size in <see cref="BoardState"/>.
        /// </summary>
        [SerializeField] private int height = 8;

        [Header("Initial Tile Layout")]
        [Tooltip("Number of columns for initial tile placement.")]
        [SerializeField] private int initialTileColumns = 3;

        [Tooltip("Number of rows for initial tile placement.")]
        [SerializeField] private int initialTileRows = 2;

        [Header("Presentation")]
        [SerializeField] private BoardPresenter boardPresenter;
        
        [Header("Controllers")]
        [SerializeField] private TileSelectionController selectionController = null!;
        [SerializeField] private MovePreviewController movePreviewController = null!;
        [SerializeField] private DestinationClickHandler destinationClickHandler = null!;
        
        /// <summary>
        /// Currently unused field. Consider removing or repurposing.
        /// If intended, it may later hold cached definitions or a direct reference to a TileLibrarySO.
        /// </summary>
        private TileDefinition[] tileLibrarySO;

        private MoveExecutor? moveExecutor;

        /// <summary>
        /// The engine-level board state backing this controller.
        /// Stores tile placements and provides lookup utilities.
        /// </summary>
        private BoardState board;
        
        /// <summary>
        /// The list of engine-level tile instances created during initialization.
        /// These are placed onto <see cref="board"/> in <see cref="CreateAndFillBoard"/>.
        /// </summary>
        private List<ModuleTile> tiles = new();

        /// <summary>
        /// Creates the tile instances early so that the list is ready by <see cref="Start"/>.
        /// This assumes <see cref="GameServices.Init"/> was already called by a bootstrap component
        /// (e.g., <c>GameBootstrap</c>) earlier in the scene initialization order.
        /// </summary>
        private void Awake()
        {
            CreateTiles();
            boardPresenter.Init(CoreServices.Get<IPoolManager>());
        }

        /// <summary>
        /// Creates a new <see cref="BoardState"/> and places the previously created tiles
        /// into random unique positions on the board.
        /// Also initializes all game systems (movement, selection, preview).
        /// </summary>
        private void Start()
        {
            CreateAndFillBoard();
            boardPresenter.Rebuild(board);
            InitializeGameSystems();
        }
        
        /// <summary>
        /// Initializes all game systems after board creation.
        /// </summary>
        private void InitializeGameSystems()
        {
            // Initialize move preview
            movePreviewController.Init(board);

            // Create move executor
            moveExecutor = new MoveExecutor(board, boardPresenter);

            // Initialize destination handler
            destinationClickHandler.Init(board, moveExecutor);

            Debug.Log("[BoardController] All game systems initialized");
        }
        
        /// <summary>
        /// Creates <c>width * height</c> engine-level <see cref="ModuleTile"/> instances from
        /// the tile definitions returned by <see cref="GameServices.TileLibrary"/>.
        ///
        /// <para>
        /// The current implementation selects the first N definitions in the tile library.
        /// This is deterministic but not randomized. Future variants can randomize, filter,
        /// or draft definitions based on level configuration.
        /// </para>
        /// </summary>
        private void CreateTiles()
        {
            // Retrieve all available tile definitions from the global tile library service.
            TileDefinition[] defs = GameServices.TileLibrary.GetAllTiles();

            // Target number of tiles equals the initial tile layout size, not the full board.
            int tilesToCreate = initialTileRows * initialTileColumns;

            // Guard against missing or empty tile library data.
            if (defs == null || defs.Length == 0)
            {
                Logger.LogError("No TileDefinitions found in TileLibrary.");
                return;
            }

            // If the library contains fewer definitions than requested, create as many as possible.
            if (defs.Length < tilesToCreate)
            {
                Logger.LogWarning(
                    $"Requested {tilesToCreate} tiles, but library only has {defs.Length}. " +
                    "Using all available.");
                tilesToCreate = defs.Length;
            }

            // Clear previous tiles if this method is ever called again.
            tiles.Clear();

            // Create tiles from the first N definitions.
            // Note: This does not currently enforce unique typeKeys beyond the assumption that the
            // library definitions are distinct. If needed, add validation/deduping later.
            for (int i = 0; i < tilesToCreate; i++)
            {
                var def = defs[i];

                // Convert Unity definition data into an engine tile instance via the Unity TileFactory.
                ModuleTile tile = TileFactory.CreateTile(def);
                tiles.Add(tile);

                Logger.Log($"[CreateTiles] Created tile Id={tile.Id}, TypeKey={tile.TypeKey}");
            }
        }
        
        /// <summary>
        /// Creates a new <see cref="BoardState"/> and fills it by placing each tile from
        /// <see cref="tiles"/> into a grid formation centered on the board.
        ///
        /// <para>
        /// Places tiles in an initialTileColumns x initialTileRows grid centered within
        /// the larger board. This leaves empty cells around the initial tile formation,
        /// allowing tiles to move into empty spaces.
        /// </para>
        /// </summary>
        private void CreateAndFillBoard()
        {
            // Create engine board state.
            board = new BoardState(width, height);

            // Calculate offset to center the initial tile grid on the board
            int offsetX = (width - initialTileColumns) / 2;
            int offsetY = (height - initialTileRows) / 2;

            // Build a list of positions for the initial tile grid (centered)
            var positions = new List<CellPos>();
            for (int y = 0; y < initialTileRows; y++)
            {
                for (int x = 0; x < initialTileColumns; x++)
                {
                    positions.Add(new CellPos { X = x + offsetX, Y = y + offsetY });
                }
            }

            // Shuffle positions (Fisher–Yates) so placement is random.
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (positions[i], positions[j]) = (positions[j], positions[i]);
            }

            // Place each tile on a distinct random position.
            // Assumes tiles.Count <= positions.Count (true if tiles were created with width*height).
            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                var pos = positions[i];

                bool placed = board.TryPlaceTile(pos, tile);
                if (!placed)
                {
                    // Placement failures should not happen with the current algorithm unless:
                    // - positions contain duplicates (they don't), or
                    // - tiles exceed board capacity, or
                    // - board bounds mismatch the intended width/height.
                    Logger.LogError(
                        $"[CreateAndFillBoard] Failed to place tile {tile.TypeKey} at ({pos.X}, {pos.Y})");
                }
                else
                {
                    Logger.Log(
                        $"[CreateAndFillBoard] Placed tile {tile.TypeKey} at ({pos.X}, {pos.Y})");
                }
            }
        }
    }
}