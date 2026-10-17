# 8. Concepts: Hestia

## 8.1 Event-Sourced Recipes

Recipes derive from `BaseEventSourcedEntity`. Operations return an
`OperationResult<Recipe>` (the new state plus the domain events to append), and every
version stays in the stream, so revert restores a prior state as a new version rather than
deleting history. There is no recipe delete. The shopping list is a plain Marten document,
not event-sourced.

## 8.2 Assistants

Hestia follows the platform assistant pattern (framework in the
[Shared module docs](../../shared/docs/README.md)). Tools live in the catalogue under
`Features/Assistants/Tools/` and are typed to `IRecipeScope`
(`Features/Assistants/Scopes/`).

| Assistant | Route | Context | Tools |
|-----------|-------|---------|-------|
| `KitchenAssistant` | `/api/hestia/recipes/assistant/completions/stream` | `{ recipeId }` | Proposal: `add_note`, `add_ingredient`, `update_ingredient` (substitutions, marking an ingredient optional), `remove_ingredient`, `update_step` |

## 8.3 Scraping Safety

The recipe scraper sends requests through an anti-SSRF HTTP handler, so imported URLs cannot
reach internal hosts on the trusted network. This is one of the few security controls in an
otherwise unauthenticated system
([platform §8.13](../../../../docs/architecture/08-crosscutting-concepts.md#813-security)).
