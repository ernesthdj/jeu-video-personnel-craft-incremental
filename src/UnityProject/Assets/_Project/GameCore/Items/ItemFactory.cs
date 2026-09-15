using System;

namespace Game.Core.Items
{
    public sealed class ItemFactory : IItemFactory
    {
        public ItemInstance CreateFromDefinition(ItemDefinition definition, ItemQuality quality) =>
            new(Guid.NewGuid().ToString("N"), definition, quality);
    }
}
