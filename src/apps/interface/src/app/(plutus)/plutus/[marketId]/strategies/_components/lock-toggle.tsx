"use client";

import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { LockBadge } from "./lock-badge";

type LockToggleProps = {
  label: string;
  locked: boolean;
  onToggle?: () => void;
  disabledReason?: string;
};

function tooltipFor({ locked, onToggle, disabledReason }: LockToggleProps) {
  if (!onToggle) return "Kept as set: never optimized";
  if (disabledReason) return disabledReason;
  return locked
    ? "Locked: the optimizer keeps this value"
    : "Unlocked: the optimizer may change this value";
}

export function LockToggle(props: LockToggleProps) {
  const { label, locked, onToggle, disabledReason } = props;
  const tooltip = tooltipFor(props);

  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>
          {onToggle ? (
            <span className="inline-flex shrink-0">
              <button
                type="button"
                aria-label={`Lock ${label}`}
                aria-pressed={locked}
                disabled={!!disabledReason}
                onClick={onToggle}
                className="rounded-full outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:pointer-events-none disabled:opacity-40"
              >
                <LockBadge
                  state={locked ? "locked" : "unlocked"}
                  className={
                    locked
                      ? "hover:bg-primary/85"
                      : "hover:bg-muted hover:text-foreground"
                  }
                />
              </button>
            </span>
          ) : (
            <span role="img" aria-label={`${label}: ${tooltip}`}>
              <LockBadge state="fixed" />
            </span>
          )}
        </TooltipTrigger>
        <TooltipContent>{tooltip}</TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
}
