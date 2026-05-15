import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent, CardHeader } from "@/components/ui/card";

interface StrategyDetailSkeletonProps {
  className?: string;
}

function ConfigRowSkeleton() {
  return (
    <div className="flex items-center justify-between py-2 border-b border-border/50">
      <Skeleton className="h-4 w-32" />
      <Skeleton className="h-4 w-16" />
    </div>
  );
}

export function StrategyDetailSkeleton({
  className,
}: StrategyDetailSkeletonProps) {
  return (
    <div className={cn("space-y-6", className)} aria-hidden="true">
      {/* Header: Card with border-l-4, pt-6 pb-6 */}
      <Card className="border-l-4">
        <CardContent className="pt-6 pb-6">
          <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
            {/* Left side */}
            <div className="space-y-3 min-w-0">
              {/* Icon + name + subtext */}
              <div className="flex items-center gap-3">
                <Skeleton className="size-10 shrink-0 rounded-lg" />
                <div className="min-w-0 space-y-1">
                  {/* Strategy name (text-2xl ≈ h-7) */}
                  <Skeleton className="h-7 w-48" />
                  {/* Subtext: "Type · Active" */}
                  <Skeleton className="h-4 w-36" />
                </div>
              </div>

              {/* Description */}
              <Skeleton className="h-4 w-64" />

              {/* InfoChips: Created, Updated, View Backtests */}
              <div className="flex flex-wrap gap-2">
                <Skeleton className="h-6 w-24 rounded-full" />
                <Skeleton className="h-6 w-28 rounded-full" />
                <Skeleton className="h-6 w-28 rounded-full" />
              </div>
            </div>

            {/* Right side: 5 buttons */}
            <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 w-full sm:w-auto">
              <Skeleton className="h-9 w-full sm:w-24" />
              <Skeleton className="h-9 w-full sm:w-28" />
              <Skeleton className="h-9 w-full sm:w-28" />
              <Skeleton className="h-9 w-full sm:w-28" />
              <Skeleton className="h-9 w-full sm:w-24" />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Read-only view skeleton */}
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-32" />
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Trading Rules section */}
          <div className="space-y-1">
            <Skeleton className="h-4 w-28" />
            <div className="mt-2 space-y-1">
              <ConfigRowSkeleton />
              <ConfigRowSkeleton />
              <ConfigRowSkeleton />
            </div>
          </div>

          {/* Signal Weights section */}
          <div className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <ConfigRowSkeleton />
            <ConfigRowSkeleton />
            <ConfigRowSkeleton />
            <ConfigRowSkeleton />
          </div>

          {/* Thresholds */}
          <div className="space-y-1">
            <ConfigRowSkeleton />
            <ConfigRowSkeleton />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
