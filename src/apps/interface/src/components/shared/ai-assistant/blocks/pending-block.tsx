import { Sparkles } from "lucide-react";

export function PendingBlock() {
  return (
    <div role="status" className="flex items-center gap-2 text-sm text-muted-foreground">
      <span className="flex size-4 items-center justify-center">
        <Sparkles aria-hidden className="size-3.5 motion-safe:animate-pulse" />
      </span>
      <span className="text-shimmer">Thinking…</span>
    </div>
  );
}
