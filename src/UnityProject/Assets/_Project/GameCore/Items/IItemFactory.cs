namespace Game.Core.Items
{
    /// <summary>Factory pattern (ARCHITECTURE.md §2.2) pour la création d'instances d'items.</summary>
    public interface IItemFactory
    {
        ItemInstance CreateFromDefinition(ItemDefinition definition, ItemQuality quality);
    }
}
