"use client";

import { BlockHeader } from "@/components/shared/ai-assistant/blocks/block-header";
import { ToolDetails } from "@/components/shared/ai-assistant/blocks/tool-details";
import { type AssistantChatActionPart } from "@/components/shared/ai-assistant/assistant-chat-parts";
import { Button } from "@/components/ui/button";
import { Collapsible, CollapsibleContent } from "@/components/ui/collapsible";
import { type AssistantActionHandler } from "@/lib/api/assistant";
import { cn } from "@/lib/utils";
import { AlertCircle, Check, CircleSlash, Loader2, Sparkles, X } from "lucide-react";
import { useId, useState } from "react";

const STATUS_LABELS: Record<AssistantChatActionPart["status"], string> = {
  proposed: "Proposed change",
  applying: "Applying…",
  applied: "Applied",
  rejected: "Rejected",
  failed: "Couldn't apply",
};

export function ActionBlock({
  action,
  handler,
  context,
  onApply,
  onReject,
}: {
  action: AssistantChatActionPart;
  context?: unknown;
  handler: AssistantActionHandler<unknown> | undefined;
  onApply: () => void;
  onReject: () => void;
}) {
  const [open, setOpen] = useState(false);
  const contentId = useId();
  const isActionable = handler !== undefined;
  const isSettled = action.status === "applied" || action.status === "rejected";

  return (
    <Collapsible
      open={open}
      onOpenChange={setOpen}
      aria-label={`${STATUS_LABELS[action.status]}: ${action.name}`}
      role="group"
      className={cn(
        "min-w-0 overflow-hidden rounded-lg border bg-card text-card-foreground shadow-xs transition-opacity",
        action.status === "rejected" && "opacity-60",
      )}
    >
      <div className="flex items-start gap-3 p-3">
        <ActionStatusIcon status={isActionable ? action.status : "unavailable"} />
        <div className="min-w-0 flex-1">
          <div className="flex min-w-0 items-baseline gap-2">
            <span className="shrink-0 text-xs font-medium text-muted-foreground">
              {isActionable ? STATUS_LABELS[action.status] : "Unavailable"}
            </span>
            <code className="truncate font-mono text-[11px] text-muted-foreground/80">
              {action.name}
            </code>
          </div>
          <p
            className={cn(
              "mt-0.5 text-sm wrap-break-word",
              action.status === "rejected" && "line-through decoration-muted-foreground/60",
              !isActionable && "text-muted-foreground",
            )}
          >
            {isActionable ? handler.describe(action.action) : "This page can’t apply this change."}
          </p>
          {action.status === "failed" && action.error && (
            <p role="alert" className="mt-1 text-sm text-destructive">
              {action.error}
            </p>
          )}
        </div>
      </div>
      <div className="flex min-h-10 items-center gap-2 border-t bg-muted/30 px-3 py-1.5">
        <BlockHeader
          open={open}
          onOpenChange={setOpen}
          controls={contentId}
          label={<span className="text-xs">Details</span>}
          className="w-fit gap-1"
        />
        {isActionable && !isSettled && (
          <div className="ml-auto flex items-center gap-1.5">
            <Button
              size="sm"
              variant="ghost"
              className="h-7"
              disabled={action.status === "applying"}
              onClick={onReject}
            >
              Reject
            </Button>
            <Button
              size="sm"
              className="h-7"
              disabled={action.status === "applying"}
              onClick={onApply}
            >
              {action.status === "applying" && <Loader2 className="motion-safe:animate-spin" />}
              {action.status === "failed" ? "Retry" : "Apply"}
            </Button>
          </div>
        )}
      </div>
      <CollapsibleContent id={contentId} className="border-t bg-muted/30 px-3 pt-2 pb-3">
        <ToolDetails
          context={context}
          input={action.arguments}
          output={action.output}
          isTruncated={action.isTruncated}
          isSettled={action.output !== undefined}
        />
      </CollapsibleContent>
    </Collapsible>
  );
}

const STATUS_ICONS: Record<
  AssistantChatActionPart["status"] | "unavailable",
  { icon: React.ReactNode; tone: string }
> = {
  proposed: { icon: <Sparkles />, tone: "bg-primary/10 text-primary" },
  applying: {
    icon: <Loader2 className="motion-safe:animate-spin" />,
    tone: "bg-muted text-muted-foreground",
  },
  applied: {
    icon: <Check />,
    tone: "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400",
  },
  rejected: { icon: <X />, tone: "bg-muted text-muted-foreground" },
  failed: { icon: <AlertCircle />, tone: "bg-destructive/10 text-destructive" },
  unavailable: { icon: <CircleSlash />, tone: "bg-muted text-muted-foreground" },
};

function ActionStatusIcon({
  status,
}: {
  status: AssistantChatActionPart["status"] | "unavailable";
}) {
  const { icon, tone } = STATUS_ICONS[status];

  return (
    <span
      aria-hidden
      className={cn(
        "mt-0.5 flex size-6 shrink-0 items-center justify-center rounded-md",
        tone,
      )}
    >
      {icon}
    </span>
  );
}
