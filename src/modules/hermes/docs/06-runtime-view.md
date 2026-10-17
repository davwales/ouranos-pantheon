# 6. Runtime View: Hermes

## 6.1 Streaming Chat

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant EP as Hermes endpoint
    participant H as GenerateCompletionHandler
    participant L as AssistantToolLoop
    participant T as Tool
    participant ML as OuranosMl (Responses)
    participant DB as Postgres (schema hermes)

    UI ->> EP: send conversation (persona, model, traits, messages + earlier tool calls)
    EP ->> H: GenerateCompletionInput
    H -->> UI: instructions event (composed system prompt)
    H ->> L: instructions, replayed history, offered tools
    loop until the model answers without a tool call
        L ->> ML: POST /responses (stream)
        ML -->> L: reasoning, text, function calls
        L -->> UI: reasoning / content / tool events
        opt function call
            L ->> T: invoke
            T -->> L: output
            L -->> UI: toolResult event
        end
    end
    L -->> UI: usage event
    opt conversation provided
        H ->> DB: persist latest user message + reply with its ordered parts
    end
    H -->> UI: done
```

The server is stateless with respect to chat history: the client sends the whole
conversation on every request, and the handler persists only the newest turn. A reply that
ends without text is reported as an `error` event and nothing is persisted. Compaction
streams its own `content`/`usage`/`complete` events, ignores the model's reasoning and
stores its result as one `Summary`-role message.

The Interface folds the events into the shared assistant turn UI. Saved replies render from
their parts; older messages without parts fall back to `<think>` tag parsing.
