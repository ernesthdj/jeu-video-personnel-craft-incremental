using System.Collections.Generic;
using System.Linq;
using Game.Core.Items;

namespace Game.Core.MiniGames
{
    /// <summary>
    /// Paramètres de balance d'un mini-jeu — donnée pure, jamais codée en dur dans la
    /// logique (règle CLAUDE.md : "variables de balance exposées via config"). Côté
    /// Content/Presentation, ces valeurs viennent d'un MiniGameConfigSO édité dans
    /// l'inspecteur Unity ; ici c'est un POCO indépendant de Unity pour que GameCore
    /// reste testable en dehors de l'éditeur (ARCHITECTURE.md §4).
    /// </summary>
    public sealed class MiniGameConfig
    {
        public string MiniGameTypeId { get; init; } = string.Empty;

        /// <summary>Durée totale d'un passage du curseur (timing_bar) en secondes.</summary>
        public float DurationSeconds { get; init; } = 2.0f;

        /// <summary>Début/fin (normalisés 0..1) de la fenêtre de réussite.</summary>
        public float TargetWindowStart { get; init; } = 0.4f;
        public float TargetWindowEnd { get; init; } = 0.6f;

        /// <summary>
        /// Seuil de score (0..1) en-dessous duquel, si <see cref="AllowsDestructiveFailure"/>
        /// est vrai, l'objet en cours est détruit (SYSTEMS.md §1 — réservé aux recettes
        /// avancées, jamais au tutoriel, GDD Selfdoubt #3).
        /// </summary>
        public float DestructiveFailureThreshold { get; init; } = 0.15f;

        public bool AllowsDestructiveFailure { get; init; }

        /// <summary>
        /// Paliers score -> qualité, du plus exigeant au moins exigeant. Doit contenir au
        /// minimum un seuil à 0 (qualité plancher) pour ne jamais laisser un score sans
        /// palier correspondant.
        /// </summary>
        public IReadOnlyList<QualityThreshold> QualityThresholds { get; init; } =
            new List<QualityThreshold>
            {
                new(0.9f, ItemQuality.Perfect),
                new(0.6f, ItemQuality.Rare),
                new(0.0f, ItemQuality.Common),
            };

        public ItemQuality ResolveQuality(float score01)
        {
            var ordered = QualityThresholds.OrderByDescending(t => t.MinScore01);
            foreach (var threshold in ordered)
            {
                if (score01 >= threshold.MinScore01)
                {
                    return threshold.Quality;
                }
            }

            return ItemQuality.Failed;
        }
    }
}
