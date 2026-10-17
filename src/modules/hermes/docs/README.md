# Hermes Module

Hermes is the chat interface over locally hosted LLMs: conversations organized in folders,
reusable personas and traits, model configurations, streaming completions and compaction.

This is the module's arc42 entry point, refining the platform's building block view
([platform Section 5](../../../../docs/architecture/05-building-block-view.md)) one level
down. Platform-wide concepts are described in the
[platform documentation](../../../../docs/architecture/README.md).

## Contents

| Section | Focus |
|---------|-------|
| [5. Building Block View](05-building-block-view.md) | The module's feature slices |
| [6. Runtime View](06-runtime-view.md) | Streaming chat |
| [8. Concepts](08-concepts.md) | Public mode |
| [12. Glossary](12-glossary.md) | Hermes ubiquitous language |
| [Decisions](adr/README.md) | Module ADR log (`HERMES-ADR-*`) |

## Responsibilities

- Persist conversations, messages, folders, personas, traits and model configurations
- Stream chat completions and conversation compaction from OuranosMl over SSE
- Keep the list of available models in sync with OuranosMl
- Limit reads to public entities while public mode is on

## Context

| Neighbour | Channel | Purpose | Code entry point |
|-----------|---------|---------|------------------|
| OuranosMl | Outbound HTTP (OpenAI-compatible Chat Completions, model list) | Chat, compaction, conversation naming, model sync | `Features/Conversations/`, `Features/Models/` |
| Flagsmith | Outbound HTTP (via the Shared module's client) | `hermes.public_mode` flag | Read handlers |
| Interface | Inbound REST + SSE | Hermes pages under `src/apps/interface/src/app/hermes/` | Every `*Endpoint.cs` |

Code paths are relative to `Ouranos.Pantheon.Modules.Hermes/`. Hermes depends only on the
shared kernel.

## Provided Interfaces

- **REST**: routes under `/api/hermes/...`, one Bruno request per endpoint in
  `bruno/ouranos-pantheon/collections/API/Hermes/`.
- **SSE**: chat completion and conversation compaction.
- **Messages**: none; Hermes has no async messaging.

### Scheduled Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|
| `SyncModelsJob` | hourly | Sync available LLM models from OuranosMl |

## Persistence

Schema `hermes` (EF Core, `HermesDbContext`), plain relational.

## Configuration

`Ouranos:Hermes` (`HermesOptions`): conversation-naming prompt and model, and the compaction
prompt, temperature and max tokens. Conversation naming is off unless both the prompt and the
model are set.
