using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Common;

namespace Game.Core.Combat
{
    /// <summary>
    /// État de la grille de combat en cours (participants + dimensions). Grille simple
    /// sans obstacles (décision World Builder, ZONE-DESIGN.md §0) — pas de pathfinding
    /// A* nécessaire pour le MVP.
    /// </summary>
    public sealed class CombatEncounterState
    {
        public IReadOnlyList<ICombatant> Combatants { get; }
        public int GridWidth { get; }
        public int GridHeight { get; }

        public CombatEncounterState(IReadOnlyList<ICombatant> combatants, int gridWidth, int gridHeight)
        {
            Combatants = combatants ?? throw new ArgumentNullException(nameof(combatants));
            GridWidth = gridWidth;
            GridHeight = gridHeight;
        }

        public IEnumerable<ICombatant> GetOpponentsOf(ICombatant combatant) =>
            Combatants.Where(c => c.IsPlayerControlled != combatant.IsPlayerControlled && !c.IsDefeated);

        public bool IsOccupied(GridPosition position) =>
            Combatants.Any(c => !c.IsDefeated && c.Position == position);

        public bool IsWithinBounds(GridPosition position) =>
            position.X >= 0 && position.X < GridWidth && position.Z >= 0 && position.Z < GridHeight;

        public bool AnyPlayerCombatantAlive() => Combatants.Any(c => c.IsPlayerControlled && !c.IsDefeated);
        public bool AnyEnemyCombatantAlive() => Combatants.Any(c => !c.IsPlayerControlled && !c.IsDefeated);
    }
}
