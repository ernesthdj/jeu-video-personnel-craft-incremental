namespace Game.Core.Combat
{
    /// <summary>
    /// Phases de la FSM de tour (SYSTEMS.md §4, ARCHITECTURE.md §6) :
    /// idle -> sélection action -> mini-jeu -> résolution -> tour suivant.
    /// </summary>
    public enum CombatPhase
    {
        TurnStart,
        ActionSelection,
        MiniGame,
        Resolution,
        TurnEnd,
        Ended,
    }
}
