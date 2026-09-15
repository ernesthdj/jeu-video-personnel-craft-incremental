using System;

namespace Game.Core.GameState
{
    /// <summary>
    /// États minimaux pour la vertical slice (Boot, Hub, Crafting, Combat —
    /// ARCHITECTURE.md §3). Un futur système (raffinage, marché...) ajoute un état sans
    /// toucher aux existants.
    /// </summary>
    public sealed class SimpleGameState : IGameState
    {
        public string Name { get; }
        private readonly Action? _onEnter;
        private readonly Action? _onExit;

        public SimpleGameState(string name, Action? onEnter = null, Action? onExit = null)
        {
            Name = name;
            _onEnter = onEnter;
            _onExit = onExit;
        }

        public void Enter() => _onEnter?.Invoke();
        public void Exit() => _onExit?.Invoke();
    }
}
