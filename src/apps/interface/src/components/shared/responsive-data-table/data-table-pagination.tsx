import { PageInfo, PaginationArgs } from "@/components/shared/responsive-data-table/types";
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
    const pageSize = paginationArgs?.pageSize ?? 10;
    const skip = paginationArgs?.skip ?? 0;

    const hasPreviousPage = pageInfo?.hasPreviousPage !== undefined
        ? pageInfo.hasPreviousPage
        : (pageInfo?.skip !== undefined ? pageInfo.skip > 0 : skip > 0);

    const hasNextPage = pageInfo?.hasNextPage !== undefined
        ? pageInfo.hasNextPage
        : (pageInfo?.totalCount !== undefined
            ? skip + pageSize < pageInfo.totalCount
            : false);

    const handlePageSizeChanged = (value: string) => {
        const newPageSize = parseInt(value);
        if (onPaginationArgsChanged) {
            onPaginationArgsChanged({
                ...paginationArgs,
                pageSize: newPageSize,
                take: newPageSize,
                skip: 0,
            });
        }
    };

    const handlePreviousPage = () => {
        if (onPaginationArgsChanged) {
            onPaginationArgsChanged({
                ...paginationArgs,
                skip: Math.max(0, skip - pageSize),
                take: pageSize,
            });
        }
    };

    const handleNextPage = () => {
        if (onPaginationArgsChanged) {
            onPaginationArgsChanged({
                ...paginationArgs,
                skip: skip + pageSize,
                take: pageSize,
            });
        }
    };

    return (
        <div {...props} className={`md:flex md:gap-2 md:items-end ${className}`}>
            {!disablePagination && (
                <div className="flex justify-between gap-2 mr-2">
                    <Button
                        onClick={handlePreviousPage}
                        disabled={!hasPreviousPage}
                        className="w-1/2 items-end"
                    >
                        <ChevronLeft /> Previous
                    </Button>

                    <Button
                        onClick={handleNextPage}
                        disabled={!hasNextPage}
                        className="w-1/2 items-end"
                    >
                        Next <ChevronRight />
                    </Button>
                </div>
            )}

            <Select value={String(pageSize)} defaultValue={String(availablePageSizes[0])} onValueChange={handlePageSizeChanged}>
                <SelectTrigger className="w-full md:w-40 mt-2">
                    <SelectValue placeholder="Page Size" />
                </SelectTrigger>
                <SelectContent>
                    {availablePageSizes.map((ps, index) => (
                        <SelectItem key={index} value={String(ps)}>{ps}</SelectItem>
                    ))}
                </SelectContent>
            </Select>
        </div>
    );
}
