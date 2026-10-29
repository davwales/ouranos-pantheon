import { DetailSkeleton } from "@/components/shared/skeletons/detail-skeleton";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { Skeleton } from "@/components/ui/skeleton";

function SymbolTableSkeleton() {
  return (
    <div>
      <Skeleton className="h-5 w-24 mb-2" />
      <div className="border rounded-lg">
        <div className="border-b p-3">
          <Skeleton className="h-4 w-full" />
        </div>
        <div className="p-3">
          <div className="grid grid-cols-3 gap-4">
            <Skeleton className="h-4 w-20" />
            <Skeleton className="h-4 w-16" />
            <Skeleton className="h-4 w-16" />
          </div>
        </div>
      </div>
    </div>
  );
}

export function RecipeDetailSkeleton() {
  return (
    <div aria-hidden="true">
      <DetailSkeleton sections={["header"]} />
      <FormSkeleton fields={2} hasTitle={false} className="mt-4 max-w-lg" />
      <div className="space-y-6 mt-4">
        <SymbolTableSkeleton />
        <SymbolTableSkeleton />
      </div>
    </div>
  );
}
