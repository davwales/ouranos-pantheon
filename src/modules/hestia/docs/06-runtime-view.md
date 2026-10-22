# 6. Runtime View: Hestia

## 6.1 Recipe Import

*An end-to-end AI-assisted ingestion flow.*

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant EP as Import endpoint
    participant M as Marten event store
    participant X as RabbitMQ exchange hestia.recipe
    participant C as ImportRecipeConsumer
    participant S as RecipeScraper (anti-SSRF)
    participant ML as OuranosMl (structured output)

    UI ->> EP: submit recipe URL
    EP ->> M: start recipe stream (import started)
    EP ->> X: ImportRecipeRequested
    EP -->> UI: 202 Accepted { recipeId }
    X ->> C: deliver (queue hestia.recipe.import)
    C ->> S: fetch + parse JSON-LD metadata
    C ->> ML: normalize ingredients/steps
    C ->> M: append import succeeded / failed events
```

Import failures are appended to the stream as `RecipeImportFailed` events. Their reasons are
short user-facing messages (the exception is logged, never stored). `ReimportRecipe` reruns the
same flow for an existing recipe, and returns 409 while an import is still in progress.

Every import ends as imported or failed, never stuck importing:

- **Timeouts:** scraper and model timeouts are failures, not cancellation. Only the consumer's
  own token cancels, so a slow site or model fails the import instead of being retried and
  dead-lettered.
- **Duplicate messages:** the consumer skips a recipe that is no longer importing, so a
  redelivered message never runs the model twice.
- **Publish failures:** the stream is saved before the message is published (there is no durable
  outbox yet). If publishing fails, the handler appends `RecipeImportFailed` straight away.
- **Sweeper:** `SweepStaleImportsJob` runs every 5 minutes and fails imports older than
  `RecipeImport.StaleImportTimeout` (15 minutes by default). This covers a crash between saving
  and publishing, and a message lost in the broker. It writes with optimistic concurrency, so an
  import that finishes during the sweep is left alone.

The scraper decompresses responses itself (gzip, deflate, brotli), because the anti-SSRF
handler does not and some sites compress without being asked. Yield, prep/cook/total times and
the image URL are read from the JSON-LD directly into `RecipeDetails`, not by the model.
Reviews, comments, ratings, media and nutrition are stripped before the JSON-LD goes to the
model. Ingredient lines without a unit become `whole` when they have an amount ("3 eggs") and
`as needed` otherwise, unless the model gives a phrase such as `to taste`. A range keeps its
lower bound as the quantity and the original range in the name, as in `eggs (2-3)`.

## 6.2 Kitchen Assistant

The Kitchen Assistant follows the platform's module-assistant flow with context
`{ recipeId }`, loading the recipe from Marten. Its tools only propose changes; applying one
goes through the normal recipe update endpoint.

```mermaid
sequenceDiagram
    autonumber
    participant UI as AssistantPanel
    participant EP as MapAssistant endpoint
    participant A as KitchenAssistant (PantheonAssistant)
    participant ES as Marten (schema hestia)
    participant ML as OuranosMl (Responses API)

    UI ->> EP: POST messages + context { recipeId }
    EP ->> A: AssistantCompletionInput<KitchenAssistantContext>
    A ->> ES: load Recipe
    A ->> ML: POST /responses stream (recipe instructions + messages + tools, reasoning effort)
    ML -->> A: reasoning deltas, then a function call (e.g. update_ingredient)
    A -->> UI: SSE reasoning … tool (callId, name, arguments)
    A ->> ES: tool loads Recipe and validates the change
    A -->> UI: SSE action (proposal), then toolResult (status, output)
    A ->> ML: POST /responses stream (same input + function call + its output)
    ML -->> A: output text deltas, then usage
    A -->> UI: SSE content … usage, done (or error)
    alt user clicks Apply
        UI ->> EP: PUT /api/hestia/recipes/{id} (normal update endpoint)
    else user clicks Reject
        UI ->> UI: mark the proposal rejected
    end
    UI ->> EP: next turn: messages (the reply carries actions: Applied / Rejected / Pending)
```

Because applied changes are ordinary recipe updates, the event-sourced history and revert
remain the undo.
