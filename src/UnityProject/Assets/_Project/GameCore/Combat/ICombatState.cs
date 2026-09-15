namespace Game.Core.Combat
{
    /// <summary>
    /// Implémentation explicite par classes d'état (ARCHITECTURE.md §6) — délibérément
    /// pas un switch sur un enum qui grossirait à chaque nouvel effet de combat. Chaque
    /// état sait ce qu'il doit faire en entrant (déclencher la logique de tour, vérifier
    /// victoire/défaite...) ; les phases purement "en attente d'input"
    /// (ActionSelection, MiniGame) n'ont pas de logique d'entrée.
    /// </summary>
    public interface ICombatState
    {
        CombatPhase Phase { get; }
        void Enter(CombatStateMachine machine);
    }
}
