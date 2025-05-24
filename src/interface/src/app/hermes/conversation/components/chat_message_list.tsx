import {
  MenuAction,
  ResponsiveContextMenu,
} from "@/app/components/responsive-context-menu";
import { Message } from "@/app/hermes/conversation/components/message";
import ConversationAssistant from "@/app/hermes/types";
import { MessageInput, Role } from "@/gql/graphql";
import { Pencil, RotateCcw, Trash } from "lucide-react";
import { useEffect, useRef } from "react";

export default function ChatMessageList({
  messages,
  assistant,
  isGenerating,
  onEditMessage,
  onDeleteMessage,
  onRetryMessage,
  ...props
}: React.ComponentProps<"div"> & {
  messages: MessageInput[];
  assistant: ConversationAssistant;
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
    const actions: MenuAction[] = [
      {
        label: "Edit",
        icon: <Pencil />,
        onClick: () => onEditMessage?.(index),
      },
    ];

    if (messages[index].role === Role.Assistant) {
      actions.push({
        label: "Retry",
        icon: <RotateCcw />,
        onClick: () => onRetryMessage?.(index),
      });
    }

    if (messages[index].role === Role.User) {
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
      {messages.map((msg, index) => (
        <div
          key={index}
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
                msg.role == Role.User
                  ? assistant.userName
                  : assistant.assistantName
              }
              role={msg.role}
              content={msg.content}
              className="w-fit text-left break-words"
            />
          </ResponsiveContextMenu>
        </div>
      ))}
      <div ref={messageListRef} />
    </div>
  );
}
