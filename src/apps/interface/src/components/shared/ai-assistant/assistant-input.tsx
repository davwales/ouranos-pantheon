"use client";

import AutosizeTextarea from "@/components/shared/autosize-textarea";
import { Button } from "@/components/ui/button";
import { SendHorizontal, Square } from "lucide-react";
import { useState } from "react";

export function AssistantInput({
  isStreaming,
  placeholder = "Ask a question...",
  autoFocus,
  onSend,
  onStop,
}: {
  isStreaming: boolean;
  placeholder?: string;
  autoFocus?: boolean;
  onSend: (content: string) => Promise<boolean>;
  onStop: () => void;
}) {
  const [text, setText] = useState("");
  const canSend = !isStreaming && text.trim().length > 0;

  const submit = async () => {
    if (!canSend) {
      return;
    }
    setText("");
    if (!(await onSend(text))) {
      setText((current) => current || text);
    }
  };

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        submit();
      }}
      className="flex items-end gap-2"
    >
      <AutosizeTextarea
        value={text}
        onChange={(e) => setText(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            submit();
          }
        }}
        placeholder={placeholder}
        aria-label="Message"
        autoFocus={autoFocus}
        className="min-h-10 max-h-40 resize-none"
      />
      {isStreaming ? (
        <Button
          type="button"
          size="icon"
          variant="outline"
          onClick={onStop}
          aria-label="Stop generating"
        >
          <Square />
        </Button>
      ) : (
        <Button type="submit" size="icon" disabled={!canSend} aria-label="Send">
          <SendHorizontal />
        </Button>
      )}
    </form>
  );
}
