import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";
import { HeadingSkeleton } from "./heading-skeleton";
import { TextSkeleton } from "./text-skeleton";
import { DataTableSkeleton } from "./data-table-skeleton";

interface DetailSkeletonProps {
  sections?: ("header" | "stats" | "chart" | "table" | "content")[];
  statCount?: number;
  tableColumns?: number;
  tableRows?: number;
  hasTableFilters?: boolean;
  hasTablePagination?: boolean;
  className?: string;
}

export function DetailSkeleton({
  sections = ["header", "stats", "content"],
  statCount = 4,
  tableColumns = 5,
  tableRows = 5,
  hasTableFilters = true,
  hasTablePagination = true,
  className,
}: DetailSkeletonProps) {
  return (
    <div className={cn("space-y-6", className)} aria-hidden="true">
      {sections.includes("header") && (
        <div className="flex items-center justify-between">
          <HeadingSkeleton width="w-1/4" />
          <div className="flex gap-2">
            <Skeleton className="h-9 w-20" />
            <Skeleton className="h-9 w-20" />
          </div>
        </div>
      )}
      {sections.includes("stats") && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {Array.from({ length: statCount }).map((_, index) => (
            <div
              key={index}
              className="rounded-xl border bg-card p-4 space-y-2"
            >
              <Skeleton className="h-3 w-16" />
              <Skeleton className="h-6 w-24" />
            </div>
          ))}
        </div>
      )}
      {sections.includes("chart") && (
        <div className="rounded-xl border bg-card p-6">
          <Skeleton className="aspect-video w-full rounded-lg" />
        </div>
      )}
      {sections.includes("table") && (
        <DataTableSkeleton
          columns={tableColumns ?? 5}
          rows={tableRows ?? 5}
          hasFilters={hasTableFilters ?? true}
          hasPagination={hasTablePagination ?? true}
        />
      )}
      {sections.includes("content") && (
        <div className="rounded-xl border bg-card p-6 space-y-4">
          <TextSkeleton lines={4} lastLineWidth="w-3/4" />
        </div>
      )}
    </div>
  );
}