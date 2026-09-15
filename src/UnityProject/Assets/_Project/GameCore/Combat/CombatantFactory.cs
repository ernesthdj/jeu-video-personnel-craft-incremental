using System;
using Game.Core.Common;

namespace Game.Core.Combat
{
    public sealed class CombatantFactory : ICombatantFactory
    {
        public ICombatant CreateFromDefinition(CombatantDefinition definition, GridPosition position)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));

            if (string.IsNullOrEmpty(definition.EnemyAiId))
            {
                return new PlayerCombatant(
                    definition.Id, definition.DisplayName, position,
                    definition.MaxHitPoints, definition.MaxActionPoints, definition.MaxMovementPoints,
                    definition.BaseDamage);
            }

            return new CreatureCombatant(
                definition.Id, definition.DisplayName, position,
                definition.MaxHitPoints, definition.MaxActionPoints, definition.MaxMovementPoints,
                definition.BaseDamage, definition.EnemyAiId);
        }
    }
}
