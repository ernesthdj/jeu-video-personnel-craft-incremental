using System;
using Game.Core.Events;
using Game.Core.Inventory;
using Game.Core.Items;
using Game.Core.MiniGames;

namespace Game.Core.Crafting
{
    /// <summary>
    /// Implémentation MVP du craft manuel (VERTICAL-SLICE.md étape 4) : consomme les
    /// matériaux de la recette, mappe le score du mini-jeu sur un palier de qualité via
    /// <see cref="MiniGameConfig.ResolveQuality"/>, et gère l'échec destructif (score
    /// sous le seuil configuré + recette qui l'autorise — SYSTEMS.md §1, jamais codé en
    /// dur, toujours piloté par <see cref="CraftRecipe.MiniGameConfig"/>).
    ///
    /// Validation serveur-autoritaire minimale (SCOPE.md) : les matériaux sont vérifiés
    /// avant consommation, jamais fait confiance à un appelant qui n'aurait pas déjà
    /// vérifié côté UI (LocalValidationService complète ce rôle plus haut dans la pile).
    /// </summary>
    public sealed class ManualCraftProcess : ICraftProcess
    {
        private readonly IInventoryService _inventory;
        private readonly IItemFactory _itemFactory;
        private readonly GameEventBus? _eventBus;

        public ManualCraftProcess(IInventoryService inventory, IItemFactory itemFactory, GameEventBus? eventBus = null)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory));
            _eventBus = eventBus;
        }

        public CraftResult Resolve(CraftRecipe recipe, MiniGameResult miniGameResult)
        {
            if (recipe is null) throw new ArgumentNullException(nameof(recipe));

            foreach (var input in recipe.Inputs)
            {
                if (!_inventory.HasQuantity(input.ItemDefinitionId, input.Quantity))
                {
                    throw new InvalidOperationException(
                        $"Matériaux insuffisants pour la recette '{recipe.Id}' : " +
                        $"'{input.ItemDefinitionId}' requiert {input.Quantity}, " +
                        $"disponible {_inventory.GetQuantity(input.ItemDefinitionId)}.");
                }
            }

            // Les matériaux investis sont consommés dès la tentative, réussie ou non —
            // c'est ce qui rend l'échec destructif réellement risqué (GDD §4, boucle
            // économique : la destruction d'items est un sink de ressources volontaire).
            foreach (var input in recipe.Inputs)
            {
                _inventory.RemoveItem(input.ItemDefinitionId, input.Quantity);
            }

            var config = recipe.MiniGameConfig;
            var isDestructiveFailure = config.AllowsDestructiveFailure
                && miniGameResult.Score01 < config.DestructiveFailureThreshold;

            if (isDestructiveFailure)
            {
                var lostItem = _itemFactory.CreateFromDefinition(recipe.Output, ItemQuality.Failed);
                _eventBus?.RaiseItemDestroyed(lostItem);

                var destroyedResult = new CraftResult
                {
                    RecipeId = recipe.Id,
                    Success = false,
                    ItemDestroyed = true,
                    Quality = ItemQuality.Failed,
                    ProducedItem = null,
                    Score01 = miniGameResult.Score01,
                };
                _eventBus?.RaiseCraftCompleted(destroyedResult);
                return destroyedResult;
            }

            var quality = config.ResolveQuality(miniGameResult.Score01);
            var producedItem = _itemFactory.CreateFromDefinition(recipe.Output, quality);
            _inventory.AddItem(producedItem);

            var result = new CraftResult
            {
                RecipeId = recipe.Id,
                Success = miniGameResult.Success,
                ItemDestroyed = false,
                Quality = quality,
                ProducedItem = producedItem,
                Score01 = miniGameResult.Score01,
            };
            _eventBus?.RaiseCraftCompleted(result);
            return result;
        }
    }
}
