using System;
using Game.Core.MiniGames;

namespace Game.Core.Combat
{
    /// <summary>
    /// Implémentation MVP : sous le seuil de réussite (0.6, cohérent avec
    /// TimingBarMiniGame), l'action rate (0 dégât). Au-dessus, les dégâts scalent
    /// linéairement entre 50% et 150% du dégât de base, avec un seuil "critique" au-delà
    /// de 0.9 (double-pulse haptique, FEEDBACK-MAP.md).
    /// </summary>
    public sealed class CombatActionResolver : IActionResolver
    {
        private const float MissThreshold = 0.6f;
        private const float CriticalThreshold = 0.9f;

        public CombatActionResult ResolveAttack(ICombatant attacker, ICombatant target, MiniGameResult miniGameResult)
        {
            if (attacker is null) throw new ArgumentNullException(nameof(attacker));
            if (target is null) throw new ArgumentNullException(nameof(target));

            var score = miniGameResult.Score01;

            if (score < MissThreshold)
            {
                return new CombatActionResult
                {
                    AttackerId = attacker.Id,
                    TargetId = target.Id,
                    Hit = false,
                    Critical = false,
                    DamageDealt = 0,
                    TargetDefeated = target.IsDefeated,
                    Score01 = score,
                };
            }

            var isCritical = score >= CriticalThreshold;
            // score in [0.6, 1.0] -> multiplicateur en [0.5, 1.5]
            var multiplier = 0.5f + Math.Clamp((score - MissThreshold) / (1f - MissThreshold), 0f, 1f);
            var damage = (int)Math.Round(attacker.BaseDamage * multiplier);

            target.ApplyDamage(damage);

            return new CombatActionResult
            {
                AttackerId = attacker.Id,
                TargetId = target.Id,
                Hit = true,
                Critical = isCritical,
                DamageDealt = damage,
                TargetDefeated = target.IsDefeated,
                Score01 = score,
            };
        }
    }
}
