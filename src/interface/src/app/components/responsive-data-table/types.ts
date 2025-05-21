import { SortEnumType } from "@/gql/graphql";
import { ColumnDef } from "@tanstack/react-table";

export interface PaginationArgs {
  pageSize?: number;
  pageSizes?: number[];
  first?: number;
  after?: string | null | undefined;
  last?: number;
  before?: string | null | undefined;
}

export interface PageInfo {
  endCursor?: string | null | undefined;
  hasNextPage?: boolean | null | undefined;
  hasPreviousPage?: boolean | null | undefined;
  startCursor?: string | null | undefined;
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

export type SortValue = SortEnumType | Record<string, SortEnumType>;
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
