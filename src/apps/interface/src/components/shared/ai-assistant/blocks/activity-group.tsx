"use client";

import { BlockHeader } from "@/components/shared/ai-assistant/blocks/block-header";
import { ReasoningBlock } from "@/components/shared/ai-assistant/blocks/reasoning-block";
import {
  formatDuration,
  ToolBlock,
  toolDurationMs,
} from "@/components/shared/ai-assistant/blocks/tool-block";
import {
  type AssistantChatReasoningPart,
  type AssistantChatToolPart,
} from "@/components/shared/ai-assistant/assistant-chat-parts";
import { Collapsible, CollapsibleContent } from "@/components/ui/collapsible";
import { cn } from "@/lib/utils";
import { Wrench } from "lucide-react";
import { useId, useState } from "react";

export type ActivityPart = AssistantChatReasoningPart | AssistantChatToolPart;

const COLLAPSE_AT = 4;

// The reasoning and tool calls between two pieces of the reply, in one stack. While the model is
// still working through them every row stays visible; once settled, a long run folds into a
// summary so the answer stays in view.
export function ActivityGroup({
  parts,
  isActive,
  streamingId,
  context,
}: {
  parts: ActivityPart[];
  isActive: boolean;
  streamingId?: string;
  context?: unknown;
}) {
  const [open, setOpen] = useState(false);
  const contentId = useId();
  const tools = parts.filter((part) => part.kind === "tool");
  const isCollapsible =
    !isActive &&
    parts.length >= COLLAPSE_AT &&
    tools.every((tool) => tool.status !== "running");
  const failed = tools.filter((tool) => tool.status === "failed").length;

  return (
    <Collapsible
      open={!isCollapsible || open}
      onOpenChange={setOpen}
      className="min-w-0 overflow-hidden rounded-lg border bg-card/40"
    >
      {isCollapsible && (
        <BlockHeader
          open={open}
          onOpenChange={setOpen}
          controls={contentId}
          icon={<Wrench aria-hidden />}
          label={<span className="text-xs">{`Used ${tools.length} ${tools.length === 1 ? "tool" : "tools"}`}</span>}
          meta={
            failed > 0 ? (
              <span className="text-destructive">{failed} failed</span>
            ) : (
              formatTotal(parts)
            )
          }
          className="px-3 py-2 hover:bg-muted/50"
        />
      )}
      <CollapsibleContent id={contentId} className={cn("divide-y", isCollapsible && "border-t")}>
        {parts.map((part) =>
          part.kind === "tool" ? (
            <ToolBlock key={part.id} tool={part} context={context} />
          ) : (
            <ReasoningBlock
              key={part.id}
              variant="row"
              content={part.content}
              isStreaming={part.id === streamingId}
              startedAt={part.startedAt}
              endedAt={part.endedAt}
            />
          ),
        )}
      </CollapsibleContent>
    </Collapsible>
  );
}

function formatTotal(parts: ActivityPart[]): string | undefined {
  const starts = parts.flatMap((part) => (part.startedAt === undefined ? [] : [part.startedAt]));
  const ends = parts.flatMap((part) => (part.endedAt === undefined ? [] : [part.endedAt]));

  if (starts.length > 0 && ends.length > 0) {
    return formatDuration(Math.max(...ends) - Math.min(...starts));
  }
  const durations = parts.flatMap((part) => {
    const durationMs = part.kind === "tool" ? toolDurationMs(part) : undefined;
    return durationMs === undefined ? [] : [durationMs];
  });

  return durations.length === 0
    ? undefined
    : formatDuration(durations.reduce((total, durationMs) => total + durationMs, 0));
}
