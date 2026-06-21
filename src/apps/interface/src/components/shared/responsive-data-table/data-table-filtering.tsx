"use client";

import { SmartFilterInput } from "@/components/shared/smart-filter";
import { FilterModeToggle } from "./filter-mode-toggle";
import type { FilterModeDef } from "./filter-mode-toggle";
import { Sparkles } from "lucide-react";
import { type ComponentProps } from "react";
import { filterGroupToQuery } from "./filter-group-converters";
import { FilterGroupBuilder } from "./filter-group-builder";
import {
  type DataTableState,
  DEFAULT_FILTER_MODE,
  EMPTY_FILTER,
  type ExtendedColumnDef,
  type FilterMode,
} from "./types";

const FILTER_MODES: FilterModeDef[] = [
  { id: "builder", label: "Builder" },
  {
    id: "smart",
    label: "Smart",
    icon: Sparkles,
    tooltip:
      "Filter with natural language, e.g. 'symbolName contains gold and limit > 100'",
  },
];

function transitionMode(
  to: FilterMode,
  state: DataTableState | undefined,
  columns: ExtendedColumnDef<any>[],
): DataTableState {
  if (to === "smart") {
    const currentGroup = state?.filter ?? EMPTY_FILTER;
    return { ...state, filterMode: to, smartQuery: filterGroupToQuery(currentGroup, columns) };
  }
  return { ...state, filterMode: to };
}

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
  const mode = state?.filterMode ?? DEFAULT_FILTER_MODE;
  const currentState = state ?? { filterMode: DEFAULT_FILTER_MODE };

  const handleModeChange = (newMode: FilterMode) => {
    onStateChange?.(transitionMode(newMode, state, columns));
  };

  return (
    <div {...props}>
      <FilterModeToggle
        modes={FILTER_MODES}
        activeMode={mode}
        onModeChange={handleModeChange}
      />
      {mode === "builder" ? (
        <div className="mt-2">
        <FilterGroupBuilder
          group={currentState.filter ?? EMPTY_FILTER}
          columns={columns}
          depth={0}
          onChange={(newGroup) =>
            onStateChange?.({ ...currentState, filter: newGroup })
          }
        />
        </div>
      ) : (
        <SmartFilterInput
          columns={columns}
          value={currentState.smartQuery ?? ""}
          onQueryChange={(query) =>
            onStateChange?.({ ...currentState, smartQuery: query })
          }
          onChange={(result) => {
            if (result) {
              onStateChange?.({ ...currentState, filter: result });
            }
          }}
          className="mt-2"
        />
      )}
    </div>
  );
}
