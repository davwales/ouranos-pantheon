import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface StatsGridSkeletonProps {
  items?: number;
  columns?: string;
  className?: string;
}

export function StatsGridSkeleton({
  items = 4,
  columns = "grid-cols-2 lg:grid-cols-4",
  className,
}: StatsGridSkeletonProps) {
  return (
    <div className={cn("grid gap-4", columns, className)} aria-hidden="true">
      {Array.from({ length: items }).map((_, i) => (
        <div key={i} className="rounded-xl border bg-card p-4 space-y-2">
          <Skeleton className="h-3 w-16" />
          <Skeleton className="h-6 w-20" />
        </div>
      ))}
    </div>
  );
}
