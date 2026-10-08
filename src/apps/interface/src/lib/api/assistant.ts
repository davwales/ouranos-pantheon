import { streamSse } from "@/lib/api-client";
import type {
  AssistantCompletionInput,
  AssistantEndpoint,
  AssistantEvent,
} from "./assistant-types";

export type {
  AssistantCompletionInput,
  AssistantEndpoint,
  AssistantEvent,
  AssistantMessageInput,
  AssistantRole,
} from "./assistant-types";

export function streamAssistant<TContext>(
  endpoint: AssistantEndpoint<TContext>,
  input: AssistantCompletionInput<TContext>,
  signal?: AbortSignal,
): AsyncGenerator<AssistantEvent> {
  return streamSse<AssistantEvent>(endpoint, input, signal);
}
