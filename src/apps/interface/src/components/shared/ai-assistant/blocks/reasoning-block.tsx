"use client";

import { BlockHeader } from "@/components/shared/ai-assistant/blocks/block-header";
import { MarkdownRenderer } from "@/components/shared/markdown-renderer";
import { Collapsible, CollapsibleContent } from "@/components/ui/collapsible";
import { cn } from "@/lib/utils";
import { Brain } from "lucide-react";
import { memo, useId, useState } from "react";

export const ReasoningBlock = memo(function ReasoningBlock({
  content,
  isStreaming,
  startedAt,
  endedAt,
  variant = "inline",
}: {
  content: string;
  isStreaming: boolean;
  startedAt?: number;
  endedAt?: number;
  variant?: "inline" | "row";
}) {
  const [open, setOpen] = useState(false);
  const contentId = useId();

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <BlockHeader
        open={open}
        onOpenChange={setOpen}
        controls={contentId}
        icon={<Brain aria-hidden className={isStreaming ? "motion-safe:animate-pulse" : undefined} />}
        label={
          isStreaming ? (
            <span className="text-shimmer">Thinking…</span>
          ) : (
            reasoningLabel(startedAt, endedAt)
          )
        }
        className={
          variant === "row" ? "px-3 py-2 text-xs hover:bg-muted/50" : "w-fit max-w-full"
        }
      />
      <CollapsibleContent id={contentId} className={variant === "row" ? "px-3 pb-3" : undefined}>
        <div
          className={cn(
            "ml-2 border-l pl-4 text-sm text-muted-foreground",
            variant === "inline" && "mt-2",
          )}
        >
          <MarkdownRenderer variant="compact" componentClassName={{ blockCode: "my-2" }}>
            {content}
          </MarkdownRenderer>
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
});

export function reasoningLabel(startedAt?: number, endedAt?: number): string {
  if (startedAt === undefined || endedAt === undefined) {
    return "Reasoning";
  }
  const seconds = Math.max(1, Math.round((endedAt - startedAt) / 1000));
  return `Thought for ${seconds}s`;
}
