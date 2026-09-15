# Sound Design — Jeu Vidéo Personnel (craft incrémental)

> Agent : World Builder (#5) · Pipeline Game Dev · Phase 2 — Design
> Contrainte impérative : le feedback sonore des mini-jeux (réussite/échec) doit être **audible distinctement** — c'est le canal de feedback le plus rapide pour un input tactile mobile où l'œil est parfois occupé par le geste lui-même.

---

## 1. Paysage sonore par zone

| Zone | Ambiance | Intention |
|---|---|---|
| Hub — L'Atelier (général) | Boucle chaude basse intensité : martèlement lointain, crépitement de forge, oiseaux discrets par une fenêtre | Zone de repos — doit inviter à rester sans fatiguer l'oreille, cohérent avec le rôle de "session courte casual" du Hub |
| Hub — Station Craft Manuel | Légère variation locale : frottement d'outils, bois qu'on travaille, en sourdine sous l'ambiance générale | Renforce le focus sur la station active sans rupture sonore brutale |
| Hub — Station Raffinage/Usinage | Bourdonnement mécanique doux, cliquetis d'engrenages, vapeur | Signale le changement de registre cognitif (dextérité → logique) aussi à l'oreille, pas seulement visuellement (ART-DIRECTION.md §2) |
| Hub — Cour Automatisation/Farm | Ambiance organique-mécanique : vent léger, grincement régulier et lent d'une roue/mécanisme | Sert de métronome passif discret — un rappel sonore doux que la production tourne, sans notification intrusive |
| Expédition 1 — La Lisière | Forêt claire : oiseaux, vent dans les feuillages, ruisseau lointain | Renforce la tension faible — un décor sonore accueillant |
| Expédition 2 — La Carrière | Écho de pioche lointaine, gouttes d'eau, réverbération de caverne | Sensation d'enclosement, tension moyenne |
| Expédition 3 — Les Ruines | Drone bas et continu, résonances cristallines éparses, silence relatif ponctué | Mystère et tension élevée — le silence partiel est utilisé comme outil de tension (moins d'éléments sonores = plus d'attention sur chaque son) |

**Règle transversale de mixage** : chaque ambiance de zone **baisse automatiquement de -6 à -10 dB dès qu'un mini-jeu démarre** (craft, raffinage ou combat), quelle que soit la zone — le feedback de mini-jeu ne doit jamais être en compétition avec l'ambiance pour l'attention auditive du joueur. L'ambiance remonte progressivement (crossfade ~0.5-1s) une fois le mini-jeu résolu.

---

## 2. SFX gameplay — feedback des mini-jeux (priorité absolue)

### Principe directeur

Le score d'un mini-jeu se mappe directement sur le résultat (CORE-LOOP.md) — le joueur doit donc pouvoir **entendre** s'il a réussi ou raté, y compris s'il regarde son geste plutôt que l'écran, et y compris sur haut-parleur de téléphone en environnement bruyant. Les sons de réussite et d'échec doivent différer par **contour de hauteur ET timbre**, pas seulement par volume — une distinction purement basée sur l'intensité serait trop fragile en conditions mobiles réelles (bruit ambiant, haut-parleur de faible qualité).

### Vocabulaire sonore commun (partagé par tous les mini-jeux)

| Résultat | Caractéristique sonore | Exemple de direction |
|---|---|---|
| Réussite (score élevé) | Court, montant, timbre clair/brillant | Carillon/cloche ascendant, ~300-500ms |
| Réussite partielle (score moyen) | Court, neutre, timbre doux | Variante atténuée du son de réussite, moins de brillance harmonique |
| Échec (score bas) | Court, descendant, timbre mat/sourd | Thud/buzz grave descendant, ~300-400ms — jamais strident (éviter la frustration auditive répétée) |
| Échec destructif (item détruit, recettes avancées) | Distinct des trois précédents — élément de casse/craquement ajouté | Renforce la gravité du résultat sans devenir punitif à l'oreille (pas de son agressif/long) |

Cette cohérence sonore transversale (même vocabulaire pour craft manuel, raffinage, combat) respecte Nielsen #4 (cohérence & standards) : une fois le joueur a appris "ce son = réussite" sur le craft, il le reconnaît immédiatement en combat, sans réapprentissage.

### Sons spécifiques par type de mini-jeu (couche "attempt", pendant l'interaction)

| Mini-jeu | Son d'interaction continue | Distinct du feedback de résultat |
|---|---|---|
| `TimingBarMiniGame` (craft manuel de base) | Tick régulier ou pulsation qui accompagne le curseur | Oui — le tick ne préjuge pas du résultat, seul le son final tranche |
| `TracePrecisionMiniGame` (craft manuel précis) | Léger crissement/frottement proportionnel à la précision du tracé en temps réel | Sert de feedback continu additionnel (le joueur "entend" s'il dévie avant même la fin du geste) |
| `LogicRoutingMiniGame` (raffinage) | Clic mécanique à chaque placement/connexion validée | Renforce la sensation de logique/mécanique de la station raffinage |
| `CombatActionMiniGame` (combat) | Son d'impact/tension bref, cohérent avec l'action choisie (attaque/défense) | Enchaîne naturellement vers le vocabulaire de résultat commun ci-dessus |

---

## 3. SFX combat additionnels

- **Hit / miss / critique** : distincts entre eux, cohérents avec le vocabulaire réussite/échec ci-dessus (un critique est une variante "renforcée" du son de réussite, pas un son totalement nouveau — évite la surcharge de vocabulaire sonore à apprendre).
- **Télégraphe ennemi** : un signal sonore discret avant qu'un ennemi résolve son action (utile pour l'anticipation tactique et l'accessibilité — un joueur qui ne regarde pas l'écran en continu peut réagir au son).
- **Victoire / défaite** : stinger musical court en fin de nœud de combat, différent entre victoire (montant, résolutif) et défaite (descendant, mais jamais humiliant — cohérent avec le ton "casual-friendly" du jeu, pas de sanction sonore excessive qui découragerait de retenter).

---

## 4. Musique adaptative

### Principe : layering vertical (empilement de stems), pas de resequencing horizontal

Choix justifié par le scope solo : le layering vertical (ajout/retrait de pistes superposées sur une même boucle) est nettement plus simple à implémenter qu'un système de transition horizontale entre morceaux complets (pas besoin de moteur audio tiers avancé — un `AudioMixer` Unity avec plusieurs `AudioSource` synchronisées et des snapshots suffit pour le MVP, cohérent avec ARCHITECTURE.md §8 qui exclut déjà les dépendances tierces non justifiées).

| Zone / état | Couches actives |
|---|---|
| Hub | Couche unique, calme, basse intensité — pas de montée en tension prévue ici (zone sûre) |
| Expédition — hors combat (déplacement entre nœuds, nœud Collecte) | Couche d'exploration (mélodie légère + pad) |
| Expédition — en combat (nœud Standard/Elite) | + couche rythmique (percussion/pulsation) qui se superpose à la couche d'exploration |
| Expédition — combat en tension (PV bas, dernier ennemi, nœud Boss) | + couche de tension supplémentaire (cordes/drone, intensité de mix augmentée) |
| Fin de nœud (victoire) | Coupure brève des couches de combat, stinger de victoire, retour à la couche d'exploration |

Chaque biome (La Lisière / La Carrière / Les Ruines) a son propre jeu de stems dans cette même structure à 3 couches (exploration / combat / tension) — le **système** de layering est unique et transversal (comme le framework de mini-jeu), seul le contenu musical change par biome. Ça limite la charge d'implémentation à une seule logique de mixage réutilisée partout.

### Ce qui est explicitement hors scope pour le MVP
- Pas de musique générée proceduralement.
- Pas de système de transition orchestrale complexe (type FMOD avec des dizaines de segments) — le layering vertical simple suffit et reste dans les moyens d'un solo dev.
- Pas de thème musical dédié au marché P2P/à la boutique — hors scope (cf. WORLD-MAP.md §4).

---

## 5. Accessibilité sonore

- Le feedback de mini-jeu ne doit **jamais dépendre uniquement du son** — toujours doublé d'un feedback visuel (couleur, animation) pour les joueurs qui coupent le son (usage mobile fréquent en transport public). Le son est un renfort de lisibilité, pas un canal exclusif.
- Prévoir un volume distinct pour Ambiance / SFX / Musique dans les options (mixer séparé), pour que le joueur puisse isoler le feedback de mini-jeu (SFX) s'il le souhaite — cohérent avec Nielsen #7 (flexibilité).
