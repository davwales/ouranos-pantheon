"use client";

import { Input } from "@/components/ui/input";
import { useApi } from "@/hooks/use-api";
import { useDebounce } from "@/hooks/use-debounce";
import { plutusApi } from "@/lib/api/plutus";
import { useState } from "react";

export interface SelectedSymbol {
  id: string;
  name: string;
}

export function SymbolSearch({
  marketId,
  numResults = 10,
  onSymbolSelected,
  ...props
}: React.ComponentProps<"div"> & {
  marketId: string;
  numResults?: number;
  onSymbolSelected?: (symbol: SelectedSymbol) => void;
}) {
  const [search, setSearch] = useState("");
  const [selectedSymbol, setSelectedSymbol] = useState<SelectedSymbol>();
  const debouncedSearch = useDebounce(search);

  const take = Math.max(1, Math.min(numResults, 50));

  const filter = [
    `marketId:eq:${marketId}`,
    ...(debouncedSearch ? [`name:like:${debouncedSearch}`] : []),
  ];

  const [state] = useApi(
    () => plutusApi.getAllSymbols({ filter, skip: 0, take }),
    [marketId, debouncedSearch, take],
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
        {state.data?.items?.map((symbol) => (
          <div
            key={symbol.id}
            onClick={() =>
              handleSymbolSelected({ id: symbol.id, name: symbol.name })
            }
            className={`cursor-pointer border-2 rounded-md p-2 hover:bg-accent ${selectedSymbol?.id === symbol.id ? "bg-accent" : ""}`}
          >
            {symbol.name}
          </div>
        ))}
      </div>
    </div>
  );
}
