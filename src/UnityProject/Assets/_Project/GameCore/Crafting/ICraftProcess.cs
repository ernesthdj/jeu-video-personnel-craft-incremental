using Game.Core.MiniGames;

namespace Game.Core.Crafting
{
    /// <summary>
    /// Strategy pattern (ARCHITECTURE.md §3) — résolution d'un craft à partir d'une
    /// recette et du résultat déjà évalué du mini-jeu associé. La séparation entre
    /// "jouer le mini-jeu" (IMiniGame) et "résoudre le craft" (ICraftProcess) permet de
    /// tester chaque brique indépendamment.
    /// </summary>
    public interface ICraftProcess
    {
        CraftResult Resolve(CraftRecipe recipe, MiniGameResult miniGameResult);
    }
}
