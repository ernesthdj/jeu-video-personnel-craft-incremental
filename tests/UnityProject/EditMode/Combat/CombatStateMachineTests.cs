using System;
using System.Collections.Generic;
using Game.Core.Combat;
using Game.Core.Common;
using Game.Core.Items;
using Game.Core.MiniGames;
using Xunit;

namespace Game.Core.Tests.Combat
{
    /// <summary>
    /// FSM de combat (SYSTEMS.md §4, ARCHITECTURE.md §6) : Idle -> Sélection action ->
    /// Mini-jeu -> Résolution -> Tour suivant. Grille simple sans obstacles/A*
    /// (ZONE-DESIGN.md §0) — vérifié par des déplacements Chebyshev directs.
    /// </summary>
    public class CombatStateMachineTests
    {
        private static PlayerCombatant MakePlayer(GridPosition position, int hp = 20, int pa = 2, int pm = 3, int damage = 10) =>
            new("player_1", "Héros", position, hp, pa, pm, damage);

        private static CreatureCombatant MakeEnemy(GridPosition position, int hp = 15, int pa = 1, int pm = 2, int damage = 5) =>
            new("enemy_1", "Gobelin", position, hp, pa, pm, damage, "aggressive");

        private static CombatStateMachine MakeSut(
            PlayerCombatant player,
            CreatureCombatant enemy,
            Func<ICombatant, ICombatant, float>? aiScoreProvider = null,
            int gridWidth = 7,
            int gridHeight = 7)
        {
            var encounter = new CombatEncounterState(new List<ICombatant> { player, enemy }, gridWidth, gridHeight);
            var resolver = new CombatActionResolver();
            IEnemyAI EnemyAiResolver(string id) => id switch
            {
                "aggressive" => new AggressiveAI(),
                _ => throw new InvalidOperationException($"IA inconnue: {id}"),
            };

            return new CombatStateMachine(encounter, new List<ICombatant> { player, enemy }, resolver, EnemyAiResolver,
                aiAttackScoreProvider: aiScoreProvider);
        }

        [Fact]
        public void should_start_in_action_selection_after_turn_start_when_combat_starts()
        {
            var player = MakePlayer(new GridPosition(0, 0));
            var enemy = MakeEnemy(new GridPosition(5, 5));
            var sut = MakeSut(player, enemy);

            sut.StartCombat();

            Assert.Equal(CombatPhase.ActionSelection, sut.CurrentPhase);
            Assert.Equal(player.Id, sut.ActiveCombatant.Id);
            Assert.Equal(player.MaxActionPoints, player.ActionPoints);
            Assert.Equal(player.MaxMovementPoints, player.MovementPoints);
        }

        [Fact]
        public void should_move_combatant_and_spend_movement_points_on_valid_move()
        {
            var player = MakePlayer(new GridPosition(0, 0));
            var enemy = MakeEnemy(new GridPosition(5, 5));
            var sut = MakeSut(player, enemy);
            sut.StartCombat();

            sut.SubmitMove(new GridPosition(2, 1)); // distance Chebyshev = 2

            Assert.Equal(new GridPosition(2, 1), player.Position);
            Assert.Equal(player.MaxMovementPoints - 2, player.MovementPoints);
            Assert.Equal(CombatPhase.ActionSelection, sut.CurrentPhase); // reste en sélection
        }

        [Fact]
        public void should_reject_move_beyond_available_movement_points()
        {
            var player = MakePlayer(new GridPosition(0, 0), pm: 1);
            var enemy = MakeEnemy(new GridPosition(5, 5));
            var sut = MakeSut(player, enemy);
            sut.StartCombat();

            Assert.Throws<InvalidOperationException>(() => sut.SubmitMove(new GridPosition(3, 0)));
        }

        [Fact]
        public void should_transition_to_minigame_phase_on_attack_within_range()
        {
            var player = MakePlayer(new GridPosition(1, 1));
            var enemy = MakeEnemy(new GridPosition(1, 2)); // distance Chebyshev = 1
            var sut = MakeSut(player, enemy);
            sut.StartCombat();

            sut.SubmitAttack(enemy);

            Assert.Equal(CombatPhase.MiniGame, sut.CurrentPhase);
        }

        [Fact]
        public void should_reject_attack_when_target_out_of_range()
        {
            var player = MakePlayer(new GridPosition(0, 0));
            var enemy = MakeEnemy(new GridPosition(5, 5));
            var sut = MakeSut(player, enemy);
            sut.StartCombat();

            Assert.Throws<InvalidOperationException>(() => sut.SubmitAttack(enemy));
        }

        [Fact]
        public void should_apply_damage_and_return_to_action_selection_after_successful_hit()
        {
            var player = MakePlayer(new GridPosition(1, 1), damage: 10);
            var enemy = MakeEnemy(new GridPosition(1, 2), hp: 30);
            var sut = MakeSut(player, enemy);
            sut.StartCombat();
            sut.SubmitAttack(enemy);

            var result = sut.ResolveMiniGame(new MiniGameResult(0.9f, true, ItemQuality.Perfect));

            Assert.True(result.Hit);
            Assert.True(result.DamageDealt > 0);
            Assert.Equal(30 - result.DamageDealt, enemy.CurrentHitPoints);
            Assert.Equal(player.MaxActionPoints - 1, player.ActionPoints);
            Assert.Equal(CombatPhase.ActionSelection, sut.CurrentPhase); // le tour continue (PA restants)
        }

        [Fact]
        public void should_miss_and_deal_no_damage_when_minigame_score_is_below_hit_threshold()
        {
            var player = MakePlayer(new GridPosition(1, 1));
            var enemy = MakeEnemy(new GridPosition(1, 2), hp: 30);
            var sut = MakeSut(player, enemy);
            sut.StartCombat();
            sut.SubmitAttack(enemy);

            var result = sut.ResolveMiniGame(new MiniGameResult(0.2f, false, ItemQuality.Failed));

            Assert.False(result.Hit);
            Assert.Equal(0, result.DamageDealt);
            Assert.Equal(30, enemy.CurrentHitPoints);
        }

        [Fact]
        public void should_reach_player_victory_outcome_when_all_enemies_defeated()
        {
            var player = MakePlayer(new GridPosition(1, 1), damage: 999);
            var enemy = MakeEnemy(new GridPosition(1, 2), hp: 10);
            var sut = MakeSut(player, enemy);
            sut.StartCombat();
            sut.SubmitAttack(enemy);

            sut.ResolveMiniGame(new MiniGameResult(1f, true, ItemQuality.Perfect));

            Assert.Equal(CombatPhase.Ended, sut.CurrentPhase);
            Assert.Equal(CombatOutcome.PlayerVictory, sut.Outcome);
            Assert.True(enemy.IsDefeated);
        }

        [Fact]
        public void should_advance_turn_to_enemy_and_run_ai_which_attacks_player_in_range()
        {
            // Ennemi déjà à portée (Chebyshev 1) pour que l'IA attaque immédiatement au
            // lieu de se déplacer d'abord — garde le test déterministe et rapide.
            var player = MakePlayer(new GridPosition(1, 1), hp: 30);
            var enemy = MakeEnemy(new GridPosition(1, 2), damage: 8);
            var sut = MakeSut(player, enemy, aiScoreProvider: (_, _) => 0.8f);
            sut.StartCombat();

            sut.EndTurn(); // le joueur ne fait rien et passe la main

            Assert.Equal(enemy.Id, sut.ActiveCombatant.Id);
            Assert.Equal(CombatPhase.ActionSelection, sut.CurrentPhase);

            sut.RunAiTurnIfNeeded();

            // L'IA doit avoir attaqué (score 0.8 > seuil de réussite 0.6) et rendu la main.
            Assert.True(player.CurrentHitPoints < 30);
            Assert.Equal(player.Id, sut.ActiveCombatant.Id); // retour au joueur
            Assert.Equal(CombatPhase.ActionSelection, sut.CurrentPhase);
        }

        [Fact]
        public void should_move_ai_toward_player_when_out_of_attack_range_then_attack_next_call()
        {
            var player = MakePlayer(new GridPosition(0, 0), hp: 30);
            // Distance Chebyshev initiale = 6, pm = 4 : après un déplacement diagonal
            // maximal la distance résiduelle est 6-4=2, donc toujours hors de portée
            // (portée d'attaque 1) — l'IA ne peut pas attaquer ce tour-ci.
            var enemy = MakeEnemy(new GridPosition(6, 6), pm: 4, damage: 6);
            var sut = MakeSut(player, enemy, aiScoreProvider: (_, _) => 0.8f);
            sut.StartCombat();
            sut.EndTurn();

            var enemyStartPosition = enemy.Position;
            sut.RunAiTurnIfNeeded();

            // L'IA doit s'être rapprochée du joueur (impossible d'atteindre la portée 1
            // en un seul tour vu la distance initiale de 6 cases et pm=4, donc elle
            // termine son tour après s'être déplacée, sans attaquer).
            var newDistance = enemy.Position.ChebyshevDistanceTo(player.Position);
            var oldDistance = enemyStartPosition.ChebyshevDistanceTo(player.Position);
            Assert.True(newDistance < oldDistance);
            Assert.Equal(30, player.CurrentHitPoints); // pas encore à portée, pas de dégâts ce tour
            Assert.Equal(player.Id, sut.ActiveCombatant.Id); // la main est repassée au joueur
        }

        [Fact]
        public void should_reach_player_defeat_outcome_when_player_is_defeated()
        {
            var player = MakePlayer(new GridPosition(1, 1), hp: 5);
            var enemy = MakeEnemy(new GridPosition(1, 2), damage: 50);
            var sut = MakeSut(player, enemy, aiScoreProvider: (_, _) => 1f);
            sut.StartCombat();
            sut.EndTurn(); // passe à l'ennemi

            sut.RunAiTurnIfNeeded();

            Assert.Equal(CombatPhase.Ended, sut.CurrentPhase);
            Assert.Equal(CombatOutcome.PlayerDefeat, sut.Outcome);
        }

        [Fact]
        public void should_throw_when_action_submitted_in_wrong_phase()
        {
            var player = MakePlayer(new GridPosition(1, 1));
            var enemy = MakeEnemy(new GridPosition(1, 2));
            var sut = MakeSut(player, enemy);
            sut.StartCombat();
            sut.SubmitAttack(enemy); // -> phase MiniGame

            // Un SubmitMove pendant la phase MiniGame est un bug d'appelant (UI qui
            // n'attend pas la résolution) — doit être rejeté explicitement.
            Assert.Throws<InvalidOperationException>(() => sut.SubmitMove(new GridPosition(2, 2)));
        }
    }
}
