import { api, streamSse } from "@/lib/api-client";

export type {
  Persona,
  ModelConfig,
  AvailableModel,
  MessageInput,
  PersonaInput,
  ModelInput,
  TraitInput,
  ConversationInput,
  GenerateCompletionInput,
  ConversationSummary,
  SavedConversationPersona,
  SavedConversationModel,
  SavedConversationTrait,
  SavedConversationMessage,
  SavedConversation,
  CompactConversationInput,
  CompactChunk,
  CreateConversationInput,
  CompletionChunk,
  Trait,
} from "./hermes-types";
export { Role } from "./hermes-types";

import type {
  Persona,
  ModelConfig,
  AvailableModel,
  MessageInput,
  PersonaInput,
  ConversationInput,
  GenerateCompletionInput,
  ConversationSummary,
  SavedConversation,
  CreateConversationInput,
  CompactConversationInput,
  Trait,
} from "./hermes-types";
import type { CompletionChunk, CompactChunk } from "./hermes-types";

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

  getAvailableModels: () =>
    api.get<AvailableModel[]>("/api/hermes/available-models"),

  getModel: (modelId: string) =>
    api.get<ModelConfig>(`/api/hermes/models/${modelId}`),

  createModel: (input: Omit<ModelConfig, "id" | "createdAt" | "updatedAt" | "isUnavailable">) =>
    api.post<{ id: string }>("/api/hermes/models", input),

  updateModel: (input: {
    modelId: string;
    name: string;
    modelIdentifier: string;
    systemPrompt: string;
    temperature?: number | null;
    maxTokens?: number | null;
    repeatPenalty?: number | null;
    contextWindow?: number | null;
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
  yield* streamSse<CompletionChunk>(
    "/api/hermes/conversations/completions/stream",
    {
      conversation: input.conversation,
      ...(input.conversationId ? { conversationId: input.conversationId } : {}),
    },
    signal,
  );
}

export async function* streamCompact(
  input: CompactConversationInput,
  signal?: AbortSignal,
): AsyncGenerator<CompactChunk> {
  yield* streamSse<CompactChunk>(
    "/api/hermes/conversations/compact",
    input,
    signal,
  );
}