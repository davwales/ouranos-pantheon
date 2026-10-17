# 8. Concepts: Hermes

## 8.1 Public Mode

Hermes reads the Flagsmith flag `hermes.public_mode` through `IFlagsmithClient`
([ADR 0012](../../../../docs/adr/0012-feature-flags-via-flagsmith.md)). While it is enabled,
the read handlers (conversations, folders, personas, traits, models) return only entities
whose `IsPublic` is set and treat the others as not found. Writes, completion and compaction
are not gated, so public mode limits what is listed, not what can be done.

## 8.2 Reply Parts and History Replay

An assistant reply is saved as `Message.Content` plus its ordered `MessagePart`s: reasoning,
text and tool calls (with status and output). `Content` is the source of truth for the
reply's text: compaction, naming and editing use it, and text parts exist only so a reloaded
reply renders with its reasoning and tool calls in the order they streamed. Parts are typed
rows in one table-per-hierarchy table rather than JSON, because EF Core's JSON columns cannot
hold a polymorphic type.

When a conversation is resent, each earlier reply's tool calls are replayed to the model as
`function_call` / `function_call_output` items before the reply's text, with outputs capped
like live tool output, so the model can build on what its tools returned. Reasoning is not
replayed.

## 8.3 Tool Calling

Hermes chat runs on the shared assistant tool loop (`AssistantToolLoop`), so it uses the same
tool abstraction, limits and event contract as the module assistants. Tools are typed to
`IConversationScope` and registered for `HermesChatContext` with `AddAssistantTool`. A model
config offers tools only when its `IsToolCallingEnabled` is set, because small models may not
support function calling; Hermes appends its own short tool guidance to the prompt only when
tools are offered. Hermes has no page to apply proposals, so tool actions are ignored and
tools answer through their output.

Reasoning effort belongs to the conversation, not the model config, so the user can raise or
lower it mid-chat without editing a shared model config. Unset sends no reasoning block and
leaves the model at its own default. Reasoning tokens count against the model config's max
tokens, which defaults to the shared model default when unset. The conversation's recorded total token count is the context the last
model round filled, not the sum over tool rounds.
