#nullable enable
using UnityEngine;

namespace Gameplay.Utilities
{
    /// <summary>
    /// Utility class for converting between world coordinates and board cell positions.
    /// Uses Unity's Vector2Int for grid positions to avoid external dependencies.
    /// </summary>
    public static class BoardCoordinateUtility
    {
        /// <summary>s
        /// Converts world position to board cell position.
        /// </summary>
        /// <param name="worldPosition">Position in world space</param>
        /// <param name="origin">World position of board cell (0,0)</param>
        /// <param name="cellSize">Size of each board cell in world units</param>
        /// <returns>Board cell position as Vector2Int</returns>
        public static Vector2Int WorldToBoardPos(Vector3 worldPosition, Vector3 origin, Vector2 cellSize)
        {
            var localPos = worldPosition - origin;
            int x = Mathf.RoundToInt(localPos.x / cellSize.x);
            int y = Mathf.RoundToInt(localPos.y / cellSize.y);
            
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Converts board cell position to world position (center of cell).
        /// </summary>
        /// <param name="boardPos">Board cell position as Vector2Int</param>
        /// <param name="origin">World position of board cell (0,0)</param>
        /// <param name="cellSize">Size of each board cell in world units</param>
        /// <returns>World position at center of cell</returns>
        public static Vector3 BoardToWorldPos(Vector2Int boardPos, Vector3 origin, Vector2 cellSize)
        {
            return origin + new Vector3(
                boardPos.x * cellSize.x,
                boardPos.y * cellSize.y,
                0
            );
        }

        /// <summary>
        /// Converts screen position to world position using the main camera.
        /// </summary>
        /// <param name="screenPosition">Screen space position (X, Y coordinates in pixels).</param>
        /// <returns>World position at Z=0 plane, or Vector3.zero if no main camera exists.</returns>
        /// <remarks>
        /// <para>
        /// For 2D orthographic cameras, <see cref="Camera.ScreenToWorldPoint"/> requires a Z coordinate
        /// to determine the depth plane in world space. This method automatically sets the Z coordinate
        /// to the negative of the camera's Z position, which places the resulting point at Z=0 in world space
        /// (where 2D game boards typically reside).
        /// </para>
        /// <para>
        /// <strong>Example:</strong> If the camera is at position (0, 0, -10), this method sets screenPosition.z = 10,
        /// resulting in a world position at Z=0.
        /// </para>
        /// <para>
        /// <strong>Use Case:</strong> Converting mouse clicks or touch input to board positions in 2D games.
        /// </para>
        /// </remarks>
        public static Vector3 ScreenToWorldPos(Vector3 screenPosition)
        {
            var camera = Camera.main;
            if (camera == null)
                return Vector3.zero;
                
            // For 2D orthographic camera, set Z to the negative of camera's Z position
            // This places the point at Z=0 in world space
            Vector3 screenPosWithZ = new Vector3(screenPosition.x, screenPosition.y, -camera.transform.position.z);
            return camera.ScreenToWorldPoint(screenPosWithZ);
        }
    }
}
