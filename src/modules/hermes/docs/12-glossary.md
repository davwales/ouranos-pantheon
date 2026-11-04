# 12. Glossary: Hermes

The ubiquitous language of the Hermes bounded context. Platform and technical terms are in
the [platform glossary](../../../../docs/architecture/12-glossary.md).

| Term | Definition |
|------|------------|
| **Persona** | A reusable assistant personality (name, description, personality, scenario) selected when a conversation is created |
| **Model config** | A named configuration of an available model (system prompt, sampling settings and default tool set); marked unavailable when the model disappears from OuranosMl |
| **Trait** | A named, reusable system-prompt fragment attached to a conversation and merged with the persona and model config when the system prompt is built |
| **Conversation** | A chat thread with an LLM using one persona and model config at a chosen reasoning effort; tracks token usage and may sit in a folder |
| **Message** | An individual message within a conversation (system, user, assistant or summary role) |
| **Message part** | One ordered piece of a saved message: reasoning, text, a tool call or an image in a reply, or an image a user message refers to |
| **Tool set** | A named collection of enabled tools, each with optional additional description appended to the tool's own, plus optional instructions appended to the system prompt when its tools are offered |
| **Tool source** | Where a tool set's tools come from (`ToolSource`); only `Native` exists today |
| **Native tool** | A tool implemented in Hermes itself and registered in code (today `generate_image` and `edit_image`), as opposed to tools from external sources such as MCP servers |
| **Active tool set** | The one tool set a conversation currently offers the model (or none); starts as the model config's default tool set and can be switched mid-chat |
| **Generated image** | An image the chat model drew with the `generate_image` or `edit_image` tool, stored in `generated_images` and kept only while a saved conversation references it |
| **Image part** | The message part that places a generated image in a reply, or attaches an earlier one to a user message |
| **Image ref** | The short name (`image 3`) the model uses for an image in the chat, numbered by first appearance on every turn instead of exposing ids |
| **Source image** | The image an edited image was redrawn from (`GeneratedImage.SourceImageId`), shown as "Edited from" in the image dialog |
| **Edit strength** | How far `edit_image` may move away from its source (0-1), configured per named change: subtle, moderate or strong |
| **LoRA** | A config for one available LoRA (name, weight, trigger phrase) that a conversation can switch on to restyle its images; reads as unavailable when its LoRA leaves the image server |
| **Available LoRA** | A LoRA on the image server, discovered by the hourly sync job, with its compat class and whether it suits the configured image model |
| **Unsaved retention** | How long an image no saved conversation references survives before `PurgeImagesJob` deletes it (24 hours by default) |
| **Folder** | Nestable container for conversations (`ParentFolderId`), at most `Folder.MaxDepth` levels deep; a move that would push the moved subtree past it is rejected |
| **Compaction** | Summarizing a conversation's messages since the last summary into a single `Summary`-role message so long chats fit the context window; streamed over SSE |
| **Available model** | An LLM served by OuranosMl, discovered by the hourly sync job |
| **Default** | `IsDefault` on a persona or model config: the one preselected for new conversations |
| **Public mode** | Flagsmith flag `hermes.public_mode`: when on, Hermes read endpoints return only items marked `IsPublic` |
