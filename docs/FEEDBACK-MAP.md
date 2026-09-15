# Feedback Map — Jeu Vidéo Personnel (craft incrémental)

> Source : docs/UI-SCREENS.md, docs/HUD-SPEC.md, docs/ARCHITECTURE.md
> Agent : UI/UX Game Designer (#3) · Pipeline Game Dev · Phase 2 — Design
> Date : 2026-09-15
>
> Tableau action → feedback (visuel / audio / haptique), tous déclenchés en < 100ms de l'action ou de
> l'événement `GameEventBus` correspondant, avec une intensité **proportionnelle à l'enjeu** — un craft raté
> qui détruit l'objet doit se sentir très différent d'un simple ajustement de trajectoire pendant le mini-jeu.

---

## 1. Échelle d'intensité de feedback

| Niveau | Quand l'utiliser | Visuel | Audio | Haptique |
|---|---|---|---|---|
| **Micro** | Ajustement continu pendant un geste (input sample) | Variation de couleur/opacité fine | Tick discret ou silence | Vibration très courte (~10-15ms), optionnelle |
| **Standard** | Résultat d'une action réussie sans enjeu élevé (craft de base, dégât normal) | Flash de couleur, popup chiffré, particules légères | SFX court et net | Vibration courte (~30-40ms) |
| **Fort** | Résultat exceptionnel positif (qualité parfaite, coup critique, palier débloqué) | Animation plus ample, glow, confettis/particules riches | SFX plus long, montée harmonique | Vibration en double-pulse |
| **Critique négatif** | Échec avec conséquence permanente (destruction d'item, KO d'unité) | Écran secoué léger, item qui se fissure/s'effondre visuellement, couleur rouge/désaturation momentanée | SFX de bris/impact distinct, net, pas ambigu avec un succès | Vibration longue et distincte (~150-200ms), pattern différent des autres (pas confondable) |

**Règle de proportionnalité** : le niveau "Critique négatif" doit être **acoustiquement et visuellement
inconfondable** avec un succès — c'est une exigence directe de Pillar 1 du GDD (le joueur doit comprendre
immédiatement pourquoi il a raté, pas seulement qu'il s'est passé quelque chose).

---

## 2. Tableau action → feedback

| Action / Événement (`GameEventBus`) | Visuel | Audio | Haptique | Timing | Intensité |
|---|---|---|---|---|---|
| **Input sample pendant mini-jeu** (drag/trace en cours, hors event bus — lu en direct depuis `IMiniGame`) | Couleur de la barre/zone évolue en continu (vert→jaune→rouge selon écart) | Aucun ou tick très léger si le joueur sort de la zone cible | Vibration micro à l'entrée/sortie de la zone de précision | < 1 frame (16-33ms) | Micro |
| **Entrée dans la fenêtre de timing (timing_bar)** | Zone cible qui pulse légèrement | Tick discret | Micro-vibration | < 100ms | Micro |
| `CraftCompleted` — qualité commune/standard | Popup "+1 [item]" avec icône, léger flash | SFX craft neutre | Vibration courte | < 100ms après `Evaluate()` | Standard |
| `CraftCompleted` — qualité rare/parfaite | Popup doré avec animation d'agrandissement, particules | SFX ascendant, plus riche | Double-pulse | < 100ms | Fort |
| `ItemDestroyed` (échec destructif) | Item qui se fissure puis disparaît en particules ternes, écran légèrement secoué (shake court, pas nauséeux) | SFX de bris net et grave, distinct de tout succès | Vibration longue distincte | < 100ms | Critique négatif |
| Échec simple (qualité en baisse, pas de destruction) | Flash rouge/orange bref, item reste mais icône "qualité réduite" | SFX négatif court mais pas alarmant | Vibration courte, pattern différent du succès | < 100ms | Standard |
| `ProductionTicked` — farm prête à collecter | Badge de notification qui apparaît sur la tuile Hub | SFX de notification discret (uniquement si l'app est au premier plan) | Aucune (évite le spam haptique en idle) | < 200ms après le calcul | Standard |
| `ProductionTicked` — collecte manuelle réussie | Compteur qui s'incrémente visuellement (count-up rapide), icône ressource qui "vole" vers le compteur | SFX de collecte court | Vibration courte | < 100ms | Standard |
| Raffinage — chaîne valide et lancée | Animation de flux dans la grille de modules | SFX mécanique court | Vibration courte | < 100ms | Standard |
| Raffinage — chaîne invalide (routage impossible) | Nœuds en erreur surlignés en rouge + icône ⚠ (pas couleur seule) | SFX d'erreur discret | Aucune | < 100ms | Standard (négatif léger, pas critique — rien n'est perdu) |
| `CombatActionResolved` — coup réussi | Popup dégâts flottant sur la cible, flash sur la barre de PV qui descend | SFX d'impact | Vibration courte | < 100ms | Standard |
| `CombatActionResolved` — coup critique/bonus | Popup dégâts agrandi + couleur distincte, écran zoom léger sur l'impact | SFX d'impact renforcé | Double-pulse | < 100ms | Fort |
| `CombatActionResolved` — action ratée (mini-jeu raté) | Popup "Raté" en grisé, pas d'animation d'impact sur la cible | SFX d'échec discret, pas humiliant | Vibration courte, pattern différent | < 100ms | Standard |
| Unité (joueur) mise KO | Grisement de l'unité sur la grille, icône distincte | SFX plus marqué, dramatique mais bref | Vibration longue distincte | < 100ms | Critique négatif |
| Fin de combat — victoire | Écran récapitulatif animé, loot qui apparaît un par un | Musique de victoire courte | Vibration de fin (double-pulse) | Non contraint (post-combat, pas de gameplay actif) | Fort |
| Tap sur bouton désactivé (matériaux manquants, hors PA/PM) | Léger shake du bouton + tooltip explicatif | SFX "refus" discret | Micro-vibration | < 100ms | Micro (prévention d'erreur, pas punition — Nielsen #5) |
| Navigation standard (changement d'écran, ouverture panneau) | Transition douce (150-250ms, cf. référentiel animations) | SFX de clic UI discret | Aucune ou micro | < 250ms | Micro |
| Marché/Boutique (stub) — tap sur "Bientôt disponible" | Aucune animation d'erreur — état clairement non interactif dès le premier regard | Aucun | Aucune | — | — (pas un échec, juste une fonctionnalité non activée) |

---

## 3. Notes d'implémentation pour le Gameplay Programmer (#4)

- Le flux **micro** (input sample continu) doit être géré **localement dans le composant de présentation du
  mini-jeu**, en lisant l'état courant de `IMiniGame` à chaque frame — **pas** via un abonnement `GameEventBus`,
  qui introduirait une latence et n'est pas fait pour un flux aussi fréquent (voir UI-SCREENS.md §3.4).
- Tous les feedbacks **standard/fort/critique négatif** sont déclenchés en réaction aux événements du
  `GameEventBus` (`CraftCompleted`, `ItemDestroyed`, `CombatActionResolved`, `ProductionTicked`) — jamais en
  dupliquant la logique de résolution côté présentation.
- Le pattern haptique du "Critique négatif" doit être implémenté comme une constante nommée dédiée (ex.
  `HapticPattern.DestructiveFailure`), réutilisée partout où une perte permanente survient (item détruit,
  unité KO) — pour garantir la cohérence perçue (Jakob) sans dupliquer les valeurs magiques dans le code.
- Toggle global "Haptique on/off" et "Réduire les animations" dans Paramètres (accessibilité, cf. UI-SCREENS.md
  §4) — le feedback visuel/audio reste toujours actif même si le haptique est coupé, jamais l'inverse.
