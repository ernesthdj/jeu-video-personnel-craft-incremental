using System;

namespace Game.Core.Items
{
    /// <summary>
    /// Instance concrète d'un item en possession du joueur (résultat d'un craft, loot de
    /// combat...). Porte sa propre qualité, contrairement à <see cref="ItemDefinition"/> qui
    /// décrit seulement le type.
    /// </summary>
    public sealed class ItemInstance
    {
        public string InstanceId { get; }
        public ItemDefinition Definition { get; }
        public ItemQuality Quality { get; }

        public ItemInstance(string instanceId, ItemDefinition definition, ItemQuality quality)
        {
            InstanceId = instanceId;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Quality = quality;
        }
    }
}
