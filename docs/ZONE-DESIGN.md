# Zone Design — Jeu Vidéo Personnel (craft incrémental)

> Agent : World Builder (#5) · Pipeline Game Dev · Phase 2 — Design
> Voir docs/WORLD-MAP.md pour la carte macro et les connexions.
> Détail par zone : thème, danger, ressources, encounters.

---

## 0. Réponse explicite — obstacles de terrain / pathfinding A*

> **Point d'attention explicite laissé par le Technical Director (#2)** dans ARCHITECTURE.md §0 et §1, et dans l'alerte de fin de JOURNAL.md : le combat MVP n'a pas de pathfinding A*, la grille est simple sans obstacles de terrain, et cette décision devait être tranchée par le World Builder avant que le Gameplay Programmer (#4) ne fige le système de grille.

**Décision : aucun obstacle de terrain bloquant le déplacement n'est prévu pour le MVP. La grille simple (déplacement Manhattan/Chebyshev, sans A*) suffit.**

Justification :
- Les 3 biomes d'expédition (§2-4 ci-dessous) apportent leur variété par le **thème visuel, l'ambiance sonore et la composition d'ennemis**, pas par la complexité géométrique de la grille de combat — cohérent avec Pillar 4 du GDD (chaque système nourrit le suivant, pas de feature qui n'apporte rien) et avec le risque déjà identifié par le Technical Director (combat = système CORE le plus lourd, complexité 5/5).
- Ajouter des obstacles bloquants maintenant reviendrait à réintroduire une charge technique (A*, calcul de portée effective, IA qui doit contourner) sur le système déjà le plus lourd du MVP, sans bénéfice de design proportionné à ce stade (le risque #1 du projet reste "les mini-jeux restent-ils fun ?", pas "le combat est-il assez tactique en positionnement").
- Des props décoratifs (rochers, caisses, piliers) peuvent exister visuellement dans les nœuds de combat **sans occuper de case de grille** (posés en bordure/hors-grille, ou strictement cosmétiques sur des cases non jouables en marge) — ils enrichissent l'identité visuelle du biome sans changer la logique de mouvement.

**Piste explicitement reportée en v2+ (hors scope de cette phase)** : si un playtest futur montre que le combat manque de profondeur tactique une fois le mini-jeu validé, des obstacles bloquants pourraient être introduits par biome (ex. les Ruines) — cela nécessiterait de revenir sur ARCHITECTURE.md §0 pour réévaluer A*. Ce n'est **pas** une demande de ce document, seulement une option notée pour ne pas la perdre.

---

## 1. Hub — L'Atelier

**Thème** : l'atelier personnel du joueur — bois chaud, établi, forge, cour attenante. Zone sûre, aucun danger, aucun combat. C'est le "chez-soi" du joueur, le point de dép/retour de chaque session.

**Danger** : aucun.

**Ressources** : aucune production directe — c'est le lieu où les ressources collectées ailleurs sont transformées (craft, raffinage) ou observées (farm en tick).

**Stations présentes** (cf. WORLD-MAP.md §1) :
- **Station Craft Manuel** — établi central, visuellement le point focal de la scène (premier système touché par le joueur, cf. SYSTEMS.md §1).
- **Station Raffinage/Usinage** — poste distinct, esthétique plus mécanique (voir ART-DIRECTION.md).
- **Cour Automatisation/Farm** — visible en périphérie, avec un feedback ambiant passif (ex. une roue qui tourne doucement, un tas de ressources qui grossit visuellement) pour que la progression idle soit lisible sans ouvrir un menu — feedback immédiat sans action requise (Nielsen #1).
- **Tableau d'Expéditions** — point d'accès au combat, positionné à l'écart des stations de craft pour marquer clairement la transition "zone sûre → zone de danger" (Gestalt — figure/fond, séparation claire des fonctions).

**Encounters** : aucun. C'est une zone de gestion pure.

**Note de scope** : le Hub est une **petite scène explorable** (marche libre courte, pas un monde ouvert) — traversable en moins de 10 secondes d'un bout à l'autre. Alternative de secours si le temps de dev manque : réduire le Hub à un écran de sélection de stations sans déplacement de personnage (menu illustré) sans rien perdre du CORE — **à garder en tête comme fallback de scope si l'implémentation d'un hub explorable prend trop de temps par rapport au reste du MVP** (voir Selfdoubt en fin de document).

---

## 2. Expédition 1 — La Lisière (tutoriel, tension faible)

**Thème** : orée de forêt, clairière ensoleillée, premiers pas hors de l'atelier. Visuellement accueillant, pas menaçant — le joueur doit sentir qu'il peut se permettre d'échouer ici sans grande conséquence.

**Danger** : faible. Ennemis lents, peu nombreux, dégâts limités. C'est la zone qui enseigne la boucle de combat (GDD.md — onboarding, 0-30 min).

**Ressources produites** : matériaux de base compatibles avec le craft manuel (bois, fibres, pierre brute) — pas de matériau avancé ici (ceux-là viennent du raffinage, pas du combat précoce).

**Encounters** (cf. WORLD-MAP.md §2 pour la séquence de nœuds) :
- 3-4 nœuds Standard (1-2 ennemis basiques, `AggressiveAI` unique).
- 1 nœud Elite optionnel (meilleur loot, ennemi renforcé unique).
- 1 nœud Boss simple en fin de séquence — gate de déverrouillage vers l'Expédition 2.
- **Règle de spawn** : composition fixe (pas de randomisation) pour les nœuds Standard — le joueur tutoriel doit vivre une expérience prévisible et lisible, pas un aléa qui brouillerait l'apprentissage du mini-jeu de combat.

---

## 3. Expédition 2 — La Carrière (mid-game, tension moyenne)

**Thème** : carrière de pierre/mine à ciel ouvert semi-fermée — poussière, échafaudages, éclairage de torches. Bascule vers un environnement plus rude, cohérent avec la montée en exigence du Mid game (GDD.md §4).

**Danger** : moyen. Ennemis plus résistants, premiers ennemis à comportement légèrement différencié (variantes de stats sur le même `AggressiveAI`, pas une nouvelle IA — cohérent avec la recommandation du Technical Director de ne pas multiplier les IA pour le MVP).

**Ressources produites** : minerais/matériaux liés au raffinage avancé — cette expédition est le débouché naturel pour tester l'équipement produit par la station de raffinage (Pillar 4 — chaque système nourrit le suivant).

**Encounters** :
- 4-5 nœuds Standard, avec **pool semi-randomisé** de compositions d'ennemis (2-3 variantes possibles par nœud) — introduit de la variété sans construire un vrai système procédural, gardant le scope raisonnable pour un solo dev.
- 1-2 nœuds Elite à position fixe dans la séquence (pas aléatoires) pour garder un contrôle du rythme.
- 1 nœud Collecte (alternative sans combat, cf. WORLD-MAP.md §2).
- Nœud Boss fixe en fin de séquence — gate vers l'Expédition 3.

---

## 4. Expédition 3 — Les Ruines (late/endgame, tension élevée)

**Thème** : ruines englouties/enfouies, architecture ancienne, lumière rare et colorée (bio-luminescence, cristaux) contrastant avec l'obscurité ambiante — sentiment de mystère et de danger accru, récompense visuelle à la hauteur de l'investissement du joueur engagé (try-hard).

**Danger** : élevé. Ennemis les plus résistants du MVP+roadmap proche, boss à plusieurs phases (toujours sur le même framework `ICombatant`/`IEnemyAI`, sans complexifier l'architecture — une phase de boss = un changement de profil de stats/comportement, pas une nouvelle classe d'IA).

**Ressources produites** : matériaux rares, loot destiné en priorité à l'équipement haut de gamme et, plus tard, au marché P2P (non actif dans ce scope, mais c'est le loot qui alimentera ce système quand il sera repris — cohérent avec ARCHITECTURE.md §5).

**Encounters** :
- 5-6 nœuds avec pool semi-randomisé plus large que La Carrière.
- 2 nœuds Elite.
- 1 nœud Collecte.
- Nœud Boss multi-phases en fin de séquence — pas de déverrouillage d'expédition suivante prévu dans ce document (une 4e expédition serait une décision de contenu future, hors scope de cette phase — voir §5).

---

## 5. Pourquoi 3 expéditions et pas plus (scope)

Le brief demande explicitement de ne pas proposer un monde ouvert massif si le scope réel est un vertical slice. La Vertical-Slice.md ne couvre techniquement qu'**un seul cycle** craft manuel + combat simple sur **une grille unique**, sans même de deuxième biome. Trois biomes sont proposés ici comme **structure de roadmap proche** (pas comme livrable immédiat) pour :
1. Donner un cadre de progression cohérent avec la courbe GDD.md §4 (Onboarding → Early → Mid → Late/Endgame) sans complexité technique nouvelle (même moteur de grille, mêmes interfaces `ICombatant`/`IEnemyAI`/`IMiniGame` réutilisés à chaque biome — zéro nouveau système).
2. Éviter de sur-promettre : un 4e biome, du contenu procédural réel, ou une vraie carte explorable ne sont **pas** recommandés à ce stade — ce serait exactement le monde ouvert massif à éviter.

**Le contenu réel de chaque expédition (nombre exact de nœuds, ennemis, loot tables) est un travail de contenu post-vertical-slice**, pas une exigence de cette phase — ce document fixe la structure et l'intention, pas le détail final calibré (cohérent avec la méthode du Game Designer : la balance numérique est un travail de QA/playtest itératif, pas de design initial).

---

## 6. Selfdoubt — navigation, cohérence, scope

| # | Affirmation | Niveau | Action recommandée |
|---|---|---|---|
| 1 | Un Hub explorable à pied (plutôt qu'un menu de sélection de stations) apporte assez de valeur d'identité/immersion pour justifier son coût de dev en solo | ⚠️ Probable | Garder le fallback "Hub-menu sans déplacement" explicitement noté en §1 — trancher après avoir mesuré le temps réel passé sur la vertical slice (cohérent avec la recommandation du Technical Director de mesurer plutôt qu'estimer a priori). |
| 2 | La structure "séquence de nœuds" par expédition (plutôt qu'un niveau continu traversé) restera lisible et motivante après plusieurs runs, sans lasser | ⚠️ Probable | À vérifier au même moment que le playtest des mini-jeux (risque #1 du GDD) — si le nœud-à-nœud paraît répétitif, envisager une variation de mise en scène (pas de mécanique) entre nœuds du même biome. |
| 3 | Trois biomes sont le bon nombre pour couvrir Early→Endgame sans sur-scope | ⚠️ Probable | Ce nombre n'est pas gravé — si le rythme réel de contenu solo est plus lent que prévu, réduire à 2 biomes (fusionner Carrière/Ruines) est une option de repli sans casser la structure macro. |
| 4 | L'absence d'obstacles de terrain (§0) ne privera pas le combat de profondeur tactique perçue par le joueur try-hard | ⚠️ Probable | Dépend directement du Selfdoubt #5 du GDD (double couche de skill stats+mini-jeu) — si le combat paraît "plat" en playtest malgré cette double couche, les obstacles bloquants deviennent la première piste d'enrichissement (v2+, cf. §0). |
| 5 | Les props décoratifs hors-grille suffisent à donner une identité visuelle distincte à chaque biome sans logique de gameplay associée | ✅ Certain | Confirmé par construction — c'est un choix purement présentation (Presentation.asmdef), zéro dépendance avec GameCore, donc zéro risque d'invalidation par un futur changement de règle de combat. |

**Hedge-to-Verify Ratio** : 4 affirmations sur 5 en ⚠️ Probable — cohérent avec le stade du pipeline (fin de Phase 2, avant tout playtest réel). Rien ici n'est bloquant pour l'implémentation du vertical slice ; tout est vérifiable une fois le prototype jouable.
