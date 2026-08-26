import { ColumnDef } from "@tanstack/react-table";
export type FilterMode = "builder" | "smart";

export const DEFAULT_FILTER_MODE: FilterMode = "builder";

export type SortDirection = "ASC" | "DESC";

export interface PaginationArgs {
  pageSize?: number;
  pageSizes?: number[];
  skip?: number;
  take?: number;
}

export interface PageInfo {
  totalCount?: number;
  skip?: number;
  take?: number;
  hasNextPage?: boolean | null | undefined;
  hasPreviousPage?: boolean | null | undefined;
}

export type FilterOperator =
  | "eq"
  | "neq"
  | "gt"
  | "gte"
  | "lt"
  | "lte"
  | "contains"
  | "startsWith"
  | "endsWith";

export const OPERATOR_LABELS: Record<FilterOperator, string> = {
  eq: "Equals",
  neq: "Not Equals",
  gt: "Greater Than",
  gte: "Greater Than or Equal",
  lt: "Less Than",
  lte: "Less Than or Equal",
  contains: "Contains",
  startsWith: "Starts With",
  endsWith: "Ends With",
};

export interface FilterCondition {
  field: string;
  operator: FilterOperator;
  value: any;
}

export type FilterGroup = {
  logic: "and" | "or";
  items: FilterGroupItem[];
};

export type FilterGroupItem = FilterCondition | FilterGroup;

export type FilterType = "string" | "number" | "boolean" | "date" | "enum";

export interface FilterConfig {
  type: FilterType;
  operators: FilterOperator[];
  enumValues?: string[];
}

export type SortValue = SortDirection | Record<string, SortDirection>;
export type SortArgs = Record<string, SortValue>;

export type ExtendedColumnDef<TData> = ColumnDef<TData> & {
  id: string;
  header: string;
  filterConfig?: FilterConfig;
};

export interface DataTableProps<TData> {
  columns: ExtendedColumnDef<TData>[];
  data: TData[] | null | undefined;
  loading?: boolean;
  skeletonRows?: number;
  scrollTop?: boolean;
  state?: DataTableState;
  onStateChange?: (state: DataTableState) => void;
  pageInfo?: PageInfo | null | undefined;
  disablePagination?: boolean;
  disableSorting?: boolean;
  disableFiltering?: boolean;
}

export interface DataTableState {
  pagination?: PaginationArgs;
  filter?: FilterGroup;
  sort?: SortArgs;
  filterMode: FilterMode;
  smartQuery?: string;
}

/** Maps frontend FilterOperator values to the backend filter operator strings */
export const BACKEND_OPERATOR: Record<FilterOperator, string> = {
  eq: "eq",
  neq: "neq",
  gt: "gt",
  gte: "gte",
  lt: "lt",
  lte: "lte",
  contains: "like",
  startsWith: "startswith",
  endsWith: "endswith",
};

export const EMPTY_FILTER: FilterGroup = { logic: "and", items: [] };

export function isFilterCondition(item: FilterGroupItem): item is FilterCondition {
  return "field" in item && "operator" in item;
}

export function isFilterGroup(item: FilterGroupItem): item is FilterGroup {
  return "logic" in item && "items" in item;
}

function serializeFilterGroupItem(item: FilterGroupItem): string {
  if (isFilterCondition(item)) {
    if (item.value === null) {
      return item.operator === "eq"
        ? `${item.field}:null`
        : `${item.field}:${BACKEND_OPERATOR[item.operator]}:null`;
    }

    return `${item.field}:${BACKEND_OPERATOR[item.operator]}:${item.value}`;
  }

  const parts = item.items.map(serializeFilterGroupItem).filter(Boolean);
  if (parts.length === 0) return "";
  if (parts.length === 1) return parts[0];
  return item.logic === "and"
    ? `and(${parts.join("|")})`
    : `or(${parts.join("|")})`;
}

export function serializeFilterGroup(group: FilterGroup): string[] {
  if (group.items.length === 0) return [];

  if (group.logic === "and") {
    return group.items.map(serializeFilterGroupItem).filter(Boolean);
  }

  const serialized = serializeFilterGroupItem(group);
  return serialized ? [serialized] : [];
}

/**
 * Extracts active backend filter strings from a DataTableState.
 * The filter group is the source of truth regardless of the active filterMode.
 */
export function extractFilter(state?: DataTableState): string[] | undefined {
  const filter = state?.filter;
  if (!filter) return undefined;
  if (filter.items.length === 0) return undefined;
  return serializeFilterGroup(filter);
}

export function withDefaultState(
  state: DataTableState | undefined,
  overrides: Partial<DataTableState>,
): DataTableState {
  return { ...(state ?? { filterMode: DEFAULT_FILTER_MODE }), ...overrides };
}

/** Extract a flat sortField + sortDirection from a SortArgs object */
export function extractSort(sort?: SortArgs): {
  sortField?: string;
  sortDirection?: string;
} {
  if (!sort) return {};
  const findEntry = (
    obj: SortArgs,
    path: string[] = [],
  ): { sortField: string; sortDirection: string } | null => {
    for (const [key, value] of Object.entries(obj)) {
      if (typeof value === "object") {
        const result = findEntry(value as SortArgs, [...path, key]);
        if (result) return result;
      } else {
        return {
          sortField: [...path, key].join("."),
          sortDirection: (value as SortDirection).toLowerCase(),
        };
      }
    }
    return null;
  };
  return findEntry(sort) ?? {};
}
