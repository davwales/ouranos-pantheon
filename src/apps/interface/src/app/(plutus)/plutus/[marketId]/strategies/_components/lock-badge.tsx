import { cn } from "@/lib/utils";
import { Lock, LockOpen } from "lucide-react";

export type LockState = "locked" | "unlocked" | "fixed";

const STATE_CLASSES: Record<LockState, string> = {
  locked: "bg-primary text-primary-foreground",
  unlocked: "text-muted-foreground",
  fixed: "bg-muted text-muted-foreground",
};

export function LockBadge({
  state,
  className,
}: {
  state: LockState;
  className?: string;
}) {
  const Icon = state === "unlocked" ? LockOpen : Lock;
  return (
    <span
      className={cn(
        "inline-flex size-6 shrink-0 items-center justify-center rounded-full transition-colors",
        STATE_CLASSES[state],
        className,
      )}
    >
      <Icon className="size-3.5" />
    </span>
  );
}
