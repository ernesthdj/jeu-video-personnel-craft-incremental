using Game.Core.Common;

namespace Game.Core.Combat
{
    public sealed class CreatureCombatant : CombatantBase
    {
        public override bool IsPlayerControlled => false;

        /// <summary>Id du IEnemyAI à utiliser pour ce combattant (résolu via le registry côté bootstrap).</summary>
        public string EnemyAiId { get; }

        public CreatureCombatant(string id, string displayName, GridPosition startPosition,
            int maxHitPoints, int maxActionPoints, int maxMovementPoints, int baseDamage, string enemyAiId)
            : base(id, displayName, startPosition, maxHitPoints, maxActionPoints, maxMovementPoints, baseDamage)
        {
            EnemyAiId = enemyAiId;
        }
    }
}
