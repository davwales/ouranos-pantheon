import { PageInfo, PaginationArgs } from "@/app/components/responsive-data-table/types";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { useMemo } from "react";

export function DataTablePagination({
    paginationArgs,
    onPaginationArgsChanged,
    pageInfo,
    className,
    disablePagination,
    ...props
}: React.ComponentProps<"div"> & {
    paginationArgs?: PaginationArgs;
    onPaginationArgsChanged?: (paginationArgs: PaginationArgs) => void;
    pageInfo?: PageInfo | null | undefined;
    disablePagination?: boolean;
}) {
    const availablePageSizes = useMemo(() => paginationArgs?.pageSizes ?? [10, 20, 30, 40, 50], [paginationArgs?.pageSizes]);

    const handlePageSizeChanged = (value: string) => {
        const pageSize = parseInt(value);

        if (onPaginationArgsChanged) {
            onPaginationArgsChanged({
                ...paginationArgs,
                pageSize: pageSize,
                last: undefined,
                before: undefined,
                first: pageSize,
                after: undefined
            });
        }
    };

    const handlePreviousPage = () => {
        if (onPaginationArgsChanged) {
            onPaginationArgsChanged({
                ...paginationArgs,
                last: paginationArgs?.pageSize,
                before: pageInfo?.startCursor,
                first: undefined,
                after: undefined
            });
        }
    };

    const handleNextPage = () => {
        if (onPaginationArgsChanged) {
            onPaginationArgsChanged({
                ...paginationArgs,
                last: undefined,
                before: undefined,
                first: paginationArgs?.pageSize,
                after: pageInfo?.endCursor
            });
        }
    };

    return (
        <div {...props} className={`md:flex md:gap-2 md:items-end ${className}`}>
            {!disablePagination && (
                <div className="flex justify-between gap-2 mr-2">
                    <Button
                        onClick={handlePreviousPage}
                        disabled={pageInfo?.hasPreviousPage === false}
                        className="w-1/2 items-end"
                    >
                        <ChevronLeft /> Previous
                    </Button>

                    <Button
                        onClick={handleNextPage}
                        disabled={pageInfo?.hasNextPage === false}
                        className="w-1/2 items-end"
                    >
                        Next <ChevronRight />
                    </Button>
                </div>
            )}
            <Select value={String(paginationArgs?.pageSize)} defaultValue={String(availablePageSizes[0])} onValueChange={handlePageSizeChanged}>
                <SelectTrigger className="w-full md:w-40 mt-2">
                    <SelectValue placeholder="Page Size" />
                </SelectTrigger>
                <SelectContent>
                    {availablePageSizes.map((pageSize, index) => (
                        <SelectItem key={index} value={String(pageSize)}>{pageSize}</SelectItem>
                    ))}
                </SelectContent>
            </Select>
        </div>
    );
}