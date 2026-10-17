export type AssistantRole = "User" | "Assistant";

export type AssistantEndpoint<TContext> = string & {
  readonly __context?: TContext;
};

export type AssistantActionStatus = "Pending" | "Applied" | "Rejected" | "Failed";

export type AssistantActionOutcome = {
  name: string;
  description: string;
  status: AssistantActionStatus;
};

export type AssistantMessageInput = {
  role: AssistantRole;
  content: string;
  actions?: AssistantActionOutcome[];
};

export type AssistantToolStatus = "Completed" | "Failed" | "Skipped";

export type AssistantImage = {
  id: string;
  url: string;
  width: number;
  height: number;
  altText: string;
};

export type AssistantCompletionInput<TContext> = {
  messages: AssistantMessageInput[];
  context: TContext;
};

export type AssistantEvent =
  | { $type: "instructions"; instructions: string }
  | { $type: "reasoning"; content: string }
  | { $type: "content"; content: string }
  | { $type: "tool"; callId: string; name: string; arguments: string }
  | {
      $type: "toolResult";
      callId: string;
      status: AssistantToolStatus;
      output: string;
      isTruncated: boolean;
      durationMs: number;
    }
  | { $type: "action"; callId: string; name: string; action: unknown }
  | { $type: "image"; callId: string; image: AssistantImage }
  | {
      $type: "usage";
      inputTokens: number;
      outputTokens: number;
      totalTokens: number;
      contextTokens: number;
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
