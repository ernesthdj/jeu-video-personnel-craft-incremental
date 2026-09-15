using Game.Core.Common;

namespace Game.Core.Combat
{
    public enum CombatDecisionType
    {
        Move,
        Attack,
        EndTurn,
    }

    /// <summary>Décision retournée par un <see cref="IEnemyAI"/> pour le tour en cours.</summary>
    public sealed class CombatDecision
    {
        public CombatDecisionType Type { get; init; }
        public GridPosition? TargetPosition { get; init; }
        public ICombatant? TargetCombatant { get; init; }

        public static CombatDecision Move(GridPosition destination) =>
            new() { Type = CombatDecisionType.Move, TargetPosition = destination };

        public static CombatDecision Attack(ICombatant target) =>
            new() { Type = CombatDecisionType.Attack, TargetCombatant = target };

        public static CombatDecision EndTurn() => new() { Type = CombatDecisionType.EndTurn };
    }
}
