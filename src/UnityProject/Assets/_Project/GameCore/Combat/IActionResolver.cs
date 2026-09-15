using Game.Core.MiniGames;

namespace Game.Core.Combat
{
    /// <summary>
    /// Résout une action offensive en combinant les stats du combattant (build) et le
    /// score du mini-jeu (habileté du joueur) — la double couche de skill du GDD Selfdoubt
    /// #5. Réutilise <see cref="MiniGameResult"/> tel quel, cohérent avec INPUT-MAP.md §2.4
    /// (CombatActionMiniGame réutilise le squelette timing_bar/trace_precision).
    /// </summary>
    public interface IActionResolver
    {
        CombatActionResult ResolveAttack(ICombatant attacker, ICombatant target, MiniGameResult miniGameResult);
    }
}
