namespace Game.Core.Combat
{
    /// <summary>Résultat d'une action de combat résolue, transmis à l'UI via GameEventBus.</summary>
    public sealed class CombatActionResult
    {
        public string AttackerId { get; init; } = string.Empty;
        public string TargetId { get; init; } = string.Empty;
        public bool Hit { get; init; }
        public bool Critical { get; init; }
        public int DamageDealt { get; init; }
        public bool TargetDefeated { get; init; }
        public float Score01 { get; init; }
    }
}
