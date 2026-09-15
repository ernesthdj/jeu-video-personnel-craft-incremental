using System;

namespace Game.Core.Production
{
    /// <summary>
    /// Calcul pur de la production accumulée pendant l'absence du joueur (delta-temps),
    /// plafonnée par le cap de stockage (SYSTEMS.md §3 : "empêche un joueur absent 2
    /// semaines de revenir avec un avantage disproportionné"). Ne dépend d'aucun état
    /// runtime — testable isolément (VERTICAL-SLICE.md §4).
    /// </summary>
    public static class OfflineProgressionCalculator
    {
        public static ProductionTickResult Calculate(
            IProductionNode node,
            DateTimeOffset nowUtc,
            int currentStoredQuantity)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));

            var elapsedSeconds = Math.Max(0, (nowUtc - node.LastCollectedAtUtc).TotalSeconds);
            var rawProduced = (int)Math.Floor(elapsedSeconds * node.RatePerSecond);

            var availableCapacity = Math.Max(0, node.StorageCap - currentStoredQuantity);
            var actuallyProduced = Math.Min(rawProduced, availableCapacity);
            var storageCapped = rawProduced > availableCapacity;

            return new ProductionTickResult
            {
                ProductionNodeId = node.Id,
                ItemDefinitionId = node.ItemDefinitionId,
                QuantityProduced = actuallyProduced,
                StorageCapped = storageCapped,
            };
        }
    }
}
