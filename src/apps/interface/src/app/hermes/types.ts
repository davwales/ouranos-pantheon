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
  repeatPenalty?: number | null;
  contextWindow?: number | null;
  isDefault: boolean;
  isPublic: boolean;
}

export interface TraitFormInput {
  id?: string;
  name: string;
  content: string;
  isPublic: boolean;
  isEphemeral?: boolean;
}

export interface ConversationConfig {
  persona: PersonaFormInput;
  model: ModelFormInput;
}
