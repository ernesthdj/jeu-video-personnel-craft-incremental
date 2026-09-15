using Game.Core.Crafting;
using Game.Core.MiniGames;

namespace Game.Core.Validation
{
    /// <summary>
    /// Backend serveur-autoritaire minimal exigé dès le MVP (SCOPE.md, TECH-STACK.md §4) :
    /// borne les scores de mini-jeu à un intervalle plausible avant qu'ils n'atteignent
    /// ICraftProcess/IActionResolver — condition nécessaire même en local (SYSTEMS.md §6 :
    /// "ne jamais faire confiance au résultat brut du client"). Implémentation locale pour
    /// la vertical slice (LocalValidationService), swappable vers un vrai serveur plus tard.
    /// </summary>
    public interface IAuthoritativeValidationService
    {
        /// <summary>Retourne un résultat borné à [0,1] — clamp défensif, jamais un rejet silencieux qui bloquerait le joueur.</summary>
        MiniGameResult ValidateMiniGameResult(MiniGameResult rawResult);

        bool ValidateRecipeInputs(CraftRecipe recipe, System.Func<string, int, bool> hasQuantity);
    }
}
