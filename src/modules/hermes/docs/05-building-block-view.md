# 5. Building Block View: Hermes

Level-2 whitebox of the Hermes blackbox described in the [module README](README.md). Paths
are relative to `Ouranos.Pantheon.Modules.Hermes/`.

| Building block | Responsibility |
|----------------|----------------|
| `Features/Conversations/` | Conversation and message persistence; streaming chat replies through the shared assistant tool loop and conversation compaction (SSE) via OuranosMl |
| `Features/Conversations/Schemas/` | Message part contract shared by the slices that read and write messages |
| `Features/Assistants/Scopes/` | `IConversationScope`, the narrow interface Hermes chat tools are typed to (conversation id and active image LoRAs) |
| `Features/Assistants/Tools/` | The chat tool catalogue: `GenerateImage` (`generate_image`) |
| `Features/Assistants/ToolSources/` | `IHermesToolSource`, which lists the tools a source offers and resolves tool set entries into chat tools; `NativeToolSource` serves the built-in catalogue, and `ToolSetAssistantTool` appends a tool set's extra guidance to a tool's description |
| `Features/ToolSets/` | Tool set CRUD and the catalogue of tools available to tool sets (`GET /api/hermes/tools`) |
| `Features/Images/` | Serving generated images and the scheduled purge of unreferenced ones |
| `Features/Personas/` | Reusable assistant personality definitions applied to conversations |
| `Features/Traits/` | Attachable traits that merge with the conversation's persona at prompt-build time |
| `Features/Loras/` | Manually registered LoRAs that a conversation can switch on to restyle generated images |
| `Features/Models/` | Model configuration CRUD and the hourly available-model sync from OuranosMl |
| `Features/Folders/` | Folders for organizing conversations |
