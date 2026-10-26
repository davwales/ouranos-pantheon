"use client";

import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { type ImageActions } from "@/components/shared/ai-assistant/image-actions-context";
import { Button } from "@/components/ui/button";
import { Pencil, Sparkles } from "lucide-react";

export function ImageActionButtons({
  image,
  actions,
  onAction,
}: {
  image: AssistantChatImagePart;
  actions: ImageActions;
  onAction?: () => void;
}) {
  const { onEdit, onMoreLikeThis } = actions;
  if (!onEdit && !onMoreLikeThis) {
    return null;
  }

  return (
    <div className="flex flex-wrap gap-1">
      {onEdit && (
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={(e) => {
            e.stopPropagation();
            onAction?.();
            onEdit(image);
          }}
        >
          <Pencil aria-hidden />
          Edit
        </Button>
      )}
      {onMoreLikeThis && (
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={(e) => {
            e.stopPropagation();
            onAction?.();
            onMoreLikeThis(image);
          }}
        >
          <Sparkles aria-hidden />
          More like this
        </Button>
      )}
    </div>
  );
}
