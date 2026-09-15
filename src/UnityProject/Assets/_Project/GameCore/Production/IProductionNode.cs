using System;

namespace Game.Core.Production
{
    /// <summary>
    /// Point d'extension pour l'automatisation/farm (SYSTEMS.md §3). Explicitement hors du
    /// périmètre du vertical slice (VERTICAL-SLICE.md §3 : "l'interface peut être posée
    /// dans GameCore sans implémentation runtime complète dans le slice") — le craft manuel
    /// et le combat sont la priorité. Seul <see cref="OfflineProgressionCalculator"/> (le
    /// calcul pur derrière ce système) est implémenté ici car il est peu coûteux et
    /// testable indépendamment de tout nœud runtime concret.
    /// </summary>
    public interface IProductionNode
    {
        string Id { get; }
        string ItemDefinitionId { get; }
        float RatePerSecond { get; }
        int StorageCap { get; }
        DateTimeOffset LastCollectedAtUtc { get; }
    }
}
