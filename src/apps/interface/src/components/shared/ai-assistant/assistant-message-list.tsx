import { Message } from "@/components/shared/ai-assistant/message";
import { type AssistantChatMessage } from "@/components/shared/ai-assistant/use-assistant-chat";
import { type AssistantActionHandlers } from "@/lib/api/assistant";
import { cn } from "@/lib/utils";

export function AssistantMessageList({
  messages,
  isStreaming,
  assistantName,
  toolLabels,
  actionHandlers,
  onApplyAction,
}: {
  messages: AssistantChatMessage[];
  isStreaming: boolean;
  assistantName: string;
  toolLabels?: Record<string, string>;
  actionHandlers?: AssistantActionHandlers;
  onApplyAction?: (messageId: string, actionId: string) => void;
}) {
  return (
    <div className="flex flex-col gap-3" role="log" aria-busy={isStreaming}>
      {messages.map((message, index) => {
        const isUser = message.role === "User";
        return (
          <div
            key={message.id}
            className={cn("flex", isUser ? "justify-end" : "justify-start")}
          >
            <Message
              name={isUser ? "You" : assistantName}
              role={message.role}
              content={message.content}
              reasoning={message.reasoning}
              isStreaming={isStreaming && index === messages.length - 1}
              tools={message.tools}
              actions={message.actions}
              toolLabels={toolLabels}
              actionHandlers={actionHandlers}
              onApplyAction={(actionId) => onApplyAction?.(message.id, actionId)}
              className="min-w-0 max-w-[90%] w-fit wrap-break-word"
            />
          </div>
        );
      })}
    </div>
  );
}
