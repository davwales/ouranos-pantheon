import { DataTableSkeleton } from "@/components/shared/skeletons";

export default function ExplorerLoading() {
  return <DataTableSkeleton columns={10} rows={5} hasFilters hasPagination />;
}