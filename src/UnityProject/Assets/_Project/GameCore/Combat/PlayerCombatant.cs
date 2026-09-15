using Game.Core.Common;

namespace Game.Core.Combat
{
    public sealed class PlayerCombatant : CombatantBase
    {
        public override bool IsPlayerControlled => true;

        public PlayerCombatant(string id, string displayName, GridPosition startPosition,
            int maxHitPoints, int maxActionPoints, int maxMovementPoints, int baseDamage)
            : base(id, displayName, startPosition, maxHitPoints, maxActionPoints, maxMovementPoints, baseDamage)
        {
        }
    }
}
