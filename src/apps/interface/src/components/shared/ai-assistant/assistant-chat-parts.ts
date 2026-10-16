import {
  type AssistantActionStatus,
  type AssistantEvent,
  type AssistantToolStatus,
} from "@/lib/api/assistant";

export type AssistantChatToolStatus =
  | "running"
  | "completed"
  | "failed"
  | "skipped"
  | "cancelled";

export type AssistantChatActionStatus =
  | "proposed"
  | "applying"
  | "applied"
  | "rejected"
  | "failed";

export type AssistantChatReasoningPart = {
  kind: "reasoning";
  id: string;
  content: string;
  startedAt?: number;
  endedAt?: number;
};

export type AssistantChatTextPart = {
  kind: "text";
  id: string;
  content: string;
};

export type AssistantChatToolPart = {
  kind: "tool";
  id: string;
  callId: string;
  name: string;
  arguments: string;
  status: AssistantChatToolStatus;
  output?: string;
  isTruncated?: boolean;
  startedAt: number;
  endedAt?: number;
};

export type AssistantChatActionPart = {
  kind: "action";
  id: string;
  callId: string;
  name: string;
  action: unknown;
  status: AssistantChatActionStatus;
  error?: string;
  arguments: string;
  output?: string;
  isTruncated?: boolean;
};

export type AssistantChatPart =
  | AssistantChatReasoningPart
  | AssistantChatTextPart
  | AssistantChatToolPart
  | AssistantChatActionPart;

export type StreamedAssistantEvent = Extract<
  AssistantEvent,
  { $type: "reasoning" | "content" | "tool" | "toolResult" | "action" }
>;

const TOOL_STATUSES: Record<AssistantToolStatus, AssistantChatToolStatus> = {
  Completed: "completed",
  Failed: "failed",
  Skipped: "skipped",
};

export function applyEvent(
  parts: AssistantChatPart[],
  event: StreamedAssistantEvent,
  now: number,
  id: string,
): AssistantChatPart[] {
  switch (event.$type) {
    case "reasoning": {
      const last = parts.at(-1);
      if (last?.kind === "reasoning" && last.endedAt === undefined) {
        return [
          ...parts.slice(0, -1),
          { ...last, content: last.content + event.content },
        ];
      }
      return [
        ...closeReasoning(parts, now),
        {
          kind: "reasoning",
          id,
          content: event.content,
          startedAt: now,
        },
      ];
    }
    case "content": {
      const closed = closeReasoning(parts, now);
      const last = closed.at(-1);
      if (last?.kind === "text") {
        return [
          ...closed.slice(0, -1),
          { ...last, content: last.content + event.content },
        ];
      }
      return [
        ...closed,
        { kind: "text", id, content: event.content },
      ];
    }
    case "tool":
      return [
        ...closeReasoning(parts, now),
        {
          kind: "tool",
          id,
          callId: event.callId,
          name: event.name,
          arguments: event.arguments,
          status: "running",
          startedAt: now,
        },
      ];
    case "action": {
      const index = findCall(parts, event.callId);
      const call = index === -1 ? undefined : parts[index];
      const action: AssistantChatActionPart = {
        kind: "action",
        id,
        callId: event.callId,
        name: event.name,
        action: event.action,
        status: "proposed",
        arguments: call?.kind === "tool" || call?.kind === "action" ? call.arguments : "",
      };
      return index === -1
        ? [...closeReasoning(parts, now), action]
        : parts.map((part, i) => (i === index ? action : part));
    }
    case "toolResult": {
      const index = findCall(parts, event.callId);
      return parts.map((part, i) => {
        if (i !== index) {
          return part;
        }
        if (part.kind === "action") {
          return { ...part, output: event.output, isTruncated: event.isTruncated };
        }
        if (part.kind === "tool") {
          return {
            ...part,
            status: TOOL_STATUSES[event.status],
            output: event.output,
            isTruncated: event.isTruncated,
            endedAt: now,
          };
        }
        return part;
      });
    }
  }
}

export function finishParts(
  parts: AssistantChatPart[],
  now: number,
): AssistantChatPart[] {
  return closeReasoning(parts, now).map((part) =>
    part.kind === "tool" && part.status === "running"
      ? { ...part, status: "cancelled", endedAt: now }
      : part,
  );
}

export function textOf(parts: AssistantChatPart[]): string {
  return parts
    .filter((part) => part.kind === "text")
    .map((part) => part.content)
    .join("\n\n");
}

export function actionOutcome(status: AssistantChatActionStatus): AssistantActionStatus {
  switch (status) {
    case "applied":
      return "Applied";
    case "rejected":
      return "Rejected";
    case "failed":
      return "Failed";
    case "proposed":
    case "applying":
      return "Pending";
  }
}

// Some models reuse call ids across rounds, so the newest call with the id is the one that
// is still waiting for its outcome.
function findCall(parts: AssistantChatPart[], callId: string): number {
  return parts.findLastIndex(
    (part) => (part.kind === "tool" || part.kind === "action") && part.callId === callId,
  );
}

function closeReasoning(
  parts: AssistantChatPart[],
  now: number,
): AssistantChatPart[] {
  const last = parts.at(-1);
  return last?.kind === "reasoning" && last.endedAt === undefined
    ? [...parts.slice(0, -1), { ...last, endedAt: now }]
    : parts;
}
