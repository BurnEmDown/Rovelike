namespace Gameplay.Engine.Tiles
{
    public interface IReadOnlyModuleTile
    {
        int Id { get; }
        string TypeKey { get; }
        bool IsAbilityAvailable { get; }
    }
}