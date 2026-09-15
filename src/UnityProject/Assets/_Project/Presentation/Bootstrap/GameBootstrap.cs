using Game.Core.Combat;
using Game.Core.Economy;
using Game.Core.Events;
using Game.Core.GameState;
using Game.Core.Inventory;
using Game.Core.Items;
using Game.Core.MiniGames;
using Game.Core.Save;
using Game.Core.Validation;
using UnityEngine;

namespace Game.Presentation.Bootstrap
{
    /// <summary>
    /// LIMITATION CONNUE : dépend de UnityEngine (MonoBehaviour), non compilable hors
    /// éditeur (voir docs/IMPLEMENTATION.md) — vérifié uniquement par revue manuelle
    /// contre les signatures réelles de GameCore (compilé et testé).
    ///
    /// Seul point du jeu qui connaît toutes les implémentations concrètes
    /// (ARCHITECTURE.md §2.3) : enregistre chaque service/mini-jeu/IA dans GameServices.
    /// Ajouter un système = une classe + une ligne ici, jamais de modification des
    /// systèmes qui consomment les interfaces.
    ///
    /// CORRECTIF : rien ne garantit nativement que Awake() s'exécute avant celui des
    /// autres composants d'une même Scene (l'ordre entre GameObjects différents n'est pas
    /// contractuel). Tout composant qui lit GameServices.Get&lt;T&gt;() dans son propre
    /// Awake() (ex. CraftManualController) dépend strictement de ce bootstrap déjà
    /// exécuté — d'où cet ordre d'exécution forcé, très négatif pour s'exécuter en premier.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Game.Core.Registry.GameServices.Reset();

            // --- Mini-jeux (Strategy + Factory + Registry, ARCHITECTURE.md §2.1-§2.3) ---
            var miniGameRegistry = new MiniGameRegistry();
            miniGameRegistry.RegisterType("timing_bar", () => new TimingBarMiniGame());
            // "trace_precision" et "logic_routing" : hors scope du vertical slice
            // (VERTICAL-SLICE.md §1 — le craft manuel utilise timing_bar pour la première
            // implémentation jouable ; le combat réutilise le même type, cf. §6).
            Game.Core.Registry.GameServices.Register<IMiniGameFactory>(miniGameRegistry);

            // --- Combat : IA (même mécanisme de registry que les mini-jeux) ---
            IEnemyAI ResolveEnemyAi(string id) => id switch
            {
                "aggressive" => new AggressiveAI(),
                _ => throw new System.InvalidOperationException($"IA inconnue: '{id}'."),
            };
            Game.Core.Registry.GameServices.Register<System.Func<string, IEnemyAI>>(ResolveEnemyAi);
            Game.Core.Registry.GameServices.Register<ICombatantFactory>(new CombatantFactory());
            Game.Core.Registry.GameServices.Register<IActionResolver>(new CombatActionResolver());

            // --- Items / Inventaire ---
            Game.Core.Registry.GameServices.Register<IItemFactory>(new ItemFactory());
            Game.Core.Registry.GameServices.Register<IInventoryService>(new LocalInventoryService());

            // --- Événements (Observer, ARCHITECTURE.md §2.4) ---
            Game.Core.Registry.GameServices.Register(new GameEventBus());

            // --- Économie : STUB uniquement (cadrage explicite de cette phase, voir
            // ARCHITECTURE.md §5, SCOPE.md) — jamais appelé par une UI dans ce slice. ---
            Game.Core.Registry.GameServices.Register<IEconomyService>(new StubEconomyService());

            // --- Validation serveur-autoritaire minimale (SCOPE.md) ---
            Game.Core.Registry.GameServices.Register<IAuthoritativeValidationService>(new LocalValidationService());

            // --- Sauvegarde locale (ARCHITECTURE.md §7) : Application.persistentDataPath
            // est le seul point d'appel UnityEngine nécessaire pour instancier ce service
            // — GameCore lui-même reste indépendant (voir JsonFileSaveService.cs). ---
            Game.Core.Registry.GameServices.Register<ISaveService>(
                new JsonFileSaveService(Application.persistentDataPath));

            // --- État applicatif global (Boot -> Hub -> Crafting -> Combat) ---
            var gameStateMachine = new GameStateMachine();
            Game.Core.Registry.GameServices.Register(gameStateMachine);
            gameStateMachine.TransitionTo(new SimpleGameState("Hub"));

            Debug.Log("[GameBootstrap] Tous les services GameCore enregistrés.");
        }
    }
}
