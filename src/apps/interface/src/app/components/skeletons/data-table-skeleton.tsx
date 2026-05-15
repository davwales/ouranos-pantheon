import { cn } from "@/lib/utils";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Content,
  ResponsiveContent,
} from "@/app/components/responsive-content";

interface DataTableSkeletonProps {
  columns?: number;
  rows?: number;
  hasFilters?: boolean;
  hasPagination?: boolean;
  className?: string;
}

const COLUMN_WIDTHS = [
  "w-24",
  "w-32",
  "w-20",
  "w-28",
  "w-36",
  "w-24",
  "w-20",
  "w-32",
  "w-28",
  "w-36",
];

export function DesktopDataTableSkeleton({
  columns = 5,
  rows = 5,
  hasFilters = false,
  hasPagination = true,
  className,
}: DataTableSkeletonProps) {
  return (
    <div className={cn("space-y-2", className)} aria-hidden="true">
      {hasFilters && (
        <div className="flex gap-2">
          <Skeleton className="h-9 w-48" />
          <Skeleton className="h-9 w-24" />
        </div>
      )}
      <Table className="rounded-md border mt-2">
        <TableHeader>
          <TableRow>
            {Array.from({ length: columns }).map((_, index) => (
              <TableHead key={index}>
                <Skeleton className="h-4" />
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: rows }).map((_, rowIndex) => (
            <TableRow key={rowIndex}>
              {Array.from({ length: columns }).map((_, colIndex) => (
                <TableCell key={colIndex}>
                  <Skeleton
                    className={cn(
                      "h-4",
                      COLUMN_WIDTHS[colIndex % COLUMN_WIDTHS.length],
                    )}
                  />
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
      {hasPagination && (
        <div className="flex items-center justify-end gap-2 mt-2">
          <Skeleton className="h-9 w-24" />
          <Skeleton className="h-9 w-24" />
          <Skeleton className="h-9 w-40" />
        </div>
      )}
    </div>
  );
}

export function MobileDataTableSkeleton({
  rows = 3,
  columns = 3,
  hasFilters = false,
  hasPagination = true,
  className,
}: DataTableSkeletonProps) {
  const cellsPerCard = Math.min(columns, 4);
  return (
    <div className={cn("space-y-2", className)} aria-hidden="true">
      {hasFilters && (
        <div className="flex gap-2">
          <Skeleton className="h-9 w-48" />
          <Skeleton className="h-9 w-24" />
        </div>
      )}
      <div className="space-y-4 mt-2">
        {Array.from({ length: rows }).map((_, index) => (
          <Card key={index}>
            <CardHeader>
              <Skeleton className="h-5 w-2/3" />
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                {Array.from({ length: cellsPerCard }).map((_, cellIndex) => (
                  <div
                    key={cellIndex}
                    className="flex justify-between items-center gap-4"
                  >
                    <Skeleton className="h-4 w-20" />
                    <Skeleton className="h-4 w-24" />
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
      {hasPagination && (
        <div className="flex items-center justify-between gap-2 mt-4">
          <Skeleton className="h-9 w-1/2" />
          <Skeleton className="h-9 w-1/2" />
          <Skeleton className="h-9 w-full mt-2" />
        </div>
      )}
    </div>
  );
}

export function DataTableSkeleton(props: DataTableSkeletonProps) {
  const { columns = 5, rows = 5, hasFilters = false, hasPagination = true, className } = props;
  return (
    <ResponsiveContent>
      <Content type="desktop">
        <DesktopDataTableSkeleton
          columns={columns}
          rows={rows}
          hasFilters={hasFilters}
          hasPagination={hasPagination}
          className={className}
        />
      </Content>
      <Content type="mobile">
        <MobileDataTableSkeleton
          columns={columns}
          rows={rows}
          hasFilters={hasFilters}
          hasPagination={hasPagination}
          className={className}
        />
      </Content>
    </ResponsiveContent>
  );
}
