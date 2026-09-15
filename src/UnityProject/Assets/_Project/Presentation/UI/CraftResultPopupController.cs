using Game.Core.Crafting;
using Game.Core.Events;
using Game.Core.Items;
using Game.Core.Registry;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.UI
{
    /// <summary>
    /// LIMITATION CONNUE : dépend de UnityEngine/UnityEngine.UI, non compilable hors
    /// éditeur (voir docs/IMPLEMENTATION.md).
    ///
    /// Popup résultat de craft (UI-SCREENS.md §1, FEEDBACK-MAP.md §2) — s'abonne à
    /// GameEventBus.CraftCompleted / ItemDestroyed, jamais de duplication de la logique
    /// de résolution. Le "Critique négatif" (destruction) doit être acoustiquement et
    /// visuellement inconfondable avec un succès (FEEDBACK-MAP.md §1) : pattern haptique
    /// dédié <see cref="HapticPattern.DestructiveFailure"/>, réutilisé aussi pour un KO en
    /// combat (jamais dupliqué, cf. alerte de fin de JOURNAL.md de l'UI/UX Game Designer).
    /// </summary>
    public sealed class CraftResultPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject _standardResultPanel = null!;
        [SerializeField] private GameObject _destructiveFailurePanel = null!;
        [SerializeField] private Text _resultText = null!;

        private void OnEnable()
        {
            var bus = GameServices.Get<GameEventBus>();
            bus.CraftCompleted += OnCraftCompleted;
            bus.ItemDestroyed += OnItemDestroyed;
        }

        private void OnDisable()
        {
            if (GameServices.TryGet<GameEventBus>(out var bus) && bus != null)
            {
                bus.CraftCompleted -= OnCraftCompleted;
                bus.ItemDestroyed -= OnItemDestroyed;
            }
        }

        /// <summary>Appel direct depuis CraftManualController — évite d'attendre un aller-retour d'event pour le feedback synchrone à l'action du joueur.</summary>
        public void ShowCraftResult(CraftResult result) => OnCraftCompleted(result);

        private void OnCraftCompleted(CraftResult result)
        {
            if (result.ItemDestroyed)
            {
                return; // déjà géré par OnItemDestroyed (feedback "Critique négatif" dédié)
            }

            _standardResultPanel.SetActive(true);
            var qualityLabel = result.Quality switch
            {
                ItemQuality.Perfect => "Qualité parfaite !",
                ItemQuality.Rare => "Qualité rare.",
                ItemQuality.Common => "Qualité commune.",
                _ => "Échec.",
            };
            _resultText.text = qualityLabel;

            HapticFeedback.Play(result.Quality == ItemQuality.Perfect
                ? HapticPattern.Fort
                : HapticPattern.Standard);
        }

        private void OnItemDestroyed(ItemInstance _)
        {
            _destructiveFailurePanel.SetActive(true);
            HapticFeedback.Play(HapticPattern.DestructiveFailure);
        }
    }

    /// <summary>
    /// Constante nommée réutilisée partout où une perte permanente survient (item détruit,
    /// unité KO) — alerte explicite de l'UI/UX Game Designer (JOURNAL.md) : jamais de
    /// valeur magique dupliquée pour ce pattern haptique.
    /// </summary>
    public enum HapticPattern
    {
        Micro,
        Standard,
        Fort,
        DestructiveFailure,
    }

    /// <summary>
    /// Point d'appel unique pour le retour haptique (toggle accessibilité global,
    /// INPUT-MAP.md §4 / FEEDBACK-MAP.md §3). Implémentation réelle (Gamepad.current.Rumble
    /// / Handheld.Vibrate) volontairement omise ici — hors du périmètre "faire fonctionner
    /// le cycle craft/combat" de cette phase, mais le point d'appel centralisé existe déjà
    /// pour que l'intégration future ne duplique rien.
    /// </summary>
    internal static class HapticFeedback
    {
        public static bool Enabled = true;

        public static void Play(HapticPattern pattern)
        {
            if (!Enabled) return;
            // TODO(non bloquant pour ce slice) : brancher sur l'API haptique réelle du device.
        }
    }
}
