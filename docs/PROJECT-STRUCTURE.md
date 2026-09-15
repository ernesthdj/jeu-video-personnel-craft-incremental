# Structure de Projet — Jeu Vidéo Personnel (craft incrémental)

> Agent : Technical Director (#2) · Pipeline Game Dev · Phase 1 — Fondation
> Date : 2026-09-15
> Découle directement d'ARCHITECTURE.md §1 (séparation GameCore/Content/Presentation).

---

## 1. Arborescence Unity

```
jeu-video-personnel-craft-incremental/
├── CLAUDE.md
├── docs/                              # Livrables pipeline (déjà en place)
├── graphify-out/
├── src/
│   └── UnityProject/                  # Projet Unity (nom de dossier réel à trancher à l'ouverture)
│       ├── Assets/
│       │   ├── _Project/
│       │   │   ├── GameCore/          # Assembly GameCore.asmdef — C# pur, testable, sans MonoBehaviour
│       │   │   │   ├── MiniGames/     # IMiniGame, implémentations, MiniGameRegistry
│       │   │   │   ├── Crafting/      # ICraftProcess, ManualCraftProcess, RefinementCraftProcess
│       │   │   │   ├── Combat/        # ICombatant, IEnemyAI, CombatStateMachine
│       │   │   │   ├── Production/    # IProductionNode, OfflineProgressionCalculator
│       │   │   │   ├── Items/         # IItemFactory, ItemInstance
│       │   │   │   ├── Economy/       # IEconomyService, StubEconomyService (voir ARCHITECTURE.md §5)
│       │   │   │   ├── Save/          # ISaveService, JsonFileSaveService
│       │   │   │   ├── Events/        # GameEventBus
│       │   │   │   ├── Registry/      # GameServices (Service Locator)
│       │   │   │   └── GameCore.asmdef
│       │   │   ├── Content/           # Assembly Content.asmdef — définitions de ScriptableObjects
│       │   │   │   ├── Definitions/   # CraftRecipeSO, ItemDefinitionSO, CombatantDefinitionSO...
│       │   │   │   └── Content.asmdef
│       │   │   ├── ContentData/       # Assets ScriptableObject instanciés (le vrai contenu du jeu)
│       │   │   │   ├── Recipes/
│       │   │   │   ├── MiniGames/
│       │   │   │   ├── Items/
│       │   │   │   └── Combatants/
│       │   │   ├── Presentation/      # Assembly Presentation.asmdef — MonoBehaviours, UI, VFX
│       │   │   │   ├── Crafting/
│       │   │   │   ├── Combat/
│       │   │   │   ├── UI/
│       │   │   │   ├── Bootstrap/     # GameBootstrap — seul point qui connaît les classes concrètes
│       │   │   │   └── Presentation.asmdef
│       │   │   ├── Art/               # Sprites, modèles, animations
│       │   │   ├── Audio/
│       │   │   └── Addressables/      # Config Addressables
│       │   ├── Packages/
│       │   └── ProjectSettings/
│       └── (fichiers générés Unity : Library/, Temp/, Obj/, Logs/ — jamais versionnés)
└── tests/
    └── UnityProject/
        ├── EditMode/                  # Tests unitaires de GameCore (pas besoin du Play Mode)
        │   ├── MiniGames/
        │   ├── Crafting/
        │   └── Combat/
        └── PlayMode/                  # Tests d'intégration Presentation <-> GameCore
```

## 2. Conventions de code C#

Conformes aux conventions Microsoft déjà actées dans le CLAUDE.md global (PascalCase classes/méthodes, camelCase variables, async/await sur toute I/O).

| Élément | Convention | Exemple |
|---|---|---|
| Classes, interfaces, méthodes publiques | PascalCase, interfaces préfixées `I` | `IMiniGame`, `CraftResult` |
| Variables locales, paramètres | camelCase | `craftRecipe`, `inputSample` |
| Champs privés sérialisés (Presentation) | `_camelCase` + `[SerializeField] private` | `[SerializeField] private float _timingWindow;` |
| ScriptableObjects | Suffixe `SO` | `CraftRecipeSO`, `MiniGameConfigSO` |
| Assemblies | Un `.asmdef` par couche (§1), noms explicites | `GameCore.asmdef` |
| Namespaces | Miroir de l'arborescence, préfixe projet | `Game.Core.MiniGames`, `Game.Presentation.Combat` |
| Async | `async/await`, jamais `.Result`/`.Wait()` (règle globale) | `await saveService.SaveAsync(data);` |
| Nouveau système/mini-jeu | Enregistrement centralisé dans `GameBootstrap` uniquement (voir ARCHITECTURE.md §2.3) | — |

## 3. Pipeline Git

### Git + LFS
Unity produit des assets binaires volumineux (textures, audio, modèles, animations) — **Git LFS (Large File Storage — extension Git qui stocke les gros fichiers binaires hors de l'historique standard)** est obligatoire dès l'initialisation du dépôt.

`.gitattributes` (à créer à l'ouverture du projet Unity) :
```
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text
*.unitypackage filter=lfs diff=lfs merge=lfs -text
```

### `.gitignore` Unity (standard, à appliquer dès la création du projet)
```
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Mm]emoryCaptures/
[Uu]serSettings/
*.csproj
*.sln
*.suo
.vs/
```

### Discipline
- Branches courtes (solo dev pour l'instant) ; passage à un vrai workflow de branches par feature au recrutement de collaborateurs.
- Règles git globales du CLAUDE.md s'appliquent intégralement (jamais de commit sans confirmation, jamais `--force` sur main, jamais `--no-verify`, commits atomiques).

## 4. Build mobile

- **iOS** : build via Xcode généré par Unity (nécessite macOS pour la signature/soumission — point d'attention matériel à anticiper).
- **Android** : build direct `.aab`/`.apk` depuis Unity.
- **CI/CD** : GitHub Actions + Unity Cloud Build (déjà suggéré en FOUNDATION.md) — **différé** : pour la vertical slice et le MVP solo, des builds manuels locaux suffisent. Automatiser au moment où des collaborateurs rejoignent (évite de maintenir un pipeline CI pour un seul développeur qui build directement dans l'éditeur).

## 5. Notes de complétion

- Le nom réel du dossier `src/UnityProject/` sera fixé à l'ouverture effective du projet Unity (dépend de la version LTS choisie à ce moment). Cette structure documente l'organisation cible, pas encore un projet Unity initialisé.
