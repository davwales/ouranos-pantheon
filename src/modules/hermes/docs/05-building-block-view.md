# 5. Building Block View: Hermes

Level-2 whitebox of the Hermes blackbox described in the [module README](README.md). Paths
are relative to `Ouranos.Pantheon.Modules.Hermes/`.

| Building block | Responsibility |
|----------------|----------------|
| `Features/Conversations/` | Conversation and message persistence; streaming chat replies through the shared assistant tool loop and conversation compaction (SSE) via OuranosMl |
| `Features/Conversations/Schemas/` | Message part contract shared by the slices that read and write messages |
| `Features/Assistants/Scopes/` | `IConversationScope`, the narrow interface Hermes chat tools are typed to |
| `Features/Personas/` | Reusable assistant personality definitions applied to conversations |
| `Features/Traits/` | Attachable traits that merge with the conversation's persona at prompt-build time |
| `Features/Models/` | Model configuration CRUD and the hourly available-model sync from OuranosMl |
| `Features/Folders/` | Folders for organizing conversations |
