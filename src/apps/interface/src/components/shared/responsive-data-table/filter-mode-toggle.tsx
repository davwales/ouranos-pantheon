"use client";

import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import { Fragment, type ComponentType } from "react";
import type { FilterMode } from "./types";

export type FilterModeDef = {
  id: FilterMode;
  label: string;
  icon?: ComponentType<{ className?: string }>;
  tooltip?: string;
};

type FilterModeToggleProps = {
  modes: FilterModeDef[];
  activeMode: FilterMode;
  onModeChange: (mode: FilterMode) => void;
};

export function FilterModeToggle({
  modes,
  activeMode,
  onModeChange,
}: FilterModeToggleProps) {
  return (
    <TooltipProvider>
      <div
        className="inline-flex items-center rounded-lg border border-input bg-muted p-1"
        role="tablist"
        aria-label="Filter mode"
      >
        {modes.map((mode) => {
          const isActive = mode.id === activeMode;
          const Icon = mode.icon;

          const button = (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              role="tab"
              aria-selected={isActive}
              className={cn(
                "flex-1 justify-center",
                isActive &&
                  "bg-background text-foreground shadow-sm hover:bg-background",
              )}
              onClick={() => onModeChange(mode.id)}
            >
              {Icon && <Icon className="h-4 w-4 mr-1" />}
              {mode.label}
            </Button>
          );

          if (mode.tooltip) {
            return (
              <Tooltip key={mode.id}>
                <TooltipTrigger asChild>{button}</TooltipTrigger>
                <TooltipContent>{mode.tooltip}</TooltipContent>
              </Tooltip>
            );
          }

          return (
            <Fragment key={mode.id}>{button}</Fragment>
          );
        })}
      </div>
    </TooltipProvider>
  );
}
