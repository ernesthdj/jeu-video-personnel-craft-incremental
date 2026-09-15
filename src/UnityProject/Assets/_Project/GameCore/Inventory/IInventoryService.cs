using System.Collections.Generic;
using Game.Core.Items;

namespace Game.Core.Inventory
{
    /// <summary>
    /// Repository-like (ARCHITECTURE.md §3) pour l'inventaire du joueur. Swappable plus
    /// tard vers une implémentation synchronisée backend sans changer les appelants
    /// (craft, combat, UI).
    /// </summary>
    public interface IInventoryService
    {
        int GetQuantity(string itemDefinitionId);
        bool HasQuantity(string itemDefinitionId, int quantity);

        /// <summary>Ajoute une instance concrète (ex. résultat d'un craft) et incrémente sa quantité.</summary>
        void AddItem(ItemInstance item, int quantity = 1);

        /// <summary>Ajoute une quantité brute sans instance dédiée (ex. ressources de farm, stock de départ).</summary>
        void AddRawQuantity(string itemDefinitionId, int quantity);

        /// <summary>Retire une quantité. Lève si le stock est insuffisant (validation serveur-autoritaire).</summary>
        void RemoveItem(string itemDefinitionId, int quantity);

        IReadOnlyList<ItemInstance> Instances { get; }
        IReadOnlyDictionary<string, int> Snapshot();
    }
}
