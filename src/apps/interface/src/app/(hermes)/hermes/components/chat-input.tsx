import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import AutosizeTextarea from "@/components/shared/autosize-textarea";
import { Button } from "@/components/ui/button";
import { apiUrl } from "@/lib/api-client";
import { X } from "lucide-react";
import { ChangeEvent, useEffect, useRef } from "react";

export default function ChatInput({
  inputText,
  isGenerating,
  isEditing,
  onInputChange,
  onNewMessage,
  onUpdateMessage,
  onCancelEdit,
  onStop,
  attachment = null,
  onRemoveAttachment,
  className,
  ...props
}: React.ComponentProps<"form"> & {
  inputText: string;
  isGenerating: boolean;
  isEditing: boolean;
  onInputChange: (value: string) => void;
  onNewMessage: () => void;
  onUpdateMessage: () => void;
  onCancelEdit?: () => void;
  onStop?: () => void;
  attachment?: AssistantChatImagePart | null;
  onRemoveAttachment?: () => void;
}) {
  const formRef = useRef<HTMLFormElement>(null);

  // Picking an image to edit moves straight on to describing the change.
  useEffect(() => {
    if (attachment) {
      formRef.current?.querySelector("textarea")?.focus();
    }
  }, [attachment]);

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (isEditing) {
      onUpdateMessage();
    } else {
      onNewMessage();
    }
  };

  const handleInputChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
    return onInputChange(event.target.value);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      if (!isGenerating && inputText.trim()) {
        formRef.current?.requestSubmit();
      }
    } else if (e.key === "Escape" && attachment && !inputText) {
      e.preventDefault();
      onRemoveAttachment?.();
    }
  };

  const inputPlaceholder = isEditing
    ? "Editing message..."
    : attachment
      ? "Describe how to change this image..."
      : "Type your message...";
  const placeholder = isGenerating
    ? "Generating response..."
    : inputPlaceholder;

  return (
    <form
      {...props}
      ref={formRef}
      onSubmit={handleSubmit}
      className={`flex items-center gap-4 ${className}`}
    >
      <div className="flex min-w-0 flex-1 flex-col gap-2">
        {attachment && !isEditing && (
          <div className="flex w-fit max-w-full items-center gap-2 rounded-lg border bg-muted/40 p-1 pr-1.5 text-sm">
            {/* Generated images come from the gateway on a host resolved at runtime, which next/image cannot allow-list. */}
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={apiUrl(attachment.url)}
              alt=""
              className="size-10 shrink-0 rounded object-cover"
            />
            <span className="min-w-0">
              <span className="block font-medium">Editing image</span>
              <span className="line-clamp-1 text-muted-foreground">{attachment.altText}</span>
            </span>
            <Button
              type="button"
              variant="ghost"
              size="icon-xs"
              aria-label="Remove image"
              onClick={onRemoveAttachment}
              className="shrink-0"
            >
              <X aria-hidden />
            </Button>
          </div>
        )}
        <AutosizeTextarea
          value={inputText}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          disabled={isGenerating}
          className="min-h-16 max-h-64"
        />
      </div>
      {isEditing ? (
        <div>
          <Button
            color="primary"
            disabled={isGenerating || !inputText.trim()}
            type="submit"
            className="w-full"
          >
            Save
          </Button>
          <Button
            variant="outline"
            color="secondary"
            onClick={onCancelEdit}
            disabled={isGenerating}
            type="button"
            className="w-full mt-2"
          >
            Cancel
          </Button>
        </div>
      ) : isGenerating && onStop ? (
        <div className="h-fit">
          <Button variant="outline" type="button" onClick={onStop} className="h-fit">
            Stop
          </Button>
        </div>
      ) : (
        <div className="h-fit">
          <Button
            color="primary"
            disabled={isGenerating || !inputText.trim()}
            type="submit"
            className="h-fit"
          >
            Send
          </Button>
        </div>
      )}
    </form>
  );
}
