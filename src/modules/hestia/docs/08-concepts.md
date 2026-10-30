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

## 8.4 Shopping List Consolidation

`ShoppingListConsolidator` (`Shared/Domain/ShoppingLists/`) merges recipe ingredients at read
time; nothing is rewritten in the recipes.

- **Line identity** is `recipe:<name key>|<measure>`. The name key is the normalized name with its
  last word singularized by suffix rules, so "Eggs" and "egg" are one line. The measure is
  `volume` or `mass` for convertible units (`MeasurementUnits`), the singular unit for anything
  else ("clove"), or `unquantified` for lines without an amount ("salt, to taste"), which stay
  apart from quantified ones. Checked state is stored by line id, so it survives edits to a
  recipe's quantity, unit spelling or plural.
- **Conversion**: mixed units of one dimension are summed and shown in the largest unit the
  recipes used (3 tsp + 1 tbsp = 2 tbsp). Volume and mass never convert into each other.
- **Categories** are assigned by the user per item and stored by name key in the list document,
  so they are remembered after an item leaves the list.
- **Clear checked** removes checked manual items and recipes whose every line is checked; a
  partly bought recipe keeps its checked lines.
