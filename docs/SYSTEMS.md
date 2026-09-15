# Systèmes de jeu — Jeu Video Personnel (craft incrémental)

> Source : docs/FOUNDATION.md · Agent : Game Designer (#1) · Phase 1 — Fondation
> Chaque système CORE identifié dans FOUNDATION.md est détaillé ici : but, mécaniques, interactions, risques.
> Voir docs/SCOPE.md pour la priorisation CORE / IMPORTANT / NICE-TO-HAVE de chacun.

---

## 1. Craft manuel

**But** — Donner une sensation physique de fabrication où l'input réel du joueur (précision, timing) détermine la qualité du résultat, pour créer un attachement à l'objet fabriqué plutôt qu'à un chiffre de progression. C'est l'entrée en matière du jeu : le premier système que le joueur touche.

**Mécaniques**
- Mini-jeu de dextérité/précision par recette (ex. suivre un tracé, doser une pression/un timing, aligner des zones).
- Le score du mini-jeu (continu, 0–100%) se mappe sur un palier de qualité de l'item produit (commun → rare → parfait).
- En dessous d'un seuil critique sur les recettes avancées : **échec destructif** — l'item en cours est perdu, les matériaux investis aussi (référence *A Township's Tale*).
- Difficulté adaptative selon le profil du joueur, pour ne pas punir un joueur casual sur une recette de base.

**Interactions**
- Consomme les ressources brutes produites par l'automatisation/farm.
- Produit les inputs du craft intermédiaire/raffinage.
- Produit l'équipement de base utilisé en combat.
- Produit des items vendables sur le marché P2P.

**Risques**
- La destruction d'objet peut générer de la frustration et du churn si elle touche des recettes trop fréquentes ou trop tôt dans la progression — à réserver aux recettes avancées, jamais au tutoriel.
- Précision tactile sur petit écran mobile : un mini-jeu pensé pour VR/PC (référence) ne se transpose pas automatiquement en confort tactile — nécessite prototypage physique tôt, pas seulement sur papier.
- Risque de lassitude si le mini-jeu est identique à chaque craft, quelle que soit la recette (voir système #5).

---

## 2. Craft intermédiaire / raffinage

**But** — Introduire une couche de planification et de logique (référence *Alchemy Factory*) : récompenser l'optimisation d'une chaîne de production plutôt que le seul geste manuel, pour donner de la profondeur aux joueurs try-hard.

**Mécaniques**
- Mini-jeu logique/puzzle de placement et de routage (modules de raffinage, contraintes de flux) qui détermine le rendement et l'efficacité d'une chaîne.
- Une fois la chaîne résolue, elle peut tourner en automatisation (lien direct vers le système #3).
- Produit des matériaux avancés à partir des sorties du craft manuel et des ressources farmées.

**Interactions**
- Consomme : sorties du craft manuel + ressources brutes de la farm.
- Alimente : l'automatisation (débloque/optimise les lignes de production), l'équipement combat haut niveau, les items de valeur pour le marché.

**Risques**
- Complexité cognitive élevée : un puzzle d'optimisation façon factory-builder peut rebuter le public casual visé par FOUNDATION.md — nécessite un mode simplifié ou un nombre de modules volontairement limité au lancement (3–5 max).
- Risque de sur-ingénierie côté développement solo : un vrai moteur d'optimisation de chaîne de production est un chantier technique conséquent, à ne pas sous-estimer (voir Selfdoubt du GDD).

---

## 3. Automatisation / farm en arrière-plan

**But** — Porter la dimension idle/incremental du jeu : garantir une progression continue même quand le joueur n'est pas actif, pour servir le public casual en sessions courtes et créer une raison de revenir.

**Mécaniques**
- Production différée calculée sur le delta de temps écoulé depuis la dernière connexion.
- Cap de stockage pour éviter l'accumulation infinie hors-ligne (empêche un joueur absent 2 semaines de revenir avec un avantage disproportionné).
- Amélioration de capacité/vitesse débloquée via le raffinage.

**Interactions**
- Source primaire de ressources brutes pour le craft manuel et le raffinage.
- Socle de la boucle de rappel "longue" (voir CORE-LOOP.md).
- Source de matières premières échangeables sur le marché.

**Risques**
- Mal calibrée, cette mécanique peut soit **trivialiser** le jeu (le joueur n'a plus besoin d'agir, la boucle active perd son intérêt) soit **frustrer** (cap trop bas = sentiment de temps perdu hors-ligne). Nécessite une balance itérative testée en conditions réelles, pas seulement calculée sur papier.
- Risque de déséquilibrer le rapport casual/try-hard si l'idle devient la stratégie dominante pour progresser économiquement (dévalorise le skill du mini-jeu, contredit le pilier #1).

---

## 4. Combat tactique tour par tour

**But** — Offrir la profondeur stratégique visée (référence *Wakfu* pour le positionnement sur grille, *Pokémon* pour la lisibilité de la structure d'affrontement), enrichie par la résolution d'action via mini-jeu plutôt qu'un pur calcul de stats.

**Mécaniques**
- Combat sur grille, machine à états (idle → sélection action → mini-jeu → résolution → tour suivant).
- Système de points d'action/mouvement (façon PA/PM) pour le positionnement tactique.
- Chaque action offensive/défensive est résolue par un mini-jeu de précision/timing qui module dégâts, effets ou chances de réussite d'un effet spécial.

**Interactions**
- Consomme l'équipement produit par le craft manuel et le raffinage — teste directement le build du joueur.
- Produit du loot et des matériaux rares réinjectés dans l'économie et le marché.

**Risques**
- **Système le plus lourd à développer en solo** : IA ennemie, pathfinding sur grille, équilibrage de stats *combiné* à l'équilibrage du mini-jeu — deux systèmes de skill superposés (stats de build + input joueur) qui doivent être calibrés ensemble, pas séparément.
- Risque de redondance ou de confusion si les deux couches de skill (stats de combat vs habileté au mini-jeu) se contredisent ou se neutralisent (ex. un joueur avec un mauvais build mais un excellent mini-jeu gagne systématiquement, ce qui viderait le système de progression de son sens) — à prototyper et jouer soi-même avant d'investir dans le contenu.

---

## 5. Système de mini-jeux transversal

**But** — Être le liant mécanique de tout le jeu : remplacer les jets de dés/barres de progression par un geste réel du joueur à chaque action clé, sans devenir fastidieux à la 50e répétition. C'est la pièce dont dépend la validité de tous les autres systèmes.

**Mécaniques**
- Catalogue de mini-jeux (dextérité type timing-bar, tracé de précision, rythme, logique/puzzle) mappés par contexte : craft manuel, raffinage, combat, éventuellement farm active.
- Difficulté adaptative selon le profil détecté du joueur (casual vs try-hard), sans casser l'équilibrage économique en aval.
- Rotation/variation des mini-jeux dans le temps pour limiter l'usure de répétition.

**Interactions**
- Système transversal branché sur pratiquement tous les autres — c'est le point de passage obligé de la core loop (voir CORE-LOOP.md).

**Risques**
- **Risque numéro un du projet.** Si les mini-jeux ne restent pas fun après des dizaines/centaines de répétitions, c'est tout l'édifice de design qui s'effondre : le craft, le raffinage et le combat dépendent tous de ce système pour leur boucle de feedback. "Intéressant sur le papier" (remplacer un jet de dés par un geste) n'est pas la même chose que "fun en pratique" après la 50e répétition — c'est précisément l'hypothèse la plus risquée du jeu (voir GDD.md → Selfdoubt).
- Nécessite un prototype jouable et testé **avant** d'investir du temps de développement dans les autres systèmes qui en dépendent.

---

## 6. Marché d'échange P2P (Player-to-Player)

**But** — Donner une utilité économique réelle à la monnaie premium et aux items rares, créer une économie pilotée par les joueurs (référence *Warframe*), et fournir une source de revenu structurelle au studio via la taxe de transaction.

**Mécaniques**
- Listing d'objets ou de monnaie par un vendeur, avec prix fixé par le joueur.
- Taxe de transaction prélevée automatiquement à chaque échange.
- Validation exclusivement côté serveur (serveur-autoritaire) — le client ne décide jamais du résultat d'une transaction.

**Interactions**
- Débouché naturel pour le loot rare produit par le craft, le raffinage et le combat.
- Consomme la monnaie premium (achetée en boutique ou gagnée en jeu).
- Génère le revenu récurrent du studio via la taxe.

**Risques**
- **Risque légal** : certains pays (dont la Belgique, cf. FOUNDATION.md §7) encadrent strictement les mécaniques de monnaie virtuelle échangeable proches du jeu d'argent — vérification réglementaire obligatoire par pays de lancement avant activation, potentiellement bloquant.
- **Risque de fraude/dupe** : un marché P2P avec vraie valeur économique exige un backend anti-triche pensé dès le début (transactions atomiques, idempotency keys) — non rattrapable a posteriori sans réécriture.
- Risque qu'un marché dominé par des joueurs très engagés (voire des bots) écrase les joueurs casuals en fixant les prix — nécessite garde-fous (limites de transaction, détection de patterns anormaux).

---

## 7. Boutique F2P

**But** — Fournir la source de revenu directe (achat de monnaie premium, pay-to-skip) sans dégrader l'équité compétitive ni contredire le pilier "l'habileté prime sur l'argent dépensé".

**Mécaniques**
- Achat de monnaie premium exclusivement via IAP (in-app purchase) officiel du store (Apple/Google) — jamais de système de paiement maison.
- Options de pay-to-skip : accélérer un temps de production/automatisation, jamais acheter directement de la puissance de combat ou un résultat de mini-jeu.

**Interactions**
- Alimente le marché P2P en monnaie premium en circulation.
- Lié structurellement à la taxe de transaction du marché.

**Risques**
- Ligne fine entre "pay-to-skip acceptable" et "pay-to-win perçu" par la communauté — nécessite une règle explicite et documentée (quels objets/avantages ne sont jamais achetables) avant tout développement de contenu boutique, pour éviter les dérives progressives une fois le jeu en production.
- Dépendance à la conformité des stores officiels (politiques IAP, taux de commission) — hors du contrôle direct du studio.
