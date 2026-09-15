using System;

namespace Game.Core.GameState
{
    /// <summary>
    /// État applicatif global (Boot -> Hub -> Crafting/Combat -> retour Hub,
    /// ARCHITECTURE.md §6). Pas de framework externe — un pattern State minimal suffit au
    /// besoin (règle YAGNI, ARCHITECTURE.md §8).
    /// </summary>
    public sealed class GameStateMachine
    {
        public IGameState? Current { get; private set; }

        public void TransitionTo(IGameState nextState)
        {
            if (nextState is null) throw new ArgumentNullException(nameof(nextState));

            Current?.Exit();
            Current = nextState;
            Current.Enter();
        }
    }
}
