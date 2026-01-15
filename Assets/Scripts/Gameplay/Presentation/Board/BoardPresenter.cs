#nullable enable
using System.Collections.Generic;
using Gameplay.Engine.Board;
using Gameplay.Engine.Tiles;
using Gameplay.Game.Services;
using Gameplay.Presentation.Tiles;
using UnityCoreKit.Runtime.Core.Interfaces;
using UnityCoreKit.Runtime.Core.Services;
using UnityCoreKit.Runtime.UserInteractions;
using UnityCoreKit.Runtime.UserInteractions.Unity;
using UnityEngine;
using static Gameplay.Engine.Board.Structs;
using Logger = UnityCoreKit.Runtime.Core.Utils.Logs.Logger;

namespace Gameplay.Presentation.Board
{
    /// <summary>
    /// Presents an engine-level board (<see cref="IBoardState"/>) in Unity by spawning and managing
    /// pooled <see cref="TileView"/> instances.
    ///
    /// <para>
    /// This presenter is strictly view-layer: it does not apply engine rules and does not modify the board state.
    /// It creates/reuses Unity objects (TileView) to represent engine tiles at their board coordinates.
    /// </para>
    /// </summary>
    public class BoardPresenter : MonoBehaviour
    {
        [Header("Pooling")]
        [Tooltip("Pool key used by PoolManager to spawn TileView instances.")]
        [SerializeField] private string tileViewPoolName = "TileView";

        [Header("Hierarchy")]
        [SerializeField] private Transform tileRoot = null!;

        [Header("Layout")]
        [Tooltip("World-space position of board cell (0,0).")]
        [SerializeField] private Vector3 origin = Vector3.zero;

        [Tooltip("Spacing between board cells in world units.")]
        [SerializeField] private Vector2 cellSize = Vector2.one;

        private readonly Dictionary<CellPos, TileView> viewsByPos = new();

        private IPoolManager? poolManager;

        private void Awake()
        {
            if (tileRoot == null)
                tileRoot = transform;
        }

        /// <summary>
        /// Initializes this presenter with the pool manager to use for spawning and returning tile views.
        /// Must be called before <see cref="Rebuild"/>.
        /// </summary>
        public void Init(IPoolManager poolManager)
        {
            this.poolManager = poolManager;
        }

        /// <summary>
        /// Rebuilds the full board presentation from scratch using pooled tile views.
        /// </summary>
        public void Rebuild(IBoardState board)
        {
            if (poolManager == null)
            {
                Debug.LogError("BoardPresenter.Rebuild called before Init(poolManager).");
                return;
            }

            ClearViews();

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var pos = new CellPos { X = x, Y = y };
                    var tile = board.GetTileAt(pos);
                    if (tile == null)
                        continue;

                    // Assumes board tiles are ModuleTile-derived in your current game.
                    SpawnTileView((IReadOnlyModuleTile)tile, pos);
                }
            }
        }

        /// <summary>
        /// Returns all active tile views back to the pool and clears internal mappings.
        /// </summary>
        public void ClearViews()
        {
            if (poolManager == null)
                return;

            foreach (var kvp in viewsByPos)
            {
                var view = kvp.Value;
                if (view == null) continue;
                
                poolManager.ReturnToPool(tileViewPoolName, view);
            }

            viewsByPos.Clear();
        }

        /// <summary>
        /// Updates a single view mapping and moves the view in world space.
        /// </summary>
        public void MoveView(CellPos from, CellPos to)
        {
            if (!viewsByPos.TryGetValue(from, out var view) || view == null)
                return;

            viewsByPos.Remove(from);
            viewsByPos[to] = view;

            view.SetBoardPosition(to);
            view.transform.position = BoardToWorld(to);
        }

        /// <summary>
        /// Converts board coordinates to world coordinates.
        /// </summary>
        public Vector3 BoardToWorld(CellPos pos)
        {
            return origin + new Vector3(pos.X * cellSize.x, pos.Y * cellSize.y, 0f);
        }

        private void SpawnTileView(IReadOnlyModuleTile tile, CellPos pos)
        {
            poolManager!.GetFromPool<TileView>(
                tileViewPoolName,
                tileRoot.gameObject,
                view =>
                {
                    if (view == null)
                    {
                        Debug.LogError($"BoardPresenter: Failed to get TileView from pool '{tileViewPoolName}'.");
                        return;
                    }

                    // Parent + placement
                    view.transform.SetParent(tileRoot, false);
                    view.transform.position = BoardToWorld(pos);
                    view.transform.rotation = Quaternion.identity;

                    // Init binds read-only engine tile reference and board position
                    view.Init(tile, pos);

                    // Apply visuals from definitions (Unity-side)
                    var def = GameServices.TileLibrary.GetTileByTypeKey(tile.TypeKey);
                    if (def != null)
                    {
                        view.SetVisuals(def);
                    }

                    viewsByPos[pos] = view;
                });
        }
    }
}