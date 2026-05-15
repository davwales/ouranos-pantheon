import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface SignalCardSkeletonProps {
  className?: string;
}

export function SignalCardSkeleton({ className }: SignalCardSkeletonProps) {
  return (
    <div className={cn("border rounded-lg p-4 flex flex-col gap-3", className)} aria-hidden="true">
      <div>
        <Skeleton className="h-5 w-2/3" />
        <Skeleton className="h-3.5 w-full mt-0.5" />
      </div>
      {/* Value bar */}
      <div>
        <div className="relative h-2 bg-muted rounded-full overflow-hidden">
          <div className="absolute inset-y-0 left-1/2 w-px bg-border z-10" />
          <div className="absolute inset-y-0 left-[55%] w-[30%] bg-muted-foreground/20 rounded-full" />
        </div>
        <div className="flex justify-between mt-1">
          <Skeleton className="h-3 w-4" />
          <Skeleton className="h-3 w-10" />
          <Skeleton className="h-3 w-4" />
        </div>
      </div>
      {/* Tags and direction */}
      <div className="flex items-center justify-between gap-2 flex-wrap">
        <div className="flex gap-1 flex-wrap">
          <Skeleton className="h-5 w-12 rounded-full" />
          <Skeleton className="h-5 w-12 rounded-full" />
        </div>
        <Skeleton className="h-4 w-20" />
      </div>
    </div>
  );
}

interface SignalsSectionSkeletonProps {
  className?: string;
}

export function SignalsSectionSkeleton({ className }: SignalsSectionSkeletonProps) {
  return (
    <div className={cn("mt-8", className)} aria-hidden="true">
      <Skeleton className="h-8 w-24 border-b pb-2" />
      {/* Aggregated score card */}
      <div className="border rounded-lg p-4 mt-4 flex flex-col gap-3">
        <div className="flex items-center justify-between gap-4">
          <div>
            <Skeleton className="h-5 w-32" />
            <Skeleton className="h-3.5 w-48 mt-0.5" />
          </div>
          <Skeleton className="h-5 w-12" />
        </div>
        <div className="relative h-3 bg-muted rounded-full overflow-hidden">
          <div className="absolute inset-y-0 left-1/2 w-px bg-border z-10" />
          <div className="absolute inset-y-0 left-[45%] w-[40%] bg-muted-foreground/20 rounded-full" />
        </div>
        <div className="flex flex-wrap gap-4 pt-1">
          <Skeleton className="h-4 w-16" />
          <Skeleton className="h-4 w-16" />
          <Skeleton className="h-4 w-16" />
        </div>
      </div>
      {/* Intent filter buttons */}
      <div className="flex gap-2 mt-4 flex-wrap">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-8 w-16 rounded-md" />
        ))}
      </div>
      {/* Signal cards grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mt-4">
        {Array.from({ length: 6 }).map((_, i) => (
          <SignalCardSkeleton key={i} />
        ))}
      </div>
    </div>
  );
}
