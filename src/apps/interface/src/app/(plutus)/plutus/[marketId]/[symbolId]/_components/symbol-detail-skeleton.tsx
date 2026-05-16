import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";
import { ChartSkeleton } from "@/components/shared/skeletons/chart-skeleton";
import { SignalsSectionSkeleton } from "./signal-card-skeleton";
import { ForecastEfficacySkeleton } from "./forecast-efficacy-skeleton";
import { DataTableSkeleton } from "@/components/shared/skeletons/data-table-skeleton";

interface SymbolDetailSkeletonProps {
  className?: string;
}

export function SymbolDetailSkeleton({ className }: SymbolDetailSkeletonProps) {
  return (
    <div className={className} aria-hidden="true">
      <div className="md:flex md:justify-between md:items-center">
        <Skeleton className="h-8 w-48 mb-2" />
        <Skeleton className="h-9 w-full md:w-50" />
      </div>
      <div className="grid grid-cols-1 md:grid-cols-8 gap-2 mt-4">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="flex items-center gap-2">
            <Skeleton className="h-4 w-16" />
            <Skeleton className="h-6 w-24" />
          </div>
        ))}
      </div>
      <div className="mt-2 grid grid-cols-1 md:grid-cols-2 gap-4 gap-x-40">
        {Array.from({ length: 8 }).map((_, i) => (
          <div key={i} className="flex justify-between items-end gap-2">
            <Skeleton className="h-5 w-24" />
            <Skeleton className="h-5 w-20" />
          </div>
        ))}
      </div>
      <ChartSkeleton className="mt-8" legendCount={4} />
      <ChartSkeleton className="mt-4" legendCount={2} />
      <SignalsSectionSkeleton />
      <ForecastEfficacySkeleton />

      {/* Open Positions */}
      <div className="mt-8">
        <div className="flex items-center justify-between">
          <Skeleton className="h-5 w-32" />
          <Skeleton className="h-9 w-36" />
        </div>
        <div className="mt-4">
          <DataTableSkeleton columns={5} rows={3} hasFilters={false} hasPagination={false} />
        </div>
      </div>

      {/* Closed Positions */}
      <div className="mt-8">
        <Skeleton className="h-5 w-36" />
        <div className="mt-4">
          <DataTableSkeleton columns={5} rows={3} hasFilters={false} hasPagination={false} />
        </div>
      </div>
    </div>
  );
}
