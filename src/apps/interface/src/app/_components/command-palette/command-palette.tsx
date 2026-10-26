"use client";

import {
  Command,
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandLoading,
} from "@/components/ui/command";
import { useTheme } from "next-themes";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { CommandPaletteTrigger } from "./command-palette-trigger";
import { type PaletteEntry } from "./command-palette-types";
import { isPaletteShortcut } from "./is-palette-shortcut";
import { useCommandPaletteData } from "./use-command-palette-data";

export function CommandPalette() {
  const router = useRouter();
  const { setTheme } = useTheme();
  const [open, setOpen] = useState(false);
  const [isActivated, setIsActivated] = useState(false);
  const [query, setQuery] = useState("");
  const { groups, isSearching } = useCommandPaletteData(isActivated, query);

  const handleOpenChange = (next: boolean) => {
    setOpen(next);
    setQuery("");
    if (next) {
      setIsActivated(true);
    }
  };

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (!isPaletteShortcut(event)) {
        return;
      }
      event.preventDefault();
      setOpen((current) => !current);
      setQuery("");
      setIsActivated(true);
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, []);

  const handleSelect = (entry: PaletteEntry) => {
    handleOpenChange(false);
    if (entry.target.kind === "link") {
      router.push(entry.target.href);
    } else {
      setTheme(entry.target.theme);
    }
  };

  return (
    <>
      <CommandPaletteTrigger onClick={() => handleOpenChange(true)} />
      <CommandDialog
        open={open}
        onOpenChange={handleOpenChange}
        title="Command Palette"
        description="Search markets, symbols, strategies, recipes and conversations, or run an action."
        showCloseButton={false}
      >
        <Command shouldFilter={false}>
          <CommandInput
            value={query}
            onValueChange={setQuery}
            placeholder="Search markets, symbols, recipes, chats…"
          />
          <CommandList>
            {isSearching && <CommandLoading>Searching…</CommandLoading>}
            {!isSearching && <CommandEmpty>No results found.</CommandEmpty>}
            {groups.map((group) => (
              <CommandGroup key={group.heading} heading={group.heading}>
                {group.entries.map((entry) => (
                  <CommandItem
                    key={entry.id}
                    value={entry.id}
                    onSelect={() => handleSelect(entry)}
                  >
                    <group.icon />
                    <span className="truncate">{entry.label}</span>
                    {entry.hint && (
                      <span className="ml-auto shrink-0 text-xs text-muted-foreground">
                        {entry.hint}
                      </span>
                    )}
                  </CommandItem>
                ))}
              </CommandGroup>
            ))}
          </CommandList>
        </Command>
      </CommandDialog>
    </>
  );
}
