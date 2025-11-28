using System.Collections.Generic;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Game.Definitions;
using Gameplay.Game.Services;
using UnityEngine;
using static Gameplay.Engine.Board.Structs; // for CellPos
using Logger = UnityCoreKit.Runtime.Core.Utils.Logs.Logger;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Test controller that:
    /// 1. Creates 6 ModuleTile instances from the TileLibrary.
    /// 2. Places them randomly on a 2x3 board (2 rows, 3 columns).
    /// 3. Prints each tile's (x, y) as stored in BoardState.
    /// 4. Prints a text-based visual representation of the board.
    /// </summary>
    public class TileBoardPlacementTestController : MonoBehaviour
    {
        [SerializeField] private int tilesToCreate = 6;

        private readonly List<ModuleTile> createdTiles = new();
        private BoardState board;

        private void Start()
        {
            CreateTiles();
            CreateAndFillBoard();
            LogBoardContents();
        }

        private void CreateTiles()
        {
            TileDefinition[] defs = GameServices.TileLibrary.GetAllTiles();

            if (defs == null || defs.Length == 0)
            {
                Logger.LogError("No TileDefinitions found in TileLibrary.");
                return;
            }

            if (defs.Length < tilesToCreate)
            {
                Logger.LogWarning(
                    $"Requested {tilesToCreate} tiles, but library only has {defs.Length}. " +
                    "Using all available.");
                tilesToCreate = defs.Length;
            }

            createdTiles.Clear();

            // For deterministic testing you could sort or pick specific ones;
            // for now we just take the first N.
            for (int i = 0; i < tilesToCreate; i++)
            {
                var def = defs[i];
                ModuleTile tile = TileFactory.CreateTile(def);
                createdTiles.Add(tile);

                Logger.Log($"[CreateTiles] Created tile Id={tile.Id}, TypeKey={tile.TypeKey}");
            }
        }

        private void CreateAndFillBoard()
        {
            // 2 rows, 3 columns → width = 3 (X), height = 2 (Y)
            board = new BoardState(width: 3, height: 2);

            // Build list of all positions on the board
            var positions = new List<CellPos>();
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    positions.Add(new CellPos { X = x, Y = y });
                }
            }

            // Shuffle positions (Fisher–Yates)
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (positions[i], positions[j]) = (positions[j], positions[i]);
            }

            // Place each tile on a distinct random position
            for (int i = 0; i < createdTiles.Count; i++)
            {
                var tile = createdTiles[i];
                var pos = positions[i];

                bool placed = board.TryPlaceTile(pos, tile);
                if (!placed)
                {
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

        private void LogBoardContents()
        {
            Logger.Log("=== Board Contents (from BoardState) ===");

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var pos = new CellPos { X = x, Y = y };
                    var tile = board.GetTileAt(pos);

                    if (tile != null)
                    {
                        Logger.Log(
                            $"Tile {tile.TypeKey} at ({pos.X}, {pos.Y}) [Id={tile.Id}]");
                    }
                    else
                    {
                        Logger.Log(
                            $"Empty cell at ({pos.X}, {pos.Y})");
                    }
                }
            }

            Logger.Log("=== End Board Contents ===");

            // ▼▼ Added: ASCII-style board print ▼▼
            PrintBoardAscii();
        }

        /// <summary>
        /// Prints a simple text-based representation of the board using initials:
        /// B, C, G, L, M, S for known module types, '.' for empty.
        /// </summary>
        private void PrintBoardAscii()
        {
            Logger.Log("=== Board ASCII View ===");

            // Print from top row (highest Y) down to bottom (Y = 0)
            for (int y = board.Height - 1; y >= 0; y--)
            {
                string line = "";
                for (int x = 0; x < board.Width; x++)
                {
                    var pos = new CellPos { X = x, Y = y };
                    var tile = board.GetTileAt(pos) as ModuleTile;

                    char symbol = GetTileSymbol(tile);
                    line += symbol + " ";
                }

                Logger.Log(line.TrimEnd());
            }

            Logger.Log("=== End ASCII View ===");
        }

        /// <summary>
        /// Maps a ModuleTile's TypeKey to a single-character symbol.
        /// Known mappings:
        /// Brain - B, Coil - C, Gripper - G, Laser - L, Motor - M, Sensor - S.
        /// Unknown types fall back to the first character of TypeKey, or '?'.
        /// Empty cells return '.'.
        /// </summary>
        private char GetTileSymbol(ModuleTile tile)
        {
            if (tile == null)
                return '.';

            switch (tile.TypeKey)
            {
                case "Brain":   return 'B';
                case "Coil":    return 'C';
                case "Gripper": return 'G';
                case "Laser":   return 'L';
                case "Motor":   return 'M';
                case "Sensor":  return 'S';
                default:
                    // Fallback: first letter of TypeKey, or '?' if empty
                    return !string.IsNullOrEmpty(tile.TypeKey)
                        ? tile.TypeKey[0]
                        : '?';
            }
        }
    }
}