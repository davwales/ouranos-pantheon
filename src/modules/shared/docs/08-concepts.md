# 8. Concepts: Shared

## 8.1 AI Assistant Framework

The platform rule for assistants (prompts, model settings and domain data stay on the
server; the client sends only history and typed context references) is stated in
[platform §8.11](../../../../docs/architecture/08-crosscutting-concepts.md#811-real-time-infrastructure).
This section describes how the kernel's `Application/Assistants/` package implements it.
Each module documents its own assistants and tools in its module docs.

- **`AssistantToolLoop<TContext>`** is the reusable engine. It streams one reply through
  `IOuranosMachineLearningClient` over the OpenAI Responses API from caller-supplied
  instructions, history items and tools, runs the tool loop below and emits the event
  contract, but never `done`, so its caller decides how the turn ends. Hermes chat uses it
  directly, because it composes its own prompt, replays persisted tool calls and saves the
  turn (see the [Hermes concepts](../../hermes/docs/08-concepts.md)).
- **`PantheonAssistant<TContext>`** is the `IPantheonStreamHandler` base class for page
  assistants, built on the loop. Subclasses implement only
  `BuildPromptAsync(TContext, CancellationToken)`, loading their own domain data and
  returning an `AssistantPrompt` (model, system prompt, temperature, max tokens, reasoning
  effort) from module options. The base class sends the system prompt as `instructions`
  followed by the shared `AssistantFormatting` guideline (short answers, small tables, no
  LaTeX), since every page assistant renders in the same narrow panel. Context may be loaded
  directly from the store or composed by invoking existing query handlers through
  `IMessageBus`, which keeps the prompt consistent with what the page shows.
- **Tools**: each assistant can offer the model function tools. A tool derives from
  `AssistantTool<TContext, TArguments>`; the arguments record is exported as the tool's JSON
  schema (every property required, optional arguments nullable, enums as names, `Id<T>` as a
  string) and parsed back from the model's call. Tools take names, or ids that another tool
  returned (never ids from the prompt), and reuse the module's query handlers.
- **Layout**: everything assistant-related sits in its module's `Features/Assistants/`. Each
  assistant has an `Ask<Assistant>/` folder (endpoint, assistant, prompt, context), next to the
  shared `Tools/` and `Scopes/`. Endpoint routes stay under the resource the assistant serves
  (e.g. `/api/<module>/<resource>/assistant/...`).
- **Tool catalogue**: every tool lives in its module's catalogue, `Features/Assistants/Tools/`,
  never inside an assistant's folder, so assistants share tools instead of copying them. A
  tool is typed to the narrowest *scope* it needs (`Features/Assistants/Scopes/`), not to an
  assistant's context. Each assistant's context implements the scopes it supports, and
  because `IAssistantTool<in TContext>` is contravariant, a module registers a tool for an
  assistant with `AddAssistantTool<TContext, TTool>()` in `Build`. `PantheonAssistant`
  receives every tool registered for its context type (`IsAvailable(context)` can hide one,
  e.g. on a page without a table). The registration block per assistant is its tool set; an
  assistant is defined by its context, its prompt and the tools it opts into. Tool
  descriptions and notes never name other tools, since the tools beside them depend on the
  assistant; which tool to prefer belongs in the assistant's prompt. Catalogues are per
  module: tools call their own module's handlers, so there are no cross-module tools.
- **Tool results**: a tool returns an `AssistantToolResult`: `Output` is compact text for the
  model (data, a confirmation, or a validation problem the model can correct), and an
  optional typed `Action` is streamed to the page. Read tools derive from
  `AssistantDataTool<TContext, TArguments, TData>` instead: the tool owns the mapping from its
  query responses to a typed `TData` record, or returns `AssistantData<TData>.WithNote(...)`
  when there is nothing to return (an unknown id, a disabled feature), and the base serializes
  it as compact JSON (camelCase, nulls omitted, enums as names, decimals rounded to four
  significant figures to save tokens). Only the system prompt is composed as text.
- **Tool loop**: the loop is bounded. It streams a round, runs the requested
  tools sequentially, appends each call and its (truncated) output to the input, and streams
  again, up to `MaxToolRounds` (`ModelDefaults`); the last round offers no tools, so the model
  must answer. Only the first `MaxToolCallsPerRound` calls in a round run, and a tool marked
  `IsSingleActionPerReply` is skipped once it has returned an action in the reply; skipped
  calls get an output telling the model why, because small models sometimes repeat a call
  many times. Skipped calls are still streamed (status `Skipped`), so the user sees every call
  the model made. Every round resends the whole input, because OuranosMl does not keep
  response state; the inference server's prompt cache keeps the shared prefix cheap. A
  failing or unknown tool becomes an output the model sees, never a failed turn.
- **Actions**: a tool never changes data itself. Every action appears as a proposal that the
  user applies or rejects: view changes update the page's table state, and data changes go
  through the module's normal update endpoint. Pages can mark a handler `autoApply`, but none
  do. The next turn sees the effect through the context, which is rebuilt from the store or
  the current view, and learns what the user decided through the history (see below).
- **Model settings**: global defaults (model, reasoning effort, max tokens) live in
  `ModelDefaults` (`Shared.Contract/Infra/OuranosMachineLearning/`). Each feature's options
  record initializes from them and overrides only what it needs. Reasoning tokens count
  against `max_output_tokens`, so budgets must leave room for thinking.
- **Context budget**: the self-hosted model's context window is limited (it is configured on
  the inference host and may change) and is shared by the system prompt, the tool
  definitions, the history, tool outputs, the reasoning and the reply; prompt length also
  dominates time to first token. Assistants are therefore page-scoped (one per page, seeing
  only what that page shows, with a small curated tool set rather than every tool in the
  catalogue) and keep a headline baseline in the prompt, with detail behind tools: compact
  numbers (`AssistantNumberFormat`, e.g. `820.6K`, because small models tokenize digits
  almost one by one), a fixed number of rows that does not grow with the selected time frame,
  no ids. Each assistant's prompt tests assert its worst-case prompt plus every tool
  definition against a shared character budget.
- **`MapAssistant<TContext>(pattern)`** (`API/AssistantEndpointExtensions`) maps a POST
  route binding `AssistantCompletionInput<TContext>` (`messages` + typed `context`),
  dispatches via `IMessageBus`, and writes events with `SseWriter`.
- **Event contract**: `$type`-discriminated `instructions` (the composed system prompt; only
  handlers that show it emit it, Hermes does, page assistants do not), `reasoning`,
  `content`, `tool` (a call started:
  `callId`, `name` and the raw JSON `arguments`), `toolResult` (`callId`, `status`
  `Completed | Failed | Skipped`, and the `output` exactly as the model received it, with
  `isTruncated` when it was cut to `MaxToolOutputCharacters`), `action` (a typed action for
  the page, with the `callId` that proposed it, sent between the call and its result),
  `usage` (input, output and total summed over the reply's rounds, plus `contextTokens`, the
  last round's input and output, i.e. how much of the context window the conversation now
  fills), `error`, `done`. Tool events let the user inspect what each call asked and
  returned; they are a debugging view, not data for the page. `reasoning` carries the model's
  thinking and is never resent as history. Errors before the first event surface as
  ProblemDetails; LLM failures mid-stream, and replies that finish without any content
  (e.g. reasoning exhausted the token budget), become an `error` event with a generic
  message.
- **Stateless, trimmed history**: the client resends the whole conversation on every turn;
  nothing is persisted. Assistant messages carry typed `actions` outcomes (`name`, the page's
  `description`, `status` `Pending | Applied | Rejected | Failed`), because pages apply
  actions without telling the server; `PantheonAssistant` renders them as a bracketed note
  after the reply for the model, so it does not claim an ignored change was made or propose a
  rejected one again. Tool calls and outputs of earlier turns are not resent.
  `PantheonAssistant` keeps only the newest messages that fit
  `AssistantPrompt.HistoryCharacterBudget` (default `ModelDefaults.HistoryCharacterBudget`),
  always keeping the newest message and never starting with an assistant turn, so long
  conversations keep fitting the limited context window.

## 8.2 Frontend

`src/apps/interface/src/components/shared/ai-assistant/` provides `AssistantButton` (a
top-bar icon button), `AssistantPanel` (bottom drawer on mobile, non-modal right sheet on
desktop so the page stays usable) and `useAssistantChat`, configured with a typed
`AssistantEndpoint<TContext>` (which binds the context type to the endpoint), a context
object, and a title.

- Optional `suggestions` render as clickable starter questions in the empty state.
- Replies render through `MarkdownRenderer` in its `compact` variant: chat-sized headings and
  spacing, with tables in a horizontally scrolling container.
- Pages place their assistant button in the nav bar through `useNavBarActions`, only while
  the assistant is usable.
- `actions` maps action names to handlers (`describe`, `apply`, `autoApply`) that the page
  owns.
- `useAssistantChat` folds the events into an ordered list of parts per reply
  (`assistant-chat-parts.ts`), and the chat renders them as a transcript: user messages are
  bubbles, and assistant turns are unbubbled blocks in the order they happened.
- Reasoning is a collapsed "Thought for Ns" row. Each tool call is a collapsed row showing
  its raw tool name, status and duration, which expands to its input and output (truncated
  JSON is still indented). The input shows the turn's context (kept on the reply as it was
  sent) beside the model's arguments, since tools also read the context.
- A run of reasoning and tool calls between two pieces of the reply shares one stack; once
  the model has moved past it, a run of four or more rows folds into one "Used N tools" row,
  so the answer stays in view.
- Each action is a card with Apply and Reject (Retry after a failure, and its call's details
  on demand), and a reply with several open proposals offers Apply all, which applies them
  one after another because handlers save the latest state.
- Hermes renders through the same `Message`: its controller folds the same events into
  parts with `assistant-chat-parts.ts`, and saved replies render from their stored parts.
  Older Hermes messages without parts fall back to `<think>` tag parsing.
