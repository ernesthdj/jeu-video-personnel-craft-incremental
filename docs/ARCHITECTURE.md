# Architecture Technique — Jeu Vidéo Personnel (craft incrémental)

> Source : docs/FOUNDATION.md, docs/GDD.md, docs/CORE-LOOP.md, docs/SYSTEMS.md, docs/SCOPE.md
> Agent : Technical Director (#2) · Pipeline Game Dev · Phase 1 — Fondation
> Date : 2026-09-15
> **LIVRABLE PRINCIPAL de cette phase**, pondéré selon le cadrage explicite de mentalyas :
> définir le **terrain de jeu = l'architecture/le framework de code**, pas la carte du monde (rôle du World Builder, #5).
> Objectif : que de futurs collègues développeurs codent de nouveaux systèmes facilement, et que du contenu
> (recettes, mini-jeux, ennemis, items) s'ajoute **sans toucher au code**, via un vrai système data-driven.

---

## 0. Analyse de faisabilité — complexité par système CORE (SCOPE.md)

| Système CORE | Complexité (1-5) | Risques principaux | Dépendances | Note perf mobile |
|---|---|---|---|---|
| Système de mini-jeux transversal | **4** | Doit être générique/extensible dès le départ (catalogue), précision tactile multi-device (tailles/DPI variables) | Aucune — c'est la fondation | Input System, boucle de jeu <16-33ms de latence perçue, éviter la physique pour rester déterministe/économe en batterie |
| Craft manuel | 2 | Faible une fois le framework mini-jeu posé — surtout du binding data → UI | Mini-jeux | UI-bound, faible charge |
| Automatisation / farm basique | 2 | Calcul offline correct (delta-temps, cap), pas de triche triviale sur l'horloge locale | Aucune (source de ressources) | Calcul en arrière-plan à la reprise, pas de polling par frame |
| Combat tactique tour par tour (version simple) | **5** | Le plus lourd : FSM (Finite State Machine — machine à états finis) de tour, IA basique, pathfinding sur grille, calibration de deux couches de skill (stats + mini-jeu) simultanément | Mini-jeux, craft manuel (équipement) | Grille petite échelle (pas de simulation de masse), pathfinding A* seulement si obstacles de terrain |
| Compte joueur + inventaire | 2 | Sérialisation propre (polymorphisme des items), pas de perte de save | Aucune | Save asynchrone, pas de blocage du thread principal |
| Backend serveur-autoritaire minimal | 3 | Le "minimal" doit rester vraiment minimal pour le MVP — voir §5 point d'extension | Tous les systèmes qui produisent un résultat (craft, combat) | Négligeable en solo/local pour la vertical slice ; devient un vrai sujet réseau à l'activation d'un vrai serveur |

**Confirmation de l'alerte Game Designer** : le combat tactique est bien le système CORE le plus lourd (complexité 5/5), cohérent avec SYSTEMS.md §4 et l'alerte de fin de JOURNAL.md. Recommandation : pour le MVP, IA à un seul comportement (`AggressiveAI`), pas de pathfinding A* tant que le terrain n'a pas d'obstacles (le World Builder, #5, tranchera ce point plus tard) — grille simple à déplacement Manhattan/Chebyshev suffit pour valider le concept.

---

## 1. Pattern d'architecture retenu : **OOP en couches, PAS d'ECS/DOTS**

### Options évaluées

| Option | Pourquoi retenue / rejetée |
|---|---|
| **ECS/DOTS** (Entity Component System / Data-Oriented Technology Stack — architecture de Unity orientée données, composition par structs, exécution parallèle par jobs) | **Rejeté.** ECS optimise pour des milliers d'entités simulées en parallèle (RTS, simulations de masse) — ce n'est pas le profil de ce jeu (combats tactiques en petit groupe, craft mono-joueur UI-driven, farm en arrière-plan qui est un calcul de delta-temps, pas une simulation par frame). Surtout : **ECS n'a pas d'héritage/polymorphisme d'interface** au sens OOP classique — il rendrait *plus difficile*, pas plus facile, l'objectif explicite de mentalyas (interfaces, Strategy, Factory, Observer). Ce serait de la sur-ingénierie technique pour un besoin de flexibilité de contenu, pas de performance de masse. |
| **Component pur Unity (GameObject/MonoBehaviour) sans séparation logique/présentation** | **Rejeté seul.** C'est le défaut Unity, mais logique métier et présentation (rendu, animation, UI) finissent mélangées dans les mêmes MonoBehaviours — difficile à tester, difficile à faire évoluer par un futur collègue qui ne connaît pas encore tout le projet. |
| **OOP en couches (Domain/Core séparé de la Présentation Unity), GameObject/Component pour la présentation** | **Retenu.** Concilie l'objectif explicite de mentalyas (interfaces, classes abstraites, patterns) avec l'écosystème Unity standard (accessible à de futurs collègues qui connaissent Unity sans connaître DOTS). |

### Principe : séparer **GameCore** (logique pure, sans dépendance Unity) de **Presentation** (MonoBehaviours, prefabs, UI)

Ce n'est pas du Clean Architecture au sens strict d'une appli web (pas de Use Cases/DTOs formels ici — over-engineering pour un jeu solo), mais **le même principe directeur** que le reste des projets de mentalyas (cf. Workflow Backend global : "aucun import de framework dans le domaine") est transposé à Unity :

```
Assembly: GameCore.asmdef          → C# pur, AUCUNE référence à UnityEngine sauf types structurels
                                       minimaux (Vector2Int pour les positions de grille — accepté
                                       comme exception pragmatique, pas de MonoBehaviour, pas de
                                       GameObject). Testable en dehors du Play Mode Unity (tests
                                       edit-mode).
Assembly: Content.asmdef           → Définitions de ScriptableObjects (données), référence GameCore.
Assembly: Presentation.asmdef      → MonoBehaviours, UI, animations, VFX. Référence GameCore + Content.
                                       Aucune logique de résolution de craft/combat n'y vit — seulement
                                       de l'affichage et de la capture d'input, qui appellent GameCore.
```

**Pourquoi ça sert l'extensibilité demandée** : un futur collègue qui ajoute un nouveau système de gameplay travaille presque exclusivement dans `GameCore` (logique) + `Content` (data), sans avoir besoin de comprendre le rendu/l'UI pour livrer une fonctionnalité testable. Ça réduit la surface de connaissance nécessaire pour contribuer — directement aligné sur "les dev [doivent] pouvoir opérer comme maîtres de jeu".

---

## 2. Patterns OOP retenus, avec interfaces concrètes

### 2.1 Strategy — types de mini-jeux interchangeables (le système le plus critique, SYSTEMS.md §5)

```csharp
// GameCore/MiniGames/IMiniGame.cs
public interface IMiniGame
{
    void Initialize(MiniGameContext context);   // recette/action en cours, profil joueur, difficulté
    void OnInputSample(MiniGameInputSample sample); // un échantillon d'input (position tactile, timing…)
    bool IsComplete { get; }
    MiniGameResult Evaluate();                  // score 0-100%, mappé ensuite sur un palier de qualité
}

// Implémentations concrètes : TracePrecisionMiniGame, TimingBarMiniGame,
// LogicRoutingMiniGame (raffinage), CombatActionMiniGame — chacune une classe
// dans GameCore/MiniGames/, aucune ne connaît le craft, le combat ou le raffinage.
```

Les systèmes appelants (craft manuel, raffinage, combat) ne connaissent **jamais** une classe concrète de mini-jeu — ils demandent un `IMiniGame` à la factory (§2.2) via un identifiant défini dans la donnée (`MiniGameConfigSO`, §2.4). Ajouter un nouveau type de mini-jeu = ajouter une classe qui implémente `IMiniGame` + l'enregistrer (§2.3), **zéro modification** des systèmes craft/combat/raffinage existants. C'est la réponse directe à l'alerte du Game Designer ("catalogue extensible, contenu itéré plusieurs fois post-playtest").

### 2.2 Factory — création d'entités/items/mini-jeux

```csharp
// GameCore/MiniGames/IMiniGameFactory.cs
public interface IMiniGameFactory
{
    IMiniGame Create(string miniGameTypeId);
}

// GameCore/Combat/ICombatantFactory.cs
public interface ICombatantFactory
{
    ICombatant CreateFromDefinition(CombatantDefinition definition, GridPosition position);
}

// GameCore/Items/IItemFactory.cs
public interface IItemFactory
{
    ItemInstance CreateFromDefinition(ItemDefinition definition, ItemQuality quality);
}
```

Chaque Factory résout un identifiant (venant d'un ScriptableObject, §2.4) vers une instance concrète, via le Registry (§2.3) plutôt qu'un `switch` géant — un `switch` géant est exactement le genre de code qu'un futur collègue casserait en l'oubliant de mettre à jour.

### 2.3 Registry / Service Locator — enregistrement dynamique de nouveaux systèmes/contenus

```csharp
// GameCore/Registry/GameServices.cs
public static class GameServices
{
    public static void Register<TService>(TService instance) where TService : class;
    public static TService Get<TService>() where TService : class;
}

// GameCore/MiniGames/MiniGameRegistry.cs
public sealed class MiniGameRegistry : IMiniGameFactory
{
    // Association id -> constructeur, remplie au bootstrap (pas de switch codé en dur)
    public void RegisterType(string id, Func<IMiniGame> constructor);
    public IMiniGame Create(string id) { /* résout via le dictionnaire */ }
}
```

Au démarrage du jeu (`GameBootstrap`, seul point qui connaît toutes les implémentations concrètes), chaque mini-jeu/type d'IA/factory s'enregistre :

```csharp
miniGameRegistry.RegisterType("timing_bar", () => new TimingBarMiniGame());
miniGameRegistry.RegisterType("trace_precision", () => new TracePrecisionMiniGame());
miniGameRegistry.RegisterType("logic_routing", () => new LogicRoutingMiniGame());
GameServices.Register<IMiniGameFactory>(miniGameRegistry);
```

**C'est le point d'extension concret pour un futur collègue** : ajouter un système = écrire la classe + une ligne d'enregistrement dans `GameBootstrap`, jamais modifier le code des systèmes qui consomment l'interface.

### 2.4 Observer — événements de jeu (succès/échec craft, résolution combat)

```csharp
// GameCore/Events/GameEventBus.cs
public sealed class GameEventBus
{
    public event Action<CraftResult> CraftCompleted;
    public event Action<ItemInstance> ItemDestroyed;
    public event Action<CombatActionResult> CombatActionResolved;
    public event Action<ProductionTickResult> ProductionTicked;

    public void RaiseCraftCompleted(CraftResult result) => CraftCompleted?.Invoke(result);
    // ...
}
```

UI, inventaire, audio, feedback visuel, et plus tard analytics/télémétrie s'abonnent au bus **sans que la logique de craft/combat ne les connaisse** — condition nécessaire au feedback immédiat exigé par CORE-LOOP.md (Nielsen #1, visibilité de l'état) sans coupler la logique métier à la présentation.

---

## 3. Interfaces de domaine par système

| Système | Interface(s) clé(s) | Implémentations MVP prévues |
|---|---|---|
| Craft manuel + raffinage | `ICraftProcess` (Strategy) — `Resolve(CraftRecipe recipe, IMiniGame result)` | `ManualCraftProcess`, `RefinementCraftProcess` |
| Combat | `ICombatant` (stats, position, PA/PM), `IActionResolver`, `IEnemyAI` (Strategy) | `PlayerCombatant`, `CreatureCombatant`, `AggressiveAI` |
| Automatisation / farm | `IProductionNode`, service `OfflineProgressionCalculator` (calcul pur, testable) | `ResourceProductionNode` |
| Inventaire | `IInventoryService` (Repository-like) | `LocalInventoryService` (MVP), swappable vers backend-synced plus tard |
| Save | `ISaveService` | `JsonFileSaveService` (Newtonsoft.Json, chemin `Application.persistentDataPath`) |
| État de jeu applicatif | `IGameState` + `GameStateMachine` (Boot, Hub, Crafting, Combat) | États concrets minimaux pour la vertical slice |

Toutes ces interfaces vivent dans `GameCore` — aucune ne référence `UnityEngine.MonoBehaviour`.

---

## 4. Couche data-driven — ScriptableObjects (contenu ajouté sans recompiler)

C'est le deuxième pilier explicite de mentalyas, au même niveau d'importance que l'OOP : un designer (ou lui-même) doit pouvoir ajouter une recette, un ennemi, un item ou une config de mini-jeu **en créant un asset dans l'éditeur Unity**, jamais en éditant du C#.

| ScriptableObject | Contenu | Consommé par |
|---|---|---|
| `MiniGameConfigSO` | id du type de mini-jeu, courbe de difficulté, seuils de qualité, paramètres visuels | `IMiniGameFactory` + `IMiniGame.Initialize` |
| `CraftRecipeSO` | inputs (JSON-like via champs sérialisés), outputs, référence à un `MiniGameConfigSO`, flag "échec destructif" + seuil | `ICraftProcess` |
| `ItemDefinitionSO` | type, rareté, stats de base | `IItemFactory` |
| `CombatantDefinitionSO` | stats de base, référence à un `IEnemyAI` (par id), loot table | `ICombatantFactory` |
| `ProductionNodeSO` | taux de production, cap de stockage, palier de déblocage | `IProductionNode` |

**Convention obligatoire** : chaque SO expose uniquement des données (pas de méthode avec logique métier au-delà de la validation de champs). La logique de résolution reste dans `GameCore`, qui **lit** ces SO comme des paramètres — ça évite le piège classique Unity où la logique fuit dans les ScriptableObjects et redevient impossible à tester hors éditeur.

**Chargement** : via **Addressables** (pas le dossier `Resources`, déprécié en bonnes pratiques) — permet un chargement asynchrone, un meilleur contrôle mémoire sur mobile, et surtout la possibilité d'ajouter du contenu post-lancement (mise à jour de catalogue) sans forcément repasser par une resoumission complète du store pour de la donnée pure.

---

## 5. Point d'extension explicite — économie P2P (stub, non détaillé ici)

Conformément au cadrage de mentalyas : **le système anti-fraude/trading n'est pas conçu ici.** Seul le point d'extension est posé.

```csharp
// GameCore/Economy/IEconomyService.cs
public interface IEconomyService
{
    Task<Balance> GetBalanceAsync(PlayerId playerId);
    Task<TransactionResult> ExecuteTradeAsync(TradeRequest request); // no-op/log en MVP
}

// GameCore/Economy/StubEconomyService.cs
public sealed class StubEconomyService : IEconomyService
{
    // Implémentation locale minimale : monnaie soft gérée localement,
    // toute opération de monnaie premium/marché retourne un résultat "non disponible"
    // ou log un avertissement. Aucune logique anti-fraude, aucun appel réseau.
}
```

Enregistré comme tout autre service : `GameServices.Register<IEconomyService>(new StubEconomyService());`. Le jour où le marché P2P est repris en détail (fin de projet, cf. FOUNDATION.md §12), une nouvelle implémentation (`ServerAuthoritativeEconomyService`, custom ou basée sur un service managé — voir TECH-STACK.md §4) remplace le stub **sans toucher aux systèmes appelants** (marché, boutique) puisqu'ils ne parlent qu'à l'interface. C'est le même mécanisme que le Registry (§2.3), appliqué à un seul service au lieu d'un catalogue.

Même logique pour le **backend serveur-autoritaire minimal** exigé dès le MVP par SCOPE.md : `IAuthoritativeValidationService`, implémentation locale (`LocalValidationService`) pour la vertical slice et les tests solo, swappable plus tard.

---

## 6. State management

- **État applicatif global** : `GameStateMachine` (Boot → MainMenu/Hub → Crafting → Combat → retour Hub), pattern State classique (`IGameState.Enter()/Exit()/Tick()`), pas de framework externe — le besoin ne justifie pas une dépendance tierce.
- **État de combat** : `CombatStateMachine` dédié (Idle → SélectionAction → MiniJeu → Résolution → TourSuivant), conforme à SYSTEMS.md §4 — implémentation explicite par classes d'état (`ICombatState`), pas un `switch` sur un enum qui grossirait à chaque nouvel effet de combat.
- **État de mini-jeu** : géré à l'intérieur de chaque `IMiniGame` (Initialize → sampling d'input → Evaluate), pas exposé au reste du jeu — encapsulation stricte.

## 7. Save system

`ISaveService` + `JsonFileSaveService` (Newtonsoft.Json pour supporter le polymorphisme des collections d'`ItemInstance`, `ICombatant`, etc. — voir TECH-STACK.md §5). Sauvegarde locale uniquement pour le MVP (`Application.persistentDataPath`), asynchrone (ne bloque jamais le thread principal — critère de fluidité mobile). Le remplacement futur par une sauvegarde cloud (Cloud Save Unity Gaming Services ou autre) suit le même principe d'interface swappable que §5.

---

## 8. Ce que cette architecture NE fait PAS (délibérément, anti-sur-ingénierie)

- Pas d'ECS/DOTS (voir §1) — non justifié par l'échelle du jeu.
- Pas de framework de DI (Dependency Injection — injection de dépendances) tiers (VContainer/Zenject) pour le MVP — un Service Locator maison (§2.3) suffit à un solo dev et reste lisible ; à réévaluer seulement si l'équipe grossit sensiblement et que le couplage au Service Locator devient gênant pour les tests.
- Pas d'interface/abstraction pour les systèmes qui n'ont **qu'une seule implémentation prévisible** et ne sont pas dans le périmètre d'extensibilité explicitement demandé par mentalyas (ex. la boutique F2P n'a pas besoin d'un Strategy tant qu'elle n'a qu'un seul flux d'achat IAP) — l'investissement en abstraction est concentré sur les 4 axes demandés : mini-jeux, craft/recettes, combattants/ennemis, items.
- Pas de moteur d'optimisation de chaîne de production générique pour le raffinage — SYSTEMS.md §2 demande volontairement un mode simplifié (3-5 modules), pas un vrai factory-builder.
