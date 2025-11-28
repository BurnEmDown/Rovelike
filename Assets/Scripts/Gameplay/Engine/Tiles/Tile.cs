

namespace Gameplay.Engine.Tiles
{
    public abstract class Tile
    {
        /// <summary>
        /// Unique runtime identifier for this tile instance.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Stable key for tile type, e.g. "JumpJet", "Grapple".
        /// </summary>
        public string TypeKey { get; }
        
        public Tile(int id, string typeKey)
        {
            Id = id;
            TypeKey = typeKey;
        }
    }
}