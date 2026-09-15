using System;
using Game.Core.Combat;
using Game.Core.Crafting;
using Game.Core.Items;
using Game.Core.Production;

namespace Game.Core.Events
{
    /// <summary>
    /// Observer pattern (ARCHITECTURE.md §2.4). Réservé aux événements de FIN d'action
    /// (craft terminé, item détruit, action de combat résolue, tick de production) — le
    /// flux continu d'input d'un mini-jeu ne passe JAMAIS par ce bus (lu en direct depuis
    /// IMiniGame côté Presentation, voir alerte Technical Director / UI-UX : c'est la
    /// condition qui protège la contrainte de latence &lt; 100ms du mini-jeu).
    /// UI, inventaire, audio, feedback visuel s'abonnent sans connaître la logique de
    /// craft/combat.
    /// </summary>
    public sealed class GameEventBus
    {
        public event Action<CraftResult>? CraftCompleted;
        public event Action<ItemInstance>? ItemDestroyed;
        public event Action<CombatActionResult>? CombatActionResolved;
        public event Action<ProductionTickResult>? ProductionTicked;

        public void RaiseCraftCompleted(CraftResult result) => CraftCompleted?.Invoke(result);
        public void RaiseItemDestroyed(ItemInstance item) => ItemDestroyed?.Invoke(item);
        public void RaiseCombatActionResolved(CombatActionResult result) => CombatActionResolved?.Invoke(result);
        public void RaiseProductionTicked(ProductionTickResult result) => ProductionTicked?.Invoke(result);
    }
}
