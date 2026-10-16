"use client";

import { Button } from "@/components/ui/button";

export function ActionGroupBar({
  count,
  disabled,
  onApplyAll,
  onRejectAll,
}: {
  count: number;
  disabled: boolean;
  onApplyAll: () => void;
  onRejectAll: () => void;
}) {
  return (
    <div className="flex min-w-0 flex-wrap items-center justify-between gap-2 rounded-lg border border-dashed px-3 py-2">
      <span className="text-sm text-muted-foreground">{count} changes proposed</span>
      <div className="flex items-center gap-1.5">
        <Button size="sm" variant="ghost" className="h-7" disabled={disabled} onClick={onRejectAll}>
          Reject all
        </Button>
        <Button size="sm" variant="outline" className="h-7" disabled={disabled} onClick={onApplyAll}>
          Apply all
        </Button>
      </div>
    </div>
  );
}
