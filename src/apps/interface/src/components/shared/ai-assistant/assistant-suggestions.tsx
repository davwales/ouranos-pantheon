"use client";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export function AssistantSuggestions({
  suggestions,
  disabled,
  onSelect,
  className,
}: {
  suggestions: string[];
  disabled?: boolean;
  onSelect: (suggestion: string) => void;
  className?: string;
}) {
  return (
    <ul
      aria-label="Suggested questions"
      className={cn("flex flex-wrap justify-center gap-2", className)}
    >
      {suggestions.map((suggestion) => (
        <li key={suggestion}>
          <Button
            type="button"
            variant="outline"
            size="sm"
            className="h-auto whitespace-normal py-1.5 text-left"
            disabled={disabled}
            onClick={() => onSelect(suggestion)}
          >
            {suggestion}
          </Button>
        </li>
      ))}
    </ul>
  );
}
