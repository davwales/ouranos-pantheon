"use client";

import { type MenuAction } from "@/components/shared/responsive-context-menu";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

/**
 * Inline buttons for a message's actions. They appear on hover or keyboard focus of the
 * surrounding `group/message`, stay visible on touch screens (which cannot hover), and are always
 * visible when `isPinned`.
 */
export function MessageActionBar({
  actions,
  disabled = false,
  isPinned = false,
  className,
  ...props
}: React.ComponentProps<"div"> & {
  actions: MenuAction[];
  disabled?: boolean;
  isPinned?: boolean;
}) {
  return (
    <div
      role="toolbar"
      aria-label="Message actions"
      className={cn(
        "flex gap-0.5 transition-opacity",
        isPinned
          ? "opacity-100"
          : "opacity-0 group-hover/message:opacity-100 group-focus-within/message:opacity-100 [@media(hover:none)]:opacity-100",
        className,
      )}
      {...props}
    >
      {actions.map((action) => (
        <Button
          key={action.label}
          type="button"
          variant="ghost"
          size="icon-sm"
          aria-label={action.label}
          title={action.label}
          disabled={disabled}
          onClick={action.onClick}
          className="text-muted-foreground hover:text-foreground"
        >
          {action.icon}
        </Button>
      ))}
    </div>
  );
}
