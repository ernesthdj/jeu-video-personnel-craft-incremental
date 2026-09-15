namespace Game.Core.MiniGames
{
    /// <summary>
    /// Contexte transmis à <see cref="IMiniGame.Initialize"/> : quelle action déclenche le
    /// mini-jeu, avec quels paramètres de difficulté. La difficulté adaptative complète
    /// (profil casual/try-hard détecté) est classée IMPORTANT et non CORE (SCOPE.md) —
    /// <see cref="DifficultyMultiplier"/> est le point d'extension minimal qui permet de la
    /// brancher plus tard sans changer la signature de l'interface.
    /// </summary>
    public sealed class MiniGameContext
    {
        public string ActionId { get; init; } = string.Empty;
        public MiniGameConfig Config { get; init; } = new();

        /// <summary>1.0 = difficulté nominale. &lt;1 = plus facile, &gt;1 = plus dur.</summary>
        public float DifficultyMultiplier { get; init; } = 1.0f;
    }
}
