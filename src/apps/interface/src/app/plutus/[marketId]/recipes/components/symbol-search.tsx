"use client";

import { Input } from "@/components/ui/input";
import { useApi } from "@/hooks/use-api";
import { useDebounce } from "@/hooks/use-debounce";
import { plutusApi } from "@/lib/api/plutus";
import { Check } from "lucide-react";
import { useMemo, useState } from "react";

export interface SelectedSymbol {
  id: string;
  name: string;
}

export function SymbolSearch({
  marketId,
  numResults = 10,
  excludeIds,
  onSymbolSelected,
  ...props
}: React.ComponentProps<"div"> & {
  marketId: string;
  numResults?: number;
  excludeIds?: string[];
  onSymbolSelected?: (symbol: SelectedSymbol) => void;
}) {
  const [search, setSearch] = useState("");
  const [selectedSymbol, setSelectedSymbol] = useState<SelectedSymbol>();
  const debouncedSearch = useDebounce(search);

  const take = Math.max(1, Math.min(numResults, 50));

  const filter = useMemo(
    () => [
      `marketId:eq:${marketId}`,
      ...(debouncedSearch ? [`name:like:${debouncedSearch}`] : []),
    ],
    [marketId, debouncedSearch],
  );

  const [state] = useApi(
    () => plutusApi.getAllSymbols({ filter, skip: 0, take }),
    [filter, take],
  );

  const handleSymbolSelected = (symbol: SelectedSymbol) => {
    setSelectedSymbol(symbol);
    onSymbolSelected?.(symbol);
  };

  return (
    <div {...props}>
      <Input
        value={search}
        placeholder="Search for a symbol"
        onChange={(e) => setSearch(e.target.value)}
      />
      <div className="mt-4 space-y-2">
        {state.data?.items?.map((symbol) => {
          const alreadyAdded = excludeIds?.includes(symbol.id);
          return (
            <div
              key={symbol.id}
              onClick={() =>
                !alreadyAdded &&
                handleSymbolSelected({ id: symbol.id, name: symbol.name })
              }
              className={`flex items-center justify-between rounded-md p-2 border-2 ${
                alreadyAdded
                  ? "opacity-50 cursor-not-allowed"
                  : `cursor-pointer hover:bg-accent ${selectedSymbol?.id === symbol.id ? "bg-accent" : ""}`
              }`}
            >
              <span>{symbol.name}</span>
              {alreadyAdded && (
                <Check className="w-4 h-4 text-muted-foreground" />
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
