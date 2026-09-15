# Tech Stack — Jeu Vidéo Personnel (craft incrémental)

> Source : docs/FOUNDATION.md, docs/GDD.md, docs/SYSTEMS.md, docs/SCOPE.md
> Agent : Technical Director (#2) · Pipeline Game Dev · Phase 1 — Fondation
> Date : 2026-09-15

---

## 1. Moteur — Unity (C#)

**Confirmé** (déjà tranché dans FOUNDATION.md §5, non remis en cause ici).

### Justification (brève, le choix n'est pas à comparer)
- Un seul codebase pour iOS + Android — cible mobile-only du FOUNDATION.md.
- C# aligné avec le profil de mentalyas (Stack IT : C#, C++) — pas de coût d'apprentissage de langage.
- **ScriptableObject (SO — asset Unity servant de conteneur de données pur, sans logique de scène)** est le mécanisme natif idéal pour la couche data-driven exigée par mentalyas (recettes, mini-jeux, ennemis, items) — pas besoin d'un moteur de config externe.
- Outillage mobile mature : Profiler, Frame Debugger, Addressables (système de chargement d'assets asynchrone et adressable, remplaçant le dossier `Resources`), Input System (package officiel de gestion des entrées, dont le tactile multi-touch).
- Écosystème solo-dev-friendly (Asset Store, documentation, large communauté) — pertinent pour un développeur solo qui recrutera plus tard.

## 2. Rendu

**URP (Universal Render Pipeline — pipeline de rendu léger et configurable de Unity, optimisé mobile)**. Justification : meilleur ratio qualité/performance/batterie sur mobile que le Built-in Render Pipeline ; supporté nativement par tous les templates Unity récents ; pas de besoin des fonctionnalités HDRP (High Definition RP, orienté AAA console/PC).

## 3. Input

**Input System** (package officiel, remplace l'ancien `Input` statique). Justification : gestion native multi-touch et gestures nécessaire aux mini-jeux de précision (tracé, timing-bar) — le socle technique le plus critique du jeu (SYSTEMS.md §5) dépend directement de la qualité de la capture d'input tactile.

## 4. Backend / économie — décision reportée, résolue par l'architecture (pas figée ici)

Le Game Designer alertait que ce choix (ASP.NET Core custom vs service managé Unity Gaming Services/PlayFab) devait être tranché **avant** l'architecture (JOURNAL.md, alerte GD). Cadrage de mentalyas pour cette phase : l'économie P2P reste un **stub**, à peaufiner plus tard.

**Résolution retenue** : ce choix n'a pas besoin d'être figé maintenant. L'architecture (voir ARCHITECTURE.md §5 — Point d'extension économie) isole toute la logique économique derrière une interface `IEconomyService`. Le MVP tourne avec une implémentation stub locale (`StubEconomyService`) ; le choix custom vs managé ne concerne que l'implémentation future de cette interface et **n'impacte aucun autre système**. Ça ne repousse pas la décision par confort — ça la rend non bloquante, ce qui est exactement ce que demandait mentalyas.

Pour le **backend serveur-autoritaire minimal** exigé dès le MVP par SCOPE.md (validation des résultats de mini-jeux/craft/combat, même sans marché actif), même logique : une interface `IAuthoritativeValidationService` avec une implémentation locale pour la vertical slice (validation faite en local, honnête pour un solo dev qui teste seul), swappable plus tard vers un vrai serveur (custom ou managé) sans réécrire les appelants.

| Option future | Avantage | Coût |
|---|---|---|
| ASP.NET Core custom | Contrôle total, réutilise C# côté client/serveur (DTOs partagés) | Chantier lourd : anti-fraude, transactions atomiques, hébergement à gérer |
| Unity Gaming Services (Economy, Cloud Save, Cloud Code) | Primitives de monnaie/inventaire/anti-triche prêtes à l'emploi, intégration native Unity | Vendor lock-in, moins de contrôle sur les règles métier fines |
| PlayFab (Microsoft/Azure) | Écosystème mature, proche d'Azure si besoin d'extension | Vendor lock-in, coût à l'échelle |

Aucune de ces options n'est engagée par l'architecture du MVP — c'est le point important.

## 5. Sérialisation / Save

**Newtonsoft.Json (Json.NET pour Unity, package officiel `com.unity.nuget.newtonsoft-json`)** plutôt que `JsonUtility` natif. Justification : `JsonUtility` ne sérialise pas correctement le polymorphisme (interfaces, héritage) ni les dictionnaires — or l'architecture orientée interfaces (IMiniGame, ICombatant, etc., voir ARCHITECTURE.md) en dépend pour les données de sauvegarde runtime (pas les ScriptableObjects eux-mêmes, qui sont des assets, pas des données de save).

## 6. Résumé stack

| Couche | Techno | Statut |
|---|---|---|
| Client mobile | Unity 6 LTS (ou dernière LTS stable), C# | Confirmé |
| Rendu | URP | Confirmé |
| Input | Input System (package officiel) | Confirmé |
| Contenu data-driven | ScriptableObjects + Addressables | Confirmé |
| Sérialisation save | Newtonsoft.Json for Unity | Confirmé |
| Backend économie/validation | Interface `IEconomyService` / `IAuthoritativeValidationService`, impl. stub locale pour le MVP | **Stub — implémentation réelle différée** |
| CI/CD | GitHub Actions + Unity Cloud Build (voir PROJECT-STRUCTURE.md §Pipeline) | Différé jusqu'à recrutement de collaborateurs — build manuel suffisant pour la vertical slice |
