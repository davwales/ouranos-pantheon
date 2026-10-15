export type AssistantRole = "User" | "Assistant";

export type AssistantEndpoint<TContext> = string & {
  readonly __context?: TContext;
};

export type AssistantMessageInput = {
  role: AssistantRole;
  content: string;
};

export type AssistantCompletionInput<TContext> = {
  messages: AssistantMessageInput[];
  context: TContext;
};

export type AssistantEvent =
  | { $type: "reasoning"; content: string }
  | { $type: "content"; content: string }
  | { $type: "tool"; name: string }
  | { $type: "action"; name: string; action: unknown }
  | {
      $type: "usage";
      inputTokens: number;
      outputTokens: number;
      totalTokens: number;
    }
  | { $type: "error"; message: string }
  | { $type: "done" };

export type AssistantActionHandler<TAction> = {
  describe(action: TAction): string;
  apply(action: TAction): Promise<void> | void;
  autoApply?: boolean;
};

export type AssistantActionHandlers = Record<
  string,
  AssistantActionHandler<unknown>
>;
