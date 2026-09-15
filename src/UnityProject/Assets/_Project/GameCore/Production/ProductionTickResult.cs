namespace Game.Core.Production
{
    /// <summary>Résultat d'un calcul de production (en ligne ou hors-ligne) pour un nœud donné.</summary>
    public sealed class ProductionTickResult
    {
        public string ProductionNodeId { get; init; } = string.Empty;
        public string ItemDefinitionId { get; init; } = string.Empty;
        public int QuantityProduced { get; init; }
        public bool StorageCapped { get; init; }
    }
}
