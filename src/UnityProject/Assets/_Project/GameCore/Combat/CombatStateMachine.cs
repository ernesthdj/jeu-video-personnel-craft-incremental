using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Events;
using Game.Core.MiniGames;

namespace Game.Core.Combat
{
    /// <summary>
    /// FSM de combat (SYSTEMS.md §4, ARCHITECTURE.md §6) : Idle -> Sélection action ->
    /// Mini-jeu -> Résolution -> Tour suivant. Orchestration explicite par instances de
    /// <see cref="ICombatState"/> (pas de switch). Grille simple, sans pathfinding A*
    /// (décision World Builder, ZONE-DESIGN.md §0) : le déplacement est validé par
    /// distance Chebyshev directe, pas de recherche de chemin.
    ///
    /// Les actions d'IA (<see cref="RunAiTurnIfNeeded"/>) réutilisent le même
    /// <see cref="IActionResolver"/> que le joueur, avec un score de mini-jeu synthétique
    /// (pas d'UI de mini-jeu pour une IA) — voir <paramref name="aiAttackScoreProvider"/>
    /// du constructeur, injectable pour les tests.
    /// </summary>
    public sealed class CombatStateMachine
    {
        private readonly List<ICombatant> _turnOrder;
        private readonly CombatEncounterState _encounterState;
        private readonly IActionResolver _actionResolver;
        private readonly Func<string, IEnemyAI> _enemyAiResolver;
        private readonly Func<ICombatant, ICombatant, float> _aiAttackScoreProvider;
        private readonly GameEventBus? _eventBus;

        private int _activeIndex;
        private (ICombatant Attacker, ICombatant Target)? _pendingAttack;
        private CombatActionResult? _lastResolvedAction;
        private ICombatState _currentState = new TurnStartState();

        public CombatPhase CurrentPhase => _currentState.Phase;
        public CombatOutcome Outcome { get; private set; } = CombatOutcome.InProgress;
        public ICombatant ActiveCombatant => _turnOrder[_activeIndex];
        public CombatEncounterState EncounterState => _encounterState;

        public CombatStateMachine(
            CombatEncounterState encounterState,
            IReadOnlyList<ICombatant> turnOrder,
            IActionResolver actionResolver,
            Func<string, IEnemyAI> enemyAiResolver,
            GameEventBus? eventBus = null,
            Func<ICombatant, ICombatant, float>? aiAttackScoreProvider = null)
        {
            _encounterState = encounterState ?? throw new ArgumentNullException(nameof(encounterState));
            _turnOrder = turnOrder?.ToList() ?? throw new ArgumentNullException(nameof(turnOrder));
            _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
            _enemyAiResolver = enemyAiResolver ?? throw new ArgumentNullException(nameof(enemyAiResolver));
            _eventBus = eventBus;
            // Score par défaut représentant une IA "compétente sans être parfaite" — un
            // combattant humain moyen l'emporte à égalité de build en jouant bien son
            // mini-jeu (cohérent avec GDD Pillar 1 : le skill du joueur doit compter).
            _aiAttackScoreProvider = aiAttackScoreProvider ?? ((_, _) => 0.75f);

            if (_turnOrder.Count == 0)
            {
                throw new ArgumentException("L'ordre des tours ne peut pas être vide.", nameof(turnOrder));
            }
        }

        public void StartCombat()
        {
            _activeIndex = 0;
            TransitionTo(new TurnStartState());
        }

        public void SubmitMove(Common.GridPosition destination)
        {
            RequirePhase(CombatPhase.ActionSelection);

            var distance = ActiveCombatant.Position.ChebyshevDistanceTo(destination);
            if (distance <= 0)
            {
                throw new InvalidOperationException("La destination doit être différente de la position actuelle.");
            }

            if (distance > ActiveCombatant.MovementPoints)
            {
                throw new InvalidOperationException(
                    $"PM insuffisants pour ce déplacement (coût {distance}, disponible {ActiveCombatant.MovementPoints}).");
            }

            if (!_encounterState.IsWithinBounds(destination))
            {
                throw new InvalidOperationException("Destination hors de la grille.");
            }

            if (_encounterState.IsOccupied(destination))
            {
                throw new InvalidOperationException("Case déjà occupée par un autre combattant.");
            }

            ActiveCombatant.SpendMovementPoints(distance);
            ActiveCombatant.Position = destination;
            // Reste en ActionSelection : le joueur peut encore attaquer/finir son tour.
        }

        public void SubmitAttack(ICombatant target)
        {
            RequirePhase(CombatPhase.ActionSelection);
            if (target is null) throw new ArgumentNullException(nameof(target));

            if (ActiveCombatant.ActionPoints < 1)
            {
                throw new InvalidOperationException("PA insuffisants pour attaquer.");
            }

            var distance = ActiveCombatant.Position.ChebyshevDistanceTo(target.Position);
            if (distance > 1)
            {
                throw new InvalidOperationException("Cible hors de portée (grille simple, portée d'attaque 1).");
            }

            _pendingAttack = (ActiveCombatant, target);
            TransitionTo(new MiniGameState());
        }

        /// <summary>Appelé par Presentation une fois le mini-jeu d'action de combat évalué.</summary>
        public CombatActionResult ResolveMiniGame(MiniGameResult miniGameResult)
        {
            RequirePhase(CombatPhase.MiniGame);

            var (attacker, target) = _pendingAttack!.Value;
            attacker.SpendActionPoints(1);

            var result = _actionResolver.ResolveAttack(attacker, target, miniGameResult);
            _eventBus?.RaiseCombatActionResolved(result);
            _lastResolvedAction = result;
            _pendingAttack = null;

            TransitionTo(new ResolutionState());
            return result;
        }

        public void EndTurn()
        {
            RequirePhase(CombatPhase.ActionSelection);
            TransitionTo(new TurnEndState());
        }

        /// <summary>
        /// Joue automatiquement le tour de l'IA (utilisé quand ActiveCombatant n'est pas
        /// contrôlé par le joueur). Boucle jusqu'à ce que l'IA rende la main
        /// (EndTurn) ou que le combat se termine.
        /// </summary>
        public void RunAiTurnIfNeeded()
        {
            if (CurrentPhase == CombatPhase.Ended) return;
            if (ActiveCombatant.IsPlayerControlled) return;

            var enemyAiId = (ActiveCombatant as CreatureCombatant)?.EnemyAiId
                ?? throw new InvalidOperationException(
                    "Le combattant actif n'est pas une créature avec IA assignée.");
            var ai = _enemyAiResolver(enemyAiId);

            // Guard-rail anti-boucle infinie : un tour ne devrait jamais dépasser
            // (PA max + PM max + 1) décisions.
            var safetyBudget = ActiveCombatant.MaxActionPoints + ActiveCombatant.MaxMovementPoints + 1;

            while (CurrentPhase == CombatPhase.ActionSelection && safetyBudget-- > 0)
            {
                var decision = ai.DecideAction(ActiveCombatant, _encounterState);
                switch (decision.Type)
                {
                    case CombatDecisionType.Move:
                        SubmitMove(decision.TargetPosition!.Value);
                        break;
                    case CombatDecisionType.Attack:
                        var attacker = ActiveCombatant;
                        SubmitAttack(decision.TargetCombatant!);
                        var score = _aiAttackScoreProvider(attacker, decision.TargetCombatant!);
                        ResolveMiniGame(new MiniGameResult(score, score >= 0.6f, Items.ItemQuality.Common));
                        break;
                    case CombatDecisionType.EndTurn:
                        EndTurn();
                        return;
                }
            }

            if (CurrentPhase == CombatPhase.ActionSelection)
            {
                // Budget de sécurité épuisé sans décision d'EndTurn explicite : on force la
                // fin de tour pour ne jamais bloquer la boucle de jeu.
                EndTurn();
            }
        }

        public CombatActionResult? LastResolvedAction => _lastResolvedAction;

        private void TransitionTo(ICombatState nextState)
        {
            _currentState = nextState;
            nextState.Enter(this);
        }

        private void RequirePhase(CombatPhase expected)
        {
            if (CurrentPhase != expected)
            {
                throw new InvalidOperationException(
                    $"Action invalide en phase '{CurrentPhase}' (attendu '{expected}').");
            }
        }

        // --- Callbacks appelés par les classes d'état (CombatStates.cs) ---

        internal void HandleTurnStartEntered()
        {
            if (TryApplyOutcome())
            {
                return;
            }

            ActiveCombatant.ResetTurnResources();
            TransitionTo(new ActionSelectionState());
        }

        internal void HandleResolutionEntered()
        {
            if (TryApplyOutcome())
            {
                return;
            }

            // Le combattant actif garde la main s'il lui reste des ressources (plusieurs
            // attaques par tour sont possibles tant que les PA le permettent).
            TransitionTo(new ActionSelectionState());
        }

        internal void HandleTurnEndEntered()
        {
            AdvanceActiveCombatant();
            TransitionTo(new TurnStartState());
        }

        private bool TryApplyOutcome()
        {
            if (!_encounterState.AnyEnemyCombatantAlive())
            {
                Outcome = CombatOutcome.PlayerVictory;
                TransitionTo(new EndedState());
                return true;
            }

            if (!_encounterState.AnyPlayerCombatantAlive())
            {
                Outcome = CombatOutcome.PlayerDefeat;
                TransitionTo(new EndedState());
                return true;
            }

            return false;
        }

        private void AdvanceActiveCombatant()
        {
            var attempts = 0;
            do
            {
                _activeIndex = (_activeIndex + 1) % _turnOrder.Count;
                attempts++;
            } while (ActiveCombatant.IsDefeated && attempts <= _turnOrder.Count);
        }
    }
}
