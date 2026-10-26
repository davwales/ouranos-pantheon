"use client";

import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { ImageActionButtons } from "@/components/shared/ai-assistant/blocks/image-action-buttons";
import { ImageDialog } from "@/components/shared/ai-assistant/blocks/image-dialog";
import { useImageActions } from "@/components/shared/ai-assistant/image-actions-context";
import { apiUrl } from "@/lib/api-client";
import { cn } from "@/lib/utils";
import { ImageOff } from "lucide-react";
import { memo, useState } from "react";

// A compact image is a small thumbnail without actions, such as one a user message refers to.
export const ImageBlock = memo(function ImageBlock({
  image,
  isCompact = false,
}: {
  image: AssistantChatImagePart;
  isCompact?: boolean;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const [isUnavailable, setIsUnavailable] = useState(false);
  const actions = useImageActions();
  const src = apiUrl(image.url);
  const aspectRatio = `${image.width} / ${image.height}`;
  const width = isCompact ? "max-w-48" : "max-w-md";

  if (isUnavailable) {
    return (
      <div
        style={{ aspectRatio }}
        className={cn(
          "flex w-full flex-col items-center justify-center gap-2 rounded-lg border border-dashed bg-muted/40 p-4 text-center text-sm text-muted-foreground",
          width,
        )}
      >
        <ImageOff aria-hidden className="size-5" />
        This image is no longer available.
      </div>
    );
  }

  return (
    <div className={cn("flex w-full flex-col gap-1", width)}>
      <div
        role="button"
        tabIndex={0}
        aria-label={`View image: ${image.altText}`}
        onClick={(e) => {
          e.stopPropagation();
          setIsOpen(true);
        }}
        onKeyDown={(e) => {
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            e.stopPropagation();
            setIsOpen(true);
          }
        }}
        style={{ aspectRatio }}
        className="w-full cursor-zoom-in overflow-hidden rounded-lg border bg-muted outline-none focus-visible:ring-[3px] focus-visible:ring-ring/50"
      >
        {/* Generated images come from the gateway on a host resolved at runtime, which next/image cannot allow-list. */}
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={src}
          alt={image.altText}
          width={image.width}
          height={image.height}
          loading="lazy"
          onError={() => setIsUnavailable(true)}
          className="size-full object-cover"
        />
      </div>
      {!isCompact && <ImageActionButtons image={image} actions={actions} />}
      <ImageDialog image={image} open={isOpen} onOpenChange={setIsOpen} />
    </div>
  );
});
