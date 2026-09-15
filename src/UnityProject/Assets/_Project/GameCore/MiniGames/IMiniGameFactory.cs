namespace Game.Core.MiniGames
{
    /// <summary>
    /// Factory pattern (ARCHITECTURE.md §2.2) — résout un identifiant de type de mini-jeu
    /// (venant d'un MiniGameConfigSO) vers une instance concrète, sans jamais exposer les
    /// classes concrètes aux appelants (craft/raffinage/combat).
    /// </summary>
    public interface IMiniGameFactory
    {
        IMiniGame Create(string miniGameTypeId);
    }
}
