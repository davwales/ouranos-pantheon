import { ColumnDef } from "@tanstack/react-table";

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
  filter?: Record<string, any>;
  sort?: SortArgs;
}

/** Maps frontend FilterOperator values to the backend filter operator strings */
const BACKEND_OPERATOR: Record<FilterOperator, string> = {
  eq: "eq",
  neq: "ne",
  gt: "gt",
  gte: "gte",
  lt: "lt",
  lte: "lte",
  contains: "like",
  startsWith: "startswith",
  endsWith: "endswith",
};

/**
 * Converts a DataTableState filter object into an array of backend filter strings.
 * Each condition becomes `field:op:value` (e.g. `symbolName:like:gold`).
 * Returns undefined when there are no active conditions.
 */
export function extractFilter(
  filter?: Record<string, any>,
): string[] | undefined {
  if (!filter || Object.keys(filter).length === 0) return undefined;

  const result: string[] = [];

  const walk = (obj: Record<string, any>, path: string[]) => {
    for (const [key, value] of Object.entries(obj)) {
      if (
        typeof value === "object" &&
        value !== null &&
        !Array.isArray(value)
      ) {
        if (Object.keys(value).some((k) => k in OPERATOR_LABELS)) {
          // Leaf: { eq: "sword" } or { gt: 100 }
          for (const [op, opValue] of Object.entries(value)) {
            if (
              op in OPERATOR_LABELS &&
              opValue !== "" &&
              opValue !== undefined &&
              opValue !== null &&
              !Number.isNaN(opValue)
            ) {
              const field = [...path, key].join(".");
              result.push(
                `${field}:${BACKEND_OPERATOR[op as FilterOperator]}:${opValue}`,
              );
            }
          }
        } else {
          walk(value as Record<string, any>, [...path, key]);
        }
      }
    }
  };

  walk(filter, []);
  return result.length > 0 ? result : undefined;
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
