import {
  type DataTableState,
  extractFilter,
  extractSort,
} from "@/components/shared/responsive-data-table/types";
import { useApi } from "@/hooks/use-api";
import { plutusApi } from "@/lib/api/plutus";
import { useMemo } from "react";

/**
 * Fetches one page of positions for a portfolio table, combining the table's
 * own sort/filter/pagination state with a fixed status filter.
 */
export function usePositionsQuery(
  marketId: string,
  tableState: DataTableState,
  statusFilter: string,
) {
  const sort = extractSort(tableState.sort);
  const filter = useMemo(
    () => [...(extractFilter(tableState) ?? []), statusFilter],
    [tableState, statusFilter],
  );

  const [state, reexecute] = useApi(
    () =>
      plutusApi.getAllPositions(marketId, {
        skip: tableState.pagination?.skip ?? 0,
        take: tableState.pagination?.take ?? 10,
        sortField: sort.sortField,
        sortDirection: sort.sortDirection,
        filter,
      }),
    [
      marketId,
      tableState.pagination,
      sort.sortField,
      sort.sortDirection,
      filter,
    ],
  );

  const data = state.data;
  const pageInfo = data
    ? {
        totalCount: data.totalCount,
        skip: data.skip,
        take: data.take,
        hasNextPage: data.skip + data.take < data.totalCount,
        hasPreviousPage: data.skip > 0,
      }
    : undefined;

  return {
    data,
    fetching: state.status === "loading",
    pageInfo,
    reexecute,
  };
}
