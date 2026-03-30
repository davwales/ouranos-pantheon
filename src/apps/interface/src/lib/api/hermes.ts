import { api } from "@/lib/api-client";

const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE ??
  process.env.NEXT_PUBLIC_API_HOST ??
  "http://localhost:8300";

export interface Persona {
  id: string;
  name: string;
  description: string;
  personality?: string | null;
  scenario?: string | null;
  isDefault: boolean;
  isPublic: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface ModelConfig {
  id: string;
  name: string;
  modelIdentifier: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
  repeatPenalty?: number | null;
  isDefault: boolean;
  isPublic: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface MessageInput {
  role: Role;
  content: string;
}

export interface PersonaInput {
  name: string;
  description: string;
  personality?: string | null;
  scenario?: string | null;
}

export interface ModelInput {
  modelIdentifier: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
  repeatPenalty?: number | null;
}

export interface TraitInput {
  name: string;
  content: string;
}

export interface ConversationInput {
  model: ModelInput;
  persona: PersonaInput;
  messages: MessageInput[];
  traits?: TraitInput[];
}

export interface GenerateCompletionInput {
  conversation: ConversationInput;
  conversationId?: string;
}

export interface ConversationSummary {
  id: string;
  name: string;
  isPublic: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SavedConversationPersona {
  id: string;
  name: string;
  description: string;
  personality?: string | null;
  scenario?: string | null;
}

export interface SavedConversationModel {
  id: string;
  name: string;
  modelIdentifier: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
  repeatPenalty?: number | null;
}

export interface SavedConversationTrait {
  id: string;
  name: string;
  content: string;
}

export interface SavedConversationMessage {
  content: string;
  role: Role;
}

export interface SavedConversation {
  id: string;
  name: string;
  isPublic: boolean;
  persona: SavedConversationPersona;
  model: SavedConversationModel;
  traits: SavedConversationTrait[];
  messages: SavedConversationMessage[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateConversationInput {
  personaId: string;
  modelConfigId: string;
  traitIds: string[];
  messages: MessageInput[];
  name?: string;
  isPublic?: boolean;
}

export interface CompletionChunk {
  content: string;
}

export enum Role {
  System = "System",
  User = "User",
  Assistant = "Assistant",
}

export interface Trait {
  id: string;
  name: string;
  content: string;
  isPublic: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export const hermesApi = {
  getAllPersonas: (params?: { filter?: string[] }) =>
    api.get<Persona[]>("/api/hermes/personas", params),

  getPersona: (personaId: string) =>
    api.get<Persona>(`/api/hermes/personas/${personaId}`),

  createPersona: (input: Omit<Persona, "id" | "createdAt" | "updatedAt">) =>
    api.post<{ id: string }>("/api/hermes/personas", input),

  updatePersona: (input: {
    personaId: string;
    name: string;
    description: string;
    personality?: string | null;
    scenario?: string | null;
    isDefault: boolean;
    isPublic: boolean;
  }) =>
    api.put<{ id: string }>(`/api/hermes/personas/${input.personaId}`, input),

  deletePersona: (personaId: string) =>
    api.del<{ id: string }>(`/api/hermes/personas/${personaId}`),

  getAllModels: (params?: { filter?: string[] }) =>
    api.get<ModelConfig[]>("/api/hermes/models", params),

  getModel: (modelId: string) =>
    api.get<ModelConfig>(`/api/hermes/models/${modelId}`),

  createModel: (input: Omit<ModelConfig, "id" | "createdAt" | "updatedAt">) =>
    api.post<{ id: string }>("/api/hermes/models", input),

  updateModel: (input: {
    modelId: string;
    name: string;
    modelIdentifier: string;
    systemPrompt: string;
    temperature?: number | null;
    maxTokens?: number | null;
    repeatPenalty?: number | null;
    isDefault: boolean;
    isPublic: boolean;
  }) => api.put<{ id: string }>(`/api/hermes/models/${input.modelId}`, input),

  deleteModel: (modelId: string) =>
    api.del<{ id: string }>(`/api/hermes/models/${modelId}`),

  getAllTraits: (params?: { filter?: string[] }) =>
    api.get<Trait[]>("/api/hermes/traits", params),

  getTrait: (traitId: string) =>
    api.get<Trait>(`/api/hermes/traits/${traitId}`),

  createTrait: (input: Omit<Trait, "id" | "createdAt" | "updatedAt">) =>
    api.post<{ id: string }>("/api/hermes/traits", input),

  updateTrait: (input: {
    traitId: string;
    name: string;
    content: string;
    isPublic: boolean;
  }) => api.put<{ id: string }>(`/api/hermes/traits/${input.traitId}`, input),

  deleteTrait: (traitId: string) =>
    api.del<{ id: string }>(`/api/hermes/traits/${traitId}`),

  getAllConversations: () =>
    api.get<ConversationSummary[]>("/api/hermes/conversations"),

  getConversation: (id: string) =>
    api.get<SavedConversation>(`/api/hermes/conversations/${id}`),

  createConversation: (input: CreateConversationInput) =>
    api.post<{ id: string; name: string }>("/api/hermes/conversations", input),

  updateConversation: (
    id: string,
    input: {
      name: string;
      personaId: string;
      modelConfigId: string;
      traitIds: string[];
      messages: MessageInput[];
      isPublic: boolean;
    },
  ) => api.put<{ id: string }>(`/api/hermes/conversations/${id}`, input),

  deleteConversation: (id: string) =>
    api.del<{ id: string }>(`/api/hermes/conversations/${id}`),
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
      body: JSON.stringify({
        conversation: input.conversation,
        ...(input.conversationId
          ? { conversationId: input.conversationId }
          : {}),
      }),
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
