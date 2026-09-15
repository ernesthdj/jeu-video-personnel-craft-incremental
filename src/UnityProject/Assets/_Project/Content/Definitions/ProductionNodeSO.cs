using UnityEngine;

namespace Game.Content.Definitions
{
    /// <summary>
    /// LIMITATION CONNUE : dépend de UnityEngine, non compilable hors éditeur (voir
    /// docs/IMPLEMENTATION.md). Hors scope du vertical slice (VERTICAL-SLICE.md §3) —
    /// posé ici uniquement comme point d'extension pour la donnée, cohérent avec
    /// IProductionNode côté GameCore.
    /// </summary>
    [CreateAssetMenu(fileName = "ProductionNode", menuName = "CraftIncremental/Farm/Noeud de production", order = 0)]
    public sealed class ProductionNodeSO : ScriptableObject
    {
        [SerializeField] private string _id = string.Empty;
        [SerializeField] private ItemDefinitionSO _producedItem = null!;
        [SerializeField, Min(0f)] private float _ratePerSecond = 0.1f;
        [SerializeField, Min(1)] private int _storageCap = 100;

        public string Id => _id;
        public ItemDefinitionSO ProducedItem => _producedItem;
        public float RatePerSecond => _ratePerSecond;
        public int StorageCap => _storageCap;
    }
}
