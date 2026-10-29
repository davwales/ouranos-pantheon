import { type ToolSource } from "@/lib/api/hermes";

export interface PersonaFormInput {
  id?: string;
  name: string;
  description: string;
  personality?: string | null;
  scenario?: string | null;
  isDefault: boolean;
  isPublic: boolean;
}

export interface ModelFormInput {
  id?: string;
  name: string;
  modelIdentifier: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
  defaultToolSetId: string | null;
  contextWindow?: number | null;
  isDefault: boolean;
  isPublic: boolean;
  isUnavailable?: boolean;
}

export interface TraitFormInput {
  id?: string;
  name: string;
  content: string;
  isPublic: boolean;
  isEphemeral?: boolean;
}

export type LoraFormInput = {
  id?: string;
  name: string;
  identifier: string;
  weight: number;
  isPublic: boolean;
  triggerPhrase?: string | null;
};

export type ToolSetToolFormInput = {
  source: ToolSource;
  name: string;
  additionalDescription: string;
};

export type ToolSetFormInput = {
  name: string;
  description: string;
  instructions: string;
  tools: ToolSetToolFormInput[];
  isPublic: boolean;
};

export interface ConversationConfig {
  persona: PersonaFormInput;
  model: ModelFormInput;
}
