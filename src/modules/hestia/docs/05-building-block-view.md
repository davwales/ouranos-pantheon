# 5. Building Block View: Hestia

Level-2 whitebox of the Hestia blackbox described in the [module README](README.md). Paths
are relative to `Ouranos.Pantheon.Modules.Hestia/`.

| Building block | Responsibility |
|----------------|----------------|
| `Features/Recipes/` | Recipe create/update with full version history and revert (no delete), persisted as a Marten event stream per recipe |
| `Features/Recipes/ImportRecipe/` | Async import: enqueue an import request → scrape the page (JSON-LD, anti-SSRF-guarded) → LLM-normalize via OuranosMl structured output (Responses API, low reasoning effort) → append events |
| `Features/Recipes/ReimportRecipe/` | Re-runs the import for an existing recipe (409 while one is in progress) |
| `Features/Recipes/SweepStaleImports/` | Scheduled job that fails imports stuck in progress past the stale timeout |
| `Features/Assistants/` | Everything assistant-related: the Kitchen Assistant (below), the tool catalogue in `Tools/` (every recipe tool, plus the recipe loading and validation they share), and in `Scopes/` the `IRecipeScope` the tools are typed to |
| `Features/Assistants/AskKitchenAssistant/` | Streaming Kitchen Assistant (`PantheonAssistant<KitchenAssistantContext>`): loads the recipe from Marten and answers questions about it via OuranosMl; opts into catalogue tools that propose notes and ingredient or step changes for the user to apply |
| `Features/ShoppingLists/` | Shopping list built from selected recipes plus manual items; stored as a single Marten document, not event-sourced |
