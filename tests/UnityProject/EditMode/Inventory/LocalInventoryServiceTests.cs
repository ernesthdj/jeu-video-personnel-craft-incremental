using System;
using Game.Core.Inventory;
using Game.Core.Items;
using Xunit;

namespace Game.Core.Tests.Inventory
{
    public class LocalInventoryServiceTests
    {
        [Fact]
        public void should_track_quantity_added_via_raw_quantity()
        {
            var inventory = new LocalInventoryService();
            inventory.AddRawQuantity("fer_brut", 5);

            Assert.Equal(5, inventory.GetQuantity("fer_brut"));
            Assert.True(inventory.HasQuantity("fer_brut", 5));
            Assert.False(inventory.HasQuantity("fer_brut", 6));
        }

        [Fact]
        public void should_throw_when_removing_more_than_available_stock()
        {
            var inventory = new LocalInventoryService();
            inventory.AddRawQuantity("fer_brut", 2);

            Assert.Throws<InvalidOperationException>(() => inventory.RemoveItem("fer_brut", 3));
        }

        [Fact]
        public void should_add_item_instance_and_increment_its_definition_quantity()
        {
            var inventory = new LocalInventoryService();
            var definition = new ItemDefinition { Id = "epee_courte", DisplayName = "Épée courte", Type = ItemType.Equipment };
            var item = new ItemFactory().CreateFromDefinition(definition, ItemQuality.Rare);

            inventory.AddItem(item);

            Assert.Equal(1, inventory.GetQuantity("epee_courte"));
            Assert.Single(inventory.Instances);
            Assert.Equal(ItemQuality.Rare, inventory.Instances[0].Quality);
        }
    }
}
