using Game.Core.Items;

namespace Game.Core.Crafting
{
    /// <summary>Résultat d'une tentative de craft, transmis à l'UI via GameEventBus.</summary>
    public sealed class CraftResult
    {
        public string RecipeId { get; init; } = string.Empty;
        public bool Success { get; init; }
        public bool ItemDestroyed { get; init; }
        public ItemQuality Quality { get; init; }
        public ItemInstance? ProducedItem { get; init; }
        public float Score01 { get; init; }
    }
}
