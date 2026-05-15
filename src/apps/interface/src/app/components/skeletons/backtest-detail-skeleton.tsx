import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";
import { HeadingSkeleton } from "./heading-skeleton";
import { StatsGridSkeleton } from "./stats-grid-skeleton";

interface BacktestDetailSkeletonProps {
  className?: string;
}

export function BacktestDetailSkeleton({
  className,
}: BacktestDetailSkeletonProps) {
  return (
    <div className={cn("space-y-6", className)} aria-hidden="true">
      {/* Back link */}
      <Skeleton className="h-4 w-36" />

      {/* Header card */}
      <div className="border-l-4 rounded-lg border bg-card p-6 space-y-4">
        <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
          <div className="space-y-3">
            <HeadingSkeleton variant="h1" width="w-64" />
            <Skeleton className="h-4 w-40" />
          </div>
          <div className="flex gap-2">
            <Skeleton className="h-9 w-24" />
            <Skeleton className="h-9 w-24" />
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
          <Skeleton className="h-6 w-24 rounded-full" />
          <Skeleton className="h-6 w-36 rounded-full" />
          <Skeleton className="h-6 w-20 rounded-full" />
        </div>
      </div>

      {/* Metrics grid */}
      <StatsGridSkeleton items={8} columns="grid-cols-2 lg:grid-cols-4" />

      {/* Statistics card */}
      <div className="rounded-xl border bg-card p-6">
        <Skeleton className="h-5 w-44 mb-4" />
        <StatsGridSkeleton
          items={10}
          columns="grid-cols-2 sm:grid-cols-3 lg:grid-cols-6"
        />
      </div>
    </div>
  );
}
