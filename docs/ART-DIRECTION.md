# Direction Artistique — Jeu Vidéo Personnel (craft incrémental)

> Agent : World Builder (#5) · Pipeline Game Dev · Phase 2 — Design
> Contrainte impérative : les mini-jeux de précision (SYSTEMS.md §5) doivent rester lisibles visuellement — le style ne doit jamais nuire à la clarté des indicateurs de mini-jeu.

---

## 1. Style retenu : stylisé low-poly peint ("painted low-poly")

**Description** : géométrie simple et lisible (faible nombre de polygones, silhouettes claires), texturée avec des dégradés peints/bakés plutôt que du PBR photoréaliste. Éclairage doux, pas de matériaux complexes (pas de reflets spéculaires agressifs, pas de post-processing lourd).

**Pourquoi ce style et pas un autre :**
- **Faisabilité solo débutant** — c'est le style le plus couvert par des packs d'assets prêts à l'emploi (Synty Studios POLYGON, Kenney.nl — voir §4), donc un développeur solo sans compétences artistiques poussées peut assembler une scène cohérente sans tout sculpter à la main.
- **Performance mobile** — faible charge de rendu (peu de polygones, pas de shaders coûteux), cohérent avec la contrainte de latence <16-33ms rappelée par le Technical Director pour le système de mini-jeux.
- **Cohérence avec les références du projet** — proche de l'esprit cosy/artisanal d'*A Township's Tale* et de la lisibilité diagrammatique d'*Alchemy Factory*, sans copier l'un ou l'autre.
- **Lisibilité gameplay (contrainte impérative)** — un style low-poly à silhouettes simples et couleurs plates/dégradées laisse un fond visuellement "calme" sur lequel un indicateur de mini-jeu (tracé, barre de timing, zone de précision) ressort naturellement, sans bruit visuel de texture concurrent. Un style photoréaliste ou très détaillé aurait été rejeté précisément pour cette raison : plus de détail de fond = plus de compétition visuelle avec l'indicateur que le joueur doit suivre au pixel près.

**Ce que ce style n'est pas** : ni cartoon flat-shaded façon mobile générique (manque de caractère, ne sert pas l'identité "artisanat" du jeu), ni réaliste/PBR poussé (coût de production hors de portée solo + risque de nuire à la lisibilité des mini-jeux).

---

## 2. Palette par zone

### Principe général — réserve chromatique pour les mini-jeux

**Règle non négociable** : un jeu de couleurs fixe est réservé exclusivement au feedback de mini-jeu et de combat, et **n'est jamais utilisé comme couleur dominante d'environnement**, dans aucune zone :

| Usage réservé | Couleur | Jamais utilisée comme couleur d'ambiance de zone |
|---|---|---|
| Cible / zone de précision | Or/blanc chaud (#F2D98A / #FFFFFF) | — |
| Réussite | Vert franc (#5FBF6E) | — |
| Échec / danger | Rouge franc (#D9534F) | — |
| Info neutre / progression | Bleu clair (#7FB8D9) | — |

Chaque mini-jeu s'affiche en outre sur un **scrim** (fond semi-transparent neutre, gris chaud désaturé à ~70-80% d'opacité) qui s'interpose entre l'environnement 3D et l'UI de mini-jeu — garantissant un contraste ≥4.5:1 quelle que soit la zone en arrière-plan (norme WCAG AA appliquée ici par bonne pratique, même hors contexte formulaire). C'est la même logique que la séparation GameCore/Presentation de l'architecture : le rendu du mini-jeu ne dépend jamais du décor de la zone où il est déclenché.

### Palettes d'ambiance

| Zone | Palette dominante | Intention |
|---|---|---|
| Hub — L'Atelier (général) | Bois chaud, ambre, terracotta (#B8895F, #D9A066, #7A5230) | Chaleur, sécurité, artisanat — Pillar 1 rendu visible : c'est un lieu de soin, pas de hasard |
| Hub — Station Craft Manuel | Variante la plus chaude/éclairée du Hub, point focal lumineux | Attire l'œil en premier (premier système touché par le joueur) |
| Hub — Station Raffinage/Usinage | Gris acier, bleu-gris, cuivre (#7D8A93, #5C6670, #B8763E accents) | Registre plus mécanique/logique, distinct visuellement du craft manuel pour signaler un changement de mode cognitif (dextérité → logique) |
| Hub — Cour Automatisation/Farm | Vert sourd, terre, bois clair (#7A9B6E, #A68A5B) | Organique-mécanique, croissance passive, contraste doux avec le reste du Hub |
| Expédition 1 — La Lisière | Vert tendre, doré, ciel pâle (#8FBF7A, #E8D9A0, #CFE8F0) | Accueillant, faible menace perçue — cohérent avec tension faible (ZONE-DESIGN.md §2) |
| Expédition 2 — La Carrière | Gris pierre désaturé, ambre de torche (#8C8478, #C98A4B) | Enclosé, rude, tension moyenne — clair-obscur marqué mais sans couleurs agressives |
| Expédition 3 — Les Ruines | Violet profond, bleu nuit, accents cristal (#4A3B6B, #2E2A4A, #7FE0D6 accents) | Mystère, tension élevée — la seule zone à utiliser des accents saturés froids, réservés aux éléments décoratifs (cristaux), jamais au sol/UI ambiante pour ne pas cannibaliser la lisibilité du combat |

**Vigilance explicite** : aucune palette de zone ne s'approche des couleurs réservées au feedback (or, vert franc, rouge franc) en usage dominant — vérifié zone par zone ci-dessus. La Lisière est la plus à risque (vert dominant) : le vert d'ambiance y est délibérément désaturé/tendre (#8FBF7A) et non le vert franc de réussite (#5FBF6E), pour rester distinguable au premier coup d'œil.

---

## 3. Lisibilité gameplay — règles transversales

1. **Contraste indicateur/fond garanti par un scrim**, jamais par la seule confiance en la palette de zone (voir §2).
2. **Silhouettes d'ennemis et de zones de danger toujours en clair-obscur net** (pas de camouflage décoratif) — un ennemi ou une case dangereuse ne doit jamais se fondre dans le décor, même dans Les Ruines où l'ambiance est volontairement sombre (utiliser un léger rim-light ou contour pour détacher les silhouettes jouables du fond).
3. **Grille de combat toujours identifiable** indépendamment du biome — le style de rendu des cases de grille (bordures, surbrillance de case active/accessible) reste visuellement constant d'un biome à l'autre ; seule la texture "sous" la grille change. Ça sert la cohérence (Nielsen #4) et évite de réapprendre à lire la grille à chaque nouveau biome.
4. **Pas d'effet de post-processing qui dégraderait la lisibilité d'un tracé/timing** (pas de motion blur sur l'UI de mini-jeu, bloom limité, jamais appliqué sur la couche UI de mini-jeu elle-même).

---

## 4. Assets pipeline — priorisation pour développeur solo débutant

### Priorité 1 — nécessaire pour la vertical slice (VERTICAL-SLICE.md)
| Asset | Source recommandée | Placeholder acceptable |
|---|---|---|
| Blockout de la station Craft Manuel + 1 établi | Modélisation primitive maison (cubes/cylindres) ou pack Kenney.nl (gratuit, low-poly) | Oui — primitives Unity texturées en couleur plate suffisent pour tester le binding data → UI |
| UI de mini-jeu (barre de timing, tracé de précision) | Sprites 2D maison (formes simples, vectoriel) | Oui — priorité absolue de test, le style visuel final peut arriver après validation du fun (risque #1 du GDD) |
| Grille de combat (1 arène, biome La Lisière) | Blockout primitive + texture de sol peinte simple | Oui |
| Personnage joueur + 1 ennemi basique | Pack low-poly asset-store (ex. Synty POLYGON — gamme cohérente, bon rapport qualité/prix pour un style unifié dès le départ) | Oui, en attendant : capsule + texture placeholder, tant que `ItemDefinitionSO`/`CombatantDefinitionSO` référencent déjà le bon champ (le swap d'asset ne touche aucun code, cf. ARCHITECTURE.md §4) |

### Priorité 2 — juste après la vertical slice
- Props et texture de la Station Raffinage/Usinage et de la Cour Farm.
- Habillage complet de La Lisière (props hors-grille décoratifs, cf. ZONE-DESIGN.md §0).
- 2-3 variantes d'ennemis pour La Carrière (recolors/reskins d'un même modèle de base avant de sculpter de nouveaux ennemis — moins coûteux, cohérent avec la recommandation "une seule IA, plusieurs profils de stats").

### Priorité 3 — roadmap proche, pas bloquant pour le MVP
- Biome Les Ruines (habillage complet, cristaux, éclairage dédié).
- Modèles de boss dédiés (vs. variantes agrandies d'ennemis existants en attendant).
- VFX de mini-jeu avancés (particules de réussite/échec au-delà du feedback sonore et de couleur de base).

**Recommandation pipeline générale** : tous les assets visuels sont référencés via les `ScriptableObject` (ARCHITECTURE.md §4) et chargés via Addressables — un placeholder peut donc être remplacé par l'asset final **sans toucher au code**, ce qui permet de développer le gameplay avec des primitives grises et de faire la passe artistique en parallèle ou après, sans bloquer l'un sur l'autre. C'est directement aligné avec le principe data-driven du Technical Director.
