"use client";

import { Loader2, Minimize2 } from "lucide-react";

export type SummaryViewProps = {
  content: string;
  isCompacting: boolean;
  compactionError: string | null;
  isLastSummary: boolean;
  onRetryCompact: () => void;
};

export default function SummaryView({
  content,
  isCompacting,
  compactionError,
  isLastSummary,
  onRetryCompact,
}: SummaryViewProps) {
  if (isCompacting && content === "") {
    return (
      <div className="flex items-center gap-1.5 mx-2 my-4 text-xs text-muted-foreground">
        <Loader2 className="h-3 w-3 animate-spin" />
        <span>Compacting…</span>
      </div>
    );
  }

  if (isCompacting && content !== "") {
    return null;
  }

  if (compactionError && isLastSummary) {
    return (
      <div className="mx-2 my-2 p-3 rounded-md bg-destructive/10 text-destructive text-sm flex items-center justify-between gap-2">
        <span>{compactionError}</span>
        <button
          onClick={onRetryCompact}
          className="px-3 py-1 rounded-md bg-destructive text-destructive-foreground text-xs font-medium hover:bg-destructive/90 transition-colors"
        >
          Retry
        </button>
      </div>
    );
  }

  if (!isCompacting && content !== "") {
    return (
      <div className="flex items-center gap-3 mx-2 my-4">
        <div className="flex-1 h-px bg-border" />
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground shrink-0">
          <Minimize2 className="h-3 w-3" />
          <span>Conversation compacted</span>
        </div>
        <div className="flex-1 h-px bg-border" />
      </div>
    );
  }

  return null;
}
