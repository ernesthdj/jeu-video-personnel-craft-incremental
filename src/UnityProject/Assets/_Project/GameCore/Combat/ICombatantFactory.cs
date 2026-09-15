using Game.Core.Common;

namespace Game.Core.Combat
{
    /// <summary>Factory pattern (ARCHITECTURE.md §2.2) pour l'instanciation des combattants.</summary>
    public interface ICombatantFactory
    {
        ICombatant CreateFromDefinition(CombatantDefinition definition, GridPosition position);
    }
}
