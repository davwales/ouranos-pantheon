import { DataTableFiltering } from "@/app/components/responsive-data-table/data-table-filtering";
import { DataTablePagination } from "@/app/components/responsive-data-table/data-table-pagination";
import { DataTableSorting } from "@/app/components/responsive-data-table/data-table-sorting";
import { DataTableProps, PaginationArgs } from "@/app/components/responsive-data-table/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { SortEnumType } from "@/gql/graphql";
import { flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table";
import React, { useMemo, useRef } from "react";

export default function MobileDataTable<TData>({
    columns,
    data,
    scrollTop = true,
    paginationArgs,
    onPaginationArgsChanged,
    pageInfo,
    disablePagination = false,
    filter,
    onFilterChange,
    sort,
    onSortChange,
    ...props
}: React.ComponentProps<"div"> & DataTableProps<TData>) {
    const tableData = useMemo(() => data ?? [], [data]);
    const topRef = useRef<HTMLDivElement>(null);

    const table = useReactTable({
        data: tableData,
        columns: columns,
        getCoreRowModel: getCoreRowModel(),
        columnResizeMode: "onChange",
        state: {
            sorting: Object.entries(sort ?? {}).map(([id, value]) => ({
                id,
                desc: value === SortEnumType.Desc
            })),
        },
    });

    const handleScrollToTop = () => {
        if (topRef) {
            topRef.current?.scrollIntoView({ behavior: "smooth" });
        }
    };

    const handlePaginationArgsChanged = (updatedPaginationArgs: PaginationArgs) => {
        if (onPaginationArgsChanged) {
            onPaginationArgsChanged(updatedPaginationArgs);

            const pageChanged = updatedPaginationArgs.after != paginationArgs?.after || updatedPaginationArgs.before != paginationArgs?.before;
            if (pageChanged) {
                handleScrollToTop();
            }
        }
    };

    return (
        <div {...props}>
            <div ref={topRef} />

            {filter && <DataTableFiltering
                columns={columns}
                filter={filter}
                onFilterChange={onFilterChange}
            />}

            {(sort || onSortChange) && (<DataTableSorting
                columns={columns}
                sort={sort}
                onSortChange={onSortChange}
            />)}

            <div className="space-y-4 mt-2">
                {table.getRowModel().rows?.length ? (
                    table.getRowModel().rows.map((row) => (
                        <Card key={row.id}>
                            <CardHeader>
                                <CardTitle>
                                    {flexRender(
                                        columns[0].cell,
                                        row.getVisibleCells()[0].getContext()
                                    )}
                                </CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="space-y-2">
                                    {row.getVisibleCells().slice(1).map((cell) =>
                                        cell.column.columnDef.header ? (
                                            <div key={cell.id} className="flex justify-between items-center gap-4">
                                                <span className="font-medium">
                                                    {cell.column.columnDef.header as string}:
                                                </span>

                                                <span>
                                                    {flexRender(
                                                        cell.column.columnDef.cell,
                                                        cell.getContext()
                                                    )}
                                                </span>
                                            </div>
                                        ) : (
                                            <div key={cell.id} className="w-full">
                                                {flexRender(
                                                    cell.column.columnDef.cell,
                                                    cell.getContext()
                                                )}
                                            </div>
                                        )
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

            {(paginationArgs || onPaginationArgsChanged) && (
                <DataTablePagination
                    paginationArgs={paginationArgs}
                    onPaginationArgsChanged={handlePaginationArgsChanged}
                    pageInfo={pageInfo}
                    disablePagination={disablePagination}
                    className="mt-4"
                />
            )}

            {scrollTop && (
                <Button variant="outline" onClick={handleScrollToTop} className="w-full mt-2">
                    Back to Top
                </Button>
            )}
        </div>
    );
}
