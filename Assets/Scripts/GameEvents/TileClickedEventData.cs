using UnityEngine;
using static Gameplay.Engine.Board.Structs;

namespace GameEvents
{
    public class TileClickedEventData
    {
        public CellPos TilePosition { get; }
        public Vector3 WorldPosition { get; }
        public string TypeKey { get; }

        public TileClickedEventData(CellPos tilePosition, Vector3 worldPosition, string typeKey)
        {
            TilePosition = tilePosition;
            WorldPosition = worldPosition;
            TypeKey = typeKey;
        }
    }
}
