import {
  AssistantTurn,
  type AssistantTurnProps,
} from "@/components/shared/ai-assistant/assistant-turn";
import {
  type AssistantChatImagePart,
  type AssistantChatPart,
} from "@/components/shared/ai-assistant/assistant-chat-parts";
import { parseSegments } from "@/components/shared/ai-assistant/parse-segments";
import { UserMessage } from "@/components/shared/ai-assistant/user-message";
import { type AssistantRole } from "@/lib/api/assistant";

export function Message({
  role,
  content,
  parts,
  context,
  name,
  isStreaming = false,
  actionHandlers,
  onApplyAction,
  onRejectAction,
  onApplyAll,
  onRejectAll,
  ...props
}: Omit<AssistantTurnProps, "parts"> & {
  role: AssistantRole;
  content: string;
  parts?: AssistantChatPart[];
}) {
  if (role === "User") {
    const images = parts?.filter((part): part is AssistantChatImagePart => part.kind === "image");
    return <UserMessage content={content} images={images} {...props} />;
  }

  return (
    <AssistantTurn
      name={name}
      parts={parts ?? partsFromContent(content, isStreaming)}
      context={context}
      isStreaming={isStreaming}
      actionHandlers={actionHandlers}
      onApplyAction={onApplyAction}
      onRejectAction={onRejectAction}
      onApplyAll={onApplyAll}
      onRejectAll={onRejectAll}
      {...props}
    />
  );
}

// Chats without streamed parts, such as Hermes, inline their reasoning in <think> tags.
function partsFromContent(content: string, isStreaming: boolean): AssistantChatPart[] {
  return parseSegments(content, isStreaming).flatMap((segment, i): AssistantChatPart[] => {
    switch (segment.type) {
      case "loading":
        return [];
      case "thinking":
        return [
          {
            kind: "reasoning",
            id: `reasoning-${i}`,
            content: segment.content,
            endedAt: segment.isStreaming ? undefined : 0,
          },
        ];
      case "text":
        return [{ kind: "text", id: `text-${i}`, content: segment.content }];
    }
  });
}
