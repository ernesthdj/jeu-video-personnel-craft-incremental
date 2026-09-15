using System.Linq;
using Game.Core.Common;

namespace Game.Core.Combat
{
    /// <summary>
    /// Seul comportement d'IA du MVP (recommandation Technical Director, ARCHITECTURE.md
    /// §0 : "IA à un seul comportement" pour ne pas alourdir le système CORE le plus
    /// lourd). Se déplace vers l'adversaire le plus proche jusqu'à portée d'attaque
    /// (distance Chebyshev == 1, cohérent avec l'absence d'obstacles/A*), puis attaque.
    /// </summary>
    public sealed class AggressiveAI : IEnemyAI
    {
        private const int AttackRange = 1;

        public CombatDecision DecideAction(ICombatant self, CombatEncounterState state)
        {
            var target = state.GetOpponentsOf(self)
                .OrderBy(o => o.Position.ChebyshevDistanceTo(self.Position))
                .FirstOrDefault();

            if (target is null)
            {
                return CombatDecision.EndTurn();
            }

            var distance = self.Position.ChebyshevDistanceTo(target.Position);

            if (distance <= AttackRange)
            {
                // Déjà à portée : soit on attaque, soit il n'y a plus rien d'utile à
                // faire ce tour. Ne JAMAIS avancer davantage ici — un pas vers une cible
                // déjà adjacente atterrirait sur sa propre case (bug corrigé : cause une
                // exception "case occupée" dans CombatStateMachine.SubmitMove).
                return self.ActionPoints > 0 ? CombatDecision.Attack(target) : CombatDecision.EndTurn();
            }

            if (self.MovementPoints > 0)
            {
                var step = StepToward(self.Position, target.Position);
                return CombatDecision.Move(step);
            }

            return CombatDecision.EndTurn();
        }

        private static GridPosition StepToward(GridPosition from, GridPosition to)
        {
            var deltaX = Sign(to.X - from.X);
            var deltaZ = Sign(to.Z - from.Z);
            return new GridPosition(from.X + deltaX, from.Z + deltaZ);
        }

        private static int Sign(int value) => value == 0 ? 0 : (value > 0 ? 1 : -1);
    }
}
