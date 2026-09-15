using System;
using Game.Core.Common;

namespace Game.Core.Combat
{
    /// <summary>
    /// Logique commune aux combattants concrets (PlayerCombatant, CreatureCombatant) —
    /// évite de dupliquer la gestion PV/PA/PM dans chaque implémentation.
    /// </summary>
    public abstract class CombatantBase : ICombatant
    {
        public string Id { get; }
        public string DisplayName { get; }
        public abstract bool IsPlayerControlled { get; }

        public GridPosition Position { get; set; }

        public int MaxHitPoints { get; }
        public int CurrentHitPoints { get; private set; }
        public bool IsDefeated => CurrentHitPoints <= 0;

        public int MaxActionPoints { get; }
        public int ActionPoints { get; private set; }
        public int MaxMovementPoints { get; }
        public int MovementPoints { get; private set; }

        public int BaseDamage { get; }

        protected CombatantBase(
            string id,
            string displayName,
            GridPosition startPosition,
            int maxHitPoints,
            int maxActionPoints,
            int maxMovementPoints,
            int baseDamage)
        {
            Id = id;
            DisplayName = displayName;
            Position = startPosition;
            MaxHitPoints = maxHitPoints;
            CurrentHitPoints = maxHitPoints;
            MaxActionPoints = maxActionPoints;
            ActionPoints = maxActionPoints;
            MaxMovementPoints = maxMovementPoints;
            MovementPoints = maxMovementPoints;
            BaseDamage = baseDamage;
        }

        public void ApplyDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Les dégâts ne peuvent pas être négatifs.");
            CurrentHitPoints = Math.Max(0, CurrentHitPoints - amount);
        }

        public void ResetTurnResources()
        {
            ActionPoints = MaxActionPoints;
            MovementPoints = MaxMovementPoints;
        }

        public void SpendActionPoints(int amount)
        {
            if (amount > ActionPoints)
            {
                throw new InvalidOperationException(
                    $"'{DisplayName}' n'a pas assez de PA (demandé {amount}, disponible {ActionPoints}).");
            }

            ActionPoints -= amount;
        }

        public void SpendMovementPoints(int amount)
        {
            if (amount > MovementPoints)
            {
                throw new InvalidOperationException(
                    $"'{DisplayName}' n'a pas assez de PM (demandé {amount}, disponible {MovementPoints}).");
            }

            MovementPoints -= amount;
        }
    }
}
