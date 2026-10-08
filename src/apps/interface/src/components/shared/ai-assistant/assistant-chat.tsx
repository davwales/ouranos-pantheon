"use client";

import { AssistantInput } from "@/components/shared/ai-assistant/assistant-input";
import { AssistantMessageList } from "@/components/shared/ai-assistant/assistant-message-list";
import { type AssistantChatMessage } from "@/components/shared/ai-assistant/use-assistant-chat";
import { Typography } from "@/components/shared/typography";
import { cn } from "@/lib/utils";
import { useEffect, useRef } from "react";

const PINNED_THRESHOLD_PX = 32;

export function AssistantChat({
  messages,
  isStreaming,
  error,
  assistantName,
  placeholder,
  emptyState,
  autoFocus,
  onSend,
  onStop,
  className,
}: {
  messages: AssistantChatMessage[];
  isStreaming: boolean;
  error: string | null;
  assistantName: string;
  placeholder?: string;
  emptyState?: React.ReactNode;
  autoFocus?: boolean;
  onSend: (content: string) => Promise<boolean>;
  onStop: () => void;
  className?: string;
}) {
  const scrollRef = useRef<HTMLDivElement | null>(null);
  const isPinnedRef = useRef(true);

  useEffect(() => {
    const el = scrollRef.current;
    if (el && isPinnedRef.current) {
      el.scrollTop = el.scrollHeight;
    }
  }, [messages]);

  const handleScroll = () => {
    const el = scrollRef.current;
    if (el) {
      isPinnedRef.current =
        el.scrollHeight - el.scrollTop - el.clientHeight < PINNED_THRESHOLD_PX;
    }
  };

  const handleSend = (content: string) => {
    isPinnedRef.current = true;
    return onSend(content);
  };

  return (
    <div className={cn("flex min-h-0 flex-1 flex-col", className)}>
      <div
        ref={scrollRef}
        onScroll={handleScroll}
        className="min-h-0 flex-1 overflow-y-auto px-4 py-2"
      >
        {messages.length === 0 && emptyState ? (
          <div className="flex h-full items-center justify-center text-center">
            {emptyState}
          </div>
        ) : (
          <AssistantMessageList
            messages={messages}
            isStreaming={isStreaming}
            assistantName={assistantName}
          />
        )}
      </div>
      <div className="border-t p-4 pb-[max(1rem,env(safe-area-inset-bottom))]">
        {error && (
          <Typography
            variant="muted"
            role="alert"
            className="mb-2 text-destructive"
          >
            {error}
          </Typography>
        )}
        <AssistantInput
          isStreaming={isStreaming}
          placeholder={placeholder}
          autoFocus={autoFocus}
          onSend={handleSend}
          onStop={onStop}
        />
      </div>
    </div>
  );
}
