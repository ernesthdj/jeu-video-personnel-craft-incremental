namespace Game.Core.GameState
{
    /// <summary>Pattern State classique (ARCHITECTURE.md §6) pour l'état applicatif global.</summary>
    public interface IGameState
    {
        string Name { get; }
        void Enter();
        void Exit();
    }
}
