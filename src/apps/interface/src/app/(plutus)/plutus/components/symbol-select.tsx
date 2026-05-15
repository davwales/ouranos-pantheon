"use client";

import { Input } from "@/components/ui/input";
import { useApi } from "@/hooks/use-api";
import { useDebounce } from "@/hooks/use-debounce";
import { type Symbol, plutusApi } from "@/lib/api/plutus";
import { useMemo, useRef, useState } from "react";

export interface SelectedSymbol {
  id: string;
  name: string;
}

export function SymbolSelect({
  marketId,
  value,
  onChange,
}: {
  marketId: string;
  value: SelectedSymbol | null;
  onChange: (symbol: SelectedSymbol | null) => void;
}) {
  const [search, setSearch] = useState("");
  const [isOpen, setIsOpen] = useState(false);
  const [hasFocus, setHasFocus] = useState(false);
  const debouncedSearch = useDebounce(search);
  const containerRef = useRef<HTMLDivElement>(null);

  const take = 10;

  const shouldFetch = hasFocus && isOpen;
  const filter = useMemo(
    () => [
      `marketId:eq:${marketId}`,
      ...(shouldFetch && debouncedSearch
        ? [`name:like:${debouncedSearch}`]
        : []),
    ],
    [marketId, shouldFetch, debouncedSearch],
  );

  const [state] = useApi(
    () =>
      shouldFetch
        ? plutusApi.getAllSymbols({ filter, skip: 0, take })
        : Promise.resolve({ items: [] as Symbol[], totalCount: 0, skip: 0, take: 0 }),
    [shouldFetch, filter, take],
  );

  const displayValue = value?.name ?? "";

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const next = e.target.value;
    setSearch(next);
    if (value) {
      onChange(null);
    }
    if (!isOpen) {
      setIsOpen(true);
    }
  };

  const handleSelect = (symbol: SelectedSymbol) => {
    onChange(symbol);
    setSearch("");
    setIsOpen(false);
    setHasFocus(false);
  };

  const handleFocus = () => {
    setHasFocus(true);
    if (value) {
      onChange(null);
      setSearch(value.name);
    }
    setIsOpen(true);
  };

  const handleBlur = () => {
    setTimeout(() => {
      setIsOpen(false);
      setHasFocus(false);
      setSearch("");
    }, 150);
  };

  const items = state.data?.items ?? [];

  return (
    <div ref={containerRef} className="relative">
      <Input
        value={hasFocus ? search : displayValue}
        placeholder="Search for a symbol"
        onChange={handleInputChange}
        onFocus={handleFocus}
        onBlur={handleBlur}
      />
      {isOpen && items.length > 0 && (
        <div className="absolute z-50 mt-1 w-full rounded-md border bg-card shadow-lg max-h-60 overflow-auto">
          {items.map((symbol) => (
            <div
              key={symbol.id}
              onMouseDown={(e) => e.preventDefault()}
              onClick={() =>
                handleSelect({ id: symbol.id, name: symbol.name })
              }
              className="cursor-pointer px-3 py-2 hover:bg-accent"
            >
              <span className="font-medium">{symbol.name}</span>
              {symbol.subcode && (
                <span className="ml-1 text-muted-foreground text-sm">
                  ({symbol.subcode})
                </span>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}