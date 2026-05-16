import {
  MenuAction,
  ResponsiveContextMenu,
} from "@/components/shared/responsive-context-menu";
import { Message } from "@/app/(hermes)/hermes/components/message";
import { MessageInput, Role } from "@/lib/api/hermes";
import { Minimize2, Pencil, RotateCcw, Trash } from "lucide-react";
import { useEffect, useRef } from "react";

export default function ChatMessageList({
  messages,
  personaName,
  isGenerating,
  onEditMessage,
  onDeleteMessage,
  onRetryMessage,
  ...props
}: React.ComponentProps<"div"> & {
  messages: MessageInput[];
  personaName: string;
  onEditMessage?: (index: number) => void;
  onDeleteMessage?: (index: number) => void;
  onRetryMessage?: (index: number) => void;
  isGenerating?: boolean;
}) {
  const messageListRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (messageListRef?.current) {
      messageListRef.current.scrollIntoView({ behavior: "smooth" });
    }
  }, [messages]);

  const getMenuActions = (index: number): MenuAction[] => {
    const msg = messages[index];

    if (msg.role === Role.Summary) {
      return [];
    }

    const actions: MenuAction[] = [
      {
        label: "Edit",
        icon: <Pencil />,
        onClick: () => onEditMessage?.(index),
      },
    ];

    if (msg.role === Role.Assistant) {
      actions.push({
        label: "Retry",
        icon: <RotateCcw />,
        onClick: () => onRetryMessage?.(index),
      });
    }

    if (msg.role === Role.User) {
      actions.push({
        label: "Delete",
        icon: <Trash />,
        onClick: () => onDeleteMessage?.(index),
      });
    }

    return actions;
  };

  return (
    <div {...props}>
      {messages.map((msg, index) => {
        const isSummary = msg.role === Role.Summary;

        return (
          <div key={index}>
            {isSummary && (
              <div className="flex items-center gap-3 mx-2 my-4">
                <div className="flex-1 h-px bg-border" />
                <div className="flex items-center gap-1.5 text-xs text-muted-foreground shrink-0">
                  <Minimize2 className="h-3 w-3" />
                  <span>Conversation compacted</span>
                </div>
                <div className="flex-1 h-px bg-border" />
              </div>
            )}
            <div
              className={`flex mt-4 mx-2 ${
                msg.role == Role.User ? "justify-end" : "justify-start"
              }`}
            >
              <ResponsiveContextMenu
                actions={getMenuActions(index)}
                title="Actions"
                description="Perform an action on the message."
                disabled={isGenerating}
              >
                <Message
                  name={
                    isSummary
                      ? "Summary"
                      : msg.role == Role.User
                        ? "You"
                        : personaName
                  }
                  role={msg.role === Role.Summary ? Role.Assistant : msg.role}
                  content={msg.content}
                  isStreaming={isGenerating && index === messages.length - 1}
                  className="w-fit text-left wrap-break-word"
                />
              </ResponsiveContextMenu>
            </div>
          </div>
        );
      })}
      <div ref={messageListRef} />
    </div>
  );
}
