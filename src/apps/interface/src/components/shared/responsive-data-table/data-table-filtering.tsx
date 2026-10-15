"use client";

import { FilterChips, SearchFilterInput } from "@/components/shared/search-filter";
import { type ComponentProps } from "react";
import { FilterBuilderDialog } from "./filter-builder-dialog";
import { filterGroupToQuery, removeFilterItem } from "./filter-group-converters";
import { type DataTableState, EMPTY_FILTER, type ExtendedColumnDef, type FilterGroup } from "./types";

export function DataTableFiltering<TData>({
  columns,
  state,
  onStateChange,
  ...props
}: ComponentProps<"div"> & {
  columns: ExtendedColumnDef<TData>[];
  state?: DataTableState;
  onStateChange?: (state: DataTableState) => void;
}) {
  const filter = state?.filter ?? EMPTY_FILTER;
  const query = state?.searchQuery ?? filterGroupToQuery(filter, columns);

  const applyFilter = (group: FilterGroup) => {
    onStateChange?.({ ...state, filter: group, searchQuery: filterGroupToQuery(group, columns) });
  };

  return (
    <div {...props}>
      <SearchFilterInput
        columns={columns}
        value={query}
        onQueryChange={(searchQuery) => onStateChange?.({ ...state, searchQuery })}
        onChange={(group) => onStateChange?.({ ...state, searchQuery: query, filter: group })}
        onClear={() => applyFilter(EMPTY_FILTER)}
        actions={<FilterBuilderDialog group={filter} columns={columns} onChange={applyFilter} />}
      />
      <FilterChips
        group={filter}
        columns={columns}
        onRemove={(path) => applyFilter(removeFilterItem(filter, path))}
      />
    </div>
  );
}
