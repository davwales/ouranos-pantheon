import { ChartSkeleton, InfoCardGridSkeleton } from "@/components/shared/skeletons";
import { Skeleton } from "@/components/ui/skeleton";

export default function MarketDetailLoading() {
  return (
    <div className="space-y-6" aria-hidden="true">
      {/* Market Overview skeleton */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <Skeleton className="h-7 w-36" />
          <Skeleton className="h-9 w-48" />
        </div>
        <div className="flex gap-6">
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-4 w-28" />
        </div>
        <ChartSkeleton className="mt-2" legendCount={2} />
      </div>

      {/* Feature cards skeleton */}
      <InfoCardGridSkeleton count={8} />
    </div>
  );
}