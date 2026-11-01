# Hermes Module

Hermes is the chat interface over locally hosted LLMs: conversations organized in folders,
reusable personas and traits, model configurations, image LoRAs, streaming completions and
compaction.

This is the module's arc42 entry point, refining the platform's building block view
([platform Section 5](../../../../docs/architecture/05-building-block-view.md)) one level
down. Platform-wide concepts are described in the
[platform documentation](../../../../docs/architecture/README.md).

## Contents

| Section | Focus |
|---------|-------|
| [5. Building Block View](05-building-block-view.md) | The module's feature slices |
| [6. Runtime View](06-runtime-view.md) | Streaming chat, image generation |
| [8. Concepts](08-concepts.md) | Public mode, reply parts and history replay, tool calling and tool sets, generated images, image LoRAs |
| [11. Risks and Technical Debt](11-risks-and-technical-debt.md) | Module risks (`HERMES-D*`) |
| [12. Glossary](12-glossary.md) | Hermes ubiquitous language |
| [Decisions](adr/README.md) | Module ADR log (`HERMES-ADR-*`) |

## Responsibilities

- Persist conversations, messages, folders, personas, traits, LoRAs, tool sets and model
  configurations
- Stream chat replies (reasoning, text and tool calls) and conversation compaction from
  OuranosMl over SSE
- Generate images for the chat model through OuranosMl and serve them to the Interface
- Keep the lists of available models and image LoRAs in sync with OuranosMl
- Limit reads to public entities and reject writes while public mode is on

## Context

| Neighbour | Channel | Purpose | Code entry point |
|-----------|---------|---------|------------------|
| OuranosMl | Outbound HTTP (OpenAI-compatible Responses, model list, streamed `/images/generations` and `/images/edits`, `/images/loras`, `/images/models`) | Chat, compaction, conversation naming, model and LoRA sync, image generation and editing | `Features/Conversations/`, `Features/Models/`, `Features/Loras/`, `Features/Assistants/Tools/` |
| Flagsmith | Outbound HTTP (via the Shared module's client) | `hermes.public_mode` flag | Read handlers, `Shared/PublicMode/`, `Features/PublicMode/` |
| Interface | Inbound REST + SSE | Hermes pages under `src/apps/interface/src/app/hermes/` | Every `*Endpoint.cs` |

Code paths are relative to `Ouranos.Pantheon.Modules.Hermes/`. Hermes depends only on the
shared kernel.

## Provided Interfaces

- **REST**: routes under `/api/hermes/...`, one Bruno request per endpoint in
  `bruno/ouranos-pantheon/collections/API/Hermes/`.
- **SSE**: chat completion (the shared assistant event contract, see the
  [Shared concepts](../../shared/docs/08-concepts.md)) and conversation compaction.
- **Messages**: none; Hermes has no async messaging.

### Scheduled Jobs

| Job | Schedule | Purpose |
|-----|----------|---------|
| `SyncModelsJob` | hourly | Sync available LLM models from OuranosMl |
| `SyncLorasJob` | hourly | Sync the image server's LoRAs into the available-LoRA catalogue; keeps it when OuranosMl lists none |
| `PurgeImagesJob` | hourly (minute 15) | Delete generated images no saved conversation references once older than the unsaved retention |

## Persistence

Schema `hermes` (EF Core, `HermesDbContext`), plain relational. Assistant replies keep their
ordered parts in `message_parts`, one table for every part kind (table-per-hierarchy on
`kind`). Generated images are stored as `bytea` in `generated_images` and served by
`GET /api/hermes/images/{imageId}`, and an edited image keeps its source in `source_image_id`; see [8.4](08-concepts.md#84-generated-images) for their
lifetime. The synced LoRA catalogue lives in `available_loras`; LoRA configs live in `loras`
(matched to the catalogue by `identifier`) and attach to conversations through the
`conversation_lora` join table, like traits. Tool sets live in `tool_sets`, with their enabled
tools in `tool_set_tools` (unique per set on source and name). `PUT /api/hermes/conversations/{id}` sets every
metadata field exactly (a `null` `folderId` means the root folder) but replaces the messages only
when `messages` is supplied, so metadata updates leave turns persisted by completions untouched.

## Configuration

`Ouranos:Hermes` (`HermesOptions`): conversation-naming prompt, model and reasoning effort,
and the compaction prompt, temperature, max tokens and reasoning effort.
`CompactionMaxInputCharacters` (default 32000; zero or negative uses the default) bounds the
conversation history sent for compaction. Conversation naming is off unless both the prompt
and the model are set.

`Ouranos:Hermes:ImageGeneration` (`HermesImageGenerationOptions`): `IsEnabled` (default
true; off also skips the LoRA sync), `Model` (null uses OuranosMl's default image model; LoRA
compatibility is judged against it), `UnsavedRetention` (default
24 hours), `MaxPromptLength` (default 1000) and `SquareSize`, `PortraitSize` and
`LandscapeSize` (`Width`/`Height` per orientation; defaults 1024x1024, 832x1216 and
1216x832, which suit SDXL- and Flux-class models). `EditStrengths` sets the img2img
strength `edit_image` uses for each change: `Subtle` (0.3), `Moderate` (0.55) and `Strong`
(0.75).

Per-chat model settings live on each model config, not in configuration: system prompt,
temperature, max tokens, context window and the default tool set. Reasoning effort is a
per-conversation setting the user can change mid-chat, as are the active tool set and image
LoRAs.
