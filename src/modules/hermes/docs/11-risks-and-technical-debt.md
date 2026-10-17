# 11. Risks and Technical Debt: Hermes

Module-scoped items. Platform-wide risks stay in
[platform Section 11](../../../../docs/architecture/11-risks-and-technical-debt.md).

| # | Item | Where | Risk / impact | Proposed severity |
|---|------|-------|---------------|-------------------|
| HERMES-D1 | **GPU contention between chat and image generation**: SwarmUI and the LLM backend may share one GPU, and nothing coordinates them | `Features/Assistants/Tools/GenerateImage`, OuranosMl | A generation can be slow while the chat model is loaded, or evict it so the next reply pays a model load | Medium |
| HERMES-D2 | **Silent chat stream during a tool call**: the OuranosMl client skips the status and progress events, so nothing reaches the browser, so the chat SSE stream is idle while an image generates | `GenerateCompletionHandler`, `AssistantToolLoop` | Fine with the Interface talking to the gateway directly; a proxy with an idle timeout would cut long generations. Forwarding progress as an event would fix this and enable a progress bar | Low |
| HERMES-D3 | **Images stored in Postgres**: generated images are `bytea` rows in the main database | `generated_images`, `PurgeImagesJob` | Saved conversations with many images grow the database and its backups; fine at personal scale, revisit with object storage if volume grows | Low |
