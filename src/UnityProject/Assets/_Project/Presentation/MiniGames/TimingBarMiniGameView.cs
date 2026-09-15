#nullable enable
using System;
using Game.Core.MiniGames;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Presentation.MiniGames
{
    /// <summary>
    /// LIMITATION CONNUE : dépend de UnityEngine/UnityEngine.InputSystem/UnityEngine.UI,
    /// non compilable hors éditeur (voir docs/IMPLEMENTATION.md). Signatures vérifiées
    /// manuellement contre Game.Core.MiniGames (GameCore, compilé et testé).
    ///
    /// Implémente les contraintes UI-SCREENS.md §0/§3.4 et FEEDBACK-MAP.md §3 :
    /// - Vit sur un Canvas isolé (assigné dans l'inspecteur, séparé du HUD) pour qu'un
    ///   Canvas.Rebuild déclenché ailleurs ne retape jamais ce mini-jeu (contrainte
    ///   &lt; 100ms).
    /// - Échantillonne l'Input System à CHAQUE frame Update et appelle
    ///   IMiniGame.OnInputSample immédiatement — jamais de file d'attente, jamais de
    ///   Coroutine.WaitForSeconds entre l'input et le feedback.
    /// - Le flux d'input continu reste STRICTEMENT LOCAL à ce composant : il ne passe
    ///   jamais par GameEventBus (réservé aux événements de fin d'action). Seul l'appel à
    ///   OnMiniGameCompleted (résultat final) informe l'extérieur, via un delegate C#
    ///   simple, pas le bus.
    /// - Le feedback visuel (barre de précision) est piloté par lecture directe de l'état
    ///   courant, pas par un événement différé.
    /// </summary>
    public sealed class TimingBarMiniGameView : MonoBehaviour
    {
        [Header("Références UI")]
        [SerializeField] private RectTransform _cursorHandle = null!;
        [SerializeField] private RectTransform _trackArea = null!;
        [SerializeField] private RectTransform _targetWindowHandle = null!;
        [SerializeField] private Image _precisionBarFill = null!;
        [SerializeField] private InputActionReference _primaryTapAction = null!;

        private IMiniGame _miniGame = null!;
        private MiniGameContext _context = null!;
        private double _elapsedSeconds;
        private Action<MiniGameResult>? _onCompleted;

        /// <summary>
        /// Démarre une résolution de mini-jeu. `miniGameFactory` vient de GameServices
        /// (jamais un `new TimingBarMiniGame()` en dehors de GameBootstrap,
        /// VERTICAL-SLICE.md §4 — critère de complétion vérifiable par revue de code).
        /// </summary>
        public void BeginResolution(IMiniGameFactory miniGameFactory, MiniGameContext context, Action<MiniGameResult> onCompleted)
        {
            _context = context;
            _miniGame = miniGameFactory.Create(context.Config.MiniGameTypeId);
            _miniGame.Initialize(context);
            _onCompleted = onCompleted;
            _elapsedSeconds = 0d;

            LayoutTargetWindow();
            gameObject.SetActive(true);
            // Zéro animation de transition > 150ms avant que la zone ne devienne
            // interactive (UI-SCREENS.md §3.4) : pas d'animation ici, activation immédiate.
        }

        private void OnEnable()
        {
            if (_primaryTapAction != null)
            {
                _primaryTapAction.action.performed += OnPrimaryTapPerformed;
                _primaryTapAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (_primaryTapAction != null)
            {
                _primaryTapAction.action.performed -= OnPrimaryTapPerformed;
            }
        }

        private void Update()
        {
            if (_miniGame is null || _miniGame.IsComplete)
            {
                return;
            }

            _elapsedSeconds += Time.deltaTime;

            // Échantillon "passif" à chaque frame (pas de tap) — fait progresser le
            // curseur et permet au mini-jeu de détecter un timeout.
            _miniGame.OnInputSample(new MiniGameInputSample(_elapsedSeconds, 0f, 0f, isPrimaryActionTriggered: false));

            RenderCursorAndPrecisionFeedback();

            if (_miniGame.IsComplete)
            {
                CompleteResolution();
            }
        }

        private void OnPrimaryTapPerformed(InputAction.CallbackContext ctx)
        {
            if (_miniGame is null || _miniGame.IsComplete)
            {
                return;
            }

            _miniGame.OnInputSample(new MiniGameInputSample(_elapsedSeconds, 0f, 0f, isPrimaryActionTriggered: true));
            RenderCursorAndPrecisionFeedback();

            if (_miniGame.IsComplete)
            {
                CompleteResolution();
            }
        }

        private void RenderCursorAndPrecisionFeedback()
        {
            // Lecture directe de l'état courant du mini-jeu pour le feedback "micro"
            // (FEEDBACK-MAP.md §1) — jamais via GameEventBus. TimingBarMiniGame
            // n'expose pas de "position courante" publique au-delà d'Evaluate() une fois
            // complet ; côté Presentation, on peut donc dériver la position visuelle du
            // curseur directement à partir du temps écoulé (même calcul que le domaine,
            // dupliqué ici uniquement pour l'affichage — pas de logique de score côté
            // Presentation, seulement une position 0..1 pour l'animation).
            var duration = _context.Config.DurationSeconds / Mathf.Max(_context.DifficultyMultiplier, 0.0001f);
            var cursorPosition01 = Mathf.Clamp01((float)(_elapsedSeconds / duration));

            if (_trackArea != null && _cursorHandle != null)
            {
                var width = _trackArea.rect.width;
                _cursorHandle.anchoredPosition = new Vector2(cursorPosition01 * width, _cursorHandle.anchoredPosition.y);
            }

            if (_precisionBarFill != null)
            {
                var inWindow = cursorPosition01 >= _context.Config.TargetWindowStart
                    && cursorPosition01 <= _context.Config.TargetWindowEnd;
                _precisionBarFill.color = inWindow ? Color.green : Color.white;
            }
        }

        private void LayoutTargetWindow()
        {
            if (_targetWindowHandle == null || _trackArea == null)
            {
                return;
            }

            var width = _trackArea.rect.width;
            var start = _context.Config.TargetWindowStart * width;
            var end = _context.Config.TargetWindowEnd * width;
            _targetWindowHandle.anchoredPosition = new Vector2(start, _targetWindowHandle.anchoredPosition.y);
            _targetWindowHandle.sizeDelta = new Vector2(end - start, _targetWindowHandle.sizeDelta.y);
        }

        private void CompleteResolution()
        {
            var result = _miniGame.Evaluate();
            gameObject.SetActive(false);
            _onCompleted?.Invoke(result);
            _onCompleted = null;
        }

        /// <summary>Bouton abandon (UI-SCREENS.md §3.4 — toujours accessible, jamais centré).</summary>
        public void OnAbandonButtonPressed()
        {
            gameObject.SetActive(false);
            // Un abandon = échec net (score 0), cohérent avec un timeout.
            _onCompleted?.Invoke(new MiniGameResult(0f, false, Game.Core.Items.ItemQuality.Failed));
            _onCompleted = null;
        }
    }
}
