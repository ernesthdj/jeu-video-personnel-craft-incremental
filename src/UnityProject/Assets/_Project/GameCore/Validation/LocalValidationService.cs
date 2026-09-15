using System;
using Game.Core.Crafting;
using Game.Core.MiniGames;

namespace Game.Core.Validation
{
    /// <summary>
    /// Validation faite en local — honnête pour un test solo dev (VERTICAL-SLICE.md §3),
    /// swappable vers un vrai serveur sans changer les appelants (ManualCraftProcess,
    /// CombatStateMachine ne connaissent que le résultat déjà borné).
    /// </summary>
    public sealed class LocalValidationService : IAuthoritativeValidationService
    {
        public MiniGameResult ValidateMiniGameResult(MiniGameResult rawResult)
        {
            var clampedScore = Math.Clamp(rawResult.Score01, 0f, 1f);
            return clampedScore.Equals(rawResult.Score01)
                ? rawResult
                : new MiniGameResult(clampedScore, clampedScore > 0f, rawResult.Quality);
        }

        public bool ValidateRecipeInputs(CraftRecipe recipe, Func<string, int, bool> hasQuantity)
        {
            if (recipe is null) throw new ArgumentNullException(nameof(recipe));
            if (hasQuantity is null) throw new ArgumentNullException(nameof(hasQuantity));

            foreach (var input in recipe.Inputs)
            {
                if (!hasQuantity(input.ItemDefinitionId, input.Quantity))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
