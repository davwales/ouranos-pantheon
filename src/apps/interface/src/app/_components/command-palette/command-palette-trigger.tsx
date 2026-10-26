"use client";

import { Button } from "@/components/ui/button";
import { Search } from "lucide-react";
import { useSyncExternalStore } from "react";

function subscribeToNothing() {
  return () => {};
}

function useShortcutLabel(): string {
  return useSyncExternalStore(
    subscribeToNothing,
    () => (/mac|iphone|ipad/i.test(navigator.userAgent) ? "⌘K" : "Ctrl K"),
    () => "⌘K",
  );
}

export function CommandPaletteTrigger({ onClick }: { onClick: () => void }) {
  const shortcut = useShortcutLabel();

  return (
    <Button
      variant="outline"
      onClick={onClick}
      aria-label="Open command palette"
      className="gap-2 text-muted-foreground md:w-56 md:justify-start"
    >
      <Search className="size-4" />
      <span className="hidden md:inline">Search…</span>
      <kbd className="ml-auto hidden rounded border bg-muted px-1.5 font-mono text-[10px] md:inline">
        {shortcut}
      </kbd>
    </Button>
  );
}
