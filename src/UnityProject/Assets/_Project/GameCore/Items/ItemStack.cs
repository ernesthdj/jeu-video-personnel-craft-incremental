namespace Game.Core.Items
{
    /// <summary>Quantité d'un type d'item donné — utilisé pour les inputs/outputs de recette.</summary>
    public readonly struct ItemStack
    {
        public string ItemDefinitionId { get; }
        public int Quantity { get; }

        public ItemStack(string itemDefinitionId, int quantity)
        {
            ItemDefinitionId = itemDefinitionId;
            Quantity = quantity;
        }
    }
}
