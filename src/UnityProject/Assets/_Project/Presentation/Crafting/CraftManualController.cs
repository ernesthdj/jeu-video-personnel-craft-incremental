#nullable enable
using Game.Content.Definitions;
using Game.Core.Crafting;
using Game.Core.Inventory;
using Game.Core.MiniGames;
using Game.Core.Registry;
using Game.Presentation.MiniGames;
using Game.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Crafting
{
    /// <summary>
    /// LIMITATION CONNUE : dépend de UnityEngine/UnityEngine.UI, non compilable hors
    /// éditeur (voir docs/IMPLEMENTATION.md).
    ///
    /// Écran Craft manuel (UI-SCREENS.md §3.2) : matériaux requis vs possédés, badge
    /// "échec = destruction" toujours visible, CTA unique "Forger" désactivé (pas caché)
    /// tant que les matériaux manquent (Nielsen #5). Toute la résolution passe par
    /// ICraftProcess + IMiniGameFactory — zéro accès direct à une classe concrète de
    /// mini-jeu ou de logique de craft (contrainte architecturale du Technical Director,
    /// rappelée en tête de UI-SCREENS.md).
    /// </summary>
    public sealed class CraftManualController : MonoBehaviour
    {
        [SerializeField] private CraftRecipeSO _recipe = null!;
        [SerializeField] private TimingBarMiniGameView _miniGameView = null!;
        [SerializeField] private Button _forgeButton = null!;
        [SerializeField] private Text _materialsListText = null!; // uGUI Text ou TMP_Text selon prefab final
        [SerializeField] private Text _destructiveWarningText = null!;
        [SerializeField] private CraftResultPopupController? _resultPopup;

        private ICraftProcess _craftProcess = null!;
        private IInventoryService _inventory = null!;
        private IMiniGameFactory _miniGameFactory = null!;

        private void Awake()
        {
            _inventory = GameServices.Get<IInventoryService>();
            _miniGameFactory = GameServices.Get<IMiniGameFactory>();
            _craftProcess = new ManualCraftProcess(_inventory, GameServices.Get<Game.Core.Items.IItemFactory>(),
                GameServices.Get<Game.Core.Events.GameEventBus>());

            _forgeButton.onClick.AddListener(OnForgeButtonPressed);
        }

        private void OnEnable() => RefreshMaterialsDisplay();

        private void RefreshMaterialsDisplay()
        {
            var recipe = _recipe.ToRecipe();
            var hasAllMaterials = true;
            var lines = new System.Text.StringBuilder();

            foreach (var input in recipe.Inputs)
            {
                var owned = _inventory.GetQuantity(input.ItemDefinitionId);
                var sufficient = owned >= input.Quantity;
                hasAllMaterials &= sufficient;
                lines.AppendLine($"{input.ItemDefinitionId} x{input.Quantity} (possédé: {owned}) {(sufficient ? "✓" : "✗")}");
            }

            _materialsListText.text = lines.ToString();
            // Grisé, jamais caché (Nielsen #1 — visibilité de l'état système).
            _forgeButton.interactable = hasAllMaterials;

            _destructiveWarningText.gameObject.SetActive(recipe.MiniGameConfig.AllowsDestructiveFailure);
        }

        private void OnForgeButtonPressed()
        {
            var recipe = _recipe.ToRecipe();
            var context = new MiniGameContext
            {
                ActionId = recipe.Id,
                Config = recipe.MiniGameConfig,
                DifficultyMultiplier = 1f, // difficulté adaptative = IMPORTANT, pas CORE (SCOPE.md)
            };

            _miniGameView.BeginResolution(_miniGameFactory, context, result =>
            {
                var craftResult = _craftProcess.Resolve(recipe, result);
                _resultPopup?.ShowCraftResult(craftResult);
                RefreshMaterialsDisplay();
            });
        }
    }
}
