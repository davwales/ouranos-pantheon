import {
  type AssistantChatImagePart,
  type AssistantChatPart,
  type AssistantChatToolStatus,
} from "@/components/shared/ai-assistant/assistant-chat-parts";
import { type AssistantToolStatus } from "@/lib/api/assistant";
import {
  type CompletionToolCallInput,
  hermesImagePath,
  type ImageMessagePart,
  type MessagePart,
} from "@/lib/api/hermes";

const CHAT_TOOL_STATUSES: Record<AssistantToolStatus, AssistantChatToolStatus> = {
  Completed: "completed",
  Failed: "failed",
  Skipped: "skipped",
};

export function toChatParts(parts: MessagePart[]): AssistantChatPart[] {
  return parts.map((part, i): AssistantChatPart => {
    const id = `part-${i}`;
    switch (part.$type) {
      case "reasoning":
        return { kind: "reasoning", id, content: part.content, endedAt: 0 };
      case "text":
        return { kind: "text", id, content: part.content };
      case "toolCall":
        return {
          kind: "tool",
          id,
          callId: part.callId,
          name: part.name,
          arguments: part.arguments,
          status: CHAT_TOOL_STATUSES[part.status],
          output: part.output,
          isTruncated: part.isTruncated,
          durationMs: part.durationMs ?? undefined,
        };
      case "image":
        return {
          kind: "image",
          id,
          callId: "",
          imageId: part.imageId,
          url: hermesImagePath(part.imageId),
          width: part.width,
          height: part.height,
          altText: part.altText,
        };
    }
  });
}

export function toMessageParts(parts: AssistantChatPart[]): MessagePart[] {
  return parts.flatMap((part): MessagePart[] => {
    switch (part.kind) {
      case "reasoning":
        return [{ $type: "reasoning", content: part.content }];
      case "text":
        return [{ $type: "text", content: part.content }];
      case "tool":
        return [
          {
            $type: "toolCall",
            callId: part.callId,
            name: part.name,
            arguments: part.arguments,
            status: toToolStatus(part.status),
            output: part.output ?? "",
            isTruncated: part.isTruncated ?? false,
            ...(part.durationMs !== undefined && { durationMs: part.durationMs }),
          },
        ];
      case "image":
        return [
          {
            $type: "image",
            imageId: part.imageId,
            width: part.width,
            height: part.height,
            altText: part.altText,
          },
        ];
      case "action":
        return [];
    }
  });
}

// A tool's image part directly follows its call part, so each image belongs to the call before it.
export function toolCallsOf(
  parts: MessagePart[] | undefined,
): CompletionToolCallInput[] | undefined {
  const calls: CompletionToolCallInput[] = [];
  for (const part of parts ?? []) {
    if (part.$type === "toolCall") {
      calls.push({
        callId: part.callId,
        name: part.name,
        arguments: part.arguments,
        output: part.output,
      });
    } else if (part.$type === "image" && calls.length > 0) {
      calls[calls.length - 1].imageId ??= part.imageId;
    }
  }
  return calls.length > 0 ? calls : undefined;
}

export function imagesOf(parts: MessagePart[] | undefined): ImageMessagePart[] | undefined {
  const images = (parts ?? []).filter((part) => part.$type === "image");
  return images.length > 0 ? images : undefined;
}

export function toImagePart(image: AssistantChatImagePart): ImageMessagePart {
  return {
    $type: "image",
    imageId: image.imageId,
    width: image.width,
    height: image.height,
    altText: image.altText,
  };
}

// An edited reply keeps its reasoning and tool calls, but its text becomes the edited content.
export function withEditedText(
  parts: MessagePart[] | undefined,
  content: string,
): MessagePart[] | undefined {
  if (!parts || parts.length === 0) {
    return parts;
  }
  return [
    ...parts.filter((part) => part.$type !== "text"),
    { $type: "text", content },
  ];
}

function toToolStatus(status: AssistantChatToolStatus): AssistantToolStatus {
  switch (status) {
    case "completed":
      return "Completed";
    case "skipped":
      return "Skipped";
    case "failed":
    case "running":
    case "cancelled":
      return "Failed";
  }
}
