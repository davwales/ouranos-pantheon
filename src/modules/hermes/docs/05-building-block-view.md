# 5. Building Block View: Hermes

Level-2 whitebox of the Hermes blackbox described in the [module README](README.md). Paths
are relative to `Ouranos.Pantheon.Modules.Hermes/`.

| Building block | Responsibility |
|----------------|----------------|
| `Features/Conversations/` | Conversation and message persistence; streaming chat replies through the shared assistant tool loop and conversation compaction (SSE) via OuranosMl |
| `Features/Conversations/Schemas/` | Message part contract shared by the slices that read and write messages |
| `Features/Assistants/Scopes/` | `IConversationScope`, the narrow interface Hermes chat tools are typed to (conversation id, active image LoRAs and the chat's numbered images) |
| `Features/Assistants/Tools/` | The chat tool catalogue: `GenerateImage` (`generate_image`) and `EditImage` (`edit_image`), which share `ImageTool` |
| `Features/Assistants/ToolSources/` | `IHermesToolSource`, which lists the tools a source offers and resolves tool set entries into chat tools; `NativeToolSource` serves the native tools, and `ToolSetAssistantTool` appends a tool set's extra guidance to a tool's description |
| `Features/ToolSets/` | Tool set CRUD and the catalogue of tools available to tool sets (`GET /api/hermes/tools`) |
| `Features/Images/` | Serving generated images and their details, and the scheduled purge of unreferenced ones |
| `Features/Personas/` | Reusable assistant personality definitions applied to conversations |
| `Features/Traits/` | Attachable traits that merge with the conversation's persona at prompt-build time |
| `Features/Loras/` | The LoRA catalogue synced from OuranosMl (`SyncLorasJob`, `GET /api/hermes/available-loras`) and the LoRA configs a conversation can switch on to restyle generated images |
| `Features/Models/` | Model configuration CRUD and the hourly available-model sync from OuranosMl |
| `Features/Folders/` | Folders for organizing conversations |
| `Features/PublicMode/` | Reports whether public mode is on (`GET /api/hermes/public-mode`) so the Interface can hide write controls |
| `Shared/PublicMode/` | `DisabledInPublicMode()` and its endpoint filter, which reject writes with 403 while public mode is on |
