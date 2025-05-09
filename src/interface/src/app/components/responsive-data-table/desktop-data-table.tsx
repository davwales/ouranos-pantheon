import { DataTableFiltering } from "@/app/components/responsive-data-table/data-table-filtering";
import { DataTablePagination } from "@/app/components/responsive-data-table/data-table-pagination";
import { DataTableSorting } from "@/app/components/responsive-data-table/data-table-sorting";
import { DataTableProps, ExtendedColumnDef } from "@/app/components/responsive-data-table/types";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { SortEnumType } from "@/gql/graphql";
import { flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table";
import { useMemo } from "react";

export default function DesktopDataTable<TData>({
    columns,
    data,
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

    return (
        <div {...props}>
            {filter && <DataTableFiltering
                columns={columns}
                filter={filter}
                onFilterChange={onFilterChange}
            />}

            <Table className="rounded-md border mt-2">
                <TableHeader>
                    {table.getHeaderGroups().map((headerGroup) => (
                        <TableRow key={headerGroup.id}>
                            {headerGroup.headers.map((header) => (
                                <TableHead
                                    key={header.id}
                                    colSpan={header.colSpan}
                                >
                                    <div className="flex items-center gap-2">
                                        {header.isPlaceholder
                                            ? null
                                            : flexRender(
                                                header.column.columnDef.header,
                                                header.getContext()
                                            )
                                        }
                                        {(sort || onSortChange) && (<DataTableSorting
                                            columns={[header.column.columnDef as ExtendedColumnDef<TData>]}
                                            sort={sort}
                                            onSortChange={onSortChange}
                                            variant="desktop"
                                        />)}
                                    </div>
                                </TableHead>
                            ))}
                        </TableRow>
                    ))}
                </TableHeader>
                <TableBody>
                    {table.getRowModel().rows?.length ? (
                        table.getRowModel().rows.map((row) => (
                            <TableRow
                                key={row.id}
                                data-state={row.getIsSelected() && "selected"}
                            >
                                {row.getVisibleCells().map((cell) => (
                                    <TableCell key={cell.id}>
                                        {flexRender(
                                            cell.column.columnDef.cell,
                                            cell.getContext()
                                        )}
                                    </TableCell>
                                ))}
                            </TableRow>
                        ))
                    ) : (
                        <TableRow>
                            <TableCell
                                colSpan={table.getAllColumns().length}
                                className="h-24 text-center"
                            >
                                No results.
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>

            <DataTablePagination
                paginationArgs={paginationArgs}
                onPaginationArgsChanged={onPaginationArgsChanged}
                pageInfo={pageInfo}
                disablePagination={disablePagination}
                className="float-right mt-2"
            />
        </div>
    );
}