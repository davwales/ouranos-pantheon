# 6. Runtime View: Hermes

## 6.1 Streaming Chat

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant EP as Hermes endpoint
    participant H as GenerateCompletionHandler
    participant ML as OuranosMl (OpenAI-compatible)
    participant DB as Postgres (schema hermes)

    UI ->> EP: send conversation (persona, model, traits, messages)
    EP ->> H: GenerateCompletionInput
    H ->> ML: chat completion stream (system prompt + messages)
    ML -->> H: streamed tokens
    H -->> UI: SSE response
    opt conversation provided
        H ->> DB: persist latest user message + assistant reply
    end
```

The server is stateless with respect to chat history: the client sends the whole
conversation on every request, and the handler persists only the newest turn. Compaction
streams the same way and stores its result as one `Summary`-role message.

The Interface renders Hermes replies through the shared assistant `Message` component, with
`<think>` tags parsed into reasoning blocks.
