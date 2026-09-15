using System.Collections.Generic;
using Game.Core.Items;
using Game.Core.MiniGames;
using UnityEngine;

namespace Game.Content.Definitions
{
    /// <summary>
    /// LIMITATION CONNUE : ce fichier référence UnityEngine (ScriptableObject) et ne peut
    /// pas être compilé dans cet environnement d'agent (pas d'installation Unity) — voir
    /// docs/IMPLEMENTATION.md. Vérifié uniquement par revue manuelle : les champs
    /// correspondent exactement aux propriétés de Game.Core.MiniGames.MiniGameConfig
    /// (GameCore, réellement compilé et testé) pour que <see cref="ToConfig"/> ne perde
    /// aucune donnée.
    ///
    /// Convention obligatoire (ARCHITECTURE.md §4) : uniquement des données + validation de
    /// champs, aucune logique de résolution ici (elle reste dans GameCore, qui LIT cette
    /// donnée via <see cref="ToConfig"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "MiniGameConfig", menuName = "CraftIncremental/Mini-Jeu/Config", order = 0)]
    public sealed class MiniGameConfigSO : ScriptableObject
    {
        [Tooltip("Id du type de mini-jeu, résolu via IMiniGameFactory (ex: 'timing_bar').")]
        [SerializeField] private string _miniGameTypeId = "timing_bar";

        [Tooltip("Durée totale d'un passage du curseur, en secondes.")]
        [SerializeField, Min(0.1f)] private float _durationSeconds = 2.0f;

        [Header("Fenêtre de réussite (normalisée 0..1)")]
        [SerializeField, Range(0f, 1f)] private float _targetWindowStart = 0.4f;
        [SerializeField, Range(0f, 1f)] private float _targetWindowEnd = 0.6f;

        [Header("Échec destructif (SYSTEMS.md §1 — réservé aux recettes avancées)")]
        [SerializeField] private bool _allowsDestructiveFailure;
        [SerializeField, Range(0f, 1f)] private float _destructiveFailureThreshold = 0.15f;

        [Header("Paliers de qualité (score minimal -> qualité)")]
        [SerializeField] private List<QualityThresholdEntry> _qualityThresholds = new()
        {
            new QualityThresholdEntry { MinScore01 = 0.9f, Quality = ItemQuality.Perfect },
            new QualityThresholdEntry { MinScore01 = 0.6f, Quality = ItemQuality.Rare },
            new QualityThresholdEntry { MinScore01 = 0f, Quality = ItemQuality.Common },
        };

        public MiniGameConfig ToConfig()
        {
            var thresholds = new List<QualityThreshold>(_qualityThresholds.Count);
            foreach (var entry in _qualityThresholds)
            {
                thresholds.Add(new QualityThreshold(entry.MinScore01, entry.Quality));
            }

            return new MiniGameConfig
            {
                MiniGameTypeId = _miniGameTypeId,
                DurationSeconds = _durationSeconds,
                TargetWindowStart = _targetWindowStart,
                TargetWindowEnd = _targetWindowEnd,
                AllowsDestructiveFailure = _allowsDestructiveFailure,
                DestructiveFailureThreshold = _destructiveFailureThreshold,
                QualityThresholds = thresholds,
            };
        }

        [System.Serializable]
        public struct QualityThresholdEntry
        {
            [Range(0f, 1f)] public float MinScore01;
            public ItemQuality Quality;
        }

        private void OnValidate()
        {
            if (_targetWindowEnd < _targetWindowStart)
            {
                (_targetWindowStart, _targetWindowEnd) = (_targetWindowEnd, _targetWindowStart);
            }
        }
    }
}
