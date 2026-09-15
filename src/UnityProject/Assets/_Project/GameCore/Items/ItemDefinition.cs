namespace Game.Core.Items
{
    /// <summary>
    /// Donnée pure décrivant un type d'item (miroir GameCore de ItemDefinitionSO,
    /// ARCHITECTURE.md §4). Pas de logique ici — uniquement des champs, validés côté
    /// Content en amont.
    /// </summary>
    public sealed class ItemDefinition
    {
        public string Id { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public ItemType Type { get; init; }
        public int BaseValue { get; init; }
    }
}
