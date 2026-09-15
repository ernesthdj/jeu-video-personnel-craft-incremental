using System;
using Game.Core.Items;

namespace Game.Core.MiniGames
{
    /// <summary>
    /// Premier mini-jeu concret, priorité absolue de la vertical slice
    /// (VERTICAL-SLICE.md étape 3) : un curseur traverse une barre en
    /// <see cref="MiniGameConfig.DurationSeconds"/> secondes, le joueur tape une fois au
    /// bon moment. Réutilisé tel quel pour l'action de combat de base
    /// (INPUT-MAP.md §2.4 : CombatActionMiniGame réutilise le squelette timing_bar).
    ///
    /// Le score est calculé à partir de l'écart entre la position du curseur au moment du
    /// tap et le centre de la fenêtre cible (<see cref="MiniGameConfig.TargetWindowStart"/>/
    /// <see cref="MiniGameConfig.TargetWindowEnd"/>) — aucun aléatoire, uniquement le geste
    /// réel du joueur (GDD Pillar 1).
    /// </summary>
    public sealed class TimingBarMiniGame : IMiniGame
    {
        private MiniGameContext _context = null!;
        private double? _startTimestamp;
        private float? _tapCursorPosition01;
        private bool _timedOut;

        public bool IsComplete { get; private set; }

        public void Initialize(MiniGameContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _startTimestamp = null;
            _tapCursorPosition01 = null;
            _timedOut = false;
            IsComplete = false;
        }

        public void OnInputSample(MiniGameInputSample sample)
        {
            if (IsComplete)
            {
                return;
            }

            _startTimestamp ??= sample.TimestampSeconds;

            var elapsed = sample.TimestampSeconds - _startTimestamp.Value;
            var duration = EffectiveDuration();
            var cursorPosition01 = duration <= 0
                ? 1f
                : (float)Math.Clamp(elapsed / duration, 0d, 1d);

            if (sample.IsPrimaryActionTriggered)
            {
                _tapCursorPosition01 = cursorPosition01;
                IsComplete = true;
                return;
            }

            // Pas de tap reçu et le curseur a atteint le bout de la barre : échec par timeout,
            // le joueur n'a pas engagé son geste à temps (score 0, pas de destruction bonus/malus caché).
            if (elapsed >= duration)
            {
                _timedOut = true;
                IsComplete = true;
            }
        }

        public MiniGameResult Evaluate()
        {
            if (!IsComplete)
            {
                throw new InvalidOperationException(
                    "TimingBarMiniGame.Evaluate() appelé avant IsComplete == true.");
            }

            if (_timedOut || _tapCursorPosition01 is null)
            {
                // Aucun engagement du joueur (pas de tap avant la fin du curseur) : un
                // "no-show" est toujours un échec net, distinct d'un tap raté mais tenté
                // qui peut encore retomber sur le palier plancher configuré
                // (MiniGameConfig.QualityThresholds) — d'où l'ItemQuality.Failed en dur
                // ici plutôt qu'un ResolveQuality(0f) qui dépendrait des seuils de la
                // recette.
                return new MiniGameResult(0f, false, ItemQuality.Failed);
            }

            var score = ComputeScore(_tapCursorPosition01.Value);
            var success = score >= SuccessThreshold;
            return new MiniGameResult(score, success, _context.Config.ResolveQuality(score));
        }

        /// <summary>
        /// Score minimal pour considérer le tap comme une réussite (cohérent avec le seuil
        /// de "raté" utilisé par CombatActionResolver et avec le palier Rare des
        /// QualityThresholds par défaut).
        /// </summary>
        private const float SuccessThreshold = 0.6f;

        private float EffectiveDuration()
        {
            // DifficultyMultiplier > 1 = plus dur = fenêtre parcourue plus vite (durée réduite).
            var multiplier = _context.DifficultyMultiplier <= 0f ? 1f : _context.DifficultyMultiplier;
            return _context.Config.DurationSeconds / multiplier;
        }

        private float ComputeScore(float cursorPosition01)
        {
            var config = _context.Config;
            var windowStart = Math.Min(config.TargetWindowStart, config.TargetWindowEnd);
            var windowEnd = Math.Max(config.TargetWindowStart, config.TargetWindowEnd);
            var windowCenter = (windowStart + windowEnd) / 2f;
            var halfWindow = Math.Max((windowEnd - windowStart) / 2f, 0.0001f);

            if (cursorPosition01 >= windowStart && cursorPosition01 <= windowEnd)
            {
                // Dans la fenêtre : score linéaire du bord (0.6) au centre (1.0) — un tap pile
                // au centre est "parfait", un tap en bord de fenêtre reste une réussite mais
                // de qualité moindre (cohérent avec le mapping continu vers qualité SYSTEMS.md §1).
                var distanceFromCenter01 = Math.Abs(cursorPosition01 - windowCenter) / halfWindow;
                return Math.Clamp(1f - (0.4f * distanceFromCenter01), 0.6f, 1f);
            }

            // Hors fenêtre : score dégressif avec la distance, jamais négatif.
            var distanceOutside = cursorPosition01 < windowStart
                ? windowStart - cursorPosition01
                : cursorPosition01 - windowEnd;

            var falloff = Math.Clamp(1f - (distanceOutside / 0.5f), 0f, 1f);
            // Un raté reste sous le seuil de réussite (0.6) par construction.
            return falloff * 0.59f;
        }
    }
}
