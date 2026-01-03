using System.Collections.Generic;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Game.Definitions;
using Gameplay.Game.Services;
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
        [SerializeField] private int width = 3;

        /// <summary>
        /// Board height (number of rows). Corresponds to the Y-axis size in <see cref="BoardState"/>.
        /// </summary>
        [SerializeField] private int height = 2;

        /// <summary>
        /// Currently unused field. Consider removing or repurposing.
        /// If intended, it may later hold cached definitions or a direct reference to a TileLibrarySO.
        /// </summary>
        private TileDefinition[] tileLibrarySO;

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
        /// Unity lifecycle method called when the object awakens.
        ///
        /// <para>
        /// Creates the tile instances early so that the list is ready by <see cref="Start"/>.
        /// This assumes <see cref="GameServices.Init"/> was already called by a bootstrap component
        /// (e.g., <c>GameBootstrap</c>) earlier in the scene initialization order.
        /// </para>
        /// </summary>
        private void Awake()
        {
            CreateTiles();
        }

        /// <summary>
        /// Unity lifecycle method called on the first frame.
        ///
        /// <para>
        /// Creates a new <see cref="BoardState"/> and places the previously created tiles
        /// into random unique positions on the board.
        /// </para>
        /// </summary>
        private void Start()
        {
            CreateAndFillBoard();
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

            // Target number of tiles equals the number of cells on the board.
            int tilesToCreate = height * width;

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
        /// <see cref="tiles"/> into a random unique cell.
        ///
        /// <para>
        /// Uses a Fisher–Yates shuffle of all available board positions and assigns one
        /// position per tile. This ensures no collisions as long as the number of tiles
        /// does not exceed the number of cells.
        /// </para>
        /// </summary>
        private void CreateAndFillBoard()
        {
            // Create engine board state.
            board = new BoardState(width, height);

            // Build a list of all board positions.
            var positions = new List<CellPos>();
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    positions.Add(new CellPos { X = x, Y = y });
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