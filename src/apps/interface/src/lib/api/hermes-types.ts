import { type AssistantEvent, type AssistantToolStatus } from "@/lib/api/assistant-types";

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
  isToolCallingEnabled: boolean;
  contextWindow?: number | null;
  isDefault: boolean;
  isPublic: boolean;
  isUnavailable: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface AvailableModel {
  id: string;
  modelIdentifier: string;
  ownedBy: string;
}

export type ReasoningEffort = "None" | "Low" | "Medium" | "High";

export type MessagePart =
  | { $type: "reasoning"; content: string }
  | { $type: "text"; content: string }
  | {
      $type: "toolCall";
      callId: string;
      name: string;
      arguments: string;
      status: AssistantToolStatus;
      output: string;
      isTruncated: boolean;
      durationMs?: number | null;
    }
  | { $type: "image"; imageId: string; width: number; height: number; altText: string };

export interface MessageInput {
  role: Role;
  content: string;
  sortOrder?: number;
  parts?: MessagePart[];
}

export type CompletionToolCallInput = {
  callId: string;
  name: string;
  arguments: string;
  output: string;
};

export type CompletionMessageInput = {
  role: Role;
  content: string;
  toolCalls?: CompletionToolCallInput[];
};

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
  isToolCallingEnabled: boolean;
}

export interface TraitInput {
  name: string;
  content: string;
}

export type LoraInput = {
  identifier: string;
  weight: number;
};

export interface ConversationInput {
  model: ModelInput;
  persona: PersonaInput;
  messages: CompletionMessageInput[];
  traits?: TraitInput[];
  reasoningEffort?: ReasoningEffort | null;
  loras?: LoraInput[];
}

export interface GenerateCompletionInput {
  conversation: ConversationInput;
  conversationId?: string;
}

export type FolderSummary = {
  id: string;
  name: string;
  isPublic: boolean;
  parentFolderId: string | null;
  conversationCount: number;
  subfolderCount: number;
  createdAt: string;
  updatedAt: string;
};

export interface ConversationSummary {
  id: string;
  name: string;
  isPublic: boolean;
  folderId: string | null;
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
  isToolCallingEnabled: boolean;
  contextWindow?: number | null;
}

export interface SavedConversationTrait {
  id: string;
  name: string;
  content: string;
}

export type SavedConversationLora = {
  id: string;
  name: string;
  identifier: string;
  weight: number;
};

export interface SavedConversationMessage {
  content: string;
  role: Role;
  sortOrder: number;
  parts: MessagePart[];
}

export interface SavedConversation {
  id: string;
  name: string;
  isPublic: boolean;
  folderId: string | null;
  reasoningEffort: ReasoningEffort | null;
  persona: SavedConversationPersona;
  model: SavedConversationModel;
  traits: SavedConversationTrait[];
  loras: SavedConversationLora[];
  messages: SavedConversationMessage[];
  tokenUsage?: {
    inputTokens: number;
    outputTokens: number;
    totalTokens: number;
  } | null;
  createdAt: string;
  updatedAt: string;
}

export interface CompactConversationInput {
  conversationId?: string;
  modelIdentifier: string;
  systemPrompt: string;
  personaName: string;
  personaDescription: string;
  messages: MessageInput[];
}

export type CompactChunk =
  | { $type: "content"; content: string }
  | {
      $type: "usage";
      inputTokens: number;
      outputTokens: number;
      totalTokens: number;
    }
  | {
      $type: "complete";
      summaryMessageId: string | null;
    };

export interface CreateConversationInput {
  personaId: string;
  modelConfigId: string;
  traitIds: string[];
  messages: MessageInput[];
  name?: string;
  isPublic?: boolean;
  folderId?: string | null;
  inputTokenCount?: number | null;
  outputTokenCount?: number | null;
  totalTokenCount?: number | null;
  reasoningEffort?: ReasoningEffort | null;
  loraIds?: string[];
}

export type UpdateConversationInput = {
  name: string;
  personaId: string;
  modelConfigId: string;
  traitIds: string[];
  isPublic: boolean;
  folderId: string | null;
  messages?: MessageInput[];
  reasoningEffort?: ReasoningEffort | null;
  loraIds?: string[];
};

export type CompletionChunk = AssistantEvent;

export interface UpdateFolderInput {
  name: string;
  isPublic: boolean;
  parentFolderId?: string | null;
}

export enum Role {
  System = "System",
  User = "User",
  Assistant = "Assistant",
  Summary = "Summary",
}

export interface Trait {
  id: string;
  name: string;
  content: string;
  isPublic: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export type Lora = {
  id: string;
  name: string;
  identifier: string;
  weight: number;
  isPublic: boolean;
};
