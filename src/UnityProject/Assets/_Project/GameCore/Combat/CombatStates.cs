namespace Game.Core.Combat
{
    /// <summary>
    /// États concrets de la FSM de combat. Regroupés dans un seul fichier par pragmatisme
    /// (chaque classe est minuscule, une par phase) — voir CombatStateMachine.cs pour
    /// l'orchestration et la logique métier de chaque transition.
    /// </summary>
    public sealed class TurnStartState : ICombatState
    {
        public CombatPhase Phase => CombatPhase.TurnStart;
        public void Enter(CombatStateMachine machine) => machine.HandleTurnStartEntered();
    }

    public sealed class ActionSelectionState : ICombatState
    {
        public CombatPhase Phase => CombatPhase.ActionSelection;
        public void Enter(CombatStateMachine machine)
        {
            // Attend une décision (SubmitMove/SubmitAttack/EndTurn) — aucune logique d'entrée.
        }
    }

    public sealed class MiniGameState : ICombatState
    {
        public CombatPhase Phase => CombatPhase.MiniGame;
        public void Enter(CombatStateMachine machine)
        {
            // Attend le résultat du mini-jeu (ResolveMiniGame) — aucune logique d'entrée.
        }
    }

    public sealed class ResolutionState : ICombatState
    {
        public CombatPhase Phase => CombatPhase.Resolution;
        public void Enter(CombatStateMachine machine) => machine.HandleResolutionEntered();
    }

    public sealed class TurnEndState : ICombatState
    {
        public CombatPhase Phase => CombatPhase.TurnEnd;
        public void Enter(CombatStateMachine machine) => machine.HandleTurnEndEntered();
    }

    public sealed class EndedState : ICombatState
    {
        public CombatPhase Phase => CombatPhase.Ended;
        public void Enter(CombatStateMachine machine)
        {
            // État terminal — plus aucune action possible.
        }
    }
}
