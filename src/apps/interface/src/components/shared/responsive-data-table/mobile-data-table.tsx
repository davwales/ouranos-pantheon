import { DataTableFiltering } from "@/components/shared/responsive-data-table/data-table-filtering";
import { DataTablePagination } from "@/components/shared/responsive-data-table/data-table-pagination";
import { DataTableSorting } from "@/components/shared/responsive-data-table/data-table-sorting";
import {
  DataTableProps,
  PaginationArgs,
  SortArgs,
} from "@/components/shared/responsive-data-table/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  flexRender,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table";
import React, { useMemo, useRef } from "react";

export default function MobileDataTable<TData>({
  columns,
  data,
  scrollTop = true,
  state,
  onStateChange,
  pageInfo,
  disablePagination = false,
  disableSorting = false,
  disableFiltering = false,
  ...props
}: React.ComponentProps<"div"> & DataTableProps<TData>) {
  "use no memo";
  const tableData = useMemo(() => data ?? [], [data]);
  const topRef = useRef<HTMLDivElement>(null);

  const table = useReactTable({
    data: tableData,
    columns: columns,
    getCoreRowModel: getCoreRowModel(),
    columnResizeMode: "onChange",
    state: {
      sorting: Object.entries(state?.sort ?? {}).map(([id, value]) => ({
        id,
        desc: value === "DESC",
      })),
    },
  });

  const handleScrollToTop = () => {
    if (topRef) {
      topRef.current?.scrollIntoView({ behavior: "smooth" });
    }
  };

  const handleFilterChange = (filter: Record<string, any>) => {
    if (onStateChange) {
      onStateChange({ ...state, filter });
    }
  };

  const handleSortChange = (sort: SortArgs) => {
    if (onStateChange) {
      onStateChange({ ...state, sort });
    }
  };

  const handlePaginationArgsChanged = (
    updatedPaginationArgs: PaginationArgs,
  ) => {
    if (onStateChange) {
      onStateChange({
        ...state,
        pagination: updatedPaginationArgs,
      });

      const pageChanged = updatedPaginationArgs.skip != state?.pagination?.skip;
      if (pageChanged) {
        handleScrollToTop();
      }
    }
  };

  return (
    <div {...props}>
      <div ref={topRef} />

      {!disableFiltering && (
        <DataTableFiltering
          columns={columns}
          filter={state?.filter ?? {}}
          onFilterChange={handleFilterChange}
        />
      )}

      {!disableSorting && (
        <DataTableSorting
          columns={columns}
          sort={state?.sort}
          onSortChange={handleSortChange}
        />
      )}

      <div className="space-y-4 mt-2">
        {table.getRowModel().rows?.length ? (
          table.getRowModel().rows.map((row) => (
            <Card key={row.id}>
              <CardHeader>
                <CardTitle>
                  {flexRender(
                    columns[0].cell,
                    row.getVisibleCells()[0].getContext(),
                  )}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2">
                  {row
                    .getVisibleCells()
                    .slice(1)
                    .map((cell) =>
                      cell.column.columnDef.header ? (
                        <div
                          key={cell.id}
                          className="flex justify-between items-center gap-4"
                        >
                          <span className="font-medium">
                            {cell.column.columnDef.header as string}:
                          </span>

                          <span>
                            {flexRender(
                              cell.column.columnDef.cell,
                              cell.getContext(),
                            )}
                          </span>
                        </div>
                      ) : (
                        <div key={cell.id} className="w-full">
                          {flexRender(
                            cell.column.columnDef.cell,
                            cell.getContext(),
                          )}
                        </div>
                      ),
                    )}
                </div>
              </CardContent>
            </Card>
          ))
        ) : (
          <Card>
            <CardContent className="h-24 flex items-center justify-center">
              No results.
            </CardContent>
          </Card>
        )}
      </div>

      {state?.pagination && (
        <DataTablePagination
          paginationArgs={state?.pagination}
          onPaginationArgsChanged={handlePaginationArgsChanged}
          pageInfo={pageInfo}
          disablePagination={disablePagination}
          className="mt-4"
        />
      )}

      {scrollTop && (
        <Button
          variant="outline"
          onClick={handleScrollToTop}
          className="w-full mt-2"
        >
          Back to Top
        </Button>
      )}
    </div>
  );
}
