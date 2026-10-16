"use client";

import { BlockHeader } from "@/components/shared/ai-assistant/blocks/block-header";
import { ToolDetails } from "@/components/shared/ai-assistant/blocks/tool-details";
import { type AssistantChatToolPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { Collapsible, CollapsibleContent } from "@/components/ui/collapsible";
import { Check, CircleSlash, Loader2, SkipForward, X } from "lucide-react";
import { memo, useId, useState } from "react";

export const ToolBlock = memo(function ToolBlock({
  tool,
  context,
}: {
  tool: AssistantChatToolPart;
  context?: unknown;
}) {
  const [open, setOpen] = useState(false);
  const contentId = useId();

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <BlockHeader
        open={open}
        onOpenChange={setOpen}
        controls={contentId}
        icon={<ToolStatusIcon status={tool.status} />}
        label={<code className="font-mono text-xs text-foreground">{tool.name}</code>}
        meta={<ToolStatusLabel tool={tool} />}
        className="px-3 py-2 hover:bg-muted/50"
      />
      <CollapsibleContent id={contentId} className="px-3 pb-3">
        <ToolDetails
          context={context}
          input={tool.arguments}
          output={tool.output}
          isTruncated={tool.isTruncated}
          isSettled={tool.status !== "running"}
        />
      </CollapsibleContent>
    </Collapsible>
  );
});

export function ToolStatusIcon({ status }: { status: AssistantChatToolPart["status"] }) {
  switch (status) {
    case "running":
      return <Loader2 aria-hidden className="motion-safe:animate-spin" />;
    case "completed":
      return <Check aria-hidden className="text-emerald-600" />;
    case "failed":
      return <X aria-hidden className="text-destructive" />;
    case "skipped":
      return <SkipForward aria-hidden />;
    case "cancelled":
      return <CircleSlash aria-hidden />;
  }
}

function ToolStatusLabel({ tool }: { tool: AssistantChatToolPart }) {
  switch (tool.status) {
    case "running":
      return "Running…";
    case "completed":
      return (
        <>
          <span className="sr-only">Completed in </span>
          {formatDuration(tool.startedAt, tool.endedAt)}
        </>
      );
    case "failed":
      return <span className="text-destructive">Failed</span>;
    case "skipped":
      return "Skipped";
    case "cancelled":
      return "Cancelled";
  }
}

function formatDuration(startedAt: number, endedAt: number | undefined): string {
  const ms = Math.max(0, (endedAt ?? startedAt) - startedAt);
  if (ms < 1000) {
    return `${Math.max(0.1, ms / 1000).toFixed(1)}s`;
  }
  return ms < 10_000 ? `${(ms / 1000).toFixed(1)}s` : `${Math.round(ms / 1000)}s`;
}
