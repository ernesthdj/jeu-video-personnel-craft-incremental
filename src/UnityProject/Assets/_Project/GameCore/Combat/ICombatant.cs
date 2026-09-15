using Game.Core.Common;

namespace Game.Core.Combat
{
    /// <summary>
    /// Contrat commun à toute unité participant au combat (joueur ou créature). Porte les
    /// stats/position/PA-PM — la double couche de skill (build + mini-jeu, GDD Selfdoubt
    /// #5) vit à la frontière entre cette interface et <see cref="IActionResolver"/>.
    /// </summary>
    public interface ICombatant
    {
        string Id { get; }
        string DisplayName { get; }
        bool IsPlayerControlled { get; }

        GridPosition Position { get; set; }

        int MaxHitPoints { get; }
        int CurrentHitPoints { get; }
        bool IsDefeated { get; }

        int MaxActionPoints { get; }
        int ActionPoints { get; }
        int MaxMovementPoints { get; }
        int MovementPoints { get; }

        int BaseDamage { get; }

        void ApplyDamage(int amount);
        void ResetTurnResources();
        void SpendActionPoints(int amount);
        void SpendMovementPoints(int amount);
    }
}
