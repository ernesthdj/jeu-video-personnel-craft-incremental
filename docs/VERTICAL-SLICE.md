# Vertical Slice Technique — Jeu Vidéo Personnel (craft incrémental)

> Agent : Technical Director (#2) · Pipeline Game Dev · Phase 1 — Fondation
> Date : 2026-09-15
> Objectif : premier prototype jouable minimal qui **prouve l'architecture** (ARCHITECTURE.md) autant que le gameplay,
> centré sur l'affirmation #1 du Selfdoubt du GDD ("les mini-jeux restent fun après répétition") — la priorité du Game Designer.

---

## 1. Scope du prototype

Un cycle complet et jouable :
1. **Craft manuel** — choix d'une recette → mini-jeu de dextérité → résultat (qualité ou destruction) → mise à jour d'inventaire.
2. **Combat tactique simple** — grille, déplacement, une action offensive résolue par mini-jeu, un ennemi basique, victoire/défaite.

Explicitement **hors** de ce slice : raffinage, automatisation/farm réelle (juste le point d'extension), marché P2P, boutique F2P (stub enregistré mais aucune UI).

## 2. Ordre d'implémentation

| # | Étape | Ce qu'elle prouve |
|---|---|---|
| 1 | Squelette `GameCore` : interfaces `IMiniGame`, `ICraftProcess`, `ICombatant`, `IEconomyService` (stub), `GameServices` (registry), `GameEventBus` | L'architecture en couches compile et est testable en Edit Mode avant même d'avoir une scène Unity |
| 2 | ScriptableObjects minimaux : `MiniGameConfigSO`, `CraftRecipeSO`, `ItemDefinitionSO`, `CombatantDefinitionSO` (champs réduits au strict nécessaire du slice) | La couche data-driven existe et est éditable dans l'inspecteur Unity |
| 3 | Une implémentation concrète de `IMiniGame` (ex. `TimingBarMiniGame`) + prefab UI + capture d'input tactile (Input System) | Le système le plus critique (SYSTEMS.md §5) fonctionne de bout en bout — priorité absolue car tout le reste en dépend |
| 4 | `ManualCraftProcess` câblé au mini-jeu + mise à jour d'inventaire (`LocalInventoryService`) + événements (`CraftCompleted`, `ItemDestroyed`) + feedback UI minimal | Le Strategy pattern (§2.1 ARCHITECTURE.md) est validé avec un vrai appelant ; le pilier "l'habileté prime sur le hasard" (GDD.md Pillar 1) est concrètement démontrable |
| 5 | `ISaveService`/`JsonFileSaveService` — sauvegarde/chargement de l'inventaire et de l'état joueur basique | Le cycle complet survit à une fermeture/réouverture de l'app |
| 6 | Combat : `CombatStateMachine` (Idle → SélectionAction → MiniJeu → Résolution → TourSuivant), grille simple (sans pathfinding A*), **réutilisation** du même `IMiniGame` framework pour l'action de combat (`CombatActionMiniGame`), un `AggressiveAI` basique | Prouve que le framework mini-jeu est réellement transversal (pas seulement pensé pour le craft) — validation directe de l'extensibilité demandée |
| 7 | `StubEconomyService` enregistré dans `GameServices` au bootstrap, sans aucune UI | Le point d'extension économie existe et compile, sans détourner de temps de dev vers l'économie (conforme au cadrage de mentalyas) |

## 3. Stubs à prévoir explicitement

- `IEconomyService` → `StubEconomyService` (voir ARCHITECTURE.md §5) — enregistré, jamais appelé par une UI dans ce slice.
- `IAuthoritativeValidationService` → `LocalValidationService` — validation faite en local (honnête pour un test solo), swappable vers un vrai serveur plus tard sans changer les appelants.
- `IProductionNode` (automatisation/farm) — l'interface peut être posée dans `GameCore` sans implémentation runtime complète dans le slice ; ce n'est pas nécessaire pour prouver craft + combat, et FOUNDATION.md/SCOPE.md la classent CORE mais séquencée après (le Game Designer place la farm après le craft manuel dans le séquençage — cohérent).

## 4. Critères de complétion du slice

- Un joueur peut lancer un craft manuel, jouer le mini-jeu, voir le résultat (item créé avec une qualité, ou détruit en cas d'échec), et retrouver cet état après avoir fermé/rouvert l'app.
- Un joueur peut entrer en combat, se déplacer sur la grille, déclencher une action résolue par le même type de mini-jeu que le craft (ou une variante), et voir le combat se terminer (victoire/défaite).
- Aucun système de combat ou de craft ne référence une classe concrète de mini-jeu — uniquement `IMiniGame` via `IMiniGameFactory` (vérifiable par revue de code : aucun `new TimingBarMiniGame()` en dehors de `GameBootstrap`).
- `GameCore` compile et est testable sans dépendre de scènes Unity (tests Edit Mode sur `ManualCraftProcess`, `OfflineProgressionCalculator` si implémenté, `CombatStateMachine`).

## 5. Ce que ce slice ne cherche pas à démontrer

Ni l'équilibrage (qualité vs difficulté du mini-jeu), ni le fun à long terme (répétabilité) — ce slice est un test **technique et architectural**, pas encore le test de playtest recommandé par le Selfdoubt du GDD (§6, affirmation #1). Une fois ce slice stable, le playtest réel (prototyper et faire tester 2-3 mini-jeux hors du studio, comme recommandé par le GDD) devient l'étape suivante prioritaire, avant d'investir dans le contenu des autres systèmes.
