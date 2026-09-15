using System;
using System.Collections.Generic;
using Game.Core.Crafting;
using Game.Core.Events;
using Game.Core.Inventory;
using Game.Core.Items;
using Game.Core.MiniGames;
using Xunit;

namespace Game.Core.Tests.Crafting
{
    /// <summary>
    /// Cycle craft manuel complet (VERTICAL-SLICE.md étape 4) : recette -> résultat de
    /// mini-jeu -> qualité ou destruction -> mise à jour d'inventaire -> événements.
    /// C'est la validation concrète du Strategy pattern (ICraftProcess) et du Pillar 1 du
    /// GDD ("l'habileté prime sur le hasard" — le score pilote directement le résultat).
    /// </summary>
    public class ManualCraftProcessTests
    {
        private static readonly ItemDefinition FerBrut = new() { Id = "fer_brut", DisplayName = "Fer brut", Type = ItemType.RawMaterial };
        private static readonly ItemDefinition Bois = new() { Id = "bois", DisplayName = "Bois", Type = ItemType.RawMaterial };
        private static readonly ItemDefinition EpeeCourte = new() { Id = "epee_courte", DisplayName = "Épée courte", Type = ItemType.Equipment };

        private static CraftRecipe MakeRecipe(bool allowsDestructive = true) => new()
        {
            Id = "epee_courte",
            DisplayName = "Épée courte",
            Inputs = new List<ItemStack> { new(FerBrut.Id, 3), new(Bois.Id, 1) },
            Output = EpeeCourte,
            MiniGameConfig = new MiniGameConfig
            {
                MiniGameTypeId = "trace_precision",
                AllowsDestructiveFailure = allowsDestructive,
                DestructiveFailureThreshold = 0.15f,
                QualityThresholds = new List<QualityThreshold>
                {
                    new(0.9f, ItemQuality.Perfect),
                    new(0.6f, ItemQuality.Rare),
                    new(0.15f, ItemQuality.Common),
                },
            },
        };

        private static (ManualCraftProcess Process, LocalInventoryService Inventory, GameEventBus EventBus) MakeSut()
        {
            var inventory = new LocalInventoryService();
            inventory.AddRawQuantity(FerBrut.Id, 5);
            inventory.AddRawQuantity(Bois.Id, 2);

            var eventBus = new GameEventBus();
            var process = new ManualCraftProcess(inventory, new ItemFactory(), eventBus);
            return (process, inventory, eventBus);
        }

        [Fact]
        public void should_produce_perfect_item_and_consume_materials_when_score_is_excellent()
        {
            var (process, inventory, eventBus) = MakeSut();
            CraftResult? raised = null;
            eventBus.CraftCompleted += r => raised = r;

            var result = process.Resolve(MakeRecipe(), new MiniGameResult(0.97f, true, ItemQuality.Perfect));

            Assert.True(result.Success);
            Assert.False(result.ItemDestroyed);
            Assert.Equal(ItemQuality.Perfect, result.Quality);
            Assert.NotNull(result.ProducedItem);

            Assert.Equal(2, inventory.GetQuantity(FerBrut.Id)); // 5 - 3
            Assert.Equal(1, inventory.GetQuantity(Bois.Id));    // 2 - 1
            Assert.Equal(1, inventory.GetQuantity(EpeeCourte.Id));

            Assert.NotNull(raised);
            Assert.Equal(result.Quality, raised!.Quality);
        }

        [Fact]
        public void should_destroy_item_and_consume_materials_when_score_is_below_destructive_threshold()
        {
            var (process, inventory, eventBus) = MakeSut();
            ItemInstance? destroyed = null;
            eventBus.ItemDestroyed += item => destroyed = item;

            var result = process.Resolve(MakeRecipe(allowsDestructive: true), new MiniGameResult(0.05f, false, ItemQuality.Failed));

            Assert.False(result.Success);
            Assert.True(result.ItemDestroyed);
            Assert.Null(result.ProducedItem);
            Assert.Equal(ItemQuality.Failed, result.Quality);

            // Les matériaux sont perdus même en cas d'échec destructif (GDD §4 : sink de
            // ressources volontaire, boucle économique).
            Assert.Equal(2, inventory.GetQuantity(FerBrut.Id));
            Assert.Equal(1, inventory.GetQuantity(Bois.Id));
            Assert.Equal(0, inventory.GetQuantity(EpeeCourte.Id));

            Assert.NotNull(destroyed);
        }

        [Fact]
        public void should_not_destroy_item_when_recipe_does_not_allow_destructive_failure()
        {
            var (process, inventory, eventBus) = MakeSut();
            var destroyedRaised = false;
            eventBus.ItemDestroyed += _ => destroyedRaised = true;

            // Même score catastrophique, mais recette tutoriel (SYSTEMS.md §1 : jamais de
            // destruction sur une recette de base).
            var result = process.Resolve(MakeRecipe(allowsDestructive: false), new MiniGameResult(0.02f, false, ItemQuality.Failed));

            Assert.False(result.ItemDestroyed);
            Assert.False(destroyedRaised);
            Assert.NotNull(result.ProducedItem); // objet produit en qualité plancher, jamais détruit
            Assert.Equal(1, inventory.GetQuantity(EpeeCourte.Id));
        }

        [Fact]
        public void should_throw_and_never_consume_materials_when_inventory_is_insufficient()
        {
            var inventory = new LocalInventoryService();
            inventory.AddRawQuantity(FerBrut.Id, 1); // il en faut 3
            var process = new ManualCraftProcess(inventory, new ItemFactory());

            Assert.Throws<InvalidOperationException>(() =>
                process.Resolve(MakeRecipe(), new MiniGameResult(1f, true, ItemQuality.Perfect)));

            // Validation serveur-autoritaire : rien n'est consommé si la validation échoue.
            Assert.Equal(1, inventory.GetQuantity(FerBrut.Id));
        }

        [Theory]
        [InlineData(0.95f, ItemQuality.Perfect)]
        [InlineData(0.7f, ItemQuality.Rare)]
        [InlineData(0.3f, ItemQuality.Common)]
        public void should_map_score_to_quality_tier_per_config_thresholds(float score, ItemQuality expectedQuality)
        {
            var (process, _, _) = MakeSut();

            var result = process.Resolve(MakeRecipe(), new MiniGameResult(score, score >= 0.6f, ItemQuality.Common));

            Assert.Equal(expectedQuality, result.Quality);
        }
    }
}
