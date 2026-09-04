import { Skeleton } from "@/components/ui/skeleton";

export function ShoppingListSkeleton() {
  return (
    <div className="m-4 space-y-4" aria-hidden="true">
      <Skeleton className="h-8 w-1/3" />
      <div className="space-y-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    </div>
  );
}
