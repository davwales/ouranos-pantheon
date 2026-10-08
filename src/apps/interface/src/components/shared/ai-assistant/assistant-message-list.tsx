import { Message } from "@/components/shared/ai-assistant/message";
import { type AssistantChatMessage } from "@/components/shared/ai-assistant/use-assistant-chat";
import { cn } from "@/lib/utils";

export function AssistantMessageList({
  messages,
  isStreaming,
  assistantName,
}: {
  messages: AssistantChatMessage[];
  isStreaming: boolean;
  assistantName: string;
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
              isStreaming={isStreaming && index === messages.length - 1}
              className="max-w-[90%] w-fit wrap-break-word"
            />
          </div>
        );
      })}
    </div>
  );
}
