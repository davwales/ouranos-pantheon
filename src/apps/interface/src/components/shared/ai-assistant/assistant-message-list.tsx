import { Message } from "@/components/shared/ai-assistant/message";
import { type AssistantChatMessage } from "@/components/shared/ai-assistant/use-assistant-chat";
import { type AssistantActionHandlers } from "@/lib/api/assistant";

export type AssistantMessageCallbacks = {
  onApplyAction?: (messageId: string, actionId: string) => void;
  onRejectAction?: (messageId: string, actionId: string) => void;
  onApplyAll?: (messageId: string) => void;
  onRejectAll?: (messageId: string) => void;
};

export function AssistantMessageList({
  messages,
  isStreaming,
  assistantName,
  actionHandlers,
  onApplyAction,
  onRejectAction,
  onApplyAll,
  onRejectAll,
}: AssistantMessageCallbacks & {
  messages: AssistantChatMessage[];
  isStreaming: boolean;
  assistantName: string;
  actionHandlers?: AssistantActionHandlers;
}) {
  return (
    <div className="flex flex-col gap-5" role="log" aria-busy={isStreaming}>
      {messages.map((message, index) => (
        <Message
          key={message.id}
          name={assistantName}
          role={message.role}
          content={message.content}
          parts={message.parts}
          context={message.context}
          isStreaming={isStreaming && index === messages.length - 1}
          actionHandlers={actionHandlers}
          onApplyAction={(actionId) => onApplyAction?.(message.id, actionId)}
          onRejectAction={(actionId) => onRejectAction?.(message.id, actionId)}
          onApplyAll={() => onApplyAll?.(message.id)}
          onRejectAll={() => onRejectAll?.(message.id)}
        />
      ))}
    </div>
  );
}
