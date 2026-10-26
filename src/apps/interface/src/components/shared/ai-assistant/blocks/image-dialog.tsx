"use client";

import { type AssistantChatImagePart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { ImageActionButtons } from "@/components/shared/ai-assistant/blocks/image-action-buttons";
import { useImageActions } from "@/components/shared/ai-assistant/image-actions-context";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogTitle,
} from "@/components/ui/dialog";
import { apiUrl } from "@/lib/api-client";
import { ArrowLeft, ExternalLink } from "lucide-react";
import { useEffect, useState } from "react";

export function ImageDialog({
  image,
  open,
  onOpenChange,
}: {
  image: AssistantChatImagePart;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const actions = useImageActions();
  const { loadSource } = actions;
  // The sources the user stepped back through, most recent last.
  const [trail, setTrail] = useState<AssistantChatImagePart[]>([]);
  const [source, setSource] = useState<{
    forId: string;
    image: AssistantChatImagePart | null;
  } | null>(null);

  const shown = trail.at(-1) ?? image;
  const src = apiUrl(shown.url);
  const shownSource = source?.forId === shown.imageId ? source.image : null;

  useEffect(() => {
    if (!open || !loadSource) return;

    let isCancelled = false;
    loadSource(shown)
      .catch(() => null)
      .then((loaded) => {
        if (!isCancelled) setSource({ forId: shown.imageId, image: loaded });
      });
    return () => {
      isCancelled = true;
    };
  }, [open, loadSource, shown]);

  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen) setTrail([]);
    onOpenChange(isOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="sm:max-w-[min(95vw,64rem)]"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center gap-2">
          {trail.length > 0 && (
            <Button
              type="button"
              variant="ghost"
              size="icon-sm"
              aria-label="Back"
              onClick={() => setTrail((prev) => prev.slice(0, -1))}
            >
              <ArrowLeft aria-hidden />
            </Button>
          )}
          <DialogTitle>{trail.length > 0 ? "Source image" : "Generated image"}</DialogTitle>
        </div>
        {/* Generated images come from the gateway on a host resolved at runtime, which next/image cannot allow-list. */}
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={src}
          alt={shown.altText}
          width={shown.width}
          height={shown.height}
          className="max-h-[75vh] w-full rounded-md object-contain"
        />
        <DialogDescription className="line-clamp-4">{shown.altText}</DialogDescription>
        {shownSource && (
          <button
            type="button"
            onClick={() => setTrail((prev) => [...prev, shownSource])}
            className="flex w-fit items-center gap-3 rounded-md border p-1.5 pr-3 text-left text-sm hover:bg-accent focus-visible:ring-[3px] focus-visible:ring-ring/50 outline-none"
          >
            {/* Generated images come from the gateway on a host resolved at runtime, which next/image cannot allow-list. */}
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src={apiUrl(shownSource.url)}
              alt=""
              className="size-12 rounded object-cover"
            />
            <span>
              <span className="block font-medium">Edited from</span>
              <span className="line-clamp-1 text-muted-foreground">{shownSource.altText}</span>
            </span>
          </button>
        )}
        <div className="flex flex-wrap items-center justify-between gap-2">
          <ImageActionButtons
            image={shown}
            actions={actions}
            onAction={() => handleOpenChange(false)}
          />
          <a
            href={src}
            target="_blank"
            rel="noreferrer"
            className="inline-flex items-center gap-1.5 text-sm font-medium text-primary underline underline-offset-4"
          >
            <ExternalLink aria-hidden className="size-4" />
            Open in new tab
          </a>
        </div>
      </DialogContent>
    </Dialog>
  );
}
