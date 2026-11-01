# 8. Concepts: Hermes

## 8.1 Public Mode

Hermes reads the Flagsmith flag `hermes.public_mode` through `IFlagsmithClient`
([ADR 0012](../../../../docs/adr/0012-feature-flags-via-flagsmith.md)). While it is enabled,
the read handlers (conversations, folders, personas, traits, models) return only entities
whose `IsPublic` is set and treat the others as not found.

- **Folders hide their subtree.** A folder or conversation is visible only when it and every
  folder above it are public (`FolderVisibility`), so a private folder hides everything below
  it, whatever the descendants' own flags say. Folder conversation and subfolder counts count
  only visible children. Generated images follow the conversations that show them.
- **Stored data is read-only.** Every create, update and delete endpoint is marked with
  `DisabledInPublicMode()`, an endpoint filter (`PublicModeWriteFilter`) that answers 403. Chat
  completion and compaction stay open for unsaved chats but are rejected when they name a
  saved conversation, because they would append to it. Visitors can chat, not save.
- **The Interface asks first.** `GET /api/hermes/public-mode` reports the flag, and the
  Interface hides its create, edit, save, move and delete controls while it is on. A saved
  conversation opens as an unsaved copy that can be continued but not saved.
Public mode fails closed: the gateway's base `appsettings.json` lists `hermes.public_mode` as
enabled under `Ouranos:Flagsmith:DefaultFlags`, so while Flagsmith is unreachable reads are
limited to public entities instead of failing.

## 8.2 Reply Parts and History Replay

An assistant reply is saved as `Message.Content` plus its ordered `MessagePart`s: reasoning,
text, tool calls (with status and output) and images. A user message has only image parts,
for the earlier images it refers to. `Content` is the source of truth for the
reply's text: compaction, naming and editing use it, and text parts exist only so a reloaded
reply renders with its reasoning and tool calls in the order they streamed. Parts are typed
rows in one table-per-hierarchy table rather than JSON, because EF Core's JSON columns cannot
hold a polymorphic type.

When a conversation is resent, each earlier reply's tool calls are replayed to the model as
`function_call` / `function_call_output` items before the reply's text, with outputs capped
like live tool output, so the model can build on what its tools returned. Reasoning is not
replayed.

**Image refs.** The model never sees image ids. Each completion numbers the images in the
history by first appearance (`image 1`, `image 2`, ...): an image a tool call showed (the
client sends its id with the replayed call) and an image a user message refers to (its image
parts). A replayed tool output gains ` It is image N.`, and a user message with an image is
replayed as `[Attached image N: "<prompt>"]` before its text, so the model can revise the
prompt. Refs are computed on every turn and never stored, so they stay right after messages
are edited or deleted; images that no longer exist get no number.

## 8.3 Tool Calling

Hermes chat runs on the shared assistant tool loop (`AssistantToolLoop`), so it uses the same
tool abstraction, limits and event contract as the module assistants. Tools are typed to
`IConversationScope` and registered for `HermesChatContext` with `AddAssistantTool`. Hermes
has no page to apply proposals, so tool actions are ignored and tools answer through their
output, plus an optional image.

Which tools a reply may call is decided by **tool sets**, not by the tools themselves. A tool
set names the tools it enables, each identified by its source and name, and can add guidance
at two levels: an additional description appended to a tool's own description, and
instructions appended to the system prompt. That lets one set tell the model to draw freely
while another draws only on request, so tool descriptions stay neutral about when to call them.

- A model config has an optional **default tool set**; models without one get no tools, since
  small models may not support function calling.
- Each conversation has exactly one **active tool set** (or none), saved on the conversation
  and sent with every completion request. Choosing a model in the conversation settings resets
  it to that model's default, and the user can switch it mid-chat.
- `ToolSetResolver` loads the active set, asks each `IHermesToolSource` to resolve its entries
  (only `NativeToolSource` today; MCP servers are planned as another source), wraps each tool
  so its description carries the set's guidance, and drops tools whose `IsAvailable` is false.
  An unknown set means no tools rather than a failed reply.
- When tools are offered, the instructions are the system prompt, then the fixed
  `ToolInstructions` line, then the set's instructions.
- Deleting a tool set clears it from the models and conversations that used it.

Reasoning effort belongs to the conversation, not the model config, so the user can raise or
lower it mid-chat without editing a shared model config. Unset sends no reasoning block and
leaves the model at its own default. Reasoning tokens count against the model config's max
tokens, which defaults to the shared model default when unset. The conversation's recorded
total token count is the context the last model round filled, not the sum over tool rounds.

## 8.4 Generated Images

The `generate_image` tool asks OuranosMl for one image per call and stores it in
`generated_images`; the reply references it with an image part (`ImageMessagePart`) placed
after the tool call. The tool delivers at most one image per reply
(`IsSingleDeliveryPerReply`), because each generation can take minutes. Every call draws from
scratch; changing an image keeps its composition only through `edit_image`.

**Editing.** The `edit_image` tool redraws an earlier image from a revised prompt with
OuranosMl's img2img (`POST /images/edits`). Its source is the image the user attached to the
message, else the ref the model names, else the latest image. SwarmUI's img2img does not follow
instructions, so the prompt describes the whole desired image, not just the change. The model
picks `change: subtle | moderate | strong` instead of a float, mapped to strengths in
`ImageGeneration:EditStrengths` (0.3, 0.55 and 0.75 by default). The new image records its
source in `SourceImageId`, without a foreign key, so the dialog can link to it while it exists.
`edit_image` is separate from `generate_image` so tool sets can enable either; both deliver at
most one image per reply each.

An image can be the whole reply, for example to illustrate a scene the model has already
described. The tool's output tells the model the user already sees the image and not to
describe it unless asked, and a reply with an image but no text is saved with empty content;
history replay then sends only its tool call.

**Lifetime.** An image is kept only while a message part references it, from an assistant
reply or a user message that attached it, and unsaved chats have no message rows, so their
images are unreferenced. `PurgeImagesJob` deletes every
unreferenced image older than `UnsavedRetention` (24 hours by default). That one rule covers
abandoned unsaved chats, replies dropped by retry, edit or delete, and deleted conversations.
Saving an unsaved chat within the window writes its image parts, which keeps the images.
OuranosMl is called with `save=false`, so SwarmUI keeps no copy either.

Image parts have no foreign key to `generated_images`: saving a chat whose images already
expired must still succeed, so the part stays and the Interface shows "This image is no
longer available" when the image returns 404.

**Access.** `GET /api/hermes/images/{imageId}` serves the bytes with a one-day private cache,
and `GET /api/hermes/images/{imageId}/details` its prompt, model, seed and source.
In public mode an image referenced by a private conversation, or one under a private folder, is
treated as not found; an unreferenced image belongs to an unsaved chat and is reachable only
by its unguessable id.

**Model output.** Markdown images in replies render as links, so a model that makes up an
image URL cannot make the page load third-party content.

## 8.5 Image LoRAs

**Catalogue.** `SyncLorasJob` mirrors OuranosMl's `GET /images/loras` into `available_loras`
every hour: id, title, compat class, default weight, trigger phrase and usage hint. It also reads
`GET /images/models` and marks each LoRA compatible unless its compat class differs from the
configured image model's (`ImageGeneration:Model`, else OuranosMl's default); an unknown class on
either side counts as compatible. An empty list or a failed call leaves the catalogue as it was,
because that is likelier a restarting image server than every LoRA being removed.

**Configs.** A LoRA config (`/api/hermes/loras`) picks one catalogue LoRA (at most one config each,
409 otherwise) and adds a display name, a weight between -2 and 2, an editable trigger phrase and
the public flag; the form prefills them from the catalogue. Configs are never deleted by the
sync: reads join the catalogue on the identifier and report `isUnavailable` (no longer listed)
and `isIncompatible`. The conversation settings list the configs whenever the active tool set
enables `generate_image` or `edit_image`, with the same badges; an unavailable one can only be
switched off. The active ones are saved with the conversation (`conversation_lora`), and the
completion request sends their ids.

**Generation.** The completion handler loads the active configs, skips unavailable ones (OuranosMl
rejects unknown LoRAs, which would fail the whole image) and puts the rest on the tool context
(`IConversationScope.ImageLoras`). Both image tools send them in OuranosMl's typed `loras` field
and prepend each trigger phrase the prompt doesn't already contain. The chat model never sees the
LoRAs: phrases are added after the prompt is capped at `MaxPromptLength`, and the stored prompt
and alt text keep the model's own wording, so "more like this" doesn't repeat them. An
incompatible LoRA is still sent if the user switched it on; OuranosMl only logs a warning.

