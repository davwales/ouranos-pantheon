import { filterGroupToQuery } from "@/components/shared/responsive-data-table/filter-group-converters";
import {
  type DataTableState,
  type ExtendedColumnDef,
  type FilterCondition,
  type FilterGroup,
  type FilterOperator,
} from "@/components/shared/responsive-data-table/types";
import { type AssistantActionHandler } from "@/lib/api/assistant";
import { type SetExplorerViewAction } from "@/lib/api/plutus-types";

const OPERATORS: Record<string, FilterOperator> = {
  eq: "eq",
  neq: "neq",
  gt: "gt",
  gte: "gte",
  lt: "lt",
  lte: "lte",
  like: "contains",
};

function toFilterGroup<TData>(
  action: SetExplorerViewAction,
  columns: ExtendedColumnDef<TData>[],
): FilterGroup {
  const conditions: FilterCondition[] = action.conditions.map((condition) => {
    const column = columns.find((c) => c.id === condition.field);
    return {
      field: condition.field,
      operator: OPERATORS[condition.operator] ?? "eq",
      value:
        column?.filterConfig?.type === "number"
          ? Number(condition.value)
          : condition.value,
    };
  });

  return { logic: "and", items: conditions };
}

export function applyExplorerView<TData>(
  state: DataTableState,
  action: SetExplorerViewAction,
  columns: ExtendedColumnDef<TData>[],
): DataTableState {
  const filter = toFilterGroup(action, columns);

  return {
    ...state,
    filter,
    searchQuery: filterGroupToQuery(filter, columns),
    sort: action.sortField
      ? { [action.sortField]: action.descending ? "DESC" : "ASC" }
      : state.sort,
    pagination: { ...state.pagination, skip: 0 },
  };
}

export function explorerViewHandler<TData>(
  columns: ExtendedColumnDef<TData>[],
  apply: (action: SetExplorerViewAction) => void,
): AssistantActionHandler<SetExplorerViewAction> {
  return {
    describe: (action) => {
      const query = filterGroupToQuery(toFilterGroup(action, columns), columns);
      const sortColumn = columns.find((c) => c.id === action.sortField);
      const sort = sortColumn
        ? `, sorted by ${sortColumn.header} ${action.descending ? "descending" : "ascending"}`
        : "";
      return `Showing ${query || "all symbols"}${sort}`;
    },
    apply,
  };
}
