# HUD Spec — Jeu Vidéo Personnel (craft incrémental)

> Source : docs/UI-SCREENS.md, docs/ARCHITECTURE.md, docs/SYSTEMS.md
> Agent : UI/UX Game Designer (#3) · Pipeline Game Dev · Phase 2 — Design
> Date : 2026-09-15
>
> Spécification du HUD (Heads-Up Display) en jeu — les éléments d'interface superposés en permanence ou en
> contexte, par opposition aux écrans pleins (voir UI-SCREENS.md). Le HUD lit l'état via `GameEventBus` et
> les interfaces `GameCore` (`IInventoryService`, `IProductionNode`, `ICombatant`) — jamais d'accès direct à
> la logique métier, conformément à la contrainte du Technical Director.

---

## 1. Principes

- **Feedback immédiat < 200ms** (règle ergonomique globale) pour toute mise à jour de compteur/ressource — le
  HUD s'abonne à `GameEventBus` (`CraftCompleted`, `ItemDestroyed`, `CombatActionResolved`, `ProductionTicked`)
  et ne poll jamais l'état par frame pour des valeurs qui ne changent que sur événement.
- **Isolation de Canvas** (cf. UI-SCREENS.md §0) : le HUD vit sur un `Canvas` distinct de celui des mini-jeux,
  pour qu'une mise à jour de compteur HUD ne déclenche jamais de re-layout pendant une résolution de mini-jeu
  — c'est la condition technique qui protège la contrainte < 100ms du mini-jeu.
- **Safe area mobile obligatoire** : tout élément fixé en haut ou en bas de l'écran respecte les encoches/
  barres système (notch, home indicator iOS, barre de navigation Android) via `Screen.safeArea`. Grille 8px,
  padding panneau 16px horizontal / 12px vertical (référentiel global).
- **3 niveaux de hiérarchie visuelle max**, jamais différenciés par taille + graisse + couleur en même temps
  (référentiel global — Gestalt figure/fond, contraste ≥ 4.5:1).

---

## 2. HUD permanent (visible sur `Hub`, persiste en filigrane sur les autres écrans si pertinent)

| Élément | Position | Priorité | Source de donnée | Mise à jour |
|---|---|---|---|---|
| Monnaie soft (⛃) | Haut-gauche, à côté de l'avatar | 🔴 Critique | `IInventoryService` (solde) | `GameEventBus` (tout événement modifiant l'inventaire soft) |
| Monnaie premium (◈) | Haut-gauche, juste après le soft, **grisée/atténuée** tant que `StubEconomyService` actif | ⚪ Consultative (MVP) | `IEconomyService` (stub) | Statique en MVP |
| Notifications (🔔 + badge numérique) | Haut-droite | 🔴 Critique | agrégation `ProductionTicked` (farm prête), `CombatActionResolved` en attente, etc. | Push dès réception d'un event |
| Accès paramètres (⚙) | Haut-droite, extrême bord (zone pouce faible fréquence, cohérent Fitts — action rare) | ⚪ Consultative | — | — |
| Navigation basse (Inventaire / Marché* / Boutique*) | Bas, zone pouce (Fitts — actions fréquentes) | 🟡 Contextuelle | — | — |

`*` = icônes présentes mais menant à l'esquisse "Bientôt disponible" (voir UI-SCREENS.md §3.7-3.8).

---

## 3. HUD contextuel — Craft manuel / Raffinage

| Élément | Position | Priorité | Notes |
|---|---|---|---|
| Bouton retour (←) | Haut-gauche | 🟡 Contextuelle | Toujours au même endroit sur tous les écrans (Jakob — cohérence) |
| Liste matériaux requis vs possédés | Corps de l'écran | 🔴 Critique | Rouge + icône ✗ si insuffisant, bloque le CTA (voir UI-SCREENS.md §3.2) |
| Badge "échec = destruction" | Sous la liste de matériaux, permanent sur recette avancée | 🔴 Critique | Jamais masqué, jamais en petit texte gris — même poids visuel que les matériaux |
| CTA "Forger" / "Valider la chaîne" | Bas, pleine largeur, zone pouce | 🔴 Critique | Unique CTA de l'écran, désactivé (grisé, pas caché) tant que les conditions ne sont pas remplies — état visible = Nielsen #1 |

---

## 4. HUD contextuel — Combat tactique

| Élément | Position | Priorité | Notes |
|---|---|---|---|
| Compteurs PA/PM | Haut, centré | 🔴 Critique | Mise à jour immédiate à chaque action (Nielsen #1 — visibilité de l'état système) |
| Indicateur de tour (Vous / Ennemi) | Haut, à côté des compteurs PA/PM | 🔴 Critique | Couleur + libellé texte (pas couleur seule, accessibilité daltonisme) |
| Barres de PV flottantes (diégétique, world-space Canvas) | Au-dessus de chaque unité sur la grille | 🔴 Critique | Suit la position de l'unité ; jamais recalculée par le HUD principal (Canvas séparé, cf. §1) |
| Barre d'action (Déplacer/Attaquer/Compétence/Objet/Fin de tour) | Bas, zone pouce | 🔴 Critique | Max 5 boutons (Hick-Hyman), icône + libellé court, désactivés si hors PA/PM disponibles |
| Log de combat (texte défilant) | Accessible via un onglet rétractable, pas affiché en permanence | ⚪ Consultative | Évite la surcharge d'info pendant l'action rapide (Miller — pas plus de 4-5 éléments visibles à la fois) |
| Pause / menu combat (⏸) | Haut-droite | 🟡 Contextuelle | |

---

## 5. HUD contextuel — Interface de mini-jeu

Réduit au strict minimum (voir UI-SCREENS.md §3.4 pour le wireframe complet) :

| Élément | Position | Priorité |
|---|---|---|
| Nom de l'action en cours | Haut, petit, non intrusif | 🟡 Contextuelle |
| Bouton abandon (✕) | Haut-droite, coin, jamais centré | ⚪ Consultative (mais toujours accessible — Nielsen #3, annuler disponible) |
| Barre/indicateur de précision en temps réel | Bas, pleine largeur | 🔴 Critique — c'est l'élément dont dépend toute la lisibilité du mini-jeu |

**Rien d'autre.** Aucun élément de HUD global (monnaie, notifications) ne doit apparaître pendant la
résolution d'un mini-jeu — la charge cognitive doit être entièrement dédiée au geste (Tesler : la complexité
est absorbée par l'UI qui masque temporairement le reste, pas déportée sur l'attention du joueur).

---

## 6. HUD contextuel — Automatisation / Farm

| Élément | Position | Priorité | Notes |
|---|---|---|---|
| Jauge de production / cap de stockage | Panneau overlay depuis le Hub | 🔴 Critique | Affiche explicitement "plein" quand le cap est atteint (évite la frustration silencieuse identifiée dans SYSTEMS.md §3) |
| Temps depuis dernière collecte | Panneau | 🟡 Contextuelle | Calculé côté `OfflineProgressionCalculator`, jamais recalculé côté UI |
| Bouton "Collecter" | Panneau, CTA unique | 🔴 Critique | |

---

## 7. Résumé des zones safe-area (mobile portrait)

```
┌ safe-area-top ───────────────────────────┐
│  HUD haut (monnaie / notifs / settings)   │
├────────────────────────────────────────── │
│                                            │
│              Contenu écran                │
│                                            │
├────────────────────────────────────────── │
│  HUD bas (navigation / CTA / barre action) │
└ safe-area-bottom ─────────────────────────┘
```
Aucun élément interactif ne doit chevaucher `env(safe-area-inset-top/bottom)` équivalent Unity
(`Screen.safeArea`) — risque de zones tactiles inaccessibles sur devices à encoche.
