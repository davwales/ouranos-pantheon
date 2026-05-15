import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface ChartSkeletonProps {
  className?: string;
  legendCount?: number;
}

export function ChartSkeleton({ className, legendCount = 3 }: ChartSkeletonProps) {
  return (
    <div className={cn("space-y-3", className)} aria-hidden="true">
      {/* Chart area */}
      <div className="relative">
        <Skeleton className="aspect-video w-full max-h-96 rounded-lg" />
        {/* X-axis labels */}
        <div className="flex justify-between mt-2 pl-8">
          <Skeleton className="h-3 w-10" />
          <Skeleton className="h-3 w-10" />
          <Skeleton className="h-3 w-10" />
          <Skeleton className="h-3 w-10" />
        </div>
      </div>
      {/* Legend */}
      <div className="flex items-center justify-center gap-4 pt-3">
        {Array.from({ length: legendCount }).map((_, i) => (
          <div key={i} className="flex items-center gap-1.5">
            <Skeleton className="h-2 w-2 rounded-[2px]" />
            <Skeleton className="h-3 w-16" />
          </div>
        ))}
      </div>
    </div>
  );
}
