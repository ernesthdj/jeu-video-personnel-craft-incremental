namespace Game.Core.Combat
{
    /// <summary>Donnée pure (miroir GameCore de CombatantDefinitionSO, ARCHITECTURE.md §4).</summary>
    public sealed class CombatantDefinition
    {
        public string Id { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public int MaxHitPoints { get; init; }
        public int MaxActionPoints { get; init; }
        public int MaxMovementPoints { get; init; }
        public int BaseDamage { get; init; }

        /// <summary>Null/vide pour un combattant joueur (aucune IA).</summary>
        public string? EnemyAiId { get; init; }
    }
}
