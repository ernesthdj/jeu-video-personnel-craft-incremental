# Graph Report - .  (2026-09-15)

## Corpus Check
- 577 files · ~99,999 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 577 nodes · 690 edges · 72 communities (21 shown, 51 thin omitted)
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 8 edges (avg confidence: 0.79)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Interfaces & Patterns GameCore|Interfaces & Patterns GameCore]]
- [[_COMMUNITY_Resolution Actions de Combat|Resolution Actions de Combat]]
- [[_COMMUNITY_Bootstrap & Ecran Craft|Bootstrap & Ecran Craft]]
- [[_COMMUNITY_Etat Applicatif & UI Craft|Etat Applicatif & UI Craft]]
- [[_COMMUNITY_Combattants & FSM Combat|Combattants & FSM Combat]]
- [[_COMMUNITY_Inventaire Joueur (Service)|Inventaire Joueur (Service)]]
- [[_COMMUNITY_Tests Combat StateMachine|Tests Combat StateMachine]]
- [[_COMMUNITY_Harnais de Test (Editor)|Harnais de Test (Editor)]]
- [[_COMMUNITY_Etats de la FSM Combat|Etats de la FSM Combat]]
- [[_COMMUNITY_Tests Craft Manuel|Tests Craft Manuel]]
- [[_COMMUNITY_Interface Inventaire|Interface Inventaire]]
- [[_COMMUNITY_Tests Mini-jeu Timing Bar|Tests Mini-jeu Timing Bar]]
- [[_COMMUNITY_Sauvegarde JSON|Sauvegarde JSON]]
- [[_COMMUNITY_Tests Sauvegarde JSON|Tests Sauvegarde JSON]]
- [[_COMMUNITY_Journal Sessions Pipeline|Journal Sessions Pipeline]]
- [[_COMMUNITY_Etat Rencontre de Combat|Etat Rencontre de Combat]]
- [[_COMMUNITY_IA Agressive (Combat)|IA Agressive (Combat)]]
- [[_COMMUNITY_Interface ICombatant|Interface ICombatant]]
- [[_COMMUNITY_Combattants Concrets|Combattants Concrets]]
- [[_COMMUNITY_GridPosition & Distances|GridPosition & Distances]]
- [[_COMMUNITY_Bus d'Evenements (Observer)|Bus d'Evenements (Observer)]]
- [[_COMMUNITY_Tests Production Offline|Tests Production Offline]]
- [[_COMMUNITY_Direction Artistique & Zones|Direction Artistique & Zones]]
- [[_COMMUNITY_Decisions de Combat|Decisions de Combat]]
- [[_COMMUNITY_Ecosysteme Workspace Hub|Ecosysteme Workspace Hub]]
- [[_COMMUNITY_Entites BDD (FOUNDATION)|Entites BDD (FOUNDATION)]]
- [[_COMMUNITY_Tests Inventaire Local|Tests Inventaire Local]]
- [[_COMMUNITY_Interface Sauvegarde|Interface Sauvegarde]]
- [[_COMMUNITY_Economie Stub|Economie Stub]]
- [[_COMMUNITY_Interface IMiniGame|Interface IMiniGame]]
- [[_COMMUNITY_Validation Serveur-Autoritaire|Validation Serveur-Autoritaire]]
- [[_COMMUNITY_Tests Registry Mini-jeux|Tests Registry Mini-jeux]]
- [[_COMMUNITY_Factory Combattants|Factory Combattants]]
- [[_COMMUNITY_Interface Economie|Interface Economie]]
- [[_COMMUNITY_Interface Etat de Jeu|Interface Etat de Jeu]]
- [[_COMMUNITY_Interface Validation Autoritaire|Interface Validation Autoritaire]]
- [[_COMMUNITY_ICombatantFactory|ICombatantFactory]]
- [[_COMMUNITY_IEnemyAI|IEnemyAI]]
- [[_COMMUNITY_ICraftProcess|ICraftProcess]]
- [[_COMMUNITY_GameStateMachine|GameStateMachine]]
- [[_COMMUNITY_IItemFactory|IItemFactory]]
- [[_COMMUNITY_IMiniGameFactory|IMiniGameFactory]]
- [[_COMMUNITY_MiniGameConfig|MiniGameConfig]]
- [[_COMMUNITY_IActionResolver|IActionResolver]]
- [[_COMMUNITY_ICombatState|ICombatState]]
- [[_COMMUNITY_OfflineProgressionCalculator|OfflineProgressionCalculator]]
- [[_COMMUNITY_IsExternalInit|IsExternalInit]]
- [[_COMMUNITY_CraftRecipe|CraftRecipe]]
- [[_COMMUNITY_CraftResult|CraftResult]]
- [[_COMMUNITY_TradeRequest|TradeRequest]]
- [[_COMMUNITY_ItemInstance|ItemInstance]]
- [[_COMMUNITY_MiniGameContext|MiniGameContext]]
- [[_COMMUNITY_ProductionTickResult|ProductionTickResult]]
- [[_COMMUNITY_ItemInstanceSaveData|ItemInstanceSaveData]]
- [[_COMMUNITY_WORLD-MAP|WORLD-MAP]]
- [[_COMMUNITY_CombatActionResult|CombatActionResult]]
- [[_COMMUNITY_CombatantDefinition|CombatantDefinition]]
- [[_COMMUNITY_PlayerId|PlayerId]]
- [[_COMMUNITY_TransactionResult|TransactionResult]]
- [[_COMMUNITY_ItemDefinition|ItemDefinition]]
- [[_COMMUNITY_IProductionNode|IProductionNode]]
- [[_COMMUNITY_GameSaveData|GameSaveData]]
- [[_COMMUNITY_CombatPhase|CombatPhase]]
- [[_COMMUNITY_ItemType|ItemType]]
- [[_COMMUNITY_MiniGameResult|MiniGameResult]]
- [[_COMMUNITY_QualityThreshold|QualityThreshold]]
- [[_COMMUNITY_CombatOutcome|CombatOutcome]]
- [[_COMMUNITY_Balance|Balance]]
- [[_COMMUNITY_ItemQuality|ItemQuality]]
- [[_COMMUNITY_ItemStack|ItemStack]]
- [[_COMMUNITY_MiniGameInputSample|MiniGameInputSample]]
- [[_COMMUNITY_GDD|GDD]]

## God Nodes (most connected - your core abstractions)
1. `CombatStateMachine` - 23 edges
2. `TimingBarMiniGameView` - 18 edges
3. `CombatStateMachineTests` - 16 edges
4. `TestHarnessSetup` - 15 edges
5. `CombatController` - 14 edges
6. `CraftManualController` - 14 edges
7. `TimingBarMiniGame` - 11 edges
8. `LocalInventoryService` - 10 edges
9. `string` - 9 edges
10. `CraftResultPopupController` - 9 edges

## Surprising Connections (you probably didn't know these)
- `Project CLAUDE.md Config` --references--> `Pipeline Jeu Craft Incrémental`  [EXTRACTED]
  CLAUDE.md → docs/PIPELINE-STATE.json
- `CLAUDE.md (jeu-video-personnel-craft-incremental)` --references--> `JOURNAL.md (jeu-video-personnel-craft-incremental)`  [EXTRACTED]
  CLAUDE.md → docs/JOURNAL.md
- `TestHarnessSetup` --references--> `string`  [EXTRACTED]
  src/UnityProject/Assets/_Project/DevTools/Editor/TestHarnessSetup.cs → tests/UnityProject/EditMode/Save/JsonFileSaveServiceTests.cs
- `JsonFileSaveService` --references--> `string`  [EXTRACTED]
  src/UnityProject/Assets/_Project/GameCore/Save/JsonFileSaveService.cs → tests/UnityProject/EditMode/Save/JsonFileSaveServiceTests.cs
- `Journal — Sessions Pipeline` --references--> `Phase 1 — Game Designer`  [EXTRACTED]
  docs/JOURNAL.md → docs/PIPELINE-STATE.json

## Hyperedges (group relationships)
- **Catalogue de Mini-jeux Extensible (Strategy+Factory+Registry → Implémentation)** — architecture_imini_game, architecture_minigame_registry, implementation_timingbar_minigame, systems_minigame_transversal, gdd_selfdoubt_minigame_fun_risk [INFERRED 0.85]
- **Chaîne de Décision : Pas de Pathfinding A* (TD → World Builder → Gameplay Programmer)** — architecture_layered_oop, zonedesign_no_terrain_obstacles_decision, implementation_combatstatemachine [EXTRACTED 1.00]
- **Point d'Extension Économie P2P (Stub Traversant les Phases)** — architecture_ieconomy_service, techstack_economy_backend_deferred, foundation_p2p_economy_backend, verticalslice_plan [EXTRACTED 1.00]

## Communities (72 total, 51 thin omitted)

### Community 0 - "Interfaces & Patterns GameCore"
Cohesion: 0.05
Nodes (60): Pattern Factory, GameEventBus, Interface ICombatant, Interface ICraftProcess, Interface IEconomyService (Stub), Interface IMiniGame, Interface IMiniGameFactory, Architecture OOP en Couches (GameCore/Content/Presentation) (+52 more)

### Community 1 - "Resolution Actions de Combat"
Cohesion: 0.05
Nodes (29): CombatActionResolver, Game.Core.Combat, CombatController, Game.Presentation.Combat, CombatantDefinitionSO, CombatStateMachine, CombatantDefinitionSO, Game.Content.Definitions (+21 more)

### Community 2 - "Bootstrap & Ecran Craft"
Cohesion: 0.06
Nodes (22): bool, Game.Presentation.Bootstrap, GameBootstrap, Button, CraftManualController, Game.Presentation.Crafting, Game.Core.Crafting, ManualCraftProcess (+14 more)

### Community 3 - "Etat Applicatif & UI Craft"
Cohesion: 0.08
Nodes (14): Action, double, Game.Core.GameState, SimpleGameState, IGameState, Image, IMiniGame, InputActionReference (+6 more)

### Community 4 - "Combattants & FSM Combat"
Cohesion: 0.13
Nodes (8): CombatantBase, Game.Core.Combat, CombatStateMachine, Game.Core.Combat, CombatActionResult, CombatEncounterState, Func, ICombatant

### Community 5 - "Inventaire Joueur (Service)"
Cohesion: 0.1
Nodes (8): Dictionary, IMiniGameFactory, Game.Core.Inventory, LocalInventoryService, Game.Core.MiniGames, MiniGameRegistry, Game.Core.Registry, GameServices

### Community 8 - "Etats de la FSM Combat"
Cohesion: 0.18
Nodes (8): ActionSelectionState, EndedState, Game.Core.Combat, MiniGameState, ResolutionState, TurnEndState, TurnStartState, ICombatState

### Community 9 - "Tests Craft Manuel"
Cohesion: 0.35
Nodes (3): Game.Core.Tests.Crafting, ManualCraftProcessTests, ItemDefinition

### Community 12 - "Sauvegarde JSON"
Cohesion: 0.31
Nodes (4): ISaveService, JsonSerializerSettings, Game.Core.Save, JsonFileSaveService

### Community 13 - "Tests Sauvegarde JSON"
Cohesion: 0.25
Nodes (3): IDisposable, Game.Core.Tests.Save, JsonFileSaveServiceTests

### Community 14 - "Journal Sessions Pipeline"
Cohesion: 0.43
Nodes (8): Journal — Sessions Pipeline, Phase 1 — Game Designer, Phase 2 — Technical Director, Phase 3 — UI/UX Game Designer, Phase 4 — World Builder, Phase 5 — Gameplay Programmer, Phase 6 — QA Playtester (pending), Phase 7 — Knowledge Curator (pending)

### Community 16 - "IA Agressive (Combat)"
Cohesion: 0.38
Nodes (3): AggressiveAI, Game.Core.Combat, IEnemyAI

### Community 18 - "Combattants Concrets"
Cohesion: 0.29
Nodes (5): CreatureCombatant, Game.Core.Combat, Game.Core.Combat, PlayerCombatant, CombatantBase

### Community 21 - "Tests Production Offline"
Cohesion: 0.29
Nodes (4): IProductionNode, FakeNode, Game.Core.Tests.Production, OfflineProgressionCalculatorTests

### Community 22 - "Direction Artistique & Zones"
Cohesion: 0.29
Nodes (7): Style Low-Poly Peint, Pillar 2 — Deux tempos, un seul jeu, URP (Universal Render Pipeline), Structure Hub-and-Spoke, Expédition 2 — La Carrière, Expédition 1 — La Lisière, Expédition 3 — Les Ruines

### Community 24 - "Ecosysteme Workspace Hub"
Cohesion: 0.53
Nodes (6): /brainstorm (cahier des charges), CLAUDE.md (jeu-video-personnel-craft-incremental), Graphify projet (graphe de connaissances local), /hub (Archiviste ProjectMaster), JOURNAL.md (jeu-video-personnel-craft-incremental), /pipeline (agents)

### Community 25 - "Entites BDD (FOUNDATION)"
Cohesion: 0.33
Nodes (6): Entité Combat, Entité CraftRecipe, Entité Item, Entité MarketListing, Entité Player, Entité Transaction

### Community 28 - "Economie Stub"
Cohesion: 0.33
Nodes (3): Game.Core.Economy, StubEconomyService, IEconomyService

### Community 30 - "Validation Serveur-Autoritaire"
Cohesion: 0.33
Nodes (3): IAuthoritativeValidationService, Game.Core.Validation, LocalValidationService

### Community 32 - "Factory Combattants"
Cohesion: 0.4
Nodes (3): CombatantFactory, Game.Core.Combat, ICombatantFactory

### Community 54 - "WORLD-MAP"
Cohesion: 0.67
Nodes (3): Paysage Sonore par Zone, Écran Hub, Hub — L'Atelier

## Knowledge Gaps
- **139 isolated node(s):** `Game.Content.Definitions`, `Game.Content.Definitions`, `Game.Content.Definitions`, `ItemType`, `Sprite` (+134 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **51 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `CombatStateMachine` connect `Combattants & FSM Combat` to `Etats de la FSM Combat`, `Resolution Actions de Combat`, `Bootstrap & Ecran Craft`?**
  _High betweenness centrality (0.058) - this node is a cross-community bridge._
- **Why does `string` connect `Resolution Actions de Combat` to `Sauvegarde JSON`, `Tests Sauvegarde JSON`, `Harnais de Test (Editor)`?**
  _High betweenness centrality (0.047) - this node is a cross-community bridge._
- **Why does `List` connect `Resolution Actions de Combat` to `Combattants & FSM Combat`, `Inventaire Joueur (Service)`?**
  _High betweenness centrality (0.040) - this node is a cross-community bridge._
- **What connects `Game.Content.Definitions`, `Game.Content.Definitions`, `Game.Content.Definitions` to the rest of the system?**
  _139 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Interfaces & Patterns GameCore` be split into smaller, more focused modules?**
  _Cohesion score 0.05 - nodes in this community are weakly interconnected._
- **Should `Resolution Actions de Combat` be split into smaller, more focused modules?**
  _Cohesion score 0.05 - nodes in this community are weakly interconnected._
- **Should `Bootstrap & Ecran Craft` be split into smaller, more focused modules?**
  _Cohesion score 0.06 - nodes in this community are weakly interconnected._