using System.Collections.Generic;
using Game.Core.Items;
using Game.Core.MiniGames;

namespace Game.Core.Crafting
{
    /// <summary>
    /// Donnée pure (miroir GameCore de CraftRecipeSO, ARCHITECTURE.md §4). Le flag
    /// "échec destructif" doit être réservé aux recettes avancées — jamais au tutoriel
    /// (SYSTEMS.md §1, GDD Selfdoubt #3) ; c'est une décision de contenu (côté Content),
    /// pas de logique ici.
    /// </summary>
    public sealed class CraftRecipe
    {
        public string Id { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public IReadOnlyList<ItemStack> Inputs { get; init; } = new List<ItemStack>();
        public ItemDefinition Output { get; init; } = null!;
        public MiniGameConfig MiniGameConfig { get; init; } = new();
    }
}
