# Hestia Module

Hestia manages cooking recipes with full version history, imports recipes from websites with
LLM normalization, keeps a shopping list, and offers the Kitchen Assistant on the recipe
page.

This is the module's arc42 entry point, refining the platform's building block view
([platform Section 5](../../../../docs/architecture/05-building-block-view.md)) one level
down. Platform-wide concepts are described in the
[platform documentation](../../../../docs/architecture/README.md).

## Contents

| Section | Focus |
|---------|-------|
| [5. Building Block View](05-building-block-view.md) | The module's feature slices |
| [6. Runtime View](06-runtime-view.md) | Recipe import, Kitchen Assistant |
| [8. Concepts](08-concepts.md) | Event sourcing, the Kitchen Assistant's tools, scraping safety |
| [11. Risks and Technical Debt](11-risks-and-technical-debt.md) | Module-scoped debt (`HESTIA-D*`) |
| [12. Glossary](12-glossary.md) | Hestia ubiquitous language |
| [Decisions](adr/README.md) | Module ADR log (`HESTIA-ADR-*`) |

## Responsibilities

- Create, update and revert recipes, keeping every version as an event stream
- Import and re-import recipes from a URL asynchronously
- Maintain one shopping list from selected recipes and manual items
- Let the Kitchen Assistant propose recipe changes for the user to apply

## Context

| Neighbour | Channel | Purpose | Code entry point |
|-----------|---------|---------|------------------|
| Recipe websites | Outbound HTTPS (HTML with JSON-LD metadata) | Recipe import | `Features/Recipes/ImportRecipe/Scraping/RecipeScraper.cs` |
| OuranosMl | Outbound HTTP (Responses API: structured output, assistant streaming) | Recipe normalization, Kitchen Assistant | `Features/Recipes/ImportRecipe/`, `Features/Assistants/` |
| Interface | Inbound REST + SSE | Hestia pages under `src/apps/interface/src/app/hestia/` | Every `*Endpoint.cs` |

Code paths are relative to `Ouranos.Pantheon.Modules.Hestia/`. Hestia depends only on the
shared kernel.

## Provided Interfaces

- **REST**: routes under `/api/hestia/...`, one Bruno request per endpoint in
  `bruno/ouranos-pantheon/collections/API/Hestia/`. Import and re-import return
  `202 Accepted`.
- **SSE**: `/api/hestia/recipes/assistant/completions/stream` (Kitchen Assistant).

### Messages

Module-internal; no message crosses a module boundary.

| Message | Exchange | Queue | DLQ |
|---------|----------|-------|-----|
| `ImportRecipeRequested` | `hestia.recipe` | `hestia.recipe.import` | `hestia.recipe.import.dlq` |

The import consumer records failures (timeouts included) as `RecipeImportFailed` events
instead of throwing, so the DLQ only sees infrastructure faults.

### Scheduled Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|
| `SweepStaleImportsJob` | every 5 minutes | Fails imports stuck in progress longer than `RecipeImport.StaleImportTimeout` |

## Persistence

Marten on PostgreSQL ([ADR 0004](../../../../docs/adr/0004-polyglot-persistence-on-postgresql.md)):
one event stream per recipe with inline snapshots, and the shopping list as a plain Marten
document. Hestia has no EF Core context.

## Configuration

`Ouranos:Hestia` (`HestiaOptions`): `RecipeImport` (model settings for normalization with low
reasoning effort, plus `StaleImportTimeout`, default 15 minutes, after which the sweeper fails
an import) and `KitchenAssistant` (assistant prompt and model overrides).
