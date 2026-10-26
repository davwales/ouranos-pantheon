"use client";

import { useApi } from "@/hooks/use-api";
import { useDebounce } from "@/hooks/use-debounce";
import { plutusApi } from "@/lib/api/plutus";
import { useMemo } from "react";
import { buildStaticGroups } from "./build-static-groups";
import { MIN_SEARCH_LENGTH } from "./command-palette-constants";
import { type PaletteGroup } from "./command-palette-types";
import { filterGroups } from "./filter-groups";
import { searchEntityGroups } from "./search-entity-groups";

export function useCommandPaletteData(
  isActivated: boolean,
  query: string,
): { groups: PaletteGroup[]; isSearching: boolean } {
  const trimmedQuery = query.trim();
  const debouncedQuery = useDebounce(trimmedQuery);
  const shouldSearch = debouncedQuery.length >= MIN_SEARCH_LENGTH;

  const [marketsState] = useApi(
    () => (isActivated ? plutusApi.getAllMarkets() : Promise.resolve([])),
    [isActivated],
  );
  const markets = useMemo(() => marketsState.data ?? [], [marketsState.data]);

  const [searchState] = useApi(
    () =>
      shouldSearch
        ? searchEntityGroups(debouncedQuery, markets)
        : Promise.resolve([]),
    [debouncedQuery, shouldSearch, markets.map((m) => m.id)],
  );

  const staticGroups = useMemo(() => buildStaticGroups(markets), [markets]);

  const groups = useMemo(() => {
    const filtered = filterGroups(staticGroups, trimmedQuery);
    if (trimmedQuery.length < MIN_SEARCH_LENGTH) {
      return filtered;
    }
    return [...filtered, ...(searchState.data ?? [])];
  }, [staticGroups, trimmedQuery, searchState.data]);

  const isSearching =
    trimmedQuery.length >= MIN_SEARCH_LENGTH &&
    (trimmedQuery !== debouncedQuery || searchState.status === "loading");

  return { groups, isSearching };
}
