import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface ForecastEfficacySkeletonProps {
  modelCount?: number;
  className?: string;
}

export function ForecastEfficacySkeleton({
  modelCount = 2,
  className,
}: ForecastEfficacySkeletonProps) {
  return (
    <div className={cn("mt-8", className)} aria-hidden="true">
      <Skeleton className="h-8 w-48" />
      <div className="flex gap-2 mt-4">
        <Skeleton className="h-8 w-20 rounded-md" />
        <Skeleton className="h-8 w-20 rounded-md" />
        <Skeleton className="h-8 w-20 rounded-md" />
      </div>
      <div className="flex flex-col gap-4 mt-4">
        {Array.from({ length: modelCount }).map((_, i) => (
          <div key={i} className="space-y-2">
            <div className="flex items-center gap-2">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-3 w-20" />
            </div>
            <div className="border rounded-lg bg-card p-4">
              <div className="grid grid-cols-3 gap-6">
                {Array.from({ length: 3 }).map((_, j) => (
                  <div key={j} className="flex flex-col items-center gap-1">
                    <Skeleton className="h-3 w-12" />
                    <Skeleton className="h-5 w-16" />
                  </div>
                ))}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}