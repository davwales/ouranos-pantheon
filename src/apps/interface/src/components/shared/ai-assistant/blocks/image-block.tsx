"use client";

import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogTitle,
} from "@/components/ui/dialog";
import { apiUrl } from "@/lib/api-client";
import { ExternalLink, ImageOff } from "lucide-react";
import { memo, useState } from "react";

export const ImageBlock = memo(function ImageBlock({ image }: { image: AssistantChatImagePart }) {
  const [isOpen, setIsOpen] = useState(false);
  const [isUnavailable, setIsUnavailable] = useState(false);
  const src = apiUrl(image.url);
  const aspectRatio = `${image.width} / ${image.height}`;

  if (isUnavailable) {
    return (
      <div
        style={{ aspectRatio }}
        className="flex w-full max-w-md flex-col items-center justify-center gap-2 rounded-lg border border-dashed bg-muted/40 p-4 text-center text-sm text-muted-foreground"
      >
        <ImageOff aria-hidden className="size-5" />
        This image is no longer available.
      </div>
    );
  }

  return (
    <>
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
        className="w-full max-w-md cursor-zoom-in overflow-hidden rounded-lg border bg-muted outline-none focus-visible:ring-[3px] focus-visible:ring-ring/50"
      >
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
      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent
          className="sm:max-w-[min(95vw,64rem)]"
          onClick={(e) => e.stopPropagation()}
        >
          <DialogTitle>Generated image</DialogTitle>
          <img
            src={src}
            alt={image.altText}
            width={image.width}
            height={image.height}
            className="max-h-[75vh] w-full rounded-md object-contain"
          />
          <DialogDescription className="line-clamp-4">{image.altText}</DialogDescription>
          <a
            href={src}
            target="_blank"
            rel="noreferrer"
            className="inline-flex items-center gap-1.5 text-sm font-medium text-primary underline underline-offset-4"
          >
            <ExternalLink aria-hidden className="size-4" />
            Open in new tab
          </a>
        </DialogContent>
      </Dialog>
    </>
  );
});
