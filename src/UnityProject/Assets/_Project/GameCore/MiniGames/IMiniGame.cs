namespace Game.Core.MiniGames
{
    /// <summary>
    /// Strategy pattern — type de mini-jeu interchangeable (ARCHITECTURE.md §2.1).
    /// Les systèmes appelants (craft, raffinage, combat) ne connaissent jamais une classe
    /// concrète : ils obtiennent une instance via <see cref="IMiniGameFactory"/>. C'est le
    /// système le plus critique du jeu (SYSTEMS.md §5, GDD Selfdoubt #1) : tout le reste de
    /// la boucle de jeu dépend de sa fiabilité et de sa réactivité (&lt; 100ms côté UI).
    /// </summary>
    public interface IMiniGame
    {
        /// <summary>Prépare le mini-jeu pour une nouvelle tentative (recette/action, difficulté).</summary>
        void Initialize(MiniGameContext context);

        /// <summary>
        /// Reçoit un échantillon d'input par frame pendant que le mini-jeu est actif.
        /// Doit rester bon marché (pas d'allocation lourde) — appelé potentiellement 30-60
        /// fois par seconde depuis Presentation.
        /// </summary>
        void OnInputSample(MiniGameInputSample sample);

        /// <summary>Vrai dès que le mini-jeu a assez d'information pour être évalué (timeout ou action déclenchante reçue).</summary>
        bool IsComplete { get; }

        /// <summary>Calcule le résultat final. Ne doit être appelé qu'une fois <see cref="IsComplete"/> vrai.</summary>
        MiniGameResult Evaluate();
    }
}
