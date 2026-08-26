import { HeadingSkeleton } from "@/components/shared/skeletons";
import { CardSkeleton } from "@/components/shared/skeletons";
import { Skeleton } from "@/components/ui/skeleton";

export function RecipeDetailSkeleton() {
  return (
    <div className="m-4 space-y-6">
      <HeadingSkeleton />
      <div className="grid gap-4 md:grid-cols-2">
        <CardSkeleton />
        <CardSkeleton />
      </div>
      <Skeleton className="h-32 w-full" />
    </div>
  );
}