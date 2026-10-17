# 12. Glossary: Hestia

The ubiquitous language of the Hestia bounded context. Platform and technical terms are in
the [platform glossary](../../../../docs/architecture/12-glossary.md).

| Term | Definition |
|------|------------|
| **Recipe** | A cooking recipe with full event-sourced version history (ingredients, steps, notes). Unrelated to Plutus's crafting recipes |
| **Recipe version** | A historical state of a recipe (event-sourced) |
| **Revert** | Restoring a prior recipe version as the current state |
| **Import** | Async pipeline turning a recipe-website URL into structured recipe data (scrape the page's JSON-LD recipe → LLM normalize → persist) |
| **Import status** | Where a recipe's import stands: none, importing, imported, or failed |
| **Re-import** | Running the import pipeline again for an existing recipe |
| **Shopping list** | A single Marten document combining ingredients of the recipes toggled onto it with manual items |
| **Manual item** | A free-text entry added to the shopping list directly rather than from a recipe |
| **Proposal** | A change the Kitchen Assistant suggests (note, ingredient, step) that the user applies or rejects |
