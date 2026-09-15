using System;
using Game.Core.Production;
using Xunit;

namespace Game.Core.Tests.Production
{
    public class OfflineProgressionCalculatorTests
    {
        private sealed class FakeNode : IProductionNode
        {
            public string Id => "farm_bois";
            public string ItemDefinitionId => "bois";
            public float RatePerSecond { get; init; } = 1f;
            public int StorageCap { get; init; } = 100;
            public DateTimeOffset LastCollectedAtUtc { get; init; }
        }

        [Fact]
        public void should_produce_quantity_proportional_to_elapsed_time()
        {
            var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
            var node = new FakeNode { RatePerSecond = 2f, LastCollectedAtUtc = now.AddSeconds(-10) };

            var result = OfflineProgressionCalculator.Calculate(node, now, currentStoredQuantity: 0);

            Assert.Equal(20, result.QuantityProduced); // 2/s * 10s
            Assert.False(result.StorageCapped);
        }

        [Fact]
        public void should_cap_production_at_available_storage_capacity()
        {
            var now = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
            // Joueur absent longtemps -> production brute largement supérieure au cap
            // (SYSTEMS.md §3 : "empêche un joueur absent 2 semaines de revenir avec un
            // avantage disproportionné").
            var node = new FakeNode { RatePerSecond = 5f, StorageCap = 50, LastCollectedAtUtc = now.AddDays(-14) };

            var result = OfflineProgressionCalculator.Calculate(node, now, currentStoredQuantity: 10);

            Assert.Equal(40, result.QuantityProduced); // cap 50 - stock actuel 10
            Assert.True(result.StorageCapped);
        }
    }
}
