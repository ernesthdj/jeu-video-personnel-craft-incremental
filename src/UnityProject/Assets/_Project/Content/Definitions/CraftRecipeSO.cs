using System.Collections.Generic;
using Game.Core.Crafting;
using Game.Core.Items;
using UnityEngine;

namespace Game.Content.Definitions
{
    /// <summary>LIMITATION CONNUE : dépend de UnityEngine, non compilable hors éditeur (voir docs/IMPLEMENTATION.md).</summary>
    [CreateAssetMenu(fileName = "CraftRecipe", menuName = "CraftIncremental/Craft/Recette", order = 0)]
    public sealed class CraftRecipeSO : ScriptableObject
    {
        [SerializeField] private string _id = string.Empty;
        [SerializeField] private string _displayName = string.Empty;

        [Header("Matériaux requis")]
        [SerializeField] private List<ItemStackEntry> _inputs = new();

        [Header("Résultat")]
        [SerializeField] private ItemDefinitionSO _output = null!;

        [Header("Mini-jeu associé")]
        [SerializeField] private MiniGameConfigSO _miniGameConfig = null!;

        public CraftRecipe ToRecipe()
        {
            var inputs = new List<ItemStack>(_inputs.Count);
            foreach (var entry in _inputs)
            {
                inputs.Add(new ItemStack(entry.ItemDefinition != null ? entry.ItemDefinition.Id : string.Empty, entry.Quantity));
            }

            return new CraftRecipe
            {
                Id = _id,
                DisplayName = _displayName,
                Inputs = inputs,
                Output = _output != null ? _output.ToDefinition() : null!,
                MiniGameConfig = _miniGameConfig != null ? _miniGameConfig.ToConfig() : new Game.Core.MiniGames.MiniGameConfig(),
            };
        }

        [System.Serializable]
        public struct ItemStackEntry
        {
            public ItemDefinitionSO ItemDefinition;
            [Min(1)] public int Quantity;
        }

        private void OnValidate()
        {
            if (_output == null)
            {
                Debug.LogWarning($"CraftRecipeSO '{name}' n'a pas d'Output défini.", this);
            }

            if (_miniGameConfig == null)
            {
                Debug.LogWarning($"CraftRecipeSO '{name}' n'a pas de MiniGameConfig associé.", this);
            }
        }
    }
}
