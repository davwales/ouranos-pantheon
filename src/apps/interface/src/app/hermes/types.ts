export interface PersonaFormInput {
  id?: string;
  name: string;
  description: string;
  personality?: string | null;
  scenario?: string | null;
  isDefault: boolean;
}

export interface ModelFormInput {
  id?: string;
  name: string;
  modelIdentifier: string;
  systemPrompt: string;
  temperature?: number | null;
  maxTokens?: number | null;
  repeatPenalty?: number | null;
  isDefault: boolean;
}

export interface ConversationConfig {
  persona: PersonaFormInput;
  model: ModelFormInput;
}
