"use client";

import { cn } from "@/lib/utils";
import { ChevronRight } from "lucide-react";

// A div rather than a button: Hermes wraps whole messages in a context-menu trigger on mobile,
// and nested interactive elements are invalid. Events stop here so toggling a block never
// opens that menu.
export function BlockHeader({
  open,
  onOpenChange,
  controls,
  icon,
  label,
  meta,
  className,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  controls: string;
  icon?: React.ReactNode;
  label: React.ReactNode;
  meta?: React.ReactNode;
  className?: string;
}) {
  return (
    <div
      role="button"
      tabIndex={0}
      aria-expanded={open}
      aria-controls={controls}
      onClick={(e) => {
        e.stopPropagation();
        onOpenChange(!open);
      }}
      onKeyDown={(e) => {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          e.stopPropagation();
          onOpenChange(!open);
        }
      }}
      className={cn(
        "group/header flex w-full min-w-0 cursor-pointer select-none items-center gap-2 text-left text-sm text-muted-foreground outline-none transition-colors hover:text-foreground focus-visible:ring-[3px] focus-visible:ring-ring/50",
        className,
      )}
    >
      {icon && (
        <span className="flex size-4 shrink-0 items-center justify-center">
          {icon}
        </span>
      )}
      <span className="min-w-0 truncate">{label}</span>
      <span className="ml-auto flex shrink-0 items-center gap-1.5">
        {meta && <span className="text-xs tabular-nums">{meta}</span>}
        <ChevronRight
          aria-hidden
          className={cn(
            "size-3.5 shrink-0 transition-transform motion-reduce:transition-none",
            open && "rotate-90",
          )}
        />
      </span>
    </div>
  );
}
