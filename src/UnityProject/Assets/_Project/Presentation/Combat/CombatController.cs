using System.Collections.Generic;
using Game.Content.Definitions;
using Game.Core.Combat;
using Game.Core.Common;
using Game.Core.MiniGames;
using Game.Core.Registry;
using Game.Presentation.MiniGames;
using UnityEngine;

namespace Game.Presentation.Combat
{
    /// <summary>
    /// LIMITATION CONNUE : dépend de UnityEngine, non compilable hors éditeur (voir
    /// docs/IMPLEMENTATION.md).
    ///
    /// Orchestration Presentation du combat tactique (UI-SCREENS.md §3.5, HUD-SPEC.md §4) :
    /// grille simple sans obstacles/A* (ZONE-DESIGN.md §0), réutilise le même
    /// TimingBarMiniGameView que le craft manuel pour l'action de combat
    /// (INPUT-MAP.md §2.4 — CombatActionMiniGame réutilise le squelette timing_bar).
    /// Toute la logique de résolution vit dans CombatStateMachine (GameCore) — ce
    /// contrôleur ne fait que traduire les taps du joueur en appels d'interface et
    /// rafraîchir l'affichage de la grille.
    /// </summary>
    public sealed class CombatController : MonoBehaviour
    {
        [SerializeField] private CombatantDefinitionSO _playerDefinition = null!;
        [SerializeField] private List<CombatantDefinitionSO> _enemyDefinitions = new();
        [SerializeField] private MiniGameConfigSO _combatActionMiniGameConfig = null!;
        [SerializeField] private TimingBarMiniGameView _miniGameView = null!;
        [SerializeField] private int _gridWidth = 7;
        [SerializeField] private int _gridHeight = 7;

        private CombatStateMachine _stateMachine = null!;

        private void Start()
        {
            var combatantFactory = GameServices.Get<ICombatantFactory>();
            var actionResolver = GameServices.Get<IActionResolver>();
            var enemyAiResolver = GameServices.Get<System.Func<string, IEnemyAI>>();

            var player = combatantFactory.CreateFromDefinition(_playerDefinition.ToDefinition(), new GridPosition(0, 0));
            var combatants = new List<ICombatant> { player };

            for (var i = 0; i < _enemyDefinitions.Count; i++)
            {
                var startPosition = new GridPosition(_gridWidth - 1, _gridHeight - 1 - i);
                combatants.Add(combatantFactory.CreateFromDefinition(_enemyDefinitions[i].ToDefinition(), startPosition));
            }

            var encounterState = new CombatEncounterState(combatants, _gridWidth, _gridHeight);
            _stateMachine = new CombatStateMachine(
                encounterState, combatants, actionResolver, enemyAiResolver,
                GameServices.Get<Game.Core.Events.GameEventBus>());

            _stateMachine.StartCombat();
            RunAiTurnsUntilPlayerOrEnded();
        }

        /// <summary>Appelé par la UI de grille (tap sur une case) — hors scope détaillé de cette phase, signature exposée pour brancher plus tard.</summary>
        public void OnPlayerRequestsMove(GridPosition destination)
        {
            _stateMachine.SubmitMove(destination);
        }

        /// <summary>Appelé par la UI de grille (tap sur une cible adjacente) — lance le mini-jeu de résolution.</summary>
        public void OnPlayerRequestsAttack(ICombatant target)
        {
            _stateMachine.SubmitAttack(target);

            var context = new MiniGameContext
            {
                ActionId = "combat_attack",
                Config = _combatActionMiniGameConfig.ToConfig(),
            };

            _miniGameView.BeginResolution(GameServices.Get<IMiniGameFactory>(), context, OnCombatMiniGameResolved);
        }

        public void OnPlayerEndsTurn()
        {
            _stateMachine.EndTurn();
            RunAiTurnsUntilPlayerOrEnded();
        }

        private void OnCombatMiniGameResolved(MiniGameResult result)
        {
            _stateMachine.ResolveMiniGame(result);
            // Le popup "-14 dégâts" / "Raté" (UI-SCREENS.md §3.5) s'abonne à
            // GameEventBus.CombatActionResolved — déjà raisé par CombatStateMachine,
            // rien à dupliquer ici.
            RunAiTurnsUntilPlayerOrEnded();
        }

        private void RunAiTurnsUntilPlayerOrEnded()
        {
            while (_stateMachine.CurrentPhase != CombatPhase.Ended
                   && !_stateMachine.ActiveCombatant.IsPlayerControlled)
            {
                _stateMachine.RunAiTurnIfNeeded();
            }

            if (_stateMachine.CurrentPhase == CombatPhase.Ended)
            {
                Debug.Log($"[CombatController] Combat terminé : {_stateMachine.Outcome}");
                // Retour au Hub, popup de victoire/défaite (UI-SCREENS.md) — hors scope
                // détaillé de cette phase.
            }
        }
    }
}
