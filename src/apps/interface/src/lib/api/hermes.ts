import { api, streamSse } from "@/lib/api-client";

export type {
  Persona,
  ModelConfig,
  AvailableModel,
  MessageInput,
  MessagePart,
  ReasoningEffort,
  CompletionMessageInput,
  CompletionToolCallInput,
  PersonaInput,
  ModelInput,
  TraitInput,
  LoraInput,
  ConversationInput,
  GenerateCompletionInput,
  ConversationSummary,
  SavedConversationPersona,
  SavedConversationModel,
  SavedConversationTrait,
  SavedConversationLora,
  SavedConversationMessage,
  SavedConversation,
  CompactConversationInput,
  CompactChunk,
  CreateConversationInput,
  UpdateConversationInput,
  CompletionChunk,
  Trait,
  Lora,
  ToolSource,
  AvailableTool,
  ToolSetToolInput,
  ToolSetTool,
  ToolSetSummary,
  ToolSet,
  SaveToolSetInput,
  FolderSummary,
  UpdateFolderInput,
} from "./hermes-types";
export { Role } from "./hermes-types";

import type {
  Persona,
  ModelConfig,
  AvailableModel,
  MessageInput,
  ReasoningEffort,
  PersonaInput,
  ConversationInput,
  GenerateCompletionInput,
  ConversationSummary,
  SavedConversation,
  CreateConversationInput,
  UpdateConversationInput,
  CompactConversationInput,
  Trait,
  Lora,
  AvailableTool,
  ToolSetSummary,
  ToolSet,
  SaveToolSetInput,
  FolderSummary,
  UpdateFolderInput,
} from "./hermes-types";
import type { CompletionChunk, CompactChunk } from "./hermes-types";

export function hermesImagePath(imageId: string): string {
  return `/api/hermes/images/${encodeURIComponent(imageId)}`;
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

  getAvailableModels: () =>
    api.get<AvailableModel[]>("/api/hermes/available-models"),

  getModel: (modelId: string) =>
    api.get<ModelConfig>(`/api/hermes/models/${modelId}`),

  createModel: (
    input: Omit<
      ModelConfig,
      "id" | "createdAt" | "updatedAt" | "isUnavailable"
    >,
  ) => api.post<{ id: string }>("/api/hermes/models", input),

  updateModel: (input: {
    modelId: string;
    name: string;
    modelIdentifier: string;
    systemPrompt: string;
    temperature?: number | null;
    maxTokens?: number | null;
    defaultToolSetId: string | null;
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

  getAllLoras: (params?: { filter?: string[] }) =>
    api.get<Lora[]>("/api/hermes/loras", params),

  getLora: (loraId: string) => api.get<Lora>(`/api/hermes/loras/${loraId}`),

  createLora: (input: Omit<Lora, "id">) =>
    api.post<{ loraId: string }>("/api/hermes/loras", input),

  updateLora: (loraId: string, input: Omit<Lora, "id">) =>
    api.put<{ loraId: string }>(`/api/hermes/loras/${loraId}`, input),

  deleteLora: (loraId: string) =>
    api.del<{ loraId: string }>(`/api/hermes/loras/${loraId}`),

  getAvailableTools: () => api.get<AvailableTool[]>("/api/hermes/tools"),

  getAllToolSets: (params?: { filter?: string[] }) =>
    api.get<ToolSetSummary[]>("/api/hermes/tool-sets", params),

  getToolSet: (toolSetId: string) =>
    api.get<ToolSet>(`/api/hermes/tool-sets/${toolSetId}`),

  createToolSet: (input: SaveToolSetInput) =>
    api.post<{ toolSetId: string }>("/api/hermes/tool-sets", input),

  updateToolSet: (toolSetId: string, input: SaveToolSetInput) =>
    api.put<{ toolSetId: string }>(`/api/hermes/tool-sets/${toolSetId}`, input),

  deleteToolSet: (toolSetId: string) =>
    api.del<{ toolSetId: string }>(`/api/hermes/tool-sets/${toolSetId}`),

  getAllConversations: (params?: { folderId?: string }) =>
    api.get<ConversationSummary[]>("/api/hermes/conversations", {
      folderId: params?.folderId,
    }),

  getConversation: (id: string) =>
    api.get<SavedConversation>(`/api/hermes/conversations/${id}`),

  createConversation: (input: CreateConversationInput) =>
    api.post<{ id: string; name: string }>("/api/hermes/conversations", input),

  updateConversation: (id: string, input: UpdateConversationInput) =>
    api.put<{ id: string }>(`/api/hermes/conversations/${id}`, input),

  deleteConversation: (id: string) =>
    api.del<{ id: string }>(`/api/hermes/conversations/${id}`),

  createFolder: (input: {
    name: string;
    isPublic: boolean;
    parentFolderId?: string | null;
  }) => api.post<{ id: string }>("/api/hermes/folders", input),

  getAllFolders: (params?: {
    parentFolderId?: string | null;
    sortField?: string;
    sortDirection?: string;
    filter?: string[];
  }) => api.get<FolderSummary[]>("/api/hermes/folders", params),

  getFolder: (folderId: string) =>
    api.get<FolderSummary>(`/api/hermes/folders/${folderId}`),

  updateFolder: (folderId: string, input: UpdateFolderInput) =>
    api.put<{ id: string }>(`/api/hermes/folders/${folderId}`, input),

  deleteFolder: (folderId: string) =>
    api.del<{ id: string }>(`/api/hermes/folders/${folderId}`),
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
