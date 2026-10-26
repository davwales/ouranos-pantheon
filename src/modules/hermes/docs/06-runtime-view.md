# 6. Runtime View: Hermes

## 6.1 Streaming Chat

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant EP as Hermes endpoint
    participant H as GenerateCompletionHandler
    participant R as ToolSetResolver
    participant L as AssistantToolLoop
    participant T as Tool
    participant ML as OuranosMl (Responses)
    participant DB as Postgres (schema hermes)

    UI ->> EP: send conversation (persona, model, traits, LoRAs, active tool set, messages + earlier tool calls)
    EP ->> H: GenerateCompletionInput
    H ->> R: active tool set id
    R ->> DB: load tool set and its tools
    R -->> H: available tools (with set guidance) + set instructions
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
ends without text is reported as an `error` event and nothing is persisted.

## 6.2 Conversation Compaction

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant H as CompactConversationHandler
    participant ML as OuranosMl (Responses)
    participant DB as Postgres (schema hermes)

    UI ->> H: conversation messages, persona, model
    alt no user or assistant messages since the last summary
        H -->> UI: error event ("Nothing to compact.")
    else
        H ->> H: keep the newest messages within CompactionMaxInputCharacters
        H ->> ML: POST /responses (stream, no tools)
        ML -->> H: reasoning (ignored), text
        H -->> UI: content events
        alt stream fails or summary is empty
            H -->> UI: error event
        else
            opt conversation provided
                H ->> DB: insert Summary message, record summary size as token usage
            end
            H -->> UI: usage event, then complete
        end
    end
```

Compaction summarizes the user and assistant messages since the last summary and stores
the result as one `Summary`-role message. The summary prompt is bounded: the newest messages
that fit `CompactionMaxInputCharacters` are kept, older ones are dropped with a note to the
model, and a single newest message that exceeds the budget on its own is truncated. Every
failure (nothing to compact, a stream error, or a summary that comes back empty because
reasoning used up the output budget) ends the stream with an `error` event and saves nothing;
the Interface shows the message and removes the placeholder summary. After a successful
compaction the conversation's token usage is the summary's size, because the summary is the
whole context the next turn starts from.

The Interface folds the events into the shared assistant turn UI. Saved replies render from
their parts; older messages without parts fall back to `<think>` tag parsing.

## 6.3 Image Generation

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant L as AssistantToolLoop
    participant T as GenerateImageTool
    participant ML as OuranosMl (/images/generations)
    participant DB as Postgres (schema hermes)

    L -->> UI: tool event (generate_image)
    L ->> T: invoke (prompt, orientation)
    T ->> ML: POST /images/generations (prompt + LoRA tags, stream, save=false)
    loop until done
        ML -->> T: status / progress events
    end
    ML -->> T: completed (base64 image), done
    T ->> DB: insert generated_images row
    T -->> L: output text + image
    L -->> UI: toolResult event, then image event
    UI ->> UI: render ImageBlock from GET /api/hermes/images/{id}
```

The tool appends one `<lora:identifier:weight>` tag per active LoRA to the prompt it sends
(see [8.5](08-concepts.md#85-image-loras)). The tool streams from OuranosMl so status and progress events keep the connection busy
through a model load and sampling; it uses only the completed image for now. The model sees
only the tool's short output text, never the image. Stopping the reply cancels the request,
OuranosMl interrupts SwarmUI and nothing is stored. While the tool runs, the Interface shows
the tool block with its elapsed time.

### Image Editing

```mermaid
sequenceDiagram
    autonumber
    participant UI as Interface
    participant H as GenerateCompletionHandler
    participant T as EditImageTool
    participant ML as OuranosMl (/images/edits)
    participant DB as Postgres (schema hermes)

    UI ->> UI: Edit (chip) or More like this on an image
    UI ->> H: completion request, user message with an image part
    H ->> DB: load prompts of the referenced images
    H ->> H: number images (image 1, image 2, ...), replay refs and the attached prompt
    H ->> T: invoke (prompt, change, image?)
    T ->> T: source = attached image, else named ref, else latest image
    T ->> DB: load the source image
    T ->> ML: POST /images/edits (image, strength, prompt + LoRA tags, no size)
    ML -->> T: completed (base64 image), done
    T ->> DB: insert generated_images row with source_image_id
    T -->> UI: toolResult event, then image event
```

**Edit** attaches the image to the composer as a removable chip; the user then describes the
change. **More like this** sends "More like this" with the image attached straight away, and
the tool description asks the model to reuse the prompt with `change: subtle`. Without `size`,
OuranosMl keeps the source's aspect ratio, and its random seed makes each variation differ.
The image dialog shows "Edited from" for an edited image (`GET /api/hermes/images/{id}/details`)
and can step back through the sources.
