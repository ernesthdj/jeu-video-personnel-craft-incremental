# UI Screens — Jeu Vidéo Personnel (craft incrémental)

> Source : docs/FOUNDATION.md, docs/GDD.md, docs/CORE-LOOP.md, docs/SYSTEMS.md, docs/SCOPE.md, docs/ARCHITECTURE.md, docs/TECH-STACK.md
> Agent : UI/UX Game Designer (#3) · Pipeline Game Dev · Phase 2 — Design
> Date : 2026-09-15
> Voir aussi : docs/HUD-SPEC.md · docs/FEEDBACK-MAP.md · docs/INPUT-MAP.md
>
> **Contrainte architecturale impérative (Technical Director, #2)** : toute UI communique avec `GameCore`
> **exclusivement** via les interfaces définies (`IMiniGame`, `ICraftProcess`, `ICombatant`, `IInventoryService`,
> `GameEventBus`). Aucun composant de `Presentation` n'accède directement à une classe concrète de logique
> métier. Chaque écran ci-dessous indique l'interface/l'événement auquel il se connecte.
>
> **Priorité de scope** : les écrans CORE (mini-jeux, craft manuel, raffinage, combat, inventaire) sont traités
> en détail. Marché P2P et Boutique F2P ne sont qu'esquissés (cf. SCOPE.md — classés IMPORTANT, backend en stub
> `StubEconomyService`) — pas de design complet, juste le contour pour ne pas bloquer le Gameplay Programmer.

---

## 0. Choix du système UI Unity

**Décision : uGUI (Canvas + Event System) + Input System, PAS UI Toolkit pour le MVP.**

| Option | Retenue / rejetée |
|---|---|
| **UI Toolkit** (UIElements, système de layout retenu type web/CSS-like) | **Rejeté pour le MVP.** Excellent pour des panneaux de données denses (inventaire liste virtualisée, éditeur), mais moins mature pour la capture de gestes tactiles bas-niveau et continus (tracé libre, drag précis multi-échantillons) qu'exige le système de mini-jeux — le socle le plus critique du jeu (SYSTEMS.md §5). Support runtime des éléments world-space (barres de vie flottantes au-dessus des unités sur la grille de combat) plus limité/récent que uGUI. Faire cohabiter deux systèmes UI (uGUI pour le gameplay + UI Toolkit pour les menus) ajouterait une deuxième paradigme UI à maintenir en solo — contredit directement la posture anti-sur-ingénierie déjà actée par le Technical Director (ARCHITECTURE.md §8 : pas de DI tierce, pas d'abstraction hors des 4 axes demandés). |
| **uGUI (Canvas/RectTransform/EventSystem) + Input System** | **Retenu.** Écosystème mature, très documenté (cohérent avec la justification "solo-dev-friendly" de TECH-STACK.md §1), intégration directe et éprouvée avec Input System pour le multi-touch et les gestes continus (`OnInputSample` du mini-jeu peut être alimenté à chaque frame sans couche d'abstraction supplémentaire). Supporte nativement le world-space Canvas pour les éléments diégétiques de combat. Un seul système UI à maîtriser pour un développeur solo. |

**Conséquence pratique** : chaque mini-jeu a son propre `Canvas` (ou sous-hiérarchie isolée) séparé du HUD principal, pour qu'un `Canvas.Rebuild` déclenché par le HUD (ex. mise à jour d'un compteur) ne provoque jamais de re-layout du mini-jeu en cours — condition nécessaire à la contrainte de latence < 100 ms (voir §3.6 et FEEDBACK-MAP.md).

**Point de vigilance transmis au Gameplay Programmer (#4)** : cette décision n'est pas irréversible (voir Selfdoubt §5) — si l'inventaire/le marché grossissent fortement en contenu post-MVP, ré-évaluer UI Toolkit *uniquement* pour ces écrans consultatifs, jamais pour les mini-jeux.

---

## 1. Inventaire des écrans

| Écran | État `GameStateMachine` / contexte | Détail | Interface(s) GameCore consommée(s) |
|---|---|---|---|
| **Boot / Splash** | `Boot` | Esquisse | — |
| **Hub (Écran principal)** | `Hub` | **CORE — détaillé** | `IInventoryService`, `IProductionNode` (résumé farm), `GameEventBus` |
| **Craft manuel** | `Crafting` (sous-mode manuel) | **CORE — détaillé** | `ICraftProcess`, `IMiniGame`/`IMiniGameFactory` |
| **Craft intermédiaire / Raffinage** | `Crafting` (sous-mode raffinage) | **CORE — détaillé** | `ICraftProcess` (`RefinementCraftProcess`), `IMiniGame` |
| **Interface de résolution des mini-jeux** (overlay transversal) | injecté dans `Crafting`/`Combat` | **CORE — le plus soigné** | `IMiniGame` (Initialize/OnInputSample/Evaluate) |
| **Combat tactique (grille)** | `Combat` | **CORE — détaillé** | `ICombatant`, `IActionResolver`, `CombatStateMachine`, `GameEventBus` |
| **Inventaire** | overlay sur `Hub` | **CORE — détaillé** | `IInventoryService` |
| **Automatisation / Farm (panneau)** | overlay sur `Hub` | **CORE — détaillé (léger)** | `IProductionNode`, `OfflineProgressionCalculator` |
| **Résultat de craft / combat (popup)** | overlay transversal | **CORE — détaillé** | `GameEventBus` (`CraftCompleted`, `ItemDestroyed`, `CombatActionResolved`) |
| **Marché P2P** | overlay sur `Hub` | **Esquisse seulement** (IMPORTANT, cf. SCOPE.md) | `IEconomyService` (stub) |
| **Boutique F2P** | overlay sur `Hub` | **Esquisse seulement** (IMPORTANT, cf. SCOPE.md) | `IEconomyService` (stub) |
| **Paramètres / Accessibilité** | overlay global | Esquisse (contenu détaillé en §4) | `ISaveService` (préférences persistées) |

### Transitions

```
Boot ──► Hub ──┬──► Craft manuel ──► [Mini-jeu overlay] ──► Résultat ──► Hub
                ├──► Raffinage ──► [Mini-jeu overlay] ──► Résultat ──► Hub
                ├──► Combat ──► (par tour) [Mini-jeu overlay] ──► Résultat de tour ──► ... ──► Résultat de combat ──► Hub
                ├──► Inventaire (overlay, retour Hub par tap arrière/swipe bas)
                ├──► Farm (overlay, retour Hub)
                ├──► Marché P2P (esquisse, overlay, retour Hub)
                ├──► Boutique F2P (esquisse, overlay, retour Hub)
                └──► Paramètres (overlay global, accessible depuis partout)
```

**Règle de navigation** (Jakob — cohérence plateforme mobile) : Inventaire, Farm, Marché, Boutique sont des **overlays** sur `Hub`, jamais de nouveaux états `GameStateMachine` — ils n'ont pas de logique de résolution (pas de mini-jeu), donc pas besoin d'un état applicatif dédié. Craft et Combat, eux, ont un état dédié car ils orchestrent un cycle complet (sélection → mini-jeu → résolution). Un bouton "retour" cohérent (coin haut-gauche, zone pouce gauche) et un swipe-back Android natif ramènent toujours au Hub — jamais d'impasse de navigation (Nielsen #3, annuler disponible).

---

## 2. Hiérarchie d'information par écran

Légende : 🔴 Critique (doit être vu/compris en < 1s, bloque l'action sinon) · 🟡 Contextuelle (utile à l'instant T, pas bloquante) · ⚪ Consultative (accessible sur demande, jamais imposée).

### Hub
- 🔴 Notifications d'action requise (farm pleine / prête, combat disponible)
- 🔴 Ressources principales (monnaie soft, matériaux clés récents)
- 🟡 Accès rapide craft/combat/inventaire (boutons de navigation)
- ⚪ Monnaie premium (stub — grisée/discrète tant que le marché n'est pas actif), marché, boutique

### Craft manuel
- 🔴 Matériaux requis vs possédés (rouge si insuffisant — bloque le lancement, Nielsen #5)
- 🔴 Flag "échec possible : destruction" sur recettes avancées (Pillar 1/3 — jamais caché, cf. Selfdoubt GDD #3)
- 🟡 Palier de qualité visé / historique de la recette
- ⚪ Détail stats de l'objet produit

### Interface de mini-jeu (tous types)
- 🔴 Zone de jeu (tracé/timing/logique) — occupe l'essentiel de l'écran, zéro distraction
- 🔴 Feedback continu de score/précision (voir §3.6)
- 🟡 Nom de l'action en cours (petit, coin, non intrusif)
- ⚪ Bouton pause/abandon (visible mais discret — jamais au centre, pour éviter un abandon accidentel)

### Combat tactique
- 🔴 PA/PM restants, tour actif, portée d'action sélectionnée
- 🔴 PV des unités visibles sur la grille (diégétique, barres flottantes)
- 🟡 Ordre des tours, effets de statut actifs
- ⚪ Historique du combat / log détaillé des dégâts

### Inventaire
- 🔴 Filtre actif, quantité des items critiques (matériaux de craft en cours)
- 🟡 Tri, rareté (couleur + icône redondante, voir §4)
- ⚪ Stats détaillées d'un item (tap pour déplier)

### Raffinage
- 🔴 État de la chaîne en cours (bloquée/valide/en production)
- 🟡 Rendement estimé, modules disponibles (max 3-5, Miller)
- ⚪ Historique de production

---

## 3. Wireframes textuels

### 3.1 Hub

```
┌──────────────────────────────────────────┐
│ ☰  Nom du Personnage        🔔3   ⚙       │ ← barre haute (safe-area top)
│    ⛃ 1 240 soft   ◈ -- (stub)             │
├──────────────────────────────────────────┤
│                                            │
│         [ Illustration / avatar ]         │
│                                            │
│   ┌────────────┐   ┌─────────────┐        │
│   │ 🔥 Farm     │   │ ⚒ Craft     │        │
│   │ prête (2)   │   │ manuel      │        │
│   └────────────┘   └─────────────┘        │
│   ┌────────────┐   ┌─────────────┐        │
│   │ ⚗ Raffinage │   │ ⚔ Combat    │        │
│   └────────────┘   └─────────────┘        │
│                                            │
├──────────────────────────────────────────┤
│  🎒 Inventaire   🏛 Marché*   🛒 Boutique* │ ← nav basse (safe-area bottom)
└──────────────────────────────────────────┘
  * esquisse seulement, badge "Bientôt" tant que StubEconomyService actif
```
Grille 2×2 de tuiles d'action = cohérent avec Hick-Hyman (max options par groupe), tuiles ≥ 8px multiples,
zones tactiles ≥ 44px conformes à la règle Fitts du référentiel global.

### 3.2 Craft manuel

```
┌──────────────────────────────────────────┐
│ ←  Épée courte — Craft manuel             │
├──────────────────────────────────────────┤
│  [ icône item ]   Recette : Épée courte   │
│                                            │
│  Matériaux requis :                       │
│   • Fer brut     x3   (possédé: 5) ✓      │
│   • Bois         x1   (possédé: 0) ✗      │ ← rouge, bloque le CTA
│                                            │
│  ⚠ Recette avancée — échec = objet détruit│ ← 🔴 badge visible en permanence,
│                                            │   jamais masqué (Pillar 1/3)
├──────────────────────────────────────────┤
│            [   FORGER   ]  (CTA unique)   │
└──────────────────────────────────────────┘
        ↓ tap FORGER (matériaux OK)
┌──────────────────────────────────────────┐
│         → ouvre l'overlay Mini-jeu        │
│           (TracePrecisionMiniGame)        │
└──────────────────────────────────────────┘
```
1 seul CTA par écran (règle globale). Le badge d'échec destructif est **toujours visible avant engagement**,
pas seulement en petit texte — c'est une prévention d'erreur (Nielsen #5), pas une surprise punitive.

### 3.3 Raffinage

```
┌──────────────────────────────────────────┐
│ ←  Chaîne de raffinage — Alliage          │
├────────────────────────┬─────────────────┤
│  Grille de routage      │ Aperçu sortie   │ ← split 62/38 (φ)
│  (LogicRoutingMiniGame) │ Rendement: 74%  │
│  ┌──┬──┬──┬──┐          │ Débit: 2/min    │
│  │▢ │▣ │  │▢ │          │                 │
│  ├──┼──┼──┼──┤          │ Sortie:         │
│  │  │▣ │▢ │  │          │  Alliage x2     │
│  └──┴──┴──┴──┘          │                 │
│  Modules dispo (3-5) :  │                 │
│  [Filtre][Presse][Four] │                 │
├────────────────────────┴─────────────────┤
│         [   VALIDER LA CHAÎNE   ]         │
└──────────────────────────────────────────┘
```
Palette de modules volontairement limitée (3-5, cf. SYSTEMS.md §2 — pas un vrai factory-builder) pour
rester accessible au joueur casual (Hick-Hyman).

### 3.4 Interface de résolution des mini-jeux — **traitement le plus soigné**

Contrainte dure : réactif en **< 100 ms**, car la promesse du jeu entier (Pillar 1) repose sur ce que le
joueur *sente* que son geste compte. Trois variantes MVP (`timing_bar`, `trace_precision`, `logic_routing`),
un même squelette d'écran pour rester prévisible (Jakob) :

```
┌──────────────────────────────────────────┐
│  Forger : Épée courte              ✕      │ ← header minimal, bouton abandon
├──────────────────────────────────────────┤
│                                            │
│         [ ZONE DE JEU — plein écran ]     │
│                                            │
│   (a) timing_bar :                        │
│       ──────[███░░]──────                 │
│       curseur qui traverse une zone verte │
│       cible, tap au bon moment            │
│                                            │
│   (b) trace_precision :                   │
│       tracé de référence en pointillé,    │
│       trait du joueur en direct par-dessus│
│       (couleur = écart en temps réel)     │
│                                            │
│   (c) logic_routing (aperçu combat/raff.) │
│       grille de résolution rapide,        │
│       tap séquentiel sur les nœuds        │
│                                            │
├──────────────────────────────────────────┤
│  ▓▓▓▓▓▓▓▓░░░░░░  Précision: 68%           │ ← 🔴 feedback continu, mis à jour
│                                            │   à CHAQUE OnInputSample, jamais
│                                            │   attendre la fin du geste
└──────────────────────────────────────────┘
```

**Principes techniques pour respecter < 100 ms** (à transmettre au Gameplay Programmer, #4) :
- La zone de jeu et la barre de précision vivent sur un `Canvas` isolé (§0), sans lien de layout avec le HUD.
- `Presentation` échantillonne l'Input System **par frame** et appelle `IMiniGame.OnInputSample` immédiatement
  — aucune mise en file d'attente, aucun `Coroutine.WaitForSeconds` entre l'input et le rendu du feedback.
- Le feedback visuel (couleur de la barre, position du curseur) est piloté en lecture directe de l'état courant
  du mini-jeu à chaque frame `Update`, pas via un événement différé du `GameEventBus` (le bus reste réservé aux
  événements de fin d'action — succès/échec/destruction — pas au flux continu d'input, qui doit rester local
  au composant de présentation du mini-jeu pour éviter la latence d'un abonnement/événement).
- Pas d'animation de transition > 150 ms avant que la zone de jeu ne devienne interactive (le joueur doit
  pouvoir commencer à tracer dès l'apparition de l'overlay).

### 3.5 Combat tactique

```
┌──────────────────────────────────────────┐
│ Tour: Vous   PA 4/4  PM 3/3      ⏸        │ ← 🔴 HUD combat (voir HUD-SPEC.md)
├──────────────────────────────────────────┤
│   ▢ ▢ ▢ ▢ ▢ ▢ ▢                          │
│   ▢ ▢ [🧑PV██░] ▢ ▢ ▢                    │ ← grille, PV flottants diégétiques
│   ▢ ▢ ▢ ▢ [👹PV███] ▢                    │   (world-space Canvas)
│   ▢ ▢ ▢ ▢ ▢ ▢ ▢                          │
├──────────────────────────────────────────┤
│ [Déplacer] [Attaquer] [Compétence] [Objet]│ ← barre d'action, max ~5 (Hick)
│                            [Fin de tour]  │
└──────────────────────────────────────────┘
       ↓ tap Attaquer sur une cible
┌──────────────────────────────────────────┐
│      → overlay Mini-jeu (CombatActionMG)  │ ← même squelette que §3.4
└──────────────────────────────────────────┘
       ↓ résolution
┌──────────────────────────────────────────┐
│   Popup flottant : "-14 dégâts" / "Raté"  │ ← feedback bref sur la grille,
│   retour immédiat à la grille (pas de     │   pas d'écran bloquant
│   modal bloquante)                        │
└──────────────────────────────────────────┘
```

### 3.6 Inventaire

```
┌──────────────────────────────────────────┐
│ ←  Inventaire        [Tout][Craft][Combat]│ ← filtres, 8px grid
├──────────────────────────────────────────┤
│  ┌───┐ ┌───┐ ┌───┐ ┌───┐                  │
│  │▨45│ │▨12│ │▨3 │ │▨1 │  ...grille       │ ← rareté = couleur ET icône
│  └───┘ └───┘ └───┘ └───┘                  │   (jamais couleur seule, §4)
├──────────────────────────────────────────┤
│  [ Détail item sélectionné, panneau 38% ] │ ← panneau slide-up, pas de
│  Nom · Rareté · Stats · Utiliser/Vendre*  │   nouvel écran plein
└──────────────────────────────────────────┘
```

### 3.7 Marché P2P (esquisse — IMPORTANT, pas CORE)

```
┌──────────────────────────────────────────┐
│ ←  Marché                                 │
├──────────────────────────────────────────┤
│   [Icône cadenas] Bientôt disponible      │
│   Le marché ouvrira une fois le système   │
│   économique finalisé.                    │
└──────────────────────────────────────────┘
```
Coquille uniquement — reflète l'état `StubEconomyService` (ARCHITECTURE.md §5). Design complet repoussé,
conformément à SCOPE.md (IMPORTANT, pas CORE-jour-1). Ne pas construire de liste d'annonces/flux d'achat tant
que l'implémentation réelle de `IEconomyService` n'est pas décidée.

### 3.8 Boutique F2P (esquisse — IMPORTANT, pas CORE)

```
┌──────────────────────────────────────────┐
│ ←  Boutique                               │
├──────────────────────────────────────────┤
│   [Icône cadenas] Bientôt disponible      │
└──────────────────────────────────────────┘
```
Même statut que §3.7. Point de vigilance déjà noté en SYSTEMS.md §7 : quand cet écran sera repris, la règle
"jamais d'achat de puissance ni de résultat de mini-jeu" devra être visible dans le design (catégories
d'objets non-achetables explicitement absentes de la boutique), pas seulement dans la doc — hors scope ici.

---

## 4. Accessibilité gaming

Contexte critique : les mini-jeux reposent sur la perception visuelle et la précision tactile — une
accessibilité mal pensée exclurait une partie du public du **cœur du jeu**, pas d'une fonctionnalité annexe.

| Axe | Mesures |
|---|---|
| **Remapping / contrôle** | Deux schémas de contrôle par mini-jeu quand c'est pertinent : mode "Drag" (par défaut) et mode "Tap-assisté" (le joueur tape des points-clés au lieu de tracer en continu, pour les utilisateurs à mobilité fine réduite). Slider de tolérance de précision (élargit la zone de succès) — **gratuit, jamais monétisé** (cf. Pillar 3 : ne pas confondre accessibilité et pay-to-skip, distinction à documenter clairement dans l'UI des paramètres pour ne pas créer d'ambiguïté P2W perçue). |
| **Sous-titres / signal visuel des cues audio** | Les mini-jeux à composante rythmique (`timing_bar`) ne doivent **jamais** reposer uniquement sur un signal audio pour le timing — toujours un indicateur visuel équivalent (métronome visuel synchronisé). Sous-titres sur tout dialogue/texte narré si ajouté plus tard. |
| **Taille de texte** | Option d'échelle de texte (100/125/150%) dans Paramètres, respectant les zones tactiles minimales (44px) même à l'échelle par défaut — ne pas dépendre de l'agrandissement pour rester lisible en 100%. |
| **Daltonisme** | 3 modes (protanopie/deutéranopie/tritanopie) qui recolorent la palette de rareté, les zones de précision des mini-jeux (vert/jaune/rouge) et les indicateurs PV/dégâts. **Règle non négociable** : aucune information critique (rareté, réussite/échec de zone, PV bas) ne repose sur la couleur seule — toujours doublée d'une forme/icône/motif (cf. wireframes §3.6, §3.4). Cette règle doit être respectée dès la première implémentation, pas ajoutée après coup, car elle conditionne des choix de design (formes des zones de mini-jeu) difficiles à changer rétroactivement. |
| **Feedback haptique optionnel** | Toggle on/off global (certains joueurs/appareils y sont sensibles), voir FEEDBACK-MAP.md pour le détail par action. |

---

## 5. Selfdoubt — lisibilité, surcharge, tactile mobile

| # | Affirmation | Niveau | Action recommandée |
|---|---|---|---|
| 1 | uGUI seul (sans UI Toolkit) reste suffisant même quand l'inventaire/le marché grossiront en contenu | ⚠️ Probable | Réévaluer uniquement si le contenu (nombre d'items, filtres) explose post-MVP ; uGUI avec `ScrollRect` + pooling suffit pour la vertical slice et le MVP CORE. |
| 2 | L'overlay mini-jeu plein écran (§3.4) ne casse pas le rythme des joueurs casual qui enchaînent beaucoup de crafts courts | ❌ Hypothèse | C'est directement lié au risque #1 du GDD (les mini-jeux doivent rester fun en répétition). À tester en priorité sur le prototype vertical slice : mesurer le temps de transition Hub → Craft → Mini-jeu → Résultat → Hub, viser un cycle perçu < 10s pour une recette simple. Si trop lent/lourd, envisager une variante "inline" (mini-jeu intégré sans overlay plein écran) pour les recettes de base uniquement. |
| 3 | La contrainte < 100 ms est atteignable avec uGUI standard sans optimisation spécifique | ⚠️ Probable | Le principe d'isolement de Canvas (§0, §3.4) est une précaution, pas une garantie mesurée — le Gameplay Programmer doit profiler la latence input→rendu sur device réel bas de gamme dès le premier mini-jeu implémenté, pas seulement sur l'éditeur/un device haut de gamme. |
| 4 | Le nombre de taps du cycle craft manuel (Hub → tuile Craft → Forger → geste mini-jeu → Collecter → retour Hub) est raisonnable pour une session casual de 5 minutes | ⚠️ Probable | Non chronométré ici (pas de device en main à cette phase). Ratio estimé : ~3 taps hors mini-jeu par craft. À valider en playtest réel avec le vertical slice. |
| 5 | Le badge "échec = destruction" toujours visible (§3.2) suffit à éviter la frustration perçue comme injuste | ⚠️ Probable | Le GDD (Selfdoubt #3) identifie déjà ce risque comme non résolu par le design seul — la visibilité de l'info réduit le sentiment d'injustice mais ne remplace pas la calibration de la difficulté (hors scope UI/UX), qui reste à valider en playtest par le Game Designer/QA. |
| 6 | Les zones tactiles ≥ 44px et le split 62/38 suffisent à un usage confortable une main sur petit écran (iPhone SE class) | ⚠️ Probable | Aucune maquette haute-fidélité ni test device n'a été fait à ce stade (wireframes textuels seulement) — à vérifier dès que le Gameplay Programmer implémente les premiers prefabs UI. |

**Hedge-to-Verify Ratio** : 5 affirmations sur 6 en ⚠️ Probable ou ❌ Hypothèse (83%) — cohérent avec le ratio
élevé déjà assumé par le Game Designer (GDD.md, 89%) : à ce stade (wireframes textuels, pas de prototype
jouable), la quasi-totalité des paris de confort tactile et de rythme perçu restent à vérifier sur device réel,
pas sur papier.
