"use client";

import {
  MenuAction,
  ResponsiveContextMenu,
} from "@/components/shared/responsive-context-menu";
import { Message } from "@/components/shared/ai-assistant/message";
import { MessageActionBar } from "@/app/(hermes)/hermes/components/message-action-bar";
import SummaryView from "@/app/(hermes)/hermes/components/summary-view";
import { toChatParts } from "@/app/(hermes)/hermes/components/message-parts";
import { type AssistantChatPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { Button } from "@/components/ui/button";
import { type ChatMessage } from "@/hooks/use-message-controller";
import { useStickToBottom } from "@/hooks/use-stick-to-bottom";
import { Role } from "@/lib/api/hermes";
import { cn } from "@/lib/utils";
import { ArrowDown, Copy, Pencil, RotateCcw, Trash } from "lucide-react";
import { useEffect } from "react";

export default function ChatMessageList({
  messages,
  streamingParts,
  personaName,
  isGenerating,
  isCompacting,
  compactionError,
  onEditMessage,
  onDeleteMessage,
  onRetryMessage,
  onRetryCompact,
  className,
  ...props
}: React.ComponentProps<"div"> & {
  messages: ChatMessage[];
  streamingParts?: AssistantChatPart[] | null;
  personaName: string;
  onEditMessage?: (index: number) => void;
  onDeleteMessage?: (index: number) => void;
  onRetryMessage?: (index: number) => void;
  onRetryCompact?: () => void;
  isGenerating?: boolean;
  isCompacting?: boolean;
  compactionError?: string | null;
}) {
  const { contentRef, isAtBottom, scrollToBottom } = useStickToBottom<HTMLDivElement>();
  const lastUserMessageId = messages.findLast((m) => m.role === Role.User)?.clientId;

  // Sending a message jumps back to the conversation's end, even after scrolling up to read.
  useEffect(() => {
    if (lastUserMessageId) {
      scrollToBottom();
    }
  }, [lastUserMessageId, scrollToBottom]);

  const getMenuActions = (index: number): MenuAction[] => {
    const msg = messages[index];
    const copy: MenuAction = {
      label: "Copy",
      icon: <Copy />,
      onClick: () => void copyToClipboard(msg.content),
    };
    const edit: MenuAction = {
      label: "Edit",
      icon: <Pencil />,
      onClick: () => onEditMessage?.(index),
    };
    const retry: MenuAction = {
      label: "Retry",
      icon: <RotateCcw />,
      onClick: () => onRetryMessage?.(index),
    };
    const remove: MenuAction = {
      label: "Delete",
      icon: <Trash />,
      onClick: () => onDeleteMessage?.(index),
    };

    switch (msg.role) {
      case Role.Summary:
        return [copy, edit, retry, remove];
      case Role.Assistant:
        return [copy, edit, retry];
      case Role.User:
        return [copy, edit, remove];
      default:
        return [copy, edit];
    }
  };

  const lastSummaryIndex = messages.findLastIndex((m) => m.role === Role.Summary);

  return (
    <div ref={contentRef} className={className} {...props}>
      {messages.map((msg, index) => {
        const isSummary = msg.role === Role.Summary;
        const isUser = msg.role === Role.User;
        const isLast = index === messages.length - 1;
        const isStreaming = !!isGenerating && isLast;
        const parts =
          isLast && streamingParts
            ? streamingParts
            : msg.parts && msg.parts.length > 0
              ? toChatParts(msg.parts)
              : undefined;
        const actions = getMenuActions(index);

        return (
          <div key={msg.clientId} className="mx-auto w-full max-w-3xl">
            {isSummary && (
              <SummaryView
                content={msg.content}
                isCompacting={!!isCompacting}
                compactionError={compactionError ?? null}
                isLastSummary={lastSummaryIndex === index}
                onRetryCompact={() => onRetryCompact?.()}
              />
            )}
            <div className="group/message mt-4 mx-2">
              <ResponsiveContextMenu
                actions={actions}
                title="Actions"
                description="Perform an action on the message."
                disabled={isGenerating}
              >
                <Message
                  name={isSummary ? "Summary" : personaName}
                  role={isUser ? "User" : "Assistant"}
                  content={msg.content}
                  parts={parts}
                  isStreaming={isStreaming}
                  className="text-left"
                />
              </ResponsiveContextMenu>
              {!isStreaming && (
                <MessageActionBar
                  actions={actions}
                  disabled={isGenerating}
                  isPinned={isLast && msg.role === Role.Assistant}
                  className={cn("mt-1", isUser && "justify-end")}
                />
              )}
            </div>
          </div>
        );
      })}
      {!isAtBottom && (
        <div className="pointer-events-none sticky bottom-4 flex h-0 justify-center">
          <Button
            type="button"
            variant="secondary"
            size="sm"
            onClick={scrollToBottom}
            className="pointer-events-auto -translate-y-full rounded-full shadow-md"
          >
            <ArrowDown />
            Jump to latest
          </Button>
        </div>
      )}
    </div>
  );
}

async function copyToClipboard(content: string) {
  try {
    await navigator.clipboard.writeText(content);
  } catch (error) {
    console.error("Failed to copy message:", error);
  }
}
