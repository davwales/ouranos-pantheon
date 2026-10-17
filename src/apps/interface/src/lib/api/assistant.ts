import { streamSse } from "@/lib/api-client";
import type {
  AssistantCompletionInput,
  AssistantEndpoint,
  AssistantEvent,
} from "./assistant-types";

export type {
  AssistantActionHandler,
  AssistantActionHandlers,
  AssistantActionOutcome,
  AssistantActionStatus,
  AssistantCompletionInput,
  AssistantEndpoint,
  AssistantEvent,
  AssistantImage,
  AssistantMessageInput,
  AssistantRole,
  AssistantToolStatus,
} from "./assistant-types";

export function streamAssistant<TContext>(
  endpoint: AssistantEndpoint<TContext>,
  input: AssistantCompletionInput<TContext>,
  signal?: AbortSignal,
): AsyncGenerator<AssistantEvent> {
  return streamSse<AssistantEvent>(endpoint, input, signal);
}
