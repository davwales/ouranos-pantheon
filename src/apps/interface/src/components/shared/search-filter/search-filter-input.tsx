"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useDebounce } from "@/hooks/use-debounce";
import { cn } from "@/lib/utils";
import { Search, X } from "lucide-react";
import React, { useMemo, useRef } from "react";
import { parseQuery } from "../responsive-data-table/filter-group-converters";
import {
  getSearchSuggestions,
  type SearchSuggestion,
} from "../responsive-data-table/search-suggestions";
import {
  type ExtendedColumnDef,
  type FilterGroup,
} from "../responsive-data-table/types";

type SearchFilterInputProps = {
  columns: ExtendedColumnDef<any>[];
  value: string;
  onQueryChange: (query: string) => void;
  onChange: (group: FilterGroup) => void;
  onClear: () => void;
  actions?: React.ReactNode;
  className?: string;
};

function SuggestionList({
  suggestions,
  onSelect,
}: {
  suggestions: SearchSuggestion[];
  onSelect: (suggestion: SearchSuggestion) => void;
}) {
  return (
    <div
      className="mt-2 flex flex-wrap items-center gap-1"
      aria-label="Search suggestions"
    >
      {suggestions.map((suggestion) => (
        <Button
          key={suggestion.query}
          type="button"
          variant="outline"
          size="sm"
          className="h-6 px-2 text-xs"
          onClick={() => onSelect(suggestion)}
        >
          {suggestion.label}
        </Button>
      ))}
      <span className="text-xs text-muted-foreground">Tab to accept</span>
    </div>
  );
}

export function SearchFilterInput({
  columns,
  value,
  onQueryChange,
  onChange,
  onClear,
  actions,
  className,
}: SearchFilterInputProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const typedRef = useRef(false);
  const debouncedQuery = useDebounce(value);

  const debouncedResult = useMemo(
    () => parseQuery(debouncedQuery, columns),
    [debouncedQuery, columns],
  );

  const suggestions = useMemo(
    () => getSearchSuggestions(value, columns),
    [value, columns],
  );

  const onChangeRef = useRef(onChange);

  React.useEffect(() => {
    onChangeRef.current = onChange;
  });

  React.useEffect(() => {
    if (!typedRef.current || "error" in debouncedResult) return;
    typedRef.current = false;
    onChangeRef.current(debouncedResult.group);
  }, [debouncedResult]);

  const handleQueryChange = (query: string) => {
    typedRef.current = true;
    onQueryChange(query);
  };

  const acceptSuggestion = (suggestion: SearchSuggestion) => {
    handleQueryChange(suggestion.query);
    inputRef.current?.focus();
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Tab" && !event.shiftKey && suggestions.length > 0) {
      event.preventDefault();
      acceptSuggestion(suggestions[0]);
    }
  };

  const hintId = "search-filter-hint";
  const hint =
    value === debouncedQuery && "error" in debouncedResult
      ? debouncedResult.error
      : null;

  return (
    <div className={className}>
      <div className="flex items-center gap-2">
        <div className="relative flex-1">
          <Search className="pointer-events-none absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            ref={inputRef}
            type="text"
            value={value}
            onChange={(e) => handleQueryChange(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Search, or filter like “Volume > 1000”"
            aria-label="Search and filter"
            aria-describedby={hint ? hintId : undefined}
            className={cn("pl-8", value.length > 0 && "pr-8")}
          />
          {value.length > 0 && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              aria-label="Clear search"
              className="absolute right-1 top-1/2 h-7 w-7 -translate-y-1/2 p-0"
              onClick={() => {
                typedRef.current = false;
                onClear();
              }}
            >
              <X className="h-4 w-4" />
            </Button>
          )}
        </div>
        {actions}
      </div>
      {suggestions.length > 0 && (
        <SuggestionList suggestions={suggestions} onSelect={acceptSuggestion} />
      )}
      {hint && suggestions.length === 0 && (
        <p
          id={hintId}
          aria-live="polite"
          className="mt-2 text-xs text-muted-foreground"
        >
          {hint}
        </p>
      )}
    </div>
  );
}
