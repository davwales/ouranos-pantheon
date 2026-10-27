import { DataTableSkeleton } from "@/components/shared/skeletons/data-table-skeleton";
import { DetailSkeleton } from "@/components/shared/skeletons/detail-skeleton";
import { FormSkeleton } from "@/components/shared/skeletons/form-skeleton";
import { Skeleton } from "@/components/ui/skeleton";

export function GroupDetailSkeleton() {
  return (
    <div aria-hidden="true">
      <DetailSkeleton sections={["header"]} />
      <FormSkeleton fields={2} hasTitle={false} className="mt-4 max-w-lg" />
      <div className="mt-8">
        <div className="flex items-center justify-between mb-1">
          <Skeleton className="h-5 w-32" />
          <Skeleton className="h-9 w-40" />
        </div>
        <DataTableSkeleton columns={5} rows={3} hasFilters={false} hasPagination={false} />
      </div>
    </div>
  );
}
