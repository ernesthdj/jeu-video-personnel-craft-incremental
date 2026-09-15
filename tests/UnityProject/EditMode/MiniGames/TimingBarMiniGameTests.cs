using Game.Core.Items;
using Game.Core.MiniGames;
using Xunit;

namespace Game.Core.Tests.MiniGames
{
    /// <summary>
    /// Vérifie le système le plus critique du jeu (SYSTEMS.md §5, GDD Selfdoubt #1) : le
    /// score dépend uniquement du geste simulé (timestamp du tap), jamais d'un aléa caché
    /// — condition nécessaire au Pillar 1 ("l'habileté prime sur le hasard").
    /// </summary>
    public class TimingBarMiniGameTests
    {
        private static MiniGameContext MakeContext(float targetStart = 0.4f, float targetEnd = 0.6f, float duration = 2f) =>
            new()
            {
                ActionId = "craft_epee_courte",
                Config = new MiniGameConfig
                {
                    MiniGameTypeId = "timing_bar",
                    DurationSeconds = duration,
                    TargetWindowStart = targetStart,
                    TargetWindowEnd = targetEnd,
                    AllowsDestructiveFailure = true,
                    DestructiveFailureThreshold = 0.15f,
                },
            };

        [Fact]
        public void should_return_high_score_when_tap_lands_at_target_window_center()
        {
            var miniGame = new TimingBarMiniGame();
            miniGame.Initialize(MakeContext());

            // Fenêtre [0.4, 0.6] sur une durée de 2s -> centre à 1.0s.
            miniGame.OnInputSample(new MiniGameInputSample(0.0, 0f, 0f, false));
            miniGame.OnInputSample(new MiniGameInputSample(1.0, 0.5f, 0f, isPrimaryActionTriggered: true));

            Assert.True(miniGame.IsComplete);
            var result = miniGame.Evaluate();

            Assert.True(result.Success);
            Assert.True(result.Score01 >= 0.95f, $"Score attendu proche de 1.0 au centre de la fenêtre, obtenu {result.Score01}");
            Assert.Equal(ItemQuality.Perfect, result.Quality);
        }

        [Fact]
        public void should_return_low_score_when_tap_lands_far_outside_target_window()
        {
            var miniGame = new TimingBarMiniGame();
            miniGame.Initialize(MakeContext());

            miniGame.OnInputSample(new MiniGameInputSample(0.0, 0f, 0f, false));
            // Tap tout au début du curseur (position ~0), très loin de la fenêtre [0.4, 0.6].
            miniGame.OnInputSample(new MiniGameInputSample(0.02, 0f, 0f, isPrimaryActionTriggered: true));

            var result = miniGame.Evaluate();

            Assert.False(result.Success);
            Assert.True(result.Score01 < 0.6f);
        }

        [Fact]
        public void should_fail_with_zero_score_when_no_tap_before_timeout()
        {
            var miniGame = new TimingBarMiniGame();
            miniGame.Initialize(MakeContext(duration: 1f));

            miniGame.OnInputSample(new MiniGameInputSample(0.0, 0f, 0f, false));
            miniGame.OnInputSample(new MiniGameInputSample(1.5, 0f, 0f, false)); // dépasse la durée sans tap

            Assert.True(miniGame.IsComplete);
            var result = miniGame.Evaluate();

            Assert.False(result.Success);
            Assert.Equal(0f, result.Score01);
            Assert.Equal(ItemQuality.Failed, result.Quality);
        }

        [Fact]
        public void should_ignore_input_samples_received_after_completion()
        {
            var miniGame = new TimingBarMiniGame();
            miniGame.Initialize(MakeContext());

            miniGame.OnInputSample(new MiniGameInputSample(0.0, 0f, 0f, false));
            miniGame.OnInputSample(new MiniGameInputSample(1.0, 0.5f, 0f, true));
            var firstResult = miniGame.Evaluate();

            // Un second tap, tardif, ne doit rien changer (protection contre un double
            // input ou un event tardif de la couche Presentation).
            miniGame.OnInputSample(new MiniGameInputSample(1.9, 0.95f, 0f, true));
            var secondResult = miniGame.Evaluate();

            Assert.Equal(firstResult.Score01, secondResult.Score01);
        }

        [Fact]
        public void should_scale_difficulty_multiplier_by_shortening_effective_duration()
        {
            var easyGame = new TimingBarMiniGame();
            easyGame.Initialize(new MiniGameContext
            {
                ActionId = "a",
                DifficultyMultiplier = 1f,
                Config = new MiniGameConfig { DurationSeconds = 2f, TargetWindowStart = 0.4f, TargetWindowEnd = 0.6f },
            });

            var hardGame = new TimingBarMiniGame();
            hardGame.Initialize(new MiniGameContext
            {
                ActionId = "a",
                DifficultyMultiplier = 2f, // curseur deux fois plus rapide
                Config = new MiniGameConfig { DurationSeconds = 2f, TargetWindowStart = 0.4f, TargetWindowEnd = 0.6f },
            });

            // Même timestamp de tap (1.0s) : sur la version "easy" (durée effective 2s),
            // le curseur est au centre (0.5) -> excellent score. Sur la version "hard"
            // (durée effective 1s), le curseur a déjà atteint le bout (1.0) -> mauvais score.
            easyGame.OnInputSample(new MiniGameInputSample(0.0, 0f, 0f, false));
            easyGame.OnInputSample(new MiniGameInputSample(1.0, 0f, 0f, true));

            hardGame.OnInputSample(new MiniGameInputSample(0.0, 0f, 0f, false));
            hardGame.OnInputSample(new MiniGameInputSample(1.0, 0f, 0f, true));

            var easyScore = easyGame.Evaluate().Score01;
            var hardScore = hardGame.Evaluate().Score01;

            Assert.True(easyScore > hardScore);
        }
    }
}
