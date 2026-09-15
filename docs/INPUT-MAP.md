# Input Map — Jeu Vidéo Personnel (craft incrémental)

> Source : docs/UI-SCREENS.md, docs/ARCHITECTURE.md (`IMiniGame.OnInputSample`), docs/TECH-STACK.md (Input System)
> Agent : UI/UX Game Designer (#3) · Pipeline Game Dev · Phase 2 — Design
> Date : 2026-09-15
>
> Mapping des contrôles tactiles mobiles (iOS/Android). Toute capture d'input passe par le package **Input
> System** (TECH-STACK.md §3) côté `Presentation`, qui transforme les gestes bruts en `MiniGameInputSample`
> transmis à `IMiniGame.OnInputSample` — `Presentation` ne contient aucune logique d'interprétation du
> résultat, seulement de la capture et du forwarding (contrainte architecturale du Technical Director).

---

## 1. Gestes globaux (hors mini-jeu)

| Geste | Usage | Écrans concernés |
|---|---|---|
| **Tap** | Sélection, navigation, validation de CTA | Tous |
| **Tap long (hold ~400ms)** | Aperçu détail rapide (ex. stats d'un item sans ouvrir le panneau complet) | Inventaire, Combat (survol unité) |
| **Drag / swipe horizontal** | Défilement de listes (inventaire, log de combat) | Inventaire, Combat |
| **Swipe bas / bouton retour matériel Android** | Retour à l'écran précédent | Tous overlays |
| **Pincement (pinch)** | Zoom sur la grille de combat (optionnel, confort) | Combat uniquement |

**Principe d'accessibilité de navigation** : la navigation globale (menus, inventaire, validation) ne repose
**jamais** sur un geste complexe ou chronométré — uniquement tap/drag simples, pour ne pas superposer une
exigence de dextérité à des écrans qui ne sont pas des mini-jeux (Tesler — absorber la complexité côté
gameplay ciblé, pas la répandre partout).

---

## 2. Contrôles par type de mini-jeu

### 2.1 `timing_bar` (craft manuel simple, actions de combat rapides)

| Geste | Effet | Alternative accessibilité |
|---|---|---|
| **Tap unique** au moment où le curseur traverse la zone cible | Envoie un `MiniGameInputSample` avec le timing exact | Mode "Tap-assisté" : zone cible élargie + curseur ralenti (slider de tolérance, gratuit) |

### 2.2 `trace_precision` (craft manuel avancé)

| Geste | Effet | Alternative accessibilité |
|---|---|---|
| **Drag continu** (appui + déplacement du doigt suivant un tracé de référence) | Chaque frame d'appui envoie un `MiniGameInputSample` (position, delta au tracé de référence) | Mode "Points-clés" : au lieu d'un tracé continu, le joueur tape une séquence de points-clés dans l'ordre — réduit l'exigence de motricité fine continue |
| **Relâcher avant la fin du tracé** | Termine prématurément le geste — traité comme un échantillon incomplet par `IMiniGame.Evaluate()` (score partiel, jamais un crash ou un blocage) | — |

### 2.3 `logic_routing` (raffinage, résolution combat basée logique)

| Geste | Effet | Alternative accessibilité |
|---|---|---|
| **Tap** sur un module dans la palette | Sélectionne le module à placer | — |
| **Tap** sur une case de la grille | Place/déplace le module sélectionné | — |
| **Tap long** sur un module déjà placé | Ouvre les options (rotation, retrait) | Bouton dédié "Rotation" visible en permanence (alternative au tap long pour les joueurs qui ont du mal avec les gestes maintenus) |

### 2.4 `CombatActionMiniGame` (résolution d'action en combat)

Réutilise le squelette de contrôle de `timing_bar` ou `trace_precision` selon le type d'action (attaque
rapide = timing, compétence de précision = trace) — cohérence intentionnelle avec §2.1/§2.2 pour ne pas
demander au joueur d'apprendre un troisième vocabulaire gestuel en combat (Jakob).

---

## 3. Contrôles par écran (hors mini-jeu)

| Écran | Contrôles spécifiques |
|---|---|
| **Hub** | Tap sur tuile = navigation. Pas de drag/swipe nécessaire (grille fixe, pas de scroll en MVP). |
| **Craft manuel / Raffinage** | Tap sur CTA. Raffinage : drag pour déplacer un module déjà placé sur la grille (en dehors du mini-jeu actif — placement de la chaîne, pas résolution temps réel). |
| **Combat — sélection d'action** | Tap sur unité pour sélectionner, tap sur case pour déplacer/cibler, tap sur bouton de la barre d'action pour choisir le type d'action. Portée valide surlignée avant confirmation (prévention d'erreur, Nielsen #5). |
| **Inventaire** | Tap pour sélectionner un item (ouvre panneau détail), drag vertical pour scroller la grille. |
| **Paramètres** | Tap uniquement (sliders = drag horizontal court, toggles = tap). |

---

## 4. Remapping et options d'accessibilité (Paramètres)

| Option | Effet | Portée |
|---|---|---|
| Mode de contrôle mini-jeu : **Drag** (défaut) / **Tap-assisté** / **Points-clés** | Change le schéma d'input attendu par `IMiniGame` pour les types concernés, sans changer la logique de scoring sous-jacente — uniquement la façon dont l'input est capté | `trace_precision`, `timing_bar` |
| Slider de tolérance de précision | Élargit les zones de succès (gratuit, non lié à la monétisation — cf. UI-SCREENS.md §4) | Tous mini-jeux |
| Vitesse des mini-jeux (curseur/tracé de référence) | Ralentit le rythme sans changer le résultat maximal atteignable | `timing_bar`, `trace_precision` |
| Bouton "Rotation" toujours visible (raffinage) | Évite de dépendre du tap long | `logic_routing` |
| Sensibilité tactile générale | Ajuste le seuil de détection de drag vs tap accidentel | Global |
| Zone de confort une main (gaucher/droitier) | Bascule la position des CTA/barre d'action côté gauche ou droite | Combat, écrans avec CTA latéraux |

**Rappel Pillar 3 (GDD.md)** : toutes les options ci-dessus sont **gratuites et non liées à la monétisation** —
elles compensent une contrainte motrice/perceptive, elles n'achètent jamais un résultat supérieur à ce
qu'un joueur sans contrainte obtiendrait au même niveau de skill. Cette distinction doit être explicite dans
le texte d'aide de l'écran Paramètres pour éviter toute perception de pay-to-win (il n'y a pas d'achat associé
à ces options, mais la limite doit rester lisible pour la communauté).

---

## 5. Latence et échantillonnage (rappel technique)

- Capture via **Input System**, échantillonnage à chaque frame pendant un mini-jeu actif (pas de polling à
  intervalle fixe indépendant du framerate, pour rester cohérent avec le budget de 16-33ms visé par
  ARCHITECTURE.md §0).
- `MiniGameInputSample` transporte au minimum : position écran/normalisée, timestamp, delta depuis le sample
  précédent — suffisant pour que chaque implémentation concrète de `IMiniGame` calcule son propre scoring sans
  que `Presentation` n'ait à connaître la logique de notation.
- Aucune interprétation de résultat (succès/échec/score) ne doit être calculée côté `Presentation` — elle lit
  uniquement l'état exposé par `IMiniGame` pour le feedback visuel (voir FEEDBACK-MAP.md §3).
