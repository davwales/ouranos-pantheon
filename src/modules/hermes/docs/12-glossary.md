# 12. Glossary: Hermes

The ubiquitous language of the Hermes bounded context. Platform and technical terms are in
the [platform glossary](../../../../docs/architecture/12-glossary.md).

| Term | Definition |
|------|------------|
| **Persona** | A reusable assistant personality (name, description, personality, scenario) selected when a conversation is created |
| **Model config** | A named configuration of an available model (system prompt and sampling settings); marked unavailable when the model disappears from OuranosMl |
| **Trait** | A named, reusable system-prompt fragment attached to a conversation and merged with the persona and model config when the system prompt is built |
| **Conversation** | A chat thread with an LLM using one persona and model config; tracks token usage and may sit in a folder |
| **Message** | An individual message within a conversation (system, user, assistant or summary role) |
| **Folder** | Nestable container for conversations (`ParentFolderId`) |
| **Compaction** | Summarizing a conversation's messages since the last summary into a single `Summary`-role message so long chats fit the context window; streamed over SSE |
| **Available model** | An LLM served by OuranosMl, discovered by the hourly sync job |
| **Default** | `IsDefault` on a persona or model config: the one preselected for new conversations |
| **Public mode** | Flagsmith flag `hermes.public_mode`: when on, Hermes read endpoints return only items marked `IsPublic` |
