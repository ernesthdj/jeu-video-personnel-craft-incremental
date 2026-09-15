using System;
using System.Collections.Generic;
using Game.Core.Items;

namespace Game.Core.Inventory
{
    /// <summary>
    /// Implémentation MVP locale (ARCHITECTURE.md §3). Pas de synchronisation réseau —
    /// c'est le rôle futur d'une implémentation backend qui respectera la même interface.
    /// </summary>
    public sealed class LocalInventoryService : IInventoryService
    {
        private readonly Dictionary<string, int> _quantities = new();
        private readonly List<ItemInstance> _instances = new();

        public int GetQuantity(string itemDefinitionId) =>
            _quantities.TryGetValue(itemDefinitionId, out var quantity) ? quantity : 0;

        public bool HasQuantity(string itemDefinitionId, int quantity) =>
            GetQuantity(itemDefinitionId) >= quantity;

        public void AddItem(ItemInstance item, int quantity = 1)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            _instances.Add(item);
            AddRawQuantity(item.Definition.Id, quantity);
        }

        public void AddRawQuantity(string itemDefinitionId, int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "La quantité ajoutée doit être positive.");
            _quantities[itemDefinitionId] = GetQuantity(itemDefinitionId) + quantity;
        }

        public void RemoveItem(string itemDefinitionId, int quantity)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "La quantité retirée doit être positive.");

            var current = GetQuantity(itemDefinitionId);
            if (current < quantity)
            {
                // Validation serveur-autoritaire (SCOPE.md — "backend serveur-autoritaire
                // minimal") : ne jamais faire confiance à un appelant qui n'aurait pas
                // vérifié le stock en amont.
                throw new InvalidOperationException(
                    $"Stock insuffisant pour '{itemDefinitionId}' : demandé {quantity}, disponible {current}.");
            }

            _quantities[itemDefinitionId] = current - quantity;
        }

        public IReadOnlyList<ItemInstance> Instances => _instances;

        public IReadOnlyDictionary<string, int> Snapshot() => new Dictionary<string, int>(_quantities);
    }
}
