using Game.Core.Items;
using UnityEngine;

namespace Game.Content.Definitions
{
    /// <summary>LIMITATION CONNUE : dépend de UnityEngine, non compilable hors éditeur (voir docs/IMPLEMENTATION.md).</summary>
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "CraftIncremental/Item/Definition", order = 0)]
    public sealed class ItemDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _id = string.Empty;
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private ItemType _type;
        [SerializeField] private int _baseValue;
        [SerializeField] private Sprite? _icon; // Purement présentation — jamais lu par GameCore.

        public string Id => _id;

        public ItemDefinition ToDefinition() => new()
        {
            Id = _id,
            DisplayName = _displayName,
            Type = _type,
            BaseValue = _baseValue,
        };

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                Debug.LogWarning($"ItemDefinitionSO '{name}' a un Id vide — il ne pourra pas être résolu par IItemFactory.", this);
            }
        }
    }
}
