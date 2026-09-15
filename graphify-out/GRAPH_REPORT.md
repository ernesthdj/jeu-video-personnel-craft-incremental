# Graph Report - .  (2026-09-15)

## Corpus Check
- Corpus is ~262 words - fits in a single context window. You may not need a graph.

## Summary
- 6 nodes · 8 edges · 2 communities
- Extraction: 75% EXTRACTED · 25% INFERRED · 0% AMBIGUOUS · INFERRED: 2 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Journal & Graphe Local|Journal & Graphe Local]]
- [[_COMMUNITY_Workflow BrainstormPipeline|Workflow Brainstorm/Pipeline]]

## God Nodes (most connected - your core abstractions)
1. `CLAUDE.md (jeu-video-personnel-craft-incremental)` - 5 edges
2. `/hub (Archiviste ProjectMaster)` - 3 edges
3. `JOURNAL.md (jeu-video-personnel-craft-incremental)` - 2 edges
4. `/brainstorm (cahier des charges)` - 2 edges
5. `/pipeline (agents)` - 2 edges
6. `Graphify projet (graphe de connaissances local)` - 2 edges

## Surprising Connections (you probably didn't know these)
- `JOURNAL.md (jeu-video-personnel-craft-incremental)` --references--> `CLAUDE.md (jeu-video-personnel-craft-incremental)`  [EXTRACTED]
  docs/JOURNAL.md → CLAUDE.md
- `JOURNAL.md (jeu-video-personnel-craft-incremental)` --references--> `/hub (Archiviste ProjectMaster)`  [EXTRACTED]
  docs/JOURNAL.md → CLAUDE.md

## Hyperedges (group relationships)
- **Scaffolding initial du projet via /hub new** — craftincremental_claude_md, craftincremental_journal_md, craftincremental_hub_skill [EXTRACTED 0.90]

## Communities (2 total, 0 thin omitted)

### Community 0 - "Journal & Graphe Local"
Cohesion: 0.67
Nodes (3): Graphify projet (graphe de connaissances local), /hub (Archiviste ProjectMaster), JOURNAL.md (jeu-video-personnel-craft-incremental)

### Community 1 - "Workflow Brainstorm/Pipeline"
Cohesion: 1.0
Nodes (3): /brainstorm (cahier des charges), CLAUDE.md (jeu-video-personnel-craft-incremental), /pipeline (agents)

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `CLAUDE.md (jeu-video-personnel-craft-incremental)` connect `Workflow Brainstorm/Pipeline` to `Journal & Graphe Local`?**
  _High betweenness centrality (0.650) - this node is a cross-community bridge._
- **Why does `/hub (Archiviste ProjectMaster)` connect `Journal & Graphe Local` to `Workflow Brainstorm/Pipeline`?**
  _High betweenness centrality (0.050) - this node is a cross-community bridge._