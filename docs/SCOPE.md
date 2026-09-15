# Scope — Jeu Video Personnel (craft incrémental)

> Source : docs/FOUNDATION.md · Agent : Game Designer (#1) · Phase 1 — Fondation
> Priorisation CORE / IMPORTANT / NICE-TO-HAVE de chaque système/fonctionnalité.

## Note de méthode

FOUNDATION.md liste 8 fonctionnalités comme "MVP" à plat, sans hiérarchie interne. En tant que Game Designer, mon rôle est de **re-prioriser en fonction du risque et de la réalité d'un développeur solo** — pas de recopier la liste telle quelle. Le critère de classement n'est pas "est-ce que FOUNDATION.md le mentionne comme MVP" mais : *"si ce système n'existe pas ou est cassé, le jeu prouve-t-il ou non son hypothèse de fun centrale (le mini-jeu remplace le jet de dés) ?"*

Conséquence : je classe le **marché P2P** et la **boutique F2P** en IMPORTANT plutôt qu'en CORE-jour-1, malgré leur présence dans la liste "MVP" de FOUNDATION.md — ils sont indispensables au modèle économique final, mais pas à la validation du concept de jeu lui-même, et ils portent la plus grosse charge technique/légale du projet (backend serveur-autoritaire anti-fraude, vérification réglementaire par pays). Retarder leur mise en production réelle (au-delà d'un stub technique) réduit le risque de ne jamais finir le MVP. **Ce point mérite validation explicite de mentalyas et du Technical Director avant de figer le séquençage de développement** — voir alerte en fin de GDD.

---

## CORE — indispensable pour un MVP jouable qui valide l'hypothèse centrale

| Système/fonctionnalité | Pourquoi CORE |
|---|---|
| Système de mini-jeux transversal (2–3 types minimum : dextérité craft manuel, logique raffinage simplifié, résolution combat) | C'est l'hypothèse de fun à valider en premier. Sans lui, rien d'autre dans le jeu n'a de sens différenciant. |
| Craft manuel | Premier système touché par le joueur, porte d'entrée de la boucle. |
| Automatisation / farm (version basique, offline progression + cap) | Porte la dimension idle/casual, condition pour servir le public "sessions courtes". |
| Combat tactique tour par tour (version simple : grille + 1 mini-jeu de résolution, IA basique) | Porte la dimension try-hard/profondeur ; sans combat, le loot et l'équipement n'ont pas de débouché. |
| Compte joueur + inventaire | Infrastructure minimale sans laquelle aucune progression ne peut être sauvegardée. |
| Backend serveur-autoritaire minimal (validation des actions de craft/combat, même sans marché) | Sécurité non-rattrapable a posteriori — à poser dès le MVP même si le marché P2P est différé. |

## IMPORTANT — complète l'expérience visée par FOUNDATION.md, mais peut suivre le MVP sans dénaturer le jeu

| Système/fonctionnalité | Pourquoi IMPORTANT et non CORE-jour-1 |
|---|---|
| Craft intermédiaire / raffinage | Ajoute la profondeur try-hard (Alchemy Factory) mais le jeu reste jouable et testable sans lui en version initiale — la boucle craft manuel → combat fonctionne seule. |
| Marché d'échange P2P + monnaie premium échangeable + taxe | Cœur du modèle économique final, mais chantier backend lourd (anti-fraude, transactions atomiques) et risque légal (loot-box) à traiter séparément, sans bloquer la validation du gameplay. |
| Boutique F2P (achat monnaie premium, pay-to-skip) | Dépend du marché P2P pour avoir un sens économique complet ; peut arriver juste après. |
| Difficulté adaptative des mini-jeux | Améliore fortement l'expérience casual/try-hard mais un set de difficultés fixes suffit pour un premier playtest. |

## NICE-TO-HAVE — v2+, repoussable sans casser le concept

| Système/fonctionnalité | Justification |
|---|---|
| Guildes/clans, coopération multijoueur | Explicitement non tranché dans FOUNDATION.md ("à explorer une fois le solo/socle posé"). |
| PvP en combat | Absent de FOUNDATION.md, ajout naturel une fois le combat PvE validé. |
| Extension du catalogue de mini-jeux au-delà du set de lancement | FOUNDATION.md le classe déjà en v2+. |
| Contenu additionnel porté par de futurs collaborateurs | Dépend du recrutement, hors du contrôle du scope solo actuel. |

## Explicitement hors scope (rappel FOUNDATION.md)

- Deadline/date de sortie imposée.
- Portage desktop/console — mobile only (iOS + Android) dans un premier temps.
