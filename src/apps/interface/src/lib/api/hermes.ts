import { api } from "@/lib/api-client";

const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE ??
  process.env.NEXT_PUBLIC_API_HOST ??
  "http://localhost:8300";

export interface Assistant {
  id: string;
  model: string;
  systemPrompt: string;
  assistantName: string;
  userName: string;
  temperature?: number | null;
  maxTokens?: number | null;
  repeatPenalty?: number | null;
  createdAt?: string;
  updatedAt?: string;
}

export interface MessageInput {
  role: Role;
  content: string;
}

export interface AssistantInput {
  model: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
  repeatPenalty?: number | null;
}

export interface ConversationInput {
  assistant: AssistantInput;
  messages: MessageInput[];
}

export interface GenerateCompletionInput {
  conversation: ConversationInput;
}

export interface CompletionChunk {
  content: string;
}

export enum Role {
  System = "SYSTEM",
  User = "USER",
  Assistant = "ASSISTANT",
}

export const hermesApi = {
  getAllAssistants: (params?: { filter?: string[] }) =>
    api.get<Assistant[]>("/api/hermes/assistants", params),

  getAssistant: (assistantId: string) =>
    api.get<Assistant>(`/api/hermes/assistants/${assistantId}`),

  createAssistant: (input: Omit<Assistant, "id" | "createdAt" | "updatedAt">) =>
    api.post<{ id: string }>("/api/hermes/assistants", input),

  updateAssistant: (input: {
    assistantId: string;
    model: string;
    systemPrompt: string;
    assistantName?: string | null;
    userName?: string | null;
    temperature?: number | null;
    maxTokens?: number | null;
    repeatPenalty?: number | null;
  }) =>
    api.put<{ id: string }>(
      `/api/hermes/assistants/${input.assistantId}`,
      input,
    ),

  deleteAssistant: (assistantId: string) =>
    api.del<{ id: string }>(`/api/hermes/assistants/${assistantId}`),
};

export async function* streamCompletion(
  input: GenerateCompletionInput,
  signal?: AbortSignal,
): AsyncGenerator<CompletionChunk> {
  const res = await fetch(
    `${API_BASE}/api/hermes/conversations/completions/stream`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(input),
      signal,
    },
  );

  if (!res.ok || !res.body) {
    throw new Error(`Streaming failed: ${res.statusText}`);
  }

  const reader = res.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;
    buffer += decoder.decode(value, { stream: true });
    const lines = buffer.split("\n\n");
    buffer = lines.pop() ?? "";
    for (const line of lines) {
      if (line.startsWith("data: ")) {
        yield JSON.parse(line.slice(6)) as CompletionChunk;
      }
    }
  }
}
