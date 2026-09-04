import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

export function RecipeCardSkeleton({ className }: { className?: string }) {
  return (
    <div
      className={cn(
        "flex h-full flex-col gap-4 rounded-xl border bg-card py-4 shadow-sm",
        className,
      )}
      aria-hidden="true"
    >
      <div className="flex flex-col gap-2">
        <Skeleton className="h-5 w-2/3" />
        <Skeleton className="h-3.5 w-1/2" />
      </div>
      <div className="flex flex-col gap-1.5">
        <Skeleton className="h-3.5 w-1/3" />
        <Skeleton className="h-3.5 w-1/4" />
      </div>
      <div className="mt-auto border-t pt-4">
        <Skeleton className="h-8 w-40" />
      </div>
    </div>
  );
}

export function RecipeCardGridSkeleton({
  count = 9,
  className,
}: {
  count?: number;
  className?: string;
}) {
  return (
    <div
      className={cn(
        "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4",
        className,
      )}
      aria-hidden="true"
    >
      {Array.from({ length: count }).map((_, i) => (
        <RecipeCardSkeleton key={i} />
      ))}
    </div>
  );
}