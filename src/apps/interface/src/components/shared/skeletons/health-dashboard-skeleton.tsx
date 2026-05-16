import { Skeleton } from "@/components/ui/skeleton";

export function HealthDashboardSkeleton() {
  return (
    <div className="space-y-0 divide-y" aria-hidden="true">
      {Array.from({ length: 5 }).map((_, i) => (
        <div
          key={i}
          className="grid grid-cols-1 md:grid-cols-[1fr_auto_1fr] gap-2 md:gap-4 py-3 items-center"
        >
          <Skeleton className="h-4 w-24" />
          <Skeleton className="h-6 w-20 rounded-full" />
          <Skeleton className="h-4 w-full md:ml-auto md:w-2/3" />
        </div>
      ))}
    </div>
  );
}
