export interface AssistantFormInput {
  id?: string;
  model: string;
  systemPrompt: string;
  assistantName: string;
  userName: string;
  temperature?: number | null | undefined;
  maxTokens?: number | null | undefined;
  repeatPenalty?: number | null | undefined;
}

export default interface ConversationAssistant {
  id?: string;
  model: string;
  systemPrompt: string;
  assistantName: string;
  userName: string;
  temperature?: number | null | undefined;
  maxTokens?: number | null | undefined;
  repeatPenalty?: number | null | undefined;
}
