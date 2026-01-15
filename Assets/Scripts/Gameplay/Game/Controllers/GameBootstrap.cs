using Gameplay.Game.Definitions;
using Gameplay.Game.Services;
using UnityEngine;

namespace Gameplay.Game.Controllers
{
    /// <summary>
    /// Responsible for initializing global game services at application startup.
    ///
    /// <para>
    /// <see cref="GameBootstrap"/> is typically placed in the first scene loaded
    /// by the game. During <see cref="Awake"/> it configures the service layer by
    /// providing the required ScriptableObjects (such as the tile library) to
    /// <see cref="GameServices"/>.
    /// </para>
    ///
    /// <para>
    /// This ensures that all gameplay systems have access to a fully initialized
    /// service environment before they begin executing. Additional service
    /// initialization steps can be added here as the project expands.
    /// </para>
    ///
    /// <para>
    /// Only the Unity/game layer should depend on this component; the engine
    /// layer remains entirely decoupled from Unity and does not reference or
    /// require this bootstrap process.
    /// </para>
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        /// <summary>
        /// The ScriptableObject containing the complete set of tile definitions
        /// used by the game. Assigned through the Unity Inspector.
        ///
        /// <para>
        /// This reference is passed to <see cref="GameServices.Init"/> to create
        /// the tile library service at runtime.
        /// </para>
        /// </summary>
        [SerializeField] private TileLibrarySO tileLibrarySO;
        
        [SerializeField] private BoardController boardControllerPrefab;
        
        /// <summary>
        /// Unity lifecycle method called when the GameObject wakes.
        /// Initializes the global service layer by supplying the tile library
        /// data to <see cref="GameServices"/>.
        ///
        /// <para>
        /// This method must execute before any system attempts to access
        /// <see cref="GameServices.TileLibrary"/>. Ensuring that this component
        /// exists in the initial scene guarantees proper initialization order.
        /// </para>
        /// </summary>
        private void Awake()
        {
            GameServices.Init(tileLibrarySO);
            
            CreateControllerObjects();
        }

        private void CreateControllerObjects()
        {
            Instantiate(boardControllerPrefab);
        }
    }
}