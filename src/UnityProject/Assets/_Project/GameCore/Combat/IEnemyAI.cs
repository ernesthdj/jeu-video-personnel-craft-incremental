namespace Game.Core.Combat
{
    /// <summary>Strategy pattern — comportement d'IA ennemie, résolu par id via le registry (comme IMiniGame).</summary>
    public interface IEnemyAI
    {
        CombatDecision DecideAction(ICombatant self, CombatEncounterState state);
    }
}
