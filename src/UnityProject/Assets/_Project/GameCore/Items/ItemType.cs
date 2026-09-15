namespace Game.Core.Items
{
    /// <summary>Catégorie d'item (FOUNDATION.md §3, table Item : "type craft manuel/raffiné/équipement/combat").</summary>
    public enum ItemType
    {
        RawMaterial,
        CraftedBase,
        RefinedMaterial,
        Equipment,
        CombatLoot,
    }
}
