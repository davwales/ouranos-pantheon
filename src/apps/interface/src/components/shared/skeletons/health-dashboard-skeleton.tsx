import { Skeleton } from "@/components/ui/skeleton";

export function HealthDashboardSkeleton() {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4" aria-hidden="true">
      {Array.from({ length: 5 }).map((_, i) => (
        <div
          key={i}
          className="border border-border rounded-xl p-4 bg-card shadow-sm space-y-2"
        >
          <div className="flex items-center justify-between gap-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-6 w-20 rounded-full" />
          </div>
          <Skeleton className="h-4 w-3/4" />
        </div>
      ))}
    </div>
  );
}
