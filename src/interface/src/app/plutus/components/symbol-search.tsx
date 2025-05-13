"use client";

import { SEARCH_SYMBOLS } from "@/app/plutus/queries";
import { Input } from "@/components/ui/input";
import { useQuery } from "@urql/next";
import { useState } from "react";

export interface SelectedSymbol {
    id: string;
    name: string;
}

export function SymbolSearch({
    numResults = 10,
    onSymbolSelected,
    ...props
}: React.ComponentProps<"div"> & {
    numResults?: number;
    onSymbolSelected?: (symbol: SelectedSymbol) => void;
}) {
    const [search, setSearch] = useState("");
    const [selectedSymbol, setSelectedSymbol] = useState<SelectedSymbol>();

    const [{ data }] = useQuery({
        query: SEARCH_SYMBOLS,
        variables: {
            query: search,
            first: Math.max(1, Math.min(numResults, 50)),
        }
    });

    const handleSymbolSelected = (symbol: SelectedSymbol) => {
        setSelectedSymbol(symbol);
        onSymbolSelected?.(symbol);
    }

    return (
        <div {...props}>
            <Input
                value={search}
                placeholder="Search for a symbol"
                onChange={(e) => setSearch(e.target.value)}
            />
            <div className="mt-4 space-y-2">
                {data?.allSymbols?.nodes?.map((symbol) => (
                    <div
                        key={symbol.id}
                        onClick={() => handleSymbolSelected({ id: symbol.id, name: symbol.name })}
                        className={`cursor-pointer border-2 rounded-md p-2 hover:bg-accent ${selectedSymbol?.id === symbol.id ? "bg-accent" : ""}`}
                    >
                        {symbol.name}
                    </div>
                ))}
            </div>
        </div>
    );
}
