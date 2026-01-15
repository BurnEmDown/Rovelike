using UnityCoreKit.Runtime.Core.Events;

namespace GameEvents
{
    public class GameEventType : EventType<GameEventType.GameEventEnum>
    {
        public enum GameEventEnum
        {
            TileClicked,
            // Add more game events as needed
        }

        // Static instance for convenience
        public static readonly GameEventType TileClicked =
            new GameEventType(GameEventEnum.TileClicked);

        // Constructor
        public GameEventType(GameEventEnum value) : base(value) { }
    }
}
