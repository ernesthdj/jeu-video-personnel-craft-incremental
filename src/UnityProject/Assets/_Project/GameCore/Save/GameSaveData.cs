using System;
using System.Collections.Generic;

namespace Game.Core.Save
{
    /// <summary>
    /// Instantané sérialisable de l'état joueur persisté (VERTICAL-SLICE.md étape 5 :
    /// "le cycle complet survit à une fermeture/réouverture de l'app"). Volontairement
    /// réduit au strict nécessaire du vertical slice (inventaire) — pas d'état de combat
    /// (jamais sauvegardé en plein combat, cf. WORLD-MAP.md §3).
    /// </summary>
    public sealed class GameSaveData
    {
        public string PlayerId { get; set; } = string.Empty;
        public Dictionary<string, int> InventoryQuantities { get; set; } = new();
        public List<ItemInstanceSaveData> InventoryInstances { get; set; } = new();
        public DateTimeOffset LastSavedAtUtc { get; set; }
    }
}
