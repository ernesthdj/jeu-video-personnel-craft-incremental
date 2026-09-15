#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Game.Content.Definitions;
using Game.Core.Items;
using Game.DevTools;
using Game.Presentation.Bootstrap;
using Game.Presentation.Crafting;
using Game.Presentation.MiniGames;
using Game.Presentation.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.DevTools.Editor
{
    /// <summary>
    /// Harnais de test uniquement (jamais versionné dans un build joueur) : génère en une
    /// passe les assets de contenu minimaux (recette, config de mini-jeu, items) et une
    /// Scene jouable qui câble GameBootstrap + l'écran de craft manuel + la vue du
    /// mini-jeu, pour permettre un premier test humain réel du vertical slice sans étape
    /// manuelle de câblage dans l'Inspector.
    /// </summary>
    public static class TestHarnessSetup
    {
        private const string AssetsDir = "Assets/_Project/DevTools/TestAssets";
        private const string SceneDir = "Assets/_Project/DevTools/TestScenes";
        private const string ScenePath = SceneDir + "/TestHarness_CraftManual.unity";

        [MenuItem("CraftIncremental/Build Test Harness")]
        public static void Build()
        {
            // Idempotent : un run precedent partiellement echoue ne doit jamais laisser
            // d'assets corrompus/dupliques trainer pour le run suivant.
            if (AssetDatabase.IsValidFolder(AssetsDir))
            {
                AssetDatabase.DeleteAsset(AssetsDir);
            }

            if (AssetDatabase.IsValidFolder(SceneDir))
            {
                AssetDatabase.DeleteAsset(SceneDir);
            }

            EnsureFolder(AssetsDir);
            EnsureFolder(SceneDir);

            // Creer la Scene AVANT tout asset de contenu : EditorSceneManager.NewScene(...,
            // Single) declenche un nettoyage d'assets inutilises qui detruit toute
            // ScriptableObject fraichement chargee/creee et seulement referencee par une
            // variable C# locale a ce stade (deja constate : CraftRecipeSO detruit). En le
            // faisant en premier, plus aucun changement de Scene n'a lieu apres coup.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var woodItem = CreateItemDefinition("wood", "Bois", ItemType.RawMaterial, "Item_Wood");
            var handleItem = CreateItemDefinition("wooden_handle", "Manche en bois", ItemType.CraftedBase, "Item_WoodenHandle");

            var miniGameConfig = ScriptableObject.CreateInstance<MiniGameConfigSO>();
            AssetDatabase.CreateAsset(miniGameConfig, $"{AssetsDir}/MiniGameConfig_Default.asset");

            var recipe = ScriptableObject.CreateInstance<CraftRecipeSO>();
            SetField(recipe, "_id", "craft_wooden_handle");
            SetField(recipe, "_displayName", "Forger un manche en bois");
            SetField(recipe, "_inputs", new List<CraftRecipeSO.ItemStackEntry>
            {
                new CraftRecipeSO.ItemStackEntry { ItemDefinition = woodItem, Quantity = 3 },
            });
            SetField(recipe, "_output", handleItem);
            SetField(recipe, "_miniGameConfig", miniGameConfig);
            var recipePath = $"{AssetsDir}/Recipe_WoodenHandle.asset";
            AssetDatabase.CreateAsset(recipe, recipePath);

            // .inputactions est un format JSON avec son propre importeur (InputActionImporter) —
            // AssetDatabase.CreateAsset() ne sait pas le créer (c'est fait pour les assets binaires
            // Unity classiques). On écrit le JSON directement puis on importe.
            var inMemoryInputActions = BuildInputActions();
            var inputActionsPath = $"{AssetsDir}/TestHarness.inputactions";
            File.WriteAllText(inputActionsPath, inMemoryInputActions.ToJson());
            Object.DestroyImmediate(inMemoryInputActions);
            AssetDatabase.ImportAsset(inputActionsPath, ImportAssetOptions.ForceSynchronousImport);

            // Un Refresh()/reimport peut invalider les references en memoire creees plus
            // haut dans cette meme methode (deja constate en pratique avec `recipe`) — on
            // force le refresh ICI, puis on recharge TOUT depuis le disque ensuite plutot
            // que de faire confiance aux variables d'origine.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // L'importeur genere automatiquement un sous-asset InputActionReference par
            // action — jamais le creer/l'ajouter nous-memes (AddObjectToAsset corrompt le
            // fichier .inputactions, deja constate). On va simplement le chercher.
            InputActionReference? actionReference = null;
            foreach (var subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(inputActionsPath))
            {
                if (subAsset is InputActionReference candidate && candidate.action?.name == "PrimaryTap")
                {
                    actionReference = candidate;
                    break;
                }
            }

            if (actionReference == null)
            {
                throw new System.InvalidOperationException(
                    "Sous-asset InputActionReference 'PrimaryTap' introuvable apres import de TestHarness.inputactions.");
            }

            var reloadedRecipe = AssetDatabase.LoadAssetAtPath<CraftRecipeSO>(recipePath);
            BuildScene(scene, reloadedRecipe, actionReference);

            Debug.Log("[TestHarnessSetup] Harnais de test généré : assets + Scene prêts.");
        }

        private static InputActionAsset BuildInputActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = asset.AddActionMap("TestHarness");
            var action = map.AddAction("PrimaryTap", InputActionType.Button);
            action.AddBinding("<Mouse>/leftButton");
            action.AddBinding("<Touchscreen>/primaryTouch/tap");
            return asset;
        }

        private static ItemDefinitionSO CreateItemDefinition(string id, string displayName, ItemType type, string assetName)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinitionSO>();
            SetField(item, "_id", id);
            SetField(item, "_displayName", displayName);
            SetField(item, "_type", type);
            AssetDatabase.CreateAsset(item, $"{AssetsDir}/{assetName}.asset");
            return item;
        }

        private static void BuildScene(Scene scene, CraftRecipeSO recipe, InputActionReference tapActionReference)
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameBootstrap>();

            var seederGo = new GameObject("TestHarnessSeeder");
            seederGo.AddComponent<TestHarnessSeeder>();

            // --- Canvas principal : écran de craft manuel ---
            var mainCanvasGo = new GameObject("MainCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var mainCanvas = mainCanvasGo.GetComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var mainScaler = mainCanvasGo.GetComponent<CanvasScaler>();
            mainScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            mainScaler.referenceResolution = new Vector2(1080, 1920);

            var panelGo = CreateStretchChild(mainCanvasGo.transform, "CraftPanel", typeof(Image));
            panelGo.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 1f);
            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 80, 40);
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var titleGo = CreateText(panelGo.transform, "TitleText", recipe.name, 48, Color.white);
            SetPreferredHeight(titleGo, 70);

            var materialsTextGo = CreateText(panelGo.transform, "MaterialsListText", string.Empty, 32, Color.white);
            SetPreferredHeight(materialsTextGo, 220);

            var destructiveWarningGo = CreateText(panelGo.transform, "DestructiveWarningText",
                "ATTENTION : un echec peut detruire les materiaux.", 28, new Color(1f, 0.4f, 0.3f));
            SetPreferredHeight(destructiveWarningGo, 60);

            var forgeButtonGo = CreateButton(panelGo.transform, "ForgeButton", "Forger");
            SetPreferredHeight(forgeButtonGo, 100);

            // --- Canvas isolé : vue du mini-jeu (UI-SCREENS.md §0 — jamais le Canvas du HUD) ---
            var miniGameCanvasGo = new GameObject("MiniGameCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var miniGameCanvas = miniGameCanvasGo.GetComponent<Canvas>();
            miniGameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            miniGameCanvas.sortingOrder = 10;
            var miniGameScaler = miniGameCanvasGo.GetComponent<CanvasScaler>();
            miniGameScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            miniGameScaler.referenceResolution = new Vector2(1080, 1920);

            var trackAreaGo = CreateUIObject(miniGameCanvasGo.transform, "TrackArea", typeof(Image));
            trackAreaGo.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.22f, 1f);
            var trackRect = trackAreaGo.GetComponent<RectTransform>();
            AnchorCentered(trackRect, new Vector2(800, 80));

            var targetWindowGo = CreateUIObject(trackAreaGo.transform, "TargetWindowHandle", typeof(Image));
            targetWindowGo.GetComponent<Image>().color = new Color(0.3f, 0.85f, 0.4f, 0.6f);
            var targetWindowRect = targetWindowGo.GetComponent<RectTransform>();
            targetWindowRect.anchorMin = new Vector2(0f, 0f);
            targetWindowRect.anchorMax = new Vector2(0f, 1f);
            targetWindowRect.pivot = new Vector2(0f, 0.5f);
            targetWindowRect.anchoredPosition = Vector2.zero;
            targetWindowRect.sizeDelta = new Vector2(0f, 0f);

            var cursorGo = CreateUIObject(trackAreaGo.transform, "CursorHandle", typeof(Image));
            cursorGo.GetComponent<Image>().color = Color.white;
            var cursorRect = cursorGo.GetComponent<RectTransform>();
            cursorRect.anchorMin = new Vector2(0f, 0f);
            cursorRect.anchorMax = new Vector2(0f, 1f);
            cursorRect.pivot = new Vector2(0.5f, 0.5f);
            cursorRect.sizeDelta = new Vector2(8f, 80f);
            cursorRect.anchoredPosition = Vector2.zero;

            var precisionBarGo = CreateUIObject(miniGameCanvasGo.transform, "PrecisionBarFill", typeof(Image));
            var precisionBarRect = precisionBarGo.GetComponent<RectTransform>();
            AnchorBottomCenter(precisionBarRect, new Vector2(400, 30), new Vector2(0, 200));
            precisionBarGo.GetComponent<Image>().color = Color.white;

            var abandonButtonGo = CreateButton(miniGameCanvasGo.transform, "AbandonButton", "Abandonner");
            var abandonRect = abandonButtonGo.GetComponent<RectTransform>();
            AnchorBottomCenter(abandonRect, new Vector2(300, 90), new Vector2(0, 60));

            var miniGameView = miniGameCanvasGo.AddComponent<TimingBarMiniGameView>();
            SetField(miniGameView, "_cursorHandle", cursorRect);
            SetField(miniGameView, "_trackArea", trackRect);
            SetField(miniGameView, "_targetWindowHandle", targetWindowRect);
            SetField(miniGameView, "_precisionBarFill", precisionBarGo.GetComponent<Image>());
            SetField(miniGameView, "_primaryTapAction", tapActionReference);
            abandonButtonGo.GetComponent<Button>().onClick.AddListener(miniGameView.OnAbandonButtonPressed);

            miniGameCanvasGo.SetActive(false);

            var craftController = panelGo.AddComponent<CraftManualController>();
            SetField(craftController, "_recipe", recipe);
            SetField(craftController, "_miniGameView", miniGameView);
            SetField(craftController, "_forgeButton", forgeButtonGo.GetComponent<Button>());
            SetField(craftController, "_materialsListText", materialsTextGo.GetComponent<Text>());
            SetField(craftController, "_destructiveWarningText", destructiveWarningGo.GetComponent<Text>());
            SetField(craftController, "_resultPopup", null);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            var scenes = new List<EditorBuildSettingsScene>(existing) { new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static GameObject CreateStretchChild(Transform parent, string name, params System.Type[] components)
        {
            var go = CreateUIObject(parent, name, components);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateUIObject(Transform parent, string name, params System.Type[] components)
        {
            var allComponents = new List<System.Type> { typeof(RectTransform) };
            allComponents.AddRange(components);
            var go = new GameObject(name, allComponents.ToArray());
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateText(Transform parent, string name, string content, int fontSize, Color color)
        {
            var go = CreateUIObject(parent, name, typeof(Text));
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.UpperLeft;
            text.text = content;
            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, string label)
        {
            var go = CreateUIObject(parent, name, typeof(Image), typeof(Button));
            go.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.8f, 1f);
            var textGo = CreateText(go.transform, "Label", label, 32, Color.white);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textGo.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            return go;
        }

        private static void SetPreferredHeight(GameObject go, float height)
        {
            var fitter = go.AddComponent<LayoutElement>();
            fitter.preferredHeight = height;
            fitter.minHeight = height;
        }

        private static void AnchorCentered(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }

        private static void AnchorBottomCenter(RectTransform rect, Vector2 size, Vector2 offsetFromBottom)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = offsetFromBottom;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)!.Replace("\\", "/");
            var folderName = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void SetField(object target, string fieldName, object? value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?? throw new System.MissingFieldException(target.GetType().FullName, fieldName);
            field.SetValue(target, value);
        }
    }
}
