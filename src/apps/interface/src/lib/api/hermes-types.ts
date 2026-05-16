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

export interface MessageInput {
  role: Role;
  content: string;
  sortOrder?: number;
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
  contextWindow?: number | null;
}

export interface SavedConversationTrait {
  id: string;
  name: string;
  content: string;
}

export interface SavedConversationMessage {
  content: string;
  role: Role;
  sortOrder: number;
}

export interface SavedConversation {
  id: string;
  name: string;
  isPublic: boolean;
  persona: SavedConversationPersona;
  model: SavedConversationModel;
  traits: SavedConversationTrait[];
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
  inputTokenCount?: number | null;
  outputTokenCount?: number | null;
  totalTokenCount?: number | null;
}

export type CompletionChunk =
  | { $type: "systemPrompt"; systemPrompt: string }
  | { $type: "content"; content: string }
  | {
      $type: "usage";
      inputTokens: number;
      outputTokens: number;
      totalTokens: number;
    };

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