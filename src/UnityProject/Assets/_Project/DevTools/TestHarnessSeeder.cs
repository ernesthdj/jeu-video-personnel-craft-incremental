using Game.Core.Inventory;
using Game.Core.Registry;
using UnityEngine;

namespace Game.DevTools
{
    /// <summary>
    /// Harnais de test uniquement — jamais dans un build joueur réel. Donne au joueur les
    /// matériaux de départ nécessaires pour tester le craft manuel immédiatement en
    /// appuyant sur Play, sans étape manuelle supplémentaire.
    ///
    /// Doit s'exécuter après GameBootstrap (DefaultExecutionOrder -1000) mais avant les
    /// contrôleurs d'écran qui lisent l'inventaire dans leur propre Awake (ex.
    /// CraftManualController) — d'où cet ordre intermédiaire.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class TestHarnessSeeder : MonoBehaviour
    {
        [SerializeField] private string _itemDefinitionId = "wood";
        [SerializeField] private int _quantity = 10;

        private void Awake()
        {
            GameServices.Get<IInventoryService>().AddRawQuantity(_itemDefinitionId, _quantity);
            Debug.Log($"[TestHarnessSeeder] Stock de départ ajouté : {_itemDefinitionId} x{_quantity}.");
        }
    }
}
