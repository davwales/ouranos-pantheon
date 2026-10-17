# 12. Glossary: Shared

Terms owned by the Shared module. Platform-level terms (module, shared kernel, vertical
slice, …) are in the [platform glossary](../../../../docs/architecture/12-glossary.md).

| Term | Definition |
|------|------------|
| **Notification** | Outbox row for a message to send on a channel (`Discord`, `Email`, `Desktop`) |
| **Health check** | An implementation of the Shared module's own `IHealthCheck`; results are aggregated by `GET /health` |
| **Assistant** | A `PantheonAssistant<TContext>`: a page-scoped, server-prompted chat over OuranosMl with optional tools |
| **Assistant tool loop** | `AssistantToolLoop<TContext>`: the engine that streams a reply over the Responses API, runs the tools the model calls and emits the assistant event contract; used by page assistants and Hermes chat |
| **Tool** | A function the model may call during an assistant reply; reads data or proposes an action |
| **Scope** | A context interface a tool is typed to, so one tool serves every assistant whose context implements it |
| **Action** | A typed proposal streamed to the page that the user applies or rejects |
| **Assistant image** | An image a tool returns alongside its output, streamed as an `image` event and shown in the turn; the model sees only the output text |
