import { type AssistantEvent, type AssistantToolStatus } from "@/lib/api/assistant-types";

export type Persona = {
  id: string;
  name: string;
  description: string;
  personality?: string | null;
  scenario?: string | null;
  isDefault: boolean;
  isPublic: boolean;
  createdAt?: string;
  updatedAt?: string;
};

export type ModelConfig = {
  id: string;
  name: string;
  modelIdentifier: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
  defaultToolSetId: string | null;
  contextWindow?: number | null;
  isDefault: boolean;
  isPublic: boolean;
  isUnavailable: boolean;
  createdAt?: string;
  updatedAt?: string;
};

export type AvailableModel = {
  id: string;
  modelIdentifier: string;
  ownedBy: string;
};

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
  | ImageMessagePart;

export type ImageMessagePart = {
  $type: "image";
  imageId: string;
  width: number;
  height: number;
  altText: string;
};

export type MessageInput = {
  role: Role;
  content: string;
  sortOrder?: number;
  parts?: MessagePart[];
};

export type CompletionToolCallInput = {
  callId: string;
  name: string;
  arguments: string;
  output: string;
  imageId?: string;
};

export type CompletionMessageInput = {
  role: Role;
  content: string;
  toolCalls?: CompletionToolCallInput[];
  images?: ImageMessagePart[];
};

export type ImageDetails = {
  id: string;
  width: number;
  height: number;
  prompt: string;
  model: string;
  seed: number | null;
  sourceImageId: string | null;
  createdAt: string;
};

export type PersonaInput = {
  name: string;
  description: string;
  personality?: string | null;
  scenario?: string | null;
};

export type ModelInput = {
  modelIdentifier: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
};

export type TraitInput = {
  name: string;
  content: string;
};

export type ConversationInput = {
  model: ModelInput;
  persona: PersonaInput;
  messages: CompletionMessageInput[];
  traits?: TraitInput[];
  reasoningEffort?: ReasoningEffort | null;
  loraIds?: string[];
  toolSetId?: string | null;
};

export type GenerateCompletionInput = {
  conversation: ConversationInput;
  conversationId?: string;
};

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

export type ConversationSummary = {
  id: string;
  name: string;
  isPublic: boolean;
  folderId: string | null;
  createdAt: string;
  updatedAt: string;
};

export type SavedConversationPersona = {
  id: string;
  name: string;
  description: string;
  personality?: string | null;
  scenario?: string | null;
};

export type SavedConversationModel = {
  id: string;
  name: string;
  modelIdentifier: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
  defaultToolSetId: string | null;
  contextWindow?: number | null;
};

export type SavedConversationTrait = {
  id: string;
  name: string;
  content: string;
};

export type SavedConversationLora = {
  id: string;
  name: string;
  identifier: string;
  weight: number;
};

export type SavedConversationMessage = {
  content: string;
  role: Role;
  sortOrder: number;
  parts: MessagePart[];
};

export type SavedConversation = {
  id: string;
  name: string;
  isPublic: boolean;
  folderId: string | null;
  reasoningEffort: ReasoningEffort | null;
  toolSetId: string | null;
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
};

export type CompactConversationInput = {
  conversationId?: string;
  modelIdentifier: string;
  personaName: string;
  personaDescription: string;
  messages: MessageInput[];
};

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
    }
  | { $type: "error"; message: string };

export type CreateConversationInput = {
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
  toolSetId?: string | null;
};

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
  toolSetId: string | null;
};

export type CompletionChunk = AssistantEvent;

export type PublicMode = {
  isEnabled: boolean;
};

export type UpdateFolderInput = {
  name: string;
  isPublic: boolean;
  parentFolderId?: string | null;
};

export enum Role {
  System = "System",
  User = "User",
  Assistant = "Assistant",
  Summary = "Summary",
}

export type Trait = {
  id: string;
  name: string;
  content: string;
  isPublic: boolean;
  createdAt?: string;
  updatedAt?: string;
};

export type Lora = {
  id: string;
  name: string;
  identifier: string;
  weight: number;
  isPublic: boolean;
  triggerPhrase: string | null;
  isUnavailable: boolean;
  isIncompatible: boolean;
};

export type SaveLoraInput = Pick<
  Lora,
  "name" | "identifier" | "weight" | "isPublic" | "triggerPhrase"
>;

export type AvailableLora = {
  id: string;
  identifier: string;
  title: string | null;
  compatClass: string | null;
  defaultWeight: number | null;
  triggerPhrase: string | null;
  usageHint: string | null;
  isCompatible: boolean;
};

export type ToolSource = "Native";

export type AvailableTool = {
  source: ToolSource;
  name: string;
  description: string;
};

export type ToolSetToolInput = {
  source: ToolSource;
  name: string;
  additionalDescription?: string | null;
};

export type ToolSetTool = {
  source: ToolSource;
  name: string;
  additionalDescription: string | null;
  isAvailable: boolean;
};

export type ToolSetSummary = {
  id: string;
  name: string;
  description: string | null;
  tools: ToolSetTool[];
  isPublic: boolean;
};

export type ToolSet = ToolSetSummary & {
  instructions: string | null;
};

export type SaveToolSetInput = {
  name: string;
  description?: string | null;
  instructions?: string | null;
  tools: ToolSetToolInput[];
  isPublic: boolean;
};
