using System;
using System.Collections.Generic;
using Game.Core.MiniGames;
using Xunit;

namespace Game.Core.Tests.MiniGames
{
    /// <summary>
    /// Vérifie le point d'extension central de l'architecture (ARCHITECTURE.md §2.3) :
    /// ajouter un mini-jeu = une classe + un enregistrement, zéro modification des
    /// systèmes appelants.
    /// </summary>
    public class MiniGameRegistryTests
    {
        [Fact]
        public void should_create_registered_minigame_type_by_id()
        {
            var registry = new MiniGameRegistry();
            registry.RegisterType("timing_bar", () => new TimingBarMiniGame());

            var instance = registry.Create("timing_bar");

            Assert.IsType<TimingBarMiniGame>(instance);
        }

        [Fact]
        public void should_throw_key_not_found_for_unregistered_id()
        {
            var registry = new MiniGameRegistry();

            Assert.Throws<KeyNotFoundException>(() => registry.Create("does_not_exist"));
        }

        [Fact]
        public void factory_consumers_never_need_to_know_the_concrete_type()
        {
            // Simule un système appelant (craft/combat) qui ne connaît que IMiniGameFactory.
            IMiniGameFactory factory = new MiniGameRegistry();
            ((MiniGameRegistry)factory).RegisterType("timing_bar", () => new TimingBarMiniGame());

            IMiniGame miniGame = factory.Create("timing_bar");
            miniGame.Initialize(new MiniGameContext { ActionId = "x", Config = new MiniGameConfig() });

            Assert.False(miniGame.IsComplete);
        }
    }
}
